using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MGSC;
using UnityEngine;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private static bool IsImperialRationRaid(RaidType raidType, string victimFaction, string stationFaction,
            string campaignFaction) =>
            raidType is RaidType.ProcMission or RaidType.StoryMission &&
            (victimFaction == ItemCategory || stationFaction == ItemCategory || campaignFaction == ItemCategory);

        private static int ImperialRationsForFloor(int reachableCells) =>
            Math.Min(16, Math.Max(4, (reachableCells + 199) / 200));

        private static readonly CellPosition[] RationAccessSteps =
            [new(0, 1), new(0, -1), new(1, 0), new(-1, 0)];

        private static bool IsReachableRationFloor(MapCell cell) =>
            cell != null && cell.ReachableCellFlag && cell.Type == MapCellType.Floor &&
            !cell.IsObjBlockPass && cell.LiquidLevel == 0 && cell.specialFlag == MapCellSpecialFlag.None;

        private static bool HasReachableRationAccess(MapObstacle obstacle, MapGrid grid) =>
            obstacle.OccupiedCells.Any(position => RationAccessSteps.Any(step =>
                IsReachableRationFloor(grid.GetCell(position + step)) && !grid.GetFence(position, position + step)));

        // This overload builds a new floor. Loading its saved JSON never adds supplies again.
        // Run after furnishing so machinery cannot cover a newly placed ration.
        [HarmonyPatch(typeof(DungeonBuilder), nameof(DungeonBuilder.Create), [typeof(InputMapData)])]
        private static class ImperialRationLoot
        {
            private static void Postfix(State ____state, DungeonCreationResult __result)
            {
                if (!_industryReady || __result != DungeonCreationResult.Success) return;
                var raid = ____state.Get<RaidMetadata>();
                if (raid?.RaidType is not (RaidType.ProcMission or RaidType.StoryMission)) return;
                var mission = ____state.Get<Missions>().Get(raid);
                var station = string.IsNullOrEmpty(raid.StationId) ? null :
                    ____state.Get<Stations>().Get(raid.StationId, logMissing: false);
                if (!IsImperialRationRaid(raid.RaidType, mission?.VictimFactionId, station?.OwnerFactionId,
                    IsCampaignMission(mission) ? mission.BeneficiaryFactionId : null)) return;

                var grid = ____state.Get<MapGrid>();
                var cells = grid.GetReference().Cast<MapCell>()
                    .Where(IsReachableRationFloor)
                    .ToArray();
                if (cells.Length == 0) return;

                var location = ____state.Get<LocationMetadata>();
                var random = new System.Random(location.Seed ^ 0x726174);
                var looseCells = cells.OrderBy(_ => random.Next()).ToList();
                var stores = ____state.Get<MapObstacles>().Obstacles
                    .Where(obstacle => obstacle.QuestStorage == null && obstacle.Store != null &&
                        obstacle.Store.UseForSpawnItems && obstacle.Store.HasAccess() &&
                        HasReachableRationAccess(obstacle, grid))
                    .OrderBy(_ => random.Next()).ToList();
                var floor = ____state.Get<ItemsOnFloor>();
                var placed = new List<CellPosition>();
                var entrance = ____state.Get<Creatures>()?.Player?.CreatureData.Position ??
                    ____state.Get<MapMetadata>().LevelEntries.Select(entry => entry.Position)
                        .DefaultIfEmpty(new CellPosition(cells[0].X, cells[0].Y)).First();
                int clean = ImperialRationsForFloor(cells.Length);
                int dirty = (clean + 3) / 4;
                int stored = 0, loose = 0;

                for (int index = 0; index < clean + dirty; index++)
                {
                    string id = index < clean ? CorpseStarch : DirtyStarch;
                    var item = SingletonMonoBehaviour<ItemFactory>.Instance.CreateForInventory(id, isSingleItem: true);
                    var record = Data.Items.GetSimpleRecord<ItemRecord>(id);
                    // The first clean ration is close to arrival; the rest occupy ordinary loot containers.
                    bool inStore = false;
                    if (index > 0)
                    {
                        for (int storeIndex = 0; storeIndex < stores.Count; storeIndex++)
                        {
                            var obstacle = stores[storeIndex];
                            var store = obstacle.Store;
                            if (!store.AllowedItemClasses.Contains(record.ItemClass) ||
                                !store.storage.TryPutItem(item, CellPosition.Zero)) continue;
                            store.RefreshVisual();
                            placed.Add(obstacle.Position);
                            stores.RemoveAt(storeIndex);
                            stored++;
                            inStore = true;
                            break;
                        }
                    }
                    if (inStore) continue;

                    MapCell target;
                    if (index == 0)
                    {
                        target = looseCells.OrderBy(cell => SpineManhattan(new CellPosition(cell.X, cell.Y), entrance)).First();
                    }
                    else
                    {
                        target = looseCells.FirstOrDefault(cell => placed.All(position =>
                            SpineManhattan(new CellPosition(cell.X, cell.Y), position) >= 6)) ?? looseCells.FirstOrDefault();
                    }
                    // Even a tiny floor can hold several rations at one reachable position.
                    target ??= cells[0];
                    var position = new CellPosition(target.X, target.Y);
                    ItemOnFloorSystem.SpawnItem(floor, grid, item, position);
                    placed.Add(position);
                    looseCells.Remove(target);
                    loose++;
                }
                Debug.Log($"[Cvar_ImperiumOfMan] Imperial rations on {location.LocationId}: " +
                    $"clean={clean}, decayed={dirty}, containers={stored}, floor={loose}, reachable cells={cells.Length}");
            }
        }
    }
}

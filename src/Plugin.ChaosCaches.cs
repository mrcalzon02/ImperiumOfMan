using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MGSC;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        [HarmonyPatch(typeof(DungeonBuilder), "Populate")]
        private static class ChaosSupplyCaches
        {
            private static void Postfix(State ____state)
            {
                var raid = ____state.Get<RaidMetadata>();
                var location = ____state.Get<LocationMetadata>();
                if (!_industryReady || raid.RaidType != RaidType.ProcMission || location.LocationId != "stage1")
                {
                    return;
                }
                var mission = ____state.Get<Missions>().Get(raid);
                if (mission?.VictimFactionId != ChaosFaction ||
                    ____state.Get<Stations>().Get(raid.StationId, logMissing: false)?.OwnerFactionId != ChaosFaction)
                {
                    return;
                }
                var site = Array.Find(ChaosSites, entry => entry.Id == raid.StationId);
                if (site == null)
                {
                    return;
                }
                var faction = ____state.Get<Factions>().Get(ChaosFaction);
                int highest = Math.Min(site.LastTier, faction.CurrentTechLevel);
                if (highest < site.FirstTier)
                {
                    return;
                }

                var random = new Random(location.Seed);
                bool recoveredRelic = site.Id == BlackMeridian && highest == 10 &&
                    ____state.Get<StoryTriggers>().IsPassed(FinaleClaimed);
                int selectedTier = recoveredRelic ? 10 : random.Next(site.FirstTier, highest + 1);
                var tier = _chaosSupply[selectedTier - 1];
                var items = new List<BasePickupItem>();
                ItemInteractionSystem.CreateItem(items, tier.Chip, 1);
                ItemInteractionSystem.CreateItem(items, tier.Salvage.Id, tier.Level < 7 ? 2 : 1);
                ItemInteractionSystem.CreateItem(items, tier.Utility.Id, tier.Utility is AmmoRecord ? 8 : 1);
                if (tier.Level < 10)
                {
                    ItemInteractionSystem.CreateItem(items, tier.Weapon.Id, 1);
                    string ammo = tier.Weapon.DefaultAmmoId;
                    if (!string.IsNullOrEmpty(tier.Weapon.RequiredAmmo) && ammo != tier.Utility.Id)
                    {
                        ItemInteractionSystem.CreateItem(items, ammo, Math.Max(6, tier.Weapon.MagazineCapacity / 2));
                    }
                }

                var stores = ____state.Get<MapObstacles>().Obstacles
                    .Where(obstacle => obstacle.QuestStorage == null && obstacle.Store != null &&
                        obstacle.Store.UseForSpawnItems && obstacle.Store.HasAccess())
                    .Select(obstacle => obstacle.Store)
                    .OrderBy(_ => random.Next())
                    .ToArray();
                var grid = ____state.Get<MapGrid>();
                var floor = ____state.Get<ItemsOnFloor>();
                MapCell fallback = null;
                foreach (BasePickupItem item in items)
                {
                    var record = Data.Items.GetSimpleRecord<ItemRecord>(item.Id);
                    if (stores.Any(store => store.AllowedItemClasses.Contains(record.ItemClass) &&
                        store.storage.TryPutItem(item, CellPosition.Zero)))
                    {
                        continue;
                    }
                    fallback ??= grid.GetReference().Cast<MapCell>()
                        .Where(cell => cell != null && cell.ReachableCellFlag && cell.Type == MapCellType.Floor &&
                            !cell.IsObjBlockPass && cell.specialFlag == MapCellSpecialFlag.None)
                        .OrderBy(_ => random.Next())
                        .FirstOrDefault();
                    if (fallback != null)
                    {
                        ItemOnFloorSystem.SpawnItem(floor, grid, item, new CellPosition(fallback.X, fallback.Y));
                    }
                }
            }
        }
    }
}

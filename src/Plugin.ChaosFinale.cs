using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using MGSC;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private const string BlackMeridian = "iomBlackMeridian";
        private const string FinaleBoss = "iom.chaos.finale.boss";
        private const string AnchorOne = "iom.chaos.finale.anchor_one";
        private const string AnchorTwo = "iom.chaos.finale.anchor_two";
        private const string SweepTurn = "iom.chaos.finale.sweep_turn";
        private const string SweepX = "iom.chaos.finale.sweep_x";
        private const string SweepY = "iom.chaos.finale.sweep_y";
        private const int ShadowAnchorHealth = 60;
        private const int AnchorProtection = 12;
        private static GameObject _shadowSweepMarker;
        private static Sprite _shadowSweepSprite;

        [Hook(ModHookType.AfterSpaceLoaded)]
        public static void RecordLoadedChaosRelic(IModContext context) => RecordRecoveredChaosRelic(context);

        [Hook(ModHookType.SpaceStarted)]
        public static void RecordRecoveredChaosRelic(IModContext context)
        {
            var state = context.State;
            var triggers = state.Get<StoryTriggers>();
            var cargo = state.Get<MagnumCargo>();
            var mercenaries = state.Get<Mercenaries>();
            if (triggers == null || cargo == null || mercenaries == null || triggers.IsPassed(FinaleClaimed))
            {
                return;
            }
            string relic = ChaosItemPrefix + "shadowblade";
            if (MagnumCargoSystem.GetAvailableCargoStorages(cargo).Any(storage => storage.CountItems(relic) > 0) ||
                mercenaries.Values.Any(mercenary => mercenary.CreatureData.Inventory.Slots
                    .Concat(mercenary.CreatureData.Inventory.Storages).Any(storage => storage.CountItems(relic) > 0)))
            {
                triggers.Pass(FinaleClaimed);
            }
        }

        [Hook(ModHookType.SpaceUpdateAfterGameLoop)]
        public static void OfferBlackMeridianChallenge(IModContext context)
        {
            var state = context.State;
            var factions = state.Get<Factions>();
            var chaos = factions.Get(ChaosFaction, logMissing: false);
            if (chaos == null || chaos.CurrentTechLevel < 10 || !factions.IsEnabledFaction(chaos) ||
                state.Get<StoryTriggers>().IsPassed(FinaleClaimed))
            {
                return;
            }
            var stations = state.Get<Stations>();
            var station = stations.Get(BlackMeridian, logMissing: false);
            var missions = state.Get<Missions>();
            if (station?.OwnerFactionId != ChaosFaction || missions.Get(BlackMeridian, reversed: false) != null)
            {
                return;
            }
            var cache = state.Get<FactionCachedData>();
            if (!cache.stationNeighbours.TryGetValue(BlackMeridian, out var neighbors) ||
                !cache.factionFriends.TryGetValue(ChaosFaction, out var friends))
            {
                return;
            }
            var sponsor = neighbors.Select(neighbor => factions.Get(neighbor.OwnerFactionId, logMissing: false))
                .Where(candidate => candidate != null && candidate.Id != ChaosFaction && candidate.FactionType == FactionType.Corp &&
                    factions.IsEnabledFaction(candidate) && !friends.Contains(candidate.Id) &&
                    missions.Values.Count(mission => mission.BeneficiaryFactionId == candidate.Id) <
                    Data.TechLevels[candidate.CurrentTechLevel].MaxMissionsCount)
                .OrderByDescending(candidate => candidate.Id == ItemCategory)
                .ThenByDescending(candidate => candidate.PlayerReputation)
                .FirstOrDefault();
            if (sponsor != null)
            {
                MissionSystem.CreateMission(state.Get<MissionFactory>(), missions, state.Get<News>(), state.Get<SpaceTime>(),
                    state.Get<TravelMetadata>(), ProceduralMissionType.Elimination, station, sponsor, chaos);
            }
        }

        [HarmonyPatch(typeof(DungeonBuilder), nameof(DungeonBuilder.Create), [typeof(InputMapData)])]
        private static class ChaosFinalFloor
        {
            private static void Postfix(State ____state, DungeonCreationResult __result)
            {
                var raid = ____state.Get<RaidMetadata>();
                if (__result != DungeonCreationResult.Success || raid.RaidType != RaidType.ProcMission)
                {
                    return;
                }
                var site = Array.Find(ChaosSites, entry => entry.Id == raid.StationId);
                var mission = ____state.Get<Missions>().Get(raid);
                if (site == null || mission?.VictimFactionId != ChaosFaction ||
                    ____state.Get<Stations>().Get(site.Id, logMissing: false)?.OwnerFactionId != ChaosFaction ||
                    ____state.Get<LocationMetadata>().LocationId != "stage" + MissionSystem.GetFloorsCount(mission))
                {
                    return;
                }

                int tier = Math.Min(site.LastTier, ____state.Get<Factions>().Get(ChaosFaction).CurrentTechLevel);
                if (tier < site.FirstTier)
                {
                    return;
                }
                var grid = ____state.Get<MapGrid>();
                var creatures = ____state.Get<Creatures>();
                bool finale = site.Id == BlackMeridian && tier == 10 &&
                    !____state.Get<StoryTriggers>().IsPassed(FinaleClaimed) && !raid.CustomCounters.ContainsKey(FinaleClaimed);
                string specialist = finale ? ChaosFinale : ChaosBestiary.Last(enemy => enemy.Tier == Math.Min(tier, 9)).Id;
                if (raid.CustomCounters.ContainsKey(FinaleClaimed) ||
                    creatures.Monsters.Any(creature => creature.CreatureData.MobClassId == specialist))
                {
                    return;
                }
                var obstacles = ____state.Get<MapObstacles>();
                var player = creatures.Player;
                var free = grid.GetReference().Cast<MapCell>().Where(cell =>
                    cell != null && cell.ReachableCellFlag && cell.Type == MapCellType.Floor &&
                    !cell.IsDoor && !cell.IsOneWideCorridor && !cell.IsObjBlockPass &&
                    cell.specialFlag == MapCellSpecialFlag.None && creatures.GetCreature(cell.Position) == null &&
                    obstacles.Get(cell.X, cell.Y, interactable: false) == null).ToArray();
                var preferred = free.OrderBy(cell => Math.Abs(10f - cell.Position.Distance(player.CreatureData.Position))).ToArray();
                var bossCell = preferred.FirstOrDefault();
                if (bossCell == null)
                {
                    return;
                }
                MapCell[] anchorCells = [];
                if (finale)
                {
                    var placements = free.ToDictionary(cell => cell.Position);
                    foreach (var candidate in preferred)
                    {
                        var visible = FindShadowAnchorCells(grid, placements, candidate.Position).ToArray();
                        if (visible.Length > anchorCells.Length)
                        {
                            bossCell = candidate;
                            anchorCells = visible;
                        }
                        if (anchorCells.Length == 2)
                        {
                            break;
                        }
                    }
                }

                var boss = CreatureSystem.SpawnMonsterFromMobClass(____state.Get<Mercenaries>(), ____state.Get<PerkFactory>(),
                    ____state.Get<Difficulty>(), creatures, raid, ____state.Get<TurnController>(), specialist, bossCell.Position,
                    techLevelLimit: tier, factionId: ChaosFaction, creatureAlliance: CreatureAlliance.VictimFaction);
                if (!finale)
                {
                    return;
                }
                raid.CustomCounters.Remove(AnchorOne);
                raid.CustomCounters.Remove(AnchorTwo);
                raid.CustomCounters[FinaleBoss] = boss.CreatureData.UniqueId;
                if (anchorCells.Length >= 1) PlaceAnchor(AnchorOne, anchorCells[0]);
                if (anchorCells.Length >= 2) PlaceAnchor(AnchorTwo, anchorCells[1]);

                void PlaceAnchor(string key, MapCell cell)
                {
                    var factory = SingletonMonoBehaviour<MapObstacleFactory>.Instance;
                    var anchor = factory.SpawnByName("cattaram_crystal", cell.Position, CellPosition.Zero, refreshOccupation: true);
                    if (anchor?.ObstacleHealth is { Health: > 0 })
                    {
                        ConfigureShadowAnchor(anchor);
                        raid.CustomCounters[key] = anchor.UniqueId;
                        AdjustShroud(boss, player, AnchorProtection);
                    }
                }
            }
        }

        [Hook(ModHookType.AfterDungeonLoaded)]
        public static void RestoreShadowAnchors(IModContext context)
        {
            var state = context.State;
            var counters = state.Get<RaidMetadata>().CustomCounters;
            var obstacles = state.Get<MapObstacles>();
            foreach (string key in new[] { AnchorOne, AnchorTwo })
            {
                if (counters.TryGetValue(key, out int id) && obstacles.Get(id) is { } anchor)
                {
                    ConfigureShadowAnchor(anchor);
                }
            }
        }

        private static void ConfigureShadowAnchor(MapObstacle anchor)
        {
            var health = anchor.ObstacleHealth;
            // The native crystal prefab is meant for a timed pact. Finale anchors have to respond to weapon hits.
            health.SetInvulnerable(false);
            health._maxHealth = ShadowAnchorHealth;
            health._armor = 3;
            health._hitChanceOnShoot = 100;
            health._resistSheet = [];
            health.Health = Math.Min(health.Health, ShadowAnchorHealth);
            if (!anchor.IsOccupyCell(anchor.Position)) anchor.OccupiedCells.Add(anchor.Position);
        }

        private static IEnumerable<MapCell> FindShadowAnchorCells(MapGrid grid, Dictionary<CellPosition, MapCell> available,
            CellPosition center)
        {
            CellPosition[] directions = [new(1, 0), new(-1, 0), new(0, 1), new(0, -1),
                new(1, 1), new(-1, -1), new(1, -1), new(-1, 1)];
            var candidates = new List<MapCell>();
            foreach (var step in directions)
            {
                for (int distance = 1; distance <= 3; distance++)
                {
                    var position = center + step * distance;
                    var cell = grid.GetCell(position);
                    if (cell == null || cell.Type != MapCellType.Floor || cell.IsDoor || cell.isObjBlockSight || cell.IsObjBlockPass)
                    {
                        break;
                    }
                    if (distance >= 2 && available.TryGetValue(position, out var anchor))
                    {
                        candidates.Add(anchor);
                        break;
                    }
                }
            }
            var first = candidates.FirstOrDefault();
            if (first == null) yield break;
            yield return first;
            var second = candidates.OrderByDescending(cell => cell.Position.DistanceSquared(first.Position))
                .FirstOrDefault(cell => cell.Position.DistanceSquared(first.Position) >= 4f);
            if (second != null) yield return second;
        }

        private static void AdjustShroud(Creature boss, Player player, int change)
        {
            var sheet = boss.CreatureData.ResistSheet.BasicResistSheet;
            for (int i = 0; i < sheet.Count; i++)
            {
                var resistance = sheet[i];
                resistance.resistPercent += change;
                sheet[i] = resistance;
            }
            CreatureSystem.RefreshResists(boss.CreatureData, player?.Mercenary);
        }

        [HarmonyPatch]
        private static class ChaosAnchorDestruction
        {
            private static MethodBase[] TargetMethods() =>
                [AccessTools.Method(typeof(MapObstacle), nameof(MapObstacle.Injure)),
                 AccessTools.Method(typeof(MapObstacle), nameof(MapObstacle.Kill))];

            private static void Postfix(MapObstacle __instance)
            {
                if (__instance.ObstacleHealth?.Health != 0 || SingletonMonoBehaviour<DungeonGameMode>.Instance == null)
                {
                    return;
                }
                var state = SingletonMonoBehaviour<DungeonGameMode>.Instance._state;
                var counters = state.Get<RaidMetadata>().CustomCounters;
                if (!counters.TryGetValue(FinaleBoss, out int bossId))
                {
                    return;
                }
                bool removed = counters.TryGetValue(AnchorOne, out int first) && first == __instance.UniqueId && counters.Remove(AnchorOne);
                removed |= counters.TryGetValue(AnchorTwo, out int second) && second == __instance.UniqueId && counters.Remove(AnchorTwo);
                if (removed && state.Get<Creatures>().GetCreature(bossId) is { } boss && boss.CreatureData.Health.Alive)
                {
                    AdjustShroud(boss, state.Get<Creatures>().Player, -AnchorProtection);
                    UI.Staff.NotificationPanel.AddNotification(Localization.Get("iom.chaos.anchor_broken"));
                }
            }
        }

        [Hook(ModHookType.DungeonUpdateAfterGameLoop)]
        public static void ProcessChaosShadowSweep(IModContext context)
        {
            var state = context.State;
            var raid = state.Get<RaidMetadata>();
            var counters = raid.CustomCounters;
            if (!counters.TryGetValue(FinaleBoss, out int id))
            {
                return;
            }
            var creatures = state.Get<Creatures>();
            var boss = creatures.GetCreature(id);
            var player = creatures.Player;
            if (boss == null || boss.CreatureData.Health.Dead || player == null || player.CreatureData.Health.Dead)
            {
                if (_shadowSweepMarker != null) Object.Destroy(_shadowSweepMarker);
                counters.Remove(SweepTurn);
                return;
            }
            if (counters.TryGetValue(SweepTurn, out int strikeTurn))
            {
                var target = new CellPosition(counters[SweepX], counters[SweepY]);
                if (raid.TurnNumber >= strikeTurn)
                {
                    counters.Remove(SweepTurn);
                    if (_shadowSweepMarker != null) Object.Destroy(_shadowSweepMarker);
                    state.Get<ExplosionFactory>().SpawnExplosion(boss, target, "mars_explosion", fastForward: true, parameters =>
                    {
                        parameters.Radius = 1;
                        parameters.Damage = 51;
                        parameters.DamageType = "chaos";
                        parameters.SelfDamaging = false;
                        parameters.GainDmgToLocation = false;
                        parameters.PropagateFire = false;
                        return parameters;
                    });
                }
                else
                {
                    ShowMarker(target);
                }
                return;
            }
            int cadence = counters.ContainsKey(AnchorOne) || counters.ContainsKey(AnchorTwo) ? 5 : 4;
            if (raid.TurnNumber > 0 && raid.TurnNumber % cadence == 0 && boss.IsSeenByPlayer &&
                CellPosition.Distance(boss.CreatureData.Position, player.CreatureData.Position) <= 5)
            {
                var target = player.CreatureData.Position;
                counters[SweepTurn] = raid.TurnNumber + 1;
                counters[SweepX] = target.X;
                counters[SweepY] = target.Y;
                ShowMarker(target);
                UI.Staff.NotificationPanel.AddNotification(Localization.Get("iom.chaos.shadow_warning"));
            }

            void ShowMarker(CellPosition cell)
            {
                if (_shadowSweepMarker != null)
                {
                    return;
                }
                if (_shadowSweepSprite == null)
                {
                    _shadowSweepSprite = ReadItemSprite(AnchorFragment + "_icon", null);
                }
                _shadowSweepMarker = new GameObject("Black Meridian strike warning", typeof(SpriteRenderer));
                var renderer = _shadowSweepMarker.GetComponent<SpriteRenderer>();
                renderer.sprite = _shadowSweepSprite;
                renderer.color = new Color(1f, 0.68f, 1f, 0.9f);
                renderer.sortingOrder = 100;
                _shadowSweepMarker.transform.SetParent(SingletonMonoBehaviour<DungeonGameMode>.Instance.transform);
                _shadowSweepMarker.transform.position = state.Get<MapRenderer>().ConvertPosToWorld(cell, Vector2.zero);
            }
        }
    }
}

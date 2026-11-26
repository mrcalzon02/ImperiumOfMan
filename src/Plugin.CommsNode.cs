using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MGSC;
using UnityEngine;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private const string CommsModule = "iom_comms_node";
        private const string SecurityIntrusion = "iom_comms_security_intrusion";
        private const string EntranceOverride = "iom_comms_entrance_override";
        private const string ExtractionOverride = "iom_comms_extraction_override";
        private const string CommsTurretsDeployed = "iom.comms.turrets_deployed";

        private static void RegisterCommsNodeUpgrades()
        {
            AddModuleText(CommsModule, "Comms Node", "Non-story missions only. Upgrade one allied turret at each unlocked location.");
            var intrusion = RegisterCommsUpgrade(SecurityIntrusion, null, new CellPosition(0, 1), "CommsSecurityIntrusion",
                "Security Intrusion", "Deploy one allied turret at a random site location. Non-story missions only.",
                ["communication_relay", "communication_relay", "communication_relay",
                    "ai_module", "ai_module", "ai_module", "darknet_unit", "darknet_unit",
                    "prototype_schematics", "electrical_parts_container", "electrical_parts_container"]);
            var entrance = RegisterCommsUpgrade(EntranceOverride, intrusion, new CellPosition(0, 2), "CommsEntranceOverride",
                "Entrance Override", "Deploy one more allied turret near the entrance. Non-story missions only.",
                ["communication_relay", "ai_module", "darknet_unit", "electrical_parts_container"]);
            var extraction = RegisterCommsUpgrade(ExtractionOverride, entrance, new CellPosition(0, 3), "CommsExtractionOverride",
                "Extraction Override", "Deploy one more allied turret near extraction. Non-story missions only.",
                ["communication_relay", "ai_module", "electrical_parts_container"]);

            MagnumPerkRecord[] roots = [intrusion, entrance, extraction];
            for (int row = 0; row < CommsLocations.Length; row++)
            {
                var location = CommsLocations[row];
                var previous = roots[row];
                for (int tier = 1; tier < CommsTurretTypes.Length; tier++)
                {
                    var type = CommsTurretTypes[tier];
                    previous = RegisterCommsUpgrade(location.PerkId(tier), previous,
                        new CellPosition(tier, location.Row), location.Art, location.Label + " " + type.Name,
                        "Replace this location's turret with a " + type.Name +
                        ". Only the highest purchased type at this location deploys. Non-story missions only.",
                        [.. type.UpgradePrice], location.RootId);
                }
            }
        }

        private static MagnumPerkRecord RegisterCommsUpgrade(string id, MagnumPerkRecord parent, CellPosition position,
            string art, string name, string description, List<string> price, string department = "")
        {
            var descriptor = ScriptableObject.CreateInstance<MagnumPerkDescriptor>();
            descriptor.name = id;
            descriptor._icon = SupportIcon(art);
            descriptor._lockedIcon = SupportIcon(art, locked: true);

            var perk = new MagnumPerkRecord
            {
                Id = id,
                Enabled = true,
                ModuleId = CommsModule,
                DepartmentId = department,
                PositionInGraph = position,
                Parents = parent == null ? [] : [parent.Id],
                Modifiers = [],
                UpgradePrice = price,
                ContentDescriptor = descriptor
            };
            parent?.Childs.Add(id);
            StageRecord(Data.MagnumPerks, id, perk);
            TranslationPatch.AddMagnumUpgrade(id, name, "Comms Node. " + description);
            return perk;
        }

        [HarmonyPatch(typeof(DungeonBuilder), "GenerateByScenarios")]
        private static class DeployCommsTurrets
        {
            private static void Postfix(State ____state)
            {
                if (!IsCommsMission(____state)) return;
                var raid = ____state.Get<RaidMetadata>();
                var progression = ____state.Get<MagnumProgression>();
                if (progression?.IsPerkPurchased(SecurityIntrusion) != true ||
                    raid.CustomCounters.ContainsKey(CommsTurretsDeployed))
                {
                    return;
                }

                string start = raid.World.StartLocationId;
                if (start == "arrival")
                {
                    start = raid.World.FindNextLocation(start, TransitionType.Spaceships)?.ID;
                }
                if (____state.Get<LocationMetadata>().LocationId != start)
                {
                    return;
                }

                var grid = ____state.Get<MapGrid>();
                var map = ____state.Get<MapMetadata>();
                var obstacles = ____state.Get<MapObstacles>();
                var entities = ____state.Get<MapEntities>();
                var creatures = ____state.Get<Creatures>();
                CellPosition entrance = creatures.Player.CreatureData.Position;
                CellPosition extraction = FindCommsExtraction(map, obstacles, entrance);
                var reserved = new HashSet<CellPosition>(map.LevelEntries.Select(entry => entry.Position));
                reserved.UnionWith(map.LevelExits.Select(exit => exit.Position));
                reserved.Add(entrance);
                reserved.Add(extraction);

                var available = new List<CellPosition>();
                for (int y = 0; y < grid.MaxHeight; y++)
                for (int x = 0; x < grid.MaxWidth; x++)
                {
                    var cell = grid.GetCell(x, y);
                    if (cell.Type == MapCellType.Floor && cell.ReachableCellFlag &&
                        cell.specialFlag == MapCellSpecialFlag.None && cell.IsPassable(ObstacleType.Unexplored) &&
                        !reserved.Contains(cell.Position) &&
                        ItemInteractionSystem.CanPlaceObject(grid, creatures, entities, cell.Position) &&
                        obstacles.Get(x, y) == null)
                    {
                        available.Add(cell.Position);
                    }
                }

                int count = 0;
                if (progression.IsPerkPurchased(EntranceOverride))
                {
                    PlaceBeside(entrance, CommsLocations[1]);
                }
                if (progression.IsPerkPurchased(ExtractionOverride))
                {
                    PlaceBeside(extraction, CommsLocations[2]);
                }

                var field = available.Where(pos => CellPosition.Distance(pos, entrance) > 5 &&
                    CellPosition.Distance(pos, extraction) > 5).ToList();
                if (field.Count == 0)
                {
                    field = available;
                }
                if (field.Count > 0)
                {
                    Place(field[Random.Range(0, field.Count)], CommsLocations[0]);
                }

                raid.CustomCounters[CommsTurretsDeployed] = count;

                void PlaceBeside(CellPosition anchor, CommsLocation location)
                {
                    var room = new List<MapCell>();
                    MapSystem.GetRoomWalkableCells(grid, obstacles, anchor, room);
                    var roomPositions = new HashSet<CellPosition>(room.Select(cell => cell.Position));
                    CellPosition? position = available
                        .Where(pos => roomPositions.Contains(pos) && CellPosition.Distance(pos, anchor) <= 6)
                        .OrderBy(pos => CellPosition.Distance(pos, anchor))
                        .Select(pos => (CellPosition?)pos)
                        .FirstOrDefault();
                    if (position.HasValue)
                    {
                        Place(position.Value, location);
                    }
                }

                void Place(CellPosition position, CommsLocation location)
                {
                    var type = CommsTurretTypes[EffectiveCommsTurretTier(progression, location)];
                    if (!TryResolveCommsTurretRole(type, out string mobClass))
                    {
                        Debug.LogWarning("[Cvar_ImperiumOfMan] Comms turret skipped; missing target for " + type.PackageId);
                        return;
                    }
                    var turret = CreatureSystem.SpawnMonsterFromMobClass(____state.Get<Mercenaries>(),
                        ____state.Get<PerkFactory>(), ____state.Get<Difficulty>(), creatures, raid,
                        ____state.Get<TurnController>(), mobClass, position,
                        creatureAlliance: CreatureAlliance.PlayerAlliance);
                    turret.CreatureData.IsSummonedCreature = true;
                    available.Remove(position);
                    count++;
                }
            }

            private static CellPosition FindCommsExtraction(MapMetadata map, MapObstacles obstacles, CellPosition entrance)
            {
                var shuttle = obstacles.Get<Shuttle>();
                if (shuttle != null)
                {
                    return shuttle.GetComponent<Shuttle>().GetSpawnPosition();
                }
                if (map.LevelExits.Count > 0)
                {
                    return map.LevelExits[0].Position;
                }
                return obstacles.Elevators.Where(lift => !lift.IsMirage)
                    .Select(lift => lift.GetSpawnPosition())
                    .OrderBy(pos => CellPosition.Distance(pos, entrance))
                    .Select(pos => (CellPosition?)pos)
                    .FirstOrDefault() ?? entrance;
            }
        }
    }
}

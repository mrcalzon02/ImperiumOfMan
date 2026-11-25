using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HarmonyLib;
using MGSC;
using SimpleJSON;
using UnityEngine;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private const string ImperialMachineContainer = "iom_machine_output";
        private const int MachineSlotWidth = 5;
        private const int MachineSlotHeight = 3;
        private static readonly float[] MachineDestructionStages = [1f, .75f, .4f, 0f];
        private static ImperialMachine[] ImperialMachines;

        private sealed class ImperialMachine
        {
            public string Key;
            public string Name;
            public int Width;
            public int Height;
            public int Health;
            public string Output;
            public string[] Sites;

            public string Descriptor(bool warp) => (warp ? "iom_warp_machine_" : "iom_machine_") + Key;
        }

        private static void RegisterImperialMachinery()
        {
            const string resource = "ImperiumOfMan.Art.ImperialMachinery.json";
            using var input = typeof(Plugin).Assembly.GetManifestResourceStream(resource)
                ?? throw new InvalidDataException("Missing " + resource);
            using var reader = new StreamReader(input);
            var machines = JSON.Parse(reader.ReadToEnd()).AsArray.Children.Select(machine => new ImperialMachine
            {
                Key = machine["key"].Value,
                Name = machine["name"].Value,
                Width = machine["footprint"][0].AsInt,
                Height = machine["footprint"][1].AsInt,
                Health = machine["health"].AsInt,
                Output = machine["output"].Value,
                Sites = machine["sites"].AsArray.Children.Select(site => site.Value).ToArray(),
            }).ToArray();
            int columns = MachineDestructionStages.Length * MachineSlotWidth;
            int rows = machines.Length * MachineSlotHeight;
            var clean = ReadDamageAtlas("ImperialMachinery", columns, rows);
            var corrupted = ReadDamageAtlas("ImperialWarpMachinery", columns, rows);
            var descriptors = CustomResources.Load<DescriptorsCollection>("DescriptorsCollections/mapobjects_descriptors");
            var donor = descriptors.Descriptors.OfType<MapObstacle>().First(obstacle =>
                obstacle.gameObject.GetComponent<Store>() != null && obstacle.Renderer != null && obstacle.BoxCollider != null);

            if (Data.ObstacleContainers.GetRecord(ImperialMachineContainer, ignoreLog: true) == null)
            {
                Data.ObstacleContainers.AddRecord(ImperialMachineContainer, new ObstacleContainerRecord
                {
                    StorageSize = new IntRange(5, 5),
                    AllowedItemClasses = [],
                });
            }

            var root = new GameObject("Imperial machinery templates");
            root.SetActive(false);
            UnityEngine.Object.DontDestroyOnLoad(root);
            for (int row = 0; row < machines.Length; row++)
            {
                var machine = machines[row];
                RegisterMachineDescriptor(descriptors, root.transform, donor, machine, clean, row, false);
                RegisterMachineDescriptor(descriptors, root.transform, donor, machine, corrupted, row, true);
            }
            ImperialMachines = machines;
        }

        private static void RegisterMachineDescriptor(DescriptorsCollection descriptors, Transform root, MapObstacle donor,
            ImperialMachine machine, Texture2D atlas, int row, bool warp)
        {
            string id = machine.Descriptor(warp);
            string caption = "iom.obstacle." + id;
            TranslationPatch.AddMachineCaption(caption, warp ? "Warp-corrupted " + machine.Name.ToLowerInvariant() : machine.Name);
            if (descriptors.TryGetDescriptor(id, out _))
            {
                return;
            }

            int width = machine.Width * ImperialTilePixels;
            int height = machine.Height * ImperialTilePixels;
            var stages = new Sprite[MachineDestructionStages.Length];
            for (int stage = 0; stage < stages.Length; stage++)
            {
                int x = stage * MachineSlotWidth * ImperialTilePixels;
                int y = atlas.height - row * MachineSlotHeight * ImperialTilePixels - height;
                stages[stage] = Sprite.Create(atlas, new Rect(x, y, width, height), Vector2.zero,
                    100f, 0, SpriteMeshType.FullRect);
                stages[stage].name = id + "_" + stage;
            }

            var prefab = new GameObject(id);
            prefab.transform.SetParent(root, false);
            prefab.layer = donor.gameObject.layer;
            var visual = new GameObject("Sprite");
            visual.transform.SetParent(prefab.transform, false);
            visual.layer = donor.Renderer.gameObject.layer;
            var renderer = visual.AddComponent<SpriteRenderer>();
            renderer.sharedMaterial = donor.Renderer.sharedMaterial;
            renderer.sortingLayerID = donor.Renderer.sortingLayerID;
            renderer.sortingOrder = donor.Renderer.sortingOrder;
            renderer.sprite = stages[0];
            var outline = visual.AddComponent<SpriteOutline>();
            var collider = prefab.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(width / 100f, height / 100f);
            collider.offset = collider.size * .5f;
            collider.isTrigger = donor.BoxCollider.isTrigger;

            var obstacle = prefab.AddComponent<MapObstacle>();
            obstacle.spRenderer = renderer;
            obstacle.spriteOutline = outline;
            obstacle.boxCollider = collider;
            AccessTools.Field(typeof(MapObstacle), "_overridenWidth").SetValue(obstacle, width);
            AccessTools.Field(typeof(MapObstacle), "_overridenHeight").SetValue(obstacle, height);
            AccessTools.Field(typeof(MapObstacle), "_occupyAlign").SetValue(obstacle, MapObstacle.OccupyAlign.Left);
            obstacle.SetRenderMode(MapObstacle.RenderMode.Flat);

            var health = prefab.AddComponent<ObstacleHealth>();
            AccessTools.Field(typeof(ObstacleHealth), "_maxHealth").SetValue(health, machine.Health);
            AccessTools.Field(typeof(ObstacleHealth), "_armor").SetValue(health, 12);
            AccessTools.Field(typeof(ObstacleHealth), "_destructionStages").SetValue(health, MachineDestructionStages);
            AccessTools.Field(typeof(ObstacleHealth), "_keepRemains").SetValue(health, true);
            AccessTools.Field(typeof(ObstacleHealth), "_makeRemainsPassable").SetValue(health, true);
            AccessTools.Field(typeof(ObstacleHealth), "_hitChanceOnShoot").SetValue(health, 70);
            AccessTools.Field(typeof(ObstacleHealth), "_pierceBlock").SetValue(health, 60);

            var store = prefab.AddComponent<Store>();
            AccessTools.Field(typeof(Store), "_containerId").SetValue(store, ImperialMachineContainer);
            AccessTools.Field(typeof(Store), "_filledSprites").SetValue(store, stages);
            AccessTools.Field(typeof(Store), "_emptySprites").SetValue(store, stages);
            AccessTools.Field(typeof(Store), "_captionTag").SetValue(store, caption);
            descriptors.AddDescriptor(id, obstacle);
        }

        private static void PlaceImperialMachinery(State state, bool warp)
        {
            if (ImperialMachines == null)
            {
                return;
            }
            var stationId = state.Get<RaidMetadata>().StationId;
            var eligible = ImperialMachines.Where(machine => Array.IndexOf(machine.Sites, stationId) >= 0).ToArray();
            if (eligible.Length == 0)
            {
                return;
            }

            var map = state.Get<MapGrid>();
            var metadata = state.Get<MapMetadata>();
            var obstacles = state.Get<MapObstacles>();
            var reserved = new HashSet<CellPosition>(obstacles.Obstacles
                .Where(obstacle => obstacle.Occupy || obstacle.Interactable || obstacle.IsAttackable)
                .SelectMany(obstacle => obstacle.OccupiedCells));
            reserved.UnionWith(metadata.LevelEntries.Select(entry => entry.Position));
            reserved.UnionWith(metadata.LevelExits.Select(exit => exit.Position));
            var generatedRooms = state.Get<LocationMetadata>().GeneratedRooms;
            var workrooms = generatedRooms
                .Where(room => (room.GroupId is -1 or 2 or 15) && !IsImperialSupplyClosetPreset(room.PresetBinaryId))
                .Select(room => (Room: room, Area: ClassifyNativePresetArea(room, obstacles.Obstacles))).ToArray();
            var factory = SingletonMonoBehaviour<MapObstacleFactory>.Instance;
            bool placed = false;
            foreach (var machine in eligible.OrderByDescending(machine => machine.Width * machine.Height))
            {
                var candidates = workrooms
                    .Select(room => (room.Room, Priority: MachineRoomPriority(machine, room.Area)))
                    .Where(room => room.Priority > 0)
                    .OrderByDescending(room => room.Priority)
                    .ThenByDescending(room => (room.Room.RightUpperCorner.X - room.Room.LeftLowerCorner.X) *
                        (room.Room.RightUpperCorner.Y - room.Room.LeftLowerCorner.Y))
                    .Select(room => room.Room);
                if (!TryPlaceMachine(map, reserved, machine, candidates, generatedRooms.Count == 0, out var pos))
                {
                    continue;
                }
                var obstacle = factory.Spawn(machine.Descriptor(warp), pos, CellPosition.Zero);
                if (obstacle == null)
                {
                    continue;
                }
                reserved.UnionWith(obstacle.OccupiedCells);
                // The pathfinder is initialized later in DungeonBuilder.Create.
                MapSystem.RefreshMapObstacleOccupation(map, obstacles);
                placed = true;
                var item = SingletonMonoBehaviour<ItemFactory>.Instance.CreateForInventory(machine.Output, isSingleItem: true);
                if (item != null && obstacle.Store.storage.TryPutItem(item, CellPosition.Zero))
                {
                    obstacle.Store.RefreshVisual();
                }
            }
            if (placed)
            {
                MapSystem.MarkReachableCells(map, obstacles, metadata);
                MapSystem.CalculateOneWideCorridors(map);
                MapSystem.CalculateObstaclesExposedCells(obstacles);
            }
        }

        private static int MachineRoomPriority(ImperialMachine machine, NativePresetArea area) => area switch
        {
            NativePresetArea.Relay when machine.Key is "power_relay" or "cell_line" => 4,
            NativePresetArea.Laboratory when machine.Key is "vat_bank" or "cell_line" => 4,
            NativePresetArea.Workshop => 3,
            NativePresetArea.Cargo when machine.Key is "assembly_gantry" or "power_relay" => 2,
            NativePresetArea.General => 1,
            _ => 0,
        };

        private static bool TryPlaceMachine(MapGrid grid, HashSet<CellPosition> reserved, ImperialMachine machine,
            IEnumerable<GeneratedRoomMetadata> rooms, bool staticLocation, out CellPosition position)
        {
            foreach (var room in rooms)
            {
                if (TryPlaceMachineInBounds(grid, reserved, machine, room.LeftLowerCorner, room.RightUpperCorner, out position))
                    return true;
            }
            position = CellPosition.Zero;
            return staticLocation && TryPlaceMachineInBounds(grid, reserved, machine, CellPosition.Zero,
                new CellPosition(grid.MaxWidth - 1, grid.MaxHeight - 1), out position);
        }

        private static bool TryPlaceMachineInBounds(MapGrid grid, HashSet<CellPosition> reserved, ImperialMachine machine,
            CellPosition lower, CellPosition upper,
            out CellPosition position)
        {
            position = CellPosition.Zero;
            bool found = false;
            var fallback = new List<CellPosition>();
            for (int y = Math.Min(upper.Y - machine.Height, grid.MaxHeight - machine.Height - 1);
                y >= Math.Max(1, lower.Y + 1); y--)
            {
                for (int x = Math.Max(1, lower.X + 1);
                    x + machine.Width <= upper.X && x + machine.Width < grid.MaxWidth; x++)
                {
                    int right = x + machine.Width;
                    int north = y + machine.Height;
                    if (!grid.GetCell(x, y - 1).ReachableCellFlag)
                    {
                        continue;
                    }
                    bool againstWall = true;
                    for (int col = x - 1; col <= right; col++)
                    {
                        againstWall &= grid.GetCell(col, north).Type is MapCellType.Wall or MapCellType.FacadeWall;
                    }
                    if (!MachineAreaClear(grid, reserved, x, y, right - 1, north - 1))
                    {
                        continue;
                    }
                    if (!MachineAreaClear(grid, reserved, x - 1, y - 1, right, againstWall ? north - 1 : north))
                    {
                        fallback.Add(new CellPosition(x, y));
                        continue;
                    }
                    position = new CellPosition(x, y);
                    if (againstWall)
                    {
                        return true;
                    }
                    found = true;
                }
            }
            if (found)
            {
                return true;
            }
            foreach (var candidate in fallback)
            {
                if (KeepsMachineAislesConnected(grid, candidate, machine.Width, machine.Height))
                {
                    position = candidate;
                    return true;
                }
            }
            return false;
        }

        private static bool MachineAreaClear(MapGrid grid, HashSet<CellPosition> reserved, int left, int bottom, int right, int top)
        {
            const ObstacleType unsuitable = ObstacleType.Obj | ObstacleType.ClosedDoor | ObstacleType.BlockedDoor
                | ObstacleType.Player | ObstacleType.Monster | ObstacleType.PlayerAlly;
            for (int row = bottom; row <= top; row++)
            {
                for (int col = left; col <= right; col++)
                {
                    var pos = new CellPosition(col, row);
                    var cell = grid.GetCell(pos);
                    if (cell.Type != MapCellType.Floor || cell.specialFlag != MapCellSpecialFlag.None ||
                        (cell.PassConditionMask & unsuitable) != 0 || reserved.Contains(pos) ||
                        (col < right && grid.GetFence(pos, new CellPosition(col + 1, row))) ||
                        (row < top && grid.GetFence(pos, new CellPosition(col, row + 1))))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private static readonly CellPosition[] MachineAisleSteps = [new(1, 0), new(-1, 0), new(0, 1), new(0, -1)];

        private static bool KeepsMachineAislesConnected(MapGrid grid, CellPosition origin, int width, int height)
        {
            bool InFootprint(CellPosition pos) => pos.X >= origin.X && pos.X < origin.X + width &&
                pos.Y >= origin.Y && pos.Y < origin.Y + height;
            bool IsAisle(CellPosition pos)
            {
                var cell = grid.GetCell(pos);
                return cell != null && !cell.IsObjBlockPass &&
                    (cell.IsFloor || cell.specialFlag == MapCellSpecialFlag.ClosedDoor);
            }

            var service = new CellPosition(origin.X, origin.Y - 1);
            if (!IsAisle(service) || grid.GetCell(service).Type != MapCellType.Floor || grid.GetFence(origin, service))
            {
                return false;
            }

            var exits = new HashSet<CellPosition>();
            for (int row = origin.Y; row < origin.Y + height; row++)
            for (int col = origin.X; col < origin.X + width; col++)
            {
                var cell = new CellPosition(col, row);
                foreach (var step in MachineAisleSteps)
                {
                    var adjacent = new CellPosition(col + step.X, row + step.Y);
                    if (!InFootprint(adjacent) && IsAisle(adjacent) && !grid.GetFence(cell, adjacent))
                    {
                        exits.Add(adjacent);
                    }
                }
            }

            var reachable = new HashSet<CellPosition> { service };
            var pending = new Queue<CellPosition>();
            pending.Enqueue(service);
            while (pending.Count > 0 && exits.Count > 0)
            {
                var cell = pending.Dequeue();
                exits.Remove(cell);
                foreach (var step in MachineAisleSteps)
                {
                    var adjacent = new CellPosition(cell.X + step.X, cell.Y + step.Y);
                    if (!InFootprint(adjacent) && IsAisle(adjacent) && !grid.GetFence(cell, adjacent) && reachable.Add(adjacent))
                    {
                        pending.Enqueue(adjacent);
                    }
                }
            }
            return exits.Count == 0;
        }

        [HarmonyPatch(typeof(Store), "ObstacleHealthOnValueChanged")]
        private static class ImperialMachineWreckSalvage
        {
            private static void Prefix(Store __instance, ObstacleHealth ____obstacleHealth, out bool __state)
            {
                __state = __instance.ContainerId == ImperialMachineContainer && ____obstacleHealth.Health == 0;
                if (__state)
                {
                    __instance.DropItemsOnHealthChanged = true;
                }
            }

            private static Exception Finalizer(Store __instance, bool __state, Exception __exception)
            {
                if (__state)
                {
                    __instance.DropItemsOnHealthChanged = false;
                }
                return __exception;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using MGSC;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private const int SpineFurnitureSpacing = 9;
        private const int SpineLightSpacing = 8;
        private const string SpineNorthLamp = "522e9d75-7def-4873-9068-0758718cbffa";
        private const string SpineWestLamp = "0525f7ca-0062-46c7-beec-6076add68296";
        private const string SpineEastLamp = "8986d69d-e407-4ab0-b25d-93ac0ee88f5b";

        private static readonly (string Use, string Id, Direction Facing)[] SpineFurniture =
        [
            ("vending", "be7c8a32-a5ba-4f2b-98a1-f35f708f7324", Direction.Down), // VendingMachine
            ("seating", "dccfa92e-71cd-4da4-9eb8-d34f6a8424b3", Direction.Down), // blue_sofa_1
            ("seating", "84a84b7b-1b3d-427c-a901-9224131add59", Direction.Up), // blue_sofa_2
            ("seating", "36e05f4e-0dfc-4b03-a17c-376e5f09197a", Direction.Right), // blue_sofa_3
            ("seating", "43008035-9be6-4c89-915e-026ee74bf09a", Direction.Left), // blue_sofa_4
            ("water", "f7a7fb36-7d75-4da5-92dd-7a9d95c09f1a", Direction.Down), // water_tank
            ("plants", "0d8d942c-a06f-483e-9a56-1df84ebc3192", Direction.Center), // flowersContainer
            ("plants", "ab0198c6-f74a-4760-8047-77ed26c79969", Direction.Center), // flowersContainer_2
            ("crates", "759c8b84-b1bb-4166-9cde-d965c1733489", Direction.Center), // IndusBox_1
            ("crates", "81d3759e-5b2e-45a7-9de5-d591a983a339", Direction.Center), // box_4
            ("crates", "9abc0f1b-04fb-4b93-9a40-65d116822869", Direction.Center), // woddenBox_1
            ("cargo", "b2a4c183-e8fa-4d00-8121-57e976b80d38", Direction.Center), // IndusBigContainer_1
            ("cargo", "d2900be4-1bcc-4b71-9127-b2b8653f57cb", Direction.Center), // container_2
            ("cargo", "df231734-0970-4333-8dc4-e3d5d0e5d18c", Direction.Center), // container_1
        ];

        private static readonly (string Use, string Id, Direction Facing)[] SpineStorageClutter =
        [
            ("barrels", "f3eaee3f-ef73-484b-ac87-b19ce1b8f368", Direction.Center), // emptyBarrel_1
            ("barrels", "c5d3c11d-07ff-468e-843f-ce263096b61f", Direction.Center), // emptyBarrel_2
            ("bins", "51954a57-6239-409e-8d59-84c8eace9171", Direction.Center), // Metal_Trash_Can
            ("plants", "0d8d942c-a06f-483e-9a56-1df84ebc3192", Direction.Center), // flowersContainer
            ("plants", "ab0198c6-f74a-4760-8047-77ed26c79969", Direction.Center), // flowersContainer_2
            ("pallet", "17935702-39ef-4de8-a1cb-a60413f089bf", Direction.Center),
            ("carton", "ab9f85de-8926-4f6c-ba95-f74c296165e7", Direction.Center), // box_5
            ("carton", "2fb21900-c31b-434f-9090-0d0cee06eb24", Direction.Center), // box_6
            ("crate", "f7f9dff0-f908-46b1-91e0-d06411b0d206", Direction.Center), // woddenBox_2
            ("crate", "930b4b81-d37d-45b7-b92d-dd83bd356e54", Direction.Center), // woddenBox_3
            ("crate", "7ba75837-a5e1-4255-a3c5-ee6830f69adb", Direction.Center), // IndusBox_2
            ("crate", "bd6b8888-1810-4d11-89e1-ea89baac4ea1", Direction.Center), // IndusBox_3
            ("cargo", "39e02f85-e47d-459f-93ab-2603cc37df4f", Direction.Center), // IndusBigContainer_2
            ("cargo", "f30fff47-35a1-45dd-8df7-169d9ccd0804", Direction.Center), // container_4
        ];

        private static readonly string[][] SpineLooseSalvage =
        [
            ["paper_towel", "tin", "glass_bottle", "plastic"],
            ["rusty_plates", "metal_tread", "rod_parts", "coarse_parts"],
            ["wire", "capacitor_parts", "tape", "rubber", "microelectronics_parts"],
            ["rags", "glass", "plastic", "paper_towel", "tin"],
        ];
        private static readonly string[] SpinePackagedSupplies = ["regular_food_container", "water_container", "worker_clothing_container"];

        private sealed class SpineProp(string use, string id, CellPosition facing, CellPosition offset, CellPosition[] cells)
        {
            internal readonly string Use = use;
            internal readonly string Id = id;
            internal readonly CellPosition Facing = facing;
            internal readonly CellPosition Offset = offset;
            internal readonly CellPosition[] Cells = cells;
        }

        private static void DressImperialSpines(MapGrid grid, MapMetadata metadata, ImperialSpinePlan layout, bool warp, int seed)
        {
            var runs = layout.Spines.Concat(layout.Elbows.SelectMany(run =>
                new[] { (Start: run.Start, End: run.Bend), (Start: run.Bend, End: run.End) })).ToArray();
            var protectedLandings = SpineDoorLandings(layout);
            var reserved = new HashSet<CellPosition>(protectedLandings);
            foreach (var door in layout.Doors.Where(door => !IsImperialSupplyClosetPreset(door.Room.header.binaryId)))
            foreach (var threshold in door.Opening)
            for (int y = -3; y <= 3; y++)
            for (int x = -3; x <= 3; x++)
                reserved.Add(threshold + new CellPosition(x, y));
            foreach (var run in runs)
            {
                var step = AxisStep(run.Start, run.End);
                int length = SpineManhattan(run.Start, run.End);
                for (int distance = 0; distance <= length; distance++)
                    reserved.Add(run.Start + step * distance);
            }

            var props = LoadSpinePropFootprints(SpineFurniture);
            var propKinds = new Dictionary<string, int>();
            var propIds = new Dictionary<string, int>();
            var placed = new HashSet<CellPosition>();
            var lamps = new HashSet<CellPosition>();
            var random = new System.Random(seed);
            foreach (var run in runs)
            {
                var step = AxisStep(run.Start, run.End);
                var across = new CellPosition(Math.Abs(step.Y), Math.Abs(step.X));
                int length = SpineManhattan(run.Start, run.End);
                int side = random.Next(2) == 0 ? -1 : 1;
                // Each run gets stations with several empty tiles between them. Endpoints and bends stay open.
                for (int station = 5; station <= length - 5; station += SpineFurnitureSpacing)
                {
                    var candidates = props.OrderBy(prop => propKinds.TryGetValue(prop.Use, out int count) ? count : 0)
                        .ThenBy(prop => propIds.TryGetValue(prop.Id, out int count) ? count : 0).ToArray();
                    if (PlaceSpineProp(grid, metadata, layout.CorridorCells, reserved, placed, candidates,
                        run.Start, step, across, length, station, side, out var prop))
                    {
                        propKinds.TryGetValue(prop.Use, out int count);
                        propKinds[prop.Use] = count + 1;
                        propIds.TryGetValue(prop.Id, out count);
                        propIds[prop.Id] = count + 1;
                        side = -side;
                    }
                }

                for (int distance = 4; distance <= length - 4; distance += SpineLightSpacing)
                {
                    var center = run.Start + step * distance;
                    int mountSide = across.Y != 0 ? -1 : side;
                    foreach (int shift in new[] { 0, 1, -1 })
                    {
                        if (AddSpineLamp(grid, metadata, center + step * shift, across, mountSide, warp, lamps))
                            break;
                    }
                    side = -side;
                }
            }

            var (clusters, loose, storage) = DressSpineSalvage(grid, metadata, layout, runs, placed, protectedLandings, random);
            Debug.Log("[Cvar_ImperiumOfMan] Branch hall detail " + string.Join(", ",
                propKinds.Select(entry => $"{entry.Key}={entry.Value}")) +
                $", wall lamps={lamps.Count}, salvage clusters={clusters}, loose items={loose}, storage clutter={storage}");
        }

        private static (int Clusters, int Loose, int Storage) DressSpineSalvage(MapGrid grid, MapMetadata metadata,
            ImperialSpinePlan layout, (CellPosition Start, CellPosition End)[] runs, HashSet<CellPosition> placed,
            HashSet<CellPosition> protectedLandings, System.Random random)
        {
            var spillProfiles = SpineLooseSalvage.Select(group => group.Where(id => Data.Items.GetRecord(id) != null).ToArray())
                .Where(group => group.Length > 0).ToArray();
            var supplies = SpinePackagedSupplies.Where(id => Data.Items.GetRecord(id) != null).ToArray();
            if (spillProfiles.Length == 0) return default;

            var boxes = LoadSpinePropFootprints(SpineStorageClutter).Where(prop => prop.Cells.Length <= 2).ToArray();
            var furnishingPositions = metadata.MapObstacles
                .Where(obj => layout.CorridorCells.Contains(obj.Pos) && SpineFurniture.Any(prop => prop.Id == obj.ObstacleName))
                .Select(obj => (obj.Pos, Use: SpineFurniture.First(prop => prop.Id == obj.ObstacleName).Use))
                .ToArray();
            var looseCells = metadata.Items.Select(item => item.Position).ToHashSet();
            var propUse = new Dictionary<string, int>();
            var storageKinds = new Dictionary<string, int>();
            int clusters = 0, loose = 0, storage = 0, packages = 0;
            foreach (var (start, end) in runs)
            {
                var step = AxisStep(start, end);
                var across = new CellPosition(Math.Abs(step.Y), Math.Abs(step.X));
                int length = SpineManhattan(start, end);
                int side = random.Next(2) == 0 ? -1 : 1;
                for (int station = random.Next(2, 5); station <= length - 2; station += random.Next(4, 8))
                {
                    foreach (int lane in new[] { side, -side })
                    {
                        var pos = start + step * station + across * lane;
                        if (!AvailableEdge(pos)) continue;
                        int kind = random.Next(spillProfiles.Length);
                        var nearest = furnishingPositions.Where(prop => SpineManhattan(prop.Pos, pos) <= 6)
                            .OrderBy(prop => SpineManhattan(prop.Pos, pos)).FirstOrDefault();
                        if (spillProfiles.Length == SpineLooseSalvage.Length)
                        {
                            if (nearest.Use is "vending" or "water" or "seating") kind = 0;
                            if (nearest.Use == "crates") kind = 1;
                            if (nearest.Use == "cargo") kind = 2;
                        }
                        var stock = spillProfiles[kind];
                        string first = stock[random.Next(stock.Length)];
                        AddLoose(first, pos);
                        clusters++;

                        // Leave the next couple of clusters light between groups of stowed cargo.
                        bool cargoCluster = (clusters & 1) == 0 && TryStoreNear(pos, lane, station);
                        var spill = pos - step;
                        if (!AvailableEdge(spill) || station <= 2) spill = pos + step;
                        if (station < length - 2 && AvailableEdge(spill))
                        {
                            var otherItems = stock.Where(id => id != first).DefaultIfEmpty(first).ToArray();
                            string extra = otherItems[random.Next(otherItems.Length)];
                            if (cargoCluster && packages < 4 && supplies.Length > 0 && random.Next(3) == 0)
                            {
                                extra = supplies[random.Next(supplies.Length)];
                                packages++;
                            }
                            AddLoose(extra, spill);
                        }
                        side = -side;
                        break;
                    }
                }

                bool AvailableEdge(CellPosition pos) => layout.CorridorCells.Contains(pos) &&
                    grid.GetCell(pos)?.Type == MapCellType.Floor && !protectedLandings.Contains(pos) &&
                    !placed.Contains(pos) && !looseCells.Contains(pos);

                bool TryStoreNear(CellPosition pos, int lane, int station)
                {
                    if (station < 4 || station > length - 4) return false;
                    foreach (int distance in new[] { 1, 2, -1, -2 })
                    foreach (var prop in boxes.OrderBy(StoragePriority)
                        .ThenBy(prop => propUse.TryGetValue(prop.Id, out int count) ? count : 0))
                    {
                        var anchor = pos + step * distance;
                        var footprint = prop.Cells.Select(offset => anchor + offset).ToArray();
                        if (footprint.Any(cell => !FitsStorage(cell))) continue;

                        StampSpineProp(metadata, placed, prop, anchor, footprint);
                        propUse.TryGetValue(prop.Id, out int uses);
                        propUse[prop.Id] = uses + 1;
                        storageKinds.TryGetValue(prop.Use, out int kindCount);
                        storageKinds[prop.Use] = kindCount + 1;
                        storage++;
                        return true;
                    }
                    return false;

                    bool FitsStorage(CellPosition cell)
                    {
                        var relative = cell - start;
                        int along = relative.X * step.X + relative.Y * step.Y;
                        int acrossLane = relative.X * across.X + relative.Y * across.Y;
                        var opposite = cell - across * (2 * lane);
                        return acrossLane == lane && along >= 3 && along <= length - 3 &&
                            AvailableEdge(cell) && !placed.Contains(opposite) &&
                            !placed.Contains(cell - step) && !placed.Contains(cell + step);
                    }
                }
            }
            Debug.Log("[Cvar_ImperiumOfMan] Branch hall storage " + string.Join(", ",
                storageKinds.Select(entry => $"{entry.Key}={entry.Value}")));
            return (clusters, loose, storage);

            int StoragePriority(SpineProp prop)
            {
                storageKinds.TryGetValue(prop.Use, out int count);
                return (prop.Use is "barrels" or "bins" or "plants") && count < 2 ? 0 : 1;
            }

            void AddLoose(string item, CellPosition pos)
            {
                metadata.AddItem(item, pos.X, pos.Y);
                looseCells.Add(pos);
                loose++;
            }
        }

        private static HashSet<CellPosition> SpineDoorLandings(ImperialSpinePlan layout)
        {
            var landings = new HashSet<CellPosition>();
            foreach (var door in layout.Doors)
            {
                var step = ImperialSpineStep(door.Outward);
                var across = new CellPosition(Math.Abs(step.Y), Math.Abs(step.X));
                var approach = (door.Outward == Direction.Down ? door.Position : door.Joint) + step;
                for (int depth = 0; depth <= 1; depth++)
                for (int width = -1; width <= 1; width++)
                    landings.Add(approach + step * depth + across * width);
            }
            return landings;
        }

        private static List<SpineProp> LoadSpinePropFootprints(IEnumerable<(string Use, string Id, Direction Facing)> catalog)
        {
            var props = new List<SpineProp>();
            var factory = SingletonMonoBehaviour<MapObstacleFactory>.Instance;
            var holder = new GameObject("Imperial corridor furniture footprint check");
            holder.SetActive(false);
            try
            {
                foreach (var (use, id, facing) in catalog)
                {
                    var prefab = factory.GetObstaclePrefab(id);
                    if (prefab == null) continue;
                    var sample = UnityEngine.Object.Instantiate(prefab, holder.transform, false);
                    // Use the same sprite size, alignment, and occupation overrides as the spawned object.
                    foreach (var offset in new[] { CellPosition.Zero, new CellPosition(4, 0),
                        new CellPosition(0, 4), new CellPosition(6, 6), new CellPosition(14, 5) })
                    {
                        sample.InitializeForGameEditor(CellPosition.Zero, offset);
                        var cells = sample.OccupiedCells.Append(CellPosition.Zero).Distinct().ToArray();
                        if (cells.Length <= 3) props.Add(new SpineProp(use, id, ImperialSpineStep(facing), offset, cells));
                    }
                }
            }
            finally
            {
                UnityEngine.Object.Destroy(holder);
            }
            return props;
        }

        private static bool PlaceSpineProp(MapGrid grid, MapMetadata metadata, HashSet<CellPosition> floor,
            HashSet<CellPosition> reserved, HashSet<CellPosition> placed, SpineProp[] props,
            CellPosition start, CellPosition step, CellPosition across, int length, int station, int side, out SpineProp chosen)
        {
            foreach (var prop in props)
            foreach (int lane in new[] { side, -side })
            foreach (int shift in new[] { 0, 1, -1, 2, -2 })
            {
                var outer = across * lane;
                if (prop.Facing.Equals(outer) || prop.Use == "seating" && !prop.Facing.Equals(outer * -1))
                    continue;
                var pos = start + step * (station + shift) + across * lane;
                var footprint = prop.Cells.Select(cell => cell + pos).ToArray();
                if (footprint.Any(cell => !CanPlace(cell))) continue;

                StampSpineProp(metadata, placed, prop, pos, footprint);
                chosen = prop;
                return true;

                bool CanPlace(CellPosition cell)
                {
                    var relative = cell - start;
                    int along = relative.X * step.X + relative.Y * step.Y;
                    int acrossLane = relative.X * across.X + relative.Y * across.Y;
                    return acrossLane == lane && along >= 4 && along <= length - 4 &&
                        floor.Contains(cell) && !reserved.Contains(cell) &&
                        grid.GetCell(cell)?.Type == MapCellType.Floor &&
                        !placed.Any(other => SpineManhattan(other, cell) <= 3);
                }
            }
            chosen = null;
            return false;
        }

        private static void StampSpineProp(MapMetadata metadata, HashSet<CellPosition> placed, SpineProp prop,
            CellPosition pos, IEnumerable<CellPosition> footprint)
        {
            metadata.AddObject(new MapObjectMetadata
            {
                UniqueId = metadata.GetNextObjIdAndIncreaseCounter(),
                ObstacleName = prop.Id,
                Pos = pos,
                Offset = prop.Offset
            });
            placed.UnionWith(footprint);
        }

        private static bool AddSpineLamp(MapGrid grid, MapMetadata metadata, CellPosition center, CellPosition across,
            int side, bool warp, HashSet<CellPosition> lamps)
        {
            var mount = center + across * (side * 2);
            var cell = grid.GetCell(mount);
            if (cell?.IsWallSurfaceType != true || lamps.Contains(mount)) return false;

            string id = across.Y != 0 ? SpineNorthLamp : side < 0 ? SpineWestLamp : SpineEastLamp;
            if (SingletonMonoBehaviour<MapObstacleFactory>.Instance.GetObstaclePrefab(id) == null)
                return false;
            // Match the stock fixtures' offsets so their emitters sit just outside the wall's shadow.
            var offset = across.Y != 0 ? new CellPosition(14, 27) : new CellPosition(side < 0 ? 29 : -1, 5);
            metadata.AddLight(new MapLightMetadata
            {
                UniqueId = metadata.GetNextObjIdAndIncreaseCounter(),
                ObstacleName = id,
                Pos = mount,
                Offset = offset,
                lightType = Light2D.LightType.Point,
                intensity = warp ? 0.58f : 0.72f,
                distance = 6,
                color = warp ? new Color(1f, 0.49f, 0.34f) : new Color(1f, 0.86f, 0.60f)
            });
            lamps.Add(mount);
            return true;
        }
    }
}

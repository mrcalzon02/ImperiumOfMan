using System;
using System.Collections.Generic;
using System.Linq;
using MGSC;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private const int ImperialSupplyClosetCount = 12;
        private const int ImperialSupplyClosetPresetId = -281000;
        private const short ImperialSupplyClosetGroupId = ExordiumDungeonGenerator.NoMonsterSpawnGroupId;
        private static readonly Direction[] SupplyClosetDirections = [Direction.Up, Direction.Down, Direction.Left, Direction.Right];

        private static bool IsImperialSupplyClosetPreset(int presetId) =>
            presetId <= ImperialSupplyClosetPresetId && presetId > ImperialSupplyClosetPresetId - ImperialSupplyClosetCount;

        private readonly struct SupplyClosetSite(PresetHeader template, int spine, Direction outward,
            CellPosition joint, CellPosition origin, CellPosition far, CellPosition doorPosition, int preference)
        {
            internal readonly PresetHeader Template = template;
            internal readonly int Spine = spine;
            internal readonly Direction Outward = outward;
            internal readonly CellPosition Joint = joint;
            internal readonly CellPosition Origin = origin;
            internal readonly CellPosition Far = far;
            internal readonly CellPosition DoorPosition = doorPosition;
            internal readonly int Preference = preference;
        }

        private static IReadOnlyDictionary<Direction, PresetHeader[]> CreateImperialSupplyClosetTemplates(
            IReadOnlyList<PresetHeader> donors)
        {
            var samples = new Dictionary<MapCellType, MapCell>();
            foreach (var donor in donors.Where(CanBuildSpineRoom))
            {
                var map = donor.MapGrid;
                for (int y = 0; y < map.MaxHeight; y++)
                for (int x = 0; x < map.MaxWidth; x++)
                {
                    var tile = map.GetCell(x, y);
                    if (tile.Type is MapCellType.Floor or MapCellType.Wall or MapCellType.FacadeWall &&
                        !samples.ContainsKey(tile.Type)) samples.Add(tile.Type, tile);
                }
                if (samples.Count == 3) break;
            }

            var previousFenceCheck = MapCell.FenceCheck;
            try
            {
                return SupplyClosetDirections.ToDictionary(direction => direction, direction => new[]
                {
                    BuildImperialSupplyClosetTemplate(samples, direction, roomWidth: 4),
                    BuildImperialSupplyClosetTemplate(samples, direction, roomWidth: 5)
                });
            }
            finally
            {
                MapCell.FenceCheck = previousFenceCheck;
            }
        }

        private static PresetHeader BuildImperialSupplyClosetTemplate(IReadOnlyDictionary<MapCellType, MapCell> samples,
            Direction outward, int roomWidth)
        {
            const int roomHeight = 6;
            var grid = new MapGrid();
            grid.Resize(roomWidth, roomHeight, Direction.Center, out _, out _);
            for (int y = 0; y < roomHeight; y++)
            for (int x = 0; x < roomWidth; x++)
            {
                var cell = grid.GetCell(x, y);
                cell.X = (byte)x;
                cell.Y = (byte)y;
                cell.Type = x == 0 || x == roomWidth - 1 || y == 0 || y == roomHeight - 1
                    ? MapCellType.Wall : y == roomHeight - 2 ? MapCellType.FacadeWall : MapCellType.Floor;
                if (samples.TryGetValue(cell.Type, out var native))
                {
                    cell.SetTilesetId(1, native.GetTilesetId(1));
                    cell.SetTileIndex(1, native.GetTileIndex(1));
                }
            }

            var joint = outward switch
            {
                Direction.Up => new CellPosition(roomWidth / 2, roomHeight - 1),
                Direction.Down => new CellPosition(roomWidth / 2, 0),
                Direction.Left => new CellPosition(0, 2),
                _ => new CellPosition(roomWidth - 1, 2)
            };
            return new PresetHeader
            {
                presetType = PresetType.Room,
                MapGrid = grid,
                mapMetadata = new MapMetadata(),
                genMetadata = new MapGenMetadata(grid)
                {
                    AllowedJoints = [joint],
                    UpperJoints = outward == Direction.Up ? [joint] : [],
                    BottomJoints = outward == Direction.Down ? [joint] : [],
                    LeftJoints = outward == Direction.Left ? [joint] : [],
                    RightJoints = outward == Direction.Right ? [joint] : []
                }
            };
        }

        private static bool AddImperialSupplyClosets(ImperialSpinePlan plan,
            IReadOnlyDictionary<Direction, PresetHeader[]> templates, int width, int height, int seed)
        {
            var runs = plan.Spines.Select((run, index) => (run.Start, run.End, Spine: index))
                .Concat(plan.Elbows.SelectMany((run, index) => new[]
                {
                    (run.Start, End: run.Bend, Spine: index + plan.Spines.Length),
                    (Start: run.Bend, run.End, Spine: index + plan.Spines.Length)
                }));
            var doorLocations = plan.Doors.Select(door => door.Position).ToList();
            var unavailable = plan.CorridorCells.ToHashSet();
            foreach (var door in plan.Doors) unavailable.UnionWith(door.Opening);
            foreach (var room in plan.Rooms) ReserveRoom(room.Room.LeftLowerCorner, room.Room.RightUpperCorner);

            var random = new Random(seed);
            var candidates = new List<SupplyClosetSite>();
            foreach (var (start, end, spine) in runs)
            {
                var along = new CellPosition(Math.Sign(end.X - start.X), Math.Sign(end.Y - start.Y));
                int length = Math.Abs(end.X - start.X) + Math.Abs(end.Y - start.Y);
                foreach (var outward in SupplyClosetDirections)
                {
                    var towardHall = ImperialSpineStep(outward);
                    if (along.X * towardHall.X + along.Y * towardHall.Y != 0) continue;
                    int wallDepth = outward == Direction.Down ? 2 : 1;
                    for (int distance = 4; distance <= length - 4; distance++)
                    {
                        var joint = start + along * distance - towardHall * (ImperialSpineHalfWidth + wallDepth);
                        var door = IsSpineVertical(outward) ? joint + new CellPosition(0, -1) : joint;
                        var approach = FirstSpineFloor(joint, outward);
                        if (!plan.CorridorCells.Contains(approach) || !plan.CorridorCells.Contains(approach + towardHall))
                            continue;
                        foreach (var template in templates[outward])
                        {
                            var origin = joint - template.genMetadata.AllowedJoints[0];
                            var far = origin + new CellPosition(template.MapGrid.MaxWidth - 1, template.MapGrid.MaxHeight - 1);
                            var candidate = new SupplyClosetSite(template, spine, outward, joint, origin, far, door, random.Next());
                            if (IsFree(candidate) && ClosestDoorDistance(candidate) > 3) candidates.Add(candidate);
                        }
                    }
                }
            }

            var bySpine = new int[plan.Spines.Length + plan.Elbows.Count];
            var byDirection = SupplyClosetDirections.ToDictionary(direction => direction, _ => 0);
            var firstDirections = SupplyClosetDirections.OrderBy(direction => candidates.Count(site => site.Outward == direction))
                .ToArray();
            for (int index = 0; index < ImperialSupplyClosetCount; index++)
            {
                var chosen = candidates.Where(site => index >= firstDirections.Length || site.Outward == firstDirections[index])
                    .Where(site => ClosestDoorDistance(site) > 3 && IsFree(site))
                    .OrderBy(site => bySpine[site.Spine])
                    .ThenBy(site => byDirection[site.Outward])
                    .ThenByDescending(site => ClosestDoorDistance(site))
                    .ThenBy(site => site.Preference)
                    .FirstOrDefault(site => SpineExtentFits(plan, [site.Origin, site.Far, site.DoorPosition], width, height));
                if (chosen.Template == null) return false;

                var header = new PresetHeader
                {
                    binaryId = ImperialSupplyClosetPresetId - index,
                    presetType = PresetType.Room,
                    MapGrid = chosen.Template.MapGrid,
                    mapMetadata = chosen.Template.mapMetadata,
                    genMetadata = chosen.Template.genMetadata
                };
                var source = new ImperialSpineRoom(new Room { header = header }, ImperialSupplyClosetGroupId);
                var room = AddSpineRoom(plan, source, chosen.Origin);
                var doorway = new ImperialSpineDoor(room, chosen.Joint, chosen.Outward, chosen.Spine, end: false);
                plan.Doors.Add(doorway);
                ReserveRoom(chosen.Origin, chosen.Far);
                unavailable.UnionWith(doorway.Opening);
                doorLocations.Add(doorway.Position);
                bySpine[chosen.Spine]++;
                byDirection[chosen.Outward]++;
            }
            return true;

            int ClosestDoorDistance(SupplyClosetSite site) =>
                doorLocations.Min(door => Math.Abs(door.X - site.DoorPosition.X) + Math.Abs(door.Y - site.DoorPosition.Y));

            bool IsFree(SupplyClosetSite site)
            {
                if (unavailable.Contains(site.DoorPosition)) return false;
                for (int y = site.Origin.Y; y <= site.Far.Y; y++)
                for (int x = site.Origin.X; x <= site.Far.X; x++)
                    if (unavailable.Contains(new CellPosition(x, y))) return false;
                return true;
            }

            void ReserveRoom(CellPosition low, CellPosition far)
            {
                for (int y = low.Y - 1; y <= far.Y + 1; y++)
                for (int x = low.X - 1; x <= far.X + 1; x++)
                    unavailable.Add(new CellPosition(x, y));
            }
        }
    }
}

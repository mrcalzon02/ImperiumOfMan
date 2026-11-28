using System;
using System.Collections.Generic;
using System.Linq;
using MGSC;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private const int MaximumCampaignCombatRooms = 16;
        private static readonly int[] CampaignOreBayPresets = [2129, 2211, 3532, 1132, 1078, 1101, 1093, 1099];

        private static PresetHeader[] CampaignCombatDonors(CampaignRouteLayout route, PresetLoader loader, PresetHeader[] donors)
        {
            if (route.Key is not ("caloris_levy" or "cinder_basin")) return donors;

            var donorIds = new HashSet<int>(donors.Select(header => header.binaryId));
            // Narrow ore galleries leave frontage for deep machinery bays and ore handling rooms.
            return donors.Concat(CampaignOreBayPresets.Select(PresetIdRegistry.NormalizePresetId)
                .Where(donorIds.Add).Select(loader.LoadSpecificPreset).Where(IsImperialSpineDonor)).ToArray();
        }

        private static int CampaignPlanFloorCount(ImperialSpinePlan layout) => layout.CorridorCells.Count +
            layout.Rooms.Sum(placement => NativeRoomFloorCount(placement.Room.header));

        private static int NativeRoomFloorCount(PresetHeader header)
        {
            var grid = header.MapGrid;
            int count = 0;
            for (int y = 0; y < grid.MaxHeight; y++)
            for (int x = 0; x < grid.MaxWidth; x++)
            {
                var cell = grid.GetCell(x, y);
                if ((cell.Type is MapCellType.Floor or MapCellType.LowGround) && cell.LiquidLevel == 0 &&
                    !cell.IsObjBlockPass) count++;
            }
            return count;
        }

        private static void AddCampaignEndgameCombatRooms(ImperialSpinePlan plan, PresetHeader[] donors,
            CampaignRouteLayout route, int targetFloors, int width, int height,
            int maximumRooms = MaximumCampaignCombatRooms, bool preferUnused = false)
        {
            int floorCount = CampaignPlanFloorCount(plan);
            if (floorCount >= targetFloors) return;

            var combatPresets = donors.Select(header => (Header: header, Floor: NativeRoomFloorCount(header)))
                .Where(room => room.Floor >= 48 && room.Header.MapGrid.MaxWidth >= 7 && room.Header.MapGrid.MaxHeight >= 7)
                .ToArray();
            if (combatPresets.Length == 0) return;

            var occupied = plan.CorridorCells.ToHashSet();
            foreach (var door in plan.Doors) occupied.UnionWith(door.Opening);
            var runs = plan.Spines.Select((run, index) => (run.Start, run.End, Spine: index))
                .Concat(plan.Elbows.SelectMany((elbow, index) => new[]
                {
                    (elbow.Start, End: elbow.Bend, Spine: plan.Spines.Length + index),
                    (Start: elbow.Bend, elbow.End, Spine: plan.Spines.Length + index)
                })).ToArray();
            var addedBySpine = new int[plan.Spines.Length + plan.Elbows.Count];
            for (int roomIndex = 0; roomIndex < maximumRooms && floorCount < targetFloors; roomIndex++)
            {
                var uses = plan.Rooms.GroupBy(room => room.Room.header.binaryId)
                    .ToDictionary(group => group.Key, group => group.Count());
                var candidates = combatPresets
                    .Where(room => !preferUnused || !uses.ContainsKey(room.Header.binaryId))
                    .OrderBy(room => uses.TryGetValue(room.Header.binaryId, out int count) && count >= 2)
                    .ThenByDescending(room => Math.Min(room.Floor, targetFloors - floorCount))
                    .ThenByDescending(room => route?.RoomPreference(room.Header, CampaignRouteRoomRole.SideRoom) ?? 0)
                    .ThenBy(room => uses.TryGetValue(room.Header.binaryId, out int count) ? count : 0).ToArray();
                bool added = false;
                foreach (var (start, end, spine) in runs.OrderBy(run => addedBySpine[run.Spine]))
                {
                    if (!TryOpenCombatRoom(start, end, spine, candidates, out int addedFloor)) continue;
                    floorCount += addedFloor;
                    addedBySpine[spine]++;
                    added = true;
                    break;
                }
                if (!added) break;
            }
            CenterImperialSpines(plan, width, height);

            bool TryOpenCombatRoom(CellPosition start, CellPosition end, int spine,
                (PresetHeader Header, int Floor)[] candidates, out int addedFloor)
            {
                var along = AxisStep(start, end);
                int length = SpineManhattan(start, end);
                var sides = along.X == 0 ? new[] { Direction.Left, Direction.Right } : new[] { Direction.Up, Direction.Down };
                foreach (var candidate in candidates)
                foreach (var outward in sides)
                {
                    var joints = SpineJoints(candidate.Header, outward).ToArray();
                    if (joints.Length == 0) continue;
                    var step = ImperialSpineStep(outward);
                    int wallDepth = outward == Direction.Down ? 2 : 1;
                    for (int distance = 5; distance <= length - 5; distance++)
                    {
                        var joint = start + along * distance - step * (ImperialSpineHalfWidth + wallDepth);
                        var opening = IsSpineVertical(outward) ? joint + new CellPosition(0, -1) : joint;
                        if (occupied.Contains(joint) || occupied.Contains(opening) ||
                            plan.Doors.Any(door => SpineManhattan(door.Position, opening) < 5))
                            continue;
                        foreach (var local in joints)
                        {
                            var origin = joint - local;
                            var far = origin + new CellPosition(candidate.Header.MapGrid.MaxWidth - 1, candidate.Header.MapGrid.MaxHeight - 1);
                            if (!SpineRoomFits(plan, [], candidate.Header, origin, width, height) ||
                                occupied.Any(cell => InsideSpineRoom(cell, origin, far)) ||
                                !SpineExtentFits(plan, [origin, far, opening], width, height))
                                continue;

                            var room = new ImperialSpineRoom(new Room { header = candidate.Header }, ExordiumDungeonGenerator.NoneGroupId);
                            var placed = AddSpineRoom(plan, room, origin);
                            var door = new ImperialSpineDoor(placed, joint, outward, spine, end: false);
                            plan.Doors.Add(door);
                            occupied.UnionWith(door.Opening);
                            addedFloor = candidate.Floor;
                            return true;
                        }
                    }
                }
                addedFloor = 0;
                return false;
            }
        }
    }
}

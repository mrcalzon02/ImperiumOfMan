using System;
using System.Collections.Generic;
using System.Linq;
using MGSC;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private const int RequiredSpineSideDoors = 5;
        private const int RequiredElbowLegCells = 15;

        private static bool AuditImperialSpinePlan(ImperialSpinePlan plan, int width, int height, out string failure)
        {
            failure = null;
            var closets = plan.Rooms.Where(placement => IsImperialSupplyClosetPreset(placement.Room.header.binaryId)).ToArray();
            if (closets.Length != ImperialSupplyClosetCount ||
                closets.Select(placement => placement.Room.header.binaryId).Distinct().Count() != ImperialSupplyClosetCount)
                return FailSpineAudit("expected twelve distinct supply closets", out failure);
            foreach (var closet in closets)
            {
                var doors = plan.Doors.Where(door => ReferenceEquals(door.Room, closet.Room)).ToArray();
                if (closet.Group != ImperialSupplyClosetGroupId || doors.Length != 1 || doors[0].End)
                    return FailSpineAudit("a supply closet needs exactly one side door", out failure);
            }
            var reserved = new Dictionary<CellPosition, MapCell>();
            var roomFloors = new Dictionary<CellPosition, CommonRoom>();
            foreach (var placement in plan.Rooms)
            {
                var room = placement.Room;
                var preset = room.header.MapGrid;
                if (room.RightUpperCorner.X - room.LeftLowerCorner.X + 1 != preset.MaxWidth ||
                    room.RightUpperCorner.Y - room.LeftLowerCorner.Y + 1 != preset.MaxHeight ||
                    !SpineCellWithinBounds(room.LeftLowerCorner, width, height) ||
                    !SpineCellWithinBounds(room.RightUpperCorner, width, height))
                    return FailSpineAudit("room does not preserve its installed preset dimensions or map bounds", out failure);

                for (int y = 0; y < preset.MaxHeight; y++)
                for (int x = 0; x < preset.MaxWidth; x++)
                {
                    var tile = preset.GetCell(x, y);
                    if (tile.Type == MapCellType.None) continue;
                    var pos = room.LeftLowerCorner + new CellPosition(x, y);
                    if (plan.CorridorCells.Contains(pos))
                        return FailSpineAudit($"room overlaps the clear spine at {pos}", out failure);
                    if (reserved.TryGetValue(pos, out var neighbor) && !(neighbor.IsWallSurfaceType && tile.IsWallSurfaceType))
                        return FailSpineAudit($"two room footprints overlap at {pos}", out failure);
                    reserved[pos] = tile;
                    if (IsSpineRoomFloor(tile)) roomFloors.Add(pos, room);
                }
            }

            if (plan.Spines.Length != 2 || plan.Elbows.Count == 0 || plan.CorridorCells.Count == 0 ||
                (plan.Spines[0].Start.Y == plan.Spines[0].End.Y) == (plan.Spines[1].Start.Y == plan.Spines[1].End.Y) ||
                plan.CorridorCells.Any(pos => !SpineCellWithinBounds(pos, width, height)))
                return FailSpineAudit("missing perpendicular straight corridors, an L corridor, or in-bounds corridor floor", out failure);

            var doorCells = new HashSet<CellPosition>();
            var attachedRooms = new HashSet<CommonRoom>();
            for (int index = 0; index < plan.Spines.Length; index++)
            {
                var spine = plan.Spines[index];
                bool horizontal = spine.Start.Y == spine.End.Y;
                if (spine.Start.Equals(spine.End) || (!horizontal && spine.Start.X != spine.End.X))
                    return FailSpineAudit("spine centerline must be a straight, nonzero run", out failure);

                int low = Math.Min(SpineAlong(spine.Start, horizontal), SpineAlong(spine.End, horizontal));
                int high = Math.Max(SpineAlong(spine.Start, horizontal), SpineAlong(spine.End, horizontal));
                int center = horizontal ? spine.Start.Y : spine.Start.X;
                for (int along = low; along <= high; along++)
                for (int side = -1; side <= 1; side++)
                {
                    var pos = horizontal ? new CellPosition(along, center + side) : new CellPosition(center + side, along);
                    if (!plan.CorridorCells.Contains(pos))
                        return FailSpineAudit($"spine {index} narrows below three floor cells at {pos}", out failure);
                }

                var sideRooms = new HashSet<CommonRoom>();
                bool startDoor = false, endDoor = false;
                foreach (var door in plan.Doors.Where(door => door.Spine == index))
                {
                    if (!AuditSpineDoor(door, plan, roomFloors, doorCells, out var approach))
                        return FailSpineAudit($"invalid native room joint, opening, or interior path at {door.Joint}", out failure);
                    attachedRooms.Add(door.Room);
                    var vector = CellPosition.DirectionToVec2Int(door.Outward);
                    bool axial = horizontal ? vector.x != 0 : vector.y != 0;
                    int along = SpineAlong(approach, horizontal);
                    int lateral = horizontal ? approach.Y : approach.X;
                    int facing = horizontal ? vector.x : vector.y;
                    if (door.End)
                    {
                        if (!axial || Math.Abs(lateral - center) > 1)
                            return FailSpineAudit($"end door is not facing down spine {index} at {door.Joint}", out failure);
                        if (along == low && facing > 0 && !startDoor) startDoor = true;
                        else if (along == high && facing < 0 && !endDoor) endDoor = true;
                        else return FailSpineAudit($"duplicate or misplaced end door at {door.Joint}", out failure);
                    }
                    else
                    {
                        int sideFacing = horizontal ? vector.y : vector.x;
                        if (axial || along < low || along > high || lateral != center - sideFacing ||
                            !sideRooms.Add(door.Room))
                            return FailSpineAudit($"side door does not open from a distinct room into spine {index} at {door.Joint}", out failure);
                    }
                }
                if (!startDoor || !endDoor || sideRooms.Count < RequiredSpineSideDoors)
                    return FailSpineAudit($"spine {index} needs a native door at both ends and five separate room doors on its sides", out failure);
            }

            int elbowSideDoors = 0;
            for (int index = 0; index < plan.Elbows.Count; index++)
            {
                var elbow = plan.Elbows[index];
                if (!AuditElbowFloor(elbow, plan.CorridorCells))
                    return FailSpineAudit($"L corridor {index} needs two perpendicular, unbroken 15-cell legs with a two-cell-wide turn", out failure);

                bool startDoor = false, endDoor = false;
                var endRooms = new HashSet<CommonRoom>();
                var sideRooms = new HashSet<CommonRoom>();
                foreach (var door in plan.Doors.Where(door => door.Spine == plan.Spines.Length + index))
                {
                    if (!AuditSpineDoor(door, plan, roomFloors, doorCells, out var approach))
                        return FailSpineAudit($"invalid native L corridor doorway at {door.Joint}", out failure);

                    attachedRooms.Add(door.Room);
                    var facing = CellPosition.DirectionToVec2Int(door.Outward);
                    var step = new CellPosition(facing.x, facing.y);
                    if (!door.End)
                    {
                        if (!sideRooms.Add(door.Room) || !plan.CorridorCells.Contains(approach + step) ||
                            !(IsElbowSideApproach(elbow.Start, elbow.Bend, approach, step) ||
                              IsElbowSideApproach(elbow.Bend, elbow.End, approach, step)))
                            return FailSpineAudit($"L corridor {index} side door does not open into the middle of a leg at {door.Joint}", out failure);
                        continue;
                    }

                    if (!endRooms.Add(door.Room))
                        return FailSpineAudit($"L corridor {index} has both end doors in the same room", out failure);
                    if (approach.Equals(elbow.Start) && step.Equals(SpineStepToward(elbow.Start, elbow.Bend)) && !startDoor)
                        startDoor = true;
                    else if (approach.Equals(elbow.End) && step.Equals(SpineStepToward(elbow.End, elbow.Bend)) && !endDoor)
                        endDoor = true;
                    else return FailSpineAudit($"duplicate or misplaced L corridor end door at {door.Joint}", out failure);
                }
                if (!startDoor || !endDoor)
                    return FailSpineAudit($"L corridor {index} needs a native room door at each end", out failure);
                if (sideRooms.Overlaps(endRooms))
                    return FailSpineAudit($"L corridor {index} has a side doorway into an end room", out failure);
                elbowSideDoors += sideRooms.Count;
            }

            if (elbowSideDoors == 0)
                return FailSpineAudit("the L corridors need a native room branching from a leg", out failure);

            if (plan.Doors.Any(door => door.Spine < 0 || door.Spine >= plan.Spines.Length + plan.Elbows.Count) ||
                plan.Rooms.Any(placement => !attachedRooms.Contains(placement.Room)))
                return FailSpineAudit("a placed room is missing a spine doorway", out failure);

            var walkable = new HashSet<CellPosition>(plan.CorridorCells);
            walkable.UnionWith(doorCells);
            walkable.UnionWith(roomFloors.Keys);
            var visited = WalkSpineCells(plan.CorridorCells.First(), walkable.Contains, RoomFence);
            if (plan.CorridorCells.Any(pos => !visited.Contains(pos)) || doorCells.Any(pos => !visited.Contains(pos)) ||
                plan.Rooms.Any(placement => !roomFloors.Any(entry =>
                    ReferenceEquals(entry.Value, placement.Room) && visited.Contains(entry.Key))))
                return FailSpineAudit("a spine, doorway, or room cannot be reached from the shared hallway", out failure);
            return true;

            bool RoomFence(CellPosition from, CellPosition to)
            {
                if (!roomFloors.TryGetValue(from, out var room) && !roomFloors.TryGetValue(to, out room)) return false;
                return room.header.MapGrid.GetFence(from - room.LeftLowerCorner, to - room.LeftLowerCorner);
            }
        }

        private static bool IsElbowSideApproach(CellPosition start, CellPosition end, CellPosition approach, CellPosition step)
        {
            bool horizontal = start.Y == end.Y;
            int facing = horizontal ? step.Y : step.X;
            int low = Math.Min(SpineAlong(start, horizontal), SpineAlong(end, horizontal));
            int high = Math.Max(SpineAlong(start, horizontal), SpineAlong(end, horizontal));
            int along = SpineAlong(approach, horizontal);
            int lateral = horizontal ? approach.Y - start.Y : approach.X - start.X;
            return facing != 0 && along > low + 1 && along < high - 1 && (lateral == 0 || lateral == -facing);
        }

        private static bool AuditElbowFloor((CellPosition Start, CellPosition Bend, CellPosition End) elbow,
            ISet<CellPosition> corridorFloor)
        {
            bool firstHorizontal = elbow.Start.Y == elbow.Bend.Y;
            bool secondHorizontal = elbow.End.Y == elbow.Bend.Y;
            if (firstHorizontal == secondHorizontal ||
                (!firstHorizontal && elbow.Start.X != elbow.Bend.X) ||
                (!secondHorizontal && elbow.End.X != elbow.Bend.X) ||
                SpineManhattan(elbow.Start, elbow.Bend) + 1 < RequiredElbowLegCells ||
                SpineManhattan(elbow.End, elbow.Bend) + 1 < RequiredElbowLegCells)
                return false;

            // Accept either side of each centerline, provided the same two cells continue
            // down each leg and the selected strips share a full square at their turn.
            var first = SpineStepToward(elbow.Bend, elbow.Start);
            var second = SpineStepToward(elbow.Bend, elbow.End);
            foreach (int firstSide in new[] { -1, 1 })
            foreach (int secondSide in new[] { -1, 1 })
            {
                var firstOffset = second * firstSide;
                var secondOffset = first * secondSide;
                if (corridorFloor.Contains(elbow.Bend + firstOffset + secondOffset) &&
                    HasWideElbowLeg(elbow.Bend, elbow.Start, first, firstOffset, corridorFloor) &&
                    HasWideElbowLeg(elbow.Bend, elbow.End, second, secondOffset, corridorFloor))
                    return true;
            }
            return false;
        }

        private static bool HasWideElbowLeg(CellPosition start, CellPosition end, CellPosition step,
            CellPosition offset, ISet<CellPosition> corridorFloor)
        {
            int length = SpineManhattan(start, end);
            for (int index = 0; index <= length; index++)
            {
                var pos = start + step * index;
                if (!corridorFloor.Contains(pos) || !corridorFloor.Contains(pos + offset)) return false;
            }
            return true;
        }

        private static bool AuditSpineDoor(ImperialSpineDoor door, ImperialSpinePlan plan,
            Dictionary<CellPosition, CommonRoom> roomFloors, HashSet<CellPosition> usedOpenings, out CellPosition approach)
        {
            approach = default;
            var room = door.Room;
            var joint = door.Joint - room.LeftLowerCorner;
            var joints = room.header.genMetadata;
            var validJoints = door.Outward switch
            {
                Direction.Up => joints.UpperJoints,
                Direction.Down => joints.BottomJoints,
                Direction.Left => joints.LeftJoints,
                Direction.Right => joints.RightJoints,
                _ => null,
            };
            if (validJoints == null || !validJoints.Contains(joint) ||
                joints.IgnoreConnectionJoints?.Contains(joint) == true ||
                room.header.MapGrid.GetCell(joint)?.IsWallSurfaceType != true)
                return false;

            var outward = CellPosition.DirectionToVec2Int(door.Outward);
            var step = new CellPosition(outward.x, outward.y);
            bool northSouth = outward.y != 0;
            var bottom = northSouth ? door.Joint + new CellPosition(0, -1) : door.Joint;
            if (door.Opening.Length != (northSouth ? 2 : 1) || !door.Position.Equals(bottom) ||
                !door.Opening.Contains(bottom) || !door.Opening.Contains(door.Joint))
                return false;
            foreach (var pos in door.Opening)
                if (plan.CorridorCells.Contains(pos) || roomFloors.ContainsKey(pos) || !usedOpenings.Add(pos))
                    return false;

            var interior = SpineDoorInterior(door);
            approach = (outward.y < 0 ? bottom : door.Joint) + step;
            if (!roomFloors.TryGetValue(interior, out var owner) || !ReferenceEquals(owner, room) ||
                !plan.CorridorCells.Contains(approach))
                return false;

            var local = interior - room.LeftLowerCorner;
            var preset = room.header.MapGrid;
            var connected = WalkSpineCells(local, pos => IsSpineRoomFloor(preset.GetCell(pos)), preset.GetFence);
            return connected.Any(pos => SpineManhattan(pos, local) >= 2);
        }

        private static bool AuditStampedImperialSpines(ImperialSpinePlan plan, MapGrid grid, MapMetadata metadata,
            string horizontalDoorId, string verticalDoorId, out string failure)
        {
            failure = null;
            var reachable = WalkSpineCells(plan.CorridorCells.First(),
                pos => IsSpineRoomFloor(grid.GetCell(pos)), grid.GetFence);
            foreach (var pos in plan.CorridorCells)
            {
                var cell = grid.GetCell(pos);
                if (cell?.Type != MapCellType.Floor || cell.LiquidLevel != 0 || cell.IsObjBlockPass ||
                    !reachable.Contains(pos))
                    return FailSpineAudit($"stamped spine cell is blocked or disconnected at {pos}", out failure);
                foreach (var (dx, dy) in FloorNeighbors)
                {
                    var next = pos + new CellPosition(dx, dy);
                    if (plan.CorridorCells.Contains(next) && grid.GetFence(pos, next))
                        return FailSpineAudit($"a fence divides the clear spine at {pos}", out failure);
                }
            }
            foreach (var door in plan.Doors)
            {
                var doorId = door.Outward is Direction.Up or Direction.Down ? horizontalDoorId : verticalDoorId;
                if (string.IsNullOrEmpty(doorId) || metadata.MapObstacles.Count(obj => obj.ObstacleName == doorId &&
                    obj.Pos.Equals(door.Position)) != 1 || door.Opening.Any(pos => !reachable.Contains(pos)) ||
                    !reachable.Contains(SpineDoorInterior(door)))
                    return FailSpineAudit($"native door was not stamped exactly once on the reachable opening at {door.Position}", out failure);
            }
            return true;
        }

        private static HashSet<CellPosition> WalkSpineCells(CellPosition start, Func<CellPosition, bool> walkable,
            Func<CellPosition, CellPosition, bool> hasFence)
        {
            var visited = new HashSet<CellPosition>();
            if (!walkable(start)) return visited;
            var pending = new Queue<CellPosition>();
            pending.Enqueue(start);
            visited.Add(start);
            while (pending.Count > 0)
            {
                var current = pending.Dequeue();
                foreach (var (dx, dy) in FloorNeighbors)
                {
                    var next = current + new CellPosition(dx, dy);
                    if (!walkable(next) || hasFence(current, next) || !visited.Add(next)) continue;
                    pending.Enqueue(next);
                }
            }
            return visited;
        }

        private static bool IsSpineRoomFloor(MapCell tile) => tile?.Type is MapCellType.Floor or
            MapCellType.ClosedDoor or MapCellType.OpenedDoor;

        private static bool SpineCellWithinBounds(CellPosition position, int width, int height) =>
            position.X >= 0 && position.X < width && position.Y >= 0 && position.Y < height;

        private static int SpineAlong(CellPosition position, bool horizontal) => horizontal ? position.X : position.Y;

        private static int SpineManhattan(CellPosition a, CellPosition b) => Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);

        private static CellPosition SpineDoorInterior(ImperialSpineDoor door)
        {
            var outward = CellPosition.DirectionToVec2Int(door.Outward);
            return (door.Outward == Direction.Up ? door.Position : door.Joint) - new CellPosition(outward.x, outward.y);
        }

        private static CellPosition SpineStepToward(CellPosition from, CellPosition to) =>
            new(Math.Sign(to.X - from.X), Math.Sign(to.Y - from.Y));

        private static bool FailSpineAudit(string reason, out string failure)
        {
            failure = reason;
            return false;
        }
    }
}

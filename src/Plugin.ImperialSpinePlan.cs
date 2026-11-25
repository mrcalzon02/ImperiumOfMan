using System;
using System.Collections.Generic;
using System.Linq;
using MGSC;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private const int ImperialSpineSideDoors = 5;
        private const int ImperialSpineHalfWidth = 1;
        private const int ImperialSpineMinimumRun = 60;
        private const int ImperialSpineDoorPitch = 7;
        private const int ImperialSpineMargin = 3;
        private const int ImperialElbowLeg = 16;

        private sealed class ImperialSpinePlan
        {
            internal readonly List<ImperialSpineRoom> Rooms = new();
            internal readonly List<ImperialSpineDoor> Doors = new();
            internal readonly HashSet<CellPosition> CorridorCells = new();
            internal readonly (CellPosition Start, CellPosition End)[] Spines = new (CellPosition, CellPosition)[2];
            internal readonly List<(CellPosition Start, CellPosition Bend, CellPosition End)> Elbows = new();
        }

        private readonly struct ImperialSpineRoom(CommonRoom room, short group)
        {
            internal readonly CommonRoom Room = room;
            internal readonly short Group = group;
        }

        private sealed class ImperialSpineDoor(CommonRoom room, CellPosition joint, Direction outward, int spine, bool end)
        {
            internal readonly CommonRoom Room = room;
            internal CellPosition Joint = joint;
            internal readonly Direction Outward = outward;
            internal readonly int Spine = spine;
            internal readonly bool End = end;
            internal CellPosition Position => IsSpineVertical(Outward) ? Joint + new CellPosition(0, -1) : Joint;
            internal CellPosition[] Opening => IsSpineVertical(Outward) ? [Position, Joint] : [Joint];
        }

        private sealed class ImperialSpineRun(Direction direction, CellPosition start, int minimumLength, int doorPitch)
        {
            internal readonly Direction Direction = direction;
            internal readonly CellPosition Start = start;
            internal readonly CellPosition Step = ImperialSpineStep(direction);
            internal readonly int MinimumLength = minimumLength;
            internal readonly int DoorPitch = doorPitch;
            internal int NextDoor = doorPitch;
            internal int SideDoors;
        }

        private static bool TryPlanImperialSpines(IReadOnlyList<(CommonRoom Room, short Group)> required,
            IReadOnlyList<PresetHeader> commonDonors, int mapWidth, int mapHeight, int seed, out ImperialSpinePlan plan,
            CampaignRouteLayout route = null, int floor = 1, int endgameTier = 0, int extraHorizontal = 0, int extraVertical = 0)
        {
            plan = null;
            if (mapWidth > 250 || mapHeight > 250 || mapWidth < ImperialSpineMinimumRun + 12 ||
                mapHeight < ImperialSpineMinimumRun + 12 || required.Any(item => !CanBuildSpineRoom(item.Room?.header)))
            {
                return false;
            }

            var mandatory = required.Select(item => new ImperialSpineRoom(item.Room, item.Group)).ToArray();
            var donors = commonDonors.Where(header => CanBuildSpineRoom(header) && !header.genMetadata.SpawnUnique)
                .Distinct().Select(header => new ImperialSpineRoom(new Room { header = header }, ExordiumDungeonGenerator.NoneGroupId)).ToArray();
            var closetTemplates = CreateImperialSupplyClosetTemplates(commonDonors);
            var random = new Random(seed);
            var directions = new[] { (Direction.Left, Direction.Up), (Direction.Left, Direction.Down),
                                     (Direction.Right, Direction.Up), (Direction.Right, Direction.Down) };
            var preferred = RouteSpineDirections(route, floor);
            int[,] patterns = { { 1, -1, 1, -1, 1 }, { -1, 1, 1, -1, 1 }, { 1, 1, -1, -1, 1 } };
            foreach (var (horizontal, vertical) in directions.OrderByDescending(direction => direction == preferred)
                .ThenBy(_ => random.Next()))
            {
                var junctions = mandatory.Where(item => item.Group < 100).Concat(donors)
                    .Where(item => SpineJoints(item.Room.header, horizontal).Any() && SpineJoints(item.Room.header, vertical).Any())
                    .OrderByDescending(item => route?.RoomPreference(item.Room.header, CampaignRouteRoomRole.Junction) ?? 0)
                    .ThenBy(_ => random.Next()).Take(8);
                foreach (var junction in junctions)
                for (int variant = 0; variant < patterns.GetLength(0); variant++)
                {
                    int row = (variant + seed % patterns.GetLength(0) + patterns.GetLength(0)) % patterns.GetLength(0);
                    int[] pattern = Enumerable.Range(0, patterns.GetLength(1)).Select(column => patterns[row, column]).ToArray();
                    int[] horizontalPattern = variant == 0 && route != null ? route.HorizontalSidePattern : pattern;
                    int[] verticalPattern = variant == 0 && route != null ? route.VerticalSidePattern : pattern.Select(side => -side).ToArray();
                    if (TryBuildImperialSpines(mandatory, donors, closetTemplates, junction, horizontal, vertical,
                        horizontalPattern, verticalPattern, mapWidth, mapHeight, seed, random, route, endgameTier,
                        extraHorizontal, extraVertical, out plan))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static (Direction Horizontal, Direction Vertical) RouteSpineDirections(CampaignRouteLayout route, int floor)
        {
            if (route == null) return (Direction.Center, Direction.Center);
            return (floor == 0 ? CellPosition.InvertDirection(route.HorizontalDirection) : route.HorizontalDirection,
                floor > 1 && floor % 2 == 0 ? CellPosition.InvertDirection(route.VerticalDirection) : route.VerticalDirection);
        }

        private static bool TryBuildImperialSpines(ImperialSpineRoom[] mandatory, ImperialSpineRoom[] donors,
            IReadOnlyDictionary<Direction, PresetHeader[]> supplyClosets,
            ImperialSpineRoom junction, Direction horizontal, Direction vertical, int[] horizontalPattern, int[] verticalPattern,
            int width, int height, int seed, Random random, CampaignRouteLayout route, int endgameTier,
            int extraHorizontal, int extraVertical, out ImperialSpinePlan plan)
        {
            var trial = new ImperialSpinePlan();
            plan = null;
            var remaining = mandatory.Where(item => item.Room != junction.Room).OrderBy(_ => random.Next()).ToList();
            var junctions = (from first in SpineJoints(junction.Room.header, horizontal)
                             from second in SpineJoints(junction.Room.header, vertical)
                             where NativeSpineRoomConnects(junction.Room.header, first, horizontal, second, vertical)
                             select (First: first, Second: second)).Take(1).ToArray();
            if (junctions.Length == 0) return false;
            var placedJunction = AddSpineRoom(trial, junction, CellPosition.Zero);
            var (horizontalJoint, verticalJoint) = junctions[0];
            trial.Doors.Add(new ImperialSpineDoor(placedJunction, horizontalJoint, horizontal, 0, end: true));
            trial.Doors.Add(new ImperialSpineDoor(placedJunction, verticalJoint, vertical, 1, end: true));
            int pitch = route?.DoorPitch ?? ImperialSpineDoorPitch;
            var wings = new[]
            {
                new ImperialSpineRun(horizontal, FirstSpineFloor(horizontalJoint, horizontal),
                    ExpandedCampaignRunLength(route?.HorizontalMinimum ?? ImperialSpineMinimumRun, endgameTier) + extraHorizontal, pitch),
                new ImperialSpineRun(vertical, FirstSpineFloor(verticalJoint, vertical),
                    ExpandedCampaignRunLength(route?.VerticalMinimum ?? ImperialSpineMinimumRun, endgameTier) + extraVertical, pitch),
            };
            var sidePatterns = new[] { horizontalPattern, verticalPattern };
            int arrivalWing = route != null && route.HorizontalMinimum > route.VerticalMinimum ? 1 : 0;

            var terminals = new ImperialSpineRoom[2];
            for (int wing = 0; wing < wings.Length; wing++)
            {
                int index = wing;
                var face = CellPosition.InvertDirection(wings[wing].Direction);
                var role = wing == 0 ? CampaignRouteRoomRole.HorizontalTerminal : CampaignRouteRoomRole.VerticalTerminal;
                var matches = remaining.Where(room => SpineJoints(room.Room.header, face).Any())
                    .OrderByDescending(room => TerminalPriority(room.Group, index == arrivalWing ? 0 : 1))
                    .ThenByDescending(room => route?.RoomPreference(room.Room.header, role) ?? 0)
                    .Concat(donors.Where(room => SpineJoints(room.Room.header, face).Any())
                        .OrderByDescending(room => route?.RoomPreference(room.Room.header, role) ?? 0)
                        .ThenBy(room => SpineRoomArea(room.Room.header)));
                terminals[wing] = matches.FirstOrDefault();
                if (terminals[wing].Room == null) return false;
                remaining.Remove(terminals[wing]);
            }

            for (int index = 0; index < ImperialSpineSideDoors; index++)
            for (int wing = 0; wing < wings.Length; wing++)
            {
                int side = SideForSpine(index, sidePatterns[wing]);
                if (!TryAddSpineSideRoom(trial, wings, wing, side, remaining, donors, width, height, route))
                    return false;
            }

            while (remaining.Count != 0)
            {
                int wing = wings[0].SideDoors <= wings[1].SideDoors ? 0 : 1;
                int side = SideForSpine(wings[wing].SideDoors, sidePatterns[wing]);
                int alternate = 1 - wing;
                if (!TryAddSpineSideRoom(trial, wings, wing, side, remaining, [], width, height, route) &&
                    !TryAddSpineSideRoom(trial, wings, alternate, SideForSpine(wings[alternate].SideDoors, sidePatterns[alternate]),
                        remaining, [], width, height, route))
                {
                    return false;
                }
            }

            for (int wing = 0; wing < wings.Length; wing++)
            {
                if (!TryCloseSpineRun(trial, wings, wing, terminals[wing], width, height))
                    return false;
            }

            AddImperialElbows(trial, donors, width, height, seed, route, endgameTier);
            if (!trial.Doors.Any(door => door.Spine >= trial.Spines.Length && !door.End) ||
                !AddImperialSupplyClosets(trial, supplyClosets, width, height, seed) ||
                !CenterImperialSpines(trial, width, height)) return false;
            plan = trial;
            return true;
        }

        private static int SideForSpine(int slot, int[] pattern) => pattern[slot % pattern.Length];

        private static int ExpandedCampaignRunLength(int minimum, int endgameTier) => minimum + minimum * endgameTier / 6;

        private static bool TryAddSpineSideRoom(ImperialSpinePlan plan, ImperialSpineRun[] wings, int wing,
            int preferredSide, List<ImperialSpineRoom> remaining, ImperialSpineRoom[] donors, int width, int height,
            CampaignRouteLayout route)
        {
            var run = wings[wing];
            bool vertical = IsSpineVertical(run.Direction);
            int maxRun = vertical ? height : width;
            var candidates = remaining.OrderByDescending(Preference).Concat(donors.OrderBy(item =>
                plan.Rooms.Count(placed => placed.Room.header.binaryId == item.Room.header.binaryId))
                .ThenByDescending(Preference)).ToArray();
            foreach (int side in new[] { preferredSide, -preferredSide })
            {
                var outward = vertical
                    ? side > 0 ? Direction.Left : Direction.Right
                    : side > 0 ? Direction.Down : Direction.Up;
                var towardHall = ImperialSpineStep(outward);
                int wallDepth = outward == Direction.Down ? 2 : 1;
                foreach (var source in candidates)
                {
                    var joints = SpineJoints(source.Room.header, outward).ToArray();
                    if (joints.Length == 0) continue;

                    for (int distance = run.NextDoor; distance < maxRun - run.DoorPitch; distance++)
                    {
                        var jointTarget = run.Start + run.Step * distance - towardHall * (ImperialSpineHalfWidth + wallDepth);
                        foreach (var joint in joints)
                        {
                            var origin = jointTarget - joint;
                            if (!SpineRoomFits(plan, wings, source.Room.header, origin, width, height)) continue;

                            var placed = AddSpineRoom(plan, source, origin);
                            plan.Doors.Add(new ImperialSpineDoor(placed, jointTarget, outward, wing, end: false));
                            remaining.Remove(source);
                            run.NextDoor = distance + run.DoorPitch;
                            run.SideDoors++;
                            return true;
                        }
                    }
                }
            }
            return false;

            int Preference(ImperialSpineRoom item) => route?.RoomPreference(item.Room.header, CampaignRouteRoomRole.SideRoom) ?? 0;
        }

        private static bool TryCloseSpineRun(ImperialSpinePlan plan, ImperialSpineRun[] wings, int wing,
            ImperialSpineRoom terminal, int width, int height)
        {
            var run = wings[wing];
            int maxRun = IsSpineVertical(run.Direction) ? height : width;
            var outward = CellPosition.InvertDirection(run.Direction);
            var joints = SpineJoints(terminal.Room.header, outward).ToArray();
            int wallDepth = outward == Direction.Down ? 2 : 1;
            for (int distance = Math.Max(run.MinimumLength - 1, run.NextDoor); distance < maxRun; distance++)
            {
                var end = run.Start + run.Step * distance;
                var jointTarget = end + run.Step * wallDepth;
                foreach (var joint in joints)
                {
                    var origin = jointTarget - joint;
                    if (!SpineRoomFits(plan, wings, terminal.Room.header, origin, width, height, wing)) continue;

                    var placed = AddSpineRoom(plan, terminal, origin);
                    plan.Doors.Add(new ImperialSpineDoor(placed, jointTarget, outward, wing, end: true));
                    plan.Spines[wing] = (run.Start, end);
                    var across = IsSpineVertical(run.Direction) ? new CellPosition(1, 0) : new CellPosition(0, 1);
                    for (int along = 0; along <= distance; along++)
                    for (int offset = -ImperialSpineHalfWidth; offset <= ImperialSpineHalfWidth; offset++)
                    {
                        plan.CorridorCells.Add(run.Start + run.Step * along + across * offset);
                    }
                    return true;
                }
            }
            return false;
        }

        private static bool SpineRoomFits(ImperialSpinePlan plan, ImperialSpineRun[] wings, PresetHeader header,
            CellPosition origin, int width, int height, int ownTerminal = -1)
        {
            var far = origin + new CellPosition(header.MapGrid.MaxWidth - 1, header.MapGrid.MaxHeight - 1);
            for (int wing = 0; wing < wings.Length; wing++)
            {
                if (wing != ownTerminal && RoomCutsSpineRay(origin, far, wings[wing])) return false;
            }
            int left = origin.X, right = far.X, bottom = origin.Y, top = far.Y;
            foreach (var entry in plan.Rooms)
            {
                var low = entry.Room.LeftLowerCorner;
                var high = entry.Room.RightUpperCorner;
                if (origin.X <= high.X + 1 && far.X >= low.X - 1 &&
                    origin.Y <= high.Y + 1 && far.Y >= low.Y - 1)
                {
                    return false;
                }
                left = Math.Min(left, low.X);
                right = Math.Max(right, high.X);
                bottom = Math.Min(bottom, low.Y);
                top = Math.Max(top, high.Y);
            }
            return right - left + 1 + ImperialSpineMargin * 2 <= width &&
                top - bottom + 1 + ImperialSpineMargin * 2 <= height;
        }

        private static bool RoomCutsSpineRay(CellPosition low, CellPosition high, ImperialSpineRun run) =>
            IsSpineVertical(run.Direction)
                ? low.X <= run.Start.X + ImperialSpineHalfWidth && high.X >= run.Start.X - ImperialSpineHalfWidth &&
                  (run.Step.Y > 0 ? high.Y >= run.Start.Y : low.Y <= run.Start.Y)
                : low.Y <= run.Start.Y + ImperialSpineHalfWidth && high.Y >= run.Start.Y - ImperialSpineHalfWidth &&
                  (run.Step.X > 0 ? high.X >= run.Start.X : low.X <= run.Start.X);

        private static CommonRoom AddSpineRoom(ImperialSpinePlan plan, ImperialSpineRoom source, CellPosition origin)
        {
            var room = source.Room.Clone();
            room.LeftLowerCorner = origin;
            room.RightUpperCorner = origin + new CellPosition(room.header.MapGrid.MaxWidth - 1, room.header.MapGrid.MaxHeight - 1);
            plan.Rooms.Add(new ImperialSpineRoom(room, source.Group));
            return room;
        }

        private static void AddImperialElbows(ImperialSpinePlan plan, ImperialSpineRoom[] donors, int width, int height, int seed,
            CampaignRouteLayout route, int endgameTier)
        {
            var orientations = new[] { (Direction.Left, Direction.Up), (Direction.Right, Direction.Up),
                                        (Direction.Right, Direction.Down), (Direction.Left, Direction.Down) };
            int firstOrientation = (int)((uint)seed % orientations.Length);
            for (int index = 0; index < orientations.Length; index++)
            {
                var (horizontal, vertical) = orientations[(firstOrientation + index) % orientations.Length];
                if (TryAddImperialElbow(plan, donors, CellPosition.InvertDirection(horizontal), vertical, width, height, route, endgameTier) ||
                    TryAddImperialElbow(plan, donors, CellPosition.InvertDirection(vertical), horizontal, width, height, route, endgameTier))
                {
                    AddImperialElbowSideRoom(plan, donors, plan.Elbows.Count - 1, width, height, route);
                }
            }
        }

        private static void AddImperialElbowSideRoom(ImperialSpinePlan plan, ImperialSpineRoom[] donors,
            int elbowIndex, int width, int height, CampaignRouteLayout route)
        {
            var (start, bend, end) = plan.Elbows[elbowIndex];
            var occupied = plan.CorridorCells.ToHashSet();
            foreach (var door in plan.Doors) occupied.UnionWith(door.Opening);
            var closets = OrderElbowDonors(plan, donors, preferredArea: 48, compactLimit: 80, route,
                CampaignRouteRoomRole.ElbowSideRoom).Take(12).ToArray();
            foreach (var closet in closets)
            foreach (var (from, to) in new[] { (start, bend), (bend, end) })
            {
                var step = new CellPosition(Math.Sign(to.X - from.X), Math.Sign(to.Y - from.Y));
                int length = Math.Abs(to.X - from.X) + Math.Abs(to.Y - from.Y);
                var sides = step.X == 0 ? new[] { Direction.Left, Direction.Right } : new[] { Direction.Down, Direction.Up };
                var header = closet.Room.header;
                foreach (var outward in sides)
                {
                    var towardHall = ImperialSpineStep(outward);
                    int wallDepth = outward == Direction.Down ? 2 : 1;
                    var joints = SpineJoints(header, outward).ToArray();
                    if (joints.Length == 0) continue;

                    for (int distance = 5; distance <= length - 5; distance++)
                    {
                        var jointTarget = from + step * distance - towardHall * (ImperialSpineHalfWidth + wallDepth);
                        if (occupied.Contains(jointTarget) ||
                            (IsSpineVertical(outward) && occupied.Contains(jointTarget + new CellPosition(0, -1)))) continue;

                        foreach (var joint in joints)
                        {
                            var origin = jointTarget - joint;
                            var far = origin + new CellPosition(header.MapGrid.MaxWidth - 1, header.MapGrid.MaxHeight - 1);
                            if (!SpineRoomFits(plan, [], header, origin, width, height) ||
                                occupied.Any(point => InsideSpineRoom(point, origin, far)) ||
                                !SpineExtentFits(plan, new[] { origin, far }, width, height)) continue;

                            var room = AddSpineRoom(plan, closet, origin);
                            plan.Doors.Add(new ImperialSpineDoor(room, jointTarget, outward,
                                elbowIndex + plan.Spines.Length, end: false));
                            return;
                        }
                    }
                }
            }
        }

        private static bool TryAddImperialElbow(ImperialSpinePlan plan, ImperialSpineRoom[] donors, Direction firstDirection,
            Direction secondDirection, int width, int height, CampaignRouteLayout route, int endgameTier)
        {
            var firstStep = ImperialSpineStep(firstDirection);
            var secondStep = ImperialSpineStep(secondDirection);
            var terminalSide = CellPosition.InvertDirection(secondDirection);
            int terminalArea = plan.Elbows.Count % 2 == 0 ? 96 : 64;
            var terminals = OrderElbowDonors(plan, donors, terminalArea, compactLimit: 120, route, CampaignRouteRoomRole.ElbowTerminal)
                .Select(item => (Room: item, Joints: SpineJoints(item.Room.header, terminalSide).ToArray()))
                .Where(item => item.Joints.Length > 0).Take(12).ToArray();
            if (terminals.Length == 0) return false;

            var sources = plan.Rooms.Where(item => item.Group < 100)
                .OrderByDescending(item => item.Room.LeftLowerCorner.X * firstStep.X + item.Room.LeftLowerCorner.Y * firstStep.Y).ToArray();
            var occupied = plan.CorridorCells.ToHashSet();
            foreach (var door in plan.Doors) occupied.UnionWith(door.Opening);
            int minimumLeg = (route?.ElbowLength ?? ImperialElbowLeg) + endgameTier * 3;
            var legs = new[] { minimumLeg, minimumLeg + 5, minimumLeg + 10, minimumLeg + 18 };
            foreach (var entry in sources)
            {
                var source = entry.Room;
                var sourceDoors = plan.Doors.Where(door => door.Room == source).ToArray();
                if (sourceDoors.Any(door => door.Outward == firstDirection)) continue;
                foreach (var localJoint in SpineJoints(source.header, firstDirection))
                {
                    if (!sourceDoors.Any(door =>
                        NativeSpineRoomConnects(source.header, door.Joint - source.LeftLowerCorner, door.Outward,
                            localJoint, firstDirection))) continue;

                    var joint = source.LeftLowerCorner + localJoint;
                    var start = FirstSpineFloor(joint, firstDirection);
                    foreach (int firstLeg in legs)
                    {
                        var bend = start + firstStep * firstLeg;
                        var firstCells = BuildElbowLeg(start, firstStep, firstLeg);
                        if (!ElbowSpaceIsClear(plan, source, occupied, firstCells)) break;
                        foreach (int secondLeg in legs)
                        {
                            var end = bend + secondStep * secondLeg;
                            var cells = firstCells.ToHashSet();
                            cells.UnionWith(BuildElbowLeg(bend, secondStep, secondLeg));
                            for (int x = -ImperialSpineHalfWidth; x <= ImperialSpineHalfWidth; x++)
                            for (int y = -ImperialSpineHalfWidth; y <= ImperialSpineHalfWidth; y++)
                                cells.Add(bend + new CellPosition(x, y));
                            if (!ElbowSpaceIsClear(plan, source, occupied, cells)) break;
                            if (!SpineExtentFits(plan, cells, width, height)) continue;

                            int wallDepth = terminalSide == Direction.Down ? 2 : 1;
                            var endJoint = end + secondStep * wallDepth;
                            foreach (var terminal in terminals)
                            foreach (var terminalJoint in terminal.Joints)
                            {
                                var origin = endJoint - terminalJoint;
                                var header = terminal.Room.Room.header;
                                var far = origin + new CellPosition(header.MapGrid.MaxWidth - 1, header.MapGrid.MaxHeight - 1);
                                if (!SpineRoomFits(plan, [], header, origin, width, height) ||
                                    cells.Any(point => InsideSpineRoom(point, origin, far)) ||
                                    occupied.Any(point => InsideSpineRoom(point, origin, far)) ||
                                    !SpineExtentFits(plan, cells.Concat(new[] { origin, far }), width, height)) continue;

                                int corridor = plan.Elbows.Count + plan.Spines.Length;
                                var target = AddSpineRoom(plan, terminal.Room, origin);
                                plan.Doors.Add(new ImperialSpineDoor(source, joint, firstDirection, corridor, end: true));
                                plan.Doors.Add(new ImperialSpineDoor(target, endJoint, terminalSide, corridor, end: true));
                                plan.Elbows.Add((start, bend, end));
                                plan.CorridorCells.UnionWith(cells);
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        private static IOrderedEnumerable<ImperialSpineRoom> OrderElbowDonors(ImperialSpinePlan plan,
            ImperialSpineRoom[] donors, int preferredArea, int compactLimit, CampaignRouteLayout route, CampaignRouteRoomRole role)
        {
            var elbowUsage = plan.Doors.Where(door => door.Spine >= plan.Spines.Length)
                .Select(door => door.Room).Distinct().GroupBy(room => room.header.binaryId)
                .ToDictionary(group => group.Key, group => group.Count());
            var totalUsage = plan.Rooms.GroupBy(item => item.Room.header.binaryId)
                .ToDictionary(group => group.Key, group => group.Count());

            // Give unused native templates room to vary in size without using a large hall as a closet.
            return donors.OrderBy(item =>
                (elbowUsage.TryGetValue(item.Room.header.binaryId, out int copies) ? copies * 40 : 0)
                + Math.Max(0, SpineRoomArea(item.Room.header) - compactLimit) / 2)
                .ThenByDescending(item => route?.RoomPreference(item.Room.header, role) ?? 0)
                .ThenBy(item => Math.Abs(SpineRoomArea(item.Room.header) - preferredArea))
                .ThenBy(item => totalUsage.TryGetValue(item.Room.header.binaryId, out int copies) ? copies : 0)
                .ThenBy(item => item.Room.header.binaryId);
        }

        private static IEnumerable<CellPosition> BuildElbowLeg(CellPosition start, CellPosition step, int length)
        {
            var across = new CellPosition(Math.Abs(step.Y), Math.Abs(step.X));
            for (int along = 0; along <= length; along++)
            for (int offset = -ImperialSpineHalfWidth; offset <= ImperialSpineHalfWidth; offset++)
                yield return start + step * along + across * offset;
        }

        private static bool ElbowSpaceIsClear(ImperialSpinePlan plan, CommonRoom source, HashSet<CellPosition> occupied,
            IEnumerable<CellPosition> cells)
        {
            foreach (var cell in cells)
            {
                if (occupied.Contains(cell) || plan.Rooms.Any(item => item.Room.ContainsPos(cell))) return false;
                foreach (var step in ImperialSpineCardinals)
                {
                    var neighbor = cell + step;
                    if (occupied.Contains(neighbor) || plan.Rooms.Any(item => item.Room != source && item.Room.ContainsPos(neighbor)))
                        return false;
                }
            }
            return true;
        }

        private static bool SpineExtentFits(ImperialSpinePlan plan, IEnumerable<CellPosition> extra, int width, int height)
        {
            var extents = GetSpineExtents(plan).Concat(extra).ToArray();
            return extents.Max(point => point.X) - extents.Min(point => point.X) + 1 + ImperialSpineMargin * 2 <= width &&
                extents.Max(point => point.Y) - extents.Min(point => point.Y) + 1 + ImperialSpineMargin * 2 <= height;
        }

        private static bool InsideSpineRoom(CellPosition point, CellPosition low, CellPosition high) =>
            point.X >= low.X && point.X <= high.X && point.Y >= low.Y && point.Y <= high.Y;

        private static bool NativeSpineRoomConnects(PresetHeader header, CellPosition firstJoint, Direction firstDirection,
            CellPosition secondJoint, Direction secondDirection)
        {
            var from = FirstSpineInterior(firstJoint, firstDirection);
            var to = FirstSpineInterior(secondJoint, secondDirection);
            var grid = header.MapGrid;
            var seen = new HashSet<CellPosition> { from };
            var pending = new Queue<CellPosition>();
            pending.Enqueue(from);
            while (pending.Count > 0)
            {
                var cell = pending.Dequeue();
                if (cell.Equals(to)) return true;
                foreach (var step in ImperialSpineCardinals)
                {
                    var next = cell + step;
                    if (grid.GetCell(next)?.Type is MapCellType.Floor or MapCellType.OpenedDoor or MapCellType.ClosedDoor
                        && !grid.GetFence(cell, next) && seen.Add(next))
                        pending.Enqueue(next);
                }
            }
            return false;
        }

        private static readonly CellPosition[] ImperialSpineCardinals =
            [new(1, 0), new(0, 1), new(-1, 0), new(0, -1)];

        private static IEnumerable<CellPosition> GetSpineExtents(ImperialSpinePlan plan) =>
            plan.Rooms.SelectMany(item => new[] { item.Room.LeftLowerCorner, item.Room.RightUpperCorner })
                .Concat(plan.CorridorCells).Concat(plan.Doors.SelectMany(door => door.Opening));

        private static bool CenterImperialSpines(ImperialSpinePlan plan, int width, int height)
        {
            var extents = GetSpineExtents(plan).ToArray();
            int left = extents.Min(point => point.X), right = extents.Max(point => point.X);
            int bottom = extents.Min(point => point.Y), top = extents.Max(point => point.Y);
            int spanX = right - left + 1, spanY = top - bottom + 1;
            if (spanX + ImperialSpineMargin * 2 > width || spanY + ImperialSpineMargin * 2 > height) return false;

            var move = new CellPosition((width - spanX) / 2 - left, (height - spanY) / 2 - bottom);
            foreach (var entry in plan.Rooms)
            {
                entry.Room.LeftLowerCorner += move;
                entry.Room.RightUpperCorner += move;
            }
            foreach (var door in plan.Doors) door.Joint += move;
            for (int index = 0; index < plan.Spines.Length; index++)
            {
                var (start, end) = plan.Spines[index];
                plan.Spines[index] = (start + move, end + move);
            }
            for (int index = 0; index < plan.Elbows.Count; index++)
            {
                var (start, bend, end) = plan.Elbows[index];
                plan.Elbows[index] = (start + move, bend + move, end + move);
            }
            var shifted = plan.CorridorCells.Select(cell => cell + move).ToArray();
            plan.CorridorCells.Clear();
            plan.CorridorCells.UnionWith(shifted);
            return true;
        }

        private static bool CanBuildSpineRoom(PresetHeader header) =>
            header?.presetType == PresetType.Room && header.MapGrid != null && header.genMetadata != null &&
            header.mapMetadata != null && header.MapGrid.MaxWidth >= 3 && header.MapGrid.MaxHeight >= 3;

        private static IEnumerable<CellPosition> SpineJoints(PresetHeader header, Direction outward)
        {
            var gen = header.genMetadata;
            var grid = header.MapGrid;
            var blocked = FixedCampaignFixtureCells(header);
            var options = outward switch
            {
                Direction.Up => gen.UpperJoints,
                Direction.Down => gen.BottomJoints,
                Direction.Left => gen.LeftJoints,
                Direction.Right => gen.RightJoints,
                _ => null,
            };
            var step = ImperialSpineStep(outward);
            var center = new CellPosition(grid.MaxWidth / 2, grid.MaxHeight / 2);
            return (options ?? []).Where(joint =>
            {
                bool straightEdge = outward switch
                {
                    Direction.Up => joint.Y == grid.MaxHeight - 1 && joint.X > 0 && joint.X + 1 < grid.MaxWidth,
                    Direction.Down => joint.Y == 0 && joint.X > 0 && joint.X + 1 < grid.MaxWidth,
                    Direction.Left => joint.X == 0 && joint.Y > 0 && joint.Y + 1 < grid.MaxHeight,
                    Direction.Right => joint.X == grid.MaxWidth - 1 && joint.Y > 0 && joint.Y + 1 < grid.MaxHeight,
                    _ => false,
                };
                if (!straightEdge || (gen.IgnoreConnectionJoints?.Contains(joint) ?? false)) return false;
                int depth = outward == Direction.Up ? 2 : 1;
                var interior = joint - step * depth;
                if (blocked != null && (blocked.Contains(interior) || blocked.Contains(FirstSpineFloor(joint, outward)) ||
                    blocked.Contains(joint) ||
                    IsSpineVertical(outward) && blocked.Contains(joint + new CellPosition(0, -1)))) return false;
                return grid.GetCell(interior)?.Type == MapCellType.Floor &&
                    grid.GetCell(joint)?.Type == MapCellType.Wall &&
                    (outward != Direction.Up || grid.GetCell(joint - step)?.IsWallSurfaceType == true);
            }).OrderBy(joint => Math.Abs(joint.X - center.X) + Math.Abs(joint.Y - center.Y));
        }

        private static int TerminalPriority(short group, int wing) => wing == 0
            ? group is >= 200 and < 300 ? 3 : group is >= 100 and < 200 ? 2 : group < 100 ? 1 : 0
            : group >= 300 ? 3 : group is >= 100 and < 200 ? 2 : group < 100 ? 1 : 0;

        private static int SpineRoomArea(PresetHeader header) => header.MapGrid.MaxWidth * header.MapGrid.MaxHeight;

        private static CellPosition FirstSpineFloor(CellPosition joint, Direction outward) =>
            joint + ImperialSpineStep(outward) * (outward == Direction.Down ? 2 : 1);

        private static CellPosition FirstSpineInterior(CellPosition joint, Direction outward) =>
            joint - ImperialSpineStep(outward) * (outward == Direction.Up ? 2 : 1);

        private static bool IsSpineVertical(Direction direction) => direction is Direction.Up or Direction.Down;

        private static CellPosition ImperialSpineStep(Direction direction) => direction switch
        {
            Direction.Up => new CellPosition(0, 1),
            Direction.Down => new CellPosition(0, -1),
            Direction.Left => new CellPosition(-1, 0),
            Direction.Right => new CellPosition(1, 0),
            _ => CellPosition.Zero,
        };
    }
}

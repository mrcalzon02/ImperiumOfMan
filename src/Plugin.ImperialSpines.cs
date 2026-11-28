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
        private const int ImperialSpinePresetId = -280000;
        private const int ImperialSpineCanvasLimit = 248;
        private static readonly CellPosition SpineNorth = new(0, 1);

        [HarmonyPatch(typeof(ExordiumDungeonGenerator), nameof(ExordiumDungeonGenerator.Generate))]
        private static class ImperialBranchGeneration
        {
            // Use the native room selection and rules even when the stock canvas could not fit the rooms.
            [HarmonyPriority(Priority.First)]
            private static void Postfix(ExordiumDungeonGenerator __instance, DungeonGenerationPlan plan,
                PresetLoader loadedPresets, InputGenData inputGenData, ref bool __result)
            {
                if (_imperialRoomGeneration?.Palette == null) return;
                try
                {
                    if (TryBuildBranchLevel(__instance, plan, loadedPresets, inputGenData, __result)) __result = true;
                }
                catch (Exception error)
                {
                    Debug.LogWarning("[Cvar_ImperiumOfMan] Could not generate the expanded hall layout. " + error);
                }
                finally
                {
                    __instance.FinalMapGrid.RefreshFenceCheckRef();
                }
            }

            private static bool TryBuildBranchLevel(ExordiumDungeonGenerator generator, DungeonGenerationPlan plan,
                PresetLoader loadedPresets, InputGenData inputGenData, bool nativeSucceeded)
            {
                var scope = _imperialRoomGeneration;
                TryGetCampaignRouteLayout(plan, out var route, out int floor);
                if ((generator.LevelStructure.NoDoors && route == null) ||
                    inputGenData.EntryLaddersCount != 0 || inputGenData.ExitLaddersCount != 0)
                    return false;

                string horizontalDoor = plan.HorDoorIds.FirstOrDefault();
                string verticalDoor = plan.VertDoorIds.FirstOrDefault();
                var factory = SingletonMonoBehaviour<MapObstacleFactory>.Instance;
                if (!IsDoor(horizontalDoor, Door.Orientation.Horizontal) || !IsDoor(verticalDoor, Door.Orientation.Vertical))
                {
                    if (route == null) return false;
                    var nativeDoors = new DungeonGenerationPlan();
                    if (!IsDoor(horizontalDoor, Door.Orientation.Horizontal)) horizontalDoor = nativeDoors.HorDoorIds[0];
                    if (!IsDoor(verticalDoor, Door.Orientation.Vertical)) verticalDoor = nativeDoors.VertDoorIds[0];
                    if (!IsDoor(horizontalDoor, Door.Orientation.Horizontal) || !IsDoor(verticalDoor, Door.Orientation.Vertical))
                    {
                        Debug.LogWarning("[Cvar_ImperiumOfMan] Native bulkhead definitions unavailable for " + route.Key);
                        return false;
                    }
                }

                var selected = NativeSpineRooms(generator.LevelStructure.RoomsByGroups).ToArray();
                var seen = new HashSet<CommonRoom>(selected.Select(entry => entry.Room));
                // The stock starter room is removed from the selection as soon as it is inserted.
                var required = NativeSpineRooms(generator.LevelStructure.InsertedRoomsByGroups)
                    .Where(entry => seen.Add(entry.Room)).Concat(selected).ToArray();
                var donorIds = plan.Rules
                    .Where(rule => rule.RuleType is PresetRuleType.RoomsGroup or PresetRuleType.AdditionalRooms)
                    .SelectMany(rule => rule.PresetBinaryIds).Distinct().ToArray();
                var donorHeaders = donorIds.Select(loadedPresets.GetLoadedPreset).Where(IsImperialSpineDonor).ToList();
                var preferredRoomIds = route?.PreferredRoomIds ?? Array.Empty<int>();
                if (preferredRoomIds.Length > 0)
                {
                    var loadedIds = new HashSet<int>(donorHeaders.Select(header => header.binaryId));
                    foreach (int binaryId in preferredRoomIds)
                    {
                        if (!loadedIds.Add(binaryId)) continue;
                        var native = loadedPresets.LoadSpecificPreset(binaryId);
                        if (IsImperialSpineDonor(native)) donorHeaders.Add(native);
                    }
                }
                var donors = donorHeaders.ToArray();
                var endgameDonors = donors;
                if (required.Length == 0 || donors.Length == 0)
                {
                    if (route != null) Debug.LogWarning($"[Cvar_ImperiumOfMan] Missing native room selection for {route.Key}.");
                    return false;
                }

                int seed = scope.State.Get<LocationMetadata>().Seed;
                int width = Math.Min(ImperialSpineCanvasLimit, Math.Max(Math.Max(156, plan.MapGridWidth + 45),
                    (route?.HorizontalMinimum ?? 0) + 66));
                int height = Math.Min(ImperialSpineCanvasLimit, Math.Max(Math.Max(156, plan.MapGridHeight + 45),
                    (route?.VerticalMinimum ?? 0) + 66));
                var layout = PlanWithRoomVariants(width, height, maximumExtension: 3);
                if ((layout?.Elbows.Count ?? 0) < 4 && (width < ImperialSpineCanvasLimit || height < ImperialSpineCanvasLimit))
                {
                    var expanded = PlanWithRoomVariants(ImperialSpineCanvasLimit, ImperialSpineCanvasLimit, maximumExtension: 3);
                    if (expanded != null && expanded.Elbows.Count > (layout?.Elbows.Count ?? 0))
                    {
                        width = height = ImperialSpineCanvasLimit;
                        layout = expanded;
                    }
                }
                if (layout == null)
                {
                    width = height = ImperialSpineCanvasLimit;
                    layout = PlanWithRoomVariants(width, height, maximumExtension: 2);
                    if (layout == null) TryPlanImperialSpines(required, donors, width, height, seed, out layout, route, floor);
                }
                if (layout == null)
                {
                    if (nativeSucceeded)
                        Debug.Log($"[Cvar_ImperiumOfMan] Native branch hall layout could not fit room joints for {route?.Key ?? "native raid"}.");
                    return false;
                }

                int originalFloorCells = 0;
                int finalFloorCells = 0;
                if (route != null)
                {
                    originalFloorCells = CampaignPlanFloorCount(layout);
                    finalFloorCells = originalFloorCells;
                    int floorTarget = originalFloorCells * CampaignEndgameMultiplier;
                    endgameDonors = CampaignCombatDonors(route, loadedPresets, donors);
                    int largeWidth = Math.Min(ImperialSpineCanvasLimit, (int)Math.Ceiling(width * Math.Sqrt(CampaignEndgameMultiplier)));
                    int largeHeight = Math.Min(ImperialSpineCanvasLimit, (int)Math.Ceiling(height * Math.Sqrt(CampaignEndgameMultiplier)));
                    const int additionalCampaignRun = 26;
                    var attempts = new[]
                    {
                        (Width: largeWidth, Height: largeHeight, Tier: 2, ExtraHorizontal: 0, ExtraVertical: 0),
                        (Width: ImperialSpineCanvasLimit, Height: ImperialSpineCanvasLimit, Tier: 2, ExtraHorizontal: 0, ExtraVertical: 0),
                        (Width: ImperialSpineCanvasLimit, Height: ImperialSpineCanvasLimit, Tier: 2, ExtraHorizontal: additionalCampaignRun, ExtraVertical: 0),
                        (Width: ImperialSpineCanvasLimit, Height: ImperialSpineCanvasLimit, Tier: 2, ExtraHorizontal: 0, ExtraVertical: additionalCampaignRun),
                        (Width: ImperialSpineCanvasLimit, Height: ImperialSpineCanvasLimit, Tier: 2, ExtraHorizontal: additionalCampaignRun, ExtraVertical: additionalCampaignRun),
                        (Width: ImperialSpineCanvasLimit, Height: ImperialSpineCanvasLimit, Tier: 1, ExtraHorizontal: 0, ExtraVertical: 0),
                        (Width: ImperialSpineCanvasLimit, Height: ImperialSpineCanvasLimit, Tier: 0, ExtraHorizontal: 0, ExtraVertical: 0)
                    }.Distinct();
                    foreach (var attempt in attempts)
                    {
                        var enlarged = PlanWithRoomVariants(attempt.Width, attempt.Height, maximumExtension: 3, attempt.Tier,
                            floorTarget, attempt.ExtraHorizontal, attempt.ExtraVertical);
                        if (enlarged == null || enlarged.Elbows.Count < layout.Elbows.Count) continue;
                        int candidateFloors = CampaignPlanFloorCount(enlarged);
                        if (candidateFloors <= finalFloorCells ||
                            !AuditImperialSpinePlan(enlarged, attempt.Width, attempt.Height, out _))
                            continue;

                        layout = enlarged;
                        width = attempt.Width;
                        height = attempt.Height;
                        finalFloorCells = candidateFloors;
                        if (finalFloorCells >= floorTarget) break;
                    }
                }

                if (!AuditImperialSpinePlan(layout, width, height, out string failure))
                {
                    Debug.LogWarning("[Cvar_ImperiumOfMan] Branch hall plan rejected. " + failure);
                    return false;
                }

                // Extra rooms are strictly optional: keep the already
                // audited original plan and fall back if native stamping fails.
                var baseLayout = layout;
                if (!scope.Palette.Warp)
                    layout = TryAddImperialVarietyRooms(layout, loadedPresets, donors, route, width, height, seed);
                ImperialSpineCanvas draft = null;
                Dictionary<short, List<CommonRoom>> roomGroups = null;
                if (layout != baseLayout)
                {
                    try
                    {
                        (draft, roomGroups) = BuildDraft(layout);
                        if (!AuditStampedImperialSpines(layout, draft.FinalMapGrid, draft.FinalMapMetadata,
                            horizontalDoor, verticalDoor, out failure))
                        {
                            Debug.LogWarning("[Cvar_ImperiumOfMan] Optional variety rooms rejected. " + failure);
                            layout = baseLayout;
                        }
                    }
                    catch (Exception error)
                    {
                        // Optional preset contents can change across game builds.
                        // Do not discard the working layout on a stamping error.
                        Debug.LogWarning("[Cvar_ImperiumOfMan] Optional room stamping failed: " + error);
                        layout = baseLayout;
                    }
                }
                if (layout == baseLayout)
                {
                    (draft, roomGroups) = BuildDraft(layout);
                    if (!AuditStampedImperialSpines(layout, draft.FinalMapGrid, draft.FinalMapMetadata,
                        horizontalDoor, verticalDoor, out failure))
                    {
                        Debug.LogWarning("[Cvar_ImperiumOfMan] Branch hall map rejected. " + failure);
                        return false;
                    }
                }
                if (layout != baseLayout)
                {
                    Debug.Log("[Cvar_ImperiumOfMan] Optional Imperial native rooms=" +
                        (layout.Rooms.Count - baseLayout.Rooms.Count));
                    if (route != null) finalFloorCells = CampaignPlanFloorCount(layout);
                }

                (ImperialSpineCanvas Draft, Dictionary<short, List<CommonRoom>> Groups) BuildDraft(ImperialSpinePlan selectedLayout)
                {
                    var canvas = new ImperialSpineCanvas(width, height, plan);
                    var groups = BuildImperialSpineMap(canvas, selectedLayout, scope.Palette, horizontalDoor, verticalDoor);
                    DressImperialSpines(canvas.FinalMapGrid, canvas.FinalMapMetadata, selectedLayout, scope.Palette.Warp, seed);
                    DressImperialSupplyClosets(canvas.FinalMapGrid, canvas.FinalMapMetadata, selectedLayout, scope.Palette.Warp, seed);
                    canvas.FillExterior(plan);
                    if (route != null)
                        DressCampaignEndgameViews(canvas.FinalMapGrid, canvas.FinalMapMetadata, selectedLayout, plan, scope.Palette.Warp, seed);
                    if (!scope.Palette.Warp)
                        DressImperialNativeRooms(canvas.FinalMapGrid, canvas.FinalMapMetadata, selectedLayout, donors, seed);
                    return (canvas, groups);
                }

                string straight = string.Join(" / ", layout.Spines.Select((run, index) =>
                    $"{run.Start}->{run.End} ({layout.Doors.Count(door => door.Spine == index && !door.End)} side)"));
                string elbows = string.Join(" / ", layout.Elbows.Select((run, index) =>
                    $"{run.Start}->{run.Bend}->{run.End} ({layout.Doors.Count(door => door.Spine == index + layout.Spines.Length && !door.End)} side)"));
                int closets = layout.Rooms.Count(placement => IsImperialSupplyClosetPreset(placement.Room.header.binaryId));
                AdoptImperialSpineMap(generator, draft, roomGroups, layout);
                Debug.Log($"[Cvar_ImperiumOfMan] Imperial branching halls {width}x{height} rooms={layout.Rooms.Count} supply closets={closets} {straight} elbows={elbows}");
                if (route != null)
                {
                    var chosenRooms = new HashSet<int>(layout.Rooms.Select(placement => placement.Room.header.binaryId));
                    int preferredRooms = preferredRoomIds.Count(chosenRooms.Contains);
                    Debug.Log($"[Cvar_ImperiumOfMan] Campaign layout {route.Key} floor={floor} preferred native rooms={preferredRooms} ({route.Title})");
                    Debug.Log($"[Cvar_ImperiumOfMan] Campaign endgame layout {route.Key} floor={floor} floor cells {originalFloorCells} → {finalFloorCells}");
                }
                return true;

                bool IsDoor(string id, Door.Orientation orientation) => !string.IsNullOrEmpty(id) &&
                    factory.GetObstaclePrefab(id)?.gameObject.GetComponent<Door>()?.DoorOrientation == orientation;

                ImperialSpinePlan PlanWithRoomVariants(int canvasWidth, int canvasHeight, int maximumExtension,
                    int endgameTier = 0, int floorTarget = 0, int extraHorizontal = 0, int extraVertical = 0)
                {
                    var rooms = required.Select((entry, index) =>
                    {
                        int roomSeed = unchecked(seed + index * 397);
                        bool combat = endgameTier > 0 && (entry.Group == ExordiumDungeonGenerator.NoneGroupId || entry.Group == 2) &&
                            IsImperialSpineDonor(entry.Room.header);
                        var expanded = combat
                            ? GetCampaignEndgameRoomVariant(generator, entry.Room.header, canvasWidth, canvasHeight, roomSeed, endgameTier)
                            : GetImperialSpineRoomVariant(generator, entry.Room.header, canvasWidth, canvasHeight, roomSeed, maximumExtension);
                        if (expanded == entry.Room.header) return entry;
                        var room = entry.Room.Clone();
                        room.header = expanded;
                        return (Room: room, entry.Group);
                    }).ToArray();
                    var donorPool = floorTarget > 0 ? endgameDonors : donors;
                    var expandedDonors = donorPool.Select(header => endgameTier > 0
                        ? GetCampaignEndgameRoomVariant(generator, header, canvasWidth, canvasHeight, seed, endgameTier)
                        : GetImperialSpineRoomVariant(generator, header, canvasWidth, canvasHeight, seed, maximumExtension)).ToArray();
                    TryPlanImperialSpines(rooms, expandedDonors, canvasWidth, canvasHeight, seed, out var candidate, route,
                        floor, endgameTier, extraHorizontal, extraVertical);
                    if (candidate != null && floorTarget > 0)
                        AddCampaignEndgameCombatRooms(candidate, expandedDonors, route, floorTarget, canvasWidth, canvasHeight);
                    return candidate;
                }
            }

            private static IEnumerable<(CommonRoom Room, short Group)> NativeSpineRooms(
                Dictionary<short, List<CommonRoom>> groups) => groups
                .Where(group => group.Key != ExordiumDungeonGenerator.CorridorsGroupId)
                .SelectMany(group => group.Value.Select(room => (room, group.Key)));
        }

        private static bool IsImperialSpineDonor(PresetHeader header)
        {
            if (header?.presetType != PresetType.Room || header.presetNamespace?.Kind != PresetNamespaceKind.Core ||
                header.genMetadata.SpawnUnique || header.mapMetadata.LevelEntries.Count != 0 ||
                header.mapMetadata.LevelExits.Count != 0 || header.mapMetadata.MapTriggers.Any())
                return false;

            var factory = SingletonMonoBehaviour<MapObstacleFactory>.Instance;
            return !header.mapMetadata.MapObstacles.Any(obj =>
            {
                var prefab = factory.GetObstaclePrefab(obj.ObstacleName);
                return prefab != null && (prefab.gameObject.GetComponent<InterfloorLadder>() != null ||
                    prefab.gameObject.GetComponent<InteractElevator>() != null ||
                    prefab.gameObject.GetComponent<Shuttle>() != null ||
                    prefab.gameObject.GetComponent<ObjectiveObstacle>() != null ||
                    prefab.gameObject.GetComponent<QuestInteractiveStorage>() != null);
            });
        }

        private sealed class ImperialSpineCanvas : DungeonGenerator
        {
            internal ImperialSpineCanvas(int width, int height, DungeonGenerationPlan plan) : base(null, width, height)
            {
                InitBrushes(plan);
                Generate(plan, null, default);
            }

            internal void PlaceRoom(CommonRoom room, ImperialDamagePalette palette)
            {
                var header = room.header;
                var source = header.MapGrid;
                var origin = room.LeftLowerCorner;
                bool supplyCloset = IsImperialSupplyClosetPreset(header.binaryId);
                for (int y = 0; y < source.MaxHeight; y++)
                for (int x = 0; x < source.MaxWidth; x++)
                {
                    var local = new CellPosition(x, y);
                    var dest = origin + local;
                    var native = source.GetCell(local);
                    var target = FinalMapGrid.GetCell(dest);
                    if (native.Type != MapCellType.None &&
                        (target.Type != MapCellType.FacadeWall || native.Type != MapCellType.Wall))
                    {
                        target.Clone(native);
                        target.X = (byte)dest.X;
                        target.Y = (byte)dest.Y;
                        if (supplyCloset)
                        {
                            if (target.Type == MapCellType.Wall)
                                PaintDeckCell(target, MapCellType.Wall, palette.Wall);
                            else if (target.Type == MapCellType.FacadeWall)
                                PaintDeckCell(target, MapCellType.FacadeWall, palette.Facade);
                            else if (target.Type == MapCellType.Floor)
                                PaintPassage(dest);
                        }
                    }
                    CopyRoomFence(source, origin, local, local + new CellPosition(1, 0));
                    CopyRoomFence(source, origin, local, local + SpineNorth);
                }
                ProcessSys(origin.X, origin.Y, header);
                ProcessContent(origin.X, origin.Y, header);
                ProcessMapObjects(origin.X, origin.Y, header);
            }

            private void CopyRoomFence(MapGrid source, CellPosition origin, CellPosition local, CellPosition adjacent)
            {
                if (source.GetFence(local, adjacent)) FinalMapGrid.SetFence(origin + local, origin + adjacent, true);
            }

            internal void PaintPassage(CellPosition pos)
            {
                var cell = FinalMapGrid.GetCell(pos);
                var brush = SB0();
                cell.Type = MapCellType.Floor;
                cell.SetTilesetId(1, brush.CurrentTileset.ShortId);
                cell.SetTileIndex(1, (byte)brush.CurrentTileIndex);
                cell.SetTilesetId(2, 0);
                cell.SetTileIndex(2, byte.MaxValue);
            }

            internal void PlaceDoor(ImperialSpineDoor door, string id)
            {
                foreach (var opening in door.Opening) PaintPassage(opening);
                var step = ImperialSpineStep(door.Outward);
                foreach (var opening in door.Opening)
                {
                    FinalMapGrid.SetFence(opening, opening + step, false);
                    FinalMapGrid.SetFence(opening, opening - step, false);
                }
                DrawDoor(id, door.Position);
            }

            internal void FillExterior(DungeonGenerationPlan plan)
            {
                _fillIsOutdoor = plan.GetParameter("FillIsOutdoor", out var value) && ParseHelper.ParseBool(value);
                for (int y = 0; y < CanvasHeight; y++)
                for (int x = 0; x < CanvasWidth; x++)
                    Canvas[x, y] = (int)FinalMapGrid.GetCell(x, y).Type;

                if (plan.GetRule(PresetRuleType.FillBordersSpace) != null) FillVoidsSpace();
                if (plan.GetRule(PresetRuleType.FillBordersSolid) != null) FillVoidsSolid();
                if (plan.GetRule(PresetRuleType.FillBordersAsteroid) != null) FillVoidsAsteroid();
                if (plan.GetRule(PresetRuleType.FillBordersStation) != null) FillInnerSolidOuterSpace();
            }
        }

        private static Dictionary<short, List<CommonRoom>> BuildImperialSpineMap(ImperialSpineCanvas draft,
            ImperialSpinePlan layout, ImperialDamagePalette palette, string horizontalDoor, string verticalDoor)
        {
            var groups = new Dictionary<short, List<CommonRoom>>();
            foreach (var placement in layout.Rooms.OrderBy(room => room.Group))
            {
                draft.PlaceRoom(placement.Room, palette);
                if (!groups.TryGetValue(placement.Group, out var rooms))
                    groups.Add(placement.Group, rooms = new List<CommonRoom>());
                rooms.Add(placement.Room);
            }

            foreach (var pos in layout.CorridorCells) draft.PaintPassage(pos);
            foreach (var door in layout.Doors)
                draft.PlaceDoor(door, door.Outward is Direction.Up or Direction.Down ? horizontalDoor : verticalDoor);
            BuildImperialSpineWalls(draft.FinalMapGrid, layout.CorridorCells, palette);

            var corridors = new List<CommonRoom>();
            foreach (var (start, end) in layout.Spines)
                AddImperialSpineRun(corridors, start, end, draft.FinalMapGrid);

            foreach (var (start, bend, end) in layout.Elbows)
            {
                // Extend the two rectangles by one at the bend so their union records the full turn.
                AddImperialSpineRun(corridors, start, bend + AxisStep(start, bend), draft.FinalMapGrid);
                AddImperialSpineRun(corridors, bend - AxisStep(bend, end), end, draft.FinalMapGrid);
            }
            groups.Add(ExordiumDungeonGenerator.CorridorsGroupId, corridors);
            return groups;
        }

        private static CellPosition AxisStep(CellPosition from, CellPosition to) =>
            new(Math.Sign(to.X - from.X), Math.Sign(to.Y - from.Y));

        private static void BuildImperialSpineWalls(MapGrid grid, HashSet<CellPosition> passage, ImperialDamagePalette palette)
        {
            var shell = new HashSet<CellPosition>();
            foreach (var pos in passage)
            for (int dy = -1; dy <= 1; dy++)
            for (int dx = -1; dx <= 1; dx++)
            {
                var edge = pos + new CellPosition(dx, dy);
                if (grid.GetCell(edge)?.Type == MapCellType.None) shell.Add(edge);
            }
            foreach (var pos in shell) PaintDeckCell(grid.GetCell(pos), MapCellType.Wall, palette.Wall);
            foreach (var pos in shell)
            {
                if (!passage.Contains(pos - SpineNorth)) continue;
                var cap = grid.GetCell(pos + SpineNorth);
                if (cap?.Type == MapCellType.None) PaintDeckCell(cap, MapCellType.Wall, palette.Wall);
                if (cap?.Type == MapCellType.Wall)
                    PaintDeckCell(grid.GetCell(pos), MapCellType.FacadeWall, palette.Facade);
            }
        }

        private static void AddImperialSpineRun(List<CommonRoom> runs, CellPosition start, CellPosition end, MapGrid grid)
        {
            bool horizontal = start.Y == end.Y;
            int xMargin = horizontal ? 0 : 1;
            int yMargin = horizontal ? 1 : 0;
            var header = new PresetHeader
            {
                binaryId = ImperialSpinePresetId - runs.Count,
                presetType = horizontal ? PresetType.HorizCorridor : PresetType.VertCorridor,
                genMetadata = new MapGenMetadata(grid)
            };
            var direction = horizontal ? (start.X < end.X ? Direction.Right : Direction.Left) :
                (start.Y < end.Y ? Direction.Up : Direction.Down);
            runs.Add(new Corridor(direction, start, end)
            {
                header = header,
                LeftLowerCorner = new CellPosition(Math.Min(start.X, end.X) - xMargin, Math.Min(start.Y, end.Y) - yMargin),
                RightUpperCorner = new CellPosition(Math.Max(start.X, end.X) + xMargin, Math.Max(start.Y, end.Y) + yMargin)
            });
        }

        private static void AdoptImperialSpineMap(ExordiumDungeonGenerator generator, ImperialSpineCanvas draft,
            Dictionary<short, List<CommonRoom>> groups, ImperialSpinePlan layout)
        {
            generator.SetFinalData(draft.FinalMapGrid, draft.FinalMapMetadata);
            var level = generator.LevelStructure;
            level.RoomsByGroups = groups;
            level.InsertedRoomsByGroups.Clear();
            level.FreeSpaceByGroups.Clear();
            level.Corridors.Clear();
            level.CorridorCrossroads.Clear();
            level.DoorCells.Clear();
            level.DoorCells.AddRange(layout.Doors.Select(door => door.Position));
            level.Corridors.AddRange(groups[ExordiumDungeonGenerator.CorridorsGroupId].Cast<Corridor>());
            foreach (var elbow in layout.Elbows) level.CorridorCrossroads.Add(elbow.Bend);
            foreach (var group in groups)
            foreach (var room in group.Value)
                level.InsertRoom(room, group.Key == 2 ? ExordiumDungeonGenerator.NoneGroupId : group.Key);
            foreach (var group in level.InsertedRoomsByGroups)
            {
                int count = 0;
                foreach (var room in group.Value)
                for (int y = room.LeftLowerCorner.Y; y < room.RightUpperCorner.Y; y++)
                for (int x = room.LeftLowerCorner.X; x < room.RightUpperCorner.X; x++)
                    if (draft.FinalMapGrid.GetCell(x, y).Type is MapCellType.Floor or MapCellType.LowGround) count++;
                level.FreeSpaceByGroups.Add(group.Key, count);
            }
        }

        private static ISet<CellPosition> GetImperialBranchFloor(State state)
        {
            HashSet<CellPosition> cells = null;
            foreach (var room in state.Get<LocationMetadata>().GeneratedRooms)
            {
                if (room.GroupId != ExordiumDungeonGenerator.CorridorsGroupId ||
                    room.PresetBinaryId > ImperialSpinePresetId || room.PresetBinaryId < ImperialSpinePresetId - 9)
                    continue;
                cells ??= new HashSet<CellPosition>();
                for (int y = room.LeftLowerCorner.Y; y <= room.RightUpperCorner.Y; y++)
                for (int x = room.LeftLowerCorner.X; x <= room.RightUpperCorner.X; x++)
                    cells.Add(new CellPosition(x, y));
            }
            return cells;
        }
    }
}

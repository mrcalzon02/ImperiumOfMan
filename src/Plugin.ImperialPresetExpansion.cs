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
        private const int ImperialRoomExtension = 2;
        private const int ImperialBroadRoomExtension = 3;
        private const int ImperialBroadRoomAttempts = 2;
        private const int ImperialRoomExpansionAttempts = 5;

        [ThreadStatic] private static ImperialRoomGeneration _imperialRoomGeneration;

        private sealed class ImperialRoomGeneration
        {
            internal readonly Dictionary<(PresetHeader Source, int Width, int Height), PresetHeader> Variants = new();
            internal readonly Dictionary<(PresetHeader Source, int Width, int Height, int Shape, int Tier), PresetHeader>
                EndgameVariants = new();
            internal readonly Dictionary<PresetHeader, PresetHeader> OriginalRooms = new();
            internal readonly Dictionary<PresetHeader, HashSet<CellPosition>> FixedFixtureCells = new();
            internal readonly Dictionary<string, MapObstacle> FixtureSamples = new(StringComparer.Ordinal);
            internal readonly Dictionary<(string Id, CellPosition Offset), CellPosition[]> FixtureFootprints = new();
            internal State State;
            internal ImperialDamagePalette Palette;
            internal bool ProtectCampaignLandings;
            internal GameObject FixtureSampleHolder;
            internal int Attempt;
            internal int BroadRoomArea;
            internal int BroadObjectiveArea;
        }

        [HarmonyPatch(typeof(DungeonBuilder), "GenerateGeometry")]
        private static class ImperialRoomGenerationScope
        {
            private static void Prefix(State ____state, InputMapData inputMapData, out ImperialRoomGeneration __state)
            {
                __state = _imperialRoomGeneration;
                var raid = ____state.Get<RaidMetadata>();
                var mission = raid?.RaidType == RaidType.StoryMission ? ____state.Get<Missions>().Get(raid) : null;
                DungeonGenerationPlan plan = null;
                bool campaign = mission != null && mission.LocationPlans.TryGetValue(inputMapData.locationId, out plan) &&
                    TryGetCampaignDeck(plan, out _, out _);
                var palette = GetImperialRaidPalette(____state);
                _imperialRoomGeneration = !inputMapData.forceLoadSpecificPreset &&
                    (campaign || palette != null) ? new ImperialRoomGeneration
                    {
                        State = ____state,
                        Palette = palette,
                        ProtectCampaignLandings = campaign && TryGetCampaignRouteLayout(plan, out _, out _)
                    } : null;
            }

            private static Exception Finalizer(Exception __exception, ImperialRoomGeneration __state)
            {
                var finished = _imperialRoomGeneration;
                _imperialRoomGeneration = __state;
                if (finished != null && finished != __state && finished.FixtureSampleHolder != null)
                    UnityEngine.Object.Destroy(finished.FixtureSampleHolder);
                return __exception;
            }
        }

        [HarmonyPatch(typeof(ExordiumDungeonGenerator), nameof(ExordiumDungeonGenerator.Generate))]
        private static class ImperialRoomExpansionRetry
        {
            private static void Postfix(bool __result)
            {
                if (!__result && _imperialRoomGeneration != null)
                {
                    _imperialRoomGeneration.Attempt++;
                    _imperialRoomGeneration.BroadRoomArea = 0;
                    _imperialRoomGeneration.BroadObjectiveArea = 0;
                }
            }
        }

        [HarmonyPatch(typeof(DungeonGenerator), "RandomizeRoom", typeof(PresetRule), typeof(PresetLoader))]
        private static class PreferImperialRoomVariant
        {
            private static void Postfix(DungeonGenerator __instance, PresetRule rule, ref PresetHeader __result)
            {
                var generation = _imperialRoomGeneration;
                if (generation == null || generation.Attempt >= ImperialRoomExpansionAttempts ||
                    __result?.presetType != PresetType.Room || __result.presetNamespace?.Kind != PresetNamespaceKind.Core ||
                    rule.RuleType is not (PresetRuleType.RoomsGroup or PresetRuleType.AdditionalRooms or
                    PresetRuleType.NoMonsterSpawnGroup or PresetRuleType.ObjectiveRooms or PresetRuleType.Lifts or PresetRuleType.Spaceships))
                {
                    return;
                }

                var source = __result;
                var map = source.MapGrid;
                int baseArea = map.MaxWidth * map.MaxHeight;
                int shortSide = Math.Min(map.MaxWidth, map.MaxHeight);
                int canvasArea = __instance.CanvasWidth * __instance.CanvasHeight;
                bool objective = rule.RuleType == PresetRuleType.ObjectiveRooms;
                bool spacious = objective ? shortSide >= 6 && baseArea >= 64
                    : (rule.RuleType is PresetRuleType.RoomsGroup or PresetRuleType.AdditionalRooms) &&
                      shortSide >= 7 && baseArea >= 96;
                if (spacious && generation.Attempt < ImperialBroadRoomAttempts)
                {
                    // Objective rooms have their own allowance so ordinary rooms cannot use it up first.
                    int budget = canvasArea / (objective ? 22 : 12);
                    ref int used = ref (objective ? ref generation.BroadObjectiveArea : ref generation.BroadRoomArea);
                    if (used < budget)
                    {
                        int seed = generation.State.Get<LocationMetadata>().Seed;
                        var (width, height) = ImperialRoomExtensions(source, __instance.CanvasWidth, __instance.CanvasHeight,
                            seed, ImperialBroadRoomExtension);
                        var broad = GetImperialRoomVariant(generation, __instance, source, width, height);
                        var expanded = broad.MapGrid;
                        int addedArea = expanded.MaxWidth * expanded.MaxHeight - baseArea;
                        if (used + addedArea <= budget)
                        {
                            if (expanded.MaxWidth - map.MaxWidth > ImperialRoomExtension ||
                                expanded.MaxHeight - map.MaxHeight > ImperialRoomExtension)
                                used += addedArea;
                            __result = broad;
                            return;
                        }
                    }
                }
                var (standardWidth, standardHeight) = ImperialRoomExtensions(source, __instance.CanvasWidth,
                    __instance.CanvasHeight, seed: 0, ImperialRoomExtension);
                __result = GetImperialRoomVariant(generation, __instance, source, standardWidth, standardHeight);
            }
        }

        private static PresetHeader GetImperialSpineRoomVariant(DungeonGenerator generator, PresetHeader source,
            int canvasWidth, int canvasHeight, int seed, int maximumExtension = ImperialBroadRoomExtension)
        {
            var generation = _imperialRoomGeneration;
            if (generation == null || source?.presetType != PresetType.Room ||
                source.presetNamespace?.Kind != PresetNamespaceKind.Core)
                return source;

            // The stock room selector may have already chosen a variant on the smaller canvas.
            if (generation.OriginalRooms.TryGetValue(source, out var original)) source = original;
            var (width, height) = ImperialRoomExtensions(source, canvasWidth, canvasHeight, seed, maximumExtension);
            return GetImperialRoomVariant(generation, generator, source, width, height);
        }

        private static (int Width, int Height) ImperialRoomExtensions(PresetHeader source, int canvasWidth,
            int canvasHeight, int seed, int maximumExtension)
        {
            int width = Math.Min(maximumExtension, ImperialBroadRoomExtension);
            int height = width;
            if (maximumExtension == ImperialBroadRoomExtension)
            {
                int shape = (source.binaryId ^ seed) & 3;
                if (shape == 1) width = ImperialRoomExtension;
                if (shape == 2) height = ImperialRoomExtension;
            }
            return (ExtensionForAxis(source.MapGrid.MaxWidth, canvasWidth, width),
                ExtensionForAxis(source.MapGrid.MaxHeight, canvasHeight, height));
        }

        private static PresetHeader GetImperialRoomVariant(ImperialRoomGeneration generation, DungeonGenerator generator,
            PresetHeader source, int widthExtension, int heightExtension)
        {
            if (widthExtension == 0 && heightExtension == 0) return source;
            var key = (source, widthExtension, heightExtension);
            if (generation.Variants.TryGetValue(key, out var variant)) return variant;

            try
            {
                variant = ExpandImperialRoom(source, widthExtension, heightExtension);
                generation.Variants.Add(key, variant);
                if (variant != source) generation.OriginalRooms.Add(variant, source);
                return variant;
            }
            finally
            {
                // MapGrid's constructor makes the most recent grid responsible for fence checks.
                generator.FinalMapGrid.RefreshFenceCheckRef();
            }
        }

        private readonly struct ImperialRoomSeam(int sourceLine, int extra)
        {
            internal readonly int SourceLine = sourceLine;
            internal readonly int Extra = extra;
            internal int Forward(int coordinate) => coordinate > SourceLine ? coordinate + Extra : coordinate;
            internal bool IsInserted(int coordinate) => coordinate > SourceLine && coordinate <= SourceLine + Extra;
            internal int Reverse(int coordinate) => IsInserted(coordinate)
                ? SourceLine + (coordinate - SourceLine > Extra / 2 ? 1 : 0)
                : coordinate > SourceLine ? coordinate - Extra : coordinate;
        }

        private static PresetHeader ExpandImperialRoom(PresetHeader source, int widthExtension, int heightExtension)
        {
            var xSeam = FindImperialRoomSeam(source, widthExtension, horizontal: true);
            var ySeam = FindImperialRoomSeam(source, heightExtension, horizontal: false);
            return BuildImperialRoomVariant(source, xSeam, ySeam);
        }

        private static PresetHeader BuildImperialRoomVariant(PresetHeader source, ImperialRoomSeam xSeam, ImperialRoomSeam ySeam)
        {
            if (xSeam.Extra == 0 && ySeam.Extra == 0) return source;

            var original = source.MapGrid;
            CellPosition Move(CellPosition pos) => new(xSeam.Forward(pos.X), ySeam.Forward(pos.Y));
            var grid = new MapGrid();
            grid.Resize(original.MaxWidth + xSeam.Extra, original.MaxHeight + ySeam.Extra, Direction.Center, out _, out _);
            for (int y = 0; y < grid.MaxHeight; y++)
            for (int x = 0; x < grid.MaxWidth; x++)
            {
                int oldX = xSeam.Reverse(x);
                int oldY = ySeam.Reverse(y);
                var cell = grid.GetCell(x, y, checkBorders: false);
                cell.Clone(original.GetCell(oldX, oldY, checkBorders: false));
                cell.X = (byte)x;
                cell.Y = (byte)y;
                if (xSeam.IsInserted(x) || ySeam.IsInserted(y))
                {
                    // Extend the adjoining materials through the seam.
                    // Painted signs, damage decals and fixture overlays keep their native positions.
                    cell.SetTilesetId(2, 0);
                    cell.SetTileIndex(2, byte.MaxValue);
                }

                var moved = new CellPosition(x, y);
                var old = new CellPosition(oldX, oldY);
                if (x + 1 < grid.MaxWidth && oldX != xSeam.Reverse(x + 1) &&
                    original.GetFence(old, new CellPosition(oldX + 1, oldY)))
                    grid.SetFence(moved, new CellPosition(x + 1, y), true);
                if (y + 1 < grid.MaxHeight && oldY != ySeam.Reverse(y + 1) &&
                    original.GetFence(old, new CellPosition(oldX, oldY + 1)))
                    grid.SetFence(moved, new CellPosition(x, y + 1), true);
            }

            var metadata = new MapMetadata();
            metadata.Clone(source.mapMetadata);
            foreach (var entry in metadata.LevelEntries.Concat(metadata.LevelExits)) entry.Position = Move(entry.Position);
            foreach (var entry in metadata.Monsters.Concat(metadata.Items)) entry.Position = Move(entry.Position);
            foreach (var prop in metadata.MapObstacles.Concat(metadata.MapLights).Concat(metadata.MapTriggers))
                prop.Pos = Move(prop.Pos);

            var gen = source.genMetadata;
            CellPosition[] MoveJoints(CellPosition[] positions) => positions.Select(Move).ToArray();
            return new PresetHeader
            {
                presetType = source.presetType,
                folder = source.folder,
                binaryId = source.binaryId,
                presetNamespace = source.presetNamespace,
                MapGrid = grid,
                mapMetadata = metadata,
                genMetadata = new MapGenMetadata(grid)
                {
                    AllowedJoints = MoveJoints(gen.AllowedJoints),
                    IgnoreConnectionJoints = MoveJoints(gen.IgnoreConnectionJoints),
                    UpperJoints = MoveJoints(gen.UpperJoints),
                    BottomJoints = MoveJoints(gen.BottomJoints),
                    LeftJoints = MoveJoints(gen.LeftJoints),
                    RightJoints = MoveJoints(gen.RightJoints),
                    ItemsTableIds = new List<string>(gen.ItemsTableIds),
                    MonstersTableIds = new List<string>(gen.MonstersTableIds),
                    SpecialItemsTableIds = new List<string>(gen.SpecialItemsTableIds),
                    SpecialMonstersTableIds = new List<string>(gen.SpecialMonstersTableIds),
                    SpecialItemsSpawnCount = gen.SpecialItemsSpawnCount,
                    SpecialMonstersSpawnCount = gen.SpecialMonstersSpawnCount,
                    PresetWeight = gen.PresetWeight,
                    SpawnUnique = gen.SpawnUnique,
                },
            };
        }

        private static int ExtensionForAxis(int length, int canvasLength, int desired)
        {
            int limit = canvasLength / 2 - 2;
            if (desired >= ImperialRoomExtension && length + desired <= limit)
                return desired;

            return desired == ImperialBroadRoomExtension && length + ImperialRoomExtension <= limit
                ? ImperialRoomExtension : 0;
        }

        private static ImperialRoomSeam FindImperialRoomSeam(PresetHeader preset, int extension, bool horizontal,
            ISet<CellPosition> endgameFloor = null)
        {
            var grid = preset.MapGrid;
            int length = horizontal ? grid.MaxWidth : grid.MaxHeight;
            int breadth = horizontal ? grid.MaxHeight : grid.MaxWidth;
            if (extension == 0 || length < 6 || preset.mapMetadata.MapTriggers.Any())
            {
                return default;
            }

            var fixtures = preset.mapMetadata.MapObstacles.Concat(preset.mapMetadata.MapLights).ToArray();
            int bestLine = -1, bestScore = -1;
            for (int line = 1; line <= length - 3; line++)
            {
                int floorCells = 0;
                int matchingMaterials = 0;
                bool valid = true;
                for (int across = 0; across < breadth; across++)
                {
                    var left = horizontal ? new CellPosition(line, across) : new CellPosition(across, line);
                    var right = horizontal ? new CellPosition(line + 1, across) : new CellPosition(across, line + 1);
                    var first = grid.GetCell(left, checkBorders: false);
                    var second = grid.GetCell(right, checkBorders: false);
                    bool roomSurface = first.Type is MapCellType.None or MapCellType.Floor or MapCellType.Wall or MapCellType.FacadeWall;
                    bool matchingRecess = endgameFloor != null && first.Type is (MapCellType.Deep or MapCellType.LowGround) &&
                        first.GetTilesetId(1) == second.GetTilesetId(1) && first.GetTileIndex(1) == second.GetTileIndex(1);
                    if (first.Type != second.Type || !horizontal && first.Type == MapCellType.FacadeWall ||
                        first.specialFlag != MapCellSpecialFlag.None || second.specialFlag != MapCellSpecialFlag.None ||
                        first.LiquidLevel != 0 || second.LiquidLevel != 0 || grid.GetFence(left, right) ||
                        !(roomSurface || matchingRecess) || endgameFloor != null && first.environmentIndex != second.environmentIndex)
                    {
                        valid = false;
                        break;
                    }
                    if (first.Type == MapCellType.Floor && (endgameFloor == null || endgameFloor.Contains(left))) floorCells++;
                    if (first.GetTilesetId(1) == second.GetTilesetId(1)) matchingMaterials++;
                }
                if (!valid || floorCells < 2 || fixtures.Any(prop => ImperialSeamCutsObject(prop, line, horizontal)))
                    continue;

                int score = (floorCells * length + matchingMaterials) * length - Math.Abs(2 * line + 1 - (length - 1));
                if (score > bestScore)
                {
                    bestLine = line;
                    bestScore = score;
                }
            }
            return bestLine < 0 ? default : new ImperialRoomSeam(bestLine, extension);
        }

        private static bool ImperialSeamCutsObject(MapObjectMetadata prop, int line, bool horizontal)
        {
            int position = horizontal ? prop.Pos.X : prop.Pos.Y;
            int offset = horizontal ? prop.Offset.X : prop.Offset.Y;
            int cut = (line + 1) * ImperialTilePixels;
            var donor = SingletonMonoBehaviour<MapObstacleFactory>.Instance.GetObstaclePrefab(prop.ObstacleName);
            var sprite = donor?.Renderer?.sprite;
            if (sprite == null) return position == line || position == line + 1;

            float pixelsToWorld = 100f / sprite.pixelsPerUnit;
            float start = position * ImperialTilePixels + offset - (horizontal ? sprite.pivot.x : sprite.pivot.y) * pixelsToWorld;
            float end = start + (horizontal ? sprite.rect.width : sprite.rect.height) * pixelsToWorld;
            return start < cut && cut < end;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MGSC;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private const short NativeSpineLitterBase = 28823;
        private const short CampaignSpineLitterBase = 28900;
        private static SurfaceTileset[,] _nativeSpineLitter;
        private static int[] _straightSpineTrimSlots;
        private static readonly HashSet<short> NativeSpineLitterIds = new();

        // Individual discarded materials, drawn in the same pixel grid as the floor atlas.
        private static readonly (string Name, string[] Pixels)[] SpineLitterArt =
        [
            ("paper", ["..iiii....", ".ipppfi...", ".ppfppp#..", ".pffppp#..", "..ppppi#..", "..##pp#..g", "....##..g."]),
            ("packing", [".....#....", "..oowo#...", ".owwowo#..", ".owowww#..", ".#oowow...", "..#o###.o.", ".o.....o#."]),
            ("wire", ["...rbb#...", "..br.rr#..", "..rb..r#..", "...r..rb..", "....rbr#..", ".g..###.b.", "..r......."]),
            ("metal", ["..mms.....", ".mssm#....", "msssm#.m..", ".#mm#..sm.", "...#..msm#", ".....m#m#.", "..r......g"]),
            ("foil", ["...gfg....", "..gpfg#...", "..pfpg#...", ".gfpg#....", ".gp###.g..", "..##..pg#.", ".......#.."]),
            ("rust", ["......r...", "...rr.b#..", "..rb#..r..", ".br#rr....", ".###rb#r..", "..r..##b..", "....g..#.."]),
        ];

        private static bool IsNativeSpineLitterId(short id) => NativeSpineLitterIds.Contains(id);

        private static void RegisterNativeSpineLitter(DescriptorsCollection descriptors, List<BaseTileset> all,
            Dictionary<short, BaseTileset> ids, Color32[] trim, int trimWidth, int trimHeight)
        {
            var kits = ImperialCampaignDecks.Concat(ChaosCampaignDecks).ToArray();
            int size = ImperialTilePixels;
            int height = SpineLitterArt.Length * 4 * size;
            _nativeSpineLitter = new SurfaceTileset[kits.Length, SpineLitterArt.Length];
            _straightSpineTrimSlots = [FloorJoinSlots[1], FloorJoinSlots[2], FloorJoinSlots[4], FloorJoinSlots[8]];
            NativeSpineLitterIds.Clear();
            foreach (var kit in kits)
            {
                int deck = kit.Floor.ShortId - CampaignTileIdBase;
                int original = OriginalSpinePaletteIndex(kit);
                bool warp = Array.IndexOf(ChaosCampaignDecks, kit) >= 0;
                // Keep the slot order of existing atlases. New routes only decorate straight edges.
                int trimShapes = original >= 0 ? CampaignJoinShapeCount : _straightSpineTrimSlots.Length;
                int width = size * trimShapes;
                int phaseSlots = trimShapes * 4;
                var pixels = new Color32[width * height];
                for (int variant = 0; variant < SpineLitterArt.Length; variant++)
                for (int phase = 0; phase < 4; phase++)
                for (int shape = 0; shape < trimShapes; shape++)
                {
                    int source = trimHeight - (deck * 4 + phase + 1) * size;
                    int target = height - (variant * 4 + phase + 1) * size;
                    int column = shape * size;
                    int sourceColumn = (original >= 0 ? shape : _straightSpineTrimSlots[shape]) * size;
                    for (int y = 0; y < size; y++)
                        Array.Copy(trim, (source + y) * trimWidth + sourceColumn, pixels, (target + y) * width + column, size);

                    // All pixels sit beyond the trim's seven-pixel wall margin.
                    var artwork = SpineLitterArt[variant].Pixels;
                    for (int row = 0; row < artwork.Length; row++)
                    for (int x = 0; x < artwork[row].Length; x++)
                    {
                        char shade = artwork[row][x];
                        if (shade == '.') continue;
                        int offsetX = 8 + (phase & 1) * 2 + ((phase & 2) == 0 ? x : artwork[row].Length - 1 - x);
                        int offsetY = 10 + (phase >> 1) * 2 + artwork.Length - 1 - row;
                        pixels[(target + offsetY) * width + column + offsetX] = SpineLitterColor(shade, warp);
                    }
                }

                var atlas = new Texture2D(width, height, TextureFormat.RGBA32, false)
                {
                    name = "Imperial corridor litter " + kit.Key,
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Clamp,
                };
                atlas.SetPixels32(pixels);
                atlas.Apply(false, true);
                for (int variant = 0; variant < SpineLitterArt.Length; variant++)
                {
                    var tileSet = ScriptableObject.CreateInstance<SurfaceTileset>();
                    tileSet.name = tileSet.Id = "iom_corridor_litter_" + kit.Key + "_" + SpineLitterArt[variant].Name;
                    int firstId = original >= 0 ? NativeSpineLitterBase + original * SpineLitterArt.Length :
                        CampaignSpineLitterBase + deck * SpineLitterArt.Length;
                    tileSet.SetShortId((short)(firstId + variant));
                    tileSet.Spritesheet = atlas;
                    tileSet.Tiles = new TileBase[phaseSlots];
                    AccessTools.Field(typeof(BaseTileset), "_decalPatterns").SetValue(tileSet, new List<AutoDecalMatchPattern>());
                    AccessTools.Field(typeof(BaseTileset), "_notUsableInPreset").SetValue(tileSet, true);
                    for (int i = 0; i < tileSet.Tiles.Length; i++)
                    {
                        var tile = ScriptableObject.CreateInstance<Tile>();
                        tile.sprite = SliceDamageSprite(atlas, variant * phaseSlots + i, trimShapes,
                            new Vector2(0.5f, 0.5f));
                        tile.colliderType = Tile.ColliderType.None;
                        tileSet.Tiles[i] = tile;
                    }
                    descriptors.AddDescriptor(tileSet.Id, tileSet);
                    all.Add(tileSet);
                    ids.Add(tileSet.ShortId, tileSet);
                    _nativeSpineLitter[deck, variant] = tileSet;
                    NativeSpineLitterIds.Add(tileSet.ShortId);
                }
            }
        }

        private static Color32 SpineLitterColor(char shade, bool warp) => shade switch
        {
            '#' => new(15, 17, 15, 126),
            'i' => warp ? new(139, 120, 103, 255) : new(177, 170, 137, 255),
            'p' => warp ? new(119, 101, 82, 255) : new(144, 136, 109, 255),
            'f' => new(107, 101, 80, 255),
            'g' => new(86, 87, 76, 255),
            'w' => new(97, 74, 49, 255),
            'o' => new(132, 96, 61, 255),
            'r' => warp ? new(126, 59, 41, 255) : new(115, 74, 46, 255),
            'b' => warp ? new(157, 84, 53, 255) : new(154, 110, 69, 255),
            'm' => new(83, 92, 90, 255),
            _ => new(125, 132, 122, 255),
        };

        private static void PaintNativeSpineLitter(State state, MapGrid grid, List<GeneratedRoomMetadata> rooms,
            ISet<CellPosition> spineFloor, List<MapObstacle> obstacles, DeckFloorZone[,] zones, CampaignDeck passage)
        {
            if (_nativeSpineLitter == null) return;
            var reserved = new HashSet<CellPosition>(obstacles.SelectMany(obstacle => obstacle.OccupiedCells));
            reserved.UnionWith(state.Get<ItemsOnFloor>().Values.Where(item => !item.Storage.Empty).Select(item => item.pos));
            foreach (var (_, far, step) in NativeSpineDoorLandings(spineFloor, obstacles))
            {
                // Keep a quiet landing on either side of the crosswalk.
                var axis = new CellPosition(step.Y, step.X);
                for (int across = -1; across <= 1; across++)
                for (int depth = -4; depth <= 1; depth++)
                    reserved.Add(far + step * depth + axis * across);
            }

            var runs = NativeSpineRuns(rooms);
            for (int index = 2; index + 1 < runs.Length; index += 2)
            {
                var (_, _, bend) = NativeSpineBend(runs[index], runs[index + 1]);
                for (int y = -1; y <= 1; y++)
                for (int x = -1; x <= 1; x++)
                    reserved.Add(bend + new CellPosition(x, y));
            }

            int deck = passage.Floor.ShortId - CampaignTileIdBase;
            short trim = _nativePresetTrims[deck].ShortId;
            short conveyor = _nativeSpineConveyors[deck].ShortId;
            bool fullTrim = OriginalSpinePaletteIndex(passage) >= 0;
            var service = NativeSpineServiceZone(passage);
            var random = new System.Random(state.Get<LocationMetadata>().Seed ^ 0x4d5c2b);
            var group = new List<(CellPosition Position, byte Tile)>(3);
            foreach (var run in runs)
            {
                bool horizontal = IsHorizontalSpineRun(run);
                int low = horizontal ? run.LeftLowerCorner.X : run.LeftLowerCorner.Y;
                int high = horizontal ? run.RightUpperCorner.X : run.RightUpperCorner.Y;
                int center = horizontal ? (run.LeftLowerCorner.Y + run.RightUpperCorner.Y) / 2 :
                    (run.LeftLowerCorner.X + run.RightUpperCorner.X) / 2;
                int side = random.Next(2) == 0 ? -1 : 1;
                for (int along = low + random.Next(2, 5); along < high; along += random.Next(5, 8))
                {
                    CollectGroup(side);
                    if (group.Count == 0) CollectGroup(-side);
                    foreach (var (pos, tile) in group)
                    {
                        var litter = _nativeSpineLitter[deck, random.Next(SpineLitterArt.Length)];
                        var cell = grid.GetCell(pos);
                        cell.SetTilesetId(2, litter.ShortId);
                        cell.SetTileIndex(2, tile);
                    }
                    if (random.Next(4) != 0) side = -side;

                    void CollectGroup(int lateral)
                    {
                        group.Clear();
                        int start = Math.Max(low + 1, along - random.Next(2));
                        int end = Math.Min(high - 1, start + random.Next(1, 3));
                        for (int length = start; length <= end; length++)
                        {
                            var pos = horizontal ? new CellPosition(length, center + lateral) :
                                new CellPosition(center + lateral, length);
                            if (!spineFloor.Contains(pos) || reserved.Contains(pos) || zones[pos.X, pos.Y] != service)
                                continue;
                            var cell = grid.GetCell(pos);
                            if (cell?.Type != MapCellType.Floor || cell.GetTilesetId(2) != trim || cell.GetTilesetId(1) == conveyor)
                                continue;
                            byte tile = cell.GetTileIndex(2);
                            if (!fullTrim)
                            {
                                int side = Array.IndexOf(_straightSpineTrimSlots, tile % CampaignJoinShapeCount);
                                if (side < 0) continue;
                                tile = (byte)(tile / CampaignJoinShapeCount * _straightSpineTrimSlots.Length + side);
                            }
                            group.Add((pos, tile));
                        }
                    }
                }
            }
        }
    }
}

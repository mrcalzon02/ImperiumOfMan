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
        private const short NativePresetTrimBase = 28800;
        private const int NativePresetTrimTiles = CampaignJoinShapeCount * 4;
        private const int MinimumNativeFloorRegion = 10;
        private const int NativeSpineFloorOwner = int.MinValue;
        private const short NativeSpineConveyorBase = 28821;
        private const short CampaignSpineConveyorBase = 28840;
        private const int NativeSpineConveyorColumns = 12;
        private static SurfaceTileset[] _nativePresetTrims;
        private static CampaignSurfaceTileset[] _nativeSpineConveyors;

        private enum NativePresetArea
        {
            General, Corridor, LiftLobby, Cargo, Workshop, Medical, Laboratory, Ceremonial, Relay, Objective
        }

        private static NativePresetArea ClassifyNativePresetArea(GeneratedRoomMetadata room, List<MapObstacle> obstacles)
        {
            if (room.GroupId == ExordiumDungeonGenerator.CorridorsGroupId) return NativePresetArea.Corridor;
            if (room.GroupId >= 300) return NativePresetArea.Objective;
            if (room.GroupId >= 200) return NativePresetArea.Cargo;
            if (room.GroupId >= 100) return NativePresetArea.LiftLobby;
            return ClassifyNativePresetContents(obstacles.Where(obstacle => room.ContainsPos(obstacle.Position)));
        }

        private static NativePresetArea ClassifyNativePresetContents(IEnumerable<MapObstacle> obstacles)
        {
            var scores = new int[(int)NativePresetArea.Objective + 1];
            foreach (var obstacle in obstacles)
            {
                if (obstacle.Elevator != null) return NativePresetArea.LiftLobby;
                if (obstacle.GetComponent<Shuttle>() != null) return NativePresetArea.Cargo;
                var (area, weight) = NativeFixtureArea(obstacle);
                scores[(int)area] += weight;
            }

            // A stack of crates can furnish a workshop or chapel without turning it into a storeroom.
            int best = 1;
            var result = NativePresetArea.General;
            foreach (var area in new[] { NativePresetArea.Medical, NativePresetArea.Laboratory,
                NativePresetArea.Ceremonial, NativePresetArea.Relay, NativePresetArea.Workshop, NativePresetArea.Cargo })
            {
                int score = area == NativePresetArea.Cargo ? Math.Min(scores[(int)area], 3) : scores[(int)area];
                if (score <= best) continue;
                result = area;
                best = score;
            }
            return result;
        }

        private static (NativePresetArea Area, int Weight) NativeFixtureArea(MapObstacle obstacle)
        {
            string name = obstacle.name?.ToLowerInvariant() ?? string.Empty;
            if (name.StartsWith("medical", StringComparison.Ordinal))
                return (NativePresetArea.Medical, 5);
            if (name.Contains("lab_table") || name.Contains("cloningtank") || name.StartsWith("sciencebigcontainer", StringComparison.Ordinal))
                return (NativePresetArea.Laboratory, 5);
            if (name.StartsWith("church_", StringComparison.Ordinal) || name.StartsWith("templestatue", StringComparison.Ordinal) ||
                name.Contains("altar") || name.StartsWith("idol_", StringComparison.Ordinal))
                return (NativePresetArea.Ceremonial, 4);
            if (name.StartsWith("server", StringComparison.Ordinal) || name.Contains("telegraph"))
                return (NativePresetArea.Relay, 4);
            if (name.StartsWith("indusfurnace", StringComparison.Ordinal) || name.StartsWith("induspress", StringComparison.Ordinal) ||
                name.Contains("fabricator") || name.Contains("workbench") || name.Contains("conveyor"))
                return (NativePresetArea.Workshop, 4);
            if (name.Contains("bigcontainer") || name.Contains("containerwide") || name.StartsWith("container_", StringComparison.Ordinal))
                return (NativePresetArea.Cargo, 1);
            if (obstacle.ObjName is "iom_machine_power_relay" or "iom_warp_machine_power_relay")
                return (NativePresetArea.Relay, 2);
            if (obstacle.ObjName?.StartsWith("iom_machine_", StringComparison.Ordinal) == true ||
                obstacle.ObjName?.StartsWith("iom_warp_machine_", StringComparison.Ordinal) == true)
                return (NativePresetArea.Workshop, 2);
            return (NativePresetArea.General, 0);
        }

        private static bool IsNativePresetTrimId(short id) => IsNativeSpineLitterId(id) ||
            (_nativePresetTrims != null && id >= NativePresetTrimBase && id < NativePresetTrimBase + _nativePresetTrims.Length);

        private static void RegisterNativePresetTrim(DescriptorsCollection descriptors, List<BaseTileset> all,
            Dictionary<short, BaseTileset> ids)
        {
            var decks = ImperialCampaignDecks.Concat(ChaosCampaignDecks).ToArray();
            var profiles = CreatePresetTrimProfiles();
            var source = decks[0].Floor.Spritesheet;
            var sourcePixels = ReadNativeTexturePixels(source);
            int pixelsPerTile = ImperialTilePixels * ImperialTilePixels;
            int columns = CampaignJoinShapeCount * ImperialTilePixels;
            int rows = decks.Length * 4 * ImperialTilePixels;
            var pixels = new Color32[columns * rows];

            // The trim material is already registered in the campaign kit as four matching phases.
            for (int deck = 0; deck < decks.Length; deck++)
            for (int phase = 0; phase < 4; phase++)
            for (int shape = 0; shape < CampaignJoinShapeCount; shape++)
            {
                int targetX = shape * ImperialTilePixels;
                int targetY = rows - (deck * 4 + phase + 1) * ImperialTilePixels;
                int sourceY = source.height - (deck + 1) * ImperialTilePixels;
                int sourceX = (9 + phase) * ImperialTilePixels;
                int profile = shape * pixelsPerTile;
                for (int y = 0; y < ImperialTilePixels; y++)
                for (int x = 0; x < ImperialTilePixels; x++)
                {
                    var color = sourcePixels[(sourceY + y) * source.width + sourceX + x];
                    color.a = profiles[profile + y * ImperialTilePixels + x];
                    pixels[(targetY + y) * columns + targetX + x] = color;
                }
            }

            var atlas = new Texture2D(columns, rows, TextureFormat.RGBA32, false)
            {
                name = "ImperialPresetFloorTrim",
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
            };
            atlas.SetPixels32(pixels);
            atlas.Apply(false, true);

            _nativePresetTrims = new SurfaceTileset[decks.Length];
            for (int index = 0; index < decks.Length; index++)
            {
                var trim = ScriptableObject.CreateInstance<SurfaceTileset>();
                trim.name = trim.Id = "iom_preset_floor_trim_" + decks[index].Key;
                trim.SetShortId((short)(NativePresetTrimBase + index));
                if (ids.ContainsKey(trim.ShortId) || descriptors.TryGetDescriptor(trim.Id, out _))
                    throw new InvalidOperationException("Imperial preset floor trim ID already registered: " + trim.Id);

                trim.Spritesheet = atlas;
                trim.Tiles = new TileBase[NativePresetTrimTiles];
                AccessTools.Field(typeof(BaseTileset), "_decalPatterns").SetValue(trim, new List<AutoDecalMatchPattern>());
                AccessTools.Field(typeof(BaseTileset), "_notUsableInPreset").SetValue(trim, true);
                for (int tileIndex = 0; tileIndex < trim.Tiles.Length; tileIndex++)
                {
                    var tile = ScriptableObject.CreateInstance<Tile>();
                    tile.sprite = SliceDamageSprite(atlas, index * NativePresetTrimTiles + tileIndex,
                        CampaignJoinShapeCount, new Vector2(0.5f, 0.5f));
                    tile.colliderType = Tile.ColliderType.None;
                    trim.Tiles[tileIndex] = tile;
                }
                descriptors.AddDescriptor(trim.Id, trim);
                all.Add(trim);
                ids.Add(trim.ShortId, trim);
                _nativePresetTrims[index] = trim;
            }
            RegisterNativeSpineConveyors(descriptors, all, ids);
            RegisterNativeSpineLitter(descriptors, all, ids, pixels, columns, rows);
        }

        private static void RegisterNativeSpineConveyors(DescriptorsCollection descriptors, List<BaseTileset> all,
            Dictionary<short, BaseTileset> ids)
        {
            var decks = ImperialCampaignDecks.Concat(ChaosCampaignDecks).ToArray();
            var sources = decks.Select(NativeSpineServiceJoin).Distinct().ToDictionary(join => join, join =>
            {
                var atlas = _campaignFloorJoins[0, (int)join].Spritesheet;
                return (Atlas: atlas, Pixels: ReadNativeTexturePixels(atlas));
            });
            int size = ImperialTilePixels;
            int columns = NativeSpineConveyorColumns * size;
            _nativeSpineConveyors = new CampaignSurfaceTileset[decks.Length];
            for (int deck = 0; deck < decks.Length; deck++)
            {
                var kit = decks[deck];
                int deckIndex = kit.Floor.ShortId - CampaignTileIdBase;
                bool warp = Array.IndexOf(ChaosCampaignDecks, kit) >= 0;
                var (source, sourcePixels) = sources[NativeSpineServiceJoin(kit)];
                var pixels = new Color32[columns * size * 4];
                for (int phase = 0; phase < 4; phase++)
                for (int side = 0; side < 4; side++)
                for (int segment = 0; segment < 3; segment++)
                {
                    int sourceY = source.height - (deckIndex * 4 + phase + 1) * size;
                    int sourceX = FloorJoinSlots[1 << side] * size;
                    int targetY = (3 - phase) * size;
                    int targetX = (side * 3 + segment) * size;
                    for (int y = 0; y < size; y++)
                    for (int x = 0; x < size; x++)
                    {
                        int across = side switch { 0 => y, 1 => x, 2 => size - 1 - y, _ => size - 1 - x };
                        int along = side % 2 == 0 ? x : y;
                        var substrate = sourcePixels[(sourceY + y) * source.width + sourceX + x];
                        pixels[(targetY + y) * columns + targetX + x] = SpineConveyorPixel(substrate,
                            warp, along, across, segment, size);
                    }
                }

                var atlas = new Texture2D(columns, size * 4, TextureFormat.RGBA32, false)
                {
                    name = "Imperial corridor conveyor " + kit.Key,
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Clamp,
                };
                atlas.SetPixels32(pixels);
                atlas.Apply(false, true);
                var tileset = ScriptableObject.CreateInstance<CampaignSurfaceTileset>();
                tileset.name = tileset.Id = "iom_corridor_conveyor_" + kit.Key;
                int original = OriginalSpinePaletteIndex(kit);
                tileset.SetShortId((short)(original >= 0 ? NativeSpineConveyorBase + original : CampaignSpineConveyorBase + deckIndex));
                tileset.Spritesheet = atlas;
                tileset.Tiles = new TileBase[NativeSpineConveyorColumns * 4];
                AccessTools.Field(typeof(BaseTileset), "_decalPatterns").SetValue(tileset, new List<AutoDecalMatchPattern>());
                AccessTools.Field(typeof(BaseTileset), "_notUsableInPreset").SetValue(tileset, true);
                for (int i = 0; i < tileset.Tiles.Length; i++)
                {
                    var tile = ScriptableObject.CreateInstance<Tile>();
                    tile.sprite = SliceDamageSprite(atlas, i, NativeSpineConveyorColumns, new Vector2(0.5f, 0.5f));
                    tile.colliderType = Tile.ColliderType.None;
                    tileset.Tiles[i] = tile;
                }
                descriptors.AddDescriptor(tileset.Id, tileset);
                all.Add(tileset);
                ids.Add(tileset.ShortId, tileset);
                ImperialDamageTileIds.Add(tileset.ShortId);
                var intact = Data.TileTransformation.GetRecord(kit.Floor.Id);
                Data.TileTransformation.AddRecord(tileset.Id, new TileTransformationRecord
                    { Health = intact.Health, Armor = intact.Armor, TilesetToMutateName = intact.TilesetToMutateName });
                _nativeSpineConveyors[deckIndex] = tileset;
            }
        }

        private static int OriginalSpinePaletteIndex(CampaignDeck kit) => kit.Key switch
        {
            "goethe_deadlock" => 0,
            "no_salvage_rights" => 1,
            _ => -1,
        };

        private static DeckFloorZone NativeSpineServiceZone(CampaignDeck kit) => kit.Key is "cinder_basin" or
            "last_bonded_shipment" or "crown_without_sun"
            ? DeckFloorZone.Field : DeckFloorZone.Secondary;

        private static DeckFloorJoin NativeSpineServiceJoin(CampaignDeck kit) =>
            CampaignFloorJoin(NativeSpineServiceZone(kit), DeckFloorZone.Lane).Value;

        private static Color32 SpineConveyorPixel(Color32 substrate, bool warp, int along, int across, int segment, int size)
        {
            if (across < 7 || across > 17 || (segment == 0 && along < 2) || (segment == 2 && along > size - 3))
                return substrate;
            bool cap = (segment == 0 && along < 5) || (segment == 2 && along > size - 6);
            if (across is 7 or 17) return new Color32(25, 27, 24, 255);
            if (across is 8 or 16 || cap)
                return warp ? new Color32(99, 60, 45, 255) : new Color32(114, 101, 70, 255);
            int roller = along % 7;
            int shade = roller switch { 0 or 5 or 6 => 33, 1 => 95, 2 => 83, 3 => 69, _ => 54 };
            if (across is 9 or 15) shade -= 12;
            return new Color32((byte)(shade + (warp ? 13 : 1)), (byte)(shade + (warp ? -1 : 2)),
                (byte)(shade - 4), 255);
        }

        private static byte[] CreatePresetTrimProfiles()
        {
            int size = ImperialTilePixels;
            var pixels = new byte[CampaignJoinShapeCount * size * size];
            var distances = new int[4];
            for (int mask = 1; mask < FloorJoinSlots.Length; mask++)
            {
                int shape = FloorJoinSlots[mask];
                if (shape < 0) continue;
                for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    distances[0] = size - 1 - y;
                    distances[1] = size - 1 - x;
                    distances[2] = y;
                    distances[3] = x;
                    float depth = size;
                    for (int side = 0; side < 4; side++)
                    {
                        int sideDepth = distances[side];
                        if ((mask & (1 << side)) != 0) depth = Math.Min(depth, sideDepth);
                        if ((mask & (16 << side)) != 0)
                        {
                            int next = distances[(side + 1) & 3];
                            // At a cell edge this quarter-circle meets the neighboring straight trim.
                            depth = Math.Min(depth, Mathf.Sqrt(sideDepth * sideDepth + next * next));
                        }
                    }
                    pixels[shape * size * size + y * size + x] = (byte)Mathf.RoundToInt(
                        255f * Mathf.Clamp01((6.75f - depth) / 1.5f));
                }
            }
            return pixels;
        }

        private static void PaintNativeImperialPresetFloors(State state, CampaignDeck campaign,
            ISet<CellPosition> spineFloor = null)
        {
            var grid = state.Get<MapGrid>();
            if (grid == null || _nativePresetTrims == null) return;

            int width = grid.MaxWidth, height = grid.MaxHeight;
            var source = new short[width, height];
            var eligible = new bool[width, height];
            var owners = new int[width, height];
            var zones = new DeckFloorZone[width, height];
            var floorKits = new CampaignDeck[width, height];
            var donorZones = new Dictionary<short, DeckFloorZone>();
            var rooms = state.Get<LocationMetadata>().GeneratedRooms;
            var obstacles = state.Get<MapObstacles>().Obstacles;
            var spineDoors = NativeSpineDoorLandings(spineFloor, obstacles).ToArray();
            var spineThresholds = new HashSet<CellPosition>();
            foreach (var (near, far, step) in spineDoors)
            {
                for (var pos = near + step; !pos.Equals(far); pos += step)
                    spineThresholds.Add(pos);
            }
            var roomFloors = new Dictionary<int, List<CellPosition>>();
            int metal = 0, ground = 0;
            for (int y = 1; y < height - 1; y++)
            for (int x = 1; x < width - 1; x++)
            {
                var cell = grid.GetCell(x, y);
                short id = cell.GetTilesetId(1);
                var pos = new CellPosition(x, y);
                bool inSpine = spineFloor?.Contains(pos) == true;
                bool isSpineDoor = spineThresholds.Contains(pos) && (cell.IsDoor || cell.specialFlag == MapCellSpecialFlag.OpenedDoor);
                if (cell.Type != MapCellType.Floor || (!isSpineDoor && cell.specialFlag != MapCellSpecialFlag.None) || cell.LiquidLevel != 0 ||
                    (!inSpine && ImperialDamageTileIds.Contains(id)) ||
                    Data.Tilesets.GetByShortId(id) is not SurfaceTileset { SurfaceType: MapCellType.Floor } donor ||
                    donor.MaterialType is SurfaceMaterialType.Liquid or SurfaceMaterialType.Virtual || !IsIntactTerrain(donor.name))
                {
                    continue;
                }
                if (donor.MaterialType == SurfaceMaterialType.Metal) metal++;
                else ground++;
                source[x, y] = id;
                eligible[x, y] = true;
                if (!donorZones.ContainsKey(id))
                    donorZones.Add(id, IsNativeAisleMaterial(donor.name) ? DeckFloorZone.Lane : DeckFloorZone.Field);

                int owner = inSpine ? NativeSpineFloorOwner : FindNativePresetRoom(rooms, pos);
                owners[x, y] = owner;
                if (!roomFloors.TryGetValue(owner, out var cells))
                {
                    cells = new List<CellPosition>();
                    roomFloors.Add(owner, cells);
                }
                cells.Add(pos);
            }
            if (ground + metal == 0) return;

            bool warp = GetImperialRaidPalette(state)?.Warp == true;
            var palette = warp ? ChaosCampaignDecks : ImperialCampaignDecks;
            var routeKit = Array.IndexOf(palette, campaign) >= 0 ? campaign : null;
            var defaultKit = routeKit ?? ChooseNativeFloorKit(warp, metal >= ground);
            var passageKit = NativePresetAreaKit(NativePresetArea.Corridor, warp, defaultKit, routeKit);
            var entrances = CollectNativeAisleEntrances(state, obstacles);
            var doorCells = new HashSet<CellPosition>(obstacles.Where(obstacle => obstacle.Door != null)
                .SelectMany(obstacle => obstacle.OccupiedCells));
            if (rooms.Count == 0)
                SeparateStaticPresetRooms(grid, eligible, owners, roomFloors);
            ApplyNativeFloorRegions(rooms, roomFloors, source, eligible, owners, zones, donorZones);
            foreach (var room in roomFloors)
            {
                var area = NativePresetArea.General;
                if (room.Key == NativeSpineFloorOwner)
                {
                    area = NativePresetArea.Corridor;
                }
                else if (room.Key >= 0 && room.Key < rooms.Count)
                {
                    area = ClassifyNativePresetArea(rooms[room.Key], obstacles);
                }
                else if (rooms.Count == 0)
                {
                    area = ClassifyNativePresetContents(obstacles.Where(obstacle =>
                    {
                        var pos = obstacle.Position;
                        return grid.GetCell(pos) != null && eligible[pos.X, pos.Y] && owners[pos.X, pos.Y] == room.Key;
                    }));
                }
                var kit = NativePresetAreaKit(area, warp, defaultKit, routeKit);
                foreach (var pos in room.Value) floorKits[pos.X, pos.Y] = kit;

                if (room.Key < 0 || room.Value.Count < 30 || area == NativePresetArea.Corridor) continue;
                if (!room.Value.Any(pos => zones[pos.X, pos.Y] == DeckFloorZone.Lane))
                    PaintNativeThroughAisle(grid, room.Value, room.Key, owners, eligible, zones, entrances);
                if (!room.Value.Any(pos => zones[pos.X, pos.Y] == DeckFloorZone.Secondary))
                    PaintNativeMachineBay(grid, room.Value, room.Key, owners, eligible, zones, obstacles);
                if (!room.Value.Any(pos => zones[pos.X, pos.Y] == DeckFloorZone.Secondary))
                    PaintNativeWallBay(grid, room.Value, room.Key, owners, eligible, zones, entrances);
            }
            AssignNativeThresholdKits(doorCells, floorKits, zones, passageKit);
            if (spineFloor?.Count > 0)
                PaintNativeSpineAisles(rooms, spineFloor, spineDoors, floorKits, zones, passageKit);

            for (int y = 1; y < height - 1; y++)
            for (int x = 1; x < width - 1; x++)
            {
                if (!eligible[x, y]) continue;
                var cell = grid.GetCell(x, y);
                var deck = floorKits[x, y];
                PaintNativePresetFloorTile(cell, deck, floorKits, zones, x, y);
                int boundary = GetNativePresetTrimMask(grid, x, y);
                if (boundary == 0) continue;
                int phase = (x & 1) + (y & 1) * 2;
                var trim = _nativePresetTrims[deck.Floor.ShortId - CampaignTileIdBase];
                cell.SetTilesetId(2, trim.ShortId);
                cell.SetTileIndex(2, (byte)(phase * CampaignJoinShapeCount + FloorJoinSlots[boundary]));
            }
            if (spineFloor?.Count > 0)
            {
                PaintNativeSpineConveyor(grid, spineFloor, obstacles, passageKit);
                PaintNativeSpineLitter(state, grid, rooms, spineFloor, obstacles, zones, passageKit);
            }
        }

        private static void PaintNativeSpineConveyor(MapGrid grid, ISet<CellPosition> spineFloor,
            List<MapObstacle> obstacles, CampaignDeck passage)
        {
            var cargo = new HashSet<string>(SpineFurniture.Where(prop => prop.Use is "cargo" or "crates").Select(prop => prop.Id));
            int deckIndex = passage.Floor.ShortId - CampaignTileIdBase;
            var join = _campaignFloorJoins[deckIndex, (int)NativeSpineServiceJoin(passage)];
            foreach (var obstacle in obstacles)
            {
                var anchor = obstacle.Position;
                if (!spineFloor.Contains(anchor) || !cargo.Contains(obstacle.ObjName)) continue;
                for (int side = 0; side < FloorNeighbors.Length; side++)
                {
                    var (dx, dy) = FloorNeighbors[side];
                    var inward = new CellPosition(dx, dy);
                    var along = new CellPosition(Math.Abs(dy), Math.Abs(dx));
                    var offsets = new[] { -1, 0, -2 };
                    foreach (int offset in offsets)
                    {
                        var start = anchor + along * offset;
                        var belt = new[] { start, start + along, start + along * 2 };
                        if (belt.All(obstacle.OccupiedCells.Contains) || !belt.All(IsUnbrokenServiceStrip)) continue;
                        for (int segment = 0; segment < belt.Length; segment++)
                        {
                            var pos = belt[segment];
                            int phase = (pos.X & 1) + (pos.Y & 1) * 2;
                            var cell = grid.GetCell(pos);
                            cell.SetTilesetId(1, _nativeSpineConveyors[deckIndex].ShortId);
                            cell.SetTileIndex(1, (byte)(phase * NativeSpineConveyorColumns + side * 3 + segment));
                        }
                        return;
                    }

                    bool IsUnbrokenServiceStrip(CellPosition pos)
                    {
                        if (!spineFloor.Contains(pos) || grid.GetCell(pos - inward)?.IsWallSurfaceType != true)
                            return false;
                        var cell = grid.GetCell(pos);
                        return cell?.Type == MapCellType.Floor && cell.GetTilesetId(1) == join.ShortId &&
                            cell.GetTileIndex(1) % CampaignJoinShapeCount == FloorJoinSlots[1 << side];
                    }
                }
            }
        }

        private static CampaignDeck ChooseNativeFloorKit(bool warp, bool metal) => warp
            ? ChaosCampaignDecks[metal ? 7 : 8]
            : ImperialCampaignDecks[metal ? 0 : 9];

        private static CampaignDeck NativePresetAreaKit(NativePresetArea area, bool warp, CampaignDeck fallback,
            CampaignDeck route)
        {
            if (route != null && (area is NativePresetArea.Corridor or NativePresetArea.LiftLobby or NativePresetArea.Objective ||
                area == RouteSignatureArea(route.Key)))
                return route;

            string key = (area, warp) switch
            {
                (NativePresetArea.Corridor or NativePresetArea.LiftLobby, false) => "goethe_deadlock",
                (NativePresetArea.Corridor or NativePresetArea.LiftLobby, true) => "no_salvage_rights",
                (NativePresetArea.Cargo, false) => "last_bonded_shipment",
                (NativePresetArea.Cargo, true) => "the_right_to_pass",
                (NativePresetArea.Workshop, false) => "prime_the_foundry",
                (NativePresetArea.Workshop, true) => "furnace_of_ur",
                (NativePresetArea.Medical, false) => "discovery_injunction",
                (NativePresetArea.Medical, true) => "prism_rent",
                (NativePresetArea.Laboratory, false) => "counterfeit_orbit",
                (NativePresetArea.Laboratory, true) => "white_frequency",
                (NativePresetArea.Ceremonial, false) => "one_standard",
                (NativePresetArea.Ceremonial, true) => "starless_oath",
                (NativePresetArea.Relay, false) => "hollow_signal",
                (NativePresetArea.Relay, true) => "white_frequency",
                _ => null
            };
            return key == null ? fallback : Array.Find(warp ? ChaosCampaignDecks : ImperialCampaignDecks, kit => kit.Key == key) ?? fallback;
        }

        private static NativePresetArea RouteSignatureArea(string key) => key switch
        {
            "caloris_levy" or "extinguish_crucible" or "cinder_basin" or "prime_the_foundry" or "furnace_of_ur" =>
                NativePresetArea.Workshop,
            "counterfeit_orbit" or "discovery_injunction" or "prism_rent" => NativePresetArea.Laboratory,
            "goethe_deadlock" or "hollow_signal" or "white_frequency" => NativePresetArea.Relay,
            "last_bonded_shipment" or "no_salvage_rights" or "the_right_to_pass" or "hound_road" or
                "crown_without_sun_alternate_route" => NativePresetArea.Cargo,
            "one_standard" or "scales_of_passage" or "starless_oath" or "unmake_the_seal" or "crown_without_sun" =>
                NativePresetArea.Ceremonial,
            _ => NativePresetArea.Objective,
        };

        private static void AssignNativeThresholdKits(HashSet<CellPosition> doors, CampaignDeck[,] kits,
            DeckFloorZone[,] zones, CampaignDeck passage)
        {
            int width = kits.GetLength(0), height = kits.GetLength(1);
            var approaches = new List<CellPosition>(4);
            foreach (var door in doors)
            {
                approaches.Clear();
                foreach (var (dx, dy) in FloorNeighbors)
                {
                    int x = door.X + dx, y = door.Y + dy;
                    if (x >= 0 && x < width && y >= 0 && y < height && kits[x, y] != null)
                        approaches.Add(new CellPosition(x, y));
                }
                if (approaches.Count < 2 || approaches.All(pos => kits[pos.X, pos.Y] == kits[approaches[0].X, approaches[0].Y]))
                    continue;
                foreach (var pos in approaches)
                {
                    kits[pos.X, pos.Y] = passage;
                    zones[pos.X, pos.Y] = DeckFloorZone.Lane;
                }
            }
        }

        private static IEnumerable<(CellPosition Near, CellPosition Far, CellPosition Step)> NativeSpineDoorLandings(
            ISet<CellPosition> spineFloor, List<MapObstacle> obstacles)
        {
            if (spineFloor?.Count is not > 0) yield break;
            foreach (var obstacle in obstacles)
            {
                if (obstacle.Door == null) continue;
                bool crossesX = obstacle.Door.DoorOrientation == Door.Orientation.Vertical;
                var step = crossesX ? new CellPosition(1, 0) : new CellPosition(0, 1);
                var near = obstacle.Position - step;
                // The horizontal door's northern joint is a second threshold tile.
                var far = obstacle.Position + step * (crossesX ? 1 : 2);
                if (spineFloor.Contains(near) != spineFloor.Contains(far)) yield return (near, far, step);
            }
        }

        private static void PaintNativeSpineAisles(List<GeneratedRoomMetadata> rooms, ISet<CellPosition> spineFloor,
            (CellPosition Near, CellPosition Far, CellPosition Step)[] doors, CampaignDeck[,] kits,
            DeckFloorZone[,] zones, CampaignDeck passage)
        {
            var service = NativeSpineServiceZone(passage);
            foreach (var pos in spineFloor)
                if (HasMaterial(pos)) zones[pos.X, pos.Y] = service;

            var runs = NativeSpineRuns(rooms);
            foreach (var run in runs.Take(2))
                PaintRunCenter(run, null);
            for (int index = 2; index + 1 < runs.Length; index += 2)
            {
                var (horizontal, vertical, bend) = NativeSpineBend(runs[index], runs[index + 1]);
                PaintRunCenter(horizontal, bend);
                PaintRunCenter(vertical, bend);

                // The inner quadrant gives the center stripe a full turning square.
                int x = OuterSpineEnd(horizontal.LeftLowerCorner.X, horizontal.RightUpperCorner.X, bend.X);
                int y = OuterSpineEnd(vertical.LeftLowerCorner.Y, vertical.RightUpperCorner.Y, bend.Y);
                MarkSpineLane(bend + new CellPosition(Math.Sign(x - bend.X), Math.Sign(y - bend.Y)));
            }

            foreach (var (near, far, step) in doors)
            {
                var hallLanding = spineFloor.Contains(near) ? near : far;
                if (!HasMaterial(hallLanding)) continue;
                for (var pos = near; !pos.Equals(far + step); pos += step)
                {
                    if (!HasMaterial(pos)) continue;
                    kits[pos.X, pos.Y] = passage;
                    zones[pos.X, pos.Y] = DeckFloorZone.Lane;
                }
            }

            bool HasMaterial(CellPosition pos) => pos.X >= 0 && pos.X < kits.GetLength(0) &&
                pos.Y >= 0 && pos.Y < kits.GetLength(1) && kits[pos.X, pos.Y] != null;

            void MarkSpineLane(CellPosition pos)
            {
                if (spineFloor.Contains(pos) && HasMaterial(pos)) zones[pos.X, pos.Y] = DeckFloorZone.Lane;
            }

            void PaintRunCenter(GeneratedRoomMetadata run, CellPosition? turn)
            {
                bool horizontal = IsHorizontalSpineRun(run);
                int low = horizontal ? run.LeftLowerCorner.X : run.LeftLowerCorner.Y;
                int high = horizontal ? run.RightUpperCorner.X : run.RightUpperCorner.Y;
                int lateral = horizontal ? (run.LeftLowerCorner.Y + run.RightUpperCorner.Y) / 2 :
                    (run.LeftLowerCorner.X + run.RightUpperCorner.X) / 2;
                if (turn is CellPosition bend)
                {
                    int joint = horizontal ? bend.X : bend.Y;
                    int outer = OuterSpineEnd(low, high, joint);
                    low = Math.Min(joint, outer);
                    high = Math.Max(joint, outer);
                }
                for (int along = low; along <= high; along++)
                    MarkSpineLane(horizontal ? new CellPosition(along, lateral) : new CellPosition(lateral, along));
            }
        }

        private static GeneratedRoomMetadata[] NativeSpineRuns(List<GeneratedRoomMetadata> rooms) =>
            rooms.Where(room => room.GroupId == ExordiumDungeonGenerator.CorridorsGroupId &&
                room.PresetBinaryId is <= ImperialSpinePresetId and >= ImperialSpinePresetId - 9)
                .OrderByDescending(room => room.PresetBinaryId).ToArray();

        private static (GeneratedRoomMetadata Horizontal, GeneratedRoomMetadata Vertical, CellPosition Bend) NativeSpineBend(
            GeneratedRoomMetadata first, GeneratedRoomMetadata second)
        {
            bool firstHorizontal = IsHorizontalSpineRun(first);
            var horizontal = firstHorizontal ? first : second;
            var vertical = firstHorizontal ? second : first;
            var bend = new CellPosition((vertical.LeftLowerCorner.X + vertical.RightUpperCorner.X) / 2,
                (horizontal.LeftLowerCorner.Y + horizontal.RightUpperCorner.Y) / 2);
            return (horizontal, vertical, bend);
        }

        private static bool IsHorizontalSpineRun(GeneratedRoomMetadata room) =>
            room.RightUpperCorner.X - room.LeftLowerCorner.X > room.RightUpperCorner.Y - room.LeftLowerCorner.Y;

        private static int OuterSpineEnd(int low, int high, int bend) => bend - low > high - bend ? low : high;

        private static int FindNativePresetRoom(List<GeneratedRoomMetadata> rooms, CellPosition pos)
        {
            int corridor = -1;
            for (int index = 0; index < rooms.Count; index++)
            {
                if (!rooms[index].ContainsPos(pos)) continue;
                if (rooms[index].GroupId != ExordiumDungeonGenerator.CorridorsGroupId) return index;
                corridor = index;
            }
            return corridor;
        }

        private static void SeparateStaticPresetRooms(MapGrid grid, bool[,] eligible, int[,] owners,
            Dictionary<int, List<CellPosition>> roomFloors)
        {
            if (!roomFloors.TryGetValue(-1, out var unassigned)) return;
            roomFloors.Remove(-1);
            var pending = new Queue<CellPosition>();
            foreach (var start in unassigned)
            {
                if (owners[start.X, start.Y] != -1) continue;
                int owner = roomFloors.Count;
                var room = new List<CellPosition>();
                roomFloors.Add(owner, room);
                owners[start.X, start.Y] = owner;
                pending.Enqueue(start);
                while (pending.Count > 0)
                {
                    var pos = pending.Dequeue();
                    room.Add(pos);
                    foreach (var (dx, dy) in FloorNeighbors)
                    {
                        var next = new CellPosition(pos.X + dx, pos.Y + dy);
                        if (!eligible[next.X, next.Y] || owners[next.X, next.Y] != -1 || grid.GetFence(pos, next)) continue;
                        owners[next.X, next.Y] = owner;
                        pending.Enqueue(next);
                    }
                }
            }
        }

        private static bool IsNativeAisleMaterial(string name) =>
            name.IndexOf("catwalk", StringComparison.OrdinalIgnoreCase) >= 0 ||
            name.IndexOf("corridor", StringComparison.OrdinalIgnoreCase) >= 0 ||
            name.IndexOf("walkway", StringComparison.OrdinalIgnoreCase) >= 0 ||
            name.IndexOf("aisle", StringComparison.OrdinalIgnoreCase) >= 0;

        private static void ApplyNativeFloorRegions(List<GeneratedRoomMetadata> rooms, Dictionary<int, List<CellPosition>> roomFloors,
            short[,] source, bool[,] eligible, int[,] owners, DeckFloorZone[,] zones, Dictionary<short, DeckFloorZone> donorZones)
        {
            var visited = new bool[eligible.GetLength(0), eligible.GetLength(1)];
            var pending = new Queue<CellPosition>();
            var component = new List<CellPosition>();
            foreach (var room in roomFloors)
            {
                var floor = room.Value;
                if (room.Key == NativeSpineFloorOwner) continue;
                if (room.Key >= 0 && room.Key < rooms.Count &&
                    rooms[room.Key].GroupId == ExordiumDungeonGenerator.CorridorsGroupId)
                {
                    foreach (var pos in floor) zones[pos.X, pos.Y] = DeckFloorZone.Lane;
                    continue;
                }

                var primary = floor.GroupBy(pos => source[pos.X, pos.Y]).OrderByDescending(group => group.Count()).First().Key;
                List<CellPosition> largestBay = null;
                foreach (var start in floor)
                {
                    short id = source[start.X, start.Y];
                    if (visited[start.X, start.Y] || (id == primary && donorZones[id] != DeckFloorZone.Lane)) continue;
                    component.Clear();
                    pending.Enqueue(start);
                    visited[start.X, start.Y] = true;
                    while (pending.Count > 0)
                    {
                        var tile = pending.Dequeue();
                        component.Add(tile);
                        foreach (var (dx, dy) in FloorNeighbors)
                        {
                            int x = tile.X + dx, y = tile.Y + dy;
                            if (!eligible[x, y] || visited[x, y] || owners[x, y] != room.Key || source[x, y] != id) continue;
                            visited[x, y] = true;
                            pending.Enqueue(new CellPosition(x, y));
                        }
                    }
                    if (component.Count < MinimumNativeFloorRegion) continue;
                    if (donorZones[id] == DeckFloorZone.Lane)
                    {
                        foreach (var tile in component) zones[tile.X, tile.Y] = DeckFloorZone.Lane;
                        continue;
                    }
                    var core = NativeMaterialCore(component);
                    if (core.Count >= MinimumNativeFloorRegion && (largestBay == null || core.Count > largestBay.Count))
                        largestBay = core;
                }
                if (largestBay != null)
                    foreach (var tile in largestBay) zones[tile.X, tile.Y] = DeckFloorZone.Secondary;
            }
        }

        private static List<CellPosition> NativeMaterialCore(List<CellPosition> component)
        {
            var cells = new HashSet<CellPosition>(component);
            var supported = new HashSet<CellPosition>();
            foreach (var cell in component)
            {
                var right = new CellPosition(cell.X + 1, cell.Y);
                var up = new CellPosition(cell.X, cell.Y + 1);
                var corner = new CellPosition(cell.X + 1, cell.Y + 1);
                if (!cells.Contains(right) || !cells.Contains(up) || !cells.Contains(corner)) continue;
                supported.Add(cell);
                supported.Add(right);
                supported.Add(up);
                supported.Add(corner);
            }
            var best = new List<CellPosition>();
            var pending = new Queue<CellPosition>();
            foreach (var start in component)
            {
                if (!supported.Remove(start)) continue;
                var connected = new List<CellPosition>();
                pending.Enqueue(start);
                while (pending.Count > 0)
                {
                    var cell = pending.Dequeue();
                    connected.Add(cell);
                    foreach (var (dx, dy) in FloorNeighbors)
                    {
                        var next = new CellPosition(cell.X + dx, cell.Y + dy);
                        if (supported.Remove(next)) pending.Enqueue(next);
                    }
                }
                if (connected.Count > best.Count) best = connected;
            }
            return best;
        }

        private static HashSet<CellPosition> CollectNativeAisleEntrances(State state, List<MapObstacle> obstacles)
        {
            var entrances = new HashSet<CellPosition>();
            var metadata = state.Get<MapMetadata>();
            foreach (var entry in metadata.LevelEntries) entrances.Add(entry.Position);
            foreach (var exit in metadata.LevelExits) entrances.Add(exit.Position);
            foreach (var obstacle in obstacles)
            {
                if (obstacle.Door != null)
                    entrances.UnionWith(obstacle.OccupiedCells);
                if (obstacle.Elevator != null)
                    entrances.Add(obstacle.Elevator.GetSpawnPosition());
                if (obstacle.GetComponent<Shuttle>() is Shuttle shuttle)
                    entrances.Add(shuttle.GetSpawnPosition());
            }
            return entrances;
        }

        private static void PaintNativeThroughAisle(MapGrid grid, List<CellPosition> floor, int owner, int[,] owners,
            bool[,] eligible, DeckFloorZone[,] zones, HashSet<CellPosition> entrances)
        {
            var approaches = floor.Where(pos => !grid.GetCell(pos).IsObjBlockPass &&
                FloorNeighbors.Any(step => entrances.Contains(new CellPosition(pos.X + step.X, pos.Y + step.Y)))).ToArray();
            if (approaches.Length < 2) return;

            // Prefer an axis-aligned through run with little deviation from the native doorway axis.
            foreach (var pair in from a in approaches
                                from b in approaches
                                let dx = Math.Abs(a.X - b.X)
                                let dy = Math.Abs(a.Y - b.Y)
                                where dx + dy >= 6
                                orderby Math.Min(dx, dy) <= 2 descending, dx + dy descending
                                select (Start: a, End: b, Distance: dx + dy))
            {
                var path = FindNativeAisle(grid, pair.Start, pair.End, owner, owners, eligible, zones);
                if (path == null || path.Count > pair.Distance + 4 || path.Count > floor.Count / 3) continue;
                foreach (var pos in path) zones[pos.X, pos.Y] = DeckFloorZone.Lane;
                return;
            }
        }

        private static List<CellPosition> FindNativeAisle(MapGrid grid, CellPosition start, CellPosition end,
            int owner, int[,] owners, bool[,] eligible, DeckFloorZone[,] zones)
        {
            var pending = new Queue<CellPosition>();
            var previous = new Dictionary<CellPosition, CellPosition> { [start] = start };
            pending.Enqueue(start);
            while (pending.Count > 0)
            {
                var pos = pending.Dequeue();
                if (pos.Equals(end))
                {
                    var path = new List<CellPosition>();
                    for (var step = end; !step.Equals(start); step = previous[step]) path.Add(step);
                    path.Add(start);
                    return path;
                }

                foreach (var (dx, dy) in FloorNeighbors)
                {
                    var next = new CellPosition(pos.X + dx, pos.Y + dy);
                    if (!eligible[next.X, next.Y] || owners[next.X, next.Y] != owner ||
                        zones[next.X, next.Y] == DeckFloorZone.Secondary ||
                        grid.GetCell(next).IsObjBlockPass || grid.GetFence(pos, next) || previous.ContainsKey(next)) continue;
                    previous.Add(next, pos);
                    pending.Enqueue(next);
                }
            }
            return null;
        }

        private static void PaintNativeMachineBay(MapGrid grid, List<CellPosition> floor, int owner, int[,] owners,
            bool[,] eligible, DeckFloorZone[,] zones, List<MapObstacle> obstacles)
        {
            foreach (var machine in obstacles)
            {
                if (machine.OccupiedCells.Count < 4 || !(machine.ObjName?.StartsWith("iom_machine_", StringComparison.Ordinal) == true ||
                    machine.ObjName?.StartsWith("iom_warp_machine_", StringComparison.Ordinal) == true ||
                    machine.ObjectiveObstacle != null))
                {
                    continue;
                }
                var anchor = machine.Position;
                if (grid.GetCell(anchor) == null || !eligible[anchor.X, anchor.Y] || owners[anchor.X, anchor.Y] != owner) continue;

                int left = machine.OccupiedCells.Min(pos => pos.X);
                int right = machine.OccupiedCells.Max(pos => pos.X);
                int lower = machine.OccupiedCells.Min(pos => pos.Y) - 1;
                int upper = machine.OccupiedCells.Max(pos => pos.Y);
                int size = (right - left + 1) * (upper - lower + 1);
                if (left < 1 || lower < 1 || right >= grid.MaxWidth - 1 || upper >= grid.MaxHeight - 1 ||
                    size < 9 || size > floor.Count / 2) continue;
                var bay = (from y in Enumerable.Range(lower, upper - lower + 1)
                           from x in Enumerable.Range(left, right - left + 1)
                           select new CellPosition(x, y)).ToArray();
                if (bay.Any(pos => !eligible[pos.X, pos.Y] || owners[pos.X, pos.Y] != owner ||
                    zones[pos.X, pos.Y] == DeckFloorZone.Lane)) continue;
                foreach (var pos in bay) zones[pos.X, pos.Y] = DeckFloorZone.Secondary;
                return;
            }
        }

        private static void PaintNativeWallBay(MapGrid grid, List<CellPosition> floor, int owner, int[,] owners,
            bool[,] eligible, DeckFloorZone[,] zones, HashSet<CellPosition> entrances)
        {
            List<CellPosition> best = null;
            foreach (var outward in FloorNeighbors)
            {
                var along = (X: outward.Y, Y: -outward.X);
                foreach (var start in floor)
                {
                    if (!HasBaySection(start) ||
                        HasBaySection(new CellPosition(start.X - along.X, start.Y - along.Y))) continue;

                    var bay = new List<CellPosition>();
                    for (var pos = start; HasBaySection(pos); pos = new CellPosition(pos.X + along.X, pos.Y + along.Y))
                    {
                        bay.Add(pos);
                        bay.Add(new CellPosition(pos.X - outward.X, pos.Y - outward.Y));
                    }
                    if (bay.Count >= 10 && bay.Count <= floor.Count / 2 && (best == null || bay.Count > best.Count))
                        best = bay;
                }

                bool HasBaySection(CellPosition pos)
                {
                    int insideX = pos.X - outward.X;
                    int insideY = pos.Y - outward.Y;
                    if (grid.GetCell(pos.X + outward.X, pos.Y + outward.Y)?.IsWallSurfaceType != true ||
                        grid.GetCell(insideX, insideY) == null || grid.GetCell(pos) == null)
                    {
                        return false;
                    }

                    return eligible[pos.X, pos.Y] && owners[pos.X, pos.Y] == owner &&
                        eligible[insideX, insideY] && owners[insideX, insideY] == owner &&
                        zones[pos.X, pos.Y] == DeckFloorZone.Field && zones[insideX, insideY] == DeckFloorZone.Field &&
                        !entrances.Contains(pos) && !entrances.Contains(new CellPosition(insideX, insideY));
                }
            }
            if (best != null)
                foreach (var pos in best) zones[pos.X, pos.Y] = DeckFloorZone.Secondary;
        }

        private static void PaintNativePresetFloorTile(MapCell cell, CampaignDeck deck, CampaignDeck[,] kits,
            DeckFloorZone[,] zones, int x, int y)
        {
            var zone = zones[x, y];
            int phase = (x & 1) + (y & 1) * 2;
            DeckFloorZone? target = null;
            int mask = 0;
            bool multipleMaterials = false;
            for (int side = 0; side < FloorNeighbors.Length; side++)
            {
                var (dx, dy) = FloorNeighbors[side];
                if (kits[x + dx, y + dy] != deck) continue;
                CollectJoin(zones[x + dx, y + dy], 1 << side);
            }
            for (int corner = 0; corner < FloorNeighbors.Length; corner++)
            {
                var (ax, ay) = FloorNeighbors[corner];
                var (bx, by) = FloorNeighbors[(corner + 1) & 3];
                if (kits[x + ax, y + ay] != deck || kits[x + bx, y + by] != deck ||
                    kits[x + ax + bx, y + ay + by] != deck || zones[x + ax, y + ay] != zone ||
                    zones[x + bx, y + by] != zone) continue;
                CollectJoin(zones[x + ax + bx, y + ay + by], 16 << corner);
            }
            if (!multipleMaterials && target is DeckFloorZone edge && CampaignFloorJoin(zone, edge) is DeckFloorJoin family)
            {
                var tileset = _campaignFloorJoins[deck.Floor.ShortId - CampaignTileIdBase, (int)family];
                cell.SetTilesetId(1, tileset.ShortId);
                cell.SetTileIndex(1, (byte)(phase * CampaignJoinShapeCount + FloorJoinSlots[mask]));
                return;
            }

            cell.SetTilesetId(1, deck.Floor.ShortId);
            cell.SetTileIndex(1, (byte)(zone switch
            {
                DeckFloorZone.Secondary => 1 + phase,
                DeckFloorZone.Lane => 5 + phase,
                _ => 0,
            }));

            void CollectJoin(DeckFloorZone neighbor, int bit)
            {
                if (neighbor == zone) return;
                if (target != null && target != neighbor) multipleMaterials = true;
                target = neighbor;
                mask |= bit;
            }
        }

        private static int GetNativePresetTrimMask(MapGrid grid, int x, int y)
        {
            int mask = 0;
            for (int side = 0; side < FloorNeighbors.Length; side++)
            {
                var (dx, dy) = FloorNeighbors[side];
                if (grid.GetCell(x + dx, y + dy)?.IsWallSurfaceType == true)
                    mask |= 1 << side;
            }
            for (int corner = 0; corner < FloorNeighbors.Length; corner++)
            {
                int sides = (1 << corner) | (1 << ((corner + 1) & 3));
                if ((mask & sides) != 0) continue;
                var (ax, ay) = FloorNeighbors[corner];
                var (bx, by) = FloorNeighbors[(corner + 1) & 3];
                if (grid.GetCell(x + ax, y + ay)?.Type != MapCellType.Floor ||
                    grid.GetCell(x + bx, y + by)?.Type != MapCellType.Floor) continue;
                if (grid.GetCell(x + ax + bx, y + ay + by)?.IsWallSurfaceType == true)
                    mask |= 16 << corner;
            }
            return mask;
        }
    }
}

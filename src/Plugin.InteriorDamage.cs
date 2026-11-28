using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using HarmonyLib;
using MGSC;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private const int ImperialTilePixels = 28;
        private const short ImperialTileIdBase = 28500;
        private const int ImperialPaletteIdStride = 32;
        private const int ImperialTerrainSlots = 244;
        private const int ImperialDoorStateCount = 5;
        private const string ImperialTerrainPaletteKey = "iom_terrain_palette";

        private static readonly string[] ImperialFloorNames = ["basalt", "deck", "chapel", "slate"];
        private static readonly int[] ImperialFloorCounts = [6, 4, 3, 3];
        private static readonly ConditionalWeakTable<Door, Sprite[]> ImperialDamageDoors = new();
        private static ImperialDamagePalette[] ImperialDamagePalettes;
        private static HashSet<short> ImperialDamageTileIds;
        private static Sprite[][] ImperialDamageDoorArt;

        private sealed class ImperialDamagePalette
        {
            public bool Warp;
            public CornerWallTileset Wall;
            public CornerWallTileset Pillar;
            public FacadeWallTileset Facade;
            public readonly SurfaceTileset[] Floors = new SurfaceTileset[4];
        }

        private static void RegisterImperialDamageTiles()
        {
            var descriptors = (DescriptorsCollection)AccessTools.Field(typeof(Tilesets), "_descriptors").GetValue(Data.Tilesets);
            var all = (List<BaseTileset>)AccessTools.Field(typeof(Tilesets), "_all").GetValue(Data.Tilesets);
            var ids = (Dictionary<short, BaseTileset>)AccessTools.Field(typeof(Tilesets), "_shortIdsToTilesets").GetValue(Data.Tilesets);
            var staged = new List<(BaseTileset Tileset, TileTransformationRecord Transformation)>();
            var clean = BuildImperialDamagePalette(false, staged);
            var warp = BuildImperialDamagePalette(true, staged);
            var doorAtlas = ReadDamageAtlas("ImperialDamageDoors", ImperialDoorStateCount, 2);
            var tallDoorAtlas = ReadDamageAtlas("ImperialDamageTallDoors", ImperialDoorStateCount, 2);
            Sprite[] DoorRow(Texture2D sheet, int row, int height)
            {
                var states = new Sprite[ImperialDoorStateCount];
                // Native doors are anchored at their bottom edge, not the cell center.
                for (int index = 0; index < states.Length; index++)
                {
                    states[index] = SliceDamageSprite(sheet, row * ImperialDoorStateCount + index,
                        ImperialDoorStateCount, new Vector2(0.5f, 0f), height);
                }
                return states;
            }
            Sprite[][] doors = [DoorRow(doorAtlas, 0, ImperialTilePixels), DoorRow(doorAtlas, 1, ImperialTilePixels),
                DoorRow(tallDoorAtlas, 0, ImperialTilePixels * 2)];

            foreach (var entry in staged)
            {
                if (ids.ContainsKey(entry.Tileset.ShortId) || descriptors.TryGetDescriptor(entry.Tileset.name, out _) ||
                    Data.TileTransformation.GetRecord(entry.Tileset.name) != null)
                {
                    throw new InvalidDataException("Imperial terrain tileset conflicts with an existing ID: " + entry.Tileset.name);
                }
            }

            var imperialIds = new HashSet<short>();
            foreach (var entry in staged)
            {
                var tileset = entry.Tileset;
                descriptors.AddDescriptor(tileset.name, tileset);
                all.Add(tileset);
                ids.Add(tileset.ShortId, tileset);
                imperialIds.Add(tileset.ShortId);
                if (entry.Transformation != null)
                {
                    Data.TileTransformation.AddRecord(tileset.name, entry.Transformation);
                }
            }

            ImperialDamageTileIds = imperialIds;
            ImperialDamageDoorArt = doors;
            ImperialDamagePalettes = [clean, warp];
            Debug.Log("[Cvar_ImperiumOfMan] Imperial damage tilesets registered: " + imperialIds.Count);
        }

        private static ImperialDamagePalette BuildImperialDamagePalette(bool warp,
            List<(BaseTileset Tileset, TileTransformationRecord Transformation)> staged)
        {
            string prefix = warp ? "iom_warp_" : "iom_";
            var atlas = ReadDamageAtlas(warp ? "ImperialWarpDamageTerrain" : "ImperialDamageTerrain", 8, 31);
            var palette = new ImperialDamagePalette { Warp = warp };
            short nextId = (short)(ImperialTileIdBase + (warp ? ImperialPaletteIdStride : 0));
            int offset = 0;

            T Add<T>(string name, int count, short health = 0, short armor = 0, string next = null,
                SurfaceMaterialType surfaceMaterial = SurfaceMaterialType.Ground) where T : BaseTileset
            {
                var tileset = ScriptableObject.CreateInstance<T>();
                tileset.name = prefix + name;
                tileset.Id = tileset.name;
                tileset.SetShortId(nextId++);
                tileset.Spritesheet = atlas;
                tileset.Tiles = new TileBase[count];
                AccessTools.Field(typeof(BaseTileset), "_decalPatterns").SetValue(tileset, new List<AutoDecalMatchPattern>());
                AccessTools.Field(typeof(BaseTileset), "_notUsableInPreset").SetValue(tileset, true);
                if (tileset is SurfaceTileset surface)
                {
                    AccessTools.Field(typeof(SurfaceTileset), "_surfaceType").SetValue(surface, MapCellType.Floor);
                    AccessTools.Field(typeof(SurfaceTileset), "_materialType").SetValue(surface, surfaceMaterial);
                }
                for (int index = 0; index < count; index++)
                {
                    var tile = ScriptableObject.CreateInstance<Tile>();
                    tile.sprite = SliceDamageSprite(atlas, offset++, 8, new Vector2(0.5f, 0.5f));
                    tile.colliderType = Tile.ColliderType.None;
                    tileset.Tiles[index] = tile;
                }
                var record = next == null ? null : new TileTransformationRecord
                {
                    Health = health,
                    Armor = armor,
                    TilesetToMutateName = prefix + next,
                };
                staged.Add((tileset, record));
                return tileset;
            }

            palette.Wall = Add<CornerWallTileset>("wall", 48, 150, 15, "wall_chipped");
            Add<CornerWallTileset>("wall_chipped", 48, 100, 10, "breach_rubble");
            palette.Pillar = Add<CornerWallTileset>("pillar", 48, 150, 15, "pillar_chipped");
            Add<CornerWallTileset>("pillar_chipped", 48, 100, 10, "breach_rubble");
            palette.Facade = Add<CampaignFacadeTileset>("facade", 1, 150, 15, "facade_chipped");
            Add<CampaignFacadeTileset>("facade_chipped", 1, 100, 10, "facade_ruined");
            Add<CampaignFacadeTileset>("facade_ruined", 1, 50, 10, "breach_rubble");
            Add<SurfaceTileset>("breach_rubble", 1);
            for (int family = 0; family < ImperialFloorNames.Length; family++)
            {
                string name = ImperialFloorNames[family];
                int count = ImperialFloorCounts[family];
                var material = family == 1 ? SurfaceMaterialType.Metal : SurfaceMaterialType.Ground;
                palette.Floors[family] = Add<SurfaceTileset>(name, count, 100, 10, name + "_chipped", material);
                Add<SurfaceTileset>(name + "_chipped", count, 50, 10, name + "_broken", material);
                Add<SurfaceTileset>(name + "_broken", count, surfaceMaterial: material);
            }
            if (offset != ImperialTerrainSlots)
            {
                throw new InvalidDataException("Imperial terrain atlas slot order does not match its registration.");
            }
            return palette;
        }

        private static Texture2D ReadDamageAtlas(string name, int columns, int rows)
        {
            var resource = "ImperiumOfMan.Art." + name + ".png";
            using Stream input = typeof(Plugin).Assembly.GetManifestResourceStream(resource) ??
                throw new InvalidDataException("Missing Imperial damage atlas " + resource);
            using var buffer = new MemoryStream();
            input.CopyTo(buffer);
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false)
            {
                name = name,
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
            };
            if (!ImageConversion.LoadImage(texture, buffer.ToArray(), true) ||
                texture.width != columns * ImperialTilePixels || texture.height != rows * ImperialTilePixels)
            {
                UnityEngine.Object.Destroy(texture);
                throw new InvalidDataException("Invalid Imperial damage atlas " + resource);
            }
            return texture;
        }

        private static Sprite SliceDamageSprite(Texture2D texture, int slot, int columns, Vector2 pivot,
            int height = ImperialTilePixels)
        {
            int x = (slot % columns) * ImperialTilePixels;
            int y = texture.height - (slot / columns + 1) * height;
            return Sprite.Create(texture, new Rect(x, y, ImperialTilePixels, height), pivot,
                100f, 0, SpriteMeshType.FullRect);
        }

        private static ImperialDamagePalette GetImperialRaidPalette(State state)
        {
            var raid = state.Get<RaidMetadata>();
            if (ImperialDamagePalettes == null || string.IsNullOrEmpty(raid?.StationId) ||
                raid.RaidType is not (RaidType.Station or RaidType.ProcMission or RaidType.StoryMission))
            {
                return null;
            }
            // Freeze the palette for the lifetime of the raid, including
            // elevator transitions and reloads. Do not repaint an old floor.
            if (raid.CustomCounters.TryGetValue(ImperialTerrainPaletteKey, out int saved) &&
                saved is 1 or 2)
            {
                return ImperialDamagePalettes[saved - 1];
            }

            // Location ownership, not the contract issuer, decides the look of
            // an Imperial/Chaos station. An Imperial raid against Chaos must
            // use the corrupted terrain kit (and vice versa).
            var station = state.Get<Stations>()?.Get(raid.StationId, logMissing: false);
            int finish = station?.OwnerFactionId switch
            {
                ItemCategory => 1,
                ChaosFaction => 2,
                _ => 0,
            };
            if (finish == 0 && raid.RaidType == RaidType.StoryMission)
            {
                var mission = state.Get<Missions>().Get(raid);
                if (IsCampaignMission(mission))
                    finish = mission.BeneficiaryFactionId == ChaosFaction ? 2 : 1;
            }
            if (finish == 0) return null;
            raid.CustomCounters[ImperialTerrainPaletteKey] = finish;
            Debug.Log("[Cvar_ImperiumOfMan] Raid terrain | station=" + raid.StationId +
                " | owner=" + station?.OwnerFactionId + " | palette=" + (finish == 2 ? "corrupted" : "imperial"));
            return ImperialDamagePalettes[finish - 1];
        }

        [HarmonyPatch(typeof(DungeonBuilder), "InitControllers")]
        private static class ImperialDamageFloorInitialization
        {
            private static void Prefix(State ____state, DungeonGameMode ____dungeon, out List<(MapCell Cell, short Health)> __state)
            {
                __state = null;
                var plan = ____state.Get<LocationMetadata>()?.Plan;
                var palette = GetImperialRaidPalette(____state);
                if (palette == null)
                {
                    return;
                }

                var grid = ____state.Get<MapGrid>();
                if (____dungeon.IsFirstLoad)
                {
                    CampaignDeck campaign = null;
                    if (plan != null) TryGetCampaignDeck(plan, out campaign, out _);
                    PlaceImperialMachinery(____state, palette.Warp);
                    PaintNativeImperialPresetFloors(____state, campaign, GetImperialBranchFloor(____state));
                    ApplyImperialTerrain(grid, palette);
                }
                else
                {
                    // The standard initialization fills health for the tileset. Retain damage
                    // already serialized by the game on this floor.
                    __state = new List<(MapCell, short)>();
                    for (int y = 0; y < grid.MaxHeight; y++)
                    for (int x = 0; x < grid.MaxWidth; x++)
                    {
                        var cell = grid.GetCell(x, y, checkBorders: false);
                        short tileset = cell.GetTilesetId(1);
                        if (tileset == LegacyDeckLampId)
                            RestoreLegacyCampaignLamp(grid, palette, cell);
                        else if (tileset >= DeckDressingTileBase && tileset < LegacyDeckLampId &&
                            cell.Type is MapCellType.Wall or MapCellType.FacadeWall)
                            PaintDeckCell(cell, cell.Type, cell.Type == MapCellType.FacadeWall ? palette.Facade :
                                tileset == DeckDressingTileBase + 1 ? palette.Pillar : palette.Wall);
                        if (ImperialDamageTileIds.Contains(cell.GetTilesetId(1)))
                        {
                            __state.Add((cell, cell.health));
                        }
                    }
                }

                foreach (var door in ____state.Get<MapObstacles>().Doors)
                {
                    if (BindImperialDamageDoor(door))
                    {
                        door.RefreshSprite(ignoreAnimator: true);
                    }
                }
            }

            private static void Postfix(List<(MapCell Cell, short Health)> __state)
            {
                if (__state == null)
                {
                    return;
                }
                foreach (var entry in __state)
                {
                    if (entry.Cell.health != short.MinValue)
                    {
                        entry.Cell.health = entry.Health;
                    }
                }
            }
        }

        private static bool BindImperialDamageDoor(Door door)
        {
            if (door == null || door.MaxHealth <= 0)
            {
                return false;
            }
            var cells = door.MapObstacle.OccupiedCells;
            bool vertical = door.DoorOrientation == Door.Orientation.Vertical;
            if (cells.Count == 1)
            {
                ImperialDamageDoors.GetValue(door, _ => ImperialDamageDoorArt[vertical ? 1 : 0]);
            }
            else if (vertical && cells.Count == 2 && cells[0].X == cells[1].X && Math.Abs(cells[0].Y - cells[1].Y) == 1)
            {
                ImperialDamageDoors.GetValue(door, _ => ImperialDamageDoorArt[2]);
            }
            else
            {
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(MapObstacleFactory), nameof(MapObstacleFactory.Spawn))]
        private static class ImperialDamageDoorSpawn
        {
            private static void Postfix(State ____state, MapObstacle __result)
            {
                if (__result?.Door != null && GetImperialRaidPalette(____state) != null)
                {
                    BindImperialDamageDoor(__result.Door);
                }
            }
        }

        private static void ApplyImperialTerrain(MapGrid grid, ImperialDamagePalette palette)
        {
            for (int y = 0; y < grid.MaxHeight; y++)
            for (int x = 0; x < grid.MaxWidth; x++)
            {
                var cell = grid.GetCell(x, y, checkBorders: false);
                var donor = Data.Tilesets.GetByShortId(cell.GetTilesetId(1));
                var transformation = donor == null ? null : Data.TileTransformation.GetRecord(donor.name);
                if (transformation == null || string.IsNullOrEmpty(transformation.TilesetToMutateName) ||
                    ImperialDamageTileIds.Contains(donor.ShortId) || !IsIntactTerrain(donor.name))
                {
                    continue;
                }
                BaseTileset target;
                byte index = 0;
                switch (cell.Type)
                {
                    case MapCellType.Wall when donor is CornerWallTileset:
                        index = donor.ExplainTile(1, x, y, grid);
                        target = index is 0 or 10 or 16 or 24 or 26 ? palette.Pillar : palette.Wall;
                        break;
                    case MapCellType.FacadeWall:
                        target = palette.Facade;
                        break;
                    case MapCellType.Floor when donor is SurfaceTileset { SurfaceType: MapCellType.Floor }:
                        target = palette.Floors[ImperialFloorFamily(donor.name)];
                        index = ImperialSurfaceVariant(x, y, target.Tiles.Length);
                        break;
                    default:
                        continue;
                }
                cell.SetTilesetId(1, target.ShortId);
                cell.SetTileIndex(1, index);
            }
        }

        private static byte ImperialSurfaceVariant(int x, int y, int count) =>
            (byte)(((x * 29) ^ (y * 67)) % count);

        [HarmonyPatch(typeof(SurfaceTileset), nameof(SurfaceTileset.ExplainTile))]
        private static class ImperialDamageSurfaceVariant
        {
            private static void Postfix(SurfaceTileset __instance, int layerIndex, int x, int y, ref byte __result)
            {
                if (layerIndex == 1 && ImperialDamageTileIds?.Contains(__instance.ShortId) == true)
                {
                    // SurfaceBrush starts a transformed tile at index zero. The position selects
                    // the matching intact, chipped, or broken tile from the same material row.
                    __result = ImperialSurfaceVariant(x, y, __instance.Tiles.Length);
                }
            }
        }

        private static bool IsIntactTerrain(string name) =>
            name.IndexOf("damag", StringComparison.OrdinalIgnoreCase) < 0 &&
            name.IndexOf("destroy", StringComparison.OrdinalIgnoreCase) < 0 &&
            name.IndexOf("broken", StringComparison.OrdinalIgnoreCase) < 0 &&
            name.IndexOf("rubble", StringComparison.OrdinalIgnoreCase) < 0;

        private static int ImperialFloorFamily(string name)
        {
            if (name.IndexOf("temple", StringComparison.OrdinalIgnoreCase) >= 0 ||
                name.IndexOf("marble", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return 2;
            }
            if (name.IndexOf("slate", StringComparison.OrdinalIgnoreCase) >= 0 ||
                name.IndexOf("concrete", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return 3;
            }
            return name.IndexOf("indus", StringComparison.OrdinalIgnoreCase) >= 0 ||
                name.IndexOf("deck", StringComparison.OrdinalIgnoreCase) >= 0 ||
                name.IndexOf("metal", StringComparison.OrdinalIgnoreCase) >= 0 ? 1 : 0;
        }

        [HarmonyPatch(typeof(Door), nameof(Door.RefreshSprite))]
        private static class ImperialDamageDoorRefresh
        {
            private static void Postfix(Door __instance, bool ignoreAnimator)
            {
                if (ignoreAnimator || !__instance.FrameAnimator.IsPlaying)
                {
                    PaintImperialDoorDamage(__instance);
                }
            }
        }

        [HarmonyPatch(typeof(Door), nameof(Door.Collapse))]
        private static class ImperialDamageDoorCollapse
        {
            private static void Postfix(Door __instance) => PaintImperialDoorDamage(__instance);
        }

        private static void PaintImperialDoorDamage(Door door)
        {
            if (!ImperialDamageDoors.TryGetValue(door, out var art))
            {
                return;
            }
            int stage = door.State == Door.DoorState.Remains ? 4 : door.DamageState switch
            {
                Door.DamagedState.Damaged => door.State == Door.DoorState.Opened ? 1 : 0,
                Door.DamagedState.Shattered1 => 2,
                Door.DamagedState.Shattered2 => 3,
                Door.DamagedState.Collapsed => 4,
                _ => -1,
            };
            if (stage < 0)
            {
                return;
            }
            door.MapObstacle.Renderer.sprite = art[stage];
            if (door.FrontFrameAnimator != null) door.FrontFrameAnimator.Renderer.sprite = null;
            if (door.BloomLight != null) door.BloomLight.sprite = null;
            if (door.UnderDoorCover != null) door.UnderDoorCover.sprite = null;
            if (door.VerticalTopCover != null) door.VerticalTopCover.sprite = null;
        }
    }
}

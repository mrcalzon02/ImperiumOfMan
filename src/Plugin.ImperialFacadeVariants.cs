using System;
using System.Collections.Generic;
using HarmonyLib;
using MGSC;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        // Keep all existing terrain IDs and their serialized slot order.
        // Campaign litter occupies IDs through 29025; reserve this new block.
        private const short ImperialFacadeVariantBase = 29040;
        private static readonly string[] ImperialFacadeVariantNames =
        [
            "cracked", "plate", "vent", "recessed_pipes",
            "brace", "hatch", "aquila", "medical",
            "recessed_grille", "junction", "archive", "shrine",
            // Append; original accent IDs and atlas indices remain stable.
            "crenellated_trim", "arched_service_trim", "reinforced_bulwark", "vent_hood",
            "banner_niche", "reliquary_arch", "illuminated_grille", "ruined_overhang",
            "conduit_channel", "imperial_cornice", "hazard_bulkhead", "gothic_arcature"
        ];
        private const int OriginalFacadeAccentCount = 12;
        private static CampaignFacadeTileset[] _imperialFacadeVariants;

        private static CampaignFacadeTileset[] StageImperialFacadeVariants(ImperialDamagePalette clean,
            List<(BaseTileset Tileset, TileTransformationRecord Transformation)> staged)
        {
            var atlas = ReadDamageAtlas("ImperialFacadeVariants", 4, 3);
            var topbarAtlas = ReadDamageAtlas("ImperialFacadeTopbarVariants", 4, 3);
            var intact = staged.Find(entry => ReferenceEquals(entry.Tileset, clean.Facade)).Transformation;
            var result = new CampaignFacadeTileset[ImperialFacadeVariantNames.Length];
            for (int index = 0; index < result.Length; index++)
            {
                var facade = ScriptableObject.CreateInstance<CampaignFacadeTileset>();
                facade.name = facade.Id = "iom_facade_accent_" + ImperialFacadeVariantNames[index];
                facade.SetShortId((short)(ImperialFacadeVariantBase + index));
                bool topbar = index >= OriginalFacadeAccentCount;
                facade.Spritesheet = topbar ? topbarAtlas : atlas;
                // One sprite per tileset, just like the original native-style
                // facade. No new orientation or auto-tiling rule is needed.
                var tile = ScriptableObject.CreateInstance<Tile>();
                tile.sprite = SliceDamageSprite(facade.Spritesheet,
                    topbar ? index - OriginalFacadeAccentCount : index, 4, new Vector2(0.5f, 0.5f));
                tile.colliderType = Tile.ColliderType.None;
                facade.Tiles = [tile];
                AccessTools.Field(typeof(BaseTileset), "_decalPatterns").SetValue(facade, new List<AutoDecalMatchPattern>());
                AccessTools.Field(typeof(BaseTileset), "_notUsableInPreset").SetValue(facade, true);
                // Exactly the base facade's physical stats and existing damage
                // chain. The decorative insert disappears when the wall chips.
                staged.Add((facade, new TileTransformationRecord
                {
                    Health = intact.Health,
                    Armor = intact.Armor,
                    TilesetToMutateName = intact.TilesetToMutateName
                }));
                result[index] = facade;
            }
            return result;
        }

        // Indices correspond to the fixed row-major atlas order above.
        private static readonly int[] GeneralFacadeAccents = [0, 1, 2, 4, 5];
        private static readonly int[] CorridorFacadeAccents = [0, 1, 2, 3, 4, 8, 9];
        private static readonly int[] CargoFacadeAccents = [1, 3, 4, 5, 8];
        private static readonly int[] WorkshopFacadeAccents = [2, 3, 4, 5, 8, 9];
        private static readonly int[] MedicalFacadeAccents = [7, 2, 5, 1];
        private static readonly int[] LaboratoryFacadeAccents = [8, 9, 2, 5, 1];
        private static readonly int[] CeremonialFacadeAccents = [6, 10, 11, 0];
        private static readonly int[] RelayFacadeAccents = [9, 2, 8, 5, 10];
        private static readonly int[] ObjectiveFacadeAccents = [6, 1, 4, 5];

        // Stronger cornice-breaking accents appear only in the longer
        // uninterrupted runs, at a lower density than ordinary accents.
        private static readonly int[] GeneralTopbarAccents = [12, 14, 15, 19];
        private static readonly int[] CorridorTopbarAccents = [12, 13, 14, 15, 18, 19, 20, 22];
        private static readonly int[] CargoTopbarAccents = [14, 15, 20, 22];
        private static readonly int[] WorkshopTopbarAccents = [14, 15, 18, 19, 20, 22];
        private static readonly int[] MedicalTopbarAccents = [13, 15, 18];
        private static readonly int[] LaboratoryTopbarAccents = [13, 15, 18, 20];
        private static readonly int[] CeremonialTopbarAccents = [16, 17, 21, 23];
        private static readonly int[] RelayTopbarAccents = [13, 15, 18, 20];
        private static readonly int[] ObjectiveTopbarAccents = [14, 16, 21, 22, 23];

        private static readonly int[] ArchiveFacadeAccents = [10, 1, 5, 6];
        private static readonly int[] SecurityFacadeAccents = [4, 5, 1, 8];
        private static readonly int[] QuartersFacadeAccents = [1, 5, 2, 10];
        private static readonly int[] UtilityFacadeAccents = [3, 9, 2, 8];
        private static readonly int[] ArchiveTopbarAccents = [21, 23, 12, 16];
        private static readonly int[] SecurityTopbarAccents = [14, 22, 12, 16];
        private static readonly int[] QuartersTopbarAccents = [13, 16, 15];
        private static readonly int[] UtilityTopbarAccents = [20, 15, 18, 14];

        private static int[] TopbarAccentsFor(ImperialRoomTheme theme) => theme switch
        {
            ImperialRoomTheme.Corridor or ImperialRoomTheme.Lift => CorridorTopbarAccents,
            ImperialRoomTheme.Cargo => CargoTopbarAccents,
            ImperialRoomTheme.Workshop => WorkshopTopbarAccents,
            ImperialRoomTheme.Medical => MedicalTopbarAccents,
            ImperialRoomTheme.Laboratory or ImperialRoomTheme.Survey => LaboratoryTopbarAccents,
            ImperialRoomTheme.Ceremonial => CeremonialTopbarAccents,
            ImperialRoomTheme.Relay => RelayTopbarAccents,
            ImperialRoomTheme.Objective => ObjectiveTopbarAccents,
            ImperialRoomTheme.Archive => ArchiveTopbarAccents,
            ImperialRoomTheme.Security => SecurityTopbarAccents,
            ImperialRoomTheme.Quarters => QuartersTopbarAccents,
            ImperialRoomTheme.Utility => UtilityTopbarAccents,
            _ => GeneralTopbarAccents
        };

        private static int[] FacadeAccentsFor(ImperialRoomTheme theme) => theme switch
        {
            ImperialRoomTheme.Corridor or ImperialRoomTheme.Lift => CorridorFacadeAccents,
            ImperialRoomTheme.Cargo => CargoFacadeAccents,
            ImperialRoomTheme.Workshop => WorkshopFacadeAccents,
            ImperialRoomTheme.Medical => MedicalFacadeAccents,
            ImperialRoomTheme.Laboratory or ImperialRoomTheme.Survey => LaboratoryFacadeAccents,
            ImperialRoomTheme.Ceremonial => CeremonialFacadeAccents,
            ImperialRoomTheme.Relay => RelayFacadeAccents,
            ImperialRoomTheme.Objective => ObjectiveFacadeAccents,
            ImperialRoomTheme.Archive => ArchiveFacadeAccents,
            ImperialRoomTheme.Security => SecurityFacadeAccents,
            ImperialRoomTheme.Quarters => QuartersFacadeAccents,
            ImperialRoomTheme.Utility => UtilityFacadeAccents,
            _ => GeneralFacadeAccents
        };

        private static HashSet<CellPosition> ImperialFacadeProtection(State state)
        {
            var protectedCells = new HashSet<CellPosition>();
            foreach (var obstacle in state.Get<MapObstacles>().Obstacles)
            {
                protectedCells.Add(obstacle.Position);
                protectedCells.UnionWith(obstacle.OccupiedCells);
            }
            var metadata = state.Get<MapMetadata>();
            if (metadata != null)
            {
                foreach (var lamp in metadata.MapLights) protectedCells.Add(lamp.Pos);
                foreach (var prop in metadata.MapObstacles) protectedCells.Add(prop.Pos);
                foreach (var entry in metadata.LevelEntries) protectedCells.Add(entry.Position);
                foreach (var exit in metadata.LevelExits) protectedCells.Add(exit.Position);
            }
            return protectedCells;
        }

        private static bool IsPlainImperialFacadeCell(MapGrid grid, int x, int y)
        {
            if (x <= 0 || x >= grid.MaxWidth - 1 || y <= 0 || y >= grid.MaxHeight - 1) return false;
            var face = grid.GetCell(x, y);
            return face != null && face.Type == MapCellType.FacadeWall &&
                !face.IsDoor && face.specialFlag == MapCellSpecialFlag.None &&
                (face.GetTilesetId(2) == 0 || face.GetTileIndex(2) == byte.MaxValue) &&
                HasCampaignWallFace(grid, x, y);
        }

        private static bool IsSafeImperialFacadeAccent(MapGrid grid, int x, int y,
            ISet<CellPosition> protectedCells)
        {
            if (!IsPlainImperialFacadeCell(grid, x, y)) return false;
            for (int dx = -1; dx <= 1; dx++)
            for (int dy = -1; dy <= 1; dy++)
                if (protectedCells.Contains(new CellPosition(x + dx, y + dy))) return false;
            return true;
        }

        private static void ApplyImperialFacadeVariation(State state, MapGrid grid, ImperialDamagePalette palette)
        {
            if (palette.Warp || _imperialFacadeVariants == null || grid == null ||
                ImperialSetting("rooms.facades", 1) == 0) return;
            int spacing = ImperialSetting("rooms.facade_spacing", 4);
            int strongEvery = ImperialSetting("rooms.strong_every", 3);
            try
            {
                var location = state.Get<LocationMetadata>();
                var rooms = location.GeneratedRooms;
                var obstacles = state.Get<MapObstacles>().Obstacles;
                var protectedCells = ImperialFacadeProtection(state);
                var catalog = BuildImperialRoomThemeCatalog();
                var themes = new Dictionary<int, ImperialRoomTheme>();
                var roomRandoms = new Dictionary<int, System.Random>();
                var signedRooms = new HashSet<int>();
                var random = new System.Random(unchecked(location.Seed ^ 0x46414345));
                var counts = new int[_imperialFacadeVariants.Length];
                var roomCounts = new Dictionary<int, int>();

                // A fixture can interrupt accent placement, but does not
                // split an otherwise continuous plain wall into tiny runs.
                bool IsBaseRunCell(int x, int y) => IsPlainImperialFacadeCell(grid, x, y) &&
                    grid.GetCell(x, y).GetTilesetId(1) == palette.Facade.ShortId;

                int OwnerAt(int x, int y) =>
                    FindNativePresetRoom(rooms, new CellPosition(x, y - 1));

                ImperialRoomTheme ThemeAt(int owner)
                {
                    if (owner < 0 || owner >= rooms.Count) return ImperialRoomTheme.General;
                    if (!themes.TryGetValue(owner, out var theme))
                    {
                        theme = ClassifyImperialRoomTheme(rooms[owner], obstacles, catalog);
                        themes.Add(owner, theme);
                    }
                    return theme;
                }

                System.Random RoomRandom(int owner)
                {
                    if (owner < 0 || owner >= rooms.Count) return random;
                    if (!roomRandoms.TryGetValue(owner, out var rng))
                    {
                        var room = rooms[owner];
                        int seed = unchecked(location.Seed ^ room.PresetBinaryId * 397 ^
                            room.LeftLowerCorner.X * 73856093 ^ room.LeftLowerCorner.Y * 19349663);
                        roomRandoms.Add(owner, rng = new System.Random(seed));
                    }
                    return rng;
                }

                for (int y = 1; y < grid.MaxHeight - 1; y++)
                {
                    int x = 1;
                    while (x < grid.MaxWidth - 1)
                    {
                        if (!IsBaseRunCell(x, y)) { x++; continue; }
                        int start = x++;
                        while (x < grid.MaxWidth - 1 && IsBaseRunCell(x, y)) x++;
                        int end = x; // exclusive
                        int length = end - start;
                        if (length < 3) continue;

                        // Leave original cells at both ends. At least three
                        // plain tiles between accents, with seeded variation
                        // on long runs instead of a mechanically repeating wall.
                        int column = start + 1;
                        int previous = -1;
                        int accentNumber = 0;
                        int topbarPhase = random.Next(Math.Max(1, strongEvery));
                        while (column < end - 1)
                        {
                            if (!IsSafeImperialFacadeAccent(grid, column, y, protectedCells))
                            {
                                column++;
                                continue;
                            }
                            int owner = OwnerAt(column, y);
                            var theme = ThemeAt(owner);
                            var rng = RoomRandom(owner);
                            bool roomSignature = strongEvery > 0 && owner >= 0 && owner < rooms.Count &&
                                theme is not (ImperialRoomTheme.Corridor or ImperialRoomTheme.Lift) &&
                                !IsImperialSupplyClosetPreset(rooms[owner].PresetBinaryId) &&
                                !signedRooms.Contains(owner);
                            // A compact room may have only one clear slot.
                            // Its first accent is still a signature from batch two.
                            bool topbar = roomSignature || strongEvery > 0 && length >= 7 &&
                                (accentNumber + topbarPhase) % strongEvery == 0;
                            int[] pool = topbar ? TopbarAccentsFor(theme) : FacadeAccentsFor(theme);
                            int choice = rng.Next(pool.Length);
                            if (pool[choice] == previous)
                                choice = (choice + 1 + rng.Next(pool.Length - 1)) % pool.Length;
                            int index = pool[choice];
                            var cell = grid.GetCell(column, y);
                            // Do not change Type, flags, geometry, fences, health,
                            // decals, collisions or any neighboring wall cap.
                            cell.SetTilesetId(1, _imperialFacadeVariants[index].ShortId);
                            cell.SetTileIndex(1, 0);
                            counts[index]++;
                            previous = index;
                            accentNumber++;
                            if (owner >= 0)
                            {
                                roomCounts.TryGetValue(owner, out int count);
                                roomCounts[owner] = count + 1;
                                if (roomSignature) signedRooms.Add(owner);
                            }
                            column += spacing + rng.Next(3);
                        }
                    }
                }
                int total = 0;
                foreach (int count in counts) total += count;
                Debug.Log("[Cvar_ImperiumOfMan] Imperial facade accents=" + total +
                    " | rooms with accents=" + roomCounts.Count + " | signature rooms=" + signedRooms.Count +
                    " | by atlas slot=[" + string.Join(",", counts) + "]");
            }
            catch (Exception error)
            {
                // Visual enhancement must not prevent floor initialization.
                Debug.LogWarning("[Cvar_ImperiumOfMan] Imperial facade variation skipped: " + error);
            }
        }
    }
}

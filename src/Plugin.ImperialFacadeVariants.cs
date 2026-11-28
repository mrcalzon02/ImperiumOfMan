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
            "recessed_grille", "junction", "archive", "shrine"
        ];
        private static CampaignFacadeTileset[] _imperialFacadeVariants;

        private static CampaignFacadeTileset[] StageImperialFacadeVariants(ImperialDamagePalette clean,
            List<(BaseTileset Tileset, TileTransformationRecord Transformation)> staged)
        {
            var atlas = ReadDamageAtlas("ImperialFacadeVariants", 4, 3);
            var intact = staged.Find(entry => ReferenceEquals(entry.Tileset, clean.Facade)).Transformation;
            var result = new CampaignFacadeTileset[ImperialFacadeVariantNames.Length];
            for (int index = 0; index < result.Length; index++)
            {
                var facade = ScriptableObject.CreateInstance<CampaignFacadeTileset>();
                facade.name = facade.Id = "iom_facade_accent_" + ImperialFacadeVariantNames[index];
                facade.SetShortId((short)(ImperialFacadeVariantBase + index));
                facade.Spritesheet = atlas;
                // One sprite per tileset, just like the original native-style
                // facade. No new orientation or auto-tiling rule is needed.
                var tile = ScriptableObject.CreateInstance<Tile>();
                tile.sprite = SliceDamageSprite(atlas, index, 4, new Vector2(0.5f, 0.5f));
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

        private static int[] FacadeAccentsFor(NativePresetArea area) => area switch
        {
            NativePresetArea.Corridor or NativePresetArea.LiftLobby => CorridorFacadeAccents,
            NativePresetArea.Cargo => CargoFacadeAccents,
            NativePresetArea.Workshop => WorkshopFacadeAccents,
            NativePresetArea.Medical => MedicalFacadeAccents,
            NativePresetArea.Laboratory => LaboratoryFacadeAccents,
            NativePresetArea.Ceremonial => CeremonialFacadeAccents,
            NativePresetArea.Relay => RelayFacadeAccents,
            NativePresetArea.Objective => ObjectiveFacadeAccents,
            _ => GeneralFacadeAccents
        };

        private static void ApplyImperialFacadeVariation(State state, MapGrid grid, ImperialDamagePalette palette)
        {
            if (palette.Warp || _imperialFacadeVariants == null || grid == null) return;
            try
            {
                var location = state.Get<LocationMetadata>();
                var rooms = location.GeneratedRooms;
                var obstacles = state.Get<MapObstacles>().Obstacles;
                var metadata = state.Get<MapMetadata>();
                var protectedCells = new HashSet<CellPosition>();
                foreach (var obstacle in obstacles)
                {
                    protectedCells.Add(obstacle.Position);
                    protectedCells.UnionWith(obstacle.OccupiedCells);
                }
                if (metadata != null)
                {
                    foreach (var lamp in metadata.MapLights) protectedCells.Add(lamp.Pos);
                    foreach (var prop in metadata.MapObstacles) protectedCells.Add(prop.Pos);
                }

                // Determine the room behind each accent, not the wall's
                // own coordinate. Cache classification only for rooms used.
                var areas = new Dictionary<int, NativePresetArea>();
                var random = new System.Random(unchecked(location.Seed ^ 0x46414345));
                var counts = new int[_imperialFacadeVariants.Length];

                bool Eligible(int x, int y)
                {
                    var face = grid.GetCell(x, y);
                    if (face == null || face.Type != MapCellType.FacadeWall ||
                        face.GetTilesetId(1) != palette.Facade.ShortId ||
                        face.IsDoor || face.specialFlag != MapCellSpecialFlag.None ||
                        (face.GetTilesetId(2) != 0 && face.GetTileIndex(2) != byte.MaxValue) ||
                        !HasCampaignWallFace(grid, x, y))
                        return false;
                    // Keep native doors, lights and obstacle overlays clear.
                    // Protect the wall, its cap, and the immediately visible floor.
                    for (int dx = -1; dx <= 1; dx++)
                    for (int dy = -1; dy <= 1; dy++)
                        if (protectedCells.Contains(new CellPosition(x + dx, y + dy)))
                            return false;
                    return true;
                }

                NativePresetArea AreaAt(int x, int y)
                {
                    int owner = FindNativePresetRoom(rooms, new CellPosition(x, y - 1));
                    if (owner < 0 || owner >= rooms.Count) return NativePresetArea.General;
                    if (!areas.TryGetValue(owner, out var area))
                    {
                        area = ClassifyNativePresetArea(rooms[owner], obstacles);
                        areas.Add(owner, area);
                    }
                    return area;
                }

                for (int y = 1; y < grid.MaxHeight - 1; y++)
                {
                    int x = 1;
                    while (x < grid.MaxWidth - 1)
                    {
                        if (!Eligible(x, y)) { x++; continue; }
                        int start = x++;
                        while (x < grid.MaxWidth - 1 && Eligible(x, y)) x++;
                        int end = x; // exclusive
                        int length = end - start;
                        if (length < 3) continue;

                        // Leave original cells at both ends. At least three
                        // plain tiles between accents, with seeded variation
                        // on long runs instead of a mechanically repeating wall.
                        int column = length <= 5 ? start + length / 2 : start + 1 + random.Next(3);
                        int previous = -1;
                        while (column < end - 1)
                        {
                            int[] pool = FacadeAccentsFor(AreaAt(column, y));
                            int choice = random.Next(pool.Length);
                            if (pool[choice] == previous)
                                choice = (choice + 1 + random.Next(pool.Length - 1)) % pool.Length;
                            int index = pool[choice];
                            var cell = grid.GetCell(column, y);
                            // Do not change Type, flags, geometry, fences, health,
                            // decals, collisions or any neighboring wall cap.
                            cell.SetTilesetId(1, _imperialFacadeVariants[index].ShortId);
                            cell.SetTileIndex(1, 0);
                            counts[index]++;
                            previous = index;
                            column += 4 + random.Next(3);
                        }
                    }
                }
                int total = 0;
                foreach (int count in counts) total += count;
                Debug.Log("[Cvar_ImperiumOfMan] Imperial facade accents=" + total +
                    " by atlas slot=[" + string.Join(",", counts) + "]");
            }
            catch (Exception error)
            {
                // Visual enhancement must not prevent floor initialization.
                Debug.LogWarning("[Cvar_ImperiumOfMan] Imperial facade variation skipped: " + error);
            }
        }
    }
}

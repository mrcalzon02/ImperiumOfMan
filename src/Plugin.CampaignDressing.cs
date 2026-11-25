using System.Collections.Generic;
using HarmonyLib;
using MGSC;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private const int DeckDressingVariants = 24;
        private const short DeckDressingTileBase = 28621;
        private const short DeckFixtureOverlayId = 28626;
        private const short LegacyDeckLampId = DeckDressingTileBase + 3;
        private sealed class DeckFixtureTileset : CornerWallTileset
        {
            public override byte ExplainTile(int layerIndex, int x, int y, MapGrid grid) =>
                layerIndex == 1 ? grid.GetCell(x, y).GetTileIndex(1) : byte.MaxValue;
        }

        private sealed class CampaignFacadeTileset : FacadeWallTileset
        {
            public override byte ExplainDecal(int x, int y, MapGrid grid, out short decalTileset)
            {
                var cell = grid.GetCell(x, y);
                if (cell.Type == MapCellType.FacadeWall && cell.GetTilesetId(2) == DeckFixtureOverlayId &&
                    cell.GetTileIndex(2) == 0 && HasCampaignWallFace(grid, x, y))
                {
                    decalTileset = DeckFixtureOverlayId;
                    return 0;
                }
                return base.ExplainDecal(x, y, grid, out decalTileset);
            }
        }

        private static string CampaignEvidenceArtwork(string missionId, bool alternate)
        {
            if (!alternate)
                return CampaignDecks.TryGetValue(missionId, out var deck) ? "CampaignObject" + deck.Tile : null;
            return missionId switch
            {
                BreadAndSeal => "CampaignLedger",
                MuteTheBuoy => "CampaignSurveyPlate",
                HollowSignal => "CampaignSignalKey",
                UnmakeTheSeal => "CampaignBinding",
                _ => null
            };
        }

        private static void RegisterCampaignDeckDressing(DescriptorsCollection descriptors, List<BaseTileset> all,
            Dictionary<short, BaseTileset> ids)
        {
            // Saved floors still refer to the retired fixture tilesets by ID.
            var atlas = ReadDamageAtlas("CampaignDeckFixtures", DeckDressingVariants, 4);
            Add<DeckFixtureTileset>("pipe", 0, DeckDressingVariants, "iom_wall_chipped");
            Add<DeckFixtureTileset>("pillar", 1, DeckDressingVariants, "iom_pillar_chipped");
            Add<DeckFixtureTileset>("cover", 2, DeckDressingVariants, "iom_breach_rubble");
            // Keep the old tilesets available for saves created before the fixtures moved to decals.
            Add<DeckFixtureTileset>("lamp", 3, 4, "iom_wall_chipped");
            Add<SurfaceTileset>("drain", 3, 4, "iom_deck_chipped", column: 4);
            var overlay = ScriptableObject.CreateInstance<DeckFixtureTileset>();
            overlay.name = overlay.Id = "iom_campaign_fixture_overlay";
            overlay.SetShortId(DeckFixtureOverlayId);
            overlay.Spritesheet = ReadDamageAtlas("CampaignFixtureOverlays", 5, 1);
            overlay.Tiles = new TileBase[5];
            AccessTools.Field(typeof(BaseTileset), "_decalPatterns").SetValue(overlay, new List<AutoDecalMatchPattern>());
            AccessTools.Field(typeof(BaseTileset), "_notUsableInPreset").SetValue(overlay, true);
            for (int i = 0; i < overlay.Tiles.Length; i++)
            {
                var tile = ScriptableObject.CreateInstance<Tile>();
                tile.sprite = SliceDamageSprite(overlay.Spritesheet, i, overlay.Tiles.Length, new Vector2(0.5f, 0.5f));
                tile.colliderType = Tile.ColliderType.None;
                overlay.Tiles[i] = tile;
            }
            descriptors.AddDescriptor(overlay.Id, overlay);
            all.Add(overlay);
            ids.Add(overlay.ShortId, overlay);

            T Add<T>(string name, int row, int count, string chipped, int column = 0) where T : BaseTileset
            {
                var tileset = ScriptableObject.CreateInstance<T>();
                tileset.name = tileset.Id = "iom_campaign_fixture_" + name;
                tileset.SetShortId((short)(DeckDressingTileBase + row + (column == 0 ? 0 : 1)));
                tileset.Spritesheet = atlas;
                tileset.Tiles = new TileBase[count];
                AccessTools.Field(typeof(BaseTileset), "_decalPatterns").SetValue(tileset, new List<AutoDecalMatchPattern>());
                AccessTools.Field(typeof(BaseTileset), "_notUsableInPreset").SetValue(tileset, true);
                for (int index = 0; index < count; index++)
                {
                    var tile = ScriptableObject.CreateInstance<Tile>();
                    tile.sprite = SliceDamageSprite(atlas, row * DeckDressingVariants + column + index,
                        DeckDressingVariants, new Vector2(0.5f, 0.5f));
                    tile.colliderType = Tile.ColliderType.None;
                    tileset.Tiles[index] = tile;
                }
                descriptors.AddDescriptor(tileset.Id, tileset);
                all.Add(tileset);
                ids.Add(tileset.ShortId, tileset);
                ImperialDamageTileIds.Add(tileset.ShortId);
                Data.TileTransformation.AddRecord(tileset.Id,
                    new TileTransformationRecord { Health = 130, Armor = 12, TilesetToMutateName = chipped });
                return tileset;
            }
        }

        private static void PutCampaignFixtureOverlay(MapCell cell, byte index)
        {
            cell.SetTilesetId(2, DeckFixtureOverlayId);
            cell.SetTileIndex(2, index);
        }

        private static bool HasCampaignWallFace(MapGrid grid, int x, int y) =>
            grid.GetCell(x, y - 1)?.Type == MapCellType.Floor && grid.GetCell(x, y + 1)?.Type == MapCellType.Wall;

        private static void RestoreLegacyCampaignLamp(MapGrid grid, ImperialDamagePalette palette, MapCell cell)
        {
            var cap = grid.GetCell(cell.X, cell.Y + 1);
            if (grid.GetCell(cell.X, cell.Y - 1)?.Type == MapCellType.Floor && cap?.Type is MapCellType.Wall or MapCellType.None)
            {
                if (cap.Type == MapCellType.None) PaintDeckCell(cap, MapCellType.Wall, palette.Wall);
                PaintDeckCell(cell, MapCellType.FacadeWall, palette.Facade);
                PutCampaignFixtureOverlay(cell, 0);
                return;
            }
            PaintDeckCell(cell, MapCellType.Wall, palette.Wall);
        }

    }
}

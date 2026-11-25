using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HarmonyLib;
using MGSC;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private const short CampaignTileIdBase = 28600;
        private const int CampaignFloorTileCount = 80;

        private sealed class CampaignSurfaceTileset : SurfaceTileset
        {
            public override byte ExplainTile(int layerIndex, int x, int y, MapGrid grid) =>
                layerIndex == 1 ? grid.GetCell(x, y).GetTileIndex(1) : byte.MaxValue;

            public override byte ExplainDecal(int x, int y, MapGrid grid, out short decalTileset)
            {
                var cell = grid.GetCell(x, y);
                byte index = cell.GetTileIndex(2);
                if (cell.Type == MapCellType.Floor && IsNativePresetTrimId(cell.GetTilesetId(2)) &&
                    index < CampaignJoinShapeCount * 4)
                {
                    decalTileset = cell.GetTilesetId(2);
                    return index;
                }
                if (cell.GetTilesetId(2) == DeckFixtureOverlayId && index is >= 1 and <= 4)
                {
                    decalTileset = DeckFixtureOverlayId;
                    return index;
                }
                return base.ExplainDecal(x, y, grid, out decalTileset);
            }
        }

        private static void RegisterCampaignDeckTiles()
        {
            var atlas = ReadDamageAtlas("CampaignFloorKits", CampaignFloorTileCount, CampaignDecks.Count);
            var descriptors = (DescriptorsCollection)AccessTools.Field(typeof(Tilesets), "_descriptors").GetValue(Data.Tilesets);
            var all = (List<BaseTileset>)AccessTools.Field(typeof(Tilesets), "_all").GetValue(Data.Tilesets);
            var ids = (Dictionary<short, BaseTileset>)AccessTools.Field(typeof(Tilesets), "_shortIdsToTilesets").GetValue(Data.Tilesets);
            int index = 0;
            foreach (var deck in ImperialCampaignDecks.Concat(ChaosCampaignDecks))
            {
                var floor = ScriptableObject.CreateInstance<CampaignSurfaceTileset>();
                floor.name = floor.Id = "iom_campaign_floor_" + deck.Key;
                floor.SetShortId((short)(CampaignTileIdBase + index));
                if (ids.ContainsKey(floor.ShortId) || descriptors.TryGetDescriptor(floor.Id, out _))
                {
                    throw new InvalidDataException("Campaign tileset conflicts with an existing tile " + floor.Id);
                }
                floor.Spritesheet = atlas;
                floor.Tiles = new TileBase[CampaignFloorTileCount];
                for (int slot = 0; slot < CampaignFloorTileCount; slot++)
                {
                    var tile = ScriptableObject.CreateInstance<Tile>();
                    tile.sprite = SliceDamageSprite(atlas, index * CampaignFloorTileCount + slot, CampaignFloorTileCount,
                        new Vector2(0.5f, 0.5f));
                    tile.colliderType = Tile.ColliderType.None;
                    floor.Tiles[slot] = tile;
                }
                index++;
                AccessTools.Field(typeof(BaseTileset), "_decalPatterns").SetValue(floor, new List<AutoDecalMatchPattern>());
                AccessTools.Field(typeof(BaseTileset), "_notUsableInPreset").SetValue(floor, true);
                descriptors.AddDescriptor(floor.Id, floor);
                all.Add(floor);
                ids.Add(floor.ShortId, floor);
                deck.Floor = floor;
                ImperialDamageTileIds.Add(floor.ShortId);
                string damaged = Array.IndexOf(ChaosCampaignDecks, deck) >= 0 ? "iom_warp_slate_chipped" : "iom_deck_chipped";
                Data.TileTransformation.AddRecord(floor.Id,
                    new TileTransformationRecord { Health = 100, Armor = 10, TilesetToMutateName = damaged });
            }
            RegisterCampaignDeckDressing(descriptors, all, ids);
            RegisterCampaignFloorJoins(descriptors, all, ids);
            RegisterNativePresetTrim(descriptors, all, ids);
        }

        private static void PaintDeckCell(MapCell cell, MapCellType type, BaseTileset tileset)
        {
            cell.Type = type;
            cell.SetTilesetId(1, tileset.ShortId);
            cell.SetTileIndex(1, 0);
        }

    }
}

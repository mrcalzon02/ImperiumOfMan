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
        private enum DeckFloorZone { Field, Secondary, Lane, Trim, Feature }
        private enum DeckFloorJoin { Wall, Doorway, Material, Recess, FieldTrim, SecondaryTrim, SecondaryLane }

        private const short CampaignDirectionalTileBase = 28640;
        private const int CampaignJoinShapeCount = 47;
        private static readonly DeckFloorJoin[] FloorJoins = (DeckFloorJoin[])Enum.GetValues(typeof(DeckFloorJoin));
        private static readonly int[] FloorJoinSlots = BuildFloorJoinSlots();
        private static CampaignSurfaceTileset[,] _campaignFloorJoins;

        // The atlas uses north, east, south, west for both masks and transitions.
        private static readonly (int X, int Y)[] FloorNeighbors = [(0, 1), (1, 0), (0, -1), (-1, 0)];

        private static int[] BuildFloorJoinSlots()
        {
            var slots = new int[256];
            int next = 0;
            for (int mask = 0; mask < slots.Length; mask++)
            {
                bool valid = true;
                for (int corner = 0; corner < 4; corner++)
                {
                    int sides = (1 << corner) | (1 << ((corner + 1) % 4));
                    if ((mask & (16 << corner)) != 0 && (mask & sides) != 0) valid = false;
                }
                slots[mask] = valid ? next++ : -1;
            }
            if (next != CampaignJoinShapeCount) throw new InvalidOperationException("Unexpected campaign floor join count.");
            return slots;
        }

        private static void RegisterCampaignFloorJoins(DescriptorsCollection descriptors, List<BaseTileset> all,
            Dictionary<short, BaseTileset> ids)
        {
            var decks = ImperialCampaignDecks.Concat(ChaosCampaignDecks).ToArray();
            _campaignFloorJoins = new CampaignSurfaceTileset[decks.Length, FloorJoins.Length];
            foreach (var join in FloorJoins)
            {
                var atlas = ReadDamageAtlas("CampaignJoin" + join, CampaignJoinShapeCount, decks.Length * 4);
                for (int deck = 0; deck < decks.Length; deck++)
                {
                    var tileset = ScriptableObject.CreateInstance<CampaignSurfaceTileset>();
                    tileset.name = tileset.Id = "iom_campaign_join_" + decks[deck].Key + "_" + join.ToString().ToLowerInvariant();
                    tileset.SetShortId((short)(CampaignDirectionalTileBase + deck * FloorJoins.Length + (int)join));
                    tileset.Spritesheet = atlas;
                    tileset.Tiles = new TileBase[CampaignJoinShapeCount * 4];
                    AccessTools.Field(typeof(BaseTileset), "_decalPatterns").SetValue(tileset, new List<AutoDecalMatchPattern>());
                    AccessTools.Field(typeof(BaseTileset), "_notUsableInPreset").SetValue(tileset, true);
                    for (int i = 0; i < tileset.Tiles.Length; i++)
                    {
                        var tile = ScriptableObject.CreateInstance<Tile>();
                        tile.sprite = SliceDamageSprite(atlas, deck * tileset.Tiles.Length + i, CampaignJoinShapeCount,
                            new Vector2(0.5f, 0.5f));
                        tile.colliderType = Tile.ColliderType.None;
                        tileset.Tiles[i] = tile;
                    }
                    descriptors.AddDescriptor(tileset.Id, tileset);
                    all.Add(tileset);
                    ids.Add(tileset.ShortId, tileset);
                    ImperialDamageTileIds.Add(tileset.ShortId);
                    var intact = Data.TileTransformation.GetRecord(decks[deck].Floor.Id);
                    Data.TileTransformation.AddRecord(tileset.Id,
                        new TileTransformationRecord { Health = intact.Health, Armor = intact.Armor, TilesetToMutateName = intact.TilesetToMutateName });
                    _campaignFloorJoins[deck, (int)join] = tileset;
                }
            }
        }

    }
}

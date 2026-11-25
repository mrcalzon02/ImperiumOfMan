using System;
using System.Collections.Generic;
using HarmonyLib;
using MGSC;
using UnityEngine;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private static readonly Dictionary<string, string> CampaignMachineArt = new(StringComparer.Ordinal)
        {
            [ItemCategory + "_quest_extinguish_crucible"] = "CampaignForgeController",
            [HollowSignal] = "CampaignHollowRelay",
            [ChaosFaction + "_quest_white_frequency"] = "CampaignChoirTransmitter",
            [UnmakeTheSeal] = "CampaignSealReliquary",
            [ChaosFaction + "_quest_crown_without_sun_alternate_route"] = "CampaignGateControls"
        };

        private static readonly Dictionary<(string Art, Sprite Donor), Sprite> CampaignMachineSprites = new();

        [HarmonyPatch(typeof(MapObstacleFactory), nameof(MapObstacleFactory.Spawn))]
        private static class PaintCampaignObjectiveMachinery
        {
            private static void Postfix(State ____state, MapObstacle __result)
            {
                if (__result?.ObjectiveObstacle == null || __result.DefaultSprite == null) return;

                var raid = ____state.Get<RaidMetadata>();
                if (raid?.RaidType != RaidType.StoryMission || ____state.Get<Missions>().Get(raid) is not Mission mission ||
                    !CampaignMachineArt.TryGetValue(mission.StoryId, out string artwork)) return;

                var donor = __result.DefaultSprite;
                var key = (artwork, donor);
                if (!CampaignMachineSprites.TryGetValue(key, out var sprite))
                {
                    var image = ReadItemSprite(artwork, donor);
                    var pivot = new Vector2(donor.pivot.x / donor.rect.width, donor.pivot.y / donor.rect.height);
                    float pixelsPerUnit = Mathf.Max(image.rect.width / donor.bounds.size.x, image.rect.height / donor.bounds.size.y);
                    sprite = Sprite.Create(image.texture, image.rect, pivot, pixelsPerUnit, 0, SpriteMeshType.FullRect);
                    sprite.name = artwork;
                    CampaignMachineSprites.Add(key, sprite);
                    UnityEngine.Object.Destroy(image);
                }
                __result.DefaultSprite = sprite;
                __result.Renderer.sprite = sprite;
            }
        }
    }
}

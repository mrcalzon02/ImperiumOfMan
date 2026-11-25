using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MGSC;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private const string CampaignDeckKey = "iom_campaign_deck";
        private const string CampaignDeckFloorKey = "iom_campaign_deck_floor";

        private sealed class CampaignDeck(string key, string tile)
        {
            internal readonly string Key = key;
            internal readonly string Tile = tile;
            internal SurfaceTileset Floor;
        }

        private static readonly CampaignDeck[] ImperialCampaignDecks =
        [
            new("caloris_levy", "CalorisLevy"),
            new("counterfeit_orbit", "CounterfeitOrbit"),
            new("goethe_deadlock", "GoetheDeadlock"),
            new("extinguish_crucible", "ExtinguishCrucible"),
            new("discovery_injunction", "DiscoveryInjunction"),
            new("hollow_signal", "SilenceHollowSignal"),
            new("cinder_basin", "SecureBasin"),
            new("last_bonded_shipment", "LastBondedShipment"),
            new("prime_the_foundry", "PrimeFoundry"),
            new("one_standard", "UnderOneStandard")
        ];

        private static readonly CampaignDeck[] ChaosCampaignDecks =
        [
            new("white_frequency", "WhiteFrequency"),
            new("scales_of_passage", "ScalesPassage"),
            new("no_salvage_rights", "NoSalvageRights"),
            new("furnace_of_ur", "FurnaceUr"),
            new("the_right_to_pass", "RightToPass"),
            new("prism_rent", "PrismRent"),
            new("starless_oath", "StarlessOath"),
            new("hound_road", "HoundRoad"),
            new("unmake_the_seal", "UnmakeSeal"),
            new("crown_without_sun", "CrownWithoutSun"),
            new("crown_without_sun_alternate_route", "HoldEbonCrown")
        ];

        private static readonly Dictionary<string, CampaignDeck> CampaignDecks = ImperialCampaignDecks
            .Select(deck => (Id: ItemCategory + "_quest_" + deck.Key, Deck: deck))
            .Concat(ChaosCampaignDecks.Select(deck => (Id: ChaosFaction + "_quest_" + deck.Key, Deck: deck)))
            .ToDictionary(entry => entry.Id, entry => entry.Deck, StringComparer.Ordinal);

        private static void ApplyCampaignDecks(Mission mission)
        {
            if (!CampaignDecks.ContainsKey(mission.StoryId)) return;

            string last = mission.WorldStructure.FindLastStage().ID;
            foreach (var entry in mission.LocationPlans)
            {
                string id = entry.Key;
                if (!id.StartsWith("stage", StringComparison.Ordinal)) continue;

                var plan = entry.Value;
                plan.CustomParameters[CampaignDeckKey] = mission.StoryId;
                // The layout on an intermediate floor is offset from the entry and objective decks.
                plan.CustomParameters[CampaignDeckFloorKey] = id == last ? "0" : id.Substring("stage".Length);
                ApplyCampaignEndgamePopulation(plan);
                if (id == "stage1" && TryGetCampaignAccess(mission.StoryId, out _))
                {
                    string item = mission.StoryId + CampaignAccessSuffix;
                    if (!plan.AdditionalItemIdsDrop.Contains(item)) plan.AdditionalItemIdsDrop.Add(item);
                }
            }
        }

        private static bool TryGetCampaignDeck(DungeonGenerationPlan plan, out CampaignDeck deck, out int floor)
        {
            deck = null;
            floor = 0;
            return plan.GetParameter(CampaignDeckKey, out string id) && CampaignDecks.TryGetValue(id, out deck) &&
                plan.GetParameter(CampaignDeckFloorKey, out string value) &&
                int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out floor);
        }
    }
}

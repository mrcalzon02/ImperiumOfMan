using System;
using System.Collections.Generic;
using System.Linq;
using MGSC;
using UnityEngine;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        // Research consumes real samples, including samples produced only after
        // the corresponding rank is installed. Provide paid/contract acquisition
        // before that rank rather than weakening research or teaching recipes.
        // Faction tech is independent of the ship's occult research rank.
        private static readonly (string Item, string Faction, string Supplier, int Tech, int Rank)[] OccultResearchSamples =
        [
            ("quasisamples", ChaosFaction, "iomHollowSignalDepot", 1, 1),
            (NativeGavvakh, ChaosFaction, "iomHollowSignalDepot", 1, 2),
            (NativeGavvakh, TezctlanFaction, null, 1, 2),
            ("demonic_eye", ChaosFaction, "iomOssuaryWorks", 2, 3),
            ("mercury_glass", ChaosFaction, "iomOssuaryWorks", 2, 3),
            ("moon_guts", ChaosFaction, "iomOssuaryWorks", 3, 4),
            ("ron_blood", ChaosFaction, "iomOssuaryWorks", 3, 4),
            ("mars_skin", "SheduThousand", null, 4, 5),
            ("cattaram_geode", ChaosFaction, null, 4, 5),
            ("jupiter_mushrooms", ChaosFaction, null, 4, 5),
            ("quasi_repair_kit", ChaosFaction, null, 5, 6),
            ("mars_cyborg_head", "SheduThousand", null, 5, 6)
        ];

        private static readonly Dictionary<string, (string Faction, string Supplier, int Tier, bool Dedicated)> OccultPatternIssuers =
            new(StringComparer.Ordinal);

        private static void RecordOccultPatternIssuer(string chip, string faction, string supplier, int tier, bool dedicated)
        {
            // Several outputs can share one legacy tier chip.
            if (!OccultPatternIssuers.ContainsKey(chip))
                OccultPatternIssuers.Add(chip, (faction, supplier, tier, dedicated));
        }

        private static bool IsTrackedOccultPattern(string chip) =>
            chip != null && OccultPatternIssuers.ContainsKey(chip);

        private static bool IsFactionBoundOccultPattern(string chip) =>
            chip != null && OccultPatternIssuers.TryGetValue(chip, out var issuer) &&
            issuer.Dedicated && issuer.Faction != "ChurchGannix";

        private static string OccultIssuerName(string faction)
        {
            string fallback = faction switch
            {
                ChaosFaction => "Chaos Undivided",
                ItemCategory => "Imperium of Man",
                TezctlanFaction => "Tezctlan",
                "XiomaraMasks" => "Feathered Masks",
                "SheduThousand" => "Shedu's Thousand",
                "ChurchGannix" => "Church of Gannix",
                _ => faction
            };
            return CraftingLocalizedName("faction." + faction + ".name", fallback);
        }

        private static void RegisterOccultResearchSampleRewards()
        {
            // RegisterRegionalSeedReward preserves earlier native entries and
            // ignores absent installed items. Reuse its native reward budget.
            foreach (var sample in OccultResearchSamples)
                RegisterRegionalSeedReward(sample.Faction, sample.Tech, sample.Item);
        }

        private static string OccultResearchSampleHint(string id)
        {
            var sources = OccultResearchSamples.Where(sample => sample.Item == id &&
                Data.Items.GetRecord(sample.Item) != null).ToArray();
            if (sources.Length == 0) return null;
            return "Research sample: " + string.Join(" / ", sources.Select(sample =>
                OccultIssuerName(sample.Faction) + " T" + sample.Tech)) +
                ". Contracts or owned stations.";
        }

        // Bounded, once per loaded campaign; not a per-frame market scan.
        // Stock means an item exists in station storage, not a guaranteed trade
        // quote or that the player has sufficient currency/reputation to buy it.
        private static void AuditOccultSupplierCoverage(Stations stations, Factions factions)
        {
            if (stations == null || factions == null) return;
            try
            {
                foreach (var group in OccultPatternIssuers.GroupBy(entry => entry.Value.Faction))
                {
                    var owner = factions.Get(group.Key, logMissing: false);
                    bool enabled = owner != null && factions.IsEnabledFaction(owner);
                    bool tradable = enabled && owner.Record.CanBeTraded;
                    var owned = stations.Values.Where(station => station.OwnerFactionId == group.Key &&
                        !string.IsNullOrEmpty(station.SpaceObjectId)).ToArray();
                    int tech = owner?.CurrentTechLevel ?? 0;
                    var eligible = owner == null || !enabled ? new HashSet<string>(StringComparer.Ordinal) :
                        new HashSet<string>(Data.FactionDrop.GetTradeItems(owner,
                            FactionDropCollection.TradeItemsFor.Chips).SelectMany(record => record.ContentIds),
                            StringComparer.Ordinal);
                    var stockMissing = new List<string>();
                    int stocked = 0;
                    foreach (var entry in group)
                    {
                        bool available = tradable && owned.Any(station =>
                            station.InternalStorage.CountItems(entry.Key) > 0);
                        if (available) stocked++;
                        else if (tradable && tech >= entry.Value.Tier) stockMissing.Add(entry.Key);
                    }
                    Debug.Log("[Cvar_ImperiumOfMan] Occult supplier coverage | faction=" + group.Key +
                        " | enabled=" + enabled + " | tradable=" + tradable + " | tech=" + tech +
                        " | owned_stations=" + owned.Length + " | declared_patterns=" + group.Count() +
                        " | eligible_contract_patterns=" + group.Count(entry => eligible.Contains(entry.Key)) +
                        " | stocked_patterns=" + stocked +
                        " | eligible_stock_missing=" + string.Join(",", stockMissing.Take(16)));
                }

                foreach (var sample in OccultResearchSamples)
                {
                    var owner = factions.Get(sample.Faction, logMissing: false);
                    if (owner == null || !factions.IsEnabledFaction(owner) ||
                        owner.Record.CanBeTraded != true || owner.CurrentTechLevel < sample.Tech)
                        continue;
                    int stocked = stations.Values.Count(station => station.OwnerFactionId == sample.Faction &&
                        !string.IsNullOrEmpty(station.SpaceObjectId) && station.InternalStorage.CountItems(sample.Item) > 0);
                    Debug.Log("[Cvar_ImperiumOfMan] Occult research sample | rank=" + sample.Rank +
                        " | item=" + sample.Item + " | faction=" + sample.Faction +
                        " | stocked_stations=" + stocked);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[Cvar_ImperiumOfMan] Occult supplier audit unavailable: " + ex.Message);
            }
        }
    }
}

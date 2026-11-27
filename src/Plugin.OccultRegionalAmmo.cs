using System;
using System.Collections.Generic;
using System.Linq;
using MGSC;
using UnityEngine;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private const string GannixConcentrate = "iom_occult_gannix_concentrate";
        private const string GannixLivingCell = "iom_occult_gannix_living_cell";
        private const string DuggurMatrix = "iom_occult_duggur_matrix";
        private const string DuggurNailBlank = "iom_occult_duggur_nail_blank";
        private const string IgvasAmmo = "quasi_basic_ammo";
        private const string NailsOfPainAmmo = "quasi_bolts_ammo";
        private const string TezctlanFaction = "Tezctlan";

        private sealed class RegionalOccultMaterial(string id, string donor, string name,
            string description, float price, int hours, int batch, params ItemQuantity[] inputs)
        {
            public readonly string Id = id;
            public readonly string Donor = donor;
            public readonly string Name = name;
            public readonly string Description = description;
            public readonly float Price = price;
            public readonly int Hours = hours;
            public readonly int Batch = batch;
            public readonly ItemQuantity[] Inputs = inputs;
        }

        private sealed class RegionalOccultAmmo(string output, string title, string region,
            string faction, string factionName, string supplier, string rank, int tier,
            RegionalOccultMaterial first, RegionalOccultMaterial second)
        {
            public readonly string Output = output;
            public readonly string Title = title;
            public readonly string Region = region;
            public readonly string Faction = faction;
            public readonly string FactionName = factionName;
            public readonly string Supplier = supplier;
            public readonly string Rank = rank;
            public readonly int Tier = tier;
            public readonly RegionalOccultMaterial[] Materials = [first, second];
        }

        // Small, complete prototype families. Counts are effective batch counts,
        // not IndustryRecipe's legacy doubled/halved arguments. Reuse the actual
        // native donor descriptor art provisionally; no fabricated new ammo record.
        private static readonly RegionalOccultAmmo[] RegionalOccultAmmoChains =
        [
            new(IgvasAmmo, "Igvas", "Gannix", TezctlanFaction, "Tezctlan", null, OccultRankThree, 3,
                new(GannixConcentrate, "venus_guts", "Combat Gavvakh Concentrate",
                    "Gavvakh enriched with living Gannixian tissue.", 6, 4, 3,
                    new(NativeGavvakh, 1), new("venus_guts", 1)),
                new(GannixLivingCell, "quasi_venus_feather", "Hardened Living Cells",
                    "Concentrated living stock prepared for Igvas.", 8, 4, 4,
                    new(GannixConcentrate, 2), new("quasi_venus_feather", 1))),
            new(NailsOfPainAmmo, "Nails of Pain", "Duggur", ChaosFaction, "Chaos Undivided",
                "iomOssuaryWorks", OccultRankFour, 4,
                new(DuggurMatrix, "moon_armor_plates", "Calcified Lunar Matrix",
                    "A bound mineral-organic stock made with lunar tissue.", 7, 4, 3,
                    new("moon_guts", 2), new("human_rib", 1), new(SufferingEssence, 1)),
                new(DuggurNailBlank, "moon_weapon_parts", "Pain-bound Nail Blanks",
                    "Lunar matrix shaped and charged for occult ammunition.", 9, 4, 4,
                    new(DuggurMatrix, 2), new(NativeGavvakh, 1)))
        ];

        private static readonly HashSet<string> RegionalOccultMaterials = new(StringComparer.Ordinal);
        private static readonly HashSet<string> ActiveRegionalAmmoChains = new(StringComparer.Ordinal);
        private static readonly Dictionary<string, (string Id, string Name)> RegionalOccultPatternFactions = new(StringComparer.Ordinal);

        private static bool IsOwnedRegionalOccultMaterial(string id) => RegionalOccultMaterials.Contains(id);

        private static bool TryRegisterRegionalOccultAmmo(string output)
        {
            var chain = Array.Find(RegionalOccultAmmoChains, candidate => candidate.Output == output);
            if (chain == null) return false;

            // No half-registered chain if an installed version/another mod owns
            // the finished recipe, if a native source is absent, or if one of our
            // prospective IDs has already been registered.
            bool ExistingRecipe(string id) => IndustryBatches.ContainsKey(id) ||
                Data.ProduceReceipts.Any(receipt => receipt.Id == id || receipt.OutputItem == id);
            if (ExistingRecipe(output) || Data.Items.GetSimpleRecord<AmmoRecord>(output) == null)
            {
                Debug.Log("[Cvar_ImperiumOfMan] Regional occult prototype skipped (existing recipe or missing ammo): " + output);
                return false;
            }
            var planned = new HashSet<string>(StringComparer.Ordinal);
            foreach (var material in chain.Materials)
            {
                var donor = Data.Items.GetSimpleRecord<ItemRecord>(material.Donor);
                if (donor?.ItemDesc?.Icon == null ||
                    Data.Items.GetRecord(material.Id) != null || ExistingRecipe(material.Id) ||
                    RegionalOccultMaterials.Contains(material.Id) ||
                    Data.Descriptors["trash"].TryGetDescriptor(material.Id, out _) ||
                    material.Inputs.Any(input => input.Count <= 0 ||
                        (!planned.Contains(input.ItemId) &&
                         input.ItemId != SufferingEssence &&
                         Data.Items.GetRecord(input.ItemId) == null)))
                {
                    Debug.LogWarning("[Cvar_ImperiumOfMan] Regional occult prototype preflight failed for " + material.Id);
                    return false;
                }
                planned.Add(material.Id);
            }
            if (!AddedOccultRecipes.Contains(SufferingEssence) &&
                chain.Materials.Any(material => material.Inputs.Any(input => input.ItemId == SufferingEssence)))
                return false;

            foreach (var material in chain.Materials)
            {
                AddMaterial(material.Id, material.Donor, material.Name, material.Description,
                    material.Price, chain.Tier, category: chain.Faction, retainNativeArtwork: true);
                RegionalOccultMaterials.Add(material.Id);
                OccultMaterialOutputs.Add(material.Id);
                RegisterOccultRecipe(material.Id, chain.Rank, material.Hours, material.Batch, material.Inputs);
            }
            RegisterOccultRecipe(chain.Output, chain.Rank, 3, 12,
                new ItemQuantity(chain.Materials[1].Id, 4));
            if (!AddedOccultRecipes.Contains(chain.Output) ||
                chain.Materials.Any(material => !AddedOccultRecipes.Contains(material.Id)))
                throw new InvalidOperationException("Incomplete regional occult recipe registration for " + chain.Output);

            ActiveRegionalAmmoChains.Add(chain.Output);
            return true;
        }

        private static void RegisterRegionalOccultPatterns()
        {
            // Single-pattern prerequisites prevent the new chains from being
            // locked entirely behind a roll in a broad random equipment catalog.
            // The existing mixed chips keep their IDs, contents and availability.
            foreach (var chain in RegionalOccultAmmoChains.Where(chain => ActiveRegionalAmmoChains.Contains(chain.Output)))
            {
                string region = chain.Region.ToLowerInvariant();
                foreach (var material in chain.Materials)
                    AddRegionalPattern(region + "_" + material.Id.Substring("iom_occult_".Length + region.Length + 1),
                        material.Name + " Pattern", chain.Faction, chain.FactionName, chain.Supplier, chain.Tier, material.Id);
                AddRegionalPattern(region + "_ammo_finish", chain.Title + " Pattern", chain.Faction, chain.FactionName,
                    chain.Supplier, chain.Tier, chain.Output);
                if (chain.Output == IgvasAmmo)
                {
                    AddRegionalPattern("tezctlan_tissue", "Gannix Tissue Culture Pattern", chain.Faction, chain.FactionName, null, 3, "venus_guts");
                    AddRegionalPattern("tezctlan_feather", "Gannix Living Fiber Pattern", chain.Faction, chain.FactionName, null, 3, "quasi_venus_feather");
                    RegisterRegionalFoundationPatterns("tezctlan", chain.Faction, chain.FactionName, null);
                    RegisterRegionalSeedReward(chain.Faction, 2, "venus_guts");
                    RegisterRegionalSeedReward(chain.Faction, 2, "quasi_venus_feather");
                }
                else if (chain.Output == NailsOfPainAmmo)
                {
                    AddRegionalPattern("duggur_tissue", "Duggur Tissue Pattern", chain.Faction, chain.FactionName, chain.Supplier, 4, "moon_guts");
                    RegisterRegionalFoundationPatterns("chaos", chain.Faction, chain.FactionName, "iomHollowSignalDepot");
                    RegisterRegionalSeedReward(chain.Faction, 3, "moon_guts");
                }
            }
        }

        private static void RegisterRegionalFoundationPatterns(string key, string faction, string name, string supplier)
        {
            AddRegionalPattern(key + "_clone", "Raw Clone Pattern", faction, name, supplier, 1, RawClone);
            AddRegionalPattern(key + "_imprint", "Torment Containment Pattern", faction, name, supplier, 1, TormentImprint);
            AddRegionalPattern(key + "_essence", "Suffering Separation Pattern", faction, name, supplier, 2, SufferingEssence);
            AddRegionalPattern(key + "_gavvakh", "Gavvakh Condensation Pattern", faction, name, supplier, 2, NativeGavvakh);
        }

        private static void AddRegionalPattern(string key, string title, string faction, string factionName,
            string supplier, int tier, string output)
        {
            if (!AddedOccultRecipes.Contains(output)) return;
            AddOccultPatternPool(key, title, faction, supplier, tier, output);
            RegionalOccultPatternFactions.Add("iom_occult_" + key + "_pattern_chip", (faction, factionName));
        }

        private static void RegisterRegionalSeedReward(string faction, int tier, string itemId)
        {
            // Only native regional feedstock, not finished ammo or equipment.
            // Keep existing earlier reward entries; do not double their weight.
            var item = Data.Items.GetSimpleRecord<ItemRecord>(itemId);
            if (item == null) return;
            Registrations.Add(() =>
            {
                string key = faction + "_rewardConsumables";
                if (Data.FactionDrop._recordsByFactions.TryGetValue(key, out var tiers) &&
                    tiers.Any(entry => entry.Key <= tier &&
                        entry.Value.Any(record => record.ContentIds.Contains(itemId))))
                    return;
                Data.FactionDrop.AddRecord(key, new ContentDropRecord
                {
                    ContentIds = [itemId], TechLevel = tier, Weight = 2, Points = item.Price
                });
            });
        }

        private static string RegionalOccultPatternSource(string chip)
        {
            string name = CraftingItemName(chip);
            return RegionalOccultPatternFactions.TryGetValue(chip, out var faction)
                ? name + " (" + CraftingLocalizedName("faction." + faction.Id + ".name", faction.Name) + " contracts / trade)" : name;
        }

        private static string RegionalOccultRecipeBranch(string output)
        {
            if (ActiveCloneVatProducts.Contains(output)) return "Clone-vat organic salvage";
            foreach (var chain in RegionalOccultAmmoChains)
                if (ActiveRegionalAmmoChains.Contains(chain.Output) &&
                    (chain.Output == output || chain.Materials.Any(material => material.Id == output)))
                    return chain.Region;
            return null;
        }

        private static bool IsOccultPatternChip(string chip) =>
            chip != null && RegionalOccultPatternFactions.ContainsKey(chip);

        private static void AuditRegionalOccultRegistration()
        {
            foreach (var entry in RegionalOccultPatternFactions)
            {
                string chip = entry.Key;
                string table = entry.Value.Id + "_rewardChips";
                var disk = Data.Items.GetSimpleRecord<DatadiskRecord>(chip);
                bool singleton = disk?.UnlockIds?.Count == 1;
                bool registered = Data.FactionDrop._recordsByFactions.TryGetValue(table, out var tiers) &&
                    tiers.Values.Any(records => records.Any(record => record.ContentIds.Contains(chip)));
                Debug.Log("[Cvar_ImperiumOfMan] Regional pattern audit " + chip +
                    " | faction=" + entry.Value.Id + " | table=" + table +
                    " | singleton=" + singleton + " | rewardRegistered=" + registered +
                    " | pattern=" + (singleton ? disk.UnlockIds[0] : "unverified"));
                if (!singleton || !registered)
                    Debug.LogWarning("[Cvar_ImperiumOfMan] Regional pattern needs reward/knowledge review: " + chip);
            }

            foreach (var chain in RegionalOccultAmmoChains.Where(chain => ActiveRegionalAmmoChains.Contains(chain.Output)))
            {
                string table = chain.Faction + "_rewardChips";
                var faction = Data.Factions.GetRecord(chain.Faction);
                string[] outputs = [.. chain.Materials.Select(material => material.Id), chain.Output];
                foreach (string output in outputs)
                {
                    var receipt = Data.ProduceReceipts.Single(recipe => recipe.Id == output && recipe.OutputItem == output);
                    var sources = OccultPatternSources[output].Where(RegionalOccultPatternFactions.ContainsKey).ToArray();
                    // Exact registered table keys and chip content prove registration,
                    // not actual reward selection/payout in an accepted contract.
                    string[] registered = sources.Where(chip =>
                        Data.Items.GetSimpleRecord<DatadiskRecord>(chip)?.UnlockIds.Contains(output) == true &&
                        Data.FactionDrop._recordsByFactions.TryGetValue(table, out var tiers) &&
                        tiers.Values.Any(records => records.Any(record => record.ContentIds.Contains(chip)))).ToArray();
                    if (registered.Length == 0)
                        Debug.LogWarning("[Cvar_ImperiumOfMan] Regional occult reward registration missing: " + output);
                    Debug.Log("[Cvar_ImperiumOfMan] Regional occult audit " + chain.Region + " | " + output +
                        " | batch=" + IndustryBatches[output] + " | hours=" + receipt.ProduceTimeInHours +
                        " | ingredients=" + string.Join(", ", receipt.RequiredItems.Select(input => input.ItemId + " x" + input.Count)) +
                        " | faction=" + chain.Faction + " | factionPresent=" + (faction != null) +
                        " | tradable=" + (faction?.CanBeTraded == true) + " | generalRewards=" + (faction?.UseGeneralRewards == true) +
                        " | rewardTable=" + table + " | chips=" + string.Join(", ", registered));
                }
            }
        }
    }
}

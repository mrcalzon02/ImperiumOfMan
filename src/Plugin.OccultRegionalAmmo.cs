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
        private const string AngerlingsAmmo = "quasi_darts_ammo";
        private const string EleriumAmmo = "quasi_energy_ammo";
        private const string EannisAmmo = "mars_gas_ammo";
        private const string FlaurosAmmo = "flauros_ammo";
        private const string GannixChitin = "iom_occult_gannix_chitin";
        private const string GannixLivingDart = "iom_occult_gannix_living_dart";
        private const string ShartamakhumSubstrate = "iom_occult_shartamakhum_substrate";
        private const string ShartamakhumCell = "iom_occult_shartamakhum_cell";
        private const string UrConcentrate = "iom_occult_ur_concentrate";
        private const string UrRedfireStock = "iom_occult_ur_redfire_stock";
        private const string FlaurosCulture = "iom_occult_flauros_spore_culture";
        private const string FlaurosBrood = "iom_occult_flauros_brood_substrate";

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
            RegionalOccultMaterial first, RegionalOccultMaterial second, int finishHours = 3)
        {
            public readonly string Output = output;
            public readonly string Title = title;
            public readonly string Region = region;
            public readonly string Faction = faction;
            public readonly string FactionName = factionName;
            public readonly string Supplier = supplier;
            public readonly string Rank = rank;
            public readonly int Tier = tier;
            public readonly int FinishHours = finishHours;
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
                    new(DuggurMatrix, 2), new(NativeGavvakh, 1))),
            new(AngerlingsAmmo, "Angerlings", "Gannix", "XiomaraMasks", "Feathered Masks", null, OccultRankThree, 3,
                new(GannixChitin, "venus_melee_parts", "Gannix Chitin Stock",
                    "Living Gannixian structural stock.", 7, 4, 3,
                    new("venus_melee_parts", 1), new(SufferingEssence, 1)),
                new(GannixLivingDart, "quasi_venus_feather", "Barbed Living Dart Blanks",
                    "Prepared living dart stock for Angerlings.", 9, 4, 4,
                    new(GannixChitin, 2), new("quasi_venus_feather", 1))),
            new(EleriumAmmo, "Elerium", "Shartamakhum", ChaosFaction, "Chaos Undivided",
                null, OccultRankThree, 3,
                new(ShartamakhumSubstrate, "mercury_glass", "Resonant Mercury Substrate",
                    "An occult substrate formed from Mercury glass and scales.", 8, 5, 3,
                    new("mercury_glass", 1), new("mercury_scales", 1), new(SufferingEssence, 1)),
                new(ShartamakhumCell, "mercury_scales", "Resonant Cell Stock",
                    "Bound energy-bearing stock for Elerium.", 10, 4, 4,
                    new(ShartamakhumSubstrate, 2), new(NativeGavvakh, 1)), 4),
            new(EannisAmmo, "Eannis Redfire", "Ur", "SheduThousand", "Shedu's Thousand",
                null, OccultRankFive, 5,
                new(UrConcentrate, "mars_skin", "Eannis Concentrate",
                    "Living forge stock concentrated for Redfire.", 9, 5, 3,
                    new("mars_skin", 1), new(NativeGavvakh, 1)),
                new(UrRedfireStock, "mars_gear", "Contained Redfire Stock",
                    "Ur forge components prepared to contain Eannis.", 11, 4, 4,
                    new(UrConcentrate, 2), new("mars_gear", 1)), 4),
            new(FlaurosAmmo, "Flies", "Flauros", ChaosFaction, "Chaos Undivided",
                null, OccultRankFive, 5,
                new(FlaurosCulture, "jupiter_mushrooms", "Flauros Spore Culture",
                    "Living spore stock from Flauros.", 8, 5, 3,
                    new("jupiter_mushrooms", 1), new("jupiter_guts", 1), new(NativeGavvakh, 1)),
                new(FlaurosBrood, "jupiter_grass", "Viable Brood Substrate",
                    "Cultured brood stock for native Flauros ammunition.", 10, 4, 4,
                    new(FlaurosCulture, 2), new("jupiter_grass", 1)), 4)
        ];

        // One acquisition ledger drives pattern registration, native rewards,
        // station restock and payout observation. Preserve the original prototype
        // chip keys, reward tiers and stock targets for already learned saves.
        private static readonly (string Chain, string Item, string Key, string Title, int SeedTier, int Stock)[] RegionalOccultSeeds =
        [
            (IgvasAmmo, "venus_guts", "tezctlan_tissue", "Gannix Tissue Culture Pattern", 2, 1),
            (IgvasAmmo, "quasi_venus_feather", "tezctlan_feather", "Gannix Living Fiber Pattern", 2, 1),
            (NailsOfPainAmmo, "moon_guts", "duggur_tissue", "Duggur Tissue Pattern", 3, 2),
            (AngerlingsAmmo, "venus_melee_parts", "xiomara_chitin_feedstock", "Gannix Structural Stock Pattern", 2, 1),
            (AngerlingsAmmo, "quasi_venus_feather", "xiomara_feather", "Gannix Living Fiber Pattern", 2, 1),
            (EleriumAmmo, "mercury_glass", "shartamakhum_glass", "Mercury Glass Pattern", 2, 1),
            (EleriumAmmo, "mercury_scales", "shartamakhum_scales", "Mercury Scales Pattern", 2, 1),
            (EannisAmmo, "mars_skin", "shedu_skin", "Ur Living Skin Pattern", 4, 1),
            (EannisAmmo, "mars_gear", "shedu_gear", "Ur Forge Gear Pattern", 4, 1),
            (FlaurosAmmo, "jupiter_mushrooms", "flauros_mushrooms", "Flauros Fungal Stock Pattern", 4, 1),
            (FlaurosAmmo, "jupiter_guts", "flauros_guts", "Flauros Tissue Pattern", 4, 1),
            (FlaurosAmmo, "jupiter_grass", "flauros_grass", "Flauros Living Fiber Pattern", 4, 1)
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
            RegisterOccultRecipe(chain.Output, chain.Rank, chain.FinishHours, 12,
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
                // The original Gannix/Duggur finish keys are save-stable. The
                // second Gannix ammunition family must have a different key.
                string finishKey = chain.Output == AngerlingsAmmo ? "gannix_angerlings_finish" : region + "_ammo_finish";
                AddRegionalPattern(finishKey, chain.Title + " Pattern", chain.Faction, chain.FactionName,
                    chain.Supplier, chain.Tier, chain.Output);
                foreach (var seed in RegionalOccultSeeds.Where(seed => seed.Chain == chain.Output))
                {
                    AddRegionalPattern(seed.Key, seed.Title, chain.Faction, chain.FactionName,
                        chain.Supplier, chain.Tier, seed.Item);
                    RegisterRegionalSeedReward(chain.Faction, seed.SeedTier, seed.Item);
                }
            }

            if (ActiveRegionalAmmoChains.Contains(IgvasAmmo))
                RegisterRegionalFoundationPatterns("tezctlan", TezctlanFaction, "Tezctlan", null);
            if (ActiveRegionalAmmoChains.Contains(AngerlingsAmmo))
                RegisterRegionalFoundationPatterns("xiomara", "XiomaraMasks", "Feathered Masks", null);
            if (ActiveRegionalAmmoChains.Contains(EannisAmmo))
                RegisterRegionalFoundationPatterns("shedu", "SheduThousand", "Shedu's Thousand", null);
            if (RegionalOccultAmmoChains.Any(chain => chain.Faction == ChaosFaction &&
                ActiveRegionalAmmoChains.Contains(chain.Output)))
                RegisterRegionalFoundationPatterns("chaos", ChaosFaction, "Chaos Undivided", "iomHollowSignalDepot");
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
            if (ActiveChaosOccultStocks.Contains(output) || ActiveChaosAmmoChains.Contains(output))
                return "Chaos Undivided · Soulforge";
            string equipmentRegion = OccultEquipmentMaterialRegion(output);
            if (equipmentRegion != null) return equipmentRegion;
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
            AuditOccultEquipmentMaterials();
            AuditChaosOccultStock();
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

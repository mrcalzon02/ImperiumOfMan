using System;
using System.Collections.Generic;
using System.Linq;
using MGSC;
using UnityEngine;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private const string GannixLivingWeave = "iom_occult_gannix_living_weave";
        private const string AggaOcularFocus = "iom_occult_agga_ocular_focus";
        private const string RonBloodConcentrate = "iom_occult_ron_blood_concentrate";
        private const string RonBindingWeave = "iom_occult_ron_binding_weave";
        private const string ScrivnusRelicCore = "iom_occult_scrivnus_relic_core";
        private const string UrForgeLattice = "iom_occult_ur_forge_lattice";
        private const string CattaramCrystalCore = "iom_occult_cattaram_crystal_core";

        private sealed class OccultEquipmentMaterial(string region, string faction, string factionName,
            string rank, int tier, RegionalOccultMaterial stock)
        {
            public readonly string Region = region;
            public readonly string Faction = faction;
            public readonly string FactionName = factionName;
            public readonly string Rank = rank;
            public readonly int Tier = tier;
            public readonly RegionalOccultMaterial Stock = stock;
        }

        // Registration order follows the research recipe graph. All counts are
        // effective batches; these materials are not native equipment records.
        private static readonly OccultEquipmentMaterial[] OccultEquipmentMaterials =
        [
            new("Agga", ChaosFaction, "Chaos Undivided", OccultRankThree, 3,
                new(AggaOcularFocus, "demonic_eye", "Agga Ocular Focus",
                    "A concentrated ocular manifestation held in a resonant focus.", 9, 5, 2,
                    new("demonic_eye", 1), new(SufferingEssence, 1))),
            new("Gannix", TezctlanFaction, "Tezctlan", OccultRankThree, 3,
                new(GannixLivingWeave, "quasi_venus_rags", "Gannix Living Weave",
                    "Living fiber stock prepared for binding and graft assembly.", 8, 5, 3,
                    new("quasi_venus_rags", 1), new("quasi_venus_feather", 1), new(NativeGavvakh, 1))),
            new("Ron", ChaosFaction, "Chaos Undivided", OccultRankFour, 4,
                new(RonBloodConcentrate, "ron_blood", "Ron Blood Concentrate",
                    "Concentrated bloodwork stock for medicine and binding.", 8, 4, 3,
                    new("ron_blood", 2), new(SufferingEssence, 1))),
            new("Ron", ChaosFaction, "Chaos Undivided", OccultRankFour, 4,
                new(RonBindingWeave, "ron_tentacle", "Ron Binding Weave",
                    "Living fiber stabilized in concentrated Ron bloodwork.", 10, 5, 2,
                    new("ron_tentacle", 1), new(RonBloodConcentrate, 1))),
            new("Scrivnus", ChaosFaction, "Chaos Undivided", OccultRankFour, 4,
                new(ScrivnusRelicCore, "demonic_eye", "Scrivnus Relic Core",
                    "A bound relic stock for Scrivnus fabrication.", 12, 6, 2,
                    new(AggaOcularFocus, 1), new("human_rib", 1), new(SufferingEssence, 1))),
            new("Ur", "SheduThousand", "Shedu's Thousand", OccultRankFive, 5,
                new(UrForgeLattice, "mars_gear", "Ur Living Forge Lattice",
                    "A living mechanism matrix for Ur body replacements.", 12, 6, 2,
                    new("mars_gear", 1), new("mars_skin", 1), new(NativeGavvakh, 1))),
            new("Cattaram", ChaosFaction, "Chaos Undivided", OccultRankFive, 5,
                new(CattaramCrystalCore, "cattaram_stone", "Cattaram Crystal Core",
                    "A refined crystal stock for occult assembly.", 12, 6, 2,
                    new("cattaram_stone", 1), new(SufferingEssence, 1)))
        ];

        private static readonly HashSet<string> ActiveOccultEquipmentMaterials = new(StringComparer.Ordinal);

        private static bool IsRegisteredOccultStock(string id) =>
            RegionalOccultMaterials.Contains(id) && AddedOccultRecipes.Contains(id);

        // Compatibility fallback is chosen at registration, never on a shortage
        // of ingredients. Keep original recipes usable if a dependency is absent.
        private static ItemQuantity OccultStockOr(string id, string fallback, int count = 1) =>
            new(IsRegisteredOccultStock(id) ? id : fallback, count);

        private static bool IsAvailableOccultStockInput(string id) =>
            id == SufferingEssence ? AddedOccultRecipes.Contains(id) :
            RegionalOccultMaterials.Contains(id) ? IsRegisteredOccultStock(id) :
            Data.Items.GetRecord(id) != null;

        private static void RegisterOccultEquipmentMaterials(string region)
        {
            foreach (var definition in OccultEquipmentMaterials.Where(entry => entry.Region == region))
            {
                var stock = definition.Stock;
                var donor = Data.Items.GetSimpleRecord<ItemRecord>(stock.Donor);
                if (stock.Batch <= 0 || stock.Hours <= 0 || donor?.ItemDesc?.Icon == null ||
                    Data.Items.GetRecord(stock.Id) != null ||
                    IndustryBatches.ContainsKey(stock.Id) || RegionalOccultMaterials.Contains(stock.Id) ||
                    Data.ProduceReceipts.Any(receipt => receipt.Id == stock.Id || receipt.OutputItem == stock.Id) ||
                    Data.Descriptors["trash"].TryGetDescriptor(stock.Id, out _) ||
                    stock.Inputs.Any(input => input.Count <= 0 || !IsAvailableOccultStockInput(input.ItemId)))
                {
                    Debug.LogWarning("[Cvar_ImperiumOfMan] Occult equipment stock skipped: " + stock.Id);
                    continue;
                }

                AddMaterial(stock.Id, stock.Donor, stock.Name, stock.Description, stock.Price,
                    definition.Tier, category: definition.Faction, retainNativeArtwork: true);
                RegionalOccultMaterials.Add(stock.Id);
                OccultMaterialOutputs.Add(stock.Id);
                RegisterOccultRecipe(stock.Id, definition.Rank, stock.Hours, stock.Batch, stock.Inputs);
                if (!AddedOccultRecipes.Contains(stock.Id))
                    throw new InvalidOperationException("Incomplete occult equipment material registration: " + stock.Id);
                ActiveOccultEquipmentMaterials.Add(stock.Id);
            }
        }

        private static string OccultEquipmentMaterialRegion(string output)
        {
            if (!ActiveOccultEquipmentMaterials.Contains(output)) return null;
            return Array.Find(OccultEquipmentMaterials, entry => entry.Stock.Id == output)?.Region;
        }

        private static void RegisterOccultEquipmentPatterns()
        {
            foreach (var entry in OccultEquipmentMaterials.Where(entry =>
                ActiveOccultEquipmentMaterials.Contains(entry.Stock.Id)))
            {
                var stock = entry.Stock;
                AddRegionalPattern("equipment_" + stock.Id.Substring("iom_occult_".Length),
                    stock.Name + " Pattern", entry.Faction, entry.FactionName, null, entry.Tier, stock.Id);
            }

            // Gannix's two native political factions share living stock
            // knowledge, not one another's specialized finished weapon patterns.
            if (ActiveOccultEquipmentMaterials.Contains(GannixLivingWeave))
            {
                AddRegionalPattern("xiomara_living_weave", "Gannix Living Weave Pattern",
                    "XiomaraMasks", "Feathered Masks", null, 3, GannixLivingWeave);
                AddRegionalPattern("xiomara_living_rags", "Gannix Living Tissue Pattern",
                    "XiomaraMasks", "Feathered Masks", null, 3, "quasi_venus_rags");
                AddRegionalPattern("tezctlan_living_rags", "Gannix Living Tissue Pattern",
                    TezctlanFaction, "Tezctlan", null, 3, "quasi_venus_rags");
                // Existing regional ammo seed patterns already license feathers.
                // Supply targeted fallback patterns only if the matching ammo
                // chain was skipped because another producer owns its receipt.
                if (!ActiveRegionalAmmoChains.Contains(IgvasAmmo))
                    AddRegionalPattern("equipment_tezctlan_feather", "Gannix Living Fiber Pattern",
                        TezctlanFaction, "Tezctlan", null, 3, "quasi_venus_feather");
                if (!ActiveRegionalAmmoChains.Contains(AngerlingsAmmo))
                    AddRegionalPattern("equipment_xiomara_feather", "Gannix Living Fiber Pattern",
                        "XiomaraMasks", "Feathered Masks", null, 3, "quasi_venus_feather");
            }
            if (IsRegisteredOccultStock(GannixChitin))
            {
                AddRegionalPattern("tezctlan_chitin", "Gannix Chitin Stock Pattern",
                    TezctlanFaction, "Tezctlan", null, 3, GannixChitin);
                AddRegionalPattern("tezctlan_chitin_feedstock", "Gannix Structural Stock Pattern",
                    TezctlanFaction, "Tezctlan", null, 3, "venus_melee_parts");
            }
            if (ActiveOccultEquipmentMaterials.Contains(RonBloodConcentrate))
                AddRegionalPattern("equipment_ron_blood", "Ron Blood Stock Pattern",
                    ChaosFaction, "Chaos Undivided", "iomOssuaryWorks", 4, "ron_blood");
            if (ActiveOccultEquipmentMaterials.Contains(RonBindingWeave))
                AddRegionalPattern("equipment_ron_tentacle", "Ron Living Fiber Pattern",
                    ChaosFaction, "Chaos Undivided", "iomOssuaryWorks", 4, "ron_tentacle");
            if (ActiveOccultEquipmentMaterials.Contains(CattaramCrystalCore))
            {
                AddRegionalPattern("equipment_cattaram_geode", "Cattaram Geode Pattern",
                    ChaosFaction, "Chaos Undivided", null, 5, "cattaram_geode");
                AddRegionalPattern("equipment_cattaram_stone", "Cattaram Refined Crystal Pattern",
                    ChaosFaction, "Chaos Undivided", null, 5, "cattaram_stone");
            }
            // Foundation patterns remain available even if the corresponding
            // ammo producer is native and that prototype was skipped.
            if ((ActiveOccultEquipmentMaterials.Contains(GannixLivingWeave) ||
                IsRegisteredOccultStock(GannixChitin)) && !ActiveRegionalAmmoChains.Contains(IgvasAmmo))
                RegisterRegionalFoundationPatterns("tezctlan", TezctlanFaction, "Tezctlan", null);
            if (ActiveOccultEquipmentMaterials.Contains(GannixLivingWeave) &&
                !ActiveRegionalAmmoChains.Contains(AngerlingsAmmo))
                RegisterRegionalFoundationPatterns("xiomara", "XiomaraMasks", "Feathered Masks", null);
            if (ActiveOccultEquipmentMaterials.Contains(UrForgeLattice) &&
                !ActiveRegionalAmmoChains.Contains(EannisAmmo))
            {
                RegisterRegionalFoundationPatterns("shedu", "SheduThousand", "Shedu's Thousand", null);
                AddRegionalPattern("equipment_shedu_skin", "Ur Living Skin Pattern",
                    "SheduThousand", "Shedu's Thousand", null, 5, "mars_skin");
                AddRegionalPattern("equipment_shedu_gear", "Ur Forge Gear Pattern",
                    "SheduThousand", "Shedu's Thousand", null, 5, "mars_gear");
            }
            if (ActiveOccultEquipmentMaterials.Any(id => Array.Find(OccultEquipmentMaterials,
                entry => entry.Stock.Id == id)?.Faction == ChaosFaction) &&
                !RegionalOccultAmmoChains.Any(chain => chain.Faction == ChaosFaction &&
                    ActiveRegionalAmmoChains.Contains(chain.Output)))
                RegisterRegionalFoundationPatterns("chaos", ChaosFaction, "Chaos Undivided", "iomHollowSignalDepot");
        }

        private static void AuditOccultEquipmentMaterials()
        {
            foreach (var entry in OccultEquipmentMaterials.Where(entry =>
                ActiveOccultEquipmentMaterials.Contains(entry.Stock.Id)))
            {
                string id = entry.Stock.Id;
                var receipt = Data.ProduceReceipts.Single(recipe => recipe.Id == id && recipe.OutputItem == id);
                Debug.Log("[Cvar_ImperiumOfMan] Occult equipment stock | region=" + entry.Region +
                    " | output=" + id + " | batch=" + IndustryBatches[id] + " | hours=" + receipt.ProduceTimeInHours +
                    " | inputs=" + string.Join(", ", receipt.RequiredItems.Select(input => input.ItemId + " x" + input.Count)) +
                    " | consumers=" + string.Join(", ", Data.ProduceReceipts.Where(recipe =>
                        recipe.RequiredItems != null && recipe.RequiredItems.Any(input => input.ItemId == id))
                        .Select(recipe => recipe.OutputItem)) +
                    " | chips=" + string.Join(", ", OccultPatternSources[id]));
            }
        }
    }
}

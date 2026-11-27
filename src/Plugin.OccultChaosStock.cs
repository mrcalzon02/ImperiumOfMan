using System;
using System.Collections.Generic;
using System.Linq;
using MGSC;
using UnityEngine;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private const string ShardMatrix = "iom_occult_chaos_shard_matrix";
        private const string InfusedShardStock = "iom_occult_chaos_infused_shard_stock";
        private const string BoundConductor = "iom_occult_chaos_bound_conductor";
        private const string ChargedCoilStock = "iom_occult_chaos_charged_coil_stock";
        private const string SoulforgeMatrix = "iom_occult_chaos_soulforge_matrix";
        private const string SoulforgeArmature = "iom_occult_chaos_soulforge_armature";

        private static readonly HashSet<string> ActiveChaosOccultStocks = new(StringComparer.Ordinal);
        private static readonly HashSet<string> ActiveChaosAmmoChains = new(StringComparer.Ordinal);
        private static readonly List<(string Id, string Title, int Tier)> ChaosStockPatterns = new();

        private static bool ChaosStockInputAvailable(string id, HashSet<string> planned) =>
            planned.Contains(id) || IsRegisteredOccultStock(id) ||
            Data.Items.GetRecord(id) != null ||
            (id.StartsWith("iom_", StringComparison.Ordinal) && IndustryBatches.ContainsKey(id));

        // Called after the relevant existing tier salvage and ritual recipes
        // have been staged. Preflight the entire chain before staging any item,
        // so a missing donor cannot leave half a new chain registered.
        private static bool TryRegisterChaosStockChain(ChaosSupplyTier tier, string rank,
            params RegionalOccultMaterial[] stocks)
        {
            var planned = new HashSet<string>(StringComparer.Ordinal);
            foreach (var stock in stocks)
            {
                var donor = Data.Items.GetSimpleRecord<ItemRecord>(stock.Donor);
                if (stock.Batch <= 0 || stock.Hours <= 0 || donor?.ItemDesc?.Icon == null ||
                    Data.Items.GetRecord(stock.Id) != null || IndustryBatches.ContainsKey(stock.Id) ||
                    RegionalOccultMaterials.Contains(stock.Id) || planned.Contains(stock.Id) ||
                    Data.ProduceReceipts.Any(receipt => receipt.Id == stock.Id || receipt.OutputItem == stock.Id) ||
                    Data.Descriptors["trash"].TryGetDescriptor(stock.Id, out _) ||
                    stock.Inputs.Any(input => input.Count <= 0 || !ChaosStockInputAvailable(input.ItemId, planned)))
                {
                    Debug.LogWarning("[Cvar_ImperiumOfMan] Chaos occult stock preflight skipped: " + stock.Id);
                    return false;
                }
                planned.Add(stock.Id);
            }

            foreach (var stock in stocks)
            {
                AddMaterial(stock.Id, stock.Donor, stock.Name, stock.Description,
                    stock.Price, tier.Level, category: ChaosFaction, retainNativeArtwork: true);
                RegionalOccultMaterials.Add(stock.Id);
                OccultMaterialOutputs.Add(stock.Id);
                RegisterOccultRecipe(stock.Id, rank, stock.Hours, stock.Batch, stock.Inputs);
                if (!AddedOccultRecipes.Contains(stock.Id))
                    throw new InvalidOperationException("Incomplete Chaos occult stock registration: " + stock.Id);
                ActiveChaosOccultStocks.Add(stock.Id);
                ChaosStockPatterns.Add((stock.Id, stock.Name + " Pattern", tier.Level));
            }
            return true;
        }

        private static bool TryRegisterChaosShardStock(ChaosSupplyTier tier)
        {
            if (!TryRegisterChaosStockChain(tier, OccultRankThree,
                new(ShardMatrix, "mercury_glass", "Shard Matrix",
                    "Ritually bound Shard Glass prepared for quasi cells.", 12, 4, 3,
                    new(tier.Salvage.Id, 1), new(BoundAsh, 1)),
                new(InfusedShardStock, "mercury_glass", "Infused Shard Stock",
                    "Living-energy stock for Shard Cells.", 14, 4, 4,
                    new(ShardMatrix, 2), new(NativeGavvakh, 1))))
                return false;

            ActiveChaosAmmoChains.Add(ShardCells);
            return true;
        }

        private static bool TryRegisterChaosCoilStock(ChaosSupplyTier tier)
        {
            if (!TryRegisterChaosStockChain(tier, OccultRankFive,
                new(BoundConductor, "wire", "Bound Conductor",
                    "An insulated conductor imbued with Soulforge essence.", 17, 5, 3,
                    new(tier.Salvage.Id, 1), new(ForgeEmber, 1)),
                new(ChargedCoilStock, "wire", "Charged Coil Stock",
                    "Occult charge stock for Excruciator Coil Cells.", 20, 5, 4,
                    new(BoundConductor, 2), OccultStockOr(ShartamakhumCell, NativeGavvakh))))
                return false;

            ActiveChaosAmmoChains.Add(CoilCells);
            return true;
        }

        private static void RegisterSoulforgeMatrix(ChaosSupplyTier tier) =>
            TryRegisterChaosStockChain(tier, OccultRankFour,
                new(SoulforgeMatrix, "demonic_eye", "Soulforge Matrix",
                    "Bound essence held in a lunar structural matrix.", 18, 6, 2,
                    new(BoundAsh, 1), OccultStockOr(DuggurMatrix, "moon_armor_plates")));

        private static void RegisterSoulforgeArmature(ChaosSupplyTier tier) =>
            TryRegisterChaosStockChain(tier, OccultRankFive,
                new(SoulforgeArmature, "moon_armor_plates", "Soulforge Armature",
                    "A reinforced occult matrix for advanced Undivided equipment.", 24, 8, 2,
                    new(ForgeEmber, 1), OccultStockOr(CattaramCrystalCore, "cattaram_stone"),
                    OccultStockOr(DuggurMatrix, "moon_armor_plates")));

        private static string ChaosStockSupplier(int tier) =>
            Array.Find(ChaosSites, site => site.FirstTier <= tier && tier <= site.LastTier)?.Id;

        private static void RegisterChaosOccultStockPatterns()
        {
            foreach (var stock in ChaosStockPatterns)
                AddRegionalPattern("stock_" + stock.Id.Substring("iom_occult_".Length), stock.Title,
                    ChaosFaction, "Chaos Undivided", ChaosStockSupplier(stock.Tier), stock.Tier, stock.Id);

            // Preserve the existing mixed tier chips and all learned recipe IDs.
            // Targeted chips are additional physical acquisition routes for the
            // legacy owned feedstock/finish recipes; research grants none.
            if (ActiveChaosAmmoChains.Contains(ShardCells))
            {
                AddChaosOwnedSingleton("chaos_shard_glass", "Shard Glass Pattern", _chaosSupply[1].Salvage.Id, 2);
                AddChaosOwnedSingleton("chaos_shard_finish", "Shard Cells Pattern", ShardCells, 2);
            }
            if (ActiveChaosAmmoChains.Contains(CoilCells) || ActiveChaosOccultStocks.Contains(SoulforgeArmature))
            {
                AddChaosOwnedSingleton("chaos_conduit", "Insulated Conduit Pattern", _chaosSupply[5].Salvage.Id, 6);
                AddChaosOwnedSingleton("chaos_ember", "Soulforge Ember Pattern", ForgeEmber, 6);
            }
            if (ActiveChaosAmmoChains.Contains(CoilCells))
                AddChaosOwnedSingleton("chaos_coil_finish", "Excruciator Coil Cells Pattern", CoilCells, 6);
            if (ActiveChaosAmmoChains.Contains(ShardCells) || ActiveChaosOccultStocks.Contains(SoulforgeMatrix))
                AddChaosOwnedSingleton("chaos_bound_ash", "Bound Ash Pattern", BoundAsh, 2);

            if (ActiveChaosOccultStocks.Any(id => !OccultPatternSources.ContainsKey(id)))
                throw new InvalidOperationException("Chaos occult stock has no physical pattern source.");
        }

        private static void AddChaosOwnedSingleton(string key, string title, string output, int tier)
        {
            if (!output.StartsWith(ChaosItemPrefix, StringComparison.Ordinal) || !IndustryBatches.ContainsKey(output))
                return;
            RegisterOccultPatternCatalog(key, title, ChaosFaction, ChaosStockSupplier(tier), tier, [output]);
            RegionalOccultPatternFactions.Add("iom_occult_" + key + "_pattern_chip", (ChaosFaction, "Chaos Undivided"));
        }

        private static void AuditChaosOccultStock()
        {
            foreach (string output in ActiveChaosOccultStocks.Concat(ActiveChaosAmmoChains))
            {
                var receipt = Data.ProduceReceipts.Single(recipe => recipe.OutputItem == output);
                Debug.Log("[Cvar_ImperiumOfMan] Chaos occult production | output=" + output +
                    " | batch=" + IndustryBatches[output] + " | hours=" + receipt.ProduceTimeInHours +
                    " | inputs=" + string.Join(", ", receipt.RequiredItems.Select(input => input.ItemId + " x" + input.Count)) +
                    " | chips=" + string.Join(", ", OccultPatternSources[output]));
            }
        }
    }
}

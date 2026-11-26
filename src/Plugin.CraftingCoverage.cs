using System;
using System.Collections.Generic;
using System.Linq;
using MGSC;
using UnityEngine;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private static bool _craftingCoverageLogged;

        // This diagnostic reads the registered recipes once. It does not change recipes,
        // unlock schematics, or infer that an item without a recipe should be craftable.
        private static void LogCraftingCoverage()
        {
            if (_craftingCoverageLogged)
            {
                return;
            }
            _craftingCoverageLogged = true;

            try
            {
                var magnum = Data.ProduceReceipts
                    .Select(recipe => new CraftingCoverageRecipe(recipe.OutputItem, recipe.RequiredItems)).ToArray();
                var workbench = Data.WorkbenchReceipts
                    .Select(recipe => new CraftingCoverageRecipe(recipe.OutputItem, recipe.RequiredItems)).ToArray();
                var rawSources = new HashSet<string>(StringComparer.Ordinal)
                {
                    Ore, "water_bottle_1", "human_meat"
                };
                // These sources deliberately retain their reward, salvage, or exotic acquisition context.
                var externalSources = new HashSet<string>(StringComparer.Ordinal)
                {
                    "iom_purity_seal", "iom_wreck_cyborg_battle", "iom_wreck_cyborg_worker",
                    "iom_wreck_drone_security", "iom_wreck_turret_ballistic", "demonic_eye",
                    "mars_cyborg_hand", "mars_gear", "mars_skin", "mercury_glass", "quasi_basic_ammo",
                    "quasi_venus_rags", "venus_armor_plates", "venus_weapon_parts"
                };
                var customOutputs = new HashSet<string>(IndustryBatches.Keys, StringComparer.Ordinal);

                Debug.Log("[Cvar_ImperiumOfMan] Crafting coverage: " + magnum.Length + " Magnum recipes and " +
                    workbench.Length + " workbench recipes inspected once; material routes only. " +
                    "Schematic and workbench access, production time, and material quantities are not evaluated.");
                LogCraftingCoverageIds("Declared external sources", externalSources);
                LogCraftingCoverageScope("Magnum", magnum, rawSources, externalSources, customOutputs);
                LogCraftingCoverageScope("Magnum + workbench", magnum.Concat(workbench).ToArray(),
                    rawSources, externalSources, customOutputs);
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[Cvar_ImperiumOfMan] Crafting coverage diagnostic could not finish; " +
                    "registration remains unchanged. " + exception);
            }
        }

        private sealed class CraftingCoverageRecipe
        {
            public readonly string Output;
            public readonly string[] Inputs;

            public CraftingCoverageRecipe(string output, IEnumerable<ItemQuantity> inputs)
            {
                Output = output;
                Inputs = inputs == null ? [] : inputs.Where(input => input.Count > 0)
                    .Select(input => input.ItemId).Distinct(StringComparer.Ordinal).ToArray();
                if (string.IsNullOrWhiteSpace(Output) || Inputs.Any(string.IsNullOrWhiteSpace))
                {
                    throw new InvalidOperationException("A production or workbench recipe contains an empty item ID.");
                }
            }
        }

        private static HashSet<string> ReachableCraftingMaterials(IEnumerable<CraftingCoverageRecipe> recipes,
            IEnumerable<string> sources)
        {
            var reachable = new HashSet<string>(sources, StringComparer.Ordinal);
            bool changed;
            do
            {
                changed = false;
                foreach (CraftingCoverageRecipe recipe in recipes)
                {
                    if (!reachable.Contains(recipe.Output) && recipe.Inputs.All(reachable.Contains))
                    {
                        changed |= reachable.Add(recipe.Output);
                    }
                }
            }
            while (changed);
            return reachable;
        }

        private static void LogCraftingCoverageScope(string scope, CraftingCoverageRecipe[] recipes,
            HashSet<string> rawSources, HashSet<string> externalSources, HashSet<string> customOutputs)
        {
            var outputs = new HashSet<string>(recipes.Select(recipe => recipe.Output), StringComparer.Ordinal);
            HashSet<string> rawReachable = ReachableCraftingMaterials(recipes, rawSources);
            HashSet<string> contextualReachable = ReachableCraftingMaterials(recipes, rawSources.Concat(externalSources));

            Debug.Log("[Cvar_ImperiumOfMan] Crafting coverage [" + scope + "]: raw materials reach " +
                outputs.Count(rawReachable.Contains) + "/" + outputs.Count + " listed outputs; with declared " +
                "external sources " + outputs.Count(contextualReachable.Contains) + "/" + outputs.Count +
                ". Registered custom batch outputs: " + customOutputs.Count(rawReachable.Contains) + "/" +
                customOutputs.Count + " raw, " + customOutputs.Count(contextualReachable.Contains) + "/" +
                customOutputs.Count + " with external sources.");

            LogCraftingCoverageIds(scope + " inputs without a listed producer (other acquisition may be intentional)",
                recipes.SelectMany(recipe => recipe.Inputs).Where(input => !outputs.Contains(input) &&
                    !rawSources.Contains(input) && !externalSources.Contains(input)));
            LogCraftingCoverageIds(scope + " outputs blocked from raw materials", outputs.Except(rawReachable));
            LogCraftingCoverageIds(scope + " outputs still blocked with declared external sources",
                outputs.Except(contextualReachable));
            LogCraftingCoverageIds(scope + " custom outputs still blocked with declared external sources",
                customOutputs.Except(contextualReachable));
        }

        private static void LogCraftingCoverageIds(string title, IEnumerable<string> values)
        {
            const int idsPerLine = 12;
            const int maxReportedIds = 48;
            string[] ids = values.Distinct(StringComparer.Ordinal).OrderBy(value => value, StringComparer.Ordinal).ToArray();
            if (ids.Length == 0)
            {
                return;
            }
            int count = Math.Min(ids.Length, maxReportedIds);
            for (int offset = 0; offset < count; offset += idsPerLine)
            {
                Debug.Log("[Cvar_ImperiumOfMan] Crafting coverage " + title + " (" + ids.Length + " total): " +
                    string.Join(", ", ids.Skip(offset).Take(Math.Min(idsPerLine, count - offset))));
            }
            if (ids.Length > count)
            {
                Debug.Log("[Cvar_ImperiumOfMan] Crafting coverage " + title + ": " + (ids.Length - count) +
                    " further IDs omitted from the bounded diagnostic.");
            }
        }
    }
}

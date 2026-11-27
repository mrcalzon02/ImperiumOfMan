using System;
using System.Collections.Generic;
using System.Linq;
using MGSC;
using UnityEngine;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        // Effective per-order costs, not IndustryRecipe's doubled/halved values.
        // One distinct native component per order; no equipment or reagent input.
        // Human flesh costs 9 biomass because the existing ordinary recipe turns
        // one human meat and water into 8 biomass. Avoid a positive-return loop.
        private static readonly (string Output, string Title, int Cost, int Hours)[] CloneVatComponents =
        [
            ("human_meat", "Human Flesh", 9, 4),
            ("human_skin", "Human Skin", 3, 3),
            ("human_hand", "Human Arm", 5, 5),
            ("human_leg", "Human Leg", 6, 5),
            ("human_skull", "Human Skull", 5, 5),
            ("human_rib", "Human Rib", 3, 3),
            ("dog_meat", "Dog Meat", 3, 3),
            ("spider_meat", "Spider Flesh", 3, 3),
            ("spider_claw", "Spider Claw", 4, 4),
            ("spider_joint", "Spider Joint", 4, 4)
        ];
        private static readonly HashSet<string> ActiveCloneVatProducts = new(StringComparer.Ordinal);

        private static bool IsCloneVatComponent(string id) =>
            id != null && CloneVatComponents.Any(component => component.Output == id);

        private static void RegisterCloneVatRecipes()
        {
            foreach (var component in CloneVatComponents)
            {
                // Never create substitute native items, unlock quest items, or
                // replace a native or other-mod producer. RegisterOccultRecipe
                // handles output ownership and the staged biomass input.
                var record = Data.Items.GetSimpleRecord<ItemRecord>(component.Output);
                if (record == null || record.ItemClass == ItemClass.QuestItem)
                {
                    Debug.LogWarning("[Cvar_ImperiumOfMan] Clone vat skipped absent/protected component: " + component.Output);
                    continue;
                }
                RegisterOccultRecipe(component.Output, OccultRankOne, component.Hours, 1,
                    new ItemQuantity(Biomass, component.Cost));
                if (AddedOccultRecipes.Contains(component.Output))
                {
                    ActiveCloneVatProducts.Add(component.Output);
                    Debug.Log("[Cvar_ImperiumOfMan] Clone vat recipe | output=" + component.Output +
                        " | biomass=" + component.Cost + " | batch=1 | hours=" + component.Hours);
                }
            }
        }

        private static void RegisterCloneVatPatterns()
        {
            foreach (var component in CloneVatComponents)
            {
                if (!ActiveCloneVatProducts.Contains(component.Output)) continue;
                // Baseline cultivation knowledge is Imperial, not conditional
                // on an alliance with an occult faction. Each physical chip
                // teaches exactly one component. Use the native contract pool
                // and paid station restock; do not grant anything to cargo.
                AddRegionalPattern("vat_" + component.Output, "Clone Vat: " + component.Title,
                    ItemCategory, "Imperium of Man", NeithVault, 1, component.Output);
            }
        }
    }
}

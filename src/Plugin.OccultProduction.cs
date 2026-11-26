using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HarmonyLib;
using MGSC;
using UnityEngine;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private const string OccultModule = "iom_occult_production";
        private const string RawClone = "iom_occult_raw_clone";
        private const string TormentImprint = "iom_occult_torment_imprint";
        private const string SufferingEssence = "iom_occult_suffering_essence";
        private const string NativeGavvakh = "quasi_medical_kit_1";
        private static readonly Dictionary<string, string> OccultRecipeRequirements = new(StringComparer.Ordinal);
        // Only recipes added by this module enter the new physical pattern-chip pools.
        private static readonly HashSet<string> AddedOccultRecipes = new(StringComparer.Ordinal);

        private static void RegisterSpoiledOrganicRecycling()
        {
            var accepted = new HashSet<string>(StringComparer.Ordinal);
            string[] fleshMarkers = ["meat", "skin", "flesh", "guts", "organ"];
            string[] spoiledMarkers = ["rotten", "rotting", "spoiled", "decayed", "putrid"];

            bool IsSpoiledOrganicTarget(string targetId, ItemRecord target) =>
                target != null && !string.IsNullOrEmpty(targetId) &&
                spoiledMarkers.Any(marker => targetId.IndexOf(marker, StringComparison.OrdinalIgnoreCase) >= 0) &&
                (target.ItemClass == ItemClass.Food || target.ItemClass == ItemClass.Drink ||
                 target.ItemClass == ItemClass.Organ || target.ItemClass == ItemClass.QuasiOrgan ||
                 (target.ItemClass == ItemClass.Parts &&
                  fleshMarkers.Any(marker => targetId.IndexOf(marker, StringComparison.OrdinalIgnoreCase) >= 0)));

            foreach (string id in Data.Items.Ids.ToArray())
            {
                var item = Data.Items.GetSimpleRecord<ItemRecord>(id);
                var source = Data.Items.GetSimpleRecord<ConsumableRecord>(id);
                // Expiry metadata lives in its own native configuration table.
                var expiry = Data.ItemExpire.GetRecord(id);
                bool flesh = fleshMarkers.Any(marker => id.IndexOf(marker, StringComparison.OrdinalIgnoreCase) >= 0);
                bool organicSource = item != null && item.ItemClass != ItemClass.QuestItem &&
                    ((source != null && source.StarvationValue > 0 &&
                      (source.ItemClass == ItemClass.Food || source.ItemClass == ItemClass.Drink)) ||
                     (flesh && (source?.ItemClass == ItemClass.Food || item.ItemClass == ItemClass.Organ ||
                                item.ItemClass == ItemClass.QuasiOrgan || item.ItemClass == ItemClass.Parts)));
                // Expiry also converts items into clean water and mechanical salvage.
                // The destination must itself be explicitly spoiled and organic.
                if (organicSource && !string.IsNullOrEmpty(expiry?.ConvertedItemId) &&
                    expiry.ConvertedItemId != id &&
                    Data.Items.GetSimpleRecord<ItemRecord>(expiry.ConvertedItemId) is ItemRecord rotten &&
                    IsSpoiledOrganicTarget(expiry.ConvertedItemId, rotten))
                    accepted.Add(expiry.ConvertedItemId);

                // Also support verified rotten food/flesh with no expiring donor.
                if (IsSpoiledOrganicTarget(id, item))
                    accepted.Add(id);
            }
            foreach (string id in accepted.OrderBy(value => value, StringComparer.Ordinal))
            {
                if (ShredProfiles.ContainsKey(id) || Data.Items.GetSimpleRecord<ItemRecord>(id) == null) continue;
                AddShredProfile([new(Biomass, 2)], id);
            }
            Debug.Log("[Cvar_ImperiumOfMan] Spoiled organic recycling: " +
                (accepted.Count == 0 ? "no verified spoiled organic targets found" :
                    string.Join(", ", accepted.OrderBy(value => value, StringComparer.Ordinal))));
        }

        private static void RegisterOccultProduction()
        {
            AddModuleText(OccultModule, "Occult Research", "Develop occult fabrication capabilities.");
            RegisterOccultResearchRanks();

            AddMaterial(RawClone, "rags", "Raw Clone", "Contained vat-grown organic stock.",
                1, maxIconSize: 24);
            AddMaterial(TormentImprint, "rags", "Torment Imprint", "Contained occult resonance.",
                2, maxIconSize: 24);
            AddMaterial(SufferingEssence, "rags", "Essence of Suffering", "Concentrated occult reagent.",
                3, maxIconSize: 24);

            // Final effective quantities; the ordinary IndustryRecipe half/double
            // adjustment is not applied to these formulas.
            RegisterOccultRecipe(RawClone, OccultRankOne, 8, 2);
            RegisterOccultRecipe(TormentImprint, OccultRankOne, 4, 4, new ItemQuantity(RawClone, 2));
            RegisterOccultRecipe(SufferingEssence, OccultRankTwo, 4, 4, new ItemQuantity(TormentImprint, 4));
            RegisterOccultRecipe(NativeGavvakh, OccultRankTwo, 4, 2, new ItemQuantity(SufferingEssence, 4));
            RegisterOccultResearchRecipes();
            RegisterOccultPatternSources();

            // This selected recycling operation spends the same clone token used by
            // torment production. It never spawns a creature or awards combat rewards.
            if (Data.Items.GetRecord("human_meat") != null &&
                Data.Items.GetRecord("human_skin") != null &&
                Data.Items.GetRecord("human_rib") != null)
                AddShredProfile([new("human_meat", 4), new("human_skin", 2), new("human_rib", 1)], RawClone);
            else
                Debug.LogWarning("[Cvar_ImperiumOfMan] Native clone harvest outputs missing; harvest not registered.");
        }

        private static void RegisterOccultRecipe(string output, string perk, float hours, int batch,
            params ItemQuantity[] inputs)
        {
            // Decide ownership before registering physical chip pools. Native recipes,
            // including their knowledge and production rules, keep their existing route.
            bool ownIntermediate = output == RawClone || output == TormentImprint || output == SufferingEssence;
            if (!ownIntermediate && Data.Items.GetRecord(output) == null)
            {
                Debug.LogWarning("[Cvar_ImperiumOfMan] Occult recipe skipped; installed output missing: " + output);
                return;
            }
            string[] missingNative = inputs.Where(input => !input.ItemId.StartsWith("iom_", StringComparison.Ordinal) &&
                Data.Items.GetRecord(input.ItemId) == null).Select(input => input.ItemId).ToArray();
            if (missingNative.Length > 0)
            {
                Debug.LogWarning("[Cvar_ImperiumOfMan] Occult recipe skipped; installed inputs missing for " +
                    output + ": " + string.Join(", ", missingNative));
                return;
            }
            if (IndustryBatches.ContainsKey(output) ||
                Data.ProduceReceipts.Any(recipe => recipe.OutputItem == output || recipe.Id == output))
            {
                Debug.Log("[Cvar_ImperiumOfMan] Existing recipe preserved: " + output);
                return;
            }
            IndustryBatches.Add(output, batch);
            OccultRecipeRequirements.Add(output, perk);
            AddedOccultRecipes.Add(output);
            Registrations.Add(() =>
            {
                // Earlier staged custom materials must exist before the formula and
                // its advertised physical schematic become usable.
                if (Data.Items.GetRecord(output) == null ||
                    inputs.Any(input => Data.Items.GetRecord(input.ItemId) == null))
                    throw new InvalidDataException("Missing staged ingredient in occult recipe " + output);
                if (Data.ProduceReceipts.Any(recipe => recipe.OutputItem == output || recipe.Id == output))
                    throw new InvalidDataException("Late duplicate occult recipe registration " + output);
                Data.ProduceReceipts.Add(new ItemProduceReceipt
                {
                    Id = output, OutputItem = output, RequiredItems = [.. inputs],
                    ProduceTimeInHours = hours, ModifyStartCost = 1,
                    ModifyStep = 1.05f, ModifyItemsGrades = []
                });
            });
        }

        [HarmonyPatch(typeof(ItemProductionSystem), nameof(ItemProductionSystem.StartMagnumItemProduction))]
        private static class RequireOccultFacility
        {
            [HarmonyPriority(Priority.First)]
            private static bool Prefix(MagnumCargo magnumCargo, MagnumProgression magnumSpaceship,
                ItemProduceReceipt receipt)
            {
                if (receipt == null || !OccultRecipeRequirements.TryGetValue(receipt.Id, out string perk)) return true;
                // The native production transaction supplies its own progression state.
                // Capability and physically acquired recipe knowledge are independent.
                return _industryReady && HasOccultCapability(magnumSpaceship, perk) &&
                    magnumCargo != null && magnumCargo.UnlockedProductionItems.Contains(receipt.OutputItem);
            }
        }

        // A separate module leaves the concurrently adjusted weapons, cargo and
        // Comms panels untouched. Engineering is the known native anchor/camera.
        [HarmonyPatch(typeof(SpaceshipScreen), "InitModulePoints")]
        private static class AddOccultModulePoint
        {
            private static void Prefix(Spaceship ____spaceship)
            {
                ____spaceship.UiAnchors[OccultModule] = ____spaceship.UiAnchors["engineer"];
                if (!____spaceship.ModuleIds.Contains(OccultModule))
                    ____spaceship.ModuleIds.Add(OccultModule);
            }
        }

        [HarmonyPatch(typeof(Spaceship), nameof(Spaceship.SetActiveModuleCamera))]
        private static class OccultModuleCamera
        {
            private static void Prefix(ref string moduleId)
            {
                if (moduleId == OccultModule) moduleId = "engineer";
            }
        }

        [HarmonyPatch(typeof(MagnumModulePoint), "SnapToWorldBone")]
        private static class OffsetOccultModulePoint
        {
            private static void Postfix(MagnumModulePoint __instance)
            {
                if (__instance.ModuleId != OccultModule) return;
                var placement = __instance.GetComponent<OccultPipPlacement>() ??
                    __instance.gameObject.AddComponent<OccultPipPlacement>();
                placement.Place();
            }
        }
    }
}

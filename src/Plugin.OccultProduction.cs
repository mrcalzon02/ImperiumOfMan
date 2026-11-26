using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MGSC;
using UnityEngine;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private const string OccultModule = "iom_occult_production";
        private const string CloneVat = "iom_occult_clone_vat";
        private const string ContainmentCradle = "iom_occult_containment_cradle";
        private const string EssenceStill = "iom_occult_essence_still";
        private const string GavvakhRetort = "iom_occult_gavvakh_retort";
        private const string RawClone = "iom_occult_raw_clone";
        private const string TormentImprint = "iom_occult_torment_imprint";
        private const string SufferingEssence = "iom_occult_suffering_essence";
        private const string NativeGavvakh = "quasi_medical_kit_1";
        private static readonly Dictionary<string, string> OccultRecipeRequirements = new();

        private static void RegisterSpoiledOrganicRecycling()
        {
            // Native IDs vary by game version. Inspect the loaded item registry rather
            // than inventing item records or changing the antibiotic recipe.
            string[] rottenMarkers = ["rotten", "rotting", "spoiled", "decayed", "expired", "putrid"];
            string[] organicMarkers = ["meat", "skin", "flesh", "food", "fish", "corpse",
                "organ", "fruit", "vegetable", "fung", "mushroom", "bread", "egg"];
            var accepted = new List<string>();
            foreach (string id in Data.Items.Ids.ToArray())
            {
                if (id.StartsWith("iom_", StringComparison.Ordinal) ||
                    !rottenMarkers.Any(marker => id.IndexOf(marker, StringComparison.OrdinalIgnoreCase) >= 0))
                    continue;
                var item = Data.Items.GetSimpleRecord<ItemRecord>(id);
                if (item == null || item.ItemDesc == null) continue;
                if (Data.Items.GetSimpleRecord<ConsumableRecord>(id) == null &&
                    !organicMarkers.Any(marker => id.IndexOf(marker, StringComparison.OrdinalIgnoreCase) >= 0))
                    continue;
                if (ShredProfiles.ContainsKey(id)) continue;
                // Spoiled food has a lower yield than the eight-unit fresh-flesh order.
                // Only player-selected Magnum recycling consumes the stack.
                AddShredProfile([new(Biomass, 2)], id);
                accepted.Add(id);
            }
            if (accepted.Count == 0)
                Debug.LogWarning("[Cvar_ImperiumOfMan] No native spoiled organic IDs matched; inspect the installed config.");
            else
                Debug.Log("[Cvar_ImperiumOfMan] Spoiled organic recycling: " + string.Join(", ", accepted));
        }

        private static void RegisterOccultProduction()
        {
            AddModuleText(OccultModule, "Occult Research", "Study occult materials and equipment.");
            var vat = RegisterSupportPerk(OccultModule, CloneVat, "OccultCloneVat", "Clone Vat",
                "Grow two raw clones.", 0, 0, null,
                ["construction_equipment", "medical_equipment", "engineering_parts_container"]);
            var cradle = RegisterSupportPerk(OccultModule, ContainmentCradle, "OccultCradle", "Containment Cradle",
                "Produce torment imprints.", 1, 0, vat,
                ["engineering_parts_container", "scientific_equipment"]);
            var still = RegisterSupportPerk(OccultModule, EssenceStill, "OccultStill", "Essence Still",
                "Separate suffering essence.", 2, 0, cradle,
                ["medical_equipment", "scientific_equipment"]);
            var retort = RegisterSupportPerk(OccultModule, GavvakhRetort, "OccultRetort", "Gavvakh Retort",
                "Condense native Gavvakh.", 3, 0, still,
                ["engineering_parts_container", "scientific_equipment", "communication_relay"]);

            RegisterOccultResearchRanks(retort);

            AddMaterial(RawClone, "rags", "Raw Clone", "Contained vat-grown organic stock.",
                1, maxIconSize: 24);
            AddMaterial(TormentImprint, "rags", "Torment Imprint", "Contained occult resonance.",
                2, maxIconSize: 24);
            AddMaterial(SufferingEssence, "rags", "Essence of Suffering", "Concentrated occult reagent.",
                3, maxIconSize: 24);

            // Explicit effective quantities: the ordinary IndustryRecipe balancing
            // helper is deliberately not applied a second time to these formulas.
            RegisterOccultRecipe(RawClone, CloneVat, 8, 2);
            RegisterOccultRecipe(TormentImprint, ContainmentCradle, 4, 4, new ItemQuantity(RawClone, 2));
            RegisterOccultRecipe(SufferingEssence, EssenceStill, 4, 4, new ItemQuantity(TormentImprint, 4));
            RegisterOccultRecipe(NativeGavvakh, GavvakhRetort, 4, 2, new ItemQuantity(SufferingEssence, 4));

            RegisterOccultResearchRecipes();

            // Alternative harvest is a deliberate ship recycling order, not a raid
            // creature or a repeatable death event. It spends the same clone token
            // that would otherwise be consumed by the torment recipe.
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
            Registrations.Add(() =>
            {
                // Never overwrite native/third-party recipes or manufacture an item
                // whose installed record is absent.
                if (Data.Items.GetRecord(output) == null ||
                    inputs.Any(input => Data.Items.GetRecord(input.ItemId) == null) ||
                    IndustryBatches.ContainsKey(output) ||
                    Data.ProduceReceipts.Any(recipe => recipe.OutputItem == output || recipe.Id == output))
                {
                    Debug.LogWarning("[Cvar_ImperiumOfMan] Occult recipe skipped (missing item or existing recipe): " + output);
                    return;
                }
                Data.ProduceReceipts.Add(new ItemProduceReceipt
                {
                    Id = output, OutputItem = output, RequiredItems = [.. inputs],
                    ProduceTimeInHours = hours, ModifyStartCost = 1,
                    ModifyStep = 1.05f, ModifyItemsGrades = []
                });
                IndustryBatches.Add(output, batch);
                OccultRecipeRequirements.Add(output, perk);
            });
        }

        private static void RefreshOccultKnowledge(MagnumCargo cargo, MagnumProgression progression)
        {
            if (!_industryReady || cargo == null || progression == null) return;
            foreach (var requirement in OccultRecipeRequirements)
            {
                if (progression.IsPerkPurchased(requirement.Value))
                    LearnRecipes(cargo, [requirement.Key]);
                // Only remove our private intermediates, not a native learned item.
                else if (requirement.Key == RawClone || requirement.Key == TormentImprint || requirement.Key == SufferingEssence)
                    cargo.UnlockedProductionItems.RemoveAll(id => id == requirement.Key);
            }
            foreach (var research in OccultResearchUnlocks)
                if (progression.IsPerkPurchased(research.Value))
                    LearnRecipes(cargo, [research.Key]);
        }

        [Hook(ModHookType.AfterSaveLoaded)]
        public static void RefreshOccultOnLoad(IModContext context)
        {
            RefreshOccultKnowledge(context.State.Get<MagnumCargo>(), context.State.Get<MagnumProgression>());
        }

        private static void RefreshCurrentOccultKnowledge()
        {
            var game = SingletonMonoBehaviour<SpaceGameMode>.Instance;
            if (game != null)
                RefreshOccultKnowledge(game.Get<MagnumCargo>(), game.Get<MagnumProgression>());
        }

        [HarmonyPatch(typeof(MagnumProductionWindow), "OnEnable")]
        private static class RefreshOccultProduction
        {
            private static void Prefix() => RefreshCurrentOccultKnowledge();
        }

        [HarmonyPatch(typeof(MagnumSelectItemToProduceWindow), nameof(MagnumSelectItemToProduceWindow.Configure), [typeof(int)])]
        private static class RefreshOccultPicker
        {
            private static void Prefix() => RefreshCurrentOccultKnowledge();
        }

        [HarmonyPatch(typeof(ItemProductionSystem), nameof(ItemProductionSystem.StartMagnumItemProduction))]
        private static class RequireOccultFacility
        {
            [HarmonyPriority(Priority.First)]
            private static bool Prefix(MagnumCargo magnumCargo, ItemProduceReceipt receipt)
            {
                if (!OccultRecipeRequirements.TryGetValue(receipt.Id, out string perk)) return true;
                var game = SingletonMonoBehaviour<SpaceGameMode>.Instance;
                if (game == null || game.Get<MagnumCargo>() != magnumCargo) return false;
                var progression = game.Get<MagnumProgression>();
                if (progression == null || !progression.IsPerkPurchased(perk)) return false;
                RefreshOccultKnowledge(magnumCargo, progression);
                return true;
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

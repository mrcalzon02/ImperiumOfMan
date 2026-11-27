using System;
using System.Collections.Generic;
using HarmonyLib;
using MGSC;
using UnityEngine;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private const string OccultRankOne = "iom_occult_research_1";
        private const string OccultRankTwo = "iom_occult_research_2";
        private const string OccultRankThree = "iom_occult_research_3";
        private const string OccultRankFour = "iom_occult_research_4";
        private const string OccultRankFive = "iom_occult_research_5";
        private const string OccultRankSix = "iom_occult_research_6";
        private static readonly string[] OccultRanks =
            [OccultRankOne, OccultRankTwo, OccultRankThree, OccultRankFour, OccultRankFive, OccultRankSix];

        private static string OccultResearchTitle(string perk) => perk switch
        {
            OccultRankOne => "I · Culture",
            OccultRankTwo => "II · Distillation",
            OccultRankThree => "III · Manifestation",
            OccultRankFour => "IV · Binding",
            OccultRankFive => "V · Infusion",
            OccultRankSix => "VI · Soulforge",
            _ => perk
        };

        private static string OccultCapabilitySummary(string perk) => perk switch
        {
            OccultRankOne => "Clone cultures, organic components and torment containment.",
            OccultRankTwo => "Suffering separation and Gavvakh condensation.",
            OccultRankThree => "Agga, Gannix and Shartamakhum fabrication.",
            OccultRankFour => "Duggur, Ron and Scrivnus fabrication.",
            OccultRankFive => "Ur, Cattaram, Flauros and Mars replacements.",
            OccultRankSix => "Advanced occult fabrication and quasi grafts.",
            _ => string.Empty
        };

        private static bool HasOccultCapability(MagnumProgression progression, string perk) =>
            progression != null && progression.IsPerkPurchased(perk);

        private static void RegisterOccultResearchRanks()
        {
            RegisterOccultUiText();
            // The six preserved capability IDs form the major spine. Branches
            // improve the facility rather than granting pattern knowledge.
            var one = RegisterSupportPerk(OccultModule, OccultRankOne, "OccultResearch1",
                OccultResearchTitle(OccultRankOne), "Cultures and components.", 0, 1, null,
                ["medical_equipment", "quasisamples"]);
            var two = RegisterSupportPerk(OccultModule, OccultRankTwo, "OccultResearch2",
                OccultResearchTitle(OccultRankTwo), "Essence separation and condensation.", 1, 1, one,
                ["scientific_equipment", NativeGavvakh]);
            var three = RegisterSupportPerk(OccultModule, OccultRankThree, "OccultResearch3",
                OccultResearchTitle(OccultRankThree), "Manifested materials.", 2, 1, two,
                ["scientific_equipment", "demonic_eye", "mercury_glass"]);
            var four = RegisterSupportPerk(OccultModule, OccultRankFour, "OccultResearch4",
                OccultResearchTitle(OccultRankFour), "Bone and blood processes.", 3, 1, three,
                ["scientific_equipment", "moon_guts", "ron_blood"]);
            var five = RegisterSupportPerk(OccultModule, OccultRankFive, "OccultResearch5",
                OccultResearchTitle(OccultRankFive), "Furnace, mineral and spore processes.", 4, 1, four,
                ["scientific_equipment", "mars_skin", "cattaram_geode", "jupiter_mushrooms"]);
            var six = RegisterSupportPerk(OccultModule, OccultRankSix, "OccultResearch6",
                OccultResearchTitle(OccultRankSix), "Advanced occult assembly.", 5, 1, five,
                ["prototype_schematics", "quasi_repair_kit", "mars_cyborg_head"]);
            RegisterOccultFacilityUpgrades(new Dictionary<string, MagnumPerkRecord>
            {
                [OccultRankOne] = one, [OccultRankTwo] = two, [OccultRankThree] = three,
                [OccultRankFour] = four, [OccultRankFive] = five, [OccultRankSix] = six
            });
        }

        private static void RegisterChaosOccultCapabilities(ChaosSupplyTier tier, IEnumerable<string> outputs)
        {
            // Existing physical faction chips remain the knowledge source. Research
            // adds fabrication capability only, never an entry in learned recipes.
            int level = tier.Level;
            string rank = level <= 2 ? OccultRankThree : level <= 4 ? OccultRankFour :
                level <= 6 ? OccultRankFive : OccultRankSix;
            foreach (string id in outputs)
            {
                if (id == FurySigilSkull || id == ChaosItemPrefix + "shadowblade") continue;
                bool demonicGear = level >= 2 && (id == tier.Weapon.Id || Array.IndexOf(tier.Armor, id) >= 0);
                bool occultStock = id == tier.Salvage.Id && (level == 2 || level == 3 || level == 4 || level == 9 || level == 10);
                bool ritual = id == BoundAsh || id == ForgeEmber || id == ShardCells ||
                    (id == CoilCells && ActiveChaosAmmoChains.Contains(CoilCells)) ||
                    id.StartsWith(ChaosItemPrefix + "skull_", StringComparison.Ordinal);
                // Food, dressings, mechanical ammunition and ordinary repair stock
                // keep their normal recipe access regardless of their faction branding.
                if (demonicGear || occultStock || ritual)
                {
                    OccultRecipeRequirements[id] = rank;
                    // Existing mixed Chaos tier chips are genuine knowledge
                    // sources too, even when no new singleton was introduced.
                    RecordOccultPatternIssuer(tier.Chip, ChaosFaction,
                        ChaosStockSupplier(level), level, false);
                    if (!OccultPatternSources.TryGetValue(id, out var sources))
                        OccultPatternSources.Add(id, sources = new List<string>());
                    if (!sources.Contains(tier.Chip)) sources.Add(tier.Chip);
                }
                if (occultStock) OccultMaterialOutputs.Add(id);
            }
        }

        [HarmonyPatch(typeof(MagnumProgression), nameof(MagnumProgression.IsAvailableToUpgrade))]
        private static class OccultSequentialResearch
        {
            private static void Postfix(MagnumProgression __instance, string id, ref bool __result)
            {
                if (TryGetOccultFacilityUpgrade(id, out var node))
                {
                    __result = __result && OccultPurchased(__instance, node.Parent) &&
                        OccultPurchased(__instance, node.RequiredRank) &&
                        (id != OccultLineIII || OccultPurchased(__instance, OccultLineII));
                    return;
                }
                int index = Array.IndexOf(OccultRanks, id);
                // Native trees allow buying backward from a purchased child.
                // Preserve the explicit major rank prerequisites.
                if (index > 0 && !HasOccultCapability(__instance, OccultRanks[index - 1]))
                    __result = false;
            }
        }
    }
}

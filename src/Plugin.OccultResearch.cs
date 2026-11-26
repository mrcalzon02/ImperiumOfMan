using System;
using System.Collections.Generic;
using System.Linq;
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
        private static readonly string[] OccultRanks =
            [OccultRankOne, OccultRankTwo, OccultRankThree, OccultRankFour, OccultRankFive];
        // Research teaches the existing designs. Faction chips can independently teach
        // the same recipes earlier; never revoke a learned recipe from an existing save.
        private static readonly Dictionary<string, string> OccultResearchUnlocks = new();

        private static string OccultResearchTitle(string perk) => perk switch
        {
            CloneVat => "Clone Vat",
            ContainmentCradle => "Containment Cradle",
            EssenceStill => "Essence Still",
            GavvakhRetort => "Gavvakh Retort",
            OccultRankOne => "I · Manifestation",
            OccultRankTwo => "II · Binding",
            OccultRankThree => "III · Infusion",
            OccultRankFour => "IV · Soulforge",
            OccultRankFive => "V · Ascension",
            _ => perk
        };

        private static void RegisterOccultResearchRanks(MagnumPerkRecord retort)
        {
            var one = RegisterSupportPerk(OccultModule, OccultRankOne, "OccultResearch1",
                OccultResearchTitle(OccultRankOne), "Agga, Gannix and Shartamakhum materials. Chaos tiers 1–2.",
                4, 0, retort, ["scientific_equipment", "medical_equipment", "engineering_parts_container"]);
            var two = RegisterSupportPerk(OccultModule, OccultRankTwo, "OccultResearch2",
                OccultResearchTitle(OccultRankTwo), "Lunar, Ron and Scrivnus patterns. Chaos tiers 3–4.",
                5, 0, one, ["scientific_equipment", "scientific_equipment", "communication_relay"]);
            var three = RegisterSupportPerk(OccultModule, OccultRankThree, "OccultResearch3",
                OccultResearchTitle(OccultRankThree), "Ur, Flauros and Cattaram patterns. Chaos tiers 5–6.",
                5, 1, two, ["scientific_equipment", "military_parts_container", "engineering_parts_container"]);
            var four = RegisterSupportPerk(OccultModule, OccultRankFour, "OccultResearch4",
                OccultResearchTitle(OccultRankFour), "Otherworldly grafts and weapons. Chaos tiers 7–8.",
                4, 1, three, ["scientific_equipment", "prototype_schematics", "medical_equipment"]);
            RegisterSupportPerk(OccultModule, OccultRankFive, "OccultResearch5",
                OccultResearchTitle(OccultRankFive), "Advanced occult gear. Chaos tiers 9–10.",
                3, 1, four, ["prototype_schematics", "prototype_schematics", "scientific_equipment"]);
        }

        private static void RegisterChaosResearchPatterns(int level, IEnumerable<string> outputs)
        {
            if (level < 1 || level > 10) return;
            string rank = OccultRanks[(level - 1) / 2];
            foreach (string id in outputs)
            {
                // The catalog already excludes these, but keep an independent safety check.
                if (id == FurySigilSkull || id == ChaosItemPrefix + "shadowblade") continue;
                OccultResearchUnlocks[id] = rank;
            }
        }

        private static void RegisterOccultResearchRecipes()
        {
            // All quantities below are final effective batches, not the half/double
            // arguments of IndustryRecipe. Each native record is checked at registration.
            // Existing native or mod recipes are never replaced by these additions.
            var a = OccultRankOne;
            RegisterOccultRecipe("demonic_eye", a, 5, 2, new(SufferingEssence, 2), new(GlassStock, 1));
            RegisterOccultRecipe("quasi_venus_rags", a, 4, 4, new(SufferingEssence, 1), new(Biomass, 1));
            RegisterOccultRecipe("venus_armor_plates", a, 5, 2, new(NativeGavvakh, 1), new(Sheet, 1), new(Silicate, 1));
            RegisterOccultRecipe("venus_weapon_parts", a, 5, 2, new(NativeGavvakh, 1), new("weapon_parts", 1));
            RegisterOccultRecipe("venus_melee_parts", a, 4, 4, new(SufferingEssence, 1), new("rod_parts", 1));
            RegisterOccultRecipe("quasi_venus_feather", a, 4, 4, new(SufferingEssence, 1), new(Textile, 1));
            RegisterOccultRecipe("venus_guts", a, 4, 4, new(SufferingEssence, 1), new(Biomass, 1));
            RegisterOccultRecipe("quasi_basic_ammo", a, 3, 12, new(NativeGavvakh, 1), new(ChemicalExtract, 1));
            RegisterOccultRecipe("quasi_darts_ammo", a, 3, 12, new(NativeGavvakh, 1), new("rod_parts", 1));
            RegisterOccultRecipe("mercury_glass", a, 5, 4, new(SufferingEssence, 1), new(GlassStock, 1), new(ReactiveMinerals, 1));
            RegisterOccultRecipe("mercury_rags", a, 4, 4, new(SufferingEssence, 1), new(Textile, 1));
            RegisterOccultRecipe("mercury_scales", a, 5, 4, new(SufferingEssence, 1), new(Plate, 1));
            RegisterOccultRecipe("mercury_guts", a, 4, 4, new(SufferingEssence, 1), new(Biomass, 1));
            RegisterOccultRecipe("quasi_energy_ammo", a, 4, 12, new(NativeGavvakh, 1), new("mercury_glass", 1), new(CellStock, 1));
            RegisterOccultRecipe("bone_knife", a, 8, 2, new("human_rib", 1), new("venus_melee_parts", 1), new(Liner, 1));
            RegisterOccultRecipe("venus_quasi_pistol_1", a, 10, 2, new("venus_weapon_parts", 1), new("mercury_glass", 1), new(Fittings, 1));

            var b = OccultRankTwo;
            RegisterOccultRecipe("moon_guts", b, 4, 4, new(SufferingEssence, 1), new(Biomass, 1));
            RegisterOccultRecipe("moon_armor_plates", b, 5, 4, new(SufferingEssence, 1), new(Plate, 1));
            RegisterOccultRecipe("moon_weapon_parts", b, 5, 4, new(SufferingEssence, 1), new("weapon_parts", 1));
            RegisterOccultRecipe("quasi_bolts_ammo", b, 3, 12, new(NativeGavvakh, 1), new("rod_parts", 1));
            RegisterOccultRecipe("moon_mace_1", b, 10, 2, new("moon_weapon_parts", 1), new("moon_armor_plates", 1), new("rod_parts", 1));
            RegisterOccultRecipe("ron_blood", b, 4, 4, new(SufferingEssence, 1), new("water_bottle_1", 1));
            RegisterOccultRecipe("ron_tentacle", b, 5, 4, new(SufferingEssence, 1), new(Biomass, 1), new(Textile, 1));
            RegisterOccultRecipe("quasi_ron_medicine_1", b, 5, 2, new("ron_blood", 2), new(ChemicalExtract, 1));
            RegisterOccultRecipe("ron_mask_1", b, 8, 2, new("ron_tentacle", 1), new(Liner, 1), new("lens", 1));
            RegisterOccultRecipe("skrivnus_knife", b, 10, 2, new("human_rib", 1), new(SufferingEssence, 1), new(Metal, 1));

            var c = OccultRankThree;
            RegisterOccultRecipe("mars_skin", c, 5, 4, new(NativeGavvakh, 1), new(Biomass, 1), new(PolymerStock, 1));
            RegisterOccultRecipe("mars_gear", c, 5, 4, new(NativeGavvakh, 1), new(Fittings, 1), new(Sheet, 1));
            RegisterOccultRecipe("mars_gas_ammo", c, 4, 12, new(NativeGavvakh, 1), new(OrganicOil, 1));
            RegisterOccultRecipe("cattaram_geode", c, 6, 4, new(SufferingEssence, 1), new(Silicate, 1), new(ReactiveMinerals, 1));
            RegisterOccultRecipe("cattaram_stone", c, 5, 2, new("cattaram_geode", 1), new(NativeGavvakh, 1));
            RegisterOccultRecipe("cattaram_mine", c, 5, 2, new("cattaram_geode", 1), new("explosives", 1), new(Fittings, 1));
            RegisterOccultRecipe("jupiter_mushrooms", c, 5, 4, new(NativeGavvakh, 1), new(Biomass, 1), new("water_bottle_1", 1));
            RegisterOccultRecipe("jupiter_grass", c, 5, 4, new(NativeGavvakh, 1), new(Biomass, 1), new(Textile, 1));
            RegisterOccultRecipe("jupiter_guts", c, 5, 4, new(SufferingEssence, 1), new(Biomass, 1), new("jupiter_mushrooms", 1));
            RegisterOccultRecipe("flauros_ammo", c, 4, 12, new("jupiter_mushrooms", 1), new("tin", 1), new("gas_ammo", 1));
            RegisterOccultRecipe("spore_mine", c, 6, 2, new("jupiter_mushrooms", 1), new("explosives", 1), new(PolymerStock, 1));

            var d = OccultRankFour;
            // Six independently selected body-part orders; no multi-output duplication.
            foreach (string part in new[] { "head", "chest", "stomach", "knee", "hand", "leg" })
                RegisterOccultRecipe("mars_cyborg_" + part, d, 10, 2, new("mars_gear", 1), new("mars_skin", 1), new(ServoCartridge, 1));
            RegisterOccultRecipe("warp_armor", d, 12, 2, new("venus_armor_plates", 2), new(NativeGavvakh, 1), new(SterileScaffold, 1));
            RegisterOccultRecipe("steadfast_heart", d, 12, 2, new("quasi_venus_rags", 2), new(SufferingEssence, 2), new(SterileScaffold, 1));
            RegisterOccultRecipe("blood_dance", d, 12, 2, new("quasi_venus_rags", 2), new("ron_blood", 1), new(SterileScaffold, 1));
            RegisterOccultRecipe("superdermal_layer", d, 12, 2, new("venus_armor_plates", 2), new("wire", 1), new(SterileScaffold, 1));
            RegisterOccultRecipe("stunning_stride", d, 12, 2, new("venus_weapon_parts", 2), new("quasi_venus_rags", 1), new(ServoCartridge, 1));
            RegisterOccultRecipe("cortex_stimulator", d, 12, 2, new("mars_gear", 2), new("mars_skin", 1), new(ImplantController, 1));

            var e = OccultRankFive;
            RegisterOccultRecipe("quasi_repair_kit", e, 10, 2, new(SufferingEssence, 2), new("cattaram_stone", 1), new(ForgeEmber, 1));
            RegisterOccultRecipe("impaler", e, 16, 2, new("moon_weapon_parts", 2), new("human_rib", 2), new(ForgeEmber, 1));
        }

        // Resolve the real native points instead of guessing a fixed canvas position.
        // Cache references; retry discovery only when the module UI is first built/rebuilt.
        private sealed class OccultPipPlacement : MonoBehaviour
        {
            private Transform _engineering;
            private Transform _research;
            private float _nextDiscovery;
            private bool _reportedMissing;

            public void Place()
            {
                if ((_engineering == null || _research == null) && Time.unscaledTime >= _nextDiscovery)
                {
                    _nextDiscovery = Time.unscaledTime + 0.75f;
                    var screen = GetComponentInParent<SpaceshipScreen>();
                    var root = screen != null ? screen.transform : transform.parent;
                    if (root != null)
                    {
                        foreach (var point in root.GetComponentsInChildren<MagnumModulePoint>(true))
                        {
                            if (point.ModuleId == "engineer") _engineering = point.transform;
                            else if (IsNativeResearchModule(point.ModuleId)) _research = point.transform;
                        }
                    }
                }
                if (_engineering != null && _research != null)
                {
                    transform.position = Vector3.Lerp(_engineering.position, _research.position, 0.5f);
                    return;
                }
                // Preserve the existing accessible fallback rather than hiding the pip
                // if the installed game changes the native research module identifier.
                transform.localPosition += new Vector3(90f, 45f, 0f);
                if (!_reportedMissing)
                {
                    _reportedMissing = true;
                    Debug.LogWarning("[Cvar_ImperiumOfMan] Occult Research waiting for native Engineering/Research pips.");
                }
            }

            private static bool IsNativeResearchModule(string id) =>
                !string.IsNullOrEmpty(id) && !id.StartsWith("iom_", StringComparison.Ordinal) &&
                (id.IndexOf("research", StringComparison.OrdinalIgnoreCase) >= 0 ||
                 id.IndexOf("science", StringComparison.OrdinalIgnoreCase) >= 0 ||
                 id.IndexOf("scientist", StringComparison.OrdinalIgnoreCase) >= 0 ||
                 id.Equals("scientific", StringComparison.OrdinalIgnoreCase));
        }
    }
}

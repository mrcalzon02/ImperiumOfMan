using System;
using System.Collections.Generic;
using System.Linq;
using MGSC;
using UnityEngine;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private static readonly Dictionary<string, List<string>> OccultPatternSources = new(StringComparer.Ordinal);

        private static void RegisterOccultPatternSources()
        {
            // RegisterCatalog uses native Datadisk selection: each physical chip learns
            // one randomly chosen pattern from its pool. Research never supplies it.
            AddOccultPatternPool("culture", "Culture Pattern", ChaosFaction, ChaosSites[0].Id, 1,
                RawClone, TormentImprint);
            AddOccultPatternPool("distillation", "Distillation Pattern", ChaosFaction, ChaosSites[0].Id, 2,
                SufferingEssence, NativeGavvakh);
            AddOccultPatternPool("agga", "Agga Manifestation Pattern", ChaosFaction, ChaosSites[1].Id, 3,
                "demonic_eye");
            AddOccultPatternPool("gannix_tissue", "Gannix Tissue Pattern", "Tezctlan", "FeatheredTemple", 3,
                "quasi_venus_rags", "quasi_venus_feather", "venus_guts");
            AddOccultPatternPool("gannix_forge", "Gannix Forge Pattern", "Tezctlan", "FeatheredTemple", 3,
                "venus_armor_plates", "venus_weapon_parts", "venus_melee_parts");
            AddOccultPatternPool("gannix_ammo", "Gannix Ammunition Pattern", "Tezctlan", "FeatheredTemple", 3,
                "quasi_basic_ammo", "quasi_darts_ammo");
            AddOccultPatternPool("gannix_arms", "Gannix Arms Pattern", "Tezctlan", "FeatheredTemple", 3,
                "bone_knife", "venus_quasi_pistol_1");
            AddOccultPatternPool("shartamakhum", "Shartamakhum Pattern", ChaosFaction, ChaosSites[3].Id, 3,
                "mercury_glass", "mercury_rags", "mercury_scales", "mercury_guts", "quasi_energy_ammo");
            AddOccultPatternPool("duggur", "Duggur Binding Pattern", ChaosFaction, ChaosSites[1].Id, 4,
                "moon_guts", "moon_armor_plates", "moon_weapon_parts", "quasi_bolts_ammo", "moon_mace_1");
            AddOccultPatternPool("ron", "Ron Bloodwork Pattern", ChaosFaction, ChaosSites[1].Id, 4,
                "ron_blood", "ron_tentacle", "quasi_ron_medicine_1", "ron_mask_1");
            AddOccultPatternPool("scrivnus", "Scrivnus Relic Pattern", ChaosFaction, ChaosSites[1].Id, 4,
                "skrivnus_knife");
            AddOccultPatternPool("ur", "Ur Furnace Pattern", "SheduThousand", "RedTiangong", 5,
                "mars_skin", "mars_gear", "mars_gas_ammo");
            AddOccultPatternPool("cattaram", "Cattaram Crystal Pattern", ChaosFaction, ChaosSites[3].Id, 5,
                "cattaram_geode", "cattaram_stone", "cattaram_mine");
            AddOccultPatternPool("flauros_culture", "Flauros Culture Pattern", ChaosFaction, ChaosSites[4].Id, 5,
                "jupiter_mushrooms", "jupiter_grass", "jupiter_guts");
            AddOccultPatternPool("flauros_ordnance", "Flauros Ordnance Pattern", ChaosFaction, ChaosSites[4].Id, 5,
                "flauros_ammo", "spore_mine");
            AddOccultPatternPool("mars_core", "Mars Core Replacement Pattern", "SheduThousand", "RedTiangong", 5,
                "mars_cyborg_head", "mars_cyborg_chest", "mars_cyborg_stomach");
            AddOccultPatternPool("mars_limb", "Mars Limb Replacement Pattern", "SheduThousand", "RedTiangong", 5,
                "mars_cyborg_knee", "mars_cyborg_hand", "mars_cyborg_leg");
            AddOccultPatternPool("tezctlan_graft", "Tezctlan Graft Pattern", "Tezctlan", "FeatheredTemple", 6,
                "warp_armor", "steadfast_heart");
            AddOccultPatternPool("xiomara_graft", "Xiomara Graft Pattern", "XiomaraMasks", "FeatheredTemple", 6,
                "blood_dance");
            AddOccultPatternPool("gannix_graft", "Gannix Graft Pattern", "ChurchGannix", "Gloria", 6,
                "stunning_stride");
            AddOccultPatternPool("soulforge", "Soulforge Relic Pattern", ChaosFaction, BlackMeridian, 6,
                "quasi_repair_kit", "impaler");

            RegisterRegionalOccultPatterns();

            string[] missing = AddedOccultRecipes.Where(id => !OccultPatternSources.ContainsKey(id)).ToArray();
            if (missing.Length > 0)
                throw new InvalidOperationException("Occult recipes have no in-game knowledge source: " + string.Join(", ", missing));
        }

        private static void AddOccultPatternPool(string key, string title, string faction, string supplier,
            int tier, params string[] candidates)
        {
            // An existing native recipe is neither replaced nor newly licensed by us.
            // Empty pools are omitted instead of creating an unusable physical chip.
            string[] outputs = candidates.Where(AddedOccultRecipes.Contains).Distinct().ToArray();
            if (outputs.Length == 0) return;
            string id = "iom_occult_" + key + "_pattern_chip";
            var disk = CloneItem<DatadiskRecord>(id, "low_chip", tier, 100 + tier * 45,
                "iom_occult_pattern", maxIconSize: 24);
            disk.Categories = [faction];
            RegisterCatalog(disk, title, "A recovered occult fabrication pattern.", faction, outputs,
                restrictOtherSources: false, supplier: supplier);
            foreach (string output in outputs)
            {
                if (!OccultPatternSources.TryGetValue(output, out var sources))
                    OccultPatternSources.Add(output, sources = new List<string>());
                sources.Add(id);
            }
            Debug.Log("[Cvar_ImperiumOfMan] Occult physical pattern pool " + id + " at " + supplier +
                ", faction tier " + tier + ": " + string.Join(", ", outputs));
        }
    }
}

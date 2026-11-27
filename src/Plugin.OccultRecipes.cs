using MGSC;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private static void RegisterOccultResearchRecipes()
        {
            // All quantities below are final effective batches, not the half/double
            // arguments of IndustryRecipe. Each native record is checked at registration.
            // Existing native or mod recipes are never replaced by these additions.
            var a = OccultRankThree;
            RegisterOccultRecipe("demonic_eye", a, 5, 2, new(SufferingEssence, 2), new(GlassStock, 1));
            RegisterOccultRecipe("quasi_venus_rags", a, 4, 4, new(SufferingEssence, 1), new(Biomass, 1));
            RegisterOccultRecipe("venus_armor_plates", a, 5, 2, new(NativeGavvakh, 1), new(Sheet, 1), new(Silicate, 1));
            RegisterOccultRecipe("venus_weapon_parts", a, 5, 2, new(NativeGavvakh, 1), new("weapon_parts", 1));
            RegisterOccultRecipe("venus_melee_parts", a, 4, 4, new(SufferingEssence, 1), new("rod_parts", 1));
            RegisterOccultRecipe("quasi_venus_feather", a, 4, 4, new(SufferingEssence, 1), new(Textile, 1));
            RegisterOccultRecipe("venus_guts", a, 4, 4, new(SufferingEssence, 1), new(Biomass, 1));
            if (!TryRegisterRegionalOccultAmmo(IgvasAmmo))
                RegisterOccultRecipe(IgvasAmmo, a, 3, 12, new(NativeGavvakh, 1), new(ChemicalExtract, 1));
            RegisterOccultRecipe("quasi_darts_ammo", a, 3, 12, new(NativeGavvakh, 1), new("rod_parts", 1));
            RegisterOccultRecipe("mercury_glass", a, 5, 4, new(SufferingEssence, 1), new(GlassStock, 1), new(ReactiveMinerals, 1));
            RegisterOccultRecipe("mercury_rags", a, 4, 4, new(SufferingEssence, 1), new(Textile, 1));
            RegisterOccultRecipe("mercury_scales", a, 5, 4, new(SufferingEssence, 1), new(Plate, 1));
            RegisterOccultRecipe("mercury_guts", a, 4, 4, new(SufferingEssence, 1), new(Biomass, 1));
            RegisterOccultRecipe("quasi_energy_ammo", a, 4, 12, new(NativeGavvakh, 1), new("mercury_glass", 1), new(CellStock, 1));
            RegisterOccultRecipe("bone_knife", a, 8, 2, new("human_rib", 1), new("venus_melee_parts", 1), new(Liner, 1));
            RegisterOccultRecipe("venus_quasi_pistol_1", a, 10, 2, new("venus_weapon_parts", 1), new("mercury_glass", 1), new(Fittings, 1));

            var b = OccultRankFour;
            RegisterOccultRecipe("moon_guts", b, 4, 4, new(SufferingEssence, 1), new(Biomass, 1));
            RegisterOccultRecipe("moon_armor_plates", b, 5, 4, new(SufferingEssence, 1), new(Plate, 1));
            RegisterOccultRecipe("moon_weapon_parts", b, 5, 4, new(SufferingEssence, 1), new("weapon_parts", 1));
            if (!TryRegisterRegionalOccultAmmo(NailsOfPainAmmo))
                RegisterOccultRecipe(NailsOfPainAmmo, b, 3, 12, new(NativeGavvakh, 1), new("rod_parts", 1));
            RegisterOccultRecipe("moon_mace_1", b, 10, 2, new("moon_weapon_parts", 1), new("moon_armor_plates", 1), new("rod_parts", 1));
            RegisterOccultRecipe("ron_blood", b, 4, 4, new(SufferingEssence, 1), new("water_bottle_1", 1));
            RegisterOccultRecipe("ron_tentacle", b, 5, 4, new(SufferingEssence, 1), new(Biomass, 1), new(Textile, 1));
            RegisterOccultRecipe("quasi_ron_medicine_1", b, 5, 2, new("ron_blood", 2), new(ChemicalExtract, 1));
            RegisterOccultRecipe("ron_mask_1", b, 8, 2, new("ron_tentacle", 1), new(Liner, 1), new("lens", 1));
            RegisterOccultRecipe("skrivnus_knife", b, 10, 2, new("human_rib", 1), new(SufferingEssence, 1), new(Metal, 1));

            var c = OccultRankFive;
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

            var d = OccultRankFive;
            // Six independently selected body-part orders; no multi-output duplication.
            foreach (string part in new[] { "head", "chest", "stomach", "knee", "hand", "leg" })
                RegisterOccultRecipe("mars_cyborg_" + part, d, 10, 2, new("mars_gear", 1), new("mars_skin", 1), new(ServoCartridge, 1));
            RegisterOccultRecipe("warp_armor", OccultRankSix, 12, 2, new("venus_armor_plates", 2), new(NativeGavvakh, 1), new(SterileScaffold, 1));
            RegisterOccultRecipe("steadfast_heart", OccultRankSix, 12, 2, new("quasi_venus_rags", 2), new(SufferingEssence, 2), new(SterileScaffold, 1));
            RegisterOccultRecipe("blood_dance", OccultRankSix, 12, 2, new("quasi_venus_rags", 2), new("ron_blood", 1), new(SterileScaffold, 1));
            RegisterOccultRecipe("stunning_stride", OccultRankSix, 12, 2, new("venus_weapon_parts", 2), new("quasi_venus_rags", 1), new(ServoCartridge, 1));

            var e = OccultRankSix;
            RegisterOccultRecipe("quasi_repair_kit", e, 10, 2, new(SufferingEssence, 2), new("cattaram_stone", 1), new(ForgeEmber, 1));
            RegisterOccultRecipe("impaler", e, 16, 2, new("moon_weapon_parts", 2), new("human_rib", 2), new(ForgeEmber, 1));
        }

    }
}

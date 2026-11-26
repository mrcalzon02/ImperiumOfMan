using MGSC;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private const string RawPulp = "iom_raw_cellulose_pulp";
        private const string OrganicOil = "iom_organic_oil";
        private const string ChemicalExtract = "iom_chemical_extract";
        private const string ReactiveMinerals = "iom_reactive_minerals";
        private const string CarbonFeedstock = "iom_carbon_feedstock";

        private static void RegisterRawMaterialChemistry()
        {
            // Each fraction has its own icon, floor sprite and shadow, keyed by its item ID.
            // Stage their records before any recipe that consumes them.
            AddMaterial(RawPulp, "rags", "Raw cellulose pulp",
                "Fibrous material separated from vat biomass for paper, reclaimed fiber, and textile production.",
                3, maxIconSize: 24);
            AddMaterial(OrganicOil, "acid", "Organic oil",
                "An oil fraction separated from vat biomass. Feedstock for fuel, polymers, and medical preparations.",
                4, maxIconSize: 24);
            AddMaterial(ChemicalExtract, "acid", "Chemical extract",
                "A separated biomass reagent used with mineral feedstock to prepare industrial chemicals and medical supplies.",
                4, maxIconSize: 24);
            AddMaterial(ReactiveMinerals, "glass", "Reactive mineral fraction",
                "A separated ore fraction used with chemical extract for acid and other industrial reagents.",
                3, maxIconSize: 24);
            AddMaterial(CarbonFeedstock, "powder", "Carbon-rich mineral feedstock",
                "A carbon-rich ore fraction for fuel, rubber, sorbent, and propellant production.",
                3, maxIconSize: 24);

            // Gathering flesh remains the organic entry point; ore and water have extraction orders.
            // The common recipe helper halves whole-item quantities and doubles these base batches.
            SharedRecipe(Biomass, 12, 4, new("human_meat", 1), new("water_bottle_1", 1));
            SharedRecipe(RawPulp, 2, 2, new(Biomass, 2), new("water_bottle_1", 1));
            SharedRecipe(OrganicOil, 3, 2, new ItemQuantity(Biomass, 2));
            SharedRecipe(ChemicalExtract, 3, 2, new(Biomass, 2), new("water_bottle_1", 1));
            SharedRecipe(ReactiveMinerals, 3, 2, new ItemQuantity(Ore, 3));
            SharedRecipe(CarbonFeedstock, 3, 2, new ItemQuantity(Ore, 3));

            // Use the native ingredient IDs so every existing recipe can consume these products.
            SharedRecipe("acid", 3, 2, new(ChemicalExtract, 1), new(ReactiveMinerals, 1), new("water_bottle_1", 1));
            SharedRecipe("gas_ammo", 4, 6, new(OrganicOil, 1), new(CarbonFeedstock, 1), new(ChemicalExtract, 1));
            SharedRecipe("paper_towel", 2, 2, new(RawPulp, 2), new("water_bottle_1", 1));
            SharedRecipe("rags", 2, 2, new ItemQuantity(Fiber, 2));
            SharedRecipe("plastic", 3, 2, new(OrganicOil, 1), new(ChemicalExtract, 1));
            SharedRecipe("rubber", 3, 2, new(PolymerStock, 1), new(CarbonFeedstock, 1));
            SharedRecipe("powder", 3, 2, new(ChemicalExtract, 1), new(ReactiveMinerals, 1), new(CarbonFeedstock, 1));
            SharedRecipe("explosives", 4, 2, new(ChemicalExtract, 1), new(ReactiveMinerals, 1), new("acid", 1));
            SharedRecipe("tin", 3, 2, new ItemQuantity(Metal, 2));

            RegisterSpoiledOrganicRecycling();
        }
    }
}

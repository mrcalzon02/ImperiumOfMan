using MGSC;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private const string CeramicFeedstock = "iom_ceramic_feedstock";
        private const string ElectronicSubstrate = "iom_electronic_substrate";

        private static void RegisterManufacturingIntermediates()
        {
            // AddMaterial creates independent records/descriptors; these prepared
            // materials have no donor disassembly or reverse-conversion recipe.
            AddMaterial(CeramicFeedstock, "glass", "Ceramic feedstock",
                "Prepared silicate and chemical feedstock for forming ceramite armor stock.",
                7, maxIconSize: 24).Disassembly = [];
            AddMaterial(ElectronicSubstrate, "circuit_board", "Electronic substrate",
                "An etched polymer blank ready for conductive wiring and circuit assembly.",
                14, maxIconSize: 24).Disassembly = [];

            // The common helper halves these declared inputs and doubles the batch:
            // two units of each ingredient produce two intermediates in two hours.
            // Each unit carries the original consumer's material cost and one hour.
            SharedRecipe(CeramicFeedstock, 2, 1, new(Silicate, 4), new(ChemicalExtract, 4));
            SharedRecipe(ElectronicSubstrate, 2, 1, new(PolymerStock, 4), new("acid", 4));
        }
    }
}

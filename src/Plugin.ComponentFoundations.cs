using System.Linq;
using MGSC;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private static void RegisterComponentFoundations()
        {
            RegisterMechanicalFoundations();
            RegisterElectricalFoundations();
            RegisterMedicalFoundations();
            RegisterMunitionsFoundations();
            RegisterCyborgFoundations();
        }

        private static void RegisterMechanicalFoundations()
        {
            // Loose parts follow the same stock-to-component paths as the station workshops.
            // Springs start at metal stock so the fittings chain cannot depend on itself.
            SharedRecipe("rusty_plates", 2, 2, new ItemQuantity(Metal, 1));
            SharedRecipe("coarse_parts", 3, 2, new(Metal, 1), new(PolymerStock, 1));
            SharedRecipe("rod_parts", 2, 2, new ItemQuantity(Metal, 1));
            SharedRecipe("spring", 2, 2, new ItemQuantity(Metal, 1));
            SharedRecipe("metal_tread", 2, 2, new ItemQuantity(Metal, 1));
            SharedRecipe("weapon_parts", 4, 2, new(Metal, 1), new("rod_parts", 1));
            SharedRecipe("armor_plates", 3, 2, new ItemQuantity(Sheet, 1));
            SharedRecipe("ceramite_plates", 5, 2, new(Sheet, 1), new(Silicate, 1), new(ChemicalExtract, 1));
            SharedRecipe("hydraulic_pump", 6, 2, new("coarse_parts", 1), new("rod_parts", 1), new("rubber", 1));
        }

        private static void RegisterElectricalFoundations()
        {
            SharedRecipe("lens", 4, 2, new(GlassStock, 1), new(PolymerStock, 1));
            SharedRecipe("capacitor_parts", 4, 2, new(Metal, 1), new(PolymerStock, 1), new("acid", 1));
            SharedRecipe("circuit_board", 4, 2, new(PolymerStock, 1), new("wire", 1), new("acid", 1));
            SharedRecipe("microelectronics_parts", 5, 2, new(Silicate, 1), new("wire", 1), new(ChemicalExtract, 1));
            SharedRecipe("transformer", 4, 2, new("wire", 1), new("capacitor_parts", 1));
            SharedRecipe("communication_relay", 6, 2, new("circuit_board", 1), new("microelectronics_parts", 1), new("wire", 1));
        }

        private static void RegisterMedicalFoundations()
        {
            SharedRecipe("medical_glue", 3, 2, new(PolymerStock, 1), new(ChemicalExtract, 1));
            // Keep the native Sorbent recipe, including its free inputs, wherever it exists.
            SharedIndustryRecipes.Add("pills_sorbent");
            if (!IndustryBatches.ContainsKey("pills_sorbent") &&
                !Data.ProduceReceipts.Any(receipt => receipt.OutputItem == "pills_sorbent"))
            {
                SharedRecipe("pills_sorbent", 3, 2, new(CarbonFeedstock, 1), new("water_bottle_1", 1));
            }

            // These are abstract game recipes using loose chemical and organic feedstock.
            IndustryRecipe("goldenstar_balm", 4, 1, new(ChemicalExtract, 1), new(OrganicOil, 1), new("tin", 1));
            IndustryRecipe("pills_antibiotics", 6, 1, new(ChemicalExtract, 1), new(Biomass, 1), new("plastic", 1));
            IndustryRecipe("pills_painkillers", 6, 1, new(ChemicalExtract, 1), new(OrganicOil, 1), new("plastic", 1));
            IndustryRecipe("splint", 3, 1, new(Textile, 1), new("plastic", 1));
            IndustryRecipe("pills_osteodron", 6, 1, new(ReactiveMinerals, 1), new(ChemicalExtract, 1), new("plastic", 1));
            IndustryRecipe("pills_tranexam", 6, 1, new(ChemicalExtract, 1), new("acid", 1), new("plastic", 1));
            IndustryRecipe("pills_cryopentol", 6, 1, new("acid", 1), new("water_bottle_1", 1), new("plastic", 1));
            IndustryRecipe("pills_fibromine", 8, 1, new("pills_cryopentol", 1), new(ChemicalExtract, 1), new("glass", 1));
            IndustryRecipe("syringe_antidote", 7, 1, new(ChemicalExtract, 1), new("pills_sorbent", 1), new("glass", 1));
            IndustryRecipe("syringe_naloxone", 7, 1, new(ChemicalExtract, 1), new(OrganicOil, 1), new("plastic", 1));
            IndustryRecipe("tech_repair_kit", 8, 1, new("circuit_board", 1), new("transformer", 1), new("microelectronics_parts", 1));

            RegisterFoundationCatalog("iom_foundation_field_medicine_chip", "Field Medicine Schematic",
                "DaydreamChems", 2, 125, "goldenstar_balm", "pills_antibiotics", "pills_painkillers", "splint");
            RegisterFoundationCatalog("iom_foundation_clinical_medicine_chip", "Clinical Medicine Schematic",
                "DaydreamChems", 3, 175, "pills_osteodron", "pills_tranexam", "pills_cryopentol");
            RegisterFoundationCatalog("iom_foundation_specialist_medicine_chip", "Specialist Medicine Schematic",
                "DaydreamChems", 4, 225, "pills_fibromine", "syringe_antidote", "syringe_naloxone");
        }

        private static void RegisterMunitionsFoundations()
        {
            IndustryRecipe("nail_bolts_ammo", 1, 6, new ItemQuantity(Metal, 1));
            IndustryRecipe("battery_basic_ammo", 2, 6, new("glass", 1), new("acid", 1), new("wire", 1));
            IndustryRecipe("rifle_basic_ammo", 2, 6, new(Metal, 1), new("powder", 1));
            IndustryRecipe("rifle_armorpierce_ammo", 3, 6, new("armor_plates", 1), new("powder", 1));
            IndustryRecipe("toxic_ammo", 2, 6, new("acid", 1), new("plastic", 1));
            IndustryRecipe("frag_mine", 6, 1, new("armor_plates", 1), new("explosives", 1), new("circuit_board", 1));
            IndustryRecipe("smoke_grenade", 5, 1, new("tin", 1), new(ChemicalExtract, 1), new(CarbonFeedstock, 1));

            // RegisterSupportProduction supplies the existing chips at their established tiers.
        }

        private static void RegisterCyborgFoundations()
        {
            IndustryRecipe("cyborg_head", 24, 1, new(AutomataChassis, 1), new(LogicCore, 1), new("lens", 1));
            IndustryRecipe("cyborg_guts", 24, 1, new(AutomataChassis, 1), new(LogicCore, 1), new("hydraulic_pump", 1));
            IndustryRecipe("cyborg_feet", 16, 1, new(Metal, 1), new("rod_parts", 1), new("rubber", 1));
            IndustryRecipe("cyborg_hand", 16, 1, new(Fittings, 1), new("weapon_parts", 1), new("wire", 1));
            IndustryRecipe("cyborg_drill", 20, 1, new(Metal, 1), new("weapon_parts", 1), new("coarse_parts", 1));
            IndustryRecipe("cyborg_recreation_wrist", 18, 1, new(Fittings, 1), new("spring", 1), new("circuit_board", 1));
            IndustryRecipe("cyborg_recreation_leg", 22, 1, new(AutomataChassis, 1), new("hydraulic_pump", 1), new("rubber", 1));
            IndustryRecipe("cyborg_battle_chest", 28, 1, new(AutomataChassis, 1), new("armor_plates", 1), new(LogicCore, 1));
            IndustryRecipe("cyborg_battle_knee", 20, 1, new("rod_parts", 1), new("metal_tread", 1), new("ceramite_plates", 1));

            // Native and existing support chips retain control of finished cyborg recipes.
        }

        private static void RegisterFoundationCatalog(string id, string title, string faction, int tier, int price,
            params string[] outputs)
        {
            var chip = CloneItem<DatadiskRecord>(id, "low_chip", tier, price, FieldPlans);
            chip.Categories = [faction];
            // Each chip rolls one recipe; native starter and workbench access remain available.
            RegisterCatalog(chip, title, "Fabrication from loose refined materials and assembled components.",
                faction, outputs, restrictOtherSources: false);
        }
    }
}

using MGSC;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private const string MedicalComponents = "medical_components";
        private const string PharmaceuticalCargo = "drug_cargo";

        private static void RegisterPharmaceuticalWorkshops()
        {
            const string daydream = "DaydreamChems";
            const string sundog = "Sundog";
            const string eStation = "EStation";
            const string clinic = "AvicennasClinic";
            const string moonTrade = "MoonTradeCenter";

            AddFactionWorkshop(daydream, sundog, 1, "iom_sundog_medical_feedstock", 120,
                [new("biomass_tank", 2), new("water_container", 1)], [new(MedicalComponents, 8)]);
            AddFactionWorkshop(daydream, sundog, 2, "iom_sundog_pharmaceutical_cargo", 132,
                [new(MedicalComponents, 2), new("biomass_tank", 1)], [new(PharmaceuticalCargo, 8)]);
            AddFactionWorkshop(daydream, "Paragon", 1, "iom_paragon_medical_glue", 96,
                [new(MedicalComponents, 1), new(PolymerStock, 1)], [new("medical_glue", 10)]);
            AddFactionWorkshop("PlanetBridge", "Mammoth", 1, "iom_mammoth_glass_sorting", 72,
                [new("glass", 3)], [new(Cullet, 8)]);
            AddFactionWorkshop("PlanetBridge", "Mammoth", 1, "iom_mammoth_glassworks", 96,
                [new(Cullet, 3)], [new(GlassStock, 8)]);

            AddPharmaceuticalLine(daydream, eStation, moonTrade, 2, "pills_antibiotics",
                [new(MedicalComponents, 2), new("water_container", 1)]);
            AddPharmaceuticalLine(daydream, eStation, moonTrade, 2, "pills_painkillers",
                [new(MedicalComponents, 2), new(PharmaceuticalCargo, 1)]);
            AddPharmaceuticalLine(daydream, eStation, moonTrade, 3, "pills_antiradin",
                [new(MedicalComponents, 2), new("pills_sorbent", 1)]);
            AddPharmaceuticalIssue(daydream, moonTrade, 1, "pills_sorbent");
            AddPharmaceuticalIssue(daydream, moonTrade, 2, "pills_osteodron");
            AddPharmaceuticalIssue(daydream, moonTrade, 2, "pills_tranexam");

            AddPharmaceuticalLine(daydream, clinic, moonTrade, 2, "syringe_bloodbag",
                [new(MedicalComponents, 2), new("water_container", 1), new(PolymerStock, 1)]);
            AddPharmaceuticalLine(daydream, clinic, moonTrade, 3, "syringe_antidote",
                [new(PharmaceuticalCargo, 1), new(Treatment, 1), new(GlassStock, 1)]);

            AddPharmaceuticalLine("FrancheComte", "QiHelium", "ShuCenter", 2, "syringe_naloxone",
                [new(MedicalComponents, 2), new(PolymerStock, 1)]);
            AddPharmaceuticalLine("FrancheComte", "QiHelium", "ShuCenter", 3, "syringe_morphine",
                [new(PharmaceuticalCargo, 1), new(GlassStock, 1)]);

            AddPharmaceuticalLine("Grasshopper", "Kyoko", "ShepherdsArc", 2, "pills_steroids",
                [new(PharmaceuticalCargo, 1), new(MedicalComponents, 1)]);
            AddPharmaceuticalLine("Grasshopper", "Kyoko", "ShepherdsArc", 6, "syringe_adrenaline",
                [new(PharmaceuticalCargo, 1), new("pills_steroids", 1), new(GlassStock, 1)]);

            AddPharmaceuticalLine("Sunlight", "DarkFlora", "Stratagem", 3, "pills_cryopentol",
                [new(MedicalComponents, 2), new(PharmaceuticalCargo, 1)]);
            AddPharmaceuticalLine("Sunlight", "DarkFlora", "Stratagem", 4, "pills_fibromine",
                [new(PharmaceuticalCargo, 1), new("pills_cryopentol", 1), new(GlassStock, 1)]);

            AddPharmaceuticalLine("SBN", "CathodeRampart", "HumankindsHope", 6, "syringe_nanoinjection",
                [new(ImplantController, 1), new(Clinical, 1), new(PolymerStock, 1)]);
        }

        private static void AddPharmaceuticalLine(string faction, string factory, string depot, int tier, string product,
            ItemQuantity[] inputs)
        {
            AddFactionWorkshop(faction, factory, tier, "iom_pharma_" + product + "_assembly", 96 + tier * 12,
                [.. inputs], [new(product, 8)]);
            AddPharmaceuticalIssue(faction, depot, tier, product);
        }

        private static void AddPharmaceuticalIssue(string faction, string depot, int tier, string product) =>
            AddFactionWorkshop(faction, depot, tier, "iom_pharma_" + product + "_issue", 240, [new(product, 1)], []);
    }
}

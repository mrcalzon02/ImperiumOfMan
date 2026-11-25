using MGSC;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private const string RealWareOreMine = "Lumina";

        private static void RegisterBaseGameWorkshops()
        {
            RegisterCivilianWorkshops();
            RegisterDaydreamWorkshops();
            RegisterArmamentWorkshops();

            AddFactionWorkshop("SBN", "Boethius", 2, "iom_boethius_electronics", 96,
                [new("electrical_parts_container", 1)],
                [new("capacitor_parts", 6), new("microelectronics_parts", 6), new("circuit_board", 6)]);
            AddFactionWorkshop("SBN", "Boethius", 3, "iom_boethius_control_modules", 120,
                [new("microelectronics_parts", 1), new("capacitor_parts", 1), new("circuit_board", 1)],
                [new(AiModule, 6)]);
            AddFactionWorkshop("SBN", "Skylab", 3, "iom_skylab_research_stations", 144,
                [new(AiModule, 2)], [new("research_disk", 6)]);
        }

        private static void RegisterCivilianWorkshops()
        {
            const string church = "ChurchRevelation";
            const string freight = "PlanetBridge";
            AddFactionWorkshop(church, "PineGapTraining", 1, "iom_pine_gap_hydroponics", 96,
                [new("water_container", 2)], [new("biomass_tank", 10)]);
            AddFactionWorkshop(church, "SterlingsFarm", 1, "iom_sterlings_ration_packing", 108,
                [new("biomass_tank", 2), new("water_container", 1), new("plastic", 1)], [new("ration_pack_1", 12)]);
            AddFactionWorkshop(church, "PlatosAcademy", 1, "iom_platos_ration_issue", 180,
                [new("ration_pack_1", 3)], [], powerGain: 1);

            AddFactionWorkshop(freight, "Mammoth", 1, "iom_mammoth_fiber_sorting", 60,
                [new("rags", 3)], [new(Fiber, 8)]);
            AddFactionWorkshop(freight, "Mammoth", 1, "iom_mammoth_textile_mills", 72,
                [new(Fiber, 3)], [new(Textile, 10)]);
            AddFactionWorkshop(freight, "Mammoth", 1, "iom_mammoth_polymer_recovery", 72,
                [new("plastic", 3)], [new(PolymerStock, 10)]);
        }

        private static void RegisterDaydreamWorkshops()
        {
            const string daydream = "DaydreamChems";
            const string clinic = "AvicennasClinic";
            AddFactionWorkshop(daydream, "EStation", 1, "iom_estation_treatment_packs", 96,
                [new("biomass_tank", 1), new("medical_components", 1)], [new(Treatment, 8), new("pills_sorbent", 6)]);
            AddFactionWorkshop(daydream, "EStation", 2, "iom_estation_clinical_stock", 144,
                [new("medical_components", 1)], [new("pills_osteodron", 6), new("pills_tranexam", 6)]);
            AddFactionWorkshop(daydream, "Paragon", 1, "iom_paragon_sterile_dressings", 72,
                [new(Textile, 1), new("medical_glue", 1)], [new(Dressing, 8)]);
            AddFactionWorkshop(daydream, "Paragon", 1, "iom_paragon_dressing_trays", 72,
                [new(Dressing, 1), new(PolymerStock, 1)], [new(Tray, 8)]);
            AddFactionWorkshop(daydream, "Paragon", 1, "iom_paragon_field_medkits", 96,
                [new(Tray, 1), new(Treatment, 1), new("splint", 1)], [new("medical_kit_1", 8)]);
            AddFactionWorkshop(daydream, clinic, 2, "iom_avicenna_clinical_refills", 120,
                [new(Treatment, 1), new("pills_osteodron", 1), new("pills_tranexam", 1)], [new(Clinical, 6)]);
            AddFactionWorkshop(daydream, clinic, 2, "iom_avicenna_sterile_instruments", 120,
                [new(Metal, 1), new(Dressing, 1), new("medical_glue", 1)], [new(Instruments, 6)]);
            AddFactionWorkshop(daydream, clinic, 2, "iom_avicenna_advanced_medkits", 144,
                [new("medical_kit_1", 1), new(Clinical, 1)], [new("medical_kit_2", 6)]);
            AddFactionWorkshop(daydream, clinic, 2, "iom_avicenna_surgical_kits", 144,
                [new("medical_kit_1", 1), new(Instruments, 1), new("syringe_naloxone", 1)], [new("surgical_kit", 6)]);
            AddFactionWorkshop(daydream, "MoonTradeCenter", 2, "iom_moon_trade_clinic", 180,
                [new("medical_kit_2", 1), new("surgical_kit", 1)], [], powerGain: 1);
        }

        private static void RegisterArmamentWorkshops()
        {
            const string realware = "RealWare";
            AddFactionWorkshop(realware, "Clotho", 1, "iom_clotho_ore_refinery", 72,
                [new("ore_cargo", 2)], [new(Metal, 12)]);
            AddFactionWorkshop(realware, "Clotho", 1, "iom_clotho_plate_mill", 84,
                [new(Metal, 3)], [new(Sheet, 8)]);
            AddFactionWorkshop(realware, "Clotho", 1, "iom_clotho_pressed_inserts", 96,
                [new(Sheet, 2)], [new(Plate, 8)]);
            AddFactionWorkshop(realware, "ArcadisTerminal", 1, "iom_arcadis_armor_liners", 96,
                [new(Textile, 1)], [new(Liner, 6)]);
            AddFactionWorkshop(realware, "ArcadisTerminal", 3, "iom_arcadis_protective_armor", 168,
                [new(Plate, 3), new(Liner, 2), new("engineering_parts_container", 1)],
                [new("rwa_light_armor_1", 4), new("rwa_light_helmet_1", 4),
                    new("rwa_light_pants_1", 4), new("rwa_light_boots_1", 4)]);
            AddFactionWorkshop(realware, "HISAres", 3, "iom_ares_armor_issue", 180,
                [new("rwa_light_armor_1", 1), new("rwa_light_helmet_1", 1),
                    new("rwa_light_pants_1", 1), new("rwa_light_boots_1", 1)], [], powerGain: 1);

            const string ancom = "AnCom";
            const string arsenal = "Akatsuki";
            AddFactionWorkshop(ancom, arsenal, 1, "iom_akatsuki_cartridge_cases", 72,
                [new(Metal, 1)], [new(Case, 8)]);
            AddFactionWorkshop(ancom, arsenal, 1, "iom_akatsuki_payload_modules", 72,
                [new("powder", 1)], [new(Module, 8)]);
            AddFactionWorkshop(ancom, arsenal, 1, "iom_akatsuki_medium_cartridges", 96,
                [new(Case, 2), new(Module, 2)], [new("medium_basic_ammo", 24)]);
            AddFactionWorkshop(ancom, arsenal, 2, "iom_akatsuki_shell_cases", 96,
                [new(PolymerStock, 1), new(Metal, 1)], [new(Shell, 8)]);
            AddFactionWorkshop(ancom, arsenal, 2, "iom_akatsuki_flechette_modules", 96,
                [new("nail_bolts_ammo", 4), new("powder", 1)], [new(Flechette, 6)]);
            AddFactionWorkshop(ancom, arsenal, 2, "iom_akatsuki_flechette_shells", 120,
                [new(Shell, 2), new(Flechette, 2)], [new("shotgun_flechette_ammo", 16)]);
            AddFactionWorkshop(ancom, "HISUnveiled", 1, "iom_unveiled_cartridge_issue", 180,
                [new("medium_basic_ammo", 4)], [], powerGain: 1);
            AddFactionWorkshop(ancom, "HISUnveiled", 2, "iom_unveiled_flechette_issue", 180,
                [new("shotgun_flechette_ammo", 4)], [], powerGain: 1);
        }
    }
}

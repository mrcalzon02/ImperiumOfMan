using MGSC;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private const string WinterBay = "WinterBay";

        private static void RegisterMaintenanceWorkshops()
        {
            RegisterMechanicalSupplyWorkshops();
            RegisterPrecisionSupplyWorkshops();
            RegisterRepairKitWorkshops();
        }

        private static void RegisterMechanicalSupplyWorkshops()
        {
            AddFactionWorkshop(PlanetBridge, "Epitome", 1, "iom_epitome_mechanical_spares", 96,
                [new(Metal, 2), new(PolymerStock, 1)],
                [new("coarse_parts", 8), new("rod_parts", 8), new("spring", 8), new("metal_tread", 8)]);
            AddFactionWorkshop(PlanetBridge, WinterBay, 1, "iom_winterbay_engineering_crates", 144,
                [new("coarse_parts", 2), new("rod_parts", 2), new("spring", 1)], [new("engineering_parts_container", 6)]);
            AddFactionWorkshop(PlanetBridge, WinterBay, 1, "iom_winterbay_welding_tools", 168,
                [new("rod_parts", 1), new("metal_tread", 1), new("wire", 1)], [new("welding_machine_device", 6)],
                powerGain: 2, techGain: 1);
            AddFactionWorkshop(PlanetBridge, "Mammoth", 1, "iom_mammoth_tool_crates", 120,
                [new("engineering_parts_container", 1), new("metal_tread", 2), new("wire", 1)],
                [new("tool_container", 6)]);
            AddFactionWorkshop(PlanetBridge, "Cryptoria", 3, "iom_cryptoria_robot_salvage", 144,
                [new("robot_parts_container", 2)], [new("rod_parts", 6), new("microelectronics_parts", 6)]);
            AddFactionWorkshop("RealWare", "Zephyr", 2, "iom_zephyr_weapon_spares", 108,
                [new(Metal, 1), new("rod_parts", 1)], [new("weapon_parts", 10)]);
        }

        private static void RegisterPrecisionSupplyWorkshops()
        {
            AddFactionWorkshop("SBN", "VanDeGraaff", 2, "iom_van_de_graaff_optics", 96,
                [new("glass", 2), new(PolymerStock, 1)], [new("lens", 8)]);
            AddFactionWorkshop("SBN", "VanDeGraaff", 2, "iom_van_de_graaff_transformers", 108,
                [new("wire", 2), new("capacitor_parts", 1)], [new("transformer", 8)]);
            AddFactionWorkshop("SBN", "AresPost", 2, "iom_ares_post_wiring_recovery", 120,
                [new("electrical_parts_container", 1)], [new("wire", 12)]);

            AddMaintenanceLine("SBN", "AresPost", "HumankindsHope", 2, "automap", 144,
                new("lens", 1), new("circuit_board", 1), new("microelectronics_parts", 1));
            AddMaintenanceLine("SBN", "AresPost", "HumankindsHope", 3, "quasi_sensor_device", 144,
                new("transformer", 1), new("lens", 1), new("microelectronics_parts", 1));
        }

        private static void RegisterRepairKitWorkshops()
        {
            AddMaintenanceLine(CoreWard, "Rachmaninoff", "HISWoodpecker", 2, "melee_repair_kit", 144,
                new("rod_parts", 1), new("coarse_parts", 1), new("spring", 1));
            AddMaintenanceLine(CoreWard, "Rachmaninoff", "HISWoodpecker", 3, "engineering_repair_kit", 144,
                new("engineering_parts_container", 1), new("metal_tread", 1));
            AddMaintenanceLine("RealWare", "ArcadisTerminal", "HISAres", 2, "firearm_repair_kit", 144,
                new("weapon_parts", 2), new("spring", 1));
            AddMaintenanceLine("RealWare", "ArcadisTerminal", "HISAres", 3, "armor_repair_kit", 168,
                new(Plate, 1), new(Textile, 1), new(Fittings, 1));
            AddMaintenanceLine("SBN", "AresPost", "HumankindsHope", 3, "tech_repair_kit", 168,
                new("circuit_board", 1), new("transformer", 1), new("microelectronics_parts", 1));
        }

        private static void AddMaintenanceLine(string faction, string factory, string depot, int tier,
            string product, int hours, params ItemQuantity[] inputs)
        {
            AddFactionWorkshop(faction, factory, tier, "iom_" + product + "_assembly", hours,
                [.. inputs], [new(product, 6)]);
            AddFactionWorkshop(faction, depot, tier, "iom_" + product + "_issue", 216,
                [new(product, 1)], [], powerGain: 1);
        }
    }
}

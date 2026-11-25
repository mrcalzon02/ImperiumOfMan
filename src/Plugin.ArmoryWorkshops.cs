using System;
using System.Linq;
using MGSC;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private static void RegisterArmoryWorkshops()
        {
            AddFactionWorkshop("RealWare", "Zephyr", 4, "iom_zephyr_ceramite", 144,
                [new(Plate, 2), new("glass", 2)], [new("ceramite_plates", 8)]);
            AddFactionWorkshop(PlanetBridge, "Budapest", 1, "iom_budapest_armor_textiles", 120,
                [new(Textile, 2), new(PolymerStock, 1)], [new(Flak, 8), new(Liner, 8)]);
            AddFactionWorkshop("Tianming", "RedTiangong", 2, "iom_tiangong_alloy_plates", 120,
                [new(Metal, 2), new(Fittings, 1)], [new("armor_plates", 8)]);
            AddFactionWorkshop("DaydreamChems", "Paragon", 1, "iom_paragon_rubber_stock", 108,
                [new(PolymerStock, 2)], [new("rubber", 10)]);
            AddFactionWorkshop("DaydreamChems", "Sundog", 1, "iom_sundog_toxic_cells", 144,
                [new("acid", 1), new("drug_cargo", 1)], [new("toxic_ammo", 24)]);

            RegisterHexarchyArmories();
            RegisterCorporateArmories();
            RegisterIndependentArmories();
            RegisterSuccessorArmories();
        }

        private static void RegisterHexarchyArmories()
        {
            AddArmoryLine("AnCom", "Akatsuki", "HISUnveiled", "service_pistol",
                [new(Metal, 2), new("weapon_parts", 2), new("spring", 2)], "anc_pistol_1");
            AddArmoryLine("AnCom", "Akatsuki", "HISUnveiled", "marksman_rifle",
                [new(Sheet, 2), new("weapon_parts", 3), new("lens", 2)], "anc_marksman_1");
            AddArmoryLine("AnCom", "KrakensHub", "HISUnveiled", "agent_armor",
                [new(Flak, 3), new(Textile, 2), new(Fittings, 2)],
                "anc_agent_armor_1", "anc_agent_mask_1", "anc_agent_pants_1", "anc_agent_boots_1");
            AddArmoryLine("AnCom", "KrakensHub", "HISUnveiled", "medium_armor",
                [new(HeavyPlate, 3), new(Flak, 3), new(Liner, 2)],
                "anc_medium_armor_1", "anc_medium_helmet_1", "anc_medium_pants_1", "anc_medium_boots_1");

            AddArmoryLine("RealWare", "ArcadisTerminal", "HISAres", "military_shotgun",
                [new(Metal, 2), new("weapon_parts", 3), new("rod_parts", 2)], "military_shotgun_1");
            AddArmoryLine("RealWare", "ArcadisTerminal", "HISAres", "heavy_machine_gun",
                [new(HeavyPlate, 3), new("weapon_parts", 5), new("metal_tread", 2)], "rwa_military_hmg_1");
            AddArmoryLine("RealWare", "Malmo", "HISAres", "heavy_armor",
                [new(HeavyPlate, 4), new("ceramite_plates", 3), new(Liner, 3)],
                "rwa_heavy_armor_1", "rwa_heavy_helmet_1", "rwa_heavy_pants_1", "rwa_heavy_boots_1");
            AddArmoryLine("RealWare", "Malmo", "HISAres", "power_armor",
                [new(HeavyPlate, 5), new("ceramite_plates", 4), new("hydraulic_pump", 2), new(LogicCore, 2)],
                "rwa_power_armor_1", "rwa_power_helmet_1", "rwa_power_pants_1", "rwa_power_boots_1");

            AddArmoryLine("SBN", "Skylab", "HumankindsHope", "rail_sidearm",
                [new(Plate, 2), new("capacitor_parts", 2), new("wire", 2)], "sbn_rail_pistol_1");
            AddArmoryLine("SBN", "Skylab", "HumankindsHope", "rail_rifle",
                [new(HeavyPlate, 2), new("transformer", 2), new("capacitor_parts", 3), new("lens", 2)], "sbn_rail_assault_1");
            AddArmoryLine("SBN", "Pertwee", "HumankindsHope", "medium_armor",
                [new(Plate, 3), new(Liner, 2), new("circuit_board", 2)],
                "sbn_medium_armor_1", "sbn_medium_helmet_1", "sbn_medium_pants_1", "sbn_medium_boots_1");
            AddArmoryLine("SBN", "Pertwee", "HumankindsHope", "heavy_armor",
                [new("ceramite_plates", 4), new(Liner, 3), new(LogicCore, 2)],
                "sbn_heavy_armor_1", "sbn_heavy_helmet_1", "sbn_heavy_pants_1", "sbn_heavy_boots_1");

            AddArmoryLine("Tianming", "RedTiangong", "Oracle", "disc_sidearm",
                [new(Sheet, 2), new("rod_parts", 2), new("spring", 2)], "tia_disc_pistol_1");
            AddArmoryLine("Tianming", "RedTiangong", "Oracle", "shock_rifle",
                [new("armor_plates", 2), new("transformer", 3), new("circuit_board", 2), new("lens", 2)], "tia_shock_assault_1");
            AddArmoryLine("Tianming", "RedTiangong", "Oracle", "heavy_armor",
                [new("armor_plates", 4), new("rubber", 2), new(Liner, 2)],
                "tia_heavy_armor_1", "tia_heavy_helmet_1", "tia_heavy_pants_1", "tia_heavy_boots_1");
            AddArmoryLine("Tianming", "RedTiangong", "Oracle", "light_armor",
                [new("ceramite_plates", 3), new(Flak, 3), new("microelectronics_parts", 2)],
                "tia_light_armor_1", "tia_light_helmet_1", "tia_light_pants_1", "tia_light_boots_1");

            AddArmoryLine("Dilthey", "Nox", "ZeroGravity", "sound_sidearm",
                [new(PolymerStock, 2), new("wire", 2), new("capacitor_parts", 2)], "dil_sound_pistol_1");
            AddArmoryLine("Dilthey", "Nox", "ZeroGravity", "ion_smg",
                [new(Sheet, 2), new("transformer", 3), new("microelectronics_parts", 2)], "dil_ion_smg_1");
            AddArmoryLine("Dilthey", "AnCapDogma", "ZeroGravity", "laboratory_armor",
                [new(Textile, 3), new("rubber", 3), new("glass", 2)],
                "science_chem_shirt_1", "science_chem_helmet_1", "science_chem_pants_1", "science_chem_boots_1");
            AddArmoryLine("Dilthey", "AnCapDogma", "ZeroGravity", "light_armor",
                [new("ceramite_plates", 3), new(Liner, 2), new("microelectronics_parts", 3)],
                "dil_light_armor_1", "dil_light_helmet_1", "dil_light_pants_1", "dil_light_boots_1");
        }

        private static void RegisterCorporateArmories()
        {
            AddArmoryLine("DaydreamChems", "Sundog", "MechtaShooting", "toxic_pistol",
                [new(PolymerStock, 2), new("weapon_parts", 1), new("toxic_ammo", 4)], "ddr_toxic_pistol_1");
            AddArmoryLine("DaydreamChems", "Sundog", "MechtaShooting", "disruptor_rifle",
                [new("ceramite_plates", 2), new("transformer", 3), new(LogicCore, 2), new("lens", 2)], "ddr_disruptor_marksman_1");
            AddArmoryLine("DaydreamChems", "Paragon", "MechtaShooting", "chemical_armor",
                [new(Textile, 3), new("rubber", 4), new("glass", 2)],
                "ddr_chem_coat_1", "ddr_chem_helmet_1", "ddr_chem_pants_1", "ddr_chem_boots_1");
            AddArmoryLine("DaydreamChems", "BillGregor", "MechtaShooting", "power_armor",
                [new("ceramite_plates", 4), new("rubber", 4), new("hydraulic_pump", 2), new(LogicCore, 2)],
                "ddr_power_armor_1", "ddr_power_helmet_1", "ddr_power_pants_1", "ddr_power_boots_1");

            AddArmoryLine(PlanetBridge, "Budapest", "Charon", "rail_sidearm",
                [new(Metal, 2), new("wire", 1), new("capacitor_parts", 1)], "rail_pistol_1");
            AddArmoryLine(PlanetBridge, "Budapest", "Charon", "cargo_machine_gun",
                [new(HeavyPlate, 3), new("weapon_parts", 4), new("coarse_parts", 2)], "plb_hmg_1");
            AddArmoryLine(PlanetBridge, "Epitome", "Charon", "laborer_armor",
                [new(Flak, 3), new(Liner, 2), new(Fittings, 2)],
                "laborer_medium_armor_1", "laborer_medium_helmet_1", "laborer_medium_pants_1", "laborer_medium_boots_1");
            AddArmoryLine(PlanetBridge, "Epitome", "Charon", "cargo_power_armor",
                [new(HeavyPlate, 4), new("metal_tread", 3), new("hydraulic_pump", 3), new(LogicCore, 2)],
                "plb_power_armor_1", "plb_power_helmet_1", "plb_power_pants_1", "plb_power_boots_1");

            AddArmoryLine("Grasshopper", "Canaan", "Escher", "compact_smg",
                [new(Metal, 2), new("weapon_parts", 2), new(PolymerStock, 2)], "grh_common_smg_1");
            AddArmoryLine("Grasshopper", "Canaan", "Escher", "microwave_rifle",
                [new("ceramite_plates", 2), new("transformer", 3), new(AiModule, 2), new("lens", 2)], "grh_microwave_marksman_1");
            AddArmoryLine("Grasshopper", "HartsWarehouse", "Escher", "winter_clothing",
                [new(Textile, 4), new(Liner, 3), new("rubber", 2)],
                "laborer_warm_shirt_1", "laborer_warm_hat_1", "laborer_warm_pants_1", "laborer_warm_boots_1");
            AddArmoryLine("Grasshopper", "HartsWarehouse", "Escher", "light_armor",
                [new(Flak, 3), new(Plate, 3), new("spring", 2)],
                "grh_light_armor_1", "grh_light_helmet_1", "grh_light_pants_1", "grh_light_boots_1");

            AddArmoryLine("Sunlight", "Polaris", "Stratagem", "laser_sidearm",
                [new(PolymerStock, 2), new("capacitor_parts", 2), new("lens", 2)], "sun_laser_pistol_1");
            AddArmoryLine("Sunlight", "Polaris", "Stratagem", "cryolaser",
                [new(HeavyPlate, 3), new("lens", 3), new("transformer", 2), new("pills_fibromine", 2)], "sun_cryolaser_hmg_1");
            AddArmoryLine("Sunlight", "Zeus", "Stratagem", "medium_armor",
                [new(Plate, 4), new(Liner, 2), new("glass", 2)],
                "sun_medium_armor_1", "sun_medium_helmet_1", "sun_medium_pants_1", "sun_medium_boots_1");
            AddArmoryLine("Sunlight", "Zeus", "Stratagem", "power_armor",
                [new("ceramite_plates", 4), new("capacitor_parts", 4), new(LogicCore, 2), new("hydraulic_pump", 2)],
                "sun_power_armor_1", "sun_power_helmet_1", "sun_power_pants_1", "sun_power_boots_1");

            AddArmoryLine("FrancheComte", "QiHelium", "ShuCenter", "patrol_sidearm",
                [new(Metal, 2), new("weapon_parts", 2), new("spring", 2)], "fra_security_pistol_1");
            AddArmoryLine("FrancheComte", "QiHelium", "ShuCenter", "shock_smg",
                [new(Sheet, 2), new("transformer", 3), new("rubber", 2)], "fra_shock_smg_1");
            AddArmoryLine("FrancheComte", "ChickMoo", "ShuCenter", "patrol_armor",
                [new(Flak, 3), new(Liner, 2), new(Plate, 2)],
                "police_medium_armor_1", "police_medium_helmet_1", "police_medium_pants_1", "police_medium_boots_1");
            AddArmoryLine("FrancheComte", "ChickMoo", "ShuCenter", "riot_armor",
                [new(HeavyPlate, 3), new(Flak, 2), new("rubber", 3)],
                "fra_heavy_armor_1", "fra_heavy_mask_1", "fra_heavy_pants_1", "fra_heavy_boots_1");

            AddArmoryLine("ChurchRevelation", "Tycho", "Deepsky", "fire_club",
                [new("rod_parts", 3), new("gas_ammo", 4), new("rubber", 2)], "chu_fireclub_1");
            AddArmoryLine("ChurchRevelation", "Tycho", "Deepsky", "plasma_rifle",
                [new("ceramite_plates", 3), new("capacitor_parts", 3), new("lens", 2)], "chu_plasma_sniper_1");
            AddArmoryLine("ChurchRevelation", "Gloria", "Deepsky", "manager_clothing",
                [new(Textile, 4), new(Liner, 2), new("glass", 1)],
                "chu_manager_shirt_1", "chu_manager_mask_1", "chu_manager_pants_1", "chu_manager_boots_1");
            AddArmoryLine("ChurchRevelation", "Gloria", "Deepsky", "power_armor",
                [new(HeavyPlate, 4), new("ceramite_plates", 3), new("hydraulic_pump", 2), new(LogicCore, 2)],
                "chu_power_armor_1", "chu_power_helmet_1", "chu_power_pants_1", "chu_power_boots_1");

            AddArmoryLine(CoreWard, "Rachmaninoff", "HISWoodpecker", "worker_pistol",
                [new(Metal, 2), new("rod_parts", 2), new("spring", 2)], "cor_worker_pistol_1");
            AddArmoryLine(CoreWard, "Rachmaninoff", "HISWoodpecker", "heavy_machine_gun",
                [new(HeavyPlate, 3), new("metal_tread", 3), new("weapon_parts", 4)], "cor_worker_hmg_2");
            AddArmoryLine(CoreWard, SampoQuern, "HISWoodpecker", "mining_armor",
                [new(Plate, 3), new(Liner, 3), new("metal_tread", 2)],
                "miner_medium_armor_1", "miner_medium_helmet_1", "miner_medium_pants_1", "miner_medium_boots_1");
            AddArmoryLine(CoreWard, SampoQuern, "HISWoodpecker", "mining_power_armor",
                [new(HeavyPlate, 4), new("hydraulic_pump", 3), new("metal_tread", 3), new(LogicCore, 2)],
                "miner_power_armor_1", "miner_power_helmet_1", "miner_power_pants_1", "miner_power_boots_1");
        }

        private static void RegisterIndependentArmories()
        {
            const string resistance = "CResistance";
            AddFactionWorkshop(resistance, "Lightbay", 1, "iom_lightbay_ore_refinery", 108,
                [new("ore_cargo", 2)], [new(Metal, 12)]);
            AddFactionWorkshop(resistance, "Purgatory", 1, "iom_purgatory_arsenal_parts", 108,
                [new(Metal, 2), new("rusty_plates", 2)], [new(Plate, 8), new("weapon_parts", 8), new("spring", 8)]);
            AddFactionWorkshop(resistance, "RogueCity", 1, "iom_rogue_city_textile_recovery", 96,
                [new("rags", 3)], [new(Textile, 8), new(Liner, 6)]);
            AddFactionWorkshop(resistance, "TeslaRing", 4, "iom_tesla_ring_armory_electronics", 144,
                [new("precious_metals", 1), new("wire", 2)], [new("circuit_board", 8), new("capacitor_parts", 8)]);
            AddArmoryLine(resistance, "Warden", "FlatObsidian", "militia_sidearm",
                [new(Metal, 2), new("weapon_parts", 2), new("spring", 1)], "civ_pistol_2");
            AddArmoryLine(resistance, "Warden", "FlatObsidian", "plasma_rifle",
                [new(Plate, 3), new("capacitor_parts", 3), new("circuit_board", 2)], "civ_plasma_assault_1");
            AddArmoryLine(resistance, "Warden", "FlatObsidian", "militia_armor",
                [new(Plate, 3), new(Textile, 2), new(Liner, 2)],
                "civ_medium_armor_2", "civ_medium_helmet_2", "civ_medium_pants_2", "civ_medium_boots_2");
            AddArmoryLine(resistance, "Warden", "FlatObsidian", "heavy_armor",
                [new(Plate, 6), new(Liner, 4), new("circuit_board", 2)],
                "civ_heavy_armor_1", "civ_heavy_helmet_1", "civ_heavy_pants_1", "civ_heavy_boots_1");

            const string belt = "UnchainedBelt";
            AddFactionWorkshop(belt, "Schwerin", 1, "iom_schwerin_scrap_sorting", 108,
                [new("rusty_plates", 3), new("rags", 2)], [new(Metal, 8), new(Textile, 8), new("weapon_parts", 8)]);
            AddArmoryLine(belt, "Bethlehem", "Hargeysa", "boarder_smg",
                [new(Metal, 2), new("weapon_parts", 2), new("spring", 1)], "unc_smg_2");
            AddArmoryLine(belt, "Bethlehem", "Hargeysa", "autogun",
                [new(HeavyPlate, 3), new("weapon_parts", 5), new("coarse_parts", 3)], "unc_autogun_1");
            AddArmoryLine(belt, "Bethlehem", "Hargeysa", "boarder_armor",
                [new(Metal, 3), new(Textile, 3), new("rubber", 2)],
                "unc_medium_armor_1", "unc_medium_helmet_1", "unc_medium_pants_1", "unc_medium_boots_1");
            AddArmoryLine(belt, "Bethlehem", "Hargeysa", "light_armor",
                [new(Flak, 4), new(Textile, 3), new("ceramite_plates", 2)],
                "unc_light_armor_1", "unc_light_helmet_1", "unc_light_pants_1", "unc_light_boots_1");

            const string tezctlan = "Tezctlan";
            AddFactionWorkshop(tezctlan, "FrescoPlatforms", 1, "iom_fresco_quasi_harvest", 144, [], [new("quasisamples", 8)]);
            AddFactionWorkshop(tezctlan, "Oddibjord", 2, "iom_oddibjord_carapace_stock", 120,
                [new("quasisamples", 2)], [new("venus_armor_plates", 8), new("venus_weapon_parts", 8), new("quasi_venus_rags", 8)]);
            AddArmoryLine(tezctlan, "IbsaPrison", "FeatheredTemple", "ritual_club",
                [new("venus_weapon_parts", 3), new("venus_armor_plates", 1)], "venus_club_1");
            AddArmoryLine(tezctlan, "IbsaPrison", "FeatheredTemple", "quasi_shotgun",
                [new("venus_weapon_parts", 3), new("quasi_venus_rags", 2)], "venus_quasi_shotgun_1");
            AddArmoryLine(tezctlan, "IbsaPrison", "FeatheredTemple", "rocket_launcher",
                [new("venus_weapon_parts", 4), new("venus_armor_plates", 4), new("quasisamples", 2)], "venus_rocketlauncher_1");
            AddArmoryLine(tezctlan, "Oddibjord", "FeatheredTemple", "carapace_armor",
                [new("venus_armor_plates", 3), new("quasi_venus_rags", 2)],
                "tez_heavy_armor_1", "tez_heavy_helmet_1", "tez_heavy_pants_1", "tez_heavy_boots_1");
        }

        private static void RegisterSuccessorArmories()
        {
            AddFactionWorkshop("XiomaraMasks", "FrescoPlatforms", 1, "iom_xiomara_quasi_harvest", 144, [], [new("quasisamples", 8)]);
            AddFactionWorkshop("XiomaraMasks", "Oddibjord", 2, "iom_xiomara_carapace_stock", 120,
                [new("quasisamples", 2)], [new("venus_armor_plates", 8), new("venus_weapon_parts", 8), new("quasi_venus_rags", 8)]);
            AddArmoryLine("XiomaraMasks", "Oddibjord", "FeatheredTemple", "blowpipe",
                [new("venus_weapon_parts", 3), new("quasi_venus_rags", 2)], "venus_blowpipe_1");
            AddArmoryLine("XiomaraMasks", "Oddibjord", "FeatheredTemple", "mask_armor",
                [new("venus_armor_plates", 3), new("quasi_venus_rags", 2)],
                "xio_light_armor_1", "xio_light_helmet_1", "xio_pants_1", "xio_light_boots_1");

            AddFactionWorkshop("Emeraldlight", "Zeus", 3, "iom_emeraldlight_optics", 132,
                [new("quasisamples", 1), new("glass", 2)],
                [new("lens", 6), new("capacitor_parts", 6), new("wire", 8)]);
            AddArmoryLine("Emeraldlight", "Polaris", "Stratagem", "cryolaser",
                [new("venus_weapon_parts", 2), new("capacitor_parts", 2), new("lens", 2)], "sun_cryolaser_pistol_1");
            AddArmoryLine("Emeraldlight", "Polaris", "Stratagem", "medium_armor",
                [new("venus_armor_plates", 4), new("quasi_venus_rags", 3), new("wire", 2)],
                "sun_medium_armor_1", "sun_medium_helmet_1", "sun_medium_pants_1", "sun_medium_boots_1");

            AddFactionWorkshop("ChurchGannix", "Gloria", 3, "iom_gannix_ritual_spares", 132,
                [new("venus_weapon_parts", 1), new("glass", 2)], [new("rod_parts", 6), new("lens", 6)]);
            AddArmoryLine("ChurchGannix", "Gloria", "Deepsky", "ritual_sniper",
                [new("venus_weapon_parts", 3), new("rod_parts", 2), new("lens", 2)], "chu_sniper_1");
            AddArmoryLine("ChurchGannix", "Gloria", "Deepsky", "spider_armor",
                [new("venus_armor_plates", 3), new("quasi_venus_rags", 3)],
                "spider_light_armor_1", "spider_light_helmet_1", "spider_light_pants_1", "spider_light_boots_1");

            AddFactionWorkshop("SheduThousand", "Gloria", 1, "iom_shedu_mars_stock", 132,
                [new("rusty_plates", 2), new("ron_blood", 1)], [new("mars_gear", 8), new("mars_skin", 8)]);
            AddArmoryLine("SheduThousand", "RedTiangong", "Gloria", "gas_shotgun",
                [new("mars_gear", 3), new("mars_skin", 1)], "she_gas_shotgun_1");
            AddArmoryLine("SheduThousand", "RedTiangong", "Gloria", "medium_armor",
                [new("mars_skin", 3), new("mars_gear", 2), new("rags", 2)],
                "possessed_medium_armor_1", "possessed_medium_helmet_1", "possessed_medium_pants_1");
        }

        private static void AddArmoryLine(string manufacturer, string factory, string depot, string line,
            ItemQuantity[] inputs, params string[] products)
        {
            int tier = products.Max(id =>
            {
                var item = Data.Items.GetSimpleRecord<ItemRecord>(id);
                if (item == null || !item.Categories.Contains(manufacturer))
                    throw new InvalidOperationException($"Invalid {manufacturer} armory product {id}");
                return item.TechLevel;
            });
            string receipt = $"iom_armory_{manufacturer.ToLowerInvariant()}_{line}";
            AddFactionWorkshop(manufacturer, factory, tier, receipt + "_assembly", 120 + tier * 12,
                [.. inputs], products.Select(id => new ItemQuantity(id, 4)).ToList());
            AddFactionWorkshop(manufacturer, depot, tier, receipt + "_issue", 240,
                products.Select(id => new ItemQuantity(id, 1)).ToList(), [], powerGain: 1);
        }
    }
}

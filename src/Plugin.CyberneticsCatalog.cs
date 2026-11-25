using MGSC;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private static void RegisterHexarchyCybernetics()
        {
            var ancom = new CyberneticsLab("AnCom", "Discovery", "HISUnveiled");
            AddNativeImplant(ancom, "neural_jack");
            AddNativeImplant(ancom, "neural_sonar");
            AddImplant(ancom, "recon_optic", "AnCom Recon Optic", "Head", 3, "Extends sight at the cost of shock resistance.",
                new() { ["los_reduce"] = 2 }, new() { ["resist_shock"] = -8 },
                [new(ImplantController, 1), new("lens", 2)]);
            AddImplant(ancom, "quickfeed_sleeve", "AnCom Quickfeed Sleeve", "Arm", 5, "Speeds up reloads and leaves the arm more vulnerable to piercing damage.",
                new() { ["reload_duration"] = -1 }, new() { ["resist_pierce"] = -8 },
                [new(ServoCartridge, 1), new(ImplantController, 1), new("spring", 2)]);

            var sbn = new CyberneticsLab("SBN", "Skylab", "HumankindsHope");
            AddNativeImplant(sbn, "sbn_shorts");
            AddNativeImplant(sbn, "biomonitor");
            AddActiveCybernetic(sbn, "survey_pulse", "SBN Survey Pulse", "neural_sonar", 4, 3,
                "Emits a short-range scouting pulse. Three charges.", [new(ImplantController, 1), new("lens", 1), new("capacitor_parts", 1)],
                ("IRadius", 10), ("ICooldown", 4));
            AddImplant(sbn, "signal_isolation", "SBN Signal Isolator", "Body", 6, "Improves shock resistance and reduces implant cooldowns at a health cost.",
                new() { ["implant_cooldown"] = -5, ["resist_shock"] = 12 }, new() { ["max_health"] = -8 },
                [new(ImplantController, 2), new("transformer", 1), new(SterileScaffold, 1)]);

            var realware = new CyberneticsLab("RealWare", "Cronus", "HISAres");
            AddNativeImplant(realware, "cortex_stimulator");
            AddNativeImplant(realware, "rib_reinforcement");
            AddImplant(realware, "braced_shoulder", "RealWare Braced Shoulder", "Shoulder", 3, "Reinforces against blunt and piercing attacks at the cost of dodge.",
                new() { ["resist_blunt"] = 10, ["resist_pierce"] = 8 }, new() { ["dodge_reduce"] = -0.04f },
                [new(ServoCartridge, 1), new(Plate, 2)]);
            AddImplant(realware, "siege_lattice", "RealWare Siege Lattice", "Body", 7, "A broad mechanical subdermal defense with a health penalty.",
                new() { ["resist_blunt"] = 14, ["resist_pierce"] = 14, ["resist_lacer"] = 6 }, new() { ["max_health"] = -12 },
                [new(ServoCartridge, 1), new("ceramite_plates", 2), new(SterileScaffold, 1)]);

            var tianming = new CyberneticsLab("Tianming", "LaCE", "Oracle");
            AddNativeImplant(tianming, "tissue_regenerator");
            AddNativeImplant(tianming, "tactile_sensor");
            AddImplant(tianming, "disc_counterweight", "Tianming Disc Counterweight", "Arm", 3, "Improves melee force and throw range, with weaker shock insulation.",
                new() { ["melee_throw_range"] = 1, ["melee_dmg_reduce"] = 0.12f }, new() { ["resist_shock"] = -8 },
                [new(ServoCartridge, 1), new("armor_plates", 1), new("rod_parts", 1)]);
            AddImplant(tianming, "pulse_governor", "Tianming Pulse Governor", "Chest", 6, "Reduces implant cooldowns and improves fire resistance, with a satiety penalty.",
                new() { ["implant_cooldown"] = -6, ["resist_fire"] = 10 }, new() { ["satiety"] = -0.2f },
                [new(ImplantController, 2), new("capacitor_parts", 2), new(SterileScaffold, 1)]);

            var dilthey = new CyberneticsLab("Dilthey", "Fortuna", "ZeroGravity");
            AddNativeImplant(dilthey, "discharge_chamber");
            AddNativeImplant(dilthey, "energy_collector");
            AddImplant(dilthey, "acoustic_dampener", "Dilthey Acoustic Dampener", "Head", 2, "Widens the field of view and shields against shock, with a small accuracy penalty.",
                new() { ["fov_angle"] = 0.2f, ["resist_shock"] = 10 }, new() { ["ranged_accuracy"] = -0.05f },
                [new(ImplantController, 1), new("rubber", 1)]);
            AddActiveCybernetic(dilthey, "static_coil", "Dilthey Static Coil", "discharge_chamber", 6, 3,
                "Discharges shock in a three-tile radius. Three charges.",
                [new(ImplantController, 2), new("transformer", 2), new("capacitor_parts", 2)],
                ("IRadius", 3), ("IMinDamage", 12), ("IMaxDamage", 44), ("ICooldown", 4));
        }

        private static void RegisterCorporateCybernetics()
        {
            var daydream = new CyberneticsLab("DaydreamChems", "AvicennasClinic", "MoonTradeCenter");
            AddNativeImplant(daydream, "blood_filtration");
            AddNativeImplant(daydream, "med_auto_injector");
            AddImplant(daydream, "detox_membrane", "Daydream Detox Membrane", "Stomach", 3, "Aids wound healing and poison resistance, with reduced piercing protection.",
                new() { ["resist_poison"] = 18, ["wound_heal_chance"] = 0.12f }, new() { ["resist_pierce"] = -8 },
                [new(SterileScaffold, 2), new(Treatment, 1)]);
            AddActiveCybernetic(daydream, "trauma_injector", "Daydream Trauma Injector", "med_auto_injector", 6, 3,
                "Starts a short burst of recovery and helps stabilize wounds. Three charges.",
                [new(SterileScaffold, 2), new(ImplantController, 1), new(Clinical, 1)],
                ("IHealthRegen", 10), ("IDuration", 3), ("ICooldown", 4));

            var bridge = new CyberneticsLab(PlanetBridge, "Cryptoria", "AllianceArcology");
            AddNativeImplant(bridge, "induction_frame");
            AddNativeImplant(bridge, "antigravity_module");
            AddImplant(bridge, "haul_assist", "PlanetBridge Haul Assist", "Shoulder", 2, "Lightens the backpack and absorbs blunt impacts at the cost of shock resistance.",
                new() { ["backpack_weight"] = -0.16f, ["resist_blunt"] = 6 }, new() { ["resist_shock"] = -8 },
                [new(ServoCartridge, 1), new(PolymerStock, 1), new("rod_parts", 1)]);
            AddImplant(bridge, "route_ram", "PlanetBridge Route Ram", "Thigh", 5, "Reduces carried item weight and improves dodge, with weaker shock insulation.",
                new() { ["items_weight"] = -0.12f, ["dodge_reduce"] = 0.04f }, new() { ["resist_shock"] = -8 },
                [new(ServoCartridge, 2), new(ImplantController, 1), new("metal_tread", 1)]);

            var grasshopper = new CyberneticsLab("Grasshopper", "Kyoko", "ShepherdsArc");
            AddNativeImplant(grasshopper, "neurocortex");
            AddNativeImplant(grasshopper, "bio_boosters");
            AddImplant(grasshopper, "soft_step", "Grasshopper Soft Step", "Feet", 2, "Soft sole actuators improve dodge but absorb less blunt damage.",
                new() { ["dodge_reduce"] = 0.06f }, new() { ["resist_blunt"] = -6 },
                [new(ServoCartridge, 1), new("rubber", 2)]);
            AddImplant(grasshopper, "courier_reflex", "Grasshopper Courier Reflex", "Knee", 5, "Improves dodge and reduces the accuracy penalty from sprinting.",
                new() { ["dodge_reduce"] = 0.05f, ["sprint_accuracy_mult"] = -0.2f }, new() { ["wound_chance_mult"] = 0.1f },
                [new(ServoCartridge, 1), new(ImplantController, 1), new("spring", 1)]);

            var sunlight = new CyberneticsLab("Sunlight", "DarkFlora", "Stratagem");
            AddNativeImplant(sunlight, "superdermal_layer");
            AddNativeImplant(sunlight, "thermal_regulator");
            AddImplant(sunlight, "heat_sink", "Sunlight Heat Sink", "Chest", 3, "Shunts beam and fire damage at the cost of cold resistance.",
                new() { ["resist_beam"] = 12, ["resist_fire"] = 12 }, new() { ["resist_cold"] = -10 },
                [new(ImplantController, 1), new("glass", 2), new(Plate, 1)]);
            AddImplant(sunlight, "cryo_stride", "Sunlight Cryo Stride", "Feet", 7, "Protects against cold and cold wounds, with weaker fire resistance.",
                new() { ["resist_cold"] = 20, ["wound_immune_cold"] = 1 }, new() { ["resist_fire"] = -12 },
                [new(ServoCartridge, 2), new("ceramite_plates", 1), new(SterileScaffold, 1)]);

            var franche = new CyberneticsLab("FrancheComte", "QiHelium", "ShuCenter");
            AddNativeImplant(franche, "optical_suppressor");
            AddNativeImplant(franche, "shielded_skull");
            AddImplant(franche, "patrol_ear", "Franche-Comté Patrol Ear", "Head", 2, "Improves field of view and ranged accuracy with weaker shock protection.",
                new() { ["fov_angle"] = 0.25f, ["ranged_accuracy"] = 0.05f }, new() { ["resist_shock"] = -6 },
                [new(ImplantController, 1), new("lens", 1)]);
            AddImplant(franche, "riot_anchor", "Franche-Comté Riot Anchor", "Thigh", 6, "Prevents knockback and absorbs blunt damage, with reduced dodge.",
                new() { ["throwback_immune"] = 1, ["resist_blunt"] = 10 }, new() { ["dodge_reduce"] = -0.08f },
                [new(ServoCartridge, 2), new(HeavyPlate, 1), new(ImplantController, 1)]);

            var church = new CyberneticsLab("ChurchRevelation", "PineGapTraining", "Deepsky");
            AddNativeImplant(church, "ultravoice");
            AddNativeImplant(church, "bionic_lungs");
            AddImplant(church, "pilgrim_respirator", "Pilgrim Respirator", "Chest", 2, "Reduces calorie use and filters poison, with a satiety penalty.",
                new() { ["food_calories"] = -0.2f, ["resist_poison"] = 8 }, new() { ["satiety"] = -0.12f },
                [new(SterileScaffold, 1), new("rubber", 1)]);
            AddActiveCybernetic(church, "signal_cantor", "Signal Cantor", "ultravoice", 5, 3,
                "Broadcasts the Church's sonic pulse in a four-tile radius. Three charges.",
                [new(ImplantController, 2), new("communication_relay", 1)],
                ("IRadius", 4), ("ICooldown", 4));

            var ward = new CyberneticsLab(CoreWard, "Rachmaninoff", "HISWoodpecker");
            AddNativeImplant(ward, "thermal_coating");
            AddNativeImplant(ward, "energy_booster");
            AddImplant(ward, "worksite_brace", "CoreWard Worksite Brace", "Shoulder", 2, "Increases melee force and fire resistance, at the expense of beam protection.",
                new() { ["melee_dmg_reduce"] = 0.14f, ["resist_fire"] = 8 }, new() { ["resist_beam"] = -6 },
                [new(ServoCartridge, 1), new("coarse_parts", 1)]);
            AddImplant(ward, "worksite_radiator", "CoreWard Worksite Radiator", "Body", 5, "Prevents burning and adds fire resistance, while reducing cold and shock resistance.",
                new() { ["burn_immune"] = 1, ["resist_fire"] = 16 }, new() { ["resist_cold"] = -12, ["resist_shock"] = -8 },
                [new(ServoCartridge, 1), new(ImplantController, 1), new(Plate, 1)]);
        }

        private static void RegisterIndependentCybernetics()
        {
            var resistance = new CyberneticsLab("CResistance", "TeslaRing", "FlatObsidian");
            AddNativeImplant(resistance, "integrated_magazine", new(ServoCartridge, 1), new("spring", 2));
            AddNativeImplant(resistance, "kinetic_stabilizer", new(ServoCartridge, 1), new(ImplantController, 1), new(Plate, 2));
            AddImplant(resistance, "scrap_guard", "Resistance Scrap Guard", "Body", 2, "Salvaged subdermal plates resist piercing and cuts at a health cost.",
                new() { ["resist_pierce"] = 10, ["resist_lacer"] = 10 }, new() { ["max_health"] = -8 },
                [new(Plate, 2), new(SterileScaffold, 1)]);
            AddImplant(resistance, "defiance_pump", "Resistance Defiance Pump", "Chest", 6, "Increases health and reduces pain received, while increasing calorie use.",
                new() { ["max_health"] = 18, ["income_pain"] = -0.15f }, new() { ["food_calories"] = 0.2f },
                [new(ImplantController, 1), new(SterileScaffold, 2), new("wire", 1)]);

            var belt = new CyberneticsLab("UnchainedBelt", "Schwerin", "Hargeysa");
            AddNativeImplant(belt, "painhibitor");
            AddNativeImplant(belt, "kinetic_absorber");
            AddImplant(belt, "boarder_joint", "Unchained Boarder Joint", "Arm", 2, "Improves melee critical chance and cut resistance. Ranged handling suffers.",
                new() { ["melee_crit_chance"] = 0.08f, ["resist_lacer"] = 8 }, new() { ["ranged_accuracy"] = -0.1f },
                [new(ServoCartridge, 1), new("weapon_parts", 1)]);
            AddImplant(belt, "holdfast_knee", "Unchained Holdfast Knee", "Knee", 5, "Prevents knockback and resists piercing damage at the cost of dodge.",
                new() { ["throwback_immune"] = 1, ["resist_pierce"] = 10 }, new() { ["dodge_reduce"] = -0.06f },
                [new(ServoCartridge, 2), new("metal_tread", 1)]);

            var tezctlan = new CyberneticsLab("Tezctlan", "Oddibjord", "FeatheredTemple", quasi: true);
            AddNativeImplant(tezctlan, "warp_armor", new("venus_armor_plates", 2), new("quasisamples", 1));
            AddNativeImplant(tezctlan, "steadfast_heart", new("quasisamples", 2), new("quasi_venus_rags", 1));
            AddImplant(tezctlan, "carapace_seed", "Tezctlan Carapace Seed", "Body", 3, "Grows protection against blunt impacts and cuts. Raises Quasimorphosis.",
                new() { ["resist_blunt"] = 12, ["resist_lacer"] = 12 }, new() { ["qmorph"] = 1 },
                [new("venus_armor_plates", 2), new("quasisamples", 1)]);
            AddImplant(tezctlan, "spine_knot", "Tezctlan Spine Knot", "Shoulder", 6, "Increases critical damage and cut resistance. Raises Quasimorphosis.",
                new() { ["crit_damage"] = 0.2f, ["resist_lacer"] = 14 }, new() { ["qmorph"] = 2 },
                [new("venus_armor_plates", 2), new("venus_weapon_parts", 2), new("quasisamples", 1)]);

            var xiomara = new CyberneticsLab("XiomaraMasks", "Oddibjord", "FeatheredTemple", quasi: true);
            AddNativeImplant(xiomara, "blood_dance", new("quasisamples", 1), new("quasi_venus_rags", 2));
            AddImplant(xiomara, "veil_tendon", "Xiomara Veil Tendon", "Feet", 3, "Improves dodge and cold resistance. Raises Quasimorphosis.",
                new() { ["dodge_reduce"] = 0.1f, ["resist_cold"] = 6 }, new() { ["qmorph"] = 2 },
                [new("quasi_venus_rags", 2), new("quasisamples", 1)]);
            AddImplant(xiomara, "masked_optic", "Xiomara Masked Optic", "Head", 6, "Improves vision and ranged precision. Raises Quasimorphosis.",
                new() { ["los_reduce"] = 1, ["ranged_accuracy"] = 0.1f }, new() { ["qmorph"] = 2 },
                [new("venus_armor_plates", 1), new("quasisamples", 2)]);

            var emerald = new CyberneticsLab("Emeraldlight", "Zeus", "Stratagem", quasi: true);
            AddNativeImplant(emerald, "superdermal_layer", new("venus_armor_plates", 2), new("wire", 1));
            AddImplant(emerald, "cold_vein", "Emeraldlight Cold Vein", "Chest", 4, "Resists cold and shock. Raises Quasimorphosis.",
                new() { ["resist_cold"] = 14, ["resist_shock"] = 8 }, new() { ["qmorph"] = 1 },
                [new("quasi_venus_rags", 2), new("capacitor_parts", 1)]);
            AddImplant(emerald, "prism_membrane", "Emeraldlight Prism Membrane", "Body", 7, "Resists beam and cold damage. Raises Quasimorphosis.",
                new() { ["resist_beam"] = 20, ["resist_cold"] = 10 }, new() { ["qmorph"] = 2 },
                [new("venus_armor_plates", 2), new("lens", 2), new("quasisamples", 1)]);

            var gannix = new CyberneticsLab("ChurchGannix", "Gloria", "Deepsky", quasi: true);
            AddNativeImplant(gannix, "stunning_stride", new("venus_weapon_parts", 2), new("quasi_venus_rags", 1));
            AddImplant(gannix, "shrine_lattice", "Gannix Shrine Lattice", "Body", 3, "Resists poison damage and poison wounds. Raises Quasimorphosis.",
                new() { ["wound_immune_poison"] = 1, ["resist_poison"] = 14 }, new() { ["qmorph"] = 1 },
                [new("venus_armor_plates", 2), new("quasi_venus_rags", 1)]);
            AddImplant(gannix, "echo_pilgrim", "Gannix Echo Pilgrim", "Chest", 6, "Increases health and softens incoming pain. Raises Quasimorphosis.",
                new() { ["income_pain"] = -0.22f, ["max_health"] = 16 }, new() { ["qmorph"] = 2 },
                [new("quasisamples", 2), new("venus_armor_plates", 1), new("rod_parts", 1)]);

            var shedu = new CyberneticsLab("SheduThousand", "RedTiangong", "Gloria", quasi: true);
            AddNativeImplant(shedu, "cortex_stimulator", new("mars_gear", 2), new("mars_skin", 1));
            AddImplant(shedu, "forge_rib", "Shedu Forge Rib", "Chest", 4, "Adds blunt and fire resistance. Raises Quasimorphosis.",
                new() { ["resist_blunt"] = 14, ["resist_fire"] = 10 }, new() { ["qmorph"] = 1 },
                [new("mars_gear", 2), new("mars_skin", 1)]);
            AddImplant(shedu, "mars_tendon", "Shedu Mars Tendon", "Knee", 7, "Improves dodge and braces against knockback. Raises Quasimorphosis.",
                new() { ["dodge_reduce"] = 0.1f, ["throwback_immune"] = 1 }, new() { ["qmorph"] = 2 },
                [new("mars_gear", 2), new("mars_skin", 2)]);
        }

        private static void RegisterReplacementWorkshops()
        {
            var ward = new CyberneticsLab(CoreWard, "Rachmaninoff", "HISWoodpecker");
            var realware = new CyberneticsLab("RealWare", "Cronus", "HISAres");
            var grasshopper = new CyberneticsLab("Grasshopper", "Kyoko", "ShepherdsArc");
            var shedu = new CyberneticsLab("SheduThousand", "RedTiangong", "Gloria", quasi: true);

            AddBodyFamily(ward, "cyborg", 2, Plate, "wire");
            AddBodyFamily(grasshopper, "cyborg_recreation", 3, "rubber", "microelectronics_parts");
            AddBodyFamily(realware, "cyborg_battle", 4, "ceramite_plates", "capacitor_parts");
            AddFactionWorkshop(CoreWard, ward.Depot, 4, "iom_cybernetics_coreward_cyborg_drill_issue", 240,
                [new("cyborg_drill", 1)], []);
            AddCyberneticsLine(grasshopper, 4, "cyborg_recreation_blade", new(ServoCartridge, 1), new("rubber", 1), new("weapon_parts", 2));
            AddCyberneticsLine(realware, 5, "cyborg_battle_machinegun", new(ServoCartridge, 1), new(HeavyPlate, 1), new("weapon_parts", 2));
            AddCyberneticsBatch(shedu, 7, "mars_body_parts", [new("mars_gear", 2), new("mars_skin", 2)],
                "mars_cyborg_head", "mars_cyborg_chest", "mars_cyborg_stomach", "mars_cyborg_knee");
            AddCyberneticsBatch(shedu, 7, "mars_limb_assemblies", [new("mars_gear", 3), new("mars_skin", 2)],
                "mars_cyborg_hand", "mars_cyborg_leg");

            AddReplacement(new(CoreWard, SampoQuern, ward.Depot), "loadframe_feet", "CoreWard Loadframe Feet", "cyborg_feet", 3, 1,
                "Load-bearing replacement feet reduce backpack weight and resist blunt damage.",
                new() { ["backpack_weight"] = -0.08f, ["resist_blunt"] = 8 }, new() { ["max_health"] = -5, ["resist_shock"] = -8 },
                [new("cyborg_feet", 1), new(ServoCartridge, 2), new(Plate, 1)]);
            AddReplacement(realware, "bulwark_chest", "RealWare Bulwark Chest", "cyborg_battle_chest", 6, 2,
                "A reinforced chest replacement with two implant sockets and reduced dodge.",
                new() { ["resist_blunt"] = 12, ["resist_pierce"] = 10 }, new() { ["max_health"] = -8, ["dodge_reduce"] = -0.06f },
                [new("cyborg_battle_chest", 1), new(HeavyPlate, 2), new(ServoCartridge, 1)]);
            AddReplacement(grasshopper, "courier_leg", "Grasshopper Courier Leg", "cyborg_recreation_leg", 5, 1,
                "A combined thigh and foot replacement for agile scouts.",
                new() { ["dodge_reduce"] = 0.1f }, new() { ["max_health"] = -5, ["resist_blunt"] = -8 },
                [new("cyborg_recreation_leg", 1), new(ServoCartridge, 2), new("rubber", 2)]);
            AddReplacement(new("Grasshopper", "HartsWarehouse", grasshopper.Depot), "flex_wrist", "Grasshopper Flex Wrist", "cyborg_recreation_wrist", 4, 1,
                "A flexible arm replacement that improves ranged handling and leaves the arm vulnerable to shock.",
                new() { ["ranged_accuracy"] = 0.08f }, new() { ["max_health"] = -5, ["resist_shock"] = -8 },
                [new("cyborg_recreation_wrist", 1), new(ServoCartridge, 1), new(ImplantController, 1)]);
            AddReplacement(new("SBN", "Skylab", "HumankindsHope"), "survey_cranium", "SBN Survey Cranium", "cyborg_head", 6, 2,
                "A two-socket head replacement with a wider field of view and better sight.",
                new() { ["fov_angle"] = 0.4f, ["los_reduce"] = 1 }, new() { ["max_health"] = -8, ["resist_shock"] = -12 },
                [new("cyborg_head", 1), new(ImplantController, 2), new("lens", 2)]);
            AddReplacement(new("Tianming", "LaCE", "Oracle"), "servo_knee", "Tianming Servo Knee", "cyborg_battle_knee", 5, 1,
                "An impact-resistant replacement knee that braces against knockback.",
                new() { ["throwback_immune"] = 1, ["resist_blunt"] = 8 }, new() { ["max_health"] = -5, ["dodge_reduce"] = -0.04f },
                [new("cyborg_battle_knee", 1), new(ServoCartridge, 2), new("armor_plates", 1)]);
            AddReplacement(new(PlanetBridge, "Cryptoria", "AllianceArcology"), "gantry_arm", "PlanetBridge Gantry Arm", "cyborg_hand", 5, 2,
                "A combined shoulder and arm replacement for hauling, with two sockets in the arm.",
                new() { ["backpack_weight"] = -0.12f, ["resist_blunt"] = 8 }, new() { ["max_health"] = -7, ["resist_shock"] = -10 },
                [new("cyborg_hand", 1), new(ServoCartridge, 2), new("metal_tread", 1)]);
            AddReplacement(new("DaydreamChems", "AvicennasClinic", "MoonTradeCenter"), "sterile_guts", "Daydream Sterile Organ Pack", "cyborg_guts", 5, 1,
                "A replacement body module that supports natural recovery and resists poison.",
                new() { ["regen_efficacy"] = 0.2f, ["resist_poison"] = 12 }, new() { ["max_health"] = -5, ["resist_shock"] = -10 },
                [new("cyborg_guts", 1), new(SterileScaffold, 2), new(Treatment, 1)]);
            AddReplacement(new("CResistance", "TeslaRing", "FlatObsidian"), "salvage_wrist", "Resistance Salvage Wrist", "cyborg_wrist", 3, 1,
                "A repairable replacement arm that absorbs impacts at the cost of ranged handling.",
                new() { ["resist_blunt"] = 10 }, new() { ["max_health"] = -4, ["ranged_accuracy"] = -0.05f },
                [new(ServoCartridge, 2), new(Plate, 1), new("weapon_parts", 1)]);
            AddReplacement(shedu, "mars_recoil_hand", "Shedu Mars Recoil Hand", "mars_cyborg_hand", 8, 2,
                "A two-slot Mars arm replacement that improves critical damage and raises Quasimorphosis.",
                new() { ["crit_damage"] = 0.25f }, new() { ["max_health"] = -10, ["qmorph"] = 1 },
                [new("mars_cyborg_hand", 1), new("mars_gear", 2), new("mars_skin", 2)]);
        }

        private static void AddBodyFamily(CyberneticsLab lab, string prefix, int tier, string frame, string electronics)
        {
            AddCyberneticsBatch(lab, tier, prefix + "_core_parts",
                [new(frame, 3), new(ImplantController, 2), new(electronics, 1), new(SterileScaffold, 1)],
                prefix + "_head", prefix + "_chest", prefix + "_guts");
            AddCyberneticsBatch(lab, tier, prefix + "_joint_parts",
                [new(frame, 2), new(ServoCartridge, 2), new(electronics, 1)],
                prefix + "_wrist", prefix + "_feet", prefix + "_knee");
            AddCyberneticsLine(lab, tier, prefix + "_hand", new(prefix + "_wrist", 1), new(ServoCartridge, 1), new("weapon_parts", 1));
            AddCyberneticsLine(lab, tier, prefix + "_leg", new(prefix + "_feet", 1), new(ServoCartridge, 1), new("metal_tread", 1));
        }
    }
}

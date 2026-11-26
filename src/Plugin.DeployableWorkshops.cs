using System.Collections.Generic;
using MGSC;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private const string CoreWard = "CoreWard";
        private const string PlanetBridge = "PlanetBridge";
        private const string SampoQuern = "SampoQuern";
        private const string AllianceArcology = "AllianceArcology";
        private const string ArcSentry = "iom_arc_sentry";
        private const string LanceSentry = "iom_lance_sentry";
        private const string MuleDrone = "iom_mule_drone";
        private const string SurveyDrone = "iom_survey_drone";
        private const string BorehandDroid = "iom_borehand_droid";
        private const string ConciergeDroid = "iom_concierge_droid";

        private static void RegisterDeployableWorkshops()
        {
            AddFactionWorkshop("RealWare", "ArcadisTerminal", 2, "iom_arcadis_frame_fittings", 120,
                [new(Metal, 1), new("spring", 1)], [new(Fittings, 10)]);
            AddFactionWorkshop("RealWare", "HISAres", 4, "iom_ares_reinforced_plates", 120,
                [new(Plate, 2), new(Fittings, 1)], [new(HeavyPlate, 8)]);
            AddFactionWorkshop(CoreWard, SampoQuern, 2, "iom_sampo_automata_frames", 84,
                [new(Plate, 1), new(Fittings, 1), new(PolymerStock, 1)], [new(AutomataChassis, 12)]);
            AddFactionWorkshop("SBN", "CathodeRampart", 2, "iom_cathode_automata_controllers", 84,
                [new("circuit_board", 2), new("microelectronics_parts", 2), new("capacitor_parts", 1), new(Cell, 1)],
                [new(LogicCore, 12)]);
            AddFactionWorkshop(CoreWard, SampoQuern, 2, "iom_sampo_robot_parts", 180,
                [new(AutomataChassis, 1), new("engineering_parts_container", 1)], [new("robot_parts_container", 6)]);
            AddFactionWorkshop(PlanetBridge, AllianceArcology, 3, "iom_arcology_robot_parts", 180,
                [new(AutomataChassis, 1), new("tool_container", 1)], [new("robot_parts_container", 6)]);

            RegisterTurretWorkshops();
            RegisterDroidWorkshops();
            RegisterDroneWorkshops();
        }

        private static void RegisterTurretWorkshops()
        {
            AddDeployableLine("RealWare", "ArcadisTerminal", "HISAres", 2, "stationary_turret", 144,
                new(AutomataChassis, 1), new(LogicCore, 1), new("weapon_parts", 2), new("metal_tread", 1));
            AddFactionWorkshop("Grasshopper", "Jinxing", 3, "iom_jinxing_turret_fuel", 120,
                [new("drug_cargo", 1), new("tin", 1)], [new("gas_ammo", 20)]);
            AddDeployableLine("Grasshopper", "Canaan", "Escher", 3, "fire_turret", 144,
                new(AutomataChassis, 1), new(LogicCore, 1), new("gas_ammo", 4));
            AddDeployableLine("SBN", "Skylab", "HumankindsHope", 4, "energy_turret", 144,
                new(AutomataChassis, 1), new(LogicCore, 1), new("lens", 1));

            var arc = RegisterDeployableWeapon(ArcSentry, "dil_shock_smg_1", 5, "Arc emitter", 4, 16, 34);
            arc.Firemodes = ["lightning_3"];
            RegisterDeployableVariant(ArcSentry, "SBN Arc Sentry", "A short-range shock turret for corridors and machine targets.",
                "energy_turret", "quest_turret_emp", "turret_emp", "SBN", 5, 525, 80, 1, 6, arc.Id);
            AddDeployableVariantLine("SBN", "Skylab", "HumankindsHope", 5, ArcSentry, "SBN Arc Sentry", 168,
                new(AutomataChassis, 1), new(LogicCore, 1), new("transformer", 1));

            var lance = RegisterDeployableWeapon(LanceSentry, "nature_laser_turret", 6, "Lance emitter", 9, 34, 47);
            lance.MagazineCapacity = lance.MinRandomAmmoCount = 1;
            lance.ReloadDuration = 3;
            RegisterDeployableVariant(LanceSentry, "RealWare Lance Sentry",
                "A long-range beam sentry that recharges between shots. Built to cover open lanes.",
                "energy_turret", "turret_energy", "turret_energy", "RealWare", 6, 650, 68, 1, 9, lance.Id);
            AddDeployableVariantLine("RealWare", "ArcadisTerminal", "HISAres", 6, LanceSentry, "RealWare Lance Sentry", 180,
                new(AutomataChassis, 1), new(LogicCore, 2), new("lens", 2));
        }

        private static void RegisterDroidWorkshops()
        {
            AddDeployableLine(CoreWard, "Rachmaninoff", "HISWoodpecker", 2, "packaged_industrial_robot", 144,
                new(AutomataChassis, 1), new(LogicCore, 1), new("engineering_parts_container", 1), new("coarse_parts", 1));
            AddDeployableLine("Grasshopper", "Kyoko", "ShepherdsArc", 3, "packaged_recreational_robot", 144,
                new(AutomataChassis, 1), new(LogicCore, 1), new(Textile, 1), new("spring", 1));
            AddDeployableLine("RealWare", "ArcadisTerminal", "HISAres", 4, "packaged_security_robot", 168,
                new(AutomataChassis, 1), new(LogicCore, 1), new(HeavyPlate, 1));

            var bore = RegisterDeployableWeapon(BorehandDroid, "nature_cyborg_drill", 4, "Borehand drill", 1, 28, 38);
            RegisterDeployableVariant(BorehandDroid, "CoreWard Borehand Industrial Droid",
                "A reinforced follower with a built-in drill for close combat.",
                "packaged_industrial_robot", "cyborg_worker", "cyborg", CoreWard, 4, 425, 112, 2, 5, bore.Id);
            AddFactionWorkshop(CoreWard, "Rachmaninoff", 4, "iom_rachmaninoff_drill_refits", 144,
                [new(Fittings, 1), new("engineering_parts_container", 1), new("rod_parts", 1)], [new("cyborg_drill", 4)]);
            AddDeployableVariantLine(CoreWard, "Rachmaninoff", "HISWoodpecker", 4, BorehandDroid, "Borehand Industrial Droid", 168,
                new(AutomataChassis, 1), new(LogicCore, 1), new("cyborg_drill", 1));

            var baton = RegisterDeployableWeapon(ConciergeDroid, "nature_cyborg_recreation_fist", 4, "Concierge shock wand", 1, 13, 22);
            baton.DefaultAmmoId = "implicted_shockgun";
            RegisterDeployableVariant(ConciergeDroid, "Grasshopper Concierge Pleasure Droid",
                "A quick, lightly built pleasure droid that stays close and defends with a shock wand.",
                "packaged_recreational_robot", "cyborg_recreation", "cyborg_recreation", "Grasshopper", 4, 390, 62, 4, 6, baton.Id);
            AddDeployableVariantLine("Grasshopper", "Kyoko", "ShepherdsArc", 4, ConciergeDroid, "Concierge Pleasure Droid", 168,
                new(AutomataChassis, 1), new(LogicCore, 1), new(Textile, 1), new("capacitor_parts", 1));
        }

        private static void RegisterDroneWorkshops()
        {
            AddDeployableLine(PlanetBridge, AllianceArcology, "Cryptoria", 5, "packaged_drone", 168,
                new(AutomataChassis, 1), new(LogicCore, 1), new(AiModule, 1));

            RegisterDeployableVariant(MuleDrone, "PlanetBridge Mule Cargo Drone",
                "A reinforced cargo chassis with a loading fork. The Mule follows on the ground and fights up close.",
                "packaged_drone", "drone_cargo", "cargo_drone", PlanetBridge, 3, 280, 90, 2, 5, "nature_cargo_drone_fork");
            AddDeployableVariantLine(PlanetBridge, AllianceArcology, WinterBay, 3, MuleDrone, "Mule Cargo Drone", 144,
                new(AutomataChassis, 1), new(LogicCore, 1), new(Fittings, 1), new("rod_parts", 1));

            var survey = RegisterDeployableWeapon(SurveyDrone, "laser_marksman_1", 6, "Survey pulse carbine", 5, 16, 25);
            survey.Firemodes = ["pulse_1"];
            RegisterDeployableVariant(SurveyDrone, "PlanetBridge Survey Drone",
                "A light flying scout with a long sight radius and a low-power pulse carbine.",
                "packaged_drone", "drone_security", "flying_drone", PlanetBridge, 6, 450, 43, 4, 10, survey.Id);
            AddDeployableVariantLine(PlanetBridge, AllianceArcology, "Cryptoria", 6, SurveyDrone, "Survey Drone", 168,
                new(AutomataChassis, 1), new(LogicCore, 1), new(AiModule, 1), new("lens", 1), new("microelectronics_parts", 1));
        }

        private static WeaponRecord RegisterDeployableWeapon(string unit, string donor, int tier, string title,
            int range, int minDamage, int maxDamage)
        {
            var weapon = CloneItem<WeaponRecord>(unit + "_weapon", donor, tier, 0);
            weapon.Categories = [];
            weapon.IsImplicit = true;
            weapon.Unbreakable = true;
            weapon.Range = range;
            weapon.GetMeleeDamageFromCreature = false;
            weapon.MinRandomAmmoCount = weapon.MagazineCapacity;
            if (weapon.IsMelee)
            {
                weapon.RequiredAmmo = string.Empty;
                weapon.MagazineCapacity = weapon.MinRandomAmmoCount = 0;
            }
            else
            {
                weapon.Traits = [.. weapon.Traits];
                if (!weapon.Traits.Contains("selfcharging")) weapon.Traits.Add("selfcharging");
            }
            SetDamage(weapon, minDamage, maxDamage);
            RegisterItem(weapon.IsMelee ? "meleeweapons" : "rangeweapons", weapon, title,
                weapon.IsMelee ? "An integrated close-combat weapon built into the automaton." : "An integrated weapon powered by the automaton's own systems.");
            return weapon;
        }

        private static void RegisterDeployableVariant(string id, string title, string description, string kitDonor,
            string roleDonor, string bodyDonor, string faction, int tier, int price, int health, int actions, int sight, string weapon)
        {
            var body = (BodyTypeRecord)CloneRecordMethod.Invoke(Data.BodyTypes.GetRecord(bodyDonor), null);
            body.Id = id + "_body";
            body.Health = health;
            body.BaseActionPoints = actions;
            // Native deployment equips the integral weapon in the bare-hands slot.
            body.HasInventoryWeaponSlots = true;
            body.HasInventoryEquipmentSlots = false;
            body.OverrideBareHandWeapon = weapon;
            body.WeaponRandomConditionCapacity = false;
            StageRecord(Data.BodyTypes, body.Id, body);

            var role = RegisterEquippedRole(id + "_ally", roleDonor, title, faction);
            role.BodyTypes = [body.Id];
            role.Los = sight;
            role.AdditAmmo = role.AdditItemCount = new(0, 0);
            role.AugCount = role.ImplantCount = new(0, 0);
            role.GrantedAugmentations = [];
            role.GrantedImplants = [];

            var package = CloneItem<PlaceableDeviceRecord>(id, kitDonor, tier, price, id);
            package.TargetObjectId = role.Id;
            package.Categories = [faction];
            package.Disassembly = [new(AutomataChassis, 1), new(LogicCore, 1)];
            RegisterItem("placeable_devices", package, title, description);
        }

        private static void AddDeployableVariantLine(string faction, string factory, string depot, int tier,
            string product, string title, int hours, params ItemQuantity[] inputs)
        {
            AddDeployableLine(faction, factory, depot, tier, product, hours, inputs);
            IndustryRecipe(product, 18 + tier * 2, 1, inputs);
            var schematic = CloneItem<DatadiskRecord>(product + "_plans", "low_chip", tier, 180 + tier * 35, FieldPlans);
            schematic.Categories = [faction];
            RegisterCatalog(schematic, title + " Schematic", $"Teaches the Magnum crew to assemble the {title}.",
                faction, [product], supplier: depot);
        }

        private static void AddDeployableLine(string faction, string factory, string depot, int tier,
            string product, int hours, params ItemQuantity[] inputs)
        {
            AddFactionWorkshop(faction, factory, tier, product + "_station_assembly", hours, [.. inputs], [new(product, 4)]);
            AddFactionWorkshop(faction, depot, tier, product + "_station_issue", 216, [new(product, 1)], [], powerGain: 1);
        }
    }
}

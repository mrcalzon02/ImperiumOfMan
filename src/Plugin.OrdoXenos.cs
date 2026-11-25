using System.Collections.Generic;
using HarmonyLib;
using MGSC;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private const int OrdoXenosTier = 9;
        private const string PhobosXenosTable = "iomPhobosXenosQuarantine";
        private const string PhobosXenosGarrison = "iomPhobosXenosGarrison";
        private const string PhobosQuarantineOffered = "iom.ordoxenos.phobos_quarantine_offered";
        private const string PhobosQuarantineStory = "iom_phobos_quarantine";
        private const string InterrogatorRole = "iom_ordoxenos_interrogator";
        private const string PistoleerRole = "iom_ordoxenos_pistoleer";

        private const string XenosHelmet = "iom_ordoxenos_helmet";
        private const string XenosCoat = "iom_ordoxenos_coat";
        private const string XenosGreaves = "iom_ordoxenos_greaves";
        private const string XenosBoots = "iom_ordoxenos_boots";
        private const string PowerBlade = "iom_ordoxenos_power_blade";
        private const string NeedlePistol = "iom_ordoxenos_needle_pistol";
        private const string XenosPlans = "iom_ordoxenos_requisition_chip";

        private static void RegisterOrdoXenosEquipment()
        {
            AddXenosArmor<HelmetRecord>("helmets", XenosHelmet, "police_medium_helmet_1", "Inquisitorial Carapace Helm",
                340, 1.4f, CreateResists(blunt: 10, pierce: 12, lacer: 7, fire: 6, poison: 6, beam: 5));
            AddXenosArmor<ArmorRecord>("armors", XenosCoat, "police_medium_armor_1", "Inquisitorial Carapace Coat",
                600, 2.9f, CreateResists(blunt: 19, pierce: 24, lacer: 12, fire: 10, poison: 9, beam: 9));
            AddXenosArmor<LeggingsRecord>("leggings", XenosGreaves, "police_medium_pants_1", "Inquisitorial Carapace Greaves",
                440, 2.05f, CreateResists(blunt: 13, pierce: 15, lacer: 9, fire: 7, poison: 6, beam: 6));
            AddXenosArmor<BootsRecord>("boots", XenosBoots, "police_medium_boots_1", "Inquisitorial Carapace Boots",
                275, 1.35f, CreateResists(blunt: 8, pierce: 10, lacer: 5, fire: 5, poison: 4, beam: 4));

            var blade = CloneItem<WeaponRecord>(PowerBlade, "sun_laser_powersword_1", OrdoXenosTier, 850, PowerBlade);
            blade.Weight = 1.35f;
            blade.MaxDurability = 110;
            blade.RepairItemIds = ["weapon_parts", "lens", "microelectronics_parts"];
            SetDamage(blade, 49, 65);
            ApplyWeaponFinish(blade, EquipmentFinish.XenosWeapon);
            RegisterItem("meleeweapons", blade, "Inquisitorial Power Blade",
                "An Interrogator's powered blade that deals Beam damage in close combat. Uses BatteryCells.");

            var needle = CloneItem<WeaponRecord>(NeedlePistol, "ddr_toxic_pistol_1", OrdoXenosTier, 625, NeedlePistol);
            needle.Weight = 1.1f;
            needle.Range = 5;
            needle.MagazineCapacity = 8;
            needle.MinRandomAmmoCount = 5;
            needle.MaxDurability = 95;
            SetDamage(needle, 23, 36);
            ApplyWeaponFinish(needle, EquipmentFinish.XenosWeapon);
            RegisterItem("rangeweapons", needle, "Inquisitorial Needle Pistol",
                "A suppressed pistol with single and paired shots. Uses standard Toxic ammunition.");
        }

        private static void AddXenosArmor<T>(string table, string id, string donor, string name, float price,
            float weight, List<DmgResist> resists) where T : ResistRecord, IArmorRecord
        {
            var armor = CloneItem<T>(id, donor, OrdoXenosTier, price, id);
            armor.Weight = weight;
            armor.MaxDurability = 155;
            armor.ResistSheet = resists;
            ApplyArmorFinish(armor, EquipmentFinish.XenosCarapace);
            RegisterItem(table, armor, name, "Compact carapace with a sealed liner for boarding and quarantine work.");
        }

        private static void RegisterOrdoXenosProduction()
        {
            IndustryRecipe(XenosHelmet, 15, 1, new(Plate, 1), new(Liner, 1), new("ceramite_plates", 1));
            IndustryRecipe(XenosCoat, 23, 1, new(HeavyPlate, 1), new(Liner, 2), new("ceramite_plates", 1));
            IndustryRecipe(XenosGreaves, 17, 1, new(Plate, 1), new(Liner, 1), new(Fittings, 2));
            IndustryRecipe(XenosBoots, 13, 1, new(PolymerStock, 1), new(Fittings, 2), new("rubber", 1));
            IndustryRecipe(PowerBlade, 26, 1, new(Sheet, 1), new("weapon_parts", 2), new("lens", 1), new("microelectronics_parts", 2));
            IndustryRecipe(NeedlePistol, 20, 1, new(Fittings, 1), new(PolymerStock, 1), new("spring", 2), new("glass", 1));
            AddCatalog(XenosPlans, "Ordo Xenos requisition", "Carapace and weapon patterns for Inquisitorial field agents, plus Hellfire Bolt Clips and the Detox Gland.",
                ItemCategory, OrdoXenosTier, 620, XenosHelmet, XenosCoat, XenosGreaves, XenosBoots, PowerBlade, NeedlePistol);
        }

        private static void RegisterOrdoXenosPatrol()
        {
            RegisterImperialRole(InterrogatorRole, "security", "Ordo Xenos Interrogator",
                PowerBlade, XenosHelmet, XenosCoat, XenosGreaves, XenosBoots);
            RegisterImperialRole(PistoleerRole, "soldier", "Inquisitorial Pistoleer",
                NeedlePistol, XenosHelmet, XenosCoat, XenosGreaves, XenosBoots);
            RegisterFormationPatrol(OrdoXenosTier, PhobosXenosTable, PhobosXenosGarrison, InterrogatorRole, PistoleerRole);
        }

        [HarmonyPatch(typeof(Control_stage1), "SpawnMonsters")]
        private static class PhobosQuarantineCounterattack
        {
            private static void Postfix(int wave, MapObstacle objective, RaidMetadata ____raidMetadata,
                Missions ____missions, LocationMetadata ____locationMetadata, Stations ____stations, Factions ____factions,
                Mercenaries ____mercenaries, PerkFactory ____perkFactory, Statistics ____statistics, Difficulty ____difficulty,
                TurnController ____turnController, DungeonGeneratedDebugData ____debugData, MapGrid ____mapGrid, Creatures ____creatures)
            {
                if (wave != 1 || !____raidMetadata.IsReversedMission || ____raidMetadata.StationId != PhobosStation)
                {
                    return;
                }

                var mission = ____missions.Get(____raidMetadata);
                if (mission.VictimFactionId != ItemCategory ||
                    !mission.LocationPlans[____locationMetadata.LocationId].CustomParameters.ContainsKey(PhobosXenosGarrison) ||
                    ____stations.Get(PhobosStation, logMissing: false)?.OwnerFactionId != ItemCategory ||
                    ____factions.Get(ItemCategory).CurrentTechLevel < OrdoXenosTier)
                {
                    return;
                }

                // Control objective floors start empty. Place the retinue with the first native attack wave.
                var patrol = UnitGenerationSystem.GenerateUnit(____statistics, ____difficulty,
                    Data.LocationUnitDrop.Get(PhobosXenosGarrison)[0], ItemCategory, OrdoXenosTier);
                if (patrol.Members.Count > 1)
                {
                    patrol.Members.RemoveRange(1, patrol.Members.Count - 1);
                }
                var monsters = SpawnSystem.SpawnFixedGroup(____mercenaries, ____perkFactory, ____difficulty, ____turnController,
                    ____debugData, ____mapGrid, ____creatures, ____raidMetadata, ____creatures.Player.CreatureData.Position,
                    patrol, minRadiusFromObject: 10, minRadiusGroup: 0, maxRadiusSpiral: 7, debugLabel: "Ordo Xenos counterattack");
                foreach (var monster in monsters)
                {
                    monster.Behaviour.SelectTarget(objective);
                }
            }
        }
    }
}

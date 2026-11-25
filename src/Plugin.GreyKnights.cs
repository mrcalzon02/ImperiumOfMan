using System.Collections.Generic;
using MGSC;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private const string TitanPaladinPatrol = "iomTitanPaladinContainment";
        private const string TitanPaladinGarrison = "iomTitanPaladinGarrison";
        private const string BroadswordPaladinPatrol = "iomBroadswordPaladinBoarding";
        private const string BroadswordPaladinGarrison = "iomBroadswordPaladinGarrison";
        private const string PaladinRole = "iom_grey_knights_paladin";
        private const string PaladinGunnerRole = "iom_grey_knights_paladin_gunner";
        private const string ArmsmanRole = "iom_grey_knights_armsman";

        private const string PaladinHelm = "iom_grey_knights_paladin_helmet";
        private const string PaladinCuirass = "iom_grey_knights_paladin_cuirass";
        private const string PaladinGreaves = "iom_grey_knights_paladin_greaves";
        private const string PaladinSabatons = "iom_grey_knights_paladin_sabatons";
        private const string NemesisSword = "iom_grey_knights_nemesis_sword";
        private const string Psycannon = "iom_grey_knights_psycannon";
        private const string AegisWard = "iom_grey_knights_aegis_ward";
        private const string PaladinPlans = "iom_grey_knights_requisition_chip";

        private static void RegisterGreyKnightsEquipment()
        {
            AddPaladinArmor<HelmetRecord>("helmets", PaladinHelm, "military_power_helmet_1", "Paladin Terminator Helm",
                575, 3.7f, CreateResists(blunt: 18, pierce: 20, lacer: 11, fire: 12, cold: 12, shock: 5, beam: 10));
            AddPaladinArmor<ArmorRecord>("armors", PaladinCuirass, "military_power_armor_1", "Paladin Terminator Cuirass",
                1050, 7.3f, CreateResists(blunt: 31, pierce: 36, lacer: 20, fire: 18, cold: 18, shock: 8, beam: 19));
            AddPaladinArmor<LeggingsRecord>("leggings", PaladinGreaves, "military_power_pants_1", "Paladin Terminator Greaves",
                790, 5.4f, CreateResists(blunt: 24, pierce: 27, lacer: 15, fire: 15, cold: 15, shock: 6, beam: 14));
            AddPaladinArmor<BootsRecord>("boots", PaladinSabatons, "military_power_boots_1", "Paladin Terminator Sabatons",
                495, 3.6f, CreateResists(blunt: 15, pierce: 17, lacer: 9, fire: 10, cold: 10, shock: 4, beam: 9));

            var sword = CloneItem<WeaponRecord>(NemesisSword, "sun_laser_powersword_1", TitanContractTier, 1200, NemesisSword);
            sword.Weight = 2.45f;
            sword.MaxDurability = 170;
            sword.MagazineCapacity = 12;
            sword.MinRandomAmmoCount = 8;
            SetDamage(sword, 56, 74);
            ApplyWeaponFinish(sword, EquipmentFinish.PaladinWeapon);
            RegisterItem("meleeweapons", sword, "Nemesis Force Sword",
                "A powered blade that deals Beam damage in close combat. Uses BatteryCells.");

            var cannon = CloneItem<WeaponRecord>(Psycannon, "army_hmg_1", TitanContractTier, 1300, Psycannon);
            cannon.Weight = 7.1f;
            cannon.Range = 7;
            cannon.MagazineCapacity = 21;
            cannon.MinRandomAmmoCount = 12;
            cannon.MaxDurability = 230;
            cannon.Firemodes = ["rifle_1", "rifle_3"];
            SetDamage(cannon, 43, 64);
            ApplyWeaponFinish(cannon, EquipmentFinish.PaladinWeapon);
            RegisterItem("rangeweapons", cannon, "Psycannon",
                "A heavy Grey Knights weapon with single and three-round fire modes. Uses Heavy rifle ammunition.");

            var ward = CloneItem<ConsumableRecord>(AegisWard, "pills_cryopentol", TitanContractTier, 260, AegisWard);
            ward.MaxUsage = 1;
            ward.MaxStack = 1;
            ward.QmorphosValue = -150;
            ward.Duration = 8;
            ward.FixWeights = new() { ["chaos"] = 0.45f };
            ward.Buffs = new() { ["healEfficiency"] = 1f };
            ward.EffectChance = new();
            ward.EffectProgression = new();
            RegisterItem("consumables", ward, "Aegis Ward",
                "A single-use ward that lowers active Quasimorphosis and improves Chaos wound treatment for eight turns.");
        }

        private static void AddPaladinArmor<T>(string table, string id, string donor, string name,
            float price, float weight, List<DmgResist> resists, int tier = TitanContractTier) where T : ResistRecord, IArmorRecord
        {
            var armor = CloneItem<T>(id, donor, tier, price, id);
            armor.Weight = weight;
            armor.MaxDurability = tier == GoldenTier ? 320 : 280;
            armor.ResistSheet = resists;
            ApplyArmorFinish(armor, tier == GoldenTier ? EquipmentFinish.GoldenPlate : EquipmentFinish.PaladinPlate);
            RegisterItem(table, armor, name, "Heavy warded power armor with balanced cold and fire resistance against Chaos damage.");
        }

        private static void RegisterGreyKnightsProduction()
        {
            IndustryRecipe(PaladinHelm, 24, 1, new(HeavyPlate, 1), new(Liner, 1), new("ceramite_plates", 2), new("capacitor_parts", 1));
            IndustryRecipe(PaladinCuirass, 38, 1, new(HeavyPlate, 3), new(Liner, 2), new("ceramite_plates", 3), new("capacitor_parts", 2));
            IndustryRecipe(PaladinGreaves, 29, 1, new(HeavyPlate, 2), new(Fittings, 2), new("ceramite_plates", 2));
            IndustryRecipe(PaladinSabatons, 20, 1, new(HeavyPlate, 1), new(Fittings, 2), new("ceramite_plates", 1), new("rubber", 2));
            IndustryRecipe(NemesisSword, 34, 1, new(Sheet, 2), new("lens", 2), new("weapon_parts", 2), new("microelectronics_parts", 2));
            IndustryRecipe(Psycannon, 36, 1, new(Sheet, 3), new(Fittings, 2), new("weapon_parts", 3), new("ceramite_plates", 2));
            IndustryRecipe(AegisWard, 12, 1, new(Fittings, 1), new(Textile, 1), new("microelectronics_parts", 2), new("lens", 1));
            AddCatalog(PaladinPlans, "Grey Knights Paladin requisition",
                "Patterns for warded Terminator armor, the Grey Knights weapons, the Aegis Ward, Tempest Bolt Clips and the Power Fist.",
                ItemCategory, TitanContractTier, 850, PaladinHelm, PaladinCuirass, PaladinGreaves, PaladinSabatons,
                NemesisSword, Psycannon, AegisWard);
        }

        private static void RegisterGreyKnightsPatrols()
        {
            RegisterImperialRole(PaladinRole, "security", "Grey Knights Paladin",
                NemesisSword, PaladinHelm, PaladinCuirass, PaladinGreaves, PaladinSabatons, AegisWard);
            RegisterImperialRole(PaladinGunnerRole, "soldier", "Grey Knights Paladin Gunner",
                Psycannon, PaladinHelm, PaladinCuirass, PaladinGreaves, PaladinSabatons, AegisWard);
            RegisterImperialRole(ArmsmanRole, "soldier", "Grey Knights Armsman",
                _lasgun.Id, _cadiaHelmet.Id, _cadiaChestplate.Id, _cadiaLeggings.Id, _cadiaBoots.Id);
            RegisterFormationPatrol(TitanContractTier, TitanPaladinPatrol, TitanPaladinGarrison, PaladinRole, ArmsmanRole);
            RegisterFormationPatrol(TitanContractTier, BroadswordPaladinPatrol, BroadswordPaladinGarrison, PaladinGunnerRole, ArmsmanRole);
        }
    }
}

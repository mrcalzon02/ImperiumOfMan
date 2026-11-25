using MGSC;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private const string GoldenHelm = "iom_golden_paladin_helmet";
        private const string GoldenCuirass = "iom_golden_paladin_cuirass";
        private const string GoldenGreaves = "iom_golden_paladin_greaves";
        private const string GoldenSabatons = "iom_golden_paladin_sabatons";
        private const string GoldenSword = "iom_golden_nemesis_sword";
        private const string GoldenCannon = "iom_golden_psycannon";
        private const string GoldenWard = "iom_golden_aegis_ward";
        private const string GoldenPlans = "iom_golden_requisition_chip";
        private const string GoldenPaladinRole = "iom_golden_paladin";
        private const string TerraGoldenPatrol = "iomTerraGoldenGuard";
        private const string TerraGoldenGarrison = "iomTerraGoldenGarrison";
        private const string TerraGoldenOffered = "iom.grey_knights.terra_golden_guard_offered";

        private static void RegisterGoldenPaladinEquipment()
        {
            AddPaladinArmor<HelmetRecord>("helmets", GoldenHelm, "military_power_helmet_1", "Golden Paladin Helm",
                725, 3.8f, CreateResists(blunt: 20, pierce: 22, lacer: 13, fire: 14, cold: 14, shock: 6, beam: 12), GoldenTier);
            AddPaladinArmor<ArmorRecord>("armors", GoldenCuirass, "military_power_armor_1", "Golden Paladin Cuirass",
                1300, 7.5f, CreateResists(blunt: 34, pierce: 39, lacer: 23, fire: 21, cold: 21, shock: 10, beam: 22), GoldenTier);
            AddPaladinArmor<LeggingsRecord>("leggings", GoldenGreaves, "military_power_pants_1", "Golden Paladin Greaves",
                990, 5.5f, CreateResists(blunt: 26, pierce: 29, lacer: 17, fire: 17, cold: 17, shock: 7, beam: 16), GoldenTier);
            AddPaladinArmor<BootsRecord>("boots", GoldenSabatons, "military_power_boots_1", "Golden Paladin Sabatons",
                620, 3.7f, CreateResists(blunt: 17, pierce: 19, lacer: 11, fire: 12, cold: 12, shock: 5, beam: 11), GoldenTier);

            var sword = CloneItem<WeaponRecord>(GoldenSword, "sun_laser_powersword_1", GoldenTier, 1520, GoldenSword);
            sword.Weight = 2.5f;
            sword.MaxDurability = 200;
            sword.MagazineCapacity = 14;
            sword.MinRandomAmmoCount = 9;
            SetDamage(sword, 61, 80);
            ApplyWeaponFinish(sword, EquipmentFinish.GoldenWeapon);
            RegisterItem("meleeweapons", sword, "Golden Nemesis Force Sword",
                "A gilded powered blade with a reinforced field emitter. Deals Beam damage and uses BatteryCells.");

            var cannon = CloneItem<WeaponRecord>(GoldenCannon, "army_hmg_1", GoldenTier, 1650, GoldenCannon);
            cannon.Weight = 7.3f;
            cannon.Range = 7;
            cannon.MagazineCapacity = 24;
            cannon.MinRandomAmmoCount = 15;
            cannon.MaxDurability = 260;
            cannon.Firemodes = ["rifle_1", "rifle_3"];
            SetDamage(cannon, 47, 69);
            ApplyWeaponFinish(cannon, EquipmentFinish.GoldenWeapon);
            RegisterItem("rangeweapons", cannon, "Golden Psycannon",
                "An improved heavy Grey Knights weapon with single and three-round fire modes. Uses Heavy rifle ammunition.");

            var ward = CloneItem<ConsumableRecord>(GoldenWard, "pills_cryopentol", GoldenTier, 340, GoldenWard);
            ward.MaxUsage = 1;
            ward.MaxStack = 1;
            ward.QmorphosValue = -220;
            ward.Duration = 10;
            ward.FixWeights = new() { ["chaos"] = 0.55f };
            ward.Buffs = new() { ["healEfficiency"] = 1f };
            ward.EffectChance = new();
            ward.EffectProgression = new();
            RegisterItem("consumables", ward, "Golden Aegis Ward",
                "A single-use ward that lowers active Quasimorphosis and improves Chaos wound treatment for ten turns.");
        }

        private static void RegisterGoldenPaladinProduction()
        {
            IndustryRecipe(GoldenHelm, 30, 1, new(HeavyPlate, 2), new(Liner, 1), new("ceramite_plates", 2), new("capacitor_parts", 2));
            IndustryRecipe(GoldenCuirass, 46, 1, new(HeavyPlate, 4), new(Liner, 2), new("ceramite_plates", 4), new("capacitor_parts", 2));
            IndustryRecipe(GoldenGreaves, 36, 1, new(HeavyPlate, 3), new(Fittings, 2), new("ceramite_plates", 3));
            IndustryRecipe(GoldenSabatons, 25, 1, new(HeavyPlate, 2), new(Fittings, 2), new("ceramite_plates", 1), new("rubber", 2));
            IndustryRecipe(GoldenSword, 41, 1, new(Sheet, 3), new("lens", 2), new("weapon_parts", 3), new("microelectronics_parts", 3));
            IndustryRecipe(GoldenCannon, 44, 1, new(Sheet, 4), new(Fittings, 3), new("weapon_parts", 4), new("ceramite_plates", 2));
            IndustryRecipe(GoldenWard, 16, 1, new(Fittings, 2), new(Textile, 1), new("scientific_equipment", 2), new(PuritySeal, 1));
            AddCatalog(GoldenPlans, "Golden Paladin requisition",
                "Patterns for Golden Paladin armor, the Golden Nemesis Force Sword, Golden Psycannon and Golden Aegis Ward. The ward consumes a Purity Seal.",
                ItemCategory, GoldenTier, 1125, GoldenHelm, GoldenCuirass, GoldenGreaves, GoldenSabatons,
                GoldenSword, GoldenCannon, GoldenWard);
        }

        private static void RegisterGoldenPaladinPatrol()
        {
            RegisterImperialRole(GoldenPaladinRole, "security", "Golden Paladin",
                GoldenSword, GoldenHelm, GoldenCuirass, GoldenGreaves, GoldenSabatons, GoldenWard);
            RegisterFormationPatrol(GoldenTier, TerraGoldenPatrol, TerraGoldenGarrison, GoldenPaladinRole, ArmsmanRole);
        }
    }
}

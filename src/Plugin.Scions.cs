using System.Collections.Generic;
using MGSC;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private const int ScionTier = 6;
        private const string TerraScionTable = "iomTerraScionEscort";
        private const string TerraScionGarrison = "iomTerraScionGarrison";
        private const string TerraExtractionOffered = "iom.scions.terra_escort_offered";
        private const string ScionRiflemanRole = "iom_tempestus_scion";
        private const string ScionSpecialistRole = "iom_tempestus_specialist";

        private const string ScionHelmet = "iom_scion_carapace_helmet";
        private const string ScionCuirass = "iom_scion_carapace_cuirass";
        private const string ScionGreaves = "iom_scion_carapace_greaves";
        private const string ScionBoots = "iom_scion_carapace_boots";
        private const string HotShotLasgun = "iom_scion_hotshot_lasgun";
        private const string HotShotPack = "iom_scion_hotshot_power_pack";
        private const string ScionTargeter = "iom_scion_targeter";
        private const string ScionPlans = "iom_scion_requisition_chip";

        private static void RegisterScionEquipment()
        {
            AddScionArmor<HelmetRecord>("helmets", ScionHelmet, "police_heavy_helmet_2", "Scion Carapace Helmet", 230, 2.35f, 155,
                CreateResists(blunt: 11, pierce: 14, lacer: 10, fire: 6, poison: 3, shock: 4, beam: 7));
            AddScionArmor<ArmorRecord>("armors", ScionCuirass, "military_heavy_armor_1", "Scion Carapace Cuirass", 365, 4.55f, 175,
                CreateResists(blunt: 18, pierce: 25, lacer: 14, fire: 11, poison: 4, shock: 5, beam: 14));
            AddScionArmor<LeggingsRecord>("leggings", ScionGreaves, "military_heavy_pants_1", "Scion Carapace Greaves", 275, 3.25f, 165,
                CreateResists(blunt: 12, pierce: 18, lacer: 10, fire: 7, poison: 2, shock: 4, beam: 10));
            AddScionArmor<BootsRecord>("boots", ScionBoots, "military_heavy_boots_1", "Scion Carapace Boots", 180, 2.05f, 155,
                CreateResists(blunt: 8, pierce: 10, lacer: 7, fire: 4, poison: 2, shock: 3, beam: 6));

            var pack = CloneItem<AmmoRecord>(HotShotPack, "battery_basic_ammo", ScionTier, 16);
            pack.Weight = 0.014f;
            pack.MaxStack = 18;
            pack.MinAmmoAmount = 3;
            pack.MaxAmmoAmount = 6;
            pack.ScatterMult = 0.8f;
            ApplyEquipmentSprites(pack.ItemDesc, EquipmentFinish.ScionCarapace);
            RegisterItem("ammo", pack, "Hot-shot Power Pack",
                "Compact BatteryCells ammunition with reduced scatter. Each stack holds up to 18 charges.");

            var rifle = CloneItem<WeaponRecord>(HotShotLasgun, "sun_laser_assault_1", ScionTier, 460);
            rifle.Weight = 3.8f;
            rifle.Range = 6;
            rifle.MagazineCapacity = 9;
            rifle.MinRandomAmmoCount = 6;
            rifle.ReloadDuration = 4;
            rifle.BonusAccuracy = 0.12f;
            rifle.BonusScatterAngle = 0.16f;
            rifle.Firemodes = ["beam_1", "beam_3"];
            rifle.OverrideAmmo = [];
            rifle.DefaultAmmoId = HotShotPack;
            rifle.Traits = [];
            SetDamage(rifle, 27, 40);
            ApplyWeaponFinish(rifle, EquipmentFinish.ScionWeapon);
            ApplyEquipmentSprites(rifle.ItemDesc, EquipmentFinish.ScionWeapon);
            RegisterItem("rangeweapons", rifle, "Hot-shot Lasgun",
                "Scion beam rifle with single shots and three-charge bursts. Accepts all BatteryCells ammunition.");

            var targeter = CloneItem<ImplantRecord>(ScionTargeter, "neural_scope", ScionTier, 475);
            targeter.ImplicitBonusEffects = new() { ["ranged_accuracy"] = 0.10f, ["scatter_angle"] = -0.10f };
            targeter.ImplicitPenaltyEffects = new() { ["resist_shock"] = -8 };
            ApplyEquipmentSprites(targeter.ItemDesc, EquipmentFinish.ScionCarapace);
            RegisterItem("implants", targeter, "Tempestus Targeter",
                "A head implant that improves ranged accuracy and narrows scatter at the cost of shock protection.");
        }

        private static void AddScionArmor<T>(string table, string id, string donor, string name, float price,
            float weight, int durability, List<DmgResist> resists) where T : ResistRecord, IArmorRecord
        {
            var armor = CloneItem<T>(id, donor, ScionTier, price);
            armor.ArmorClass = ArmorClass.HeavyArmor;
            armor.Weight = weight;
            armor.MaxDurability = durability;
            armor.ResistSheet = resists;
            armor.RepairItemIds = ["plastic", "armor_plates", "ceramite_plates"];
            ApplyArmorFinish(armor, EquipmentFinish.ScionCarapace);
            ApplyEquipmentSprites(armor.ItemDesc, EquipmentFinish.ScionCarapace);
            RegisterItem(table, armor, name, "Heavy Tempestus carapace with laminated protection against beam fire.");
        }

        private static void RegisterScionProduction()
        {
            IndustryRecipe(ScionHelmet, 12, 1, new(HeavyPlate, 1), new(Liner, 1), new(Fittings, 1));
            IndustryRecipe(ScionCuirass, 19, 1, new(HeavyPlate, 2), new(Flak, 2), new(Liner, 1), new(Fittings, 2));
            IndustryRecipe(ScionGreaves, 15, 1, new(HeavyPlate, 1), new(Flak, 1), new(Liner, 1), new(Fittings, 1));
            IndustryRecipe(ScionBoots, 10, 1, new(Plate, 1), new(PolymerStock, 1), new(Fittings, 1));
            IndustryRecipe(HotShotLasgun, 18, 1, new(Sheet, 2), new("weapon_parts", 2), new("lens", 2), new("circuit_board", 1));
            IndustryRecipe(HotShotPack, 5, 6, new(Cell, 2), new(Housing, 1), new("capacitor_parts", 1));
            IndustryRecipe(ScionTargeter, 14, 1, new("lens", 1), new("circuit_board", 1), new("microelectronics_parts", 2));
            AddCatalog(ScionPlans, "Tempestus requisition", "Patterns for Scion weapons, carapace, power packs, the Targeter, and Metal Storm and Helfrost Bolt Clips.",
                ItemCategory, ScionTier, 425, HotShotLasgun, HotShotPack, ScionTargeter, ScionHelmet, ScionCuirass,
                ScionGreaves, ScionBoots);
        }

        private static void RegisterScionPatrol()
        {
            var rifleman = RegisterImperialRole(ScionRiflemanRole, "soldier", "Tempestus Scion",
                HotShotLasgun, ScionHelmet, ScionCuirass, ScionGreaves, ScionBoots, "medical_glue");
            var specialist = RegisterImperialRole(ScionSpecialistRole, "security", "Tempestus Targeter Specialist",
                HotShotLasgun, ScionHelmet, ScionCuirass, ScionGreaves, ScionBoots, "smoke_grenade");
            // Give the specialist one deterministic implant and leave the rifleman on the standard gun profile.
            rifleman.ImplantCount = new(0, 0);
            specialist.ImplantCount = new(0, 0);
            specialist.GrantedImplants = [ScionTargeter];
            RegisterFormationPatrol(ScionTier, TerraScionTable, TerraScionGarrison, ScionSpecialistRole, ScionRiflemanRole);
        }
    }
}

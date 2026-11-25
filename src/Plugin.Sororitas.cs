using System.Collections.Generic;
using MGSC;
using Object = UnityEngine.Object;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private const int SororitasTier = 8;
        private const string TerraSororitasTable = "iomTerraSororitasRecovery";
        private const string TerraSororitasGarrison = "iomTerraSororitasGarrison";
        private const string TerraReliquaryOffered = "iom.sororitas.terra_reliquary_offered";
        private const string BattleSisterRole = "iom_sororitas_battle_sister";
        private const string FlamerSisterRole = "iom_sororitas_flamer_sister";

        private const string SororitasHelmet = "iom_sororitas_helmet";
        private const string SororitasCuirass = "iom_sororitas_cuirass";
        private const string SororitasGreaves = "iom_sororitas_greaves";
        private const string SororitasBoots = "iom_sororitas_boots";
        private const string SororitasBolter = "iom_sororitas_bolter";
        private const string SororitasFlamer = "iom_sororitas_flamer";
        private const string DevotionalWard = "iom_sororitas_devotional_ward";
        private const string SororitasReliquary = "iom_sororitas_reliquary";
        private const string SororitasReliquaryStory = "iom_sororitas_reliquary_recovery";
        private const string SororitasPlans = "iom_sororitas_requisition_chip";

        private static void RegisterSororitasEquipment()
        {
            AddSororitasArmor<HelmetRecord>("helmets", SororitasHelmet, "military_power_helmet_1", "Sororitas Power Helm",
                400, 2.9f, CreateResists(blunt: 14, pierce: 16, lacer: 9, fire: 12, cold: 6, beam: 8));
            AddSororitasArmor<ArmorRecord>("armors", SororitasCuirass, "military_power_armor_1", "Sororitas Power Cuirass",
                725, 5.5f, CreateResists(blunt: 25, pierce: 29, lacer: 17, fire: 22, cold: 10, beam: 15));
            AddSororitasArmor<LeggingsRecord>("leggings", SororitasGreaves, "military_power_pants_1", "Sororitas Power Greaves",
                540, 4.0f, CreateResists(blunt: 18, pierce: 21, lacer: 12, fire: 16, cold: 8, beam: 11));
            AddSororitasArmor<BootsRecord>("boots", SororitasBoots, "military_power_boots_1", "Sororitas Power Boots",
                350, 2.6f, CreateResists(blunt: 12, pierce: 13, lacer: 8, fire: 11, cold: 5, beam: 7));

            var bolter = _bolter.Clone(SororitasBolter);
            var descriptor = Object.Instantiate((WeaponDescriptor)_bolter.ContentDescriptor);
            descriptor.name = SororitasBolter;
            descriptor._overridenRenderId = string.Empty;
            bolter.ContentDescriptor = descriptor;
            bolter.TechLevel = SororitasTier;
            bolter.Price = 640;
            bolter.Weight = 4.4f;
            bolter.MaxDurability = 145;
            bolter.MagazineCapacity = 15;
            bolter.MinRandomAmmoCount = 9;
            bolter.Firemodes = ["rifle_1", "rifle_3"];
            SetDamage(bolter, 35, 54);
            RegisterSororitasWeapon(bolter, "Sororitas Bolter",
                "A Battle Sister's bolter with single and three-round fire modes. Uses Heavy rifle ammunition.");

            var flamer = CloneItem<WeaponRecord>(SororitasFlamer, "worker_flamethrower_1", SororitasTier, 680);
            flamer.Weight = 4.8f;
            flamer.Range = 4;
            flamer.MagazineCapacity = 50;
            flamer.MinRandomAmmoCount = 30;
            flamer.MaxDurability = 120;
            flamer.RepairItemIds = ["rod_parts", "rubber", "armor_plates"];
            SetDamage(flamer, 36, 65);
            RegisterSororitasWeapon(flamer, "Sororitas Flamer",
                "Projects a short stream of fire that can ignite the floor. Uses standard Gas fuel.");

            var ward = CloneItem<ConsumableRecord>(DevotionalWard, "pills_cryopentol", SororitasTier, 150);
            ward.MaxUsage = 1;
            ward.MaxStack = 1;
            ward.Duration = 8;
            ward.FixWeights = new() { ["fire"] = 0.4f };
            ward.Buffs = new() { ["healEfficiency"] = 1f };
            ward.EffectChance = new();
            ward.EffectProgression = new();
            ApplyEquipmentSprites(ward.ItemDesc, EquipmentFinish.SororitasArms);
            RegisterItem("consumables", ward, "Devotional Ward",
                "A single-use devotional seal that improves the chance of successfully treating burns for eight turns.");

            var reliquary = CloneItem<TrashRecord>(SororitasReliquary, "quest_fresco_data", SororitasTier, 999);
            reliquary.Categories = [];
            ApplyEquipmentSprites(reliquary.ItemDesc, EquipmentFinish.SororitasArms);
            RegisterItem("trash", reliquary, "Sororitas Reliquary", "A sealed reliquary to return to the Golden Throne.");
        }

        private static void AddSororitasArmor<T>(string table, string id, string donor, string name, float price,
            float weight, List<DmgResist> resists) where T : ResistRecord, IArmorRecord
        {
            var armor = CloneItem<T>(id, donor, SororitasTier, price);
            armor.Weight = weight;
            armor.ResistSheet = resists;
            ApplyArmorFinish(armor, EquipmentFinish.SororitasPlate);
            ApplyEquipmentSprites(armor.ItemDesc, EquipmentFinish.SororitasPlate);
            RegisterItem(table, armor, name, "Heat-shielded power armor issued to the Battle Sisters.");
        }

        private static void RegisterSororitasWeapon(WeaponRecord weapon, string name, string description)
        {
            ApplyWeaponFinish(weapon, EquipmentFinish.SororitasArms);
            ApplyEquipmentSprites(weapon.ItemDesc, EquipmentFinish.SororitasArms);
            RegisterItem("rangeweapons", weapon, name, description);
        }

        private static void RegisterSororitasProduction()
        {
            IndustryRecipe(SororitasHelmet, 17, 1, new(HeavyPlate, 1), new(Liner, 1), new("ceramite_plates", 1));
            IndustryRecipe(SororitasCuirass, 26, 1, new(HeavyPlate, 2), new(Liner, 1), new("ceramite_plates", 2), new("capacitor_parts", 2));
            IndustryRecipe(SororitasGreaves, 21, 1, new(HeavyPlate, 1), new(Fittings, 2), new("ceramite_plates", 1));
            IndustryRecipe(SororitasBoots, 15, 1, new(HeavyPlate, 1), new(Fittings, 1), new("rubber", 2));
            IndustryRecipe(SororitasBolter, 22, 1, new(Sheet, 2), new(Fittings, 1), new("weapon_parts", 2), new("spring", 2));
            IndustryRecipe(SororitasFlamer, 23, 1, new(Sheet, 2), new(Fittings, 2), new("rod_parts", 2), new("rubber", 2));
            IndustryRecipe(DevotionalWard, 7, 1, new(Textile, 1), new(PolymerStock, 1), new("pills_cryopentol", 1));
            AddCatalog(SororitasPlans, "Sororitas armory requisition", "Patterns for Battle Sister gear, Inferno Bolt Clips and the Melta Grenade.",
                ItemCategory, SororitasTier, 570, SororitasHelmet, SororitasCuirass, SororitasGreaves, SororitasBoots,
                SororitasBolter, SororitasFlamer, DevotionalWard);
        }

        private static void RegisterSororitasPatrol()
        {
            var sister = RegisterImperialRole(BattleSisterRole, "soldier", "Battle Sister",
                SororitasBolter, SororitasHelmet, SororitasCuirass, SororitasGreaves, SororitasBoots, DevotionalWard);
            var specialist = RegisterImperialRole(FlamerSisterRole, "soldier", "Battle Sister Flamer Specialist",
                SororitasFlamer, SororitasHelmet, SororitasCuirass, SororitasGreaves, SororitasBoots);
            sister.BodyTypes = ["woman"];
            specialist.BodyTypes = ["woman"];
            RegisterFormationPatrol(SororitasTier, TerraSororitasTable, TerraSororitasGarrison, FlamerSisterRole, BattleSisterRole);
        }

        private static void ConfigureReliquaryRecovery(Mission mission)
        {
            var parameters = mission.WinCondition.WinConditionParameters;
            string original = parameters[0];
            foreach (var plan in mission.LocationPlans.Values)
            {
                var drops = plan.AdditionalItemIdsDrop;
                for (int index = 0; index < drops.Count; index++)
                {
                    if (drops[index] == original)
                    {
                        drops[index] = SororitasReliquary;
                    }
                }
            }
            parameters[0] = SororitasReliquary;
            mission.StoryId = SororitasReliquaryStory;
            AddPuritySealRewards(mission);
        }
    }
}

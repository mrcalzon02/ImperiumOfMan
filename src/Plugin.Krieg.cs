using System.Collections.Generic;
using HarmonyLib;
using MGSC;
using UnityEngine;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private const int KriegTier = 5;
        private const string HavocKriegTable = "iomHavocKriegControl";
        private const string HavocKriegGarrison = "iomHavocKriegGarrison";
        private const string HavocSiegeOffered = "iom.krieg.havoc_control_offered";
        private const string SiegeRiflemanRole = "iom_krieg_siege_rifleman";
        private const string CombatEngineerRole = "iom_krieg_combat_engineer";

        private const string SiegeMask = "iom_krieg_siege_mask";
        private const string SiegeCoat = "iom_krieg_siege_greatcoat";
        private const string TrenchTrousers = "iom_krieg_trench_trousers";
        private const string TrenchBoots = "iom_krieg_trench_boots";
        private const string SiegeLasgun = "iom_krieg_siege_lasgun";
        private const string EngineerShotgun = "iom_krieg_engineer_shotgun";
        private const string EntrenchingTool = "iom_krieg_entrenching_tool";
        private const string DemolitionCharge = "iom_krieg_demolition_charge";
        private const string SiegePlans = "iom_krieg_siege_requisition_chip";

        private static void RegisterKriegEquipment()
        {
            AddSiegeArmor<HelmetRecord>("helmets", SiegeMask, "miner_gasmask_1", "Krieg Siege Mask", 165, 1.95f, 125,
                EquipmentFinish.KriegMask, CreateResists(blunt: 11, pierce: 10, lacer: 6, fire: 4, cold: 4, poison: 14));
            AddSiegeArmor<ArmorRecord>("armors", SiegeCoat, "ddr_reinforced_chem_coat_1", "Krieg Siege Greatcoat", 260, 3.85f, 140,
                EquipmentFinish.KriegCloth, CreateResists(blunt: 16, pierce: 21, lacer: 10, fire: 5, cold: 6, poison: 7));
            AddSiegeArmor<LeggingsRecord>("leggings", TrenchTrousers, "military_medium_pants_1", "Krieg Trench Trousers", 185, 2.65f, 130,
                EquipmentFinish.KriegCloth, CreateResists(blunt: 13, pierce: 16, lacer: 7, fire: 4, cold: 4, poison: 3));
            AddSiegeArmor<BootsRecord>("boots", TrenchBoots, "military_heavy_boots_1", "Krieg Trench Boots", 145, 1.9f, 130,
                EquipmentFinish.KriegCloth, CreateResists(blunt: 8, pierce: 9, lacer: 4, fire: 2, cold: 3, poison: 2));

            var rifle = _lasgun.Clone(SiegeLasgun);
            var descriptor = Object.Instantiate((WeaponDescriptor)_lasgun.ContentDescriptor);
            descriptor.name = SiegeLasgun;
            descriptor._overridenRenderId = string.Empty;
            rifle.ContentDescriptor = descriptor;
            rifle.TechLevel = KriegTier;
            rifle.Price = 295;
            rifle.Weight = 3.55f;
            rifle.MaxDurability = 135;
            rifle.MagazineCapacity = 9;
            rifle.MinRandomAmmoCount = 6;
            rifle.ReloadDuration = 3;
            rifle.Firemodes = ["pulse_1"];
            rifle.BonusAccuracy = 0.1f;
            SetDamage(rifle, 22, 34);
            RegisterKriegWeapon("rangeweapons", rifle, "Krieg Siege Lasgun",
                "A braced service rifle for deliberate single shots. Uses Las Clips and other BatteryCells.");

            var shotgun = CloneItem<WeaponRecord>(EngineerShotgun, "military_shotgun_1", KriegTier, 275);
            shotgun.MagazineCapacity = 3;
            shotgun.MinRandomAmmoCount = 2;
            SetDamage(shotgun, 38, 66);
            RegisterKriegWeapon("rangeweapons", shotgun, "Krieg Engineer Shotgun",
                "A short three-shell shotgun for close defense. Uses standard Shells.");

            var tool = CloneItem<WeaponRecord>(EntrenchingTool, "military_shovel_1", KriegTier, 140);
            SetDamage(tool, 30, 39);
            RegisterKriegWeapon("meleeweapons", tool, "Krieg Entrenching Tool",
                "A reinforced trench spade carried by combat engineers. Doubles as a blunt sidearm.");

            var charge = CloneItem<PlaceableDeviceRecord>(DemolitionCharge, "frag_mine", KriegTier, 155, DemolitionCharge);
            charge.MaxStack = 1;
            charge.TurnsToActivation = 3;
            charge.Explosion = "he_explosion";
            RegisterItem("placeable_devices", charge, "Krieg Demolition Charge",
                "A single-use proximity charge with a tight blast radius. Arms after a delay and flashes once active.");
        }

        private static void AddSiegeArmor<T>(string table, string id, string donor, string name, float price,
            float weight, int durability, EquipmentFinish finish, List<DmgResist> resists) where T : ResistRecord, IArmorRecord
        {
            var armor = CloneItem<T>(id, donor, KriegTier, price, id);
            armor.ArmorClass = ArmorClass.MediumArmor;
            armor.Weight = weight;
            armor.MaxDurability = durability;
            armor.RepairItemIds = id == SiegeMask ? ["rubber", "armor_plates"] : ["rags", "armor_plates"];
            armor.ResistSheet = resists;
            ApplyArmorFinish(armor, finish);
            RegisterItem(table, armor, name, "Reinforced trench gear with blunt and pierce protection.");
        }

        private static void RegisterKriegWeapon(string table, WeaponRecord weapon, string name, string description)
        {
            ApplyWeaponFinish(weapon, EquipmentFinish.KriegSteel);
            ApplyItemArtwork(weapon.ItemDesc, weapon.Id);
            RegisterItem(table, weapon, name, description);
        }

        private static void RegisterKriegProduction()
        {
            IndustryRecipe(SiegeMask, 10, 1, new(Plate, 1), new(Fittings, 1), new("rubber", 2));
            IndustryRecipe(SiegeCoat, 15, 1, new(Plate, 2), new(Flak, 2), new(Liner, 1), new(Fittings, 1));
            IndustryRecipe(TrenchTrousers, 12, 1, new(Plate, 1), new(Flak, 1), new(Liner, 1));
            IndustryRecipe(TrenchBoots, 9, 1, new(Plate, 1), new(PolymerStock, 1), new(Fittings, 1));
            IndustryRecipe(SiegeLasgun, 15, 1, new(Sheet, 2), new(Fittings, 1), new("weapon_parts", 2), new("lens", 1));
            IndustryRecipe(EngineerShotgun, 14, 1, new(Sheet, 2), new("weapon_parts", 2), new("spring", 2));
            IndustryRecipe(EntrenchingTool, 8, 1, new(Sheet, 1), new(Fittings, 1), new("rod_parts", 1));
            IndustryRecipe(DemolitionCharge, 12, 1, new("frag_mine", 1), new(Fittings, 1));
            AddCatalog(SiegePlans, "Krieg siege requisition", "Patterns for trench armor, siege and engineer kit, Bolt Rifle, Kraken Bolt Clips and Krak Grenades.",
                ItemCategory, KriegTier, 340, SiegeCoat, SiegeMask, TrenchTrousers, TrenchBoots, SiegeLasgun,
                EngineerShotgun, EntrenchingTool, DemolitionCharge);
        }

        private static void RegisterKriegPatrol()
        {
            RegisterImperialRole(SiegeRiflemanRole, "soldier", "Krieg Siege Rifleman",
                SiegeLasgun, SiegeMask, SiegeCoat, TrenchTrousers, TrenchBoots, "bandage");
            RegisterImperialRole(CombatEngineerRole, "security", "Krieg Combat Engineer",
                EngineerShotgun, SiegeMask, SiegeCoat, TrenchTrousers, TrenchBoots, EntrenchingTool, DemolitionCharge);
            RegisterFormationPatrol(KriegTier, HavocKriegTable, HavocKriegGarrison, CombatEngineerRole, SiegeRiflemanRole);
        }

        [HarmonyPatch(typeof(CreatureSystem), nameof(CreatureSystem.SpawnMonsterFromMobClass))]
        private static class KriegDefensiveBehavior
        {
            private static void Postfix(string mobClassId, Monster __result)
            {
                // Native defensive AI can take cover and deploy mines. Its setting is serialized with the monster.
                if (mobClassId == SiegeRiflemanRole || mobClassId == CombatEngineerRole)
                {
                    __result.Behaviour.DefenseOnly = true;
                }
            }
        }
    }
}

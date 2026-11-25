using MGSC;
using UnityEngine;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private const string LunaStation = "iomMoom";
        private const string LunaValhallanTable = "iomLunaValhallanSecurity";
        private const string LunaValhallanGarrison = "iomLunaValhallanGarrison";
        private const string LunaSecurityOffered = "iom.valhallan.luna_security_offered";
        private const int ValhallanTier = 3;

        private const string IceRiflemanRole = "iom_valhallan_rifleman";
        private const string IceMarksmanRole = "iom_valhallan_marksman";
        private const string WinterLasgun = "iom_winter_lasgun";
        private const string WinterCap = "iom_valhallan_cap";
        private const string WinterCoat = "iom_valhallan_coat";
        private const string WinterTrousers = "iom_valhallan_trousers";
        private const string WinterBoots = "iom_valhallan_boots";
        private const string Tanna = "iom_valhallan_tanna";
        private const string WinterInsulation = "iom_winter_insulation";
        private const string WinterPlans = "iom_valhallan_requisition_chip";

        private static void RegisterValhallanEquipment()
        {
            AddWinterArmor<HelmetRecord>("helmets", WinterCap, "common_hat_4", "Valhallan Insulated Cap", 95, 0.48f, 65, 10, 4, 2);
            AddWinterArmor<ArmorRecord>("armors", WinterCoat, "ddr_coat_1", "Valhallan Greatcoat", 180, 1.8f, 95, 20, 8, 8);
            AddWinterArmor<LeggingsRecord>("leggings", WinterTrousers, "miner_pants_1", "Valhallan Lined Trousers", 125, 1.2f, 75, 13, 5, 4);
            AddWinterArmor<BootsRecord>("boots", WinterBoots, "miner_boots_1", "Valhallan Cold-Weather Boots", 100, 0.8f, 80, 9, 4, 2);

            WeaponRecord rifle = _lasgun.Clone(WinterLasgun);
            var weaponDescriptor = Object.Instantiate((WeaponDescriptor)_lasgun.ContentDescriptor);
            weaponDescriptor.name = WinterLasgun;
            weaponDescriptor._overridenRenderId = string.Empty;
            rifle.ContentDescriptor = weaponDescriptor;
            rifle.TechLevel = ValhallanTier;
            rifle.Price = 220;
            rifle.MaxDurability = 120;
            rifle.MagazineCapacity = 12;
            rifle.RepairItemIds = ["lens", "bulb", "weapon_parts"];
            SetDamage(rifle, 16, 29);
            ApplyWeaponFinish(rifle, EquipmentFinish.WinterRifle);
            ApplyItemArtwork(weaponDescriptor, WinterLasgun);
            RegisterItem("rangeweapons", rifle, "Winter-pattern Lasgun", "A durable Valhallan service rifle chambered for Las Clips and other BatteryCells.");

            var tanna = CloneItem<ConsumableRecord>(Tanna, "water_bottle_1", ValhallanTier, 20, Tanna);
            tanna.EffectChance = new();
            tanna.EffectProgression = new() { ["coldEffect"] = -25 };
            RegisterItem("consumables", tanna, "Valhallan Tanna", "Hot field tea that reduces existing cold buildup.");
        }

        private static void AddWinterArmor<T>(string table, string id, string donor, string name,
            float price, float weight, int durability, int cold, int cryo, int pierce) where T : ResistRecord
        {
            var armor = CloneItem<T>(id, donor, ValhallanTier, price, id);
            armor.Weight = weight;
            armor.MaxDurability = durability;
            armor.RepairItemIds = ["rags", "plastic"];
            armor.ResistSheet = [.. armor.ResistSheet];
            armor.SetResist("cold", cold);
            armor.SetResist("pierce", pierce);
            if (armor.ResistSheet.Exists(resist => resist.damage == "cryo"))
            {
                armor.SetResist("cryo", cryo);
            }
            else
            {
                armor.ResistSheet.Add(new DmgResist { damage = "cryo", resistPercent = cryo });
            }
            ApplyArmorFinish(armor, EquipmentFinish.WinterCloth);
            RegisterItem(table, armor, name, "Insulated Valhallan field clothing with cold and cryo resistance.");
        }

        private static void RegisterValhallanProduction()
        {
            AddMaterial(WinterInsulation, "rags", "Winter insulation", "Layered batting for the Valhallan field set.", 17, ValhallanTier);
            IndustryRecipe(WinterInsulation, 5, 2, new(Textile, 2), new(PolymerStock, 1));
            IndustryRecipe(WinterCap, 5, 1, new(WinterInsulation, 1), new(Liner, 1));
            IndustryRecipe(WinterCoat, 10, 1, new(WinterInsulation, 2), new(Flak, 2), new(Fittings, 1));
            IndustryRecipe(WinterTrousers, 7, 1, new(WinterInsulation, 1), new(Flak, 1), new(Liner, 1));
            IndustryRecipe(WinterBoots, 6, 1, new(WinterInsulation, 1), new(PolymerStock, 1), new(Fittings, 1));
            IndustryRecipe(WinterLasgun, 13, 1, new(Sheet, 2), new(Fittings, 1), new("weapon_parts", 2));
            IndustryRecipe(Tanna, 3, 2, new("water_bottle_1", 1), new(Biomass, 1));
            AddCatalog(WinterPlans, "Valhallan winter requisition", "Patterns for the winter kit, rifle, insulation and Tanna.",
                ItemCategory, ValhallanTier, 215, WinterCoat, WinterCap, WinterTrousers,
                WinterBoots, WinterLasgun, WinterInsulation, Tanna);
        }

        private static void RegisterValhallanPatrol()
        {
            RegisterImperialRole(IceRiflemanRole, "soldier", "Valhallan Ice Rifleman",
                WinterLasgun, WinterCap, WinterCoat, WinterTrousers, WinterBoots, Tanna);
            RegisterImperialRole(IceMarksmanRole, "soldier", "Valhallan Ice Marksman",
                _longLas.Id, WinterCap, WinterCoat, WinterTrousers, WinterBoots, Tanna);
            RegisterFormationPatrol(ValhallanTier, LunaValhallanTable, LunaValhallanGarrison, IceMarksmanRole, IceRiflemanRole);
        }

        [Hook(ModHookType.SpaceUpdateAfterGameLoop)]
        public static void OfferLunaSecurity(IModContext context)
            => OfferFormationContract(context, LunaStation, LunaValhallanTable, LunaValhallanGarrison, LunaSecurityOffered, ValhallanTier);
    }
}

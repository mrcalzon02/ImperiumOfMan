using MGSC;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private const string HavocStation = "iomHavoc";
        private const int SteelLegionTier = 3;
        private const string HavocLegionTable = "iomHavocSteelLegionSecurity";
        private const string HavocLegionGarrison = "iomHavocSteelLegionGarrison";
        private const string HavocSecurityOffered = "iom.steel_legion.havoc_security_offered";
        private const string LegionRiflemanRole = "iom_steel_legion_rifleman";
        private const string LegionSupportRole = "iom_steel_legion_support";

        private const string LegionMask = "iom_steel_legion_mask";
        private const string LegionCoat = "iom_steel_legion_coat";
        private const string LegionTrousers = "iom_steel_legion_trousers";
        private const string LegionBoots = "iom_steel_legion_boots";
        private const string LegionCarbine = "iom_steel_legion_lascarbine";
        private const string LegionSmoke = "iom_steel_legion_smoke";
        private const string LegionFilter = "iom_steel_legion_filter";
        private const string LegionSeal = "iom_steel_legion_seal";
        private const string LegionPlans = "iom_steel_legion_requisition_chip";

        private static void RegisterSteelLegionEquipment()
        {
            AddLegionArmor<HelmetRecord>("helmets", LegionMask, "miner_gasmask_1",
                "Steel Legion Respirator", 135, 1.7f, 120, 24, 7, 2);
            AddLegionArmor<ArmorRecord>("armors", LegionCoat, "medical_chem_shirt_1",
                "Steel Legion Sealed Flak Coat", 195, 2.8f, 115, 16, 17, 4);
            AddLegionArmor<LeggingsRecord>("leggings", LegionTrousers, "medical_chem_pants_1",
                "Steel Legion Overtrousers", 135, 1.7f, 100, 7, 6, 3);
            AddLegionArmor<BootsRecord>("boots", LegionBoots, "miner_medium_boots_1",
                "Steel Legion Sealed Boots", 110, 1.55f, 95, 5, 6, 4);

            var carbine = CloneItem<WeaponRecord>(LegionCarbine, "laser_smg_1", SteelLegionTier, 245, LegionCarbine);
            carbine.Weight = 1.7f;
            carbine.MagazineCapacity = 16;
            carbine.Range = 5;
            carbine.ReloadDuration = 3;
            carbine.DefaultAmmoId = LasClip;
            carbine.OverrideAmmo = [];
            SetDamage(carbine, 19, 28);
            ApplyWeaponFinish(carbine, EquipmentFinish.LegionCarbine);
            RegisterItem("rangeweapons", carbine, "Steel Legion Lascarbine",
                "Compact beam carbine with single and four-pulse fire. Issued with Guard Las Clips.");

            var smoke = CloneItem<GrenadeRecord>(LegionSmoke, "smoke_grenade", SteelLegionTier, 70, LegionSmoke);
            RegisterItem("grenades", smoke, "Steel Legion Smoke Grenade",
                "Produces an obscuring smoke cloud for the squad.");

            var filter = CloneItem<RepairRecord>(LegionFilter, "armor_repair_kit", SteelLegionTier, 26, LegionFilter);
            filter.RepairSpecialRule = RepairSpecialRule.None;
            filter.RestoreAmount = 24;
            filter.MaxCapacity = 4;
            filter.MaxUsage = 1;
            filter.UsageCost = 1;
            filter.MaxStack = 5;
            filter.Weight = 0.1f;
            filter.Disassembly = [];
            RegisterItem("repairs", filter, "Steel Legion Spare Filter",
                "Replacement cartridge that restores durability to a Steel Legion respirator.");
        }

        private static void AddLegionArmor<T>(string table, string id, string donor, string name,
            float price, float weight, int durability, int poison, int pierce, int cold) where T : ResistRecord, IArmorRecord
        {
            var armor = CloneItem<T>(id, donor, SteelLegionTier, price, id);
            armor.ArmorClass = ArmorClass.MediumArmor;
            armor.Weight = weight;
            armor.MaxDurability = durability;
            armor.RepairItemIds = id == LegionMask ? [LegionFilter] : ["rubber", "armor_plates"];
            armor.ResistSheet = [.. armor.ResistSheet];
            armor.SetResist("poison", poison);
            armor.SetResist("pierce", pierce);
            armor.SetResist("cold", cold);
            ApplyArmorFinish(armor, id == LegionMask ? EquipmentFinish.LegionMask : EquipmentFinish.LegionCloth);
            RegisterItem(table, armor, name, "Sealed Armageddon field armor for toxic industrial deployments.");
        }

        private static void RegisterSteelLegionProduction()
        {
            AddMaterial(LegionSeal, "rubber", "Steel Legion seal lining", "Cut gaskets and reinforced seam lining.", 19, SteelLegionTier);
            IndustryRecipe(LegionSeal, 4, 2, new(PolymerStock, 2), new(Textile, 1));
            IndustryRecipe(LegionFilter, 4, 2, new(LegionSeal, 1), new("pills_sorbent", 1));
            IndustryRecipe(LegionMask, 8, 1, new(LegionSeal, 1), new(LegionFilter, 1), new(Plate, 1));
            IndustryRecipe(LegionCoat, 11, 1, new(LegionSeal, 2), new(Flak, 2), new(Fittings, 1));
            IndustryRecipe(LegionTrousers, 7, 1, new(LegionSeal, 1), new(Flak, 1), new(Liner, 1));
            IndustryRecipe(LegionBoots, 7, 1, new(LegionSeal, 1), new(PolymerStock, 1), new(Fittings, 1));
            IndustryRecipe(LegionCarbine, 12, 1, new(Sheet, 2), new("weapon_parts", 2), new("lens", 1));
            IndustryRecipe(LegionSmoke, 5, 2, new(PolymerStock, 1), new("tin", 2), new("acid", 1));
            AddCatalog(LegionPlans, "Steel Legion requisition", "Patterns for sealed gear, the Lascarbine, smoke and replacement filters.",
                ItemCategory, SteelLegionTier, 235, LegionCoat, LegionMask, LegionTrousers,
                LegionBoots, LegionCarbine, LegionSmoke, LegionFilter, LegionSeal);
        }

        private static void RegisterSteelLegionPatrol()
        {
            RegisterImperialRole(LegionRiflemanRole, "soldier", "Steel Legion Trooper",
                LegionCarbine, LegionMask, LegionCoat, LegionTrousers, LegionBoots, LegionSmoke);
            RegisterImperialRole(LegionSupportRole, "soldier", "Steel Legion Industrial Support Trooper",
                LegionCarbine, LegionMask, LegionCoat, LegionTrousers, LegionBoots, LegionFilter, GuardFrag);
            RegisterFormationPatrol(SteelLegionTier, HavocLegionTable, HavocLegionGarrison, LegionSupportRole, LegionRiflemanRole);
        }

        [Hook(ModHookType.SpaceUpdateAfterGameLoop)]
        public static void OfferHavocContracts(IModContext context)
        {
            OfferFormationContract(context, HavocStation, HavocKriegTable, HavocKriegGarrison,
                HavocSiegeOffered, KriegTier, FormationDeployment.Entrance, ProceduralMissionType.Control);
            OfferFormationContract(context, HavocStation, HavocLegionTable, HavocLegionGarrison, HavocSecurityOffered, SteelLegionTier);
        }
    }
}

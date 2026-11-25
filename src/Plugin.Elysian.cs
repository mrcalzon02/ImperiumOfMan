using MGSC;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private const int ElysianTier = 4;
        private const string PhobosStation = "iomPhobos";
        private const string PhobosElysianTable = "iomPhobosElysianSecurity";
        private const string PhobosElysianGarrison = "iomPhobosElysianGarrison";
        private const string PhobosSecurityOffered = "iom.elysian.phobos_security_offered";
        private const string DropRiflemanRole = "iom_elysian_rifleman";
        private const string DropSpotterRole = "iom_elysian_spotter";

        private const string DropHelmet = "iom_elysian_drop_helmet";
        private const string DropVest = "iom_elysian_flak_vest";
        private const string DropTrousers = "iom_elysian_harness_trousers";
        private const string DropBoots = "iom_elysian_landing_boots";
        private const string DropCarbine = "iom_elysian_drop_lascarbine";
        private const string DropHarness = "iom_elysian_drop_harness";
        private const string DropPlans = "iom_elysian_requisition_chip";

        private static void RegisterElysianEquipment()
        {
            AddDropArmor<HelmetRecord>("helmets", DropHelmet, "military_light_helmet_1", "Elysian Drop Helmet",
                105, 0.9f, 90, 4, 6);
            AddDropArmor<ArmorRecord>("armors", DropVest, "military_shirt_3", "Elysian Flak Vest",
                170, 2.1f, 88, 7, 10);
            AddDropArmor<LeggingsRecord>("leggings", DropTrousers, "military_pants_2", "Elysian Harness Trousers",
                125, 1.55f, 82, 5, 6);
            AddDropArmor<BootsRecord>("boots", DropBoots, "military_light_boots_1", "Elysian Landing Boots",
                100, 1.15f, 90, 5, 5);

            var carbine = CloneItem<WeaponRecord>(DropCarbine, "laser_smg_1", ElysianTier, 235);
            carbine.Weight = 0.95f;
            carbine.MagazineCapacity = 11;
            carbine.Range = 5;
            carbine.ReloadDuration = 2;
            carbine.DefaultAmmoId = LasClip;
            carbine.OverrideAmmo = [];
            SetDamage(carbine, 18, 26);
            ApplyWeaponFinish(carbine, EquipmentFinish.ElysianCarbine);
            ApplyEquipmentSprites(carbine.ItemDesc, EquipmentFinish.ElysianCarbine);
            RegisterItem("rangeweapons", carbine, "Elysian Drop Lascarbine",
                "A lightweight beam carbine with quick reloads. Uses Las Clips and other BatteryCells.");

            var harness = CloneItem<BackpackRecord>(DropHarness, "small_backpack_1", ElysianTier, 120);
            harness.Width = 4;
            harness.Height = 2;
            harness.Weight = 0.22f;
            harness.BackpackWeightMult = 0.3f;
            ApplyEquipmentSprites(harness.ItemDesc, EquipmentFinish.ElysianCloth);
            RegisterItem("backpacks", harness, "Elysian Drop Harness",
                "A compact flight harness with eight slots and reduced carried weight for its contents.");
        }

        private static void AddDropArmor<T>(string table, string id, string donor, string name, float price,
            float weight, int durability, int blunt, int pierce) where T : ResistRecord, IArmorRecord
        {
            var armor = CloneItem<T>(id, donor, ElysianTier, price);
            armor.ArmorClass = ArmorClass.LightArmor;
            armor.Weight = weight;
            armor.MaxDurability = durability;
            armor.RepairItemIds = ["rags", "plastic"];
            armor.ResistSheet = [.. armor.ResistSheet];
            armor.SetResist("blunt", blunt);
            armor.SetResist("pierce", pierce);
            ApplyArmorFinish(armor, EquipmentFinish.ElysianCloth);
            ApplyEquipmentSprites(armor.ItemDesc, EquipmentFinish.ElysianCloth);
            RegisterItem(table, armor, name, "Lightweight Elysian flight kit for orbital infantry.");
        }

        private static void RegisterElysianProduction()
        {
            IndustryRecipe(DropHelmet, 6, 1, new(Flak, 1), new(Liner, 1), new(Fittings, 1));
            IndustryRecipe(DropVest, 9, 1, new(Flak, 2), new(Liner, 1), new(Fittings, 1));
            IndustryRecipe(DropTrousers, 7, 1, new(Flak, 1), new(Textile, 2), new(Fittings, 1));
            IndustryRecipe(DropBoots, 6, 1, new(PolymerStock, 1), new(Liner, 1), new(Fittings, 1));
            IndustryRecipe(DropCarbine, 11, 1, new(Sheet, 1), new("weapon_parts", 2), new("lens", 1));
            IndustryRecipe(DropHarness, 5, 1, new(Textile, 2), new(Liner, 1), new(Fittings, 1));
            AddCatalog(DropPlans, "Elysian drop requisition", "Patterns for flight armor, the drop harness, Drop Lascarbine, Bolt Pistol and standard Bolt Clips.",
                ItemCategory, ElysianTier, 240, DropVest, DropHelmet, DropTrousers, DropBoots,
                DropCarbine, DropHarness);
        }

        private static void RegisterElysianPatrol()
        {
            RegisterImperialRole(DropRiflemanRole, "soldier", "Elysian Drop Rifleman",
                DropCarbine, DropHelmet, DropVest, DropTrousers, DropBoots, DropHarness);
            RegisterImperialRole(DropSpotterRole, "soldier", "Elysian Drop Spotter",
                _longLas.Id, DropHelmet, DropVest, DropTrousers, DropBoots, DropHarness, "smoke_grenade");
            RegisterFormationPatrol(ElysianTier, PhobosElysianTable, PhobosElysianGarrison, DropSpotterRole, DropRiflemanRole);
        }

        [Hook(ModHookType.SpaceUpdateAfterGameLoop)]
        public static void OfferPhobosContracts(IModContext context)
        {
            OfferFormationContract(context, PhobosStation, PhobosXenosTable, PhobosXenosGarrison,
                PhobosQuarantineOffered, OrdoXenosTier, FormationDeployment.FinalFloor, ProceduralMissionType.Sabotage,
                mission => mission.StoryId = PhobosQuarantineStory,
                floor => floor.CustomParameters[PhobosXenosGarrison] = "1");
            OfferFormationContract(context, PhobosStation, PhobosElysianTable, PhobosElysianGarrison,
                PhobosSecurityOffered, ElysianTier, FormationDeployment.Entrance);
        }
    }
}

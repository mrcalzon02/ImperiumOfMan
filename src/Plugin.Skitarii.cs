using System.Collections.Generic;
using MGSC;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private const int SkitariiTier = 7;
        private const string FoundrySkitariiTable = "iomFoundrySkitariiRecovery";
        private const string FoundrySkitariiGarrison = "iomFoundrySkitariiGarrison";
        private const string FoundryRecoveryOffered = "iom.skitarii.foundry_recovery_offered";
        private const string RangerRole = "iom_skitarii_ranger";
        private const string VanguardRole = "iom_skitarii_vanguard";

        private const string SkitariiHood = "iom_skitarii_rebreather_hood";
        private const string SkitariiCoat = "iom_skitarii_insulated_coat";
        private const string SkitariiLeggings = "iom_skitarii_sealed_leggings";
        private const string SkitariiBoots = "iom_skitarii_insulated_boots";
        private const string GalvanicRifle = "iom_skitarii_galvanic_rifle";
        private const string ArcRifle = "iom_skitarii_arc_rifle";
        private const string SkitariiAuspex = "iom_skitarii_auspex";
        private const string SkitariiRepairKit = "iom_skitarii_repair_kit";
        private const string CircuitAssembly = "iom_skitarii_recovered_circuit";
        private const string SkitariiPlans = "iom_skitarii_requisition_chip";

        private static void RegisterSkitariiEquipment()
        {
            AddSkitariiArmor<HelmetRecord>("helmets", SkitariiHood, "science_chem_helmet_1", "Skitarii Rebreather Hood", 220, 1.55f, 145,
                CreateResists(blunt: 6, pierce: 7, lacer: 4, fire: 5, poison: 10, shock: 12, beam: 4));
            AddSkitariiArmor<ArmorRecord>("armors", SkitariiCoat, "ddr_chem_coat_1", "Skitarii Insulated Coat", 365, 3.35f, 165,
                CreateResists(blunt: 12, pierce: 16, lacer: 8, fire: 9, poison: 11, shock: 23, beam: 8));
            AddSkitariiArmor<LeggingsRecord>("leggings", SkitariiLeggings, "science_chem_pants_1", "Skitarii Sealed Leggings", 255, 2.35f, 150,
                CreateResists(blunt: 8, pierce: 10, lacer: 5, fire: 5, poison: 6, shock: 15, beam: 5));
            AddSkitariiArmor<BootsRecord>("boots", SkitariiBoots, "science_chem_boots_1", "Skitarii Insulated Boots", 165, 1.5f, 140,
                CreateResists(blunt: 5, pierce: 6, lacer: 4, fire: 4, poison: 4, shock: 12, beam: 3));

            var galvanic = CloneItem<WeaponRecord>(GalvanicRifle, "army_sniper_1", SkitariiTier, 560);
            galvanic.Range = 8;
            galvanic.Weight = 4.1f;
            galvanic.MagazineCapacity = 8;
            galvanic.MinRandomAmmoCount = 5;
            galvanic.ReloadDuration = 4;
            galvanic.MaxDurability = 120;
            galvanic.Traits = ["optic_sight"];
            galvanic.RepairItemIds = ["spring", "weapon_parts", SkitariiRepairKit];
            SetDamage(galvanic, 44, 62);
            RegisterSkitariiWeapon(galvanic, "Skitarii Galvanic Rifle", "A Ranger marksman rifle for deliberate single shots. Uses Heavy rifle ammunition.");

            var arc = CloneItem<WeaponRecord>(ArcRifle, "fra_shock_smg_1", SkitariiTier, 625);
            arc.WeaponClass = WeaponClass.AssaultRifle;
            arc.MagazineCapacity = 12;
            arc.MinRandomAmmoCount = 7;
            arc.Firemodes = ["lightning_1", "lightning_2"];
            arc.DefaultAmmoId = LasClip;
            // Use the game's electrical payload in both fire modes while drawing charges from standard batteries.
            arc.OverrideAmmo = ["implicted_stungun", "implicted_stungun"];
            arc.Traits = [];
            arc.RepairItemIds = ["transformer", "microelectronics_parts", SkitariiRepairKit];
            SetDamage(arc, 12, 36);
            RegisterSkitariiWeapon(arc, "Vanguard Arc Rifle", "A short-range shock weapon with single or paired arcs. Uses BatteryCells ammunition.");

            var auspex = CloneItem<DeviceRecord>(SkitariiAuspex, "automap", SkitariiTier, 190, SkitariiAuspex);
            auspex.MaxUsage = 1;
            auspex.MaxStack = 1;
            RegisterItem("general_devices", auspex, "Skitarii Auspex", "A one-use survey device that maps the current floor.");

            var kit = CloneItem<RepairRecord>(SkitariiRepairKit, "tech_repair_kit", SkitariiTier, 170, SkitariiRepairKit);
            kit.RepairSpecialRule = RepairSpecialRule.None;
            kit.MaxCapacity = 4;
            kit.MaxUsage = 1;
            kit.MaxStack = 1;
            kit.RestoreAmount = 38;
            kit.Disassembly = [];
            RegisterItem("repairs", kit, "Skitarii Field Repair Kit", "Restores durability to a Galvanic Rifle or Vanguard Arc Rifle.");

            AddMaterial(CircuitAssembly, "circuit_board", "Recovered Circuit Assembly",
                "Reconditioned electronics for Skitarii weapons and field equipment.", 46, SkitariiTier);
        }

        private static void AddSkitariiArmor<T>(string table, string id, string donor, string name, float price,
            float weight, int durability, List<DmgResist> resists) where T : ResistRecord, IArmorRecord
        {
            var armor = CloneItem<T>(id, donor, SkitariiTier, price, id);
            armor.ArmorClass = ArmorClass.MediumArmor;
            armor.Weight = weight;
            armor.MaxDurability = durability;
            armor.ResistSheet = resists;
            armor.RepairItemIds = ["rubber", "armor_plates"];
            ApplyArmorFinish(armor, EquipmentFinish.SkitariiCloth);
            RegisterItem(table, armor, name, "Sealed forge-world armor with electrical insulation.");
        }

        private static void RegisterSkitariiWeapon(WeaponRecord weapon, string name, string description)
        {
            ApplyWeaponFinish(weapon, EquipmentFinish.SkitariiMetal);
            ApplyItemArtwork(weapon.ItemDesc, weapon.Id);
            RegisterItem("rangeweapons", weapon, name, description);
        }

        private static void RegisterSkitariiProduction()
        {
            IndustryRecipe(CircuitAssembly, 6, 1, new("circuit_board", 1), new(Conductors, 1), new("microelectronics_parts", 1));
            IndustryRecipe(SkitariiHood, 11, 1, new(Plate, 1), new(Liner, 1), new("rubber", 2), new(CircuitAssembly, 1));
            IndustryRecipe(SkitariiCoat, 16, 1, new(Flak, 2), new(Liner, 1), new(Fittings, 1), new("rubber", 2));
            IndustryRecipe(SkitariiLeggings, 12, 1, new(Flak, 1), new(Textile, 1), new(Fittings, 1), new("rubber", 1));
            IndustryRecipe(SkitariiBoots, 9, 1, new(PolymerStock, 1), new(Fittings, 1), new("rubber", 1));
            IndustryRecipe(GalvanicRifle, 18, 1, new(Sheet, 2), new("weapon_parts", 2), new("lens", 1), new(CircuitAssembly, 1));
            IndustryRecipe(ArcRifle, 20, 1, new(Sheet, 2), new("transformer", 1), new(Cell, 1), new(CircuitAssembly, 2));
            IndustryRecipe(SkitariiAuspex, 8, 1, new(CircuitAssembly, 1), new("lens", 1), new(PolymerStock, 1));
            IndustryRecipe(SkitariiRepairKit, 7, 1, new(CircuitAssembly, 1), new(Fittings, 1), new("spring", 1));
            AddCatalog(SkitariiPlans, "Skitarii forge requisition", "Patterns for Ranger and Vanguard gear, Antiphasic Bolt Clips and the Subdermal Underplate.",
                ItemCategory, SkitariiTier, 495, GalvanicRifle, ArcRifle, SkitariiHood, SkitariiCoat, SkitariiLeggings,
                SkitariiBoots, SkitariiAuspex, SkitariiRepairKit, CircuitAssembly);
        }

        private static void RegisterSkitariiPatrol()
        {
            RegisterImperialRole(RangerRole, "sniper", "Skitarii Ranger",
                GalvanicRifle, SkitariiHood, SkitariiCoat, SkitariiLeggings, SkitariiBoots, SkitariiAuspex);
            RegisterImperialRole(VanguardRole, "soldier", "Skitarii Vanguard",
                ArcRifle, SkitariiHood, SkitariiCoat, SkitariiLeggings, SkitariiBoots, SkitariiRepairKit);
            RegisterFormationPatrol(SkitariiTier, FoundrySkitariiTable, FoundrySkitariiGarrison, VanguardRole, RangerRole);
        }

        [Hook(ModHookType.SpaceUpdateAfterGameLoop)]
        public static void OfferFoundryDataRecovery(IModContext context)
            => OfferFormationContract(context, FoundryStation, FoundrySkitariiTable, FoundrySkitariiGarrison,
                FoundryRecoveryOffered, SkitariiTier, FormationDeployment.Entrance, ProceduralMissionType.Espionage);
    }
}

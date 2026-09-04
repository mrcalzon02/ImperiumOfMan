using MGSC;
using UnityEngine;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private static BackpackRecord _servitorBackpack;
        private static WeaponRecord _bolter;
        private static WeaponRecord _lasgun;
        private static WeaponRecord _plasmaPistol;
        private static WeaponRecord _melta;
        private static WeaponRecord _longLas;
        private static BootsRecord _lightBoots;
        private static LeggingsRecord _lightLeggings;
        private static ArmorRecord _lightChestplate;
        private static HelmetRecord _lightHelmet;
        private static BootsRecord _cadiaBoots;
        private static LeggingsRecord _cadiaLeggings;
        private static ArmorRecord _cadiaChestplate;
        private static HelmetRecord _cadiaHelmet;

        private static void RegisterEquipment()
        {
            RegisterServitorBackpack();
            RegisterWeapons();
            RegisterArmor();
        }

        private static void RegisterServitorBackpack()
        {
            BackpackDescriptor descriptor = LoadRequiredAsset<BackpackDescriptor>("Servitor");
            _servitorBackpack = new BackpackRecord
            {
                Id = "iom_sevitor_backpack",
                ContentDescriptor = descriptor,
                Categories = [ItemCategory],
                TechLevel = 1,
                Price = 600,
                Weight = 0.01f,
                InventoryWidthSize = 1,
                ItemClass = ItemClass.Backpack,
                MaxDurability = 120,
                MinDurabilityAfterRepair = 0,
                Unbreakable = false,
                RepairItemIds = ["plastic", "rusty_plates"],
                Width = 3,
                Height = 3,
                DropChanceOnBroken = 0.2f,
                AddServoArm = true,
                ReloadTurnMod = -2,
                BackpackWeightMult = 0.35f
            };

            Data.Items.AddRecord(_servitorBackpack.Id, _servitorBackpack);
            Data.Descriptors["backpacks"].AddDescriptor(_servitorBackpack.Id, descriptor);
        }

        private static void RegisterWeapons()
        {
            WeaponDescriptor bolterAsset = LoadRequiredAsset<WeaponDescriptor>("bolter");
            WeaponDescriptor bolterDescriptor = CloneWeaponDescriptor("military_assault_1", bolterAsset);
            _bolter = new WeaponRecord
            {
                Id = "iom_bolter",
                ContentDescriptor = bolterDescriptor,
                Categories = [ItemCategory],
                TechLevel = 1,
                Price = 400,
                Weight = 4,
                InventoryWidthSize = 2,
                ItemClass = ItemClass.Weapon,
                MaxDurability = 110,
                MinDurabilityAfterRepair = 0,
                Unbreakable = false,
                RepairItemIds = ["weapon_parts", "spring", "armor_plates"],
                WeaponClass = WeaponClass.AssaultRifle,
                WeaponSubClass = WeaponSubClass.Firearm,
                RequiredAmmo = "Heavy",
                OverrideAmmo = [],
                DefaultAmmoId = "rifle_basic_ammo",
                DefaultGrenadeId = string.Empty,
                Damage = new DmgInfo { damage = string.Empty, minDmg = 19, maxDmg = 35, critChance = 0, critDmg = 1.75f },
                Firemodes = ["rifle_1", "rifle_4"],
                Range = 6,
                ReloadDuration = 4,
                MagazineCapacity = 20,
                AllowedGrenadeIds = []
            };
            AddWeapon(_bolter, bolterDescriptor);

            WeaponDescriptor lasgunAsset = LoadRequiredAsset<WeaponDescriptor>("lasgun");
            WeaponDescriptor lasgunDescriptor = CloneWeaponDescriptor("laser_sniper_1", lasgunAsset);
            _lasgun = new WeaponRecord
            {
                Id = "iom_lasgun",
                ContentDescriptor = lasgunDescriptor,
                Categories = [ItemCategory],
                TechLevel = 1,
                Price = 300,
                Weight = 3,
                InventoryWidthSize = 2,
                ItemClass = ItemClass.Weapon,
                MaxDurability = 100,
                MinDurabilityAfterRepair = 0,
                Unbreakable = false,
                RepairItemIds = ["lens", "bulb", "microelectronics_parts"],
                WeaponClass = WeaponClass.AssaultRifle,
                WeaponSubClass = WeaponSubClass.Firearm,
                RequiredAmmo = "BatteryCells",
                OverrideAmmo = [],
                DefaultAmmoId = "battery_basic_ammo",
                DefaultGrenadeId = string.Empty,
                Damage = new DmgInfo { damage = string.Empty, minDmg = 10, maxDmg = 33, critChance = 0, critDmg = 2f },
                Firemodes = ["pulse_1", "pulse_2"],
                Range = 7,
                ReloadDuration = 2,
                MagazineCapacity = 10,
                AllowedGrenadeIds = []
            };
            AddWeapon(_lasgun, lasgunDescriptor);

            WeaponDescriptor plasmaAsset = LoadRequiredAsset<WeaponDescriptor>("plasma_pistol");
            WeaponDescriptor plasmaDescriptor = CloneWeaponDescriptor("laser_pistol_1", plasmaAsset);
            _plasmaPistol = new WeaponRecord
            {
                Id = "iom_plasma_pistol",
                ContentDescriptor = plasmaDescriptor,
                Categories = [ItemCategory],
                TechLevel = 1,
                Price = 200,
                Weight = 2.1f,
                InventoryWidthSize = 2,
                ItemClass = ItemClass.Weapon,
                MaxDurability = 110,
                MinDurabilityAfterRepair = 0,
                Unbreakable = false,
                RepairItemIds = ["lens", "bulb", "microelectronics_parts"],
                WeaponClass = WeaponClass.Pistol,
                WeaponSubClass = WeaponSubClass.Plasma,
                RequiredAmmo = "BatteryCells",
                OverrideAmmo = [],
                DefaultAmmoId = "battery_basic_ammo",
                DefaultGrenadeId = string.Empty,
                Damage = new DmgInfo { damage = string.Empty, minDmg = 19, maxDmg = 33, critChance = 0, critDmg = 2.25f },
                Firemodes = ["pulse_1", "pulse_2"],
                Range = 3,
                ReloadDuration = 4,
                MagazineCapacity = 7,
                AllowedGrenadeIds = []
            };
            AddWeapon(_plasmaPistol, plasmaDescriptor);

            WeaponDescriptor meltaAsset = LoadRequiredAsset<WeaponDescriptor>("melta");
            WeaponDescriptor meltaDescriptor = CloneWeaponDescriptor("chu_meltathrower_1", meltaAsset);
            _melta = new WeaponRecord
            {
                Id = "iom_melta",
                ContentDescriptor = meltaDescriptor,
                Categories = [ItemCategory],
                TechLevel = 1,
                Price = 400,
                Weight = 2.8f,
                InventoryWidthSize = 2,
                ItemClass = ItemClass.Weapon,
                MaxDurability = 110,
                MinDurabilityAfterRepair = 0,
                Unbreakable = false,
                RepairItemIds = ["coarse_parts", "rod_parts", "rusty_plates"],
                WeaponClass = WeaponClass.Flamethrower,
                WeaponSubClass = WeaponSubClass.Plasma,
                RequiredAmmo = "Gas",
                OverrideAmmo = ["implicted_flamethrower"],
                DefaultAmmoId = "gas_ammo",
                DefaultGrenadeId = string.Empty,
                Damage = new DmgInfo { damage = string.Empty, minDmg = 30, maxDmg = 50, critChance = 0, critDmg = 2.2f },
                Firemodes = ["flamethrower_1"],
                Range = 2,
                ReloadDuration = 4,
                MagazineCapacity = 60,
                AllowedGrenadeIds = []
            };
            AddWeapon(_melta, meltaDescriptor);

            WeaponDescriptor longLasAsset = LoadRequiredAsset<WeaponDescriptor>("lassniper");
            WeaponDescriptor longLasDescriptor = CloneWeaponDescriptor("laser_sniper_1", longLasAsset);
            _longLas = new WeaponRecord
            {
                Id = "iom_lassniper",
                ContentDescriptor = longLasDescriptor,
                Categories = [ItemCategory],
                TechLevel = 1,
                Price = 350,
                Weight = 3.2f,
                InventoryWidthSize = 2,
                ItemClass = ItemClass.Weapon,
                MaxDurability = 110,
                MinDurabilityAfterRepair = 0,
                Unbreakable = false,
                RepairItemIds = ["lens", "bulb", "microelectronics_parts"],
                WeaponClass = WeaponClass.MarksmanRifle,
                WeaponSubClass = WeaponSubClass.Firearm,
                RequiredAmmo = "BatteryCells",
                OverrideAmmo = [],
                DefaultAmmoId = "battery_basic_ammo",
                DefaultGrenadeId = string.Empty,
                Damage = new DmgInfo { damage = string.Empty, minDmg = 32, maxDmg = 58, critChance = 0, critDmg = 2.25f },
                Firemodes = ["beam_1"],
                Range = 9,
                ReloadDuration = 3,
                MagazineCapacity = 10,
                AllowedGrenadeIds = []
            };
            AddWeapon(_longLas, longLasDescriptor);
        }

        private static WeaponDescriptor CloneWeaponDescriptor(string donorId, WeaponDescriptor artwork)
        {
            var donor = Data.Descriptors["rangeweapons"].GetDescriptor(donorId);
            if (donor == null)
            {
                throw new System.InvalidOperationException("Required Quasimorph weapon descriptor donor is unavailable: " + donorId);
            }

            var descriptor = (WeaponDescriptor)Object.Instantiate(donor);
            descriptor._icon = artwork._icon;
            descriptor._smallIcon = artwork._smallIcon;
            descriptor._shadow = artwork._shadow;
            return descriptor;
        }

        private static void AddWeapon(WeaponRecord record, WeaponDescriptor descriptor)
        {
            Data.Items.AddRecord(record.Id, record);
            Data.Descriptors["rangeweapons"].AddDescriptor(record.Id, descriptor);
        }

        private static void RegisterArmor()
        {
            _lightBoots = CreateBoots(
                "iom_lightboots", "lightBoots", 100, 1.6f, 75,
                CreateResists(3f, 4f, lacer: 3f), ArmorClass.LightArmor);

            _lightLeggings = CreateLeggings(
                "iom_lightleggings", "lightLeggings", 190, 2f, 90,
                CreateResists(4f, 4f, lacer: 4f, cold: 2f), ArmorClass.LightArmor);

            _lightChestplate = CreateArmor(
                "iom_lightChestplate", "lightChestplate", 190, 2.9f, 90,
                CreateResists(7f, 8f, lacer: 7f, cold: 2f), ArmorClass.LightArmor);

            _lightHelmet = CreateHelmet(
                "iom_lightHelmet", "lightHelmet", 190, 1.8f, 105,
                CreateResists(4f, 7f, lacer: 4f), ArmorClass.LightArmor);

            _cadiaBoots = CreateBoots(
                "iom_cadiaboots", "cadiaboots", 100, 1.6f, 75,
                CreateResists(6f, 7f, lacer: 7f), ArmorClass.MediumArmor);

            _cadiaLeggings = CreateLeggings(
                "iom_cadialeggings", "cadiapants", 190, 2f, 90,
                CreateResists(8f, 8f, lacer: 8f, cold: 2f), ArmorClass.MediumArmor);

            _cadiaChestplate = CreateArmor(
                "iom_cadiaChestplate", "cadiaarmor", 190, 2.9f, 90,
                CreateResists(12f, 12f, lacer: 10f, cold: 2f), ArmorClass.MediumArmor);

            _cadiaHelmet = CreateHelmet(
                "iom_cadiaHelmet", "cadiahelmet", 190, 1.8f, 105,
                CreateResists(6f, 9f, lacer: 7f), ArmorClass.MediumArmor);
        }

        private static BootsRecord CreateBoots(string id, string assetName, int price, float weight, int durability, System.Collections.Generic.List<DmgResist> resists, ArmorClass armorClass)
        {
            BootsDescriptor artwork = LoadRequiredAsset<BootsDescriptor>(assetName);
            BootsDescriptor descriptor = (BootsDescriptor)Object.Instantiate(Data.Descriptors["boots"].GetDescriptor("military_heavy_boots_1"));
            CopyArtwork(descriptor, artwork);
            var record = new BootsRecord
            {
                Id = id,
                ContentDescriptor = descriptor,
                Categories = [ItemCategory],
                TechLevel = 1,
                Price = price,
                Weight = weight,
                InventoryWidthSize = 1,
                ItemClass = ItemClass.Boots,
                MaxDurability = durability,
                MinDurabilityAfterRepair = 0,
                RepairItemIds = ["rags", "rusty_plates", "armor_plates"],
                ResistSheet = resists,
                ArmorClass = armorClass,
                ArmorSubClass = ArmorSubClass.Default
            };
            Data.Items.AddRecord(id, record);
            Data.Descriptors["boots"].AddDescriptor(id, descriptor);
            return record;
        }

        private static LeggingsRecord CreateLeggings(string id, string assetName, int price, float weight, int durability, System.Collections.Generic.List<DmgResist> resists, ArmorClass armorClass)
        {
            LeggingsDescriptor artwork = LoadRequiredAsset<LeggingsDescriptor>(assetName);
            LeggingsDescriptor descriptor = (LeggingsDescriptor)Object.Instantiate(Data.Descriptors["leggings"].GetDescriptor("military_heavy_pants_1"));
            CopyArtwork(descriptor, artwork);
            var record = new LeggingsRecord
            {
                Id = id,
                ContentDescriptor = descriptor,
                Categories = [ItemCategory],
                TechLevel = 1,
                Price = price,
                Weight = weight,
                InventoryWidthSize = 1,
                ItemClass = ItemClass.Leggings,
                MaxDurability = durability,
                MinDurabilityAfterRepair = 0,
                RepairItemIds = ["rags", "rusty_plates", "armor_plates"],
                ResistSheet = resists,
                ArmorClass = armorClass,
                ArmorSubClass = ArmorSubClass.Default
            };
            Data.Items.AddRecord(id, record);
            Data.Descriptors["leggings"].AddDescriptor(id, descriptor);
            return record;
        }

        private static ArmorRecord CreateArmor(string id, string assetName, int price, float weight, int durability, System.Collections.Generic.List<DmgResist> resists, ArmorClass armorClass)
        {
            ArmorDescriptor artwork = LoadRequiredAsset<ArmorDescriptor>(assetName);
            ArmorDescriptor descriptor = (ArmorDescriptor)Object.Instantiate(Data.Descriptors["armors"].GetDescriptor("military_heavy_armor_1"));
            CopyArtwork(descriptor, artwork);
            var record = new ArmorRecord
            {
                Id = id,
                ContentDescriptor = descriptor,
                Categories = [ItemCategory],
                TechLevel = 1,
                Price = price,
                Weight = weight,
                InventoryWidthSize = 1,
                ItemClass = ItemClass.Armor,
                MaxDurability = durability,
                MinDurabilityAfterRepair = 0,
                RepairItemIds = ["rags", "rusty_plates", "armor_plates"],
                ResistSheet = resists,
                ArmorClass = armorClass,
                ArmorSubClass = ArmorSubClass.Default
            };
            Data.Items.AddRecord(id, record);
            Data.Descriptors["armors"].AddDescriptor(id, descriptor);
            return record;
        }

        private static HelmetRecord CreateHelmet(string id, string assetName, int price, float weight, int durability, System.Collections.Generic.List<DmgResist> resists, ArmorClass armorClass)
        {
            HelmetDescriptor artwork = LoadRequiredAsset<HelmetDescriptor>(assetName);
            HelmetDescriptor descriptor = (HelmetDescriptor)Object.Instantiate(Data.Descriptors["helmets"].GetDescriptor("military_heavy_helmet_1"));
            CopyArtwork(descriptor, artwork);
            var record = new HelmetRecord
            {
                Id = id,
                ContentDescriptor = descriptor,
                Categories = [ItemCategory],
                TechLevel = 1,
                Price = price,
                Weight = weight,
                InventoryWidthSize = 1,
                ItemClass = ItemClass.Armor,
                MaxDurability = durability,
                MinDurabilityAfterRepair = 0,
                RepairItemIds = ["rags", "rusty_plates", "armor_plates"],
                ResistSheet = resists,
                ArmorClass = armorClass,
                ArmorSubClass = ArmorSubClass.Default,
                HideHair = true
            };
            Data.Items.AddRecord(id, record);
            Data.Descriptors["helmets"].AddDescriptor(id, descriptor);
            return record;
        }

        private static void CopyArtwork(BootsDescriptor target, BootsDescriptor source)
        {
            target._icon = source._icon;
            target._smallIcon = source._smallIcon;
            target._shadow = source._shadow;
        }

        private static void CopyArtwork(LeggingsDescriptor target, LeggingsDescriptor source)
        {
            target._icon = source._icon;
            target._smallIcon = source._smallIcon;
            target._shadow = source._shadow;
        }

        private static void CopyArtwork(ArmorDescriptor target, ArmorDescriptor source)
        {
            target._icon = source._icon;
            target._smallIcon = source._smallIcon;
            target._shadow = source._shadow;
        }

        private static void CopyArtwork(HelmetDescriptor target, HelmetDescriptor source)
        {
            target._icon = source._icon;
            target._smallIcon = source._smallIcon;
            target._shadow = source._shadow;
        }
    }
}

using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using MGSC;
using UnityEngine;

namespace ImperiumOfMan
{
    public static class Plugin
    {
        public static Logger Logger = new Logger();

        public const string ItemCategory = "iom_faction"; // should be same as Faction ID otherwise randomiser will do brrrrrrrrrrr errr
        
        public static AssetBundle ModBundle;

        [Hook(ModHookType.BeforeBootstrap)]
        public static void BeforeConfig(IModContext context)
        {
            //Harmony.DEBUG = true;
            new Harmony("BadRyuner_ImperiumOfMan").PatchAll(typeof(Plugin).Assembly);
            //h.Patch(AccessTools.Method(typeof(Localization), "LoadDB"), postfix: AccessTools.Method(typeof(TranslationPatch), "LoadDBPostfix"));
            Debug.Log($"[{nameof(ImperiumOfMan)}] BeforeBootstrap.");
        }
        
        [Hook(ModHookType.AfterConfigsLoaded)]
        public static void AfterConfig(IModContext context)
        {
            
        }

        [Hook(ModHookType.AfterBootstrap)]
        public static void AfterBootstrap(IModContext context)
        {
            ModBundle = AssetBundle.LoadFromFile(Path.Combine(context.ModContentPath, "imperiumofman.bundle"));

            #region Faction Data
            var imperatorPortrait = ModBundle.LoadAsset<PortraitDescriptor>("Imperator");
            Data.Portraits.AddRecord("faction_iom_faction", new()
            {
                Id = "faction_iom_faction",
                ContentDescriptor = imperatorPortrait
            });
            Data.Descriptors["portraits"].AddDescriptor("faction_iom_faction", imperatorPortrait);
            
            var faction = ModBundle.LoadAsset<FactionDescriptor>("ImperiumOfManFaction");
            Data.Factions.AddRecord("iom_faction", new()
            {
                Id = "iom_faction",
                ContentDescriptor = faction,
                Enabled = true,
                InitialPower = 1900,
                InitialTechLevel = 1,
                InitialPlayerReputation = 0,
                FactionType = FactionType.Corp,
                AllianceType = "iom_alliance",
                SpawnMissionChance = 0.1f,
                Strategies = [ new(1f, FactionStrategy.Expansion) ],
                StrategyDurationMinHours = 672,
                StrategyDurationMaxHours = 1344,
                GuardCreatureId = "elite_sbn",
                AgentCreatureId = "civilian",
                MinQmorphosWhenVictims = 0,
                UseGeneralRewards = true,
                PortraitsByStrategy = false,
                ItemDropCategories = [ ItemCategory ]
            });
            Data.Descriptors["factions"].AddDescriptor("iom_faction", faction);
            
            var alliance = ModBundle.LoadAsset<AllianceDescriptor>("HumanAlliance");
            Data.Alliances.AddRecord("iom_alliance", new()
            {
                Id = "iom_alliance",
                ContentDescriptor = alliance,
                AllowStrife = false,
                TradeFactionLists = ["iom_faction", "ChurchRevelation", "SBN"],
            });
            Data.Descriptors["alliances"].AddDescriptor("iom_alliance", alliance);
            #endregion
            
            UnitDropRecord[] dropRecord =
            [
                new()
                {
                    TechLevel = 1,
                    UnitSize = new(2, 2),
                    LeaderSpawn = [],
                    MobClasses = [new(1f, "soldier")],
                    AllowedFactions = ["iom_faction"],
                    Weight = 12,
                    Points = 80
                },
                new()
                {
                    TechLevel = 1,
                    UnitSize = new(1, 1),
                    LeaderSpawn = [],
                    MobClasses = [new(1f, "soldier")],
                    AllowedFactions = ["iom_faction"],
                    Weight = 6,
                    Points = 20
                },
                new()
                {
                    TechLevel = 2,
                    UnitSize = new(2, 2),
                    LeaderSpawn = [],
                    MobClasses = [new(1f, "soldier")],
                    AllowedFactions = ["iom_faction"],
                    Weight = 12,
                    Points = 90
                },
                new()
                {
                    TechLevel = 3,
                    UnitSize = new(1, 1),
                    LeaderSpawn = [],
                    MobClasses = [new(1f, "soldier")],
                    AllowedFactions = ["iom_faction"],
                    Weight = 6,
                    Points = 30
                },
                new()
                {
                    TechLevel = 4,
                    UnitSize = new(1, 1),
                    LeaderSpawn = [],
                    MobClasses = [new(1f, "soldier")],
                    AllowedFactions = ["iom_faction"],
                    Weight = 6,
                    Points = 25
                },
            ];
            foreach (var key in Data.LocationUnitDrop._recordsByLocations.Keys)
            {
                Data.LocationUnitDrop._recordsByLocations[key].AddRange(dropRecord);
            }
            
            var earthStationDummy = Data.Stations.GetRecord("Paragon");
            
            Data.Stations.AddRecord("iomTerra", new StationRecord
            {
                Id = "iomTerra",
                ContentDescriptor = earthStationDummy.ContentDescriptor,
                SpaceObjectId = "earth",
                InitialOwnerFactionId = "iom_faction",
                Power = 0,
                PowerGain = 2,
                TechLevelGain = 3,
                UncapturableByDefault = true,
                CaptureChance = 0.2f,
                MissionTemplateId = "orbit1",
                MissionNameTemplateId = "mercurySurfaceLevels",
                BramfaturaId = "Earth",
                StationType = "SpaceStation",
                SpaceObjectItemDropPercent = 0.1f,
                InitialPopulation = 22314,
                MaxPopulation = 60840,
            });
            
            Data.StationBarter.AddRecord("iomTerra", new StationBarterRecord
            {
                Id = "iomTerra",
                ContentDescriptor = null,
                CorpProduceItems = [],
                CorpAdditionalConsumeItems = [],
                CivResProduceItems = [],
                CivResAdditionalConsumeItems = [],
                QuasiProduceItems = [],
                QuasiAdditionalConsumeItems = [],
                PiratesProduceItems = [],
                PiratesAdditionalConsumeItems = [],
            });
            
            Data.Stations.AddRecord("iomMoom", new StationRecord
            {
                Id = "iomMoom",
                ContentDescriptor = earthStationDummy.ContentDescriptor,
                SpaceObjectId = "fuller",
                InitialOwnerFactionId = "iom_faction",
                Power = 0,
                PowerGain = 1,
                TechLevelGain = 1,
                UncapturableByDefault = false,
                CaptureChance = 0.2f,
                MissionTemplateId = "orbit1",
                MissionNameTemplateId = "mercurySurfaceLevels",
                BramfaturaId = "Moon",
                StationType = "SpaceStation",
                SpaceObjectItemDropPercent = 0.1f,
                InitialPopulation = 7201,
                MaxPopulation = 33034,
            });
            
            Data.StationBarter.AddRecord("iomMoom", new StationBarterRecord
            {
                Id = "iomMoom",
                ContentDescriptor = null,
                CorpProduceItems = [],
                CorpAdditionalConsumeItems = [],
                CivResProduceItems = [],
                CivResAdditionalConsumeItems = [],
                QuasiProduceItems = [],
                QuasiAdditionalConsumeItems = [],
                PiratesProduceItems = [],
                PiratesAdditionalConsumeItems = [],
            });
            
            Data.Stations.AddRecord("iomPhobos", new StationRecord
            {
                Id = "iomPhobos",
                ContentDescriptor = earthStationDummy.ContentDescriptor,
                SpaceObjectId = "phobos",
                InitialOwnerFactionId = "iom_faction",
                Power = 0,
                PowerGain = 1,
                TechLevelGain = 2,
                UncapturableByDefault = false,
                CaptureChance = 0.2f,
                MissionTemplateId = "moon4",
                MissionNameTemplateId = "mercurySurfaceLevels",
                BramfaturaId = "Moon",
                StationType = "SpaceStation",
                SpaceObjectItemDropPercent = 0.1f,
                InitialPopulation = 8451,
                MaxPopulation = 39932,
            });
            
            Data.StationBarter.AddRecord("iomPhobos", new StationBarterRecord
            {
                Id = "iomPhobos",
                ContentDescriptor = null,
                CorpProduceItems = [],
                CorpAdditionalConsumeItems = [],
                CivResProduceItems = [],
                CivResAdditionalConsumeItems = [],
                QuasiProduceItems = [],
                QuasiAdditionalConsumeItems = [],
                PiratesProduceItems = [],
                PiratesAdditionalConsumeItems = [],
            });
            
            Data.Stations.AddRecord("iomHavoc", new StationRecord
            {
                Id = "iomHavoc",
                ContentDescriptor = earthStationDummy.ContentDescriptor,
                SpaceObjectId = "havoc",
                InitialOwnerFactionId = "iom_faction",
                Power = 0,
                PowerGain = 2,
                TechLevelGain = 1,
                UncapturableByDefault = false,
                CaptureChance = 0.2f,
                MissionTemplateId = "moon1",
                MissionNameTemplateId = "mercurySurfaceLevels",
                BramfaturaId = "Venus",
                StationType = "SpaceStation",
                SpaceObjectItemDropPercent = 0.1f,
                InitialPopulation = 14451,
                MaxPopulation = 42112,
            });
            
            Data.StationBarter.AddRecord("iomHavoc", new StationBarterRecord
            {
                Id = "iomHavoc",
                ContentDescriptor = null,
                CorpProduceItems = [],
                CorpAdditionalConsumeItems = [],
                CivResProduceItems = [],
                CivResAdditionalConsumeItems = [],
                QuasiProduceItems = [],
                QuasiAdditionalConsumeItems = [],
                PiratesProduceItems = [],
                PiratesAdditionalConsumeItems = [],
            });
            
            var servitorBackpack = ModBundle.LoadAsset<BackpackDescriptor>("Servitor");
            var servitorBackpackItem = new BackpackRecord
            {
                Id = "iom_sevitor_backpack",
                ContentDescriptor = servitorBackpack,
                Categories = [ItemCategory],
                TechLevel = 1,
                Price = 600,
                Weight = 0.01f,
                InventoryWidthSize = 1,
                ItemClass = ItemClass.Backpack,
                MaxDurability = 120,
                MinDurabilityAfterRepair = 0,
                Unbreakable = false,
                RepairCategory = "engineering_weapon",
                Width = 3,
                Height = 3,
                DropChanceOnBroken = 0.2f,
                AddServoArm = true,
                TotalItemsWeightMult = 0.4f,
                ReloadTurnBonus = 2
            };
            Data.Items.AddRecord("iom_sevitor_backpack", servitorBackpackItem);
            Data.Descriptors["backpacks"].AddDescriptor("iom_sevitor_backpack", servitorBackpack);

            #region Weapons
            var bolterDescRef = ModBundle.LoadAsset<WeaponDescriptor>("bolter");
            var bolterDesc = (WeaponDescriptor)Object.Instantiate(Data.Descriptors["rangeweapons"].GetDescriptor("military_assault_1"));
            //  bolterDesc._overrideBullet = dummyRifle._overrideBullet;
            //  bolterDesc._reloadSoundBanks = dummyRifle._reloadSoundBanks;
            // bolterDesc._failedAttackSoundBanks = dummyRifle._failedAttackSoundBanks;
            // bolterDesc._dryShotSoundBanks = dummyRifle._dryShotSoundBanks;
            bolterDesc._icon = bolterDescRef._icon;
            bolterDesc._smallIcon = bolterDescRef._smallIcon;
            bolterDesc._shadow = bolterDescRef._shadow;
            var bolterItem = new WeaponRecord
            {
                Id = "iom_bolter",
                ContentDescriptor = bolterDesc,
                Categories = [ ItemCategory ],
                TechLevel = 1,
                Price = 400,
                Weight = 4,
                InventoryWidthSize = 2,
                ItemClass = ItemClass.Weapon,
                MaxDurability = 110,
                MinDurabilityAfterRepair = 0,
                Unbreakable = false,
                RepairCategory = "firearms_weapon",
                WeaponClass = WeaponClass.AssaultRifle,
                WeaponSubClass = WeaponSubClass.Firearm,
                RequiredAmmo = "Heavy",
                OverrideAmmo = string.Empty,
                DefaultAmmoId = "rifle_basic_ammo",
                DefaultGrenadeId = string.Empty,
                Damage = new()
                {
                    damage = string.Empty,
                    minDmg = 19,
                    maxDmg = 35,
                    critChance = 0,
                    critDmg = 1.75f
                },
                Firemodes = ["basic_rifle_single", "basic_rifle_shortauto"],
                Range = 6,
                ReloadDuration = 4,
                ReloadOneClip = false,
                MagazineCapacity = 20,
                AllowedGrenadeIds = [],
                WoundChanceOnPierce = 0,
                ArmorPenetration = 0.25f,
                FovLookAngleMult = 1,
            };
            Data.Items.AddRecord("iom_bolter", bolterItem);
            Data.Descriptors["rangeweapons"].AddDescriptor("iom_bolter", bolterDesc);

            var lasgunDescRef = ModBundle.LoadAsset<WeaponDescriptor>("lasgun");
            var lasgunDesc = (WeaponDescriptor)Object.Instantiate(Data.Descriptors["rangeweapons"].GetDescriptor("laser_sniper_1"));
            //lasgunDesc._overrideBullet = dummyRifle._overrideBullet;
            //lasgunDesc._reloadSoundBanks = dummyRifle._reloadSoundBanks;
            //lasgunDesc._failedAttackSoundBanks = dummyRifle._failedAttackSoundBanks;
            //lasgunDesc._dryShotSoundBanks = dummyRifle._dryShotSoundBanks;
            //lasgunDesc._attackSoundBanks = dummyRifle._attackSoundBanks;
            lasgunDesc._icon = lasgunDescRef._icon;
            lasgunDesc._smallIcon = lasgunDescRef._smallIcon;
            lasgunDesc._shadow = lasgunDescRef._shadow;
            var lasgunItem = new WeaponRecord
            {
                Id = "iom_lasgun",
                ContentDescriptor = lasgunDesc,
                Categories = [ ItemCategory ],
                TechLevel = 1,
                Price = 300,
                Weight = 3,
                InventoryWidthSize = 2,
                ItemClass = ItemClass.Weapon,
                MaxDurability = 100,
                MinDurabilityAfterRepair = 0,
                Unbreakable = false,
                RepairCategory = "laser_weapon",
                WeaponClass = WeaponClass.AssaultRifle,
                WeaponSubClass = WeaponSubClass.Firearm,
                RequiredAmmo = "BatteryCells",
                OverrideAmmo = string.Empty,
                DefaultAmmoId = "battery_basic_ammo",
                DefaultGrenadeId = string.Empty,
                Damage = new()
                {
                    damage = string.Empty,
                    minDmg = 10,
                    maxDmg = 33,
                    critChance = 0,
                    critDmg = 2f
                },
                Firemodes = ["basic_energy_rifle_single", "basic_energy_rifle_shortauto"],
                Range = 7,
                ReloadDuration = 2,
                ReloadOneClip = false,
                MagazineCapacity = 10,
                AllowedGrenadeIds = [],
                WoundChanceOnPierce = 0,
                ArmorPenetration = 0.05f,
                FovLookAngleMult = 0.9f,
            };
            Data.Items.AddRecord("iom_lasgun", lasgunItem);
            Data.Descriptors["rangeweapons"].AddDescriptor("iom_lasgun", lasgunDesc);
            
            var plasmaPistolDescRef = ModBundle.LoadAsset<WeaponDescriptor>("plasma_pistol");
            var plasmaPistolDesc = (WeaponDescriptor)Object.Instantiate(Data.Descriptors["rangeweapons"].GetDescriptor("laser_pistol_1"));
            plasmaPistolDesc._icon = plasmaPistolDescRef._icon;
            plasmaPistolDesc._smallIcon = plasmaPistolDescRef._smallIcon;
            plasmaPistolDesc._shadow = plasmaPistolDescRef._shadow;
            var plasmaPistolItem = new WeaponRecord
            {
                Id = "iom_plasma_pistol",
                ContentDescriptor = plasmaPistolDesc,
                Categories = [ ItemCategory ],
                TechLevel = 1,
                Price = 200,
                Weight = 2.1f,
                InventoryWidthSize = 2,
                ItemClass = ItemClass.Weapon,
                MaxDurability = 110,
                MinDurabilityAfterRepair = 0,
                Unbreakable = false,
                RepairCategory = "laser_weapon",
                WeaponClass = WeaponClass.Pistol,
                WeaponSubClass = WeaponSubClass.Plasma,
                RequiredAmmo = "BatteryCells",
                OverrideAmmo = string.Empty,
                DefaultAmmoId = "battery_basic_ammo",
                DefaultGrenadeId = string.Empty,
                Damage = new()
                {
                    damage = string.Empty,
                    minDmg = 19,
                    maxDmg = 33,
                    critChance = 0,
                    critDmg = 2.25f
                },
                Firemodes = ["basic_energy_pistol_single", "basic_energy_pistol_burst"],
                Range = 3,
                ReloadDuration = 4,
                ReloadOneClip = false,
                MagazineCapacity = 7,
                AllowedGrenadeIds = [],
                WoundChanceOnPierce = 0,
                ArmorPenetration = 0.00f,
                FovLookAngleMult = 1f,
            };
            Data.Items.AddRecord("iom_plasma_pistol", plasmaPistolItem);
            Data.Descriptors["rangeweapons"].AddDescriptor("iom_plasma_pistol", plasmaPistolDesc);
            
            var meltaDescRef = ModBundle.LoadAsset<WeaponDescriptor>("melta");
            var meltaDesc = (WeaponDescriptor)Object.Instantiate(Data.Descriptors["rangeweapons"].GetDescriptor("chu_meltatrower_1"));
            meltaDesc._icon = meltaDescRef._icon;
            meltaDesc._smallIcon = meltaDescRef._smallIcon;
            meltaDesc._shadow = meltaDescRef._shadow;
            var meltaItem = new WeaponRecord
            {
                Id = "iom_melta",
                ContentDescriptor = meltaDesc,
                Categories = [ ItemCategory ],
                TechLevel = 1,
                Price = 400,
                Weight = 2.8f,
                InventoryWidthSize = 2,
                ItemClass = ItemClass.Weapon,
                MaxDurability = 110,
                MinDurabilityAfterRepair = 0,
                Unbreakable = false,
                RepairCategory = "engineering_weapon",
                WeaponClass = WeaponClass.Flamethrower,
                WeaponSubClass = WeaponSubClass.Plasma,
                RequiredAmmo = "Gas",
                OverrideAmmo = "implicted_flamethrower",
                DefaultAmmoId = "gas_ammo",
                DefaultGrenadeId = string.Empty,
                Damage = new()
                {
                    damage = string.Empty,
                    minDmg = 30,
                    maxDmg = 50,
                    critChance = 0,
                    critDmg = 2.2f
                },
                Firemodes = ["plasma_energy_burst", "plasma_energy_shortauto"],
                Range = 2,
                ReloadDuration = 4,
                ReloadOneClip = false,
                MagazineCapacity = 60,
                AllowedGrenadeIds = [],
                WoundChanceOnPierce = 0,
                ArmorPenetration = 0.3f,
                FovLookAngleMult = 0.7f,
            };
            Data.Items.AddRecord("iom_melta", meltaItem);
            Data.Descriptors["rangeweapons"].AddDescriptor("iom_melta", meltaDesc);

            var lassniperDescRef = ModBundle.LoadAsset<WeaponDescriptor>("lassniper");
            var lassniperDesc = (WeaponDescriptor)Object.Instantiate(Data.Descriptors["rangeweapons"].GetDescriptor("laser_sniper_1"));
            lassniperDesc._icon = lassniperDescRef._icon;
            lassniperDesc._smallIcon = lassniperDescRef._smallIcon;
            lassniperDesc._shadow = lassniperDescRef._shadow;
            var lassniperItem = new WeaponRecord
            {
                Id = "iom_lassniper",
                ContentDescriptor = lassniperDesc,
                Categories = [ ItemCategory ],
                TechLevel = 1,
                Price = 350,
                Weight = 3.2f,
                InventoryWidthSize = 2,
                ItemClass = ItemClass.Weapon,
                MaxDurability = 110,
                MinDurabilityAfterRepair = 0,
                Unbreakable = false,
                RepairCategory = "laser_weapon",
                WeaponClass = WeaponClass.MarksmanRifle,
                WeaponSubClass = WeaponSubClass.Firearm,
                RequiredAmmo = "BatteryCells",
                OverrideAmmo = string.Empty,
                DefaultAmmoId = "battery_basic_ammo",
                DefaultGrenadeId = string.Empty,
                Damage = new()
                {
                    damage = string.Empty,
                    minDmg = 32,
                    maxDmg = 58,
                    critChance = 0,
                    critDmg = 2.25f
                },
                Firemodes = ["basic_energy_rifle_single"],
                Range = 9,
                ReloadDuration = 3,
                ReloadOneClip = false,
                MagazineCapacity = 10,
                AllowedGrenadeIds = [],
                WoundChanceOnPierce = 0,
                ArmorPenetration = 0.2f,
                FovLookAngleMult = 0.8f,
            };
            Data.Items.AddRecord("iom_lassniper", lassniperItem);
            Data.Descriptors["rangeweapons"].AddDescriptor("iom_lassniper", lassniperDesc);
            #endregion
            
            #region Armor
            #region Light Armor
            var lightBootsDescRef = ModBundle.LoadAsset<BootsDescriptor>("lightBoots");;
            var lightBootsDesc = (BootsDescriptor)Object.Instantiate(Data.Descriptors["boots"].GetDescriptor("military_heavy_boots_1"));
            lightBootsDesc._icon = lightBootsDescRef._icon;
            lightBootsDesc._smallIcon = lightBootsDescRef._smallIcon;
            lightBootsDesc._shadow = lightBootsDescRef._shadow;
            var lightBoots = new BootsRecord
            {
                Id = "iom_lightboots",
                ContentDescriptor = lightBootsDesc,
                Categories = [ItemCategory],
                TechLevel = 1,
                Price = 100,
                Weight = 1.6f,
                InventoryWidthSize = 1,
                ItemClass = ItemClass.Boots,
                MaxDurability = 75,
                MinDurabilityAfterRepair = 0,
                RepairCategory = "aram_armor",
                ResistSheet = CreateResists(3f, 4f, lacer: 3f),
                ArmorClass = ArmorClass.LightArmor,
                ArmorSubClass = ArmorSubClass.Default,
            };
            Data.Items.AddRecord("iom_lightboots", lightBoots);
            Data.Descriptors["boots"].AddDescriptor("iom_lightboots", lightBootsDesc);
            
            var lightLegginsDescRef = ModBundle.LoadAsset<LeggingsDescriptor>("lightLeggings");
            var lightLeggingsDesc = (LeggingsDescriptor)Object.Instantiate(Data.Descriptors["leggings"].GetDescriptor("military_heavy_pants_1"));
            lightLeggingsDesc._icon = lightLegginsDescRef._icon;
            lightLeggingsDesc._smallIcon = lightLegginsDescRef._smallIcon;
            lightLeggingsDesc._shadow = lightLegginsDescRef._shadow;
            var lightLeggings = new LeggingsRecord
            {
                Id = "iom_lightleggings",
                ContentDescriptor = lightLeggingsDesc,
                Categories = [ItemCategory],
                TechLevel = 1,
                Price = 190,
                Weight = 2f,
                InventoryWidthSize = 1,
                ItemClass = ItemClass.Leggings,
                MaxDurability = 90,
                MinDurabilityAfterRepair = 0,
                RepairCategory = "aram_armor",
                ResistSheet = CreateResists(4f, 4f, lacer: 4f, cold: 2f),
                ArmorClass = ArmorClass.LightArmor,
                ArmorSubClass = ArmorSubClass.Default,
            };
            Data.Items.AddRecord("iom_lightleggings", lightLeggings);
            Data.Descriptors["leggings"].AddDescriptor("iom_lightleggings", lightLeggingsDesc);

            var lightChestplateDescRef = ModBundle.LoadAsset<ArmorDescriptor>("lightChestplate");;
            var lightChestplateDesc = (ArmorDescriptor)Object.Instantiate(Data.Descriptors["armors"].GetDescriptor("military_heavy_armor_1"));
            lightChestplateDesc._icon = lightChestplateDescRef._icon;
            lightChestplateDesc._smallIcon = lightChestplateDescRef._smallIcon;
            lightChestplateDesc._shadow = lightChestplateDescRef._shadow;
            var lightChestplate = new ArmorRecord()
            {
                Id = "iom_lightChestplate",
                ContentDescriptor = lightChestplateDesc,
                Categories = [ItemCategory],
                TechLevel = 1,
                Price = 190,
                Weight = 2.9f,
                InventoryWidthSize = 1,
                ItemClass = ItemClass.Armor,
                MaxDurability = 90,
                MinDurabilityAfterRepair = 0,
                RepairCategory = "aram_armor",
                ResistSheet = CreateResists(7f, 8f, lacer: 7f, cold: 2f),
                ArmorClass = ArmorClass.LightArmor,
                ArmorSubClass = ArmorSubClass.Default,
            };
            Data.Items.AddRecord("iom_lightChestplate", lightChestplate);
            Data.Descriptors["armors"].AddDescriptor("iom_lightChestplate", lightChestplateDesc); 
            
            var lightHelmetDescRef = ModBundle.LoadAsset<HelmetDescriptor>("lightHelmet");;
            var lightHelmetDesc = (HelmetDescriptor)Object.Instantiate(Data.Descriptors["helmets"].GetDescriptor("military_heavy_helmet_1"));
            lightHelmetDesc._icon = lightHelmetDescRef._icon;
            lightHelmetDesc._smallIcon = lightHelmetDescRef._smallIcon;
            lightHelmetDesc._shadow = lightHelmetDescRef._shadow;
            var lightHelmet = new HelmetRecord
            {
                Id = "iom_lightHelmet",
                ContentDescriptor = lightHelmetDesc,
                Categories = [ItemCategory],
                TechLevel = 1,
                Price = 190,
                Weight = 1.8f,
                InventoryWidthSize = 1,
                ItemClass = ItemClass.Armor,
                MaxDurability = 105,
                MinDurabilityAfterRepair = 0,
                RepairCategory = "aram_armor",
                ResistSheet = CreateResists(4f, 7f, lacer: 4f),
                ArmorClass = ArmorClass.LightArmor,
                ArmorSubClass = ArmorSubClass.Default,
                HideHair = true,
            };
            Data.Items.AddRecord("iom_lightHelmet", lightHelmet);
            Data.Descriptors["helmets"].AddDescriptor("iom_lightHelmet", lightHelmetDesc);
            #endregion
            #region Cadian Armor
            var cadiaBootsDescRef = ModBundle.LoadAsset<BootsDescriptor>("cadiaboots");;
            var cadiaBootsDesc = (BootsDescriptor)Object.Instantiate(Data.Descriptors["boots"].GetDescriptor("military_heavy_boots_1"));
            cadiaBootsDesc._icon = cadiaBootsDescRef._icon;
            cadiaBootsDesc._smallIcon = cadiaBootsDescRef._smallIcon;
            cadiaBootsDesc._shadow = cadiaBootsDescRef._shadow;
            var cadiaBoots = new BootsRecord
            {
                Id = "iom_cadiaboots",
                ContentDescriptor = cadiaBootsDesc,
                Categories = [ItemCategory],
                TechLevel = 1,
                Price = 100,
                Weight = 1.6f,
                InventoryWidthSize = 1,
                ItemClass = ItemClass.Boots,
                MaxDurability = 75,
                MinDurabilityAfterRepair = 0,
                RepairCategory = "aram_armor",
                ResistSheet = CreateResists(6f, 7f, lacer: 7f),
                ArmorClass = ArmorClass.MediumArmor,
                ArmorSubClass = ArmorSubClass.Default,
            };
            Data.Items.AddRecord("iom_cadiaboots", cadiaBoots);
            Data.Descriptors["boots"].AddDescriptor("iom_cadiaboots", cadiaBootsDesc);
            
            var cadiaLegginsDescRef = ModBundle.LoadAsset<LeggingsDescriptor>("cadiapants");
            var cadiaLeggingsDesc = (LeggingsDescriptor)Object.Instantiate(Data.Descriptors["leggings"].GetDescriptor("military_heavy_pants_1"));
            cadiaLeggingsDesc._icon = cadiaLegginsDescRef._icon;
            cadiaLeggingsDesc._smallIcon = cadiaLegginsDescRef._smallIcon;
            cadiaLeggingsDesc._shadow = cadiaLegginsDescRef._shadow;
            var cadiaLeggings = new LeggingsRecord
            {
                Id = "iom_cadialeggings",
                ContentDescriptor = cadiaLeggingsDesc,
                Categories = [ItemCategory],
                TechLevel = 1,
                Price = 190,
                Weight = 2f,
                InventoryWidthSize = 1,
                ItemClass = ItemClass.Leggings,
                MaxDurability = 90,
                MinDurabilityAfterRepair = 0,
                RepairCategory = "aram_armor",
                ResistSheet = CreateResists(8f, 8f, lacer: 8f, cold: 2f),
                ArmorClass = ArmorClass.MediumArmor,
                ArmorSubClass = ArmorSubClass.Default,
            };
            Data.Items.AddRecord("iom_cadialeggings", cadiaLeggings);
            Data.Descriptors["leggings"].AddDescriptor("iom_cadialeggings", cadiaLeggingsDesc);

            var cadiaChestplateDescRef = ModBundle.LoadAsset<ArmorDescriptor>("cadiaarmor");;
            var cadiaChestplateDesc = (ArmorDescriptor)Object.Instantiate(Data.Descriptors["armors"].GetDescriptor("military_heavy_armor_1"));
            cadiaChestplateDesc._icon = cadiaChestplateDescRef._icon;
            cadiaChestplateDesc._smallIcon = cadiaChestplateDescRef._smallIcon;
            cadiaChestplateDesc._shadow = cadiaChestplateDescRef._shadow;
            var cadiaChestplate = new ArmorRecord()
            {
                Id = "iom_cadiaChestplate",
                ContentDescriptor = cadiaChestplateDesc,
                Categories = [ItemCategory],
                TechLevel = 1,
                Price = 190,
                Weight = 2.9f,
                InventoryWidthSize = 1,
                ItemClass = ItemClass.Armor,
                MaxDurability = 90,
                MinDurabilityAfterRepair = 0,
                RepairCategory = "aram_armor",
                ResistSheet = CreateResists(12f, 12f, lacer: 10f, cold: 2f),
                ArmorClass = ArmorClass.MediumArmor,
                ArmorSubClass = ArmorSubClass.Default,
            };
            Data.Items.AddRecord("iom_cadiaChestplate", cadiaChestplate);
            Data.Descriptors["armors"].AddDescriptor("iom_cadiaChestplate", cadiaChestplateDesc); 
            
            var cadiaHelmetDescRef = ModBundle.LoadAsset<HelmetDescriptor>("cadiahelmet");;
            var cadiaHelmetDesc = (HelmetDescriptor)Object.Instantiate(Data.Descriptors["helmets"].GetDescriptor("military_heavy_helmet_1"));
            cadiaHelmetDesc._icon = cadiaHelmetDescRef._icon;
            cadiaHelmetDesc._smallIcon = cadiaHelmetDescRef._smallIcon;
            cadiaHelmetDesc._shadow = cadiaHelmetDescRef._shadow;
            var cadiaHelmet = new HelmetRecord
            {
                Id = "iom_cadiaHelmet",
                ContentDescriptor = cadiaHelmetDesc,
                Categories = [ItemCategory],
                TechLevel = 1,
                Price = 190,
                Weight = 1.8f,
                InventoryWidthSize = 1,
                ItemClass = ItemClass.Armor,
                MaxDurability = 105,
                MinDurabilityAfterRepair = 0,
                RepairCategory = "aram_armor",
                ResistSheet = CreateResists(6f, 9f, lacer: 7f),
                ArmorClass = ArmorClass.MediumArmor,
                ArmorSubClass = ArmorSubClass.Default,
                HideHair = true,
            };
            Data.Items.AddRecord("iom_cadiaHelmet", cadiaHelmet);
            Data.Descriptors["helmets"].AddDescriptor("iom_cadiaHelmet", cadiaHelmetDesc);
            #endregion
            #endregion

            #region Add Items From Other Factions To Drop Table
            Data.Items.GetSimpleRecord<ItemRecord>("trash_sawblade_1").Categories.Add(ItemCategory);
            #endregion
            
            //Data.Items
            //     .GetSimpleRecord<DatadiskRecord>("itemChip")
            //    .UnlockIds
            //    .AddRange([lightHelmet.Id, lightChestplate.Id, lightLeggings.Id, lightBoots.Id, bolterItem.Id, lasgunItem.Id, lassniperItem.Id]); // req crafting recipe
            
            var dropDict = new Dictionary<int, List<ContentDropRecord>>();
            dropDict.Add(0, []);
            dropDict.Add(1, [lightBoots.ToDropRecord(3f, 200), lightLeggings.ToDropRecord(3f, 200), lightChestplate.ToDropRecord(3f, 200), lightHelmet.ToDropRecord(3f, 200)]);
            dropDict.Add(2, [lasgunItem.ToDropRecord(10, 200), cadiaBoots.ToDropRecord(3f, 200), cadiaLeggings.ToDropRecord(3f, 200), cadiaChestplate.ToDropRecord(3f, 200), cadiaHelmet.ToDropRecord(3f, 200)]);
            dropDict.Add(3, [servitorBackpackItem.ToDropRecord(10, 250), lassniperItem.ToDropRecord(5f, 400), plasmaPistolItem.ToDropRecord(3f, 250)]);
            dropDict.Add(4, [bolterItem.ToDropRecord(10, 300)]);
            dropDict.Add(5, [meltaItem.ToDropRecord(7, 310)]);
            dropDict.Add(6, []);
            dropDict.Add(7, []);
            dropDict.Add(8, []);
            dropDict.Add(9, []);
            dropDict.Add(10, []);
            Data.FactionDrop._recordsByFactions.Add("iom_faction_rewardEquipment", dropDict);
            
            dropDict = new();
            dropDict.Add(0, []);
            dropDict.Add(1, [new()
                {
                    TechLevel = 1,
                    ContentIds = ["itemChip"],
                    Weight = 1,
                    Points = 100
                }
            ]);
            dropDict.Add(2, [new()
                {
                    TechLevel = 2,
                    ContentIds = ["itemChip"],
                    Weight = 1,
                    Points = 100
                }
            ]);
            dropDict.Add(3, [new()
                {
                    TechLevel = 3,
                    ContentIds = ["itemChip"],
                    Weight = 1,
                    Points = 100
                }
            ]);
            dropDict.Add(4, []);
            dropDict.Add(5, []);
            dropDict.Add(6, []);
            dropDict.Add(7, []);
            dropDict.Add(8, []);
            dropDict.Add(9, []);
            dropDict.Add(10, []);
            Data.FactionDrop._recordsByFactions.Add("iom_faction_rewardConsumables", dropDict);
            
            dropDict = new();
            dropDict.Add(0, []);
            dropDict.Add(1, [new()
            {
                TechLevel = 1,
                ContentIds = ["rifle_basic_ammo"],
                Weight = 4,
                Points = 200
            }]);
            dropDict.Add(2, [new() { ContentIds = ["rifle_armorpierce_ammo"], Weight = 5f, Points = 150f, TechLevel = 2}, new() { ContentIds = ["battery_basic_ammo"], Weight = 3f, Points = 100f, TechLevel = 2}]);
            dropDict.Add(3, []);
            dropDict.Add(4, []);
            dropDict.Add(5, []);
            dropDict.Add(6, []);
            dropDict.Add(7, []);
            dropDict.Add(8, []);
            dropDict.Add(9, []);
            dropDict.Add(10, []);
            Data.FactionDrop._recordsByFactions.Add("iom_faction_rewardChips", dropDict);
            
            Debug.Log("[ImperiumOfMan : AfterConfig] Loaded!");
        }
        
        public static List<DmgResist> CreateResists(float blunt = 0f, float pierce = 0f, float lacer = 0f, float fire = 0f, float cold = 0f,  float poison = 0f, float shock = 0f, float beam = 0f)
        {
            return [new() { damage = "blunt", resistPercent = blunt },
                new() { damage = "pierce", resistPercent = pierce },
                new() { damage = "lacer", resistPercent = lacer },
                new() { damage = "fire", resistPercent = fire },
                new() { damage = "cold", resistPercent = cold },
                new() { damage = "poison", resistPercent = poison },
                new() { damage = "shock", resistPercent = shock },
                new() { damage = "beam", resistPercent = beam }
            ];
        }
    }
}
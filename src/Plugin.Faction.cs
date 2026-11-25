using System;
using System.Collections.Generic;
using MGSC;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private const string PdfRole = "iom_pdf";
        private const string CadianRole = "iom_cadian";

        private static void RegisterFaction()
        {
            var imperatorPortrait = LoadRequiredAsset<PortraitDescriptor>("Imperator");
            StageRecord(Data.Portraits, "faction_iom_faction", new ConfigTableRecord
            {
                Id = "faction_iom_faction",
                ContentDescriptor = imperatorPortrait
            });
            StageDescriptor("portraits", "faction_iom_faction", imperatorPortrait);
            RegisterChaosUndividedComms();

            var factionDescriptor = LoadRequiredAsset<FactionDescriptor>("ImperiumOfManFaction");
            StageRecord(Data.Factions, "iom_faction", new FactionRecord
            {
                Id = "iom_faction",
                ContentDescriptor = factionDescriptor,
                Enabled = true,
                InitialPower = 1900,
                InitialTechLevel = 1,
                InitialPlayerReputation = 0,
                FactionType = FactionType.Corp,
                AllianceType = "iom_alliance",
                SpawnMissionChance = 0.1f,
                GuardCreatureId = PdfRole,
                CEOGuardCreatureId = CadianRole,
                AgentCreatureId = "manager",
                CeoMobClassId = "chu_ceo",
                MinQmorphosWhenVictims = 0,
                UseGeneralRewards = true,
                CanBeTraded = true,
                CanBeCaptured = true,
                PortraitsByStrategy = false,
                ItemDropCategories = [ItemCategory]
            });
            StageDescriptor("factions", "iom_faction", factionDescriptor);

            var allianceDescriptor = LoadRequiredAsset<AllianceDescriptor>("HumanAlliance");
            StageRecord(Data.Alliances, "iom_alliance", new AllianceRecord
            {
                Id = "iom_alliance",
                ContentDescriptor = allianceDescriptor,
                AllowStrife = false,
                DefaultFactionType = FactionType.Corp,
                TradeFactionLists = ["iom_faction", "ChurchRevelation", "SBN"]
            });
            StageDescriptor("alliances", "iom_alliance", allianceDescriptor);
            RegisterChaosFaction();
        }

        private static void RegisterUnitDrops()
        {
            RegisterImperialRole(PdfRole, "soldier", "Planetary Defence Trooper",
                _lasgun.Id, _lightHelmet.Id, _lightChestplate.Id, _lightLeggings.Id, _lightBoots.Id);
            RegisterImperialRole(CadianRole, "soldier", "Cadian Guardsman",
                _lasgun.Id, _cadiaHelmet.Id, _cadiaChestplate.Id, _cadiaLeggings.Id, _cadiaBoots.Id, GuardFrag);

            var dropRecords = new List<UnitDropRecord> { CreateUnitDrop(1, 2, 3, 54, null) };
            // Later missions can request an exact tech tier. Keep Guard formations eligible at those tiers.
            for (int techLevel = 2; techLevel <= Data.Global.MaxTechLevel; techLevel++)
            {
                dropRecords.Add(CreateUnitDrop(techLevel, 1, 2, 95, CadianRole));
            }

            foreach (string locationId in Data.StationTypes.Ids)
            {
                var drops = Data.LocationUnitDrop.Get(locationId);
                if (drops != null)
                {
                    Registrations.Add(() => drops.AddRange(dropRecords));
                }
            }
        }

        private static MobClassRecord RegisterImperialRole(string id, string donorId, string name, params string[] loadout) =>
            RegisterEquippedRole(id, donorId, name, ItemCategory, loadout);

        private static MobClassRecord RegisterEquippedRole(string id, string donorId, string name,
            string factionCategory, params string[] loadout)
        {
            var donor = Data.MobClasses.GetRecord(donorId)
                ?? throw new InvalidOperationException("Combat role " + id + " requires donor " + donorId);
            var role = (MobClassRecord)CloneRecordMethod.Invoke(donor, null);
            role.Id = id;
            role.DefaultItemFactionTag = factionCategory;
            role.ItemCategoriesWhitelist = new() { ["Faction"] = 20 };
            role.PrimaryWeapon = new() { [WeaponClass.None] = 1 };
            role.SecondaryWeapon = new() { [WeaponClass.None] = 1 };
            role.Head = [];
            role.Armor = [];
            role.Leggings = [];
            role.Boots = [];
            role.AdditItemClasses = [];
            role.GrantedItems = [.. loadout];
            StageRecord(Data.MobClasses, id, role);
            Registrations.Add(() => TranslationPatch.AddRole(id, name));
            return role;
        }

        private static UnitDropRecord CreateUnitDrop(int techLevel, int minSize, int maxSize, float points, string leader, string member = PdfRole)
        {
            return new UnitDropRecord
            {
                TechLevel = techLevel,
                UnitSize = new(minSize, maxSize),
                LeaderSpawn = leader == null ? [] : [new(1f, leader)],
                MobClasses = [new(1f, member)],
                AllowedFactions = [ItemCategory],
                Weight = 10,
                Points = points
            };
        }
    }
}

using System;
using System.Collections.Generic;
using MGSC;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private const string PdfRole = "iom_pdf";
        private const string CadianRole = "iom_cadian";
        private const string StormtrooperRole = "iom_stormtrooper";

        private static void RegisterFaction()
        {
            var imperatorPortrait = LoadRequiredAsset<PortraitDescriptor>("Imperator");
            StageRecord(Data.Portraits, "faction_iom_faction", new ConfigTableRecord
            {
                Id = "faction_iom_faction",
                ContentDescriptor = imperatorPortrait
            });
            StageDescriptor("portraits", "faction_iom_faction", imperatorPortrait);

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
                CEOGuardCreatureId = StormtrooperRole,
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

            string questlineId = $"iom_faction{FactionQuestlineRecord.ENDGAME_LOOP_POSTFIX}";
            StageRecord(Data.FactionQuestlines, questlineId, new FactionQuestlineRecord
            {
                Id = questlineId,
                ContentDescriptor = null,
                FactionId = "iom_faction",
                Strategies = [new(100f, "Expansion"), new(60f, "Scouting"), new(45f, "Defense")],
                NextStepOnFail = string.Empty
            });

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
        }

        private static void RegisterUnitDrops()
        {
            RegisterImperialRole(PdfRole, "soldier", "Planetary Defence Trooper",
                _lasgun.Id, _lightHelmet.Id, _lightChestplate.Id, _lightLeggings.Id, _lightBoots.Id);
            RegisterImperialRole(CadianRole, "soldier", "Cadian Guardsman",
                _lasgun.Id, "iom_bolt_pistol", _cadiaHelmet.Id, _cadiaChestplate.Id, _cadiaLeggings.Id, _cadiaBoots.Id, "iom_frag_grenade");
            RegisterImperialRole(StormtrooperRole, "sbn_elite", "Imperial Stormtrooper",
                _bolter.Id, _plasmaPistol.Id, _cadiaHelmet.Id, _cadiaChestplate.Id, _cadiaLeggings.Id, _cadiaBoots.Id, "iom_krak_grenade");

            UnitDropRecord[] dropRecords =
            [
                CreateUnitDrop(1, 1, 2, 10, 48, null, (1, PdfRole)),
                CreateUnitDrop(2, 2, 3, 9, 105, CadianRole, (3, PdfRole), (2, CadianRole)),
                CreateUnitDrop(3, 2, 3, 8, 145, CadianRole, (1, PdfRole), (3, CadianRole)),
                CreateUnitDrop(4, 2, 3, 7, 185, StormtrooperRole, (3, CadianRole), (1, StormtrooperRole)),
                CreateUnitDrop(5, 3, 4, 6, 260, StormtrooperRole, (1, CadianRole), (2, StormtrooperRole))
            ];

            foreach (string locationId in Data.StationTypes.Ids)
            {
                var drops = Data.LocationUnitDrop.Get(locationId);
                if (drops != null)
                {
                    Registrations.Add(() => drops.AddRange(dropRecords));
                }
            }
        }

        private static void RegisterImperialRole(string id, string donorId, string name, params string[] loadout)
        {
            var donor = Data.MobClasses.GetRecord(donorId)
                ?? throw new InvalidOperationException("Imperium combat role " + id + " requires donor " + donorId);
            var role = (MobClassRecord)CloneRecordMethod.Invoke(donor, null);
            role.Id = id;
            role.DefaultItemFactionTag = ItemCategory;
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
        }

        private static UnitDropRecord CreateUnitDrop(int techLevel, int minSize, int maxSize, float weight,
            float points, string leader, params (float Weight, string Id)[] members)
        {
            var classes = new List<Tuple<float, string>>();
            foreach (var member in members)
            {
                classes.Add(Tuple.Create(member.Weight, member.Id));
            }
            return new UnitDropRecord
            {
                TechLevel = techLevel,
                UnitSize = new(minSize, maxSize),
                LeaderSpawn = leader == null ? [] : [new(1f, leader)],
                MobClasses = classes,
                AllowedFactions = ["iom_faction"],
                Weight = weight,
                Points = points
            };
        }
    }
}

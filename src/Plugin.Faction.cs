using System.Collections.Generic;
using MGSC;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private static void RegisterFaction()
        {
            var imperatorPortrait = LoadRequiredAsset<PortraitDescriptor>("Imperator");
            Data.Portraits.AddRecord("faction_iom_faction", new ConfigTableRecord
            {
                Id = "faction_iom_faction",
                ContentDescriptor = imperatorPortrait
            });
            Data.Descriptors["portraits"].AddDescriptor("faction_iom_faction", imperatorPortrait);

            var factionDescriptor = LoadRequiredAsset<FactionDescriptor>("ImperiumOfManFaction");
            Data.Factions.AddRecord("iom_faction", new FactionRecord
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
                GuardCreatureId = "elite_sbn",
                AgentCreatureId = "civilian",
                MinQmorphosWhenVictims = 0,
                UseGeneralRewards = true,
                PortraitsByStrategy = false,
                ItemDropCategories = [ItemCategory]
            });
            Data.Descriptors["factions"].AddDescriptor("iom_faction", factionDescriptor);

            string questlineId = $"iom_faction{FactionQuestlineRecord.ENDGAME_LOOP_POSTFIX}";
            Data.FactionQuestlines.AddRecord(questlineId, new FactionQuestlineRecord
            {
                Id = questlineId,
                ContentDescriptor = null,
                FactionId = "iom_faction",
                Strategies = [new(100f, "Expansion")],
                NextStepOnFail = string.Empty
            });

            var allianceDescriptor = LoadRequiredAsset<AllianceDescriptor>("HumanAlliance");
            Data.Alliances.AddRecord("iom_alliance", new AllianceRecord
            {
                Id = "iom_alliance",
                ContentDescriptor = allianceDescriptor,
                AllowStrife = false,
                DefaultFactionType = FactionType.Corp,
                TradeFactionLists = ["iom_faction", "ChurchRevelation", "SBN"]
            });
            Data.Descriptors["alliances"].AddDescriptor("iom_alliance", allianceDescriptor);
        }

        private static void RegisterUnitDrops()
        {
            UnitDropRecord[] dropRecords =
            [
                CreateUnitDrop(1, 2, 2, 12, 80),
                CreateUnitDrop(1, 1, 1, 6, 20),
                CreateUnitDrop(2, 2, 2, 12, 90),
                CreateUnitDrop(3, 1, 1, 6, 30),
                CreateUnitDrop(4, 1, 1, 6, 25)
            ];

            foreach (string locationId in Data.LocationUnitDrop._recordsByLocations.Keys)
            {
                Data.LocationUnitDrop._recordsByLocations[locationId].AddRange(dropRecords);
            }
        }

        private static UnitDropRecord CreateUnitDrop(int techLevel, int width, int height, float weight, float points)
        {
            return new UnitDropRecord
            {
                TechLevel = techLevel,
                UnitSize = new(width, height),
                LeaderSpawn = [],
                MobClasses = [new(1f, "soldier")],
                AllowedFactions = ["iom_faction"],
                Weight = weight,
                Points = points
            };
        }
    }
}

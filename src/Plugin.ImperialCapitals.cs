using System;
using System.Linq;
using HarmonyLib;
using MGSC;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private const string TitanFortress = "iomTitan";
        private const string BroadswordStation = "iomBroadsword";
        private const string PalaceProduction = "iom_palace_armory";
        private const string TitanProduction = "iom_titan_vault_workshop";
        private const string BroadswordProduction = "iom_broadsword_repair_yard";
        private const int TitanContractTier = 10;
        private const int TitanContractIntervalDays = 28;
        private static readonly string[] PermanentImperialSites = [TerraStation, TitanFortress, BroadswordStation, FoundryStation];

        private static void RegisterImperialCapitals(StationRecord stationDonor)
        {
            var palaceBarter = Data.StationBarter.GetRecord("PineGapTraining");
            var fortressBarter = Data.StationBarter.GetRecord("CassinisShadow");
            var navalBarter = Data.StationBarter.GetRecord("Chimera9");
            if (palaceBarter == null || fortressBarter == null || navalBarter == null)
            {
                throw new InvalidOperationException("Native barter profiles for the Imperial capital and Titan facilities are unavailable.");
            }

            AddStationReceipt(PalaceProduction, 48, [new(Sheet, 2), new(Textile, 1)], [new(Plate, 2), new(Liner, 1)]);
            RegisterStation(stationDonor, palaceBarter, TerraStation, "earth", "Military", PalaceProduction,
                powerGain: 2, techLevelGain: 3, uncapturable: true, missionTemplateId: "orbit_Military",
                bramfaturaId: "No_Bramfatura", initialPopulation: 22314, maxPopulation: 60840, captureChance: 0f,
                corpReceipts: ["growth_military", "data_military", "use_training", "use_armor", "use_weapon"]);

            AddStationReceipt(TitanProduction, 72, [new(Sheet, 2), new("scientific_equipment", 1)],
                [new(HeavyPlate, 1), new("tech_repair_kit", 1)]);
            RegisterStation(stationDonor, fortressBarter, TitanFortress, "titan", "Military", TitanProduction,
                powerGain: 1, techLevelGain: 2, uncapturable: true, missionTemplateId: "moon_Military_ganymede",
                bramfaturaId: "No_Bramfatura", initialPopulation: 15300, maxPopulation: 34500, captureChance: 0f,
                corpReceipts: ["growth_military", "data_military", "use_science", "use_training", "use_armor"]);

            AddStationReceipt(BroadswordProduction, 60, [new(Metal, 2), new("electrical_parts_container", 1)],
                [new("firearm_repair_kit", 1), new("armor_repair_kit", 1)]);
            RegisterStation(stationDonor, navalBarter, BroadswordStation, "titan", "Military", BroadswordProduction,
                powerGain: 2, techLevelGain: 1, uncapturable: true, missionTemplateId: "orbit_Military",
                bramfaturaId: "No_Bramfatura", initialPopulation: 9900, maxPopulation: 22600, captureChance: 0f,
                corpReceipts: ["growth_military", "factory_military", "use_comm", "use_tool", "use_weapon"]);
        }

        [Hook(ModHookType.SpaceUpdateAfterGameLoop)]
        public static void OfferTitanContracts(IModContext context)
        {
            var state = context.State;
            var factions = state.Get<Factions>();
            var imperium = factions.Values.Find(faction => faction.Id == ItemCategory);
            if (imperium == null || imperium.CurrentTechLevel < TitanContractTier || !factions.IsEnabledFaction(imperium))
            {
                return;
            }

            var missions = state.Get<Missions>();
            var stations = state.Get<Stations>();
            var time = state.Get<SpaceTime>();
            var triggers = state.Get<StoryTriggers>();
            var cache = state.Get<FactionCachedData>();
            if (!cache.factionFriends.TryGetValue(ItemCategory, out var friends))
            {
                return;
            }
            Offer(TitanFortress, ProceduralMissionType.Elimination, TitanPaladinPatrol, TitanPaladinGarrison,
                FormationDeployment.FinalFloor);
            Offer(BroadswordStation, ProceduralMissionType.Security, BroadswordPaladinPatrol, BroadswordPaladinGarrison,
                FormationDeployment.Entrance);

            void Offer(string id, ProceduralMissionType type, string patrolTable, string garrisonTable, FormationDeployment deployment)
            {
                var station = stations.Get(id, logMissing: false);
                if (station?.OwnerFactionId != ItemCategory)
                {
                    return;
                }
                var active = missions.Get(id, reversed: false);
                if (active != null)
                {
                    if (active.BeneficiaryFactionId == ItemCategory && active.ProcMissionType == type &&
                        !active.LocationPlans.Values.Any(plan => plan.MonstersTableIds.Contains(patrolTable)))
                    {
                        DeployFormation(active, missions.Get(id, reversed: true), patrolTable, garrisonTable, TitanContractTier, deployment);
                    }
                    return;
                }

                string timer = "iom.contract." + id;
                if (missions.Values.Count(mission => mission.BeneficiaryFactionId == ItemCategory) >=
                        Data.TechLevels[imperium.CurrentTechLevel].MaxMissionsCount ||
                    (triggers.TimeEventMap.ContainsKey(timer) && !triggers.HasTimeEventElapsed(timer, TitanContractIntervalDays, time.Time)) ||
                    !cache.stationNeighbours.TryGetValue(id, out var neighbors))
                {
                    return;
                }

                var raiders = neighbors.Select(site => factions.Get(site.OwnerFactionId))
                    .FirstOrDefault(faction => faction != null && faction.Id != ItemCategory &&
                        faction.FactionType == FactionType.Corp && factions.IsEnabledFaction(faction) &&
                        !friends.Contains(faction.Id));
                if (raiders == null)
                {
                    return;
                }

                MissionSystem.CreateMission(state.Get<MissionFactory>(), missions, state.Get<News>(), time,
                    state.Get<TravelMetadata>(), type, station, imperium, raiders);
                if (missions.Get(id, reversed: false) is Mission contract)
                {
                    DeployFormation(contract, missions.Get(id, reversed: true), patrolTable, garrisonTable, TitanContractTier, deployment);
                    triggers.TimeEventMap[timer] = time.Time;
                }
            }
        }

        [HarmonyPatch(typeof(MissionSystem), nameof(MissionSystem.GetMissionName))]
        private static class ImperialContractNames
        {
            private static void Postfix(Mission mission, ref string __result)
            {
                if (mission.IsStoryMission || mission.BeneficiaryFactionId != ItemCategory)
                {
                    return;
                }

                if (mission.StationId == TitanFortress && mission.ProcMissionType == ProceduralMissionType.Elimination)
                {
                    __result = Localization.Get("iom.contract.titan.name");
                }
                else if (mission.StationId == BroadswordStation && mission.ProcMissionType == ProceduralMissionType.Security)
                {
                    __result = Localization.Get("iom.contract.broadsword.name");
                }
                else if (mission.StoryId == PhobosQuarantineStory)
                {
                    __result = Localization.Get("mission." + PhobosQuarantineStory + ".name");
                }
                else if (mission.StoryId == SororitasReliquaryStory)
                {
                    __result = Localization.Get("mission." + SororitasReliquaryStory + ".name");
                }
                else if (mission.StationId == TerraStation &&
                    mission.LocationPlans.Values.Any(plan => plan.MonstersTableIds.Contains(TerraGoldenPatrol)))
                {
                    __result = Localization.Get("iom.contract.golden.name");
                }
            }
        }
    }
}

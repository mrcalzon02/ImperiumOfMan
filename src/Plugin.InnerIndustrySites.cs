using System;
using System.Collections.Generic;
using System.Linq;
using MGSC;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private const string MeridianWaterworks = "iomMeridianWaterworks";
        private const string GreenlineProvisions = "iomGreenlineProvisions";
        private const string JunctionReclamation = "iomJunctionReclamation";
        private const string CinderBasinMine = "iomCinderBasinMine";
        private const string HesperiaFabrication = "iomHesperiaFabrication";
        private const string VallisGrowworks = "iomVallisGrowworks";
        private const string GrowBiomass = "iom_inner_biomass";

        private static readonly Dictionary<string, string> InnerIndustryOwners = new()
        {
            [MeridianWaterworks] = "FrancheComte",
            [GreenlineProvisions] = "DaydreamChems",
            [JunctionReclamation] = "PlanetBridge",
            [CinderBasinMine] = "CoreWard",
            [HesperiaFabrication] = "ChurchRevelation",
            [VallisGrowworks] = "Grasshopper"
        };

        private static void RegisterInnerIndustrySites(StationRecord stationDonor)
        {
            const string toolRecovery = "iom_junction_tools";
            const string electricalAssembly = "iom_hesperia_electrical";
            AddStationReceipt(GrowBiomass, 144, [new("water_container", 2)], [new("biomass_tank", 4)]);
            AddStationReceipt(toolRecovery, 168, [new("ore_cargo", 1), new("engineering_parts_container", 1)],
                [new("tool_container", 2)]);
            AddStationReceipt(electricalAssembly, 168, [new("ore_cargo", 2), new("tool_container", 1)],
                [new("electrical_parts_container", 3)]);

            Register(MeridianWaterworks, "earth", "Mine", "orbit_Industrial", "CalorisPlanitia", 18200, 31500,
                ["growth_mine", "mining_water"]);
            Register(GreenlineProvisions, "earth", "ColonyFarm", "orbit_Farm", "SterlingsFarm", 24200, 39000,
                ["growth_basic", "food_basic"], GrowBiomass);
            Register(JunctionReclamation, "earth", "Industrial", "orbit_Industrial", "ArcadisTerminal", 19100, 33000,
                ["growth_industrial", "mining_metal"], toolRecovery);

            Register(CinderBasinMine, "mars", "Mine", "mars_marsMine_mars", "Argyre", 17500, 31000,
                ["growth_mine", "mining_metal", "mining_water"]);
            Register(HesperiaFabrication, "mars", "Industrial", "mars_Spaceport_mars", "Gloria", 22800, 37000,
                ["growth_industrial", "factory_engineer"], electricalAssembly);
            Register(VallisGrowworks, "mars", "ColonyFarm", "orbit_Farm", "SterlingsFarm", 18100, 33000,
                ["growth_basic", "food_basic"], GrowBiomass);

            void Register(string id, string body, string type, string template, string donorId,
                int population, int maxPopulation, List<string> production, string recipe = null)
            {
                string owner = InnerIndustryOwners[id];
                var donor = Data.StationBarter.GetRecord(donorId);
                if (donor == null || Data.Factions.GetRecord(owner) == null)
                {
                    throw new InvalidOperationException("Missing station or faction data for " + id);
                }

                var barter = RegisterStation(stationDonor, donor, id, body, type, recipe,
                    powerGain: 1, techLevelGain: 1, uncapturable: false, missionTemplateId: template,
                    bramfaturaId: body == "earth" ? "No_Bramfatura" : "Ur_Sleeping",
                    initialPopulation: population, maxPopulation: maxPopulation, captureChance: 1f,
                    corpReceipts: production, initialOwner: owner);
                barter.CivResProduceItems = [.. barter.CorpProduceItems];
                barter.PiratesProduceItems = [.. barter.CorpProduceItems];
                StationVisitLayouts.Add(id, donorId);
            }
        }

        private static Faction SelectInnerIndustryOwner(Factions factions, Stations stations, string preferredId)
        {
            var preferred = factions.Get(preferredId, logMissing: false);
            if (preferred != null && factions.IsEnabledFaction(preferred))
            {
                return preferred;
            }

            // A faction eliminated before the update should not reappear with a new station.
            return factions.Values
                .Where(faction => factions.IsEnabledFaction(faction) &&
                    faction.FactionType is FactionType.Corp or FactionType.CivilRes or FactionType.Pirates)
                .OrderByDescending(faction => InnerIndustryOwners.ContainsValue(faction.Id))
                .ThenBy(faction => stations.Values.Count(station => station.OwnerFactionId == faction.Id))
                .ThenBy(faction => faction.Id, StringComparer.Ordinal)
                .FirstOrDefault();
        }
    }
}

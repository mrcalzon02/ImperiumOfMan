using MGSC;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private static void RegisterStations()
        {
            StationRecord stationDonor = Data.Stations.GetRecord("Paragon");
            StationBarterRecord barterDonor = Data.StationBarter.GetRecord("Paragon");
            if (stationDonor == null || stationDonor.ContentDescriptor == null)
            {
                throw new System.InvalidOperationException("Quasimorph station donor 'Paragon' is unavailable.");
            }

            if (barterDonor == null)
            {
                throw new System.InvalidOperationException("Quasimorph station barter donor 'Paragon' is unavailable.");
            }

            RegisterStation(
                stationDonor,
                barterDonor,
                "iomTerra",
                "earth",
                powerGain: 2,
                techLevelGain: 3,
                uncapturable: true,
                missionTemplateId: "orbit_Military",
                bramfaturaId: "No_Bramfatura",
                initialPopulation: 22314,
                maxPopulation: 60840);

            RegisterStation(
                stationDonor,
                barterDonor,
                "iomMoom",
                "fuller",
                powerGain: 1,
                techLevelGain: 1,
                uncapturable: false,
                missionTemplateId: "orbit_Station",
                bramfaturaId: "Duggur",
                initialPopulation: 7201,
                maxPopulation: 33034);

            RegisterStation(
                stationDonor,
                barterDonor,
                "iomPhobos",
                "phobos",
                powerGain: 1,
                techLevelGain: 2,
                uncapturable: false,
                missionTemplateId: "orbit_Spaceport",
                bramfaturaId: "Ur_Sleeping",
                initialPopulation: 8451,
                maxPopulation: 39932);

            RegisterStation(
                stationDonor,
                barterDonor,
                "iomHavoc",
                "havoc",
                powerGain: 2,
                techLevelGain: 1,
                uncapturable: false,
                missionTemplateId: "orbit_Spaceport",
                bramfaturaId: "Gannix",
                initialPopulation: 14451,
                maxPopulation: 42112);
        }

        private static void RegisterStation(
            StationRecord stationDonor,
            StationBarterRecord barterDonor,
            string id,
            string spaceObjectId,
            int powerGain,
            int techLevelGain,
            bool uncapturable,
            string missionTemplateId,
            string bramfaturaId,
            int initialPopulation,
            int maxPopulation)
        {
            Data.Stations.AddRecord(id, new StationRecord
            {
                Id = id,
                ContentDescriptor = stationDonor.ContentDescriptor,
                SpaceObjectId = spaceObjectId,
                InitialOwnerFactionId = "iom_faction",
                Power = 0,
                PowerGain = powerGain,
                TechLevelGain = techLevelGain,
                UncapturableByDefault = uncapturable,
                CaptureChance = 0.2f,
                MissionTemplateId = missionTemplateId,
                MissionNameTemplateId = "mercurySurfaceLevels",
                BramfaturaId = bramfaturaId,
                StationType = "SpaceStation",
                SpaceObjectItemDropPercent = 0.1f,
                InitialPopulation = initialPopulation,
                MaxPopulation = maxPopulation,
                SpawnOnStart = true
            });

            Data.StationBarter.AddRecord(id, CloneBarter(id, barterDonor));
        }

        private static StationBarterRecord CloneBarter(string id, StationBarterRecord donor)
        {
            return new StationBarterRecord
            {
                Id = id,
                ContentDescriptor = null,
                CorpProduceItems = [.. donor.CorpProduceItems],
                CorpAdditionalConsumeItems = [.. donor.CorpAdditionalConsumeItems],
                CivResProduceItems = [.. donor.CivResProduceItems],
                CivResAdditionalConsumeItems = [.. donor.CivResAdditionalConsumeItems],
                QuasiProduceItems = [.. donor.QuasiProduceItems],
                QuasiAdditionalConsumeItems = [.. donor.QuasiAdditionalConsumeItems],
                PiratesProduceItems = [.. donor.PiratesProduceItems],
                PiratesAdditionalConsumeItems = [.. donor.PiratesAdditionalConsumeItems]
            };
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using MGSC;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private static void RegisterStations()
        {
            StationRecord stationDonor = Data.Stations.GetRecord("Paragon");
            var lunaBarter = Data.StationBarter.GetRecord("SterlingsFarm");
            var phobosBarter = Data.StationBarter.GetRecord("Canaan");
            var havocBarter = Data.StationBarter.GetRecord("SampoQuern");
            if (stationDonor == null)
            {
                throw new System.InvalidOperationException("Quasimorph station donor 'Paragon' is unavailable.");
            }

            if (lunaBarter == null || phobosBarter == null || havocBarter == null)
            {
                throw new System.InvalidOperationException("The native farm, munitions depot and industrial trade profiles are unavailable.");
            }

            RegisterImperialCapitals(stationDonor);

            RegisterStation(
                stationDonor,
                lunaBarter,
                LunaStation,
                "fuller",
                "SpaceStation",
                null,
                powerGain: 1,
                techLevelGain: 1,
                uncapturable: false,
                missionTemplateId: "orbit_Farm",
                bramfaturaId: "Duggur",
                initialPopulation: 7201,
                maxPopulation: 33034,
                corpReceipts: ["growth_basic", "food_basic", "food_military", "data_memory"]);

            RegisterStation(
                stationDonor,
                phobosBarter,
                PhobosStation,
                "phobos",
                "Military",
                null,
                powerGain: 1,
                techLevelGain: 2,
                uncapturable: false,
                missionTemplateId: "orbit_Spaceport",
                bramfaturaId: "Ur_Sleeping",
                initialPopulation: 8451,
                maxPopulation: 39932,
                corpReceipts: ["growth_military", "factory_military", "data_military", "use_weapon", "use_tool"]);

            AddStationReceipt(HavocFabrication, 60, [new(Metal, 2), new(PolymerStock, 1)], [new(Fittings, 3), new(Sheet, 1)]);

            RegisterStation(
                stationDonor,
                havocBarter,
                HavocStation,
                "havoc",
                "Industrial",
                HavocFabrication,
                powerGain: 2,
                techLevelGain: 1,
                uncapturable: false,
                missionTemplateId: "orbit_Industrial",
                bramfaturaId: "Gannix",
                initialPopulation: 14451,
                maxPopulation: 42112,
                corpReceipts: havocBarter.CorpProduceItems.Where(receipt => receipt != "device_robot").ToList());

            RegisterMechanicusSites(stationDonor);
            RegisterInnerIndustrySites(stationDonor);
            RegisterChaosSites(stationDonor, phobosBarter, havocBarter);
            RegisterFringeOutposts(stationDonor, phobosBarter, havocBarter);
        }

        private static StationBarterRecord RegisterStation(
            StationRecord stationDonor,
            StationBarterRecord barterDonor,
            string id,
            string spaceObjectId,
            string stationType,
            string receipt,
            int powerGain,
            int techLevelGain,
            bool uncapturable,
            string missionTemplateId,
            string bramfaturaId,
            int initialPopulation,
            int maxPopulation,
            float captureChance = 0.2f,
            SpaceObjectRecord newSpaceObject = null,
            List<string> corpReceipts = null,
            string initialOwner = ItemCategory)
        {
            if ((newSpaceObject?.Id != spaceObjectId && Data.SpaceObjects.GetRecord(spaceObjectId) == null) ||
                Data.StationTypes.GetRecord(stationType) == null ||
                Data.ProcMissionTemplates.GetRecord(missionTemplateId) == null ||
                Data.Bramfaturas.GetRecord(bramfaturaId) == null)
            {
                throw new System.InvalidOperationException("Quasimorph station references are unavailable for " + id);
            }

            if (newSpaceObject != null)
            {
                StageRecord(Data.SpaceObjects, spaceObjectId, newSpaceObject);
            }
            StageRecord(Data.Stations, id, new StationRecord
            {
                Id = id,
                ContentDescriptor = stationDonor.ContentDescriptor,
                SpaceObjectId = spaceObjectId,
                InitialOwnerFactionId = initialOwner,
                Power = 0,
                PowerGain = powerGain,
                TechLevelGain = techLevelGain,
                UncapturableByDefault = uncapturable,
                CaptureChance = captureChance,
                MissionTemplateId = missionTemplateId,
                MissionNameTemplateId = TranslationPatch.FloorNameTemplate,
                BramfaturaId = bramfaturaId,
                StationType = stationType,
                SpaceObjectItemDropPercent = 0.1f,
                InitialPopulation = initialPopulation,
                MaxPopulation = maxPopulation,
                SpawnOnStart = true
            });

            var barter = CloneBarter(id, barterDonor, receipt, corpReceipts);
            StageRecord(Data.StationBarter, id, barter);
            return barter;
        }

        private static StationBarterRecord CloneBarter(string id, StationBarterRecord donor, string receipt,
            List<string> corpReceipts)
        {
            var produce = corpReceipts ?? donor.CorpProduceItems;
            return new StationBarterRecord
            {
                Id = id,
                ContentDescriptor = null,
                CorpProduceItems = receipt == null ? [.. produce] : [.. produce, receipt],
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

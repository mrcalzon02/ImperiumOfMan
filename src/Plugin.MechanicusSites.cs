using System;
using System.Linq;
using HarmonyLib;
using MGSC;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private const string FoundryStation = "iomFoundryPrimaris";
        private const string FoundryReceipt = "iom_foundry_primaris_smelting";
        private const string NeithBody = "iomNeith";
        private const string NeithVault = "iomNeithVault";
        private const string NeithReceipt = "iom_neith_seed_vault";
        private static PortraitDescriptor _mechanicusPortrait;
        private static string _mechanicusBriefingStation;

        private static void RegisterMechanicusSites(StationRecord stationDonor)
        {
            var foundryBarter = Data.StationBarter.GetRecord("RedTiangong");
            var vaultBarter = Data.StationBarter.GetRecord("EStation");
            if (foundryBarter == null || vaultBarter == null)
            {
                throw new InvalidOperationException("The native Mars industrial and Venus orbital lab barter profiles are unavailable.");
            }

            AddStationReceipt(FoundryReceipt, 40, [new(Ore, 4)], [new(Metal, 2), new(Sheet, 1)]);
            RegisterStation(stationDonor, foundryBarter, FoundryStation, "mars", "Industrial", FoundryReceipt,
                powerGain: 2, techLevelGain: 1, uncapturable: true, missionTemplateId: "mars_Spaceport_mars",
                bramfaturaId: "No_Bramfatura", initialPopulation: 21700, maxPopulation: 46800, captureChance: 0f);

            var orbitDonor = Data.SpaceObjects.GetRecord("havoc");
            if (orbitDonor?.ParentId != "venus")
            {
                throw new InvalidOperationException("Neith needs the existing Venus satellite reference.");
            }
            var neith = new SpaceObjectRecord
            {
                Id = NeithBody,
                Available = true,
                ParentId = "venus",
                SpaceObjectType = SpaceObjectType.Satelite,
                ContentDescriptor = orbitDonor.ContentDescriptor
            };
            // Keep the original outputs available for orders already in progress.
            AddStationReceipt(NeithReceipt, 54, [new("water_bottle_1", 2)], [new(Biomass, 4)]);
            RegisterStation(stationDonor, vaultBarter, NeithVault, NeithBody, "Lab", null,
                powerGain: 3, techLevelGain: 1, uncapturable: false, missionTemplateId: "orbit_Lab",
                bramfaturaId: "No_Bramfatura", initialPopulation: 10900, maxPopulation: 28400,
                newSpaceObject: neith,
                corpReceipts: ["growth_lab", "lab_basic", "data_memory", "use_science", "use_chem", "use_sensor"]);

            _mechanicusPortrait = ScriptableObject.CreateInstance<PortraitDescriptor>();
            _mechanicusPortrait.name = "Mechanicus facility transmission";
            _mechanicusPortrait._normalSprite = ReadItemSprite("Mechanicus-Mars-CRT", null);
            const string portraitId = "spaceobject_" + NeithBody;
            StageRecord(Data.Portraits, portraitId, new ConfigTableRecord
                { Id = portraitId, ContentDescriptor = _mechanicusPortrait });
        }

        [Hook(ModHookType.AfterSpaceLoaded)]
        public static void RestoreImperialSitesOnLoad(IModContext context)
        {
            var state = context.State;
            var stations = state.Get<Stations>();
            var factory = state.Get<StationFactory>();
            var factions = state.Get<Factions>();
            var prices = state.Get<ItemsPrices>();
            var progression = state.Get<MagnumProgression>();
            if (stations == null || factory == null || Data.Stations.GetRecord(FoundryStation) == null)
            {
                return;
            }

            bool changed = false;
            foreach (string id in PermanentImperialSites)
            {
                changed |= AddSite(id, permanent: true);
            }
            changed |= AddSite(NeithVault, permanent: false);
            AddSite(LunaStation, permanent: false, createMissing: false);
            AddSite(PhobosStation, permanent: false, createMissing: false);
            AddSite(HavocStation, permanent: false, createMissing: false);
            foreach (var site in InnerIndustryOwners)
            {
                changed |= AddSite(site.Key, permanent: false, initialOwner: site.Value);
            }
            ProtectImperialStrongholds(stations);
            if (changed)
            {
                var cache = state.Get<FactionCachedData>();
                CacheSystem.CacheStationOwners(cache, stations);
                CacheSystem.CacheStationNeighbours(cache, stations);
                RefreshIndustryMarkets(stations, factions, state.Get<SpaceTime>());
            }

            bool AddSite(string id, bool permanent, bool createMissing = true, string initialOwner = null)
            {
                var station = stations.Get(id, logMissing: false);
                if (station != null)
                {
                    bool restoredOwner = permanent && station.OwnerFactionId != ItemCategory;
                    if (restoredOwner)
                    {
                        station.OwnerFactionId = ItemCategory;
                        station.OwnerChangedDirty = false;
                    }
                    var barter = Data.StationBarter.GetRecord(id);
                    var receipts = factions.Get(station.OwnerFactionId).FactionType switch
                    {
                        FactionType.Corp => barter.CorpProduceItems,
                        FactionType.CivilRes => barter.CivResProduceItems,
                        FactionType.Pirates => barter.PiratesProduceItems,
                        FactionType.Tezctlan or FactionType.Xiomara or FactionType.Shedu => barter.QuasiProduceItems,
                        _ => null
                    };
                    if (restoredOwner || (receipts != null && !station.CurrentReceipts.SequenceEqual(receipts)))
                    {
                        StationSystem.RefreshConsumableItems(prices, progression, factions, station);
                    }
                    return restoredOwner;
                }
                if (!createMissing || (!permanent && stations.DeletedStationsIds.Contains(id)))
                {
                    return false;
                }
                station = factory.Create(id);
                if (initialOwner != null)
                {
                    var owner = SelectInnerIndustryOwner(factions, stations, initialOwner);
                    if (owner == null)
                    {
                        return false;
                    }
                    station.OwnerFactionId = owner.Id;
                }
                StationSystem.RefreshConsumableItems(prices, progression, factions, station);
                stations.DeletedStationsIds.Remove(id);
                stations.Values.Add(station);
                return true;
            }
        }

        private static bool IsImperialStronghold(string id) => Array.IndexOf(PermanentImperialSites, id) >= 0;

        private static void ProtectImperialStrongholds(Stations stations)
        {
            foreach (Station station in stations.Values)
            {
                if (IsImperialStronghold(station.Id))
                {
                    station.UncapturableByDefault = true;
                    station.ImmuneToAttack = true;
                }
            }
        }

        [HarmonyPatch(typeof(StationSystem), nameof(StationSystem.Create))]
        private static class ImperialStrongholdStartingProtection
        {
            private static void Postfix(Stations __result) => ProtectImperialStrongholds(__result);
        }

        [HarmonyPatch(typeof(FactionSystem), nameof(FactionSystem.RefreshFactionStatus))]
        private static class ImperialStrongholdCampaignProtection
        {
            private static void Postfix(Stations stations) => ProtectImperialStrongholds(stations);
        }

        [HarmonyPatch(typeof(StationSystem), nameof(StationSystem.CaptureStation))]
        private static class ImperialStrongholdCaptureProtection
        {
            private static bool Prefix(string stationId) => !IsImperialStronghold(stationId);
        }

        [HarmonyPatch(typeof(StationSystem), nameof(StationSystem.DeleteStation),
            [typeof(Stations), typeof(Missions), typeof(FactionCachedData), typeof(TravelMetadata), typeof(string)])]
        private static class ImperialStrongholdDeletionProtection
        {
            private static bool Prefix(string stationId) => !IsImperialStronghold(stationId);
        }

        [HarmonyPatch(typeof(SpaceObjects), nameof(SpaceObjects.FillWithContent))]
        private static class NeithStarmap
        {
            private static void Prefix(ref SpaceObject[] spaceObjects)
            {
                if (Data.SpaceObjects.GetRecord(NeithBody) == null ||
                    spaceObjects.Any(body => body.ID == NeithBody))
                {
                    return;
                }
                var container = SingletonMonoBehaviour<SpacemapContainer>.Instance;
                var donor = spaceObjects.FirstOrDefault(body => body.ID == "havoc");
                var venusGroup = donor == null ? null : container.GetObjectGroup(donor);
                if (venusGroup == null)
                {
                    throw new InvalidOperationException("The Venus space object group could not be loaded for Neith.");
                }

                OrbitRing originalRing = donor.OrbitRing;
                var ring = Object.Instantiate(originalRing, originalRing.transform.parent);
                ring.name = "Neith orbit";
                var ellipse = ring.EllipseGenerator;
                ellipse.SetScales(ellipse.XYScale * 1.35f, ellipse.ZScale * 1.35f);
                ellipse.CalculateEllipse();
                var moon = Object.Instantiate(donor, donor.transform.parent);
                moon.name = "Neith";
                moon._id = NeithBody;
                moon._orbit = ring;
                moon._origXYScale = ellipse.XYScale;
                moon._origZScale = ellipse.ZScale;
                moon.Progress = (donor.Progress + 0.37) % 1.0;
                moon._fullSizeScale *= 0.82f;
                moon._inactiveSizeScale *= 0.82f;
                ring._orbitalUsers = [moon];

                venusGroup.SpaceObjects.Add(moon);
                container._orbitRings = [.. container.OrbitRings, ring];
                container._spaceObjects = [.. spaceObjects, moon];
                spaceObjects = container.SpaceObjects;
            }
        }

        private static bool IsImperialMechanicusSite(Station station) =>
            station?.OwnerFactionId == ItemCategory && (station.Id == FoundryStation || station.Id == NeithVault);

        [HarmonyPatch(typeof(BriefingScreen), "ShowDialog")]
        private static class MechanicusBriefingScope
        {
            private static void Prefix(Station ____station, Mission ____mission, out string __state)
            {
                __state = _mechanicusBriefingStation;
                _mechanicusBriefingStation = IsImperialMechanicusSite(____station) && ____mission?.IsStoryMission != true
                    ? ____station.Id
                    : null;
            }

            private static Exception Finalizer(Exception __exception, string __state)
            {
                _mechanicusBriefingStation = __state;
                return __exception;
            }
        }

        [HarmonyPatch(typeof(NarrativeTextScreen), nameof(NarrativeTextScreen.Configure))]
        private static class MechanicusBriefingPortrait
        {
            private static void Prefix(ref PortraitDescriptor portrait, ref string captionTag)
            {
                if (_mechanicusBriefingStation != null && _mechanicusPortrait != null)
                {
                    portrait = _mechanicusPortrait;
                    captionTag = "station." + _mechanicusBriefingStation + ".name";
                }
            }
        }

        [HarmonyPatch(typeof(StorySystem), nameof(StorySystem.ShowBriefingMessageOnPeaceStation))]
        private static class MechanicusArrivalMessage
        {
            private static void Postfix(StoryTriggers storyTriggers, Station station, ref string message, ref bool __result)
            {
                if (__result || !IsImperialMechanicusSite(station))
                {
                    return;
                }
                string trigger = "iom.mechanicus.arrived." + station.Id;
                if (!storyTriggers.IsPassed(trigger))
                {
                    storyTriggers.Pass(trigger);
                    message = Localization.Get("station." + station.Id + ".briefing");
                    __result = true;
                }
            }
        }
    }
}

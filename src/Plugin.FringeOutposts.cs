using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MGSC;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private const string FringeIdPrefix = "iomFringe";
        private const int OutpostsPerOrbit = 4;
        private static readonly string[] FringeBodyDonors = ["vesta", "geist", "interamnia", "hygeia"];
        private static readonly float[] FringeOrbitScales = [1.8f, 2.2f, 2.65f, 3.1f];

        private sealed class FringeOutpost(string id, string rock, string station, string code, string faction,
            string template, string description)
        {
            internal readonly string Id = FringeIdPrefix + id;
            internal string Body => Id + "Rock";
            internal readonly string Rock = rock;
            internal readonly string Station = station;
            internal readonly string Code = code;
            internal readonly string Faction = faction;
            internal readonly string Template = "orbit_" + template;
            internal readonly string Description = description;
        }

        // Entries are grouped by orbit, from the first rocks beyond Saturn to the outermost belt.
        private static readonly FringeOutpost[] FringeOutposts =
        [
            new("Ashmouth", "Ashmouth", "Ashmouth Anchorage", "ASH", "CattaramFeral", "Spaceport",
                "A single anchorage cut into an ash-dark rock on the first crossing beyond Saturn."),
            new("Wakescar", "Wakescar", "Wakescar Listening Post", "WAK", "RonFeral", "Router",
                "A listening post buried in an old collision scar. Its receivers track passing drives."),
            new("Blackglass", "Blackglass", "Blackglass Relay", "BLG", "UnchainedBelt", "Router",
                "An unattended tracking mast turned into a paid listening post by Belt salvagers."),
            new("CopperJaw", "Copper Jaw", "Copper Jaw Breakers", "CPJ", "ScrivnusFeral", "Industrial",
                "A breaker yard hangs between two copper-colored ridges. It has room for one working berth."),
            new("SlagVigil", "Slag Vigil", "Slag Vigil Foundry", "SLV", "FlaurosFeral", "Industrial",
                "The small foundry below the docking spindle supplies the reclaimers with frame stock."),
            new("SaltCairn", "Salt Cairn", "Salt Cairn Archive", "SCA", "RonFeral", "Lab",
                "A sealed archive drilled through a bank of salt and black ice."),
            new("SumpChoir", "Sump Choir", "Sump Choir Cages", "SUM", "DuggurFeral", "Station",
                "Duggur's cage deck occupies one old coolant station in a hollow asteroid."),
            new("StoneDebt", "Stone Debt", "Stone Debt Bonded Hold", "SDB", "CattaramFeral", "Spaceport",
                "A bonded hold where a single crane lifts sealed levies into passing barges."),
            new("BleachedMast", "Bleached Mast", "Bleached Mast Choir", "BMC", "OlirnaFeral", "Router",
                "A long-range transmitter protrudes from the face of this white rock."),
            new("BloodToll", "Blood Toll", "Blood Toll Gate", "BTG", "GannixFeral", "Military",
                "A single gun deck commands the gap between two slow fields of debris."),
            new("Kedge", "Kedge", "Kedge Recovery Post", "KRP", "CoreWard", "Spaceport",
                "CoreWard keeps a condemned hull moored alongside its solitary salvage office."),
            new("CinderLock", "Cinder Lock", "Cinder Lock Furnace", "CLF", "UrFeral", "Industrial",
                "Heat leaks through the rock around a compact furnace and its assembly bay."),
            new("BlueRuin", "Blue Ruin", "Blue Ruin Observatory", "BRO", "ShartamakhumFeral", "Lab",
                "The little observatory uses mirrors embedded in blue ice to read the outer crossings."),
            new("SableMile", "Sable Mile", "Sable Mile Redoubt", "SMR", "ScrivnusFeral", "Military",
                "A meeting chamber sits behind the only defended docking collar on the rock."),
            new("HoundsRest", "Hound's Rest", "Hound's Rest Kennels", "HRK", "OlirnaFeral", "Station",
                "A former waystation houses pens for the hounds that guard the final approach."),
            new("EbonCrown", "Ebon Crown", "Ebon Crown Gate", "ECG", "OlirnaFeral", "Military",
                "A gate station at the last charted berth. Its collar can shut the outer crossing.")
        ];

        private sealed class FringeHost(string faction, string bramfatura, string trooper, string veteran, string specialist)
        {
            internal readonly string Faction = faction;
            internal readonly string Bramfatura = bramfatura;
            internal readonly string Trooper = trooper;
            internal readonly string Veteran = veteran;
            internal readonly string Specialist = specialist;
            internal string Table => "iom_fringe_" + Faction;
        }

        private static readonly Dictionary<string, FringeHost> FringeHosts = new FringeHost[]
        {
            new("CattaramFeral", "Cattaram", "cattaram_servitor", "cattaram_knight", "cattaram_pact_elemental"),
            new("RonFeral", "Ron", "ron_slave", "ron_shade", "ron_knight"),
            new("ScrivnusFeral", "Scrivnus", "scrivnus_servitor", "scrivnus_knight", "scrivnus_terror"),
            new("FlaurosFeral", "Flauros", "flauros_pact_zombie", "flauros_servitor", "flauros_knight"),
            new("DuggurFeral", "Duggur", "moon_slave", "moon_servitor", "moon_knight"),
            new("OlirnaFeral", "Olirna_Awakened", "olirna_monk", "olirna_imp", "olirna_mastiff"),
            new("GannixFeral", "Gannix", "venus_slave", "venus_acolyte", "venus_jaguar_knight"),
            new("UrFeral", "Ur_Awakened", "mars_support", "mars_knight", "mars_battle_gun"),
            new("ShartamakhumFeral", "Shartamakhum", "mercury_slave", "mercury_spellsword", "mercury_knight")
        }.ToDictionary(host => host.Faction);

        private static void RegisterFringeOutposts(StationRecord stationDonor, StationBarterRecord depot, StationBarterRecord forge)
        {
            int saturnNeighbour = Data.SpaceObjects.GetRecord("saturn").RootNeighbourIndex;
            foreach (FringeHost host in FringeHosts.Values)
            {
                AddUnit(host.Trooper, 1, 1, 2, 100);
                AddUnit(host.Veteran, 2, 1, 2, 175);
                AddUnit(host.Specialist, 4, 1, 1, 240);

                void AddUnit(string member, int tier, int min, int max, int points)
                {
                    if (Data.MobClasses.GetRecord(member) == null)
                    {
                        throw new InvalidOperationException("Fringe host " + host.Faction + " is missing " + member);
                    }
                    var unit = CreateUnitDrop(tier, min, max, points, leader: member, member: host.Trooper);
                    unit.AllowedFactions = [host.Faction];
                    Registrations.Add(() => Data.LocationUnitDrop.AddRecord("units_" + host.Table, unit));
                }
            }

            for (int index = 0; index < FringeOutposts.Length; index++)
            {
                FringeOutpost post = FringeOutposts[index];
                string donor = FringeBodyDonors[index % OutpostsPerOrbit];
                var body = new SpaceObjectRecord
                {
                    Id = post.Body,
                    Available = true,
                    SpaceObjectType = SpaceObjectType.Planet,
                    RootNeighbourIndex = saturnNeighbour + 1 + index / OutpostsPerOrbit,
                    ContentDescriptor = Data.SpaceObjects.GetRecord(donor).ContentDescriptor
                };
                string portraitId = "spaceobject_" + post.Body;
                StageRecord(Data.Portraits, portraitId, new ConfigTableRecord
                {
                    Id = portraitId,
                    ContentDescriptor = Data.Portraits.GetRecord("spaceobject_" + donor).ContentDescriptor
                });
                string type = post.Template switch
                {
                    "orbit_Military" => "Military",
                    "orbit_Industrial" => "Industrial",
                    "orbit_Lab" => "Lab",
                    _ => "SpaceStation"
                };
                string bramfatura = FringeHosts.TryGetValue(post.Faction, out var host) ? host.Bramfatura : "No_Bramfatura";
                RegisterStation(stationDonor, type == "Industrial" ? forge : depot, post.Id, post.Body, type,
                    receipt: null, powerGain: 0, techLevelGain: 0, uncapturable: true, missionTemplateId: post.Template,
                    bramfaturaId: bramfatura, initialPopulation: 1200, maxPopulation: 3600, captureChance: 0f,
                    newSpaceObject: body, corpReceipts: ["growth_basic"], initialOwner: post.Faction);
                Registrations.Add(() =>
                {
                    TranslationPatch.AddFringeBody(post.Body, post.Rock, post.Description);
                    TranslationPatch.AddStation(post.Id, post.Station, post.Code, "Fringe outpost", post.Description);
                });
            }
        }

        [HarmonyPatch(typeof(ItemDropSystem), nameof(ItemDropSystem.AddNewItemsAtStation))]
        private static class FringeQuartermasters
        {
            // Native feral factions have no trade stock. Ordinary stock generation resumes after a claim.
            private static bool Prefix(Station station) =>
                !station.Id.StartsWith(FringeIdPrefix, StringComparison.Ordinal) || !FringeHosts.ContainsKey(station.OwnerFactionId);
        }

        [Hook(ModHookType.AfterSpaceLoaded)]
        public static void RestoreFringeOutpostsOnLoad(IModContext context)
        {
            if (Data.Stations.GetRecord(FringeOutposts[0].Id) == null)
            {
                return;
            }

            var state = context.State;
            var stations = state.Get<Stations>();
            var factions = state.Get<Factions>();
            var factory = state.Get<StationFactory>();
            bool added = false;
            foreach (FringeOutpost post in FringeOutposts)
            {
                if (stations.Get(post.Id, logMissing: false) != null || stations.DeletedStationsIds.Contains(post.Id))
                {
                    continue;
                }
                Station station = factory.Create(post.Id);
                StationSystem.RefreshConsumableItems(state.Get<ItemsPrices>(), state.Get<MagnumProgression>(), factions, station);
                stations.Values.Add(station);
                added = true;
            }
            if (added)
            {
                var cache = state.Get<FactionCachedData>();
                CacheSystem.CacheStationNeighbours(cache, stations);
                CacheSystem.CacheStationOwners(cache, stations);
            }
        }

        [HarmonyPatch(typeof(SpaceObjects), nameof(SpaceObjects.FillWithContent))]
        private static class FringeStarmap
        {
            private static void Prefix(ref SpaceObject[] spaceObjects)
            {
                if (Data.SpaceObjects.GetRecord(FringeOutposts[0].Body) == null ||
                    spaceObjects.Any(body => body.ID == FringeOutposts[0].Body))
                {
                    return;
                }

                var container = SingletonMonoBehaviour<SpacemapContainer>.Instance;
                var asteroids = spaceObjects.Where(body => FringeBodyDonors.Contains(body.ID)).ToDictionary(body => body.ID);
                var saturn = spaceObjects.First(body => body.ID == "saturn");
                var sourceRing = saturn.OrbitRing;
                var bodies = new List<SpaceObject>(spaceObjects);
                var groups = new List<SpaceObjectGroup>(container.SpaceObjectGroups);
                var rings = new List<OrbitRing>(container.OrbitRings);
                for (int orbit = 0; orbit < FringeOrbitScales.Length; orbit++)
                {
                    float scale = FringeOrbitScales[orbit];
                    var ring = Object.Instantiate(sourceRing, sourceRing.transform.parent);
                    ring.name = "Fringe orbit " + (orbit + 1);
                    var ellipse = ring.EllipseGenerator;
                    ellipse.SetScales(sourceRing.EllipseGenerator.XYScale * scale, sourceRing.EllipseGenerator.ZScale * scale);
                    ellipse.CalculateEllipse();
                    ring._orbitalUsers = [];
                    rings.Add(ring);

                    for (int slot = 0; slot < OutpostsPerOrbit; slot++)
                    {
                        FringeOutpost post = FringeOutposts[orbit * OutpostsPerOrbit + slot];
                        var body = Object.Instantiate(asteroids[FringeBodyDonors[slot]], saturn.transform.parent);
                        body.name = post.Rock;
                        body._id = post.Body;
                        body._orbit = ring;
                        body._origXYScale = ellipse.XYScale;
                        body._origZScale = ellipse.ZScale;
                        body._relativeSpeedTarget = null;
                        body._normalizedOrbitSpeed = saturn.NormalizedSpeedPerSec / Math.Pow(scale, 1.5);
                        body.Progress = (saturn.Progress + 0.11 + slot * 0.055 + orbit * 0.025) % 1.0;
                        body._fullSizeScale *= 0.75f;
                        body._inactiveSizeScale *= 0.75f;
                        ring.AddOrbitUser(body);
                        bodies.Add(body);

                        var group = new GameObject(post.Rock + " group").AddComponent<SpaceObjectGroup>();
                        group.transform.SetParent(container.WorldRoot, worldPositionStays: false);
                        group._spaceObjects = [body];
                        group._scaleMode = SpaceObjectGroup.ScaleMode.All;
                        groups.Add(group);
                    }
                }
                container._spaceObjectGroups = groups.ToArray();
                container._orbitRings = rings.OrderByDescending(ring => ring.Priority).ToArray();
                container._spaceObjects = bodies.ToArray();
                spaceObjects = container.SpaceObjects;
            }
        }

        [HarmonyPatch(typeof(StarmapScreen), "OnEnable")]
        private static class FringeStarmapMarkers
        {
            private static void Prefix(ref UiOrbitUser[] ____orbitUsers, ref UIPlanetVisuals[] ____planetVisuals,
                TravelMetadata ____travelMetadata)
            {
                if (Data.SpaceObjects.GetRecord(FringeOutposts[0].Body) == null)
                {
                    return;
                }

                if (!____orbitUsers.Any(user => user.SpaceObjectId == FringeOutposts[0].Body))
                {
                    var nativeOrbit = ____orbitUsers.First(user => user.SpaceObjectId == "saturn")._ellipseGenerator;
                    string donorId = FringeBodyDonors[0];
                    var donor = ____orbitUsers.First(user => user.SpaceObjectId == donorId);
                    var donorVisual = ____planetVisuals.First(visual => visual.SpaceObjectId == donorId);
                    var users = new List<UiOrbitUser>(____orbitUsers);
                    var visuals = new List<UIPlanetVisuals>(____planetVisuals);
                    for (int orbit = 0; orbit < FringeOrbitScales.Length; orbit++)
                    {
                        var ellipse = new GameObject("Fringe map orbit " + (orbit + 1), typeof(RectTransform))
                            .AddComponent<UIEllipseGenerator>();
                        ellipse.transform.SetParent(nativeOrbit.transform.parent, worldPositionStays: false);
                        ellipse.XScale = nativeOrbit.XScale * FringeOrbitScales[orbit];
                        ellipse.YScale = nativeOrbit.YScale * FringeOrbitScales[orbit];
                        ellipse.EllipseSegments = nativeOrbit.EllipseSegments;
                        ellipse.CalculateEllipse();

                        for (int slot = 0; slot < OutpostsPerOrbit; slot++)
                        {
                            FringeOutpost post = FringeOutposts[orbit * OutpostsPerOrbit + slot];
                            var markerRoot = Object.Instantiate(donor.UserTransform, donor.UserTransform.parent);
                            markerRoot.name = post.Rock + " marker";
                            markerRoot.localScale *= 0.75f;
                            var marker = markerRoot.GetComponent<UiOrbitUser>() ?? markerRoot.gameObject.AddComponent<UiOrbitUser>();
                            marker._userTransform = markerRoot;
                            marker._spaceObjectId = post.Body;
                            marker._ellipseGenerator = ellipse;
                            users.Add(marker);

                            var visual = markerRoot.GetComponentInChildren<UIPlanetVisuals>(includeInactive: true);
                            if (visual == null)
                            {
                                visual = Object.Instantiate(donorVisual, markerRoot);
                            }
                            visual._spaceObjectId = post.Body;
                            if (visual._activeObjectHint != null)
                            {
                                visual._activeObjectHint._spaceObjectId = post.Body;
                            }
                            visuals.Add(visual);
                        }
                    }
                    ____orbitUsers = users.ToArray();
                    ____planetVisuals = visuals.ToArray();
                }

                foreach (var user in ____orbitUsers)
                {
                    if (user.SpaceObjectId.StartsWith(FringeIdPrefix, StringComparison.Ordinal))
                    {
                        user.gameObject.SetActive(!____travelMetadata.IsInBramfatura);
                    }
                }
            }
        }
    }
}

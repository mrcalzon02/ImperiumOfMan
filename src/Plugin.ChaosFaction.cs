using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MGSC;
using UnityEngine;
using UnityEngine.UI;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private const string ChaosAlliance = "iom_chaos_alliance";

        private sealed class ChaosSite(string id, string name, string shortName, string emblem, string body, string type,
            string template, string bramfatura, int firstTier, int lastTier, string description)
        {
            internal readonly string Id = id;
            internal readonly string Name = name;
            internal readonly string ShortName = shortName;
            internal readonly string Emblem = emblem;
            internal readonly string Body = body;
            internal readonly string Type = type;
            internal readonly string Template = template;
            internal readonly string Bramfatura = bramfatura;
            internal readonly int FirstTier = firstTier;
            internal readonly int LastTier = lastTier;
            internal readonly string Description = description;
        }

        private static readonly ChaosSite[] ChaosSites =
        [
            new("iomHollowSignalDepot", "Hollow Signal Depot", "HSD", "hollow_signal_depot", "phobos", "SpaceStation",
                "orbit_Spaceport", "Ur_Sleeping", 1, 2, "A freight relay turned cult landing point. Its receiving bays hold hides, ration stock and shard cells."),
            new("iomOssuaryWorks", "Ossuary Works", "OSW", "ossuary_works", "moon", "Industrial",
                "moon_Industrial_moon", "Duggur", 3, 4, "A lunar reclamation plant where the cult fits ribplate and binds salvaged carapace."),
            new("iomVeiledGate", "Veiled Gate", "VGT", "veiled_gate", "venus", "Military",
                "venus_Spaceport_venus", "Gannix", 5, 5, "A Venusian launch site that services Talon equipment and stocks assault gear."),
            new("iomCinderCrucible", "Cinder Crucible", "CCR", "cinder_crucible", "mercury", "Industrial",
                "orbit_Industrial", "Shartamakhum", 6, 7, "A machine works with insulated firing bays and a vault of fused fleshmetal stock."),
            new("iomChainforge", "Chainforge", "CFG", "chainforge", "io", "Industrial",
                "mercury_Industrial_io", "Flauros", 8, 8, "An Io foundry that stamps armor and feeds the siege line."),
            new(BlackMeridian, "Black Meridian", "BMD", "black_meridian", "titan", "Military",
                "orbit_Military", "Olirna_Sleeping", 9, 10, "The host's fortified command station in Titan orbit. Its reliquary houses the Shadowbound patterns.")
        ];

        private static Dictionary<string, Sprite> _chaosEmblems;
        private static bool _chaosBriefing;

        private static void RegisterChaosFaction()
        {
            var faction = ScriptableObject.CreateInstance<FactionDescriptor>();
            faction.name = "ChaosUndividedFaction";
            faction._bigIcon = ReadItemSprite("chaos_undivided_big", null);
            faction._smallIcon = ReadItemSprite("chaos_undivided_small", null);
            faction._bigVersusBeneficiary = ReadItemSprite("chaos_undivided_big_beneficiary", null);
            faction._bigVersusVictim = ReadItemSprite("chaos_undivided_big_victim", null);
            faction._smallVersusBeneficiary = ReadItemSprite("chaos_undivided_small_beneficiary", null);
            faction._smallVersusVictim = ReadItemSprite("chaos_undivided_small_victim", null);
            StageRecord(Data.Factions, ChaosFaction, new FactionRecord
            {
                Id = ChaosFaction, ContentDescriptor = faction, Enabled = true, InitialTechLevel = 1,
                InitialPower = 1400, InitialPlayerReputation = -10, FactionType = FactionType.Corp,
                AllianceType = ChaosAlliance, SpawnMissionChance = 0.12f, GuardCreatureId = ChaosFury,
                CEOGuardCreatureId = ChaosPrince, AgentCreatureId = "manager",
                UseGeneralRewards = false, CanBeTraded = true, CanBeCaptured = true,
                ItemDropCategories = [ChaosFaction]
            });
            StageDescriptor("factions", ChaosFaction, faction);
            _chaosEmblems = ChaosSites.ToDictionary(site => site.Id, site => ReadItemSprite(site.Emblem, faction.SmallIcon));

            var alliance = ScriptableObject.CreateInstance<AllianceDescriptor>();
            alliance.name = "ChaosUndividedAlliance";
            alliance._normalIcon = ReadItemSprite("chaos_undivided_alliance", null);
            alliance._hoverIcon = ReadItemSprite("chaos_undivided_alliance_hover", null);
            StageRecord(Data.Alliances, ChaosAlliance, new AllianceRecord
            {
                Id = ChaosAlliance, ContentDescriptor = alliance, DefaultFactionType = FactionType.Corp,
                AllowStrife = false, TradeFactionLists = [ChaosFaction]
            });
            StageDescriptor("alliances", ChaosAlliance, alliance);

            Registrations.Add(() => TranslationPatch.AddChaosFaction(ChaosFaction, ChaosAlliance));
        }

        private static void RegisterChaosSites(StationRecord stationDonor, StationBarterRecord depot, StationBarterRecord forge)
        {
            foreach (ChaosSite site in ChaosSites)
            {
                RegisterStation(stationDonor, site.Type == "Industrial" ? forge : depot, site.Id,
                    site.Body, site.Type, receipt: null, powerGain: 1, techLevelGain: site.FirstTier == 6 ? 2 : 1,
                    uncapturable: site.LastTier == 10, missionTemplateId: site.Template, bramfaturaId: site.Bramfatura,
                    initialPopulation: 8500, maxPopulation: 24000, initialOwner: ChaosFaction,
                    corpReceipts: ["growth_basic", "factory_military", "data_military", "use_weapon", "use_tool"]);
                Registrations.Add(() => TranslationPatch.AddStation(site.Id, site.Name, site.ShortName, site.Type, site.Description));
            }
        }

        private static void RegisterChaosGarrisons()
        {
            var garrisons = new UnitDropRecord[_chaosSupply.Length];
            foreach (ChaosSupplyTier tier in _chaosSupply)
            {
                string role = ChaosItemPrefix + "garrison_" + Math.Min(tier.Level, 9);
                if (tier.Level < 10)
                {
                    string name = tier.Level < 4 ? "Cult Reaver" : tier.Level < 7 ? "Bound Legionary" : "Soulforge Veteran";
                    RegisterEquippedRole(role, tier.Level < 4 ? "soldier" : "security", name, ChaosFaction,
                        [tier.Weapon.Id, .. tier.Armor]);
                }
                string leader = tier.Level == 1 ? null :
                    ChaosBestiary.Last(enemy => enemy.Tier == Math.Min(tier.Level, 9)).Id;
                string member = tier.Level == 1 ? ChaosFury : ChaosMutant;
                var squad = CreateUnitDrop(tier.Level, 1, tier.Level <= 2 ? 2 : 1,
                    75 + 12 * tier.Level, leader, member);
                squad.AllowedFactions = [ChaosFaction];
                garrisons[tier.Level - 1] = squad;
            }
            foreach (string type in Data.StationTypes.Ids)
            {
                var drops = Data.LocationUnitDrop.Get(type);
                if (drops != null)
                {
                    Registrations.Add(() => drops.AddRange(garrisons));
                }
            }
        }

        [Hook(ModHookType.AfterSpaceLoaded)]
        public static void AddChaosHoldingsToExistingCampaign(IModContext context)
        {
            var state = context.State;
            var factions = state.Get<Factions>();
            var stations = state.Get<Stations>();
            var factory = state.Get<StationFactory>();
            if (Data.Factions.GetRecord(ChaosFaction) == null || factions == null || stations == null || factory == null ||
                factions.Get(ChaosFaction, logMissing: false) != null)
            {
                return;
            }
            factions.Values.Add(Faction.Create(ChaosFaction));
            foreach (ChaosSite site in ChaosSites)
            {
                if (stations.Get(site.Id, logMissing: false) != null || stations.DeletedStationsIds.Contains(site.Id))
                {
                    continue;
                }
                var station = factory.Create(site.Id);
                StationSystem.RefreshConsumableItems(state.Get<ItemsPrices>(), state.Get<MagnumProgression>(), factions, station);
                stations.Values.Add(station);
            }
            var cache = state.Get<FactionCachedData>();
            CacheSystem.CacheFactionRelations(cache, factions);
            CacheSystem.CacheStationNeighbours(cache, stations);
            CacheSystem.CacheStationOwners(cache, stations);
        }

        [HarmonyPatch(typeof(SpaceStationPanel), nameof(SpaceStationPanel.RefreshStatus))]
        private static class ChaosStationEmblem
        {
            private static void Postfix(StationStatus status, Station ____station, Image ____factionIcon)
            {
                if ((status is StationStatus.Peaceful or StationStatus.LockedAfterCapture or StationStatus.NoReputationToVisit) &&
                    ____station.OwnerFactionId == ChaosFaction && _chaosEmblems != null &&
                    _chaosEmblems.TryGetValue(____station.Id, out var emblem))
                {
                    ____factionIcon.sprite = emblem;
                }
            }
        }

        [HarmonyPatch(typeof(BriefingScreen), "ShowDialog")]
        private static class ChaosBriefingScope
        {
            private static void Prefix(Mission ____mission, out bool __state)
            {
                __state = _chaosBriefing;
                _chaosBriefing = ____mission?.BeneficiaryFactionId == ChaosFaction &&
                    (!____mission.IsStoryMission || CampaignQuests.ContainsKey(____mission.StoryId));
            }

            private static Exception Finalizer(Exception __exception, bool __state)
            {
                _chaosBriefing = __state;
                return __exception;
            }
        }

        [HarmonyPatch(typeof(NarrativeTextScreen), nameof(NarrativeTextScreen.Configure))]
        private static class ChaosBriefingPortrait
        {
            [HarmonyPriority(Priority.Last)]
            private static void Prefix(ref PortraitDescriptor portrait, ref string captionTag)
            {
                if (_chaosBriefing && _chaosCommsPortrait != null)
                {
                    portrait = _chaosCommsPortrait;
                    captionTag = "faction." + ChaosFaction + ".name";
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MGSC;
using UnityEngine;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private const int CampaignMinReputation = 25;
        private static readonly Dictionary<string, CampaignQuest> CampaignQuests = new(StringComparer.Ordinal);

        private static string CampaignQuestId(string factionId, CampaignQuest quest) => factionId + "_quest_" + quest.Key;

        private static bool HasCampaignStanding(Faction faction) => faction?.PlayerReputation >= CampaignMinReputation;

        private static bool IsCampaignMission(Mission mission) =>
            mission?.IsStoryMission == true && CampaignQuests.ContainsKey(mission.StoryId);

        private static void RegisterFactionCampaigns()
        {
            RegisterCampaign(ItemCategory, "The Inner Writ", ImperialCampaign);
            RegisterCampaign(ChaosFaction, "The Starless March", UndividedCampaign);
        }

        private static void RegisterCampaign(string factionId, string title, CampaignQuest[] quests)
        {
            var weights = Data.FactionStrategies.GetRecord("Scouting").ProcMissionWeights;
            for (int index = 0; index < quests.Length; index++)
            {
                CampaignQuest quest = quests[index];
                string id = CampaignQuestId(factionId, quest);
                int level = 1 + index / 2;
                string reward = quest.Reward ?? (factionId == ChaosFaction
                    ? (index % 2 == 0 ? _chaosSupply[level - 1].Utility.Id : _chaosSupply[level - 1].Chip)
                    : level <= 3 ? "low_chip" : level <= 7 ? "medium_chip" : "high_chip");
                string progress = $"{title} ({index + 1}/{quests.Length})";
                bool finale = index == quests.Length - 1;
                RegisterCampaignMission(factionId, id, quest, level, reward, progress, finale);

                List<PerkParameter> finish = [PerkParameter.Parse("SFinishMission " + id)];
                if (CampaignRoutes.TryGetValue(id, out var route))
                {
                    string alternate = CampaignRouteId(id);
                    RegisterCampaignMission(factionId, alternate, route.Quest, level, reward, progress, finale);
                    finish = [PerkParameter.Parse("SFinishMissionAny " + id), PerkParameter.Parse("SFinishMissionAny " + alternate)];
                }
                StageRecord(Data.FactionStrategies, id, new FactionStrategyRecord
                {
                    Id = id, ProcMissionWeights = [.. weights],
                    WinConditions = finish,
                    FailConditions = [], InstantFailConditions = [], RunMissionsOnStart = [], RunMissionsOnSuccess = []
                });
                StageRecord(Data.FactionQuestlines, id, new FactionQuestlineRecord
                {
                    Id = id, FactionId = factionId, Strategies = [new(100f, id)], NextStepOnFail = string.Empty
                });
            }

            string loopId = factionId + FactionQuestlineRecord.ENDGAME_LOOP_POSTFIX;
            StageRecord(Data.FactionQuestlines, loopId, new FactionQuestlineRecord
            {
                Id = loopId, FactionId = factionId, NextStepOnFail = string.Empty,
                Strategies = [new(100f, "Expansion"), new(60f, "Scouting"), new(45f, "Defense")]
            });
        }

        private static void RegisterCampaignMission(string factionId, string id, CampaignQuest quest, int level,
            string reward, string progress, bool finale)
        {
            if (quest.Objective == CampaignObjective.Recover)
            {
                var evidence = CloneItem<TrashRecord>(id + "_evidence", "quest_fresco_data", level, 0, CampaignEvidenceArtwork(id, alternate: false));
                evidence.Categories = [];
                RegisterItem("trash", evidence, quest.Target, "An objective item to deliver to the mission sponsor.");
            }
            if ((quest.Objective is CampaignObjective.Hunt or CampaignObjective.Escort) &&
                Data.MobClasses.GetRecord(quest.Target) == null && quest.Rival != ChaosFaction)
            {
                throw new InvalidOperationException("Unknown campaign target " + quest.Target + " for " + id);
            }

            var (condition, parameters) = quest.Objective switch
            {
                CampaignObjective.Clear => (WinCondition.KillAllPercent, new List<string> { "0.65" }),
                CampaignObjective.Recover => (WinCondition.ItemInInventoryById, [id + "_evidence", "1"]),
                CampaignObjective.Sabotage => (WinCondition.KillObstacle, [quest.Target, "1"]),
                CampaignObjective.Hunt => (WinCondition.KillMonster, [quest.Target, "1"]),
                CampaignObjective.Escort => (WinCondition.EscortVIP, [quest.Target]),
                CampaignObjective.Control => (WinCondition.DefenseObstacles, [quest.Target, "1"]),
                _ => throw new ArgumentOutOfRangeException(nameof(quest.Objective))
            };
            StageRecord(Data.StoryMissions, id, new StoryMissionRecord
            {
                Id = id, StationId = quest.Station, BeneficiaryFactionId = factionId, VictimFactionId = quest.Rival,
                QuestlineOwnerId = factionId, PrizeItems = [reward, level <= 3 ? "medical_kit_1" : "medical_kit_2"],
                WinCondition = condition, WinConditionParameters = parameters,
                AddTriggersOnCompletion = finale ? [factionId + "_campaign_complete"] : [],
                MinTechLevel = level, ShownDifficulty = level, FactionIdsByStages = [],
                ItemsPointsByStages = [], MonsterPointsByStages = [], ItemsRequirement = [], ItemsRequirementAny = []
            });

            CampaignEndings.TryGetValue(id, out var ending);
            string access = RegisterCampaignAccess(id, level, quest.Objective);
            if (ending != null)
            {
                var evidence = CloneItem<TrashRecord>(CampaignEndingItem(id), "quest_fresco_data", level, 0, CampaignEvidenceArtwork(id, alternate: true));
                evidence.Categories = [];
                RegisterItem("trash", evidence, ending.ItemName, "Pick up to commit to the alternate objective. Extract to confirm the outcome.");
            }
            string strategy = $"{progress} · {quest.Name}. Requires +{CampaignMinReputation} faction reputation.";
            Registrations.Add(() =>
            {
                CampaignQuests.Add(id, quest);
                string task = string.Join(" ", new[] { quest.Task, access, ending?.Instructions }.Where(text => text != null));
                string briefing = string.Join("\n\n", new[] { quest.Briefing, access, ending?.Instructions }.Where(text => text != null));
                TranslationPatch.AddCampaignQuest(id, quest.Name, briefing, task, quest.Report, strategy, ending?.Report);
                TranslationPatch.AddCampaignDeckNames(id, quest.Decks.Entry, quest.Decks.Middle, quest.Decks.Final);
            });
        }

        [Hook(ModHookType.SpaceUpdateAfterGameLoop)]
        public static void OfferFactionCampaignQuests(IModContext context)
        {
            if (CampaignQuests.Count == 0)
            {
                return;
            }

            var state = context.State;
            var factions = state.Get<Factions>();
            var missions = state.Get<Missions>();
            var stations = state.Get<Stations>();
            var travel = state.Get<TravelMetadata>();
            var triggers = state.Get<StoryTriggers>();
            var time = state.Get<SpaceTime>();
            var factory = state.Get<MissionFactory>();
            Offer(ItemCategory, ImperialCampaign);
            Offer(ChaosFaction, UndividedCampaign);

            void Offer(string factionId, CampaignQuest[] quests)
            {
                Faction faction = factions.Get(factionId, logMissing: false);
                if (faction == null || string.IsNullOrEmpty(faction.QuestlineId) || !factions.IsEnabledFaction(faction) ||
                    travel.IsInBramfatura)
                {
                    return;
                }

                // Earlier versions started both factions directly in their ordinary contract loops.
                string migration = factionId + "_campaign_initialized";
                if (!triggers.IsPassed(migration))
                {
                    if (faction.QuestlineId == factionId + FactionQuestlineRecord.ENDGAME_LOOP_POSTFIX)
                    {
                        var next = Array.Find(quests, quest => !CampaignStepFinished(triggers, CampaignQuestId(factionId, quest)));
                        if (next != null)
                        {
                            QuestlineSystem.JumpToQuestline(time, factions, missions, travel, factory, triggers, stations,
                                state.Get<MagnumProgression>(), state.Get<MagnumCargo>(), state.Get<Mercenaries>(),
                                faction, CampaignQuestId(factionId, next));
                            if (UI.IsShowing<SpaceHudScreen>())
                            {
                                UI.Get<SpaceHudScreen>().Questlines.Refresh();
                            }
                        }
                    }
                    triggers.Pass(migration);
                }

                string missionId = ResolveCampaignMission(triggers, faction.QuestlineId);
                if (!CampaignQuests.TryGetValue(missionId, out var current) || CampaignStepFinished(triggers, faction.QuestlineId))
                {
                    return;
                }

                bool hasStanding = HasCampaignStanding(faction);
                var active = missions.Get(current.Station, reversed: false);
                if (active?.IsStoryMission == true)
                {
                    if (active.StoryId == missionId || active.StoryId == faction.QuestlineId)
                    {
                        active.IsBlocked = !hasStanding;
                        EnsureCampaignEndingItem(active);
                        ApplyCampaignDecks(active);
                    }
                    return;
                }

                var station = stations.Get(current.Station, logMissing: false);
                if (!hasStanding || station == null)
                {
                    return;
                }

                // Give the authored quest the station's normal contract slot and leave other story missions alone.
                var record = Data.StoryMissions.GetRecord(missionId);
                var mission = factory.CreateStoryMission(record);
                if (active != null)
                {
                    MissionSystem.RemoveMission(missions, current.Station);
                }
                missions.Values.Add(mission);
                if (travel.CurrentSpaceObject == station.SpaceObjectId &&
                    !travel.IsInTravel && UI.IsShowing<SpaceHudScreen>())
                {
                    UI.Get<SpaceHudScreen>().RefreshUIOnArrival(travel.CurrentSpaceObject);
                }
            }
        }

        [HarmonyPatch(typeof(MissionSystem), nameof(MissionSystem.HandleBlockedMission))]
        private static class ExplainCampaignStanding
        {
            private static bool Prefix(Mission mission)
            {
                if (!IsCampaignMission(mission))
                {
                    return true;
                }

                string sponsor = Localization.Get("faction." + mission.BeneficiaryFactionId + ".name");
                string message = string.Format(Localization.Get(TranslationPatch.CampaignStandingRequired), CampaignMinReputation, sponsor);
                UI.Chain<AlertDialogWindow>().Invoke(window => window.Configure(message)).Show();
                return false;
            }
        }

        [HarmonyPatch(typeof(SpaceGameMode), "StartMission")]
        private static class RequireCampaignStanding
        {
            private static bool Prefix(Mission mission, State ____state, ref bool ____finished)
            {
                if (!IsCampaignMission(mission))
                {
                    return true;
                }

                var sponsor = ____state.Get<Factions>().Get(mission.BeneficiaryFactionId, logMissing: false);
                mission.IsBlocked = !HasCampaignStanding(sponsor);
                if (!mission.IsBlocked)
                {
                    return true;
                }

                // A briefing can already be open when standing changes. Return to the space HUD without ending space mode.
                ____finished = false;
                UI.BackToDefault();
                MissionSystem.HandleBlockedMission(mission);
                return false;
            }
        }

        [HarmonyPatch(typeof(MissionFactory), nameof(MissionFactory.CreateStoryMission))]
        private static class BuildCampaignRaid
        {
            private static bool Prefix(MissionFactory __instance, State ____state, StoryMissionRecord missionRecord,
                ref Mission __result)
            {
                if (!CampaignQuests.TryGetValue(missionRecord.Id, out var quest))
                {
                    return true;
                }

                var station = ____state.Get<Stations>().Get(quest.Station);
                var factions = ____state.Get<Factions>();
                var sponsor = factions.Get(missionRecord.BeneficiaryFactionId);
                var opponent = factions.Get(quest.Rival);
                var time = ____state.Get<SpaceTime>().Time;
                bool fringe = sponsor.Id == ChaosFaction;
                int tier = missionRecord.MinTechLevel;
                var mission = new Mission
                {
                    IsStoryMission = true, StoryId = missionRecord.Id, StationId = station.Id,
                    BeneficiaryFactionId = sponsor.Id, VictimFactionId = opponent.Id,
                    CreationTime = time, ExpireTime = time.AddYears(100),
                    BramfaturaId = station.BramfaturaId, StagesNameId = missionRecord.Id,
                    MinTechLevel = tier, MissionDifficulty = Math.Min(fringe ? 3 : 4, (tier + 2) / 3),
                    WorldStructure = new GameWorldStructure(),
                    ProcMissionType = quest.Objective switch
                    {
                        CampaignObjective.Clear => ProceduralMissionType.RaiderCapture,
                        CampaignObjective.Recover => ProceduralMissionType.Espionage,
                        CampaignObjective.Sabotage => ProceduralMissionType.Sabotage,
                        CampaignObjective.Escort => ProceduralMissionType.Escort,
                        CampaignObjective.Control => ProceduralMissionType.Control,
                        _ => ProceduralMissionType.Elimination
                    }
                };
                mission.WinCondition.Init(missionRecord);
                string units = station.Record.StationType;
                if (fringe && FringeHosts.TryGetValue(opponent.Id, out var host))
                {
                    units = host.Table;
                    mission.BramfaturaId = host.Bramfatura;
                }

                // The native generator builds the same room objectives, elevators, and item drops as ordinary contracts.
                var template = Data.ProcMissionTemplates.GetRecord(station.Record.MissionTemplateId);
                __instance.GenerateProceduralStructure(mission, template, sponsor, opponent.Id, units);
                var preset = ____state.Get<Difficulty>().Preset;
                foreach (var plan in mission.LocationPlans)
                {
                    if (plan.Key.StartsWith("stage", StringComparison.Ordinal))
                    {
                        plan.Value.ItemsPointsLimit = Mathf.RoundToInt((160 + tier * 55) * preset.ItemPoints);
                        plan.Value.MonstersPointsLimit = Mathf.RoundToInt((360 + tier * 110) * preset.MonsterPoints);
                    }
                }
                EnsureCampaignEndingItem(mission);
                ApplyCampaignDecks(mission);
                foreach (string reward in missionRecord.PrizeItems)
                {
                    MissionSystem.AddReward(SingletonMonoBehaviour<ItemFactory>.Instance, mission, reward);
                }
                __instance.InitReputation(mission, opponent);
                __result = mission;
                return false;
            }
        }

        [HarmonyPatch(typeof(FormatHelper), nameof(FormatHelper.GetLocationCaptionTag))]
        private static class CampaignLocationCaptions
        {
            private static void Postfix(RaidMetadata raidMetadata, Missions missions, string locationCustomId, ref string __result)
            {
                if (raidMetadata == null) return;
                if (raidMetadata.StationVisit)
                {
                    // Native station decks may use a donor caption. Preserve a
                    // valid deck name and use the actual custom station only if
                    // that caption is absent.
                    UseCustomStationCaptionIfMissing(raidMetadata, ref __result);
                    return;
                }

                // Resolve by story ID so an in-progress raid with an old floor template uses the same captions.
                var mission = missions?.Get(raidMetadata);
                if (!IsCampaignMission(mission))
                {
                    UseCustomStationCaptionIfMissing(raidMetadata, ref __result);
                    return;
                }
                if (locationCustomId == "arrival")
                {
                    __result = TranslationPatch.ArrivalShuttleCaption;
                    return;
                }

                string deck = locationCustomId == mission.WorldStructure.FindLastStage()?.ID ? "final"
                    : locationCustomId == "stage1" ? "entry" : locationCustomId;
                string key = "story." + mission.StoryId + ".deck.";
                string caption = key + deck + ".name";
                __result = Localization.HasKey(caption) ? caption : key + "middle.name";
            }

            private static void UseCustomStationCaptionIfMissing(RaidMetadata raidMetadata, ref string caption)
            {
                if (string.IsNullOrEmpty(raidMetadata.StationId) ||
                    !raidMetadata.StationId.StartsWith("iom", StringComparison.Ordinal) ||
                    (!string.IsNullOrEmpty(caption) && Localization.HasKey(caption))) return;
                string stationTag = "station." + raidMetadata.StationId + ".name";
                if (Localization.HasKey(stationTag)) caption = stationTag;
            }
        }

        [HarmonyPatch(typeof(StorySystem), nameof(StorySystem.MissionFinished))]
        private static class ClaimCampaignStation
        {
            private static void Postfix(Stations stations, Factions factions, SpaceTime spaceTime, TravelMetadata travelMetadata,
                FactionCachedData factionCachedData, StoryTriggers storyTriggers, RaidMetadata raidMetadata, Mission mission, bool success)
            {
                if (!success || mission?.IsStoryMission != true ||
                    !CampaignQuests.TryGetValue(mission.StoryId, out var quest))
                {
                    return;
                }

                bool alternate = HasRaidCampaignEnding(raidMetadata, mission);
                if (alternate)
                {
                    storyTriggers.Pass(CampaignEndingTrigger(mission.StoryId));
                }
                if (!quest.Claim || (CampaignEndings.TryGetValue(mission.StoryId, out var ending) && ending.PreserveStation &&
                    storyTriggers.IsPassed(CampaignEndingTrigger(mission.StoryId))))
                {
                    return;
                }
                var station = stations.Get(mission.StationId, logMissing: false);
                if (station != null && station.OwnerFactionId != mission.BeneficiaryFactionId)
                {
                    StationSystem.CaptureStation(stations, spaceTime, travelMetadata, factions, station.Id, mission.BeneficiaryFactionId);
                    CacheSystem.CacheStationOwners(factionCachedData, stations);
                }
            }
        }
    }
}

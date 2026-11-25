using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using MGSC;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private const string BreadAndSeal = ItemCategory + "_quest_bread_and_seal";
        private const string HollowSignal = ItemCategory + "_quest_hollow_signal";
        private const string MuteTheBuoy = ChaosFaction + "_quest_mute_the_buoy";
        private const string UnmakeTheSeal = ChaosFaction + "_quest_unmake_the_seal";

        private sealed class CampaignEnding(string itemName, string instructions, string report,
            bool preserveStation = false, bool preserveRivalStanding = false)
        {
            internal readonly string ItemName = itemName;
            internal readonly string Instructions = instructions;
            internal readonly string Report = report;
            internal readonly bool PreserveStation = preserveStation;
            internal readonly bool PreserveRivalStanding = preserveRivalStanding;
        }

        private sealed class CampaignRoute(string precedingMission, CampaignQuest quest)
        {
            internal readonly string PrecedingMission = precedingMission;
            internal readonly CampaignQuest Quest = quest;
        }

        private static readonly Dictionary<string, CampaignEnding> CampaignEndings = new(StringComparer.Ordinal)
        {
            [BreadAndSeal] = new("Signed Harvest Charter",
                "Pick up the Signed Harvest Charter before destroying the server to transmit it to the Palace. Greenline stays under local control on a tithe agreement. The Junction foreman will need an escort.",
                "The Palace has accepted the charter. Greenline stays under local control and will honor the tithe. Junction's foreman has offered to sign his own agreement if you get him through the yard alive.",
                preserveStation: true, preserveRivalStanding: true),
            [HollowSignal] = new("Spoofing Key",
                "Pick up the Spoofing Key before destroying the beacon to send the convoy a verified frequency. Leave the station under its current owner so Imperial listeners can keep tracking the false signal.",
                "The convoy has its verified frequency. The beacon is still broadcasting, and Imperial listeners can follow it back to the cult. Hollow Signal remains under local control.",
                preserveStation: true),
            [MuteTheBuoy] = new("Shadow Lane Chart",
                "Pick up the Shadow Lane Chart before destroying the scanner to take an unmonitored route. Leave the Belt's relay working and keep your standing with the Belt.",
                "Our ships have the shadow lane. Blackglass can keep selling its usual tracks. The Belt has no reason to know we passed.",
                preserveRivalStanding: true),
            [UnmakeTheSeal] = new("Gatekeeper's Binding",
                "Pick up the Gatekeeper's Binding before breaking the reliquary to open the service approach. The final assault will hold the gate controls while Olirna counterattacks.",
                "The Binding has opened the service approach. Bring a crew that can hold the gate controls. Olirna will send a counterattack as soon as we reach them.")
        };

        private static readonly Dictionary<string, CampaignRoute> CampaignRoutes = new(StringComparer.Ordinal)
        {
            [ItemCategory + "_quest_yard_ledgers"] = new(BreadAndSeal,
                new("yard_ledgers", "The Foreman's Passage", JunctionReclamation, "PlanetBridge", CampaignObjective.Escort,
                    ("Junction Freight Yard", "Tool Workshops", "Signing Bunker"),
                    "The Greenline charter set a precedent. Junction's foreman will hand over the yard in person, but a PlanetBridge security crew has cut off his route to the signing bunker.",
                    "Escort the Junction foreman to the final-floor bunker, then extract.",
                    "The foreman signed the handover. Junction now answers to the Palace and the convoy can depart for Fuller.",
                    "manager", claim: true)),
            [ChaosFaction + "_quest_crown_without_sun"] = new(UnmakeTheSeal,
                new("crown_without_sun", "Hold the Ebon Crown", "iomFringeEbonCrown", "OlirnaFeral", CampaignObjective.Control,
                    ("Service Sallyport", "Gatehouse Passages", "Ebon Crown Gatehouse"),
                    "The service approach bypasses the baron's guard. Use it to reach the gate controls, then keep them intact until Olirna's counterattack breaks.",
                    "Find the marked gate controls, hold them through the counterattack, and extract to claim the Crown.",
                    "The controls answer to the Undivided. Olirna's baron lost the crossing without ever drawing us onto his deck.",
                    Reliquary, claim: true))
        };

        private static string CampaignEndingItem(string id) => id + "_alternate_evidence";
        private static string CampaignEndingTrigger(string id) => id + "_alternate_ending";
        private static string CampaignRouteId(string id) => id + "_alternate_route";

        private static string ResolveCampaignMission(StoryTriggers triggers, string step) =>
            CampaignRoutes.TryGetValue(step, out var route) && triggers.IsPassed(CampaignEndingTrigger(route.PrecedingMission))
                ? CampaignRouteId(step) : step;

        private static bool CampaignStepFinished(StoryTriggers triggers, string step) =>
            triggers.FinishedStoryMissions.Contains(step) || (CampaignRoutes.ContainsKey(step) &&
                triggers.FinishedStoryMissions.Contains(CampaignRouteId(step)));

        private static void EnsureCampaignEndingItem(Mission mission)
        {
            if (CampaignEndings.ContainsKey(mission.StoryId) && mission.LocationPlans.TryGetValue("stage1", out var plan))
            {
                string item = CampaignEndingItem(mission.StoryId);
                if (!plan.AdditionalItemIdsDrop.Contains(item))
                {
                    plan.AdditionalItemIdsDrop.Add(item);
                }
            }
        }

        private static bool HasRaidCampaignEnding(RaidMetadata raid, Mission mission) =>
            mission?.IsStoryMission == true && raid.RaidType == RaidType.StoryMission && raid.StationId == mission.StationId &&
            raid.CustomCounters.ContainsKey(CampaignEndingTrigger(mission.StoryId));

        [Hook(ModHookType.DungeonUpdateAfterGameLoop)]
        public static void CheckCampaignEndingEvidence(IModContext context)
        {
            var state = context.State;
            var raid = state.Get<RaidMetadata>();
            if (raid.RaidType != RaidType.StoryMission || raid.WinCondition.WinCondition != WinCondition.KillObstacle ||
                raid.WinCondition.WinConditionsPassed)
            {
                return;
            }
            var mission = state.Get<Missions>().Get(raid);
            if (mission?.IsStoryMission != true || !CampaignEndings.ContainsKey(mission.StoryId))
            {
                return;
            }
            var player = state.Get<Creatures>().Player;
            if (player == null || player.CreatureData.Health.Dead)
            {
                return;
            }
            string item = CampaignEndingItem(mission.StoryId);
            var inventory = player.CreatureData.Inventory;
            if (inventory.GetSpecificItemCount(item) > 0)
            {
                // Transmitting the evidence commits the raid to this result. The campaign only records it on extraction.
                inventory.RemoveSpecificItem(item, 1);
                raid.CustomCounters[CampaignEndingTrigger(mission.StoryId)] = 1;
                raid.WinCondition.SetWinConditionPassed(true);
            }
        }

        [HarmonyPatch(typeof(MissionSystem), nameof(MissionSystem.ProcessFinishedDungeonData))]
        private static class ApplyCampaignEndingReputation
        {
            private static void Prefix(Missions missions, RaidMetadata raidMetadata, DungeonFinishedData finishedData)
            {
                if (finishedData.Reason != GameFinishedReason.MissionFinished || !raidMetadata.WinCondition.WinConditionsPassed)
                {
                    return;
                }
                var mission = missions.Get(raidMetadata);
                if (HasRaidCampaignEnding(raidMetadata, mission) && CampaignEndings[mission.StoryId].PreserveRivalStanding)
                {
                    mission.VictimReputationDelta = 0;
                }
            }
        }

        // The native scenarios provide VIP movement, objective markers, defense waves, and save state.
        [HarmonyPatch(typeof(ScenarioRunner), nameof(ScenarioRunner.RunDungeonScenarios))]
        private static class RunNativeCampaignScenarios
        {
            private static void Prefix(ref string storyId)
            {
                if (!CampaignQuests.TryGetValue(storyId, out var quest))
                {
                    return;
                }
                storyId = quest.Objective switch
                {
                    CampaignObjective.Escort => "ProceduralMission_Escort",
                    CampaignObjective.Control => "ProceduralMission_Control",
                    _ => storyId
                };
            }
        }

        [HarmonyPatch(typeof(SpawnSystem), nameof(SpawnSystem.SpawnMonsters),
            [typeof(Mercenaries), typeof(Statistics), typeof(Difficulty), typeof(TurnController),
                typeof(DungeonGeneratedDebugData), typeof(MapGrid), typeof(Creatures), typeof(RaidMetadata), typeof(PerkFactory),
                typeof(string), typeof(Faction), typeof(Mission), typeof(float), typeof(int), typeof(bool), typeof(string)])]
        private static class UseFringeCounterattackGarrison
        {
            private static void Prefix(Mission mission, Faction faction, ref string biomeId)
            {
                if (mission?.IsStoryMission == true && CampaignQuests.TryGetValue(mission.StoryId, out var quest) &&
                    quest.Objective == CampaignObjective.Control && FringeHosts.TryGetValue(faction.Id, out var host))
                {
                    biomeId = host.Table;
                }
            }
        }

        private static string GetCampaignReport(string key, bool warnIfMissingTag, StoryTriggers triggers)
        {
            const string prefix = "story.";
            const string suffix = ".after";
            if (key != null && triggers != null && key.StartsWith(prefix, StringComparison.Ordinal) && key.EndsWith(suffix, StringComparison.Ordinal))
            {
                string id = key.Substring(prefix.Length, key.Length - prefix.Length - suffix.Length);
                if (CampaignEndings.ContainsKey(id) && triggers.IsPassed(CampaignEndingTrigger(id)))
                {
                    key += "_alternate";
                }
            }
            return Localization.Get(key, warnIfMissingTag);
        }

        [HarmonyPatch]
        private static class ShowCampaignEndingReport
        {
            private static MethodBase TargetMethod() => AccessTools.EnumeratorMoveNext(
                AccessTools.Method(typeof(AfterRaidBriefingScreen), "ProcessShowBriefing"));

            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original)
            {
                var translate = AccessTools.Method(typeof(Localization), nameof(Localization.Get), [typeof(string), typeof(bool)]);
                var translateReport = AccessTools.Method(typeof(Plugin), nameof(GetCampaignReport));
                var owner = AccessTools.GetDeclaredFields(original.DeclaringType).Single(field => field.FieldType == typeof(AfterRaidBriefingScreen));
                var triggers = AccessTools.Field(typeof(AfterRaidBriefingScreen), "_storyTriggers");
                foreach (var instruction in instructions)
                {
                    if (!instruction.Calls(translate))
                    {
                        yield return instruction;
                        continue;
                    }
                    var loadScreen = new CodeInstruction(OpCodes.Ldarg_0).MoveLabelsFrom(instruction).MoveBlocksFrom(instruction);
                    yield return loadScreen;
                    yield return new CodeInstruction(OpCodes.Ldfld, owner);
                    yield return new CodeInstruction(OpCodes.Ldfld, triggers);
                    yield return new CodeInstruction(OpCodes.Call, translateReport);
                }
            }
        }
    }
}

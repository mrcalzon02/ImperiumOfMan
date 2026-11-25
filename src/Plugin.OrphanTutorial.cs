using HarmonyLib;
using MGSC;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private const string OrphanStation = "Orphan";
        private const string OrphanTutorial = "tutorial_1_defense";

        [HarmonyPatch(typeof(MissionSystem), nameof(MissionSystem.CreateMission),
            new[] { typeof(MissionFactory), typeof(Missions), typeof(TravelMetadata), typeof(StoryMissionRecord), typeof(Stations) })]
        private static class ReuseOrphanTutorial
        {
            private static bool Prefix(Missions missions, StoryMissionRecord missionRecord, ref Mission __result)
            {
                if (_industryReady && missionRecord.StationId == OrphanStation && missionRecord.Id == OrphanTutorial)
                {
                    __result = KeepOrphanTutorial(missions);
                    return __result == null;
                }
                return true;
            }
        }

        [Hook(ModHookType.AfterSaveLoaded)]
        public static void RepairOrphanTutorialOnLoad(IModContext context)
        {
            if (!_industryReady)
            {
                return;
            }

            var missions = context.State.Get<Missions>();
            var triggers = context.State.Get<StoryTriggers>();
            if (triggers.FinishedStoryMissions.Contains(OrphanTutorial) || triggers.IsPassed("Finish_Tutorial"))
            {
                missions.Values.RemoveAll(IsOrphanTutorial);
            }
            else
            {
                KeepOrphanTutorial(missions);
            }
        }

        [Hook(ModHookType.DungeonStarted)]
        public static void RepairOrphanTutorialInRaid(IModContext context)
        {
            if (_industryReady && context.State.Get<RaidMetadata>().StationId == OrphanStation)
            {
                KeepOrphanTutorial(context.State.Get<Missions>());
            }
        }

        private static Mission KeepOrphanTutorial(Missions missions)
        {
            int first = missions.Values.FindIndex(IsOrphanTutorial);
            if (first < 0)
            {
                return null;
            }

            // Keep the first mission because the tutorial already tracks its counters and rewards.
            for (int index = missions.Values.Count - 1; index > first; index--)
            {
                if (IsOrphanTutorial(missions.Values[index]))
                {
                    missions.Values.RemoveAt(index);
                }
            }
            return missions.Values[first];
        }

        private static bool IsOrphanTutorial(Mission mission) =>
            mission.IsStoryMission && mission.StationId == OrphanStation && mission.StoryId == OrphanTutorial;
    }
}

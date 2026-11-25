using System;
using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
using MGSC;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private const string CampaignAccessSuffix = "_access_component";
        private const string CampaignAccessInstalledSuffix = "_access_installed";

        private static readonly Dictionary<string, string> CampaignAccessNames = new(StringComparer.Ordinal)
        {
            ["caloris_levy"] = "Ore-Hoist Contactor",
            ["goethe_deadlock"] = "Gun-Deck Authorization Key",
            ["extinguish_crucible"] = "Forge Governor Shunt",
            ["hollow_signal"] = "Frequency Synchronizer",
            ["cinder_basin"] = "Lift Drum Spindle",
            ["prime_the_foundry"] = "Forge Interlock Coupler",
            ["one_standard"] = "Dispatch Writ Seal",
            ["white_frequency"] = "Choir Resonance Plug",
            ["scales_of_passage"] = "Tollkeeper's Totem",
            ["furnace_of_ur"] = "Engine Keeper",
            ["the_right_to_pass"] = "Berth Command Tab",
            ["starless_oath"] = "Pact Lock Stone",
            ["hound_road"] = "Kennel Latch Pack",
            ["unmake_the_seal"] = "Seal-Key Loop",
            ["crown_without_sun"] = "Baron's Signet Key",
            ["crown_without_sun_alternate_route"] = "Gatehouse Control Fuse"
        };

        private static bool TryGetCampaignAccess(string id, out string name)
        {
            name = null;
            return CampaignDecks.TryGetValue(id, out var deck) && CampaignAccessNames.TryGetValue(deck.Key, out name);
        }

        private static string RegisterCampaignAccess(string id, int tier, CampaignObjective objective)
        {
            if (!TryGetCampaignAccess(id, out string name)) return null;

            string itemId = id + CampaignAccessSuffix;
            var item = CloneItem<TrashRecord>(itemId, "quest_fresco_data", tier, 0, "CampaignObject" + CampaignDecks[id].Tile);
            item.Categories = [];
            item.MaxStack = 1;
            string use = objective == CampaignObjective.Control ? "activate the gate controls" : "unlock the next deck's lift";
            RegisterItem("trash", item, name, "A mission component fitted as soon as it is picked up.");
            return $"Find the {name} in a side room on the first deck to {use}.";
        }

        private static bool CampaignAccessLocked(RaidMetadata raid, Mission mission) =>
            raid.RaidType == RaidType.StoryMission && mission?.IsStoryMission == true && TryGetCampaignAccess(mission.StoryId, out _) &&
            mission.LocationPlans.TryGetValue("stage1", out var approach) &&
            approach.AdditionalItemIdsDrop.Contains(mission.StoryId + CampaignAccessSuffix) &&
            !raid.CustomCounters.ContainsKey(mission.StoryId + CampaignAccessInstalledSuffix);

        [Hook(ModHookType.DungeonUpdateAfterGameLoop)]
        public static void InstallCampaignAccessOnPickup(IModContext context)
        {
            var state = context.State;
            var raid = state.Get<RaidMetadata>();
            if (raid.RaidType != RaidType.StoryMission) return;

            var mission = state.Get<Missions>().Get(raid);
            if (!CampaignAccessLocked(raid, mission)) return;

            var player = state.Get<Creatures>().Player;
            if (player == null || player.CreatureData.Health.Dead) return;

            var inventory = player.CreatureData.Inventory;
            string id = mission.StoryId + CampaignAccessSuffix;
            if (inventory.GetSpecificItemCount(id) == 0) return;

            inventory.RemoveSpecificItem(id, 1);
            raid.CustomCounters[mission.StoryId + CampaignAccessInstalledSuffix] = 1;
        }

        [HarmonyPatch(typeof(ElevatorWindow), "PanelGoToLocation")]
        private static class RequireCampaignLiftAccess
        {
            private static bool Prefix(string obj, RaidMetadata ____raidMetadata, LocationMetadata ____locationMetadata, Missions ____missions)
            {
                if (____locationMetadata.LocationId != "stage1" || obj == "stage1" || ____raidMetadata.WinCondition.WinConditionsPassed)
                    return true;

                var mission = ____missions.Get(____raidMetadata);
                if (!CampaignAccessLocked(____raidMetadata, mission)) return true;

                string item = Localization.Get("item." + mission.StoryId + CampaignAccessSuffix + ".name");
                string message = string.Format(Localization.Get(TranslationPatch.CampaignLiftLocked), item);
                UI.Chain<AlertDialogWindow>().Invoke(window => window.Configure(message)).Show();
                return false;
            }
        }

        [HarmonyPatch(typeof(Control_stage1), "DefenseLoop")]
        private static class RequireCampaignControlAccess
        {
            private static void Postfix(ref IEnumerator __result, RaidMetadata ____raidMetadata, Missions ____missions)
            {
                var mission = ____missions.Get(____raidMetadata);
                if (CampaignAccessLocked(____raidMetadata, mission))
                    __result = WaitForControlFuse(__result, ____raidMetadata, mission);
            }

            private static IEnumerator WaitForControlFuse(IEnumerator nativeDefense, RaidMetadata raid, Mission mission)
            {
                while (CampaignAccessLocked(raid, mission)) yield return null;
                while (nativeDefense.MoveNext()) yield return nativeDefense.Current;
            }
        }
    }
}

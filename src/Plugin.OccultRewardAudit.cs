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
        // Observe native mission generation and successful completion only.
        // Do not inject rewards, change the roll, spend points, or grant knowledge.
        private static bool IsRegionalOccultReward(string id)
        {
            if (string.IsNullOrEmpty(id)) return false;
            if (IsOccultPatternChip(id)) return true;
            return RegionalOccultSeeds.Any(seed => seed.Item == id && ActiveRegionalAmmoChains.Contains(seed.Chain));
        }

        private static Dictionary<string, int> SelectedRegionalRewards(Mission mission) =>
            mission.RewardItems.Where(item => item != null && IsRegionalOccultReward(item.Id))
                .GroupBy(item => item.Id, StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.Sum(item => (int)item.StackCount),
                    StringComparer.Ordinal);

        private static string OccultMissionAuditKey(Mission mission) =>
            mission.BeneficiaryFactionId + "/" + mission.StationId + "/" +
            mission.CreationTime.ToString("O");

        [HarmonyPatch(typeof(MissionFactory), "InitReward")]
        private static class TraceRegionalOccultContractOffer
        {
            private static void Postfix(Mission mission, Faction beneficiary)
            {
                if (!_industryReady || mission == null || beneficiary == null || mission.IsStoryMission)
                    return;
                try
                {
                    var selected = SelectedRegionalRewards(mission);
                    bool regionalIssuer = RegionalOccultPatternFactions.Values.Any(faction =>
                        faction.Id == beneficiary.Id);
                    if (!regionalIssuer && selected.Count == 0) return;

                    var chips = Data.FactionDrop.GetTradeItems(beneficiary,
                        FactionDropCollection.TradeItemsFor.Chips);
                    int eligible = chips.SelectMany(record => record.ContentIds)
                        .Where(IsOccultPatternChip).Distinct(StringComparer.Ordinal).Count();
                    int cap = Data.FactionDrop.GetTechLevelLimit(beneficiary);
                    Debug.Log("[Cvar_ImperiumOfMan] Occult contract offer | mission=" +
                        OccultMissionAuditKey(mission) + " | tech=" + beneficiary.CurrentTechLevel +
                        " | reputation=" + beneficiary.PlayerReputation + " | reputation_cap=" + cap +
                        " | budget=" + mission.OriginalRewardPoints + " | points_remaining=" +
                        mission.RemainsRewardPoints + " | eligible_regional_chips=" + eligible +
                        " | selected=" + (selected.Count == 0 ? "none" :
                            string.Join(",", selected.Select(entry => entry.Key + " x" + entry.Value))));
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("[Cvar_ImperiumOfMan] Occult contract offer audit unavailable: " + ex.Message);
                }
            }
        }

        [HarmonyPatch(typeof(MissionSystem), nameof(MissionSystem.MissionFinishedByPlayer))]
        private static class TraceRegionalOccultContractPayout
        {
            private static int CargoCount(MagnumCargo cargo, string id) =>
                MagnumCargoSystem.GetAvailableCargoStorages(cargo).Sum(storage => storage.CountItems(id));

            private static void Prefix(MagnumCargo magnumCargo, Mission mission,
                out Dictionary<string, (int Expected, int Before)> __state)
            {
                __state = null;
                if (!_industryReady || magnumCargo == null || mission == null || mission.IsStoryMission)
                    return;
                try
                {
                    var selected = SelectedRegionalRewards(mission);
                    if (selected.Count == 0) return;
                    __state = selected.ToDictionary(entry => entry.Key,
                        entry => (entry.Value, CargoCount(magnumCargo, entry.Key)), StringComparer.Ordinal);
                }
                catch (Exception ex)
                {
                    __state = null;
                    Debug.LogWarning("[Cvar_ImperiumOfMan] Occult contract payout baseline unavailable: " + ex.Message);
                }
            }

            private static void Postfix(MagnumCargo magnumCargo, Mission mission,
                Dictionary<string, (int Expected, int Before)> __state, bool __runOriginal)
            {
                if (!__runOriginal || __state == null) return;
                try
                {
                    foreach (var entry in __state)
                    {
                        int after = CargoCount(magnumCargo, entry.Key);
                        Debug.Log("[Cvar_ImperiumOfMan] Occult contract payout | mission=" +
                            OccultMissionAuditKey(mission) + " | item=" + entry.Key +
                            " | offered=" + entry.Value.Expected + " | before=" + entry.Value.Before +
                            " | after=" + after + " | cargo_delta=" + (after - entry.Value.Before));
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("[Cvar_ImperiumOfMan] Occult contract payout audit unavailable: " + ex.Message);
                }
            }
        }
    }
}

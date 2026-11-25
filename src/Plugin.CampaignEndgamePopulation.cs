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
        private const string CampaignEndgameProvisioned = "iom_endgame_population";
        private const int CampaignEndgameMultiplier = 2;

        private static void ApplyCampaignEndgamePopulation(DungeonGenerationPlan plan)
        {
            if (!TryGetCampaignRouteLayout(plan, out _, out _) ||
                plan.CustomParameters.ContainsKey(CampaignEndgameProvisioned))
                return;

            plan.MonstersPointsLimit *= CampaignEndgameMultiplier;
            plan.ItemsPointsLimit *= CampaignEndgameMultiplier;
            plan.AlliesPointsMult /= CampaignEndgameMultiplier;
            plan.CustomParameters[CampaignEndgameProvisioned] = "1";
        }

        private sealed class CampaignEnemyRoll(DungeonGenerationPlan plan, string faction)
        {
            internal readonly List<string> Tables = plan.MonstersTableIds;
            internal readonly string Faction = faction;
            internal bool Pending = true;
        }

        [ThreadStatic] private static CampaignEnemyRoll _campaignEnemyRoll;

        [HarmonyPatch(typeof(DungeonBuilder), "SpawnDroppableContent")]
        private static class CampaignEnemyRollScope
        {
            private static void Prefix(DungeonGenerationPlan plan, string factionEquipmentId, out CampaignEnemyRoll __state)
            {
                __state = _campaignEnemyRoll;
                _campaignEnemyRoll = null;
                if (plan == null || !TryGetCampaignRouteLayout(plan, out _, out _)) return;
                ApplyCampaignEndgamePopulation(plan);
                _campaignEnemyRoll = new CampaignEnemyRoll(plan, factionEquipmentId);
            }

            private static Exception Finalizer(Exception __exception, CampaignEnemyRoll __state)
            {
                _campaignEnemyRoll = __state;
                return __exception;
            }
        }

        [HarmonyPatch(typeof(UnitGenerationSystem), nameof(UnitGenerationSystem.GenerateUnitsByPoints))]
        private static class DoubleCampaignHostileRoster
        {
            private static void Prefix(List<string> locationIds, string factionId, ref float totalPoints,
                UnitGenerationConditions conditions, out bool __state)
            {
                var roll = _campaignEnemyRoll;
                __state = roll?.Pending == true && ReferenceEquals(locationIds, roll.Tables) &&
                    string.Equals(roll.Faction, factionId, StringComparison.Ordinal) &&
                    !conditions.IsQuestGroup && !conditions.IsEventGroup;
                if (!__state) return;

                roll.Pending = false;
                totalPoints /= CampaignEndgameMultiplier;
            }

            private static void Postfix(List<Unit> __result, bool __state)
            {
                if (!__state || __result.Count == 0) return;

                int incoming = __result.Sum(unit => unit.GetMembersCount());
                // Native spawning assigns a new group index and location to every reserve squad.
                foreach (var squad in __result.ToArray())
                {
                    __result.Add(new Unit
                    {
                        LeaderMobClassId = squad.LeaderMobClassId,
                        Points = squad.Points,
                        Members = new List<string>(squad.Members),
                        FactionId = squad.FactionId,
                        TechLevelLimit = squad.TechLevelLimit,
                        Alliance = squad.Alliance,
                        DebugNote = "Campaign reserve squad"
                    });
                }
                Debug.Log($"[Cvar_ImperiumOfMan] Campaign hostile roster {incoming} → {incoming * CampaignEndgameMultiplier} regular enemies");
            }
        }
    }
}

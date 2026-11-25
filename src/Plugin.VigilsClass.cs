using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using MGSC;
using UnityEngine;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private const int VigilRadius = 2;
        private static string WardKey(State state) => ClassFloor(state) + "ward.";
        private static Dictionary<string, int> ClassCounters(State state) => state.Get<RaidMetadata>().CustomCounters;

        private static bool WardPlaced(State state) => ClassCounters(state).ContainsKey(WardKey(state) + "turns");

        private static bool CanPlaceVigilWard(State state, CellPosition position)
        {
            var player = state.Get<Creatures>().Player;
            var cell = state.Get<MapGrid>().GetCell(position);
            return !WardPlaced(state) && ClassPerkRank(player.CreatureData, VigilWard) > 0 &&
                CellPosition.Distance(player.CreatureData.Position, position) <= VigilRadius &&
                cell != null && cell.isSeen && cell.IsFloor && !cell.IsObjBlockPass && !cell.IsDoor &&
                (position.Equals(player.CreatureData.Position) || state.Get<Creatures>().GetCreature(position.X, position.Y) == null);
        }

        private static bool PlaceVigilWard(State state, CellPosition position)
        {
            if (!CanPlaceVigilWard(state, position))
            {
                return false;
            }
            var player = state.Get<Creatures>().Player;
            var counters = ClassCounters(state);
            string key = WardKey(state);
            int rank = ClassPerkRank(player.CreatureData, VigilWard);
            int seal = ClassPerkRank(player.CreatureData, SealBreach);
            counters[key + "x"] = position.X;
            counters[key + "y"] = position.Y;
            counters[key + "turns"] = 8 + (rank - 1) * 2;
            counters[key + "charges"] = Math.Min(seal, 3);
            // A placement made on the clone's current turn starts its full duration on the following turn.
            counters[key + "fresh"] = 1;
            SpendClassAction(state);
            return true;
        }

        private static string VigilWardLabel(State state)
        {
            var counters = ClassCounters(state);
            string key = WardKey(state);
            int turns = ReadCounter(counters, key + "turns");
            if (turns > 0)
            {
                int charges = ReadCounter(counters, key + "charges");
                return $"Ward · {turns} turns · {charges} seals";
            }
            return WardPlaced(state) ? "Vigil Ward spent" : "Vigil Ward · radius 2";
        }

        private static void DrawWardRadius(MapGrid grid, SelectTargetView view, CellPosition center)
        {
            for (int x = -VigilRadius; x <= VigilRadius; x++)
            {
                for (int y = -VigilRadius; y <= VigilRadius; y++)
                {
                    var position = new CellPosition(center.X + x, center.Y + y);
                    if (CellPosition.Distance(center, position) <= VigilRadius && grid.GetCell(position) is { IsFloor: true, isSeen: true })
                    {
                        view.DrawSpareAreaCell(position);
                    }
                }
            }
        }

        private static bool HoldsWard(State state, Player player, CellPosition target)
        {
            if (state == null || ClassPerkRank(player.CreatureData, VigilWard) == 0)
            {
                return false;
            }
            var counters = ClassCounters(state);
            string key = WardKey(state);
            var center = new CellPosition(ReadCounter(counters, key + "x"), ReadCounter(counters, key + "y"));
            return ReadCounter(counters, key + "turns") > 0 &&
                CellPosition.Distance(center, target) <= VigilRadius &&
                CellPosition.Distance(center, player.CreatureData.Position) <= VigilRadius;
        }

        private static State CurrentClassRaid()
        {
            var mode = SingletonMonoBehaviour<DungeonGameMode>.Instance;
            return mode == null ? null : mode._state;
        }

        [HarmonyPatch(typeof(DungeonGameMode), "FinishGame")]
        private static class ClearVigilWardOnFloorExit
        {
            private static void Prefix(DungeonFinishedData dungeonFinishedData, State ____state, bool ____finished)
            {
                if (____finished || dungeonFinishedData.Reason is not (GameFinishedReason.FinishReached or GameFinishedReason.MoveNextLocation))
                {
                    return;
                }
                if (WardPlaced(____state))
                {
                    ClassCounters(____state)[WardKey(____state) + "turns"] = 0;
                }
                if (BoundServant(____state) is { } servant)
                {
                    CreatureSystem.DespawnCreature(____state.Get<TurnController>(), ____state.Get<Creatures>(), servant);
                }
            }
        }

        [HarmonyPatch(typeof(Player), nameof(Player.NextTurnStarted))]
        private static class TickMercenaryClassActions
        {
            private static void Postfix(Player __instance)
            {
                var state = CurrentClassRaid();
                if (state == null)
                {
                    return;
                }
                if (ClassPerkRank(__instance.CreatureData, VigilWard) > 0)
                {
                    var counters = ClassCounters(state);
                    string key = WardKey(state);
                    if (ReadCounter(counters, key + "turns") > 0 && !counters.Remove(key + "fresh"))
                    {
                        counters[key + "turns"]--;
                    }
                }
                TickBoundServant(state);
            }
        }

        [HarmonyPatch(typeof(QmorphosController), nameof(QmorphosController.ProcessActionPoint))]
        private static class SuppressAmbientQuasimorphosis
        {
            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var setter = AccessTools.PropertySetter(typeof(QmorphosController), nameof(QmorphosController.QmorphosValue));
                var replacement = AccessTools.Method(typeof(SuppressAmbientQuasimorphosis), nameof(ApplyAmbientValue));
                foreach (var instruction in instructions)
                {
                    if (instruction.Calls(setter))
                    {
                        instruction.opcode = OpCodes.Call;
                        instruction.operand = replacement;
                    }
                    yield return instruction;
                }
            }

            private static void ApplyAmbientValue(QmorphosController controller, int value)
            {
                int previous = controller.QmorphosValue;
                var state = CurrentClassRaid();
                var player = state?.Get<Creatures>()?.Player;
                if (value <= previous || player == null || !HoldsWard(state, player, player.CreatureData.Position) ||
                    controller.Now is not { MaxLevel: > 0, QmorphLevelGrow: true, BaronPhase: false })
                {
                    controller.QmorphosValue = value;
                    return;
                }
                controller.QmorphosValue = value - 1;
                var counters = ClassCounters(state);
                string key = WardKey(state) + "xp";
                if (!counters.ContainsKey(key))
                {
                    counters[key] = 1;
                    AwardClassPerk(player, state.Get<PerkFactory>(), VigilWard);
                }
            }
        }

        private static bool TrySealEmergence(CellPosition position)
        {
            var state = CurrentClassRaid();
            var player = state?.Get<Creatures>()?.Player;
            if (player == null || ClassPerkRank(player.CreatureData, SealBreach) == 0 || !HoldsWard(state, player, position))
            {
                return false;
            }
            var counters = ClassCounters(state);
            string key = WardKey(state) + "charges";
            if (ReadCounter(counters, key) <= 0)
            {
                return false;
            }
            counters[key]--;
            AwardClassPerk(player, state.Get<PerkFactory>(), SealBreach);
            return true;
        }

        [HarmonyPatch(typeof(QmorphosController), "TryEccolapseAliveMonsters")]
        private static class SealLivingEmergence
        {
            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var original = AccessTools.Method(typeof(QmorphosController), nameof(QmorphosController.EccolapseAliveMonster),
                    [typeof(Creature), typeof(QmorphosRecord)]);
                var replacement = AccessTools.Method(typeof(SealLivingEmergence), nameof(TryEmerge));
                foreach (var instruction in instructions)
                {
                    if (instruction.Calls(original))
                    {
                        instruction.opcode = OpCodes.Call;
                        instruction.operand = replacement;
                    }
                    yield return instruction;
                }
            }

            private static void TryEmerge(QmorphosController controller, Creature host, QmorphosRecord stage)
            {
                var state = CurrentClassRaid();
                if (stage.ForceEccolapseAll || (state != null && MissionSystem.IsQuestMonster(state.Get<RaidMetadata>(), host as Monster)) ||
                    !TrySealEmergence(host.CreatureData.Position))
                {
                    controller.EccolapseAliveMonster(host, stage);
                }
            }
        }

        [HarmonyPatch(typeof(QmorphosController), "OnMonsterKilled")]
        private static class SealDeathEmergence
        {
            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var range = AccessTools.Method(typeof(UnityEngine.Random), nameof(UnityEngine.Random.Range), [typeof(float), typeof(float)]);
                var replacement = AccessTools.Method(typeof(SealDeathEmergence), nameof(RollEmergence));
                foreach (var instruction in instructions)
                {
                    if (!instruction.Calls(range))
                    {
                        yield return instruction;
                        continue;
                    }
                    yield return new CodeInstruction(OpCodes.Ldarg_0).MoveLabelsFrom(instruction).MoveBlocksFrom(instruction);
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Call, replacement);
                }
            }

            private static float RollEmergence(float minimum, float maximum, QmorphosController controller, CreatureData host)
            {
                float roll = UnityEngine.Random.Range(minimum, maximum);
                var state = CurrentClassRaid();
                var creature = state?.Get<Creatures>().MonstersToRelease.Keys.FirstOrDefault(monster => monster.CreatureData == host) as Monster;
                if (roll < controller.Now.EccolapseChanceAfterDeath &&
                    (creature == null || !MissionSystem.IsQuestMonster(state.Get<RaidMetadata>(), creature)) && TrySealEmergence(host.Position))
                {
                    return maximum;
                }
                return roll;
            }
        }

        [HarmonyPatch(typeof(Creature), "ProcessDamage")]
        private static class VigilBanishmentHit
        {
            private static bool QualifyingHit(State state, Creature victim, DamageHitInfo hit, out Player player)
            {
                player = hit.damageDealer as Player;
                var target = victim.CreatureData;
                return player != null && (hit.usedWeapon || hit.wasMeleeHit) && !hit.wasThrown &&
                    !victim.IsAlly(player) && !target.Health.Invulnerability &&
                    (target.CreatureClass == CreatureClass.Quasimorph || ChaosEnemies.ContainsKey(target.MobClassId)) &&
                    ClassPerkRank(player.CreatureData, Banishment) > 0 && HoldsWard(state, player, target.Position);
            }

            private static void Prefix(Creature __instance, DamageHitInfo hitInfo, out bool __state)
            {
                var state = CurrentClassRaid();
                __state = QualifyingHit(state, __instance, hitInfo, out var player);
                if (!__state)
                {
                    return;
                }
                var counters = ClassCounters(state);
                string key = WardKey(state);
                string mark = key + "mark." + __instance.CreatureData.UniqueId;
                int expires = ReadCounter(counters, mark);
                int turns = ReadCounter(counters, key + "turns");
                if (expires > 0 && turns >= expires)
                {
                    hitInfo.finalDmg += Mathf.RoundToInt(hitInfo.finalDmg * (0.15f + 0.05f * ClassPerkRank(player.CreatureData, Banishment)));
                    counters[mark] = 0;
                    AwardClassPerk(player, state.Get<PerkFactory>(), Banishment);
                }
            }

            private static void Postfix(Creature __instance, DamageHitInfo hitInfo, bool __state)
            {
                if (!__state || !hitInfo.wasWound || !__instance.CreatureData.Health.Alive)
                {
                    return;
                }
                var state = CurrentClassRaid();
                var counters = ClassCounters(state);
                string key = WardKey(state);
                string mark = key + "mark." + __instance.CreatureData.UniqueId;
                if (!counters.ContainsKey(mark))
                {
                    counters[mark] = Math.Max(1, ReadCounter(counters, key + "turns") - 1);
                }
            }
        }
    }
}

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
        private enum SupportAction { Fire, Supply, Troops }

        private const string FireSpent = "iom.support.fire_spent";
        private const string PodsSpent = "iom.support.pods_spent";
        private const string BarrageKey = "iom.support.barrage.";
        private static readonly CellPosition[] LandingOffsets =
            [CellPosition.Zero, new(0, 1), new(1, 0), new(0, -1), new(-1, 0)];

        private static bool IsSupportRaid(State state) =>
            state.Get<RaidMetadata>()?.RaidType is RaidType.ProcMission or RaidType.StoryMission &&
            state.Get<LocationMetadata>()?.LocationId is string location && location != "arrival";

        private static int ReadCounter(Dictionary<string, int> counters, string key) =>
            counters.TryGetValue(key, out int value) ? value : 0;

        private static int SupportCharges(State state, SupportAction action)
        {
            var progression = state.Get<MagnumProgression>();
            var counters = state.Get<RaidMetadata>().CustomCounters;
            bool fire = action == SupportAction.Fire;
            string bonus = fire ? BarrageCalls : PodCalls;
            return Math.Max(0, 1 + (progression.IsPerkPurchased(bonus) ? 1 : 0) -
                ReadCounter(counters, fire ? FireSpent : PodsSpent));
        }

        private static string FloorBarrage(State state) => BarrageKey + state.Get<LocationMetadata>().LocationId + ".";

        private static bool HasPendingBarrage(State state) =>
            state.Get<RaidMetadata>().CustomCounters.ContainsKey(FloorBarrage(state) + "next");

        private static bool CanTargetSupport(State state)
        {
            var creatures = state.Get<Creatures>();
            return IsSupportRaid(state) && creatures?.Player is Player player && player.IsProcessing &&
                !player.AnyActionProcessedFlag && player.ActionPointsLeft > 0 &&
                TurnSystem.CanProcessPlayerTurn(state.Get<TurnController>(), state.Get<TurnMetadata>(), creatures);
        }

        private static bool CanRequestSupport(State state, SupportAction action, int choice)
        {
            if (!CanTargetSupport(state) || SupportCharges(state, action) == 0)
            {
                return false;
            }
            var progression = state.Get<MagnumProgression>();
            return action switch
            {
                SupportAction.Fire => choice >= 0 && choice < ShipMounts.Length &&
                    progression.IsPerkPurchased(ShipMounts[choice].Id) && !HasPendingBarrage(state),
                SupportAction.Supply => progression.IsPerkPurchased(SupplyDrop),
                SupportAction.Troops => choice >= 0 && choice < DropSquads.Length &&
                    DropSquads[choice].Tier <= ImperialSupportTech(state.Get<Factions>()) &&
                    progression.IsPerkPurchased(TroopTier(DropSquads[choice].Tier)),
                _ => false
            };
        }

        private static List<CellPosition> SupportLanding(State state, CellPosition target, int count, bool isSupply)
        {
            var grid = state.Get<MapGrid>();
            var creatures = state.Get<Creatures>();
            var obstacles = state.Get<MapObstacles>();
            var positions = new List<CellPosition>(count);
            foreach (var offset in LandingOffsets)
            {
                var position = target + offset;
                var cell = grid.GetCell(position);
                if (cell?.Type == MapCellType.Floor && cell.isSeen && cell.ReachableCellFlag &&
                    (!isSupply || !cell.IsOneWideCorridor) &&
                    cell.specialFlag == MapCellSpecialFlag.None && cell.IsPassable(ObstacleType.OneWayCorridor) &&
                    creatures.GetCreature(position.X, position.Y) == null && obstacles.Get(position.X, position.Y) == null)
                {
                    positions.Add(position);
                    if (positions.Count == count)
                    {
                        return positions;
                    }
                }
                else if (offset.Equals(CellPosition.Zero))
                {
                    break;
                }
            }
            positions.Clear();
            return positions;
        }

        private static int RequiredLandingCells(State state, SupportAction action) => action == SupportAction.Supply ? 1 :
            TroopPodCapacity(state.Get<MagnumProgression>());

        private static bool IsFireTarget(MapCell cell) => cell != null && cell.IsFloorSurfaceType && cell.IsExplored;

        private static int SalvoSize(MagnumProgression progression, ShipMount mount) =>
            mount.Rounds + (progression.IsPerkPurchased(BarrageRounds) ? 1 : 0);

        private static bool HorizontalStrike(CellPosition origin, CellPosition target) =>
            Math.Abs(target.X - origin.X) >= Math.Abs(target.Y - origin.Y);

        private static IEnumerable<CellPosition> SalvoCells(MapGrid grid, CellPosition target, bool horizontal, int rounds)
        {
            for (int i = 0; i < rounds; i++)
            {
                int offset = (i + 1) / 2 * (i % 2 == 0 ? 1 : -1);
                var position = horizontal ? new CellPosition(target.X + offset, target.Y) :
                    new CellPosition(target.X, target.Y + offset);
                // A later salvo still hits its marked coordinates after the first changes the terrain.
                yield return grid.GetCell(position) == null ? target : position;
            }
        }

        private static bool TryRequestSupport(State state, SupportAction action, int choice, CellPosition target)
        {
            if (!CanRequestSupport(state, action, choice))
            {
                return false;
            }
            bool fire = action == SupportAction.Fire;
            var raid = state.Get<RaidMetadata>();
            var player = state.Get<Creatures>().Player;
            if (fire)
            {
                if (!IsFireTarget(state.Get<MapGrid>().GetCell(target)))
                {
                    return false;
                }
                var progression = state.Get<MagnumProgression>();
                string key = FloorBarrage(state);
                var counters = raid.CustomCounters;
                counters[key + "mount"] = choice;
                counters[key + "x"] = target.X;
                counters[key + "y"] = target.Y;
                counters[key + "horizontal"] = HorizontalStrike(player.CreatureData.Position, target) ? 1 : 0;
                counters[key + "rounds"] = SalvoSize(progression, ShipMounts[choice]);
                counters[key + "turns"] = progression.IsPerkPurchased(BarrageDuration) ? 2 : 1;
                counters[key + "next"] = raid.TurnNumber + (choice == 1 ? 2 : 1);
            }
            else
            {
                var landing = SupportLanding(state, target, RequiredLandingCells(state, action), action == SupportAction.Supply);
                if (landing.Count == 0)
                {
                    return false;
                }
                if (action == SupportAction.Supply)
                {
                    DeliverSupplies(state, target);
                }
                else
                {
                    DeploySupportSquad(state, DropSquads[choice], landing);
                }
                SingletonMonoBehaviour<SoundController>.Instance.PlayUISound(
                    SingletonMonoBehaviour<SoundsStorage>.Instance.CallAutonomousCapsule);
            }

            string spent = fire ? FireSpent : PodsSpent;
            raid.CustomCounters[spent] = ReadCounter(raid.CustomCounters, spent) + 1;
            state.Get<PlayerCommandQueue>().Clear();
            player.CreatureData.EffectsController.PropagateAction(PlayerActionHappened.HandAction);
            player.AnyActionProcessedFlag = true;
            player.IsProcessing = false;
            return true;
        }

        private static void DeliverSupplies(State state, CellPosition target)
        {
            var items = new List<BasePickupItem>();
            foreach (var cargo in SupplyPodManifest(state.Get<MagnumProgression>(), state.Get<StoryTriggers>()))
                ItemInteractionSystem.CreateItem(items, cargo.Id, cargo.Count);
            const string nativeSupplyCapsule = "89708297-fad2-4293-ad7e-73e0b94db27c";
            var capsule = SingletonMonoBehaviour<MapObstacleFactory>.Instance.Spawn(nativeSupplyCapsule,
                target, new CellPosition(13, 0), register: true, refreshOccupation: true);
            var floor = state.Get<ItemsOnFloor>();
            var grid = state.Get<MapGrid>();
            var storage = capsule?.Store?.storage as ItemStorage;
            foreach (var item in items)
            {
                if (storage != null)
                {
                    if (storage.TryPutItem(item, CellPosition.Zero)) continue;
                    // Native cargo uses this same one-row expansion when a delivery
                    // will not fit. Preserve overflow even if an item still cannot fit.
                    storage.ExpandHeight(1);
                    if (storage.TryPutItem(item, CellPosition.Zero)) continue;
                }
                ItemOnFloorSystem.SpawnItem(floor, grid, item, target);
            }
        }

        private static void DeploySupportSquad(State state, DropSquad squad, List<CellPosition> landing)
        {
            var creatures = state.Get<Creatures>();
            var turns = state.Get<TurnController>();
            var mercenaries = state.Get<Mercenaries>();
            var perks = state.Get<PerkFactory>();
            var difficulty = state.Get<Difficulty>();
            var raid = state.Get<RaidMetadata>();
            int group = raid.FreeGroupIndex++;
            for (int index = 0; index < landing.Count; index++)
            {
                bool leader = index == 0;
                var ally = CreatureSystem.SpawnMonsterFromMobClass(mercenaries, perks, difficulty, creatures,
                    raid, turns, leader ? squad.Leader : squad.Partner, landing[index], groupIndex: group,
                    isLeader: leader, techLevelLimit: squad.Tier, factionId: ItemCategory,
                    actAfter: creatures.Player, creatureAlliance: CreatureAlliance.PlayerAlliance);
                ally.CreatureData.IsSummonedCreature = true;
                ally.Behaviour.StartFollowing(creatures.Player);
                ally.Behaviour.WaitTransferAtElevator = true;
                ally.IsTransferable = true;
            }
        }

        [HarmonyPatch(typeof(DungeonGameMode), "OnTurnChanged")]
        private static class ProcessSupportFire
        {
            private static void Postfix(State ____state, ITurnable obj)
            {
                if (obj == ____state.Get<FireController>() && IsSupportRaid(____state))
                {
                    FireScheduledSalvo(____state);
                }
            }
        }

        private static void FireScheduledSalvo(State state)
        {
            var raid = state.Get<RaidMetadata>();
            var counters = raid.CustomCounters;
            string key = FloorBarrage(state);
            if (!counters.TryGetValue(key + "next", out int next) || next > raid.TurnNumber)
            {
                return;
            }

            var mount = ShipMounts[counters[key + "mount"]];
            var target = new CellPosition(counters[key + "x"], counters[key + "y"]);
            var grid = state.Get<MapGrid>();
            var factory = state.Get<ExplosionFactory>();
            var player = state.Get<Creatures>().Player;
            foreach (var position in SalvoCells(grid, target, counters[key + "horizontal"] == 1, counters[key + "rounds"]))
            {
                factory.SpawnExplosion(player, position, mount.Explosion, fastForward: true, parameters =>
                {
                    parameters.Radius = mount.Radius;
                    parameters.Damage = mount.Damage;
                    parameters.DamageType = mount.DamageType;
                    parameters.SelfDamaging = true;
                    parameters.PropagateFire = false;
                    parameters.Throwback = false;
                    return parameters;
                });
            }
            if (--counters[key + "turns"] > 0)
            {
                counters[key + "next"] = raid.TurnNumber + 1;
                return;
            }
            foreach (string field in new[] { "next", "mount", "x", "y", "horizontal", "rounds", "turns" })
            {
                counters.Remove(key + field);
            }
        }

        private sealed class ShipTarget(State state, SelectAbilityTarget view, SupportAction action, int choice)
        {
            public readonly State State = state;
            public readonly SelectAbilityTarget View = view;
            public readonly SupportAction Action = action;
            public readonly int Choice = choice;
            public readonly Timer StickTimer = new();
            public CellPosition Cursor = state.Get<Creatures>().Player.CreatureData.Position;
        }

        private static ShipTarget _shipTarget;

        private static void StartSupportTargeting(State state, SupportAction action, int choice)
        {
            if (CanRequestSupport(state, action, choice))
            {
                state.Get<PlayerCommandQueue>().Clear();
                UI.Chain<SelectAbilityTarget>().Invoke(view =>
                {
                    _shipTarget = new ShipTarget(state, view, action, choice);
                    _shipTarget.StickTimer.Start();
                }).Show();
            }
        }

        [HarmonyPatch(typeof(SelectAbilityTarget), nameof(SelectAbilityTarget.Process))]
        private static class TargetShipSupport
        {
            private static bool Prefix(SelectAbilityTarget __instance, ref bool interruptProcessing)
            {
                var order = _shipTarget;
                if (order?.View != __instance)
                {
                    return true;
                }
                interruptProcessing = true;
                var input = SingletonMonoBehaviour<InputController>.Instance;
                if (input.IsKeyDown("SecondaryCursorAction") || input.IsKeyDown("Back", "UI") || input.IsKeyDown("Menu"))
                {
                    UI.Hide<SelectAbilityTarget>();
                    return false;
                }
                State state = order.State;
                var targetView = state.Get<SelectTargetView>();
                targetView.FreeBorders();
                targetView.FreeHitHints();
                targetView.HideSingleCell();
                if (input.Mode == InputController.InputMode.KeyboardAndMouse)
                {
                    order.Cursor = state.Get<MapRenderer>().GetCellUnderCursor();
                }
                else
                {
                    var axis = input.GetAxis("UI", "Movement", out _,
                        SingletonMonoBehaviour<GameSettings>.Instance.KeyThreshold, order.StickTimer);
                    CellPosition.ChangePositionByDir(InputHelper.GetDirectionFromAxis(axis), ref order.Cursor.X, ref order.Cursor.Y);
                    PlayerInteractionSystem.ProcessCameraInput(input, state.Get<GameCamera>());
                }

                var grid = state.Get<MapGrid>();
                if (input.IsKeyDown("PrimaryCursorAction") ||
                    (input.Mode != InputController.InputMode.KeyboardAndMouse && input.IsKeyDown("UseUltimate")))
                {
                    if (TryRequestSupport(state, order.Action, order.Choice, order.Cursor))
                    {
                        UI.Hide<SelectAbilityTarget>();
                        return false;
                    }
                    SingletonMonoBehaviour<SoundController>.Instance.PlayUiSound(SingletonMonoBehaviour<SoundsStorage>.Instance.EmptyAttack);
                }
                if (grid.GetCell(order.Cursor) == null)
                {
                    return false;
                }
                if (order.Action == SupportAction.Fire && IsFireTarget(grid.GetCell(order.Cursor)))
                {
                    var mount = ShipMounts[order.Choice];
                    bool horizontal = HorizontalStrike(state.Get<Creatures>().Player.CreatureData.Position, order.Cursor);
                    int rounds = SalvoSize(state.Get<MagnumProgression>(), mount);
                    var affected = new HashSet<CellPosition>();
                    var cells = new List<CellPosition>();
                    foreach (var center in SalvoCells(grid, order.Cursor, horizontal, rounds))
                    {
                        ExplosionEntity.CollectAffectedCells(grid, center, mount.Radius, cells);
                        affected.UnionWith(cells);
                    }
                    foreach (var cell in affected.Where(cell => grid.GetCell(cell)?.IsFloor == true))
                    {
                        targetView.DrawSpareAreaCell(cell);
                    }
                    targetView.DrawSingleCell(order.Cursor, SelectTargetView.BorderColor.Selected);
                }
                else if (order.Action != SupportAction.Fire)
                {
                    var landing = SupportLanding(state, order.Cursor, RequiredLandingCells(state, order.Action), order.Action == SupportAction.Supply);
                    foreach (var cell in landing)
                    {
                        targetView.DrawSpareAreaCell(cell);
                    }
                    targetView.DrawSingleCell(order.Cursor, landing.Count > 0 ? SelectTargetView.BorderColor.PlaceableSelected :
                        SelectTargetView.BorderColor.Red);
                }
                else
                {
                    targetView.DrawSingleCell(order.Cursor, SelectTargetView.BorderColor.Red);
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(SelectAbilityTarget), "OnInputModeChanged")]
        private static class SupportTargetHint
        {
            private static void Postfix(SelectAbilityTarget __instance, HintPanel ____hintPanel, InputController.InputMode inputMode)
            {
                var order = _shipTarget;
                if (order?.View != __instance)
                {
                    return;
                }
                string caption = order.Action switch
                {
                    SupportAction.Fire => $"Fire for Effect · {ShipMounts[order.Choice].Name} · {(order.Choice == 1 ? 2 : 1)}-turn delay" +
                        (order.State.Get<MagnumProgression>().IsPerkPurchased(BarrageDuration) ? " · two salvos" : string.Empty),
                    SupportAction.Supply => "Mark a clear supply landing tile",
                    _ => $"Mark a clear landing zone · {DropSquads[order.Choice].Name}"
                };
                ____hintPanel.Initialize(caption, yellow: false);
                bool mouse = inputMode == InputController.InputMode.KeyboardAndMouse;
                ____hintPanel.AddAction(mouse ? "PrimaryCursorAction" : "UseUltimate", "iom.support.confirm");
                ____hintPanel.AddAction(mouse ? "SecondaryCursorAction" : "Back", "ui.tooltip.Cancel");
            }
        }

        [HarmonyPatch(typeof(SelectAbilityTarget), "OnDisable")]
        private static class ClearSupportTarget
        {
            private static void Postfix(SelectAbilityTarget __instance)
            {
                if (_shipTarget?.View == __instance)
                {
                    var view = _shipTarget.State.Get<SelectTargetView>();
                    view?.FreeBorders();
                    view?.HideSingleCell();
                    _shipTarget = null;
                }
            }
        }
    }
}

using System;
using System.Linq;
using HarmonyLib;
using MGSC;
using UnityEngine;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private enum ClassAction { Attune, Greater, Sacrifice, Service, Recover, Deploy, Reclaim, Ward }

        private static readonly (ClassAction Action, string Perk, string Label)[] ClassActions =
        [
            (ClassAction.Attune, RaiseFallen, "Attune Raise the Fallen"),
            (ClassAction.Greater, GreaterVessel, "Greater Vessel"),
            (ClassAction.Sacrifice, SacrificialRelease, "Sacrificial Release"),
            (ClassAction.Service, FieldService, "Field Service"),
            (ClassAction.Recover, RecoverChassis, "Recover Chassis"),
            (ClassAction.Deploy, DoorwayDeployment, "Doorway Deployment"),
            (ClassAction.Reclaim, WreckReclamation, "Wreck Reclamation"),
            (ClassAction.Ward, VigilWard, "Vigil Ward")
        ];

        private sealed class ClassTarget(State state, SelectAbilityTarget view, ClassAction action)
        {
            internal readonly State State = state;
            internal readonly SelectAbilityTarget View = view;
            internal readonly ClassAction Action = action;
            internal readonly Timer Timer = new();
            internal CellPosition Cursor = state.Get<Creatures>().Player.CreatureData.Position;
        }

        private static ClassTarget _classTarget;

        private static string ClassFloor(State state) => "iom.class." + state.Get<LocationMetadata>().LocationId + ".";

        private static bool CanUseClassAction(State state)
        {
            var creatures = state.Get<Creatures>();
            return state.Get<RaidMetadata>() is { StationVisit: false } && creatures?.Player is { } player &&
                player.CreatureData.Health.Alive && player.IsProcessing && !player.AnyActionProcessedFlag &&
                player.ActionPointsLeft > 0 && TurnSystem.CanProcessPlayerTurn(state.Get<TurnController>(), state.Get<TurnMetadata>(), creatures);
        }

        private static void SpendClassAction(State state, int actionPoints = 1)
        {
            var creatures = state.Get<Creatures>();
            var player = creatures.Player;
            player.CreatureData.EffectsController.PropagateAction(PlayerActionHappened.HandAction);
            if (actionPoints > 1)
            {
                int additional = actionPoints - 1;
                player._actionPointsProcessed += additional;
                player.CreatureData.EffectsController.PropagateActionPointSpent(additional);
            }
            PlayerInteractionSystem.EndPlayerTurn(creatures, PlayerEndTurnContext.InventoryInteraction);
            UI.Get<DungeonHudScreen>().RefreshAllHud();
        }

        [Hook(ModHookType.DungeonStarted)]
        public static void AddMercenaryClassHud(IModContext context)
        {
            var state = context.State;
            if (!_customClassesReady || state.Get<RaidMetadata>() is not { StationVisit: false } ||
                state.Get<Creatures>()?.Player is not { } player)
            {
                return;
            }
            var actions = ClassActions.Where(action => ClassPerkRank(player.CreatureData, action.Perk) > 0).ToArray();
            var hud = UI.Get<DungeonHudScreen>();
            if (hud == null)
            {
                return;
            }
            var previous = hud.transform.Find("ImperialMercenaryClasses");
            if (previous != null)
            {
                previous.gameObject.SetActive(false);
                UnityEngine.Object.Destroy(previous.gameObject);
            }
            if (actions.Length == 0)
            {
                return;
            }
            float width = Math.Min(152f, 735f / actions.Length);
            var panel = CreateSupportRect(hud.transform, "ImperialMercenaryClasses", new Vector2(0.5f, 1f),
                new Vector2(width * actions.Length, 30f), new Vector2(0f, -68f));
            panel.gameObject.AddComponent<ClassHud>().Initialize(state, actions, width);
        }

        private sealed class ClassHud : MonoBehaviour
        {
            private State _state;
            private (ClassAction Action, string Label, ShipSupportButton Button)[] _buttons;

            internal void Initialize(State state, (ClassAction Action, string Perk, string Label)[] actions, float width)
            {
                _state = state;
                _buttons = new (ClassAction, string, ShipSupportButton)[actions.Length];
                for (int index = 0; index < actions.Length; index++)
                {
                    var action = actions[index];
                    var rect = CreateSupportRect(transform, action.Label, new Vector2(0f, 0.5f),
                        new Vector2(width - 4f, 28f), new Vector2(width * (index + 0.5f), 0f));
                    var button = rect.gameObject.AddComponent<ShipSupportButton>();
                    button.Configure(null, action.Label, true, () => BeginClassAction(state, action.Action));
                    _buttons[index] = (action.Action, action.Label, button);
                }
            }

            private void Update()
            {
                var player = _state?.Get<Creatures>()?.Player;
                if (player == null || _buttons == null)
                {
                    return;
                }
                bool canAct = !UI.IsAnyShowing(typeof(DungeonHudScreen)) && CanUseClassAction(_state);
                foreach (var entry in _buttons)
                {
                    bool available = ClassActionReady(_state, entry.Action);
                    string label = entry.Action == ClassAction.Ward ? VigilWardLabel(_state) : entry.Label;
                    if (entry.Action == ClassAction.Greater && GreaterVesselSelected(_state))
                    {
                        label += ClassSupply(player, BoundAsh) == null ? " · needs Bound Ash" : " [on]";
                    }
                    if (entry.Action == ClassAction.Attune && player.CreatureData.UltimatePerk != null)
                    {
                        label = IsRaisePact(player.CreatureData) ? "Raise the Fallen attuned" : "Pact slot occupied";
                    }
                    if (entry.Action == ClassAction.Deploy && FirstDoorwayPackage(player) is { } package)
                    {
                        label = "Deploy · " + DoorwayMachineTitle(package);
                    }
                    entry.Button.SetStatus(label, canAct && available);
                }
            }
        }

        private static bool ClassActionReady(State state, ClassAction action) => action switch
        {
            ClassAction.Attune => state.Get<Creatures>().Player.CreatureData.UltimatePerk == null,
            ClassAction.Greater => IsRaisePact(state.Get<Creatures>().Player.CreatureData),
            ClassAction.Sacrifice => BoundServant(state) != null,
            ClassAction.Service => ClassSupply(state.Get<Creatures>().Player, EngineerKit) is { IsUsable: true },
            ClassAction.Deploy => FirstDoorwayPackage(state.Get<Creatures>().Player) != null,
            ClassAction.Reclaim => ClassSupply(state.Get<Creatures>().Player, WreckPart) != null,
            ClassAction.Ward => !WardPlaced(state),
            _ => true
        };

        private static void BeginClassAction(State state, ClassAction action)
        {
            if (!CanUseClassAction(state) || !ClassActionReady(state, action))
            {
                return;
            }
            if (action == ClassAction.Attune)
            {
                AttuneRaisePact(state);
                return;
            }
            if (action == ClassAction.Greater)
            {
                ToggleGreaterVessel(state);
                return;
            }
            state.Get<PlayerCommandQueue>().Clear();
            UI.Chain<SelectAbilityTarget>().Invoke(view =>
            {
                _classTarget = new ClassTarget(state, view, action);
                _classTarget.Timer.Start();
            }).Show();
        }

        private static bool ClassTargetValid(State state, ClassAction action, CellPosition cell) => action switch
        {
            ClassAction.Ward => CanPlaceVigilWard(state, cell),
            ClassAction.Sacrifice => CanSacrifice(state, cell),
            _ => EngineerTargetValid(state, action, cell)
        };

        private static bool ApplyClassAction(State state, ClassAction action, CellPosition cell) => action switch
        {
            ClassAction.Ward => PlaceVigilWard(state, cell),
            ClassAction.Sacrifice => SacrificeServant(state, cell),
            _ => UseEngineerAction(state, action, cell)
        };

        [HarmonyPatch(typeof(SelectAbilityTarget), nameof(SelectAbilityTarget.Process))]
        private static class ClassTargeting
        {
            private static bool Prefix(SelectAbilityTarget __instance, ref bool interruptProcessing)
            {
                var target = _classTarget;
                if (target?.View != __instance)
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
                var state = target.State;
                var view = state.Get<SelectTargetView>();
                view.FreeBorders();
                view.FreeHitHints();
                view.HideSingleCell();
                if (input.Mode == InputController.InputMode.KeyboardAndMouse)
                {
                    target.Cursor = state.Get<MapRenderer>().GetCellUnderCursor();
                }
                else
                {
                    var axis = input.GetAxis("UI", "Movement", out _, SingletonMonoBehaviour<GameSettings>.Instance.KeyThreshold, target.Timer);
                    CellPosition.ChangePositionByDir(InputHelper.GetDirectionFromAxis(axis), ref target.Cursor.X, ref target.Cursor.Y);
                    PlayerInteractionSystem.ProcessCameraInput(input, state.Get<GameCamera>());
                }
                bool valid = ClassTargetValid(state, target.Action, target.Cursor);
                if (input.IsKeyDown("PrimaryCursorAction") ||
                    (input.Mode != InputController.InputMode.KeyboardAndMouse && input.IsKeyDown("UseUltimate")))
                {
                    if (valid && ApplyClassAction(state, target.Action, target.Cursor))
                    {
                        UI.Hide<SelectAbilityTarget>();
                        return false;
                    }
                    SingletonMonoBehaviour<SoundController>.Instance.PlayUiSound(SingletonMonoBehaviour<SoundsStorage>.Instance.EmptyAttack);
                }
                if (state.Get<MapGrid>().GetCell(target.Cursor) != null)
                {
                    if (valid && target.Action == ClassAction.Ward)
                    {
                        DrawWardRadius(state.Get<MapGrid>(), view, target.Cursor);
                    }
                    view.DrawSingleCell(target.Cursor, valid ? SelectTargetView.BorderColor.PlaceableSelected : SelectTargetView.BorderColor.Red);
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(SelectAbilityTarget), "OnInputModeChanged")]
        private static class ClassTargetHint
        {
            private static void Postfix(SelectAbilityTarget __instance, HintPanel ____hintPanel, InputController.InputMode inputMode)
            {
                if (_classTarget?.View != __instance)
                {
                    return;
                }
                ____hintPanel.Initialize(ClassActions.First(action => action.Action == _classTarget.Action).Label, yellow: false);
                bool mouse = inputMode == InputController.InputMode.KeyboardAndMouse;
                ____hintPanel.AddAction(mouse ? "PrimaryCursorAction" : "UseUltimate", "iom.support.confirm");
                ____hintPanel.AddAction(mouse ? "SecondaryCursorAction" : "Back", "ui.tooltip.Cancel");
            }
        }

        [HarmonyPatch(typeof(SelectAbilityTarget), "OnDisable")]
        private static class ClearClassTarget
        {
            private static void Postfix(SelectAbilityTarget __instance)
            {
                var target = _classTarget;
                if (target?.View == __instance)
                {
                    _classTarget = null;
                    var view = target.State.Get<SelectTargetView>();
                    view?.FreeBorders();
                    view?.HideSingleCell();
                }
            }
        }
    }
}

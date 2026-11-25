using System;
using System.Collections;
using System.Linq;
using HarmonyLib;
using MGSC;
using Object = UnityEngine.Object;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        internal const string RaisePact = "iom_undivided_raise_fallen";
        private const string RaiseFocus = "iom_undivided_raise_fallen_focus";
        private const string GreaterSelection = "iom.class.greater_vessel";
        private const string RaiseFirstAttuned = "iom.class.raise.attuned";
        private const string ServantRole = "iom_chaos_fury";

        private static void RegisterRaisePact()
        {
            var focus = CloneItem<SkullRecord>(RaiseFocus, "skull_ron_blindness", 3, 0);
            focus.PerkId = RaisePact;
            focus.Essence = ChaosEssence;
            focus.Categories = [];
            focus.CanBaronUse = false;
            focus.Upgrade = focus.Downgrade = focus.Sidegrade = RaiseFocus;
            RegisterItem("skulls", focus, "Raise the Fallen", "The focus holds the Necromancer's pact in the clone's pact slot.");

            var descriptor = Object.Instantiate((PerkDescriptor)Data.Perks.GetRecord("ron_blindness").ContentDescriptor);
            descriptor.name = RaisePact;
            StageDescriptor("pacts", RaisePact, descriptor);
            StageRecord(Data.Perks, RaisePact, new PactRecord
            {
                Id = RaisePact, ContentDescriptor = descriptor, BramfaturaId = ChaosFaction,
                PerkType = PerkType.Ultimate, PactTier = 3, CurseValue = 65, RechargeValue = 95,
                Parameters = Parameters("SMobClass " + ServantRole + " SFaction " + ChaosFaction + " BIsFollower true"),
                ActiveWeaponClassLimit = [], ActiveWeaponSubClassLimit = [], AIParameters = []
            });
            Registrations.Add(() => TranslationPatch.AddChaosPact(RaisePact, "Raise the Fallen", "Bind one visible enemy corpse as a Fury."));
        }

        private static bool IsRaisePact(CreatureData data) => data.UltimatePerk?.PerkId == RaisePact;

        private static void AttuneRaisePact(State state)
        {
            var player = state.Get<Creatures>().Player;
            if (player.CreatureData.UltimatePerk != null || ClassPerkRank(player.CreatureData, RaiseFallen) == 0)
            {
                return;
            }
            var mercenary = player.Mercenary;
            state.Get<Mercenaries>().MercToUltimateSkulls[mercenary.ProfileId] = RaiseFocus;
            PerkSystem.AddUltimatePerk(RaisePact, RaiseFocus, mercenary, state.Get<PerkFactory>());
            var counters = ClassCounters(state);
            if (!counters.ContainsKey(RaiseFirstAttuned) && Data.Perks.GetRecord(RaisePact) is PactRecord pact)
            {
                counters[RaiseFirstAttuned] = 1;
                mercenary.PactLevel = pact.RechargeValue;
            }
            UI.Get<DungeonHudScreen>().RefreshUltimatePerk();
        }

        private static void RemoveUnequippedRaisePact(Mercenary mercenary, Mercenaries mercenaries)
        {
            if (mercenary == null || !IsRaisePact(mercenary.CreatureData) || ClassPerkRank(mercenary.CreatureData, RaiseFallen) != 0)
            {
                return;
            }
            mercenaries?.MercToUltimateSkulls.Remove(mercenary.ProfileId);
            mercenary.CreatureData.RemoveUltimatePerk();
            CurseSystem.RemoveCurse(mercenary);
        }

        [HarmonyPatch(typeof(MercenarySystem), nameof(MercenarySystem.ApplyClassForMercenary))]
        private static class ClearUnequippedRaisePact
        {
            private static void Postfix(Mercenary mercenary) => RemoveUnequippedRaisePact(mercenary,
                SingletonMonoBehaviour<SpaceGameMode>.Instance?.Get<Mercenaries>());
        }

        [HarmonyPatch(typeof(MercenarySystem), nameof(MercenarySystem.CloneMercenary))]
        private static class ClearInvalidReclonedPact
        {
            private static void Postfix(Mercenaries mercenaries)
            {
                foreach (var mercenary in mercenaries.Values)
                {
                    RemoveUnequippedRaisePact(mercenary, mercenaries);
                }
            }
        }

        [Hook(ModHookType.DungeonStarted)]
        public static void ReconcileClassPact(IModContext context)
        {
            var player = context.State.Get<Creatures>()?.Player;
            RemoveUnequippedRaisePact(player?.Mercenary, context.State.Get<Mercenaries>());
        }

        private static bool GreaterVesselSelected(State state) => ClassCounters(state).ContainsKey(GreaterSelection);

        private static void ToggleGreaterVessel(State state)
        {
            var counters = ClassCounters(state);
            if (!counters.Remove(GreaterSelection))
            {
                counters[GreaterSelection] = 1;
            }
        }

        private static BasePickupItem ClassSupply(Player player, string id) => player.CreatureData.Inventory.AllContainers
            .SelectMany(storage => storage.Items).FirstOrDefault(item => item.Id == id && !item.Locked);

        private static bool IsBoundServant(CreatureData creature) => creature.IsSummonedCreature && creature.MobClassId == ServantRole &&
            creature.EffectsController.First<ImperialBoundServant>() != null;

        private static string BoundServantKey(State state) => ClassFloor(state) + "bound_fury";

        private static Creature BoundServant(State state)
        {
            return ClassCounters(state).TryGetValue(BoundServantKey(state), out int id) &&
                state.Get<Creatures>().GetCreature(id) is Monster servant && servant.CreatureData.Health.Alive &&
                IsBoundServant(servant.CreatureData) ? servant : null;
        }

        private static string CorpseRuleKey(int id) => "iom.class.corpse." + id;

        [HarmonyPatch(typeof(CreatureSystem), nameof(CreatureSystem.GenerateCorpse))]
        private static class RecordClassCorpseRules
        {
            private static void Postfix(Creature killedMonster, CreatureData __result)
            {
                var state = CurrentClassRaid();
                var player = state?.Get<Creatures>()?.Player;
                if (player == null || killedMonster is not Monster monster)
                {
                    return;
                }
                bool protectedBody = killedMonster.CreatureData.IsQuestCreature || killedMonster.CreatureData.TriggeredWinCondition ||
                    MissionSystem.IsQuestMonster(state.Get<RaidMetadata>(), monster);
                int eligibility = protectedBody || (killedMonster.CreatureData.IsSummonedCreature && !RecoverableMachine(killedMonster.CreatureData, out _))
                    ? 0 : killedMonster.IsAlly(player) ? 1 : 2;
                ClassCounters(state)[ClassFloor(state) + CorpseRuleKey(__result.UniqueId)] = eligibility;
            }
        }

        private static int CorpseEligibility(State state, CorpseStorage corpse) =>
            corpse.ResurrectBlocked ? 0 : ReadCounter(ClassCounters(state), ClassFloor(state) + CorpseRuleKey(corpse.CreatureData.UniqueId));

        internal static MapObstacle FindRaiseCorpse(Player player, CellPosition position)
        {
            var state = CurrentClassRaid();
            int rank = ClassPerkRank(player.CreatureData, RaiseFallen);
            var cell = state?.Get<MapGrid>().GetCell(position);
            if (rank == 0 || cell == null || !cell.isSeen || !cell.IsFloor || cell.IsObjBlockPass || cell.IsDoor ||
                CellPosition.Distance(player.CreatureData.Position, position) > (rank >= 3 ? 3 : 2) || BoundServant(state) != null ||
                state.Get<Creatures>().GetCreature(position.X, position.Y) != null ||
                (GreaterVesselSelected(state) && ClassPerkRank(player.CreatureData, GreaterVessel) > 0 && ClassSupply(player, BoundAsh) == null))
            {
                return null;
            }
            return state.Get<MapObstacles>().RaycastMapObstacles(position.X, position.Y)
                .Where(obstacle => obstacle.CorpseStorage is { } corpse && corpse.CreatureData.CreatureClass == CreatureClass.Human &&
                    CorpseEligibility(state, corpse) == 2)
                .OrderBy(obstacle => obstacle.UniqueId).FirstOrDefault();
        }

        [HarmonyPatch(typeof(SelectUltimateTarget), nameof(SelectUltimateTarget.Process))]
        private static class PreviewRaiseCorpse
        {
            private static int _lastTarget = int.MinValue;
            private static InputController.InputMode _lastInputMode;

            private static void Postfix(SelectUltimateTarget __instance, Creatures ____creatures, HintPanel ____hintPanel,
                int ____x, int ____y)
            {
                var player = ____creatures.Player;
                if (!__instance.isActiveAndEnabled || !IsRaisePact(player.CreatureData))
                {
                    _lastTarget = int.MinValue;
                    return;
                }
                var body = FindRaiseCorpse(player, new CellPosition(____x, ____y));
                int target = body == null ? -1 : body.UniqueId;
                var mode = SingletonMonoBehaviour<InputController>.Instance.Mode;
                if (target == _lastTarget && mode == _lastInputMode)
                {
                    return;
                }
                _lastTarget = target;
                _lastInputMode = mode;
                string caption = body == null ? "Choose a hostile corpse" :
                    "Raise · " + Localization.Get("monster." + body.CorpseStorage.CreatureData.LocalizationId + ".name");
                ____hintPanel.Initialize(caption, yellow: false);
                bool mouse = mode == InputController.InputMode.KeyboardAndMouse;
                ____hintPanel.AddAction(mouse ? "PrimaryCursorAction" : "UseUltimate", "ui.tooltip.ApplyUltimate");
                ____hintPanel.AddAction(mouse ? "SecondaryCursorAction" : "Back", "ui.tooltip.Cancel");
            }
        }

        private static void DropCorpseInventory(State state, MapObstacle corpse)
        {
            var floor = state.Get<ItemsOnFloor>();
            var grid = state.Get<MapGrid>();
            foreach (var storage in corpse.CorpseStorage.CreatureData.Inventory.AllContainers)
            {
                foreach (var item in storage.Items.ToArray())
                {
                    storage.Remove(item);
                    if (!item.IsImplicit)
                    {
                        ItemOnFloorSystem.SpawnItem(floor, grid, item, corpse.pos);
                    }
                }
            }
        }

        internal static void BindRaisedServant(Player player, Creature servant, MapObstacle corpse)
        {
            var state = CurrentClassRaid();
            int grave = ClassPerkRank(player.CreatureData, GraveBinding);
            int greater = ClassPerkRank(player.CreatureData, GreaterVessel);
            var data = servant.CreatureData;
            data.SpawnCorpse = false;
            data.SpawnLoot = false;
            data.AllowEccolapse = false;
            data.QuasimorphosisReward = 0;
            if (servant is Monster monster)
            {
                monster.IsTransferable = false;
            }
            int turns = 8 + (grave == 0 ? 0 : grave + 1);
            data.EffectsController.Add(new ImperialBoundServant(turns), merge: false);
            ClassCounters(state)[BoundServantKey(state)] = data.UniqueId;
            if (greater > 0 && GreaterVesselSelected(state) && ClassSupply(player, BoundAsh) is { } ash)
            {
                ItemInteractionSystem.ConsumeItem(ash);
                data.Health.Reinitialize(data.Health.MaxValue + 8 + greater * 8);
                AwardClassPerk(player, state.Get<PerkFactory>(), GreaterVessel);
            }
            DropCorpseInventory(state, corpse);
            state.Get<MapObstacles>().Remove(corpse);
            AwardClassPerk(player, state.Get<PerkFactory>(), RaiseFallen);
        }

        private static void TickBoundServant(State state)
        {
            if (BoundServant(state) is not { } servant)
            {
                return;
            }
            var binding = servant.CreatureData.EffectsController.First<ImperialBoundServant>();
            if (--binding.Duration <= 0)
            {
                ExpireBoundServant(servant);
            }
        }

        private static void ExpireBoundServant(Creature servant)
        {
            servant.CreatureData.Health.ReasonOfDeath = HealthInfo.DeathReason.EccolapseBack;
            servant.CreatureData.Health.Value = 0;
        }

        private static bool CanSacrifice(State state, CellPosition position)
        {
            var player = state.Get<Creatures>().Player;
            var target = state.Get<Creatures>().GetCreature(position.X, position.Y);
            var servant = BoundServant(state);
            return ClassPerkRank(player.CreatureData, SacrificialRelease) > 0 && servant != null && target != null &&
                !target.IsAlly(player) && target.CreatureData.Health.Alive && state.Get<MapGrid>().GetCell(position)?.isSeen == true &&
                CellPosition.Distance(servant.CreatureData.Position, position) <= 1;
        }

        private static bool SacrificeServant(State state, CellPosition position)
        {
            if (!CanSacrifice(state, position))
            {
                return false;
            }
            var player = state.Get<Creatures>().Player;
            var target = state.Get<Creatures>().GetCreature(position.X, position.Y);
            var servant = BoundServant(state);
            float damage = servant.CreatureData.BaseOverallDmgMult;
            try
            {
                servant.CreatureData.BaseOverallDmgMult = damage * (1.15f + 0.10f * ClassPerkRank(player.CreatureData, SacrificialRelease));
                servant.ProcessMeleeAttackOnEnemy(target, out _, new Creature.MeleeAttackInput { Instant = true });
            }
            finally
            {
                servant.CreatureData.BaseOverallDmgMult = damage;
                ExpireBoundServant(servant);
            }
            AwardClassPerk(player, state.Get<PerkFactory>(), SacrificialRelease);
            SpendClassAction(state);
            return true;
        }

        [HarmonyPatch(typeof(Creature), "ProcessDamage")]
        private static class BoundServantFirstWound
        {
            private static void Postfix(Creature __instance, DamageHitInfo hitInfo)
            {
                if (!hitInfo.wasWound || hitInfo.damageDealer is not Monster servant || !IsBoundServant(servant.CreatureData))
                {
                    return;
                }
                var state = CurrentClassRaid();
                var player = state?.Get<Creatures>()?.Player;
                var binding = servant.CreatureData.EffectsController.First<ImperialBoundServant>();
                if (player != null && !__instance.IsAlly(player) && !binding.WoundRecorded)
                {
                    binding.WoundRecorded = true;
                    AwardClassPerk(player, state.Get<PerkFactory>(), GraveBinding);
                }
            }
        }
    }

    public sealed class ImperialBoundServant : BaseEffect
    {
        [Save] public bool WoundRecorded;

        public ImperialBoundServant() { }

        public ImperialBoundServant(int turns)
        {
            // Player turns, rather than the servant's AP, are the clock for the binding.
            Endless = true;
            Duration = OriginalDuration = turns;
        }
    }

    [Scenario("pact_iom_undivided_raise_fallen", "")]
    [NeedTargetSelection]
    public sealed class RaiseTheFallenPact : BasePactScenario
    {
        public override bool DrawSelectionHighlight(SelectTargetView drawer)
        {
            if (User is not Player player || Plugin.FindRaiseCorpse(player, Target) == null || !IsValidForSpawn(Target))
            {
                return false;
            }
            drawer.DrawSingleCell(Target, SelectTargetView.BorderColor.PactSelected);
            return true;
        }

        [Checkpoint(0, false)]
        private IEnumerator InitializeScenario()
        {
            if (User is Player player && Plugin.FindRaiseCorpse(player, Target) is { } corpse &&
                SpawnCreature(Target, player.CreatureData.CreatureAlliance) is { } servant)
            {
                Plugin.BindRaisedServant(player, servant, corpse);
            }
            yield return null;
        }
    }
}

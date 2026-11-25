using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MGSC;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private const string ChaosEssence = "iom_undivided_essence";
        private const string BoundAsh = ChaosItemPrefix + "bound_ash";
        private const short BoundAshMaxStack = 30;
        private const string ForgeEmber = ChaosItemPrefix + "forge_ember";
        private const string WarpScrip = ChaosItemPrefix + "warp_scrip";
        private const string PactPrefix = "iom_undivided_";

        private sealed class ChaosPact(string key, string name, string description, int tier, int curse, int recharge,
            PerkAIUsage ai, int cooldown, string parameters)
        {
            internal readonly string Key = key, Name = name, Description = description, Parameters = parameters;
            internal readonly int Tier = tier, Curse = curse, Recharge = recharge, Cooldown = cooldown;
            internal readonly PerkAIUsage AI = ai;
        }

        private static readonly ChaosPact[][] ChaosPacts =
        [
            [
                new("carrion_mark", "Carrion Mark", "Cast a short Chaos bolt at an enemy. A clear line of fire gives the mark its best chance to land.",
                    1, 18, 34, PerkAIUsage.TargetCell, 6,
                    "SProjectileId quasi_charge SDamageType chaos IMinDamage 17 IMaxDamage 29 IMaxDistance 5 FAccuracyBonus 0.12"),
                new("rib_cage", "Rib Cage", "Grow protective bone for six turns. Each use also sends a short shock pulse around the caster.",
                    2, 32, 60, PerkAIUsage.None, 8,
                    "IResists 12 IDuration 6 SProjectileId lighting_small SDamageType shock IMinDamage 7 IMaxDamage 15 IMaxDistance 2 IRadius 2"),
                new("sever_binding", "Sever the Binding", "A concentrated Chaos lance pierces a target and can root it briefly.",
                    3, 50, 90, PerkAIUsage.TargetCell, 9,
                    "SProjectileId quasi_charge SDamageType chaos IMinDamage 32 IMaxDamage 51 IMaxDistance 7 FPierce 0.34 FRootChance 0.5 IRootDuration 2"),
                new("excruciator_chain", "Excruciator Chain", "Send shock projectiles from the caster to enemies near the chosen target. Walls can break the chain.",
                    4, 72, 130, PerkAIUsage.None, 12,
                    "IRadius 2 SProjectileId lighting_small SDamageType shock IMinDamage 24 IMaxDamage 45 IMaxDistance 8"),
                new("crown_of_ash", "Crown of Ash", "Ignite an area with Soulforge fire. The blast can also hit your crew.",
                    5, 125, 230, PerkAIUsage.None, 20, "IRadius 3 SExplosionId mars_explosion")
            ],
            [
                new("fury_sigil", "Fury Sigil", "Call one Fury into a free adjacent cell. The summoned ally carries no bounty.",
                    1, 22, 40, PerkAIUsage.None, 8,
                    "IMaxDistance 1 SMobClass iom_chaos_fury SFaction iom_chaos_undivided BIsFollower true"),
                new("step_between", "Step Between", "Step through shadow to a free visible cell within four tiles.",
                    2, 30, 55, PerkAIUsage.None, 8, "IMaxDistance 4"),
                new("black_carapace", "Black Carapace", "Grow a shadow shell that adds 20 armor resistance for eight turns.",
                    3, 54, 100, PerkAIUsage.Always, 15, "IResists 20 IDuration 8"),
                new("soulforge_muster", "Soulforge Muster", "Call a Venomcrawler into a free cell within three tiles. It follows the caster and carries no bounty.",
                    4, 78, 140, PerkAIUsage.None, 15,
                    "IMaxDistance 3 SMobClass iom_chaos_venomcrawler SFaction iom_chaos_undivided BIsFollower true"),
                new("black_meridian", "Black Meridian", "Step up to six tiles through shadow and gain a short protective shell on arrival.",
                    5, 118, 215, PerkAIUsage.None, 18, "IMaxDistance 6 IResists 27 IDuration 5")
            ]
        ];

        private static void RegisterChaosPacts()
        {
            var ash = ChaosItem<PactComponentRecord>(BoundAsh, "demonic_eye", 2, 100);
            // The donor's pact record is supplemental: its device record supplies these defaults.
            // Bound Ash is a standalone pact component, so define its inventory behavior here.
            ash.InventoryWidthSize = 1;
            ash.MaxStack = BoundAshMaxStack;
            ash.Essence = ChaosEssence;
            ash.EssencePower = 225;
            ash.EssenceStability = 37;
            AddChaosItem("pactcomponents", ash, "Bound Ash", "Stable Undivided essence for a skull ritual.");

            var ember = ChaosItem<PactComponentRecord>(ForgeEmber, "demonic_eye", 6, 230);
            ember.Essence = ChaosEssence;
            ember.EssencePower = 410;
            ember.EssenceStability = 28;
            AddChaosItem("pactcomponents", ember, "Soulforge Ember", "Potent Undivided essence. Pair it with stable components to control the ritual.");

            StageRecord(Data.Essences, ChaosEssence, new EssenceRecord { Id = ChaosEssence, FailedRitualItemId = BoundAsh });
            AddAffinity(ChaosEssence, ChaosEssence, 1.1f, 1.05f);
            foreach (EssenceRecord native in Data.Essences.Records)
            {
                float power = native.Id == "agga" ? 1.25f : 0.90f;
                float stability = native.Id == "siaira" ? 1.30f : 0.85f;
                AddAffinity(native.Id, ChaosEssence, power, stability);
                AddAffinity(ChaosEssence, native.Id, power, stability);
            }
            StageRecord(Data.Bramfaturas, ChaosFaction, new BramfaturaRecord
            {
                Id = ChaosFaction, ItemDropCategories = [ChaosFaction], ForbiddenItemClasses = []
            });
            AddCurse(1, 1, "IDoorNoiseRadiusMin 1 IDoorNoiseRadiusMax 5", "Whispers through iron");
            AddCurse(2, 200, "FPainReceivedMin 0.05 FPainReceivedMax 0.30", "The flesh remembers");
            AddCurse(3, 400, "FEquipmentResistPenaltyMin 0.05 FEquipmentResistPenaltyMax 0.25", "Spoil the mail");
            AddCurse(4, 700, "FChaosEnemyDamageBonusPercentMin 0.05 FChaosEnemyDamageBonusPercentMax 0.20", "The host draws near");
            AddCurse(5, 1000, "ILosFlatReductionMin 3 ILosFlatReductionMax 3", "The Dark Master's eclipse");

            for (int branch = 0; branch < ChaosPacts.Length; branch++)
            {
                var path = ChaosPacts[branch];
                for (int index = 0; index < path.Length; index++)
                {
                    var pact = path[index];
                    string id = PactPrefix + pact.Key;
                    string skullId = ChaosSkullId(pact.Key);
                    var skull = ChaosItem<SkullRecord>(skullId, "skull_ron_blindness", 2 * pact.Tier - 1, 360 + 240 * pact.Tier);
                    skull.PerkId = id;
                    skull.Essence = ChaosEssence;
                    skull.Categories = [];
                    skull.CanBaronUse = pact.AI != PerkAIUsage.None;
                    skull.Upgrade = ChaosSkullId(path[Math.Min(index + 1, path.Length - 1)].Key);
                    skull.Downgrade = ChaosSkullId(path[Math.Max(index - 1, 0)].Key);
                    skull.Sidegrade = ChaosSkullId(ChaosPacts[1 - branch][index].Key);
                    ApplyChaosSkullArtwork(skull);
                    RegisterItem("skulls", skull, pact.Name + " Skull", pact.Description);

                    var descriptor = Object.Instantiate((PerkDescriptor)Data.Perks.GetRecord("ron_blindness").ContentDescriptor);
                    descriptor.name = id;
                    descriptor._icon = skull.ItemDesc.Icon;
                    descriptor._effectPanelIcon = skull.ItemDesc.Icon;
                    StageDescriptor("pacts", id, descriptor);
                    StageRecord(Data.Perks, id, new PactRecord
                    {
                        Id = id, ContentDescriptor = descriptor, BramfaturaId = ChaosFaction, PerkType = PerkType.Ultimate,
                        PactTier = pact.Tier, CurseValue = pact.Curse, RechargeValue = pact.Recharge, AIUsage = pact.AI,
                        AIParameters = Parameters("ICooldown " + pact.Cooldown + " IUsageRadius 5"),
                        Parameters = Parameters(pact.Parameters), ActiveWeaponClassLimit = [], ActiveWeaponSubClassLimit = []
                    });
                    Registrations.Add(() => TranslationPatch.AddChaosPact(id, pact.Name, pact.Description));
                }
            }

            AddMaterial(WarpScrip, "precious_metals", "Warp Scrip",
                "Dense trade salvage cut from the seals of a slain Undivided creature.", 35, 1, ChaosFaction);
            return;

            void AddAffinity(string source, string target, float power, float stability)
            {
                string id = source + "_" + target;
                StageRecord(Data.EssenceAffinities, id, new EssenceAffinityRecord
                {
                    Id = id, SourceEssenceId = source, TargetEssenceId = target, PowerMult = power, StabilityMult = stability
                });
            }

            void AddCurse(int level, int threshold, string parameters, string name)
            {
                string id = PactPrefix + "curse_" + level;
                StageRecord(Data.Curses, id, new CurseRecord
                {
                    Id = id, BramfaturaId = ChaosFaction, ActivationLevel = threshold, Parameters = Parameters(parameters)
                });
                Registrations.Add(() => TranslationPatch.AddChaosCurse(id, name));
            }
        }

        private static List<PerkParameter> Parameters(string values)
        {
            string[] tokens = values.Split(' ');
            var result = new List<PerkParameter>(tokens.Length / 2);
            for (int i = 0; i < tokens.Length; i += 2)
            {
                result.Add(PerkParameter.Parse(tokens[i] + " " + tokens[i + 1]));
            }
            return result;
        }

        private static string ChaosSkullId(string key) => ChaosItemPrefix + "skull_" + key;

        private static void ApplyChaosSkullArtwork(SkullRecord skull)
        {
            ApplyChaosItemArtwork(skull);
            var descriptor = (SkullDescriptor)skull.ItemDesc;
            descriptor._iconsOnFloor = [descriptor.SmallIcon];
            descriptor._shadowsOnFloor = [descriptor.ShadowOnFloor];
        }
    }

    public abstract class ChaosLancePact : PactTemplate_AttackEnemy_In_LOS
    {
        public override bool ApplyCondition(Creature creature) => !creature.IsAlly(User);
        public override void ProcessCreature(Creature creature) => SpawnProjectile(User, creature.CreatureData.Position);
    }

    [Scenario("pact_iom_undivided_carrion_mark", "")]
    public sealed class CarrionMarkPact : ChaosLancePact { }

    [Scenario("pact_iom_undivided_sever_binding", "")]
    public sealed class SeverBindingPact : ChaosLancePact { }

    [Scenario("pact_iom_undivided_rib_cage", "")]
    public sealed class RibCagePact : BasePactScenario
    {
        [Checkpoint(0, false)]
        private IEnumerator Cast()
        {
            User.CreatureData.EffectsController.Add(new CarapaceBuff(GetInt("IDuration"), GetInt("IResists")));
            foreach (Creature target in _creatures.Values.Where(other => other != User && !other.IsAlly(User) &&
                CellPosition.Distance(User.CreatureData.Position, other.CreatureData.Position) <= GetInt("IRadius")).ToArray())
            {
                SpawnProjectile(User, target.CreatureData.Position);
            }
            yield break;
        }
    }

    [Scenario("pact_iom_undivided_excruciator_chain", "")]
    public sealed class ExcruciatorChainPact : PactTemplate_Area_In_LOS
    {
        public override void ProcessArea(List<MapCell> areaCells, List<CellPosition> validCells, List<Creature> enemies)
        {
            foreach (Creature enemy in enemies)
            {
                SpawnProjectile(User, enemy.CreatureData.Position);
            }
        }
    }

    [Scenario("pact_iom_undivided_crown_of_ash", "")]
    public sealed class CrownOfAshPact : PactTemplate_Area_In_LOS
    {
        public override void ProcessArea(List<MapCell> areaCells, List<CellPosition> validCells, List<Creature> enemies) =>
            _explosionFactory.SpawnExplosion(User, Target, GetStr("SExplosionId"), fastForward: true,
                parameters =>
                {
                    parameters.SelfDamaging = true;
                    parameters.Radius = GetInt("IRadius");
                    return parameters;
                });
    }

    public abstract class ChaosSummoningPact : PactTemplate_SpawnAllyOnCellInLos
    {
        public override bool DrawSelectionHighlight(SelectTargetView drawer) =>
            CellPosition.Distance(User.CreatureData.Position, Target) <= GetInt("IMaxDistance") &&
            base.DrawSelectionHighlight(drawer);
    }

    [Scenario("pact_iom_undivided_fury_sigil", "")]
    public sealed class FurySigilPact : ChaosSummoningPact { }

    [Scenario("pact_iom_undivided_soulforge_muster", "")]
    public sealed class SoulforgeMusterPact : ChaosSummoningPact { }

    [NeedTargetSelection]
    public abstract class ChaosShadowStepPact : BasePactScenario
    {
        private bool CanStep => User != null && !User.CreatureData.Health.Dead && IsValidForSpawn(Target) &&
            CellPosition.Distance(User.CreatureData.Position, Target) <= Math.Min(GetInt("IMaxDistance"), GetUserLos());

        [Checkpoint(0, false)]
        protected IEnumerator Cast()
        {
            if (CanStep)
            {
                CreatureSystem.UpdatePosition(_mapRenderer, User, Target);
                if (GetInt("IDuration") > 0)
                {
                    User.CreatureData.EffectsController.Add(new CarapaceBuff(GetInt("IDuration"), GetInt("IResists")));
                }
                if (User is Player)
                {
                    MapSystem.RefreshFogAndMinimap(_mapController, _creatures, _itemsOnFloor, _mapObstacles, _mapRenderer,
                        _mapEntities, _mapGrid, _fireController, _visibilities);
                    _gameCamera.CenterOnFollowTarget();
                }
            }
            yield break;
        }

        public override bool DrawSelectionHighlight(SelectTargetView drawer)
        {
            if (!CanStep)
            {
                return false;
            }
            drawer.DrawSingleCell(Target, SelectTargetView.BorderColor.PactSelected);
            return true;
        }
    }

    [Scenario("pact_iom_undivided_step_between", "")]
    public sealed class StepBetweenPact : ChaosShadowStepPact { }

    [Scenario("pact_iom_undivided_black_meridian", "")]
    public sealed class BlackMeridianPact : ChaosShadowStepPact { }

    [Scenario("pact_iom_undivided_black_carapace", "")]
    public sealed class BlackCarapacePact : BasePactScenario
    {
        [Checkpoint(0, false)]
        private IEnumerator Cast()
        {
            User.CreatureData.EffectsController.Add(new CarapaceBuff(GetInt("IDuration"), GetInt("IResists")));
            yield break;
        }
    }
}

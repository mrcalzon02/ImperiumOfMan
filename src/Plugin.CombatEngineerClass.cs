using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MGSC;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private const string EngineerKit = "engineering_repair_kit";
        private const string WreckPart = "coarse_parts";

        private sealed class EngineerMachine(string role, string package, string title, bool mobile, int rank)
        {
            internal readonly string Role = role;
            internal readonly string Package = package;
            internal readonly string Title = title;
            internal readonly bool Mobile = mobile;
            internal readonly int Rank = rank;
            internal string Recovered => "iom_recovered_" + Role;
            internal string Wreck => "iom_wreck_" + Role;
        }

        private static readonly EngineerMachine[] EngineerMachines =
        [
            new("cyborg_worker", "packaged_industrial_robot", "Industrial Robot", true, 1),
            new("cyborg_battle", "packaged_security_robot", "Security Robot", true, 3),
            new("drone_security", "packaged_drone", "Security Drone", true, 4),
            new("turret_ballistic", "stationary_turret", "Ballistic Turret", false, 4)
        ];

        private static void RegisterRecoveredChassis()
        {
            var wreckDonor = Data.Items.GetSimpleRecord<ItemRecord>("rusty_plates").ItemDesc;
            foreach (var machine in EngineerMachines)
            {
                var package = CloneItem<PlaceableDeviceRecord>(machine.Recovered, machine.Package, 5, 240);
                package.Categories = [];
                package.MaxStack = 1;
                package.Disassembly = [];
                RegisterItem("placeable_devices", package, "Recovered " + machine.Title,
                    "A recoverable automata chassis. Deployment preserves the damage and equipment recorded when it was packed.");

                var descriptor = UnityEngine.Object.Instantiate(wreckDonor);
                descriptor.name = machine.Wreck;
                var wreck = new TrashRecord
                {
                    Id = machine.Wreck, ContentDescriptor = descriptor, Categories = [], TechLevel = 5, Price = 90,
                    Weight = package.Weight, InventoryWidthSize = package.InventoryWidthSize, ItemClass = ItemClass.Parts,
                    MaxStack = 1, SubType = TrashSubtype.BartherResource
                };
                RegisterItem("trash", wreck, machine.Title + " Wreck", "An intact machine frame for refurbishment on the Magnum.");
                IndustryRecipe(machine.Recovered, 12, 1, new(machine.Wreck, 1), new(Fittings, 1), new(LogicCore, 1));
            }
            var schematic = CloneItem<DatadiskRecord>("iom_automata_refurbishment_chip", "low_chip", 5, 260, FieldPlans);
            schematic.Categories = [ItemCategory];
            RegisterCatalog(schematic, "Automata Refurbishment Schematic", "Patterns for rebuilding recovered Industrial Robots, Security Robots, Security Drones, and Ballistic Turrets.",
                ItemCategory, EngineerMachines.Select(machine => machine.Recovered).ToArray(), supplier: HavocStation);
        }

        private static bool RecoverableMachine(CreatureData creature, out EngineerMachine machine)
        {
            machine = EngineerMachines.FirstOrDefault(candidate => candidate.Role == creature.MobClassId);
            return machine != null && creature.CreatureClass == CreatureClass.Cyborg;
        }

        private static BasePickupItem FirstDoorwayPackage(Player player) => player.CreatureData.Inventory.AllContainers
            .SelectMany(storage => storage.Items).FirstOrDefault(item => !item.Locked && !item.IsUseRestricted && EngineerMachines.Any(machine =>
                machine.Mobile && (machine.Package == item.Id || machine.Recovered == item.Id)));

        private static string DoorwayMachineTitle(BasePickupItem package) => EngineerMachines.First(machine =>
            machine.Package == package.Id || machine.Recovered == package.Id).Title;

        private static bool EngineerTargetValid(State state, ClassAction action, CellPosition position)
        {
            var player = state.Get<Creatures>().Player;
            if (state.Get<MapGrid>().GetCell(position)?.isSeen != true || CellPosition.Distance(player.CreatureData.Position, position) != 1)
            {
                return false;
            }
            var monster = state.Get<Creatures>().GetCreature(position.X, position.Y) as Monster;
            var machine = monster != null && monster.IsAlly(player) && monster.CreatureData.Health.Alive &&
                !MissionSystem.IsQuestMonster(state.Get<RaidMetadata>(), monster) && RecoverableMachine(monster.CreatureData, out var eligible)
                ? eligible : null;
            return action switch
            {
                ClassAction.Service => machine != null && ClassPerkRank(player.CreatureData, FieldService) > 0 &&
                    ClassSupply(player, EngineerKit) is { IsUsable: true } kit &&
                    (!monster.CreatureData.Health.IsFull || MechanicalWound(monster, kit) != null),
                ClassAction.Recover => machine != null && ClassPerkRank(player.CreatureData, RecoverChassis) >= machine.Rank &&
                    player.ActionPointsLeft >= PackCost(player) && monster.CreatureData.EffectsController.First<ConvertAllianceEffect>() == null,
                ClassAction.Deploy => ClassPerkRank(player.CreatureData, DoorwayDeployment) > 0 && FirstDoorwayPackage(player) != null &&
                    Doorway(state, position, out _) != null && player.ActionPointsLeft >= (player.FreeInteractObstacles ? 1 : 2),
                ClassAction.Reclaim => ClassSupply(player, WreckPart) != null && FindReclaimableWreck(state, position, out _) != null,
                _ => false
            };
        }

        private static int PackCost(Player player) => ClassPerkRank(player.CreatureData, RecoverChassis) >= 3 ? 1 : 2;

        private static WoundSlotRecord MechanicalWound(Monster monster, BasePickupItem kit) => monster.CreatureData.EffectsController
            .OfType<BodyPartWound>().Where(wound => !wound.IsFixated).Select(wound => wound.WoundSlotRecord)
            .FirstOrDefault(slot => slot != null && ItemInteractionSystem.CanFixate(slot, kit, monster));

        private static bool UseEngineerAction(State state, ClassAction action, CellPosition position)
        {
            if (!EngineerTargetValid(state, action, position))
            {
                return false;
            }
            var creatures = state.Get<Creatures>();
            var player = creatures.Player;
            var monster = creatures.GetCreature(position.X, position.Y) as Monster;
            return action switch
            {
                ClassAction.Service => ServiceMachine(state, player, monster),
                ClassAction.Recover => PackMachine(state, player, monster),
                ClassAction.Deploy => DeployThroughDoor(state, position),
                ClassAction.Reclaim => ReclaimWreck(state, player, position),
                _ => false
            };
        }

        private static bool ServiceMachine(State state, Player player, Monster monster)
        {
            var kit = ClassSupply(player, EngineerKit);
            var slot = MechanicalWound(monster, kit);
            var health = monster.CreatureData.Health;
            int rank = ClassPerkRank(player.CreatureData, FieldService);
            if (slot == null)
            {
                health.Restore(5 + 2 * rank);
                ItemInteractionSystem.ConsumeItem(kit);
            }
            else
            {
                if (!ItemInteractionSystem.TryFixate(slot, kit, monster))
                {
                    return false;
                }
                health.Restore(2 * rank);
            }
            AwardClassPerk(player, state.Get<PerkFactory>(), FieldService);
            SpendClassAction(state);
            return true;
        }

        private static void CarryOrDropClassItem(State state, Player player, BasePickupItem item)
        {
            if (!player.CreatureData.Inventory.TakeOrEquip(item))
            {
                ItemOnFloorSystem.SpawnItem(state.Get<ItemsOnFloor>(), state.Get<MapGrid>(), item, player.CreatureData.Position);
            }
        }

        private static bool PackMachine(State state, Player player, Monster monster)
        {
            if (!RecoverableMachine(monster.CreatureData, out var machine) ||
                SingletonMonoBehaviour<ItemFactory>.Instance.CreateForInventory(machine.Recovered) is not PickupItem package)
            {
                return false;
            }
            var data = monster.CreatureData;
            var identity = data.EffectsController.First<ImperialMachineIdentity>();
            if (identity == null)
            {
                identity = new ImperialMachineIdentity { Serial = Guid.NewGuid().ToString("N") };
                data.EffectsController.Add(identity, merge: false);
            }
            package.Add(new ImperialRecoveredMachine
            {
                Health = data.Health.Value, MaxHealth = data.Health.MaxValue, DamageMultiplier = data.BaseOverallDmgMult,
                Inventory = data.Inventory, Effects = data.EffectsController, WoundSlots = data.WoundSlotMap,
                Augmentations = data.AugmentationMap
            });
            var creatures = state.Get<Creatures>();
            var position = data.Position;
            CreatureSystem.DespawnCreature(state.Get<TurnController>(), creatures, monster);
            CreatureSystem.RefreshMonstersAndEntitiesFlags(state.Get<MapGrid>(), creatures, state.Get<MapEntities>(), position, null);
            CarryOrDropClassItem(state, player, package);
            var counters = ClassCounters(state);
            string key = "iom.class.pack." + identity.Serial;
            if (!counters.ContainsKey(key))
            {
                counters[key] = 1;
                AwardClassPerk(player, state.Get<PerkFactory>(), RecoverChassis);
            }
            SpendClassAction(state, PackCost(player));
            return true;
        }

        [HarmonyPatch(typeof(ItemInteractionSystem), "SpawnPlaceableMonster")]
        private static class RestoreRecoveredMachine
        {
            private static void Postfix(BasePickupItem item, Monster __result)
            {
                var saved = item.Comp<ImperialRecoveredMachine>();
                if (saved == null || __result == null)
                {
                    return;
                }
                var data = __result.CreatureData;
                data.WoundSlotMap = saved.WoundSlots;
                data.AugmentationMap = saved.Augmentations;
                __result.SetInventory(saved.Inventory);
                __result.SetEffectsController(saved.Effects);
                data.Health.MaxValue = saved.MaxHealth;
                data.Health.Value = Math.Max(1, Math.Min(saved.Health, saved.MaxHealth));
            }
        }

        [HarmonyPatch(typeof(ItemInteractionSystem), nameof(ItemInteractionSystem.TryPlaceObject))]
        private static class PreserveRecoveredMachineBonuses
        {
            private static void Postfix(Creatures creatures, BasePickupItem item, CellPosition position, bool __result)
            {
                if (!__result || item.Comp<ImperialRecoveredMachine>() is not { } saved ||
                    creatures.GetCreature(position.X, position.Y) is not Monster monster)
                {
                    return;
                }
                // Native placement initializes eligible turret bonuses. Restore the already applied condition once.
                monster.CreatureData.BaseOverallDmgMult = saved.DamageMultiplier;
                monster.CreatureData.Health.MaxValue = saved.MaxHealth;
                monster.CreatureData.Health.Value = Math.Max(1, Math.Min(saved.Health, saved.MaxHealth));
            }
        }

        private static Door Doorway(State state, CellPosition position, out CellPosition destination)
        {
            var creatures = state.Get<Creatures>();
            var origin = creatures.Player.CreatureData.Position;
            int dx = position.X - origin.X;
            int dy = position.Y - origin.Y;
            destination = new CellPosition(position.X + dx, position.Y + dy);
            var obstacle = state.Get<MapObstacles>().Get<Door>(position.X, position.Y);
            return Math.Abs(dx) + Math.Abs(dy) == 1 && obstacle?.Door is { State: Door.DoorState.Closed } door &&
                obstacle.ObjectiveObstacle == null && PlayerInteractionSystem.CanInteractObstacles(creatures, state.Get<Scenarios>(), obstacle, out _)
                ? door : null;
        }

        private static bool DeployThroughDoor(State state, CellPosition position)
        {
            var creatures = state.Get<Creatures>();
            var player = creatures.Player;
            var door = Doorway(state, position, out var destination);
            var package = FirstDoorwayPackage(player);
            if (door == null || package == null)
            {
                return false;
            }
            PlayerInteractionSystem.OpenTheDoor(creatures, state.Get<Scenarios>(), state.Get<TurnMetadata>(), door);
            if (door.State != Door.DoorState.Opened)
            {
                return false;
            }
            var grid = state.Get<MapGrid>();
            var cell = grid.GetCell(destination);
            if (cell == null || !cell.isSeen || !cell.IsExplored || cell.IsDoor ||
                !ItemInteractionSystem.CanPlaceObject(grid, creatures, state.Get<MapEntities>(), destination))
            {
                return true; // The door opened normally. Keep the package and the door action cost.
            }
            var perks = state.Get<PerkFactory>();
            if (!ItemInteractionSystem.TryPlaceObject(state.Get<Mercenaries>(), perks, state.Get<Difficulty>(), creatures,
                    state.Get<RaidMetadata>(), state.Get<TurnController>(), state.Get<MapEntities>(), state.Get<MapObstacles>(),
                    player, package, destination, playSounds: true))
            {
                return true;
            }
            if (creatures.GetCreature(destination.X, destination.Y) is Monster monster &&
                monster.Behaviour.TryGetTimer(AITimerType.Loading, out var timer) && timer is LoadingAITimer loading)
            {
                loading.LoadingTurns = Math.Max(0, loading.LoadingTurns - (ClassPerkRank(player.CreatureData, DoorwayDeployment) - 1));
            }
            if (package.Record<PlaceableDeviceRecord>().ApplyTurretBonus)
            {
                player.RaisePerkAction(PerkLevelUpActionType.PlaceTurret);
            }
            AwardClassPerk(player, perks, DoorwayDeployment);
            SpendClassAction(state, player.FreeInteractObstacles ? 1 : 2);
            return true;
        }

        private static MapObstacle FindReclaimableWreck(State state, CellPosition position, out EngineerMachine machine)
        {
            var player = state.Get<Creatures>().Player;
            int rank = ClassPerkRank(player.CreatureData, WreckReclamation);
            foreach (var obstacle in state.Get<MapObstacles>().RaycastMapObstacles(position.X, position.Y))
            {
                if (obstacle.CorpseStorage is not { } corpse || !RecoverableMachine(corpse.CreatureData, out var candidate) || rank < candidate.Rank)
                {
                    continue;
                }
                int eligibility = CorpseEligibility(state, corpse);
                if (eligibility == 1 || (eligibility == 2 && rank >= 2))
                {
                    machine = candidate;
                    return obstacle;
                }
            }
            machine = null;
            return null;
        }

        [HarmonyPatch(typeof(AmputationSystem), nameof(AmputationSystem.AmputateCorpse))]
        private static class MarkStrippedWreck
        {
            private static void Postfix(CorpseStorage corpseStorage, string slotToAmputate, bool __result)
            {
                var state = CurrentClassRaid();
                if (__result && state != null && RecoverableMachine(corpseStorage.CreatureData, out _) &&
                    Data.WoundSlots.GetRecord(slotToAmputate)?.SlotType is "Head" or "Chest" or "Body")
                {
                    ClassCounters(state)[ClassFloor(state) + CorpseRuleKey(corpseStorage.CreatureData.UniqueId)] = 0;
                }
            }
        }

        private static bool ReclaimWreck(State state, Player player, CellPosition position)
        {
            var corpse = FindReclaimableWreck(state, position, out var machine);
            var part = ClassSupply(player, WreckPart);
            if (corpse == null || part == null ||
                SingletonMonoBehaviour<ItemFactory>.Instance.CreateForInventory(machine.Wreck) is not { } wreck)
            {
                return false;
            }
            DropCorpseInventory(state, corpse);
            state.Get<MapObstacles>().Remove(corpse);
            ItemInteractionSystem.ConsumeItem(part);
            CarryOrDropClassItem(state, player, wreck);
            AwardClassPerk(player, state.Get<PerkFactory>(), WreckReclamation);
            SpendClassAction(state);
            return true;
        }
    }

    public sealed class ImperialMachineIdentity : BaseEffect
    {
        [Save] public string Serial;

        public ImperialMachineIdentity() => Endless = true;
    }

    public sealed class ImperialRecoveredMachine : PickupItemComponent
    {
        [Save] public int Health;
        [Save] public int MaxHealth;
        [Save] public float DamageMultiplier;
        [Save] public Inventory Inventory;
        [Save] public EffectsController Effects;
        [Save] public Dictionary<string, ImplantSocketData> WoundSlots;
        [Save] public Dictionary<string, string> Augmentations;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using MGSC;
using Object = UnityEngine.Object;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private const string NecromancerClass = "iom_necromancer";
        private const string EngineerClass = "iom_combat_engineer";
        private const string VigilsClass = "iom_vigils";
        internal const string RaiseFallen = "iom_raise_fallen";
        internal const string GraveBinding = "iom_grave_binding";
        internal const string SacrificialRelease = "iom_sacrificial_release";
        internal const string GreaterVessel = "iom_greater_vessel";
        private const string FieldService = "iom_field_service";
        private const string RecoverChassis = "iom_recover_chassis";
        private const string DoorwayDeployment = "iom_doorway_deployment";
        private const string WreckReclamation = "iom_wreck_reclamation";
        private const string VigilWard = "iom_vigil_ward";
        private const string SealBreach = "iom_seal_the_breach";
        private const string Banishment = "iom_banishment";

        private static readonly string[] ClassPerkGrades = ["basic", "advanced", "master", "legend"];
        private static bool _customClassesReady;

        private static void RegisterMercenaryClasses()
        {
            RegisterClassPerk(RaiseFallen, "Raise the Fallen", "possession_basic",
                "Unlocks an Undivided pact that raises a chosen hostile corpse as a bound Fury. Only one Fury can be bound at a time.");
            RegisterClassPerk(GraveBinding, "Grave Binding", "ritualism_basic",
                "Furies raised with Raise the Fallen remain bound longer. Earn experience when a bound Fury first wounds an enemy.");
            RegisterClassPerk(SacrificialRelease, "Sacrificial Release", "cannibalism_basic",
                "Order a bound Fury to deliver one final strike against an adjacent enemy. The servant expires after the strike.");
            RegisterClassPerk(GreaterVessel, "Greater Vessel", "insanity_basic",
                "Spend Bound Ash during Raise the Fallen to bind a Fury with more health.");
            RegisterClassPerk(FieldService, "Field Service", "first_aid_basic",
                "Use an Engineering Repair Kit to treat a damaged friendly machine in the field.");
            RegisterClassPerk(RecoverChassis, "Recover Chassis", "stationary_defense_basic",
                "Pack a friendly robot into a recoverable chassis. Its damage carries over when redeployed.");
            RegisterClassPerk(DoorwayDeployment, "Doorway Deployment", "military_training_basic",
                "Open an unlocked door and send a packaged robot inside while the clone stays outside.");
            RegisterClassPerk(WreckReclamation, "Wreck Reclamation", "marauder_basic",
                "Recover the chassis of an eligible wreck. Repair it aboard the Magnum before redeploying.");
            RegisterClassPerk(VigilWard, "Vigil Ward", "shielding_basic",
                "Place a ward once per floor. It slows ambient Quasimorphosis buildup while you hold within two tiles.");
            RegisterClassPerk(SealBreach, "Seal the Breach", "gear_maintenance_basic",
                "Spend a Vigil Ward charge to stop an ordinary emergence within the ward while you hold it.");
            RegisterClassPerk(Banishment, "Banishment", "enemy_anatomy_basic",
                "Wound a hostile Bramfatura creature within a Vigil Ward to mark it for a stronger follow-up hit.");

            RegisterClass(NecromancerClass, "Necromancer", "doppelganger_pack", 3,
                "Binds fallen enemies to Chaos Undivided and directs their daemonic servants.",
                Basic(RaiseFallen), Basic(GraveBinding), Basic(SacrificialRelease), Basic(GreaterVessel), "possession_basic", "ritualism_basic");
            RegisterClass(EngineerClass, "Combat Engineer", "golem_group", KriegTier,
                "Services friendly machines and recovers their damaged chassis for another raid.",
                Basic(FieldService), Basic(RecoverChassis), Basic(DoorwayDeployment), Basic(WreckReclamation),
                "stationary_defense_basic", "gear_maintenance_basic");
            RegisterClass(VigilsClass, "Vigils", "phoenix_brigade", TitanContractTier,
                "Holds warded ground and fights the creatures that emerge from the Bramfatura.",
                Basic(VigilWard), Basic(SealBreach), Basic(Banishment), "shielding_basic", "cold_weapon_wielding_basic", "first_aid_basic");

            RegisterRaisePact();
            RegisterRecoveredChassis();
            Registrations.Add(() => _customClassesReady = true);
        }

        private static string Basic(string perk) => perk + "_basic";
        private static string ClassChip(string id) => id + "_chip";

        private static void RegisterClass(string id, string title, string donor, int tier, string description, params string[] perks)
        {
            var record = Data.MercenaryClasses.GetRecord(donor).Clone(id);
            var descriptor = Object.Instantiate((MercenaryClassDescriptor)record.ContentDescriptor);
            descriptor.name = id;
            record.ContentDescriptor = descriptor;
            record.PerkIds = [.. perks];
            StageRecord(Data.MercenaryClasses, id, record);
            StageDescriptor("mercenary_classes", id, descriptor);
            Registrations.Add(() => TranslationPatch.AddMercenaryClass(id, title, description));

            var chip = CloneItem<DatadiskRecord>(ClassChip(id), "class_chip", tier, 350);
            chip.Categories = [];
            chip.UnlockIds = [id];
            RegisterItem("datadisks", chip, title + " Class Chip", "Unlocks the " + title + " class on the Magnum.");
        }

        private static void RegisterClassPerk(string id, string title, string donor, string description)
        {
            var descriptor = Object.Instantiate((PerkDescriptor)Data.Perks.GetRecord(donor).ContentDescriptor);
            descriptor.name = id;
            Registrations.Add(() => TranslationPatch.AddImplantPerk(id, title, description));
            for (int rank = 0; rank < ClassPerkGrades.Length; rank++)
            {
                string perkId = id + "_" + ClassPerkGrades[rank];
                StageDescriptor("perks", perkId, descriptor);
                StageRecord(Data.Perks, perkId, new PerkRecord
                {
                    Id = perkId, ContentDescriptor = descriptor, PerkType = PerkType.Passive,
                    ActiveWeaponClassLimit = [], ActiveWeaponSubClassLimit = [], Parameters = [], AIParameters = [],
                    ExpPointsPerAction = 10, ExpToLevelUp = rank == 3 ? 0 : 60 + rank * 60,
                    NextPerkId = rank == 3 ? string.Empty : id + "_" + ClassPerkGrades[rank + 1]
                });
                Registrations.Add(() => TranslationPatch.AddImplantPerk(perkId, title, description));
            }
        }

        internal static int ClassPerkRank(CreatureData creature, string id)
        {
            foreach (var perk in creature.Perks)
            {
                ParseHelper.GetGradeByPerkId(perk.PerkId, out int rank, out string tag, out _);
                if (tag == id)
                {
                    return rank;
                }
            }
            return 0;
        }

        internal static void AwardClassPerk(Player player, PerkFactory factory, string id)
        {
            int rank = ClassPerkRank(player.CreatureData, id);
            if (rank is > 0 and < 4)
            {
                PerkSystem.AddExpToMultiplePerks([id + "_" + ClassPerkGrades[rank - 1]], 10, player.Mercenary, factory);
            }
        }

        [HarmonyPatch(typeof(MercenarySystem), nameof(MercenarySystem.FillStartMercsAndClasses))]
        private static class ExcludeFactionClassesFromStart
        {
            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var getter = AccessTools.PropertyGetter(typeof(ConfigRecordCollection<MercenaryClassRecord>), nameof(ConfigRecordCollection<MercenaryClassRecord>.Ids));
                var replacement = AccessTools.Method(typeof(ExcludeFactionClassesFromStart), nameof(StartingCandidates));
                foreach (var instruction in instructions)
                {
                    if (instruction.Calls(getter))
                    {
                        instruction.opcode = OpCodes.Call;
                        instruction.operand = replacement;
                    }
                    yield return instruction;
                }
            }

            private static IEnumerable<string> StartingCandidates(ConfigRecordCollection<MercenaryClassRecord> classes) =>
                classes.Ids.Where(id => id is not (NecromancerClass or EngineerClass or VigilsClass));
        }

        [HarmonyPatch(typeof(MissionSystem), nameof(MissionSystem.ProcessFinishedDungeonData))]
        private static class GrantFactionClassChip
        {
            private static void Prefix(Missions missions, Mercenaries mercenaries, MagnumCargo magnumCargo, MagnumProgression magnumSpaceship,
                Factions factions, RaidMetadata raidMetadata, DungeonFinishedData finishedData)
            {
                if (!_customClassesReady || finishedData.Reason != GameFinishedReason.MissionFinished ||
                    !raidMetadata.WinCondition.WinConditionsPassed || raidMetadata.RaidType is not (RaidType.ProcMission or RaidType.StoryMission))
                {
                    return;
                }
                var mission = missions.Get(raidMetadata);
                if (mission == null)
                {
                    return;
                }
                var sponsor = factions.Get(mission.BeneficiaryFactionId);
                string id = EligibleClass(mission, sponsor);
                if (id == null || mercenaries.UnlockedClasses.Contains(id))
                {
                    return;
                }
                string chip = ClassChip(id);
                bool HasChip(ItemStorage storage) => storage != null && storage.Items.Any(item => item.Id == chip);
                if (mission.RewardItems.Any(item => item.Id == chip) || magnumCargo.ShipCargo.Any(HasChip) ||
                    HasChip(magnumCargo.RecyclingStorage) || HasChip(magnumCargo.FridgeStorage) ||
                    HasChip(magnumSpaceship.GetDepartment<AutonomousCapsuleDepartment>()?.CapsuleStorage) ||
                    HasChip(magnumSpaceship.GetDepartment<ShuttleCargoDepartment>()?.ShuttleCargo) ||
                    mercenaries.Values.Any(merc => merc.CreatureData.Inventory.AllContainers.Any(HasChip)))
                {
                    return;
                }
                MissionSystem.AddReward(SingletonMonoBehaviour<ItemFactory>.Instance, mission, chip);
            }

            private static string EligibleClass(Mission mission, Faction sponsor)
            {
                if (sponsor?.Id == ChaosFaction && sponsor.CurrentTechLevel >= 3 &&
                    sponsor.PlayerReputation + mission.BeneficiaryReputationDelta >= CampaignMinReputation)
                {
                    return NecromancerClass;
                }
                if (sponsor?.Id != ItemCategory || mission.IsStoryMission)
                {
                    return null;
                }
                if (mission.StationId == HavocStation && mission.ProcMissionType == ProceduralMissionType.Control &&
                    sponsor.CurrentTechLevel >= KriegTier && mission.LocationPlans.Values.Any(plan => plan.MonstersTableIds.Contains(HavocKriegTable)))
                {
                    return EngineerClass;
                }
                return mission.StationId == TitanFortress && mission.ProcMissionType == ProceduralMissionType.Elimination &&
                    sponsor.CurrentTechLevel >= TitanContractTier && mission.LocationPlans.Values.Any(plan => plan.MonstersTableIds.Contains(TitanPaladinPatrol))
                    ? VigilsClass : null;
            }
        }
    }
}

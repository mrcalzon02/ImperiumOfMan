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
        private const string DataRack = "iom_data_rack";
        private const string RackBasicChip = "low_chip";
        private const string RackArtworkDonor = "electrical_parts_container";
        private const int DataRackHours = 48;
        private const int RackMinChips = 5;
        private const int RackMaxChips = 8;
        private const int RackLootPercent = 12;
        private static readonly string[] RackIngredients =
            ["communication_relay", "quasi_sensor_device", "research_disk", AiModule];

        // The native faction disks are tier 10. Do not put them into the
        // rack: make separate basic disks instead. Fallback patterns come
        // exclusively from the game's existing basic-item chip pool.
        private static readonly (string Faction, string Source, string Title, string[] Basics)[] RackFactionArchives =
        [
            ("AnCom", "anc_chip", "AnCom", ["pmc_smg_1", "pmc_shotgun_1", "pmc_assault_1"]),
            ("RealWare", "rwa_chip", "RealWare", ["military_pistol_2", "military_shotgun_1", "army_sniper_1"]),
            ("DaydreamChems", "ddr_chip", "Daydream Chems", ["medical_glue", "goldenstar_balm", "syringe_antidote"]),
            ("CoreWard", "cor_chip", "Coreward", ["worker_torch_1", "miner_breaker_1", "worker_discthrower_1"]),
            ("SBN", "sbn_chip", "SBN", ["rail_pistol_1", "servo_backpack_1", "science_reinforced_shirt_1"]),
            ("Dilthey", "dil_chip", "Dilthey", ["stun_grenade", "rail_pistol_1", "science_reinforced_shirt_1"]),
            ("Grasshopper", "gra_chip", "Grasshopper", ["trucker_pistol_2", "trucker_shotgun_1", "trucker_marksman_1"]),
            ("Tianming", "tia_chip", "Tianming", ["throwing_knife", "common_axe_1", "common_assault_1"]),
            ("PlanetBridge", "plb_chip", "PlanetBridge", ["worker_pistol_1", "medium_backpack_1", "trucker_pistol_2"]),
            ("FrancheComte", "fra_chip", "Franche-Comte", ["police_club_1", "pmc_pistol_2", "pmc_common_vest_1"]),
            ("Sunlight", "sun_chip", "Sunlight Coven", ["common_pistol_2", "armored_vest_1", "common_shotgun_1"]),
            ("ChurchRevelation", "chu_chip", "Church of Revelation", ["trucker_pistol_2", "vodka_bottle_1", "military_mask_1"])
        ];
        private static readonly HashSet<string> RackHumanCatalogFactions = new(StringComparer.Ordinal)
        {
            ItemCategory, "AnCom", "RealWare", "DaydreamChems", "CoreWard", "SBN",
            "Dilthey", "Grasshopper", "Tianming", "PlanetBridge", "FrancheComte", "Sunlight", "ChurchRevelation"
        };
        private static readonly System.Random RackRandom = new();
        private static string[][] _rackFactionPools = Array.Empty<string[]>();
        private static bool _dataRackRegistered;
        private static bool _dataRackReady;
        private static IEnumerable<string> DataRackPossibleChips =>
            new[] { RackBasicChip }.Concat(_rackFactionPools.SelectMany(pool => pool))
                .Distinct(StringComparer.Ordinal);

        private static void RegisterDataRack()
        {
            // Check native records on the installed game version before staging.
            if (RackIngredients.Any(id => Data.Items.GetRecord(id) == null) ||
                Data.Items.GetRecord(RackArtworkDonor) is not CompositeItemRecord donor ||
                donor.GetRecord<TrashRecord>() is not TrashRecord nativeCargo ||
                nativeCargo.ItemDesc == null || !IsUsableRackChip(RackBasicChip) ||
                Data.Items.GetRecord(DataRack) != null ||
                Data.ProduceReceipts.Any(recipe => recipe.OutputItem == DataRack || recipe.Id == DataRack))
            {
                Debug.LogWarning("[Cvar_ImperiumOfMan] Data Rack skipped: missing native ingredients, cargo donor, basic chip, or conflicting ID.");
                return;
            }

            RegisterBasicFactionArchives();

            // Native footprint, authored inventory/floor/shadow art.
            var rack = CloneItem<TrashRecord>(DataRack, RackArtworkDonor, 3, 650, DataRack, 24);
            rack.MaxStack = 1;
            rack.Weight = 1.4f;
            // Enable native disassembly. One basic chip is a guaranteed minimum;
            // actual opening replaces this with the complete random contents.
            rack.Disassembly = [new(RackBasicChip, 1)];
            RegisterItem("trash", rack, "Data Rack",
                "Sealed fabrication archive. Disassemble or recycle to recover 5–8 random basic and ordinary faction item chips. Consumed on opening.");

            // Exact effective cost/yield; do not use the halved-cost/doubled-output helper.
            SharedIndustryRecipes.Add(DataRack);
            IndustryBatches.Add(DataRack, 1);
            Registrations.Add(() =>
            {
                Data.ProduceReceipts.Add(new ItemProduceReceipt
                {
                    Id = DataRack, OutputItem = DataRack,
                    RequiredItems = [.. RackIngredients.Select(id => new ItemQuantity(id, 1))],
                    ProduceTimeInHours = DataRackHours,
                    ModifyStartCost = 1, ModifyStep = 1.05f, ModifyItemsGrades = []
                });
                _dataRackRegistered = true;
            });
        }

        private static bool IsUsableRackChip(string id)
        {
            return Data.Items.GetRecord(id) is CompositeItemRecord composite &&
                composite.GetRecord<DatadiskRecord>() is DatadiskRecord disk &&
                disk.UnlockType == DatadiskUnlockType.ProductionItem &&
                disk.UnlockIds != null && disk.UnlockIds.Count > 0;
        }

        private static void RegisterBasicFactionArchives()
        {
            var basic = Data.Items.GetSimpleRecord<DatadiskRecord>(RackBasicChip);
            var basicOutputs = new HashSet<string>(basic.UnlockIds, StringComparer.Ordinal);
            var nativeReceipts = new HashSet<string>(Data.ProduceReceipts.Select(receipt => receipt.OutputItem),
                StringComparer.Ordinal);
            bool SafeOutput(string id) => nativeReceipts.Contains(id) && !LicensedProducts.Contains(id) &&
                !OccultRecipeRequirements.ContainsKey(id) && Data.Items.GetRecord(id) != null;

            foreach (var archive in RackFactionArchives)
            {
                if (Data.Factions.GetRecord(archive.Faction) == null || !IsUsableRackChip(archive.Source))
                    continue;
                string id = "iom_basic_" + archive.Source;
                if (Data.Items.GetRecord(id) != null || IndustryCatalogs.ContainsKey(id)) continue;
                var native = Data.Items.GetSimpleRecord<DatadiskRecord>(archive.Source);
                // Prefer genuinely low-tech faction-specific recipes, but the
                // native archive may contain only advanced equipment. In that
                // case use the faction's explicitly themed common basics.
                var outputs = native.UnlockIds.Where(output =>
                    SafeOutput(output) && Data.Items.GetSimpleRecord<ItemRecord>(output) is ItemRecord item &&
                    item.TechLevel <= 2 && !IsTrackedOccultPattern(output)).Distinct().ToArray();
                // Two native disks are shared with occult successors. For those
                // use only the ordinary common basics, never the shared pool.
                if (archive.Source == "chu_chip" || archive.Source == "sun_chip")
                    outputs = Array.Empty<string>();
                if (outputs.Length == 0)
                    outputs = archive.Basics.Where(output => basicOutputs.Contains(output) && SafeOutput(output))
                        .Distinct().ToArray();
                if (outputs.Length == 0) continue;

                var chip = CloneItem<DatadiskRecord>(id, archive.Source, 2, 300);
                chip.Categories = [archive.Faction];
                RegisterCatalog(chip, archive.Title + " Basic Item Chip",
                    "A recovered basic fabrication pattern issued by " + archive.Title + ".",
                    archive.Faction, outputs, restrictOtherSources: false);
            }
        }

        private static void InitializeDataRackPool()
        {
            if (!_dataRackRegistered) return;
            var groups = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
            void Include(string faction, string chip)
            {
                if (chip == RackBasicChip || IsTrackedOccultPattern(chip) || !IsUsableRackChip(chip)) return;
                if (!groups.TryGetValue(faction, out var entries))
                    groups.Add(faction, entries = new HashSet<string>(StringComparer.Ordinal));
                entries.Add(chip);
            }
            // Only low-tier mundane catalogs, including the new basic
            // archives. Never inject an original tier-10 faction disk.
            foreach (var group in CatalogStock)
            {
                if (!RackHumanCatalogFactions.Contains(group.Key)) continue;
                foreach (var entry in group.Value)
                {
                    if (entry.Tier <= 2 &&
                        IndustryCatalogs.TryGetValue(entry.Chip, out var outputs) &&
                        outputs.Length > 0 && outputs.All(output => !OccultRecipeRequirements.ContainsKey(output)))
                        Include(group.Key, entry.Chip);
                }
            }
            _rackFactionPools = groups.OrderBy(group => group.Key, StringComparer.Ordinal)
                .Select(group => group.Value.OrderBy(id => id, StringComparer.Ordinal).ToArray())
                .Where(group => group.Length > 0).ToArray();
            _dataRackReady = IsUsableRackChip(RackBasicChip);
            Debug.Log("[Cvar_ImperiumOfMan] Data Rack ready=" + _dataRackReady +
                " faction groups=" + _rackFactionPools.Length + " chip types=" +
                DataRackPossibleChips.Count());
        }

        private static List<ItemQuantity> RollDataRack()
        {
            return DataRackRoller.Roll(RackRandom, RackBasicChip, _rackFactionPools,
                    RackMinChips, RackMaxChips)
                .Select(id => new ItemQuantity(id, 1)).ToList();
        }

        [HarmonyPatch(typeof(ItemInteractionSystem), nameof(ItemInteractionSystem.Disassemble))]
        private static class OpenDataRack
        {
            private static bool Prefix(BasePickupItem item, ref bool guaranteed, out Action __state)
            {
                __state = null;
                if (item?.Id != DataRack) return true;
                // Fail closed rather than expose the one-chip UI sentinel.
                if (!_dataRackReady || !IsEligibleShredInput(item) || item.StackCount != 1) return false;

                // Let native synchronous disassembly handle consumption, chip
                // creation, inventory placement and overflow.
                var record = Data.Items.GetSimpleRecord<ItemRecord>(DataRack);
                var original = record.Disassembly;
                var rolled = RollDataRack();
                __state = () => record.Disassembly = original;
                record.Disassembly = [.. rolled.GroupBy(entry => entry.ItemId, StringComparer.Ordinal)
                    .Select(group => new ItemQuantity(group.Key, group.Sum(entry => entry.Count)))];
                guaranteed = true;
                return true;
            }

            private static Exception Finalizer(Exception __exception, Action __state)
            {
                // Restore the catalogue immediately, including failed openings.
                __state?.Invoke();
                return __exception;
            }
        }

        // Rare ordinary tech loot, independent of intrusion and research.
        // Only successful fresh procedural entrance floors are considered.
        [HarmonyPatch(typeof(DungeonBuilder), nameof(DungeonBuilder.Create), [typeof(InputMapData)])]
        private static class DataRackTechLoot
        {
            private static void Postfix(State ____state, DungeonCreationResult __result)
            {
                if (!_dataRackReady || !_industryReady || __result != DungeonCreationResult.Success) return;
                try
                {
                    var raid = ____state.Get<RaidMetadata>();
                    var location = ____state.Get<LocationMetadata>();
                    if (raid?.RaidType != RaidType.ProcMission || location == null ||
                        location.LocationId != "stage1") return;
                    var random = new System.Random(location.Seed ^ 0x64617461);
                    if (random.Next(100) >= RackLootPercent) return;

                    var rackRecord = Data.Items.GetSimpleRecord<ItemRecord>(DataRack);
                    var chipClass = Data.Items.GetSimpleRecord<ItemRecord>(RackBasicChip).ItemClass;
                    var grid = ____state.Get<MapGrid>();
                    var stores = ____state.Get<MapObstacles>().Obstacles
                        .Where(obstacle => obstacle.QuestStorage == null && obstacle.Store != null &&
                            obstacle.Store.UseForSpawnItems && obstacle.Store.HasAccess() &&
                            obstacle.Store.AllowedItemClasses.Contains(rackRecord.ItemClass) &&
                            (obstacle.Store.AllowedItemClasses.Contains(chipClass) ||
                             obstacle.Store.storage.Items.Any(item => IsRackTechLoot(item.Id))) &&
                            HasReachableRationAccess(obstacle, grid))
                        .OrderByDescending(obstacle => obstacle.Store.storage.Items.Any(item => IsRackTechLoot(item.Id)))
                        .ThenBy(_ => random.Next()).ToArray();
                    if (stores.Length == 0) return;
                    var rack = SingletonMonoBehaviour<ItemFactory>.Instance.CreateForInventory(DataRack, isSingleItem: true);
                    foreach (var obstacle in stores)
                    {
                        if (!obstacle.Store.storage.TryPutItem(rack, CellPosition.Zero)) continue;
                        obstacle.Store.RefreshVisual();
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("[Cvar_ImperiumOfMan] Data Rack loot placement skipped: " + ex.Message);
                }
            }
        }

        private static bool IsRackTechLoot(string id) =>
            id == RackBasicChip || id == "circuit_board" || id == "microelectronics_parts" ||
            id == RackArtworkDonor || RackIngredients.Contains(id) ||
            _rackFactionPools.Any(pool => pool.Contains(id));
    }
}

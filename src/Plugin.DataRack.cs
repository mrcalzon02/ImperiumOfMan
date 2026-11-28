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

        // Ordinary native human faction chip records. Native single-chip
        // selection and companion unlocks are intentionally unchanged.
        private static readonly (string Faction, string Chip)[] RackNativeFactionChips =
        [
            ("AnCom", "anc_chip"), ("RealWare", "rwa_chip"),
            ("DaydreamChems", "ddr_chip"), ("CoreWard", "cor_chip"),
            ("SBN", "sbn_chip"), ("Dilthey", "DiltheyItemChip"),
            ("GrassHopper", "GrassHopperItemChip"), ("Tianming", "tia_chip")
        ];
        private static readonly HashSet<string> RackHumanCatalogFactions = new(StringComparer.Ordinal)
        {
            ItemCategory, "AnCom", "RealWare", "DaydreamChems", "CoreWard", "SBN",
            "Dilthey", "GrassHopper", "Tianming", "PlanetBridge"
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

            // Reuse actual native electrical cargo art and footprint provisionally.
            var rack = CloneItem<TrashRecord>(DataRack, RackArtworkDonor, 3, 650);
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
                disk.UnlockIds != null && disk.UnlockIds.Count > 0;
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
            foreach (var native in RackNativeFactionChips)
                Include(native.Faction, native.Chip);
            // Only low-tier mundane mod-added catalogs, never occult or Chaos.
            foreach (var group in CatalogStock)
            {
                if (!RackHumanCatalogFactions.Contains(group.Key)) continue;
                foreach (var entry in group.Value)
                {
                    if (entry.Tier <= 2 && IndustryCatalogs.ContainsKey(entry.Chip))
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
            var counts = new Dictionary<string, int>(StringComparer.Ordinal);
            int total = RackRandom.Next(RackMinChips, RackMaxChips + 1);
            for (int i = 0; i < total; i++)
            {
                // At least one basic and one faction chip, when available.
                // Later draws favor basics; pick faction before chip type.
                string id = RackBasicChip;
                if (_rackFactionPools.Length > 0 && (i == 1 || (i > 1 && RackRandom.Next(100) < 40)))
                {
                    string[] pool = _rackFactionPools[RackRandom.Next(_rackFactionPools.Length)];
                    id = pool[RackRandom.Next(pool.Length)];
                }
                counts.TryGetValue(id, out int count);
                counts[id] = count + 1;
            }
            return counts.Select(entry => new ItemQuantity(entry.Key, entry.Value)).ToList();
        }

        [HarmonyPatch(typeof(ItemInteractionSystem), nameof(ItemInteractionSystem.Disassemble))]
        private static class OpenDataRack
        {
            private static bool Prefix(BasePickupItem item, ref bool guaranteed, out Action __state)
            {
                __state = null;
                if (item?.Id != DataRack || !_dataRackReady) return true;
                if (!IsEligibleShredInput(item)) return false;

                // Let native synchronous disassembly handle consumption, chip
                // creation, inventory placement and overflow.
                var record = Data.Items.GetSimpleRecord<ItemRecord>(DataRack);
                var original = record.Disassembly;
                var rolled = RollDataRack();
                __state = () => record.Disassembly = original;
                record.Disassembly = [.. rolled];
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

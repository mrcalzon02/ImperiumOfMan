using System;
using System.Collections.Generic;
using System.Linq;
using MGSC;
using UnityEngine;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        // A read-only snapshot built when the window opens or the player requests Refresh.
        // These records never become authoritative recipe or save data.
        private sealed class CraftingIndex
        {
            public readonly Dictionary<string, CraftingItem> Items = new(StringComparer.Ordinal);
            public readonly List<CraftingRecipe> Recipes = new();
            public readonly Dictionary<string, List<CraftingRecipe>> Producers = new(StringComparer.Ordinal);
            public readonly Dictionary<string, List<CraftingRecipe>> Consumers = new(StringComparer.Ordinal);
            public string Summary;
        }

        private sealed class CraftingItem
        {
            public string Id;
            public string Name;
            public string Category;
            public Sprite Icon;
        }

        private sealed class CraftingAmount
        {
            public string ItemId;
            // A non-positive output count means the adapter cannot verify its yield.
            public int Count;
        }

        private sealed class CraftingRecipe
        {
            public string Key;
            public string Name;
            public string Venue;
            public List<CraftingAmount> Inputs = new();
            public List<CraftingAmount> Outputs = new();
            public float? Hours;
            public string Access;
            public bool? Learned;
            // Learned, capable and enough cargo; does not assert that a line is free.
            public bool? Ready;
        }

        private static CraftingIndex BuildCraftingIndex()
        {
            var index = new CraftingIndex();
            var incomplete = new List<string>();
            MagnumCargo cargo = null;
            Stations stations = null;
            MagnumProjects projects = null;
            MagnumProgression occultProgression = null;
            Capture("campaign access", () =>
            {
                var game = SingletonMonoBehaviour<SpaceGameMode>.Instance;
                if (game != null)
                {
                    cargo = game.Get<MagnumCargo>();
                    stations = game.Get<Stations>();
                    projects = game.Get<MagnumProjects>();
                    occultProgression = game.Get<MagnumProgression>();
                }
            });

            Capture("item catalogue", () =>
            {
                foreach (string id in Data.Items.Ids)
                {
                    IncludeItem(id);
                }
            });
            int registeredItems = index.Items.Count;

            Capture("Magnum production", () =>
            {
                int sequence = 0;
                foreach (var receipt in Data.ProduceReceipts)
                {
                    // Native production resolves upgraded projects before creating its output.
                    string output = projects?.GetWithModifications(receipt.OutputItem)?.CustomRecord?.Id ?? receipt.OutputItem;
                    var route = new CraftingRecipe
                    {
                        Key = "magnum/" + receipt.Id + "/" + sequence++,
                        Name = CraftingItemName(receipt.OutputItem) + " production",
                        Venue = "Magnum",
                        Inputs = CopyCraftingAmounts(receipt.RequiredItems),
                        Outputs = [new CraftingAmount { ItemId = output,
                            Count = IndustryBatches.TryGetValue(receipt.OutputItem, out int batch) ? batch : 1 }],
                        Hours = receipt.ProduceTimeInHours,
                        Access = "Recipe knowledge and cargo unavailable in this view."
                    };
                    if (cargo != null)
                    {
                        route.Learned = cargo.UnlockedProductionItems.Contains(receipt.OutputItem);
                        int available = route.Inputs.Count == 0 ? int.MaxValue :
                            ItemProductionSystem.GetAvailableToProduceCount(
                                MagnumCargoSystem.GetAvailableCargoStorages(cargo), receipt.RequiredItems);
                        route.Ready = route.Learned.Value && available > 0;
                        route.Access = route.Learned.Value ? "Recipe learned. " : "Recipe not learned. ";
                        route.Access += route.Inputs.Count == 0 ? "No item ingredients. " :
                            available > 0 ? "Cargo has ingredients for " + available + " order(s). " :
                            "Cargo ingredients missing. ";
                        route.Access += "Production-line availability is separate.";
                    }
                    if (OccultRecipeRequirements.TryGetValue(receipt.Id, out string occultPerk))
                    {
                        bool? capable = occultProgression == null ? (bool?)null :
                            HasOccultCapability(occultProgression, occultPerk);
                        route.Access += " Capability: " + OccultResearchTitle(occultPerk) +
                            (capable == true ? " installed." : capable == false ? " not installed." : " unavailable in this view.");
                        if (capable != true) route.Ready = capable == false ? false : (bool?)null;
                        if (OccultPatternSources.TryGetValue(receipt.OutputItem, out var patternChips))
                            route.Access += " Recipe source: " + string.Join(", ", patternChips.Select(CraftingItemName)) +
                                ". Each physical chip teaches one pattern.";
                        else
                            route.Access += " Recipe knowledge is acquired independently through in-game schematics.";
                    }
                    if (output != receipt.OutputItem)
                    {
                        route.Access += " Produces the current modified project for " + CraftingItemName(receipt.OutputItem) + ".";
                    }
                    AddRoute(route);
                }
            });

            Capture("workbench production", () =>
            {
                int sequence = 0;
                foreach (var receipt in Data.WorkbenchReceipts)
                {
                    string benches = receipt.AllowedWorkbenches == null || receipt.AllowedWorkbenches.Count == 0
                        ? "Workbench requirements not listed."
                        : "Required workbench: " + string.Join(", ", receipt.AllowedWorkbenches.Select(CraftingLabel)) + ".";
                    AddRoute(new CraftingRecipe
                    {
                        Key = "workbench/" + receipt.Id + "/" + sequence++,
                        Name = CraftingItemName(receipt.OutputItem) + " assembly",
                        Venue = "Workbench",
                        Inputs = CopyCraftingAmounts(receipt.RequiredItems),
                        Outputs = [new CraftingAmount { ItemId = receipt.OutputItem, Count = 0 }],
                        Access = benches + " Yield, time, and current bench access are not captured."
                    });
                }
            });

            Capture("station production", () =>
            {
                var activeStations = new Dictionary<string, List<string>>(StringComparer.Ordinal);
                if (stations != null)
                {
                    foreach (Station station in stations.Values)
                    {
                        foreach (string receipt in station.CurrentReceipts.Distinct(StringComparer.Ordinal))
                        {
                            if (!activeStations.TryGetValue(receipt, out var names))
                            {
                                names = new List<string>();
                                activeStations.Add(receipt, names);
                            }
                            names.Add(CraftingLocalizedName("station." + station.Id + ".name", station.Id));
                        }
                    }
                }
                foreach (BarterReceipt receipt in Data.BarterReceipts.Records)
                {
                    string access = stations == null ? "Current station availability is not captured." :
                        activeStations.TryGetValue(receipt.Id, out var names)
                            ? "Listed at " + string.Join(", ", names) + "."
                            : "No current station lists this registered order.";
                    if (receipt.InputPopulation > 0)
                    {
                        access += " Population requirement: " + receipt.InputPopulation + ".";
                    }
                    if (receipt.OutputPowerGain != 0 || receipt.OutputTechLevelGain != 0)
                    {
                        access += " Station gains: " + receipt.OutputPowerGain + " power, " +
                            receipt.OutputTechLevelGain.ToString("0.##") + " technology.";
                    }
                    AddRoute(new CraftingRecipe
                    {
                        Key = "station/" + receipt.Id,
                        Name = CraftingLabel(receipt.Id),
                        Venue = "Station",
                        Inputs = CopyCraftingAmounts(receipt.InputItems),
                        Outputs = CopyCraftingAmounts(receipt.OutputItems),
                        Hours = receipt.ProduceTimeInHours,
                        Access = access + " Station industry; not a Magnum order."
                    });
                }
            });

            Capture("recycling and disassembly", () =>
            {
                foreach (string id in Data.Items.Ids)
                {
                    var record = Data.Items.GetSimpleRecord<ItemRecord>(id);
                    if (record != null && record.CanDisassembly && record.Disassembly != null && record.Disassembly.Count > 0)
                    {
                        AddRoute(new CraftingRecipe
                        {
                            Key = "disassembly/" + id,
                            Name = id == OreCargo ? "Open Ore Cargo" : CraftingItemName(id) + " disassembly",
                            Venue = "Recycling",
                            Inputs = [new CraftingAmount { ItemId = id, Count = 1 }],
                            Outputs = CopyCraftingAmounts(record.Disassembly),
                            Access = id == OreCargo
                                ? "Opening consumes the cargo and delivers these contents. Locked and quest items are excluded."
                                : "Possible salvage per item; native recovery chance and item restrictions apply."
                        });
                    }
                    if (ShredProfiles.TryGetValue(id, out var outputs))
                    {
                        int inputBatch = GetShredInputBatch(id);
                        AddRoute(new CraftingRecipe
                        {
                            Key = "shredding/" + id,
                            Name = CraftingItemName(id) + " ship recycling",
                            Venue = "Recycling",
                            Inputs = [new CraftingAmount { ItemId = id, Count = inputBatch }],
                            Outputs = CopyCraftingAmounts(outputs),
                            Access = inputBatch > 1
                                ? "Magnum shredding combines selected stacks and consumes complete batches of " + inputBatch +
                                    ". Smaller remainders are retained. Locked and quest items are excluded."
                                : "Magnum shredding returns these materials per item. Locked and quest items are excluded; weapon ammunition is unloaded separately."
                        });
                    }
                }
            });

            index.Summary = registeredItems + " registered items · " + index.Recipes.Count + " live recipe routes · " +
                "refresh after changing recipes, cargo, or knowledge";
            if (index.Items.Count > registeredItems)
            {
                index.Summary += " · " + (index.Items.Count - registeredItems) + " additional referenced items";
            }
            if (incomplete.Count > 0)
            {
                index.Summary += " · Partial capture: " + string.Join(", ", incomplete);
            }
            return index;

            void Capture(string scope, Action action)
            {
                try
                {
                    action();
                }
                catch (Exception exception)
                {
                    incomplete.Add(scope);
                    Debug.LogWarning("[Cvar_ImperiumOfMan] Crafting tree could not fully capture " + scope + ": " + exception);
                }
            }

            void IncludeItem(string id)
            {
                if (string.IsNullOrWhiteSpace(id) || index.Items.ContainsKey(id))
                {
                    return;
                }
                var item = new CraftingItem { Id = id, Name = CraftingItemName(id), Category = "Unclassified" };
                index.Items.Add(id, item);
                // Some registered records are supplemental or internal. Keep them searchable
                // even when they have no ordinary inventory descriptor or ItemRecord.
                try
                {
                    var primary = Data.Items.GetRecord(id);
                    item.Icon = primary?.ItemDesc?.Icon;
                    var record = Data.Items.GetSimpleRecord<ItemRecord>(id);
                    if (record != null)
                    {
                        item.Category = record.ItemClass.ToString();
                        if (record.Categories != null && record.Categories.Count > 0)
                        {
                            item.Category += " · " + string.Join(" / ", record.Categories.Select(CraftingLabel));
                        }
                    }
                }
                catch (Exception)
                {
                    item.Category = "Descriptor unavailable";
                }
            }

            void AddRoute(CraftingRecipe route)
            {
                if (route.Outputs.Count == 0)
                {
                    return;
                }
                index.Recipes.Add(route);
                foreach (string id in route.Inputs.Select(input => input.ItemId).Distinct(StringComparer.Ordinal))
                {
                    IncludeItem(id);
                    if (!index.Consumers.TryGetValue(id, out var consumers))
                    {
                        consumers = new List<CraftingRecipe>();
                        index.Consumers.Add(id, consumers);
                    }
                    consumers.Add(route);
                }
                foreach (string id in route.Outputs.Select(output => output.ItemId).Distinct(StringComparer.Ordinal))
                {
                    IncludeItem(id);
                    if (!index.Producers.TryGetValue(id, out var producers))
                    {
                        producers = new List<CraftingRecipe>();
                        index.Producers.Add(id, producers);
                    }
                    producers.Add(route);
                }
            }
        }

        private static List<CraftingAmount> CopyCraftingAmounts(IEnumerable<ItemQuantity> amounts) =>
            amounts == null ? new List<CraftingAmount>() : amounts.Where(amount => amount.Count > 0 &&
                !string.IsNullOrWhiteSpace(amount.ItemId)).GroupBy(amount => amount.ItemId, StringComparer.Ordinal)
                .Select(group => new CraftingAmount { ItemId = group.Key, Count = group.Sum(amount => amount.Count) }).ToList();

        private static string CraftingItemName(string id) =>
            CraftingLocalizedName("item." + id + ".name", id);

        private static string CraftingLocalizedName(string key, string fallback) =>
            Localization.HasKey(key) ? Localization.Get(key) : CraftingLabel(fallback);

        private static string CraftingLabel(string id) =>
            string.IsNullOrWhiteSpace(id) ? "Unknown item" : id.Replace('_', ' ');
    }
}

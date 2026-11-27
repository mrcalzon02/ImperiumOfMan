using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            public readonly CraftingDependencyResolver<CraftingRecipe> Dependencies = new(
                recipe => recipe.Inputs.Select(amount => amount.ItemId),
                recipe => recipe.Outputs.Select(amount => amount.ItemId));
            public Dictionary<string, List<CraftingRecipe>> Producers => Dependencies.Producers;
            public Dictionary<string, List<CraftingRecipe>> Consumers => Dependencies.Consumers;
            public string Summary;
            public string[] IncompleteSources = Array.Empty<string>();
        }

        private sealed class CraftingItem
        {
            public string Id;
            public string Name;
            public string Category;
            public ItemClass? Class;
            public string[] Tags = Array.Empty<string>();
            public bool Registered;
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
            // Original receipt identity, retained when a project changes its displayed output.
            public string OccultOutput;
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
            Difficulty difficulty = null;
            Capture("cargo and research", () =>
            {
                var game = SingletonMonoBehaviour<SpaceGameMode>.Instance;
                if (game != null)
                {
                    cargo = game.Get<MagnumCargo>();
                    stations = game.Get<Stations>();
                    projects = game.Get<MagnumProjects>();
                    occultProgression = game.Get<MagnumProgression>();
                    difficulty = game.Get<Difficulty>();
                }
            });

            Capture("item catalogue", () =>
            {
                foreach (string id in Data.Items.Ids.ToArray())
                {
                    IncludeItem(id, true);
                }
            });
            int registeredItems = index.Items.Count;

            Capture("Magnum recipes", () =>
            {
                int sequence = 0;
                foreach (var receipt in Data.ProduceReceipts.ToArray())
                {
                    Capture("Magnum recipes", () =>
                    {
                        bool occult = TryGetOccultRequirement(receipt, out string occultPerk);
                        // Native production resolves upgraded projects before creating its output.
                        string output = projects?.GetWithModifications(receipt.OutputItem)?.CustomRecord?.Id ?? receipt.OutputItem;
                        var route = new CraftingRecipe
                        {
                            Key = "magnum/" + receipt.Id + "/" + sequence++,
                            Name = "Produce " + CraftingItemName(output),
                            Venue = "Magnum",
                            OccultOutput = occult ? receipt.OutputItem : null,
                            Inputs = CopyCraftingAmounts(receipt.RequiredItems),
                            Outputs = [new CraftingAmount { ItemId = output,
                                // Native receipts do not expose a yield field. A separate
                                // mod may change the batch through its own production hook.
                                Count = IndustryBatches.TryGetValue(receipt.OutputItem, out int batch) ? batch : 0 }],
                            Hours = receipt.ProduceTimeInHours,
                            Access = "Check the Magnum's production menu for the recipe and required materials."
                        };
                        // Availability checks may fail for an orphan ingredient supplied
                        // by another mod. Keep its registered recipe visible regardless.
                        AddRoute(route);
                        if (cargo != null)
                        {
                            route.Learned = cargo.UnlockedProductionItems.Contains(receipt.OutputItem);
                            int available = route.Inputs.Count == 0 ? int.MaxValue :
                                ItemProductionSystem.GetAvailableToProduceCount(
                                    MagnumCargoSystem.GetAvailableCargoStorages(cargo), receipt.RequiredItems,
                                    countCustomItems: occult);
                            route.Ready = route.Learned.Value && available > 0;
                            route.Access = route.Learned.Value ? "Recipe learned. " : "Recipe not learned. ";
                            route.Access += route.Inputs.Count == 0 ? "No ingredients needed. " :
                                available > 0 ? "Cargo holds enough materials to make this " + available +
                                    (available == 1 ? " time. " : " times. ") : "More materials are needed. ";
                            route.Access += "Use a free production line to begin.";
                        }
                        if (occult)
                        {
                            bool? capable = occultProgression == null ? (bool?)null :
                                HasOccultCapability(occultProgression, occultPerk);
                            if (IndustryBatches.TryGetValue(receipt.OutputItem, out int occultBaseBatch))
                                route.Outputs[0].Count = OccultBatchSize(receipt, occultProgression, occultBaseBatch);
                            // Use the identical integer, difficulty-adjusted duration as the
                            // native occult receipt panel and actual queued order. Without
                            // live difficulty data the duration is not known.
                            route.Hours = difficulty == null ? (float?)null :
                                OccultBatchHours(receipt, occultProgression, difficulty);
                            route.Access += " Requires an occult production line; ordinary lines cannot fabricate this recipe.";
                            if (difficulty == null) route.Access += " Check Production for the actual batch time.";
                            string research = CraftingLocalizedName("mgperk." + occultPerk + ".name", OccultResearchTitle(occultPerk));
                            route.Access += capable == true ? " Research installed: " + research + "." :
                                capable == false ? " Requires " + research + "." : " Check research: " + research + ".";
                            if (capable != true || OccultLineCount(occultProgression) == 0)
                                route.Ready = occultProgression != null ? false : (bool?)null;
                            string regionalBranch = RegionalOccultRecipeBranch(receipt.OutputItem);
                            if (regionalBranch != null) route.Access += " Occult resource family: " + regionalBranch + ".";
                            if (OccultPatternSources.TryGetValue(receipt.OutputItem, out var patternChips))
                                route.Access += " Learn from " + string.Join(", ", patternChips.Select(RegionalOccultPatternSource)) +
                                    ". Each chip teaches one recipe.";
                            else
                                route.Access += " The schematic must also be learned.";
                        }
                        if (output != receipt.OutputItem)
                        {
                            route.Access += " Includes your upgrades to " + CraftingItemName(receipt.OutputItem) + ".";
                        }
                        if (route.Outputs[0].Count <= 0)
                            route.Access += " Check Production for the amount made.";
                    });
                }
            });

            Capture("workbench recipes", () =>
            {
                int sequence = 0;
                foreach (var receipt in Data.WorkbenchReceipts.ToArray())
                {
                    Capture("workbench recipes", () =>
                    {
                        string benches = receipt.AllowedWorkbenches == null || receipt.AllowedWorkbenches.Count == 0
                            ? "Check the workbench for requirements."
                            : "Use " + string.Join(" or ", receipt.AllowedWorkbenches.Select(CraftingWorkbenchName).Distinct()) + ".";
                        AddRoute(new CraftingRecipe
                        {
                            Key = "workbench/" + receipt.Id + "/" + sequence++,
                            Name = "Craft " + CraftingItemName(receipt.OutputItem),
                            Venue = "Workbench",
                            Inputs = CopyCraftingAmounts(receipt.RequiredItems),
                            Outputs = [new CraftingAmount { ItemId = receipt.OutputItem, Count = 0 }],
                            Access = benches + " Check it for the amount made and crafting time."
                        });
                    });
                }
            });

            Capture("station recipes", () =>
            {
                var activeStations = new Dictionary<string, List<string>>(StringComparer.Ordinal);
                bool stationListingsKnown = stations != null;
                if (stations != null)
                {
                    foreach (Station station in stations.Values.ToArray())
                    {
                        stationListingsKnown &= Capture("station availability", () =>
                        {
                            foreach (string receipt in station.CurrentReceipts.Distinct(StringComparer.Ordinal))
                            {
                                if (!activeStations.TryGetValue(receipt, out var names))
                                {
                                    names = new List<string>();
                                    activeStations.Add(receipt, names);
                                }
                                names.Add(CraftingStationName(station.Id));
                            }
                        });
                    }
                }
                foreach (BarterReceipt receipt in Data.BarterReceipts.Records.ToArray())
                {
                    Capture("station recipes", () =>
                    {
                        string access = !stationListingsKnown ? "Check a station for availability." :
                            activeStations.TryGetValue(receipt.Id, out var names)
                                ? "Available at " + string.Join(", ", names.Distinct()) + "."
                                : "Not currently offered by a station.";
                        if (receipt.InputPopulation > 0)
                        {
                            access += " Population needed: " + receipt.InputPopulation + ".";
                        }
                        if (receipt.OutputPowerGain != 0 || receipt.OutputTechLevelGain != 0)
                        {
                            access += " Station gains: " + receipt.OutputPowerGain + " power, " +
                                receipt.OutputTechLevelGain.ToString("0.##") + " technology.";
                        }
                        AddRoute(new CraftingRecipe
                        {
                            Key = "station/" + receipt.Id,
                            Name = CraftingStationRecipeName(receipt.OutputItems),
                            Venue = "Station",
                            Inputs = CopyCraftingAmounts(receipt.InputItems),
                            Outputs = CopyCraftingAmounts(receipt.OutputItems),
                            Hours = receipt.ProduceTimeInHours,
                            Access = access
                        });
                    });
                }
            });

            Capture("recycling and disassembly", () =>
            {
                foreach (string id in Data.Items.Ids.ToArray())
                {
                    Capture("recycling recipes", () =>
                    {
                        var record = Data.Items.GetSimpleRecord<ItemRecord>(id);
                        if (record != null && record.CanDisassembly && record.Disassembly != null && record.Disassembly.Count > 0)
                        {
                            AddRoute(new CraftingRecipe
                            {
                                Key = "disassembly/" + id,
                                Name = (id == OreCargo ? "Open " : "Disassemble ") + CraftingItemName(id),
                                Venue = "Recycling",
                                Inputs = [new CraftingAmount { ItemId = id, Count = 1 }],
                                Outputs = CopyCraftingAmounts(record.Disassembly),
                                Access = id == OreCargo
                                    ? "Open the cargo to collect its contents. Unlock it first; quest items cannot be opened this way."
                                    : "Possible salvage from one item. Some materials may be lost during disassembly."
                            });
                        }
                        if (ShredProfiles.TryGetValue(id, out var outputs))
                        {
                            AddRoute(new CraftingRecipe
                            {
                                Key = "shredding/" + id,
                                Name = "Recycle " + CraftingItemName(id),
                                Venue = "Recycling",
                                Inputs = [new CraftingAmount { ItemId = id, Count = 1 }],
                                Outputs = CopyCraftingAmounts(outputs),
                                Access = "Recycle one item aboard the Magnum. Locked and quest items cannot be recycled; weapon ammunition is removed first."
                            });
                        }
                    });
                }
            });

            index.Summary = registeredItems + " items · " + index.Recipes.Count + " recipes";
            if (index.Items.Count > registeredItems)
            {
                index.Summary += " · " + (index.Items.Count - registeredItems) + " other ingredients";
            }
            if (incomplete.Count > 0)
            {
                index.IncompleteSources = incomplete.Distinct(StringComparer.Ordinal).ToArray();
                index.Summary += " · Some details are unavailable: " + string.Join(", ", index.IncompleteSources);
            }
            return index;

            bool Capture(string scope, Action action)
            {
                try
                {
                    action();
                    return true;
                }
                catch (Exception exception)
                {
                    incomplete.Add(scope);
                    Debug.LogWarning("[Cvar_ImperiumOfMan] Crafting tree could not fully capture " + scope + ": " + exception);
                    return false;
                }
            }

            void IncludeItem(string id, bool registered = false)
            {
                if (string.IsNullOrWhiteSpace(id) || index.Items.ContainsKey(id))
                {
                    return;
                }
                var item = new CraftingItem { Id = id, Name = CraftingItemName(id), Category = "Other", Registered = registered };
                index.Items.Add(id, item);
                if (registered)
                    index.Dependencies.RegisterItem(id);
                // Some registered records are supplemental or internal. Keep them searchable
                // even when they have no ordinary inventory descriptor or ItemRecord.
                try
                {
                    var primary = Data.Items.GetRecord(id);
                    item.Registered |= primary != null;
                    if (item.Registered)
                        index.Dependencies.RegisterItem(id);
                    var record = primary == null ? null : Data.Items.GetSimpleRecord<ItemRecord>(id);
                    if (record != null)
                    {
                        item.Class = record.ItemClass;
                        item.Category = CraftingClassLabel(record.ItemClass);
                        item.Tags = record.Categories?.Where(tag => !string.IsNullOrWhiteSpace(tag))
                            .Distinct(StringComparer.Ordinal).ToArray() ?? Array.Empty<string>();
                    }
                    // Match the native recipe picker, including its placeholder for a
                    // registered mod item that has not supplied inventory artwork.
                    // A composite supplied by another mod may put its visible
                    // descriptor on a supplemental record instead of the first.
                    var descriptor = primary is CompositeItemRecord composite
                        ? composite.Records?.Where(recordPart => recordPart != null)
                            .Select(recordPart => recordPart.ItemDesc).FirstOrDefault(candidate => candidate?.Icon != null)
                            ?? composite.Records?.Where(recordPart => recordPart != null)
                                .Select(recordPart => recordPart.ItemDesc).FirstOrDefault(candidate => candidate != null)
                        : primary?.ItemDesc;
                    if (descriptor != null)
                        item.Icon = SingletonMonoBehaviour<ItemFactory>.Instance?.ResolveIcon(descriptor, 1) ?? descriptor.Icon;
                    if (!item.Registered)
                        item.Category = "Unavailable item";
                }
                catch (Exception)
                {
                    // Missing art or an unusual supplemental record must not remove
                    // an otherwise registered item from the dependency catalogue.
                    if (!item.Registered)
                        item.Category = "Unavailable item";
                }
            }

            void AddRoute(CraftingRecipe route)
            {
                if (!route.Outputs.Any(output => !string.IsNullOrWhiteSpace(output.ItemId)))
                {
                    return;
                }
                index.Recipes.Add(route);
                index.Dependencies.AddRecipe(route);
                foreach (string id in route.Inputs.Select(input => input.ItemId)
                    .Concat(route.Outputs.Select(output => output.ItemId)).Distinct(StringComparer.Ordinal))
                {
                    IncludeItem(id);
                }
            }
        }

        private static List<CraftingAmount> CopyCraftingAmounts(IEnumerable<ItemQuantity> amounts) =>
            amounts == null ? new List<CraftingAmount>() : amounts.Where(amount => amount.Count > 0 &&
                !string.IsNullOrWhiteSpace(amount.ItemId)).GroupBy(amount => amount.ItemId, StringComparer.Ordinal)
                .Select(group => new CraftingAmount { ItemId = group.Key, Count = group.Sum(amount => amount.Count) }).ToList();

        private static string CraftingItemName(string id) => CraftingLocalizedName("item." + id + ".name", id);

        private static string CraftingStationName(string id) => CraftingLocalizedName("station." + id + ".name", id);

        private static string CraftingWorkbenchName(string id) =>
            CraftingLocalizedName("ui.mapobject." + id + ".caption", id);

        private static string CraftingStationRecipeName(IEnumerable<ItemQuantity> outputs)
        {
            string[] names = outputs == null ? Array.Empty<string>() : outputs
                .Where(amount => amount.Count > 0 && !string.IsNullOrWhiteSpace(amount.ItemId))
                .Select(amount => CraftingItemName(amount.ItemId)).Distinct(StringComparer.Ordinal).ToArray();
            return names.Length == 0 ? "Station production" : "Produce " + string.Join(" + ", names);
        }

        private static string CraftingClassLabel(ItemClass itemClass)
        {
            // Keep native classification in Class; these are display labels only.
            return itemClass.ToString() switch
            {
                "None" => "Other",
                "ThrowableWeapon" => "Thrown weapon",
                "Helmet" => "Head protection",
                "Armor" => "Body armor",
                "Leggings" => "Leg protection",
                "Boots" => "Footwear",
                "Vest" => "Tactical vest",
                "Ammo" => "Ammunition",
                "Medpack" => "Medical kit",
                "Parts" => "Material or component",
                "RepairKit" => "Repair supplies",
                "MilitaryBarter" => "Military supplies",
                "ScienceBarter" => "Research supplies",
                "IndustrialBarter" => "Industrial supplies",
                "ValuableBarter" => "Valuables",
                "QuasiArtefact" => "Quasimorph artifact",
                "QuasiPact" => "Pact",
                "QuasiOrgan" => "Quasimorph organ",
                "Blueprint" => "Schematic",
                "QuestItem" => "Mission item",
                _ => CraftingLabel(itemClass.ToString())
            };
        }

        private static string CraftingLocalizedName(string key, string fallback)
        {
            try
            {
                if (Localization.HasKey(key))
                {
                    string name = Localization.Get(key);
                    if (!string.IsNullOrWhiteSpace(name) && !string.Equals(name, key, StringComparison.Ordinal))
                        return name;
                }
            }
            catch (Exception) { }
            return CraftingLabel(fallback);
        }

        private static string CraftingLabel(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "Unnamed item";
            value = value.Trim();
            if (value.StartsWith("iom_", StringComparison.Ordinal))
                value = value.Substring(4);
            else if (value.Length > 3 && value.StartsWith("iom", StringComparison.Ordinal) && char.IsUpper(value[3]))
                value = value.Substring(3);
            var label = new StringBuilder();
            for (int i = 0; i < value.Length; i++)
            {
                char character = value[i];
                if (character == '_' || character == '.' || character == '/' || character == '\\')
                    label.Append(' ');
                else
                {
                    if (i > 0 && char.IsUpper(character) && char.IsLower(value[i - 1]))
                        label.Append(' ');
                    label.Append(character);
                }
            }
            string result = string.Join(" ", label.ToString().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
            return result.Length == 0 ? "Unnamed item" : char.ToUpperInvariant(result[0]) + result.Substring(1);
        }
    }
}

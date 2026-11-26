using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private sealed partial class CraftingMapWindow
        {
            private bool _layoutDirty;
            private CraftingGraphLayout.Result _layout;
            private static readonly string[] GroupTitles = { "Shared materials", "Metalworking", "Minerals & ceramics", "Chemicals & polymers",
                "Electronics", "Organics", "Textiles & paper", "Food & provisions", "Medical supplies",
                "Weapons & equipment", "Occult materials", "Recovery", "Mixed processing", "Other processing" };
            private static readonly Color32[] GroupColors = { new Color32(129,157,135,255), new Color32(136,171,120,255),
                new Color32(168,158,108,255), new Color32(101,167,147,255), new Color32(113,157,177,255),
                new Color32(145,168,105,255), new Color32(162,151,119,255), new Color32(174,158,107,255),
                new Color32(125,175,157,255), new Color32(139,158,146,255), new Color32(155,132,170,255),
                new Color32(146,149,119,255), new Color32(134,154,144,255), new Color32(117,145,128,255) };

            private void TraceSelectedIngredients()
            {
                var roots = SelectedItems(true).OrderBy(id => id, StringComparer.Ordinal).ToArray();
                if (_index?.Dependencies == null || roots.Length == 0) return;
                var allowed = new HashSet<CraftingRecipe>();
                var missing = new HashSet<string>(StringComparer.Ordinal);
                var unknown = new HashSet<string>(StringComparer.Ordinal);
                var cycles = new HashSet<string>(StringComparer.Ordinal);
                bool limited = false;
                foreach (string root in roots)
                {
                    var trace = _index.Dependencies.TraceIngredients(root, MapNodeLimit, MapNodeLimit * 2, InVenue);
                    foreach (var recipe in trace.Recipes.Where(InVenue)) allowed.Add(recipe);
                    missing.UnionWith(trace.MissingItems);
                    unknown.UnionWith(trace.NoKnownProducer);
                    cycles.UnionWith(trace.CycleItems);
                    limited |= trace.Truncated;
                }
                var queue = new Queue<string>(roots);
                var visited = new HashSet<string>(StringComparer.Ordinal);
                int added = 0;
                while (queue.Count > 0)
                {
                    string id = queue.Dequeue();
                    if (!visited.Add(id)) continue;
                    if (!_nodes.TryGetValue(ItemKey(id), out var item)) continue;
                    foreach (var recipe in Routes(id, true).Where(allowed.Contains))
                    {
                        bool present = _nodes.ContainsKey(RecipeKey(recipe.Key));
                        if (!present && !AddRecipe(recipe, item.Column - 1))
                        {
                            limited = true;
                            continue;
                        }
                        if (!present) added++;
                        foreach (string input in recipe.Inputs.Select(amount => amount.ItemId).Distinct().OrderBy(value => value, StringComparer.Ordinal))
                            queue.Enqueue(input);
                    }
                }
                Changed(false);
                var notes = new List<string> { added + " ingredient recipes added." };
                if (unknown.Count > 0) notes.Add(unknown.Count + " ingredients have no recipe in this source view. " +
                    (_venue == "All" ? "Loot or trade may supply them." : "Try All sources for more recipes."));
                if (missing.Count > 0) notes.Add(missing.Count + " ingredients are unavailable in the current game.");
                if (cycles.Count > 0) notes.Add("Repeating recipe chains are shown once.");
                if (limited) notes.Add("View limit reached. Focus an ingredient to continue.");
                if (_venue != "All") notes.Add("The current source filter is applied.");
                _detailNote.text = string.Join(" ", notes);
                _mapStatus.text = "Ingredient tree · " + _nodes.Count + " items & recipes · " +
                    (limited ? "focus an ingredient to continue" : _venue == "All" ? "all known alternatives are retained" : "current source filter applied") + " · drag to pan · wheel to zoom";
            }

            private void ArrangeGraph()
            {
                if (!_layoutDirty || _nodes.Count == 0) return;
                _nodes.TryGetValue(_selectedKey ?? "", out var anchor);
                var oldPosition = anchor == null ? Vector2.zero : anchor.Position;
                foreach (var node in _nodes.Values)
                    node.Group = node.Recipe == null ? ItemGroup(node.Item) : RecipeGroup(node.Recipe);
                foreach (var node in _nodes.Values.Where(node => node.Recipe == null))
                {
                    var connected = _edges.Where(edge => edge.From == node || edge.To == node)
                        .Select(edge => edge.From.Recipe != null ? edge.From.Group : edge.To.Group).Distinct().ToArray();
                    if (connected.Length > 1 && (node.Group <= 6 || node.Group == 13)) node.Group = 0;
                    else if (node.Group == 13 && connected.Length == 1) node.Group = connected[0];
                }
                var nodes = _nodes.Values.Select(node => new CraftingGraphLayout.Node
                {
                    Key = node.Key, SortKey = node.ItemId == null ? node.Recipe.Name : Name(node.ItemId),
                    Column = node.Column, Group = node.Group,
                    Width = node.Recipe == null ? 220f : 244f, Height = 84f
                }).ToArray();
                var edges = _edges.Select(edge => new CraftingGraphLayout.Edge
                { Key = edge.From.Key + "\n" + edge.To.Key, From = edge.From.Key, To = edge.To.Key }).ToArray();
                _layout = CraftingGraphLayout.Arrange(nodes, edges, anchor?.Key,
                    new CraftingGraphLayout.Point(oldPosition.x, oldPosition.y));
                foreach (var placed in nodes)
                {
                    var node = _nodes[placed.Key];
                    node.Position = new Vector2(placed.X, placed.Y);
                    node.Size = new Vector2(placed.Width, placed.Height);
                }
                for (int i = 0; i < edges.Length; i++) _edges[i].Layout = edges[i];
                _layoutDirty = false;
            }

            private int RecipeGroup(CraftingRecipe recipe)
            {
                if (recipe.Venue == "Recycling") return 11;
                var outputs = recipe.Outputs.Select(amount => _index.Items.TryGetValue(amount.ItemId, out var item) ? ItemGroup(item) : 13)
                    .Distinct().ToArray();
                if (outputs.Length == 1 && outputs[0] != 13) return outputs[0];
                if (outputs.Any(group => group != 13)) return 12;
                var inputs = recipe.Inputs.Select(amount => _index.Items.TryGetValue(amount.ItemId, out var item) ? ItemGroup(item) : 13)
                    .Where(group => group != 13).Distinct().ToArray();
                return inputs.Length == 1 ? inputs[0] : 13;
            }

            private static int ItemGroup(CraftingItem item)
            {
                if (item == null) return 13;
                string id = (item.Id ?? "").ToLowerInvariant();
                // These are display families only. Native recipes, item classes and dependencies stay unchanged.
                if (id.StartsWith("iom_occult_", StringComparison.Ordinal) || HasAny(id, "gavvakh", "suffering_essence", "torment_imprint")) return 10;
                if (HasAny(id, "electronic_substrate", "circuit", "conductive_wire", "copper_wire", "wiring", "logic_core", "battery", "electronic")) return 4;
                if (id == "acid" || id == "plastic" || id == "rubber" || id == "gas_ammo" ||
                    HasAny(id, "chemical_extract", "polymer_stock", "organic_oil", "solvent", "reagent", "chemical_base")) return 3;
                if (HasAny(id, "ceramic_feedstock", "ceramite", "silicate", "reactive_mineral", "carbon_feedstock", "glass")) return 2;
                string kind = item.Class.HasValue ? item.Class.Value.ToString() : "";
                if (HasAny(kind, "QuasiArtefact", "QuasiPact", "QuasiOrgan")) return 10;
                if (HasAny(kind, "Weapon", "Ammo", "Armor", "Helmet", "Cuirass", "Greave", "Boot", "Glove", "Shield", "Turret", "Cyborg", "Mine", "Grenade")) return 9;
                if (HasAny(kind, "Food", "Drink", "Alcohol")) return 7;
                if (HasAny(kind, "Pills", "Syringe", "Medpack", "Dressing")) return 8;
                if (HasAny(id, "medical", "medicine", "antibiotic", "bandage", "dressing", "clinical", "treatment_pack")) return 8;
                if (HasAny(id, "ration", "food", "provision", "tobacco", "cigarette", "drink")) return 7;
                if (HasAny(id, "biomass", "cellulose", "human_meat", "human_rib", "organic", "bone", "flesh", "organ_")) return 5;
                if (HasAny(id, "textile", "fabric", "cloth", "fiber", "fibre", "thread", "leather", "paper", "pulp")) return 6;
                if (HasAny(id, "metal", "alloy", "rolled_plate", "ingot", "conductive_scrap", "steel", "ore", "wire", "scrap")) return 1;
                if (HasAny(id, "mineral", "ceramic", "silica", "stone", "crystal")) return 2;
                if (HasAny(id, "chemical", "polymer", "fuel", "oil", "resin", "adhesive")) return 3;
                return 13;
            }

            private static bool HasAny(string value, params string[] fragments)
                => fragments.Any(fragment => value.IndexOf(fragment, StringComparison.OrdinalIgnoreCase) >= 0);

            private static string GroupTitle(int group)
                => GroupTitles[Math.Max(0, Math.Min(group, GroupTitles.Length - 1))];

            private static Color GroupColor(int group)
                => GroupColors[Math.Max(0, Math.Min(group, GroupColors.Length - 1))];

            private static string VenueLabel(string venue) => venue == "Recycling" ? "Recovery" : venue == "Station" ? "Station industry" : venue;

            private Sprite RecipeIcon(CraftingRecipe recipe)
            {
                foreach (var amount in recipe.Outputs.Concat(recipe.Inputs))
                    if (_index.Items.TryGetValue(amount.ItemId, out var item) && item.Icon != null) return item.Icon;
                return null;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace ImperiumOfMan
{
    // This index knows nothing about particular mods, material names, or recipe
    // balance. The game adapter supplies the recipes registered for this snapshot.
    // Original recipe objects retain their quantities, coproducts and conditions.
    internal sealed class CraftingDependencyResolver<TRecipe> where TRecipe : class
    {
        private readonly Func<TRecipe, IEnumerable<string>> _inputs;
        private readonly Func<TRecipe, IEnumerable<string>> _outputs;
        private readonly Dictionary<TRecipe, RecipeLinks> _links = new Dictionary<TRecipe, RecipeLinks>();
        private readonly HashSet<string> _registered = new HashSet<string>(StringComparer.Ordinal);

        public readonly Dictionary<string, List<TRecipe>> Producers = new Dictionary<string, List<TRecipe>>(StringComparer.Ordinal);
        public readonly Dictionary<string, List<TRecipe>> Consumers = new Dictionary<string, List<TRecipe>>(StringComparer.Ordinal);

        public CraftingDependencyResolver(Func<TRecipe, IEnumerable<string>> inputs,
            Func<TRecipe, IEnumerable<string>> outputs)
        {
            _inputs = inputs ?? throw new ArgumentNullException(nameof(inputs));
            _outputs = outputs ?? throw new ArgumentNullException(nameof(outputs));
        }

        public void RegisterItem(string id)
        {
            if (!string.IsNullOrWhiteSpace(id))
                _registered.Add(id);
        }

        public bool IsRegistered(string id) => id != null && _registered.Contains(id);

        public void AddRecipe(TRecipe recipe)
        {
            if (recipe == null)
                throw new ArgumentNullException(nameof(recipe));
            if (_links.ContainsKey(recipe))
                return;
            var links = new RecipeLinks(CleanIds(_inputs(recipe)), CleanIds(_outputs(recipe)));
            if (links.Outputs.Length == 0)
                return;
            _links.Add(recipe, links);
            foreach (string id in links.Inputs)
                Add(Consumers, id, recipe);
            foreach (string id in links.Outputs)
                Add(Producers, id, recipe);
        }

        // Follow every known producer of every required ingredient. Coproducts
        // stay attached to their original recipe; no substitute or new recipe
        // is invented. Bounds and cycle reporting keep modded loops navigable.
        public CraftingDependencyTrace<TRecipe> TraceIngredients(string itemId,
            int recipeLimit = 256, int itemLimit = 512, Func<TRecipe, bool> includeRecipe = null)
        {
            if (recipeLimit < 1)
                throw new ArgumentOutOfRangeException(nameof(recipeLimit));
            if (itemLimit < 1)
                throw new ArgumentOutOfRangeException(nameof(itemLimit));
            var trace = new CraftingDependencyTrace<TRecipe>();
            var visited = new HashSet<string>(StringComparer.Ordinal);
            var active = new List<string>();
            var recipes = new HashSet<TRecipe>();
            Visit(itemId);
            return trace;

            bool Include(string id)
            {
                if (trace.Items.Contains(id))
                    return true;
                if (trace.Items.Count >= itemLimit)
                {
                    trace.Truncated = true;
                    return false;
                }
                trace.Items.Add(id);
                if (!IsRegistered(id))
                    trace.MissingItems.Add(id);
                return true;
            }

            void Visit(string id)
            {
                if (string.IsNullOrWhiteSpace(id) || !Include(id))
                    return;
                int cycleStart = active.IndexOf(id);
                if (cycleStart >= 0)
                {
                    for (int i = cycleStart; i < active.Count; i++)
                        trace.CycleItems.Add(active[i]);
                    return;
                }
                if (!visited.Add(id))
                    return;
                // Apply the current source filter before walking dependencies or
                // spending the view budget. A hidden alternative must not report
                // its missing ingredients or recycling cycles in this view.
                TRecipe[] producers = Producers.TryGetValue(id, out var candidates)
                    ? candidates.Where(recipe => includeRecipe == null || includeRecipe(recipe)).ToArray()
                    : Array.Empty<TRecipe>();
                if (producers.Length == 0)
                {
                    if (IsRegistered(id))
                        trace.NoKnownProducer.Add(id);
                    return;
                }
                active.Add(id);
                foreach (TRecipe recipe in producers)
                {
                    if (!recipes.Contains(recipe))
                    {
                        if (recipes.Count >= recipeLimit)
                        {
                            trace.Truncated = true;
                            break;
                        }
                        recipes.Add(recipe);
                        trace.Recipes.Add(recipe);
                    }
                    RecipeLinks links = _links[recipe];
                    foreach (string output in links.Outputs)
                        Include(output);
                    foreach (string input in links.Inputs)
                        Visit(input);
                }
                active.RemoveAt(active.Count - 1);
            }
        }

        private static string[] CleanIds(IEnumerable<string> values) => values == null
            ? Array.Empty<string>()
            : values.Where(id => !string.IsNullOrWhiteSpace(id)).Distinct(StringComparer.Ordinal).ToArray();

        private static void Add(Dictionary<string, List<TRecipe>> index, string id, TRecipe recipe)
        {
            if (!index.TryGetValue(id, out var entries))
            {
                entries = new List<TRecipe>();
                index.Add(id, entries);
            }
            entries.Add(recipe);
        }

        private sealed class RecipeLinks
        {
            public readonly string[] Inputs;
            public readonly string[] Outputs;

            public RecipeLinks(string[] inputs, string[] outputs)
            {
                Inputs = inputs;
                Outputs = outputs;
            }
        }
    }

    internal sealed class CraftingDependencyTrace<TRecipe> where TRecipe : class
    {
        public readonly List<TRecipe> Recipes = new List<TRecipe>();
        public readonly HashSet<string> Items = new HashSet<string>(StringComparer.Ordinal);
        public readonly HashSet<string> MissingItems = new HashSet<string>(StringComparer.Ordinal);
        // A registered item without a producer in the current source filter may
        // be loot, a reward, or part of another crafting system/source. It is not
        // assumed to be raw ore or to be missing from the installed catalogue.
        public readonly HashSet<string> NoKnownProducer = new HashSet<string>(StringComparer.Ordinal);
        public readonly HashSet<string> CycleItems = new HashSet<string>(StringComparer.Ordinal);
        public bool Truncated;
    }
}

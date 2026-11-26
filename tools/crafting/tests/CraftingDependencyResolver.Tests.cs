using System;
using System.Collections.Generic;
using System.Linq;

namespace ImperiumOfMan.Tests
{
    // Runs the production resolver directly, without Unity, game files or packages.
    internal static class CraftingDependencyResolverTests
    {
        private static int Main(string[] args)
        {
            var tests = new Action[]
            {
                CustomModChain,
                SharedIngredientAndAlternativeProducers,
                QuantitiesAndCoproductsRemainUnchanged,
                RecyclingCycleDoesNotHidePrimaryProduction,
                MissingRegistrationIsNotARawResource,
                RefreshRecognizesLateRegistration,
                IngredientFreeProductionIsARecipe,
                UnknownYieldIsNotReplacedWithOne,
                FilteredTreesIgnoreHiddenBranches,
                LargeGraphsReportTheirBounds
            };
            Action[] selected = args.Length == 0 ? tests : tests.Where(test => args.Contains(test.Method.Name)).ToArray();
            if (selected.Length == 0)
            {
                Console.Error.WriteLine("No named dependency scenario matched.");
                return 2;
            }
            foreach (Action test in selected)
            {
                try
                {
                    test();
                    Console.WriteLine("PASS " + test.Method.Name);
                }
                catch (Exception error)
                {
                    Console.Error.WriteLine("FAIL " + test.Method.Name + ": " + error.Message);
                    return 1;
                }
            }
            Console.WriteLine(selected.Length + " dependency scenarios passed.");
            return 0;
        }

        private static void CustomModChain()
        {
            var refine = R("another.mod/refine", A("ore", 3), A("another.mod/alloy", 2));
            var circuit = R("third.mod/electronics", A("another.mod/alloy", 2), A("third.mod/circuit", 4));
            var weapon = R("third.mod/weapon", A("third.mod/circuit", 3), A("third.mod/weapon_custom", 1));
            var graph = Build(new[] { "ore", "another.mod/alloy", "third.mod/circuit", "third.mod/weapon_custom" }, refine, circuit, weapon);
            var trace = graph.TraceIngredients("third.mod/weapon_custom");
            SameRecipes(trace, refine, circuit, weapon);
            EqualSet(trace.NoKnownProducer, "ore");
            Check(trace.MissingItems.Count == 0 && trace.CycleItems.Count == 0 && !trace.Truncated,
                "An ordinary foreign-mod chain must resolve without invented missing items or cycles.");
        }

        private static void SharedIngredientAndAlternativeProducers()
        {
            var mined = R("refine", A("ore", 2), A("alloy", 1));
            var recovered = R("recover", A("scrap", 4), A("alloy", 1));
            var frame = R("frame", A("alloy", 3), A("frame", 1));
            var wire = R("wire", A("alloy", 2), A("wire", 5));
            var final = new Recipe("machine", new[] { A("frame", 1), A("wire", 2) }, new[] { A("machine", 1) });
            var graph = Build(new[] { "ore", "scrap", "alloy", "frame", "wire", "machine" }, mined, recovered, frame, wire, final);
            var trace = graph.TraceIngredients("machine");
            SameRecipes(trace, mined, recovered, frame, wire, final);
            Check(graph.Producers["alloy"].Count == 2, "Every alternative producer must remain available.");
            Check(graph.Consumers["alloy"].Count == 2, "Both downstream uses must remain available.");
            EqualSet(trace.NoKnownProducer, "ore", "scrap");
        }

        private static void QuantitiesAndCoproductsRemainUnchanged()
        {
            var original = new Recipe("station/coproduction",
                new[] { A("textile", 2), A("polymer", 1) },
                new[] { A("laminate", 8), A("liner", 8) });
            var graph = Build(new[] { "textile", "polymer", "laminate", "liner" }, original);
            var trace = graph.TraceIngredients("liner");
            Check(trace.Recipes.Count == 1 && ReferenceEquals(trace.Recipes[0], original),
                "The dependency view must retain the actual recipe rather than synthesize a replacement.");
            EqualSet(trace.Items, "textile", "polymer", "laminate", "liner");
            Check(original.Inputs[0].Count == 2 && original.Inputs[1].Count == 1 &&
                original.Outputs[0].Count == 8 && original.Outputs[1].Count == 8,
                "Dependency discovery must preserve the original input and coproduct quantities.");
            graph.AddRecipe(original);
            Check(graph.Producers["liner"].Count == 1, "Reindexing the same recipe must not duplicate it.");
        }

        private static void RecyclingCycleDoesNotHidePrimaryProduction()
        {
            var smelt = R("primary", A("ore", 3), A("plate", 2));
            var recover = R("recycle", A("plate", 1), A("scrap", 1));
            var reuse = R("reuse", A("scrap", 2), A("plate", 1));
            var graph = Build(new[] { "ore", "plate", "scrap" }, smelt, recover, reuse);
            var trace = graph.TraceIngredients("plate");
            SameRecipes(trace, smelt, recover, reuse);
            EqualSet(trace.CycleItems, "plate", "scrap");
            EqualSet(trace.NoKnownProducer, "ore");
            Check(!trace.Truncated, "A recycling cycle must terminate through cycle detection, not a budget failure.");
        }

        private static void MissingRegistrationIsNotARawResource()
        {
            var recipe = new Recipe("foreign.mod/recipe",
                new[] { A("missing.mod/component", 2), A("quest_reward", 1) },
                new[] { A("finished", 1) });
            var graph = Build(new[] { "quest_reward", "finished" }, recipe);
            var trace = graph.TraceIngredients("finished");
            EqualSet(trace.MissingItems, "missing.mod/component");
            EqualSet(trace.NoKnownProducer, "quest_reward");
            Check(graph.Producers.ContainsKey("finished"), "An orphan ingredient must not erase the registered recipe.");
        }

        private static void RefreshRecognizesLateRegistration()
        {
            var liveItems = new List<string> { "ore", "custom_part" };
            var liveRecipes = new List<Recipe>();
            CraftingDependencyResolver<Recipe> Snapshot() => Build(liveItems, liveRecipes.ToArray());
            var first = Snapshot();
            EqualSet(first.TraceIngredients("custom_part").NoKnownProducer, "custom_part");
            liveItems.Add("another_mod/reagent");
            var late = R("late.mod/recipe", A("another_mod/reagent", 2), A("custom_part", 3));
            liveRecipes.Add(late);
            var refreshed = Snapshot();
            SameRecipes(refreshed.TraceIngredients("custom_part"), late);
            Check(refreshed.IsRegistered("another_mod/reagent"), "Refresh must recognize newly registered item IDs.");
            Check(!first.Producers.ContainsKey("custom_part"), "A prior snapshot must remain independent of later registration.");
        }

        private static void IngredientFreeProductionIsARecipe()
        {
            var extract = new Recipe("collect", Array.Empty<Amount>(), new[] { A("ore", 4) });
            var graph = Build(new[] { "ore" }, extract);
            var trace = graph.TraceIngredients("ore");
            SameRecipes(trace, extract);
            Check(trace.NoKnownProducer.Count == 0 && extract.Outputs[0].Count == 4,
                "Ingredient-free collection is still a known recipe with its actual yield.");
        }

        private static void LargeGraphsReportTheirBounds()
        {
            var recipes = Enumerable.Range(1, 30).Select(i => R("step" + i, A("item" + (i - 1), 1), A("item" + i, 1))).ToArray();
            var graph = Build(Enumerable.Range(0, 31).Select(i => "item" + i), recipes);
            var trace = graph.TraceIngredients("item30", recipeLimit: 3, itemLimit: 4);
            Check(trace.Truncated && trace.Recipes.Count <= 3 && trace.Items.Count <= 4,
                "A bounded view must report omitted branches without silently exceeding its limits.");
            Check(graph.Producers.Count == 30, "Limiting a view must not trim the full recipe index.");
        }

        private static void UnknownYieldIsNotReplacedWithOne()
        {
            var scripted = R("another.mod/scripted-batch", A("ore", 2), A("custom", 0));
            var graph = Build(new[] { "ore", "custom" }, scripted);
            var trace = graph.TraceIngredients("custom");
            SameRecipes(trace, scripted);
            Check(trace.Recipes[0].Outputs[0].Count == 0,
                "A provider without a registered output amount must stay unknown, not become a one-item recipe.");
        }

        private static void FilteredTreesIgnoreHiddenBranches()
        {
            var hidden = new Recipe("hidden alternative", new[] { A("missing_mod_item", 1), A("cycle_a", 1) },
                new[] { A("finished", 1) }, "Workbench");
            var cycleA = new Recipe("hidden cycle a", new[] { A("cycle_b", 1) }, new[] { A("cycle_a", 1) }, "Workbench");
            var cycleB = new Recipe("hidden cycle b", new[] { A("cycle_a", 1) }, new[] { A("cycle_b", 1) }, "Workbench");
            var hiddenOnly = new Recipe("hidden-only producer", new[] { A("missing_mod_item", 1) },
                new[] { A("hidden_only", 1) }, "Workbench");
            var visible = R("visible finish", A("intermediate", 1), A("finished", 1));
            var upstream = R("visible upstream", A("ore", 2), A("intermediate", 1));
            var graph = Build(new[] { "finished", "intermediate", "ore", "hidden_only", "cycle_a", "cycle_b" },
                hidden, cycleA, cycleB, hiddenOnly, visible, upstream);
            bool MagnumOnly(Recipe recipe) => recipe.Venue == "Magnum";
            var trace = graph.TraceIngredients("finished", 2, 3, MagnumOnly);
            SameRecipes(trace, visible, upstream);
            EqualSet(trace.Items, "finished", "intermediate", "ore");
            EqualSet(trace.NoKnownProducer, "ore");
            Check(trace.MissingItems.Count == 0 && trace.CycleItems.Count == 0 && !trace.Truncated,
                "Hidden missing items, cycles and recipes must not contaminate diagnostics or consume the visible budget.");
            var filteredOut = graph.TraceIngredients("hidden_only", 1, 1, MagnumOnly);
            EqualSet(filteredOut.NoKnownProducer, "hidden_only");
            Check(filteredOut.Recipes.Count == 0 && filteredOut.MissingItems.Count == 0 && !filteredOut.Truncated,
                "A registered item with only hidden producers has no recipe in this source view; it is not missing registration.");
            Check(graph.Producers["finished"].Count == 2,
                "Applying a source filter must leave the complete producer catalogue unchanged.");
        }

        private static CraftingDependencyResolver<Recipe> Build(IEnumerable<string> items, params Recipe[] recipes)
        {
            var graph = new CraftingDependencyResolver<Recipe>(r => r.Inputs.Select(a => a.Id), r => r.Outputs.Select(a => a.Id));
            foreach (string item in items) graph.RegisterItem(item);
            foreach (Recipe recipe in recipes) graph.AddRecipe(recipe);
            return graph;
        }

        private static void SameRecipes(CraftingDependencyTrace<Recipe> trace, params Recipe[] expected)
        {
            Check(trace.Recipes.Count == expected.Length && expected.All(trace.Recipes.Contains),
                "Expected every original recipe once: " + string.Join(", ", expected.Select(r => r.Name)));
        }

        private static void EqualSet(IEnumerable<string> actual, params string[] expected) =>
            Check(new HashSet<string>(actual, StringComparer.Ordinal).SetEquals(expected),
                "Expected item set: " + string.Join(", ", expected));

        private static void Check(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }

        private static Amount A(string id, int count) => new Amount(id, count);
        private static Recipe R(string name, Amount input, Amount output) => new Recipe(name, new[] { input }, new[] { output });

        private sealed class Recipe
        {
            public readonly string Name;
            public readonly string Venue;
            public readonly Amount[] Inputs;
            public readonly Amount[] Outputs;
            public Recipe(string name, Amount[] inputs, Amount[] outputs, string venue = "Magnum")
            { Name = name; Venue = venue; Inputs = inputs; Outputs = outputs; }
        }

        private sealed class Amount
        {
            public readonly string Id;
            public readonly int Count;
            public Amount(string id, int count) { Id = id; Count = count; }
        }
    }
}

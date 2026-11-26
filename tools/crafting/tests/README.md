# Crafting dependency checks

Run the production resolver's self-contained scenarios with .NET 8:

```sh
dotnet run --project tools/crafting/tests/CraftingDependencyResolver.Tests.csproj --configuration Release
```

The project links `src/CraftingDependencyResolver.cs` directly. It needs no Unity installation, game assemblies, NuGet test framework, saves, or running game.

The scenarios cover a foreign mod's multistep ingredient chain, shared inputs, alternative producers, unchanged quantities and coproducts, recycling cycles, missing item registration, registered items without a known producer, late registration on a fresh snapshot, ingredient-free production, unknown output amounts, source-filtered traversal, and explicit view limits.

One scenario can be selected by name, for example:

```sh
dotnet run --project tools/crafting/tests/CraftingDependencyResolver.Tests.csproj --configuration Release -- FilteredTreesIgnoreHiddenBranches
```

That regression checks that hidden alternatives do not contribute missing ingredients or cycles, spend the visible traversal budget, or change the complete recipe catalogue. A registered item with only hidden producers is reported as having no recipe in the current source view, not as a missing item.

The runtime adapter rebuilds its snapshot from the game's current item, Magnum, workbench, station, and disassembly registrations whenever the map refreshes. Other mods using those registrations are discovered without an item-name allowlist. A separate crafting system that does not publish recipes into those tables cannot be inferred; no replacement recipe or balance adjustment is invented for it. The checks here verify dependency discovery, not native game execution or another mod's private crafting hooks.

Magnum receipt records provide an output ID but no yield field. The adapter uses the explicit Imperium batch table when available and otherwise keeps the output amount unknown, since another mod can change it through a production hook. Workbench output amounts likewise remain unknown. Explicit station and disassembly amounts are preserved; uncertain disassembly success is described separately.

# Runtime ingredient discovery

The Crafting Map builds a new snapshot when it opens and when the player selects Refresh. It reads the current registered item catalogue, Magnum recipes, workbench recipes, station recipes, item disassembly lists, and Imperium recycling profiles. Recipes and items added by other mods to those tables are included without an item-name allowlist.

The dependency resolver follows every known producer of a required ingredient, including alternative recipes and shared ingredients. It applies the current source filter before following a branch or spending the view budget, so hidden alternatives do not contribute missing-item or cycle warnings. It keeps the original recipe objects and their inputs, output lists, quantities, and conditions. Coproducts remain connected to the recipe that produces them. Cycle detection prevents recycling loops from recursing indefinitely; a limited view reports truncation instead of silently treating the omitted branches as absent.

## Quantity evidence

| Recipe source | Displayed output amount | What that means |
|---|---|---|
| Magnum output in `IndustryBatches` | Explicit Imperium batch amount | Taken from this mod's registered batch contract. |
| Other Magnum output | Unknown (`Count = 0`, displayed as `?`) | Native receipt records provide an output ID but no output-quantity field. Another mod can change its batch through a production hook, so the map does not assume one item. Player wording: “Check Production for the amount made.” |
| Workbench | Unknown (`Count = 0`, displayed as `?`) | The current adapter has no verified output amount. |
| Station | Explicit registered output amounts, including every coproduct | Read from the live station recipe. |
| Item disassembly | Listed possible salvage amounts | Amounts come from the item record; recovery chance and item restrictions still apply. |
| Imperium ship recycling | Explicit registered recovery amounts | Read from this mod's recycling profile. |

Registered ingredient amounts are retained. The map does not rewrite another mod's formulas or create substitute production routes. These data checks do not prove the execution behavior of an arbitrary mod's private production hooks.

## Missing data and refresh

A registered item without a known producer is not assumed to be a raw resource. It can be loot, a reward, or part of a separate crafting system. A recipe that refers to an item missing from the registered catalogue is reported separately. A broken availability check does not discard the recipe itself, and a malformed recipe does not prevent later recipes in the same provider from being indexed. New registrations become visible on a fresh snapshot.

Crafting systems that do not publish into the supported runtime tables cannot be inferred. Their absence from the map is not a claim that their recipes do not exist.

Item and station names use the game's current localization. Workbench names use the native `ui.mapobject.<id>.caption` key. Recipes without a separate display-name field are named from their actual output items; internal receipt keys remain internal. If a mod has not supplied localization, the fallback removes identifier separators and the Imperium prefix without inventing item properties. Category labels are player-facing, while the native class and category tags remain separate metadata for grouping.

## Self-contained checks

`tools/crafting/tests/CraftingDependencyResolver.Tests.csproj` compiles the production resolver directly, without Unity or game files. The initial nine scenarios cover a custom-mod chain, alternatives and shared inputs, unchanged quantities and coproducts, a recycling cycle, missing registration versus a registered item without a producer, refresh after late registration, ingredient-free production, an unknown yield, and bounded traversal. All nine passed with .NET SDK 8.0.428 during local source preparation. A tenth, focused regression for source-filtered traversal was then added and passed separately after the filter correction. Native integration and headed UI validation remain separate gates.

Run:

```sh
dotnet run --project tools/crafting/tests/CraftingDependencyResolver.Tests.csproj --configuration Release
```

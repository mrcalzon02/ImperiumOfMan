# Recipe cost and batch balance — 1.1.3

Baseline: [`7f28f8a45736f35dec8e772304a94305336a531f`](https://github.com/mrcalzon02/ImperiumOfMan/commit/7f28f8a45736f35dec8e772304a94305336a531f), the complete 1.1.2 package.

Implementation: [`a107c145f32ed9e6b567aa4380ae840a4e7d79d2`](https://github.com/mrcalzon02/ImperiumOfMan/commit/a107c145f32ed9e6b567aa4380ae840a4e7d79d2).

The requested change preserves multi-ingredient recipes, reduces ingredient quantities, and increases output across custom Magnum fabrication. This document records the source change and inspected native behavior. [Native build 45762690450](https://github.com/mrcalzon02/Dens-Mortis/actions/runs/45762690450), requested by controller commit `902f412fd61bed8a61a096c0c1bf4900ff722c36`, is queued and has not started. A new compiled package or live gameplay result is not claimed here.

## Rule and scope

`IndustryRecipe` applies the adjustment once when registering a recipe:

- Each ingredient count becomes `max(1, ceil(previous count / 2))`. Counts of 1, 2, 3, and 4 therefore become 1, 1, 2, and 2. Every ingredient type remains required.
- The declared output batch is doubled. The existing production hook applies that batch to newly queued orders.
- Ingredients are copied into new `ItemQuantity` objects. The original arrays can still be used by station factories or native disassembly records without receiving the player cost adjustment.
- Recipe IDs, base production times, chip pools, and unlock conditions are unchanged. Single-item ingredients stay at one; doubled output still halves their material cost per produced item.

The common helper covers shared foundations, ration and medical lines, weapons, armor, ammunition, specialist equipment, custom cybernetics, custom deployables, Chaos recipes, and mod-created support fallback recipes. Some of these explicit mod recipes produce native items such as Glass, Cable, water, ammunition, and medical kits. They receive the same rule.

`EnsureSupportRecipe` leaves an existing native production recipe in place. Only a missing recipe created through `IndustryRecipe` receives this change. Faction station manufacturing, trade stock, prices, loot, Ore Cargo contents, and recycling yields are outside this adjustment.

The preliminary Las Clip receipt in `RegisterGuardProduction` is registered before the `IndustryRecipe` replacement. Its final cost and output therefore follow the common rule without a second adjustment. No separate custom production-receipt insertion path needs a second multiplier.

Source: [recipe registration and production hook](../src/Plugin.Industry.cs), [registration order](../src/Plugin.Core.cs), [support fallback](../src/Plugin.SupportProduction.cs), [shared ordnance inputs](../src/Plugin.SpecialistSupplies.cs), [deployable inputs](../src/Plugin.DeployableWorkshops.cs), and [cybernetics inputs](../src/Plugin.Cybernetics.cs).

## Representative orders

Each row is one newly queued order. Base hours remain subject to the game's existing production-speed and difficulty modifiers.

| Product | 1.1.3 ingredients | Previous output | 1.1.3 output | Base hours |
| --- | --- | ---: | ---: | ---: |
| Corpse Starch | 1 vat ration base + 1 wrapper | 4 | 8 | 3 |
| Dirty Corpse Starch | 1 raw vat biomass + 1 wrapper | 3 | 6 | 2 |
| Raw vat biomass | 1 Piece of Flesh + 1 Water Bottle | 4 | 8 | 12 |
| Vat ration base | 2 raw vat biomass | 4 | 8 | 3 |
| Ration wrapper | 1 paper towel | 4 | 8 | 2 |
| Lasgun | 1 rolled armor plate + 1 armor fitting + 1 weapon component | 1 | 2 | 12 |
| Las Clip | 1 sealed cell assembly + 1 empty housing | 6 | 12 | 3 |
| Metal smelting feedstock | None | 2 | 4 | 3 |
| Water Bottle | None | 2 | 4 | 3 |
| Refined metal stock | 2 ore | 2 | 4 | 6 |
| Conductive scrap | 2 ore | 2 | 4 | 2 |
| Silicate feedstock | 2 ore | 2 | 4 | 2 |
| Glass | 1 silicate feedstock | 2 | 4 | 2 |
| Industrial Glass | 2 silicate feedstock | 2 | 4 | 4 |
| Cable | 2 conductive scrap | 1 | 2 | 3 |

## Native order and save behavior

The installed-game capture from [run 45755660429](https://github.com/mrcalzon02/Dens-Mortis/actions/runs/45755660429), pinned in [the captured `ItemProductionSystem`](https://github.com/mrcalzon02/Dens-Mortis/blob/edb2f8fe12b5329a04454b7dcaf777b800352825/runner-results/quasimorph-native/45755660429-1/native-api/MGSC.ItemProductionSystem.txt), uses game assembly SHA-256 `d72cd91f98de4d6cce8ee420ac1a906d89e68ba0cb164694d266b56f46339ebe`.

- Starting production sets the order's initial `Count` to the requested number of orders. Duration is calculated from recipe hours, production-speed effects, difficulty, and that requested count.
- The native start path expands `RequiredItems` into the actual paid-item list and removes those items. The mod then multiplies the new order's output `Count` by its registered batch; it does not multiply the paid-input list or duration.
- Completion produces and adds one item for each unit of the saved output `Count`. `ProduceItem` sets `StackCount` to one, including ammunition, and removes loaded ammunition from newly produced weapons. The custom batch therefore denotes produced item units, rather than native maximum-size stacks.
- Cancellation refunds one item per entry in the saved `RequiredItems` list. It does not multiply the refund by the output `Count`.
- Existing queued orders retain their saved output count, duration, and paid-input snapshot. Registering new recipe costs and batches does not reprocess those orders.

These are source and native-code observations. They do not establish a live-game UI or gameplay pass for 1.1.3. The production-list screenshot alone does not show the batch output count.

## Recycling diagnostic

The larger fabrication batches make some material return paths profitable. For example, two Glass now produce four cullet; recycling those four cullet recovers four silicate; four basic-Glass orders then produce sixteen Glass. The route consumes production time and turns two Glass into sixteen.

Industrial Glass also gains material on a return path: two Industrial Glass recycle into two cullet and then two silicate, which fabricate four Industrial Glass. Ore and bottled water already have time-only production routes.

This is a balance diagnostic, not evidence of a build or launch failure. No recovery-yield changes are included in this request. The 1.1.2 audit's material-loss calculations are historical and must not be cited as proof that the new recipe economics prevent multiplication or trade arbitrage.

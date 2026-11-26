# Recipe foundations and dependency audit

**Historical 1.1.2 audit.** The 1.1.3 [recipe cost and batch change](RecipeCostBatchBalance.md) supersedes the quantities and recycling-return calculations below. This document retains the original implementation and build evidence; its material-loss results do not describe 1.1.3.

Implementation commit: `c474fe05e5257236a6c5ff2854d2df30273071cd` (includes Ore Cargo unpacking). The foundational recipe tree was first published as `0272f112f236b331d8e8f13c20f8a0f68175ce9d`.

Compared with remote baseline `bbf977dda6e16933a4f6c9b504b079b365ea8d15`.

Status: source published to `main` and read back. Native build [45755660429](https://github.com/mrcalzon02/Dens-Mortis/actions/runs/45755660429) passed against the installed game with zero warnings and zero errors in 13.20 seconds. All thirty transport chunks, the ZIP hash and CRC, and all four runtime member hashes were verified. The compiled assembly reports version `1.1.2.0` and contains the Ore Cargo unpacking hook. Install the complete 1.1.2 ZIP; the repository's separate loose runtime folder remains the older baseline. No live gameplay pass is claimed.

## Implemented recipes

| Product per order | Inputs | Base hours |
| --- | --- | --- |
| 2 metal smelting feedstock (ore) | None | 3 |
| 2 Water Bottles | None | 3 |
| 2 refined metal stock | 3 ore | 6 |
| 2 conductive scrap | 3 ore | 2 |
| 1 Cable | 3 conductive scrap | 3 |
| 2 silicate feedstock | 3 ore | 2 |
| 2 basic Glass | 2 silicate feedstock | 2 |
| 2 Industrial Glass | 3 silicate feedstock | 4 |

The ore and water orders consume only production time. All listed recipes except conductive scrap are shared starter recipes and are granted by the existing new-game and save-load hooks. Conductive scrap retains its Workshop Fundamentals schematic unlock. Basic and industrial glass are separate silicate products with different material costs and processing times.

Sorted metal scrap recycles into ore one-for-one. Cullet recycles into silicate one-for-one. These salvage routes keep existing stock useful while removing salvage as a mandatory starting ingredient.

## Ore Cargo unpacking

Each native Ore Cargo crate opens into twelve metal smelting feedstock, six silicate feedstock, and six conductive scrap. The change replaces its `TrashRecord.Disassembly` contents list; it does not create a new container, add a fabrication recipe that consumes trade cargo, or generate inventory during save loading. Its native item ID, price, weight, stack capacity, trade classification, and station uses remain intact.

A narrow Harmony prefix sets the native `guaranteed` flag only for ready, positive-count, unlocked, non-quest Ore Cargo. The original disassembly method consumes the chosen source count before spawning and handles stack limits and overflow. It remains the only item-generation path. Existing crates use the updated global record when opened; no per-container migration is necessary. Eight crates yield 96 ore, 48 silicate, and 48 conductors before native stack splitting.

The screenshot’s `0/8` is the item stack count and maximum stack, not an empty internal inventory. This was verified in the current installed-game `ItemTooltipBuilder` capture. The exact current menu label has not been checked in a live game.

## Confirmed reversals corrected

1. **Conductive scrap / Cable:** the previous recipes consumed three Cable to make two conductive scrap, then three scrap to make one Cable. Scrap now comes from ore. The old loop was a circular prerequisite that lost material, not a multiplication exploit.
2. **Automata controllers / AI modules:** Cathode Rampart previously consumed an AI module to make basic Logic Cores, while player and Broadsword AI-module recipes consumed Logic Cores. Cathode now uses two circuit boards, two microelectronics parts, one capacitor, and one sealed cell. Its existing twelve-core yield and 84-hour station processing time are retained.

The glass chain had an external-source gap: it required existing glass to make cullet and silicate. Ore now supplies silicate directly, and silicate can produce either Glass product.

## Dependency review

A temporary source extractor expanded literal declarations and inspected recipe-family helpers. It compared the baseline and patched source separately for Magnum, station, and combined manufacturing dependencies.

| Inspected source records | Count |
| --- | ---: |
| Explicit Magnum recipes | 308 |
| Explicit station manufacturing records | 303 |
| Distinct station outputs | 379 |
| Combined explicit manufacturing records | 611 |
| Additional conditional support fallback recipes | 38 |

The baseline contains the conductive-scrap/Cable cycle and the AI-module/Logic-Core cycle. Both are absent from the patched manufacturing graph. The explicit Magnum graph has no circular dependency or self-dependency.

The combined graph still contains an optional salvage component involving electrical, engineering, and robot cargo, rods, tools, and Cable. It is formed by Cryptoria robot salvage and Ares Post electrical-wiring recovery. Independent material-production routes supply these items. Excluding those two recovery operations leaves no cycle in the explicit combined manufacturing graph.

Manual inspection covered fundamentals, equipment recycling, Guard and specialist supplies, the ten Imperial equipment tiers, Chaos recipes, cybernetics, pharmaceuticals, base-game workshops, automata, deployables, maintenance, armories, and provisioning. Medicine packing and equipment upgrades were retained where lower-stage items genuinely assemble into a higher-stage product. Native `nail_bolts_ammo` is loose 90 mm nails, so nails in flechette payloads are a valid component input rather than dismantling a complete cartridge. Native `glass` is a basic crafting component, distinct from the mod's Industrial Glass. See the [native crafting reference](https://quasimorph.wiki.gg/wiki/Crafting) and [Glass record](https://quasimorph.wiki.gg/wiki/Glass).

## Quantity checks on the modified Magnum routes

Checks used the declarations and recycling profiles in the modified source:

- **Empty inventory to Cable:** three ore orders produce six ore; two conductor orders consume that ore and produce four scrap; one Cable order leaves one Cable and one scrap.
- **Basic Glass return path:** six Glass become four cullet, then four silicate, then four Glass. The complete path retains two-thirds of its starting material.
- **Industrial Glass return path:** six Industrial Glass recycle into six cullet and six silicate, which refine into four Industrial Glass. It also retains two-thirds.
- **Existing sorted scrap:** six scrap recycle into six ore and then four refined metal.

These quantity checks apply to Magnum fabrication and recycling. Existing station factory batch yields differ from player batches; this review does not establish global trade-arbitrage balance.

## Native production and saved orders

The current installed-game production-system capture from [native build run 45722065128](https://github.com/mrcalzon02/Dens-Mortis/actions/runs/45722065128) was inspected:

- `GetAvailableToProduceCount` returns `int.MaxValue` for an empty required-items list.
- `StartMagnumItemProduction` creates a timed order with no consumed materials when the list is empty.
- `CancelProduction` refunds the queued order's stored `RequiredItems`, rather than recalculating from the changed recipe. Previously paid ore, conductor, or silicate orders therefore retain their original cancellation inputs.
- Existing item, recipe, and chip-selection identifiers are preserved.

The bundled game assembly's UI explicitly converts unlimited material availability into a bounded quantity selector and hides empty ingredient rows. That UI assembly differs from the current installed-game capture; this is code inspection, not a live UI test.

## Scope limits and build provenance

The source graph does not load the entire native game database. Twenty-six native implant factory ingredient arrays and fifty-one support products depend on external native recipe or disassembly records. Conditional support fallbacks are counted separately because native recipes take precedence. Issue-only station sinks are excluded; retired recipe definitions can remain in static counts for existing saved orders. Dependency analysis does not establish all tech gates, prices, freight reachability, or live gameplay behavior.

The exact native-build request is `RecipeFoundationsBuildRequest.json`. It was submitted to the existing Dens-Mortis workflow by controller commit `8783076f8675e8747e43ec37087061d2b18f167e`. The 1.1.2 package comes from that pinned source build and matches its receipt. Compilation and package checks remain separate from live gameplay verification.

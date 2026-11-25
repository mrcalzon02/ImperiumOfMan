# Fury, food, data-chip, and fabrication corrections — 2026-11-25

## Source and verification status

Source reviewed: [`1332988a4b5645727c966a2836e098bc9fd1af3d`](https://github.com/mrcalzon02/ImperiumOfMan/commit/1332988a4b5645727c966a2836e098bc9fd1af3d).
This includes the twelve player-recipe corrections, the all-chip correction in `37afa41ab6bfee46a06db0239b6bddbcd86713be`, the Fury/food correction in `38c8da9d4d8fac3e39134aa212ca632391428edf`, and the Bound Ash correction in `a1ba1358c8f77f7d43e8f6956f15a3c1c500947a`.

| Evidence | Status |
| --- | --- |
| Source diff and acquisition-path review | Complete; `git diff --check` passed |
| Chip source harness with explicit game-API test doubles | **16 assertions passed** |
| Food source harness with explicit game-API test doubles | **24 assertions passed** |
| Full build against the installed game | **Passed in 8.93 seconds, zero warnings and zero errors** for the exact source above in [run 45722065128](https://github.com/mrcalzon02/Dens-Mortis/actions/runs/45722065128), using .NET SDK 9.0.203 |
| Current-game launch, gameplay, and screenshots | **Not run** |
| Final runtime package | **Verified**: ZIP CRC, all four member hashes, byte length, SHA-256, and Git blob identity match the native build receipt |

The 40 harness assertions exercise production-source logic with substitutes for the game APIs. They do not establish native game loading, Harmony compatibility, rendering, or gameplay success. The full source also compiled against the real assemblies from the runner's installed Quasimorph game. That successful compile is separate from runtime and gameplay verification. The earlier Bound-Ash-only ZIP is a separate artifact and does not contain all changes described here.

### Download and package identity

Download [ImperiumOfMan-1.1.1-FuryFoodChipFixes.zip](../package/ImperiumOfMan-1.1.1-FuryFoodChipFixes.zip). Close Quasimorph, extract its `ImperiumOfMan` folder over the existing mod folder, then reload the game. Assembly version is `1.1.1.0`.

| Property | Value |
| --- | --- |
| ZIP bytes | `11728251` |
| ZIP SHA-256 | `8445e076eca9a9c66b0daf8b5dbfc1b597642cb3d4c94d17de2a6b3d7be7ed86` |
| ZIP Git blob | `ecaa7c2372aee53e993d1b8de33aafcfa2c02d19` |
| Built DLL SHA-256 | `20bc670f3a86fe32ac18cfff515d5ed4e3f4df97f5d3fdd5cfb42102b8428dbf` |
| Referenced game `Assembly-CSharp.dll` SHA-256 | `d72cd91f98de4d6cce8ee420ac1a906d89e68ba0cb164694d266b56f46339ebe` |

The ZIP contains only `ImperiumOfMan.dll`, `imperiumofman.bundle`, `modmanifest.json`, and `thumbnail.png` beneath `ImperiumOfMan/`. [Machine-readable build and package receipt](FuryFoodChipBuildReceipt.json) records each member's size and hash. No game assemblies, decompiled game code, runner paths, or user saves are included.

## Fury Sigil: faction purchase

- Hostile Furies have a **0% sigil drop chance**. Their existing scrip, salvage, and Bound Ash payouts remain.
- Enabled Chaos Undivided at Tech 1+ stocks one sigil at each currently owned station, through the existing market initialization/load/restock path. Stock is kept outside shared contract-reward and raid-cache tables.
- No Fury Sigil fabrication recipe is registered. Carrion Mark cannot sidegrade into it; Step Between cannot downgrade into it. Already owned sigils remain usable and can evolve into other pacts.
- Save loading removes the retired sigil recipe unlock and cancels matching queued production through the native cancellation API, which supplies the ingredient refund. Other jobs and owned/equipped sigils are retained.

Ownership gates new stock. Previously generated physical items, including stock already present when a station changes owner, are not purged.

Source: [pacts](../src/Plugin.ChaosPacts.cs), [kill loot](../src/Plugin.ChaosEnemies.cs), [production](../src/Plugin.ChaosProduction.cs), [markets and save migration](../src/Plugin.IndustrySupply.cs).

## Imperial raid food

Newly generated Imperial procedural/story floors receive existing **Corpse Starch** and **Dirty Corpse Starch** as ordinary container or floor loot. Imperial campaign missions are included even away from Imperial-owned stations. Peaceful visits and unrelated raids are excluded.

For `R` eligible reachable floor cells, clean rations are `min(16, max(4, ceil(R / 200)))`; dirty rations are `ceil(clean / 4)`. A floor with no eligible cells receives none.

| Eligible cells | Clean | Dirty | Total |
| ---: | ---: | ---: | ---: |
| 400 | 4 | 1 | 5 |
| 1,600 | 8 | 2 | 10 |
| 3,200 or more | 16 | 4 | 20 |

The first clean ration is placed near the actual arrival position. Remaining items use compatible non-quest containers with reachable, unfenced access; inaccessible or full containers fall back to clear floor positions. Wet, special, and blocked floor cells are excluded. This runs once after successful new-floor creation, after furnishing. Loading an existing saved floor does not add supplies or replenish looted food.

The 24 food assertions cover allocation/caps, later floors, faction scope, arrival placement, container compatibility and access, full-container fallback, multi-cell containers, failed generation, and the new-floor-only hook. Source: [ration placement](../src/Plugin.ImperialRationLoot.cs).

## All custom data chips

All **103 custom production chips** use native single-recipe selection. The **three class chips** already select one class and remain unchanged. Selection is stored on each physical chip and shown by the native tooltip; known duplicates use native behavior. Native weapon/ammunition pairing, native armor-set companion unlocks, and original base-game chip pools are preserved. The mod does not register armor sets for its custom Imperial or Chaos armor, so those pieces remain individual unlocks. The RealWare protective armor pattern can retain the base game's matching armor-set unlock.

| Example chip | Recipe possibilities per chip | Recipes selected |
| --- | ---: | ---: |
| Hollow Signal Pattern | 8 | 1 |
| Field Support Patterns | 11 | 1 |
| Imperial Field Plans | 4 | 1 |
| Workshop Fundamentals | 3 scrap-sorting recipes | 1 |

The full pools are finalized after the seven specialist extensions, retaining their 15 additional recipes and removing duplicate entries. AI Module remains in Support Tech 5. Bulk-unlock, whole-catalog tooltip, and whole-catalog known-state overrides are removed. Loading a save no longer expands Field Plans or specialist catalogs from one known recipe. Shared starter-material recipes remain available.

Existing learned recipes and chips' stored selections are preserved, except the explicitly retired Fury Sigil fabrication recipe; old chips are not rerolled. The 16 chip assertions cover pool registration/finalization, late extensions, native-pool preservation, deduplication, descriptions, unchanged tech/prices, and removal of bulk grants. Inspection of the actual installed game's `ItemFactory`, `DatadiskComponent`, `ItemInteractionSystem`, and `ItemTooltipBuilder` confirms the saved selection and native companion rules. Source: [registration](../src/Plugin.Industry.cs), [extensions](../src/Plugin.SpecialistSupplies.cs), [load handling](../src/Plugin.IndustrySupply.cs).

## Included Bound Ash correction

Bound Ash has a one-cell inventory footprint, a one-cell-scaled icon, and base stack size **30**. Saved instances receive the corrected width and native difficulty-adjusted stack limit while preserving counts. See [Bound Ash provenance and verification](BoundAshInventoryFix.md) for the earlier artifact's separate evidence.

## Player fabrication uses loose materials

The Magnum biomass recipe is **one Piece of Flesh + one Water Bottle → four raw vat biomass**, retaining its 12 base hours. Its game-displayed duration follows the existing production-speed modifiers. The raw meat item is `human_meat`; it is not a food cargo package. Native recipe inputs are conjunctive, so this is the explicit meat-and-water recipe rather than a new ingredient-alternative system.

Twelve custom player recipes no longer consume packaged trade goods:

| Recipe | Replacement for cargo input |
| --- | --- |
| Raw vat biomass | One Piece of Flesh replaces food cargo; one Water Bottle remains required. |
| Harvested tobacco | One raw vat biomass replaces food cargo; one Water Bottle remains required. |
| Sterile implant scaffold | Two raw vat biomass replace one Biowaste Tank. |
| Tissue Graft | Two raw vat biomass replace one Biowaste Tank. |
| Aegis Ward | Two microelectronics parts and one lens replace one Research Station. |
| Golden Aegis Ward | Four microelectronics parts and two lenses replace two Research Stations. |
| Carapace Seed, Spine Knot, Veil Tendon, Prism Membrane | Two raw vat biomass replace one Quasi-samples item in each player recipe. |
| Masked Optic, Echo Pilgrim | Four raw vat biomass replace two Quasi-samples items in each player recipe. |

The remaining ingredients, recipe times, yields, and unlock requirements are preserved. The implant conversion creates a separate player ingredient array, so faction laboratory orders keep their original trade-cargo inputs. Station industry and freight continue to provide demand for packaged cargo. Explosives remain a normal native crafting component. Purpose-built intermediates such as treatment packs, sealed cells, and implant controllers remain ingredients.

The flesh-and-water → biomass → food/tobacco dependency is acyclic. This section records the audited custom recipes; it does not claim to rewrite all native game recipes or other mods' inputs. Source: [industry](../src/Plugin.Industry.cs), [cybernetics](../src/Plugin.Cybernetics.cs), [Tissue Graft](../src/Plugin.SpecialistSupplies.cs), [Aegis Ward](../src/Plugin.GreyKnights.cs), [Golden Aegis Ward](../src/Plugin.GoldenPaladins.cs).

## Remaining runtime verification

Compilation and package verification are complete. The game was not launched, and no live game or save was modified. In-game acceptance still needs to cover Fury loot and faction shop access; queued-order refunds; new-floor food and save/reload behavior; selected chip and duplicate behavior; biomass crafting; and fresh/saved Bound Ash footprint, stacking, and count preservation. The exact native game reference is identified by its hash above; this report does not infer a game version label from the mod manifest.

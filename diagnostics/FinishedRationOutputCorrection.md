# Finished-ration output correction — 1.1.10

**Source:** `5fae43b49ff9fe4ee21cb88a3e8e30eb8e3558ad`, assembly **1.1.10.0**. [Native run 45870535209](https://github.com/mrcalzon02/Dens-Mortis/actions/runs/45870535209) passed with zero warnings and zero errors in 12.43 seconds. The [current receipt](FinishedRationBuildReceipt.json) verifies the package, all 24 embedded PNGs and compiled correction. No game was launched for this source.

## Intended direction

| Consumed materials | Produced batch | Baseline time |
| --- | --- | --- |
| 1 Raw Vat Biomass + 1 wrapper | 6 Dirty Corpse Starch | 2 ship-hours |

This existing forward recipe is unchanged. Finished **Corpse Starch** and **Dirty Corpse Starch** are production outputs, not recycling inputs. Both cloned ration records have explicitly empty disassembly lists.

The correction removes the reverse eight-ration biomass route, its bulk-input accounting and remainder logic, and its dedicated recycler-button/item-tooltip hooks. The live Crafting Map no longer advertises that recovery route. Clone harvesting, eligible spoiled-organic recovery and protection of locked/quest custom inputs remain separate systems. The six capability ranks, physical recipe knowledge, occult quantities and artwork are unchanged.

## Compiled verification

The verified DLL retains the forward ration formula and creates a fresh empty disassembly list for both finished ration records. The reverse profile, eight obsolete bulk/UI types, eight helpers and three fields are absent. These are static checks on the compiled assembly; they do not claim a new gameplay transaction. The four-member runtime ZIP is available as [ImperiumOfMan-1.1.10-CraftingMap.zip](../package/ImperiumOfMan-1.1.10-CraftingMap.zip).

## Reference and evidence

The bounded offline manufacturing graph already contains Dirty Corpse Starch only as an output. Its forward quantities and the 751-record catalogue are retained; the source pin and file fingerprints are refreshed for this correction. Recycling profiles are outside that offline parser's scope.

The [1.1.9 package documentation](../package/README-1.1.9.md), [347 build/runtime receipt](OccultBuildReceipt.json) and [native captures](../docs/crafting/native-occult-ui-347de7d.json) remain historical records of the version they tested. The old reverse route was never accepted through a completed native recycling transaction. Those screenshots and receipts are not relabeled as 1.1.10 evidence.

[Correction change notes](../docs/crafting/CHANGE_NOTES_1.1.10.txt) · [Current installation/build status](../package/README.md).

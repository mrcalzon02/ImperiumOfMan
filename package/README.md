# Runtime package

**Current source — 1.1.11:** [e6c3026b3a11e4194b4597866099fcee850d836a](https://github.com/mrcalzon02/ImperiumOfMan/commit/e6c3026b3a11e4194b4597866099fcee850d836a) contains the Crafting Map appearance pass. Its native compilation and runtime checks are pending. The verified 1.1.10 ZIP below predates that appearance pass.

## Current verified package — 1.1.10 finished-ration output correction

Source **`5fae43b49ff9fe4ee21cb88a3e8e30eb8e3558ad`**, assembly **1.1.10.0**. Download the verified **[ImperiumOfMan-1.1.10-CraftingMap.zip](https://raw.githubusercontent.com/mrcalzon02/ImperiumOfMan/186e2b76464b91888101657caec5e5cd531c80bb/package/ImperiumOfMan-1.1.10-CraftingMap.zip)**. [GitHub file page](https://github.com/mrcalzon02/ImperiumOfMan/blob/186e2b76464b91888101657caec5e5cd531c80bb/package/ImperiumOfMan-1.1.10-CraftingMap.zip). Native compilation passed with zero warnings and zero errors; archive, compiled correction and all 24 embedded PNGs are verified. [Published verification receipt](https://github.com/mrcalzon02/ImperiumOfMan/blob/186e2b76464b91888101657caec5e5cd531c80bb/diagnostics/FinishedRationBuildReceipt.json) · [Correction notes](../docs/crafting/CHANGE_NOTES_1.1.10.txt).

[Screenshot gallery and direct full-size image links](../docs/crafting/DELIVERABLES.md) · [Installation](#install-the-verified-1110-package-for-testing). The gallery records historical 1.1.9/source-347 observations.

**1 Raw Vat Biomass + 1 wrapper produces 6 Dirty Corpse Starch in 2 base ship-hours.** Finished Corpse Starch and Dirty Corpse Starch are outputs, not recycling inputs. The reverse ration-to-biomass route, its bulk-only bookkeeping and its dedicated recycler UI have been removed; each ration record receives an independent empty disassembly list, preserving the native donor item.

Clone harvesting, eligible spoiled-organic recovery, equipment salvage and locked/quest-item protection remain in place. The six occult capabilities, independently learned physical patterns, map browser, chemistry, ship support, campaigns and artwork are retained. [Correction scope](../diagnostics/FinishedRationOutputCorrection.md) · [Occult capabilities](../diagnostics/OccultResearchExpansion.md) · [Production and recovery](../diagnostics/OccultOrganicProduction.md).

## Install the verified 1.1.10 package for testing

Use the complete verified 1.1.10 ZIP linked above. Close Quasimorph and open the established Workshop installation:

```text
D:\SteamLibrary B\steamapps\workshop\content\2059170\3754620214\
```

For another Steam library, use the same `steamapps\workshop\content\2059170\3754620214\` suffix. Extract the ZIP and copy these four files from its `ImperiumOfMan` folder directly into `3754620214`, replacing the existing copies:

- `ImperiumOfMan.dll`
- `imperiumofman.bundle`
- `modmanifest.json`
- `thumbnail.png`

Keep those files directly inside the Workshop folder. The separate loose `package/ImperiumOfMan/` directory in this repository is the older baseline.

## Verification scope

[Native run 45870535209](https://github.com/mrcalzon02/Dens-Mortis/actions/runs/45870535209) compiled the exact source in 12.43 seconds with zero warnings/errors. Verification checked all 30 transport chunks, the four exact archive members, CRC and hashes, assembly 1.1.10.0 and all 24 embedded PNGs. Compiled-code inspection confirms the reverse registration and obsolete bulk/UI machinery are absent, the forward recipe remains, and each finished-ration record receives a fresh empty disassembly list.

The ZIP is **11,794,130 bytes**, SHA-256 `8af3889a259a8a988d7e2a154d4a87af653b59b3a05de5c9616a32e529e00cf1`, Git blob `23af475717941b31052bac180a47568267881483`. No game was launched for 1.1.10.

Existing source-347 evidence documents 1.1.9 map interactions, a bounded 46-hour-20-minute game-time check, six research nodes and the corrected Culture/Manifestation tooltips. Its original package/save restoration and fixture cleanup were verified. These records keep their original source and version; they do not establish native gameplay acceptance of the corrected 1.1.10 binary.

Research purchases, physical-chip learning, exact recycling and sample/cargo consumption, cancellation/refunds, legacy migration and purchased/item sprite states retain their separate test scope. The historical source-347 [session-007 acceptance](../diagnostics/CraftingMapAcceptance-347de7d-007.json) already verifies the named Metal smelting feedstock stack increased from **2/30 to 6/30** after one four-output batch. That session also verified Focus/double-click, station cooutputs and no-producer search; its [full historical instructions and evidence](README-1.1.9.md) are preserved.

## Artwork and source review

The [art manifest](../media/OccultProduction/art-manifest.json) links 10 generated masters, 24 runtime variants, six node IDs/positions/parents and one unchanged user-provided Navigation guide. [Research state sheet](../docs/crafting/occult-research-ranks.png) · [Inventory/floor/shadow sheet](../docs/crafting/occult-starter-sprites.png) · [Historical source-347 native captures and cleanup](../docs/crafting/native-occult-ui-347de7d.json).

The [offline source map](../docs/crafting/recipe-map.html) retains the forward Dirty Starch recipe. The [source/art review ZIP](../docs/crafting/ImperiumReforged-Crafting-Design-Review.zip) is pinned to corrected 1.1.10 source `5fae43b49ff9fe4ee21cb88a3e8e30eb8e3558ad`. It contains that source, the full reusable artwork and original reference, 1.1.10 build facts and separately attributed historical native evidence. It contains no runtime binaries or later 1.1.11 appearance changes.

## Historical packages

The preserved [1.1.9 archive](ImperiumOfMan-1.1.9-CraftingMap.zip) is the superseded package that included the removed reverse route. Its [original README](README-1.1.9.md), [cumulative notes](../docs/crafting/CHANGE_NOTES_1.1.9.txt) and [complete source-347 receipt](../diagnostics/OccultBuildReceipt.json) remain available under their original scope.

Earlier [1.1.8 instructions](README-1.1.8.md) and [1.1.4 instructions](README-1.1.4.md) are historical. The [bdb receipt](../diagnostics/OccultBuildReceipt-bdb1759.json) and its native screenshot remain tied to that earlier candidate.

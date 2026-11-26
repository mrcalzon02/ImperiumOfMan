# Runtime package

## Current verified native test package — 1.1.11

[Published source b89e15990b42d8bc4911237d5d1502fe0c283d99](https://github.com/mrcalzon02/ImperiumOfMan/commit/b89e15990b42d8bc4911237d5d1502fe0c283d99), assembly **1.1.11.0**, includes native menu styling and compact button fixes, separated processing branches and clearer connections, recipe icons, seven new material artworks with 21 runtime sprites, and an Ingredient Tree built from registered game/mod recipes. Unknown quantities remain unknown.

Ceramic feedstock and Electronic substrate separate two preparation stages while preserving total material cost and base time over two assembly orders. The first order from unprepared stock takes six base hours for four plates or five for four boards and leaves one spare intermediate. Paid Ore Cargo restock targets are 3 at Rachmaninoff, 2 at Cinder Basin Mine and 2 at Foundry Primaris, under existing ownership and Tech 1 requirements. Rachmaninoff and Cinder require CoreWard ownership; Cinder stops receiving this top-up if captured. Foundry Primaris remains Imperial. Production and trade can reduce stock between top-ups.

**[Download ImperiumOfMan-1.1.11-CraftingMap.zip](https://raw.githubusercontent.com/mrcalzon02/ImperiumOfMan/2c5f6dc35a6d41c63440a8c78c7dc01f31e826f3/package/ImperiumOfMan-1.1.11-CraftingMap.zip)** · [GitHub file page](https://github.com/mrcalzon02/ImperiumOfMan/blob/2c5f6dc35a6d41c63440a8c78c7dc01f31e826f3/package/ImperiumOfMan-1.1.11-CraftingMap.zip) · [Published build receipt](../diagnostics/CraftingMapBuildReceipt-1.1.11.json) · [Installation](#install-the-verified-1111-package-for-testing).

[Native run 45886658790](https://github.com/mrcalzon02/Dens-Mortis/actions/runs/45886658790) passed in **9.68 seconds with zero warnings and zero errors**. Verification checked all 31 transport chunks, the ZIP CRC and hashes, four exact runtime members, assembly 1.1.11.0, TMP references and compiled graph/dependency methods. The ZIP is **11,819,417 bytes**, SHA-256 `db5e72f9202d8303fdacf467af04fb7f9d59412fd5ea05d41652eb9df9bdd539`.

**Verified native test package; in-game checks incomplete. A manual foreground step is required.** Sessions 012 and 013 stopped at the Windows foreground-activation check, before a frame or any input action. No campaign was loaded with this binary; map layout, icons, intermediate recipes and a paid Ore Cargo purchase remain unverified in game. Sessions 009–013 restored the original package, preserved all 11 protected campaign files and removed their fixtures. No test game remains active. Resume when Core30 is available and the next Quasimorph test window can be brought to the foreground. [Runtime recovery record](../diagnostics/CraftingMapRuntimeRecovery-b89e159.json) · [Change notes](../docs/crafting/CHANGE_NOTES_1.1.11.txt) · [Material quantities and timing](../diagnostics/ManufacturingIntermediates.md) · [Gallery](../docs/crafting/DELIVERABLES.md).

## Install the verified 1.1.11 package for testing

Close Quasimorph and extract the complete 1.1.11 ZIP linked above. Copy these four files from its `ImperiumOfMan` folder directly into the Workshop installation, replacing the existing copies:

- `ImperiumOfMan.dll`
- `imperiumofman.bundle`
- `modmanifest.json`
- `thumbnail.png`

```text
D:\SteamLibrary B\steamapps\workshop\content\2059170\3754620214\
```

For another Steam library, use the same `steamapps\workshop\content\2059170\3754620214\` suffix. Keep the four files directly inside that folder. This is the verified native test package; its in-game checks remain incomplete, with a coordinated manual foreground step required to resume validation.

## Historical verified package — 1.1.10 finished-ration output correction

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

## 1.1.10 verification scope

[Native run 45870535209](https://github.com/mrcalzon02/Dens-Mortis/actions/runs/45870535209) compiled the exact source in 12.43 seconds with zero warnings/errors. Verification checked all 30 transport chunks, the four exact archive members, CRC and hashes, assembly 1.1.10.0 and all 24 embedded PNGs. Compiled-code inspection confirms the reverse registration and obsolete bulk/UI machinery are absent, the forward recipe remains, and each finished-ration record receives a fresh empty disassembly list.

The ZIP is **11,794,130 bytes**, SHA-256 `8af3889a259a8a988d7e2a154d4a87af653b59b3a05de5c9616a32e529e00cf1`, Git blob `23af475717941b31052bac180a47568267881483`. No game was launched for 1.1.10.

Existing source-347 evidence documents 1.1.9 map interactions, a bounded 46-hour-20-minute game-time check, six research nodes and the corrected Culture/Manifestation tooltips. Its original package/save restoration and fixture cleanup were verified. These records keep their original source and version; they do not establish native gameplay acceptance of the corrected 1.1.10 binary.

Research purchases, physical-chip learning, exact recycling and sample/cargo consumption, cancellation/refunds, legacy migration and purchased/item sprite states retain their separate test scope. The historical source-347 [session-007 acceptance](../diagnostics/CraftingMapAcceptance-347de7d-007.json) already verifies the named Metal smelting feedstock stack increased from **2/30 to 6/30** after one four-output batch. That session also verified Focus/double-click, station cooutputs and no-producer search; its [full historical instructions and evidence](README-1.1.9.md) are preserved.

## Artwork and source review

The [art manifest](../media/OccultProduction/art-manifest.json) links 10 generated masters, 24 runtime variants, six node IDs/positions/parents and one unchanged user-provided Navigation guide. [Research state sheet](../docs/crafting/occult-research-ranks.png) · [Inventory/floor/shadow sheet](../docs/crafting/occult-starter-sprites.png) · [Historical source-347 native captures and cleanup](../docs/crafting/native-occult-ui-347de7d.json).

The [offline source map](../docs/crafting/recipe-map.html) retains the forward Dirty Starch recipe. The [source/art review ZIP](../docs/crafting/ImperiumReforged-Crafting-Design-Review.zip) is pinned to corrected 1.1.10 source `5fae43b49ff9fe4ee21cb88a3e8e30eb8e3558ad`. It contains that source, its reusable artwork and original reference, 1.1.10 build facts and separately attributed historical native evidence. It contains no runtime binaries or later 1.1.11 changes.

## Historical packages

The preserved [1.1.9 archive](ImperiumOfMan-1.1.9-CraftingMap.zip) is the superseded package that included the removed reverse route. Its [original README](README-1.1.9.md), [cumulative notes](../docs/crafting/CHANGE_NOTES_1.1.9.txt) and [complete source-347 receipt](../diagnostics/OccultBuildReceipt.json) remain available under their original scope.

Earlier [1.1.8 instructions](README-1.1.8.md) and [1.1.4 instructions](README-1.1.4.md) are historical. The [bdb receipt](../diagnostics/OccultBuildReceipt-bdb1759.json) and its native screenshot remain tied to that earlier candidate.

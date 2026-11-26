# Runtime package

Use **[ImperiumOfMan-1.1.2-RecipeFoundations.zip](ImperiumOfMan-1.1.2-RecipeFoundations.zip)** for the current update. Close Quasimorph, extract the ZIP’s `ImperiumOfMan` folder over the existing mod folder, and reload your save.

## Recipe foundations

- Produce two metal smelting feedstock (ore) or two Water Bottles in three base hours with no ingredients.
- Convert ore into refined metal, conductive scrap for Cable, or silicate feedstock.
- Make two basic Glass from two silicate in two base hours, or two Industrial Glass from three silicate in four base hours.
- Open one Ore Cargo crate for twelve ore, six silicate, and six conductive scrap. The selected crate is consumed through the native container action.
- Recycle stored metal scrap and glass cullet into useful ore and silicate.
- Cathode Rampart builds Logic Cores from electronic components and a sealed cell; AI modules are downstream products.

Ore, water, and shared refining recipes are added on loading an existing save as well as at a new-game start. Conductive scrap retains its Workshop Fundamentals schematic unlock. Existing queued orders retain their paid ingredients for cancellation refunds.

## Included previous corrections

This full package also includes the previous custom-chip selection fix, Fury Sigil faction-purchase restrictions, clean and decayed ration loot on new Imperial floors, meat-and-water biomass and other loose-material recipe corrections, and the Bound Ash footprint and stacking fix.

## Build and verification

The 1.1.2 ZIP contains all four runtime files: the newly compiled DLL, authoritative manifest, existing Unity content bundle, and thumbnail. Its ZIP checksum, CRC, and all four file checksums were verified against the native build receipt.

The separate loose `ImperiumOfMan/` folder remains the older baseline. Install the complete 1.1.2 ZIP for the current fixes. The binary-upload transport accepted the compressed package but could not accept the larger loose DLL.

The native build uses source `c474fe05e5257236a6c5ff2854d2df30273071cd` and the installed Quasimorph game. See [run 45755660429](https://github.com/mrcalzon02/Dens-Mortis/actions/runs/45755660429), the [recipe audit](../diagnostics/RecipeFoundationsAudit.md), and the [build receipt and package hashes](../diagnostics/RecipeFoundationsBuildReceipt.json). Compilation and package verification are separate from live gameplay; no live-game test is claimed.

The older ZIPs remain available for reference. Use the 1.1.2 Recipe Foundations ZIP for all current corrections.

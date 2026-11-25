# Runtime package

Use **[ImperiumOfMan-1.1.1-FuryFoodChipFixes.zip](ImperiumOfMan-1.1.1-FuryFoodChipFixes.zip)** for the current corrections. Close Quasimorph, extract the ZIP's `ImperiumOfMan` folder over the existing mod folder, and reload your save. The package contains the compiled DLL, manifest, existing content bundle, and thumbnail.

This package includes:

- One saved native recipe selection per custom production chip, including faction, support, requisition, and Chaos chips. Normal weapon-ammunition and native armor-set companion unlocks still apply. Existing learned recipes remain learned, except the retired Fury Sigil fabrication recipe; existing chips keep their stored selection.
- Fury Sigil faction purchase restrictions, including removal from Fury loot and fabrication.
- Clean and decayed Imperial rations on newly generated Imperial raid floors. Already generated floors are not replenished on load.
- Meat-and-water biomass fabrication and raw-material replacements for packaged trade cargo in twelve custom player recipes.
- The earlier Bound Ash inventory footprint and stacking correction.

The loose `ImperiumOfMan/ImperiumOfMan.dll` is the original compiled baseline and does **not** contain these corrections. The older `ImperiumOfMan-BoundAsh-Fix.zip` covers only Bound Ash. Building `src/ImperiumOfMan.csproj` against the installed Quasimorph game regenerates the loose runtime package from source.

See [source, build provenance, package hashes, and verification limits](../diagnostics/FuryFoodChipBalanceFix.md). Current-gameplay validation is recorded separately from compilation; a successful compile is not a gameplay pass.

This ZIP was built from source `1332988a4b5645727c966a2836e098bc9fd1af3d` against the installed game with zero warnings and zero errors. Its SHA-256 is `8445e076eca9a9c66b0daf8b5dbfc1b597642cb3d4c94d17de2a6b3d7be7ed86`; its size is 11,728,251 bytes. The [machine-readable receipt](../diagnostics/FuryFoodChipBuildReceipt.json) also records every runtime member hash. Live gameplay has not been tested.

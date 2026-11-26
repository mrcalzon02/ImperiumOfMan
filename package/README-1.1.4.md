# Runtime package

## 1.1.4 Chemistry Foundations test build

Download **[ImperiumOfMan-1.1.4-ChemistryFoundations.zip](ImperiumOfMan-1.1.4-ChemistryFoundations.zip)**. Native compilation and package verification passed. Game boot and gameplay have not been tested.

## Install for testing

Close Quasimorph. Open your Steam library's Workshop folder:

```text
C:\Program Files (x86)\Steam\steamapps\workshop\content\2059170\3754620214\
```

If Steam is installed elsewhere, use the same `steamapps\workshop\content\2059170\3754620214\` suffix in that library. This is the established Workshop installation for Imperium Reforged.

Extract the ZIP, open its `ImperiumOfMan` folder, and copy these **four files directly into `3754620214`**, replacing the existing copies:

- `ImperiumOfMan.dll`
- `imperiumofman.bundle`
- `modmanifest.json`
- `thumbnail.png`

Do not add a nested `ImperiumOfMan` directory inside the Workshop folder. The separate loose `package/ImperiumOfMan/` folder in this repository is the older baseline; use the complete ZIP.

## Included changes

Custom Magnum recipes retain their different ingredient types while each quantity is halved, rounded up with a minimum of one. Existing custom output batches are doubled. A service Lasgun order now uses one rolled plate, one armor fitting, and one weapon component to produce two Lasguns. Production time and the established recipe/unlock rules still apply.

The chemistry update adds five raw fractions: cellulose pulp, organic oil, chemical extract, reactive minerals, and carbon-rich feedstock. Biomass supplies the organic branches; ore supplies the mineral branches. These connect to acid, gasoline, paper, plastic, rubber, propellant, explosives, and ordinary mechanical and electrical components. New component batches generally produce four units; the new ammunition orders produce twelve, while finished medicine, repair, grenade, and cyborg orders produce two.

Biomass preparation and conductor separation are now shared foundations. New ration/workshop chips omit those already-known outputs, and new support chips omit shared gasoline production. Existing saved chip selections remain valid. Medicine catalogs retain native access, finished support goods keep their established tiers, and each custom chip still selects one recipe. Existing native Sorbent production is preserved; only a missing native recipe receives the new fallback.

This full package also contains the earlier ore/water extraction, glass production, Ore Cargo opening, chip-selection fix, Fury Sigil purchase restrictions, Imperial ration loot, loose-material recipe corrections, and Bound Ash footprint/stacking fixes. [Chemistry audit and scope](../diagnostics/RawMaterialChemistryAudit.md) · [Draft change notes](../docs/crafting/CHANGE_NOTES_1.1.3-1.1.4.txt)

## Focused in-game checks

These checks remain to be performed on the installed game:

1. Load an existing save and confirm shared biomass, conductor, and chemical recipes appear. Confirm the five new material icons display.
2. Complete a biomass order: one Piece of Flesh and one Water Bottle should produce eight biomass. Use biomass and water to make pulp and chemical extract, and biomass alone to make oil.
3. Complete the mineral branches, then produce acid and gasoline. The acid order should yield four units and the gasoline order twelve.
4. With an already learned recipe, confirm reduced inputs and doubled output on a representative custom equipment order. Check that a newly found chip still teaches one selected recipe.
5. Start and cancel one new ordinary order, then save/load a separate active order. Check its paid inputs, progress, and final output. Inspect the log for `[Cvar_ImperiumOfMan] Crafting coverage` and any registration or missing-resource errors.

The graph audit is diagnostic and does not establish recovery balance. The crafting browser and occult-production trees are future designs and are not game features in this ZIP.

## Build evidence

Source: `de43df89d6b5878422606fa69c43a4c544f2987e`. Native build: [run 45773773570](https://github.com/mrcalzon02/Dens-Mortis/actions/runs/45773773570). The build passed against the installed game with zero warnings and zero errors. All 30 transport chunks, the ZIP checksum and CRC, and all four runtime file hashes were verified. Non-executing DLL metadata inspection confirmed assembly version `1.1.4.0`, the new chemistry/coverage methods, five material constants, and all 15 reused artwork resources. [Verification receipt](../diagnostics/RawMaterialChemistryBuildReceipt.json)

ZIP size: **11,732,085 bytes**. SHA-256:

```text
820924a63ecceb21aa8008af45538d697507de993de1420d016072a2ccb6e637
```

Older ZIPs remain available for reference. This test build has not been published as a Steam update.

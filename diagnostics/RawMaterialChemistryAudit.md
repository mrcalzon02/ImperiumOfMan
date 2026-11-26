# Raw material chemistry and component audit

## Scope and source revisions

This audit compares the immediate pre-chemistry source at `a107c145f32ed9e6b567aa4380ae840a4e7d79d2` with the chemistry/component implementation at `de43df89d6b5878422606fa69c43a4c544f2987e`.

The reviewed implementation is in [RawMaterialChemistry](../src/Plugin.RawMaterialChemistry.cs), [ComponentFoundations](../src/Plugin.ComponentFoundations.cs), [Industry](../src/Plugin.Industry.cs), and [IndustryRecycling](../src/Plugin.IndustryRecycling.cs). The one-time runtime diagnostic is in [CraftingCoverage](../src/Plugin.CraftingCoverage.cs).

The results below describe inspected source recipes. Native game configuration was not loaded during the source audit. A route through ingredient IDs does not establish schematic ownership, research, tech-tier access, a usable workbench, quest completion, or economic balance.

## Recipe inventory

| Source graph | Before | After | Interpretation |
|---|---:|---:|---|
| Explicit Magnum production outputs | 308 | 365 | One explicit recipe registration per output; **57 new outputs** |
| Station production records | 303 | 303 | Multi-output records are counted once |
| Distinct station products | 379 | 379 | Some products have more than one station route |
| Declared native-conditional fallback recipes | 38 | 30 | These are separate from the explicit Magnum graph |
| Total extracted recipe records | 649 | 698 | Explicit Magnum, station, and conditional records combined |

The new chemistry file declares 15 explicit routes, including the existing biomass route moved into shared access. The component file declares 43 unconditional routes and one native-preserving Sorbent fallback. The net explicit increase is 57, because biomass already had a production recipe.

The exporter retains **113 unresolved call sites**. These are not 113 proven missing recipes: the list includes parameterized helper bodies and inputs that require native data. It includes 26 native implant disassembly cases and 42 support products whose native production/disassembly inputs cannot be determined from this source alone. No new unresolved recipe calls were introduced by the chemistry or component files.

## Material reachability

The raw starting set is `iom_metal_ore`, `water_bottle_1`, and `human_meat`. Ore and water have production-time extraction routes. Flesh remains an acquired organic starting material.

Each calculation adds an output when every required ingredient has a reachable source. It repeats until no new output becomes reachable. Counts ignore quantities, production time, and access requirements.

| Scenario | Reachable outputs | Blocked outputs |
|---|---:|---:|
| Before: explicit Magnum recipes from the raw starting set | 18 / 308 | 290 |
| After: explicit Magnum recipes, with native recipes omitted | 290 / 365 | 75 |
| After: preserved native Sorbent available | 306 / 365 | 59 |
| After: declared contextual sources available, Sorbent omitted | 349 / 365 | 16 |
| After: native Sorbent and declared contextual sources available | 365 / 365 | 0 |
| After: missing-native Sorbent fallback activated, raw starting set | 307 / 366 | 59 |
| After: Sorbent fallback and declared contextual sources available | 366 / 366 | 0 |

The fallback scenarios include Sorbent itself as an additional product. They do not assert that the fallback is active in a particular installed game.

All 30 declared conditional fallbacks, if hypothetically active together, produce a graph of 395 distinct outputs. Of these, 336 are reachable from the raw starting set; the remaining 59 retain contextual material dependencies. Native recipes take precedence over these fallbacks, so this is a diagnostic scenario rather than an installed-game result.

### Remaining explicit leaf inputs

The unconditional player graph has 16 ingredient IDs without an explicit player producer:

| Category | IDs |
|---|---|
| Raw organic starting source | `human_meat` |
| Preserved native production | `pills_sorbent` |
| Reward context | `iom_purity_seal` |
| Machine recovery | `iom_wreck_cyborg_battle`, `iom_wreck_cyborg_worker`, `iom_wreck_drone_security`, `iom_wreck_turret_ballistic` |
| Exotic/native acquisition context | `demonic_eye`, `mars_cyborg_hand`, `mars_gear`, `mars_skin`, `mercury_glass`, `quasi_basic_ammo`, `quasi_venus_rags`, `venus_armor_plates`, `venus_weapon_parts` |

There are **no unexpected ordinary material leaves** beyond the deliberately preserved native Sorbent branch. The 14 contextual IDs are assumed acquired only in the contextual scenario; the audit does not manufacture them, unlock their sources, or assert they all share the same acquisition rules.

Sorbent needs a specific qualification. The implementation preserves an existing native production recipe and grants shared recipe access. If the native recipe is absent, the mod supplies a carbon-feedstock-and-water fallback. The source audit cannot establish the ingredient list of an existing native recipe; the runtime diagnostic will inspect that actual branch.

## Registration, ingredient, and artwork checks

The bounded source checks passed:

- All 365 explicit Magnum output IDs are unique.
- No explicit player-manufacturing dependency cycles were found.
- The chemistry and component recipe calls resolve to known item IDs or the five newly registered materials.
- New material records are staged before recipes that consume them.
- Component recipes register before conditional support production, so known explicit routes take precedence.
- No packaged trade cargo is used as an explicit player recipe input.
- Fury Sigil, purity seals, recovered wrecks, and the designated contextual exotic inputs receive no new manufacturing producer.
- The three new medicine catalogs preserve other recipe sources through `restrictOtherSources: false`.
- The diagnostic call follows `FinishIndustryRegistration`.

The common recipe helper continues to copy input quantities, preserve ingredient types, apply ceiling-half with a minimum of one item, and double base output batches. Direct literal calls expose these values in the export. The audit does not infer quantities for expanded family helpers or native recipes.

The new native-item routes use `IndustryRecipe`, which replaces an existing native production definition for the same output. Sorbent deliberately uses conditional preservation. Only two pre-existing explicit mod production dependency lists were changed:

| Existing output | Previous input | Updated input |
|---|---|---|
| `iom_reclaimed_fiber` | `rags` | `iom_raw_cellulose_pulp` |
| `iom_polymer_stock` | `iom_polymer_scrap` | `iom_organic_oil` + `iom_chemical_extract` |

These changes provide forward routes from biomass while retaining the scrap/recovery systems. Other pre-existing explicit mod downstream recipe ingredient types remain intact.

The five new materials reuse existing embedded-artwork names:

| New material | Existing artwork key |
|---|---|
| Raw cellulose pulp | `iom_raw_vat_biomass` |
| Organic oil | `iom_polymer_stock` |
| Chemical extract | `iom_cell_stock` |
| Reactive mineral fraction | `iom_raw_silicate` |
| Carbon-rich mineral feedstock | `iom_conductive_scrap` |

All **15 corresponding icon, floor, and shadow PNG source files** were verified in the available full source tree. The verified 1.1.4 DLL also contains all 15 corresponding embedded resource names. This confirms packaging; their in-game appearance still needs a visual check.

## Recovery loops and balance caveat

The Magnum-only manufacturing graph is acyclic. The combined Magnum-and-station graph has three strongly connected groups:

| Group | Relationship |
|---|---|
| Electrical/engineering/robot/tool cargo, rods, and wire | Existing station production/recovery cycle |
| `iom_refined_metal` and `rusty_plates` | New loose-plate production meets existing station scrap sorting |
| `iom_reclaimed_fiber` and `rags` | New rag production meets existing station fiber recovery |

The metal/plate and fiber/rag cycles remain when the two previously identified station recovery records `iom_cryptoria_robot_salvage` and `iom_ares_post_wiring_recovery` are excluded. They are not player-manufacturing bootstrap locks: forward raw-material routes exist. Their economic effect depends on quantities, time, station access, and recovery behavior, which this dependency audit does not settle.

Generic recycling is excluded from the manufacturing graph and can add further return paths. Consequently, an acyclic player graph and complete ordinary ingredient reachability are not evidence that every fabrication/recovery loop is economically balanced. Recovery yields remain a separate balancing review.

## One-time runtime coverage diagnostic

`LogCraftingCoverage()` snapshots the actual registered Magnum and workbench recipe lists once, after registration. It reports Magnum-only and combined Magnum/workbench material routes separately, retaining alternative recipes. Each scope reports both raw starting materials and the declared contextual sources, plus a separate count for `IndustryBatches` outputs.

The diagnostic uses a fixed-point calculation and bounded ID lists. It does not scan every frame, enumerate all item records, generate recipes, alter unlocks, or fail startup because coverage is incomplete. Exceptions inside the diagnostic are logged and contained; primary registration errors remain outside its catch block.

The actual diagnostic source compiled against API-shaped scratch stubs. Focused checks covered alternative routes, unseeded cycles, free recipes, zero-cost entries, contextual roots, the separate workbench scope, custom-output counts, the one-time guard, and a contained invalid-recipe exception. An independent read-only code review found no requested changes.

The scratch verification did **not** execute the game or validate the current native ABI. The subsequent native build compiled against the installed game successfully. Compilation does not establish that the diagnostic has run during a game boot or that its live coverage results match the source snapshot.

## Native build and boot status

The first chemistry build, [run 45770493091](https://github.com/mrcalzon02/Dens-Mortis/actions/runs/45770493091), identified five C# `CS8752` errors in single-input recipe declarations. The compiler treated target-typed `new(...)` as an array target for a `params` argument. Source commit `de43df89d6b5878422606fa69c43a4c544f2987e` makes those five constructors explicit as `new ItemQuantity(...)`. The exported recipe records and reachability results are unchanged by that correction.

- Chemistry source commit: `de43df89d6b5878422606fa69c43a4c544f2987e`.
- Native build request: `raw-material-chemistry-002`.
- Controller commit: `886e2e8b4ef644baac72b5c1811b2f413167263c` in `mrcalzon02/Dens-Mortis`.
- [Native build 45773773570](https://github.com/mrcalzon02/Dens-Mortis/actions/runs/45773773570), job `139752399222`: native compile/API-inspection step **passed** against the installed game.
- Compilation: **zero warnings and zero errors**.
- Delivered package verification: **passed** for all 30 transport chunks, archive SHA-256/Git blob SHA, ZIP CRC, and all four runtime file hashes.
- DLL inspection: assembly `1.1.4.0`, ten chemistry/component/coverage methods, all five new material constants, and all 15 borrowed artwork resources verified without executing the assembly.
- Package: [ImperiumOfMan-1.1.4-ChemistryFoundations.zip](../package/ImperiumOfMan-1.1.4-ChemistryFoundations.zip), 11,732,085 bytes; [build receipt and hashes](RawMaterialChemistryBuildReceipt.json).
- Private delivery commit: `bf4fe578b5a3aaf94bfd06d3448276bfa3ee77dc`; public package Git blob: `4513ea73e0ae7f16dd710bc1533c3ee2f6cfb824`.
- Game boot and actual recipe coverage log: **not run**.
- In-game crafting, schematic availability, recovery balance, and gameplay: **not tested**.

## Reproducing the bounded source export

The reusable exporter is [tools/crafting/export_source_graph.py](../tools/crafting/export_source_graph.py). It resolves its default repository root relative to its own location; `--root` can select a different checkout.

```sh
python tools/crafting/export_source_graph.py --ref de43df89d6b5878422606fa69c43a4c544f2987e --out chemistry-source-graph.json
python tools/crafting/export_source_graph.py --ref a107c145f32ed9e6b567aa4380ae840a4e7d79d2 --out pre-chemistry-source-graph.json
```

Omit `--ref` to inspect the working tree. `source_commit` identifies the selected commit, while `source_ref: working-tree` makes local-file extraction explicit. `source_hashes` are SHA-256 hashes of normalized, comment-stripped source text with line positions preserved; they are not raw-file hashes.

The export keeps explicit Magnum records, station records, conditional fallbacks, material titles/artwork keys, and unresolved calls. It expands only inspected helper patterns. New or changed source helpers require review before a visualization can describe their routes as covered. Generated-family and native quantities remain unknown, and schematic/workbench access is outside the export. The resulting visual map should therefore identify its source scope and native-data gaps rather than claim whole-game coverage.

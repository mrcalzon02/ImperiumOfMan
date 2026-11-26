# Organic recycling and occult production

**Current source — 1.1.10:** `5fae43b49ff9fe4ee21cb88a3e8e30eb8e3558ad` preserves the six capabilities and removes finished-ration recycling. Native compilation passed with zero warnings/errors; the package, compiled correction and all 24 embedded PNGs are verified in the [current receipt](FinishedRationBuildReceipt.json). No native game was launched for 1.1.10. [Correction scope](FinishedRationOutputCorrection.md).

**Historical 1.1.9 build:** source `347de7db77f9121ad26e1c12c534b97760ee92f2`, assembly **1.1.9.0**, passed native build [45838490203](https://github.com/mrcalzon02/Dens-Mortis/actions/runs/45838490203) with **zero warnings and zero errors**. The [source-347 receipt](OccultBuildReceipt.json) verifies the historical 1.1.9 candidate, four runtime files and all 24 embedded occult PNGs. Exact-source session 005 loaded the disposable campaign and recorded 21 pattern pools, 53 occult outputs, the seven intended spoiled-organic records and Ron Mask pact preservation.

**Historical source-347 runtime:** session 005 verified the map controls and custom-order queue completion. Session 006 passed Space/save and **46 hours 20 minutes of game time**, with zero missing-preset diagnostics or managed-exception headings in its complete 15,897-byte log. Source-347 captures verify the six-node tree and both corrected tooltip hovers. All three sessions restored the original package, preserved original saves and cleaned their fixtures. [Build/runtime receipt](OccultBuildReceipt.json) · [Source-347 native UI evidence](../docs/crafting/native-occult-ui-347de7d.json).

**Remaining transaction scope:** no research purchase or physical-chip learning was performed in the visual checks. Exact recycling, sample/cargo consumption, refunds, legacy migration and purchased/item sprite states still require their targeted cases. The separate raw-ore measurement gap was closed by [session 007](CraftingMapAcceptance-347de7d-007.json) on historical source `347de7d` / 1.1.9: one four-output batch increased the named Metal smelting feedstock stack from **2/30 to 6/30**. This is a named-stack measurement, not a whole-inventory aggregate or a 1.1.10 gameplay test.

## Four-step production chain

The figures below are final batch quantities and baseline ship-hours. They do not pass through the ordinary recipe helper's half-input/double-output adjustment. Native production speed and difficulty modifiers still apply.

| Process | Consumed cargo | Batch output | Baseline time | Capability |
| --- | --- | --- | --- | --- |
| Clone gestation | None | 2 Raw Clones | 8 hours | I Culture |
| Torment containment | 2 Raw Clones | 4 Torment Imprints | 4 hours | I Culture |
| Suffering separation | 4 Torment Imprints | 4 Essence of Suffering | 4 hours | II Distillation |
| Gavvakh condensation | 4 Essence of Suffering | 2 native Gavvakh (`quasi_medical_kit_1`) | 4 hours | II Distillation |

Each step also requires its own learned recipe. The Culture and Distillation physical chips each select one pattern from their respective two-candidate pools; purchasing research never supplies knowledge. The whole occult addition declares 21 chip pools and 53 candidate formulas, with existing/missing native routes skipped before pool construction. A pre-existing native Gavvakh formula is preserved, so the displayed condensation formula is conditional on no conflict.

The root produces two Gavvakh in twenty serial baseline hours when all four formulas are registered and learned. Clone gestation consumes a normal production order and time despite having no cargo input.

## Harvest and recycling

A selected ship-recycling harvest consumes one Raw Clone and yields 4 native `human_meat`, 2 `human_skin`, and 1 `human_rib`, provided those installed records exist. The same clone cannot also enter torment containment. The process uses inventory tokens; it spawns no actor and grants no kill, mission or faction reward. Harvest is a separate selected route, not a free coproduct of torment.

The manufacturing source graph and current `CraftingCoverage` report do not include `ShredProfiles`. Their blocked flags for `bone_knife`, `impaler` and `skrivnus_knife` omit this rib-producing harvest route and must not be read as proof that `human_rib` or those recipes are unobtainable. The harvest transaction itself still needs its targeted in-game check.

Native spoiled-organic targets are checked against actual `ItemExpireRecord.ConvertedItemId` relationships and loaded destination records. The destination must have an explicit spoilage marker (`rotten`, `rotting`, `spoiled`, `decayed`, or `putrid`) and a Food, Drink, Organ, or QuasiOrgan class; a Parts record also needs a flesh marker such as `meat` or `skin`. The same destination rule supports rotten records without an expiring donor. An expiry link alone does not qualify clean water or mechanical salvage as biomass. Quest and medical classes are excluded. One recognized spoiled organic item yields 2 Raw Vat Biomass through selected ship recycling. Locked and quest items covered by custom recycling profiles are removed from the native processing pass and returned unchanged, including any loaded ammunition. Existing antibiotics and fresh-flesh fabrication remain intact; native recipes and item behavior are not replaced.

### Finished Imperial rations remain outputs

**1 Raw Vat Biomass + 1 wrapper → 6 Dirty Corpse Starch in 2 base ship-hours.** This forward production recipe is unchanged. Finished Corpse Starch and Dirty Corpse Starch have empty disassembly lists and no custom recycling profiles. The removed reverse route has no entry in the current live recovery index and no dedicated recycler-button or item-tooltip behavior. [1.1.10 correction](FinishedRationOutputCorrection.md).

The retained clone/spoiled-organic recovery hooks were inspected against the installed `MagnumCargoSystem` API from run `45802511629`; historical compilation and visual results remain attributed to their original sources. The 1.1.10 build has its separate [verification receipt](FinishedRationBuildReceipt.json).

## Runtime checks still required

Earlier native compile run `45804161967` identified an incorrect generic lookup for expiry metadata. Source `585715f` uses the installed game's own `Data.ItemExpire.GetRecord(id)` accessor, verified in the current `ItemFactory` export, and native compile run `45813237686` passed after that correction. Its `ConvertedItemId` still drives the recycling target; item records and expiry records remain separate.

Headed session [45815345319](https://github.com/mrcalzon02/Dens-Mortis/actions/runs/45815345319), immutable log delivery `4b681f8caf70730ad38c83b25fa8358f480fd6a4`, exposed twelve accepted destinations: the seven valid rotten records below plus unintended `water_bottle_1`, `spring`, `weapon_parts`, `metal_tread`, and `wire`. The cause was treating every expiry conversion into the broad Parts/Drink classes as organic spoilage. The source correction requires spoilage evidence on the destination itself and flesh identity for Parts.

Both historical `bdb1759` and exact-source 347 session 005 native registration retain exactly `rotten_soup_can_1`, `rotten_human_meat`, `rotten_dog_meat`, `rotten_spider_meat`, `rotten_burger_pack_1`, `rotten_burger_pack_2`, and `rotten_human_skin`, while rejecting all five unintended destinations. The headed registration evidence confirms the destination predicate; actual recycling transactions remain a separate check.

The earlier campaign load also encountered a native project-cloning failure for the supplemental pact record on `ron_mask_1`. The current candidate retains the [scoped Pact-component compatibility change](OccultProjectCompatibility.md). Historical exact-build headed session [45824345407](https://github.com/mrcalzon02/Dens-Mortis/actions/runs/45824345407), immutable delivery `8978febd64c670f2a22a8ccaff36054545c047a7`, confirms preserved `ron_mask_1_custom` pact data, successful disposable slot 2 loading into Dungeon and completed autosave, with no recurrence of that exception. It also reports 53 occult recipes and 21 pattern pools.

Immutable cleanup delivery `bd04d4a192cefee80a2acaf7ed3ee3f58d857729` records operator stop at sequence 21, restored original package, unchanged original campaign files and completed disposable-fixture cleanup, with no retained or unexpected campaign files.

Verify each physical chip learns only one recipe, research teaches none, and both knowledge and capability are enforced when starting an order. Check no-input orders, normal queue limits, exact ingredient consumption, native cancellation refunds, decay, further save/load behavior, sprite fit and exclusive clone harvest. The observed registration, campaign-load and cleanup results are confirmed; these remaining interactions have not received a complete gameplay pass.

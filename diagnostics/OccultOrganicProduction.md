# Organic recycling and occult production

**Build status:** source `585715f554181ed56220c5ba2ed9055b46b44b38`, assembly **1.1.9.0**, passed native build [45813237686](https://github.com/mrcalzon02/Dens-Mortis/actions/runs/45813237686) with **zero warnings and zero errors**. Its headed boot subsequently exposed an overbroad spoiled-organic intake rule; the source correction below still requires a new native build and boot before that archive can be replaced.

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

Native spoiled-organic targets are checked against actual `ItemExpireRecord.ConvertedItemId` relationships and loaded destination records. The destination must have an explicit spoilage marker (`rotten`, `rotting`, `spoiled`, `decayed`, or `putrid`) and a Food, Drink, Organ, or QuasiOrgan class; a Parts record also needs a flesh marker such as `meat` or `skin`. The same destination rule supports rotten records without an expiring donor. An expiry link alone does not qualify clean water or mechanical salvage as biomass. Quest and medical classes are excluded. One recognized spoiled organic item yields 2 Raw Vat Biomass through selected ship recycling. Locked and quest items covered by custom recycling profiles are removed from the native processing pass and returned unchanged, including any loaded ammunition. Existing antibiotics and fresh-flesh fabrication remain intact; native recipes and item behavior are not replaced.

Dirty Corpse Starch now has a ship-recycling bulk route: **8 eligible rations yield 1 Raw Vat Biomass**. Selected stacks count together; only complete batches are consumed. Seven rations remain untouched, eight yield one biomass, and nine yield one biomass with one original ration retained. Locked and quest items do not contribute. The rate stays below recovery of the existing six-ration fabrication batch, so the route does not multiply biomass.

The native ship cargo screen already permits nonquest rations in its recycling tab. The scoped button and start guards recognize complete bulk batches while preserving ordinary mixed-item recycling. A partial bulk-only selection does not start a useless cycle. The existing native recycler department and its duration remain required. Native item tooltips show the exact batch rate and current selection's returns/remainder; the Crafting Map uses eight input units.

The completion prefix temporarily removes bulk-treated items from the native per-item pass, consumes the required quantity, and returns original leftover objects after native completion. Native `AddCargo(..., splittedItem: true)` avoids merging those remnants into a different stack. No fake per-item `Disassembly` output, newly created replacement ration, or cellulose-pulp conversion is used. Ordinary field-disassembly behavior remains unchanged.

These hooks were checked against the current installed `MagnumCargoSystem` and `ScreenWithShipCargo` API export from native run `45802511629` (game assembly SHA-256 `d72cd91f98de4d6cce8ee420ac1a906d89e68ba0cb164694d266b56f46339ebe`). Bounded arithmetic checks covered split stacks, incomplete batches and protected items. The bulk implementation passed native compilation in run `45813237686`; the later destination-filter correction still needs its own native build and boot.

## Runtime checks still required

Earlier native compile run `45804161967` identified an incorrect generic lookup for expiry metadata. Source `585715f` uses the installed game's own `Data.ItemExpire.GetRecord(id)` accessor, verified in the current `ItemFactory` export, and native compile run `45813237686` passed after that correction. Its `ConvertedItemId` still drives the recycling target; item records and expiry records remain separate.

Headed session [45815345319](https://github.com/mrcalzon02/Dens-Mortis/actions/runs/45815345319), immutable log delivery `4b681f8caf70730ad38c83b25fa8358f480fd6a4`, exposed twelve accepted destinations: the seven valid rotten records below plus unintended `water_bottle_1`, `spring`, `weapon_parts`, `metal_tread`, and `wire`. The cause was treating every expiry conversion into the broad Parts/Drink classes as organic spoilage. The source correction requires spoilage evidence on the destination itself and flesh identity for Parts.

The next native boot must retain `rotten_soup_can_1`, `rotten_human_meat`, `rotten_dog_meat`, `rotten_spider_meat`, `rotten_burger_pack_1`, `rotten_burger_pack_2`, and `rotten_human_skin`, while rejecting all five unintended destinations. The local twelve-ID predicate regression is a source check, not that runtime receipt.

Verify each physical chip learns only one recipe, research teaches none, and both knowledge and capability are enforced when starting an order. Check no-input orders, normal queue limits, exact ingredient consumption, native cancellation refunds, decay, save/load, sprite fit, and exclusive clone harvest. Native compilation, source counts and static checks do not prove boot or gameplay success.

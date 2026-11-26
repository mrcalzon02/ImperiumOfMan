# Organic recycling and occult production

**Source status:** continuation from `40939a3`. Native build and headed gameplay validation for this revision are still pending.

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

Native spoiled-organic targets are derived from actual `ItemExpireRecord.ConvertedItemId` relationships and bounded food/organic record classes. Explicit named rotten flesh is a fallback for records without an expiring donor. Quest items and medical classes are excluded from that fallback. One recognized spoiled organic item yields 2 Raw Vat Biomass through selected ship recycling. Locked and quest items covered by custom recycling profiles are removed from the native processing pass and returned unchanged, including any loaded ammunition. Existing antibiotics and fresh-flesh fabrication remain intact; native recipes and item behavior are not replaced.

Dirty Corpse Starch now has a ship-recycling bulk route: **8 eligible rations yield 1 Raw Vat Biomass**. Selected stacks count together; only complete batches are consumed. Seven rations remain untouched, eight yield one biomass, and nine yield one biomass with one original ration retained. Locked and quest items do not contribute. The rate stays below recovery of the existing six-ration fabrication batch, so the route does not multiply biomass.

The native ship cargo screen already permits nonquest rations in its recycling tab. The scoped button and start guards recognize complete bulk batches while preserving ordinary mixed-item recycling. A partial bulk-only selection does not start a useless cycle. The existing native recycler department and its duration remain required. Native item tooltips show the exact batch rate and current selection's returns/remainder; the Crafting Map uses eight input units.

The completion prefix temporarily removes bulk-treated items from the native per-item pass, consumes the required quantity, and returns original leftover objects after native completion. Native `AddCargo(..., splittedItem: true)` avoids merging those remnants into a different stack. No fake per-item `Disassembly` output, newly created replacement ration, or cellulose-pulp conversion is used. Ordinary field-disassembly behavior remains unchanged.

These hooks were checked against the current installed `MagnumCargoSystem` and `ScreenWithShipCargo` API export from native run `45802511629` (game assembly SHA-256 `d72cd91f98de4d6cce8ee420ac1a906d89e68ba0cb164694d266b56f46339ebe`). Bounded arithmetic checks covered split stacks, incomplete batches and protected items. The final candidate still requires native compile and headed gameplay validation.

## Runtime checks still required

Inspect the actual accepted spoilage IDs and skipped formulas in the installed game. Verify each physical chip learns only one recipe, research teaches none, and both knowledge and capability are enforced when starting an order. Check no-input orders, normal queue limits, exact ingredient consumption, native cancellation refunds, decay, save/load, sprite fit, and exclusive clone harvest. Current source counts and static checks do not prove boot or gameplay success.

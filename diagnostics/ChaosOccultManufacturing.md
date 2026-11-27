# Undivided ammunition and Soulforge integration

## Dedicated production

All entries below are effective base batch quantities, before difficulty/time and material yield modifiers. New intermediates use the direct occult registrar. Existing Chaos finished ammunition and Soulforge Ember retain the legacy IndustryRecipe convention (half input counts, rounded up; double batch output).

| Rank | Recipe | Inputs per order | Output | Base hours |
|---|---|---|---:|---:|
| III | Shard Matrix | 1 Shard Glass + 1 Bound Ash | 3 | 4 |
| III | Infused Shard Stock | 2 Shard Matrix + 1 Gavvakh | 4 | 4 |
| III | Shard Cells | 4 Infused Shard Stock | 16 | 3 |
| IV | Soulforge Matrix | 1 Bound Ash + 1 Duggur Matrix | 2 | 6 |
| V | Soulforge Ember (existing) | 1 Soulforge Matrix + 1 Insulated Conduit + 1 circuit board | 2 | 12 |
| V | Bound Conductor | 1 Insulated Conduit + 1 Soulforge Ember | 3 | 5 |
| V | Charged Coil Stock | 2 Bound Conductor + 1 Resonant Cell Stock | 4 | 5 |
| V | Excruciator Coil Cells | 4 Charged Coil Stock | 16 | 5 |
| V | Soulforge Armature | 1 Soulforge Ember + 1 Cattaram Crystal Core + 1 Duggur Matrix | 2 | 8 |

If a regional intermediate is not registered, existing native regional stock is used as a compatible input. If a whole new chain cannot be registered, keep the older owned formula for its finished output. New stock is staged after its legacy prerequisite recipes. Tier 7–10 cuirasses and tier 7–9 weapons consume Soulforge Armature in place of the ordinary heavy plate input when the stock is available. Fury Sigil stays faction purchase-only; Shadowblade stays finale-only. Neither finished ammunition output is eligible for the material yield bonus. Coil fabrication is routed to occult production when its dedicated chain is active.

New stock and targeted legacy output singleton chips are additional Chaos-owned contract and station routes. Existing mixed tier chips, original ammo records, weapon compatibility and learned recipe IDs remain unchanged. Existing native drop and ordinary battery ammunition trade remain intact. New stock uses donor descriptors provisionally.

Source integration only: native compilation, transaction testing, faction progression, balance and original art remain unverified. No release.

# Organic recycling and occult starter production

Source baseline: ef6f83b8ccce6afae57f21f9d5f0f01e0b1ee42e. This change is source and embedded artwork, not a compiled Workshop package.

## Player-facing routes
- Ship Recycling: one recognized spoiled organic item (including native rotten meat and skin where present) yields two Raw Vat Biomass. Existing antibiotics and fresh-flesh fabrication stay intact. Native spoilage variants are discovered from the loaded registry and logged; no invented vanilla IDs are registered.
- Clone Vat: no cargo inputs, 8 ship-hours, 2 Raw Clones.
- Containment Cradle: 2 Raw Clones, 4 hours, 4 Torment Imprints.
- Essence Still: 4 Torment Imprints, 4 hours, 4 Essence of Suffering.
- Gavvakh Retort: 4 Essence of Suffering, 4 hours, 2 native Gavvakh, only if no existing recipe conflicts.
- Alternative clone harvest in Ship Recycling: consume one Raw Clone, recover 4 native meat, 2 skin and 1 rib. This is mutually exclusive with torment production; no spawned actors or kill rewards.

The occult module is a separate tree anchored near engineering. Purchase gates are checked when an order starts and learned recipes refresh on load and opening Production. All new item icons have embedded inventory, floor and shadow PNGs; the four upgrade icons are 36px. See [sprite sheet](../docs/crafting/occult-starter-sprites.png).

## Verification limits
These are deliberately initial quantities and construction prices, not balance-tested. The current native config must be checked for actual spoiled item IDs, and the build/launch must confirm purchase gates, zero-input orders, cargo consumption, refund behavior, decay, visual fit, and the existing recipe picker. A conflicting native Gavvakh recipe is preserved and reported, not overwritten. This implementation does not yet add all otherworldly equipment families, nor an automatic meat coproduct from the torment order. The harvest is a separate recycling route. No change to other panels or existing published package.

# Baseline clone-vat component patterns

Source intent: organic components should have a repeatable production route from raw vat biomass alone, without making the player dependent on corpse loot or on supernatural faction alliances.

All new orders require Occult Research I (Culture), a learned physical singleton chip, a free occult production queue and the specified biomass. No recipe is granted by buying research. Chips are Imperial faction technology level 1 contract rewards and paid station stock, with Neith Vault as preferred supplier and the existing same-owner fallback. Do not fall back to pirates if the issuer is unavailable. Native receipts, item records, decay, equipment behavior and existing raw-clone harvesting remain unchanged. If an installed record is missing/protected or an existing Magnum producer owns its output, skip the new formula and its chip rather than overriding it.

| Component | Native item ID | Biomass | Output per order | Base hours |
|---|---|---:|---:|---:|
| Human Flesh | `human_meat` | 9 | 1 | 4 |
| Human Skin | `human_skin` | 3 | 1 | 3 |
| Human Arm | `human_hand` | 5 | 1 | 5 |
| Human Leg | `human_leg` | 6 | 1 | 5 |
| Human Skull | `human_skull` | 5 | 1 | 5 |
| Human Rib | `human_rib` | 3 | 1 | 3 |
| Dog Meat | `dog_meat` | 3 | 1 | 3 |
| Spider Flesh | `spider_meat` | 3 | 1 | 3 |
| Spider Claw | `spider_claw` | 4 | 1 | 4 |
| Spider Joint | `spider_joint` | 4 | 1 | 4 |

Balance: the ordinary recipe `SharedRecipe(Biomass, 12, 4, human_meat × 1, water × 1)` is transformed by IndustryRecipe into 8 biomass from one meat plus one water. A cultured meat costs 9 biomass, so converting it back loses one biomass plus water. Current spoiled-organic recovery yields 2 biomass per eligible spoiled item, less than every new vat cost. Do not include these outputs in the occult yield bonus set; fixed one component per order. Speed/queue upgrades retain their normal effect. Other native or third-party recovery routes and market prices still require installed-game verification.

The existing organic salvage map band and pattern-source details identify each component, including native equipment-class limbs and claws. Research I tooltips indicate biomass-only component patterns. No new artwork: use native item icons and the existing occult pattern-chip art.

Verification: native compile and actual registration count, one chip learning one recipe, rank gate, paid stock, biomass-only consumption, output, cancellation, save/load, decay and absence of positive-return cycles. No release.

# Native organic ingredients in occult fabrication

Scope: existing game inventory records; no new item, artwork, disassembly, corpse handling, drop, research, or knowledge grant behavior. Existing recipe IDs, capability ranks, outputs, base batch/time and chip pools are preserved. Native or third-party recipe ownership remains protected by RegisterOccultRecipe. Already queued production orders are not rewritten.

The following single ingredient replacements apply only when the installed native ItemRecord exists and is not a quest item; otherwise retain the previous formula ingredient. These are registration-time compatibility fallbacks, not alternative recipes when player inventory is short.

| Existing fabrication output | Native ingredient | Previous ingredient |
|---|---|---|
| `demonic_eye` | `human_skull` | `GlassStock` |
| `quasi_venus_rags` | `human_skin` | `Biomass` |
| `venus_weapon_parts` | `human_hand` | `weapon_parts` |
| `venus_melee_parts` | `spider_claw` | `rod_parts` |
| `venus_guts` | `dog_meat` | `Biomass` |
| `mercury_scales` | `spider_joint` | `Plate` |
| `mercury_guts` | `spider_meat` | `Biomass` |
| `moon_guts` | `human_meat` | `Biomass` |
| `moon_armor_plates` | `human_leg` | `Plate` |
| `moon_weapon_parts` | `human_rib` | `weapon_parts` |
| `ron_tentacle` | `spider_meat` | `Biomass` |
| `mars_skin` | `human_skin` | `Biomass` |
| `jupiter_guts` | `dog_meat` | `Biomass` |

Native reference IDs: [Arm](https://quasimorph.wiki.gg/wiki/Arm) uses human_hand (formerly Hand); [Leg](https://quasimorph.wiki.gg/wiki/Leg), [Skull](https://quasimorph.wiki.gg/wiki/Skull), [Spider Flesh](https://quasimorph.wiki.gg/wiki/Spider_Flesh), [Spider Claw](https://quasimorph.wiki.gg/wiki/Spider_Claw), [Spider Joint](https://quasimorph.wiki.gg/wiki/Spider_Joint), [Dog Meat](https://quasimorph.wiki.gg/wiki/Dog_Meat). Installed data remains authoritative. Human meat, skin and rib are already used by the existing clone salvage route.

The crafting map groups these specific inventory records in an Organic salvage band, not the conventional weapons band. Shared nodes remain shared; there are no invented animal limb records. This batch deliberately retains the existing clone harvest, ordinary food/recycling, native equipment attributes, and the Gannix/Duggur prototype batch ledger.

Validation pending: compile against the installed game; confirm actual ingredient records, existing learned chip identity, exact displayed recipe counts, inventory consumption and cancellation refund for each item class (especially arm, leg and claw), and native decay for perishable inputs. Provisional substitution balance requires playtesting. No new release.

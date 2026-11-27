# Regional occult ammunition prototype

Baseline: e0379c107ab1a0105617442d5ad3d7e6a2f61cfc. Scope: Gannix and Duggur only. Implements the first prototype stage from [Occult faction economy design](OccultFactionEconomyDesign.md).

## Recipes (effective per-order batch, before facility yield/time upgrades)

| Rank | Inputs | Output | Hours |
|---|---|---|---:|
| III | 1 Gavvakh + 1 Venusian gut stock | 3 Combat Gavvakh Concentrate | 4 |
| III | 2 Combat Gavvakh Concentrate + 1 Venusian feather | 4 Hardened Living Cells | 4 |
| III | 4 Hardened Living Cells | 12 Igvas | 3 |
| IV | 2 Lunar guts + 1 human rib + 1 Essence of Suffering | 3 Calcified Lunar Matrix | 4 |
| IV | 2 Calcified Lunar Matrix + 1 Gavvakh | 4 Pain-bound Nail Blanks | 4 |
| IV | 4 Pain-bound Nail Blanks | 12 Nails of Pain | 3 |

These are initial balance proposals. Existing finished ammunition records, drop identities and learned recipe keys are preserved. The existing owned occult recipe is replaced in source, not overwritten at runtime; native/other-mod owned recipes are preserved, and a missing prototype dependency retains the older supported formula where available. Existing scheduled orders are not rewritten. New materials use existing donor artwork provisionally, pending distinct inventory/floor/shadow art. No recipe grants occur through research.

## Knowledge and rewards

- Each active chain adds three targeted singleton chips (two intermediates and the finished ammo), plus singleton precursor patterns for owned tissue/feather formulas and faction-local foundation recipes. With all original formulas available, this is 17 additional targeted chips. The actual installed-game count can differ because existing native recipes are preserved.
- Gannix: Tezctlan rewardChips and eligible Tezctlan-owned suppliers. Duggur: Chaos Undivided rewardChips and the Ossuary Works or another eligible Chaos-owned supplier. Native faction contracts use their existing reward machinery; no unconditional reward or custom completion grants are added.
- Add missing native Venusian tissue/feather to Tezctlan rewardConsumables at tech 2 and lunar tissue to Chaos rewardConsumables at tech 3, with weight 2. If that item is already registered at or before the proposed tech tier, do not duplicate it. Enemy loot and native trade remain unchanged.
- New prototype chips do not fall back to Unchained Belt when their maker is inactive. Existing catalogs retain their prior fallback behavior until the full supplier and recovery-path audit is complete; this is deliberately narrower than the design's eventual blanket restriction, to avoid stranding existing recipes.
- Startup audit logs the actual registered chip, pattern, faction, reward table presence, recipe requirements, output batch and time. This is evidence of registration only. Native general-reward behavior and actual successful payout require separate verification.

## Source verification

Seven focused source checks passed: original output IDs retained; conflict checks before registration; two intermediate stages then finished ammo; singleton pattern registration; faction reward/merchant scope; no conventional ammunition input; staged ingredient/positive-count guards. These are not native gameplay tests. At baseline yield a complete three-order chain consumes 11 game hours (excluding reagent/tissue fabrication), with surplus first-stage stock; material yield +1/+2 increases intermediate output, not finished ammunition. Repeated batches should be reviewed for price arbitrage and throughput under real difficulty settings.

## Next native checks

1. Compile exact integrated source on the installed game; inspect native faction reward selection and reward table key semantics.
2. Run headed disposable-slot campaign. Verify both active chains and each singleton reward record in the startup log; acquire chips through actual native contracts or trade; verify one-pattern unlock and no automatic grants.
3. Queue three dependent orders, observe material counts and timing, and use the output with a compatible native weapon. Check old learned recipes, cancellation, saves, and research prerequisite availability. Confirm visual identity of all four materials; distinct original artwork is still required.
4. If either native recipe conflict is detected, document its ownership and leave it untouched. Do not expand to Angerlings, Elerium, Eannis or Flauros until the two prototypes work.

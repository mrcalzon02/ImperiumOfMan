# Occult faction knowledge and parallel resource economy — design proposal

**Status:** Planning only. No recipe, item, reward, weapon, research, save-schema or runtime implementation changes are authorized by this document. Baseline reviewed: `3d0eb57d300350686b3917ab182438c6c5fe93e8`. Proposed names, quantities, tech bands and prices require review and installed-game verification before implementation.

## 1. Audit of current implementation

- `src/Plugin.OccultKnowledge.cs` declares 21 physical pattern pools and uses the shared catalog helper. The historical native build recorded 53 added occult formulas, but that is not a fresh live count. Existing native or other-mod recipes are skipped where the occult registrar detects ownership conflicts.
- `RegisterCatalog` in `Plugin.Industry.cs` already calls `AddFactionReward(faction, "rewardChips", disk.TechLevel, 4, disk.Price, disk.Id)`. This is a faction-specific reward-table registration, not proof of how often the installed game's mission payout selects it. Physical datadisks choose one output pattern from the pool.
- `RefreshIndustryMarkets` in `Plugin.IndustrySupply.cs` restocks one chip at an owned eligible supplier; when the maker is absent or not tradable, the generic catalog fallback is Unchained Belt. Occult faction-specific catalogs must not silently become universal pirate stock just because a faction is inactive or defeated. Design a separately specified rare recovered-pattern route instead.
- Current supplier assignments include Tezctlan/FeatheredTemple, XiomaraMasks/FeatheredTemple, SheduThousand/RedTiangong and ChurchGannix/Gloria. Public canon identifies Feathered Masks' headquarters as Louhi-City and Shedu's Thousand's as Sinkhole Oasis. Verify the exact installed station IDs, ownership, faction activation state and reward-table keys rather than guessing IDs from display names. Correct preferred suppliers only after verifying runtime ownership; preserve a valid faction-owned fallback. ChurchGannix is not presumed a fourth native contract-capable legion.
- The current occult ammunition recipes depend on ordinary inputs: Igvas on chemical extract, Angerlings and Nails of Pain on rod parts, Elerium on ordinary cell stock, Eannis Redfire on organic oil, and Flauros ammo on ordinary gas ammo. Chaos Shard Cells consume Igvas and Coil Cells consume battery ammunition. Some are acceptable hybrid routes, but they do not constitute a distinctive parallel production economy.
- Current research has six retained capability ranks plus ten facility nodes. Dedicated occult line IDs 10000–10002, learned-pattern checks, queue capacity, and speed/yield logic have been implemented in source; native compilation/headed validation is separate and must be verified before building more on top. Existing patterns, research IDs, item IDs and queued orders must remain valid.

## 2. Three independent progression gates

1. **Capability**: six research ranks authorize fabrication families; optional facility upgrades improve lines, queues, yield and speed. Research never grants recipe knowledge.
2. **Knowledge**: physical, faction-themed pattern chips, earned from missions, bought at an eligible owned station or recovered as loot where appropriate. One chip carries one learned pattern, not a whole branch.
3. **Supply**: base occult reagents, regional seed/catalyst stock and processed materials. Recipes need inputs and an available occult line.

Do not enforce ongoing faction allegiance as a condition of using already learned knowledge. Defection changes future access to the faction's recipes and stocks; it does not erase learned recipes or purchased research.

### Reward acquisition policy (proposed)

- Preserve the native mission beneficiary, reputation, faction technology, reward budget, and extraction/completion conditions. Add the appropriate chip to that beneficiary's verified rewardChips pool; never give the enemy faction's reward simply because its station was attacked.
- Early pools contain core culture/processing patterns and ordinary faction intermediates; the next tier contains ammunition and specialized components; the advanced tier contains equipment and grafts.
- Use one to three candidates per chip family where practical; first prerequisite patterns may be singleton chips so the supply chain can start. Existing previously distributed chips keep their IDs and learned meanings. Existing large pools may remain valid legacy pools; add more focused new chips rather than invalidating them.
- Start with native selection and pricing rather than a parallel reward algorithm or unconditional bonus. Measure actual rewards from each eligible faction and difficulty, including small/large contracts and low/high reputation. Tune reward weights and price only from evidence. If rare prerequisites remain unreachable, consider a once-per-faction or bounded milestone reward of a physical prerequisite chip. Require idempotent mission identity and save/reload proof before using any additional grant.
- A duplicate physical chip stays a legitimate trade/research resource; no retroactive reroll of a chip already carrying a pattern. If duplicate-heavy progression blocks access, offer a faction-owned targeted singleton purchase or a separately specified exchange of duplicates.
- Non-contract Bramfaturas are not fabricated diplomatic factions: source their patterns from regional loot, expeditions, recovered caches or allied intermediaries. Design a rare archival recovery route for eliminated factions if needed, without reviving them or exposing their full catalog universally.
- Preserve special restrictions: Fury Sigil Skull stays faction purchase-only; Shadowblade remains finale-only. Do not copy native pacts, special items or non-obtainable story artifacts into generic pools.

## 3. Faction catalog ownership

| Native/mod entity | First group | Further groups | Acquisition |
|---|---|---|---|
| Tezctlan (Emerald Empire, Gannix) | Gavvakh processing; Gannix tissue and chitin | Igvas, Angerlings/common Gannix stock, living weapons, armor, grafts | Beneficiary missions and owned suppliers |
| Feathered Masks (XiomaraMasks) | Gannix processing; woven tissue | Angerlings and precision weapon patterns, Xiomara-specific equipment and grafts; shared Igvas where compatible | Own beneficiary missions and owned suppliers, not Tezctlan reward pool |
| Shedu's Thousand (SheduThousand) | Ur forge inputs, living metal | Eannis Redfire, forged mechanisms, body replacements, heat weapons | Own missions and suppliers after native activation |
| Chaos Undivided (mod faction) | Culture/containment, distillation, ritual fragments | Duggur/Ron/Shartamakhum recovery, Chaos-specific salvage, Bound Ash, Forge Ember, Shard Cells, Coil Cells, Soulforge arms | Own contracts and stations, with regional patterns in small themed pools |
| Other Bramfaturas | Their own seed, stock and basic processing | Regional ammo, parts, armor, relics and appropriate grafts | Verified expeditions/loot or restricted recovery route, not invented contract sponsors |

Gannix's two political factions share appropriate base material knowledge because they belong to the same Bramfatura, but retain separate rare armaments and graft catalogs. Keep the current physical recipe discovery model and acquire only those specific patterns that the installed game does not already supply through a legitimate native path.

## 4. Resource graph architecture

The intended shared foundation remains the existing time-limited clone/reagent route:
Raw Clone -> Torment Imprint -> Essence of Suffering -> Gavvakh.

Retain the existing time-only Raw Clone foundation for now; review throughput and whether common biomass/water should be a separate optional efficiency recipe, not an unapproved mandatory cost. Regionally sourced tissue/minerals, native salvage or recovered seed stock combine with shared reagents to produce distinct occult intermediates. Basic branches must have a seed available before their research rank demands the processed product. Do not let a research cost require an item that can only be made with that research and cannot be found, bought or earned beforehand.

Recommended depth:
- raw/manifested feedstock -> reusable regional stock -> specialized component -> finished item
- typically 2–3 meaningful intermediate transformations for ammo, 3–4 for armor/weapons/grafts; share intermediates between products, avoid creating a dozen single-use items per ammunition type.
- Common mundane feedstock can enter at the resource stage; finished conventional ammunition should not be mandatory feedstock for native occult ammo.
- Retain hybrid manufacture as an optional separate recipe only if the native receipt and picker support multiple recipes for one output without ambiguity. Otherwise select one owned canonical recipe; do not silently overwrite an existing native or third-party recipe.
- Preserve native item type, ammo class, expiry, stack size, weapon compatibility, damage rules, self-charging behavior and enemy drop identities unless a separate balancing change explicitly requires otherwise.

## 5. Proposed ammunition production families

The following are in-game crafting concepts, not final recipes. All new intermediate names below are proposed player-facing labels; no IDs have been reserved. Verify installed native names and compatibility before committing formulas.

| Region | Starting materials | Intermediate chain | Existing target |
|---|---|---|---|
| Gannix / Shartamakhum | Gavvakh, regional organic stock | Combat Gavvakh concentrate -> hardened living cell | Igvas (`quasi_basic_ammo`) |
| Gannix | Venusian tissue, feathers, essence | Chitin stock -> barbed dart blank -> living dart | Angerlings (`quasi_darts_ammo`) |
| Duggur | Lunar innards, lunar bone stock, essence | Calcified lunar matrix -> pain-bound nail blank | Nails of Pain (`quasi_bolts_ammo`) |
| Shartamakhum | Mercury glass, scales, essence | Resonant mercury substrate -> occult energy cell | Elerium (`quasi_energy_ammo`) |
| Ur | Mars skin/gear, Gavvakh, regional recovered stock | Eannis concentrate -> contained Redfire stock | Eannis Redfire (`mars_gas_ammo`) |
| Flauros | Jupiter mushrooms, grass/innards, Gavvakh | Spore culture -> brood substrate -> viable swarm | Flies (current mod target `flauros_ammo`; verify installed display ID) |
| Chaos Undivided | Shard Glass, bound essence | Shard matrix -> infused cell | Shard Cells (existing mod ID) |
| Chaos Undivided | Insulated Conduit, Forge Ember, occult energy stock | Bound conductor -> charged coil stock | Excruciator Coil Cells (existing mod ID) |

Important: Nails of Pain are native Duggur ammunition obtainable from quasimorph corpses. Keep that loot path intact. Do not force existing native weapon families onto unrelated ammo or require external ammo for a native self-charging weapon. AnCom's legitimate native use of Igvas remains valid. For hybrid Chaos weapons, audit each weapon's default ammo, allowed ammo family, magazine behavior, loot replenishment and existing supply before migrating a dependency; preserve compatible existing ammunition.

### Example preliminary batch ledger: Duggur

- 2 lunar bone stock + 1 Essence of Suffering -> 3 Calcified Lunar Matrix.
- 2 Calcified Lunar Matrix + 1 Gavvakh -> 4 Pain-bound Nail Blanks.
- 4 Pain-bound Nail Blanks -> 12 Nails of Pain.
- Numbers are provisional gameplay balancing, not a commitment. Audit net material costs and the +1/+2 facility yields before adopting the numbers. The new intermediates may share a Duggur metallurgy/armor branch, so they are not exclusively an ammunition tax.

The equivalent material branches should also feed other output families:
- Gannix: chitin plates, living weave, quasibronze/mechanism parts -> weapons, armor, grafts.
- Duggur: lunar matrix and occult metal -> armor plates, weapon parts, relic components.
- Shartamakhum: resonant glass, heat-bound scales -> energy stock and equipment.
- Ron: blood stock and tentacle fiber -> medicine, masks, grafts; no invented ballistic family where the native arsenal uses natural weapons.
- Ur: living metal and forge components -> fuel, forged skin, mechanisms, body replacements.
- Cattaram: geodes -> refined crystal -> weapon/armor cores and mines.
- Flauros: spore and living-fiber cultures -> organic materials, mines, compatible ammo and equipment.
- Agga/Scrivnus: verify native items and identities first; recovered ocular/relic processing with narrowly supported products. Do not invent a fully equipped faction.
- Olirna: defer a full chain until actual native region, equipment, material IDs and intended story restrictions have been audited.

## 6. Recipe data and balance rules

Plan a single explicit catalog that maps output -> owning recipe ID -> capability -> primary region -> physical pattern(s) -> input vector -> base batch -> base hours -> yield class -> source routes. Reuse it for registration, reward eligibility and crafting-map categories. Keep source ownership clear: new intermediate versus existing owned occult recipe versus native/other-mod recipe. Do not implement one generic catch-all formula for missing products. No native recipe replacement without an explicit audited compatibility decision.

For each proposed recipe verify positive input/output counts, existing registered IDs, no unwanted cycle, prerequisite reachability from physical loot/trade/reward or time-only foundations, no new free intermediate or self-sustaining multiply-and-recycle loop, no duplicate producer ambiguity, no accidental loss of original knowledge, no price arbitrage, and no runaway interaction of the two yield upgrades. Model normal, +1/+2 yields and 1–3 lines, at native difficulty/time settings. Yield improvements should apply to genuine intermediates and not multiply finished weapons or advanced grafts. Ammo output quantity is its own balancing decision; do not count rounds as material recovery. Preserve expiration and refrigeration rules for native items.

All newly added material chips should be available before dependent weapon chips, subject to faction activity. Loot/samples must permit reaching each capability cost. Existing trade/reward stacks should not evaporate when recipes change. Distinguish raw material acquisition, pattern acquisition and production availability in the UI; no raw internal identifiers in player text.

## 7. Crafting Map presentation and artwork

The map needs visible subtrees for Gannix, Duggur, Shartamakhum, Ron, Ur, Cattaram, Flauros, and Chaos, with shared reagent spine shown once and cross-family links visible rather than copied. Existing item imagery is preferred. For new intermediates, create distinct consistent inventory/floor/shadow assets and localization; research images retain the native green/crimson border. Add a recipe-detail field for the actual pattern chip and acquisition channel. Do not introduce a new research rank solely to make the graph bigger; retain six ranks and attach specialization to their recipe families.

## 8. Implementation and validation gates

Phase A (read-only inventory): export installed faction IDs, CanBeTraded/enabled status, station ownership, native mission reward selection and reputation/tech caps, current rewardChips records/weights and prices, all item records/weapon ammo bindings, expiry, existing native/other-mod recipes and chip unlock pools. Reconcile 21 historical pools with live results. Confirm latest occult facility native build and headed behavior. Produce a gap matrix before writing code.

Phase B (small prototype): first implement one Duggur Nails of Pain chain and prerequisite pattern route, plus one Tezctlan Gannix chain with actual successful contract reward. Verify a player can obtain a chip through gameplay, learn one recipe, research capability, order ingredients, fabricate the native item and use it with a compatible weapon. Validate real drop and trade behavior; inspect exact receipts and actual screenshots. Don't claim a pass from registration or a loading screen.

Phase C: expand verified approach across Gannix/Xiomara, Ur, Shartamakhum and Flauros, then the remaining regional materials and Chaos. Review each stage's economics and compatibility independently. Preserve all original campaign saves; use disposable fixture for tests.

Phase D: validate cross-faction reputation/activation/capture/defeat, reward eligibility and actual successful payout, cheap and expensive contracts, duplicate chips, save/reload, queue/cancellation and old orders, expiry, all ammo compatibility, native recipes, other installed mods, graph/UI, and no new release-blocking runtime errors. Owner review before publication.

## References checked

- Official Quasimorph Wiki: [Factions](https://quasimorph.wiki.gg/wiki/Factions), [Bramfaturas](https://quasimorph.wiki.gg/wiki/Bramfaturas), [Nails of Pain](https://quasimorph.wiki.gg/wiki/Nails_of_Pain), [Igvas](https://quasimorph.wiki.gg/wiki/Igvas), [Angerlings](https://quasimorph.wiki.gg/wiki/Angerlings), [Elerium](https://quasimorph.wiki.gg/wiki/Elerium), [Eannis Redfire](https://quasimorph.wiki.gg/wiki/Eannis_Redfire), [Flauros](https://quasimorph.wiki.gg/wiki/Flauros), [Herald of the Flies](https://quasimorph.wiki.gg/wiki/Herald_of_the_Flies). Wiki names and lore are reference evidence; installed game configuration is authoritative for identifiers, stats, current faction state and receipt ownership.
- Repository: `Plugin.OccultKnowledge.cs`, `Plugin.OccultRecipes.cs`, `Plugin.OccultProduction.cs`, `Plugin.OccultFacility.cs`, `Plugin.Industry.cs`, `Plugin.IndustrySupply.cs`, `Plugin.Drops.cs`, `Plugin.ChaosProduction.cs`, `Plugin.ChaosEquipment.cs`, `diagnostics/OccultResearchExpansion.md`.

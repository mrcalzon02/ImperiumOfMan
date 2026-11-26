# Occult capabilities

**Source status:** continuation from `40939a3`; this working change has not yet passed a native build or a headed game session. Source consistency checks are not runtime receipts.

## Six purchases

Occult Research provides fabrication **capability only**. It never teaches, removes, or refreshes recipe knowledge. A production order needs both its purchased capability and its independently learned pattern. Existing learned recipes remain in the save.

The six nodes form an adjacent two-row sequence: `(0,0) -> (1,0) -> (2,0) -> (2,1) -> (1,1) -> (0,1)`. There are no separate prerequisite facility purchases. Each node after I requires the previous capability. The module pip belongs between the native Engineering and Research markers.

| Level | Capability | One-time native research samples and supplies |
| --- | --- | --- |
| I Culture | Clone cultures and torment containment | `medical_equipment`, `quasisamples` |
| II Distillation | Suffering separation and Gavvakh condensation | `scientific_equipment`, `quasi_medical_kit_1` |
| III Manifestation | Agga, Gannix and Shartamakhum fabrication | `scientific_equipment`, `demonic_eye`, `mercury_glass` |
| IV Binding | Duggur, Ron and Scrivnus fabrication | `scientific_equipment`, `moon_guts`, `ron_blood` |
| V Infusion | Ur, Cattaram, Flauros and six Mars replacements | `scientific_equipment`, `mars_skin`, `cattaram_geode`, `jupiter_mushrooms` |
| VI Soulforge | Advanced occult assembly and selected quasi grafts | `prototype_schematics`, `quasi_repair_kit`, `mars_cyborg_head` |

These samples are consumed by the native upgrade purchase transaction. Their purpose is to require acquired occult material before repeatable manufacture. They are not fabrication inputs charged on every subsequent order. `quasisamples` is a real native ScienceBarter record in the immutable [exported game configuration](https://github.com/Raigir/Quasimorph-Mod-Tool/blob/e6e557f1f679da0b2462ba2862296e08bd70cd28/ref/base/trash.txt); the installed game's complete item/configuration snapshot still requires runtime verification.

## In-game knowledge

`Plugin.OccultKnowledge.cs` declares **21 physical pattern-chip pools** covering all **53 candidate new formulas**. Each physical chip teaches **one** pattern selected by the native datadisk mechanism; the largest declared pool has five candidates. Research purchases, loading a save, and opening Production grant none of these patterns.

| Pattern groups | Existing supplier route |
| --- | --- |
| Culture; Distillation | Chaos Undivided, Hollow Signal Depot |
| Agga; Duggur; Ron; Scrivnus | Chaos Undivided, Ossuary Works |
| Gannix tissue, forge, ammunition and arms | Tezctlan, Feathered Temple |
| Shartamakhum; Cattaram | Chaos Undivided, Cinder Crucible |
| Ur; Mars core and limb replacements | Shedu Thousand, Red Tiangong |
| Flauros cultures and ordnance | Chaos Undivided, Chainforge |
| Tezctlan, Xiomara and Gannix grafts | Their existing faction stock and chip-reward routes |
| Soulforge relics | Chaos Undivided, Black Meridian |

The existing `RegisterCatalog` stock/reward system supplies the chips. If the preferred station is unavailable, its existing faction-station fallback applies. These catalogs use `restrictOtherSources: false`: native starts, workbench formulas, and other schematic sources are preserved.

The numbers above describe source candidates. Missing installed native outputs/inputs and existing native/mod formulas are logged and skipped **before** chip pools are created. Empty pools are omitted. Late missing staged custom ingredients fail registration rather than advertise a broken formula. Actual registered counts must come from the game log.

## Scope

Selected existing Chaos weapons, armor, pact skulls, esoteric stock, Bound Ash and Soulforge Embers gain capability requirements while retaining their existing physical schematics. Ordinary food, dressings, medicines, mechanical supplies and physical stock keep normal crafting. Shared ordinary implants `cortex_stimulator` and `superdermal_layer` are not taken over by occult production. Fury Sigil remains purchase-only; Shadowblade remains the finale reward; Purity Seals keep their existing reward route.

The Crafting Map reports recipe knowledge, capability and available materials separately. Inventory/floor/shadow assets and the six research/locked images must be reviewed in the native game. This change does not claim complete equipment coverage for all nine Bramfaturas; dedicated Olirna stock remains outside the verified catalog.

## Pending verification

Native compile and headed launch must verify registration/skips, six-node linking and pip placement, artwork states, sample consumption, physical chip selection, independent knowledge/capability gates, cargo consumption, cancellation, save/load, and protected acquisition rules. The current installed cancellation API refunds its consumed-item ledger once; the obsolete reference-DLL refund concern does not require a patch.

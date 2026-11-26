# Occult capabilities

**Current source — 1.1.10:** `5fae43b49ff9fe4ee21cb88a3e8e30eb8e3558ad` preserves the six capabilities and removes finished-ration recycling. Native compilation passed with zero warnings/errors; the package, compiled correction and all 24 embedded PNGs are verified in the [current receipt](FinishedRationBuildReceipt.json). No native game was launched for 1.1.10. [Correction scope](FinishedRationOutputCorrection.md).

**Historical 1.1.9 build:** source `347de7db77f9121ad26e1c12c534b97760ee92f2`, assembly **1.1.9.0**, passed native build [45838490203](https://github.com/mrcalzon02/Dens-Mortis/actions/runs/45838490203) with **zero warnings and zero errors**. The [source-347 receipt](OccultBuildReceipt.json) verifies the historical 1.1.9 candidate, four runtime files and all 24 embedded occult PNGs. Exact-source session 005 loaded the disposable campaign and recorded 21 pattern pools, 53 occult outputs, the seven intended spoiled-organic records and Ron Mask pact preservation.

**Historical source-347 runtime:** session 005 verified the map controls and custom-order queue completion. Session 006 passed Space/save and **46 hours 20 minutes of game time**, with zero missing-preset diagnostics or managed-exception headings in its complete 15,897-byte log. Source-347 captures verify the six-node tree and both corrected tooltip hovers. All three sessions restored the original package, preserved original saves and cleaned their fixtures. [Build/runtime receipt](OccultBuildReceipt.json) · [Source-347 native UI evidence](../docs/crafting/native-occult-ui-347de7d.json).

**Remaining transaction scope:** no research purchase or physical-chip learning was performed in the visual checks. Exact recycling, sample/cargo consumption, refunds, legacy migration and purchased/item sprite states still require their targeted cases. The separate raw-ore measurement gap was closed by [session 007](CraftingMapAcceptance-347de7d-007.json) on historical source `347de7d` / 1.1.9: one four-output batch increased the named Metal smelting feedstock stack from **2/30 to 6/30**. This is a named-stack measurement, not a whole-inventory aggregate or a 1.1.10 gameplay test.

## Six purchases

Occult Research provides fabrication **capability only**. Occult research purchases, their migration and the occult UI never teach, remove or refresh recipe knowledge. A production order needs both its purchased capability and its independently learned pattern. The occult migration preserves existing learned recipes.

This guarantee is scoped to those occult paths. The separate general Industry load hook still teaches `SharedIndustryRecipes` and retires Fury Sigil's legacy recipe/orders to retain its purchase-only route; those existing behaviors are not occult research grants.

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

## Existing saves

The prior layout used four facility purchases followed by five research ranks. Its research IDs were reused by the six-capability layout, so saves containing any obsolete facility ID receive a one-time capability migration during `MagnumProgression.OnAfterLoad`.

| Highest old purchase | Preserved capability |
| --- | --- |
| Clone Vat or Containment Cradle | I Culture |
| Essence Still or Gavvakh Retort | II Distillation |
| I Manifestation | III Manifestation |
| II Binding | IV Binding |
| III Infusion | V Infusion |
| IV Soulforge or V Ascension | VI Soulforge |

Migration snapshots the old purchases before adding the mapped capability and its prerequisites with native `AddPerk(..., silent: true)`. It then removes only the four obsolete facility IDs. Other purchases and every learned recipe remain unchanged. Removing the old markers makes later loads a no-op; saves without those markers are never reinterpreted. The migration does not add facility records or extra visible nodes. Native save/load validation remains pending.

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

The numbers above describe source candidates. Missing installed native outputs/inputs and existing native/mod formulas are logged and skipped **before** chip pools are created. Empty pools are omitted. Late missing staged custom ingredients fail registration rather than advertise a broken formula. Both the historical `bdb1759` run and exact-source 347 session 005 confirm 53 registered occult recipes and 21 physical pattern pools; this registration count does not by itself verify their purchase or production transactions.

## Scope

Selected existing Chaos weapons, armor, pact skulls, esoteric stock, Bound Ash and Soulforge Embers gain capability requirements while retaining their existing physical schematics. Ordinary food, dressings, medicines, mechanical supplies and physical stock keep normal crafting. Shared ordinary implants `cortex_stimulator` and `superdermal_layer` are not taken over by occult production. Fury Sigil remains purchase-only; Shadowblade remains the finale reward; Purity Seals keep their existing reward route.

The Crafting Map reports recipe knowledge, capability and available materials separately. The historical source-347 tree was viewed with Culture's dim green available image and five crimson locked images, five adjacent links, and native yellow hover. Purchased states and inventory/floor/shadow sprite fit still require their targeted checks. This change does not claim complete equipment coverage for all nine Bramfaturas; dedicated Olirna stock remains outside the verified catalog.

## Historical bdb validation and pending current checks

The historical bdb native build used installed game assembly SHA-256 `d72cd91f98de4d6cce8ee420ac1a906d89e68ba0cb164694d266b56f46339ebe`. The verified candidate `ImperiumOfMan-bdb17590322a-native-candidate.zip` has compiled assembly version 1.1.9.0 and archive SHA-256 `3e958daebacf3cee4fe7f503bc453d2dbc773bddb7a619b292a376f5d37b9f23`. All six normal/locked research pairs and all four inventory/floor/shadow asset sets are present in the DLL with verified dimensions and hashes.

The earlier `585715f` candidate is superseded. Its headed session exposed an overbroad spoiled-organic intake rule and a native `PactComponentRecord` project-cloning failure while loading `ron_mask_1`. The current compiled source includes the strict destination predicate described in [Organic recycling](OccultOrganicProduction.md) and the scoped compatibility change documented in [Native equipment projects with pact components](OccultProjectCompatibility.md). Their bounded runtime observations are recorded below and in the linked organic diagnostic.

The historical exact-build headed session [45824345407](https://github.com/mrcalzon02/Dens-Mortis/actions/runs/45824345407) uses source `bdb17590322a8f27090918f83e0bf613eaee04de`. Immutable delivery `8978febd64c670f2a22a8ccaff36054545c047a7` confirms successful disposable slot 2 loading into Dungeon, completed autosave and preservation of the `ron_mask_1` project pact component; frame 0006 shows dungeon gameplay. No recurrence of the earlier pact-cloning exception appears in that reviewed load.

Frame 0014, immutable delivery `310c507f6f8f63baf893c17cc59f8518830076b6`, shows exactly six nodes in two rows and five adjacent snake links, with no separate facilities. Culture is dim green and the other five nodes have crimson icons and borders. Frame 0012 places the occult marker at approximately `(915,577)`, midway between observed Engineering `(777,632)` and Research `(1053,521)` markers; selecting it opens Occult Research. The runtime diagnostic identifies the actual anchors as `engineer / research`.

Culture and Manifestation hover tooltips preserve their native captions, Available/Blocked status, cost icons and yellow selection border. Two text defects were observed: the recipe-knowledge label overlaps its value, and the combined Gannix/Shartamakhum value clips the final name. The scoped source fix committed at `8563a18c50f63b8e6d54d51d0db8d79c8b7fa104` shortens the label to `Recipes`, retains `Acquired in game`, and gives Agga, Gannix and Shartamakhum their own Materials rows. These corrections are not in the historical `bdb1759` ZIP. They are included in compiled 347 and passed [focused native visual review](../docs/crafting/native-occult-ui-347de7d.json), run 45857447221. The [Culture capture](../docs/crafting/native-occult-culture-347de7d.png) shows the shared row without overlap; the [Manifestation capture](../docs/crafting/native-occult-manifestation-347de7d.png) shows all three Materials names in full. Native status and costs remain visible.

Subsequent Distillation, Infusion and Soulforge hovers show their specific values fitting the native rows, including `Essence of Suffering`, `Six Mars replacements` and `Advanced occult work`. Those names and values remain unchanged. No broader tooltip resizing is proposed.

The session ended by operator stop at action sequence 21. Immutable cleanup delivery `bd04d4a192cefee80a2acaf7ed3ee3f58d857729` confirms that the original package was restored, original campaign files were unchanged and disposable-fixture cleanup completed, with no retained or unexpected campaign files.

The [unaltered native tree screenshot](../docs/crafting/native-occult-tree-bdb1759.png) and its [source/hash manifest](../docs/crafting/native-occult-tree-bdb1759.json) preserve frame 0014 from the `bdb1759` candidate. This image predates the tooltip text fixes committed at `8563a18`; it does not verify those later changes.

Remaining checks cover purchased artwork states, sample consumption, physical chip selection, independent knowledge/capability gates, cargo consumption, cancellation, further save/load behavior and protected acquisition rules. The confirmed visual, boot/load and cleanup results do not establish those interactions. The current installed cancellation API refunds its consumed-item ledger once; the obsolete reference-DLL refund concern does not require a patch.

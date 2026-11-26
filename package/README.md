# Runtime package

## Current verified package — 1.1.10 ration recovery correction

**[Download ImperiumOfMan-1.1.10-CraftingMap.zip](https://raw.githubusercontent.com/mrcalzon02/ImperiumOfMan/186e2b76464b91888101657caec5e5cd531c80bb/package/ImperiumOfMan-1.1.10-CraftingMap.zip)** · [GitHub file](https://github.com/mrcalzon02/ImperiumOfMan/blob/186e2b76464b91888101657caec5e5cd531c80bb/package/ImperiumOfMan-1.1.10-CraftingMap.zip) · [Screenshot gallery and direct image links](../docs/crafting/DELIVERABLES.md)

Compiled source: [5fae43b49ff9fe4ee21cb88a3e8e30eb8e3558ad](https://github.com/mrcalzon02/ImperiumOfMan/commit/5fae43b49ff9fe4ee21cb88a3e8e30eb8e3558ad). Assembly **1.1.10.0**. Publication commit: `186e2b76464b91888101657caec5e5cd531c80bb`.

This package removes the unintended eight-ration-to-one-biomass conversion and its batch machinery/UI. Both Corpse Starch and Dirty Corpse Starch have independent empty disassembly lists, preserving their native donor item. Forward manufacturing, eligible spoiled-organic recovery, clone harvesting, equipment salvage and protected-item handling remain. [1.1.10 change notes](../docs/crafting/CHANGE_NOTES_1.1.10.txt).

[Native build 45870535209](https://github.com/mrcalzon02/Dens-Mortis/actions/runs/45870535209) passed with zero warnings and errors. The [published verification receipt](https://github.com/mrcalzon02/ImperiumOfMan/blob/186e2b76464b91888101657caec5e5cd531c80bb/diagnostics/FinishedRationBuildReceipt.json) records complete transport, ZIP CRC, four-member, member-hash, manifest, assembly-version and embedded-artwork checks. Static inspection of the compiled DLL verified removal of the ration reverse route and its bulk hooks, preservation of the forward recipe, and independent empty ration disassembly lists.

ZIP: **11,794,130 bytes**. SHA-256: `8af3889a259a8a988d7e2a154d4a87af653b59b3a05de5c9616a32e529e00cf1`.

**Gameplay scope:** game boot and gameplay were not run for 1.1.10. The gallery and earlier runtime observations are from source `347de7d`, assembly 1.1.9.0; they retain that attribution.

## Install the verified 1.1.10 package for testing

Use the complete 1.1.10 ZIP linked above. It includes the finished-ration correction.

Close Quasimorph. On the tested installation, open this Workshop folder:

```text
D:\SteamLibrary B\steamapps\workshop\content\2059170\3754620214\
```

If Steam is installed elsewhere, use the same `steamapps\workshop\content\2059170\3754620214\` suffix in that library. This is the established Workshop installation for Imperium Reforged.

Extract the ZIP, open its `ImperiumOfMan` folder, and copy these **four files directly into `3754620214`**, replacing the existing copies:

- `ImperiumOfMan.dll`
- `imperiumofman.bundle`
- `modmanifest.json`
- `thumbnail.png`

Do not add a nested `ImperiumOfMan` directory inside the Workshop folder. The separate loose `package/ImperiumOfMan/` folder in this repository is the older baseline; use the complete ZIP.

## Historical 1.1.9 package and gameplay evidence

**Historical version:** this older `347de7d` ZIP contains the former ration recovery route. Use the 1.1.10 download above for the correction. The 1.1.9 features and gameplay records below retain their original source attribution.

Historical package: **[ImperiumOfMan-1.1.9-CraftingMap.zip](ImperiumOfMan-1.1.9-CraftingMap.zip)**. Source `347de7db77f9121ad26e1c12c534b97760ee92f2`, assembly **1.1.9.0**. Native build [45838490203](https://github.com/mrcalzon02/Dens-Mortis/actions/runs/45838490203) and package verification passed with zero warnings/errors. Session 005 verified the map controls and queue completion. Corrected-directory session 006 passed Space/save and a bounded 46-hour-20-minute game-time check without the earlier preset exception. Session 007 used the unchanged package and confirmed an exact +4 change in the named Metal smelting feedstock stack, plus Focus, double-click, station cooutputs and a searchable item without a producer. The 1.1.9 six-node visuals and both corrected tooltip hovers passed. Research purchases, chip learning, recycling and other transaction checks retain the limits below. [Concise 1.1.9 crafting notes](../docs/crafting/CHANGE_NOTES_1.1.9_CRAFTING_MAP.txt) · [Cumulative 1.1.9 change notes](../docs/crafting/CHANGE_NOTES_1.1.9.txt).

The cumulative comparison starts at the published [v1.1.0 release](https://github.com/mrcalzon02/ImperiumOfMan/releases/tag/v1.1.0), source `b08c27e38bbc73712e073ec9141a6e8769984d9a`. The campaign and endgame content already present there is retained background. The changes below are included in this compiled test package; their gameplay acceptance is recorded separately. This review has not published the candidate as a Steam update.

## Changes included in the verified 1.1.9 package

### In-game Crafting map

A **Crafting map** tab appears on the left edge of Production and its recipe picker. Its muted sage-green panel reads the active game's registered recipes and item sprites. Use the mouse wheel to zoom, drag the canvas to pan, and click an item or recipe to highlight its connections. Single-click selection preserves the view; Focus or double-click can recenter the selected branch. Ingredients, outputs, base time, and access/readiness information appear below the graph. Quantity labels now appear only on connections incident to the selected item or process, while all connections remain visible. Both lower detail columns have auto-hiding scrollbars and a stable text gutter.

Search includes native and mod items, including items without producers. Filters cover Magnum production, workbenches, station industry, and recovery. Alternative recipes, multiple outputs, modified-project outputs, and custom batch multipliers are preserved. Opening the map or pressing Refresh captures current recipe knowledge, cargo, station listings, and occult requirements. The map is read-only; queue submission and quantity planning remain further work.

The **Recovery** view includes Raw Clone harvesting: one clone returns four native meat, two skin, and one rib where those records exist. The separate startup reachability log checks Magnum and workbench recipes only; it omits recovery, station routes, acquisition conditions, and quantities. Its warnings for `bone_knife`, `impaler`, and `skrivnus_knife` therefore do not prove global unreachability. Recipe registration, learned patterns, capabilities, recycler access, and actual completed orders remain separate checks.

### Crafting balance and chemistry

Custom Magnum recipes retain their different ingredient types while each quantity is halved, rounded up with a minimum of one. Existing custom output batches are doubled. A service Lasgun order now uses one rolled plate, one armor fitting, and one weapon component to produce two Lasguns. Production time and the established recipe/unlock rules still apply.

The chemistry update adds five raw fractions: cellulose pulp, organic oil, chemical extract, reactive minerals, and carbon-rich feedstock. Biomass supplies the organic branches; ore supplies the mineral branches. These connect to acid, gasoline, paper, plastic, rubber, propellant, explosives, and ordinary mechanical and electrical components. New component batches generally produce four units; the new ammunition orders produce twelve, while finished medicine, repair, grenade, and cyborg orders produce two.

Biomass preparation and conductor separation are now shared foundations. New ration/workshop chips omit those already-known outputs, and new support chips omit shared gasoline production. Existing saved chip selections remain valid. Medicine catalogs retain native access, finished support goods keep their established tiers, and each custom chip still selects one recipe. Existing native Sorbent production is preserved; only a missing native recipe receives the new fallback.

This full package also contains the earlier ore/water extraction, glass production, Ore Cargo opening, chip-selection fix, Fury Sigil purchase restrictions, Imperial ration loot, loose-material recipe corrections, and Bound Ash footprint/stacking fixes. [Chemistry audit and scope](../diagnostics/RawMaterialChemistryAudit.md)

### Occult production and ship controls

The **Occult Research** module has six sequential capabilities: Culture, Distillation, Manifestation, Binding, Infusion, and Soulforge. Research purchases provide capability; physical in-game pattern chips each teach one independently selected recipe. The native tooltip uses a shorter Recipes label and separate Gannix and Shartamakhum rows to fit the available width. Orders require both the appropriate capability and learned recipe. Known patterns with missing capability are dimmed in the production picker and explain their requirement.

Culture supports two Raw Clones in eight base hours without cargo ingredients, then two clones into four Torment Imprints in four base hours. Distillation supports four imprints into four Essence of Suffering, then four essence into two native Gavvakh; each operation takes four base hours and needs its own learned pattern. A pre-existing native Gavvakh formula is preserved. Clone harvesting is a separate recycling operation. Later capabilities cover selected native otherworldly items and selected Chaos fabrication. Ordinary crafting retains its established access. Full equipment coverage for every otherworldly family remains further work.

The implemented six-capability progression supersedes the earlier four-facility/five-rank proposals in the design documents. Dedicated Olirna stock, remaining family equipment coverage, quantity planning, automatic queue submission, and complete runtime exports are future work. The authored candidate does not establish a complete raw-material path to every game object.

The candidate adds a one-time migration for saves containing obsolete occult facility purchases, preserving their earned capability in the six-node progression while leaving recipe knowledge unchanged. The map now reads each recycling route's consumed-input batch size and explains whole batches and retained remainders. Migration and batch behavior still require native acceptance.

Dirty Corpse Starch has an explicit bulk route: eight eligible rations produce one Raw Vat Biomass. Selected stacks count together; incomplete batches retain their original item state and remain separate from other stacks. Locked and quest custom-profile inputs are protected. The native recycle button previews returns and remainders and disables an incomplete ration-only selection. Ordinary mixed-item recycling, the native recycler department, and its duration remain in use. Expiry metadata is read from the native `Data.ItemExpire` table.

The spoiled-organic rule now checks the destination itself for spoilage and organic identity, excluding clean water and mechanical salvage. Equipment outputs added by the occult recipes retain their pact component when native projects create an upgraded custom variant. The compatibility path preserves native primary equipment cloning and upgrades, uses independent pact category/disassembly lists, and respects another patch skipping native injection. Both the historical `bdb1759` run and exact-source 347 session 005 verified the intended organic target set and successful copied-campaign loading with the pact component retained. Corrected-directory session 006 passed the bounded Space/time-advance check; the remaining transactions still need their focused tests. [Observed defects](../diagnostics/CraftingMapHeadedValidation.md) · [Compatibility implementation](../diagnostics/OccultProjectCompatibility.md)

The candidate also includes the top raid call-in drawer, native upgrade effect rows and item descriptions, and separate Comms Node turret replacement chains for non-story missions. Random-site, entrance, and extraction locations independently progress from Ballistic through Flamer, Beamer, Arc, and Lance; each deploys its highest purchased type. Ship marker, node-size, lock-tint, and fitted-mount connection corrections are retained. [Occult production](../diagnostics/OccultOrganicProduction.md) · [Capabilities](../diagnostics/OccultResearchExpansion.md) · [Call-in drawer](../diagnostics/ShipSupportDrawer.md) · [Comms turrets](../diagnostics/CommsTurretProgression.md)

## Historical 1.1.9 observations and remaining limits

### Session 007: completed production and map continuation

[Session 007](https://github.com/mrcalzon02/Dens-Mortis/actions/runs/45869745770) used the same source `347de7d` and unchanged 1.1.9 package. Its [acceptance record](../diagnostics/CraftingMapAcceptance-347de7d-007.json) records the continuation evidence.

| Check | Observed result |
| --- | --- |
| Named-stack production | Metal smelting feedstock was identified at 2/30 before production and 6/30 afterward. One batch was selected; both the native queue and produced banner showed four outputs. This establishes an exact +4 change in that stack, without claiming a whole-inventory aggregate measurement. |
| Focus and double-click | Both map actions were exercised successfully. |
| Station cooutputs | The Stations view retained two outputs of eight units each for the inspected recipe. |
| Item without a producer | Search found Purity Seal and showed zero producing routes and three uses. |
| Cleanup | Final status at immutable delivery `64d12e10c05b364fee33e392c6c38bdc09e9d034` confirmed `original_package_restored=true`, `existing_campaign_files_unchanged=true` and fixture `cleanup_complete=true`. |

### Earlier session 005: map refinements and queue completion

[Session 005](https://github.com/mrcalzon02/Dens-Mortis/actions/runs/45841032741) used this exact 347 candidate. Its [sanitized acceptance record](../diagnostics/CraftingMapAcceptance-347de7d.json) retains the original frame hashes and evidence references; its cargo-measurement gap was subsequently closed by session 007 above.

| Check | Observed result |
| --- | --- |
| Startup and copied-campaign load | Passed; 21 pattern pools, 53 occult outputs, seven intended spoiled-organic records and Ron Mask pact preservation were recorded. |
| Map entry and live index | Production and its picker showed the left map tab; the live index reported 2,363 items and 3,017 routes. |
| Quantities and details | Selected item/process labels remained focused; both overflowing detail lists reached their final rows; wheel input stayed within the detail list; short lists hid their scrollbars. |
| Fit, Escape and owner lifetime | Fit contained the 19-node example at 48%; Escape closed the map while retaining Production; leaving Production removed the tab. |
| Custom order | One batch was explicitly chosen; the native queue showed four raw-ore outputs, one displayed hour and Conveyor 1/1, then later returned to Conveyor 0/1. |
| Cargo increase | A matching-looking stack of six was visible afterward. This session lacked a named before-stack measurement; the exact +4 named-stack result belongs to session 007 above. |
| Raid Call-ins header | Collapsed and expanded labels fit at 1920 × 1080. This is separate from the spacecraft Drop Support header comparison. |
| Cleanup | Operator stop at sequence 42 restored the original package, left all 11 protected original campaign files unchanged and removed the six remaining disposable-fixture files. |

**Corrected launch-context check passed:** the original session-005 launch context had an empty global preset catalogue and repeated native mission-generation exceptions. The [read-only loader audit](https://github.com/mrcalzon02/Dens-Mortis/actions/runs/45849018302) verified the relative `GameData/BinaryPresetsMap.json` lookup and silent empty-map fallback. The adapter now launches from the installed game directory without changing mod C#.

Corrected-directory [session 006](https://github.com/mrcalzon02/Dens-Mortis/actions/runs/45849635092) used the same 347 binary, completed the Space transition and save, and advanced from 26.10.2200 00:04 to 27.10.2200 22:24: **46 hours 20 minutes of in-game time**. Its complete 15,897-byte log contains zero missing-preset diagnostic lines and zero managed-exception headings. This confirms the bounded launch-context retest, not every gameplay transaction.

Session 006 ended at `bounded_slot_finished`, sequence 32. Immutable delivery `99ca540fe800abc8c9adda725c1bf4281bb339eb` confirms the original package was restored, original campaign files were unchanged and the disposable fixture was fully cleaned.

The [current Culture capture](../docs/crafting/native-occult-culture-347de7d.png) shows `Recipes` / `Acquired in game` clearly separated. The [current Manifestation capture](../docs/crafting/native-occult-manifestation-347de7d.png) shows Agga, Gannix and Shartamakhum in three complete Materials rows. Native status, material costs and yellow hover remain intact. The [six-node tree](../docs/crafting/native-occult-tree-347de7d.png) and [provenance/cleanup record](../docs/crafting/native-occult-ui-347de7d.json) retain the exact 347 identity. The older bdb screenshot remains historical. Capability purchases, pattern-chip learning, protected recycling, other cargo transactions, cancellation/refunds, further save/load and purchased/item sprite states remain separate checks.

### Artwork and design review

The [art manifest](../media/OccultProduction/art-manifest.json) links 10 generated masters, 24 runtime PNG variants, six capability IDs/positions/parents and one separately identified original Navigation style guide. The [design review ZIP](../docs/crafting/ImperiumReforged-Crafting-Design-Review.zip) includes those files, the exporter, source, companion documents and the offline map. The [historical native tree image](../docs/crafting/native-occult-tree-bdb1759.png) and its [manifest](../docs/crafting/native-occult-tree-bdb1759.json) retain their original bdb source and hash.

## Focused in-game checks

The session-005 results above establish bounded map behavior and native queue completion. The corrected-directory simulation check also passed its bounded scope. Session 007 confirmed the named feedstock stack increased by exactly four after one batch. Occult purchases, recycling and the following broader production-chain cases remain pending:

1. Load an existing save and confirm shared biomass, conductor, and chemical recipes appear. Confirm the five new material icons display.
2. Complete a biomass order: one Piece of Flesh and one Water Bottle should produce eight biomass. Use biomass and water to make pulp and chemical extract, and biomass alone to make oil.
3. Complete the mineral branches, then produce acid and gasoline. The acid order should yield four units and the gasoline order twelve.
4. With an already learned recipe, confirm reduced inputs and doubled output on a representative custom equipment order. Check that a newly found chip still teaches one selected recipe.
5. Start and cancel one new ordinary order, then save/load a separate active order. Check its paid inputs, progress, and final output. Inspect the log for `[Cvar_ImperiumOfMan] Crafting coverage` and any registration or missing-resource errors.

Map acceptance covers its presence in Production and the recipe picker, left-side placement, sage styling, zoom, pan, selection, recipe details, filters, collapse/Escape, and removal when Production closes. Session 007 adds Focus, double-click, station cooutputs and a searchable item without a producer. Observations are recorded in the [crafting map audit](../diagnostics/CraftingMapAudit.md) and [session-007 acceptance](../diagnostics/CraftingMapAcceptance-347de7d-007.json). The graph audit does not establish recovery balance or completion of every crafting chain. Research purchases, other cargo transactions, further save/load behavior and wider gameplay remain unverified until separately exercised.

## Build evidence

Source `347de7db77f9121ad26e1c12c534b97760ee92f2`, request `crafting-map-visual-diagnostic-001`, [native build 45838490203](https://github.com/mrcalzon02/Dens-Mortis/actions/runs/45838490203), controller `b42126b92be056ecd6ea1da1cce542d55b8c16e7`: native compilation passed with zero warnings and zero errors. Package verification covered the 30 transport chunks, exact four members, ZIP CRC, member hashes, manifest identity, assembly version and relevant static hook metadata. All 24 embedded occult PNGs were verified.

[Companion build receipt](../diagnostics/CraftingMapBuildReceipt.json) · [Combined build/runtime receipt](../diagnostics/OccultBuildReceipt.json) · [Session-005 acceptance cases](../diagnostics/CraftingMapAcceptance-347de7d.json) · [Session-007 continuation](../diagnostics/CraftingMapAcceptance-347de7d-007.json).

ZIP size: **11,796,267 bytes**. SHA-256: `4a096861fa4315820e3391fdc948cc704b01301d502b2c1897056feb05825413`.

The combined build/runtime receipt records the 005 map findings and completed 006 launch-context check. The separate session-007 record adds the verified named-stack +4 result and map continuation checks on the same source and package. Both current corrected tooltip hovers also passed; remaining transactions retain their separate scope. The [archived bdb build receipt](../diagnostics/OccultBuildReceipt-bdb1759.json) preserves the superseded package and native tree evidence without relabeling them as 347.

### Historical live map result and current limitation

[Headed session 003](https://github.com/mrcalzon02/Dens-Mortis/actions/runs/45825014723) tested source `bdb17590322a8f27090918f83e0bf613eaee04de`. The copied campaign loaded successfully, the sage panel appeared in Production and its recipe picker, and search, selection, wheel zoom, drag pan, and Escape were exercised. The live index contained 2,363 items and 3,017 routes. Lasgun details showed one plate, one fitting, and one weapon part producing two Lasguns in twelve base hours; Raw Clone details showed no inventory inputs, two outputs, eight base hours, and separate capability/pattern requirements. One ordinary medical order was submitted at quantity one and displayed one item, one hour, and Conveyor 1/1. This did not complete a custom batch, cancel/refund an order, or perform a recovery transaction.

That session exposed overlapping connection quantities and an unclear detail-list scrolling affordance. Session 005 on 347 verified the revised connection labels and detail scrolling. The narrower occult tooltip rows subsequently passed current-source hover checks in focused run 45857447221. It also contains bounded diagnostics for the separate mission-generation null collection observed after returning to Space. The diagnostic identifies station/template/folder context without repairing presets or suppressing the original error. The corrected-directory Space check passed; the unperformed transaction cases remain listed separately. [Detailed observations and original-file restoration](../diagnostics/CraftingMapHeadedValidation.md)

The earlier 1.1.9 package from `585715f554181ed56220c5ba2ed9055b46b44b38` booted but failed when the copied campaign loaded. It also accepted unintended organic recycling targets. Its Production/map interactions were not reached. The original runtime package and all original campaign files were preserved.

## Historical 1.1.6 candidate

The earlier 1.1.6 candidate belongs to source `40939a398f1a2d3f79ec9ece08cfe5170444d885`, native build [45798510569](https://github.com/mrcalzon02/Dens-Mortis/actions/runs/45798510569). It passed native compilation and complete package verification; the unchanged candidate booted and completed mod registration in [headed run 45801114949](https://github.com/mrcalzon02/Dens-Mortis/actions/runs/45801114949). The fresh slot-1 campaign reached the locked Conveyor requirement, displayed as `1 (0)`, so Production and the Crafting map were not reached. The session closed with the original runtime package restored and all seven pre-existing slot-0 files unchanged. This is historical startup and cleanup evidence, not a map/UI pass or verification of the changed 1.1.9 code. [Execution record](../diagnostics/CraftingMapHeadedValidation.md)

The historical archive was 11,763,638 bytes; SHA-256 `0a8b5e397896fc3597a9b9f7da5185fc11b5b9eb7d29be4d8d87b2ba269c3db3`. It is not the current test download. This review has not published the candidate as a Steam update.

## Earlier verified 1.1.8 Native Tooltips package

[ImperiumOfMan-1.1.8-NativeTooltips.zip](ImperiumOfMan-1.1.8-NativeTooltips.zip) pins source `d2da0db3a4b794cdfa25ae8369c3ef098e0d40ac`. Its 44 custom upgrades have native effect rows, with brief subtitles and wrapping item descriptions. Its ship-support and Comms features predate the six-capability occult redesign. Native build [45802511629](https://github.com/mrcalzon02/Dens-Mortis/actions/runs/45802511629) passed with zero warnings and errors; its [receipt](../diagnostics/UpgradeTooltipBuildReceipt.json) records the verified archive/member hashes and assembly version. Live tooltip hover and gameplay acceptance remain unrecorded for that package. [Version scope](../diagnostics/NativeUpgradeTooltips.md) · [Coverage audit](../diagnostics/UpgradeTooltipIntegrationAudit.json) · [Original presentation audit](../diagnostics/UpgradeTooltipSourceAudit.json)

The [1.1.4 Chemistry Foundations package notes](README-1.1.4.md) and earlier ZIPs remain available for reference.

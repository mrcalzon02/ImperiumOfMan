# In-game crafting map — 1.1.9 candidate

## Current corrected-directory result — session 006

Corrected-directory [session 006](https://github.com/mrcalzon02/Dens-Mortis/actions/runs/45849635092) used the same 347 binary, completed the Space transition and save, and advanced from 26.10.2200 00:04 to 27.10.2200 22:24: **46 hours 20 minutes of in-game time**. Its complete 15,897-byte log contains zero missing-preset diagnostic lines and zero managed-exception headings. This confirms the bounded launch-context retest, not every gameplay transaction.

Session 006 ended at `bounded_slot_finished`, sequence 32. Immutable delivery `99ca540fe800abc8c9adda725c1bf4281bb339eb` confirms the original package was restored, original campaign files were unchanged and the disposable fixture was fully cleaned.

The exact source remains `347de7db77f9121ad26e1c12c534b97760ee92f2`, candidate SHA-256 `4a096861fa4315820e3391fdc948cc704b01301d502b2c1897056feb05825413`; no mod C# was changed for this retest. Controller `1ffc3f3b8094452b2031c83b08b236d022d6c3d6` launches from the installed game directory. The complete final log SHA-256 is `ff0ab3f3f813bf26637d43fe4e69d40ff4591ced7c611b79f09af79af84d2bdd`; cleanup receipt SHA-256 is `c20b112d26f68e18d8b72d085c3d6dcd40eb6f3dc4f7834407e152abe465021b`.

[Current receipt](OccultBuildReceipt.json) · [Session-005 cases and frame hashes](CraftingMapAcceptance-347de7d.json) · [Current ship/UI evidence](../docs/crafting/native-occult-ui-347de7d.json). Current Culture and Manifestation hovers passed in focused run 45857447221: Recipes / Acquired in game fits, and the three Manifestation Materials names are complete. Six-node topology, native status/costs and green/crimson/yellow states are retained. The focused session stopped at sequence 17 with restoration and fixture cleanup verified. Controlled exact cargo increase, capability purchases, chip learning and recycling transactions remain separate checks. Session 005's map/queue observations and the original launch-context failure remain recorded below under their actual scope.

Pinned candidate: `347de7db77f9121ad26e1c12c534b97760ee92f2`, assembly version `1.1.9.0`. Native compilation and complete package verification passed. Exact-candidate headed session 005 verified the revised map controls, raid Call-ins headings, and native custom queue submission/completion. Its exact four-unit cargo delta remains unverified. The bounded corrected-directory session-006 result is recorded above. Source links are pinned so later work on `main` is not mistaken for this candidate.

The cumulative change-note baseline is the published [v1.1.0 release](https://github.com/mrcalzon02/ImperiumOfMan/releases/tag/v1.1.0), whose annotated tag resolves to `b08c27e38bbc73712e073ec9141a6e8769984d9a`. The source comparison establishes which changes are new; compile/package evidence and actual gameplay observations establish different validation stages. No Steam publication of this candidate is recorded by this review.

## Requested interface

The map belongs inside the mod. A narrow **Crafting map** tab appears on the left edge while Production or its item-selection window is open. Expanding the tab reveals a sage-green graph panel; its right-edge handle collapses it. The canvas supports mouse-wheel zoom, click-drag panning, and click-to-highlight selection. Details remain below the graph. Single-click selection preserves the current view; an explicit focus action can recenter a branch.

The entry hooks are in [Plugin.CraftingTreeEntry.cs](https://github.com/mrcalzon02/ImperiumOfMan/blob/347de7db77f9121ad26e1c12c534b97760ee92f2/src/Plugin.CraftingTreeEntry.cs). They target the concrete private `OnEnable()` methods of `MagnumProductionWindow` and `MagnumSelectItemToProduceWindow` in the installed game. An owner-bound companion observes deactivation/destruction and removes only that owner's tab. The earlier proposed ship-screen button was removed before publication.

## Loaded game data

[Plugin.CraftingIndex.cs](https://github.com/mrcalzon02/ImperiumOfMan/blob/347de7db77f9121ad26e1c12c534b97760ee92f2/src/Plugin.CraftingIndex.cs) builds an on-demand read-only snapshot from the registered item catalogue, Magnum production, workbench recipes, station industry, native disassembly, and mod shredding profiles. It preserves all item IDs, including items without producers, alternate recipes, and station cooutputs. It uses current modified-project output IDs when native Magnum production would do so.

Registered costs are already the effective costs; the index does not halve them again. Custom Magnum output batches come from `IndustryBatches`; unchanged native Magnum orders produce one unit. Registered production hours are base hours. Unknown workbench yields and times remain unknown. Native chance-based salvage and explicit ship recycling are separate routes.

The pinned revision uses each explicit recycling route's input batch size. A batch larger than one is described as combining eligible selected stacks, consuming complete batches, and retaining smaller remainders. This is separate from native per-item disassembly. Dirty Corpse Starch now has a registered eight-rations-to-one-biomass route; the map must display eight input units. Its native recycle control also previews exact selected returns and retained rations. The registration and UI still need actual-game acceptance for this candidate.

Learned recipe state, cargo ingredients, and current station listings are captured when the map opens or Refresh is requested. A recipe with sufficient cargo is not a promise that a production line is available. The graph does not create recipes, grant knowledge, queue orders, alter item definitions, or save its index as campaign state.

The 1.1.9 index reports occult fabrication capability separately from learned recipe knowledge. It identifies physical pattern-chip sources where registered, reports unavailable progression state explicitly, and does not treat buying research as learning a recipe. The candidate's six-capability progression and native picker gates differ from the earlier 1.1.6 build and need their own live checks.

### Coverage diagnostic and recovery scope

The startup [coverage diagnostic](https://github.com/mrcalzon02/ImperiumOfMan/blob/347de7db77f9121ad26e1c12c534b97760ee92f2/src/Plugin.CraftingCoverage.cs) computes ingredient reachability over Magnum recipes and then Magnum plus workbench recipes. It omits recycling/disassembly and station production, and does not evaluate quantities, time, knowledge, or capability. Its three reported custom-output warnings for `bone_knife`, `impaler`, and `skrivnus_knife` are limited to that graph.

The [occult production source](https://github.com/mrcalzon02/ImperiumOfMan/blob/347de7db77f9121ad26e1c12c534b97760ee92f2/src/Plugin.OccultProduction.cs) registers a separate Raw Clone recycling route returning four `human_meat`, two `human_skin`, and one `human_rib` per clone when all three native records exist. Each of those three warned recipes consumes ribs. The live Crafting map imports `ShredProfiles` and displays this route under **Recovery**, providing a source omitted by the startup diagnostic. Those warnings are therefore not proof of global unreachability, nor is their explanation a completed-production pass. Recipe knowledge, capability purchases, recycler access, and the rest of each ingredient chain still need exact-game acceptance.

## Interface implementation

[Plugin.CraftingTreeUi.cs](https://github.com/mrcalzon02/ImperiumOfMan/blob/347de7db77f9121ad26e1c12c534b97760ee92f2/src/Plugin.CraftingTreeUi.cs) contains the native Unity UI. It uses actual item sprites and muted sage colors, a connected graph canvas, compact search/venue controls, and a lower information panel. Catalogue and inventory scans occur only on opening or explicit Refresh. Lifecycle and input handling keep the map associated with Production and return control when it is collapsed or closed.

The revised graph renders quantities only on connections incident to the selected item or process, preserving every graph connection and the full detail values. Both lower detail columns now use an auto-hiding vertical scrollbar with a reserved gutter. These refinements respond to the overlapping counts and unclear scrolling affordance observed in the earlier headed session. The native occult tooltip also shortens Recipes and splits the Gannix/Shartamakhum family row to fit its width.

The expanded map intercepts `MGSC.UI.Process(out bool interruptProcessing)` and reports that input was consumed. Current `SpaceGameMode.Update` ticks the game before calling that method, so the map blocks native navigation and hotkeys without replacing the game loop. The block persists through the closing animation and its final frame.

## Exact-candidate validation status


- Native compilation passed for request `crafting-map-visual-diagnostic-001`, [run 45838490203](https://github.com/mrcalzon02/Dens-Mortis/actions/runs/45838490203), job `139973573585`, source `347de7db77f9121ad26e1c12c534b97760ee92f2`, controller `b42126b92be056ecd6ea1da1cce542d55b8c16e7`: zero warnings, zero errors, 15.17 seconds. Both installed-game assembly hashes match the inspected current references.
- Complete package verification passed at immutable private delivery `41f649f324759041d881d9f2bb45e911c6d4036c`: all 30 chunk lengths/hashes/Git blobs, combined ZIP length/SHA-256/Git blob, CRC, exact four members and their hashes, and manifest identity. DLL metadata confirmed `1.1.9.0`, the mission diagnostic target, and the Pact/Map/picker/migration hooks. [Machine-readable receipt](CraftingMapBuildReceipt.json)
- Package `ImperiumOfMan-1.1.9-CraftingMap.zip`: 11,796,267 bytes; SHA-256 `4a096861fa4315820e3391fdc948cc704b01301d502b2c1897056feb05825413`; immutable public Git blob `5ef0e238445463fd370774cb4f8ae575b9026941`. Its public readback matched the verified archive byte for byte.
- Exact-candidate headed session [005](https://github.com/mrcalzon02/Dens-Mortis/actions/runs/45841032741), controller `11277b5f0a23c7e3239396c381f54924101a08f6`, exercised the same verified source and ZIP. Targeted results are below. Session 004 stopped during a candidate-download interruption before game launch or fixture creation; it supplies no gameplay result.
- Independent capability/knowledge transactions, exact inventory gain, cancellation/refunds, recovery, active-order save/load, call-in/controller use, turret deployment, and wider gameplay retain separate acceptance status.

### Exact source 347de7d: headed session 005

The final acceptance ledger, including the subsequent loader audit, is pinned by SHA-256 `d2a360b013fa3574cbeff8d4d82b8da61756f104c959eacb9912c338a5cfa496`. The operator’s observations and independently checked screenshot/log transports establish:

| Check | Observed result | Status |
|---|---|---|
| Startup and copied campaign | Seven intended organic destinations; Ron Mask pact component preserved; slot 2 loaded in 4.608027 seconds | Passed |
| Production entry and catalogue | Left sage tab in Production and picker; 2,363 items and 3,017 loaded recipes | Passed |
| Selected quantities | Only incident quantities shown for the selected item/process; full graph connections and detailed four-output recipe retained | Passed |
| Detail lists | Both overflow bars visible; drags reached the last producing/usage rows; wheel input affected only its list; both bars hid for short lists | Passed |
| Fit | Entire 19-node feedstock graph centered at 48% zoom | Passed |
| Ownership and closing | Escape closed only the map; picker retained its tab; leaving Production removed the tab | Passed |
| Raid Call-ins headings | Collapsed label and expanded Weapons, Pod Support, and No pods installed text fit at 1920×1080 | Passed |
| Spacecraft Drop Support heading | Separate module-header comparison | Not run |
| Custom production queue | Explicit quantity one queued four feedstock outputs at one displayed hour; later Conveyor 0/1 after native time advancement | Passed |
| Exact cargo increase | Post-test Arsenal tab 1 showed a matching-icon stack of six, without a before-total or item-name confirmation | Unverified |

The map’s three base hours and the native queue’s one hour describe different values: the native queue applies the current campaign’s production modifiers. Frame 32 shows x4 and Conveyor 1/1; frame 37 shows Conveyor 0/1; frame 39 shows the post-test matching stack. Queue completion is observed. The exact item identity and aggregate +4 inventory delta are not established by that icon alone. Historical source `bdb1759` separately supplies the search, pan, and graph-zoom observations below.

Cleanup passed: operator stop sequence 42 restored the original package, preserved all 11 protected original campaign files, and archived/removed the six remaining owned fixture files with no leftovers. Immutable final-status snapshot: `2eec3e6597f6049d47bcd63d94501338f0c072ed`; status Git blob `f7f29318586518d933fb45dd85bb5dd6dd686f9f`, SHA-256 `e300acffc33ed7a670832a52e86084155f815b513b810515a1c5cd6844a20c69`. [Detailed headed record](CraftingMapHeadedValidation.md)

### Session 005 mission failure and subsequent loader diagnosis

Session 005’s first diagnostic identified native station `Patton`, template `cerera_cereraMine_annaherman`, and requested folder `CereraSurface`. Fourteen diagnostic identities across native and mod stations reported an empty global catalogue (`total=0`); native `ArgumentNullException` stacks followed and recurred during Space time advancement.

The fixed read-only [preset-path audit 45849018302](https://github.com/mrcalzon02/Dens-Mortis/actions/runs/45849018302) completed successfully on Core30. Immutable delivery `bee68f710da80411898ca20206f4489101532a8e`, prefix `runner-results/quasimorph-preset-path/45849018302-1`, contains full `Data`, `PresetIdRegistry`, `PresetNamespaceInfo`, and `PresetAuthoringTarget` definitions and the complete `Data.LoadBinaryPresetsMap` body. Before/after assembly hashes matched; all export hashes and completeness checks passed. Retained current `FileManager` evidence from delivery `41f649f324759041d881d9f2bb45e911c6d4036c` confirms that `applyPersistentDataPath: false` uses the supplied path unchanged.

The core loader reads relative `GameData/BinaryPresetsMap.json` and silently falls back to an empty map when it is absent. The session-005 launcher used its newly created evidence directory as the process working directory, explaining the empty catalogue and subsequent null lists. Existing private launcher correction `366b2d35c4f62a5ca7ac10c0f0577d9eb0054397` changes that working directory to the game installation. The diagnosed mechanism is established. Session 006 subsequently passed the corrected-directory Space/save and bounded game-time check described above. The mod diagnostic remains observational; remaining transaction cases are not covered by that result.

### Historical source bdb1759: actual map execution

[Headed run 45825014723](https://github.com/mrcalzon02/Dens-Mortis/actions/runs/45825014723) tested `bdb17590322a8f27090918f83e0bf613eaee04de` and the earlier ZIP `3e958daebacf3cee4fe7f503bc453d2dbc773bddb7a619b292a376f5d37b9f23`. It confirmed the seven intended organic targets, successful copied-campaign loading with the Ron Mask pact component retained, the left Production/picker tab, sage layout, search, selection, zoom, pan, and Escape. The live index contained 2,363 items and 3,017 routes. These are observed map results, not only source inspection.

Live process details showed: metal feedstock with no inventory inputs, four outputs, three base hours; Lasgun with one plate, one fitting, and one weapon part producing two outputs in twelve base hours; Raw Clone with no inventory inputs, two outputs, eight base hours, and separate missing Culture/pattern requirements. An ordinary native medical order was submitted at quantity one, then displayed one item, one hour, and Conveyor 1/1. This confirms ordinary queue compatibility; it does not establish custom batch completion, recovery returns, or refunds. Fit/Focus, leaving Production entirely, the new scrollbar interaction, and the prior Drop Support header comparison were not completed. [Full evidence and cleanup](CraftingMapHeadedValidation.md)

The session also exposed a mission-generation `ArgumentNullException` after returning to Space. Installed API [run 45831181826](https://github.com/mrcalzon02/Dens-Mortis/actions/runs/45831181826), delivery `45f38e0ed4a9c6f08e7981699f0d1c82240c674c`, verified full mission definitions and complete IL against unchanged `Assembly-CSharp.dll`. The statement near reported offset `0x001f2` passes `presets.Elevators` to `AddRange` at IL `0x0204`; missing preset lookup returns a default value with null lists. The later exact-candidate diagnostic and preset-loader audit above establish the test launcher’s working-directory cause; the bounded corrected-launch confirmation is now recorded in session 006 above.

The earlier candidate `585715f554181ed56220c5ba2ed9055b46b44b38` passed native/package verification and startup but failed during campaign reconstruction because native project cloning rejected `ron_mask_1`'s pact component. Its log also exposed overly broad organic recycling targets. Production and the map were not reached in that earlier session.

Current installed-game API captures established the clone path and inherited mutable fields. Five project types were captured in run `45817562241`; the sixth requested type, `MGSC.ItemTransformationRecord`, is absent. The separate three-base-type capture in run `45819885607` passed. Both captures used the same unchanged game assembly and performed no game launch or save access. [Compatibility evidence](OccultProjectCompatibility.md)

Compilation, package integrity, startup, campaign loading, and interaction acceptance remain separate stages.

## Earlier 1.1.6 evidence

The following evidence belongs to source `40939a398f1a2d3f79ec9ece08cfe5170444d885`. It records the original entry-hook/API corrections and earlier index checks. It does not verify the revised quantity labels or detail scrollbars, nor the modified 1.1.9 index, occult gates, picker hooks, artwork, call-in drawer, tooltip work, or turret progression.

- Focused compile/behavior checks of the earlier index source passed against API-shaped fixtures: complete item discovery, alternatives and cycles, cooutputs, effective batches, learned/material state, station listings, modified projects, salvage distinctions, unknown workbench values, explicit refresh, and partial adapter failures.
- Installed-game API inspection corrected obsolete bundled `Show`/`Hide` and `InputController.GetInput` assumptions. The inspected production views use `OnEnable`, and the native input boundary is `UI.Process`. Inspected `Assembly-CSharp.dll` SHA-256: `d72cd91f98de4d6cce8ee420ac1a906d89e68ba0cb164694d266b56f46339ebe`.
- Native build [45798510569](https://github.com/mrcalzon02/Dens-Mortis/actions/runs/45798510569), job `139838254040`, passed on `OLD-CORE30-homebrew` with zero warnings and zero errors. The project explicitly references `UnityEngine.UIModule.dll`; the DLL contains the native hooks and owner cleanup.
- All 30 transport chunks, ZIP checksum and CRC, exact four members, member hashes, manifest identity, and DLL metadata passed verification. Historical package: `ImperiumOfMan-1.1.6-CraftingMap.zip`, 11,763,638 bytes, SHA-256 `0a8b5e397896fc3597a9b9f7da5185fc11b5b9eb7d29be4d8d87b2ba269c3db3`.
- The unchanged candidate launched during [headed run 45801114949](https://github.com/mrcalzon02/Dens-Mortis/actions/runs/45801114949), using Quasimorph `1.0.4.590s.969afc6` and Unity `2022.3.62f2`. Actual title, main-menu, and campaign-selection captures were inspected at 1920×1080, and the log confirmed mod registration completed. Those observations do not establish Production/map interaction acceptance.
- A fresh slot-1 campaign reached the locked Conveyor requirement, displayed as `1 (0)`. Production and the Crafting map were not reached, so this session supplies no map visual or interaction pass. Final cleanup confirmed the original runtime package was restored and all seven pre-existing slot-0 campaign files remained unchanged.

These are distinct verification stages. Native compilation and actual startup do not establish that the crafting window or every production chain has been exercised.

## Focused in-game acceptance

Open Production and its recipe picker; the tab must appear in both and disappear when leaving Production. Expand/collapse it, zoom around the pointer, drag the canvas, select nodes without losing the view, and inspect lower-panel ingredients, output, time, and availability. Confirm native hotkeys do not act through an expanded map, and Escape returns to Production without closing the underlying screen in the same frame. Search for native and mod items, including a no-producer item; inspect an alternate recipe and a station cooutput. Confirm the interface remains usable at the player's actual resolution.

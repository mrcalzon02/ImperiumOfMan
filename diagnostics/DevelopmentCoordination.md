# Development reconciliation — 26 November 2026

The concurrent **Fix Icon Borders** recovery and **Prepare Change Notes / Crafting map appearance** work was reconciled in [2db1cebb69b4086a7dfce80816b2f83d482cfbd6](https://github.com/mrcalzon02/ImperiumOfMan/commit/2db1cebb69b4086a7dfce80816b2f83d482cfbd6). The active continuation is a **verified 1.1.11 native test package; runtime acceptance pending**, compiled from [b89e15990b42d8bc4911237d5d1502fe0c283d99](https://github.com/mrcalzon02/ImperiumOfMan/commit/b89e15990b42d8bc4911237d5d1502fe0c283d99), tree `01fc2ceefdbc865442b932d59f0502f87e227b5e`. Its native build and package checks passed, and the test ZIP was published in [2c5f6dc35a6d41c63440a8c78c7dc01f31e826f3](https://github.com/mrcalzon02/ImperiumOfMan/commit/2c5f6dc35a6d41c63440a8c78c7dc01f31e826f3). Headed acceptance remains pending.

## What overlapped

Both publication passes started from `186e2b76464b91888101657caec5e5cd531c80bb`, the published, verified 1.1.10 package. The Crafting map pass reached `main` as `e6c3026b3a11e4194b4597866099fcee850d836a`. The icon recovery had a separate, clean local publication commit, `00893686c5871d168964780a3f3c03dded36b22c`, that had not reached GitHub.

The recovered icon commit changed 19 documentation, export, archive and packaging-helper files. It contained no runtime C# or new image-asset implementation. Its only shared paths with the map commit were:

- `README.md`
- `package/README.md`
- `docs/crafting/CHANGE_NOTES_1.1.10.txt`

The completed integration preserved the other 16 icon-publication files byte for byte and reconciled those three text files. It retained the working direct PNG links, separated current source from the last verified runtime package, and moved the complete 1.1.9 package history into `package/README-1.1.9.md`.

The older dirty occult checkout supplies no additional runtime code, icon artwork or unique correction. Its unmatched text predates the session-007 named-stack evidence and corrected game launch directory; copying it over the newer records would regress those findings.

## Source preservation

The original `e6c3026` Crafting map commit changed only its UI file, the TMP project reference, assembly version and four delivery documents. Every other existing Git blob from its parent was unchanged. The reconciliation retained:

| Work | Preserved source or publication |
| --- | --- |
| Six occult capabilities and artwork | `ceba30c2292be3975748cdfb750c287e20eff78d`, published in `64345956c693d2ca763282c910a5725878479d64` |
| Finished-ration correction | `5fae43b49ff9fe4ee21cb88a3e8e30eb8e3558ad` |
| Native upgrade tooltips and width correction | `11ed2b0c134683021b5a228161e0db8c1b16650d`, `8563a18c50f63b8e6d54d51d0db8d79c8b7fa104` |
| Call-in drawer, input guard and icon lookup | `1be4c891ec8642fb36377aae80f37cb87a821f2b`, `e123a4b149f36c006667827ba0f65844130b2ec1`, `d2da0db3a4b794cdfa25ae8369c3ef098e0d40ac` |
| Independent Comms turret progressions | `b9a79b05895ac7127ab4ce9fbace1aa59a993375` |

The reconciliation carried the recovered publication forward on `main`; it did not restore an old checkout over current source or recreate the removed ration recycling UI.

## Version and evidence boundaries

| Deliverable | Exact version and source | Current evidence boundary |
| --- | --- | --- |
| Current native test package | 1.1.11.0, `b89e15990b42d8bc4911237d5d1502fe0c283d99` | Native build and package verification passed; published in `2c5f6dc`. [Build receipt](https://github.com/mrcalzon02/ImperiumOfMan/blob/2c5f6dc35a6d41c63440a8c78c7dc01f31e826f3/diagnostics/CraftingMapBuildReceipt-1.1.11.json). Session-009 runtime acceptance pending. |
| Earlier appearance baseline | 1.1.11.0, `e6c3026b3a11e4194b4597866099fcee850d836a` | Native build [45878912836](https://github.com/mrcalzon02/Dens-Mortis/actions/runs/45878912836) and its package passed. Session 008 loaded and reviewed it, identified blank compact buttons, then restored the original package and protected campaign files. This superseded baseline does not validate the expanded candidate. [Receipt](CraftingMapNativeStyleBaselineReceipt.json). |
| Historical corrected runtime | 1.1.10.0, `5fae43b49ff9fe4ee21cb88a3e8e30eb8e3558ad` | [ZIP and receipt](../package/README.md) verified; this binary had no separate headed session. |
| Recovered source/art review | 1.1.10.0, `5fae43b49ff9fe4ee21cb88a3e8e30eb8e3558ad` | 235 entries; 233 manifest-listed files verified; all 140 included build-source files match this revision. It intentionally predates the 1.1.11 appearance changes. |
| Earlier live-game evidence | 1.1.9.0, `347de7db77f9121ad26e1c12c534b97760ee92f2` | Sessions 005–007 retain their source attribution. Session 007 established the named feedstock stack changing from 2/30 to 6/30 after one four-output batch. |

The recovered source/art ZIP is 17,105,316 bytes, SHA-256 `3d7adb09bd1b7b849260712c94bc05687ecb50fdac55ebef894e361420aa9ede`. Its manifest separates compiled source from the later documentation baseline. The packaging helper deliberately refuses to label modified 1.1.11 source as a 1.1.10 review archive.

## Current candidate scope

- Native language font and menu framing, compact-button text correction, separated processing-family branches, clearer connections, and recipe icons.
- Live registered-recipe tracing across supported game/mod tables, retaining alternatives, shared inputs and coproducts. Unknown yields remain unknown; private script-only systems are outside the adapter's coverage. [Coverage and resolver checks](CraftingDependencyIndex.md).
- Seven distinct chemistry/manufacturing designs, imported as 21 inventory, floor and shadow sprites.
- Ceramic feedstock and Electronic substrate with two minimally changed Magnum consumers. Source balance checks preserve expanded material cost and time over two orders. First orders from unprepared stock take six/five base hours and leave one spare intermediate. [Exact quantities and limits](ManufacturingIntermediates.md).
- Paid Ore Cargo top-ups of 3 at Rachmaninoff, 2 at Cinder Basin Mine and 2 at Foundry Primaris under the existing owner and Tech 1 gates. Rachmaninoff and Cinder require CoreWard ownership, so Cinder stops receiving the top-up after capture; Foundry Primaris remains Imperial. Production and trade can consume stock. Native API inspection confirms the storage path; actual listing and purchase remain a headed check. [Source audit](OreCargoMarketSourceAudit.md).

## Build complete; finish runtime acceptance

[Native run 45886658790](https://github.com/mrcalzon02/Dens-Mortis/actions/runs/45886658790) compiled exact source `b89e159` with zero warnings/errors in 9.68 seconds. Delivery `119cfe8c7c47a4d01d0263c203451fb64778a208` supplied 31 verified transport chunks. The reconstructed ZIP is 11,819,417 bytes, SHA-256 `db5e72f9202d8303fdacf467af04fb7f9d59412fd5ea05d41652eb9df9bdd539`; all four members, hashes, assembly version, TMP references and new compiled methods are verified. [Published receipt](https://github.com/mrcalzon02/ImperiumOfMan/blob/2c5f6dc35a6d41c63440a8c78c7dc01f31e826f3/diagnostics/CraftingMapBuildReceipt-1.1.11.json).

[Headed session 009](https://github.com/mrcalzon02/Dens-Mortis/actions/runs/45887627504) uses controller `5e8a70f7d2186cb2720842c74ee9af0e3cad04af` and the exact b89 package. It is queued behind another job on the shared native machine. Runtime acceptance has not yet been established.

1. Continue the existing session-009 request under the coordinator's sole GUI control; avoid a duplicate native request.
2. Check compact controls, family spacing/routing, ingredient tracing, recipe/item icons, search/filter navigation, detail scrolling and Production ownership. Confirm the new material registrations and an actual paid Ore Cargo purchase. Record any unperformed checks explicitly.
3. Restore the original package and protected campaigns and remove the fixture.
4. Add current screenshots, observed acceptance and cleanup evidence to the delivery notes. Keep the already-published test ZIP and its exact build identity separate from the pending runtime result.

## Queue for later focused work

These items are not silently included in the appearance result:

- Research purchases and independent pattern-chip learning, including capability-versus-knowledge gating.
- Actual recycling completion, clone harvest yields, protected inputs and the corrected forward ration transaction.
- Legacy occult-purchase migration and the remaining research/icon states.
- Call-in charge consumption and independent turret deployment transaction checks.
- Quantity planning, automatic queue submission, complete runtime exports and remaining otherworldly equipment-family coverage.

Completed source-347 observations remain usable within their stated scope; they do not become new-version transaction acceptance.

## Working arrangement for this continuation

Use current `main` as the integration point. Read the latest head before changing shared publication files, preserve unrelated paths, update the branch with the expected previous SHA, and read the result back. Keep the source SHA, assembly version, ZIP digest and headed-session source together in every delivery record.

The coordinator owns market stock changes, asset generation/import, package publication and all headed input. Separate owners handle graph/UI geometry, runtime indexing, and manufacturing/art registration plus delivery documentation. No second process should replace the native request or send actions to the active game session. No existing automation or unrelated queued project is disabled by this reconciliation.

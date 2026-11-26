# Development reconciliation — 26 November 2026

This record reconciles the concurrent **Fix Icon Borders** recovery and **Prepare Change Notes / Crafting map appearance** work. The immediate continuation is to publish both sets of deliverables and finish the already-started 1.1.11 native appearance check.

## What overlapped

Both publication passes started from `186e2b76464b91888101657caec5e5cd531c80bb`, the published, verified 1.1.10 package. The Crafting map pass reached `main` as `e6c3026b3a11e4194b4597866099fcee850d836a`. The icon recovery had a separate, clean local publication commit, `00893686c5871d168964780a3f3c03dded36b22c`, that had not reached GitHub.

The unpublished icon commit changes 19 documentation, export, archive and packaging-helper files. It contains no runtime C# or new image-asset implementation. Its only shared paths with the map commit are:

- `README.md`
- `package/README.md`
- `docs/crafting/CHANGE_NOTES_1.1.10.txt`

The integration preserves the other 16 icon-publication files byte for byte and reconciles those three text files. It retains the working direct PNG links, separates current source from the last verified runtime package, and moves the complete 1.1.9 package history into `package/README-1.1.9.md`.

The older dirty occult checkout supplies no additional runtime code, icon artwork or unique correction. Its unmatched text predates the session-007 named-stack evidence and corrected game launch directory; copying it over the newer records would regress those findings.

## Source preservation

The Crafting map commit changes only its UI file, the TMP project reference, assembly version and four delivery documents. Every other existing Git blob from its parent is unchanged. In particular, the integrated source retains:

| Work | Preserved source or publication |
| --- | --- |
| Six occult capabilities and artwork | `ceba30c2292be3975748cdfb750c287e20eff78d`, published in `64345956c693d2ca763282c910a5725878479d64` |
| Finished-ration correction | `5fae43b49ff9fe4ee21cb88a3e8e30eb8e3558ad` |
| Native upgrade tooltips and width correction | `11ed2b0c134683021b5a228161e0db8c1b16650d`, `8563a18c50f63b8e6d54d51d0db8d79c8b7fa104` |
| Call-in drawer, input guard and icon lookup | `1be4c891ec8642fb36377aae80f37cb87a821f2b`, `e123a4b149f36c006667827ba0f65844130b2ec1`, `d2da0db3a4b794cdfa25ae8369c3ef098e0d40ac` |
| Independent Comms turret progressions | `b9a79b05895ac7127ab4ce9fbace1aa59a993375` |

There is one remote branch, `main`. The reconciliation carries the unpublished publication forward; it does not restore an old checkout over current source or recreate the removed ration recycling UI.

## Version and evidence boundaries

| Deliverable | Exact version and source | State at reconciliation |
| --- | --- | --- |
| Current Crafting map implementation | 1.1.11.0, `e6c3026b3a11e4194b4597866099fcee850d836a` | Native build [45878912836](https://github.com/mrcalzon02/Dens-Mortis/actions/runs/45878912836) completed successfully; candidate verification and headed comparison are the active continuation. |
| Published corrected runtime | 1.1.10.0, `5fae43b49ff9fe4ee21cb88a3e8e30eb8e3558ad` | [ZIP and receipt](../package/README.md) verified; this binary had no separate headed session. |
| Recovered source/art review | 1.1.10.0, `5fae43b49ff9fe4ee21cb88a3e8e30eb8e3558ad` | 235 entries; 233 manifest-listed files verified; all 140 included build-source files match this revision. It intentionally predates the 1.1.11 appearance changes. |
| Earlier live-game evidence | 1.1.9.0, `347de7db77f9121ad26e1c12c534b97760ee92f2` | Sessions 005–007 retain their source attribution. Session 007 established the named feedstock stack changing from 2/30 to 6/30 after one four-output batch. |

The recovered source/art ZIP is 17,105,316 bytes, SHA-256 `3d7adb09bd1b7b849260712c94bc05687ecb50fdac55ebef894e361420aa9ede`. Its manifest separates compiled source from the later documentation baseline. The packaging helper deliberately refuses to label modified 1.1.11 source as a 1.1.10 review archive.

## Resume now

1. Complete the icon publication integration and read back the resulting `main` commit and artifacts.
2. Finish verification of the single existing 1.1.11 native build. The documentation integration does not change its runtime source or embedded assets, so it does not require a duplicate build.
3. Run one observed headed session for the new map appearance, search selection, zoom/pan, detail lists, source filters and Production ownership. Capture the actual game and restore the original package and protected campaigns when finished.
4. Publish that exact candidate, current screenshots and concise change notes with explicit download links and evidence provenance.

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

The coordinator for this continuation owns package publication and headed input. Other work can prepare isolated source/document changes, but a second process should not replace the native request or send actions to the active game session. No existing automation or unrelated queued project is disabled by this reconciliation.

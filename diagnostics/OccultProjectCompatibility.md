# Native equipment projects with pact components

## Observed load failure

Source `585715f554181ed56220c5ba2ed9055b46b44b38` registered all 53 occult formulas, but loading a disposable campaign copy in [headed run 45815345319](https://github.com/mrcalzon02/Dens-Mortis/actions/runs/45815345319) failed during `MagnumProjects.OnAfterLoad`. Native `MagnumDevelopmentSystem.InjectItemRecord` rejected the supplemental `PactComponentRecord` attached to `ron_mask_1` and returned `FailedReadSaveData` before entering Space or Dungeon.

The native injector also constructs default equipment variants for production outputs absent from the saved project list. Removing saved project data would therefore not address this recipe-triggered failure. No saved project data was changed.

## Current native evidence

[Project-clone API audit 45817562241](https://github.com/mrcalzon02/Dens-Mortis/actions/runs/45817562241) captured the current `MagnumDevelopmentSystem`, `MagnumProjects`, `MagnumProject`, `PactComponentRecord`, and `CompositeItemRecord` types. Their lengths and SHA-256 digests were verified against the immutable receipt at private delivery `82f5f270d6a4f5c36d984d990fb2f8709c5bb46f`, prefix `runner-results/quasimorph-project-clone/45817562241-1`.

The audit job reports a partial result because a sixth requested type, `MGSC.ItemTransformationRecord`, does not exist in this installed assembly. The five types used for this fix were captured successfully. The installed game assembly SHA-256 is `d72cd91f98de4d6cce8ee420ac1a906d89e68ba0cb164694d266b56f46339ebe`.

## Compatibility change

`Plugin.OccultProjectCompatibility.cs` replaces the single record-list getter inside native `InjectItemRecord`. For this mod's newly registered occult recipes with supported equipment and an exact `PactComponentRecord` supplement, it supplies a filtered list to the native equipment-cloning loop. It never removes records from the source composite.

Native weapon, armor, helmet, leggings and boots cloning, localization, custom-record replacement, and `ApplyModifications` remain in the original method. After that method succeeds, the postfix checks Harmony's `__runOriginal` flag and recipe ownership before it appends an independent pact record under the same `_custom` item ID. Native equipment remains the first record and therefore retains its descriptor and applied upgrades. The pact's essence, power, stability, stack size and inherited values are retained; its categories and disassembly quantities are copied independently.

Ordinary items and recipes not added by this occult module retain their original record-list object. A native method skipped by another prefix causes no postfix mutation. Successful pact attachment writes a bounded source/custom-ID diagnostic for headed verification. Unsupported supplemental types still reach the native exception path. A pact record supplied by another compatible patch is preserved. The transpiler requires exactly one matching getter in the native method, so an incompatible future method shape is reported explicitly.

All 53 occult formulas and the existing physical chip pools remain present. This fix changes no research purchase, recipe knowledge, or saved project collection.

## Verification status

Current source `347de7db77f9121ad26e1c12c534b97760ee92f2` passed native build [45838490203](https://github.com/mrcalzon02/Dens-Mortis/actions/runs/45838490203) with zero warnings and zero errors. Exact-source [session 005](https://github.com/mrcalzon02/Dens-Mortis/actions/runs/45841032741) again preserved the Ron Mask pact component and loaded the disposable campaign. The [current receipt](OccultBuildReceipt.json) records this bounded pass. The subsequent simulation issue involved an empty global preset catalogue; the relative loader path and silent fallback have been verified. Corrected-directory session 006 completed Space/save and 46 hours 20 minutes of game time with no missing-preset diagnostics or managed-exception headings in the complete log, then restored the original package and saves. Specific equipment modification and occult transactions remain separate checks.

### Historical bdb confirmation

Source review matched the single getter site and all five native clone/modification branches to the current exported method. Source `bdb17590322a8f27090918f83e0bf613eaee04de` passed native build [45822897943](https://github.com/mrcalzon02/Dens-Mortis/actions/runs/45822897943) with zero warnings and zero errors.

The historical bdb candidate then loaded disposable campaign slot 2 successfully into Dungeon in [headed run 45824345407](https://github.com/mrcalzon02/Dens-Mortis/actions/runs/45824345407). Immutable delivery `8978febd64c670f2a22a8ccaff36054545c047a7` records `Preserved project pact components: ron_mask_1 -> ron_mask_1_custom (1)`, successful slot loading and completed autosave; frame 0006 shows dungeon gameplay. The earlier pact-cloning exception did not recur in that reviewed load. Specific equipment modification, research UI, production and recycling interactions remain separate pending checks; this load result is not a complete gameplay pass.

After the later Space transition, faction simulation raised repeated `MissionFactory.GenerateProceduralPlan` / `List.AddRange` `ArgumentNullException` errors. The first reviewed bdb log contains 15 occurrences. Later source-347 diagnostics identify an empty global preset catalogue; the loader audit and successful bounded corrected-directory session-006 check are documented in the current receipt. This later failure does not erase the observed pact-component preservation and Dungeon-load result, and it prevents a claim that the candidate has passed complete runtime validation.

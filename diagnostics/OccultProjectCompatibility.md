# Native equipment projects with pact components

## Observed load failure

Source `585715f554181ed56220c5ba2ed9055b46b44b38` registered all 53 occult formulas, but loading a disposable campaign copy in [headed run 45815345319](https://github.com/mrcalzon02/Dens-Mortis/actions/runs/45815345319) failed during `MagnumProjects.OnAfterLoad`. Native `MagnumDevelopmentSystem.InjectItemRecord` rejected the supplemental `PactComponentRecord` attached to `ron_mask_1` and returned `FailedReadSaveData` before entering Space or Dungeon.

The native injector also constructs default equipment variants for production outputs absent from the saved project list. Removing saved project data would therefore not address this recipe-triggered failure. No saved project data was changed.

## Current native evidence

[Project-clone API audit 45817562241](https://github.com/mrcalzon02/Dens-Mortis/actions/runs/45817562241) captured the current `MagnumDevelopmentSystem`, `MagnumProjects`, `MagnumProject`, `PactComponentRecord`, and `CompositeItemRecord` types. Their lengths and SHA-256 digests were verified against the immutable receipt at private delivery `82f5f270d6a4f5c36d984d990fb2f8709c5bb46f`, prefix `runner-results/quasimorph-project-clone/45817562241-1`.

The audit job reports a partial result because a sixth requested type, `MGSC.ItemTransformationRecord`, does not exist in this installed assembly. The five types used for this fix were captured successfully. The installed game assembly SHA-256 is `d72cd91f98de4d6cce8ee420ac1a906d89e68ba0cb164694d266b56f46339ebe`.

## Compatibility change

`Plugin.OccultProjectCompatibility.cs` replaces the single record-list getter inside native `InjectItemRecord`. For supported equipment with an exact `PactComponentRecord` supplement, it supplies a filtered list to the native equipment-cloning loop. It never removes records from the source composite.

Native weapon, armor, helmet, leggings and boots cloning, localization, custom-record replacement, and `ApplyModifications` remain in the original method. After that method succeeds, the postfix appends an independent pact record under the same `_custom` item ID. Native equipment remains the first record and therefore retains its descriptor and applied upgrades. The pact's essence, power, stability, stack size and inherited values are retained; its categories and disassembly quantities are copied independently.

Ordinary items retain their original record-list object. Unsupported supplemental types still reach the native exception path. A pact record supplied by another compatible patch is preserved. The transpiler requires exactly one matching getter in the native method, so an incompatible future method shape is reported explicitly.

All 53 occult formulas and the existing physical chip pools remain present. This fix changes no research purchase, recipe knowledge, or saved project collection.

## Verification status

Source review matched the single getter site and all five native clone/modification branches to the current exported method. Native compilation, Harmony application, campaign loading and visual interaction for the combined correction remain the next validation gates. Prior startup success does not establish those results.

# Bound Ash inventory correction — 2026-11-25

Requested behavior: one tile visually, one occupied inventory tile, and stacking.

## Correction

- `RegisterChaosPacts` explicitly sets Bound Ash's `InventoryWidthSize` to 1 and its base `MaxStack` to 30.
- `ApplyChaosItemArtwork` caps Bound Ash's inventory icon at 22 UI pixels using the existing native-size helper.
- A Bound-Ash-only `PickupItem.OnAfterLoad` postfix corrects the saved instance width and queues its stack-limit repair.
- Separate save, space, and dungeon load hooks process that queue once the item factory and difficulty are ready. They use native `GetMaxStackSize`, preserve the existing count, and attach the native stack component if an older item lacks it.
- The queue clears after processing. No update-loop or inventory-scanning hook was added.

The donor `demonic_eye` has a primary device record and a supplemental pact-component record. Only its primary record supplies inventory width and stacking. Bound Ash cloned the supplemental record and registered it alone, so it needs its own inventory defaults. Its essence, ritual values, and source artwork are unchanged.

## Package provenance and verification

The DLL in `package/ImperiumOfMan-BoundAsh-Fix.zip` is a scoped binary patch of the existing compiled mod, accompanied by the matching source changes. It is **not a full rebuild against the current installed game**. The repository's bundled game assemblies predate the current source, and a current Quasimorph installation was unavailable in this environment.

Use the ZIP for this correction. The loose DLL under `package/ImperiumOfMan/` is retained as the original verified baseline. The GitHub connector's 16 MiB request limit rejects the updated DLL's 18.4 MB base64 payload; the complete compressed ZIP fits and contains the exact verified patched DLL. A normal full source build against current game assemblies will regenerate the loose runtime package.

- Baseline source: `a57e4f11642540a92489099a31ce219ef5b9e2cd`.
- Baseline DLL SHA-256: `ef5bd289d9348e2132f1b3b6a8fcc64d03b6ba849e6ee33094872ddbc765f850`.
- Patched DLL SHA-256: `f32fc899fb3ed5932db31fd37a149b58dfab079276a0dea50ec011ea44c426f6`.
- The new migration source compiled against real game reference metadata, Harmony 2.3.3, and Unity references with zero warnings and errors. Those references are for compilation only and are not included in the package.
- The injected load-hook attributes use the matching event attributes from the current baseline DLL rather than assuming older reference enum values are current.
- All 1,419 embedded resources are byte-identical to the baseline.
- Only two pre-existing method bodies changed: `RegisterChaosPacts` and `ApplyChaosItemArtwork`. The other 1,786 pre-existing methods and their signatures are unchanged.
- Six compiled migration methods and one nested patch class were added. Assembly identity and dependency references are unchanged; there is no dependency on the temporary migration compiler assembly.

## Remaining live-game verification

Current-game launch, an actual inventory screenshot, and interactive stacking have not been verified here. Check freshly generated and previously saved Bound Ash: one-cell icon/border, adjacent-cell occupancy, merge/split/transfer, count preservation across save/reload, and consumption of one unit by Greater Vessel and a skull ritual. The mod's existing Greater Vessel call already passes a consumption count of one.

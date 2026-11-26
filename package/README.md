# Runtime package

## 1.1.8 Native Tooltips build

Download **[ImperiumOfMan-1.1.8-NativeTooltips.zip](ImperiumOfMan-1.1.8-NativeTooltips.zip)**.

This package includes brief upgrade subtitles, native effect rows, wrapping item descriptions, and the current 1.1.8 ship-support and Comms features. All 44 custom upgrades in this build have dedicated effect rows. Native availability and purchase-cost panels remain part of the popup.

The ZIP pins source `d2da0db3a4b794cdfa25ae8369c3ef098e0d40ac`. The subsequent six-capability occult redesign on `main` is a separate 1.1.9 build. [Tooltip changes and version scope](../diagnostics/NativeUpgradeTooltips.md)

## Install

Close Quasimorph. Open the existing Imperium Reforged Workshop folder in your Steam library:

```text
steamapps\workshop\content\2059170\3754620214\
```

Extract the ZIP and open its `ImperiumOfMan` folder. Copy these four files directly into the Workshop folder, replacing the existing copies:

- `ImperiumOfMan.dll`
- `imperiumofman.bundle`
- `modmanifest.json`
- `thumbnail.png`

The four files belong directly in `3754620214`; avoid an extra nested `ImperiumOfMan` directory. Use the complete ZIP for this version. The repository's loose `package/ImperiumOfMan/` folder is an older baseline.

## Verification

Native compilation passed against the installed game with **zero warnings and zero errors**. [Native build 45802511629](https://github.com/mrcalzon02/Dens-Mortis/actions/runs/45802511629)

The [build and package receipt](../diagnostics/UpgradeTooltipBuildReceipt.json) records the exact source, archive checksum, all four runtime-file hashes, and compiled assembly version. The [1.1.8 coverage audit](../diagnostics/UpgradeTooltipIntegrationAudit.json) covers all 44 upgrade nodes; the [original source audit](../diagnostics/UpgradeTooltipSourceAudit.json) records the presentation changes and preserved descriptions.

This build has not been checked with a live in-game tooltip hover capture. Compilation and archive verification are distinct from visual and gameplay acceptance.

## Earlier package

The [1.1.4 Chemistry Foundations release notes](README-1.1.4.md) and earlier ZIPs remain available for reference.

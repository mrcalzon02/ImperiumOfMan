# Native upgrade tooltips

Tooltip source change: `11ed2b0c134683021b5a228161e0db8c1b16650d`. The combined build includes the later Comms expansion and call-in compatibility fix at `d2da0db3a4b794cdfa25ae8369c3ef098e0d40ac`.

## Presentation

All 44 custom upgrades in the pinned 1.1.8 source use a brief subtitle and native tooltip property rows. The upgrade subtitles contain at most three words and 20 characters. Status, native modifiers, purchase quantities, and material costs remain under the game's native tooltip builder; custom effects appear above the original cost grid.

| Upgrade group | Nodes | Effect information |
| --- | ---: | --- |
| Comms Node | 15 | Allied turret count and type, three placement locations, twelve turret replacements, mission-arrival timing, non-story restriction, and requirements |
| Orbital weapons and barrage upgrades | 6 | Damage, strike shape, impacts, firing turns, delay, and calls per raid |
| Drop support and troop rosters | 14 | Supply contents, troop capacity, shared charges, individual unlocked squads, and Imperial technology requirements |
| Occult production | 4 | Registered output, batch quantities, base production time, ingredients, and prerequisite facility |
| Occult research | 5 | Registered recipe counts, design tiers, research families, and prerequisite rank |

Incremental upgrades show the amount added and the resulting total. Mount and pod hovers read the purchased ship upgrades; the native default preview shows base values. Occult production reports the registered recipe rather than advertising an unavailable recipe. Production times are labeled as base hours because native modifiers affect final order time.

## Brief headers and preserved descriptions

Four module subtitles, custom item subtitles, station types, and alliance subtitles are concise. Complete item descriptions appear in native wrapping body panels. All 29 original literal item paragraphs, including the existing Russian translations, are preserved. Dynamically registered items receive a short category derived from their actual composite item record.

Troop node names use `Troop Roster 2` through `Troop Roster 11`; each unlocked squad has its own effect row. A newly registered custom upgrade without dedicated rows falls back to a wrapping description body while retaining a brief subtitle.

## Validation scope

The original tooltip source audit confirms 32/32 upgrade subtitles and effect branches, 4/4 module subtitles, all 68 referenced row text keys, and preservation of the original item paragraphs. The separate integration audit covers all 44 upgrades after the concurrent Comms expansion, including the 12 generated replacement IDs. Gameplay, purchase requirements, technology-tree organization, and support behavior are unchanged by the tooltip commit. The tooltip change advanced the assembly version to `1.1.7.0`; the subsequent Comms integration advances the combined build to `1.1.8.0`.

The adapter signatures were checked against the installed game's `TooltipFactory`, `PropertiesTooltip`, `TooltipProperty`, and `ItemTooltipBuilder`. They use the current native pooled panels and layout lifecycle.

Native compilation and package verification are recorded separately in [UpgradeTooltipBuildReceipt.json](UpgradeTooltipBuildReceipt.json). A successful compile does not establish a live visual or gameplay pass; an in-game hover capture has not been recorded.

## Subsequent occult redesign

The separate `ceba30c` feature replaces four occult facilities and five research ranks with six capability nodes in 1.1.9. Its native tooltip supplier in `Plugin.OccultUi.cs` was reviewed separately: all six have brief subtitles and structured native rows, and the shared supplier explicitly excludes those IDs to prevent duplicates. Current source therefore has 41 upgrade nodes. That source review is distinct from the pinned 1.1.8 compilation and package receipt.

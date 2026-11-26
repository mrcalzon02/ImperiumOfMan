# Security Intrusion turret progression

Security Intrusion applies to **non-story missions only**. The code requires a procedural raid with a resolved mission whose `IsStoryMission` flag is false. This excludes both native story raids and the mod's story campaigns, including campaigns that use procedural objective types. The restriction is shared by every location and turret tier, and appears in the Comms module caption and every affected upgrade's information.

## Three independent replacement chains

| Large location unlock | Small replacement 1 | Small replacement 2 | Small replacement 3 | Small replacement 4 |
| --- | --- | --- | --- | --- |
| Security Intrusion: random site turret | Flamer | Beamer | Arc Sentry | Lance Sentry |
| Entrance Override: entrance turret | Flamer | Beamer | Arc Sentry | Lance Sentry |
| Extraction Override: extraction turret | Flamer | Beamer | Arc Sentry | Lance Sentry |

Every location starts with the existing ballistic turret. The large unlocks occupy column 0, rows 1–3; their four small upgrades occupy columns 1–4 of the same row. This is 3 major and 12 minor nodes within the native 6-by-5 grid. The large unlocks retain the existing Security Intrusion → Entrance Override → Extraction Override dependency. A location's optional replacement chain depends only on its own preceding tier.

Only the **highest purchased type at that location** deploys. Lower purchased tiers remain in the save as prerequisites but do not create additional turrets. A player can leave one location ballistic, upgrade another to Arc, and upgrade the third to Lance. Fully upgrading all three produces at most three Lance Sentries, one per successfully placed location.

The ladder follows the existing [turret equipment progression](../src/Plugin.DeployableWorkshops.cs): Ballistic → Flamer → Beamer → Arc → Lance. The custom Lance weapon has a 34–47 damage range and range 9; Arc has 16–34 and range 4. Lance also has a one-charge magazine and recharge behavior, while Arc has its own shock firemode. These are equipment tiers, not a claim that every successor has higher sustained damage against every target. Native ammunition, firing modes and defenses retain their existing behavior; no turret combat stats are rebalanced here.

## Upgrade prices

These prices are paid once for each location's permanent replacement unlock. The original three location-unlock prices are unchanged.

| Replacement | One-time inputs per location |
| --- | --- |
| Flamer | 1 Fire Turret kit, 1 Electrical Parts Container |
| Beamer | 1 Energy Turret kit, 1 Electrical Parts Container, 1 Communication Relay |
| Arc | 1 SBN Arc Sentry kit, 1 AI Module, 1 Communication Relay |
| Lance | 1 RealWare Lance Sentry kit, 1 AI Module, 1 Darknet Unit, 1 Communication Relay |

Deployment on later eligible missions does not consume another kit. Native upgrade information displays the actual registered costs.

## Placement and persistence

- The original three location perk IDs are retained. New replacement IDs append `_flamer`, `_beamer`, `_arc` or `_lance` to that location's original ID.
- The existing once-per-raid deployment counter is retained. Reloading or reentering an already generated raid does not spawn another set. New upgrades affect subsequent eligible mission generation; already deployed turrets are not replaced in place.
- Each deployment resolves the selected kit's native `PlaceableDeviceRecord.TargetObjectId`. Custom package IDs are not mistaken for their `_ally` mob-class IDs.
- Custom kit validation and preview binding happen after all staged item, mob and perk registrations. A missing target disables its new upgrade instead of accepting a purchase that cannot deploy.
- The existing arrival-floor, room, proximity, passability and reserved-entry rules remain. Native object-placement checks also reject occupied map entities. Each successful deployment reserves its cell for the remaining locations.
- Existing allied ownership and summoned status remain. This feature continues the existing creation of allied turrets; it does not convert a hostile turret already present on the map.

## Verification scope

Reviewed against the installed game's captured perk, progression and deployment APIs. The graph has 15 unique node IDs, 14 adjacent orthogonal links, and separate tier resolution for all three locations. The non-story check runs before every selection, spawn and deployment-counter write. Source whitespace checks pass.

Native compilation and an in-game run have not been performed for this revision. Focused live checks are mixed location tiers, all three locations at Lance, no deployment in native or custom story missions, and save/reentry without duplicate turrets. Turret-preview rendering also needs an in-game capture.

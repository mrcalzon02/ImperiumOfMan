# Imperial room variety and facade seeding

This change extends the approved 24-accent facade system into ordinary
Imperial rooms and expands the optional native room catalog. It does not
add new generated artwork, replace either atlas, or touch Chaos layouts.

## Room themes

Seven route-derived native candidate families use IDs already present in
the Imperial campaign route and ore-bay catalogs. The installed game
resolves and validates the headers; the route labels are suggestions,
not verified per-preset room names. Actual installed fixtures take
precedence for room classification. Archives, security, quarters,
utilities and survey rooms have explicit accent pools; existing medical,
laboratory, chapel, workshop, cargo, relay, lift, corridor and objective
pools remain available. Rooms without identifiable fixtures use the
seeded preset-family fallback. For clean floors, this fallback also
selects an existing coherent floor kit rather than introducing more
adjacency bleed.

## Facade coverage

The native preservation pass retains native wall runs except for at
most one complete eligible feature run per ordinary room that does not
already have a safe Imperial face slot. All cells in that run must belong
to the same room, be ordinary south-facing facade cells with a wall cap,
and have no overlay. A run must have a safe interior position. Native
materials are not interleaved with isolated Imperial accent tiles.
Lift/objective groups, corridors and supply closets are not subject to
this feature-run conversion.

A room-specific PRNG is derived from floor seed, preset ID and room
position. The first safe slot in each eligible ordinary room uses
the second (molding-breaking) atlas as its signature. Subsequent
accents use the appropriate pool, preserving the spacing of at least
three plain tiles between accents within a run. Blocked interior slots
are skipped without splitting the base wall run. Original end tiles,
doors, light and obstacle footprints, native decals and arrival/exit
positions remain protected. There is no forced accent for a room with
no safe slot. The log reports room coverage, signature rooms, feature
wall conversions and per-atlas counts. Only fresh clean Imperial floors
are painted; existing saved terrain and all 24 tile IDs remain unchanged.

## Room count and planning limits

The prior 24-ID rotating list is replaced by the expanded known route
catalog. Seeded round-robin sampling prevents the larger industrial
family dominating selection. At most 21 additional preset loads, 24
candidate headers, four additional distinct rooms and 240 extra floor
cells are allowed per accepted layout. Candidates require safe existing
Core donor rules, 24–160 floor cells, dimensions 5–22, at least one
native doorway joint, and no special item/monster tables or special
item spawn count. Native required mission rooms are unchanged.
The optional search charges each distance probe and each joint check
against a 6,000-step budget, favors unused donor IDs and underused
families, and runs only after the original layout passed its audit.
The base plan remains cloned and available if either structural or
stamped-map auditing rejects the additions. Extra rooms are best-effort,
not guaranteed. Existing endgame expansion uses its original limits.

## Acceptance pending

Compile with the installed game SDK. Capture new Imperial station,
procedural and campaign maps across seeds; check room-family diversity,
short-room signatures, long-wall seam behavior, doors, fixtures,
minimap, native damage and reloading. Compare Chaos and saved maps.
Check room generation timings against the old baseline, candidate and
search budgets, unchanged required mission content, and fallbacks.
No native build or release is claimed.

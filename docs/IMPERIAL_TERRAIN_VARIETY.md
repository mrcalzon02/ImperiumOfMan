# Clean Imperial floors and native architectural variety

## Scope

New clean Imperial floors, including procedural maps and the Imperial
campaign, use selective adjacency. Chaos retains its current corrupted
palette, adjacency, trim, and furnishings. All existing tileset IDs and
legacy directional sprites remain registered; serialized floors are not
repainted. The new flat conveyor variants are appended after the original
48 slots, so existing conveyor indices remain valid.

## Material policy

- Only the cargo deck (`last_bonded_shipment`) and forge deck
  (`prime_the_foundry`) can use directional adjacency on clean Imperial
  maps, and only from a field or secondary material into a service lane.
- Other rooms receive a single coherent field material with crisp
  transitions. No synthetic wall bays or general wall-edge trim/litter
  overlays are applied to new clean Imperial floors. Existing native
  signs, meaningful decals, special cells, liquids and damage are not
  deliberately removed. Hallway stripes, thresholds, native props,
  loose salvage and intentional conveyors remain.
- Medical, laboratory and ceremonial rooms, plus a seeded subset of
  general rooms, may keep a native floor when that material originally
  covers at least 75% of eligible room cells and has a valid intact
  native damage transformation. Ordinary eligible cells are unified to
  the same material; existing indices are retained on matching cells.
- Native intact facade runs of at least three consecutive cells retain
  their native tilesets and damage chains; isolated cells and the
  constructed spine remain Imperial. Custom fixture-overlay cells retain
  their custom facade renderer. No topology, collisions, doors, health,
  or saved damage is explicitly rewritten by the facade selection.
- Additional native barrels, bins, pallets and crates are eligible for
  the existing corridor furniture slots, with the same spacing and
  placement checks.

## Room variety

The existing room planner may add up to two different compact native
rooms (48–160 floor cells each, aiming for 120 extra floor cells) to a
successfully audited clean Imperial branch layout. A seeded rotation of
existing Imperial route donor IDs is checked against installed Core
presets, with at most 12 candidate loads and 12 candidate headers.
Unique, objective, elevator, shuttle, quest-storage and trigger-bearing
donors are rejected by the existing donor rules. All rooms use ordinary
native joints, dimensions, fixtures, corridor connections and collision
checks. The candidate plan is cloned; failed structural audits retain
the base plan. A failed stamped-map audit or exception while stamping an optional donor retries the original plan.
This is a best-effort addition, not a guaranteed room count.

## Native acceptance (pending)

1. Compile on the installed-game runner. Generate fresh Imperial
   industrial, laboratory, medical, cargo and ceremonial maps; inspect
   clean room interiors, hard boundaries and both permitted lane joins.
2. Check that conveyors still appear where appropriate and that wall
   fixtures, native signs, doors, minimap, paths, and damage work.
3. Compare native façade runs and materials in several seeds. Check
   native room coverage and logs for added-room counts, audit fallback
   and preserved native floor/facade cells.
4. Compare an Imperial-owned and a Chaos-owned station, plus fresh
   procedural and campaign floors; Chaos must retain its corrupted art.
5. Reload an existing pre-change map and confirm old join, trim, litter
   and conveyor IDs still render, with existing destruction and damage
   intact. No release or runtime verification is claimed here.

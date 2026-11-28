# Native Imperial room furnishings

This pass supplements the established native room presets with additional
native decorative props. It executes during clean Imperial branching
layout stamping, after corridor furnishings, supply closets and campaign
views, before the existing stamped-map audit. It does not replace native
room contents, change façade atlases, paint terrain, or spawn extra loot.

## Native object discovery

The installed Core room headers already selected or available as normal
donors provide the object IDs and their original authored offsets and
wall-facing positions. Each eligible object resolves through the native
MapObstacleFactory; a real inactive prefab instance measures its native
OccupiedCells using InitializeForGameEditor. No invented GUIDs or
substitute art. The selection is a positive name/profile allowlist
for native medical and laboratory equipment, archives, ceremonial
fixtures, security furniture, communications equipment, workshop and
utility machinery, ordinary cargo props and crew/general furniture.
Actual installed names determine which objects are available. All
interactive objects, stores (including loot containers), doors, windows,
elevators, ladders, shuttles, mission objects, triggers, active turrets,
hazards and unsupported decorative prefabs are excluded.

Objects must have been placed against an actual wall in the source
preset. Their same authored offset and wall-facing relationship are
reused in a new room, never arbitrarily rotated. A room theme uses its
matching native prop family first, compatible supporting details second.
Unknown or unresolvable native objects are not guessed.

## Placement rules

- Only ordinary clean Imperial branch rooms; no supply closets, unique
  or scripted rooms, entry/exit rooms, special item/monster tables,
  or special spawn counts.
- Preserve existing objects and their measured footprints, lights,
  triggers, doors and two-cell interior landing neighborhoods, authored
  items and monsters, corridor cells, meaningful layer-two decals,
  special flags, and liquids.
- Prefer room edges away from entrances. Preserve the donor's wall-facing
  direction and leave a clear front/service tile.
- Treat every new footprint conservatively as solid. Flood-fill the
  room from its existing entrance before placing anything; skip an
  already disconnected room. After each proposed placement, all
  previously free cells outside the proposed footprint and all native
  entrance cells must remain connected. No extra objects in narrow rooms
  where there is no spare room budget.
- Each room retains at least 75% free floor by the conservative occupancy
  budget and uses at most 1/8 of its initial free floor for new details,
  with at most 1–3 added objects. Prefer distinct roles and native object
  IDs; no duplicate of an existing prop ID within a room.
- Fixed upper bounds: 48 donor headers, 768 inspected donor object
  references, 128 authored candidate variants, 256 unique footprint
  samples, 48 rooms, 48 new props, 4096 candidate
  anchor probes, 96 room-connectivity flood fills. All are per draft.
  A failed optional step skips the detail rather than aborting generation.
- Objects are stamped as ordinary MapObjectMetadata, so native loading,
  saving, destruction and art are unchanged. Subsequent façade placement
  sees their actual native footprints and protects them.

The existing draft audit and fallback remain in place. No new
decorative props are placed on saved floors, Chaos environments, or
static/non-branching native maps. Native prefab availability and
eligible room counts depend on the installed game and its contents.

## Acceptance pending

1. Compile against the installed game and verify native metadata fields
   and prefab sampling.
2. On the headed runner, inspect workshops, quarters, archive/relay,
   medical/laboratory, cargo and ceremonial rooms across seeds; check
   actual prefab names and counts in the native-room-detail log.
3. Verify no overlapping fixtures, blocked doors, inaccessible existing
   pickups, unexpected interaction/loot, or loss of signature façades.
4. Compare clean Imperial map-generation timings against the baseline;
   confirm limits and safe skips. Reload saved maps and inspect Chaos
   maps for unintended changes.
5. Confirm native stamps and runtime occupation agree. No compilation,
   native screenshot or release is claimed in this source commit.

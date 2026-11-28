# Data Rack — source implementation and acceptance

One rack consumes one Switchboard (communication_relay), one Quasimorphosis
Sensor (quasi_sensor_device), one Research Dump Disk (research_disk), and one
AI Module (ai_module). "Data" is interpreted as Research Dump Disk. The
Magnum recipe is shared, including existing saves, with an initial 48-hour
base production time and an exact output of one rack. This is deliberately
outside the ordinary halved-cost/doubled-output helper.

The sealed item is nonstackable; initial price 650 and weight 1.4 kg.
Native electrical cargo artwork is reused pending distinctive art.
Disassemble in the field or recycle aboard the Magnum to consume it
and produce five to eight real chips, inclusive. At least one is a basic
item chip and one is a faction chip if a valid faction pool is present.
Additional draws are 60% basic and 40% faction. A faction is drawn first,
then an eligible chip in that faction. No occult, Chaos, class or high-tech
chip types are explicitly added. Existing normal native corporate chip
pools are left intact (they can include advanced recipes); eligible mod
catalogs are tier 1–2 mundane catalogs only. Known duplicates remain
possible; no recipe is learned on opening. Native recipe selection occurs
on physical chip creation. Contents are rolled on opening, not sealed
at item creation; reloading a save before opening may reroll.

A 12% chance per fresh procedural entrance floor attempts to place one
rack in accessible, non-quest, compatible chip/technical loot storage,
preferring storage with actual technical goods. No periodic scans,
guaranteed drops, story injections or floor fallback. The crafting map
lists possible outputs as alternatives with unknown individual amounts.

## Pending native acceptance
- Compile against the actual installed game; package and verify DLL.
- Confirm all four ingredient names, exact 1:1:1:1 cost, 48 base hours,
  one output per order, and acquisition in existing/new campaigns.
- Open repeated racks in a raid and via the ship recycler, including
  full inventory, locked/quest protection and failure cases. Confirm
  consumption once, total 5–8 per rack, native per-chip selections,
  no direct unlock and no dropped/duplicated goods.
- Inspect tooltip, inventory dimensions, and crafting graph alternatives;
  verify native disassembly preview does not misrepresent actual payout.
- Generate fresh procedural entrance floors and reload saved floors;
  verify rare technical storage placement and no respawn on load.
- No MCM, turret intrusion, occult research or other balance changes.

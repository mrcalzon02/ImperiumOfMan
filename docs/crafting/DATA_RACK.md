# Data Rack — source implementation and acceptance

One rack consumes one Switchboard (communication_relay), one Quasimorphosis
Sensor (quasi_sensor_device), one Research Dump Disk (research_disk), and one
AI Module (ai_module). "Data" is interpreted as Research Dump Disk. The
Magnum recipe is shared, including existing saves, with an initial 48-hour
base production time and an exact output of one rack. This is deliberately
outside the ordinary halved-cost/doubled-output helper.

The sealed item is nonstackable; initial price 650 and weight 1.4 kg.
Authored 24×24 inventory, floor and shadow sprites are embedded via the existing artwork pipeline.
Disassemble in the field or recycle aboard the Magnum to consume it
and produce five to eight real chips, inclusive. At least one is a basic
item chip and one is a faction chip if a valid faction pool is present.
Additional draws are 60% basic and 40% faction. A faction is drawn first,
then an eligible chip in that faction. The native faction disks are tier 10 and are excluded. Separate tier-2 basic faction disks use eligible native low-tech faction recipes or curated patterns already present in the native basic chip pool. They are offered through existing faction reward/market paths without replacing the original pools. Eligible existing mod catalogs are tier 1–2 and must contain no tracked occult recipe outputs. Class, mercenary and faction-bound occult disks are excluded. Known duplicates remain
possible; no recipe is learned on opening. Native recipe selection occurs
on physical chip creation. Contents are rolled on opening, not sealed
at item creation; reloading a save before opening may reroll.

A 12% chance per fresh procedural entrance floor attempts to place one
rack in accessible, non-quest, compatible chip/technical loot storage,
preferring storage with actual technical goods. No periodic scans,
guaranteed drops, story injections or floor fallback. The crafting map
lists possible outputs as alternatives with unknown individual amounts. The native disassembly preview has a one-chip sentinel for its availability check; the actual opening hook overrides that sentinel, and the detailed item tooltip describes the true range. Native preview rendering still requires acceptance testing.

## Payout test harness

Run `dotnet run --project tools/crafting/data-rack-tests/DataRackRoller.Tests.csproj` with the .NET 8 SDK. The harness references the actual game-independent production selector and checks 10,000 mixed rolls and 1,000 fallback rolls, payout bounds, valid chip IDs, guaranteed mixed payout and faction fairness. It does not exercise the native disassembly or recycler APIs. Native build and test execution remain pending.

Public reference catalogue: https://github.com/Raigir/Quasimorph-Mod-Tool/blob/e6e557f1f679da0b2462ba2862296e08bd70cd28/ref/base/datadisks.txt . The game runtime still validates every source, faction, and recipe.

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

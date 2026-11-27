# Mercenary pod capacity progression

- Base Mercenary Pod: one mercenary per call; first capacity upgrade: two; second capacity upgrade: three.
- Both capacity upgrades are small adjacent nodes on the top row at (2,0) and (3,0), independently of the supply/mercenary shared call reserve and roster progression.
- The existing `iom_pod_capacity` ID and price are retained for rank I. Existing saves with that perk now deploy two rather than three until rank II is purchased. Rank II uses `iom_pod_capacity_2` and requires rank I; no save rewrite or automatic grants.
- Rank II reuses existing MercenaryArmor artwork and adds a military-parts container to the rank-I price. This is a balancing default, not an owner-specified price.
- Native tooltip total, target preview, actual deployment and call-in tooltip use the same capacity calculation. Supply capsules still require one non-corridor tile; a single mercenary retains the troop landing rules.
- Source checks: graph slots unique across all 15 drop-support nodes; base/first/second capacity and invalid rank-II-only combinations checked. These are source checks, not a native gameplay result.
- Native validation: compile; check both adjacent upgrades and prerequisite; spawn one/two/three units on clear tiles; check charge count, shared reserve, target preview, and purchased-rank persistence after reload.

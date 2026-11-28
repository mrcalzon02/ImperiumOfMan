# Transit and missing minimap regression

Reports: transit slows with mod installed; minimap absent at an Imperial location; Chaos-owned locations need corrupted terrain.

## Confirmed source-level defects repaired

- Campaign preparation repeatedly traversed and wrote existing dungeon plans on every space update. Preparation now runs once per mission object and revalidates at mission entry. Live standing checks and mission offering remain in the native space-update hook.
- Market restock now resolves first owned stations once per refresh and checks fully stocked occult supplies once instead of three times. Native stock quantities, eligibility, station preference, and cadence remain. Elapsed-time logging is diagnostic only.
- Call-in drawer idle hover no longer intercepts the native map hotkey, or blocks input in the absence of a pointer gesture. Actual drawer press/drag/release remains exclusive. Hidden drawers avoid recurring visibility writes and tile updates.
- New raids use the current Imperial/Chaos station owner for clean/corrupted terrain before considering authored campaign sponsorship. Existing raids retain their persisted palette rather than recoloring saved floors. Unrelated factions retain native terrain outside authored campaign routes.
- Explicitly initialize primary custom campaign floor, join, and conveyor surface/material fields using the established native field names. This fixes missing descriptor initialization, but the relationship to blank minimap rendering requires inspection of the installed MinimapScreen and map renderer.

## Required verification

1. Compare mod-disabled and enabled space transit on the same disposable campaign state, route and travel setting. Record frame time, native game-time advancement and market-restock spikes; do not infer a speedup from source changes.
2. Fresh visit to an Imperial station and a Chaos station; verify the normal map key and button, reveal/fog, player and elevator markers, and the map on a saved/reloaded floor. Include a normal native station control.
3. Compare Imperial-sponsored contract at a Chaos-owned location and Chaos-sponsored contract at an Imperial-owned location. Check actual terrain, walls and machinery. Preserve unrelated native environments.
4. Examine the current native minimap creation and color selection API before implementing a rendering-specific patch. Do not reveal unexplored tiles or replace native map behavior.

Status: source repairs only; native compile, measured transit and headed minimap confirmation outstanding.
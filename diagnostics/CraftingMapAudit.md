# In-game crafting map — 1.1.5

## Requested interface

The map belongs inside the mod. A narrow **Crafting map** tab appears on the left edge while Production or its item-selection window is open. Expanding the tab reveals a sage-green graph panel; its right-edge handle collapses it. The canvas supports mouse-wheel zoom, click-drag panning, and click-to-highlight selection. Details remain below the graph. Single-click selection preserves the current view; an explicit focus action can recenter a branch.

The entry hooks are in [Plugin.CraftingTreeEntry.cs](../src/Plugin.CraftingTreeEntry.cs). They target the concrete private `OnEnable()` methods of `MagnumProductionWindow` and `MagnumSelectItemToProduceWindow` in the installed game. An owner-bound companion observes deactivation/destruction and removes only that owner's tab. The earlier proposed ship-screen button was removed before publication.

## Loaded game data

[Plugin.CraftingIndex.cs](../src/Plugin.CraftingIndex.cs) builds an on-demand read-only snapshot from the registered item catalogue, Magnum production, workbench recipes, station industry, native disassembly, and mod shredding profiles. It preserves all item IDs, including items without producers, alternate recipes, and station cooutputs. It uses current modified-project output IDs when native Magnum production would do so.

Registered costs are already the effective costs; the index does not halve them again. Custom Magnum output batches come from `IndustryBatches`; unchanged native Magnum orders produce one unit. Registered production hours are base hours. Unknown workbench yields and times remain unknown. Native chance-based salvage and explicit ship recycling are separate routes.

Learned recipe state, cargo ingredients, and current station listings are captured when the map opens or Refresh is requested. A recipe with sufficient cargo is not a promise that a production line is available. The graph does not create recipes, grant knowledge, queue orders, alter item definitions, or save its index as campaign state.

## Interface implementation

[Plugin.CraftingTreeUi.cs](../src/Plugin.CraftingTreeUi.cs) contains the native Unity UI. It uses actual item sprites and muted sage colors, a connected graph canvas, compact search/venue controls, and a lower information panel. Catalogue and inventory scans occur only on opening or explicit Refresh. Lifecycle and input handling keep the map associated with Production and return control when it is collapsed or closed.

The expanded map intercepts `MGSC.UI.Process(out bool interruptProcessing)` and reports that input was consumed. Current `SpaceGameMode.Update` ticks the game before calling that method, so the map blocks native navigation and hotkeys without replacing the game loop. The block persists through the closing animation and its final frame.

## Validation status

- Focused compile/behavior checks of the actual index source passed against API-shaped fixtures: complete item discovery, alternatives and cycles, cooutputs, effective batches, learned/material state, station listings, modified projects, salvage distinctions, unknown workbench values, explicit refresh, and partial adapter failures.
- Current installed-game API inspection corrected the obsolete bundled DLL's `Show`/`Hide` and `InputController.GetInput` assumptions. The installed production views use `OnEnable`, and the verified native input boundary is `UI.Process`. Inspected `Assembly-CSharp.dll` SHA-256: `d72cd91f98de4d6cce8ee420ac1a906d89e68ba0cb164694d266b56f46339ebe`.
- Current installed-game native compilation of the corrected hooks: pending.
- Runtime package verification: pending.
- Game boot, visual capture, mouse interactions, and gameplay: not run.

These are distinct verification stages. A successful native compile does not establish that the window has been exercised in a running game.

## Focused in-game acceptance

Open Production and its recipe picker; the tab must appear in both and disappear when leaving Production. Expand/collapse it, zoom around the pointer, drag the canvas, select nodes without losing the view, and inspect lower-panel ingredients, output, time, and availability. Confirm native hotkeys do not act through an expanded map, and Escape returns to Production without closing the underlying screen in the same frame. Search for native and mod items, including a no-producer item; inspect an alternate recipe and a station cooutput. Confirm the interface remains usable at the player's actual resolution.

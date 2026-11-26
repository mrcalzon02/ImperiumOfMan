# Ship call-in drawer

The raid HUD has a compact pull tab at the top of the screen. Opening it slides down two groups, **Weapons** and **Pod Support**, in the Crafting Map's muted sage palette. Each installed weapon has its own tile. The pod group contains the supply call and the troop detachment selected in the ship's loadout.

Each tile uses its actual research descriptor's icon. A separate upper-right badge shows remaining calls, including **0**. Exhausted artwork and backgrounds darken, and exhausted calls cannot activate. Hover and native navigation focus show the call's name and availability without putting descriptions on the tile.

## Existing mechanics

- Weapon tiles read the existing shared fire reserve. Supply and troop tiles read the existing shared pod reserve.
- No charges, upgrade costs, cooldowns, targeting rules, troop roster selections, or save counters change.
- Activating a usable tile collapses the drawer and enters the established targeting/confirmation flow. Cancellation does not spend a charge.
- Native menus and targeting hide the drawer and retain their input. Closing the tray reserves that frame's input. Pointer presses begun on the drawer remain owned through release, even after dragging outside it.
- Tiles retain native `CommonButton` navigation registration and activation. Hidden category groups unregister when the collapse finishes. Reaching and using the tray with a controller still requires live acceptance.
- The tray groups stack on narrow canvases. A new dungeon retires previous HUD instances before checking eligibility; `DungeonFinished` cleans up the drawer.

## Source

- `src/Plugin.ShipSupportDrawer.cs`: layout, slide, categories, lifecycle, input boundary.
- `src/Plugin.ShipSupportTile.cs`: research art, badge, exhausted state, native buttons.
- `src/Plugin.ShipSupportUi.cs`: existing dungeon hook delegates to the drawer; ship loadout and class controls retain their existing implementation.
- `src/ImperiumOfMan.csproj`: explicit installed-game `UnityEngine.UIModule` reference required by Canvas, CanvasGroup, and RectTransformUtility.

## Verification

- Source review and whitespace checks completed.
- Current installed-game API inspection from native run `45797203026` confirmed that ordinary raids call `DungeonHudScreen.Process` directly and honor its input-consumed result before player commands. The drawer patches this boundary as well as `UI.Process` for native overlays and the same-frame targeting latch. Simulation and turn updates remain outside this input guard.
- Exact-commit installed-game build and runtime package: pending.
- In-game screenshot, animation, mouse click-through, controller, and save/load acceptance: not run.

The native compile/package and live visual/interaction checks are separate. No simulated preview is an in-game capture.

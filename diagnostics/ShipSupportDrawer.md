# Ship call-in drawer

The raid HUD has a compact pull tab at the top of the screen. Opening it slides down two groups, **Weapons** and **Pod Support**, in the Crafting Map's muted sage palette. Each installed weapon has its own tile. The pod group contains the supply call and the troop detachment selected in the ship's loadout.

Each tile uses its actual research descriptor's icon. A separate upper-right badge shows remaining calls, including **0**. Exhausted artwork and backgrounds darken, and exhausted calls cannot activate. Hover and native navigation focus show the call's name and availability without putting descriptions on the tile.

## Existing mechanics

- Weapon tiles read the existing shared fire reserve. Supply and troop tiles read the existing shared pod reserve.
- No charges, upgrade costs, cooldowns, targeting rules, troop roster selections, or save counters change.
- Activating a usable tile collapses the drawer and enters the established targeting/confirmation flow. Cancellation does not spend a charge.
- Native menus and targeting hide the drawer and retain their input. Closing the tray reserves that frame's input. Pointer presses begun on the drawer remain owned through release, even after dragging outside it.
- The native `CommonButton` integration keeps controller/keyboard navigation. Hidden category groups unregister from navigation when the collapse finishes.
- The tray groups stack on narrow canvases. A new dungeon retires previous HUD instances before checking eligibility; `DungeonFinished` cleans up the drawer.

## Source

- `src/Plugin.ShipSupportDrawer.cs`: layout, slide, categories, lifecycle, input boundary.
- `src/Plugin.ShipSupportTile.cs`: research art, badge, exhausted state, native buttons.
- `src/Plugin.ShipSupportUi.cs`: existing dungeon hook delegates to the drawer; ship loadout and class controls retain their existing implementation.
- `src/ImperiumOfMan.csproj`: explicit installed-game `UnityEngine.UIModule` reference required by Canvas, CanvasGroup, and RectTransformUtility.

## Verification

- Source review and whitespace checks completed.
- Exact-commit installed-game build and runtime package: pending.
- In-game screenshot, animation, mouse click-through, controller, and save/load acceptance: not run.

The native compile/package and live visual/interaction checks are separate. No simulated preview is an in-game capture.

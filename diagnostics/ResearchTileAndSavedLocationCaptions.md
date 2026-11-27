# Research tile alignment and saved-location captions

Source baseline: `bc2a0665d9a0581ca20c68ad734dd3ecfb4f76f2`.

## Changes
- Restore pooled native panel geometry before RefreshGrid copies navigation-cell positions.
- Center the icon, black cover, button and button background on the original slot rectangle's actual center; fit the tile to 36 canvas units before applying a single root scale (major 36, minor 18). Keep the yellow availability marker at its native displayed size. Restore native geometry when changing modules.
- Supply non-destructive custom location aliases from the registered station and celestial-body names in each loaded language; include existing legacy station aliases.
- Preserve native station/deck captions where present and fall back to a registered Imperium station caption for missing location captions. Campaign-specific deck names are retained.

## Verification boundary
The native decompilation for `MagnumModuleTechTreeWindow.RefreshGrid` and `MagnumTechTreePanel.Initialize` was inspected from Dens-Mortis run 45938889174. The former copies pooled panel positions before Initialize; the latter sizes the icon and black cover and reuses the button sprite. The exact native save-slot/orbit caption consumer has not yet been inspected: the location/orbit aliases are compatibility coverage, not proof that the save-menu error is resolved. Do not alter save schema, IDs, existing save files or vanilla labels.

Required next checks: native build, headed major/minor alignment in all four custom modules, switch back to vanilla tree and reopen, purchase/hover/click geometry; enumerate the native save-slot caption consumer and save/reload disposable fixture at each custom station and body, including Neith, fringe bodies, old Luna/Havoc aliases and campaign decks.

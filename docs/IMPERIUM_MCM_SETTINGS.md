# Imperium Reforged configuration (MCM)

## Scope and activation

The original confirmed MCM information entry remains unchanged. Additional
typed API registrations create compact sidebar pages: Drops, Weapons, Armor,
Items, Recipes, Rooms, plus paginated detail pages for individual mod-added
equipment, items, ship recipes and item-only station workshops. Crynano's
public API has header sections but no nested tab registration; independent
entries are used as category tabs. Each page is bounded to 72 editable rows
and keeps item groups together. See the public API, RangeConfig and
DropdownConfig in https://github.com/Crynano/Mod-Configuration-Menu.
The original mod works without MCM; optional API failures do not stop startup.

Settings are stored separately from Steam mod files and saves:
Application.persistentDataPath/ImperiumReforged/settings-v1.ini.
The typed MCM API is fileless; each validated page callback saves a merged
configuration atomically. The game loads it once after registering all
Imperium content. **Restart Quasimorph after changing settings.** A page
reset only resets that page. Missing settings equal current defaults.
Both category and per-record multipliers combine, bounded to 10–400% of
base. Nonstackable records stay nonstackable, positive ingredient and yield
counts remain positive, and capacity/durability reductions are disabled.
No recipe unlocks, item identities, quest gates or technology tiers change.

## Controls

- Drops: Data Rack attempt percentage (12), Chaos supply-cache attempt
  percentage (100), minimum/maximum rack chips (5/8), ration quantity
  (100%), relative added equipment, consumables and chip reward weights.
  Weights are not direct probabilities; zero weights are disallowed.
- Weapons: damage, ranged range, durability, magazine capacity, plus
  individual weapon values. No weapon class or ammo identity replacement.
- Armor: positive resistance, durability, individual armor multipliers.
  Negative resistances are preserved.
- Items: weight, base value, stack increases, per-item multipliers for
  mod-added records. Original item registry baseline isolates native and
  unrelated mod items; existing quest costs are not changed.
- Recipes: ingredient costs, base hours, output batches for explicitly
  mod-managed Magnum recipes and newly added item-only station recipes;
  individual recipe controls for the same three values. Inputs round up;
  no zero-cost conversion of existing positive ingredients. Workbench,
  recycling, population and strategic receipts remain unchanged.
- Rooms: adaptive/three palettes, retain native floor toggle, façade
  toggle, spacing, strong accent cadence, additional room/floor limits,
  extra furniture counts. All retain the existing safe bounds.

## Three curated Imperial room palettes

The selector uses existing 28px ImperialCampaignDecks art, not generated
placeholder swatches or new IDs. The exact matrix, room-by-room, is in
SelectImperialRoomPalette. Adaptive preserves the previous logic.

| Style | Typical room surfaces | Other materials |
| --- | --- | --- |
| Foundry iron | Caloris rust-brown loader iron, Primaris coal-black steel | Tiangong oxide cargo plate, cool terminal-room panels, burnt copper masonry |
| Fortress basalt | Hesperia warm gray dressed basalt | Goethe moss-gray cast iron, Cinder Basin traction steel, Discovery slate |
| Scholastic slate | Discovery pale slate ceramic, Boethius cool blue-gray deck | Hollow Signal relay insets, Hesperia ceremonial basalt, industrial service plates |

The selected profile overrides ordinary clean room and passage kit
selection; objective route kit remains. Explicit palettes suppress adaptive
native-floor retention. Only the two established cargo/foundry kits use
material joins. Chaos, existing saved floors, IDs and geometry stay unchanged.
These are curated palettes using the existing artwork library, not three
newly drawn texture atlases.

## Validation to perform on headed runner

1. Build against installed game and launch with current MCM. Check all
   category buttons, scrolling, tooltips, item headers and reset.
2. Set non-default values and restart; compare record stats, recipe counts,
   actual production durations/yields and loot behavior. Reset, restart and
   compare to original; confirm scaling never compounds.
3. Test invalid range/reversed rack bounds and missing MCM. Compare clean
   default maps and each explicit room palette on fresh Imperial floors,
   including signature facades and room furnishing settings.
4. Check save load, quest/occult prerequisites and third-party record
   isolation. Review English captions; other languages currently use English
   fallback except the existing localized info and restart line.
5. Native runtime/headed testing pending.

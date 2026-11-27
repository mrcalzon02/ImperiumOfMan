# Occult facility expansion
## Scope
The original six capability perk IDs, research prices, recipe requirements, and physical pattern-chip distribution remain. They are now the major spine in a 6-column native grid; ten optional smaller nodes improve the facility. No research purchase teaches a recipe.

| Branch | Rank I | Rank II |
|---|---|---|
| Culture expansion | +1 raw reagent per batch | +2 total |
| Ritual automation | -15% occult batch time | -30% total |
| Material recovery | +1 designated occult material per batch | +2 total |
| Ritual scheduling | 2 orders per line | 3 orders per line |
| Independent lines | Second occult line at rank III | Third at rank V, after second |

Rank I unlocks one line with one order. Reagent yield applies to Raw Clone, Torment Imprint, Essence of Suffering, and owned Gavvakh recipes; material yield applies to designated Bramfatura intermediates and Chaos salvage, not finished equipment. Time uses native difficulty scaling, whole-hour rounding and a one-hour minimum. Ordinary production bonuses do not alter occult machinery.

## Production integration
The occult research tree opens the native production window in an occult-only mode; a tab switches between ordinary and occult production. Both reuse native queue panels, cargo consumption, save data, order completion and cancellation. Occult queues use fixed line keys 10000–10002, independent of ordinary line upgrades. The native production transaction and picker both enforce recipe ownership, learned knowledge, capability, and unlocked queue capacity. Ordinary queues cannot start newly gated recipes; existing queued orders are preserved and can finish normally. Existing native/other-mod recipes that were intentionally not gated remain ordinary recipes. The recipe picker shows batch output and timing; crafting-map access identifies the required facility and names the existing pattern chips where available.

## Source verification
Six major and ten minor positions were checked for unique coordinates, bounds, valid parent and rank requirements, and adjacency. Native API paths were reviewed for production initialization, line identifiers, event handlers, recipe picker fields, order storage, batch duration and cancellation. These are structural checks, not a native build or live acceptance result.

## Headed acceptance
- Open occult research; confirm all sixteen nodes, small/major sizing, links, and localized tooltips.
- Unlock rank I without acquiring its pattern; verify no recipe is automatically learned. Learn a physical pattern and start an eligible order.
- Open ordinary and occult tabs; verify category filters and locked capabilities; confirm no cross-line order start.
- Purchase the throughput, yield, queue and additional-line upgrades; compare batch output and duration, confirm parallel queues and queue limits.
- Save/reload active queues and verify correct completion, cancellation/refund, and no disruption of existing ordinary orders.
- Verify icon appearance, compact captions and native navigation on the headed runner.

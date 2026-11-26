# Crafting map and occult reference

The current source implements six occult research capabilities, independent physical pattern chips, the clone-to-Gavvakh chain, and the in-game Crafting Map. Native build and gameplay evidence are tracked in [Occult research](../../diagnostics/OccultResearchExpansion.md) and [Organic recycling](../../diagnostics/OccultOrganicProduction.md).

## Open the map

Open [recipe-map.html](recipe-map.html) in a browser. Its data is embedded, so it works offline. Search for an item, choose Magnum/station/all sources, inspect producer recipes, expand the upstream trace, or select a downstream consumer.

Enable **Include conditional recipe candidates** to inspect occult additions that may be omitted when an installed native formula already exists or a required item is absent. Conditional candidates are hidden initially. The in-game Crafting Map reads the accepted live catalogue and the current campaign's recipe knowledge, capability and cargo.

The offline snapshot is pinned to source commit `ceba30c2292be3975748cdfb750c287e20eff78d`, version **1.1.9.0**. Its **751 captured records** comprise 368 unconditional Magnum source records, 303 station records, and 80 conditional Magnum candidates. This includes **53 new occult candidates** and their **21 declared physical pattern-chip pools**. Source counts do not establish the number registered by the installed game.

## Six capabilities and acquired knowledge

![Six occult research capabilities using the exported runtime icons](occult-capabilities.png)

This reference diagram uses the actual exported research icons. The native game positions and states still require the headed game check. Research progresses through **Culture, Distillation, Manifestation, Binding, Infusion and Soulforge**, with exactly six purchases and no separate facility levels.

Research buys the capability to use a class of formulas. Each physical pattern chip teaches one selected recipe from its small pool. Production needs both requirements. The offline recipe details show these separately and preserve the occult formulas' literal quantities; the ordinary half-input/double-output helper is not applied to them.

## Files

| File | Purpose |
| --- | --- |
| [Source graph JSON](recipe-graph.json) | Captured records, exact known quantities, source provenance, conditional rules and unresolved cases |
| [Capability SVG](occult-capabilities.svg) / [PNG](occult-capabilities.png) | Six-node research sequence; SVG embeds the actual runtime icons |
| [Research sprites](occult-research-ranks.png) | All six green normal images and corresponding crimson locked images |
| [Inventory sprites](occult-starter-sprites.png) | Clone, imprint, essence and physical pattern-chip icon/floor/shadow versions |
| [Occult research details](../../diagnostics/OccultResearchExpansion.md) | Purchase samples, knowledge-source groups, capability gates and verification status |
| [Organic production details](../../diagnostics/OccultOrganicProduction.md) | Exact four-step production quantities and selected recycling behavior |
| [Raw-material SVG](raw-material-overview.svg) / [PNG](raw-material-overview.png) | Ordinary chemistry routes with effective input quantities |
| [Crafting expansion design](../../CRAFTING_TREE_EXPANSION_DESIGN.md) | Broader design and future work beyond the current live browser |
| [Historical occult proposal](../../OCCULT_PRODUCTION_DESIGN.md) | Original equipment and worldbuilding ideas; superseded by the six-capability implementation where they differ |

Keep the folder structure when extracting a review ZIP so companion links continue to work. The complete HTML itself works independently.

## Rebuild the reference

Run from the repository root with Python 3 and Graphviz installed:

```sh
python tools/crafting/export_source_graph.py --ref ceba30c2292be3975748cdfb750c287e20eff78d --out /tmp/occult-source-graph.json
python tools/crafting/build_source_map.py --graph /tmp/occult-source-graph.json
```

The bounded exporter recognizes the reviewed source helpers and the six explicit Mars replacement variants. It retains unresolved calls. A changed helper may need a corresponding exporter change. Native-only recipes, active station state, inventory, learned schematics and accepted runtime registration results require the live game view.

The older `occult-proposal.*` files remain historical design drawings. Their proposed facility structure does not describe the current six-purchase tree. The current capability drawing and its source are `occult-capabilities.*`.

## Verification scope

The source export captures all 53 new occult candidates with a capability and physical knowledge source, including all six Mars parts. Exact starter batches are 2 clones, 4 imprints, 4 essence and 2 Gavvakh, with baseline times 8/4/4/4 hours. No occult parse calls are unresolved. JavaScript syntax, graph generation and the rendered capability reference were checked.

The art exporter produced 24 runtime sprites with the required RGBA dimensions, matching normal/locked silhouettes, green normal borders and crimson locked borders. The sheets above are asset previews. Native compilation, game launch and gameplay evidence are recorded separately; source and diagram checks are not substitutes for those results.

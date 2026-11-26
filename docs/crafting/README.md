# Crafting design and visual reference

This folder contains a source snapshot and future design material. It is not a game installation package.

## Open the map

Open [recipe-map.html](recipe-map.html) in a browser. The graph is embedded, so the page works offline without a server or external scripts. Search for an item, choose Magnum/station/all sources, inspect its producer recipes, expand the upstream trace, or select a downstream consumer. Conditional fallbacks are off by default because a native recipe may replace them in the active game.

Keep this folder structure when extracting the review ZIP so its links to the diagrams, graph data, and design documents continue to work. The complete HTML remains usable on its own, but its companion-file links need the other files.

## Contents

| File | Purpose |
| --- | --- |
| [Crafting expansion design](../../CRAFTING_TREE_EXPANSION_DESIGN.md) | Active-game index, graph window, quantities, access, exports, and delivery phases |
| [Occult production design](../../OCCULT_PRODUCTION_DESIGN.md) | Proposed clone/essence production, upgrades, family equipment trees, and icon specification |
| [Source graph JSON](recipe-graph.json) | 698 captured records with source provenance, known quantities, conditional flags, and unresolved cases |
| [Raw-material SVG](raw-material-overview.svg) / [PNG](raw-material-overview.png) | Exact subset of the 1.1.4 source chemistry routes; incoming quantities are effective per-order costs |
| [Occult proposal SVG](occult-proposal.svg) / [PNG](occult-proposal.png) | Future production proposal, clearly separate from existing recipes |
| [Draft change notes](CHANGE_NOTES_1.1.3-1.1.4.txt) | Unreleased source changes and documentation additions |
| [Source audit](../../diagnostics/RawMaterialChemistryAudit.md) | Coverage, validation, exceptions, economy caveats, and native build evidence |

The snapshot is pinned to source commit `de43df89d6b5878422606fa69c43a4c544f2987e`. It captures 365 explicit Magnum recipes, 303 station records, and 30 conditional fallbacks. It does not load native-only recipes, inventory, learned schematics, other mods, or active station state. Generated-family quantities are shown as unknown when the bounded exporter cannot establish them. The live game browser, complete runtime export, queue planner, and occult production are future work.

## Rebuild the source map

Run from the repository root with Python 3. Graphviz supplies the `dot` command used for SVG/PNG output.

```sh
python tools/crafting/export_source_graph.py --ref de43df89d6b5878422606fa69c43a4c544f2987e --out chemistry-source-graph.json
python tools/crafting/build_source_map.py --graph chemistry-source-graph.json
dot -Tsvg docs/crafting/occult-proposal.dot -o docs/crafting/occult-proposal.svg
dot -Tpng docs/crafting/occult-proposal.dot -o docs/crafting/occult-proposal.png
```

The occult DOT file is an authored proposal, not generated from the active recipe catalogue. Updating its drawing does not implement those recipes. Source helper changes may require an exporter update; review unresolved calls rather than treating the snapshot as a complete game database.

## Review and limits

The source graph and UI logic received independent review. JavaScript syntax, embedded JSON, unique element IDs, source counts, links, and diagram rendering were checked. Graphviz output was visually inspected. A full Chromium interaction/layout check was unavailable in this environment, so browser behavior is not reported as a completed visual test. Native compilation passed against the installed game on the Windows Steam runner. The separate runtime ZIP passed checksum, CRC, member-hash, assembly-version, method, constant, and embedded-resource checks. This review bundle contains no compiled mod DLL. Game boot, live coverage logs, and gameplay are still unverified. See the source audit for the final package verification record.

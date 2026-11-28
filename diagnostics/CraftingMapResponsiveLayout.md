# Responsive crafting map and concise inspector

User-reported screenshot: 2048 x 576 (32:9). The previous width/height averaged CanvasScaler and fixed top/bottom panels left about 120 physical pixels for graph content. This revision uses height-limited canvas scaling, compact controls outside the graph, a side inspector on wide screens, and a hide/show inspector control. On the reported resolution at 100% interface scale, pure layout geometry predicts a graph viewport of about 1619 x 498 physical pixels. This is a calculated target, not a gameplay screenshot.

Controls: A-/A+ adjust interface scale 70–150% independently of graph zoom; Fit uses current viewport bounds; Focus/search starts with an overview of the selected item's graph. The open panel uses small margins and a single Close action; the left edge entry remains.

Remove persistent usage instructions, repeated item type and producer/use counts, unnecessary action prefixes, and generic selection footers. Actual requirements and warnings remain in an optional scroll area only when needed. Details and Sources retain their separate scrollable lists. Long detail rows wrap, and paging arrows only appear when required.

Acceptance: run the pure geometry tests at normal and wide aspect ratios, including 2048 x 576; compile against the installed native API; headed captures at wide and 16:9, at 70/100/150% UI, with inspector shown/hidden, Sources toggled, and fit/pan/zoom. Do not substitute calculated layout for visual acceptance.

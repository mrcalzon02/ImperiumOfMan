#!/usr/bin/env python3
"""Package the reviewed crafting documents, visual map, and source evidence."""
import hashlib
import json
from pathlib import Path
import subprocess
import zipfile

root = Path(__file__).resolve().parents[2]
destination = root / 'docs' / 'crafting' / 'ImperiumReforged-Crafting-Design-Review.zip'
files = list(root.glob('*.md'))
files += [root / name for name in (
    'CHAOS_UNDIVIDED_DESIGN.md', 'FACTION_CAMPAIGNS_DESIGN.md',
    'IMPERIAL_FORMATIONS_DESIGN.md', 'PRODUCTION_CHAIN_DESIGN.md',
)]
files += list((root / 'src').glob('*.cs'))
files += [root / 'src' / 'Properties' / 'AssemblyInfo.cs']
files += list((root / 'src' / 'Art' / 'OccultProduction').glob('*.png'))
files += list((root / 'media' / 'OccultProduction' / 'Source').glob('*.png'))
files += [root / 'media' / 'OccultProduction' / 'Reference' / 'navigation-technology-tree.png']
files += [root / 'media' / 'OccultProduction' / 'art-manifest.json']
files += [root / 'media' / 'ImperialInteriors' / 'README.md']
files += [root / 'tools' / 'export_occult_art.py']
files += list((root / 'tools' / 'crafting').glob('*.py'))
files += [p for p in (root / 'docs' / 'crafting').iterdir() if p.is_file() and p.suffix != '.zip']
files += list((root / 'diagnostics').glob('*.md'))
files += list((root / 'diagnostics').glob('*.json'))
tracked_diagnostics = subprocess.check_output(
    ['git', 'ls-files', '--', 'diagnostics'], cwd=root, text=True).splitlines()
files += [root / name for name in tracked_diagnostics
          if Path(name).suffix in ('.md', '.json')]
files += list((root / 'package').glob('README*.md'))
files += [root / 'package' / 'README-1.1.4.md', root / 'package' / 'README-1.1.8.md']
files = sorted(set(files))
missing = [path.relative_to(root).as_posix() for path in files if not path.is_file()]
if missing:
    raise SystemExit('Missing review documents or assets; expand the sparse checkout before packaging:\n'
                     + '\n'.join(missing))
graph = json.loads((root / 'docs' / 'crafting' / 'recipe-graph.json').read_text())
source_head = subprocess.check_output(['git', 'rev-parse', 'HEAD'], cwd=root, text=True).strip()
changed_paths = subprocess.check_output(['git', 'status', '--porcelain'], cwd=root, text=True).splitlines()
manifest = {'kind': 'crafting-design-review', 'runtime_package': False,
            'source_commit': source_head, 'graph_source_commit': graph['source_commit'],
            'working_tree_changes_at_packaging': changed_paths, 'members': []}
start = '''IMPERIUM REFORGED — CRAFTING SOURCE AND ART REVIEW

Verified snapshot: source 347de7d. Native compilation, bounded map checks,
corrected-directory Space/time advancement, six-node research visuals and both
corrected tooltip hovers passed. The receipts distinguish these observations from
unperformed purchases, chip learning, recycling and exact inventory-delta checks.

Open docs/crafting/recipe-map.html in your browser to explore the source recipe map.
It works offline. Keep the extracted folder structure for companion-file links.

Main plan: CRAFTING_TREE_EXPANSION_DESIGN.md
Current occult research: diagnostics/OccultResearchExpansion.md
Current organic production: diagnostics/OccultOrganicProduction.md
Historical equipment proposal: OCCULT_PRODUCTION_DESIGN.md
Flowcharts: docs/crafting/raw-material-overview.svg and occult-capabilities.svg
PNG copies and cumulative change notes are in the same folder.
Image generation prompts and node/variant links: media/OccultProduction/art-manifest.json
Generated source masters (10): media/OccultProduction/Source/
Native research and inventory/floor/shadow PNGs (24): src/Art/OccultProduction/
Original user-provided style reference (1):
media/OccultProduction/Reference/navigation-technology-tree.png
The style reference is unmodified, not generated, and not gameplay validation.
Deterministic artwork exporter: tools/export_occult_art.py
Current ship/tree/tooltip evidence and cleanup: docs/crafting/native-occult-ui-347de7d.json
Current native captures: native-occult-{ship,tree,culture,manifestation}-347de7d.png
(the four PNG files are in docs/crafting/)
Historical native tree evidence: docs/crafting/native-occult-tree-bdb1759.json
Verification: diagnostics/RawMaterialChemistryAudit.md
Current build/runtime receipt: diagnostics/OccultBuildReceipt.json
Session-005 cases and frame hashes: diagnostics/CraftingMapAcceptance-347de7d.json
Historical bdb receipt: diagnostics/OccultBuildReceipt-bdb1759.json
Current installation instructions: package/README.md

This bundle contains documentation, an offline map, diagrams, reusable PNG assets,
generated source masters, the artwork exporter, and source evidence.
It is not a game installation package and contains no runtime binaries or nested ZIPs.
Runtime ZIP links in the copied repository documents refer to a separate download.
Find published runtime packages at:
https://github.com/mrcalzon02/ImperiumOfMan/tree/main/package
Background faction/interior notes also link to separate repository asset trees,
Unity sources and asset-build scripts; those are outside this crafting review.
Native decompiler dumps are not included.
Full repository for those background references:
https://github.com/mrcalzon02/ImperiumOfMan
The current source includes the live Crafting Map and six occult capabilities.
Physical pattern chips teach recipes; research provides the capability to use them.
Source candidates and asset previews do not establish installed-game acceptance.
Check the current diagnostics for native build, launch and gameplay evidence.
REVIEW_MANIFEST.json distinguishes the packaged source revision from the pinned
graph revision and records a checksum for every included file.
'''
with zipfile.ZipFile(destination, 'w', compression=zipfile.ZIP_DEFLATED, compresslevel=9) as archive:
    archive.writestr('START_HERE.txt', start)
    for path in files:
        relative = path.relative_to(root).as_posix()
        content = path.read_bytes()
        archive.writestr(relative, content)
        manifest['members'].append({'path': relative, 'bytes': len(content),
                                    'sha256': hashlib.sha256(content).hexdigest()})
    archive.writestr('REVIEW_MANIFEST.json', json.dumps(manifest, indent=2) + '\n')
with zipfile.ZipFile(destination) as archive:
    assert archive.testzip() is None
    assert not any(name.lower().endswith(('.dll', '.exe', '.bundle', '.zip'))
                   for name in archive.namelist())
print(json.dumps({'path': str(destination), 'members': len(manifest['members']) + 2,
                  'bytes': destination.stat().st_size,
                  'sha256': hashlib.sha256(destination.read_bytes()).hexdigest()}, indent=2))

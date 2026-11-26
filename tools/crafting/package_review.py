#!/usr/bin/env python3
"""Package the reviewed crafting documents, visual map, and source evidence."""
import hashlib
import json
from pathlib import Path
import zipfile

root = Path(__file__).resolve().parents[2]
destination = root / 'docs' / 'crafting' / 'ImperiumReforged-Crafting-Design-Review.zip'
files = list(root.glob('*.md'))
files += list((root / 'src').glob('*.cs'))
files += [root / 'src' / 'Properties' / 'AssemblyInfo.cs']
files += list((root / 'tools' / 'crafting').glob('*.py'))
files += [p for p in (root / 'docs' / 'crafting').iterdir() if p.is_file() and p.suffix != '.zip']
files += list((root / 'diagnostics').glob('*.md'))
files += list((root / 'diagnostics').glob('*.json'))
files = sorted(set(files))
graph = json.loads((root / 'docs' / 'crafting' / 'recipe-graph.json').read_text())
manifest = {'kind': 'crafting-design-review', 'runtime_package': False,
            'source_commit': graph['source_commit'], 'members': []}
start = '''IMPERIUM REFORGED — CRAFTING DESIGN REVIEW

Open docs/crafting/recipe-map.html in your browser to explore the source recipe map.
It works offline. Keep the extracted folder structure for companion-file links.

Main plan: CRAFTING_TREE_EXPANSION_DESIGN.md
Current occult research: diagnostics/OccultResearchExpansion.md
Current organic production: diagnostics/OccultOrganicProduction.md
Historical equipment proposal: OCCULT_PRODUCTION_DESIGN.md
Flowcharts: docs/crafting/raw-material-overview.svg and occult-capabilities.svg
PNG copies and draft change notes are in the same folder.
Verification: diagnostics/RawMaterialChemistryAudit.md

This bundle contains documentation, an offline map, diagrams, and source evidence.
It is not a game installation package and contains no compiled mod DLL.
The current source includes the live Crafting Map and six occult capabilities.
Physical pattern chips teach recipes; research provides the capability to use them.
Source candidates and asset previews do not establish installed-game acceptance.
Check the current diagnostics for native build, launch and gameplay evidence.
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
    assert not any(name.lower().endswith('.dll') for name in archive.namelist())
print(json.dumps({'path': str(destination), 'members': len(manifest['members']) + 2,
                  'bytes': destination.stat().st_size,
                  'sha256': hashlib.sha256(destination.read_bytes()).hexdigest()}, indent=2))

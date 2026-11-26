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
files += [root / 'src' / 'Properties' / 'AssemblyInfo.cs',
          root / 'src' / 'ImperiumOfMan.csproj']
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
files += [root / 'package' / 'README-1.1.4.md', root / 'package' / 'README-1.1.8.md',
          root / 'package' / 'README-1.1.9.md']
files += [root / name for name in (
    'diagnostics/FinishedRationBuildReceipt.json',
    'diagnostics/FinishedRationOutputCorrection.md',
    'diagnostics/CraftingMapAcceptance-347de7d-007.json',
)]
files = sorted(set(files))
missing = [path.relative_to(root).as_posix() for path in files if not path.is_file()]
if missing:
    raise SystemExit('Missing review documents or assets; expand the sparse checkout before packaging:\n'
                     + '\n'.join(missing))
graph = json.loads((root / 'docs' / 'crafting' / 'recipe-graph.json').read_text())
receipt = json.loads((root / 'diagnostics' / 'FinishedRationBuildReceipt.json').read_text())
repository_head = subprocess.check_output(['git', 'rev-parse', 'HEAD'], cwd=root, text=True).strip()
compiled_source = receipt['source_sha']
assembly_version = receipt['assembly_version']
if graph['source_commit'] != compiled_source:
    raise SystemExit('The offline graph must describe the source in the current build receipt.')
source_tree = subprocess.check_output(
    ['git', 'ls-tree', '-r', '--full-tree', compiled_source, '--', 'src'],
    cwd=root, text=True).splitlines()
source_blobs = {}
for entry in source_tree:
    metadata, name = entry.split('\t', 1)
    source_blobs[name] = metadata.split()[2]
source_files = [path for path in files if path.is_relative_to(root / 'src')]
source_mismatches = []
for path in source_files:
    content = path.read_bytes()
    blob_sha = hashlib.sha1(f'blob {len(content)}\0'.encode() + content).hexdigest()
    if source_blobs.get(path.relative_to(root).as_posix()) != blob_sha:
        source_mismatches.append(path.relative_to(root).as_posix())
if source_mismatches:
    raise SystemExit('Packaged source differs from the compiled revision:\n'
                     + '\n'.join(source_mismatches))
changed_paths = subprocess.check_output(['git', 'status', '--porcelain'], cwd=root, text=True).splitlines()
manifest = {'schema': 'iom.crafting-design-review.v2',
            'kind': 'crafting-design-review', 'runtime_package': False,
            'repository_baseline_commit': repository_head,
            'source_commit': compiled_source, 'compiled_source_commit': compiled_source,
            'assembly_version': assembly_version,
            'graph_source_commit': graph['source_commit'], 'graph_version': graph['version'],
            'packaged_source_verification': {
                'matching_files': len(source_files),
                'scope': 'Included C# source, assembly metadata, project file and occult runtime PNGs '
                         'match the compiled source commit. Other build inputs remain outside this review.'},
            'working_tree_changes_at_packaging': changed_paths, 'members': []}
start = f'''IMPERIUM REFORGED — CRAFTING SOURCE AND ART REVIEW

Current correction: assembly {assembly_version}, compiled source {compiled_source}.
Finished-ration recycling is removed. One Raw Vat Biomass and one wrapper produce
six Dirty Corpse Starch in two base hours. Native compilation, package verification,
compiled correction checks and all 24 embedded occult PNGs passed. No game was
launched for this corrected binary. See diagnostics/FinishedRationBuildReceipt.json.

The historical screenshots and gameplay receipts document 1.1.9/source 347de7d.
They include the six-node tree, corrected tooltip hovers, bounded map checks,
and corrected-directory Space/time advancement. Session 007 additionally verified
the named Metal smelting feedstock stack changing from 2/30 to 6/30 after one
four-output batch, Focus/double-click, station cooutputs and a no-producer item.
Those observations are not relabeled as gameplay acceptance of 1.1.10.
Research purchases, pattern learning, recycling completion and legacy migration
retain their unperformed transaction scope.

REVIEW_MANIFEST.json distinguishes the repository baseline from the actual compiled
source and offline graph revision. It records current document changes, source
byte verification and a checksum for every included repository file.

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
Historical ship/tree/tooltip evidence and cleanup: docs/crafting/native-occult-ui-347de7d.json
Historical native captures: native-occult-{{ship,tree,culture,manifestation}}-347de7d.png
(the four PNG files are in docs/crafting/)
Historical native tree evidence: docs/crafting/native-occult-tree-bdb1759.json
Verification: diagnostics/RawMaterialChemistryAudit.md
Current build receipt: diagnostics/FinishedRationBuildReceipt.json
Correction scope: diagnostics/FinishedRationOutputCorrection.md
Historical 1.1.9 build/runtime receipt: diagnostics/OccultBuildReceipt.json
Historical session-005 cases and frame hashes: diagnostics/CraftingMapAcceptance-347de7d.json
Historical session-007 continuation: diagnostics/CraftingMapAcceptance-347de7d-007.json
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
The compiled source commit identifies the included implementation. The repository
baseline and working-tree list identify the surrounding documentation snapshot.
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

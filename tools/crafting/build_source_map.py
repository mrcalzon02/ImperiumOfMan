#!/usr/bin/env python3
"""Build the portable recipe-map page and raw-material diagram from a source export.

Requires Python 3 and Graphviz's dot executable for SVG/PNG generation.
The input must be produced by export_source_graph.py; it is not a live-game dump.
"""
import argparse
import base64
import json
import pathlib
import re
import subprocess

ROOT = pathlib.Path(__file__).resolve().parents[2]
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--graph', type=pathlib.Path, required=True)
parser.add_argument('--out', type=pathlib.Path, default=ROOT / 'docs' / 'crafting')
args = parser.parse_args()
source = json.loads(args.graph.read_text(encoding='utf-8'))
assembly_info = ((ROOT / 'src' / 'Properties' / 'AssemblyInfo.cs').read_text(encoding='utf-8')
                 if source['source_ref'] == 'working-tree' else subprocess.check_output(
                     ['git', 'show', source['source_commit'] + ':src/Properties/AssemblyInfo.cs'],
                     cwd=ROOT, text=True))
assembly_version = re.search(r'AssemblyVersion\("([\d.]+)"\)', assembly_info).group(1)
display_version = '.'.join(assembly_version.split('.')[:3]) + ' source'
output = args.out
output.mkdir(parents=True, exist_ok=True)

names = {item['id']: item['title'] for item in source['materials'] if item.get('title')}
names.update({
    'human_meat': 'Piece of Flesh', 'water_bottle_1': 'Water Bottle',
    'acid': 'Acid', 'gas_ammo': 'Gasoline / Gas fuel', 'paper_towel': 'Paper Towels',
    'rags': 'Rags', 'plastic': 'Plastic', 'rubber': 'Rubber', 'glass': 'Glass', 'wire': 'Cable',
    'powder': 'Propellant powder', 'explosives': 'Explosives', 'tin': 'Tin',
    'weapon_parts': 'Weapon components', 'pills_sorbent': 'Sorbent',
    'quasi_medical_kit_1': 'Gavvakh', 'demonic_eye': 'Demonic Eye / Agga component',
})
external = [
    'iom_purity_seal', 'iom_wreck_cyborg_battle', 'iom_wreck_cyborg_worker',
    'iom_wreck_drone_security', 'iom_wreck_turret_ballistic', 'demonic_eye',
    'mars_cyborg_hand', 'mars_gear', 'mars_skin', 'mercury_glass', 'quasi_basic_ammo',
    'quasi_venus_rags', 'venus_armor_plates', 'venus_weapon_parts',
]
graph = {
    'schema': 'iom.crafting.source-map.v1',
    'version': display_version,
    'source_commit': source['source_commit'],
    'source_ref': source['source_ref'],
    'source_kind': 'bounded-source-parser',
    'native_records_loaded': False,
    'names': names,
    'raw_sources': ['iom_metal_ore', 'water_bottle_1', 'human_meat'],
    'external_sources': external,
    'records': source['records'],
    'summaries': source['graphs'],
    'unresolved_calls': source['unresolved_calls'],
    'source_hashes': source['source_hashes'],
    'source_hashes_description': source['source_hashes_description'],
    'direct_recipe_quantity_policy': source['direct_recipe_quantity_policy'],
    'occult_recipe_quantity_policy': source.get('occult_recipe_quantity_policy', 'unknown'),
    'occult_pattern_pools': source.get('occult_pattern_pools', []),
    'limitations': source['limitations'],
}
(output / 'recipe-graph.json').write_text(json.dumps(graph, indent=2) + '\n', encoding='utf-8')
template = (ROOT / 'docs' / 'crafting' / 'recipe-map.template.html').read_text(encoding='utf-8')
payload = json.dumps(graph, separators=(',', ':')).replace('</', '<\\/')
assert template.count('__RECIPE_GRAPH_JSON__') == 1
(output / 'recipe-map.html').write_text(template.replace('__RECIPE_GRAPH_JSON__', payload), encoding='utf-8')

# This exact-recipe subset is intentionally smaller than the complete searchable map.
recipes = {record['outputs'][0]: record for record in graph['records']
           if record['kind'] == 'magnum' and not record['conditional'] and len(record['outputs']) == 1}
targets = ['acid', 'gas_ammo', 'paper_towel', 'rubber', 'plastic', 'iom_reclaimed_fiber',
           'iom_refined_metal', 'iom_raw_silicate', 'iom_conductive_scrap']
nodes = set()


def include(item):
    if item in nodes:
        return
    nodes.add(item)
    for ingredient in recipes.get(item, {}).get('inputs', []):
        include(ingredient)


for target in targets:
    include(target)


def quote(value):
    return json.dumps(value, ensure_ascii=False)


lines = [
    'digraph Chemistry {',
    'graph [rankdir=TB,bgcolor="#101714",pad="0.5",nodesep="0.28",ranksep="0.75",splines=polyline,',
    'fontname="DejaVu Sans",fontcolor="#e8eee6",fontsize=23,labelloc=t,',
    'label="RAW MATERIALS TO SHARED CHEMISTRY\\n' + display_version + ' · every incoming ingredient is required\\nQuantities are effective units per order; time is base hours"];',
    'node [shape=box,style="rounded,filled",fillcolor="#24372b",color="#627b66",',
    'fontname="DejaVu Sans",fontcolor="#e8eee6",fontsize=15,margin="0.16,0.12"];',
    'edge [color="#9eafa0",fontname="DejaVu Sans",fontcolor="#e8eee6",fontsize=12,arrowsize=.7];',
]
for item in sorted(nodes):
    recipe = recipes.get(item)
    name = names.get(item, item.replace('iom_', '').replace('_', ' ').title())
    if name == 'Carbon-rich mineral feedstock':
        name = 'Carbon-rich mineral\nfeedstock'
    if name == 'Metal smelting feedstock':
        name = 'Mixed ore feedstock'
    if name == 'Reactive mineral fraction':
        name = 'Reactive mineral\nfraction'
    label = name
    if recipe and recipe.get('effective_batch') is not None:
        hours = f"{recipe['hours']:g}"
        label += f"\n{recipe['effective_batch']} units · {hours} h"
        if not recipe['inputs']:
            label += '\nNo item ingredients'
    elif item == 'human_meat':
        label += '\nGathered organic source'
    fill = '#394d5a' if item in graph['raw_sources'] else '#4b4028' if item in targets else '#24372b'
    lines.append(f'{quote(item)} [label={quote(label)},fillcolor={quote(fill)}];')
    if recipe:
        for ingredient in recipe['inputs']:
            count = next((q['effective_count'] for q in recipe.get('quantities', []) if q['item'] == ingredient), None)
            lines.append(f'{quote(ingredient)} -> {quote(item)} [label={quote(str(count) if count else "")}];')
lines.extend(['{rank=same; "human_meat"; "water_bottle_1"; "iom_metal_ore";}', '}'])
dot_path = output / 'raw-material-overview.dot'
dot_path.write_text('\n'.join(lines) + '\n', encoding='utf-8')
for extension in ('svg', 'png'):
    subprocess.run(['dot', '-T' + extension, str(dot_path), '-o', str(output / ('raw-material-overview.' + extension))], check=True)

# Keep the current capability reference portable: SVG embeds the exact runtime
# icons; its PNG is a documentation diagram, never a claimed game screenshot.
occult_dot = ROOT / 'docs' / 'crafting' / 'occult-capabilities.dot'
if graph['occult_pattern_pools'] and occult_dot.exists():
    for extension in ('svg', 'png'):
        destination = output / ('occult-capabilities.' + extension)
        subprocess.run(['dot', '-T' + extension, str(occult_dot), '-o', str(destination)],
                       cwd=ROOT, check=True)
        if extension == 'svg':
            svg = destination.read_text(encoding='utf-8')
            def embed_icon(match):
                payload = base64.b64encode((ROOT / match[1]).read_bytes()).decode('ascii')
                return 'style="image-rendering:pixelated" xlink:href="data:image/png;base64,' + payload + '"'
            svg = re.sub(r'xlink:href="(src/Art/OccultProduction/OccultResearch[1-6]\.png)"',
                         embed_icon, svg)
            destination.write_text(svg, encoding='utf-8')
print(json.dumps({'source_commit': graph['source_commit'], 'recipe_records': len(graph['records']),
                  'overview_items': len(nodes), 'html_bytes': (output / 'recipe-map.html').stat().st_size,
                  'output': str(output)}, indent=2))

#!/usr/bin/env python3
"""Export the inspected Imperium of Man source recipe graph to JSON.

This bounded parser covers explicit Magnum calls, station routes, and reviewed
source-generated families. It preserves conditional native fallbacks and lists
unresolved calls. It does not load game assemblies, infer unknown native recipes,
or establish schematic, workbench, research, trade, or quest availability.

Example invocations from the repository root:
    python tools/crafting/export_source_graph.py --out recipe-graph.json
    python tools/crafting/export_source_graph.py --ref COMMIT --out historical-graph.json

Extending a source helper can require a reviewed parser change; an unresolved
call must not be silently treated as a native recipe or a complete route.
"""
import argparse, collections, hashlib, json, pathlib, re, subprocess

ap = argparse.ArgumentParser(description=__doc__)
ap.add_argument('--root', type=pathlib.Path, default=pathlib.Path(__file__).resolve().parents[2],
                help='Repository root; defaults to this script\'s repository.')
ap.add_argument('--ref', help='Optional Git revision to read instead of the working tree.')
ap.add_argument('--out', type=pathlib.Path, required=True, help='Destination JSON file.')
args = ap.parse_args()
ROOT = args.root.resolve()
source_head = subprocess.check_output(['git', 'rev-parse', 'HEAD'], cwd=ROOT, text=True).strip()
source_commit = subprocess.check_output(
    ['git', 'rev-parse', '--verify', '--end-of-options', (args.ref or 'HEAD') + '^{commit}'],
    cwd=ROOT, text=True).strip()
if args.ref:
    revision_paths = subprocess.check_output(
        ['git', 'ls-tree', '-r', '--name-only', source_commit, '--', 'src'], cwd=ROOT, text=True).splitlines()
    paths = [ROOT / path for path in revision_paths if path.count('/') == 1 and path.endswith('.cs')]
else:
    paths = sorted((ROOT / 'src').glob('*.cs'))


def clean(s):
    # Keep string contents and line positions; remove comments for bounded matching.
    return re.sub(r'"(?:\\.|[^"\\])*"|/\*[\s\S]*?\*/|//[^\n]*',
                  lambda m: m[0] if m[0].startswith('"') else '\n' * m[0].count('\n'), s)


src = {
    str(path.relative_to(ROOT)): clean(
        subprocess.check_output(['git', 'show', source_commit + ':' + str(path.relative_to(ROOT))],
                                cwd=ROOT, text=True) if args.ref else path.read_text(encoding='utf-8'))
    for path in paths
}

def split(s,sep=','):
    out=[];start=0;stack=[];quote=False;escape=False
    for i,c in enumerate(s):
        if quote:
            if escape: escape=False
            elif c=='\\':escape=True
            elif c=='"':quote=False
        elif c=='"':quote=True
        elif c in '([{':stack.append(c)
        elif c in ')]}':
            if stack:stack.pop()
        elif c==sep and not stack:out.append(s[start:i].strip());start=i+1
    out.append(s[start:].strip());return out

def balanced(s,start):
    opening=s[start];closing={'(':')','[':']','{':'}'}[opening];depth=0;quote=False;escape=False
    for i in range(start,len(s)):
        c=s[i]
        if quote:
            if escape:escape=False
            elif c=='\\':escape=True
            elif c=='"':quote=False
        elif c=='"':quote=True
        elif c==opening:depth+=1
        elif c==closing:
            depth-=1
            if depth==0:return s[start+1:i],i+1
    raise ValueError('unbalanced')

def calls(s,names):
    pat=r'\b('+'|'.join(re.escape(n) for n in names)+r')\s*(?:<[^>]+>)?\s*\('
    for m in re.finditer(pat,s):
        line=s[s.rfind('\n',0,m.start())+1:m.start()]
        if re.search(r'\b(private|public|internal)\s+(static\s+)?',line):continue
        body,end=balanced(s,s.find('(',m.start(),m.end()))
        yield m[1],split(body),m.start(),end

symbols={};unknown=[];records=[]
def resolve(v,env=None):
    v=v.strip();env=env or {}
    if v in env:return env[v]
    if v in symbols:return symbols[v]
    if re.fullmatch(r'"(?:\\.|[^"\\])*"',v):return json.loads(v)
    parts=split(v,'+')
    if len(parts)>1:return ''.join(resolve(p,env) for p in parts)
    raise ValueError('unresolved '+v)

consts=[(m[1],m[2]) for s in src.values() for m in re.finditer(r'const\s+string\s+([A-Z]\w*)\s*=\s*([^;]+);',s)]
for _ in range(8):
    for k,v in consts:
        try:symbols[k]=resolve(v)
        except ValueError:pass
for s in src.values():
    for m in re.finditer(r'(_\w+)\s*=\s*new\s+\w+(?:\([^)]*\))?\s*\{\s*Id\s*=\s*([^,]+)',s):
        try:symbols[m[1]+'.Id']=resolve(m[2])
        except ValueError:pass
    for m in re.finditer(r'(_\w+)\s*=\s*Create(?:Boots|Leggings|Armor|Helmet)\(\s*([^,]+)',s):
        try:symbols[m[1]+'.Id']=resolve(m[2])
        except ValueError:pass

def quantities(v,env=None):
    v=v.strip()
    if v.startswith('[') and v.endswith(']'):v=v[1:-1]
    if not v:return []
    result=[]
    for part in split(v):
        m=re.fullmatch(r'new\s*(?:ItemQuantity\s*)?\((.*)\)',part,re.S)
        if not m:raise ValueError('unresolved quantities '+part)
        items=split(m[1]);result.append(resolve(items[0],env))
    return result

def put(kind,output,inputs,path,line,method,conditional=False,recipe=None):
    outputs=[output] if isinstance(output,str) else output
    if not outputs:return
    records.append(dict(kind=kind,outputs=outputs,inputs=list(dict.fromkeys(inputs)),source=path,line=line,method=method,conditional=conditional,recipe=recipe))

def failure(name,a,path,line,reason):
    unknown.append(dict(method=name,source=path,line=line,reason=str(reason),arguments=a))

# Interpret quantities only for the two common-helper implementations reviewed
# in this project. Future helper changes leave effective values unknown.
quantity_policy = {'ingredient_rule': 'unknown', 'batch_multiplier': None}
helper_source = src.get('src/Plugin.Industry.cs', '')
helper_match = re.search(r'private\s+static\s+void\s+IndustryRecipe\s*\(', helper_source)
if helper_match:
    _, signature_end = balanced(helper_source, helper_source.find('(', helper_match.start()))
    helper_body, _ = balanced(helper_source, helper_source.find('{', signature_end))
    compact_helper = re.sub(r'\s+', '', helper_body)
    if 'Math.Max(1,input.Count/2+input.Count%2)' in compact_helper and 'RequiredItems=[..fabricationInputs]' in compact_helper:
        quantity_policy['ingredient_rule'] = 'ceil_half_minimum_one'
    elif 'RequiredItems=[..inputs]' in compact_helper:
        quantity_policy['ingredient_rule'] = 'literal_inputs_unchanged'
    if 'IndustryBatches.Add(output,checked(batch*2));' in compact_helper:
        quantity_policy['batch_multiplier'] = 2
    elif 'IndustryBatches.Add(output,batch);' in compact_helper:
        quantity_policy['batch_multiplier'] = 1

names=['IndustryRecipe','SharedRecipe','AddFactionWorkshop','AddSupportWorkshop','AddStationReceipt','AddArmorCatalog','AddArmoryLine','AddMaintenanceLine','RegisterOrdnanceLine','AddDeployableLine','AddDeployableVariantLine','AddPharmaceuticalLine','AddImplant','AddActiveCybernetic','AddReplacement','AddNativeImplant','AddBodyFamily','AddCyberneticsLine','AddCyberneticsBatch']
for path,s in src.items():
    native_guards = []
    for match in re.finditer(r'\bif\s*\(', s):
        condition, condition_end = balanced(s, s.find('(', match.start(), match.end()))
        brace = condition_end
        while brace < len(s) and s[brace].isspace():
            brace += 1
        if 'Data.ProduceReceipts' in condition and 'OutputItem' in condition and brace < len(s) and s[brace] == '{':
            _, guard_end = balanced(s, brace)
            native_guards.append((brace, guard_end, condition))
    for name,a,start,end in calls(s,names):
        line=s.count('\n',0,start)+1
        try:
            if name in ['IndustryRecipe','SharedRecipe']:
                guard = next((condition for guard_start, guard_end, condition in native_guards if guard_start < start < guard_end), None)
                put('magnum',resolve(a[0]),quantities(','.join(a[3:])),path,line,name,conditional=guard is not None)
                if guard is not None:
                    records[-1]['conditional_reason'] = 'Existing native production receipt takes precedence: ' + ' '.join(guard.split())
                if re.fullmatch(r'\d+', a[2]):
                    records[-1]['base_batch'] = int(a[2])
                    if quantity_policy['batch_multiplier'] is not None:
                        records[-1]['effective_batch'] = int(a[2]) * quantity_policy['batch_multiplier']
                try:
                    numeric_inputs = []
                    for raw in a[3:]:
                        matched = re.fullmatch(r'new\s*(?:ItemQuantity\s*)?\((.*)\)', raw.strip(), re.S)
                        pair = split(matched[1])
                        count = int(pair[1])
                        entry = dict(item=resolve(pair[0]), base_count=count)
                        if quantity_policy['ingredient_rule'] == 'ceil_half_minimum_one':
                            entry['effective_count'] = max(1, (count + 1) // 2)
                        elif quantity_policy['ingredient_rule'] == 'literal_inputs_unchanged':
                            entry['effective_count'] = count
                        numeric_inputs.append(entry)
                    records[-1]['quantities'] = numeric_inputs
                    records[-1]['hours'] = float(a[1])
                except (ValueError, TypeError, IndexError):
                    pass
            elif name in ['AddFactionWorkshop','AddSupportWorkshop','AddStationReceipt']:
                i,o,r={'AddFactionWorkshop':(5,6,3),'AddSupportWorkshop':(4,5,2),'AddStationReceipt':(2,3,0)}[name]
                ins=quantities(a[i]);outs=quantities(a[o])
                try:recipe=resolve(a[r])
                except ValueError:recipe=a[r]
                put('station',outs,ins,path,line,name,recipe=recipe)
            elif name=='AddArmorCatalog':
                for out in a[6:]:
                    oid=resolve(out);insert=resolve('HeavyPlate' if out=='SubductorCuirass' else a[5])
                    put('magnum',oid,[insert,resolve('Liner'),resolve('Fittings')],path,line,name)
            elif name=='AddArmoryLine':put('station',[resolve(x) for x in a[5:]],quantities(a[4]),path,line,name)
            elif name in ['AddMaintenanceLine','AddDeployableLine','AddDeployableVariantLine']:
                i=7 if name=='AddDeployableVariantLine' else 6
                ins=quantities(','.join(a[i:]));out=resolve(a[4])
                put('station',out,ins,path,line,name)
                if name=='AddDeployableVariantLine':put('magnum',out,ins,path,line,name)
            elif name=='RegisterOrdnanceLine':
                ins=quantities(','.join(a[7:]));out=resolve(a[0])
                for kind in ['magnum','station']:put(kind,out,ins,path,line,name)
            elif name=='AddPharmaceuticalLine':put('station',resolve(a[4]),quantities(a[5]),path,line,name)
            elif name in ['AddImplant','AddActiveCybernetic','AddReplacement']:
                out='iom_cyber_'+resolve(a[1]);i={'AddImplant':8,'AddActiveCybernetic':7,'AddReplacement':9}[name]
                ins=quantities(a[i]);put('station',out,ins,path,line,name)
                ins=[resolve('Biomass') if x=='quasisamples' else x for x in ins]
                put('magnum',out,ins,path,line,name)
            elif name=='AddNativeImplant':
                if len(a)==2:raise ValueError('native Disassembly materials require installed-game data')
                put('station',resolve(a[1]),quantities(','.join(a[2:])),path,line,name)
            elif name=='AddBodyFamily':
                pre=resolve(a[1]);frame=resolve(a[3]);electronics=resolve(a[4])
                put('station',[pre+'_'+x for x in ['head','chest','guts']],[frame,resolve('ImplantController'),electronics,resolve('SterileScaffold')],path,line,name)
                put('station',[pre+'_'+x for x in ['wrist','feet','knee']],[frame,resolve('ServoCartridge'),electronics],path,line,name)
                put('station',pre+'_hand',[pre+'_wrist',resolve('ServoCartridge'),'weapon_parts'],path,line,name)
                put('station',pre+'_leg',[pre+'_feet',resolve('ServoCartridge'),'metal_tread'],path,line,name)
            elif name=='AddCyberneticsLine':put('station',resolve(a[2]),quantities(','.join(a[3:])),path,line,name)
            elif name=='AddCyberneticsBatch':put('station',[resolve(x) for x in a[4:]],quantities(a[3]),path,line,name)
        except (ValueError,IndexError) as e:failure(name,a,path,line,e)

# Occult formulas deliberately bypass the common half-input/double-output helper.
# Interpret the reviewed literal calls and the one explicit Mars body-part loop.
# Native outputs remain conditional: an installed receipt or missing item can
# cause RegisterOccultRecipe to omit a candidate before creating its chip pool.
occult_records = []
occult_titles = {}
occult_research = src.get('src/Plugin.OccultResearch.cs', '')
for match in re.finditer(r'(OccultRank\w+)\s*=>\s*("[^"\n]+")', occult_research):
    occult_titles.setdefault(resolve(match[1]), resolve(match[2]))
occult_helper = src.get('src/Plugin.OccultProduction.cs', '')
helper = re.search(r'private\s+static\s+void\s+RegisterOccultRecipe\s*\(', occult_helper)
occult_literal_quantities = False
if helper:
    _, signature_end = balanced(occult_helper, occult_helper.find('(', helper.start()))
    body, _ = balanced(occult_helper, occult_helper.find('{', signature_end))
    compact = re.sub(r'\s+', '', body)
    occult_literal_quantities = ('IndustryBatches.Add(output,batch);' in compact and
                                'RequiredItems=[..inputs]' in compact)
for path, s in src.items():
    aliases = {}
    for match in re.finditer(r'\bvar\s+(\w+)\s*=\s*(OccultRank\w+)\s*;', s):
        aliases[match[1]] = resolve(match[2])
    for name, a, start, end in calls(s, ['RegisterOccultRecipe']):
        line = s.count('\n', 0, start) + 1
        try:
            environments = [aliases]
            if a[0] == '"mars_cyborg_" + part':
                loop = re.search(r'foreach\s*\(string\s+part\s+in\s+new\[\]\s*\{([^}]+)\}\)\s*$', s[:start])
                if loop is None:
                    raise ValueError('unreviewed generated occult output loop')
                environments = [{**aliases, 'part': resolve(value)} for value in split(loop[1])]
            for env in environments:
                output = resolve(a[0], env)
                perk = resolve(a[1], env)
                own_stock = output in {symbols.get('RawClone'), symbols.get('TormentImprint'),
                                       symbols.get('SufferingEssence')}
                put('magnum', output, quantities(','.join(a[4:]), env), path, line, name,
                    conditional=not own_stock)
                record = records[-1]
                record['capability'] = {'id': perk, 'title': occult_titles.get(perk, perk)}
                record['knowledge_rule'] = 'One pattern learned from a physical in-game chip; research grants capability only.'
                record['registration_rule'] = 'Candidate only; existing receipts and missing native records are skipped before chip pools are created.'
                if not own_stock:
                    record['conditional_reason'] = record['registration_rule']
                if occult_literal_quantities:
                    record['base_batch'] = record['effective_batch'] = int(a[3])
                    record['hours'] = float(a[2])
                    record['quantities'] = []
                    for raw in a[4:]:
                        match = re.fullmatch(r'new\s*(?:ItemQuantity\s*)?\((.*)\)', raw.strip(), re.S)
                        pair = split(match[1])
                        count = int(pair[1])
                        record['quantities'].append(dict(item=resolve(pair[0], env), base_count=count,
                                                         effective_count=count))
                occult_records.append(record)
        except (ValueError, TypeError, IndexError, AttributeError) as error:
            failure(name, a, path, line, error)

# Capture declared physical knowledge sources without claiming that the installed
# game accepted every candidate. Empty runtime pools are omitted by the mod.
occult_pools = []
site_ids = []
chaos_faction_source = src.get('src/Plugin.ChaosFaction.cs', '')
sites = re.search(r'ChaosSites\s*=\s*\[', chaos_faction_source)
if sites:
    body, _ = balanced(chaos_faction_source, chaos_faction_source.find('[', sites.start()))
    for declaration in split(body):
        arguments, _ = balanced(declaration, declaration.find('('))
        site_ids.append(resolve(split(arguments)[0]))
for path, s in src.items():
    for name, a, start, end in calls(s, ['AddOccultPatternPool']):
        line = s.count('\n', 0, start) + 1
        try:
            site = re.fullmatch(r'ChaosSites\[(\d+)\]\.Id', a[3])
            supplier = site_ids[int(site[1])] if site else resolve(a[3])
            pool = dict(id='iom_occult_' + resolve(a[0]) + '_pattern_chip', title=resolve(a[1]),
                        faction=resolve(a[2]), supplier=supplier, tier=int(a[4]),
                        candidates=[resolve(value) for value in a[5:]],
                        patterns_per_chip=1, source=path, line=line)
            occult_pools.append(pool)
        except (ValueError, IndexError) as error:
            failure(name, a, path, line, error)
for record in occult_records:
    record['knowledge_sources'] = [pool for pool in occult_pools if record['outputs'][0] in pool['candidates']]

# Expand Chaos families from the actual registered tier data; execute only the
# dependency-selection branches of the source helpers, not the game API.
chaos_path='src/Plugin.ChaosEquipment.cs';s=src[chaos_path]
for m in re.finditer(r'var\s+(\w+)\s*=\s*ChaosItem<\w+>\(\s*([^,]+),',s):
    try:symbols[m[1]+'.Id']=resolve(m[2])
    except ValueError:pass
m=re.search(r'_chaosSupply\s*=\s*\[',s)
tiers={}
if m:
    body,_=balanced(s,s.find('[',m.start()))
    for tier_raw in split(body):
        inside,_=balanced(tier_raw,tier_raw.find('('));a=split(inside);level=int(a[0])
        w=next(calls(a[1],['ChaosWeapon']))[1];armor=next(calls(a[2],['ChaosArmor']))[1]
        salvage_call=next(calls(a[3],['ChaosSalvage','ChaosRepairSalvage']));sv=resolve(salvage_call[1][1])
        if salvage_call[0]=='ChaosSalvage':sv=resolve('ChaosItemPrefix')+sv
        tiers[level]=dict(weapon=resolve('ChaosItemPrefix')+resolve(w[1]),armor=resolve('ChaosItemPrefix')+resolve(armor[1]),salvage=sv,utility=resolve(a[4]+'.Id'))
cp='src/Plugin.ChaosProduction.cs';s=src[cp]
for level,t in tiers.items():
    env={'tier.Weapon.Id':t['weapon'],'tier.Salvage.Id':t['salvage'],'salvage':t['salvage'],'id':t['utility']}
    if level<10:
        ins=[t['salvage'],resolve('Metal' if level<=3 else 'Sheet' if level<=6 else 'HeavyPlate'),'rod_parts' if level in (1,3) else 'weapon_parts']
        put('magnum',t['weapon'],ins,cp,20,'ChaosWeaponFamily')
    insert=resolve('Textile' if level==1 else 'Flak' if level==2 else 'Plate' if level<=6 else 'HeavyPlate')
    for slot in ['head','torso','legs','boots']:
        put('magnum',t['armor']+'_'+slot,[t['salvage'],insert,'rubber' if slot=='boots' else resolve('Liner')],cp,31,'ChaosArmorFamily')
    mm=re.search(r'\b'+str(level)+r'\s*=>\s*(\[[^;]+?\])\s*[,\n]',s)
    if mm:put('magnum',t['salvage'],quantities(mm[1]),cp,s.count('\n',0,mm.start())+1,'ChaosMaterialSwitch')
    for name,a,start,end in calls(s,['IndustryRecipe']):
        before=s[max(0,start-14):start]
        if re.search(r'case\s+'+str(level)+r'\s*:\s*$',before):
            put('magnum',t['utility'],quantities(','.join(a[3:]),env),cp,s.count('\n',0,start)+1,'ChaosUtilitySwitch')
    if level==2:put('magnum',resolve('BoundAsh'),['demonic_eye',t['salvage']],cp,43,'ChaosBoundAsh')
    if level==6:put('magnum',resolve('ForgeEmber'),[resolve('BoundAsh'),t['salvage'],'circuit_board'],cp,48,'ChaosForgeEmber')
# Pact manufacture derives its tier's salvage and lower essence, never another skull.
ps=src['src/Plugin.ChaosPacts.cs'];m=re.search(r'ChaosPacts\s*=\s*\[',ps)
if m:
    body,_=balanced(ps,ps.find('[',m.start()))
    for cm in re.finditer(r'\bnew\(',body):
        inner,_=balanced(body,body.find('(',cm.start()));a=split(inner)
        if len(a)<4:continue
        key=resolve(a[0]);pact_tier=int(a[3]);level=2*pact_tier-1
        if key==resolve('FurySigilKey'):continue
        put('magnum',resolve('ChaosItemPrefix')+'skull_'+key,['demonic_eye',resolve('ForgeEmber' if level>=7 else 'BoundAsh' if level>=3 else 'Textile'),tiers[level]['salvage']],cp,58,'ChaosPactFamily')

# Recovered-machine recipes have known recorded wreck IDs (four entries).
p='src/Plugin.CombatEngineerClass.cs';s=src[p];m=re.search(r'EngineerMachines\s*=\s*\[',s)
if m:
    body,_=balanced(s,s.find('[',m.start()))
    for cm in re.finditer(r'\bnew\(',body):
        inner,_=balanced(body,body.find('(',cm.start()));a=split(inner);role=resolve(a[0])
        put('magnum','iom_recovered_'+role,['iom_wreck_'+role,resolve('Fittings'),resolve('LogicCore')],p,54,'RecoveredMachineFamily')

# Conditional support fabrication must not be asserted as active: native recipes
# take precedence. Keep explicit fallback patterns separate from the main graph.
p='src/Plugin.SupportProduction.cs';s=src[p];fallback={}
for m in re.finditer(r'("[^"\n]+")\s*=>\s*(\[[\s\S]*?\])\s*,',s):
    try:fallback[resolve(m[1])]=quantities(m[2])
    except ValueError:pass
for name,a,start,end in calls(s,['RegisterSupportTier']):
    for raw in a[4:]:
        try:product=resolve(raw)
        except ValueError as e:failure(name,[raw],p,s.count('\n',0,start)+1,e);continue
        if any(product in r['outputs'] for r in records if r['kind']=='magnum'):continue
        if product in fallback:ins=fallback[product]
        elif product.startswith('cyborg'):ins=[resolve('Fittings'),resolve('Metal'),'wire']
        elif product in ['packaged_industrial_robot','packaged_recreational_robot','packaged_security_robot']:
            chassis={'packaged_industrial_robot':'cyborg','packaged_recreational_robot':'cyborg_recreation','packaged_security_robot':'cyborg_battle'}[product]
            ins=[chassis+'_'+part for part in ['head','knee','hand','leg']]+[resolve('AutomataChassis'),resolve('LogicCore')]
        else:
            failure('EnsureSupportRecipe',[product],p,s.count('\n',0,start)+1,'native production recipe or Disassembly fallback requires installed-game data');continue
        put('magnum',product,ins,p,s.count('\n',0,start)+1,'EnsureSupportRecipeFallback',conditional=True)

def graph_report(rr):
    adj=collections.defaultdict(set);edge_records=collections.defaultdict(list)
    for i,r in enumerate(rr):
        for o in r['outputs']:
            adj[o]
            for ing in r['inputs']:adj[ing].add(o);edge_records[(ing,o)].append(i)
    index=0;indices={};low={};stack=[];on=set();components=[]
    def visit(v):
        nonlocal index
        indices[v]=low[v]=index;index+=1;stack.append(v);on.add(v)
        for w in adj[v]:
            if w not in indices:visit(w);low[v]=min(low[v],low[w])
            elif w in on:low[v]=min(low[v],indices[w])
        if low[v]==indices[v]:
            comp=[]
            while True:
                w=stack.pop();on.remove(w);comp.append(w)
                if w==v:break
            if len(comp)>1 or v in adj[v]:components.append(sorted(comp))
    for node in list(adj):
        if node not in indices:visit(node)
    detail=[]
    for comp in components:
        nodes=set(comp);ids=set()
        for (i,o),rs in edge_records.items():
            if i in nodes and o in nodes:ids.update(rs)
        detail.append(dict(items=comp,recipes=[rr[i] for i in sorted(ids)]))
    return dict(records=len(rr),distinct_products=len(set(o for r in rr for o in r['outputs'])),nodes=len(adj),edges=sum(map(len,adj.values())),cycles=detail)

material_records=[]
for path,s in src.items():
    for name,a,start,end in calls(s,['AddMaterial']):
        try:
            item_id=resolve(a[0]); art=item_id
            for argument in a[1:]:
                if argument.strip().startswith('artwork:'):
                    art=resolve(argument.split(':',1)[1])
            try:
                title = resolve(a[2])
            except ValueError:
                title = None
            material_records.append(dict(id=item_id,title=title,donor=resolve(a[1]),artwork=art,source=path,line=s.count('\n',0,start)+1))
        except ValueError:
            pass

report = dict(
    symbols=symbols,
    materials=material_records,
    source_root=str(ROOT),
    source_ref=args.ref or 'working-tree',
    source_commit=source_commit,
    source_head=source_head,
    source_hashes_description='SHA-256 of normalized UTF-8 C# text: comments removed and their line breaks preserved. These are not raw-file hashes.',
    source_hashes={path: hashlib.sha256(value.encode('utf-8')).hexdigest() for path, value in src.items()},
    direct_recipe_quantity_policy=quantity_policy,
    occult_recipe_quantity_policy='literal_inputs_and_batches' if occult_literal_quantities else 'unknown',
    occult_pattern_pools=occult_pools,
    record_count=len(records),
    by_method=dict(collections.Counter(record['method'] for record in records)),
    graphs={},
    records=records,
    unresolved_calls=unknown,
    limitations=[
        'This is a bounded source parser of the inspected helpers and top-level src/*.cs files, not a C# interpreter or an installed-game export.',
        'No native game records are loaded. Native implant Disassembly arrays and unmodified native/support recipe inputs remain unknown; unresolved_calls retains the inspected cases.',
        'Conditional fallbacks are separate because existing native ProduceReceipts take precedence. A fallback is not proof of the active native recipe.',
        'Factories can have multiple outputs; no edges between co-outputs are created.',
        'Cycles are graph candidates, not proof of a bootstrap lock when alternative independent recipes exist.',
        'Retired station receipt definitions remain visible as legacy metadata; separate source review governs whether new orders schedule them.',
        'Recycling profiles are intentionally excluded from manufacturing graphs; recovery can create deliberate material return paths.',
        'String constants and generated family IDs are resolved only through the inspected source patterns. Unresolved calls remain explicit and must be reviewed when source helpers change.',
        'Direct literal IndustryRecipe/SharedRecipe calls include available base inputs, batch sizes, and times. Effective values are supplied only for the recognized common-helper policy; generated-family quantities and native quantities remain unknown.',
        'Occult calls use separately verified literal quantities. Their declared capability and physical chip sources do not prove that a runtime recipe was registered or that the player has acquired either requirement.',
        'Dependency edges do not establish schematic, workbench, research, tech-tier, trade, quest, or reward access. The export cannot establish whole-game crafting coverage.',
        'The legacy Guard clip receipt is superseded by the later explicit IndustryRecipe for the same output; the active explicit route is represented.',
    ],
)
for kind in ['magnum','station']:
    report['graphs'][kind]=graph_report([r for r in records if r['kind']==kind and not r['conditional']])
report['graphs']['combined']=graph_report([r for r in records if not r['conditional']])
report['graphs']['combined_with_conditional_support']=graph_report(records)
report['graphs']['combined_without_explicit_station_recovery']=graph_report([r for r in records if not r['conditional'] and r['recipe'] not in ['iom_cryptoria_robot_salvage','iom_ares_post_wiring_recovery']])
args.out.parent.mkdir(parents=True, exist_ok=True)
args.out.write_text(json.dumps(report,indent=2)+'\n', encoding='utf-8')
summary={k:v for k,v in report.items() if k in ['source_ref','source_commit','record_count','by_method','direct_recipe_quantity_policy','limitations']}
summary['graphs']={k:{**v,'cycles':[c['items'] for c in v['cycles']]} for k,v in report['graphs'].items()}
summary['unresolved_count']=len(unknown)
print(json.dumps(summary,indent=2))

# Production chains and faction facilities

Design draft for Quasimorph. Production from raw stock should be slow enough that a crew has a reason to look for finished supplies, battlefield salvage and better crafting facilities. Mining and growing give the crew a way to make its own supply when trade or loot falls short. Building that capacity takes investment.

## Set the scale

Add **one or two meaningful processing steps** to existing one-step recipes where the material warrants it. Use roughly three operations for a simple manufactured item. Products with several independent inputs can have longer parallel branches. Harvesting or growing supplies the raw material. Refining makes usable stock, and fabrication turns that stock into parts that can go into final assembly.

Use time, equipment access, parts and cargo space to make starting from raw stock a deliberate commitment. Give each operation a distinct output so it can also appear in loot, trade or a different recipe. Let an unlocked bench process a batch in one order, while keeping the wait and material cost of each stage. Access to the right equipment should make the chain worth building when the crew needs a steady supply.

## Make battlefield salvage valuable

Each production chain should give raiders something worth bringing home at several stages. Finished items are ready to use. Late-stage components skip the most work and can justify carrying them over a bulky load of ore. Refined stock is useful across several recipes. Raw stock gives an equipped crew more control over what it produces.

| What the crew recovers | Work it can skip |
| --- | --- |
| An intact item from troops, lockers or dispatch crates | The entire crafting chain. |
| An armor insert, filter or sealed cell from a repair bench or production rack | The upstream refinement and fabrication for that part. |
| Refined metal or processed fiber from rack storage | The mine or grow trip and initial processing. |
| Ore or unprocessed biomass from an extraction site | Sourcing the raw feedstock. |

Use the room's function to telegraph which salvage is likely. An armor press can hold finished inserts, a barracks can hold complete kit, and a mining site can mainly yield ore. Track real cargo cost, prices, bench time and the number of orders a recovered part avoids. Use loot frequency, recipe price, batch yield and processing time as separate tuning values.

## Representative supply chains

| Product | Raw supply | Refining | Component manufacture | Final output |
| --- | --- | --- | --- | --- |
| Metal stock and parts | Ore from an asteroid work site | Smelt ore into refined metal stock. | Roll plate or draw rod from stock, then cut or stamp usable fittings, inserts or brackets where required. | Use the part in equipment assembly or trade it as an intermediate. |
| Las Clips | Silicate rubble from a mining site and chemical feedstock from station salvage or trade | Silicate goes to industrial glass. Feedstock goes to a cell electrolyte. | A cell bench makes a sealed cell assembly from both inputs. | A Guard supply line or the Magnum packs a batch of Las Clips. |
| Arbites carapace | Alloy-bearing ore and textile fiber from a vat or station supplier | Smelt alloy plate stock and process the fiber into industrial cloth. | An armor press makes overlapping plate inserts. A textile bench makes padded liners. | Final assembly combines inserts, liners and fasteners into a helmet, cuirass, greaves or boots. |
| Corpse Starch | Biomass feedstock and water or nutrient stock | A closed vat produces a nutrient slurry. | A food line presses a ration base and prepares wrappers from packaging stock. | A packaging line yields sealed rations. Damaged or abandoned facilities can supply a contaminated salvage variant. |
| Electronics and sensors | Conductive scrap, trace mineral concentrate and recovered circuit stock | Produce conductive stock and cleaned substrates. | A component line builds circuit assemblies. | Combine assemblies with casing and fittings for a tool, sensor or upgrade. |

The crew can buy a finished stack, assemble components found on a raid, or commit to the full raw-material chain. Give the low-tier Guard a reliable market and loot supply while making self-sufficiency more demanding. In the metal route, finding a usable bracket or plate insert skips two or three orders and gives the crew a reason to search a repair shop instead of hauling more ore.

## Cigarettes and manufacturer recipes

The player buys the cigarette recipe from the faction that makes the cigarettes. Use DaydreamChems as the proposed first manufacturer because its planned facilities already cover growing and chemical processing. Its producing station sells the recipe as a learnable schematic, trades finished stock, and has room loot that reflects its production. Learning the schematic unlocks the cigarette-specific orders at compatible benches. Generic board stock and other shared materials can come from unrelated suppliers.

| Operation | Inputs and output | Where the crew can recover the output |
| --- | --- | --- |
| Grow and prepare tobacco | Grow tobacco in a dedicated room. Harvest it and prepare it into tobacco stock for the rolling bench. | Grow-room harvest trays and leaf storage. |
| Make filters | Turn chemical feedstock and filter media into unfinished filters, then cut them to size in batches at the component bench. | Filter racks and production crates. |
| Make boxes | Fold and assemble box blanks from paperboard stock. | Carton line and packaging stores. |
| Roll loose cigarettes | Combine prepared tobacco, rolling paper and filters at the rolling bench. | Rolling-line trays and work-in-progress crates. |
| Pack boxes | Combine loose cigarettes and assembled boxes at the packing bench to yield the existing base-game cigarette item, `cigarettes_1`. | Dispatch crates, vendors and ordinary finished-item loot. |

Boxes and filters can be made in parallel with growing. Salvaged loose cigarettes and spare boxes reduce the chain to a single packing order once the crew has learned the recipe. An intact pack is usable as soon as it is recovered. The installed crafting table already offers `cigarettes_1` directly from four sorbent pills at the medical printer. The overhaul would replace that direct crafting recipe so the faction purchase and packing step matter. Keep the existing item ID and its downstream uses, including the medical kit recipe that consumes it.

For faction recipes in general, record the manufacturer, which station sells the schematic, its price, compatible benches, and which orders it unlocks. Keep the unlock after purchase and save/reload. For the first pass, test who sells future copies if the manufacturing station changes hands.

## Collection and throughput

- **Asteroid work sites** begin as a small number of mapped mining locations using the current orbital framework and mine presets. A visit can supply ore and silicate through drill-side bins, broken mineral seams and cargo lockers. An exhausted loadout or an occupied room changes how much the crew brings home. Add travel, mining rights or autonomous extraction only after one manually visited site supports the full chain.
- **Grow rooms** take feedstock through loading, growth and collection. Use distinct game resources for nutrient stock, vat biomass and processed food. Harvest yields components that can move through station stock and shipping. Vat cycles should be easy to read from the room and terminal.
- **Batch sizes** should match use. A normal ammo batch should support a meaningful portion of a raid. A single ore haul should feed multiple assemblies. Preserve one production order per stage and account for stack caps. Native player and station recipe records have different output shapes, so design and verify batch behavior at the corresponding entry point.
- **Power and stalls** should create legible choices. Required input appears in demand. A delayed cycle has a readable reason and a recovery path such as delivery, repair or routing from another facility. Keep raw hauls, transit inventory and usable outputs distinct so the economy can be audited.

## Give factions production identities

These are proposed facility roles built on the existing station and faction structure. Assign a test facility to an existing station before placing new sites on the map.

| Faction | Facility identity | Visible on a raid | Trade specialty |
| --- | --- | --- | --- |
| **Imperium** | Terra assembles formal requisition equipment. Phobos makes munitions, Luna runs ration lines, and Havoc handles heavier fabrication. | Armory benches, inspection racks, freight carts and stenciled stock lanes. | Guard cells and rations at the early stations, then selected regimental and Arbites assemblies. |
| **RealWare** | Bulk metallurgy and automated heavy fabrication. | Crucible bay, hot conveyor and stamping press, including unfinished plate and rejection bins. | Plate stock, fasteners and heavy casings. |
| **DaydreamChems** | Bio-processing, chemical refinement and sterile vessel supply. Proposed first cigarette manufacturer. | Feed pumps, instrument windows, lit vats and a purge or recovery bay. Its cigarette plant can have a grow room, rolling benches and a packing line. | Nutrient media, processed biomass, fictional intermediates for cells or medicine, and the proposed cigarette recipe and finished packs. |
| **SBN** | Fine electronics and clean component manufacturing. | Substrate loading rack, assembly machines, quality-control station and clean packing bench. | Control boards, sensors and specialty components. |
| **CoreWard** | Extraction logistics and rough material sorting. | Mining berth, cutter service bay, ore bins and dust extraction. | Mixed raw ore, silicate and first-pass concentrate. |
| **PlanetBridge** | Cross-faction logistics, recycling and cargo consolidation. | Sorting depots with incoming crates, inspection, repacking and dispatch. | Reclaimed components and freight between stations with complementary demand. |

Start with one Imperial, one mining and one vat specialization. Additional factions can reuse room footprints with different machinery, dressing, products and defensive roles. Keep faction ownership, the physical facility and the station's assigned recipes together so that capture or interrupted trade has a predictable effect on output.

## Room design brief

Every production room should show **what came in, what is happening to it, and where the result goes**. Readable silhouettes and prop placement should convey the process from the doorway. Each room type needs a compact variant and enough exits, cover or alternate lanes for an actual mission.

| Room | Incoming → process → outgoing | Gameplay or visual prop |
| --- | --- | --- |
| Asteroid cutter berth | Loose rock and cutter tools → serviced drill rig → sealed ore bins | Docked cutter silhouette, compressed cargo lane, emergency bypass. |
| Ore sorting line | Mixed rubble → enclosed screen and conveyor → separate concentrate bins | Two different output containers and one motor controls station power. |
| Foundry and rolling bay | Charge carts → furnace and caster → stacked slate or plate | Lit hot zone, cool maintenance route and distinct unfinished stock. |
| Cell fabrication line | Glass and electrolyte trays → cell bench and inspection → packed Las Clips | Cells in test holders, reject cage and reserve stock. |
| Armor press | Plate stock and fasteners → segmented stamping die → unfinished plates | Heavy press profile, plate rack, quality-control lane. |
| Liner and harness bench | Fiber bolts → sewing or bonding stations → padded liners and rigging | Rolls, offcuts, known output color and repaired gear bins. |
| Growing-vat aisle | Nutrient drums → lit vessels and feed pipes → spent or harvest-ready trays | Idle, growing and ready lighting states. A pump or sampling station gives mission objectives a physical target. |
| Ration packing room | Pressed nutrient base and wrappers → seal and inspection line → packed ration crates | Distinct sealed output and contaminated salvage return. |
| Tobacco grow and prep room | Grow trays → harvest and preparation racks → prepared tobacco bins | Different tray states, leaf storage and ready-to-haul stock. |
| Cigarette assembly and packing line | Tobacco, filters, rolling paper and box blanks → rolling bench and packing station → boxed cigarettes | Filter racks, folded cartons, loose cigarettes in trays and finished boxes at dispatch. |
| Electronics clean line | Board stock and trays → fine assembly and test → protected component boxes | Light bench line, open false-floor access when adapted for a raid, compact high-value loot. |
| Dispatch and salvage | Finished stock or returns → inspection, salvage or palletizing → outgoing freight | Cargo manifests, faction stock markings and crates with actual production items. |

Use the native map preset workflow for an initial room rotation. Author rooms for existing Industrial, Mine, Lab and orbital preset families. Validate doors and connectors, cover, objectives and item visibility over multiple seeded layouts. [Quasimorph developer modding guide](https://steamcommunity.com/sharedfiles/filedetails/?id=3281671312)

## Connect the room to the economy

The installed build already has Magnum production orders, station barter recipes with inputs and outputs, and station-to-station shipping. For the first version, reuse those paths and register a small, auditable production graph. A station recipe and its floor presets should draw from the same list of facility products. Incoming supplies, material racks and output bins should seed loot from their corresponding stage and technology. Keep loot and economic stock accounted for so a raid may advance one system without multiplying goods in the other.

Some vanilla station recipes keep producing more slowly when inputs are absent. For facilities that are meant to depend on harvested feedstock, prototype strict input accounting for the new recipes and show stalled production in the facility's interface. Preserve the normal behavior of existing station recipes. Later, a mission result such as a rescued shipment or repaired feed line can affect the actual site's stock or queue if the same state remains valid through save and load.

## Build order and acceptance

1. **Baseline.** Inventory the current recipes, prices, acquisition routes and station output cadence. Choose a single Las Clip chain as the three-step benchmark. Keep the existing Guard supply available during the prototype.
2. **One complete ore loop.** Add one mining preset, one refining room and one cell line. Follow one raw batch through collection, processing, shipping or trade, final manufacture, and use on a raid.
3. **One grow-to-pack loop with a faction recipe.** Use cigarettes to test schematic purchase, independent filter and box production, rolling, and packaging into the existing item. Test a second run that starts with salvaged loose cigarettes and boxes. Reuse those grow-room and packing conventions for the ration chain.
4. **Expand suppliers.** Add RealWare plate stock, DaydreamChems biomass, SBN components and PlanetBridge dispatch one at a time. Verify each link before making other recipes depend on it.
5. **Add mid-tier recipes and variants.** Move Arbites armor onto the longer route once a player can obtain both plates and liners through normal supply. Add alternate layouts and then tie facility state to compatible mission objectives.

Acceptance includes one full source-to-equipment test, economy totals before and after production, a check for stuck dependencies and profitable conversion loops, lost and captured station scenarios, and save/reload during an active order. Compare the same item made from raw stock, assembled from battlefield salvage and acquired finished. Confirm the seller requires a purchase to teach the recipe and that the old one-step cigarette craft cannot bypass it. View the actual generated rooms from a player's approach and verify that input, machinery and output read clearly before opening a container or terminal.

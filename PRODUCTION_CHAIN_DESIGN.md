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
| Corpse Starch | Raw meat and bottled water | A closed vat produces raw nutrient biomass. | A food line presses a ration base and prepares wrappers from packaging stock. | A packaging line yields sealed rations. Damaged or abandoned facilities can supply a contaminated salvage variant. |
| Electronics and sensors | Ore, trace mineral concentrate and recovered circuit stock | Separate conductive material from ore and draw Cable. Recovered conductors can skip the separation order. | A component line builds circuit assemblies. | Combine assemblies with casing and fittings for a tool, sensor or upgrade. |

The crew can buy a finished stack, assemble components found on a raid, or commit to the full raw-material chain. Give the low-tier Guard a reliable market and loot supply while making self-sufficiency more demanding. In the metal route, finding a usable bracket or plate insert skips two or three orders and gives the crew a reason to search a repair shop instead of hauling more ore.

## Utility, food, and standard supplies by tech level

Each row lists what the tier adds to the Imperial supply line. Earlier Imperial stock stays in the pool as the faction advances. Station type affects which classes are stocked, and mission rewards use the native reputation limits. Each requisition chip contains one randomly selected recipe from its listed pool and unlocks that recipe on the Magnum. Its selection is saved on the chip and shown in the tooltip; further chips can contain other recipes or already known duplicates. Normal weapon-ammunition and native armor-set companion unlocks still apply. Listed facility stock and native loot give additional ways to acquire the same item.

| Imperial tech | Food and drink | Utility and field supplies | Standard ammunition and equipment |
| --- | --- | --- | --- |
| **1 · Planetary Defence Force** | Corpse Starch, bottled water and standard ration packs enter Imperial stock. Luna packs clean Corpse Starch. Clean and Dirty Corpse Starch are basic loot on every newly generated Imperial raid floor. | Bandages and splints. Raw biomass preparation is shared; the Imperial ration line schematic can contain a ration base, wrapper or packing recipe. | Las Clips, service Lasgun and PDF flak. Imperial Field Plans draw one recipe from cell stock, sealed cells, empty clip housings and final packing. |
| **2 · Cadian Guard** | — | Hydrogel bandages and melee repair kits. | Cadian flak and Guard Frag Grenades. Phobos produces grenades alongside Las Clips. |
| **3 · Arbites, Valhallans, Steel Legion** | Valhallan Tanna enters Imperial stock and Luna's specialist supply. It reduces existing cold buildup. | Field medkits, Steel Legion Smoke Grenades and Spare Filters. Luna supplies Winter insulation and Havoc supplies Steel Legion seal lining. Both components are also taught by their regimental chips. | Standard shotgun shells for Judgement. Arbites carapace, winter gear, sealed Steel Legion gear, Long-Las and Plasma Pistol. Servitor backpack enters the equipment pool. |
| **4 · Elysians** | — | Elysian Drop Harness and firearm repair kits. Native smoke grenades are carried by Elysian spotters. | Standard Heavy rifle ammunition for the Bolter. The Drop Lascarbine uses the existing BatteryCells supply. |
| **5 · Death Korps of Krieg** | — | Advanced medkits, Krieg Demolition Charge and Entrenching Tool. | Gas fuel for the Melta Rifle. Krieg weapons draw from the existing BatteryCells and shotgun shell supply. |
| **6 · Tempestus Scions** | — | Tempestus Targeter and armor repair kits. | Compact Hot-shot Power Packs and Scion carapace. |
| **7 · Skitarii** | — | Skitarii Auspex, Field Repair Kit and Recovered Circuit Assembly. | The Galvanic Rifle uses Heavy rifle ammunition. The Arc Rifle uses BatteryCells. |
| **8 · Sororitas** | — | Devotional Ward for burn treatment. The Sororitas Reliquary is a contract objective. | Armor-piercing Heavy rifle rounds join standard Imperial stock. The Sororitas Bolter uses Heavy rifle ammunition and the Flamer uses gas. |
| **9 · Ordo Xenos** | — | The Phobos Quarantine contract opens. | Standard Toxic ammunition supports the Needle Pistol. The Power Blade uses BatteryCells. |
| **10 · Grey Knights** | — | Aegis Ward for Chaos wounds and active Quasimorphosis. Titan and Broadsword also have local repair supplies. | Paladin Terminator armor, Nemesis Force Sword and Psycannon. They use the existing BatteryCells and Heavy rifle supply. |
| **11 · Golden Paladins** | — | Golden Aegis Ward and the Golden Paladin Guard contract. | Gilded Terminator armor, Golden Nemesis Force Sword and Golden Psycannon. Both weapons use the same ammunition families as the Tech 10 versions. |

Each newly generated Imperial raid floor supplies one clean Corpse Starch per 200 reachable, clear floor cells, with a minimum of four and a maximum of sixteen, plus one Dirty Corpse Starch per four clean rations (rounded up). The first clean ration is near the arrival point; remaining rations go into distinct ordinary food-compatible containers with reachable access, or onto reachable floor tiles if those containers are absent or full. This applies from Tech 1 on every floor, including later stages; loading a previously generated floor does not replenish it. The added supply ranges from 2,060 to 8,240 calories per floor. Dirty Corpse Starch retains its existing health and poisoning penalties.

### Manufacturing and freight

These tiers belong to the manufacturer named in each row. Finished base-game goods remain available through their native trade and loot sources. The new multi-step Magnum production orders require the corresponding schematic.

In 1.1.3, each custom Magnum recipe retains every ingredient type while its required quantities are halved, rounded up to whole items with a minimum of one. Each order produces twice its previous batch. Production times, recipe IDs, chip pools, and unlock requirements stay the same. Faction factory batches and existing native support recipes keep their own quantities. Existing queued orders retain the output and paid ingredients recorded when they were started. [Balance scope and examples](diagnostics/RecipeCostBatchBalance.md)

In 1.1.4, explicit raw-material and ordinary component recipes extend those rules. Biomass and conductor separation become shared foundations; newly generated ration and workshop chips no longer roll those already-known recipes. New support chips likewise omit shared gasoline production while retaining finished fuel in trade stock. Existing saved chip selections and learned IDs remain valid. Selected native Magnum ingredient lists now use the new chemical and component routes; other native support recipes remain native. [Chemistry scope and dependency audit](diagnostics/RawMaterialChemistryAudit.md)

The Magnum's raw vat biomass order consumes one Piece of Flesh (`human_meat`) and one Water Bottle (`water_bottle_1`) to produce eight biomass in 12 base hours. Sealed food cargo remains a finished trade good; it is not the feedstock for this order.

Player fabrication uses loose materials and prepared components. Packaged food, Biowaste Tanks, Research Stations, and Quasi-samples retain their role as valuable trade cargo. Station industries can still buy and process bulk cargo through their existing freight chains.

| Magnum recipe | Loose ingredients replacing trade cargo |
| --- | --- |
| Raw vat biomass | One Piece of Flesh and one Water Bottle yield eight biomass. |
| Harvested tobacco | One raw vat biomass and one Water Bottle yield eight harvested tobacco. |
| Sterile implant scaffold | One raw vat biomass replaces the Biowaste Tank; dressing and polymer stock remain required. |
| Tissue Graft | One raw vat biomass replaces the Biowaste Tank; clinical refill and medical glue remain required. |
| Aegis Ward | One microelectronics part and one lens replace the Research Station; fittings and textile remain required. |
| Golden Aegis Ward | Two microelectronics parts and one lens replace the two Research Stations; fittings, textile, and the Purity Seal remain required. |
| Six custom quasi implants using research samples | One raw vat biomass per former Quasi-samples item is required in the player recipe. Their other quasi materials remain required; faction laboratory orders still accept sample cargo. |

Explosives are a native crafting component and remain valid grenade ingredients. The same quantity reduction and doubled batches apply to custom rations, weapons, armor, ammunition, implants, and their component recipes.

| Source | Tech | Supplies, components and acquisition |
| --- | --- | --- |
| **Imperial and shared workshops** | 1 | The Magnum produces ore and bottled water using time alone. Ore feeds metal, conductors, silicate, reactive minerals, and carbon feedstock. Biomass separates into pulp, oil, and chemical extract. These foundations supply ordinary loose chemicals, materials, and mechanical/electrical parts. Scrap provides optional recovery routes. Shared recipes are available aboard the Magnum. Dedicated stations stock or produce established materials from the start. |
| **Luna and Neith** | 1 | Luna stocks biomass, vat ration base and wrappers. Havoc sends bulk water to Neith, which grows biomass for Luna's ration line and also stocks bottled water. Dirty Corpse Starch can appear in Luna raid salvage. The Magnum ration line makes clean Corpse Starch. |
| **Phobos** | 1 | Stocks silicate, cell stock, sealed cell assemblies and empty Las Clip housings. The schematic comes from standard Imperial chip rolls. |
| **AnCom · Akatsuki** | 1 | The cartridge schematic covers medium cartridges, sorted casings and payload modules. Akatsuki stocks the components. |
| **AnCom · Akatsuki** | 2 | The specialist shell schematic covers flechette shells, empty shell casings and flechette modules. Akatsuki adds the components to stock and salvage. |
| **DaydreamChems · Paragon** | 1 | The cigarette line uses harvested and prepared tobacco, filter stock, filters, boxes and loose cigarettes. The field medical line uses sterile dressing rolls, sealed treatment packs and prepacked dressing trays. The facility stocks cigarettes, field medkits and selected components. |
| **DaydreamChems · Avicenna's Clinic** | 2 | The clinical schematic teaches sealed clinical refills, sterile instrument trays, advanced medkits and surgical kits. The clinic stocks the finished kits and their specialty parts. |
| **RealWare · Clotho** | 1 | The protective armor pattern covers the four-piece RealWare set. Clotho also supplies metal stock, rolled plate, pressed inserts and liners. |
| **CoreWard · Rachmaninoff and PlanetBridge · Mammoth** | 1 | Rachmaninoff supplies ore and silicate. Mammoth supplies sorted metal, fiber, polymer, glass and conductive scrap. |
| **Terra and Mars freight network** | Campaign start | The six contestable facilities produce bulk water, ore cargo, biomass tanks, ration cargo, tools, and engineering and electrical parts. These are station inputs and freight goods. |

### Magnum material foundations

Ore and bottled water are independent starting materials. Their orders consume production time without requiring any item, including a bottle, scrap, Cable, or Glass. Flesh is the gathered organic starting resource. All shared recipes below are granted when a new Magnum is created and when an existing save loads. Biomass and conductive scrap are now shared foundations.

| Output per order | Ingredients consumed | Base hours | Availability |
| --- | --- | --- | --- |
| 4 metal smelting feedstock (ore) | None | 3 | Shared starter recipe |
| 4 Water Bottles | None | 3 | Shared starter recipe |
| 4 refined metal stock | 2 ore | 6 | Shared starter recipe |
| 4 conductive scrap | 2 ore | 2 | Shared starter recipe |
| 2 Cable | 2 conductive scrap | 3 | Shared starter recipe |
| 4 silicate feedstock | 2 ore | 2 | Shared starter recipe |
| 4 basic Glass | 1 silicate feedstock | 2 | Shared starter recipe |
| 4 Industrial Glass | 2 silicate feedstock | 4 | Shared starter recipe |
| 8 raw vat biomass | 1 Piece of Flesh + 1 Water Bottle | 12 | Shared starter recipe |
| 4 raw cellulose pulp | 1 biomass + 1 Water Bottle | 2 | Shared starter recipe |
| 4 organic oil | 1 biomass | 3 | Shared starter recipe |
| 4 chemical extract | 1 biomass + 1 Water Bottle | 3 | Shared starter recipe |
| 4 reactive mineral fraction | 2 ore | 3 | Shared starter recipe |
| 4 carbon-rich mineral feedstock | 2 ore | 3 | Shared starter recipe |
| 4 Acid | 1 chemical extract + 1 reactive mineral + 1 Water Bottle | 3 | Shared starter recipe |
| 12 Gasoline / Gas fuel | 1 organic oil + 1 carbon feedstock + 1 chemical extract | 4 | Shared starter recipe |
| 4 reclaimed fiber | 2 raw pulp | 2 | Shared starter recipe |
| 4 moldable polymer stock | 1 organic oil + 1 chemical extract | 4 | Shared starter recipe |
| 4 Paper Towels | 1 raw pulp + 1 Water Bottle | 2 | Shared starter recipe |
| 4 Plastic | 1 organic oil + 1 chemical extract | 3 | Shared starter recipe |
| 4 Rubber | 1 polymer stock + 1 carbon feedstock | 3 | Shared starter recipe |

Glass and Industrial Glass are alternative products of silicate. The basic route produces four Glass from one silicate; industrial refining produces four Industrial Glass from two silicate and takes twice as long. Neither route needs pre-existing glass or cullet. Water feeds the existing biomass, tobacco, medical, and other water-consuming recipes; those recipes still require their other listed ingredients.

Recycling one sorted metal scrap recovers one ore, recycling one cullet recovers one silicate, and recycling one polymer scrap now recovers one moldable polymer stock. The optional Glass-to-cullet sorting order consumes two Glass for four cullet. With the larger fabrication batches, recycling and remaking goods can increase material quantities; the [balance diagnostic](diagnostics/RecipeCostBatchBalance.md#recycling-diagnostic) records this consequence. The new chemistry audit records the additional polymer recovery route; it does not claim a lossless or exploit-free economy.

The five new chemical/mineral fractions are abstract game materials. Their native outputs use the existing item IDs so other recipes can consume them directly. Ordinary mechanical and electrical recipes make four parts per order; explicit new ammunition recipes make twelve units, and finished medicine, repair, grenade, and cyborg orders make two. Sorbent preserves an existing native recipe exactly and gains a raw-material fallback only when that recipe is absent.

Finished support ammunition and cyborg components retain their established support-tier patterns. Three additional DaydreamChems catalogs offer medicine/splint recipes at tiers 2, 3, and 4 while preserving native starter, workbench, and other schematic access. Each chip still selects one recipe. [Complete source recipe map](docs/crafting/recipe-map.html)

Ore Cargo (`ore_cargo`) is a bulk source of the same raw materials. Its native container opening/disassembly action consumes each selected crate and yields twelve ore, six silicate feedstock, and six conductive scrap. All three outputs are guaranteed; the game splits stacks and handles inventory overflow through its usual item-placement path. The crate's native trade category, weight, stack capacity, and station-industry uses are retained. Existing unopened crates use the updated contents without adding items during save loading. No raw-material-to-Ore-Cargo repacking recipe is added.

### Specialist supply and fabrication

All eight Bolt Clips use the Heavy ammo family. Ordinary Heavy rounds still fit the bolt weapons. The suppliers below restock small lots at their own tech levels. The Golden Throne stocks the armory patterns as the Imperium advances, and each requisition draws one recipe from its pool, including the possibilities listed in its row.

| Tech | Items | Supplier and unlock |
| --- | --- | --- |
| **3** | Imperial Requisition | Earned from Imperial procedural contracts, including Arbites Security. Sell it at the Golden Throne for Imperial trade credit to spend on patterns. |
| **4** | Bolt Clip and Bolt Pistol | Phobos stocks the standard clip and compact sidearm. Both recipes are possible results of the Elysian drop requisition. |
| **5** | Kraken Bolt Clip, Krak Grenade and Bolt Rifle | Havoc stocks the armor-piercing round, narrow-blast grenade and long-range rifle. All three recipes are possible results of the Krieg siege requisition. |
| **6** | Metal Storm Bolt Clip, Helfrost Bolt Clip and Tissue Graft | The Golden Throne's Tempestus armory stocks Metal Storm, which scatters two lacerating fragments. Luna stocks Helfrost, which deals Cold damage. Both clip recipes are in the Tempestus requisition pool. Avicenna's Clinic stocks the chest graft at **DaydreamChems Tech 6**. |
| **7** | Antiphasic Bolt Clip and Subdermal Underplate | Foundry Primaris stocks the Shock round and protective body implant. Both recipes are in the Skitarii forge requisition pool. |
| **8** | Inferno Bolt Clip, Melta Grenade and Purity Seal | The Golden Throne stocks the incendiary round and short-range Plasma grenade. Both recipes are in the Sororitas armory requisition pool. Reliquary recovery pays two seals and can return after 28 days. Melta Grenade manufacture consumes one seal. |
| **9** | Hellfire Bolt Clip and Detox Gland | Phobos stocks the Poison round and protective stomach implant. Both recipes are in the Ordo Xenos requisition pool. |
| **10** | Tempest Bolt Clip and Power Fist | Titan's vault armory stocks the Beam round and charged gauntlet. Both recipes are in the Grey Knights Paladin requisition pool. |
| **11** | Golden Paladin equipment and Golden Aegis Ward | The Golden Throne stocks Golden Paladin requisitions, each carrying one of seven possible recipes. The ward consumes one Purity Seal. The Golden Psycannon accepts all eight Bolt Clips. |

### Support munitions and allied machines

Imperial support patterns each contain one randomly selected Magnum recipe from a pool of conventional munitions, mines, battlefield devices, repair and recovery equipment, and native deployable allies. The table lists the possible recipes at each tier. The items retain their native models, ammunition families, and deployment behavior. Patterns enter Imperial chip stock at the listed tier and are stocked at the supplier. Existing workbench recipes and other factions' unlocks still apply. AnCom supplies the manufacturing patterns for medium cartridges and flechette shells, and the Imperium also trades finished rounds.

| Imperial tech | Supplier | New support stock and fabrication |
| --- | --- | --- |
| **1** | Phobos | Small rounds, nail bolts, makeshift cells, stun grenades, improvised fragmentation grenades, fragmentation mines, throwing knives, Automap, Quasi Sensor, human defibrillator and robot revival device. |
| **2** | Havoc | Ballistic turret, industrial robot, body parts and drill assembly, melee repair kit, standard BatteryCells, sawblades, police cartridges, shotgun slugs, improvised smoke and incendiaries. Havoc also stocks robot parts crates. |
| **3** | Luna | Fire turret, recreational robot, body parts and blade assembly, engineering repair kit, burst ammunition, Dilthey cells, Heavy rifle loads, standard fragmentation, high-explosive and toxic grenades. |
| **4** | Foundry Primaris | Energy turret, security robot, body parts and machine gun assembly, deployable cover, firearm and technical repair kits, standard rifle ammunition, expanding small rounds, smoke and thermal grenades. |
| **5** | Broadsword Station | Security drone, standard rockets, gas trade stock, Cold cartridges and explosive shotgun shells. Gas production is a shared material foundation and is omitted from new chip rolls. |
| **6** | Golden Throne | Armor repair, nanoinjector, Plasma rifle rounds, Heavy flak, poisoned sawblades, frost and toxic gas grenades, and nanorobot grenades. |
| **7** | Foundry Primaris | Incendiary rockets and Heavy rounds, Dragon shotgun shells, and Hellfire Grenades. |
| **8** | Golden Throne | Deployable energy barrier and armor-piercing rifle rounds. |
| **9** | Phobos | Frost Grenades, Toxic ammunition and poison rockets. |
| **10** | Titan Fortress-Monastery | Nuclear rocket ammunition and its manufacture pattern. |
| **11** | Golden Throne | Forward stock of energy turrets, energy barriers, security robots, drones and technical repair kits alongside Golden Paladin requisitions. Earlier support patterns and manufacturing remain available. |

Havoc builds ballistic turrets and industrial robots at Tech 2. Luna builds fire turrets and recreational robots at Tech 3. Foundry Primaris builds energy turrets and security robots at Tech 4. Broadsword produces drones at Tech 5, and the Golden Throne produces energy barriers at Tech 8. The factories operate under Imperial ownership at the required tech and draw from the component network below.

### Automata components and freight

| Site | Imperial tech | Inputs per order | Outputs | Base hours |
| --- | --- | --- | --- | --- |
| Phobos glassworks | 1 | 3 silicate feedstock | 2 industrial glass | 48 |
| Phobos cell stock bench | 1 | 2 acid, 3 Cable | 3 cell stock | 48 |
| Phobos sealed cell bench | 1 | 2 industrial glass, 2 cell stock | 6 sealed cell assemblies | 54 |
| Phobos electrical works | 2 | 2 ore cargo, 1 tool container | 3 electrical parts containers | 144 |
| Havoc chassis works | 2 | 6 rolled plates, 3 armor fittings, 2 moldable polymer stock | 4 Automata Chassis | 64 |
| Foundry Primaris logic works | 2 | 1 electrical parts container, 2 sealed cell assemblies | 3 Automata Logic Cores | 72 |
| Broadsword avionics | 5 | 3 Automata Logic Cores, 1 electrical parts container | 2 AI modules | 96 |

Phobos supplies cells and electrical parts to Foundry Primaris. Havoc supplies chassis to Luna, Foundry Primaris, and Broadsword. Foundry Primaris supplies logic cores to the automata factories and the Golden Throne. Broadsword turns surplus cores and electrical parts into AI modules for drones and the palace energy-barrier line. Ore cargo, tools, and electrical crates can also come from the contestable inner-system factories through player trade.

The recipes expose their inputs as station demand. Imperial freight uses the game's reserve and cargo limits, and the player can deliver shortages through station barter. Each automata or component order waits for a full batch of materials and consumes it when the order starts. Stock produced by one line can feed the next. The Magnum starts with chassis and logic core fabrication. AI module fabrication is a separate possible result of the Tech 5 drone pattern chip; it is not automatically taught when a drone recipe is learned or a save is loaded. Havoc and Foundry raid salvage can yield their respective components, and the Magnum can shred surplus chassis and cores back into scrap.

### Rations, medical supplies, and munitions freight

| Chain | Imperial tech | Station production and freight |
|---|---:|---|
| Rations | 1 | Havoc reclaims bulk water and ships it to Neith's biomass vats. Neith sends biomass to Luna for ration base. Havoc also recovers fiber from rags and mills it into textile. Foundry Primaris recovers moldable polymer from plastic. Luna presses wrappers from those two materials, packs Corpse Starch, and sends rations to the Phobos garrison. |
| Field supplies | 1–3 | Havoc sends textile and Foundry Primaris sends polymer to the Golden Throne, which makes sterile dressings and prepacked trays. Neith makes treatment packs from biomass and game medical stock. At Tech 3 the Golden Throne packs field medkits and sends them to Broadsword. |
| Advanced medicine | 5–7 | At Tech 5 Neith turns treatment packs and medical stock into clinical refills for Broadsword, which upgrades field medkits and ships them to Titan. At Tech 7 the Golden Throne sends field medkits and sterile instrument trays to Neith for surgical kits, which also ship to Titan. |
| Las Clips and Guard Frags | 1–2 | Phobos sorts silicate from ore cargo and uses its glassworks and cell benches to make sealed cells. Havoc molds housings from Foundry polymer and ships them to Phobos. Phobos packs Las Clips for the Golden Throne and assembles Guard Frags at Tech 2 for Broadsword. |
| Bolt Clips and specialist grenades | 4–11 | Havoc turns refined metal and game munitions stock into rifle rounds and sends them to Phobos for standard Bolt Clips. Phobos supplies the Broadsword garrison and the specialist lines. Those lines ship finished rounds and grenades along the routes below. |

| Imperial tech | Output | Factory → freight destination |
|---|---|---|
| 5 | Kraken Bolt Clip | Havoc → Broadsword |
| 5 | Krak Grenade | Havoc → Foundry Primaris |
| 6 | Metal Storm Bolt Clip | Golden Throne → Broadsword |
| 6 | Helfrost Bolt Clip | Luna makes its own glass from Phobos silicate and ships the finished clips back to Phobos |
| 7 | Antiphasic Bolt Clip | Foundry Primaris → Broadsword |
| 8 | Inferno Bolt Clip | Golden Throne → Foundry Primaris |
| 8 | Melta Grenade | Golden Throne → Titan |
| 9 | Hellfire Bolt Clip | Phobos → Titan |
| 10–11 | Beam Tempest Bolt Clip | Titan builds at Tech 10 → Golden Throne issues at Tech 11 |

Factories wait for complete batches and post demand for missing ingredients through normal station barter. Some specialist inputs still come from outside suppliers and mission rewards, including AnCom flechette shells, DaydreamChems medical stock, and Purity Seals for the Melta line. Finished stock can be traded and shipped to the listed stations under native freight and reserve rules. Garrison orders use part of that stock. The older Luna, Neith, and Phobos orders can finish if they were already queued in a save.

### Base-game manufacturers and freight

Existing stations now run these additional workshops alongside their original industries. Each workshop uses its manufacturer's tech tier and requires a full batch of inputs. The original manufacturer must hold the site to schedule new workshop orders. Domestic freight serves the participating suppliers, including RealWare's existing Lumina ore mine. Trade between factions follows the campaign's alliances. The player can also move goods through station barter.

| Manufacturer | Tech | Raw supply, production, and freight |
|---|---:|---|
| Church of Revelation | 1 | Caloris Planitia supplies bulk water. Pine Gap grows biomass and ships it to Sterling's Farm and DaydreamChems' E station. Sterling's Farm packs standard rations for Plato Academy. |
| PlanetBridge | 1 | Mammoth sorts rags into fiber and mills it into textile. It recovers moldable polymer from plastic. Textile supplies Paragon and Arcadis Terminal. Polymer also supplies Akatsuki. |
| DaydreamChems | 1–2 | E station turns biomass and medical cargo into treatment packs for Paragon and Avicenna's Clinic. Paragon packs field medkits from sterile dressings, polymer trays, and treatment packs. At Tech 2, the clinic receives field kits, dressings, and clinical stock. It assembles advanced and surgical kits and ships them to Moon Trade Center. |
| RealWare | 1–3 | Lumina ships ore to Clotho. Clotho refines metal for Akatsuki and presses inserts for Arcadis Terminal. Arcadis makes textile liners and combines the parts into complete protective armor sets at Tech 3. HIS Ares draws the finished equipment. |
| AnCom | 1–2 | Akatsuki makes medium cartridge cases and payload modules, then packs medium ammunition for HIS Unveiled. At Tech 2 it adds shell cases, flechette modules, and finished flechette shells for the same depot. |
| SBN | 2–3 | Boethius sorts electrical freight into capacitors, microelectronics, and boards. At Tech 3 it assembles AI modules and ships them to Skylab, which uses them to produce research disks. |

Imperial factories also have new upstream suppliers for three established support lines. At Tech 5, Broadsword makes gas ammunition for Luna's fire turret assembly and the Golden Throne's later Inferno Bolt Clips. At Tech 6, the Golden Throne uses Neith's clinical refills and microelectronics to make nanoinjections for Broadsword. At Tech 9, Phobos makes Toxic ammunition from industrial stock for its Hellfire Bolt Clip line. The base-game products and their existing Magnum recipes remain available.

### Base-game turrets, drones, and droids

CoreWard and SBN share two component lines with the deployable factories. RealWare's Clotho refines Lumina ore and presses inserts. Arcadis Terminal stamps fittings from the refined metal, and Mammoth reclaims polymer from scrap plastic. Sampo Quern turns the inserts, fittings, and polymer into Automata Chassis. On the electronics side, precious metals from Origin can feed Polaris's electrical freight. Cathode Rampart assembles twelve Automata Logic Cores from two circuit boards, two microelectronics parts, one capacitor, and one sealed cell in 84 base hours. AI modules are downstream products of controllers, so manufacturing a controller does not consume an AI module. The finished chassis and controllers ship to the assembly stations below through normal trade.

| Manufacturer and factory | Tech | Deployable | Receiving station |
|---|---:|---|---|
| RealWare · Arcadis Terminal | 2 | Ballistic turret | HIS Ares |
| Grasshopper · Canaan | 3 | Fire turret, with gas ammunition from Jinxing | Escher |
| SBN · Skylab | 4 | Energy turret | Humankind's Hope |
| SBN · Skylab | 5 | **Arc Sentry**, short-range shock turret | Humankind's Hope |
| RealWare · Arcadis Terminal | 6 | **Lance Sentry**, long-range beam turret with a recharge between shots | HIS Ares |
| CoreWard · Rachmaninoff | 2 | Industrial droid | HIS Woodpecker |
| CoreWard · Rachmaninoff | 4 | **Borehand Industrial Droid**, reinforced melee follower with a built-in drill | HIS Woodpecker |
| Grasshopper · Kyoko | 3 | Recreational droid | Shepherd's Arc |
| Grasshopper · Kyoko | 4 | **Concierge Pleasure Droid**, quick escort with a shock wand | Shepherd's Arc |
| RealWare · Arcadis Terminal | 4 | Security droid, with reinforced plates from HIS Ares | HIS Ares |
| PlanetBridge · Alliance Arcology | 3 | **Mule Cargo Drone**, ground follower with a loading fork for melee | Winter Bay |
| PlanetBridge · Alliance Arcology | 5 | Security drone | Cryptoria |
| PlanetBridge · Alliance Arcology | 6 | **Survey Drone**, flying scout with extended sight and a light pulse carbine | Cryptoria |

Each factory waits for a complete batch and assembles four packages. Native freight retains three units and supplies the listed depots from available stock. Depots issue the units gradually. Each new variant also has a manufacturer-specific Magnum schematic at the listed tech. Sampo Quern and Alliance Arcology make separate robot parts crates and schedule individual deployable orders under their original owners. Old saves can still finish any automatic multi-product robot batch already underway.

#### Variant sprite prompts

The built-in image generator used a shared brief for a single transparent, centered, three-quarter inventory object in chunky 16-bit sci-fi pixel art, readable at 48 pixels, with one strong accent and no text, border, cast shadow, or environment. Each object has separate inventory, floor, and shadow sprites. Deployed units use the existing animated chassis.

| Variant | Subject prompt |
|---|---|
| Arc Sentry | Low square foldout sentry with copper half-ring electrodes around a short cyan emitter, navy ceramic housing, and three squat stabilizer feet |
| Lance Sentry | Tall folding tripod sentry with one long amber laser lance, a cream ceramic heat sink, graphite mount, and asymmetric scope |
| Mule Cargo Drone | Amber ground-skimming cargo drone with a pale rectangular casing and two prominent forward fork tines |
| Survey Drone | Compact flying drone with four folding winglets, a broad blue optical sensor, off-white fuselage, and dark blue joints |
| Borehand Industrial Droid | Folded industrial droid with thick orange shoulders, telescoping legs, and a large silver drill cone |
| Concierge Pleasure Droid | Folded pleasure droid with a rounded burgundy and ivory shell, dark oval face plate, and short docked shock wand |

### Repair kits, tools, and spare parts

| Manufacturer and site | Tech | Production and freight |
|---|---:|---|
| PlanetBridge · Epitome Colony | 1 | Refined metal from Clotho and recovered polymer from Mammoth become coarse parts, rods, springs, and metal treads. Factories and repair shops buy them through native freight. |
| PlanetBridge · Winter Bay and Mammoth Terminal | 1 | Winter Bay packs mechanical spares into engineering crates and builds welding machines from rods, treads, and wire. Epitome can use the crates and welders in its construction industry. Mammoth uses engineering crates, treads, and wire to pack tool crates for Alliance Arcology and other workshops. |
| RealWare · Zephyr and Arcadis Terminal | 2–3 | Zephyr makes weapon parts from refined metal and rods. Arcadis uses those parts and springs for firearm repair kits. It also makes armor repair kits from pressed inserts, textiles, and fittings. HIS Ares receives both kits. |
| CoreWard · Rachmaninoff | 2–3 | Rachmaninoff makes melee repair kits from rods, coarse parts, and springs. Its engineering kits use Winter Bay's crates and metal treads. HIS Woodpecker receives both. Engineering kits retain their native use for cyborg wounds. |
| SBN · Van de Graaff and High Ares Post | 2–3 | Ares Post recovers wire from electrical freight. Van de Graaff makes lenses from glass and polymer, and transformers from wire and capacitors. Ares Post uses optics and Boethius electronics to make Automaps, Quasi Sensors, and technical repair kits for Humankind's Hope. |
| PlanetBridge · Cryptoria | 3 | Cryptoria recovers rods and microelectronics from Alliance Arcology's robot-parts crates and ships them back for drone assembly. |

The same supplies feed deployable factories. Arcadis uses Zephyr's weapon parts and Epitome's metal treads in ballistic turrets. SBN's lenses and transformers feed energy turrets and Arc Sentries. CoreWard uses crates, coarse parts, and drill refits for its industrial droids, Grasshopper uses springs and textile for recreational droids, and Alliance Arcology uses recovered rods for Mule drones and microelectronics for Survey drones.

These workshops require full batches. Depot demand uses native freight and reserves. Winter Bay's new welding line keeps the original production bonuses. Its original order stays available to finish jobs already underway and belongs to the normal profile under a different owner. The repair kits and tools keep their native behavior and Magnum recipes.

### Base-game weapon and armor manufacturers

The armories make a selection of native weapons and armor for every starting faction with branded equipment. Each line uses the original faction tags and item records. A weapon unlocks at its native tech, and an armor set unlocks when the manufacturer reaches the highest native tech among its pieces. Factories consume full batches, and receiving depots issue finished equipment gradually. Gear shares cargo space with other trade goods, so an armor set can arrive in several shipments.

Clotho refines Lumina's ore into metal, rolled plate, and inserts. Zephyr fires ceramite from inserts and glass. Tianming presses alloy plates at Red Tiangong. Mammoth recovers textile and polymer, Budapest turns them into flak and liners, and Paragon makes rubber stock. Epitome, Zephyr, Boethius, Van de Graaff, and Cathode Rampart supply mechanical spares, weapon parts, electronics, optics, and controllers. Power armor also draws on existing hydraulic pump production. Civil Resistance has a domestic route from Lightbay's ore and Rogue City's textile salvage through its part works and armory. Tezctlan grows carapace and other native stock from the Fresco harvest for its own workshops.

| Manufacturer | Weapon lines and tech | Armor lines and tech | Factories → receiving depot |
|---|---|---|---|
| AnCom | Service pistol 3, marksman rifle 5 | Agent 3, medium 7 | Akatsuki Kojou and Kraken's Hub → HIS Unveiled |
| RealWare | Military shotgun 3, heavy machine gun 10 | Existing protective set 3, heavy 7, power 10 | Arcadis Terminal and Malmö → HIS Ares |
| SBN | Rail sidearm 3, rail assault rifle 10 | Medium 3, heavy 8 | Skylab and Pertwee → Humankind's Hope Citadel |
| Tianming | Disc sidearm 2, shock assault rifle 10 | Heavy 4, light 7 | Red Tiangong → Oracle Bastion |
| Dilthey | Sound sidearm 1, ion SMG 8 | Lab protection 5, light 8 | Nox and AnCap Dogma → 0G TC |
| DaydreamChems | Toxic pistol 1, disruptor marksman rifle 9 | Chemical protection 5, power 9 | Sundog Zone, Paragon FC, and Bill Gregor → Mechta Shooting Range |
| PlanetBridge | Rail sidearm 1, heavy machine gun 9 | Laborer medium 3, power 9 | Budapest and Epitome Colony → Charon DocBay |
| Grasshopper | Compact SMG 2, microwave marksman rifle 10 | Winter clothing 4, light 6 | Canaan and Hart's Warehouse → Escher |
| Sunlight | Laser sidearm 3, cryolaser heavy machine gun 10 | Medium 5, power 10 | Polaris and Zeus Mk. III → Ji-Stratagem |
| FrancheComté | Patrol sidearm 2, shock SMG 7 | Patrol 4, riot 5 | Qi Helium-3 and Chick-Chick/Moo-Moo → Shu Center |
| Church of Revelation | Fire club 4, plasma sniper rifle 8 | Manager clothing 2, power 9 | Tycho Terminal and Gloria Terminal → Deepsky |
| CoreWard | Worker pistol 2, heavy machine gun 9 | Mining armor 5, mining power armor 7 | Rachmaninov-Energia and Sampo Quern → HIS Woodpecker |
| Civil Resistance | Militia pistol 1, plasma assault rifle 7 | Militia 2, heavy 8 | Lightbay, Purgatory, Rogue City, and Tesla Ring supply Warden-D → Flat Obsidian |
| Unchained Belt | Boarder SMG 4, autogun 10 | Boarder medium 3, light 8 | Schwerin supplies Bethlehem → Hargeisa |
| Tezctlan | Ritual club and shotgun 4, launcher 8 | Carapace 3 | MZ Fresco Platforms supplies Oddibjord-3 and L-BSA → Feathered Temple |

Four other brands have ownership-gated lines at native sites. Their workshops open when the named faction controls the site and reaches the listed tech. The campaign keeps its original starting owners.

| Manufacturer | Equipment | Factories → receiving depot |
|---|---|---|
| Xiomara Masks | Blowpipe and mask armor at Tech 3 | MZ Fresco Platforms supplies Oddibjord-3 → Feathered Temple |
| Emeraldlight | Cryolaser and medium armor at Tech 5 | Zeus Mk. III supplies Polaris → Ji-Stratagem |
| Church Gannix | Ritual sniper and spider armor at Tech 4 | Gloria Terminal → Deepsky |
| Shedu Thousand | Gas shotgun at Tech 3, possessed armor at Tech 5 | Gloria Terminal supplies Red Tiangong → Gloria Terminal |

### Cybernetics and implant manufacturers

The cybernetics network adds 38 implants and ten replacement parts, plus station production for native implants, limbs, and mounted cyborg weapons. Manufacturers assemble complete batches and ship finished stock to their own depots. New items have manufacturer schematics for the Magnum. The four conditional manufacturers use the same ownership rules as their armories.

Three shared components tie the labs to the earlier industries. Winter Bay turns rods, treads, and recovered polymer into **servo cartridges**. Cathode Rampart assembles **implant controllers** from Boethius electronics and High Ares Post wiring. Sundog makes **sterile scaffolds** from farm biomass, Paragon dressings, and recovered polymer. Civil Resistance has its own salvage routes through Rogue City and Tesla Ring. Tezctlan and the conditional quasi manufacturers draw on Venus carapace stock and Mars salvage as appropriate.

| Manufacturer | New implants and tech | Workshop → receiving depot |
|---|---|---|
| AnCom | Recon Optic 3, Quickfeed Sleeve 5 | Discovery → HIS Unveiled |
| SBN | Survey Pulse 4, Signal Isolator 6 | Skylab → Humankind's Hope Citadel |
| RealWare | Braced Shoulder 3, Siege Lattice 7 | Cronus → HIS Ares |
| Tianming | Disc Counterweight 3, Pulse Governor 6 | LaCE → Oracle Bastion |
| Dilthey | Acoustic Dampener 2, Static Coil 6 | Fortuna → 0G TC |
| DaydreamChems | Detox Membrane 3, Trauma Injector 6 | Avicenna's Clinic → Moon Trade Center |
| PlanetBridge | Haul Assist 2, Route Ram 5 | Cryptoria → Alliance Arcology |
| Grasshopper | Soft Step 2, Courier Reflex 5 | Kyoko → Shepherd's Arc |
| Sunlight | Heat Sink 3, Cryo Stride 7 | DarkFlora → Ji-Stratagem |
| Franche-Comté | Patrol Ear 2, Riot Anchor 6 | Qi Helium-3 → Shu Center |
| Church of Revelation | Pilgrim Respirator 2, Signal Cantor 5 | Pine Gap → Deepsky |
| CoreWard | Worksite Brace 2, Worksite Radiator 5 | Rachmaninov-Energia → HIS Woodpecker |
| Civil Resistance | Scrap Guard 2, Defiance Pump 6 | Tesla Ring → Flat Obsidian |
| Unchained Belt | Boarder Joint 2, Holdfast Knee 5 | Schwerin → Hargeisa |
| Tezctlan | Carapace Seed 3, Spine Knot 6 | Oddibjord-3 → Feathered Temple |
| Xiomara Masks | Veil Tendon 3, Masked Optic 6 | Oddibjord-3 → Feathered Temple |
| Emeraldlight | Cold Vein 4, Prism Membrane 7 | Zeus Mk. III → Ji-Stratagem |
| Church Gannix | Shrine Lattice 3, Echo Pilgrim 6 | Gloria Terminal → Deepsky |
| Shedu Thousand | Forge Rib 4, Mars Tendon 7 | Red Tiangong → Gloria Terminal |

The new active implants have three charges each. Survey Pulse reveals nearby space, Static Coil discharges shock, Trauma Injector aids recovery and wound stabilization, and Signal Cantor emits a sonic pulse. Their passive counterparts trade combat, mobility, recovery, or carry capacity against effects such as weaker resistances, lower health, or higher Quasimorphosis. All use native anatomy and surgical eligibility.

Replacement parts include CoreWard Loadframe Feet, RealWare Bulwark Chest, Grasshopper Courier Leg and Flex Wrist, SBN Survey Cranium, Tianming Servo Knee, PlanetBridge Gantry Arm, Daydream Sterile Organ Pack, Resistance Salvage Wrist, and Shedu Mars Recoil Hand. Courier Leg, Gantry Arm, and Mars Recoil Hand replace linked anatomical slots. Bulwark Chest, Survey Cranium, Gantry Arm, and Mars Recoil Hand provide two implant sockets in the primary replacement slot.

### Pharmaceuticals and emergency supplies

Pine Gap grows biomass from bulk water. Sundog turns that biomass and water into Medical Components and uses both to package Drug Cargo. Mammoth recovers polymer and sorts scrap glass into cullet for its glassworks. Paragon uses Medical Components and recycled polymer to make the medical glue used in its dressings and Avicenna's surgical instruments.

| Manufacturer | Station → depot | Native products and faction tech |
|---|---|---|
| DaydreamChems | EStation → Moon Trade Center | Sorbent at Tech 1, antibiotics, painkillers, Osteodron, and Tranexam at Tech 2, and Antiradin at Tech 3 |
| DaydreamChems | Avicenna's Clinic → Moon Trade Center | Blood bags at Tech 2 and antidote at Tech 3 |
| Franche-Comté | Qi Helium-3 → Shu Center | Naloxone at Tech 2 and morphine at Tech 3 |
| Grasshopper | Kyoko → Shepherd's Arc | Steroids at Tech 2 and adrenaline at Tech 6 |
| Sunlight | DarkFlora → Ji-Stratagem | Cryopentol at Tech 3 and Fibromine at Tech 4 |
| SBN | Cathode Rampart → Humankind's Hope Citadel | Nanoinjectors at Tech 6, assembled from implant controllers, clinical refills, and polymer stock |

The station batches require their full inputs and supply both commerce and downstream industry. Naloxone goes into Avicenna's surgical kits, Fibromine feeds cryolaser and Helfrost production, and Drug Cargo supplies station gas and toxic munitions. Sorbent, Osteodron, and Tranexam ship from their established EStation lines. Finished goods retain native effects and artwork. The 1.1.4 Magnum foundations explicitly replace selected medicine ingredient lists with loose chemical routes; Sorbent retains its existing native recipe where present.

### Planned live crafting map and occult production

The 1.1.5 source adds an in-game **Crafting map** tab on the left edge of Production and its recipe picker. It reads loaded items and recipes, opens a muted sage graph panel, and supports zoom, pan, node highlighting, and lower-panel recipe details. [Implementation and validation](diagnostics/CraftingMapAudit.md). The [crafting-tree expansion design](CRAFTING_TREE_EXPANSION_DESIGN.md) also specifies later quantity planning, exports, and separate occult resource/equipment families. The [offline visual map](docs/crafting/recipe-map.html) remains the historical source snapshot. Existing reward-only and exotic acquisition rules are unchanged by the browser.

## Cigarettes and manufacturer recipes

The player buys the cigarette recipe from DaydreamChems, whose facilities cover growing and chemical processing. The faction's standard chip pool supplies the schematic. Its production facilities trade finished stock and have room loot that reflects their work. Each schematic contains one randomly selected recipe from the tobacco, filter, box, rolling and packing supply pool. Generic board stock and other shared materials can come from unrelated suppliers.

| Operation | Inputs and output | Where the crew can recover the output |
| --- | --- | --- |
| Grow and prepare tobacco | Grow tobacco in a dedicated room. Harvest it and prepare it into tobacco stock for the rolling bench. | Grow-room harvest trays and leaf storage. |
| Make filters | Turn chemical feedstock and filter media into unfinished filters, then cut them to size in batches at the component bench. | Filter racks and production crates. |
| Make boxes | Fold and assemble box blanks from paperboard stock. | Carton line and packaging stores. |
| Roll loose cigarettes | Combine prepared tobacco, rolling paper and filters at the rolling bench. | Rolling-line trays and work-in-progress crates. |
| Pack boxes | Combine loose cigarettes and assembled boxes at the packing bench to yield the existing base-game cigarette item, `cigarettes_1`. | Dispatch crates, vendors and ordinary finished-item loot. |

Boxes and filters can be made in parallel with growing. Salvaged loose cigarettes and spare boxes reduce the chain to a single packing order once the crew has learned the recipe. An intact pack is usable as soon as it is recovered. The installed crafting table already offers `cigarettes_1` directly from four sorbent pills at the medical printer. The overhaul would replace that direct crafting recipe so the faction purchase and packing step matter. Keep the existing item ID and its downstream uses, including the medical kit recipe that consumes it.

For faction recipes in general, record the manufacturer, the tech level that adds the schematic to its chip pool, its price, compatible benches, and the possible recipes in its pool. Keep the selected recipe fixed on the chip and retain learned recipes across save/reload. After capture, future restocks use the new owner's chip pool and the station's normal type weights.

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

### Imperial station interiors

The Imperial sites use Quasimorph's standard procedural generator and room pools. Peaceful visits use a comparable base-game interior while retaining the Imperial station's own owner, trade inventory and stash. The mod supplies Imperial units and station-specific salvage, which appears in compatible containers on the first raid floor.

| Imperial site | Native raid rooms | Peaceful interior reference | Main production inputs and outputs |
| --- | --- | --- | --- |
| Golden Throne | Orbital military | Pine Gap Training | Rolled plate and processed textile → pressed inserts and fitted liners |
| Luna | Orbital farm | Sterling's Farm | Vat biomass → ration base; ration base and wrappers → Corpse Starch |
| Phobos | Orbital spaceport | Canaan | Silicate and cell stock → sealed cells → Las Clips; also Guard Frags and Bolt Clips |
| Havoc | Orbital industrial | Sampo Quern | Water reclamation, textile milling, clip housings and rifle rounds. Refined metal and polymer also feed armor fabrication. |
| Foundry Primaris | Mars surface and spaceport | Gloria | Ore → refined metal and rolled plate |
| Neith Seed Vault | Orbital laboratory | Fresco Platforms | Bulk water → vat biomass; treatment packs, clinical refills and surgical kits |
| Titan Fortress-Monastery | Moon surface and military | Carcosa | Rolled plate and scientific equipment → reinforced inserts and technical repair kit |
| Broadsword Station | Orbital military | Cassini's Shadow | Refined metal and electrical parts → firearm and armor repair kits |

The production column lists each site's main industries. Native population, research and industry recipes contribute additional demand and income. Havoc's armor fabrication order uses two refined metal and one moldable polymer to produce three fittings and one rolled plate in 60 base hours. On load, existing stations pick up updated trade profiles under their current owners. [Quasimorph developer modding guide](https://steamcommunity.com/sharedfiles/filedetails/?id=3281671312)

### Contestable industry on Terra and Mars

Six capturable sites broaden the inner system's sources of basic goods. Both planetary markets have their own water, ore, biomass and ration producers. Terra's tool workshop and Mars's parts plant have complementary inputs so freight has a reason to move in both directions.

| World | Site | Starting faction | Principal goods and inputs |
| --- | --- | --- | --- |
| Terra | **Meridian Waterworks** | Franche-Comté | Bulk water. Extraction has no imported material input. |
| Terra | **Greenline Provisions** | DaydreamChems | Bulk water feeds biomass production. Water and biomass feed ration packing. |
| Terra | **Junction Reclamation** | PlanetBridge | Recovered ore cargo. Ore and engineering parts feed tool production. |
| Mars | **Cinder Basin Mine** | CoreWard | Ore cargo and bulk water from local deposits. |
| Mars | **Hesperia Fabrication** | Church of Revelation | Ore feeds engineering parts. Ore and tools feed electrical parts. |
| Mars | **Vallis Growworks** | Grasshopper | Bulk water feeds biomass production. Water and biomass feed ration packing. |

Both growworks convert two bulk water containers into four biomass tanks in 144 base hours. Junction uses one ore cargo and one engineering parts container to make two tool containers in 168 base hours. Hesperia uses two ore cargo and one tool container to make three electrical parts containers in 168 base hours. The other orders use native extraction and production recipes. Stations share the existing planetary destinations and use native mining, industrial, spaceport and orbital farm layouts.

Human captors retain the site's industrial recipes. Trade uses the current owner's native alliance rules, stock limits and freight schedules. Existing campaigns add missing sites once and choose an active human owner if the designated minor faction has fallen. Native capture replaces the operating faction without removing the station's role. These sites provide alternative suppliers when outer routes fail, with actual stock and prices still driven by the campaign.

### Future custom production rooms

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

The automata network requires full input batches and exposes shortages through native trade demand. Later, a mission result such as a rescued shipment or repaired feed line can affect the actual site's stock or queue if the same state remains valid through save and load.

## Build order and acceptance

1. **Baseline.** Inventory the current recipes, prices, acquisition routes and station output cadence. Choose a single Las Clip chain as the three-step benchmark. Keep the existing Guard supply available during the prototype.
2. **One complete ore loop.** Add one mining preset, one refining room and one cell line. Follow one raw batch through collection, processing, shipping or trade, final manufacture, and use on a raid.
3. **One grow-to-pack loop with a faction recipe.** Use cigarettes to test schematic purchase, independent filter and box production, rolling, and packaging into the existing item. Test a second run that starts with salvaged loose cigarettes and boxes. Reuse those grow-room and packing conventions for the ration chain.
4. **Expand suppliers.** Add RealWare plate stock, DaydreamChems biomass, SBN components and PlanetBridge dispatch one at a time. Verify each link before making other recipes depend on it.
5. **Add mid-tier recipes and variants.** Move Arbites armor onto the longer route once a player can obtain both plates and liners through normal supply. Add alternate layouts and then tie facility state to compatible mission objectives.

Acceptance includes one full source-to-equipment test, economy totals before and after production, a check for stuck dependencies and profitable conversion loops, lost and captured station scenarios, and save/reload during an active order. Compare the same item made from raw stock, assembled from battlefield salvage and acquired finished. Confirm the seller requires a purchase to teach the recipe and that the old one-step cigarette craft cannot bypass it. View the actual generated rooms from a player's approach and verify that input, machinery and output read clearly before opening a container or terminal.

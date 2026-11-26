# Imperium of Man

Imperium of Man is a Warhammer 40,000 fan mod for Quasimorph. It adds the Imperium as a campaign faction you can take contracts from, earn reputation and equipment with, and barter with at its stations.

**Current package — 1.1.9:** [ImperiumOfMan-1.1.9-CraftingMap.zip](package/ImperiumOfMan-1.1.9-CraftingMap.zip), source `347de7db77f9121ad26e1c12c534b97760ee92f2`. Native compilation and package verification passed. Session 005 verified the map refinements. Corrected-directory session 006 loaded the same binary, saved after entering Space and completed 46 hours 20 minutes of in-game time without the previous missing-preset or managed-exception diagnostics. The six-node research tree and both corrected tooltip hovers also passed current-build visual review. Research purchase, chip-learning and recycling transactions remain separate checks. The verification scope and remaining transaction cases are recorded with the package. [Installation and verification](package/README.md).

**Release comparison:** the cumulative notes compare the published [v1.1.0 release](https://github.com/mrcalzon02/ImperiumOfMan/releases/tag/v1.1.0), source `b08c27e38bbc73712e073ec9141a6e8769984d9a`, with this candidate. The established Imperial/Chaos campaigns, endgame interiors and faction equipment remain part of that baseline. [Cumulative 1.1.9 change notes](docs/crafting/CHANGE_NOTES_1.1.9.txt).

**Crafting foundations:** biomass separation, mineral chemistry and ordinary components retain the lower ingredient quantities and doubled custom batches. Existing native exceptions and normal acquisition rules remain distinct from the literal occult batches. [Chemistry audit](diagnostics/RawMaterialChemistryAudit.md).

**In-game Crafting map:** Production and its recipe picker expose the sage-green map tab. In exact-source session 005, both detail scrollbars reached their final rows, wheel scrolling stayed within the detail list, short lists hid the bars, selected-item/process quantity labels remained focused, and Fit contained the 19-node example at 48% zoom. Escape and owner cleanup behaved as expected. The live index reported 2,363 items and 3,017 routes. [Current observations](diagnostics/CraftingMapAcceptance-347de7d.json) · [Execution history](diagnostics/CraftingMapHeadedValidation.md).

**Occult Research:** Culture, Distillation, Manifestation, Binding, Infusion and Soulforge are six capability purchases. Physical pattern chips independently teach one recipe each. Research teaches no recipes, including the four foundational patterns. Registration confirmed 21 pools and 53 occult outputs; the clone-to-Gavvakh chain preserves its exact batches and times. [Capabilities](diagnostics/OccultResearchExpansion.md) · [Production](diagnostics/OccultOrganicProduction.md).

**Organic recycling:** eight eligible Dirty Corpse Starch rations yield one Raw Vat Biomass, with combined selected stacks and retained remainders. A clone can instead yield four meat, two skin and one rib. The live map includes that recovery route; the separate startup coverage log omits it, so its three rib-dependent blocked flags do not prove those recipes are unobtainable. Exact recycling transactions still require their targeted check.

**Bounded runtime result:** session 005 preserved the Ron Mask pact component and selected the seven intended spoiled-organic records. One deliberately selected custom order queued four raw-ore outputs and later left the queue. A stack of six was subsequently visible, but no pre-order cargo baseline was captured, so an exact four-unit cargo increase is not established. The loader audit explains how a missing relative `GameData/BinaryPresetsMap.json` silently creates an empty catalogue. Session 006 confirms that the prior exception did not recur in its corrected-directory Space/time-advance check; package, original saves and fixture cleanup were verified. [Build and runtime receipt](diagnostics/OccultBuildReceipt.json).

**Artwork and reference:** the [art manifest](media/OccultProduction/art-manifest.json) links all six node IDs, positions and parent connections to 10 generated masters and 24 runtime variants. The [original Navigation guide](media/OccultProduction/Reference/navigation-technology-tree.png) is preserved unchanged as a separate user-provided reference. [Research state sheet](docs/crafting/occult-research-ranks.png) · [Inventory/floor/shadow sheet](docs/crafting/occult-starter-sprites.png). The current [six-node tree](docs/crafting/native-occult-tree-347de7d.png), [Culture tooltip](docs/crafting/native-occult-culture-347de7d.png), [Manifestation tooltip](docs/crafting/native-occult-manifestation-347de7d.png) and [ship/provenance record](docs/crafting/native-occult-ui-347de7d.json) verify the 347 placement, icon states and both text-fit corrections. The [bdb tree image](docs/crafting/native-occult-tree-bdb1759.png) remains historical.

**Offline reference and design review:** the [offline map](docs/crafting/recipe-map.html) is pinned to source `347de7d`, with 751 captured records, 53 occult candidates and 21 pattern pools. The [design review ZIP](docs/crafting/ImperiumReforged-Crafting-Design-Review.zip) includes source, companion documents, original/generated/runtime artwork, provenance and the offline map. It contains no runtime binaries. Quantity planning, automatic queue submission, complete exports and remaining family equipment coverage are later work. [Broader design](CRAFTING_TREE_EXPANSION_DESIGN.md).

Planetary Defence Troopers carry Lasguns and wear flak armor. Cadian Guardsmen reinforce them with Lasguns and Guard Frag Grenades. On Terra, Arbites Vigilants carry Judgement shotguns and Arbites Subductors wield Arbites Shock Mauls. You can fight alongside the Arbites or face them as enemies.

At Imperial Tech 3, Arbites, Valhallan, and Steel Legion requisitions enter the Imperial chip pool. Valhallan gear protects against cold and cryo damage. Steel Legion sealed armor protects against poison. At Tech 4, Elysian drop requisitions enter the same pool, and Phobos can issue an orbital Security contract featuring a drop rifleman and a Long-Las spotter.

At Tech 5, the Death Korps of Krieg bring reinforced trench armor, a single-shot Siege Lasgun, and engineer equipment. Havoc can issue a Control contract with a siege rifleman and a combat engineer. The engineer carries one delayed proximity charge that flashes when armed.

At Tech 6, a Tempestus Scion and a Targeter Specialist deploy on the objective floor of the Golden Throne’s Escort contract. Their Hot-shot Lasgun fires single shots or three-charge bursts. The compact power pack reduces scatter, and the specialist’s head implant improves aim at the cost of shock protection.

At Tech 7, Foundry Primaris can issue a data-recovery contract with a Skitarii Ranger and Vanguard. The Ranger carries a Galvanic Rifle and a one-use Auspex that maps the floor. The Vanguard carries an Arc Rifle and a field kit that repairs either Skitarii rifle.

At Tech 8, the Golden Throne can issue a contract to recover the Sororitas Reliquary. A Battle Sister with a bolter and a flamer specialist deploy on the final floor in fire-resistant power armor. The one-use Devotional Ward improves the chance of treating burns for eight turns.

At Tech 9, the Ordo Xenos can issue Phobos Quarantine. An Interrogator with a Power Blade and a Pistoleer with a suppressed Needle Pistol deploy on the final floor. Their four-piece carapace weighs 7.7 kg.

At Tech 10, a Grey Knights Paladin and an Armsman deploy on the final floor of Titan Vault Containment. Broadsword Boarding Defense fields an Armsman and a Psycannon Paladin on the entrance floor. The Paladin requisition draws one recipe from a pool covering warded Terminator armor, the Nemesis Force Sword, Psycannon, Aegis Ward, Power Fist and Tempest Bolt Clips.

The Imperium reaches Tech 11 after earning 5,500 XP at Tech 10. Golden Paladin Guard deploys a gilded Grey Knights Paladin and an Armsman on the entrance floor of the Golden Throne. Each Golden Paladin requisition contains one randomly selected recipe from seven possibilities for upgraded armor, weapons, and the Golden Aegis Ward.

## Chaos Undivided

Chaos Undivided holds six stations, from Hollow Signal Depot on Phobos to Black Meridian in Titan orbit. Its enemies drop Warp Scrip, fabrication salvage, and pact skulls. The Fury Sigil is purchased from Chaos Undivided stations from Tech 1; Furies do not drop it. The Magnum can fabricate ten tiers of Chaos equipment and run skull rituals along two pact paths. At Tech 10, recover Shadowblade from Be’lakor at Black Meridian. [Chaos Undivided campaign and supply catalog](CHAOS_UNDIVIDED_DESIGN.md)

The Inner Writ and The Starless March each have 20 quests. Four raids give the crew a choice between the original target and recovering evidence. That choice can change station ownership, protect faction standing, or send the crew on an escort or a gate defense. [Campaign missions and endings](FACTION_CAMPAIGNS_DESIGN.md)

## Magnum ship support

The **Comms Node** has its own upgrade tree for Security Intrusion, Entrance Override, and Extraction Override. Its ship marker sits by the antenna, aft of the former Ship Weapons position. **Ship Weapons** sits beneath the painted MAGNUM name. **Drop Support** sits over the cargo section, directly above the central Engineering marker. Main systems use large nodes; incremental upgrades use small connecting nodes.

**Security Intrusion: non-story missions only.** Random-site, entrance and extraction turrets each have their own optional Ballistic → Flamer → Beamer → Arc → Lance progression. A location deploys one turret of its highest purchased type; upgrading one location does not change the other two. [Turret replacements, costs and save behavior](diagnostics/CommsTurretProgression.md)

Open **Ship Weapons** or **Drop Support** in the Magnum's ship view. Fit a gun, missile, or laser mount and select the active mount at the bottom of its upgrade tree. Fire for Effect starts on the following turn, with an extra turn of warning for missiles. The aiming view marks affected tiles. Fire can hit the player and friendly units.

Fit the **Supply Pod** for ammunition and dressings or upgrade to the **Mercenary Pod** to call in two clone Guardsmen with standard Lasguns and PDF flak. Purchase troop upgrades as the Imperium reaches each tech tier. Select an unlocked squad on the Drop Support tree. Tech 10 unlocks a Grey Knights Paladin and Armsman. Supplies and troops share a drop reserve. Ship fire uses its own reserve.

The raid HUD shows the available calls and their remaining uses. Choose a call, then confirm a marked strike or clear landing zone. [Full Imperial call-in roster](IMPERIAL_FORMATIONS_DESIGN.md#magnum-mercenary-call-ins)

## Locations

- **Golden Throne (Earth)** is the permanent Imperial capital in the Himalayas. Palace workshops make armor inserts and liners, and the Adeptus Arbites defend the outer precincts.
- **Moon (Luna)** uses biomass from Neith to make Corpse Starch. Dirty Corpse Starch can turn up in Luna raid salvage.
- **Phobos** is an Imperial military station that produces Las Clips and Guard Frag Grenades and stages the Elysian Security and Phobos Quarantine contracts.
- **Havoc** is an Imperial industrial station that turns refined metal and polymer into armor fittings and rolled plate.
- **Foundry Primaris (Mars)** is a permanent Imperial Mechanicus facility protected from random hostile targeting, capture and destruction. The forge refines ore, trades metal supplies, and hosts the Tech 7 Skitarii contract.
- **Artificial Moon (Neith)** is a travelable satellite in Venus orbit. Its guarded Mechanicus seed vault preserves samples of every known plant species in the Imperium, produces biomass, and hosts missions in an orbital lab. The vault starts under Imperial control and can change hands through normal faction capture.
- **Titan Fortress-Monastery** is the permanent Grey Knights stronghold on Titan's surface. Its vault workshop stocks reinforced armor inserts and technical repair kits. Titan Vault Containment opens at Imperial Tech 10.
- **Broadsword Station** is a permanent Imperial fleet base above Titan. Its yards supply firearm and armor repair kits. Broadsword Boarding Defense opens at Imperial Tech 10.

Imperial and Chaos Undivided raids use Quasimorph's room pools with enlarged variants where the original walls and fixtures leave room to extend them. Compatible raid floors join those rooms with wide branching corridors and furnished side strips. Their floors use distinct Imperial or warp-corrupted finishes. Peaceful station visits use the same finishes with each station's own trade stock and stash. [Imperial interiors](media/ImperialInteriors/README.md)

Imperial raid floors supply clean and decayed rations as ordinary loot from Tech 1. Larger floors receive more food, with a clean ration near the arrival point and the rest in reachable containers or on clear floor tiles. Newly generated floors receive 4–16 Corpse Starch and 1–4 Dirty Corpse Starch; already visited floors retain their saved loot.

### Contestable industry on Terra and Mars

Six production sites start under minor human factions and can change hands through normal campaign missions. Terra has Meridian Waterworks, Greenline Provisions, and Junction Reclamation. Mars has Cinder Basin Mine, Hesperia Fabrication, and Vallis Growworks. They supply water, ore, rations, and workshop goods through normal production and freight. [Station owners and production](PRODUCTION_CHAIN_DESIGN.md#contestable-industry-on-terra-and-mars)

## Items

See the [Tech 1–11 supply map](PRODUCTION_CHAIN_DESIGN.md#utility-food-and-standard-supplies-by-tech-level) for food, ammunition, tools, manufacturing materials, and where to get them.

In **1.1.3**, custom Magnum recipes keep every ingredient type, halve the required quantities rounded up to whole items with a minimum of one, and produce twice their previous batch. Production times and schematic requirements stay the same. A Lasgun order uses one rolled armor plate, one armor fitting, and one weapon component to produce two rifles. [Crafting quantities and scope](diagnostics/RecipeCostBatchBalance.md)

The Magnum can produce **four metal smelting feedstock (ore)** or **four Water Bottles** in three base hours without consuming ingredients. These starting recipes are available in new games and added when an existing save loads. Ore can become refined metal, conductive scrap for Cable, silicate, reactive minerals, or carbon-rich feedstock. One silicate makes four basic Glass in two base hours; two silicate make four Industrial Glass in four base hours. Conductor separation is now a shared foundation. [Material recipes and yields](PRODUCTION_CHAIN_DESIGN.md#magnum-material-foundations)

Raw vat biomass separates into **cellulose pulp, organic oil, and chemical extract**. These combine with mineral fractions to supply acid, gasoline, plastics, rubber, paper, propellant, and explosives. Loose metal and chemical stock also feed mechanical parts, electronics, medicines, ammunition, and ordinary cyborg components. Basic material recipes are shared; finished medicine and equipment retain schematic access. [Raw-material flowchart](docs/crafting/raw-material-overview.svg)

**Ore Cargo** can be opened through its normal container action. Each crate yields **12 metal smelting feedstock, 6 silicate feedstock, and 6 conductive scrap**. Opening consumes the selected crate; intact cargo can still be traded or used by station industry. Existing crates receive the same contents when opened.

Redeem Imperial Requisitions at the Golden Throne for trade credit. Sororitas reliquary recovery pays two Purity Seals for advanced fabrication. Phobos, Havoc, Luna, Foundry Primaris, the Golden Throne and Titan stock different Bolt Clips. Avicenna's Clinic stocks the Tissue Graft at DaydreamChems Tech 6. [Specialist suppliers and recipe unlocks](PRODUCTION_CHAIN_DESIGN.md#specialist-supply-and-fabrication)

Support patterns cover conventional ammunition, mines, grenades, repair gear, field devices, and native deployable machines. Imperial workshops produce ballistic, fire, and energy turrets, industrial, recreational, and security robots, and security drones as their tech advances. [Support suppliers and factory unlocks](PRODUCTION_CHAIN_DESIGN.md#support-munitions-and-allied-machines)

Imperial factories trade cells, electrical parts, chassis, logic cores, and AI modules through a production chain connecting Phobos, Havoc, Foundry Primaris, and Broadsword. Automata orders wait for their full material kits. [Component recipes and freight routes](PRODUCTION_CHAIN_DESIGN.md#automata-components-and-freight)

Imperial stations also refine, assemble, and ship rations, field and surgical kits, Las Clips, and specialist Bolt Clips and grenades. [Provisioning factories and freight routes](PRODUCTION_CHAIN_DESIGN.md#rations-medical-supplies-and-munitions-freight)

Base-game stations also manufacture and ship standard rations, medical kits, protective armor, AnCom ammunition, and SBN electronics. The chains share materials across manufacturers through the campaign's trade network. [Base-game manufacturing and freight](PRODUCTION_CHAIN_DESIGN.md#base-game-manufacturers-and-freight)

CoreWard frames and SBN controllers feed thirteen turret, drone, and droid lines, including six new deployable variants. Manufacturers ship finished packages to their depots and offer variant schematics for the Magnum. [Deployable factories and freight routes](PRODUCTION_CHAIN_DESIGN.md#base-game-turrets-drones-and-droids)

Maintenance workshops turn refined metal, recycled polymer, and electrical freight into repair kits, tools, and spare parts. The same parts supply deployable factories, and Cryptoria recovers robot spares for Alliance Arcology's drone lines. [Maintenance factories and freight](PRODUCTION_CHAIN_DESIGN.md#repair-kits-tools-and-spare-parts)

The base-game armories manufacture native weapons and armor through Tech 10, from Civil Resistance militia equipment to rail weapons and corporate power armor. Each manufacturer has its own factories, materials, tech gates, and receiving depots. [Weapon and armor manufacturers](PRODUCTION_CHAIN_DESIGN.md#base-game-weapon-and-armor-manufacturers)

Nineteen manufacturers build and ship cybernetics. The catalog adds 38 implants and ten replacement parts, including active abilities and multi-socket prostheses. [Cybernetics workshops and implant catalog](PRODUCTION_CHAIN_DESIGN.md#cybernetics-and-implant-manufacturers)

Six suppliers turn biomass, medical stock, and recycled packaging into medicines, emergency injectables, and performance drugs. Finished batches ship to faction depots and supply surgical kits, cryolaser production, and specialist munitions. [Pharmaceutical factories and freight](PRODUCTION_CHAIN_DESIGN.md#pharmaceuticals-and-emergency-supplies)

| Category | Items |
| --- | --- |
| Weapons | Lasgun, Long-Las, Bolter, Plasma Pistol, Melta Rifle, Judgement, Arbites Shock Maul, Winter-pattern Lasgun, Steel Legion Lascarbine, Elysian Drop Lascarbine, Krieg Siege Lasgun, Krieg Engineer Shotgun, Krieg Entrenching Tool, Hot-shot Lasgun, Skitarii Galvanic Rifle, Vanguard Arc Rifle, Sororitas Bolter, Sororitas Flamer, Inquisitorial Power Blade, Inquisitorial Needle Pistol, Nemesis Force Sword, Psycannon |
| Ammunition and explosives | Las Clip, Hot-shot Power Pack, Guard Frag Grenade, Steel Legion Smoke Grenade, Krieg Demolition Charge |
| Planetary Defence Force armor | Planetary Defence Force Helmet, Planetary Defence Force Armor, Planetary Defence Force Leggings, Planetary Defence Force Boots |
| Cadian armor | Cadia Helmet, Cadia Armor, Cadia Leggings, Cadia Boots |
| Arbites armor | Arbites Helmet, Vigilant Cuirass, Subductor Cuirass, Arbites Greaves, Arbites Boots |
| Valhallan armor | Valhallan Insulated Cap, Valhallan Greatcoat, Valhallan Lined Trousers, Valhallan Cold-Weather Boots |
| Steel Legion armor | Steel Legion Respirator, Steel Legion Sealed Flak Coat, Steel Legion Overtrousers, Steel Legion Sealed Boots |
| Elysian armor | Elysian Drop Helmet, Elysian Flak Vest, Elysian Harness Trousers, Elysian Landing Boots |
| Krieg armor | Krieg Siege Mask, Krieg Siege Greatcoat, Krieg Trench Trousers, Krieg Trench Boots |
| Scion armor | Scion Carapace Helmet, Scion Carapace Cuirass, Scion Carapace Greaves, Scion Carapace Boots |
| Skitarii armor | Skitarii Rebreather Hood, Skitarii Insulated Coat, Skitarii Sealed Leggings, Skitarii Insulated Boots |
| Sororitas armor | Sororitas Power Helm, Sororitas Power Cuirass, Sororitas Power Greaves, Sororitas Power Boots |
| Inquisitorial armor | Inquisitorial Carapace Helm, Inquisitorial Carapace Coat, Inquisitorial Carapace Greaves, Inquisitorial Carapace Boots |
| Grey Knights armor | Paladin Terminator Helm, Paladin Terminator Cuirass, Paladin Terminator Greaves, Paladin Terminator Sabatons, and their Tech 11 Golden Paladin variants |
| Backpacks | Servitor, Elysian Drop Harness |
| Rations and field supplies | Corpse Starch, Dirty Corpse Starch, Valhallan Tanna, Steel Legion Spare Filter, Skitarii Auspex, Skitarii Field Repair Kit, Devotional Ward, Aegis Ward, Golden Aegis Ward |
| Tech 11 weapons and plans | Golden Nemesis Force Sword, Golden Psycannon, Golden Paladin requisition |
| Implant | Tempestus Targeter |
| Imperial manufacturing plans | Imperial Field Plans, PDF flak patterns, Cadian flak patterns, Guard armory schematic, Imperial ration line schematic, Arbites carapace patterns, Arbites armory schematic, Valhallan winter requisition, Steel Legion requisition, Elysian drop requisition, Krieg siege requisition, Tempestus requisition, Skitarii forge requisition, Sororitas armory requisition, Ordo Xenos requisition, Grey Knights Paladin requisition |
| Other manufacturer plans | AnCom cartridge schematic, AnCom specialist shell schematic, RealWare protective armor pattern, DaydreamChems cigarette line schematic, DaydreamChems field medical schematic, DaydreamChems clinical schematic |
| Metal, textiles, and polymers | Metal smelting feedstock, Sorted metal scrap, Refined metal stock, Rolled armor plate, Armor fittings, Pressed armor insert, Reinforced carapace insert, Reclaimed fiber, Processed textile, Flak laminate, Fitted armor liner, Winter insulation, Steel Legion seal lining, Polymer scrap, Moldable polymer stock |
| Glass, electrical, and munitions components | Silicate feedstock, Glass cullet, Industrial glass, Conductive scrap, Recovered Circuit Assembly, Cell stock, Sealed cell assembly, Empty Las Clip housing, Sorted medium casing, Medium payload module, Empty specialist shell, Sealed flechette module |
| Ration, cigarette, and medical components | Raw vat biomass, Vat ration base, Ration wrapper, Harvested tobacco, Prepared tobacco, Filter stock, Cigarette filters, Empty cigarette box, Loose cigarettes, Sterile dressing roll, Sealed treatment pack, Prepacked dressing tray, Advanced clinical refill, Sterile instrument tray |

Every manufacturing chip contains one recipe randomly selected from its supply pool when the chip is created. Its tooltip identifies that selection; duplicates of an already known recipe stay marked as learned. Normal weapon-ammunition and native armor-set companion unlocks still apply. Plans enter their manufacturer's standard chip pool at the required tech level. The station's current owner, station type, and normal restock rolls determine what appears in trade. Elysian flight armor weighs 5.7 kg, about 29% less than the Cadian set. Imperial Field Plans can contain a recipe for Las Clips, cell stock, sealed cells, or clip housings.

On the Magnum, one Piece of Flesh and one Water Bottle produce eight raw vat biomass. One vat ration base and one wrapper produce eight Corpse Starch; one raw vat biomass and one wrapper produce six Dirty Corpse Starch. Tobacco, implant, graft, and ward recipes use raw biomass or loose parts in place of packaged trade goods. Food cargo, Biowaste Tanks, Research Stations, and Quasi-samples remain available for trade. [Player fabrication ingredients](PRODUCTION_CHAIN_DESIGN.md#manufacturing-and-freight)

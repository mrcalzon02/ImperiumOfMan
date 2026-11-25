# Imperial and Chaos Undivided quest chains

Each faction has a 20-quest story chain in four chapters. The Imperium takes control of a supply corridor across Earth, Luna, Venus, Mercury, and Mars. Chaos Undivided fights nine Bramfatura hosts over routes and anchorages on 16 new asteroids beyond Saturn.

Both chains use Quasimorph's faction quest tracker, story briefings, raid generator, and extraction rules. Victory advances the chain, while a failed raid leaves the current quest available to retry. A **claim** transfers station ownership to the sponsor after a successful raid. Completing either chain returns its sponsor to ordinary faction strategies.

Each quest requires at least **+25 reputation with its sponsor**. If standing falls below +25, the current quest pauses and completed quests stay recorded. The same quest resumes when standing recovers.

## Optional endings

Four raids offer a second way to finish. The briefing names the evidence and explains what it changes. Find it on the first playable floor and pick it up to transmit it. The first completed objective determines the ending. Extract successfully to record the result and receive its debrief. Failed raids leave the choice available on retry.

| Raid | Standard ending | Alternate ending |
|---|---|---|
| Bread and Seal | Destroy the dispatch server. The Imperium claims Greenline, and The Yard Ledgers is a recovery raid. | Transmit the Signed Harvest Charter. Greenline keeps its current owner, Daydream standing is preserved, and the next raid escorts Junction's foreman to a bunker. |
| Silence the Hollow Signal | Destroy the beacon. The Imperium claims Hollow Signal Depot. | Transmit the Spoofing Key. The convoy gets a verified frequency and the depot keeps its current owner. |
| Mute the Buoy | Destroy the tracking scanner and take the crossing by force. | Transmit the Shadow Lane Chart. Chaos takes a route the relay isn't tracking and the crew keeps its Belt standing. |
| Unmake the Seal | Destroy the reliquary. The finale is a hunt for the Olirna baron. | Transmit the Gatekeeper's Binding. The finale uses Control to defend the gate mechanism from an Olirna counterattack. The Undivided claim the Crown on victory in either finale. |

## Campaign interiors

The twenty campaign locations below and the alternate Ebon Crown defense each have a layout profile. The profile chooses native rooms for junctions, side wings and termini, and sets the length, direction and door spacing of the main corridors and their L-shaped branches. Entry and objective decks use different approaches. The native mission supplies each deck's shuttle, lifts, objective rooms and encounters.

Ordinary presets can gain two or three tiles per eligible axis. For these late campaign routes, the generator compares each deck with its standard layout and targets twice the dry floor area. It expands combat rooms along safe native seams, extends the main runs, and connects extra rooms where the layout has space. Wall caps, facades, door positions, and fixtures follow the native order. The route palette runs through the halls, lift approaches, and signature spaces. Other rooms draw their materials from their function.

Native hostile units arrive with a second set of regular squads, and the loot budget doubles. Named objectives and their encounter rules follow the mission. Orbital hulls use windows and open views onto the existing space backdrop. Ground decks use their native rock boundaries. Evidence and lift access components follow the mission's placement rules, and deck count follows the mission and difficulty.

Find the mission component in a first-deck side room to power the lift to the next deck. In **Hold the Ebon Crown**, fit the Gatehouse Control Fuse to activate the defense machinery. Recovery raids use their named evidence as the extraction objective. All 21 routes have their own floor palette and mission pickup. Alternate-ending evidence keeps its own position and sprite.

| Quest | Imperial route and mission find | Chaos route and mission find |
|---|---|---|
| 11 | Caloris ore lanes and hoist galleries · Ore-Hoist Contactor | Bleached Mast nave and transmitter wings · Choir Resonance Plug |
| 12 | Boethius signal exchange and routing archive · Mercury Routing Cipher | Blood Toll altar and gun deck · Tollkeeper's Totem |
| 13 | Goethe gun deck and blockade command rooms · Gun-Deck Authorization Key | Kedge breaker berth and bonded files · Kedge Bill of Sale |
| 14 | Cinder Crucible forge line and controller room · Forge Governor Shunt | Cinder Lock reactor cradles and engine bay · Engine Keeper |
| 15 | Discovery survey vault and inspection bays · Mercury Survey Plate | Bleached Mast replacement berths and muster rooms · Berth Command Tab |
| 16 | Hollow Signal relay and beacon room · Frequency Synchronizer or Spoofing Key | Blue Ruin lens court and observatory · Passage Prism |
| 17 | Cinder Basin shaft approaches and lower galleries · Lift Drum Spindle | Sable Mile pact corridors and guard rooms · Pact Lock Stone |
| 18 | Red Tiangong cargo exam hall and registry · Martian Bond Register | Hound's Rest culverts and kennel passages · Kennel Latch Pack |
| 19 | Foundry Primaris loading levels and interlock bays · Forge Interlock Coupler | Ebon Crown seal vault and reliquary approach · Seal-Key Loop or Gatekeeper's Binding |
| 20 | Hesperia dispatch court and yard · Dispatch Writ Seal | Ebon Crown baron hall · Baron's Signet Key. The alternate defense uses the sallyport and Gatehouse Control Fuse. |

| Dressing | Variations |
|---|---|
| Walls and floors | Imperial and warp-corrupted surfaces follow the native geometry. Perimeter trim follows the first walkable row beside the wall and turns at inside and outside corners. |
| Lifts and shuttles | Lifts use the native hardware, footprint, and floor links. Imperial metalwork meets the facade and filled wall above. |
| Items and objective machines | Mission pickups and custom objective sprites dress the forge controller, two relays, seal reliquary, and Ebon Crown gate controls. Eligible production sites also place large interactable machines in rooms with clear service access. |

[Campaign layout profiles](src/Plugin.CampaignRouteLayouts.cs) · [Room enlargement](src/Plugin.ImperialPresetExpansion.cs) · [Floor atlas packer](media/ImperialInteriors/pack_campaign_floor_kits.py) · [Tile art prompts](src/Art/CampaignDecks/SPRITE_PROMPTS.md) · [Fixture and objective art prompts](src/Art/CampaignDecks/FIXTURE_PROMPTS.md)

## The Inner Writ

The Palace commissions the Magnum to bring the inner system's water, food, freight, and arms under Imperial authority. Up to twelve quests transfer stations to Imperial control, depending on the crew's choices.

| # | Quest | Location | Objective |
| --- | --- | --- | --- |
| 1 | The Palace Writ | Golden Throne, Earth | Recover the sealed writ from a breakaway cell. |
| 2 | The Water Tithe | Meridian Waterworks, Earth orbit | Clear Franche-Comté's pump galleries and **claim** the waterworks. |
| 3 | Bread and Seal | Greenline Provisions, Earth orbit | Destroy Daydream's dispatch server and **claim** the ration works, or transmit the signed harvest charter. |
| 4 | The Yard Ledgers / The Foreman's Passage | Junction Reclamation, Earth orbit | Recover the workshop ledger or escort the foreman to the bunker. **Claim** the yard after victory. |
| 5 | The Fuller Picket | Imperial provisioning station, Fuller | Clear Belt boarders from the convoy's loading bays. |
| 6 | False Clearance | Imperial Fabrication Works, Havoc | Recover CoreWard's forged seal matrix. |
| 7 | The Quern Yard | Sampo Quern, Havoc | Clear CoreWard's seal press and **claim** the yard. |
| 8 | The Cold Archive | Neith Seed Vault, Venus | Destroy Daydream's uplink before it transmits the vault inventory. |
| 9 | The Cult at the Airlock | Veiled Gate, Venus | Kill a Warp Talon and **claim** the launch site. |
| 10 | Customs Blackout | EStation, Mechta | Destroy the private manifest server and **claim** the customs registry. |
| 11 | The Caloris Levy | Caloris Planitia, Mercury | Clear Franche-Comté's concession guard and **claim** the mine. |
| 12 | Counterfeit Orbit | Boethius, Mercury orbit | Recover SBN's routing cipher to free the ore barges. |
| 13 | The Goethe Deadlock | Goethe, Mercury orbit | Eliminate RealWare's blockade commander. |
| 14 | Extinguish the Crucible | Cinder Crucible, Mercury orbit | Destroy the forge controller and **claim** the works. |
| 15 | Discovery Injunction | Discovery, Mercury | Recover AnCom's survey plate and **claim** the freight survey site. |
| 16 | Silence the Hollow Signal | Hollow Signal Depot, Phobos | Destroy the spoofing beacon and **claim** the depot, or transmit the key to give the convoy a clear route. |
| 17 | Secure the Basin | Cinder Basin Mine, Mars | Clear the CoreWard contract troops and **claim** the mine. |
| 18 | The Last Bonded Shipment | Red Tiangong, Mars orbit | Recover Tianming's bonded cargo register. |
| 19 | Prime the Foundry | Foundry Primaris, Mars | Eliminate the Belt commander threatening to shut down the forge. |
| 20 | Under One Standard | Hesperia Fabrication, Mars | Clear the final garrison and **claim** the workshops. |

The chapters secure provisions, enforce Venus customs, open Mercury's ore route, and connect it to Martian production. Requisition rewards include Valhallan, Steel Legion, Ordo Xenos, and Sororitas patterns at their respective operations.

## The Starless March

Chaos Undivided starts on the first crossing beyond Saturn. The campaign follows rival signals, supply routes, and toll gates progressively farther out. Eighteen quests target Bramfatura hosts and two target human outfits supplying or tracking them.

| # | Quest | Fringe outpost | Objective |
| --- | --- | --- | --- |
| 1 | Beyond Saturn's Light | Ashmouth Anchorage | Clear Cattaram's watch and **claim** the first berth. |
| 2 | A Borrowed Wake | Wakescar Listening Post | Recover Ron's wake chart. |
| 3 | Mute the Buoy | Blackglass Relay | Destroy the Belt's tracking scanner or transmit the Shadow Lane Chart. |
| 4 | Copper Hunger | Copper Jaw Breakers | Kill the Scrivnus knight and **claim** the cutter yard. |
| 5 | A Hand on the Lock | Ashmouth Anchorage | Clear the Flauros boarding party and retake the berth. |
| 6 | The Iron Muster | Slag Vigil Foundry | Destroy the Flauros muster server. |
| 7 | Census of Shadows | Salt Cairn Archive | Recover Ron's census of reachable berths. |
| 8 | The Sump Tithe | Sump Choir Cages | Kill Duggur's cage-keeper. |
| 9 | Debt in Kind | Stone Debt Bonded Hold | Destroy the cargo block securing Cattaram's levy. |
| 10 | Forges Don't Pray | Slag Vigil Foundry | Clear Flauros's reclaimers and **claim** the foundry. |
| 11 | The White Frequency | Bleached Mast Choir | Destroy Olirna's transmitter. |
| 12 | Scales of Passage | Blood Toll Gate | Kill Gannix's jaguar knight and **claim** the gun deck. |
| 13 | No Salvage Rights | Kedge Recovery Post | Recover CoreWard's bill of sale and identify its buyers. |
| 14 | The Furnace of Ur | Cinder Lock Furnace | Destroy the Ur battle engine. |
| 15 | The Right to Pass | Bleached Mast Choir | Clear Olirna's replacement choir and **claim** the mast. |
| 16 | Prism Rent | Blue Ruin Observatory | Recover Shartamakhum's passage prism. |
| 17 | The Starless Oath | Sable Mile Redoubt | Eliminate the Scrivnus baron enforcing the rival pact. |
| 18 | The Hound Road | Hound's Rest Kennels | Clear Olirna's hounds and **claim** the embarkation point. |
| 19 | Unmake the Seal | Ebon Crown Gate | Break the reliquary that seals the outer gate or transmit the Gatekeeper's Binding. |
| 20 | A Crown Without Sun / Hold the Ebon Crown | Ebon Crown Gate | Kill the Olirna baron or defend the gate controls. **Claim** the outer gate after victory. |

Each tier gives a cult field supply followed by its corresponding schematic chip. The finale leaves Chaos with seven distinct fringe holdings if the player completes every claim.

## Outer-fringe asteroid outposts

Each asteroid has one station, a map entry, and a native orbital interior chosen for its role. The four orbits range from 1.8 to 3.1 times Saturn's map orbit radius. Each band's four asteroids share an orbit and travel in the same direction. The outposts have small populations and contribute zero base power and technology.

| Orbit | Asteroids and initial occupants |
| --- | --- |
| First crossing | Ashmouth (Cattaram), Wakescar (Ron), Blackglass (Unchained Belt), Copper Jaw (Scrivnus) |
| Middle fringe | Slag Vigil (Flauros), Salt Cairn (Ron), Sump Choir (Duggur), Stone Debt (Cattaram) |
| Far fringe | Bleached Mast (Olirna), Blood Toll (Gannix), Kedge (CoreWard), Cinder Lock (Ur) |
| Outermost belt | Blue Ruin (Shartamakhum), Sable Mile (Scrivnus), Hound's Rest (Olirna), Ebon Crown (Olirna) |

[Quest records and dialogue](src/Plugin.CampaignQuests.cs) · [Native campaign integration](src/Plugin.FactionCampaigns.cs) · [Fringe outpost and garrison records](src/Plugin.FringeOutposts.cs)

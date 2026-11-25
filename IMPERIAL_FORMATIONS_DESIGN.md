# Imperial formations design for Quasimorph

Design draft for the public Quasimorph build **1.0.4.590s.969afc6** (Steam build **25651594**). Weapon figures, armor targets and unlock levels are starting points for playtesting.

## Progression

The Imperial Guard supplies the low-tier equipment and common troops. Each formation keeps its own name, gear and assignments within the existing Imperial campaign faction. The Imperium reaches Tech 11 through normal faction XP, with a 5,500 XP threshold at Tech 10.

| Tech | Current implementation |
| --- | --- |
| **1** | PDF patrols, Guard flak and Lasguns, Las Clips and rations. |
| **2** | Cadian support troops and flak. The Guard mission, payout and supply loop has been tested. |
| **3** | Arbites on Terra, Valhallans on Luna and Steel Legion on Havoc. Each branch has a tested encounter and a requisition in the Imperial chip pool. |
| **4** | The Elysian drop requisition enters the Imperial chip pool. Phobos can field a two-person detachment for its orbital Security contract. The Bolter enters the Imperial reward pool. |
| **5** | Krieg siege equipment and its requisition enter the Imperial pools. Havoc fields a siege rifleman and a combat engineer in its Control contract. The Melta enters the Imperial reward pool. |
| **6** | The Tempestus requisition opens. The Golden Throne fields a Scion and a Targeter Specialist on the objective floor of its native Escort or reverse Elimination contract. |
| **7** | Foundry Primaris fields one Ranger and one Vanguard in its data-recovery contract and reverse Infiltration. |
| **8** | The Golden Throne fields a Battle Sister and a flamer specialist on the final floor of its reliquary recovery contract and reverse Infiltration. |
| **9** | An Interrogator and a Pistoleer support Phobos Quarantine and join the first Control counterattack on the final floor of its reverse contract. |
| **10** | Titan Vault Containment deploys a Nemesis-armed Paladin and an Armsman on the final floor. Broadsword Boarding Defense deploys a Psycannon Paladin and an Armsman on the entrance floor. |
| **11** | Each Golden Paladin requisition contains one randomly selected recipe from seven gilded Grey Knights possibilities. Golden Paladin Guard and its reverse Robbery deploy a Golden Paladin and an Armsman at the Golden Throne entrance. |

## Magnum mercenary call-ins

The first crewed drop pod carries two basic clone Guardsmen. They start with PDF flak, standard Lasguns, and Las Clips. Each mercenary upgrade advances the call-in roster by one Imperial tech tier, up to the Imperium's current campaign tech. Select an unlocked squad before deployment. Pod capacity and call-in count are ship upgrades that apply to the selected squad.

| Tier | Call-in roster unlock |
| --- | --- |
| **1** | Two basic clone Guardsmen in PDF flak with standard Lasguns. |
| **2** | Two Cadian-equipped clone Guardsmen with Cadian flak and Guard Frag Grenades. |
| **3** | Arbites, Valhallan, or Steel Legion loadouts for close quarters, cold, or toxic conditions. |
| **4** | Elysian Drop Rifleman and Drop Spotter. |
| **5** | Krieg Siege Rifleman and Combat Engineer. |
| **6** | Tempestus Scion and Targeter Specialist. |
| **7** | Skitarii Ranger and Vanguard. |
| **8** | Battle Sister and flamer specialist. |
| **9** | Ordo Xenos Interrogator and Pistoleer. |
| **10** | Grey Knights Paladin and Armsman. The Paladin can use the Nemesis Force Sword or Psycannon loadout. |
| **11** | Golden Paladin and Armsman. |

Reuse the existing formation roles and their gear for the specialist squads. Earlier squads stay selectable so the player can match the team to the mission. The crewed pod icon represents the initial two-clone team. The roster shows the formation and equipment selected for the raid.

## Held and worn equipment

The Imperial arsenal has 27 held weapons and 53 armor pieces built on the existing Quasimorph donors. Each formation has its own fittings. The pieces mount on the donor’s existing grip, muzzle, and independent armor joints. The native texture layout retains its shading and receives the formation’s plate, fabric, and hardware tones. [Editable equipment source](src/Art/Imperial/Build-EquipmentModels.py) · [Native fitting integration](src/Plugin.EquipmentFittings.cs)

PDF gear uses webbing and low-profile closures, while Cadian armor uses heavier inserts and a helmet radio cup. The Tech 3 branches use an Arbites face shield, Valhallan insulation, and Steel Legion respirator hoses. Elysian gear uses a compact flight harness and reinforced landing boots. Krieg field kit uses a sealed mask, trench soles, and a supported rifle fore-end. Scions use side optics and a detachable power bay. Skitarii fittings show their insulated cables, and Sororitas armor uses fluted plates. Ordo Xenos carapace has an offset rangefinder and holster fittings. Grey Knights and Golden Paladins use broad gorgets and overlapping shoulder shells. Golden Paladins add a raised crown lip and gilded armor stops.

## Formations for tech four through eleven

| Tech | Formation and supply station | First equipment and upgrade unlocks | First encounter |
| --- | --- | --- | --- |
| **4** | **Elysian Drop Troops**, Phobos | Drop Lascarbine, four-piece flight armor and a compact harness that reduces the weight of its contents. The spotter carries a Long-Las and smoke. | One rifleman and one spotter deploy with native first-floor placement in Phobos Security or its reverse Robbery. |
| **5** | **Death Korps of Krieg**, Havoc | Siege Lasgun, four-piece trench armor, engineer shotgun, entrenching tool and one proximity charge. | One defensive pair on the first floor of Havoc Control or its reverse Sabotage. The charge uses the native arming delay and warning animation. [Warhammer Community’s Krieg army preview](https://www.warhammer-community.com/en-gb/articles/rlzyczbw/death-korps-of-krieg-whats-in-the-new-army-box/) |
| **6** | **Militarum Tempestus Scions**, Golden Throne on Earth | Hot-shot Lasgun with a nine-charge magazine and three-charge bursts, heavy carapace, compact BatteryCells power pack and a head-mounted Targeter. | The native Escort and reverse Elimination routes field one Scion and one Targeter Specialist on the objective floor. [Tempestus Scions datasheet](https://assets.warhammer-community.com/warhammer40000_indexes_astramilitarum_eng_24.09-wscnypwalo.pdf) |
| **7** | **Adeptus Mechanicus Skitarii**, Foundry Primaris on Mars | Heavy-ammo Galvanic Rifle, BatteryCells Arc Rifle, four pieces of insulated armor, Auspex and field repair kit. Recovered Circuit Assemblies feed the forge recipes. | One Ranger with an Auspex and one Vanguard with a repair kit deploy on the entrance floor of the native data-recovery or reverse Infiltration contract. Neith can support a future guarded seed-vault assignment. [Adeptus Mechanicus faction focus](https://www.warhammer-community.com/en-gb/articles/extdvyea/warhammer-40000-faction-focus-adeptus-mechanicus/) |
| **8** | **Adepta Sororitas Battle Sisters**, Golden Throne on Earth | Fire-resistant power armor, a Heavy-ammo bolter, a short-range flamer that uses Gas, and a one-use Devotional Ward that improves burn treatment through the native healing effect. The existing Melta remains a future specialist option. | Recover the Sororitas Reliquary through the native Espionage objective or face the same two roles in reverse Infiltration. The Battle Sister and flamer specialist deploy on the final floor. [Adepta Sororitas faction focus](https://www.warhammer-community.com/en-gb/articles/e6hdtglp/warhammer-40000-faction-focus-adepta-sororitas/) |
| **9** | **Ordo Xenos retinue**, Phobos | Four-piece 7.7 kg Inquisitorial carapace, BatteryCells Power Blade, and a suppressed Needle Pistol that uses Toxic ammunition. The full kit has dedicated inventory and floor sprites. | Phobos Quarantine uses native Sabotage to destroy the specimen containment equipment. An Interrogator and a Pistoleer support the final floor. In reverse Control they arrive with the first attack wave on that floor. The Deathwatch can reinforce a future Ordo Xenos contract. [Inquisitorial Agents roster](https://www.warhammer-community.com/en-gb/articles/20UWMhSR/inquisitorial-agents-sprues-revealed-meet-one-of-the-most-flexible-kill-teams-ever/) |
| **10** | **Grey Knights**, Titan Fortress-Monastery and Broadsword Station | Four pieces of 20 kg Terminator armor with Chaos resistance, a BatteryCells Nemesis Force Sword, and a Heavy-ammo Psycannon. The one-use Aegis Ward improves Chaos wound treatment and reduces Quasimorphosis where active. The seven-recipe requisition uses standard Imperial trade. | Titan's native Elimination and reverse Escort field one Paladin and one Armsman on the final floor. Broadsword's native Security and reverse Robbery field one Paladin Gunner and one Armsman on the entrance floor. [Grey Knights lore and Titan](https://www.warhammer-community.com/en-gb/articles/w7eootuv/starting-a-grey-knights-army-in-warhammer-40000-everything-you-need-to-know-from-painting-to-lore/) · [Paladin equipment](https://assets.warhammer-community.com/warhammer40000_indexes_greyknights_eng_24.09-ucsnbqscje.pdf) |
| **11** | **Golden Paladins**, Golden Throne on Earth | Four pieces of 20.5 kg gilded Terminator armor, Golden Nemesis Force Sword, Golden Psycannon and a ten-turn Golden Aegis Ward. | One Golden Paladin and one Armsman deploy on the entrance floor of native Security and reverse Robbery. |

The station assignments and unlock tiers are for this mod's campaign. Each tier needs a signature weapon, a complete wearable set, a field item or upgrade, a requisition chip with a normal purchase route, and at least one generated encounter. Keep equipment useful by role as late-tier protection rises. Light drop kit still saves weight, winter gear still pays off in cold, and sealed Steel Legion equipment still protects against toxic gas.

Build the first new tiers on the existing equipment slots and maps. Test the Astartes and Paladin silhouettes separately for fitting, held meshes, camera readability and occupied cells before moving them into ordinary generation. A capstone encounter should be rare enough that Guard and earlier specialists still appear at tech ten. Respect the native active-mission limits and preserve existing gear and station IDs.

## What can be reused

| State | Content | Design use |
| --- | --- | --- |
| Inherited source and Unity assets | PDF and Cadian armor, service Lasgun, Long-Las, Servitor backpack, Bolter, plasma pistol and Melta | Start with the two Guard armor sets and the Lasgun. Use the other pieces as shared or limited specialist gear once the formation roster needs them. |
| Authored assets and prototype item records | Las Clip, Bolt Clip and seven specialty Bolt variants, two Corpse Starch rations, Purity Seal, Imperial Requisition, Frag/Krak/Melta grenades, Power Fist, Power Blade, Bolt Pistol, Bolt Rifle and Judgement | Pull the Las Clip, rations and Frag into the common pool. Use Judgement as the candidate Arbites shotgun. Reserve the other assets for specific roles or supply routes before adding them to general drops. |
| Runtime evidence | The restoration prototype created 38 item records, four stations, three reward tables and 22 recipes in the installed game. | Use this as the technical baseline for registration and packaging. Formation encounters, station access, balance, combat effects and the later tuning changes still need their own acceptance tests. |
| New art and integration | Dedicated mid-tier armor sets, the shock maul, compact las variants, respirators, harnesses and formation-specific roster records | Start with verified vanilla behavior donors and build the missing visual identities. The mid-tier roster should be recognizable through its equipment, portraits and encounter roles. |

The proposed roles below use Quasimorph’s native equipment slots, damage types and gear generation. Preserve existing Imperial item and station IDs when reusing records, including the stored `iomMoom` station ID and the existing Judgement ID.

## Formation armor and equipment

| Formation | Head | Torso | Legs | Boots | Utility and trade-off |
| --- | --- | --- | --- | --- | --- |
| **Imperial Guard** | PDF helmet at T1, Cadian helmet at T2 | PDF flak, then Cadian chestplate | Field trousers, then reinforced Cadian leggings | Service boots | Small field pack, ration and dressing. The light set is inexpensive to repair. Cadian armor improves ordinary protection at a weight and repair cost. |
| **Adeptus Arbites** | Visored medium helmet | Hard carapace over an anti-laceration liner | Segmented riot greaves | Reinforced breacher boots | Hemostatic dressing, stun grenade and optional armored vest. Strong against cuts and close-quarters impacts. Higher weight and ordinary elemental protection leave room for other sets. |
| **Valhallan Ice Warriors** | Insulated cap with ear and neck protection | Insulated greatcoat over light flak | Lined trousers | Cold-weather boots | Tanna or winter ration and a warming dressing. Best cold protection in this group, with less pierce protection than carapace. Price the full set as field equipment. |
| **Armageddon Steel Legion** | Sealed respirator helmet | Sealed flak coat with limited plating | Protective overtrousers | Sealed work boots | Spare filter and smoke grenade. Prioritize poison exposure resistance and reliable medium protection. The sealed kit carries extra weight and offers ordinary cold protection. |
| **Elysian Drop Troops** | Lightweight drop helmet | Low-bulk flak over a flight suit | Flexible harness trousers | Reinforced landing boots | Compact drop pack or vest with limited capacity. Lowest carried weight and lower sustained protection. Prototype the mobility advantage through load weight, weapon handling and placement. |
| **Death Korps of Krieg** | Siege mask and helmet | Greatcoat over segmented siege flak | Reinforced trench trousers | Heavy trench boots | Entrenching tool, field dressing and engineer demolition supplies. Weight and repair costs pay for strong blunt/pierce protection and moderate poison resistance. |

### Protection targets

- **Guard** uses PDF flak as the baseline. Cadian medium gear should approach ordinary vanilla medium kit at similar slot, price and weight. Guard is the best option for cheap replacement and general use.
- **Arbites** treats “blood resistant” as protection against cuts and laceration injuries. Use the verified `lacer` damage key on the visor, cuirass and greaves. Start at roughly **25–40% more laceration resistance than same-slot police donors**, with only a modest blunt increase. Prototype a separate wound-control dressing. Validate any direct effect on blood loss before promising it in a tooltip.
- **Valhallan** uses the vanilla warm clothing set as the cold benchmark. Start with roughly **20–30% more cold resistance on the coat and boots**, modest gains on the other pieces, and no tier-wide armor upgrade. Test against both cold and cryo hits and observe `coldEffect` separately.
- **Steel Legion** uses military and miner gas masks as the poison benchmark. Put most of the improvement on the helmet and a smaller amount on the coat. Test poison damage, poison status and cloud exposure separately. A filter should only affect mechanics that can be verified in the game.
- **Elysian** aims for **20–30% lower worn armor weight** than a common medium kit. Use light protection, a compact inventory footprint for support gear and modest carrying capacity. This gives a measurable mobility benefit under the ordinary game rules.
- **Krieg** puts the protection budget into the chest and leg plates. Start near vanilla military medium pierce resistance and add modest blunt protection. Use moderate mask protection and keep the Steel Legion ahead on poison. Test blast damage through its existing blunt and pierce channels.

Compare each piece with the same slot on a vanilla donor at matching durability. Test complete sets and mixed gear. Initial sets use per-piece stats so players can mix a Valhallan coat with an Arbites visor or a Steel Legion respirator.

## Formation armory

Values are initial targets. Ranged damage figures refer to the weapon record **before** ammo, armor, falloff, fire mode and critical effects. Capacity is loaded rounds or charges. Prices are base item values before station and reputation modifiers. Verify actual close and long-range outcomes against the vanilla donors before locking these numbers.

| Formation | Main arm | Starting target | Support arm and supply |
| --- | --- | --- | --- |
| **Guard** | Existing **service Lasgun**. Compare with `pmc_assault_1` for rifle handling and `battery_crafted_ammo` for early economy. | T1, **12–25 damage**, 6–7 cells of range, **10 charges**, base price **150–200**. Single shot and deliberate burst. | Existing Las Clip as the first unlock, with `BatteryCells` compatibility kept. T2 support uses the same rifle and a frag grenade or dressing. Offer the shared Long-Las through later requisitions. |
| **Arbites** | **Judgement combat shotgun**, using `common_shotgun_1` as the normal handling donor. | T3, **28–60 damage**, range 4–5, **5 shells**, price **240–300**. Give the close-range mode a reason to exist through handling and falloff. | **Shock maul** from `fra_police_club_1`, T3–4, **23–31 damage** before its linked ammo effect, ordinary battery cells, price **180–230**. Standard `Shells` first. Add an uncommon lower-capacity slug load only after auditing shell interactions. |
| **Valhallan** | **Winter-pattern Lasgun**, derived from the service rifle, with cold-weather reliability represented through durability and straightforward repairs. | T3, **16–29 damage**, range 7, **10–12 charges**, price **190–250**. Low scatter and ordinary damage keep it useful to cold-weather troops. | A spotter or marksman uses the shared **Long-Las** and a warming kit. Add a portable heated ration or Tanna using a vanilla food/drink donor. Cold protection comes from the kit. |
| **Steel Legion** | **Steel Legion Lascarbine**, using `laser_smg_1` for compact pulse handling and the shared battery family. | T3–4, **19–28 damage**, range 5, **14–16 charges**, price **215–275**. Strong short bursts for fighting through industrial corridors. | Rifleman carries a smoke grenade or filter. A T5 support trooper fields a guarded Melta or a limited demolition item. Smoke supplies concealment. Verify any filtering effect against the actual hazard system. |
| **Elysian** | **Drop Lascarbine**, using the same compatibility family and a lighter handling profile. | T4, **18–26 damage**, range 4–5, **10–12 charges**, price **200–265**. Lightweight weapon, quicker reload than the Steel carbine, less sustained fire. | **Drop marksman** uses the existing Long-Las or a light scoped variant based on `laser_marksman_1` at T5. Spotters carry smoke and small packs. Secure the insertion role using existing mission deployment points first. |
| **Krieg** | **Siege Lasgun**, using the service Lasgun as base, with steadier single-shot fire and slower reload. | T5, **22–34 damage**, range 6–7, **8–10 charges**, price **260–330**. Support a defensive firing line with the weaker close-range profile kept visible. | **Engineer** uses a short shotgun based on `military_shotgun_1`, an entrenching tool, and one limited explosive. Later Watchmasters can draw a Bolt Pistol or Power Blade from shared requisitions. |

**Shared inventory rules**

- Keep Las weapons on `BatteryCells`, Judgement and the Krieg shotgun on `Shells`, and existing Bolt weapons on their compatible rifle-ammo family while the old inventory IDs are in use. Verify the loaded round, reserve ammo, reload UI and enemy resupply through the actual item factory. Changing a default round should preserve the old usable ammunition.
- The standard Bolt Clip opens at Tech 4. The seven specialty rounds use the existing Heavy ammo family, with different payloads and suppliers listed in the [supply map](PRODUCTION_CHAIN_DESIGN.md#specialist-supply-and-fabrication).
- Use Corpse Starch as a common single-use ration. Keep Dirty Corpse Starch as a cheap shortage or salvage item with a visible contamination chance. Let winter rations and compact drop rations earn their place through niche value such as stack size or carried weight, with nutrition compared per use and per kilogram.
- Reuse ordinary dressings, smoke and standard explosives while each formation is proving its kit. A reusable shield, jump ability, vehicle or new hazard immunity needs a separate gameplay and asset specification.

## Patrol and mission roster

Start with one ordinary combat role and one specialist per formation. Add a leader only when its squads reliably equip the intended weapons and armor. Select role groups by Imperial ownership, actual station or mission context, and faction technology. Other owners keep their own pools after capture.

| Formation | Initial roles and issued loadouts | Formation encounter | Cap and later role |
| --- | --- | --- | --- |
| **Guard** | **PDF rifleman** with Lasgun and light flak. **Cadian support trooper** with Cadian armor, Lasgun and one Frag or dressing. | A 2–3 person crew on basic patrol and defense floors. Guard soldiers bulk out mid-tier squads. | One grenade carrier per early squad. Add a Guard sergeant only once the normal rifleman and support loadouts can be reproduced. |
| **Arbites** | **Vigilant** with Judgement, standard shells and a visor. **Subductor** with shock maul, battery cells and heavier carapace. | Security, escort or control missions around Terra. Short sight lines and paired ranged/melee coverage. | One maul specialist per first squad. Add a T5 sergeant once stagger or shock chains allow a clear response. |
| **Valhallan** | **Ice rifleman** with winter Lasgun, warming kit and insulated coat. **Ice marksman** with Long-Las in a mixed group. | Cold-room or exposed-hazard assignments supported by Luna. Riflemen hold approaches and the marksman covers longer lanes. | One marksman per group. Standard Guard remains eligible when no cold map variant is available. |
| **Steel Legion** | **Carbine trooper** with sealed kit and smoke. **Industrial support trooper** with a spare filter and a limited demolition load. | Havoc production, recovery or escort missions in industrial layouts. Medium-range pressure through lanes and chokepoints. | One specialist per group. Delay regular Melta use until the player can recognize and interrupt it. |
| **Elysian** | **Drop rifleman** with light kit and drop carbine. **Drop spotter** with Long-Las and smoke. | Phobos orbital Security and reverse Robbery use native dungeon placement. A separate reinforcement wave can use a shuttle or side entry in a later iteration. | One Elysian pair on the first floor. Ordinary units fill the remaining encounter budget. |
| **Krieg** | **Siege rifleman** with Siege Lasgun, trench armor and a bandage. **Combat engineer** with a shotgun, entrenching tool and one proximity charge. | Havoc Control or reverse Sabotage. Both roles use the game’s defensive combat behavior. | One engineer per encounter. The charge gives a three-turn arming delay and uses the native mine warning animation. |

Verify the complete chain from faction and station through mission template, unit group, mob class, inventory, reserves and launch. A valid item record by itself leaves open whether a trooper ever receives it. For signature roles, start with a defined equipment core so the correct weapon and armor appear consistently. Use weighted tables for supplies and later substitutions. Ensure a role's technology requirement covers every piece of gear the role is issued.

## Stations, acquisition and manufacture

The Imperial capital keeps its original station identifier. Titan's fortress-monastery and orbital fleet base are available from campaign start, with their contracts unlocked at Imperial Tech 10.

| Station | Planned role | Production and player route |
| --- | --- | --- |
| **Golden Throne** (`iomTerra`, Himalayas, Earth) | Permanent `Military` capital at the Imperial Palace. Uses the existing fortified interior and retains the Arbites Security route. Population 22,314 / 60,840. | Two rolled plates and one processed textile produce two pressed armor inserts and one fitted liner in 48 base hours. [Imperial Palace lore](https://www.warhammer-community.com/en-gb/articles/faggbycw/saturnine-lore-focus-the-imperial-palace/) |
| **Luna** (`iomMoom`, Fuller) | Field logistics with a Valhallan detachment. | Produce Corpse Starch, Las Clips and winter supplies. A cold-operations contract or requisition opens the coat, boots and winter Lasgun. Luna’s orbital location is separate from the temperature of the actual mission map. |
| **Phobos** (`iomPhobos`, Mars) | Military depot, Elysian dispatch and Ordo Xenos quarantine site. | Produces standard munitions and stages Elysian Security at Tech 4 and Phobos Quarantine at Tech 9. Both requisitions follow native Imperial stock and reward rolls. |
| **Havoc** (`iomHavoc`, Venus) | Industrial manufacture, Steel Legion Security and Krieg Control. | The Tech 5 Krieg requisition follows the standard Imperial stock and reward rolls. Siege armor and engineer equipment use pressed inserts, flak and fittings with longer production cycles. |
| **Foundry Primaris** (`iomFoundryPrimaris`, Mars) | Permanent `Industrial` station using the Mars surface mission layout. Population 21,700 at start, capped at 46,800. | Uses Red Tiangong's Mars industrial economy. Smelting converts four units of ore into two refined metal and one sheet in 40 base hours. Trades metal, sheets and fittings. Hosts Skitarii data recovery at Tech 7. |
| **Neith Seed Vault** (`iomNeithVault`, `iomNeith`, Venus orbit) | Guarded `Lab` station on a separate artificial satellite. Population 10,900 at start, capped at 28,400. | Uses Venus orbital lab trade rules and an Imperial research production list. The vault converts two bottles of water into four vat biomass in 54 base hours. Its guarded archive holds samples of the Imperium's known plant species. [Neith lore](https://wh40k.lexicanum.com/wiki/Neith) |
| **Titan Fortress-Monastery** (`iomTitan`, Titan surface) | Permanent `Military` fortress with a native moon-surface layout. Population 15,300 / 34,500. | Two rolled plates and one unit of scientific equipment produce a reinforced carapace insert and a technical repair kit in 72 base hours. Titan Vault Containment uses a native Elimination objective at Tech 10. [Grey Knights lore and Titan](https://www.warhammer-community.com/en-gb/articles/w7eootuv/starting-a-grey-knights-army-in-warhammer-40000-everything-you-need-to-know-from-painting-to-lore/) |
| **Broadsword Station** (`iomBroadsword`, Titan orbit) | Permanent `Military` fleet base with a separate orbital layout. Population 9,900 / 22,600. | Two refined metal and one unit of electrical parts produce a firearm repair kit and an armor repair kit in 60 base hours. Broadsword Boarding Defense uses a native Security objective at Tech 10. [Broadsword Station](https://wh40k.lexicanum.com/wiki/Broadsword_Station) |

Foundry has base power and tech XP multipliers of 2 and 1. Neith has multipliers of 3 and 1. The game applies these values and the difficulty setting to each native recipe's income yields. The custom smelting and biomass recipes supply goods. Imperial arrival and raid briefings use the approved Mechanicus emblem at both sites.

Foundry's attack immunity keeps it out of the normal hostile mission rotation. The scheduled Tech 7 data-recovery contract and its reverse use the native Mars surface layout. Neith is eligible for normal campaign targeting. Mission rewards use the game's standard values.

- **Common supply** comes from Guard rewards, barter and the first field plans. Start recipes with existing components such as rags, plastic, armor plates, glass, acid and weapon parts. Use station output for bulk items so one blueprint roll cannot strand all Las users.
- **Formation supply** uses the standard Imperial chip pool. Requisitions join at their formation's tech tier. Trade stock follows the station owner's pool, the station type and the normal restock rolls. Equipment rewards and enemy drops give players other ways to acquire finished gear.
- **Specialist supply** uses Imperial Requisitions redeemed for trade credit at the Golden Throne. Sororitas reliquary recovery supplies Purity Seals for the Melta Grenade and Golden Aegis Ward recipes.
- **Ownership and stock** follow the live station owner and intended tech band. Prove that each station actually demands its inputs, completes at least one production cycle and exposes its expected stock to the player. Recheck after capture and reload so Imperial-only stock has a defined behavior when control changes.

### Permanent imperial capitals

The Golden Throne keeps `iomTerra` on Earth. Its station information identifies the **Himalayas**. Titan Fortress-Monastery and Broadsword Station share the Titan travel destination, then branch into separate station entries with their own orders, stock and layouts.

The Golden Throne, Titan Fortress-Monastery and Broadsword Station are immune to automatic hostile targeting, capture and station deletion. The campaign load hook restores Imperial ownership and inserts missing permanent sites by station ID. The Titan contracts are offered independently at Tech 10 and have separate 28-day intervals between offers. They use normal mission reward calculations.

Foundry Primaris also has permanent Imperial ownership. Its station guard blocks capture and deletion of `iomFoundryPrimaris` and restores its protected status during campaign updates. Neith starts Imperial-owned and follows the ordinary ownership rules.

The capital and Titan sites have power and tech XP multipliers of 2 and 3 at the Golden Throne, 1 and 2 at the fortress, and 2 and 1 at Broadsword. The capital demands training and armor supplies. Titan also uses scientific equipment. Broadsword demands ore cargo, tools and communications supplies in addition to its yard inputs. Their custom workshops feed existing armor and repair equipment. Grey Knights-specific equipment and troops are tracked in the Tech 10 formation design.

## Tuning and acceptance

Expose per-item base price, per-weapon minimum and maximum damage, and firearm magazine size via Mod Configuration Menu. Add faction-level mission roll chance and payout multiplier. The payout multiplier applies to the reward budget before the game chooses rewards. Use a **10%** initial Imperial mission chance and **1.0×** payout as reference defaults from the restoration baseline. Keep station cooldowns, neighbor eligibility, faction power and native tech mission limits in the comparison. The installed build allows three active missions per faction at tech 1–2, four at 3–5, five at 6–8, and six at 9–10. Track ordinary chance-based mission rolls and missions launched by a strategy or scenario as distinct events during testing. [Mod Configuration Menu](https://steamcommunity.com/sharedfiles/filedetails/?id=3469678797)

Measure the following on the same game build with matching difficulty and reproducible test conditions.

| Check | Pass condition |
| --- | --- |
| **Weapons and ammo** | Each primary weapon spawns loaded, reloads from the displayed compatible stock, uses its intended fire modes, and performs its intended role at near and far range. Record shots per kill, turns to disable, reload time and ammunition consumed. Check the shock maul against shock-resistant targets and compare Judgement against two same-tier shotguns. |
| **Armor and field gear** | Each piece equips in the correct slot. Compare worn weight, durability, repair cost, damage taken and status accumulation. Arbites win the laceration comparison, Valhallans the cold comparison, Steel Legion the poison comparison, Elysians the weight comparison and Krieg the defensive attrition comparison. |
| **Troopers and squads** | Run at least 20 seeded equipment pulls for each signature role across its intended tech band. Every pull has the intended primary and four valid armor pieces. Inspect several in-mission raids for reserve ammo, visual silhouettes, squad compositions and the intended specialist caps. |
| **Stations and contracts** | Imperial stations, Neith and the planned Titan locations appear with the intended owner and location. Complete one production cycle per manufacturing station. Generate, finish and collect an Imperial contract. Confirm a second station remains eligible while the first is under cooldown. Repeat with an existing Imperial save, a captured station and each permanently protected location. |
| **Progression and settings** | Obtain a formation weapon and armor through ordinary play, craft one unlocked item, then save and reload with the equipped gear. Confirm price, damage and magazine edits reach the intended records. Check the mission frequency at 0%, the baseline and a higher setting, and compare the same seeded reward roll at 1.0× and 2.0×. |

## Implementation checkpoints

1. **Guard baseline.** Finish the smallest playable loop with PDF/Cadian flak, service Lasgun, Las Clip, Corpse Starch, two equipped roles, station barter and one production unlock. Confirm one Imperial mission starts and pays out before expanding the roster.
2. **Arbites first.** Add the four carapace pieces, set the Judgement profile, and prototype the battery-powered shock maul. Prove the two very different roles in one Terra security encounter. Use this implementation to establish the record and art checklist for the other formations.
3. **Environmental equipment.** Add Valhallans, then Steel Legion. Demonstrate distinct cold and poison performance against controlled vanilla hazards and validate the Luna and Havoc supply routes independently.
4. **Mobile encounter.** Add Elysian lightweight kit and the two carbine/spotter roles. Prove Phobos contract generation and a fair reinforcement arrival with the current spawn system.
5. **Siege encounter.** Add Krieg trench armor, siege rifle and engineer. Add a Watchmaster and specialty gear only after the positional encounter, manufacturing inputs and campaign save checks pass.
6. **Late campaign.** Integrate Scions, Skitarii, Sororitas, the Ordo Xenos retinue and Grey Knights in that order. Test the tier before and after each unlock, buy the chip, craft one signature item, launch and pay out a native encounter, and reload a save with that equipment. Check that each new tier preserves an acquisition route for earlier gear.

For each formation, track art and runtime separately. A finished gear entry includes inventory, floor and shadow art, held or worn visuals where supported, a valid donor or descriptor, sound behavior, localization, ammo and repair references, enemy assignment, and at least one acquisition route. Record whether it was loaded, equipped, used in a raid and restored from a save.

## References

- Quasimorph implementation reference for the native loader and hooks. [Developer modding guide](https://steamcommunity.com/sharedfiles/filedetails/?id=3281671312)
- Armory and encounter references for the Imperial Guard and Arbites. [Astra Militarum Lasgun](https://www.warhammer-community.com/en-gb/articles/cvvjq1ua/las-canon-how-the-astra-militarums-indomitable-lasgun-works/) · [Adeptus Arbites Exaction Squads](https://www.warhammer-community.com/en-gb/articles/FX0RQ4Re/kill-teams-of-the-imperium-enforce-the-emperors-will-in-the-new-edition/)
- Formation references for insulation, sealed operations, airborne roles and siege engineers. [Valhallan equipment](https://www.warhammer-community.com/en-gb/articles/xordlfvg/commissar-cain-comes-to-the-tabletop/) · [Armageddon](https://www.warhammer-community.com/en-gb/articles/wmgvv3rx/new40k-round-table-the-lore-of-armageddon/) · [Elysian Sniper Squad](https://assets.warhammer-community.com/eng_11-06_warhammer40000_faction_pack_astra_militarum-etf9ihpodv-mbqdsa1bfv.pdf) · [Death Korps and Combat Engineers](https://www.warhammer-community.com/en-gb/articles/t3bhbkgg/sunday-preview-the-galaxy-calls-and-krieg-answers/)

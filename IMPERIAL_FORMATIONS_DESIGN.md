# Imperial formations design for Quasimorph

Design draft for the public Quasimorph build **1.0.4.590s.969afc6** (Steam build **25651594**). Weapon figures, armor targets and unlock levels are starting points for playtesting.

## Progression

The Imperial Guard supplies the low-tier equipment and common troops. Five mid-tier formations specialize in different jobs within the same Imperial campaign faction. The Adeptus Arbites retain their identity as Imperial law enforcement through their name, gear and assignment to security operations. The other formations use their own regimental names, uniforms and loadouts.

| Tech | Player equipment and enemy progression |
| --- | --- |
| **1–2** | Guard flak, service lasgun, shared power cells, basic rations and field equipment. Tier 2 adds Cadian armor, a support role and access to better field kits. These items stay affordable and repairable throughout the campaign. |
| **3–4** | Each formation introduces a recognizable four-piece armor set, signature weapon and two enemy roles. The first specialty appears at tier 3. Tier 4 gives each formation a reliable trade or blueprint route. |
| **5–6** | Formation leaders and specialists join mixed squads. Limited specialist arms, specialty rounds and production recipes expand the same roles. Guard riflemen remain in many encounters. |

The build order is **Imperial Guard → Adeptus Arbites → Valhallan Ice Warriors → Armageddon Steel Legion → Elysian Drop Troops → Death Korps of Krieg.** Implementation order is independent of when the formations appear in a finished campaign. Krieg’s first gear lands at tier 4 alongside the others’ mid-tier equipment.

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
| **Elysian** | **Drop Lascarbine**, using the same compatibility family and a lighter handling profile. | T3–4, **18–26 damage**, range 4–5, **10–12 charges**, price **200–265**. Lightweight weapon, quicker reload than the Steel carbine, less sustained fire. | **Drop marksman** uses the existing Long-Las or a light scoped variant based on `laser_marksman_1` at T5. Spotters carry smoke and small packs. Secure the insertion role using existing mission deployment points first. |
| **Krieg** | **Siege Lasgun**, using the service Lasgun as base, with steadier single-shot fire and slower reload. | T4, **22–34 damage**, range 6–7, **8–10 charges**, price **260–330**. Support a defensive firing line with the weaker close-range profile kept visible. | **Engineer** uses a short shotgun based on `military_shotgun_1`, an entrenching tool, and one limited explosive. T5–6 Watchmasters can draw a Bolt Pistol or Power Blade from shared requisitions. |

**Shared inventory rules**

- Keep Las weapons on `BatteryCells`, Judgement and the Krieg shotgun on `Shells`, and existing Bolt weapons on their compatible rifle-ammo family while the old inventory IDs are in use. Verify the loaded round, reserve ammo, reload UI and enemy resupply through the actual item factory. Changing a default round should preserve the old usable ammunition.
- Put specialty Bolt rounds in the shared T4–6 arsenal. Give each a trade-off in damage type, range, stack size, cost or availability. Test Kraken and one elemental round before adding all seven to reward rotation. Use `lacer` for Metal Storm if its role remains cutting damage. The installed damage table uses this key.
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
| **Elysian** | **Drop rifleman** with light kit and drop carbine. **Drop spotter** with smoke or a light marksman rifle. | Phobos orbital reinforcement, counterattack or escort scenarios. Smaller groups use existing side or shuttle spawn locations when the map supports them. | One small reinforcement wave at a time. Spawn outside immediate adjacency to the player and use the scenario’s normal warning and turn order. |
| **Krieg** | **Siege rifleman** with siege Lasgun and trench gear. **Combat engineer** with close weapon and one demolition item. | Havoc siege, defense or control assignments. Riflemen hold a position while the engineer covers a door or breach. | One engineer per squad and one placed threat in the first encounter. Add a T5–6 Watchmaster after the engineer’s behavior and warnings are proven. |

Verify the complete chain from faction and station through mission template, unit group, mob class, inventory, reserves and launch. A valid item record by itself leaves open whether a trooper ever receives it. For signature roles, start with a defined equipment core so the correct weapon and armor appear consistently. Use weighted tables for supplies and later substitutions. Ensure a role's technology requirement covers every piece of gear the role is issued.

## Stations, acquisition and manufacture

Reuse the four Imperial locations and their existing game identifiers. Establish jobs for each station before adding sectors or ships.

| Station | Planned role | Production and player route |
| --- | --- | --- |
| **Terra** (`iomTerra`, Earth) | Imperial administration, Guard supply and Arbites jurisdiction. | Field requisitions, early PDF gear and Guard plans. T3 Arbites security contracts lead to the shotgun and carapace plans. The maul becomes available through a later security requisition. |
| **Luna** (`iomMoom`, Fuller) | Field logistics with a Valhallan detachment. | Produce Corpse Starch, Las Clips and winter supplies. A cold-operations contract or requisition opens the coat, boots and winter Lasgun. Luna’s orbital location is separate from the temperature of the actual mission map. |
| **Phobos** (`iomPhobos`, Mars) | Military depot and Elysian dispatch. | Produce standard munitions, drop harnesses and carbine parts. Orbital contracts unlock drop kit and a later marksman requisition. Ordinary Guard gear stays in stock for players without access to drop contracts. |
| **Havoc** (`iomHavoc`, Venus) | Industrial manufacture, Steel Legion security and later Krieg siege supply. | Barter plates, filters and sealed gear first. Add a distinct later recipe line for siege armor and engineer supplies. Expensive inputs and longer production cycles keep demolition equipment bounded. |

- **Common supply** comes from Guard rewards, barter and the first field plans. Start recipes with existing components such as rags, plastic, armor plates, glass, acid and weapon parts. Use station output for bulk items so one blueprint roll cannot strand all Las users.
- **Formation supply** starts with a reliable signature route. A rank-appropriate contract or redemption guarantees the formation’s introductory plans or a first signature item. Reward tables and enemy drops provide alternatives. Use small plan groups or explicit recipe unlocks so the player can target the armor or weapon they need.
- **Specialist supply** consumes Imperial Requisitions for access and Purity Seals for selected higher-cost plans or recipes. For example, a winter coat uses textiles and insulation, a sealed respirator uses a mask and filter materials, and a siege cuirass uses plate and fittings. Price the end product against the whole input basket and the manufacturing time.
- **Ownership and stock** follow the live station owner and intended tech band. Prove that each station actually demands its inputs, completes at least one production cycle and exposes its expected stock to the player. Recheck after capture and reload so Imperial-only stock has a defined behavior when control changes.

## Tuning and acceptance

Expose per-item base price, per-weapon minimum and maximum damage, and firearm magazine size via Mod Configuration Menu. Add faction-level mission roll chance and payout multiplier. The payout multiplier applies to the reward budget before the game chooses rewards. Use a **10%** initial Imperial mission chance and **1.0×** payout as reference defaults from the restoration baseline. Keep station cooldowns, neighbor eligibility, faction power and native tech mission limits in the comparison. Public-build tech levels 3–5 allow up to four active missions per faction, and tech level 6 allows up to five. Track ordinary chance-based mission rolls and missions launched by a strategy or scenario as distinct events during testing. [Mod Configuration Menu](https://steamcommunity.com/sharedfiles/filedetails/?id=3469678797)

Measure the following on the same game build with matching difficulty and reproducible test conditions.

| Check | Pass condition |
| --- | --- |
| **Weapons and ammo** | Each primary weapon spawns loaded, reloads from the displayed compatible stock, uses its intended fire modes, and performs its intended role at near and far range. Record shots per kill, turns to disable, reload time and ammunition consumed. Check the shock maul against shock-resistant targets and compare Judgement against two same-tier shotguns. |
| **Armor and field gear** | Each piece equips in the correct slot. Compare worn weight, durability, repair cost, damage taken and status accumulation. Arbites win the laceration comparison, Valhallans the cold comparison, Steel Legion the poison comparison, Elysians the weight comparison and Krieg the defensive attrition comparison. |
| **Troopers and squads** | Run at least 20 seeded equipment pulls for each signature role across its intended tech band. Every pull has the intended primary and four valid armor pieces. Inspect several in-mission raids for reserve ammo, visual silhouettes, squad compositions and the intended specialist caps. |
| **Stations and contracts** | In a fresh campaign, all four stations appear with the intended owner and location. Complete one production cycle per station. Generate, finish and collect an Imperial contract. Confirm a second station remains eligible while the first is under cooldown. Repeat with an existing Imperial save and after a station changes owner. |
| **Progression and settings** | Obtain a formation weapon and armor through ordinary play, craft one unlocked item, then save and reload with the equipped gear. Confirm price, damage and magazine edits reach the intended records. Check the mission frequency at 0%, the baseline and a higher setting, and compare the same seeded reward roll at 1.0× and 2.0×. |

## Implementation checkpoints

1. **Guard baseline.** Finish the smallest playable loop with PDF/Cadian flak, service Lasgun, Las Clip, Corpse Starch, two equipped roles, station barter and one production unlock. Confirm one Imperial mission starts and pays out before expanding the roster.
2. **Arbites first.** Add the four carapace pieces, set the Judgement profile, and prototype the battery-powered shock maul. Prove the two very different roles in one Terra security encounter. Use this implementation to establish the record and art checklist for the other formations.
3. **Environmental equipment.** Add Valhallans, then Steel Legion. Demonstrate distinct cold and poison performance against controlled vanilla hazards and validate the Luna and Havoc supply routes independently.
4. **Mobile encounter.** Add Elysian lightweight kit and the two carbine/spotter roles. Prove Phobos contract generation and a fair reinforcement arrival with the current spawn system.
5. **Siege encounter.** Add Krieg trench armor, siege rifle and engineer. Add a Watchmaster and specialty gear only after the positional encounter, manufacturing inputs and campaign save checks pass.

For each formation, track art and runtime separately. A finished gear entry includes inventory, floor and shadow art, held or worn visuals where supported, a valid donor or descriptor, sound behavior, localization, ammo and repair references, enemy assignment, and at least one acquisition route. Record whether it was loaded, equipped, used in a raid and restored from a save.

## References

- Quasimorph implementation reference for the native loader and hooks. [Developer modding guide](https://steamcommunity.com/sharedfiles/filedetails/?id=3281671312)
- Armory and encounter references for the Imperial Guard and Arbites. [Astra Militarum Lasgun](https://www.warhammer-community.com/en-gb/articles/cvvjq1ua/las-canon-how-the-astra-militarums-indomitable-lasgun-works/) · [Adeptus Arbites Exaction Squads](https://www.warhammer-community.com/en-gb/articles/FX0RQ4Re/kill-teams-of-the-imperium-enforce-the-emperors-will-in-the-new-edition/)
- Formation references for insulation, sealed operations, airborne roles and siege engineers. [Valhallan equipment](https://www.warhammer-community.com/en-gb/articles/xordlfvg/commissar-cain-comes-to-the-tabletop/) · [Armageddon](https://www.warhammer-community.com/en-gb/articles/wmgvv3rx/new40k-round-table-the-lore-of-armageddon/) · [Elysian Sniper Squad](https://assets.warhammer-community.com/eng_11-06_warhammer40000_faction_pack_astra_militarum-etf9ihpodv-mbqdsa1bfv.pdf) · [Death Korps and Combat Engineers](https://www.warhammer-community.com/en-gb/articles/t3bhbkgg/sunday-preview-the-galaxy-calls-and-krieg-answers/)

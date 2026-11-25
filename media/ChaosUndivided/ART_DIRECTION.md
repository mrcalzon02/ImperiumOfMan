# Chaos Undivided artwork

The built-in ImageGen tool produced six source boards. Item icons, floor sprites, and creature portraits live under `src/Art/ChaosUndivided`. The inventory review sheet renders the packed sprites at twice their native size. The bestiary sheet pairs the original creature concepts with the playable pact icons.

## In-world enemies

Each enemy starts with an installed Quasimorph actor mesh. The mod reshapes the donor and applies its native animation frames to the new model. Each role has separate geometry and materials for features such as wings, armor, claws, and weapon fittings. The Warp Talon uses lighter armor edges and exposed jump-pack coils so it reads against the dark floor from the front and rear. [Editable fitting source](../../src/Art/ChaosUndivided/Build-EnemyModels.py) · [Native actor integration](../../src/Plugin.ChaosModels.cs)

## Held weapons and wearable armor

Weapon fittings leave the donor grip and muzzle clear. The Shard Pistol uses a raised reservoir and split guides, the Excruciator has return conduits on both sides, and the Reforged Harvester has cooling fins, a distinct feed bridge, and an underslung drum. The Hellforged Blade has a broad heated edge. Shadowblade has twin curved rails with a visible space between them. Armor fittings follow the independent head, chest, shoulder, shin, and foot mounts, with separate parts for buckles, bone ribs, rebreathers, and heat shielding. New finishes preserve the donor texture layout and use different tones for recessed cloth, plates, and hardware. [Editable equipment source](../../src/Art/ChaosUndivided/Build-EquipmentModels.py) · [Native fitting integration](../../src/Plugin.EquipmentFittings.cs)

## Prompt set

The creature board called for eleven original three-quarter tactical silhouettes on a charcoal field. The four-column grid ran from a low, bone-jawed Fury through a thorn-spitting Mutant, a tendrilled Torment, an asymmetrical Spawn, a hooked Possessed legionary, and a split-vane Warp Talon. It continued through the six-legged Venomcrawler, the one-armed Fleshmetal Projector on the Obliterator, the industrial Soul Grinder, the horned Daemon Prince, and a violet-winged manifestation of Be’lakor.

The four equipment boards used transparent four-column by five-row atlases with one isolated object per cell. The early armor board specified stitched hides, burgundy flak, ivory rib plates, asymmetrical iron and tendon, and swept assault armor. The late board specified insulated copper, fused ivory, orange siege steel, molten-edged panoply, and violet-black mantle armor. Each row supplied head, torso, legs, and boots. The weapon and field-supply board specified the silhouettes in the ten-tier catalog, including the curved ivory fang, compact red shard pistol, bone maul, heavy support gun, and forked violet Shadowblade. The salvage and schematic board paired raw materials with ten different electronic relic shapes and detail motifs.

The skull and ritual board used a transparent four-column grid. Its forms were a red-marked skull, a ribbed cage, a severed faceplate, a skull with a blue chain, an ash crown, a winged Fury skull, a split violet skull, a shield skull, an engine skull, and a jeweled black crown. The final objects were bound ash, a grate holding a molten ember, and a stack of triangular scrip.

The source prompts specified crisp pixel clusters, separated outlines, and dark leather, bone, iron, copper, and violet materials. All boards called for original designs with a clear silhouette at game scale.

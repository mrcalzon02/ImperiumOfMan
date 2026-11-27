# Regional equipment material integration

Scope: seven new shared material recipes; existing owned equipment and material formulas reuse the new stock and previously registered ammunition intermediates. Existing equipment item records, research gates, chip IDs, batches, hours and native/third-party recipe ownership are retained. If a new component is not registered, the consuming formula uses its prior ingredient. No live cargo or save edits.

| Rank | New stock | Inputs per order | Batch | Base hours | Principal consumers |
|---|---|---|---:|---:|---|
| III | Agga Ocular Focus | 1 Demonic Eye + 1 Essence | 2 | 5 | Scrivnus Relic Core; Bound Ash |
| III | Gannix Living Weave | 1 Venusian rags + 1 feather + 1 Gavvakh | 3 | 5 | Warp Armor, Steadfast Heart, Blood Dance |
| IV | Ron Blood Concentrate | 2 Ron blood + 1 Essence | 3 | 4 | Ron medicine, Ron Binding Weave |
| IV | Ron Binding Weave | 1 Ron tentacle + 1 concentrate | 2 | 5 | Ron Mask |
| IV | Scrivnus Relic Core | 1 Agga focus + 1 rib + 1 Essence | 2 | 6 | Scrivnus Knife |
| V | Ur Living Forge Lattice | 1 Mars gear + 1 Mars skin + 1 Gavvakh | 2 | 6 | Six Mars body replacements |
| V | Cattaram Crystal Core | 1 Cattaram stone + 1 Essence | 2 | 6 | Cattaram Mine, occult repair kit |

Additional existing stock reuse: Gannix chitin in Venusian armor plates and Bone Knife; Hardened Living Cells in Venusian pistol; Duggur Matrix in Moon Mace and Impaler; Viable Brood Substrate in Spore Mine. All substitutions keep prior ingredient counts except where the new stock itself introduces further upstream processing. No change to finished ammo yield. The existing legacy IndustryRecipe helper still applies its existing quantity rules to Bound Ash. No compulsory dependence on advanced research for earlier stock.

New stock is registered in dependency order, using the existing native donor descriptors temporarily. Each registered stock gets a physical singleton pattern through its corresponding faction. Shared Gannix precursor knowledge is additionally available through Tezctlan and XiomaraMasks; no new faction, automatic recipe unlock, or universal pirate fallback. Existing mixed patterns remain valid.

Provisional balance, original art, native compilation, actual faction access and production transactions are outstanding. The startup audit records material formulas, their consumers and pattern sources. This is source integration, not native gameplay acceptance.

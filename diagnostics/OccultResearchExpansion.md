# Occult Research expansion

Source baseline: 4a727c5dec55a8f0d2930abfa40b24699e6f9056. Source change only; no native build or gameplay receipt yet.

The existing occult module ID and its four starter perk IDs are preserved for saved games. The displayed title is **Occult Research**. The marker uses the midpoint of the actual native Engineering and Research markers, with the previously used fallback if the installed game's identifiers differ. The separate research tree uses the existing engineering camera anchor, so other ship module placements are unchanged.

| Rank | Prerequisite | Research unlocks |
| --- | --- | --- |
| I Manifestation | Gavvakh Retort | Agga components, Gannix tissue/metal, Shartamakhum glass/organics/ammo, basic native equipment, Chaos 1–2 |
| II Binding | I | Duggur materials/bolts, Ron blood and medicine, selected native equipment, Chaos 3–4 |
| III Infusion | II | Ur skin/gears/fuel, Cattaram crystals and ordnance, Flauros organics/ammo, Chaos 5–6 |
| IV Soulforge | III | Six Shedu replacement-part recipes, selected native quasi implants, Chaos 7–8 |
| V Ascension | IV | High occult repair and selected weapon manufacture, Chaos 9–10 |

New recipes use explicit installed item IDs and effective quantities. If a native or other mod recipe already exists, preserve it and log the skipped addition. Newly registered occult recipes require the purchased research perk at order start. Existing Chaos weapon, armor, utility, material and pact patterns are taught by research in pairs of tiers. Existing faction chips remain independent, earlier knowledge sources: no learned patterns are removed. Fury Sigil and Shadowblade remain excluded.

The Crafting Map states the occult prerequisite or alternate research/chip knowledge route. All new upgrade captions are short; the existing main-node sizing and crimson lock treatment applies. Five unique 36x36 RGBA icons are embedded.

## Remaining validation

Native build and headed launch are required to verify marker midpoint, graph links and icon readability, actual registered/skipped native recipes, research purchase/learning, queue consumption, save/load, and protected acquisition rules. The initial research cargo prices, grouping and quantities are tuning choices. The ranks cover existing Chaos equipment and a verified-ID subset of native otherworldly items, not every item from all nine families. Dedicated original Olirna materials and complete native family equipment coverage remain outstanding. The pre-existing occult starter and spoiled-organic recycling validation are also pending.

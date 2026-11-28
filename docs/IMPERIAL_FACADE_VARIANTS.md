# Imperial facade accent library

The approved ImageGen sheet is the source for twelve new 28x28-pixel
wall-face accents, not replacements for wall caps, standalone props or
new floor materials. Source generation ID: f55886e0-314e-4846-a3de-82871941167f.
Approved sheet: 1448x1086 RGB. Crop columns [26,351), [387,713),
[749,1075), [1111,1437); rows [25,345), [374,697), [726,1059).
Resampled with Lanczos, matched tone to the base facade's side margins,
and blended only within the original face interior. The original top
nine rows, bottom two rows, and three columns on either side remain
byte-identical to the existing 28x28 RGBA base sprite (atlas slot 192).

The committed runtime atlas is 112x84 RGBA (4 columns x 3 rows).
SHA-256: 63a85a0303132409ffef1b2dcf6c464cc357c7e156c554ed92fc5aad7481cfdb.
The full-resolution source generation remains in the approval conversation;
the runtime atlas is embedded from the existing Runtime PNG resource glob.

| Slot | Short ID | Detail |
| --- | --- | --- |
| 0 | 29040 | Cracked masonry |
| 1 | 29041 | Bolted plate |
| 2 | 29042 | Vent |
| 3 | 29043 | Recessed pipe alcove |
| 4 | 29044 | Brace |
| 5 | 29045 | Hatch |
| 6 | 29046 | Imperial relief |
| 7 | 29047 | Medical marking |
| 8 | 29048 | Recessed grille |
| 9 | 29049 | Recessed junction |
| 10 | 29050 | Archive plaque |
| 11 | 29051 | Shrine |

Each uses the original CampaignFacadeTileset renderer, a one-sprite atlas
slice, the base facade's 150 health / 15 armor and the existing
iom_facade_chipped -> iom_facade_ruined -> iom_breach_rubble chain.
New short IDs do not shift existing ones. Damage reverts to the
established plain chipped artwork; variant-specific damaged art is not
included in this batch.

On first loading a clean Imperial floor only, the optional accent
pass scans ordinary existing Imperial facade runs with a valid
wall cap and south floor. It preserves native retained wall runs,
special cells, doors, neighboring fixtures and layer-2 decals. Long
runs get an accent every 4–6 eligible tiles (3–5 plain tiles between),
with room-specific pools and a seeded non-repeating choice. Runs shorter
than three cells are untouched. Each run keeps plain end tiles. No new
objects, light emitters, terrain geometry, passability or floor blending
are introduced by this feature. Chaos and saved floors are not repainted.

## Acceptance

- Offline: run `python tools/terrain/validate_facade_atlas.py` (Pillow);
  verifies all 48 protected edge regions, dimensions and 12 unique sprites.
- Native build: compile against the installed Quasimorph game and confirm
  all 12 new IDs register without collisions; inspect the embedded PNG.
- Headed: inspect new Imperial station/procedural/campaign maps, especially
  long corridor walls and medical, forge, relay and ceremonial rooms.
  Verify the accents meet the base texture, don't cover props or doors,
  don't change geometry or minimap, and damage and save/reload preserve
  native behavior. Confirm no changes to Chaos art or existing saved maps.
- Native compilation and in-game screenshots are pending.

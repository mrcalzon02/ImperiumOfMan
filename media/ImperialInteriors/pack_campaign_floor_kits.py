"""Pack campaign floor variants from the existing 28 px deck tiles and material library."""

import json
from math import hypot, pi, sin
from pathlib import Path

from PIL import Image, ImageChops


HERE = Path(__file__).resolve().parent
CAMPAIGN = HERE.parents[1] / 'src' / 'Art' / 'CampaignDecks'
SIZE = 28
KIT_WIDTH = 80

# The runtime uses the same north/east/south/west bit order. A diagonal matters only
# if neither adjacent side belongs to the other surface.
JOIN_MASKS = [mask for mask in range(256) if all(
    not mask & (16 << corner) or not mask & ((1 << corner) | (1 << ((corner + 1) % 4)))
    for corner in range(4))]

# name, endpoints, reach, feather, seam width, and texture wear
JOINS = [
    ('Wall', 'field', 'trim', 4.7, 2.5, 1.6, 0.8),
    ('Doorway', 'field', 'lane', 9.2, 2.9, 2.1, 0.9),
    ('Material', 'field', 'secondary', 8.1, 4.9, 0.9, 1.5),
    ('Recess', 'trim', 'inset', 6.0, 2.1, 1.3, 0.7),
    ('FieldTrim', 'field', 'trim', 5.9, 2.8, 1.3, 1.0),
    ('SecondaryTrim', 'secondary', 'trim', 5.9, 2.8, 1.3, 1.0),
    ('SecondaryLane', 'secondary', 'lane', 8.6, 3.5, 1.8, 1.1),
]

# Campaign order. The four materials are the second field, traffic lane, trim, and inset.
PALETTES = [
    ('caloris_levy', 'rust_tread', 'olive_rib', 'brass_seam', 'foundry_wheel'),
    ('counterfeit_orbit', 'smoke_marble', 'telemetry', 'brass_seam', 'ashlar'),
    ('goethe_deadlock', 'olive_plate', 'loading_chevron', 'brass_seam', 'red_tread'),
    ('extinguish_crucible', 'rust_tread', 'riveted_tread', 'copper_junction', 'foundry_wheel'),
    ('discovery_injunction', 'cloister', 'ashlar', 'smoke_marble', 'telemetry'),
    ('hollow_signal', 'telemetry', 'drain_plate', 'brass_seam', 'cloister'),
    ('cinder_basin', 'olive_rib', 'drain_plate', 'rust_tread', 'rust_grate'),
    ('last_bonded_shipment', 'olive_plate', 'loading_chevron', 'red_tread', 'riveted_tread'),
    ('prime_the_foundry', 'copper_junction', 'riveted_tread', 'brass_seam', 'foundry_wheel'),
    ('one_standard', 'smoke_marble', 'ashlar', 'red_diamond', 'red_medallion'),
    ('white_frequency', 'violet_conduit', 'turbine_grid', 'blood_inlay', 'ritual_rings'),
    ('scales_of_passage', 'blood_inlay', 'smoke_marble', 'red_diamond', 'red_medallion'),
    ('no_salvage_rights', 'olive_plate', 'rust_tread', 'copper_junction', 'rust_grate'),
    ('furnace_of_ur', 'red_tread', 'riveted_tread', 'blood_inlay', 'foundry_wheel'),
    ('the_right_to_pass', 'violet_conduit', 'loading_chevron', 'brass_seam', 'turbine_grid'),
    ('prism_rent', 'jade_fault', 'violet_conduit', 'smoke_marble', 'ritual_rings'),
    ('starless_oath', 'smoke_marble', 'blood_inlay', 'red_diamond', 'ritual_rings'),
    ('hound_road', 'olive_rib', 'drain_plate', 'rust_tread', 'rust_grate'),
    ('unmake_the_seal', 'blood_inlay', 'violet_conduit', 'red_diamond', 'ritual_rings'),
    ('crown_without_sun', 'smoke_marble', 'blood_inlay', 'red_diamond', 'red_medallion'),
    ('crown_without_sun_alternate_route', 'red_tread', 'loading_chevron', 'brass_seam', 'copper_junction'),
]

IMPERIAL_PALETTE_COUNT = 10
CHAOS_ONLY_MATERIALS = {"violet_conduit", "turbine_grid", "ritual_rings", "blood_inlay"}
FIELD_OVERRIDES = {"hollow_signal": "olive_rib"}


def material_tiles():
    source = Image.open(HERE / 'imperial-floor-library-source.png').convert('RGBA')
    entries = json.loads((HERE / 'imperial-floor-library.json').read_text(encoding='utf-8'))
    block = source.width // 6
    assert source.size == (block * 6, block * 4)
    materials = {}
    for index, entry in enumerate(entries):
        x, y = index % 6 * block, index // 6 * block
        panel = source.crop((x, y, x + block, y + block)).resize((SIZE * 2, SIZE * 2), Image.Resampling.LANCZOS)
        # Map Y runs upward. Phase zero is the lower-left quarter.
        materials[entry['key']] = tuple(panel.crop((col, row, col + SIZE, row + SIZE))
                                        for row in (SIZE, 0) for col in (0, SIZE))
    return materials


def direction_mask(side, opacity):
    mask = Image.new('L', (SIZE, SIZE))
    mask.putdata([opacity((y, SIZE - 1 - x, SIZE - 1 - y, x)[side])
                  for y in range(SIZE) for x in range(SIZE)])
    return mask


def perimeter(field, trim, sides):
    mask = Image.new('L', (SIZE, SIZE))
    for side in range(4):
        if sides & (1 << side):
            edge = direction_mask(side, lambda distance: 255 if distance < 6 else 144 if distance == 6 else 0)
            mask = ImageChops.lighter(mask, edge)
    return Image.composite(trim, field, mask)


def transition(inside, outside, trim, side, width, band):
    material = direction_mask(side, lambda distance: 255 if distance < width else 110 if distance == width else 0)
    joint = direction_mask(side, lambda distance: 235 if width <= distance < width + band else 0)
    return Image.composite(trim, Image.composite(outside, inside, material), joint)


def normalized(value):
    return max(0.0, min(1.0, value))


def falloff(value):
    value = normalized(value)
    return value * value * (3.0 - 2.0 * value)


def join_masks(mask, phase, reach, feather, seam, wear):
    material, bevel, light, shadow = [], [], [], []
    for y in range(SIZE):
        for x in range(SIZE):
            gx = x + (phase & 1) * SIZE
            gy = SIZE - 1 - y + (phase >> 1) * SIZE
            # The two-cell period matches the floor sheets. Adjacent tiles share the same
            # rough contour and only the wall/room direction changes the contour's side.
            wave_x = sin(gx * pi / 14) + 0.42 * sin(gx * pi / 4 + 0.8)
            wave_y = sin(gy * pi / 14 + 1.3) + 0.42 * sin(gy * pi / 4)
            weather = wear * (0.54 * sin((gx + gy) * pi / 7.0) + 0.4 * sin((gx - gy) * pi / 4.0))
            d = (y - wear * wave_x, SIZE - 1 - x - wear * wave_y,
                 SIZE - 1 - y - wear * wave_x, x - wear * wave_y)
            distances = [d[side] for side in range(4) if mask & (1 << side)]
            distances += [hypot(d[corner], d[(corner + 1) % 4]) + 1.1 for corner in range(4) if mask & (16 << corner)]
            distances.sort()
            distance = distances[0] if distances else SIZE * 2
            if len(distances) > 1:
                gap = max(4 - (distances[1] - distance), 0)
                distance -= gap * gap / 16
            offset = distance + weather - reach
            material.append(round(255 * falloff((feather - offset) / (2 * feather))))
            bevel.append(round(188 * falloff((seam + 0.8 - abs(offset)) / (seam + 0.8))))
            light.append(round(105 * falloff((1.8 - abs(offset - seam * 0.7)) / 1.8)))
            shadow.append(round(208 * falloff((2.6 - abs(offset + seam * 0.85)) / 2.6)))
    masks = []
    for values in (material, bevel, light, shadow):
        image = Image.new('L', (SIZE, SIZE))
        image.putdata(values)
        masks.append(image)
    return masks


def build_directional(material_rows):
    assert len(JOIN_MASKS) == 47
    for name, base, boundary, reach, feather, seam, wear in JOINS:
        columns = len(JOIN_MASKS)
        output = Image.new('RGBA', (columns * SIZE, len(material_rows) * 4 * SIZE))
        for phase in range(4):
            profiles = [join_masks(mask, phase, reach, feather, seam, wear) for mask in JOIN_MASKS]
            for row, materials in enumerate(material_rows):
                inside, outside = materials[base][phase], materials[boundary][phase]
                trim = materials['trim'][phase]
                # Bias the exposed sill toward the same material as its neighboring edge.
                # Multiply light and shadow into the grain so the relief isn't a painted outline.
                rim = Image.blend(inside, trim, 0.52 if name in ('Doorway', 'SecondaryLane') else 0.18)
                if name == 'Wall':
                    outside = Image.blend(outside, Image.new('RGBA', outside.size, '#17161a'), 0.38)
                for column, (material, bevel, light, shadow) in enumerate(profiles):
                    mixed = Image.composite(outside, inside, material)
                    if name != 'Material':
                        mixed = Image.composite(rim, mixed, bevel)
                    bright = Image.blend(mixed, Image.new('RGBA', mixed.size, '#dec6a0'), 0.27)
                    dark = Image.blend(mixed, Image.new('RGBA', mixed.size, '#15121c'), 0.34)
                    mixed = Image.composite(bright, mixed, light)
                    mixed = Image.composite(dark, mixed, shadow)
                    output.paste(mixed, (column * SIZE, (row * 4 + phase) * SIZE))
        output.save(CAMPAIGN / f'CampaignJoin{name}.png')


def build():
    materials = material_tiles()
    field_sheet = Image.open(CAMPAIGN / 'CampaignDeckTerrain.png').convert('RGBA')
    game_layout = (CAMPAIGN.parents[1] / 'Plugin.CampaignDecks.cs').read_text(encoding='utf-8')
    route_positions = [game_layout.index(f'new("{name}",') for name, *_ in PALETTES]
    assert route_positions == sorted(route_positions), 'The atlas rows must match the runtime campaign order.'
    for name, *roles in PALETTES[:IMPERIAL_PALETTE_COUNT]:
        assert not CHAOS_ONLY_MATERIALS.intersection([*roles, FIELD_OVERRIDES.get(name)]), (
            f'Imperial floor palette {name} uses a Chaos-only material.'
        )
    output = Image.new('RGBA', (KIT_WIDTH * SIZE, len(PALETTES) * SIZE))
    material_rows = []
    for row, (name, second_name, lane_name, trim_name, inset_name) in enumerate(PALETTES):
        x, y = row % 7 * SIZE, row // 7 * SIZE
        field = field_sheet.crop((x, y, x + SIZE, y + SIZE))
        if name in FIELD_OVERRIDES:
            field = materials[FIELD_OVERRIDES[name]][0]
        secondary, lane, trim, inset = (materials[name] for name in (second_name, lane_name, trim_name, inset_name))
        material_rows.append(dict(field=(field,) * 4, secondary=secondary, lane=lane, trim=trim, inset=inset))
        tiles = [field, *secondary, *lane, *trim, *inset]
        tiles += [perimeter(field, trim[0], mask) for mask in range(1, 16)]
        tiles += [transition(field, lane[phase], trim[phase], side, 12, 3) for side in range(4) for phase in range(4)]
        tiles += [transition(field, secondary[phase], trim[phase], side, 8, 2) for side in range(4) for phase in range(4)]
        tiles += [transition(trim[phase], inset[phase], field, side, 6, 2) for side in range(4) for phase in range(4)]
        assert len(tiles) == KIT_WIDTH
        assert all(tile.getchannel('A').getextrema() == (255, 255) for tile in tiles)
        for column, tile in enumerate(tiles):
            output.paste(tile, (column * SIZE, row * SIZE))
    output.save(CAMPAIGN / 'CampaignFloorKits.png', optimize=True)
    build_directional(material_rows)


if __name__ == '__main__':
    build()

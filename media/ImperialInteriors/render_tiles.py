"""Slice the Imperial master sheets and render the same room plans with both finishes."""

import random
from collections import deque
from pathlib import Path

from PIL import Image, ImageChops, ImageDraw, ImageEnhance, ImageFilter, ImageFont, ImageStat

from wall_tiles import NORTH, SLOT_FOR_MASK, SOUTH, WEST, pier_family, wall_atlas, wall_family, wall_index

HERE = Path(__file__).resolve().parent
CELL = 28
SIZE = (CELL, CELL)
RESAMPLE = Image.Resampling.LANCZOS
WALLS = {"#", "%", "!", "@"}
WALL_FIXTURES = {"!": "wall_lamp", "@": "wall_relief"}
FLOORS = {
    "_": "chapel_paver", "x": "slate", "g": "grate", ",": "service_channel",
    "+": "gate_ns", "|": "gate_ew", "~": "hazard",
}
PROPS = {
    "p": "barrier_ew", "P": "barrier_ns", "c": "crate", "b": "bunk", "k": "locker",
    "t": "cogitator", "A": "altar", "h": "bench", "f": "brazier", "R": "reliquary",
}
OPEN = set(FLOORS) | set(".:=s-")
SYMBOLS = OPEN | WALLS | PROPS.keys() | {" "}
CARDINALS = [(0, -1), (-1, 0), (1, 0), (0, 1)]

MATERIALS = [
    "flagstone_0", "flagstone_1", "flagstone_2", "flagstone_3",
    "deck_0", "deck_1", "chapel_paver", "slate",
    "runner_ns", "grate", "service_channel", "gate_ns",
    "hazard", "wall_ornate", None, None,
]
FLOOR_VARIANTS = {
    "flagstone": tuple(f"flagstone_{index}" for index in range(6)),
    "deck": tuple(f"deck_{index}" for index in range(4)),
    "chapel_paver": ("chapel_paver", "chapel_paver_1", "chapel_paver_2"),
    "slate": ("slate", "slate_1", "slate_2"),
}
EXTRA_FLOOR_ROWS = [
    ("flagstone_4", "flagstone_5", "deck_2", "deck_3"),
    ("chapel_paver_1", "chapel_paver_2", "slate_1", "slate_2"),
]
EXTRA_FLOOR_REFERENCES = ("flagstone_0", "flagstone_0", "deck_0", "deck_0", "chapel_paver", "chapel_paver", "slate", "slate")
# Sprite sizes are the longest dimension in a 28-pixel cell.
FURNISHINGS = [
    ("inlay", 23), ("wall_lamp", 17), ("wall_relief", 23), ("barrier_ew", 26),
    ("crate", 23), ("bunk", 25), ("locker", 22), ("cogitator", 24),
    ("altar", 25), ("bench", 25), ("brazier", 24), ("reliquary", 26),
]
ATLAS_ROWS = [
    MATERIALS[:8],
    ["runner_ns", "runner_ew", "inlay", "grate", "service_channel", "gate_ns", "gate_ew", "hazard"],
    ["wall_cap", "wall_face", "pier_cap", "pier_end", "wall_lamp", "wall_relief", "barrier_ns", "barrier_ew"],
    [name for name, _ in FURNISHINGS[4:]],
    ["wall_ornate", "runner_ns_fill", "runner_ns_left", "runner_ns_right",
     "runner_ew_fill", "runner_ew_top", "runner_ew_bottom", "wall_edge"],
]
GROWTH_SPRITES = ["wall_collar", "fault", "bone_ring", "root_bundle", "mineral_island", "web"]
GROWTH_SIZE = 5 * CELL
ROTATIONS = {90: Image.Transpose.ROTATE_90, 180: Image.Transpose.ROTATE_180, 270: Image.Transpose.ROTATE_270}


def sheet_cells(image, columns, rows):
    x = [round(col * image.width / columns) for col in range(columns + 1)]
    y = [round(row * image.height / rows) for row in range(rows + 1)]
    return [
        image.crop((x[col], y[row], x[col + 1], y[row + 1]))
        for row in range(rows) for col in range(columns)
    ]


def load_paired_art(filename, columns, rows_per_finish):
    with Image.open(HERE / filename) as sheet:
        cells = sheet_cells(sheet.convert("RGBA"), columns, rows_per_finish * 2)
    tiles = [cell.resize(SIZE, RESAMPLE) for cell in cells]
    middle = columns * rows_per_finish
    clean, deep = tiles[:middle], tiles[middle:]
    faded = [Image.blend(base, damaged, 0.30) for base, damaged in zip(clean, deep, strict=True)]
    return clean, faded, deep


def crop_sprite(cell):
    # Preserve real alpha while ignoring faint source noise when measuring the silhouette.
    silhouette = cell.getchannel("A").point(lambda value: 255 if value >= 32 else 0)
    bounds = silhouette.getbbox()
    if bounds is None:
        raise ValueError("Empty sprite cell")
    return cell.crop(bounds)


def sprite(cell, longest):
    art = crop_sprite(cell)
    art.thumbnail((longest, longest), RESAMPLE)
    tile = Image.new("RGBA", SIZE)
    tile.alpha_composite(art, ((CELL - art.width) // 2, (CELL - art.height) // 2))
    return tile


def load_furnishings():
    with Image.open(HERE / "imperial-furnishings-source.png") as sheet:
        cells = sheet_cells(sheet.convert("RGBA"), 4, 3)
    return {name: sprite(cell, longest) for (name, longest), cell in zip(FURNISHINGS, cells, strict=True)}


def load_doors():
    with Image.open(HERE / "imperial-doors-ns-source.png") as sheet:
        horizontal = sheet_cells(sheet.convert("RGBA"), 2, 2)[:2]
    with Image.open(HERE / "imperial-door-side-source.png") as sheet:
        vertical = sheet_cells(sheet.convert("RGBA"), 2, 2)
    parts = []
    dimensions = [(18, 7, 0), (18, 6, 22), (6, 21, 4), (2, 22, 3)]
    for cell, (width, height, top) in zip(vertical, dimensions, strict=True):
        part = Image.new("RGBA", SIZE)
        part.alpha_composite(crop_sprite(cell).resize((width, height), RESAMPLE), ((CELL - width) // 2, top))
        parts.append(part)

    top_cap, bottom_housing, blade, guide = parts
    open_side, closed_side = guide.copy(), guide.copy()
    blade = ImageEnhance.Brightness(blade).enhance(1.27)
    parts[2] = blade
    closed_side.alpha_composite(blade)
    closed_side.putpixel((13, 14), (190, 112, 42, 255))
    for side in (open_side, closed_side):
        side.alpha_composite(top_cap)
        side.alpha_composite(bottom_housing)
    return [sprite(cell, CELL) for cell in horizontal] + [open_side, closed_side], parts


def door_threshold(floor, door):
    threshold = ImageEnhance.Brightness(floor).enhance(0.94)
    threshold.alpha_composite(door)
    return threshold


def trim_runner(tiles, source):
    width, height = source.size
    border = round(width * 0.107)
    center = source.crop((border, 0, width - border, height)).resize(SIZE, RESAMPLE)
    for kind, keep_left, keep_right in [("fill", False, False), ("left", True, False), ("right", False, True)]:
        tile = center.copy()
        if keep_left:
            tile.paste(tiles["runner_ns"].crop((0, 0, 3, CELL)), (0, 0))
        if keep_right:
            tile.paste(tiles["runner_ns"].crop((CELL - 3, 0, CELL, CELL)), (CELL - 3, 0))
        tiles[f"runner_ns_{kind}"] = tile
        horizontal = {"fill": "fill", "left": "bottom", "right": "top"}[kind]
        tiles[f"runner_ew_{horizontal}"] = tile.transpose(Image.Transpose.ROTATE_90)


def make_tiles(source_file, furnishings, doors, floor_art, structural_art):
    with Image.open(source_file) as sheet:
        sources = {name: cell for name, cell in zip(MATERIALS, sheet_cells(sheet.convert("RGBA"), 4, 4), strict=True)
                   if name is not None}
    tiles = {name: art.resize(SIZE, RESAMPLE) for name, art in sources.items()}
    # Keep floor detail below the value range of carved wall crowns and interactive props.
    for name in MATERIALS[:8]:
        brightness = 0.72 if name == "chapel_paver" else 0.94
        contrast = 0.82 if name.startswith("flagstone_") else 1.0
        art = ImageEnhance.Contrast(tiles[name]).enhance(contrast)
        tiles[name] = ImageEnhance.Brightness(art).enhance(brightness)

    extra_names = (name for row in EXTRA_FLOOR_ROWS for name in row)
    for name, reference, source in zip(extra_names, EXTRA_FLOOR_REFERENCES, floor_art, strict=True):
        target = ImageStat.Stat(tiles[reference].convert("L")).mean[0]
        value = ImageStat.Stat(source.convert("L")).mean[0]
        tiles[name] = ImageEnhance.Brightness(source).enhance(target / value)

    tiles["runner_ew"] = tiles["runner_ns"].transpose(Image.Transpose.ROTATE_90)
    for name, door in zip(("gate_ns", "gate_ew"), (doors[0], doors[2]), strict=True):
        tiles[name] = door_threshold(tiles["slate"], door)
    trim_runner(tiles, sources["runner_ns"])
    tiles.update(furnishings)
    tiles["barrier_ns"] = tiles["barrier_ew"].transpose(Image.Transpose.ROTATE_90)
    crown, face, pier_top = structural_art
    walls = wall_family(crown, face)
    piers = pier_family(walls, pier_top)
    tiles["wall_cap"] = crown
    tiles["wall_face"] = face
    tiles["wall_edge"] = walls[SLOT_FOR_MASK[NORTH | SOUTH]]
    tiles["pier_cap"] = piers[SLOT_FOR_MASK[0]]
    tiles["pier_end"] = piers[SLOT_FOR_MASK[WEST]]
    for label, family in [("wall", walls), ("pier", piers)]:
        tiles.update((f"{label}_{index}", wall) for index, wall in enumerate(family))
    return tiles


def validate_room(room):
    name = room["name"]
    rows = room["grid"]
    width, height = len(rows[0]), len(rows)
    if any(len(row) != width for row in rows):
        raise ValueError(f"{name}: row widths must match")
    walkable, doors = set(), set()
    for y, row in enumerate(rows):
        for x, symbol in enumerate(row):
            if symbol not in SYMBOLS:
                raise ValueError(f"{name}: unknown symbol {symbol!r} at {(x, y)}")
            if symbol in OPEN:
                walkable.add((x, y))
            if symbol == "+" and y in (0, height - 1) and 0 < x < width - 1:
                doors.add((x, y))
            elif symbol == "|" and x in (0, width - 1) and 0 < y < height - 1:
                doors.add((x, y))
            elif symbol in "+|" or ((x in (0, width - 1) or y in (0, height - 1)) and symbol not in WALLS | {" "}):
                raise ValueError(f"{name}: invalid boundary or connector at {(x, y)}")
            if symbol in OPEN - {"+", "|"} and any(rows[y + dy][x + dx] == " " for dx, dy in CARDINALS):
                raise ValueError(f"{name}: floor at {(x, y)} opens directly to the void")
    if not doors:
        raise ValueError(f"{name}: room needs a connector")
    visited = {min(doors)}
    pending = deque(visited)
    while pending:
        x, y = pending.popleft()
        for dx, dy in CARDINALS:
            neighbor = (x + dx, y + dy)
            if neighbor in walkable and neighbor not in visited:
                visited.add(neighbor)
                pending.append(neighbor)
    if visited != walkable:
        raise ValueError(f"{name}: unreachable floor cells: {sorted(walkable - visited)}")
    for anchor, position in room["anchors"].items():
        if tuple(position) not in walkable - doors:
            raise ValueError(f"{name}: {anchor} needs an interior walkable cell")


def floor_choice(room, x, y):
    rows = room["grid"]
    symbol = rows[y][x]
    runner = {"=": "runner_ns", "-": "runner_ew", "s": room["inlay_floor"]}
    floor = runner.get(symbol)
    if floor == "runner_ns":
        neighbors = rows[y]
        before = x > 0 and runner.get(neighbors[x - 1]) == floor
        after = x + 1 < len(neighbors) and runner.get(neighbors[x + 1]) == floor
        edges = {(True, True): "_fill", (False, True): "_left", (True, False): "_right"}
        return floor + edges.get((before, after), "")
    if floor == "runner_ew":
        before = y > 0 and runner.get(rows[y - 1][x]) == floor
        after = y + 1 < len(rows) and runner.get(rows[y + 1][x]) == floor
        edges = {(True, True): "_fill", (False, True): "_top", (True, False): "_bottom"}
        return floor + edges.get((before, after), "")
    surface = floor or FLOORS.get(symbol)
    if surface is None:
        if symbol == ":":
            surface = "deck"
        elif symbol in PROPS:
            surface = room.get("prop_floor", room["base_floor"])
        else:
            surface = room["base_floor"]
    variants = FLOOR_VARIANTS.get(surface)
    return random.Random(y * 65536 + x).choice(variants) if variants else surface


def load_growth():
    with Image.open(HERE / "imperial-corrupted-v2-growth-source.png") as sheet:
        cells = sheet_cells(sheet.convert("RGBA"), 3, 2)
    growth = {}
    for name, cell in zip(GROWTH_SPRITES, cells, strict=True):
        # Remove the faint alpha haze around the generated sprites before export.
        cell.putalpha(cell.getchannel("A").point(lambda value: max(0, (value - 17) * 255 / 237)))
        growth[name] = cell.resize((GROWTH_SIZE, GROWTH_SIZE), RESAMPLE)
    return growth


def growth_layer(room, sprites, placements):
    rows = room["grid"]
    width, height = len(rows[0]), len(rows)
    layer = Image.new("RGBA", (width * CELL, height * CELL))
    for placement in placements:
        x, y = placement["cell"]
        size = placement["size"]
        radius = size // 2
        if (size not in (3, 5) or not radius <= x < width - radius
                or not radius <= y < height - radius):
            raise ValueError(f"{room['name']}: growth placement outside the room: {placement}")
        art = sprites[placement["sprite"]].resize((size * CELL, size * CELL), RESAMPLE)
        if placement["rotation"]:
            art = art.transpose(ROTATIONS[placement["rotation"]])
        layer.alpha_composite(art, ((x - radius) * CELL, (y - radius) * CELL))

    mask = Image.new("L", layer.size, 255)
    draw = ImageDraw.Draw(mask)
    protected = {tuple(cell) for cell in room["anchors"].values()}
    for y, row in enumerate(rows):
        for x, symbol in enumerate(row):
            if symbol in " +|~s" or (x, y) in protected:
                draw.rectangle((x * CELL, y * CELL, (x + 1) * CELL - 1, (y + 1) * CELL - 1), fill=0)
    alpha = ImageChops.multiply(layer.getchannel("A"), mask)
    layer.putalpha(alpha)
    strength = ImageChops.multiply(alpha.filter(ImageFilter.GaussianBlur(CELL * 0.62)), mask)
    return layer, strength.resize((width, height), Image.Resampling.BOX)


def composite_surface(image, position, name, tiles, deep_tiles, growth):
    surface = tiles[name]
    if growth is not None:
        x, y = position
        layer, strength = growth
        coverage = strength.getpixel((x // CELL, y // CELL))
        if coverage > 5:
            surface = Image.blend(surface, deep_tiles[name], min(1.0, (coverage - 5) / 28))
        image.alpha_composite(surface, position)
        image.alpha_composite(layer.crop((x, y, x + CELL, y + CELL)), position)
        return
    image.alpha_composite(surface, position)


def render_room(room, tiles, *, deep_tiles=None, growth=None, surface_for=floor_choice):
    rows = room["grid"]
    width, height = len(rows[0]), len(rows)
    image = Image.new("RGBA", (width * CELL, height * CELL), "#161a1d")
    for y, row in enumerate(rows):
        for x, symbol in enumerate(row):
            pos = (x * CELL, y * CELL)
            if symbol == " ":
                continue
            if symbol in WALLS:
                family = "pier" if symbol == "%" else "wall"
                wall = f"{family}_{wall_index(rows, x, y, WALLS)}"
                composite_surface(image, pos, wall, tiles, deep_tiles, growth)
                if symbol in WALL_FIXTURES:
                    image.alpha_composite(tiles[WALL_FIXTURES[symbol]], pos)
                continue

            composite_surface(image, pos, surface_for(room, x, y), tiles, deep_tiles, growth)
            if y > 0 and rows[y - 1][x] in WALLS:
                for depth, alpha in enumerate((106, 60, 31, 14)):
                    image.alpha_composite(Image.new("RGBA", (CELL, 1), (14, 13, 16, alpha)),
                                          (pos[0], pos[1] + depth))
            overlay = "inlay" if symbol == "s" else PROPS.get(symbol)
            if overlay:
                prop = tiles[overlay]
                if symbol in PROPS:
                    shade = Image.new("RGBA", SIZE, (8, 7, 8))
                    shade.putalpha(prop.getchannel("A").filter(ImageFilter.GaussianBlur(1))
                                   .point(lambda value: value * 0.63))
                    image.alpha_composite(shade, (pos[0] + 1, pos[1] + 2))
                image.alpha_composite(prop, pos)
    return image


def render_atlas(tiles, rows=ATLAS_ROWS):
    atlas = Image.new("RGBA", (len(rows[0]) * CELL, len(rows) * CELL))
    for y, row in enumerate(rows):
        for x, name in enumerate(row):
            atlas.alpha_composite(tiles[name], (x * CELL, y * CELL))
    return atlas


def render_door_atlas(doors, columns):
    atlas = Image.new("RGBA", (columns * CELL, (len(doors) // columns) * CELL))
    for index, door in enumerate(doors):
        atlas.alpha_composite(door, ((index % columns) * CELL, (index // columns) * CELL))
    return atlas


def render_door_preview(tiles, doors):
    image = Image.new("RGBA", (808, 740), "#161a1d")
    draw = ImageDraw.Draw(image)
    draw.text((24, 18), "Imperial bulkheads", font=ImageFont.load_default(30), fill="#eee7d9")
    draw.text((24, 56), "Doors shown at twice tile size in their wall runs", font=ImageFont.load_default(15), fill="#b9aa95")
    scenarios = [
        ("North to south", "gate_ns", [".....", ".....", "##+##", ".....", "....."]),
        ("East to west", "gate_ew", ["..#..", "..#..", "..|..", "..#..", "..#.."]),
    ]
    for axis, (label, tile, rows) in enumerate(scenarios):
        plan = {"grid": rows, "inlay_floor": None, "base_floor": "flagstone", "anchors": {}}
        for state, state_label in enumerate(("Open", "Closed")):
            x, y = 24 + state * 388, 90 + axis * 322
            draw.rectangle((x, y, x + 371, y + 306), fill="#1e2327", outline="#36383b")
            art = door_threshold(tiles["slate"], doors[axis * 2 + state])
            room = render_room(plan, tiles | {tile: art})
            image.alpha_composite(room.resize((280, 280), Image.Resampling.NEAREST), (x + 46, y + 9))
            title = f"{label}  /  {state_label}"
            draw.text((x + 186, y + 296), title, anchor="mm", font=ImageFont.load_default(16), fill="#eee7d9")
    return image.convert("RGB")


def render_pillar_preview(clean, corrupted):
    image = Image.new("RGBA", (970, 475), "#161a1d")
    draw = ImageDraw.Draw(image)
    draw.text((24, 18), "Imperial pillar tops", font=ImageFont.load_default(30), fill="#eee7d9")
    draw.text((24, 57), "Top-down views at twice tile size", font=ImageFont.load_default(15), fill="#b9aa95")
    samples = [
        ("Freestanding", [".....", "..%..", "....."]),
        ("Horizontal wall end", [".....", "##%..", "....."]),
        ("Vertical wall end", ["..#..", "..%..", "....."]),
    ]
    for row, (finish, tiles) in enumerate((("Imperial", clean), ("Warp-corrupted", corrupted))):
        for col, (kind, grid) in enumerate(samples):
            x, y = 24 + col * 310, 85 + row * 182
            draw.rectangle((x, y, x + 300, y + 176), fill="#1e2327", outline="#36383b")
            plan = {"grid": grid, "inlay_floor": None, "base_floor": "flagstone"}
            room = render_room(plan, tiles)
            image.alpha_composite(room.resize((280, 168), Image.Resampling.NEAREST).crop((0, 28, 280, 140)), (x + 10, y + 16))
            draw.text((x + 150, y + 147), f"{kind} / {finish}", anchor="mm", font=ImageFont.load_default(16), fill="#eee7d9")
    return image.convert("RGB")


def render_preview(rooms, draw_room, finish, *, title="Golden Throne outer precinct"):
    art_height = max(308, max(len(room["grid"]) for room in rooms) * CELL)
    extra_height = art_height - 308
    card_width = max(518, max(len(room["grid"][0]) for room in rooms) * CELL + 36)
    card_height, gap, margin = 405 + extra_height, 12, 28
    grid_y = 112
    width = 2 * margin + 3 * card_width + 2 * gap
    row_count = (len(rooms) + 2) // 3
    height = grid_y + row_count * card_height + (row_count - 1) * gap + margin
    image = Image.new("RGBA", (width, height), "#161a1d")
    draw = ImageDraw.Draw(image)
    draw.rectangle((margin, 24, margin + 37, 27), fill="#b99a68")
    draw.text((margin, 37), title, font=ImageFont.load_default(36), fill="#eee7d9")
    draw.text((margin, 83), f"{finish}  /  28 px native tile grid",
              font=ImageFont.load_default(16), fill="#b9aa95")
    for index, room in enumerate(rooms):
        x = margin + (index % 3) * (card_width + gap)
        y = grid_y + (index // 3) * (card_height + gap)
        render = draw_room(room)
        draw.rectangle((x, y, x + card_width - 1, y + card_height - 1),
                       fill="#1e2327", outline="#36383b")
        draw.rectangle((x + 12, y + 12, x + card_width - 13, y + 331 + extra_height), fill="#161a1d")
        image.alpha_composite(render, (x + (card_width - render.width) // 2, y + 19 + (art_height - render.height) // 2))
        label_y = y + extra_height
        draw.line((x + 15, label_y + 345, x + card_width - 15, label_y + 345), fill="#46433e")
        draw.text((x + 15, label_y + 354), room["name"], font=ImageFont.load_default(22), fill="#eee7d9")
        draw.text((x + 15, label_y + 382), room["role"], font=ImageFont.load_default(14), fill="#baad99")
    return image.convert("RGB")


def main():
    furnishings = load_furnishings()
    doors, door_parts = load_doors()
    variant_floors = load_paired_art("imperial-floor-variants-source.png", 4, 2)
    structures = load_paired_art("imperial-structural-materials-source.png", 3, 1)
    render_door_atlas(doors, 2).save(HERE / "imperial-door-states.png")
    render_door_atlas(door_parts, 4).save(HERE / "imperial-door-side-components.png")
    clean, corrupted, deep = [
        make_tiles(HERE / f"imperial{suffix}-materials-source.png", furnishings, doors, floor_art, structural_art)
        for suffix, floor_art, structural_art in zip(("", "-corrupted", "-corrupted-v2"), variant_floors, structures, strict=True)
    ]
    render_door_preview(clean, doors).save(HERE / "imperial-door-preview.png")
    render_pillar_preview(clean, deep).save(HERE / "imperial-pillar-preview.png")
    for suffix, tiles in [("", clean), ("-corrupted", corrupted)]:
        render_atlas(tiles).save(HERE / f"imperial{suffix}-tiles.png")
        render_atlas(tiles, EXTRA_FLOOR_ROWS).save(HERE / f"imperial{suffix}-floor-variants.png")

    render_atlas(deep).save(HERE / "imperial-corrupted-v2-tiles.png")
    render_atlas(deep, EXTRA_FLOOR_ROWS).save(HERE / "imperial-corrupted-v2-floor-variants.png")
    for prefix, tiles in [("imperial", clean), ("imperial-corrupted-v2", deep)]:
        for family in ("wall", "pier"):
            wall_atlas([tiles[f"{family}_{index}"] for index in range(48)]).save(HERE / f"{prefix}-{family}-tiles.png")
    sprites = load_growth()
    growth_atlas = Image.new("RGBA", (3 * GROWTH_SIZE, 2 * GROWTH_SIZE))
    for index, name in enumerate(GROWTH_SPRITES):
        growth_atlas.alpha_composite(sprites[name], ((index % 3) * GROWTH_SIZE, (index // 3) * GROWTH_SIZE))
    growth_atlas.save(HERE / "imperial-corrupted-v2-growth.png")


if __name__ == "__main__":
    main()

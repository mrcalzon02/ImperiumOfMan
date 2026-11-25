"""Assemble generated floor materials into room-scale patterns."""

import json

from PIL import Image, ImageDraw, ImageEnhance, ImageFont, ImageOps

import render_tiles as art

VARIATION = (.91, .97, 1.0, 1.055)


def load_materials():
    catalog = json.loads((art.HERE / "imperial-floor-library.json").read_text(encoding="utf-8"))
    with Image.open(art.HERE / "imperial-floor-library-source.png") as source:
        cells = art.sheet_cells(source.convert("RGBA"), 6, 4)
    materials = {}
    for record, source in zip(catalog, cells[:len(catalog)], strict=True):
        period = record["period"]
        source = ImageEnhance.Brightness(source).enhance(record["brightness"])
        materials[record["key"]] = (
            record, art.sheet_cells(source.resize((period * art.CELL, period * art.CELL), art.RESAMPLE), period, period)
        )
    return materials


def floor_tiles(materials, warp=False, deep=False):
    tiles = {}
    for key, (_, pattern) in materials.items():
        for index, tile in enumerate(pattern):
            if warp:
                bruise = ImageOps.colorize(ImageOps.grayscale(tile), "#15121b", "#d5b1ad").convert("RGBA")
                tile = Image.blend(tile, bruise, .54 if deep else .31)
                if deep:
                    tile = ImageEnhance.Contrast(tile).enhance(1.26)
            for variant, brightness in enumerate(VARIATION):
                name = (key, index, variant)
                tiles[name] = ImageEnhance.Brightness(tile).enhance(brightness)
    return tiles


def surface_picker(room, materials):
    regions = {}
    base = room.get("floor_material")
    grid = room["grid"]
    replaceable = set(".:s") | art.PROPS.keys()
    for y, row in enumerate(grid):
        for x, symbol in enumerate(row):
            if base and symbol in replaceable:
                regions[x, y] = (base, 0, 0)
    for zone in room.get("floor_zones", []):
        left, top, right, bottom = zone["rect"]
        key = zone["material"]
        if not (0 <= left <= right < len(grid[0]) and 0 <= top <= bottom < len(grid)):
            raise ValueError(f"{room['name']}: floor zone out of bounds: {zone}")
        for y in range(top, bottom + 1):
            for x in range(left, right + 1):
                if grid[y][x] in replaceable:
                    regions[x, y] = (key, left, top)
    name_seed = sum((i + 1) * ord(char) for i, char in enumerate(room["name"]))

    def choose(plan, x, y):
        if (x, y) not in regions:
            return art.floor_choice(plan, x, y)
        key, left, top = regions[x, y]
        period = materials[key][0]["period"]
        pattern_x, pattern_y = (x - left) % period, (y - top) % period
        block_x, block_y = (x - left) // period, (y - top) // period
        phase = (block_x * 1597334677 + block_y * 3812015801 + name_seed * 2654435761) & 0xffffffff
        phase = ((phase ^ (phase >> 15)) * 2246822519) & 0xffffffff
        variant = (0, 1, 1, 2, 2, 2, 3, 3)[phase >> 29]
        return key, pattern_y * period + pattern_x, variant

    return choose


def room_terrain(room, materials, deep_tiles=None, growth_sprites=None):
    terrain = {"surface_for": surface_picker(room, materials)}
    if growth_sprites and room.get("growth"):
        service = {f"service_{i}": fitting["service"] for i, fitting in enumerate(room.get("machinery", ()))}
        protected = room | {"anchors": room["anchors"] | service}
        terrain["growth"] = art.growth_layer(protected, growth_sprites, room["growth"])
        terrain["deep_tiles"] = deep_tiles
    return terrain


def material_preview(materials, tiles):
    width, height = 183, 106
    columns = 6
    rows = (len(materials) + columns - 1) // columns
    panel = Image.new("RGBA", (columns * width, rows * height), "#161a1d")
    draw = ImageDraw.Draw(panel)
    font = ImageFont.load_default(13)
    for index, (key, (record, _)) in enumerate(materials.items()):
        x, y = (index % columns) * width, (index // columns) * height
        draw.rectangle((x + 4, y + 3, x + width - 4, y + height - 3), fill="#212528", outline="#34383a")
        period = record["period"]
        for row in range(2):
            for col in range(4):
                panel.alpha_composite(tiles[key, row % period * period + col % period, 2],
                                      (x + 35 + col * art.CELL, y + 11 + row * art.CELL))
        label = record["name"]
        draw.text((x + width // 2, y + 83), label, font=font, fill="#e5d9c8", anchor="mm")
    return panel.convert("RGB")

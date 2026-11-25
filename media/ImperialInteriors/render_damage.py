"""Export Imperial damage atlases and a native-scale state comparison."""

from PIL import Image, ImageChops, ImageDraw, ImageEnhance, ImageFont

import render_tiles as art
from wall_tiles import pier_family, wall_family

RUNTIME = art.HERE / "Runtime"
FLOOR_DECALS = {"flagstone": (4, 5), "deck": (6, 7), "chapel_paver": (8, 9), "slate": (10, 11)}


def load_decals():
    with Image.open(art.HERE / "imperial-damage-decals-source.png") as sheet:
        # The floor chips and flattened rubble serve as overlays. The structural
        # crown and pier chips come from the projection-matched texture sheet.
        return [art.sprite(cell, 28 if index >= 4 else 25)
                for index, cell in enumerate(art.sheet_cells(sheet.convert("RGBA"), 4, 3))]


def weather_tile(base, decal, strength, index=0):
    result = base.copy()
    layer = decal.transpose(Image.Transpose.FLIP_LEFT_RIGHT) if index % 2 else decal.copy()
    layer.putalpha(layer.getchannel("A").point(lambda alpha: round(alpha * strength)))
    result.alpha_composite(layer)
    return result


def terrain_art(intact, damaged_structure, decals, warp):
    crown, face, pillar = damaged_structure
    chipped_wall = wall_family(crown, face)
    chipped_pillar = pier_family(chipped_wall, pillar)
    ruined_facade = weather_tile(ImageEnhance.Brightness(face).enhance(0.79), decals[2], 0.85)
    rubble_floor = ImageEnhance.Brightness(intact["flagstone_1"]).enhance(0.79)
    rubble = weather_tile(rubble_floor, decals[2], 0.90)
    if warp:
        ruined_facade = Image.blend(ruined_facade, face, 0.22)
        rubble = Image.blend(rubble, rubble_floor, 0.22)

    sheets = []
    rows = []
    for key, chipped, sample in [("wall", chipped_wall, 25), ("pier", chipped_pillar, 10)]:
        original = [intact[f"{key}_{slot}"] for slot in range(48)]
        sheets.extend(original + chipped)
        rows.append(("Wall run" if key == "wall" else "Pillar top", [original[sample], chipped[sample], rubble]))
    sheets.extend((intact["wall_face"], face, ruined_facade, rubble))
    rows.append(("Wall face", [intact["wall_face"], face, ruined_facade, rubble]))

    for family, keys in art.FLOOR_VARIANTS.items():
        first_decal, last_decal = FLOOR_DECALS[family]
        original = [intact[key] for key in keys]
        chipped = [weather_tile(tile, decals[first_decal], 0.66 if warp else 0.76, i)
                   for i, tile in enumerate(original)]
        destroyed = [weather_tile(tile, decals[last_decal], 0.82 if warp else 0.95, i)
                     for i, tile in enumerate(original)]
        sheets.extend(original + chipped + destroyed)
        rows.append((family.replace("_", " ").capitalize(), [original[0], chipped[0], destroyed[0]]))
    return sheets, rows


def load_damaged_doors():
    with Image.open(art.HERE / "imperial-damaged-doors-source.png") as sheet:
        sheet = sheet.convert("RGBA")
    # The source contains a translucent neutral matte. Remove the matte while
    # retaining the original openings and keeping both axes registered.
    sheet.putalpha(sheet.getchannel("A").point(lambda alpha: max(0, min(255, round((alpha - 218) * 255 / 25)))))
    cells = art.sheet_cells(sheet, 4, 2)
    sprites = []
    for row in range(2):
        axis = cells[row * 4 : (row + 1) * 4]
        silhouette = Image.new("L", axis[0].size)
        for cell in axis:
            silhouette = ImageChops.lighter(silhouette, cell.getchannel("A").point(lambda a: 255 if a > 48 else 0))
        bounds = silhouette.getbbox()
        if bounds is None:
            raise ValueError("Empty door damage row")
        for cell in axis:
            sprite = Image.new("RGBA", art.SIZE)
            part = cell.crop(bounds)
            part.thumbnail(art.SIZE, art.RESAMPLE)
            sprite.alpha_composite(part, ((art.CELL - part.width) // 2, (art.CELL - part.height) // 2))
            sprites.append(sprite)
    return sprites


def damaged_open(opened, chipped_closed, axis):
    # Recover the open lane from the intact frame, keeping damage on the sockets.
    mask = Image.new("L", art.SIZE)
    box = (5, 0, 22, 27) if axis == 0 else (0, 6, 27, 21)
    ImageDraw.Draw(mask).rectangle(box, fill=255)
    return Image.composite(opened, chipped_closed, mask)


def tall_side_door(sprite):
    # Extend the blade and its gaps over two cells. The end housings retain
    # their original depth and the edge-on blade retains its original width.
    housing = 6
    result = Image.new("RGBA", (art.CELL, art.CELL * 2))
    result.alpha_composite(sprite.crop((0, 0, art.CELL, housing)))
    blade = sprite.crop((0, housing, art.CELL, art.CELL - housing))
    result.alpha_composite(blade.resize((art.CELL, art.CELL * 2 - housing * 2), art.RESAMPLE), (0, housing))
    result.alpha_composite(sprite.crop((0, art.CELL - housing, art.CELL, art.CELL)), (0, art.CELL * 2 - housing))
    return result


def pack(cells, columns=8):
    rows = (len(cells) + columns - 1) // columns
    width, height = cells[0].size
    sheet = Image.new("RGBA", (columns * width, rows * height))
    for index, cell in enumerate(cells):
        sheet.alpha_composite(cell, (index % columns * width, index // columns * height))
    return sheet


def render_preview(finishes, doors, damage_doors, floor):
    image = Image.new("RGBA", (862, 835), "#161a1d")
    draw = ImageDraw.Draw(image)
    label = ImageFont.load_default(15)
    draw.text((24, 18), "Imperial damage states", font=ImageFont.load_default(29), fill="#eee7d9")
    draw.text((24, 57), "28-pixel tiles at twice their size. Intact to broken, left to right.", font=label, fill="#b9aa95")

    def draw_tiles(x, y, name, stages):
        draw.text((x, y + stages[0].height - 5), name, font=label, fill="#ddd3c2")
        for index, tile in enumerate(stages):
            image.alpha_composite(tile.resize((tile.width * 2, tile.height * 2), Image.Resampling.NEAREST),
                                  (x + 107 + index * 64, y))

    for finish, title, left in [(finishes[0], "Imperial", 24), (finishes[1], "Warp-corrupted", 436)]:
        draw.text((left, 95), title, font=ImageFont.load_default(21), fill="#eee7d9")
        for index, (name, stages) in enumerate(finish):
            draw_tiles(left, 132 + index * 62, name, stages)
    draw.text((24, 585), "Door progression", font=ImageFont.load_default(21), fill="#eee7d9")
    for axis, title in enumerate(("North to south", "East to west")):
        original = doors[axis * 2 + 1]
        stages = [original, *damage_doors[axis * 5 : axis * 5 + 1], *damage_doors[axis * 5 + 2 : (axis + 1) * 5]]
        threshold = floor if axis == 0 else pack([floor, floor], columns=1)
        states = stages if axis == 0 else [tall_side_door(stage) for stage in stages]
        draw_tiles(24, 620 + axis * 62, title, [art.door_threshold(threshold, stage) for stage in states])
    return image.convert("RGB")


def main():
    RUNTIME.mkdir(exist_ok=True)
    furnishing = art.load_furnishings()
    doors, _ = art.load_doors()
    patterns = art.load_paired_art("imperial-floor-variants-source.png", 4, 2)
    structure = art.load_paired_art("imperial-structural-materials-source.png", 3, 1)
    chipped = art.load_paired_art("imperial-damaged-structure-source.png", 3, 1)
    decals = load_decals()
    previews = []
    for index, suffix, name in [(0, "", "ImperialDamageTerrain"), (2, "-corrupted-v2", "ImperialWarpDamageTerrain")]:
        intact = art.make_tiles(art.HERE / f"imperial{suffix}-materials-source.png", furnishing, doors,
                                patterns[index], structure[index])
        cells, rows = terrain_art(intact, chipped[index], decals, index == 2)
        pack(cells).save(RUNTIME / f"{name}.png")
        previews.append(rows)
        if index == 0:
            door_floor = intact["slate"]

    door_debris = load_damaged_doors()
    door_states = []
    for axis in range(2):
        dented, shattered, breached, collapsed = door_debris[axis * 4 : (axis + 1) * 4]
        door_states.extend([dented, damaged_open(doors[axis * 2], dented, axis), shattered, breached, collapsed])
    pack(door_states, columns=5).save(RUNTIME / "ImperialDamageDoors.png")
    pack([tall_side_door(door) for door in door_states[5:]], columns=5).save(RUNTIME / "ImperialDamageTallDoors.png")
    render_preview(previews, doors, door_states, door_floor).save(art.HERE / "imperial-damage-preview.png")


if __name__ == "__main__":
    main()

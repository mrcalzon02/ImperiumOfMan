"""Export multisquare machinery and its destruction stages."""

import json

from PIL import Image, ImageChops, ImageDraw

import render_damage as damage
import render_tiles as art

SLOT_WIDTH, SLOT_HEIGHT = 5 * art.CELL, 3 * art.CELL
STAGES = ("Intact", "Battered", "Disabled", "Wrecked")


def load_json(name):
    return json.loads((art.HERE / name).read_text(encoding="utf-8"))


def master_cells(name):
    with Image.open(art.HERE / name) as image:
        sprites = image.convert("RGBA")
        sprites.putalpha(sprites.getchannel("A").point(lambda value: value if value >= 32 else 0))
        return art.sheet_cells(sprites, 3, 2)


def fit_machine(cell, bounds, footprint):
    width, height = (dimension * art.CELL for dimension in footprint)
    return cell.crop(bounds).resize((width, height), art.RESAMPLE)


def load_machine_art(specs):
    suffixes = ("", "-battered", "-disabled", "-wreck")
    sheets = [
        list(zip(*(master_cells(f"imperial{finish}-machinery{suffix}-source.png") for suffix in suffixes), strict=True))
        for finish in ("", "-warp")
    ]
    finishes = [{}, {}]
    for spec, clean, warp in zip(specs, *sheets, strict=True):
        shared_bounds = ImageChops.lighter(clean[0].getchannel("A"), warp[0].getchannel("A")).getbbox()
        if shared_bounds is None:
            raise ValueError(f"Empty sprite slot for {spec['key']}")
        for finish, stages in zip(finishes, (clean, warp), strict=True):
            finish[spec["key"]] = tuple(fit_machine(cell, shared_bounds, spec["footprint"]) for cell in stages)
    return finishes


def export_machine_atlas(specs, machinery):
    atlas = Image.new("RGBA", (len(STAGES) * SLOT_WIDTH, len(specs) * SLOT_HEIGHT))
    for index, spec in enumerate(specs):
        for stage, sprite in enumerate(machinery[spec["key"]]):
            atlas.alpha_composite(sprite, (stage * SLOT_WIDTH, index * SLOT_HEIGHT))
    return atlas


def destruction_preview(specs, machine_art):
    label_width, column_width, row_height = 141, 156, 98
    panel = Image.new("RGBA", (label_width + column_width * len(STAGES), (len(specs) * 2 + 1) * row_height),
                      (32, 28, 28, 255))
    labels = ImageDraw.Draw(panel)
    for index, stage in enumerate(STAGES):
        labels.text((label_width + index * column_width + 12, 20), stage, fill="#ead5b7")
    for finish_index, finish in enumerate(machine_art):
        for row, spec in enumerate(specs):
            y = 47 + (finish_index * len(specs) + row) * row_height
            labels.rectangle((0, y, panel.width, y + row_height - 1), fill=(45 + (row % 2) * 6, 40, 42))
            labels.text((9, y + 23), spec["name"], fill="#ead5b7")
            labels.text((9, y + 43), ("Imperial", "Warp-corrupted")[finish_index], fill="#c5a391")
            for stage, sprite in enumerate(finish[spec["key"]]):
                panel.alpha_composite(sprite, (
                    label_width + stage * column_width + (column_width - sprite.width) // 2,
                    y + (row_height - sprite.height) // 2,
                ))
    return panel.crop((0, 0, panel.width, 47 + len(specs) * 2 * row_height))


def main():
    specs = load_json("imperial-machinery.json")
    machine_art = load_machine_art(specs)
    destruction_preview(specs, machine_art).save(art.HERE / "imperial-machinery-destruction.png")
    damage.RUNTIME.mkdir(exist_ok=True)
    for name, finish in zip(("ImperialMachinery", "ImperialWarpMachinery"), machine_art, strict=True):
        export_machine_atlas(specs, finish).save(damage.RUNTIME / f"{name}.png")


if __name__ == "__main__":
    main()

#!/usr/bin/env python3
"""Import seven generated material masters as native inventory/floor/shadow PNGs.

Source artwork is supplied by the generating operator. This tool performs only
the existing occult-item import steps: nearest-neighbour fitting, binary alpha,
and an alpha-derived floor shadow. It does not draw or replace existing items.
"""
import argparse
from pathlib import Path
import shutil
import subprocess
import tempfile

from PIL import Image


ROOT = Path(__file__).resolve().parents[1]
ITEM_IDS = (
    "iom_raw_cellulose_pulp",
    "iom_organic_oil",
    "iom_chemical_extract",
    "iom_reactive_minerals",
    "iom_carbon_feedstock",
    "iom_ceramic_feedstock",
    "iom_electronic_substrate",
)


def run(*args):
    subprocess.run(["magick", *map(str, args)], check=True)


def export(source_dir, output_dir):
    # Validate every master before changing any runtime files.
    sources = {item: source_dir / (item + ".png") for item in ITEM_IDS}
    for item, source in sources.items():
        if not source.is_file():
            raise ValueError(f"Missing generated master for {item}: {source}")
        with Image.open(source) as image:
            if "A" not in image.getbands():
                raise ValueError(f"Master needs an alpha channel: {source}")
            alpha = image.getchannel("A")
            if alpha.getbbox() is None or alpha.getextrema()[0] == 255:
                raise ValueError(f"Master needs a visible object and transparent background: {source}")

    with tempfile.TemporaryDirectory(prefix="iom-chemistry-import-") as temporary:
        stage = Path(temporary)
        for item, source in sources.items():
            for suffix, size, fit in (("icon", 24, 22), ("floor", 16, 14)):
                destination = stage / (item + "_" + suffix + ".png")
                run(source, "-trim", "+repage", "-filter", "point", "-resize", f"{fit}x{fit}",
                    "-gravity", "center", "-background", "none", "-extent", f"{size}x{size}",
                    "-channel", "A", "-threshold", "50%", "+channel",
                    "-strip", "PNG32:" + str(destination))
            floor = stage / (item + "_floor.png")
            shadow = stage / (item + "_shadow.png")
            run(floor, "-channel", "RGB", "-evaluate", "set", "0", "+channel",
                "-channel", "A", "-evaluate", "multiply", "0.45", "+channel",
                "-strip", "PNG32:" + str(shadow))

        # Validate the complete generated set before copying runtime PNGs.
        for item in ITEM_IDS:
            for suffix, size in (("icon", 24), ("floor", 16), ("shadow", 16)):
                path = stage / (item + "_" + suffix + ".png")
                with Image.open(path) as image:
                    if image.mode != "RGBA" or image.size != (size, size) or image.getbbox() is None:
                        raise ValueError(f"Invalid imported sprite: {path.name}")
        output_dir.mkdir(parents=True, exist_ok=True)
        for path in sorted(stage.glob("*.png")):
            shutil.copyfile(path, output_dir / path.name)
    print(f"Imported {len(ITEM_IDS) * 3} native chemistry PNGs into {output_dir}")


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--source-dir", type=Path, default=ROOT / "media/RawMaterialChemistry/Source")
    parser.add_argument("--output-dir", type=Path, default=ROOT / "src/Art/RawMaterialChemistry")
    args = parser.parse_args()
    try:
        export(args.source_dir.resolve(), args.output_dir.resolve())
    except (ValueError, OSError, subprocess.CalledProcessError) as error:
        parser.exit(1, f"Chemistry art import failed: {error}\n")


if __name__ == "__main__":
    main()

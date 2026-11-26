#!/usr/bin/env python3
"""Import generated occult masters into exact native PNG sizes and UI states.

All object artwork is image_gen output. ImageMagick only performs deterministic
game import: nearest-neighbour sizing, native palette/state mapping, the user's
requested pixel frame, and alpha-derived floor shadows. It never redraws art.
"""
import json
from pathlib import Path
import subprocess
import tempfile

ROOT = Path(__file__).resolve().parents[1]
MANIFEST = ROOT / "media/OccultProduction/art-manifest.json"
OUT = ROOT / "src/Art/OccultProduction"


def run(*args):
    subprocess.run(["magick", *map(str, args)], check=True)


def export():
    data = json.loads(MANIFEST.read_text())
    OUT.mkdir(parents=True, exist_ok=True)
    with tempfile.TemporaryDirectory(prefix="iom-occult-import-") as temporary:
        palette = Path(temporary) / "sage-palette.png"
        run("-size", "1x1", "xc:#153e32", "xc:#34735b", "xc:#65966a", "+append", palette)
        for entry in data["research"]:
            source = ROOT / entry["source"]
            base = OUT / (entry["filename"] + ".png")
            locked = OUT / (entry["filename"] + "Locked.png")
            fitted = Path(temporary) / (entry["filename"] + "-fitted.png")
            alpha = Path(temporary) / (entry["filename"] + "-alpha.png")
            colored = Path(temporary) / (entry["filename"] + "-palette.png")
            run(source, "-trim", "+repage", "-filter", "point", "-resize", "29x29",
                "-gravity", "center", "-background", "none", "-extent", "36x36",
                "-channel", "A", "-threshold", "50%", "+channel", fitted)
            run(fitted, "-alpha", "extract", alpha)
            run(fitted, "-alpha", "off", "+dither", "-remap", palette, colored)
            # ImageMagick's palette remap also maps alpha. Restore the original
            # pixel silhouette explicitly before adding the requested frame.
            run(colored, alpha, "-alpha", "off", "-compose", "CopyOpacity", "-composite",
                "-compose", "Over",
                "+antialias", "-stroke", "#34735b", "-strokewidth", "1", "-fill", "none",
                "-draw", "rectangle 0,0 35,35", "-strip", "PNG32:" + str(base))
            # Exactly the same geometry and alpha, in the native blocked palette.
            run(base, "-channel", "RGB", "-fill", "#58071c", "-opaque", "#153e32",
                "-fill", "#8d1131", "-opaque", "#34735b",
                "-fill", "#c21642", "-opaque", "#65966a", "+channel",
                "-strip", "PNG32:" + str(locked))
        for entry in data["items"]:
            source = ROOT / entry["source"]
            for suffix, size, fit in (("icon", 24, 22), ("floor", 16, 14)):
                destination = OUT / (entry["filename"] + "_" + suffix + ".png")
                run(source, "-trim", "+repage", "-filter", "point", "-resize", f"{fit}x{fit}",
                    "-gravity", "center", "-background", "none", "-extent", f"{size}x{size}",
                    "-channel", "A", "-threshold", "50%", "+channel",
                    "-strip", "PNG32:" + str(destination))
            floor = OUT / (entry["filename"] + "_floor.png")
            shadow = OUT / (entry["filename"] + "_shadow.png")
            run(floor, "-channel", "RGB", "-evaluate", "set", "0", "+channel",
                "-channel", "A", "-evaluate", "multiply", "0.45", "+channel",
                "-strip", "PNG32:" + str(shadow))
    print(f"Exported {len(data['research']) * 2 + len(data['items']) * 3} native PNG assets to {OUT}")


if __name__ == "__main__":
    export()

#!/usr/bin/env python3
"""Check both actual runtime facade atlases against the stock 28px texture."""
from pathlib import Path
from PIL import Image
import argparse
import hashlib

root = Path(__file__).resolve().parents[2]
parser = argparse.ArgumentParser()
parser.add_argument("--runtime-dir", type=Path,
                    default=root / "media" / "ImperialInteriors" / "Runtime")
parser.add_argument("--base-sprite", type=Path,
                    help="Optional exact extracted slot 192 for offline asset-only checks.")
args = parser.parse_args()
if args.base_sprite:
    base = Image.open(args.base_sprite).convert("RGBA")
else:
    original = Image.open(args.runtime_dir / "ImperialDamageTerrain.png").convert("RGBA")
    assert original.size == (224, 868)
    base = original.crop((0, 24 * 28, 28, 25 * 28))
assert base.size == (28, 28)

seen = set()
for filename, top_rows in [("ImperialFacadeVariants.png", 9),
                           ("ImperialFacadeTopbarVariants.png", 2)]:
    path = args.runtime_dir / filename
    atlas = Image.open(path).convert("RGBA")
    assert atlas.size == (112, 84), filename
    edges = [(0, 0, 3, 28), (25, 0, 28, 28),
             (0, 0, 28, top_rows), (0, 26, 28, 28)]
    for index in range(12):
        x, y = index % 4 * 28, index // 4 * 28
        tile = atlas.crop((x, y, x + 28, y + 28))
        for region in edges:
            assert tile.crop(region).tobytes() == base.crop(region).tobytes(), (filename, index, region)
        pixels = tile.tobytes()
        assert pixels != base.tobytes() and pixels not in seen, (filename, index)
        seen.add(pixels)
        if top_rows == 2:
            # Distinctive variants must actually alter the central molding,
            # rather than merely add a fixture below an unchanged cornice.
            assert tile.crop((5, 3, 23, 9)).tobytes() != base.crop((5, 3, 23, 9)).tobytes(), index
    print(filename, "SHA256:", hashlib.sha256(path.read_bytes()).hexdigest())
assert len(seen) == 24
print("PASS: 24 distinct facade tiles; all 96 protected edge regions match the original; 12 molding interiors differ.")

#!/usr/bin/env python3
"""Validate the actual runtime atlas against the original Imperial facade."""
from pathlib import Path
from PIL import Image
import hashlib

root = Path(__file__).resolve().parents[2]
runtime = root / "media" / "ImperialInteriors" / "Runtime"
original = Image.open(runtime / "ImperialDamageTerrain.png").convert("RGBA")
atlas_path = runtime / "ImperialFacadeVariants.png"
atlas = Image.open(atlas_path).convert("RGBA")
assert original.size == (224, 868)
assert atlas.size == (112, 84)
base = original.crop((0, 24 * 28, 28, 25 * 28))
edges = [(0, 0, 3, 28), (25, 0, 28, 28), (0, 0, 28, 9), (0, 26, 28, 28)]
seen = set()
for index in range(12):
    x, y = index % 4 * 28, index // 4 * 28
    tile = atlas.crop((x, y, x + 28, y + 28))
    for region in edges:
        assert tile.crop(region).tobytes() == base.crop(region).tobytes(), (index, region)
    assert tile.tobytes() != base.tobytes(), index
    seen.add(tile.tobytes())
assert len(seen) == 12
print("PASS: 12 distinct facade tiles; all 48 protected edge regions match the original.")
print("SHA256:", hashlib.sha256(atlas_path.read_bytes()).hexdigest())

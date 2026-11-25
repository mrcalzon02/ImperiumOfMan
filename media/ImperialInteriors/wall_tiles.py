"""Full-cell Imperial wall art in Quasimorph's CornerWallTileset slot order."""

from PIL import Image

NORTH, EAST, SOUTH, WEST = (1, 2, 4, 8)
NEIGHBORS = (
    (0, -1, NORTH), (1, 0, EAST), (0, 1, SOUTH), (-1, 0, WEST),
    (1, -1, 16), (1, 1, 32), (-1, 1, 64), (-1, -1, 128),
)
# Diagonals count only when both walls that meet at that corner are present.
CORNERS = {NORTH | EAST: 16, SOUTH | EAST: 32, SOUTH | WEST: 64, NORTH | WEST: 128}
SLOT_MASKS = (
    4, 6, 11, 12, 38, 111, 76, 15, 5, 13, 0, 7, 63, 255, 207, 143,
    1, 3, 14, 9, 19, 159, 137, 31, 2, 10, 8, 39, 77, 95, 175, 47,
    46, 110, 78, 55, 205, 127, 239, 79, 27, 155, 139, 23, 141, 223, 191,
)
SLOT_FOR_MASK = {mask: index for index, mask in enumerate(SLOT_MASKS)}
FACADE = 47


def canonical_mask(mask):
    for sides, diagonal in CORNERS.items():
        if mask & sides != sides:
            mask &= ~diagonal
    return mask


def wall_index(rows, x, y, wall_symbols):
    mask = 0
    for dx, dy, bit in NEIGHBORS:
        nx, ny = x + dx, y + dy
        if 0 <= ny < len(rows) and 0 <= nx < len(rows[ny]) and rows[ny][nx] in wall_symbols:
            mask |= bit
    return SLOT_FOR_MASK[canonical_mask(mask)]


def corner_profile(mask, size):
    # This mask guides the corner bevel. The background plate stays fully textured.
    profile = Image.new("L", (size, size), 255)
    pixels = profile.load()
    for y in range(size):
        vertical = NORTH if y < size // 2 else SOUTH
        dy = min(y, size - y - 1)
        for x in range(size):
            horizontal = WEST if x < size // 2 else EAST
            dx = min(x, size - x - 1)
            joined_x, joined_y = mask & horizontal, mask & vertical
            convex = not joined_x and not joined_y
            concave = joined_x and joined_y and not mask & CORNERS[vertical | horizontal]
            if dx + dy < 2 and (convex or concave):
                pixels[x, y] = 0
    return profile


def bevel_tile(mask, crown, facade):
    size = crown.width
    inside = corner_profile(mask, size).load()
    surface = crown.copy()
    pixels = surface.load()
    face_pixels = facade.load()

    def solid(x, y):
        if (x < 0 and not mask & WEST) or (x >= size and not mask & EAST):
            return 0
        if (y < 0 and not mask & NORTH) or (y >= size and not mask & SOUTH):
            return 0
        return inside[max(0, min(size - 1, x)), max(0, min(size - 1, y))]

    for y in range(size):
        for x in range(size):
            original = pixels[x, y]
            north, west, east, south = solid(x, y - 1), solid(x - 1, y), solid(x + 1, y), solid(x, y + 1)
            if not inside[x, y]:
                color, strength = (86, 69, 55), 0.24
            elif not north or not west:
                color, strength = (146, 112, 79), 0.54
            elif not east or not south:
                color, strength = (115, 85, 57), 0.43
            elif not solid(x, y + 4):
                face = face_pixels[x, size - 10 + min(4, y % 5)]
                color, strength = face[:3], 0.28
            else:
                continue
            pixels[x, y] = tuple(round(base * (1 - strength) + accent * strength)
                                 for base, accent in zip(original[:3], color)) + (255,)
    return surface


def wall_family(crown, facade):
    return [bevel_tile(mask, crown, facade) for mask in SLOT_MASKS] + [facade]


def pier_top(mask, wall, stone):
    """Draw the horizontal top of a freestanding or wall-connected pier."""
    size = stone.width
    cap = stone.copy()
    pixels = cap.load()
    wall_pixels = wall.load()
    for y in range(size):
        for x in range(size):
            sides = ((y, NORTH), (size - 1 - x, EAST), (size - 1 - y, SOUTH), (x, WEST))
            join = min((distance for distance, direction in sides if mask & direction), default=size)
            if join == 0:
                pixels[x, y] = wall_pixels[x, y]
                continue

            depth, edge = min((side for side in sides if not mask & side[1]), default=(size, 0))
            color = pixels[x, y][:3]
            if depth < 3:
                rim = (155, 121, 89) if edge & (NORTH | WEST) else (116, 88, 65)
                strength = (0.68, 0.48, 0.13)[depth]
                color = tuple(round(base * (1 - strength) + tint * strength)
                              for base, tint in zip(color, rim))
            if join < 5:
                strength = (5 - join) / 5
                color = tuple(round(base * (1 - strength) + neighbor * strength)
                              for base, neighbor in zip(color, wall_pixels[x, y][:3]))
            pixels[x, y] = (*color, 255)
    return cap


def pier_family(walls, stone):
    caps = [pier_top(mask, wall, stone) for mask, wall in zip(SLOT_MASKS, walls[:FACADE], strict=True)]
    return caps + [walls[FACADE]]


def wall_atlas(family):
    size = family[0].width
    atlas = Image.new("RGBA", (8 * size, 6 * size))
    for index, wall in enumerate(family):
        atlas.alpha_composite(wall, (index % 8 * size, index // 8 * size))
    return atlas

"""Cut the existing lamp and grate sprites into transparent decal tiles."""

from pathlib import Path

from PIL import Image, ImageChops


ART = Path(__file__).resolve().parents[2] / 'src' / 'Art' / 'CampaignDecks'
SIZE = 28


def build():
    sheet = Image.open(ART / 'CampaignDeckFixtures.png').convert('RGB')
    row = SIZE * 3
    lamp = sheet.crop((0, row, SIZE, row + SIZE))
    difference = ImageChops.difference(lamp, Image.new('RGB', lamp.size, lamp.getpixel((0, 0))))
    red, green, blue = difference.split()
    alpha = ImageChops.lighter(red, ImageChops.lighter(green, blue)).point(lambda value: min(255, value * 12))
    lamp.putalpha(alpha)
    lamp = lamp.crop(alpha.getbbox()).resize((15, 5), Image.Resampling.LANCZOS)
    output = Image.new('RGBA', (5 * SIZE, SIZE))
    output.paste(lamp, (6, 13))

    for index in range(4):
        left = (4 + index) * SIZE
        grate = sheet.crop((left + 2, row + 2, left + SIZE - 2, row + SIZE - 2))
        grate = grate.resize((18, 18), Image.Resampling.LANCZOS)
        mask = Image.new('L', grate.size)
        mask.putdata([min(255, 88 * (1 + min(x, y, 17 - x, 17 - y))) for y in range(18) for x in range(18)])
        grate.putalpha(mask)
        output.paste(grate, ((1 + index) * SIZE + 5, 5))
    output.save(ART / 'CampaignFixtureOverlays.png', optimize=True)


if __name__ == '__main__':
    build()

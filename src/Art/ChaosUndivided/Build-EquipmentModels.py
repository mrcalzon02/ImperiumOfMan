"""Author the fittings that mount on Quasimorph's weapon and armor meshes.

Coordinates are normalized per mount: x across, y up, z forward. Leave
the grip, muzzle bore, eyes and joint plane clear. The game retains its
original meshes and maps these exported solids into those native bounds.
"""
import argparse
from pathlib import Path
import runpy
import sys

import bmesh
import bpy

BASE = Path(__file__).resolve().parent
TOOLS = runpy.run_path(str(BASE / "Build-EnemyModels.py"))
Sculpt = TOOLS["Sculpt"]


def point(x, y, z):
    return x, -z, y


class Fitting(Sculpt):
    def panel_at(self, name, center, size, surface, lean=0, bevel=.025):
        return self.panel(name, point(*center), (size[0], size[2], size[1]), surface, (0, lean, 0), bevel)

    def tube(self, name, line, radii, surface=3, sides=8, organic=False):
        return self.loft(name, [point(*position) for position in line], radii, surface, sides, organic)

    def jewel(self, name, center, radii, surface=4):
        return self.mass(name, point(*center), (radii[0], radii[2], radii[1]), surface)

    def blade(self, name, outline, half_width, surface=1):
        # The forged edge narrows toward the tip. Keep both faces and the rim closed.
        count = len(outline)
        vertices = [point(side * half_width * max(.08, .72 - .83 * z), y, z)
                    for side in (-1, 1) for y, z in outline]
        faces = [tuple(reversed(range(count))), tuple(range(count, count * 2))]
        faces.extend((i, (i + 1) % count, (i + 1) % count + count, i + count) for i in range(count))
        part = self.mesh(name, vertices, faces, surface)
        bevel = part.modifiers.new("Ground edge bevel", 'BEVEL')
        bevel.width = .034
        bevel.segments = 1
        return part


# Plate, edge, organic insert, leather, powered parts, and fitted hardware.
KITS = (
    ("fury_fang_knife", "hidewraps", ("634338", "e1bb8e", "98724f", "8c5e4d", "dcbf8c", "be9465")),
    ("shard_pistol", "ritual_flak", ("735151", "bd9680", "9f6563", "713f46", "f2a995", "a58a65")),
    ("spawnbone_maul", "ribplate", ("6c5950", "e0c39b", "b1997b", "8d5147", "d79b70", "a8785e")),
    ("bound_cleaver", "possessed_carapace", ("675361", "d19373", "8f4850", "785057", "f3b477", "a07d62")),
    ("warp_claw_gauntlet", "talon_plate", ("7f6078", "ded3e7", "98617c", "69545f", "ffcee7", "c393a2")),
    ("excruciator_carbine", "forge_carapace", ("59675e", "a7caba", "985658", "60534d", "9df0de", "b28659")),
    ("fleshmetal_projector", "fleshmetal_plate", ("7a6255", "ccb294", "a77a65", "844f50", "f2ba84", "a88165")),
    ("reforged_harvester", "soulforge_siege_plate", ("5d5260", "a99c96", "8f514b", "665058", "ffab61", "c98f66")),
    ("hellforged_blade", "hellforged_panoply", ("633f4a", "df9c68", "a35655", "734b51", "ffc465", "ae704d")),
    ("shadowblade", "shadowbound_plate", ("5e526d", "c6b6df", "957490", "5d526b", "dcc0ff", "8d758d")),
)


def weapon(fit, tier):
    if tier == 1:
        fit.blade("Hooked fang with asymmetric spine", [(-.18, -.02), (.00, -.12), (.26, .11), (.50, .39),
                  (.36, .59), (.18, .48), (-.19, .28)], .43, 1)
        fit.tube("Binding along fang root", [(0, -.03, -.18), (0, .03, -.05), (0, .09, .05)], [.16, .23, .16], 3)
    elif tier == 2:
        for side in (-1, 1):
            fit.tube("Shard guide with clear bore", [(side * .31, .17, -.06), (side * .41, .20, .20),
                     (side * .27, .17, .45)], [(.16, .11), (.13, .12), (.038, .057)], 1)
        fit.panel_at("Breech shard reservoir", (0, .29, -.24), (.64, .33, .32), 2)
        fit.jewel("Siphon viewing bulb", (0, .47, -.20), (.18, .13, .14))
    elif tier == 3:
        fit.panel_at("Broad maul anvil face", (0, .04, .40), (.87, .70, .26), 0, bevel=.075)
        for station, width in ((.23, .38), (.32, .49), (.44, .57)):
            fit.tube("Formed rib surrounding the maul", [(0, -.45, station), (-width, -.27, station),
                     (-width, .25, station), (0, .47, station), (width, .25, station), (width, -.27, station)],
                     [.047, .08, .10, .11, .10, .065], 1)
        fit.panel_at("Wedge socket at shaft junction", (0, 0, .10), (.51, .46, .21), 5)
    elif tier == 4:
        fit.blade("Off-center cleaver blade", [(-.62, .35), (-.45, -.07), (-.13, -.08), (.16, .18),
                  (.61, .36), (.44, .57), (-.25, .59)], .53, 1)
        fit.panel_at("Blade-retention bracket", (.12, .10, .19), (.89, .30, .28), 0)
        fit.jewel("Binding pin", (.58, .06, .20), (.09, .12, .065), 4)
    elif tier == 5:
        fit.panel_at("Segmented dorsal knuckle brace", (0, .35, .16), (.99, .34, .34), 0)
        for offset, reach in ((-.36, .56), (0, .68), (.36, .57)):
            fit.tube("Hook talon anchored at the knuckle", [(offset, .24, .17), (offset, .38, .39),
                     (offset * .94, .19, reach), (offset * .89, -.12, reach - .08)], [.116, .103, .066, .016], 1)
            fit.jewel("Knuckle hinge cap", (offset, .46, .21), (.10, .066, .055), 5)
    elif tier == 6:
        for side in (-1, 1):
            fit.tube("Forward electrode rail", [(side * .42, .17, -.13), (side * .54, .17, .18),
                     (side * .31, .17, .47)], [.12, .13, .047], 1)
            fit.tube("Insulated return from the coil", [(side * .41, -.14, -.32), (side * .52, .06, -.24),
                     (side * .55, .18, .07), (side * .37, .19, .25)], [.116, .09, .08, .05], 4)
        fit.panel_at("Power pack over receiver", (0, .32, -.28), (.70, .36, .35), 0)
    elif tier == 7:
        for side in (-1, 1):
            fit.tube("Fused fore-end casting", [(side * .23, .06, -.17), (side * .41, .08, .04),
                     (side * .39, .14, .29), (side * .25, .18, .48)], [.12, .24, .18, .07], 2, organic=True)
            fit.jewel("Receiver blister", (side * .38, -.13, -.05), (.23, .23, .23), 2)
            fit.tube("Heat sink cheek", [(side * .48, -.03, -.16), (side * .54, .05, .13),
                     (side * .41, .20, .41)], [.085, .115, .054], 5)
        fit.panel_at("Raised breech spine", (0, .32, -.09), (.47, .33, .56), 0)
    elif tier == 8:
        fit.panel_at("Heavy raised feed bridge", (0, .32, -.22), (.87, .37, .49), 0)
        fit.panel_at("Underslung armored drum", (0, -.45, -.12), (.79, .39, .40), 5, bevel=.07)
        for side in (-1, 1):
            fit.panel_at("Open barrel shroud cheek", (side * .43, .09, .20), (.32, .54, .40), 0, lean=side * .12)
            for z in (.05, .18, .31):
                fit.panel_at("Raised cooling fin", (side * .60, .15, z), (.24, .47, .055), 5)
        fit.panel_at("Breech status window", (0, .54, -.25), (.32, .048, .20), 4)
    elif tier == 9:
        fit.blade("Broad stepped Hellforged blade", [(-.54, -.13), (-.34, .49), (-.08, .61), (.32, .47),
                  (.50, .26), (.42, -.13), (.10, -.27)], .43, 1)
        for y, z in ((.36, .10), (.39, .24), (.32, .38)):
            fit.tube("Forged spine tooth", [(0, y - .14, z - .06), (0, y + .24, z), (0, y + .12, z + .12)],
                     [.096, .07, .012], 0)
        fit.tube("Heated channel formed into face", [(0, .12, -.15), (0, .11, .20), (0, -.11, .53)], [.18, .19, .04], 4)
    else:
        for side in (-1, 1):
            fit.tube("Separated shadowblade edge", [(side * .09, .04, -.23), (side * .38, -.15, -.06),
                     (side * .51, -.16, .24), (side * .20, -.05, .55), (side * .06, .03, .68)],
                     [(.15, .16), (.15, .18), (.102, .128), (.079, .075), (.013, .015)], 1)
            fit.tube("Enamel channel inside split edge", [(side * .31, -.06, -.06), (side * .42, -.07, .21),
                     (side * .14, .03, .57)], [.056, .080, .025], 4)
        fit.panel_at("Relic fork collar", (0, .04, -.23), (.78, .46, .14), 0)


def head(fit, tier):
    if tier == 1:
        fit.tube("Stitched hood hem", [(-.51, -.31, .27), (-.41, .19, .38), (0, .43, .27),
                 (.41, .19, .38), (.51, -.31, .27)], [.065, .08, .087, .08, .065], 3)
        fit.panel_at("Offset hood clasp", (.31, -.26, .46), (.17, .18, .13), 5)
    elif tier == 2:
        fit.tube("Veil crown band", [(-.47, .19, .36), (-.26, .45, .43), (.22, .43, .47), (.50, .29, .29)],
                 [.09, .13, .13, .078], 5)
        fit.panel_at("Dropped cheek veil buckles", (-.30, -.27, .44), (.30, .40, .15), 3, lean=-.24)
        fit.panel_at("Upper brow seal", (.15, .43, .44), (.16, .26, .15), 4)
    elif tier == 3:
        fit.panel_at("Splayed bone brow", (0, .38, .27), (.93, .30, .42), 1)
        for side in (-1, 1):
            fit.tube("Rib around respirator cheek", [(side * .4, .22, .21), (side * .50, -.07, .49),
                     (side * .17, -.43, .52)], [.12, .10, .062], 1)
    elif tier in (4, 5):
        lean = .56 if tier == 5 else -.13
        for side in (-1, 1):
            fit.tube("Swept temple spar" if tier == 5 else "Riveted cheek crown", [(side * .31, -.17, .33),
                     (side * .49, .24, .27), (side * (.65 if tier == 5 else .46), .56, lean)],
                     [(.125, .10), (.145, .12), (.034, .047)], 5 if tier == 5 else 1)
        fit.panel_at("Visible hood edge", (0, .28, .42), (.72, .16, .16), 4 if tier == 5 else 0)
    elif tier == 6:
        fit.panel_at("Insulated rebreather body", (0, -.27, .47), (.62, .40, .23), 0)
        for side in (-1, 1):
            fit.tube("Gas return routed round temple", [(side * .24, -.27, .55), (side * .61, -.19, .31),
                     (side * .59, .10, -.28)], [.115, .10, .07], 3)
        fit.panel_at("Glass filter slit", (0, -.19, .59), (.39, .064, .054), 4)
    elif tier in (7, 8):
        fit.panel_at("Reinforced furnace brow", (0, .32, .43), (1.06, .30, .26), 0)
        for side in (-1, 1):
            fit.panel_at("Jaw heat shield", (side * .45, -.21, .27), (.24, .50, .49), 5, lean=side * .18)
        if tier == 7:
            fit.jewel("Fused mask ridge", (-.20, .47, .36), (.28, .27, .23), 2)
        else:
            fit.panel_at("Raised siege baffle", (0, .50, -.11), (.79, .33, .46), 5)
            fit.panel_at("Furnace slit", (0, .22, .58), (.50, .065, .06), 4)
    else:
        for side in (-1, 1):
            reach = .72 if tier == 9 else .81
            fit.tube("Hellforged crown horn" if tier == 9 else "Floating-point crown bracket",
                     [(side * .29, .26, .24), (side * .53, .56, .29), (side * .57, reach, .06 if tier == 9 else -.28)],
                     [.135, .115, .031], 1)
        fit.panel_at("Raised crown crest", (0, .51, .41), (.29, .38, .19), 0)
        fit.jewel("Inset crown lens", (0, .48, .52), (.102, .13, .076), 4)


def chest(fit, tier):
    if tier <= 2:
        tilt = -.35 if tier == 1 else .26
        fit.panel_at("Tensioned hide strap" if tier == 1 else "Crossed ritual sash",
                     (0, 0, .45), (.21, .91, .15), 3, lean=tilt)
        fit.panel_at("Chest closure", (.14, .18, .54), (.26, .21 if tier == 1 else .38, .12), 5, lean=tilt)
        if tier == 2:
            fit.panel_at("Second sash over collar", (-.18, .26, .39), (.24, .49, .18), 2, lean=-.49)
    elif tier == 3:
        fit.panel_at("Sternum load spreader", (0, -.02, .44), (.27, .74, .16), 2)
        for height, width in ((-.22, .37), (.02, .51), (.29, .57)):
            fit.tube("Cuirass rib socket", [(-width, height, .19), (-width * .72, height + .04, .48),
                     (0, height - .05, .56), (width * .72, height + .04, .48), (width, height, .19)],
                     [.075, .10, .093, .10, .075], 1)
    elif tier in (4, 7, 8):
        width = .88 if tier == 4 else 1.08
        fit.panel_at("Segmented breastplate", (0, .21, .40), (width, .53, .30), 0, bevel=.065)
        fit.panel_at("Flared abdomen plate", (0, -.28, .39), (width * .65, .33, .28), 5)
        if tier == 4:
            fit.panel_at("Asymmetric binding badge", (.24, .18, .57), (.27, .34, .13), 4, lean=.20)
        elif tier == 7:
            fit.jewel("Fused ribs over breastplate", (-.25, .24, .53), (.39, .29, .23), 2)
            fit.tube("Seamed ossified stitch", [(-.50, .33, .44), (-.15, .19, .66), (.17, -.16, .53)], [.06, .09, .04], 1)
        else:
            for y in (.04, .20, .37):
                fit.panel_at("Siege heat louver", (0, y, .57), (.69, .105, .16), 5)
            fit.panel_at("Belt temperature block", (.30, -.29, .53), (.25, .14, .12), 4)
    elif tier in (5, 6):
        fit.panel_at("Reinforced harness back plate", (0, .12, -.42), (.86, .72, .33), 0)
        for side in (-1, 1):
            fit.tube("Harness pack conduit", [(side * .21, -.33, -.48), (side * .36, -.05, -.65),
                     (side * .39, .47, -.55), (side * .30, .61, -.38)], [.11, .14, .13, .073], 4 if tier == 5 else 5)
        fit.panel_at("Harness breast buckle", (0, .11, .46), (.47, .31, .16), 5)
        if tier == 6:
            fit.tube("Insulated seal around front plate", [(-.41, .28, .38), (-.31, -.28, .48),
                     (.36, -.28, .45), (.43, .24, .34)], [.096, .112, .112, .062], 3)
    else:
        for side in (-1, 1):
            fit.panel_at("Overlapping mantle plate", (side * .31, .11, .40), (.56, .75, .29), 0, lean=side * .24)
            fit.tube("Mantle edging", [(side * .13, -.38, .52), (side * .44, -.13, .55),
                     (side * .59, .42, .34)], [.058, .10, .07], 1)
        fit.panel_at("Tapered center gorget", (0, .31, .52), (.31, .53 if tier == 9 else .65, .21), 5)
        fit.jewel("Relic chamber lens", (0, .23, .67), (.13, .21, .095), 4)


def shoulder(fit, tier):
    load = (.15, .24, .46, .46, .29, .48, .55, .61, .53, .45)[tier - 1]
    fit.panel_at("Shoulder load plate", (.21, .31, .11), (.68 + load, .30 + load * .27, .63),
                 3 if tier < 3 else 0, lean=.30 if tier in (4, 5, 10) else .10)
    if tier <= 2:
        fit.panel_at("Shoulder seam buckle", (.43, .37, .18), (.25, .14, .23), 5)
    elif tier == 3:
        fit.tube("Bone shoulder rib", [(.10, .38, .34), (.43, .56, .23), (.73, .26, .10)], [.088, .12, .044], 1)
    elif tier in (4, 7, 9):
        fit.tube("Shoulder ridge", [(.37, .37, .05), (.65, .59, .12), (.69, .54, -.25)], [.11, .14, .056], 2 if tier == 7 else 1)
    elif tier in (5, 10):
        fit.tube("Swept shoulder fairing", [(.19, .36, .28), (.49, .57, -.06), (.71, .63, -.45)], [.125, .14, .045], 1)
        fit.panel_at("Shoulder-edge tracer", (.52, .57, -.10), (.26, .072, .34), 4)
    else:
        fit.panel_at("Shielded shoulder housing", (.42, .49, .03), (.51, .36, .48), 5)
        fit.panel_at("Coupler inspection slot", (.48, .63, .16), (.27, .078, .20), 4)


def greave(fit, tier):
    if tier <= 2:
        fit.panel_at("Wrap closure", (0, .11, .42), (.59, .48 if tier == 1 else .63, .14), 3, lean=.32)
        fit.panel_at("Wrap keeper", (.13, -.12, .49), (.30, .17, .095), 5)
    elif tier == 3:
        for y in (-.23, .05, .34):
            fit.tube("Raised leg rib", [(-.42, y, .17), (-.28, y + .065, .49), (.27, y + .06, .49),
                     (.42, y, .23)], [.068, .104, .092, .048], 1)
    elif tier == 10:
        for side in (-1, 1):
            fit.tube("Split greave keel", [(side * .35, -.34, .35), (side * .41, -.06, .48),
                     (side * .30, .41, .52), (side * .13, .58, .38)], [.115, .15, .13, .024], 0)
        fit.panel_at("Recessed lower blade junction", (0, -.24, .45), (.41, .24, .17), 5)
        fit.tube("Exposed shadow channel", [(0, -.20, .51), (-.045, .13, .47), (0, .42, .40)], [.054, .074, .04], 4)
    else:
        width = .78 if tier == 5 else .91
        fit.panel_at("Greave front plate", (0, .22, .40), (width, .61, .30), 0,
                     lean=.35 if tier in (4, 5, 10) else .09, bevel=.064)
        fit.panel_at("Lower articulated leaf", (-.11 if tier == 7 else .05, -.25, .43), (.58, .24, .19), 5)
        if tier in (5, 9):
            fit.tube("Greave seam channel", [(-.38, -.15, .45), (0, .26, .59), (.34, .43, .46)], [.055, .069, .043], 4)
        elif tier == 6:
            fit.tube("Greave cable outside ankle plane", [(-.22, -.29, .43), (-.49, -.03, .46),
                     (-.49, .41, .32), (-.16, .40, .49)], [.09, .103, .08, .069], 4)
        elif tier == 7:
            fit.jewel("Fused kneecap", (.13, .40, .55), (.30, .23, .19), 2)
        elif tier == 8:
            fit.panel_at("Siege knee stop", (0, .55, .35), (.80, .25, .29), 5)


def boots(fit, tier):
    if tier <= 2:
        fit.panel_at("Flexible instep strap", (0, .29, .22), (.91, .16, .27), 3, lean=.15)
        fit.panel_at("Toe seam cap", (0, .15, .48), (.65, .22, .17), 5)
    else:
        toe_width = .87 if tier in (5, 10) else 1.06
        fit.panel_at("Forged toe shell", (0, .13, .45), (toe_width, .36, .36), 0,
                     bevel=.083 if tier in (5, 7, 10) else .041)
        fit.panel_at("Replaceable heel block", (0, -.24, -.38), (.86, .29, .31), 5)
        if tier in (3, 7):
            fit.panel_at("Bone toe rail", (0, .33, .42), (.68, .14, .26), 1)
        elif tier == 10:
            for side in (-1, 1):
                fit.tube("Hook toe reinforcement", [(side * .38, .11, .22), (side * .39, .16, .49),
                         (side * .19, .10, .68)], [(.12, .08), (.12, .09), (.03, .021)], 1)
        elif tier in (5, 6, 9):
            for side in (-1, 1):
                fit.panel_at("Tread edge insert", (side * .42, .09, .37), (.20, .20, .31), 4)
        else:
            for side in (-1, 1):
                fit.panel_at("Reinforced tread flange", (side * .47, -.16, .30), (.24, .30, .55), 5)


def build(options):
    output = options.output
    for index, (weapon_id, kit, palette) in enumerate(KITS, start=1):
        if options.tier and index != options.tier:
            continue
        for key, make in ((weapon_id, weapon), (kit + "_head", head), (kit + "_torso", chest),
                          (kit + "_torso_shoulder", shoulder), (kit + "_legs", greave), (kit + "_boots", boots)):
            bpy.ops.object.select_all(action='SELECT')
            bpy.ops.object.delete(use_global=False)
            sculpt = Fitting(key + "_fittings", palette)
            make(sculpt, index)
            if len(sculpt.organic) > 1:
                sculpt.union_skin()
            for part in sculpt.parts:
                mesh = bmesh.new()
                try:
                    mesh.from_mesh(part.data)
                    bmesh.ops.recalc_face_normals(mesh, faces=mesh.faces)
                    mesh.to_mesh(part.data)
                finally:
                    mesh.free()
            model, skin = TOOLS["export"](sculpt, output, minimum_vertices=16, minimum_faces=20)
            vertices, triangles, _ = TOOLS["reimport"](model)
            print(f"[Chaos equipment] {key} {len(vertices)} vertices, {len(triangles)} triangles", flush=True)
            if options.review:
                for view in ("front", "rear", "clay"):
                    TOOLS["review"](model, skin, options.review, view, centered=True)


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--tier", type=int, choices=range(1, 11))
    parser.add_argument("--output", type=Path, default=BASE / "EquipmentModels")
    parser.add_argument("--review", type=Path)
    build(parser.parse_args(sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []))

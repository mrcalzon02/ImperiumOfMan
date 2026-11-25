"""Build the editable Chaos creature sculpts and Quasimorph mesh/texture exports.

Run with Blender in background mode. Runtime exports are in Models. An optional
--review DIRECTORY stages reimported renders without modifying the runtime inputs.
"""
import argparse
from pathlib import Path
import math
import struct
import sys
import zlib

import bpy
from mathutils import Vector

BASE = Path(__file__).resolve().parent

# Material order also gives each surface its own strip in the baked texture.
PALETTES = {
    "fury":      ("60333c", "9a5955", "c4aa80", "291e29", "e08056", "4e363e"),
    "accursed_mutant": ("855044", "b06b58", "cfac7a", "362b29", "ec7950", "62463c"),
    "torment":   ("853947", "b45454", "d6ae7c", "492b36", "e98758", "674041"),
    "spawn":     ("63414a", "94594f", "d3b38a", "4d4b4c", "d97253", "4c3539"),
    "possessed": ("60363d", "934b4f", "c7a983", "48444b", "e77852", "8d634b"),
    "warp_talon":("795b7a", "995661", "e0d5ef", "9c7788", "ffd3e8", "c3998a"),
    "venomcrawler": ("563c39", "955e44", "a88b64", "535052", "68beb7", "b28051"),
    "obliterator": ("724248", "a15d57", "ceb999", "594b49", "ed9859", "8b6d61"),
    "soul_grinder": ("563f42", "8d523e", "9c8469", "514748", "f5924e", "b07a50"),
    "daemon_prince": ("574151", "88505a", "c2a48b", "3b384a", "f79558", "a17553"),
    "belakor":   ("484052", "635376", "a395b0", "383443", "bc8ef2", "746387"),
}
SURFACES = ("flesh", "fiber", "bone", "plate", "energy", "trim")


def rgb(value):
    return tuple(int(value[i:i + 2], 16) / 255 for i in (0, 2, 4))


def face_material(name, color):
    material = bpy.data.materials.new(name)
    material.diffuse_color = (*color, 1)
    return material


class Sculpt:
    def __init__(self, name, palette=None):
        self.name = name
        self.parts = []
        self.organic = []
        self.colors = tuple(map(rgb, PALETTES[name] if palette is None else palette))
        self.materials = [face_material(f"{name} {surface}", color) for surface, color in zip(SURFACES, self.colors)]

    def keep(self, name, surface, organic=False):
        part = bpy.context.object
        part.name = name
        part.data.materials.clear()
        part.data.materials.append(self.materials[surface])
        self.parts.append(part)
        if organic:
            self.organic.append(part)
        return part

    def mass(self, name, at, radii, surface=0, organic=False):
        bpy.ops.mesh.primitive_uv_sphere_add(segments=12, ring_count=8, location=at)
        bpy.context.object.scale = radii
        return self.keep(name, surface, organic)

    def panel(self, name, at, dimensions, surface=3, tilt=(0, 0, 0), bevel=.018):
        bpy.ops.mesh.primitive_cube_add(size=1, location=at, rotation=tilt)
        part = self.keep(name, surface)
        part.dimensions = dimensions
        bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
        edge = part.modifiers.new("Forged corners", 'BEVEL')
        edge.width = bevel
        edge.segments = 1
        return part

    def loft(self, name, points, radii, surface, sides=8, organic=False):
        points = list(map(Vector, points))
        verts = []
        faces = []
        for index, (center, radius) in enumerate(zip(points, radii)):
            tangent = (points[min(index + 1, len(points) - 1)] - points[max(index - 1, 0)]).normalized()
            reference = Vector((0, 1, 0)) if abs(tangent.y) < .85 else Vector((1, 0, 0))
            lateral = tangent.cross(reference).normalized()
            depth = tangent.cross(lateral).normalized()
            wide, deep = radius if isinstance(radius, tuple) else (radius, radius)
            for side in range(sides):
                angle = side * math.tau / sides
                verts.append(center + lateral * (wide * math.cos(angle)) + depth * (deep * math.sin(angle)))
                if index:
                    a = (index - 1) * sides + side
                    b = (index - 1) * sides + (side + 1) % sides
                    faces.append((a, b, b + sides, a + sides))
        faces.append(tuple(reversed(range(sides))))
        faces.append(tuple(range(len(verts) - sides, len(verts))))
        return self.mesh(name, verts, faces, surface, organic)

    def mesh(self, name, verts, faces, surface, organic=False):
        data = bpy.data.meshes.new(name)
        data.from_pydata(verts, [], faces)
        data.update()
        part = bpy.data.objects.new(name, data)
        bpy.context.collection.objects.link(part)
        bpy.ops.object.select_all(action='DESELECT')
        part.select_set(True)
        bpy.context.view_layer.objects.active = part
        return self.keep(name, surface, organic)

    def union_skin(self):
        if len(self.organic) < 2:
            return
        bpy.ops.object.select_all(action='DESELECT')
        for part in self.organic:
            part.select_set(True)
        bpy.context.view_layer.objects.active = self.organic[0]
        self.parts = [part for part in self.parts if part not in self.organic]
        bpy.ops.object.join()
        part = bpy.context.object
        bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
        part.data.remesh_voxel_size = .038
        bpy.ops.object.voxel_remesh()
        smooth = part.modifiers.new("Blended muscle junctions", 'SMOOTH')
        smooth.factor = .55
        smooth.iterations = 2
        decimate = part.modifiers.new("Tactical distance topology", 'DECIMATE')
        decimate.ratio = .37
        self.keep("Unified tissue and limb sockets", 0)

    def horn(self, name, points, width=.07, surface=2):
        return self.loft(name, points, [width, width * .72, .004], surface, sides=6)

    def fangs(self, x, y, z, spread, length, surface=2):
        for index in range(4):
            cx = x + (index - 1.5) * spread / 3
            self.loft("Tooth", [(cx, y, z), (cx, y - .018, z - length * .46),
                               (cx * .97, y - .005, z - length)], [.020, .013, .003], surface, sides=5)

    def wing(self, sign, x, y, z, span, height, surface=1):
        p = Vector((sign * x, y, z))
        joints = [p, p + Vector((sign * span * .46, .07, height)),
                  p + Vector((sign * span, .12, height * .83)),
                  p + Vector((sign * span * .85, .12, -height * .39)),
                  p + Vector((sign * span * .58, .08, -height * .12)),
                  p + Vector((sign * span * .49, .05, -height * .57)),
                  p + Vector((sign * span * .21, .02, -height * .25))]
        edges = [*joints, *(point + Vector((0, .027, 0)) for point in joints)]
        faces = []
        for index in range(1, len(joints) - 1):
            faces.extend(((0, index, index + 1), (7, index + 8, index + 7)))
        faces.extend((i, (i + 1) % 7, (i + 1) % 7 + 7, i + 7) for i in range(7))
        self.mesh("Folded wing membrane", edges, faces, surface)
        self.loft("Wing leading spar", joints[:3], [.044, .031, .006], 0)
        for end in (joints[3], joints[5]):
            self.loft("Membrane finger", [joints[1], (joints[1] + end) * .54, end], [.023, .013, .004], 0, sides=6)

    def coolant(self, path, width=.023):
        self.loft("External conduit", path, [width] * len(path), 5, sides=8)

    def eye(self, x, y, z, scale=.045):
        self.mass("Eye or rune socket", (x, y + .019, z), (scale * 1.45, scale * .45, scale * .88), 3)
        self.mass("Exposed optical core", (x, y, z), (scale, scale * .38, scale * .53), 4)


def quadruped(sc, width, body_height, head_height):
    sc.mass("Lumbar mass", (0, .21, body_height - .06), (width * .77, .29, .18), organic=True)
    sc.mass("Chest mass", (0, -.11, body_height), (width, .31, .21), organic=True)
    sc.loft("Shoulder to jaw", [(0, -.13, body_height), (0, -.37, head_height + .06),
                                (0, -.53, head_height)], [.15, .12, .11], 0, organic=True)
    for sign in (-1, 1):
        for front in (True, False):
            root_y = -.20 if front else .23
            x = sign * width * .73
            sc.loft("Weight-bearing leg", [(x, root_y, body_height), (x * 1.63, root_y + (.12 if front else -.16), .29),
                                          (x * 1.67, root_y - .13, .10), (x * 1.68, root_y - .24, .055)],
                    [.095, .070, .043, .04], 0, organic=True)
            for offset in (-.028, .028):
                sc.loft("Hooked foot claw", [(x * 1.68 + offset, root_y - .21, .08), (x * 1.69 + offset, root_y - .30, .055),
                                            (x * 1.70 + offset, root_y - .36, .030)], [.029, .019, .004], 2, sides=6)


def humanoid(sc, stance, width, depth, height, arm_scale=1, armor=False, asymmetric=False):
    hips = height * .45
    chest = height * .66
    crown = height * .89
    sc.mass("Pelvis", (0, .045, hips), (width * .72, depth * .88, height * .13), organic=True)
    sc.mass("Thorax", (0, 0, chest), (width, depth, height * .20), organic=True)
    sc.mass("Neck", (0, -.018, crown - .12), (width * .38, depth * .73, .12), organic=True)
    sc.mass("Skull", (0, -.052, crown), (width * .55, depth * .83, height * .116), organic=True)
    for side in (-1, 1):
        foot = side * stance
        sc.loft("Leg muscle", [(side * width * .47, .045, hips), (foot, -.081, height * .27),
                              (foot * 1.05, .056, .12), (foot * 1.05, -.078, .058)],
                [width * .42, width * .32, width * .24, width * .27], 0, organic=True)
        big = arm_scale * (1.48 if asymmetric and side == 1 else 1)
        shoulder = Vector((side * width * .88, .015, chest + .045))
        elbow = Vector((side * width * (1.42 + .13 * big), -.10, height * .47))
        hand = Vector((side * width * 1.62, -.23, height * (.30 if asymmetric and side == 1 else .36)))
        sc.loft("Upper and lower arm", [shoulder, elbow, hand], [.092 * big, .082 * big, .065 * big], 0, organic=True)
        if armor:
            sc.panel("Overlapping shoulder shell", (shoulder.x * 1.14, .003, shoulder.z + .026),
                     (width * .89, depth * 1.63, .146), 3, (0, side * .24, 0), .033)
            sc.panel("Knee and shin plate", (foot, -.115, height * .21), (width * .55, .09, height * .23), 3, bevel=.023)
        else:
            for shift in (-.029, .03):
                x = hand.x + shift
                sc.horn("Finger spur", [(x, hand.y, hand.z), (x + side * .018, hand.y - .055, hand.z - .11),
                                        (x + side * .030, hand.y - .12, hand.z - .075)], .028)
    return crown


def fury(sc):
    quadruped(sc, .20, .43, .38)
    sc.mass("Brow skull", (0, -.505, .402), (.115, .16, .103), 2)
    sc.mass("Lower jaw", (0, -.54, .295), (.097, .129, .035), 2)
    sc.fangs(0, -.626, .357, .164, .071)
    for side in (-1, 1):
        sc.wing(side, .10, .09, .49, .47, .27)
        sc.horn("Swept cheek spine", [(side * .09, -.49, .44), (side * .15, -.43, .52), (side * .22, -.37, .53)], .033)
    sc.loft("Balancing tail", [(0, .38, .42), (.072, .58, .35), (.18, .72, .40), (.27, .76, .51)],
            [.078, .056, .029, .004], 0, organic=True)


def mutants(sc, torment=False):
    if torment:
        crown = humanoid(sc, .25, .33, .31, 1.03, 1.26)
        sc.mass("Swollen ventral sac", (0, -.23, .58), (.27, .19, .29), 1)
        for side, offset in ((-1, 0), (1, .09), (-1, .17)):
            sc.loft("Grasping tendril", [(side * .23, .16, .70 + offset), (side * .46, .23, .90 + offset),
                                        (side * .54, .14, 1.12 + offset), (side * .65, -.055, 1.08 + offset),
                                        (side * .59, -.22, .95 + offset)], [.096, .073, .051, .035, .009], 1)
        sc.horn("Sternum spine", [(0, -.35, .67), (0, -.42, .75), (.055, -.52, .75)], .043)
    else:
        crown = humanoid(sc, .17, .22, .15, .91, 1.12)
        sc.mass("Spitting throat sac", (0, -.18, crown - .126), (.144, .145, .11), 1)
        for side in (-1, 1):
            sc.coolant([(side * .20, .11, .63), (side * .15, .31, .73), (side * .10, .19, crown + .14)])
            sc.eye(side * .075, -.159, crown + .041, .038)
    sc.mass("Hollow maw", (0, -.202, crown - .015), (.143 if torment else .103, .073, .095), 3)
    sc.fangs(0, -.260, crown + .04, .19 if torment else .145, .077)


def spawn(sc):
    crown = humanoid(sc, .24, .32, .26, 1.11, 1.45, asymmetric=True)
    sc.mass("Crushing hand", (.55, -.23, .33), (.19, .18, .17), organic=True)
    for offset, span in enumerate((.18, .23, .25, .22)):
        z = .54 + offset * .103
        for side in (-1, 1):
            sc.loft("Exposed curved rib", [(side * .30, -.05, z + .07), (side * span, -.26, z + .046),
                                         (side * .11, -.34, z), (side * .042, -.29, z - .035)], [.066, .045, .032, .012], 2)
    sc.horn("Broken spine", [(-.22, .14, .84), (-.34, .22, 1.06), (-.45, .25, 1.17)], .101)
    sc.eye(-.065, -.218, crown, .041)


def legionary(sc, talon=False):
    width = .237 if talon else .294
    crown = humanoid(sc, .20, width, .174, 1.07, .96, armor=True)
    sc.panel("Ribbed cuirass", (0, -.167, .664), (width * 1.69, .146, .303), 3, bevel=.042)
    sc.panel("Visored helm", (0, -.077, crown + .02), (.246, .259, .245), 3, bevel=.04)
    for side in (-1, 1):
        sc.eye(side * .054, -.213, crown + .046, .028)
        sc.horn("Helmet fin" if talon else "Brow horn",
                [(side * .10, -.098, crown + .08), (side * .173, -.012 if talon else -.15, crown + .147),
                 (side * .206, .115 if talon else -.17, crown + .249)], .047, 5 if talon else 2)
        if talon:
            sc.panel("Jump vane", (side * .292, .204, .755), (.188, .231, .458), 3, (0, side * .51, 0), .023)
            sc.panel("Jet mouth", (side * .33, .244, .574), (.085, .122, .068), 4, (0, side * .51, 0), .012)
            sc.panel("Exposed jump-pack coil", (side * .315, .317, .739), (.060, .026, .187), 4,
                     (0, side * .51, 0), .011)
            for i in range(3):
                x = side * (width * 1.61 + .038 * i)
                sc.horn("Paired warp claw", [(x, -.23, .405), (x + side * .045, -.30, .253),
                                             (x + side * .01, -.422, .195)], .032, 2)
    if not talon:
        sc.loft("Fused weapon arm", [(.458, -.245, .417), (.513, -.369, .281), (.48, -.52, .187),
                                    (.389, -.59, .28)], [.097, (.098, .049), (.063, .028), .005], 2)
        sc.horn("Asymmetric shoulder spikes", [(-.36, .05, .773), (-.49, .04, .873), (-.55, .094, .925)], .059)


def bridge_engine(sc, grinder=False):
    size = 1.24 if grinder else 1
    sc.panel("Split welded hull", (0, .055, .448 * size), (.52 * size, .66, .27 * size), 3, bevel=.061)
    sc.mass("Rounded flywheel housing", (0, -.137, .544 * size), (.275 * size, .39, .184), 5)
    sc.panel("Forehead shield", (0, -.361, .457 * size), (.374 * size, .156, .219), 3, bevel=.026)
    for side in (-1, 1):
        sc.eye(side * .092 * size, -.441, .49 * size, .047)
        for index, z in enumerate((-.25, .055, .325)):
            end = side * (.54 + (.05 if index == 1 else 0)) * size
            sc.loft("Wheel-hub leg link", [(side * .17 * size, z, .452), (side * .37 * size, z + .068, .471),
                                          (end, z - .068, .211), (end * 1.07, z - .135, .029)],
                    [(.088, .083), (.11, .075), (.064, .05), (.015, .025)], 3)
            sc.mass("Leg hub and golden thrust cap", (side * .34 * size, z + .048, .425), (.120, .082, .087), 5)
        sc.coolant([(side * .23, .26, .52), (side * .32, .32, .68), (side * .27, .06, .745)])
    if grinder:
        sc.panel("Armored flue tower", (-.06, .145, .813), (.255, .235, .34), 3, bevel=.034)
        sc.panel("Flue glowing throat", (-.06, .145, .989), (.172, .147, .032), 4)
        sc.loft("Harvester receiver and barrel", [(-.27, -.12, .61), (-.29, -.43, .56), (-.31, -.65, .51)],
                [.155, .125, .089], 3, sides=10)
        sc.loft("Serrated machinery claw", [(.34, -.16, .49), (.56, -.36, .31), (.66, -.54, .185), (.59, -.65, .15)],
                [.135, (.12, .079), (.074, .051), .007], 5)
    else:
        for side in (-1, 1):
            sc.loft("Excruciator barrel", [(side * .112, -.015, .63), (side * .134, -.273, .675),
                                           (side * .16, -.557, .683)], [.094, .072, .058], 3)
            sc.mass("Coil jacket seam", (side * .128, -.21, .669), (.086, .045, .082), 5)
            sc.mass("Bore", (side * .16, -.563, .683), (.047, .022, .046), 4)


def obliterator(sc):
    crown = humanoid(sc, .238, .34, .24, 1.13, 1.22, armor=True, asymmetric=True)
    sc.mass("Mineral shoulder blister", (-.278, -.087, .84), (.211, .21, .225), 2)
    sc.mass("Fused armored gut", (0, -.17, .633), (.28, .142, .20), 2)
    sc.loft("Fleshmetal receiver through the radial forearm", [(.469, -.049, .503), (.566, -.285, .423), (.57, -.566, .364)],
            [.127, .222, .139], 2, sides=10)
    sc.mass("Projector hot bore", (.57, -.575, .362), (.097, .026, .094), 4)
    sc.coolant([(.30, .12, .77), (.49, .16, .665), (.57, -.03, .57), (.57, -.33, .575)], .042)
    sc.eye(-.046, -.216, crown, .046)


def lord(sc, shadow=False):
    crown = humanoid(sc, .253, .318, .205, 1.21, 1.15, armor=True)
    sc.panel("Chest shield and gorget", (0, -.171, .783), (.449, .185, .356), 3, bevel=.048)
    sc.panel("Temple mask", (0, -.088, crown - .015), (.274, .246, .240), 3, bevel=.043)
    for side in (-1, 1):
        sc.horn("Crown horns", [(side * .102, -.081, crown + .06), (side * .232, .003, crown + .203),
                               (side * .226, -.023, crown + .367)], .065, 5 if shadow else 2)
        sc.eye(side * .058, -.22, crown - .004, .038)
        sc.wing(side, .19, .18, .97, .62 if shadow else .49, .414 if shadow else .305, 1)
        sc.panel("Overlapping mantle trim", (side * .26, -.104, .834), (.137, .105, .284), 5, (0, side * .37, 0), .017)
    if shadow:
        sc.loft("Sword hilt", [(-.486, -.265, .54), (-.499, -.257, .397)], [.045, .043], 5, sides=8)
        sc.loft("Faceted sword", [(-.487, -.295, .49), (-.485, -.362, .27), (-.51, -.58, .117)],
                [(.099, .031), (.088, .026), (.004, .004)], 3, sides=6)
        sc.loft("Edge channel", [(-.536, -.319, .467), (-.545, -.385, .274), (-.518, -.573, .122)],
                [.016, .019, .003], 4, sides=6)
    else:
        sc.loft("Halberd shaft", [(.51, -.25, .09), (.51, -.27, .41), (.56, -.30, 1.10)],
                [.030, .045, .031], 5, sides=7)
        sc.loft("Crescent halberd", [(.54, -.30, .86), (.60, -.31, 1.00), (.71, -.32, 1.08),
                                     (.76, -.33, 1.28), (.66, -.32, 1.33)],
                [(.075, .034), (.117, .036), (.096, .026), (.058, .021), (.005, .004)], 2, sides=6)
        sc.eye(.56, -.345, 1.025, .039)
    sc.eye(0, -.273, .799, .045 if shadow else .030)


DESIGNS = {
    "fury": fury, "accursed_mutant": mutants, "torment": lambda sc: mutants(sc, True),
    "spawn": spawn, "possessed": legionary, "warp_talon": lambda sc: legionary(sc, True),
    "venomcrawler": bridge_engine, "obliterator": obliterator, "soul_grinder": lambda sc: bridge_engine(sc, True),
    "daemon_prince": lord, "belakor": lambda sc: lord(sc, True),
}
ATLAS_WIDTH = 96
ATLAS_HEIGHT = 72
TONES = 8


def png_chunk(tag, data):
    return struct.pack(">I", len(data)) + tag + data + struct.pack(">I", zlib.crc32(tag + data) & 0xffffffff)


def export(sculpt, folder, minimum_vertices=200, minimum_faces=150, prefix="iom_chaos_"):
    # Runtime receives the same closed meshes reimported for the review renders.
    deps = bpy.context.evaluated_depsgraph_get()
    vertices = []
    faces = []
    used = {}
    for part in sculpt.parts:
        obj = part.evaluated_get(deps)
        mesh = obj.to_mesh()
        mesh.calc_loop_triangles()
        for tri in mesh.loop_triangles:
            surface = sculpt.materials.index(part.data.materials[tri.material_index])
            normal = (part.matrix_world.to_quaternion() @ tri.normal).normalized()
            shade = max(0, min(TONES - 1, round(3.65 + normal.z * 2.2 - normal.y * 1.15 + normal.x * .6)))
            face = []
            for index in tri.vertices:
                point = part.matrix_world @ mesh.vertices[index].co
                basis = (round(point.x, 5), round(point.z, 5), round(-point.y, 5))
                key = (basis, surface, shade)
                if key not in used:
                    used[key] = len(vertices)
                    # The palette is arranged as eight painted facets for each named material.
                    u = (shade + .5) / TONES
                    v = 1 - (surface + .5) / len(SURFACES)
                    vertices.append((*basis, u, v))
                face.append(used[key])
            if len(set(face)) == 3:
                faces.append((face[0], face[2], face[1]))
        obj.to_mesh_clear()
    if len(vertices) < minimum_vertices or len(faces) < minimum_faces:
        raise ValueError(f"Incomplete topology for {sculpt.name}")
    folder.mkdir(parents=True, exist_ok=True)
    name = prefix + sculpt.name
    mesh_path = folder / f"{name}.meshbytes"
    with mesh_path.open("wb") as stream:
        stream.write(b"ICM1")
        stream.write(struct.pack("<II", len(vertices), len(faces)))
        for vertex in vertices:
            stream.write(struct.pack("<5f", *vertex))
        for face in faces:
            stream.write(struct.pack("<3I", *face))
    pixels = bytearray()
    for y in range(ATLAS_HEIGHT):
        pixels.append(0)
        for x in range(ATLAS_WIDTH):
            surface = min(y * len(SURFACES) // ATLAS_HEIGHT, len(SURFACES) - 1)
            shade = min(x * TONES // ATLAS_WIDTH, TONES - 1)
            gain = .58 + .104 * shade
            color = sculpt.colors[surface]
            pixels.extend(max(0, min(255, round(channel * gain * 255 + shade * 2))) for channel in color)
    texture_path = folder / f"{name}_model.png"
    texture_path.write_bytes(b"\x89PNG\r\n\x1a\n" +
        png_chunk(b"IHDR", struct.pack(">IIBBBBB", ATLAS_WIDTH, ATLAS_HEIGHT, 8, 2, 0, 0, 0)) +
        png_chunk(b"IDAT", zlib.compress(bytes(pixels))) + png_chunk(b"IEND", b""))
    return mesh_path, texture_path


def reimport(path):
    data = path.read_bytes()
    if data[:4] != b"ICM1":
        raise ValueError(path)
    vertex_count, face_count = struct.unpack_from("<II", data, 4)
    verts = list(struct.iter_unpack("<5f", data[12:12 + vertex_count * 20]))
    faces = list(struct.iter_unpack("<3I", data[12 + vertex_count * 20:]))
    assert len(faces) == face_count and all(len(set(face)) == 3 and max(face) < vertex_count for face in faces)
    assert all(math.isfinite(value) for vertex in verts for value in vertex)
    points = [Vector((x, -z, y)) for x, y, z, _, _ in verts]
    for a, b, c in faces:
        assert (points[b] - points[a]).cross(points[c] - points[a]).length > 1e-10, (path, a, b, c)
    return verts, faces, points


def review(mesh, texture, output, view, centered=False):
    bpy.ops.object.select_all(action='SELECT')
    bpy.ops.object.delete(use_global=False)
    verts, faces, points = reimport(mesh)
    data = bpy.data.meshes.new(mesh.stem)
    data.from_pydata(points, [], [(a, c, b) for a, b, c in faces])
    data.update()
    uv = data.uv_layers.new(name="Baked material palette")
    for loop in data.loops:
        uv.data[loop.index].uv = verts[loop.vertex_index][3:]
    obj = bpy.data.objects.new(mesh.stem, data)
    bpy.context.collection.objects.link(obj)
    material = bpy.data.materials.new("Inspected runtime surface")
    material.use_nodes = True
    nodes = material.node_tree.nodes
    surface = nodes.get("Principled BSDF")
    surface.inputs["Roughness"].default_value = .78
    if view != "clay":
        image = nodes.new("ShaderNodeTexImage")
        image.image = bpy.data.images.load(str(texture.resolve()), check_existing=True)
        image.interpolation = 'Closest'
        material.node_tree.links.new(image.outputs["Color"], surface.inputs["Base Color"])
    else:
        surface.inputs["Base Color"].default_value = (.58, .54, .49, 1)
    data.materials.append(material)
    height = max(point.z for point in points)
    target = Vector((0, 0, height * .51))
    if centered:
        low = Vector(tuple(min(point[i] for point in points) for i in range(3)))
        high = Vector(tuple(max(point[i] for point in points) for i in range(3)))
        target = (low + high) * .5
        height = max(high - low)
    camera_position = Vector((2.6, 3.1 if view == "rear" else -3.1, 2.45))
    bpy.ops.object.camera_add(location=target + camera_position if centered else camera_position)
    camera = bpy.context.object
    camera.rotation_euler = (target - camera.location).to_track_quat('-Z', 'Y').to_euler()
    camera.data.type = 'ORTHO'
    camera.data.ortho_scale = max(1.36, height * 1.55)
    scene = bpy.context.scene
    scene.camera = camera
    for light_position, energy, size in [((3, -4, 6), 420, 5), ((-3, 1, 4), 145, 4)]:
        bpy.ops.object.light_add(type='AREA', location=light_position)
        light = bpy.context.object
        light.rotation_euler = (target - light.location).to_track_quat('-Z', 'Y').to_euler()
        light.data.energy = energy
        light.data.shape = 'DISK'
        light.data.size = size
    scene.render.engine = 'BLENDER_EEVEE'
    scene.world.color = (.21, .21, .21)
    scene.render.resolution_x = scene.render.resolution_y = 260
    scene.render.resolution_percentage = 100
    scene.render.image_settings.file_format = 'PNG'
    output.mkdir(parents=True, exist_ok=True)
    scene.render.filepath = str((output / f"{mesh.stem}-{view}.png").resolve())
    bpy.ops.render.render(write_still=True)


def parse():
    parser = argparse.ArgumentParser(__doc__)
    parser.add_argument("--pilot", choices=DESIGNS)
    parser.add_argument("--output", type=Path, default=BASE / "Models")
    parser.add_argument("--review", type=Path)
    return parser.parse_args(sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else [])


if __name__ == "__main__":
    options = parse()
    keys = [options.pilot] if options.pilot else DESIGNS
    products = []
    for key in keys:
        bpy.ops.object.select_all(action='SELECT')
        bpy.ops.object.delete(use_global=False)
        creature = Sculpt(key)
        DESIGNS[key](creature)
        creature.union_skin()
        mesh_path, texture_path = export(creature, options.output)
        verts, faces, points = reimport(mesh_path)
        print(f"[Chaos art] {key} {len(verts)} vertices {len(faces)} triangles "
              f"height={max(point.z for point in points):.3f} ground={min(point.z for point in points):.3f}", flush=True)
        products.append((mesh_path, texture_path))
    if options.review:
        for mesh_path, texture_path in products:
            for view in ("front", "rear", "clay") if options.pilot else ("front", "rear"):
                review(mesh_path, texture_path, options.review, view)

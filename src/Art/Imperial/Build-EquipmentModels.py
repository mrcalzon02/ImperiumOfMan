"""Build Imperial fittings for the mounted Quasimorph weapon and armor donors.

Fore-end fittings keep the native muzzle centered and the grip unchanged.
Armor fittings are authored in separate head, chest, shoulder, shin and foot
frames so the native costume joints remain free to move.
"""
import argparse
from pathlib import Path
import runpy
import sys

import bmesh
import bpy

BASE = Path(__file__).resolve().parent
COMMON = runpy.run_path(str(BASE.parent / "ChaosUndivided" / "Build-EquipmentModels.py"))
Fitting, TOOLS = COMMON["Fitting"], COMMON["TOOLS"]

# Painted plate, machined edge, inner shell, fabric, instrumentation, fasteners.
FORMATIONS = {
    "pdf": ("556048", "a6a682", "595447", "7b6e51", "d1ab63", "9e865b"),
    "cadia": ("526345", "a8ab83", "5e5846", "7d7257", "e3bf76", "a78d62"),
    "arbites": ("545a66", "979daa", "695559", "4c4b56", "db8a64", "a98f76"),
    "valhallan": ("747b7d", "c8c6b3", "9e9681", "8d8778", "ddb989", "a6a28a"),
    "steel": ("7b7251", "a9a486", "655d49", "6f664e", "c3ba7a", "a88e60"),
    "elysian": ("607783", "aabfbc", "596f78", "6a7576", "b4d8cd", "c4a675"),
    "krieg": ("5a6570", "a0a5a4", "6c6156", "6c7175", "d5b583", "aa9476"),
    "scion": ("4d637e", "8cabc2", "5f6075", "57647d", "9fd8e5", "c5a34f"),
    "skitarii": ("725350", "afb1a1", "867569", "8d4645", "9dddc7", "b69168"),
    "sororitas": ("514f59", "c2bdaf", "75464d", "814952", "f2b88c", "cb9b60"),
    "ordoxenos": ("5d6069", "929eaa", "73646b", "575661", "ccae9a", "b48a65"),
    "paladin": ("8595a1", "c7d3d5", "687583", "5c6873", "addbe9", "ba9975"),
    "golden": ("b58b51", "e0b876", "8d6543", "654f49", "b4dbdb", "cba580"),
}


ARMOR = {
    "pdf": ("iom_lightHelmet", "iom_lightChestplate", "iom_lightleggings", "iom_lightboots"),
    "cadia": ("iom_cadiaHelmet", "iom_cadiaChestplate", "iom_cadialeggings", "iom_cadiaboots"),
    "arbites": ("iom_arbites_helmet", "iom_arbites_cuirass", "iom_arbites_greaves", "iom_arbites_boots"),
    "valhallan": ("iom_valhallan_cap", "iom_valhallan_coat", "iom_valhallan_trousers", "iom_valhallan_boots"),
    "steel": ("iom_steel_legion_mask", "iom_steel_legion_coat", "iom_steel_legion_trousers", "iom_steel_legion_boots"),
    "elysian": ("iom_elysian_drop_helmet", "iom_elysian_flak_vest", "iom_elysian_harness_trousers", "iom_elysian_landing_boots"),
    "krieg": ("iom_krieg_siege_mask", "iom_krieg_siege_greatcoat", "iom_krieg_trench_trousers", "iom_krieg_trench_boots"),
    "scion": ("iom_scion_carapace_helmet", "iom_scion_carapace_cuirass", "iom_scion_carapace_greaves", "iom_scion_carapace_boots"),
    "skitarii": ("iom_skitarii_rebreather_hood", "iom_skitarii_insulated_coat", "iom_skitarii_sealed_leggings", "iom_skitarii_insulated_boots"),
    "sororitas": ("iom_sororitas_helmet", "iom_sororitas_cuirass", "iom_sororitas_greaves", "iom_sororitas_boots"),
    "ordoxenos": ("iom_ordoxenos_helmet", "iom_ordoxenos_coat", "iom_ordoxenos_greaves", "iom_ordoxenos_boots"),
    "paladin": ("iom_grey_knights_paladin_helmet", "iom_grey_knights_paladin_cuirass", "iom_grey_knights_paladin_greaves", "iom_grey_knights_paladin_sabatons"),
    "golden": ("iom_golden_paladin_helmet", "iom_golden_paladin_cuirass", "iom_golden_paladin_greaves", "iom_golden_paladin_sabatons"),
}


# Every receiver keeps its donor envelope at the grip. Fore-end and feed
# measurements describe visible mechanisms in the normalized donor box.
RIFLES = {
    "iom_lasgun": ("pdf", .40, .48, .08, "flat", "service"),
    "iom_winter_lasgun": ("valhallan", .60, .55, .08, "flat", "jacket"),
    "iom_steel_legion_lascarbine": ("steel", .40, .54, .11, "flat", "gasket"),
    "iom_elysian_drop_lascarbine": ("elysian", .30, .46, .12, "notch", "folding"),
    "iom_krieg_siege_lasgun": ("krieg", .72, .51, .09, "hood", "bipod"),
    "iom_scion_hotshot_lasgun": ("scion", .52, .69, .10, "optic", "power"),
    "iom_skitarii_galvanic_rifle": ("skitarii", .69, .46, .11, "optic", "gasket"),
    "iom_lassniper": ("pdf", .73, .47, .14, "optic", "bipod"),
}


BOLT = {
    "iom_bolter": ("cadia", .67, .54, .37, .32, "box"),
    "iom_sororitas_bolter": ("sororitas", .70, .63, .28, .30, "rib"),
    "iom_bolt_pistol": ("arbites", .55, .65, .19, .30, "pistol"),
    "iom_bolt_rifle": ("scion", .58, .54, .47, .41, "scope"),
    "iom_grey_knights_psycannon": ("paladin", .95, .66, .53, .40, "rail"),
    "iom_golden_psycannon": ("golden", 1.00, .72, .55, .42, "jacket"),
}


SPECIAL = {
    "iom_plasma_pistol": ("scion", "plasma"), "iom_melta": ("krieg", "melta"),
    "iom_judgement": ("arbites", "riot"), "iom_arbites_shock_maul": ("arbites", "baton"),
    "iom_krieg_engineer_shotgun": ("krieg", "trench"), "iom_krieg_entrenching_tool": ("krieg", "spade"),
    "iom_skitarii_arc_rifle": ("skitarii", "arc"), "iom_sororitas_flamer": ("sororitas", "flamer"),
    "iom_ordoxenos_needle_pistol": ("ordoxenos", "needle"), "iom_ordoxenos_power_blade": ("ordoxenos", "blade"),
    "iom_grey_knights_nemesis_sword": ("paladin", "nemesis"), "iom_golden_nemesis_sword": ("golden", "gilt"),
    "iom_power_fist": ("paladin", "fist"),
}


def helmet(fit, form):
    if form in ("pdf", "cadia", "valhallan"):
        thickness = .17 if form == "pdf" else .22 if form == "cadia" else .32
        fit.tube("Rolled brim and temple edge", [(-.47, .19, .30), (-.36, .37, .45),
                 (0, .42, .52), (.36, .37, .45), (.47, .19, .30)], [.085, thickness, thickness, thickness, .085],
                 3 if form == "valhallan" else 0)
        if form == "cadia":
            fit.panel_at("Low patrol radio cup", (.47, .06, -.02), (.24, .41, .33), 2)
            fit.panel_at("Radio clasp", (.60, .11, .05), (.07, .15, .12), 5)
        elif form == "valhallan":
            fit.panel_at("Insulated side ear flap", (.44, -.17, .16), (.22, .48, .39), 3)
    elif form in ("steel", "krieg", "skitarii"):
        width = .64 if form == "steel" else .48 if form == "krieg" else .60
        fit.panel_at("Sealed front respirator", (0, -.23, .42), (width, .37, .30), 2, bevel=.064)
        for side in (-1, 1):
            lift = .10 if form == "skitarii" else -.16
            fit.tube("Mounted return hose", [(side * .19, -.24, .51), (side * .47, -.32, .40),
                     (side * .56, lift, .03), (side * .39, .13, -.18)], [.076, .097, .084, .051], 3)
        fabric = .15 if form == "krieg" else .29
        fit.panel_at("Gasketed mask seal bar", (0, .16, .47), (width, fabric, .13), 4 if form == "skitarii" else 1)
        if form == "krieg":
            fit.panel_at("Raised trench helmet ridge", (0, .48, .02), (.23, .18, .83), 0)
    elif form in ("arbites", "elysian", "scion", "ordoxenos"):
        wide = {"arbites": .96, "elysian": .86, "scion": .74, "ordoxenos": .61}[form]
        fit.panel_at("Angular face shield brow", (0, .20, .43), (wide, .21, .27), 0, bevel=.043)
        fit.panel_at("Recessed visor strip", (0, .11, .55), (wide * .82, .092, .076), 4 if form in ("elysian", "scion") else 1)
        if form in ("scion", "ordoxenos"):
            fit.panel_at("Offset rangefinder housing", (.44, .26, .23), (.28, .29, .47), 2)
            fit.jewel("Rangefinder lens", (.44, .26, .48), (.081, .069, .03), 4)
        if form == "arbites":
            fit.panel_at("Extended riot jaw guard", (0, -.28, .46), (.82, .29, .24), 2)
        if form == "elysian":
            fit.panel_at("Flush top transponder", (0, .53, -.27), (.43, .17, .33), 5)
    else:
        central = .33 if form == "sororitas" else .43
        fit.panel_at("Raised center helm ridge", (0, .43, .10), (central, .32, .78), 0, bevel=.079)
        fit.panel_at("Deep frontal lower mask", (0, -.12, .46), (.72, .41, .21), 1, bevel=.066)
        fit.panel_at("Paired lenses in armored recess", (0, .20, .51), (.54, .091, .09), 4)
        if form == "golden":
            fit.panel_at("Gilt crown lip over brow", (0, .53, .35), (.73, .25, .26), 5)


def torso(fit, form, subductor=False):
    if form in ("pdf", "cadia", "valhallan", "krieg", "ordoxenos"):
        width = .62 if form in ("pdf", "valhallan") else .80
        fit.panel_at("Service insert across chest", (0, .16, .40), (width, .47, .24),
                     3 if form == "valhallan" else 0, bevel=.065)
        for side in (-1, 1):
            fit.panel_at("Field packet held by harness", (side * .29, -.24, .44), (.30, .33, .22),
                         3 if form != "ordoxenos" else 2, lean=side * .10)
        fit.panel_at("Front harness keeper", (0, -.16, .53), (.22, .18, .10), 5)
        if form in ("valhallan", "krieg", "ordoxenos"):
            fit.tube("Collar or lapel welt", [(-.48, .38, .27), (-.25, .29, .48),
                     (0, .11, .51), (.35, .43, .32)], [.077, .095, .069, .051], 3)
    elif form in ("steel", "elysian", "scion", "skitarii"):
        pack = {"steel": (.67, .47), "elysian": (.83, .68), "scion": (.70, .52), "skitarii": (.75, .62)}[form]
        fit.panel_at("Mounted rear service module", (0, .10, -.41), (pack[0], pack[1], .34),
                     3 if form == "steel" else 0, bevel=.065)
        for side in (-1, 1):
            fit.tube("Service connection at harness side", [(side * .27, -.28, -.45), (side * .48, -.26, -.21),
                     (side * .40, .24, .41)], [.083, .093, .064], 4 if form in ("scion", "skitarii") else 3)
        fit.panel_at("Harness chest release", (0, .05, .48), (.53, .28, .17), 5)
        if form == "elysian":
            fit.panel_at("Flat harness attachment rail", (0, .48, -.41), (.92, .13, .22), 1)
    else:
        width = .94 + .14 * subductor + .07 * (form in ("paladin", "golden"))
        fit.panel_at("Load-bearing upper breastplate", (0, .23, .40), (width, .52, .32), 0, bevel=.069)
        fit.panel_at("Recessed joint cover", (0, -.30, .42), (width * .73, .29, .24), 2)
        fit.tube("Shaped gorget rim", [(-width / 2, .47, .30), (-.24, .43, .52),
                 (0, .34, .56), (.24, .43, .52), (width / 2, .47, .30)], [.077, .107, .092, .107, .077], 1)
        if form == "sororitas":
            fit.panel_at("Slender chest crest", (0, .23, .59), (.27, .61, .11), 5)
        if form == "golden":
            fit.panel_at("Reinforced gilded centerline", (0, .18, .62), (.40, .68, .12), 5, bevel=.044)


def shoulder(fit, form, subductor=False):
    width = {"pdf": .68, "cadia": .81, "arbites": .98, "valhallan": .74, "steel": .80, "elysian": .65,
             "krieg": .83, "scion": .89, "skitarii": .79, "sororitas": .93, "ordoxenos": .65,
             "paladin": 1.10, "golden": 1.18}[form] + .12 * subductor
    fit.panel_at("Shoulder yoke", (.19, .25, .08), (width, .40, .58),
                 3 if form in ("pdf", "valhallan", "elysian") else 0, lean=.20 if form == "elysian" else .07, bevel=.058)
    fit.panel_at("Raised shoulder fastening edge", (.43, .33, .34), (.24, .21, .17), 5)
    if form in ("arbites", "scion", "sororitas", "paladin", "golden"):
        fit.tube("Rolled outer shoulder lip", [(.34, .42, -.26), (.64, .42, -.02), (.61, .37, .33)], [.093, .108, .078], 1)
    if form == "golden":
        fit.panel_at("Gilt shoulder plate cap", (.31, .53, -.05), (.53, .19, .54), 5)


def shin(fit, form):
    soft = form in ("pdf", "valhallan", "steel", "krieg", "elysian")
    fit.panel_at("Shin closure plate", (0, .20, .40), (.69 if soft else .89, .48 if soft else .67, .20 if soft else .28),
                 3 if soft else 0, lean=.29 if form in ("elysian", "ordoxenos") else .05, bevel=.051)
    fit.panel_at("Shin lower closure", (0, -.25, .45), (.63, .15, .15), 5)
    if form in ("scion", "skitarii", "paladin", "golden"):
        fit.tube("Shin plate service rail", [(-.32, -.21, .44), (-.31, .09, .55),
                 (-.27, .43, .43)], [.072, .089, .052], 4 if form == "skitarii" else 1)
    if form == "valhallan":
        fit.panel_at("Insulated ankle overlap", (0, -.40, .22), (.80, .28, .35), 3, bevel=.070)
    elif form == "pdf":
        fit.panel_at("Canvas gaiter stirrup", (.24, -.31, .43), (.23, .31, .11), 3, lean=.17)
    elif form == "cadia":
        fit.panel_at("Folded knee flange", (0, .51, .37), (.71, .19, .27), 1)
    elif form == "arbites":
        fit.panel_at("Reinforced outer knee block", (.30, .48, .31), (.36, .27, .31), 2)
    elif form in ("steel", "krieg"):
        reach = .48 if form == "steel" else .29
        fit.tube("Sealed overlap at outside gaiter", [(.28, -.31, .47), (.46, .03, .35), (.39, reach, .31)],
                 [.066, .088, .065], 3)
    elif form == "elysian":
        fit.panel_at("Landing brace hinge", (.31, .44, .30), (.27, .27, .29), 1, lean=.30)
    elif form == "scion":
        fit.panel_at("Carapace knee gusset", (.27, .49, .39), (.37, .23, .24), 2, lean=-.24)
    elif form == "skitarii":
        fit.jewel("Actuator socket", (.31, .41, .47), (.20, .17, .11), 5)
    elif form == "sororitas":
        fit.panel_at("Fluted central reinforcement", (0, .24, .57), (.26, .51, .12), 1, bevel=.047)
    elif form == "ordoxenos":
        fit.panel_at("Retention plate for leg holster", (.31, .02, .46), (.24, .49, .15), 2, lean=-.26)
    else:
        fit.panel_at("Overlapping siege knee stop", (0, .51 if form == "paladin" else .58, .45),
                     (.73, .24, .23), 5 if form == "golden" else 1, bevel=.046)


def foot(fit, form):
    heavy = form in ("arbites", "scion", "sororitas", "paladin", "golden")
    width = 1.03 if heavy else .84
    fit.panel_at("Toe cap with room for ankle travel", (0, .10, .42), (width, .36 if heavy else .26, .35),
                 0, bevel=.056)
    fit.panel_at("Heel wear block", (0, -.19, -.41), (width * .90, .26, .25), 5)
    if form in ("elysian", "skitarii", "golden"):
        for side in (-1, 1):
            fit.panel_at("Load transfer shoe rail", (side * .41, .04, .19), (.24, .24, .46), 1)
    if form == "krieg":
        fit.panel_at("Trench sole toe guard", (0, -.19, .49), (.96, .15, .25), 1)
    elif form == "pdf":
        fit.panel_at("Canvas instep strap", (0, .29, .23), (.78, .13, .24), 3)
    elif form == "cadia":
        fit.panel_at("Compact toe bumper", (0, -.02, .58), (.70, .19, .18), 1)
    elif form == "arbites":
        fit.panel_at("Lateral heel armor", (.40, .09, -.25), (.29, .36, .38), 2)
    elif form == "valhallan":
        fit.panel_at("Insulated toe welt", (0, .29, .43), (.79, .23, .28), 3, bevel=.060)
    elif form == "steel":
        fit.tube("Wraparound weather seal", [(-.42, .13, .29), (-.34, .21, .52), (.30, .21, .52),
                 (.42, .13, .31)], [.056, .075, .075, .056], 3)
    elif form == "elysian":
        fit.panel_at("Landing heel buffer", (0, .12, -.46), (.66, .36, .25), 2)
    elif form == "scion":
        fit.panel_at("Open toe plate ledge", (0, .28, .35), (.67, .13, .29), 1)
    elif form == "skitarii":
        fit.tube("Insulated supply loop", [(.15, .37, -.26), (.49, .27, -.05), (.42, .14, .38)], [.09, .10, .063], 4)
    elif form == "sororitas":
        fit.panel_at("Narrow forged toe rib", (0, .31, .44), (.25, .13, .35), 1, bevel=.041)
    elif form == "ordoxenos":
        fit.panel_at("Offset strap keeper", (.27, .28, .16), (.24, .16, .30), 5)
    elif form == "paladin":
        fit.panel_at("Overlapping armored toe panel", (0, .28, .51), (.77, .22, .21), 1, bevel=.062)
    else:
        fit.panel_at("Gilded toe stop", (0, .26, .54), (.91, .19, .20), 5)


def rifle(fit, profile):
    _, length, width, height, sight, feature = profile
    for side in (-1, 1):
        fit.panel_at("Fore-end box rail", (side * width * .53, .11, .37 - length * .5),
                     (.21 if feature != "jacket" else .35, .26, length), 3 if feature == "jacket" else 0)
    fit.panel_at("Receiver top registration strip", (0, .33, -.12), (width * .79, .10, length * .66), 5)
    if sight == "optic":
        fit.tube("Supported optic tube", [(0, .46, -.30), (0, .46, -.04), (0, .44, .21)],
                 [(.12, .15), (.11, .14), (.145, .18)], 2)
        fit.jewel("Scope lens behind sight hood", (0, .45, .22), (.085, .080, .03), 4)
    else:
        fit.panel_at("Front sight mount", (0, .34 + height, .38), (.24, .20, .12), 1)
    if feature in ("bipod", "folding", "power", "gasket"):
        for side in (-1, 1):
            fit.tube("Foregrip support", [(side * .22, -.07, .31), (side * .44, -.24, .05),
                     (side * .41, -.19, -.14)], [.074, .090, .075], 4 if feature == "power" else 5)
        if feature == "power":
            fit.panel_at("Detachable insulated power bay", (0, -.30, -.03), (.67, .35, .32), 2)
        elif feature == "folding":
            fit.panel_at("Compact folding joint over the receiver", (0, .43, -.32), (.47, .20, .24), 2)
        elif feature == "gasket":
            fit.panel_at("Sealed power shutter", (0, -.19, .03), (.58, .25, .31), 3)


def bolt(fit, profile):
    _, width, height, feed, barrel, kind = profile
    fit.panel_at("Forged bolter receiver shell", (0, .17, -.13), (width, height * .67, .55), 0, bevel=.055)
    fit.panel_at("Under-receiver magazine guard", (0, -.31, -.12), (width * .72, feed, .29), 2)
    for side in (-1, 1):
        fit.panel_at("Muzzle jacket flank", (side * width * .45, .17, .46 - barrel * .5), (.21, height * .51, barrel), 1)
        if kind in ("rib", "rail", "jacket"):
            fit.panel_at("Reinforced side feed strip", (side * width * .51, .27, -.04), (.11, .19, .42), 5)
    if kind in ("scope", "jacket"):
        fit.panel_at("Boxed sight housing", (0, .49, -.03), (width * .53, .20, .37), 2)
        fit.panel_at("Sight window", (0, .49, .16), (width * .32, .10, .056), 4)


def special(fit, kind):
    if kind in ("plasma", "melta", "arc", "flamer"):
        fit.panel_at("Energy receiver frame", (0, .32, -.17), (.66 if kind != "melta" else .92, .33, .40), 0)
        for side in (-1, 1):
            if kind == "arc":
                fit.tube("Insulated arc lead", [(side * .29, -.18, -.21), (side * .53, .07, -.08),
                         (side * .53, .17, .31), (side * .25, .17, .45)], [.098, .12, .085, .042], 4)
                fit.panel_at("Arc terminal bracket", (side * .37, .19, .32), (.28, .19, .19), 2)
            else:
                fit.tube("Coil return mounted along receiver", [(side * .31, .17, -.26), (side * .48, .30, -.03),
                         (side * .49, .23, .24), (side * .29, .17, .44)], [.10, .13, .09, .045], 4 if kind == "plasma" else 1)
        if kind in ("melta", "flamer"):
            fit.panel_at("Heat shield lower lip", (0, -.25, .14), (.77, .32, .42), 5)
        if kind == "flamer":
            fit.tube("Fuel return by the lower shroud", [(0, -.25, -.41), (.39, -.38, -.09),
                     (.39, -.10, .21)], [.14, .15, .081], 3)
    elif kind in ("riot", "trench", "needle"):
        width = .42 if kind == "needle" else .63
        for side in (-1, 1):
            fit.panel_at("Jeweled receiver cheek" if kind == "needle" else "Fore-end guard",
                         (side * width * .53, .14, .18), (.22, .37 if kind == "riot" else .24, .52), 0)
        fit.panel_at("Clamped upper rail", (0, .34, .05), (width, .15, .65 if kind == "trench" else .49), 5)
        if kind == "needle":
            fit.panel_at("Narrow toxin reservoir", (.36, -.04, -.17), (.29, .27, .26), 4)
    elif kind == "baton":
        fit.tube("Closed shock baton core", [(0, 0, .21), (0, 0, .35), (0, 0, .52)],
                 [(.22, .26), (.38, .36), (.31, .32)], 0)
        for side in (-1, 1):
            fit.panel_at("Guarded baton contact", (side * .35, .10, .38), (.23, .29, .22), 4)
    elif kind == "spade":
        fit.blade("Flared tool steel spade", [(-.48, .12), (-.39, .45), (0, .60), (.39, .45),
                  (.48, .12), (.23, .09), (-.23, .09)], .34)
        fit.panel_at("Tool shaft socket", (0, 0, .07), (.45, .35, .21), 5)
    elif kind == "fist":
        fit.panel_at("Forged power-fist knuckle shoe", (0, .32, .34), (.99, .43, .36), 0, bevel=.065)
        for x in (-.32, 0, .32):
            fit.panel_at("Separate blunt knuckle cap", (x, .25, .55), (.25, .32, .20), 1)
        fit.panel_at("Inset power block", (0, .57, .30), (.64, .12, .22), 4)
    else:
        width = .27 if kind == "blade" else .43
        fit.blade("Tapered powered sword edge", [(-width, -.12), (-width, .39), (-.10, .55),
                   (0, .65), (.13, .46), (width, .23), (width, -.12)], .40)
        fit.tube("Power seam along sword spine", [(0, .05, -.15), (0, .03, .24), (0, .01, .52)],
                 [.20, .15, .045], 4)
        if kind == "gilt":
            fit.panel_at("Gold guard collar and stops", (0, 0, -.16), (.78, .69, .15), 5)
        else:
            fit.panel_at("Blade guard block", (0, -.05, -.14), (.62, .48, .17), 0)


def export(id, form, create, options):
    bpy.ops.object.select_all(action='SELECT')
    bpy.ops.object.delete(use_global=False)
    fit = Fitting(id + "_fittings", FORMATIONS[form])
    create(fit)
    for part in fit.parts:
        mesh = bmesh.new()
        try:
            mesh.from_mesh(part.data)
            bmesh.ops.recalc_face_normals(mesh, faces=mesh.faces)
            mesh.to_mesh(part.data)
        finally:
            mesh.free()
    model, skin = TOOLS["export"](fit, options.output, minimum_vertices=16, minimum_faces=20, prefix="")
    vertices, triangles, _ = TOOLS["reimport"](model)
    print(f"[Imperial equipment] {id} {len(vertices)} vertices, {len(triangles)} triangles", flush=True)
    if options.review:
        for view in ("front", "rear", "clay"):
            TOOLS["review"](model, skin, options.review, view, centered=True)


def build(options):
    for form, ids in ARMOR.items():
        if options.form and options.form != form:
            continue
        for id, maker in zip(ids, (helmet, torso, shin, foot)):
            export(id, form, lambda fit: maker(fit, form), options)
        export(ids[1] + "_shoulder", form, lambda fit: shoulder(fit, form), options)
    if options.form in (None, "arbites"):
        export("iom_arbites_subductor_cuirass", "arbites", lambda fit: torso(fit, "arbites", subductor=True), options)
        export("iom_arbites_subductor_cuirass_shoulder", "arbites", lambda fit: shoulder(fit, "arbites", subductor=True), options)
    for profile_table, maker in ((RIFLES, rifle), (BOLT, bolt), (SPECIAL, special)):
        for id, profile in profile_table.items():
            form = profile[0]
            if options.form and options.form != form:
                continue
            export(id, form, lambda fit: maker(fit, profile[1] if maker == special else profile), options)


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--form", choices=FORMATIONS)
    parser.add_argument("--output", type=Path, default=BASE / "EquipmentModels")
    parser.add_argument("--review", type=Path)
    build(parser.parse_args(sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []))

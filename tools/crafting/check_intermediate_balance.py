#!/usr/bin/env python3
"""Check the two new middle-tier recipes against actual exported helper quantities.

This is a bounded source check, not native execution. The existing source exporter
recognizes IndustryRecipe's normalization before supplying effective quantities.
No committed graph export is modified; temporary exports are removed afterward.
"""
import argparse
from collections import defaultdict
from fractions import Fraction
import hashlib
import json
from pathlib import Path
import subprocess
import sys
import tempfile


ROOT = Path(__file__).resolve().parents[2]
BASELINE = "e6c3026b3a11e4194b4597866099fcee850d836a"
CASES = (
    ("ceramite_plates", "iom_ceramic_feedstock", "iom_rolled_plate",
     {"iom_raw_silicate": 2, "iom_chemical_extract": 2}, 5, 4),
    ("circuit_board", "iom_electronic_substrate", "wire",
     {"iom_polymer_stock": 2, "acid": 2}, 4, 3),
)


def require(condition, message):
    if not condition:
        raise ValueError(message)


def export_graph(destination, ref=None):
    command = [sys.executable, str(ROOT / "tools/crafting/export_source_graph.py"),
               "--root", str(ROOT), "--out", str(destination)]
    if ref:
        command.extend(("--ref", ref))
    subprocess.run(command, cwd=ROOT, check=True, capture_output=True, text=True)
    return json.loads(destination.read_text())


def one_recipe(graph, output):
    matches = [row for row in graph["records"]
               if row["kind"] == "magnum" and row["outputs"] == [output]]
    require(len(matches) == 1, f"Expected one source Magnum recipe for {output}")
    return matches[0]


def inputs(recipe):
    return {entry["item"]: Fraction(entry["effective_count"])
            for entry in recipe["quantities"]}


def canonical_other_recipes(graph):
    changed = {item for case in CASES for item in case[:2]}
    return sorted(json.dumps({key: value for key, value in row.items()
                              if key not in ("source", "line")}, sort_keys=True)
                  for row in graph["records"] if changed.isdisjoint(row["outputs"]))


def check(baseline_ref):
    with tempfile.TemporaryDirectory(prefix="iom-intermediate-balance-") as temporary:
        directory = Path(temporary)
        before = export_graph(directory / "before.json", baseline_ref)
        after = export_graph(directory / "after.json")

    expected_policy = {"ingredient_rule": "ceil_half_minimum_one", "batch_multiplier": 2}
    require(before["direct_recipe_quantity_policy"] == expected_policy and
            after["direct_recipe_quantity_policy"] == expected_policy,
            "IndustryRecipe normalization changed or is not recognized; review before comparing")
    require(canonical_other_recipes(before) == canonical_other_recipes(after),
            "Another source recipe changed outside the two approved consumers/intermediates")

    findings = []
    for output, middle, retained, preparation_inputs, old_hours, new_hours in CASES:
        old = one_recipe(before, output)
        new = one_recipe(after, output)
        preparation = one_recipe(after, middle)
        require(all(row["method"] == "SharedRecipe" for row in (old, new, preparation)),
                f"Shared access changed for {output}")
        require(old["effective_batch"] == new["effective_batch"] == 4,
                f"Consumer batch changed for {output}")
        require(preparation["effective_batch"] == 2 and preparation["hours"] == 2,
                f"Preparation must produce two {middle} in two base hours")
        require(inputs(preparation) == preparation_inputs, f"Preparation inputs differ for {middle}")
        require(inputs(new) == {retained: 1, middle: 1}, f"Consumer inputs differ for {output}")
        require(old["hours"] == old_hours and new["hours"] == new_hours,
                f"Consumer time differs for {output}")

        expanded = inputs(new)
        intermediate_count = expanded.pop(middle)
        share = intermediate_count / preparation["effective_batch"]
        for item, count in inputs(preparation).items():
            expanded[item] = expanded.get(item, Fraction(0)) + share * count
        amortized_hours = Fraction(str(new["hours"])) + share * Fraction(str(preparation["hours"]))
        require(expanded == inputs(old), f"Expanded material cost changed for {output}")
        require(amortized_hours == old_hours, f"Amortized base time changed for {output}")

        # One preparation batch supplies two consumer orders. Starting from zero
        # incurs the full preparation time and leaves one intermediate in cargo.
        first_order_hours = preparation["hours"] + new["hours"]
        spare_units = preparation["effective_batch"] - intermediate_count
        two_order_hours = preparation["hours"] + new["hours"] * 2
        require(spare_units == 1 and two_order_hours == old_hours * 2,
                f"First/second-order batch accounting differs for {output}")
        findings.append({
            "consumer": output, "intermediate": middle,
            "preparation_effective_inputs": preparation_inputs,
            "preparation_output": preparation["effective_batch"],
            "preparation_base_hours": preparation["hours"],
            "before_effective_inputs": {key: int(value) for key, value in inputs(old).items()},
            "after_effective_inputs": {key: int(value) for key, value in inputs(new).items()},
            "consumer_output": new["effective_batch"],
            "before_base_hours": old["hours"], "after_assembly_base_hours": new["hours"],
            "amortized_total_base_hours": float(amortized_hours),
            "first_order_from_zero_intermediates_base_hours": first_order_hours,
            "first_order_spare_intermediates": int(spare_units),
            "two_consumer_orders_total_base_hours": two_order_hours,
            "expanded_material_cost_unchanged": True, "shared_recipe_access_unchanged": True,
        })

    adjacency = defaultdict(set)
    for row in after["records"]:
        for item in row["inputs"]:
            adjacency[item].update(row["outputs"])
    for _, middle, *_ in CASES:
        seen, pending = set(), list(adjacency[middle])
        while pending:
            item = pending.pop()
            require(item != middle, f"Manufacturing cycle includes {middle}")
            if item not in seen:
                seen.add(item)
                pending.extend(adjacency[item])

    industry = (ROOT / "src/Plugin.Industry.cs").read_text()
    intermediate_source = (ROOT / "src/Plugin.ManufacturingIntermediates.cs").read_text()
    require(industry.index("RegisterManufacturingIntermediates();") <
            industry.index("RegisterComponentFoundations();"),
            "Intermediates must be registered before their consumer recipes")
    require(intermediate_source.count(".Disassembly = [];") == 2 and
            "AddShredProfile" not in intermediate_source,
            "New materials must retain independent empty disassembly and no recycling profile")
    for name in ("src/Plugin.GuardSupplies.cs", "src/Plugin.IndustryRecycling.cs"):
        original = subprocess.check_output(["git", "show", before["source_commit"] + ":" + name], cwd=ROOT)
        require((ROOT / name).read_bytes() == original, f"Protected ration/recycling source changed: {name}")

    relevant_paths = ("src/Plugin.Industry.cs", "src/Plugin.ComponentFoundations.cs",
                      "src/Plugin.ManufacturingIntermediates.cs", "src/Plugin.IndustrySupply.cs")
    return {
        "schema": "iom.manufacturing-intermediate-balance.v1",
        "status": "passed_bounded_source_check",
        "baseline_source_sha": before["source_commit"],
        "working_tree_head": after["source_commit"],
        "checked_source_sha256": {name: hashlib.sha256((ROOT / name).read_bytes()).hexdigest()
                                  for name in relevant_paths},
        "actual_helper_policy": after["direct_recipe_quantity_policy"],
        "cases": findings,
        "other_exported_recipes_unchanged": True,
        "new_material_manufacturing_cycles": [],
        "registration_precedes_consumers": True,
        "new_material_disassembly": "Independent empty lists; no recycling profiles added",
        "finished_ration_and_protected_recycling_source_unchanged": True,
        "scope": "Source-export normalization and dependency checks only. No native assemblies or game were executed; native registration, runtime output delivery and active-order save compatibility remain separate checks.",
    }


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--baseline-ref", default=BASELINE)
    parser.add_argument("--out", type=Path, help="Optional destination for the bounded source-check receipt")
    args = parser.parse_args()
    try:
        result = check(args.baseline_ref)
    except (KeyError, OSError, ValueError, subprocess.CalledProcessError) as error:
        parser.exit(1, f"Intermediate balance check failed: {error}\n")
    text = json.dumps(result, indent=2) + "\n"
    if args.out:
        args.out.parent.mkdir(parents=True, exist_ok=True)
        args.out.write_text(text)
        print(f"Passed; receipt written to {args.out}")
    else:
        print(text, end="")


if __name__ == "__main__":
    main()

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using MGSC;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private const string ServoCartridge = "iom_servo_cartridge";
        private const string ImplantController = "iom_implant_controller";
        private const string SterileScaffold = "iom_sterile_scaffold";
        private const int CyberneticsIconSize = 20;
        private static readonly Dictionary<string, string> ImplantAbilityDonors = new();

        private sealed class CyberneticsLab(string maker, string site, string depot, bool quasi = false)
        {
            internal string Maker { get; } = maker;
            internal string Site { get; } = site;
            internal string Depot { get; } = depot;
            internal bool Quasi { get; } = quasi;
        }

        private static void RegisterCyberneticsWorkshops()
        {
            AddMaterial(ServoCartridge, "metal_tread", "Servo cartridge", "A sealed actuator and bearing cartridge for replacement joints.",
                38, 1, PlanetBridge, maxIconSize: CyberneticsIconSize).Weight = 0.20f;
            AddMaterial(ImplantController, "circuit_board", "Implant controller", "A shielded signal board for powered implants.",
                48, 2, "SBN", maxIconSize: CyberneticsIconSize).Weight = 0.08f;
            AddMaterial(SterileScaffold, "plastic", "Sterile implant scaffold", "A sealed support matrix for implant housings and grafts.",
                42, 2, "DaydreamChems", maxIconSize: CyberneticsIconSize).Weight = 0.12f;

            SharedRecipe(ServoCartridge, 4, 2, new("rod_parts", 1), new("metal_tread", 1), new(PolymerStock, 1));
            SharedRecipe(ImplantController, 5, 2, new("microelectronics_parts", 1), new("circuit_board", 1), new("wire", 1));
            SharedRecipe(SterileScaffold, 5, 2, new("biomass_tank", 1), new(Dressing, 1), new(PolymerStock, 1));
            AddFactionWorkshop(PlanetBridge, WinterBay, 1, "iom_winterbay_servo_cartridges", 108,
                [new("rod_parts", 1), new("metal_tread", 1), new(PolymerStock, 1)], [new(ServoCartridge, 10)]);
            AddFactionWorkshop("SBN", "CathodeRampart", 2, "iom_cathode_implant_controllers", 120,
                [new("microelectronics_parts", 1), new("circuit_board", 1), new("wire", 1)], [new(ImplantController, 10)]);
            AddFactionWorkshop("DaydreamChems", "Sundog", 2, "iom_sundog_sterile_scaffolds", 120,
                [new("biomass_tank", 1), new(Dressing, 1), new(PolymerStock, 1)], [new(SterileScaffold, 10)]);
            AddFactionWorkshop("CResistance", "RogueCity", 2, "iom_resistance_servo_cartridges", 120,
                [new(Metal, 1), new("spring", 1)], [new(ServoCartridge, 8)]);
            AddFactionWorkshop("CResistance", "TeslaRing", 4, "iom_resistance_implant_controllers", 144,
                [new("electrical_parts_container", 1), new("wire", 1)], [new(ImplantController, 8)]);
            AddFactionWorkshop("CResistance", "RogueCity", 2, "iom_resistance_implant_scaffolds", 132,
                [new("water_container", 1), new(Textile, 2)], [new(SterileScaffold, 8)]);

            RegisterHexarchyCybernetics();
            RegisterCorporateCybernetics();
            RegisterIndependentCybernetics();
            RegisterReplacementWorkshops();
        }

        private static void AddNativeImplant(CyberneticsLab lab, string id, params ItemQuantity[] inputs)
        {
            var record = Data.Items.GetSimpleRecord<ImplantRecord>(id);
            ItemQuantity[] materials = inputs.Length == 0 ? record.Disassembly.ToArray() : inputs;
            AddCyberneticsLine(lab, record.TechLevel, id, materials);
        }

        private static void AddImplant(CyberneticsLab lab, string key, string title, string slot, int tier, string description,
            Dictionary<string, float> bonuses, Dictionary<string, float> penalties, ItemQuantity[] inputs)
        {
            string id = "iom_cyber_" + key;
            var record = CloneItem<ImplantRecord>(id, lab.Quasi ? "warp_armor" : "neural_jack", tier, 190 + tier * 75, id,
                maxIconSize: CyberneticsIconSize);
            record.SlotType = slot;
            record.AugmentationClass = lab.Quasi ? AugmentationClass.QuasiPassive : AugmentationClass.Passive;
            record.Categories = [lab.Maker];
            record.ImplicitBonusEffects = bonuses;
            record.ImplicitPenaltyEffects = penalties;
            record.Disassembly = [];
            if (lab.Maker == "SheduThousand")
                record.NatureTypes = ["human", "cyborg", "quasimorph"];
            RegisterItem("implants", record, title, description);
            AddCustomCybernetic(lab, id, title, tier, inputs);
        }

        private static void AddActiveCybernetic(CyberneticsLab lab, string key, string title, string donor, int tier, int charges,
            string description, ItemQuantity[] inputs, params (string Name, int Value)[] tuning)
        {
            string id = "iom_cyber_" + key;
            var implant = CloneItem<ImplantRecord>(id, donor, tier, 260 + tier * 80, id, maxIconSize: CyberneticsIconSize);
            implant.Categories = [lab.Maker];
            implant.Charges = charges;
            implant.Disassembly = [];
            RegisterItem("implants", implant, title, description);

            var perk = (PerkRecord)CloneRecordMethod.Invoke(Data.Perks.GetRecord(donor), null);
            perk.Id = id;
            perk.Parameters = [.. perk.Parameters.Where(parameter => !tuning.Any(entry => entry.Name == parameter.Name)),
                .. tuning.Select(entry => new PerkParameter
                {
                    Name = entry.Name, ValType = PerkParameter.ValueType.Int, IntVal = entry.Value
                })];
            StageRecord(Data.Perks, id, perk);
            ImplantAbilityDonors.Add(id, donor);
            Registrations.Add(() => TranslationPatch.AddImplantPerk(id, title, description));
            AddCustomCybernetic(lab, id, title, tier, inputs);
        }

        private static void AddReplacement(CyberneticsLab lab, string key, string title, string donor, int tier, int sockets,
            string description, Dictionary<string, float> bonuses, Dictionary<string, float> penalties, ItemQuantity[] inputs)
        {
            string id = "iom_cyber_" + key;
            var replacement = CloneItem<AugmentationRecord>(id, donor, tier, 210 + tier * 80, id, maxIconSize: CyberneticsIconSize);
            var slots = replacement.WoundSlotIds;
            var slot = (WoundSlotRecord)CloneRecordMethod.Invoke(Data.WoundSlots.GetRecord(slots[0]), null);
            slot.Id = id + "_slot";
            slot.ImplicitBonusEffects = bonuses;
            slot.ImplicitPenaltyEffects = penalties;
            slot.ImplantSocketsDefault = slot.ImplantSocketsMin = slot.ImplantSocketsMax = sockets;
            StageRecord(Data.WoundSlots, slot.Id, slot);
            Registrations.Add(() => TranslationPatch.AddAugmentationSlot(slot.Id, title));
            replacement.WoundSlotIds = [slot.Id, .. slots.Skip(1)];
            replacement.Categories = [lab.Maker];
            replacement.Disassembly = [];
            RegisterItem("augmentations", replacement, title, description);
            AddCustomCybernetic(lab, id, title, tier, inputs);
        }

        private static void AddCustomCybernetic(CyberneticsLab lab, string id, string title, int tier, ItemQuantity[] inputs)
        {
            AddCyberneticsLine(lab, tier, id, inputs);
            IndustryRecipe(id, 8 + tier * 2, 1, inputs);
            var schematic = CloneItem<DatadiskRecord>(id + "_plans", "low_chip", tier, 110 + tier * 38, FieldPlans);
            schematic.Categories = [lab.Maker];
            RegisterCatalog(schematic, title + " Schematic", $"Teaches the Magnum crew to fabricate the {title}.",
                lab.Maker, [id], supplier: lab.Depot);
        }

        private static void AddCyberneticsLine(CyberneticsLab lab, int tier, string product, params ItemQuantity[] inputs) =>
            AddCyberneticsBatch(lab, tier, product, inputs, product);

        private static void AddCyberneticsBatch(CyberneticsLab lab, int tier, string line,
            ItemQuantity[] inputs, params string[] products)
        {
            string suffix = line.StartsWith("iom_cyber_", StringComparison.Ordinal) ? line.Substring("iom_cyber_".Length) : line;
            string receipt = $"iom_cybernetics_{lab.Maker.ToLowerInvariant()}_{suffix}";
            AddFactionWorkshop(lab.Maker, lab.Site, tier, receipt + "_assembly", 120 + tier * 12,
                [.. inputs], products.Select(id => new ItemQuantity(id, 4)).ToList());
            AddFactionWorkshop(lab.Maker, lab.Depot, tier, receipt + "_issue", 240,
                products.Select(id => new ItemQuantity(id, 1)).ToList(), []);
        }

        private static string ImplantAbilityDispatchId(string id) =>
            ImplantAbilityDonors.TryGetValue(id, out string donor) ? donor : id;

        [HarmonyPatch(typeof(ActiveAbilitySystem), nameof(ActiveAbilitySystem.ProcessAbility))]
        private static class DispatchCyberneticAbilities
        {
            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var perkId = AccessTools.Field(typeof(Perk), nameof(Perk.PerkId));
                var alias = AccessTools.Method(typeof(Plugin), nameof(ImplantAbilityDispatchId));
                int reads = 0;
                foreach (var instruction in instructions)
                {
                    yield return instruction;
                    // The switch uses the donor's behavior. Item lookup, charges, and combat logs keep the installed ID.
                    if (instruction.opcode == OpCodes.Ldfld && Equals(instruction.operand, perkId) && ++reads == 2)
                        yield return new CodeInstruction(OpCodes.Call, alias);
                }
            }
        }
    }
}

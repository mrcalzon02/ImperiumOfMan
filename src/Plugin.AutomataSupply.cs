using System.Collections.Generic;
using HarmonyLib;
using MGSC;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private const string AutomataChassis = "iom_automata_chassis";
        private const string LogicCore = "iom_automata_logic_core";
        private const string AiModule = "ai_module";

        private static void RegisterAutomataComponents()
        {
            AddMaterial(AutomataChassis, "rusty_plates", "Automata Chassis",
                "A jointed alloy frame for industrial, recreational, and security automata.", 45, 2).Weight = 0.65f;
            AddMaterial(LogicCore, "circuit_board", "Automata Logic Core",
                "A powered controller for automata, drone avionics, and barrier systems.", 72, 2).Weight = 0.2f;

            SharedRecipe(AutomataChassis, 10, 1, new(Sheet, 2), new(Fittings, 1), new(PolymerStock, 1));
            SharedRecipe(LogicCore, 12, 1, new("circuit_board", 2), new("microelectronics_parts", 2),
                new("capacitor_parts", 1), new(Cell, 1));
            IndustryRecipe(AiModule, 22, 1, new(LogicCore, 1), new("microelectronics_parts", 2), new("capacitor_parts", 1));
        }

        private static void RegisterAutomataWorkshops()
        {
            AddSupportWorkshop(PhobosStation, 1, "iom_phobos_glassworks", 48,
                [new(Silicate, 3)], [new(GlassStock, 2)]);
            AddSupportWorkshop(PhobosStation, 1, "iom_phobos_cell_stock", 48,
                [new("acid", 2), new("wire", 3)], [new(CellStock, 3)]);
            AddSupportWorkshop(PhobosStation, 1, "iom_phobos_sealed_cells", 54,
                [new(GlassStock, 2), new(CellStock, 2)], [new(Cell, 6)]);
            AddSupportWorkshop(PhobosStation, 2, "iom_phobos_electrical_works", 144,
                [new("ore_cargo", 2), new("tool_container", 1)], [new("electrical_parts_container", 3)]);

            AddSupportWorkshop(HavocStation, 2, "iom_havoc_chassis_works", 64,
                [new(Sheet, 6), new(Fittings, 3), new(PolymerStock, 2)], [new(AutomataChassis, 4)]);
            AddSupportWorkshop(FoundryStation, 2, "iom_foundry_logic_works", 72,
                [new("electrical_parts_container", 1), new(Cell, 2)], [new(LogicCore, 3)]);
            AddSupportWorkshop(BroadswordStation, 5, "iom_broadsword_avionics", 96,
                [new(LogicCore, 3), new("electrical_parts_container", 1)], [new(AiModule, 2)]);
        }

        private static bool IsWorkshopBusyOrMissingInputs(Station station, string receiptId)
        {
            if (StationSystem.IsProducingItem(station, receiptId))
            {
                return true;
            }
            if (!_industryReady || !MaterialGatedWorkshops.Contains(receiptId))
            {
                return false;
            }

            var recipe = Data.BarterReceipts.GetRecord(receiptId);
            return ItemProductionSystem.GetAvailableToProduceCount(station.InternalStorage, recipe.InputItems) == 0;
        }

        [HarmonyPatch(typeof(StationSystem), nameof(StationSystem.ProduceItems))]
        private static class RequireWorkshopInputs
        {
            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var busy = AccessTools.Method(typeof(StationSystem), nameof(StationSystem.IsProducingItem));
                var needsInputs = AccessTools.Method(typeof(Plugin), nameof(IsWorkshopBusyOrMissingInputs));
                foreach (var instruction in instructions)
                {
                    // Check each recipe in order so earlier orders cannot spend the materials twice.
                    if (instruction.Calls(busy))
                    {
                        instruction.operand = needsInputs;
                    }
                    yield return instruction;
                }
            }
        }

        private static bool CanSendWorkshopFreight(FactionCachedData relations, string from, string to, Station source) =>
            (_industryReady && from == to &&
                (from == ItemCategory || (from == "RealWare" && source.Id == RealWareOreMine) ||
                    SupportWorkshops.TryGetValue(source.Id, out var workshops) &&
                    workshops.Exists(workshop => workshop.Faction == from))) ||
            relations.IsTradePartners(from, to);

        [HarmonyPatch(typeof(ShippingSystem), nameof(ShippingSystem.TryToShipItems))]
        private static class DomesticWorkshopFreight
        {
            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var partnership = AccessTools.Method(typeof(FactionCachedData), nameof(FactionCachedData.IsTradePartners));
                var canSend = AccessTools.Method(typeof(Plugin), nameof(CanSendWorkshopFreight));
                foreach (var instruction in instructions)
                {
                    // The native cache only contains other factions. Workshop suppliers also ship domestically.
                    if (instruction.Calls(partnership))
                    {
                        yield return new CodeInstruction(System.Reflection.Emit.OpCodes.Ldarg_S, (byte)6)
                            .MoveLabelsFrom(instruction);
                        instruction.opcode = System.Reflection.Emit.OpCodes.Call;
                        instruction.operand = canSend;
                    }
                    yield return instruction;
                }
            }
        }
    }
}

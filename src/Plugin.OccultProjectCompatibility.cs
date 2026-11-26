using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using MGSC;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        // The current native project injector clones equipment records but rejects
        // supplemental pact records. Extend that one path without replacing native
        // equipment cloning, project modifications, localization or saved projects.
        [HarmonyPatch(typeof(MagnumDevelopmentSystem), nameof(MagnumDevelopmentSystem.InjectItemRecord))]
        private static class PreserveProjectPactComponents
        {
            private static readonly MethodInfo RecordsGetter =
                AccessTools.PropertyGetter(typeof(CompositeItemRecord), nameof(CompositeItemRecord.Records));
            private static readonly MethodInfo ProjectRecordsGetter =
                AccessTools.Method(typeof(PreserveProjectPactComponents), nameof(GetNativeProjectRecords));

            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = instructions.ToList();
                int matches = codes.Count(code => code.Calls(RecordsGetter));
                if (matches != 1)
                    throw new InvalidOperationException("Pact project compatibility expected one native record enumeration; found " + matches);

                foreach (CodeInstruction code in codes)
                {
                    if (!code.Calls(RecordsGetter)) continue;
                    // Both calls consume the composite and return its record list.
                    // Reusing the instruction preserves its labels and exception blocks.
                    code.opcode = OpCodes.Call;
                    code.operand = ProjectRecordsGetter;
                }
                return codes;
            }

            private static bool IsPactComponent(BasePickupItemRecord record) =>
                record?.GetType() == typeof(PactComponentRecord);

            private static bool IsPactEquipment(List<BasePickupItemRecord> records) =>
                records.Count > 0 &&
                (records[0] is WeaponRecord || records[0] is ArmorRecord || records[0] is HelmetRecord ||
                 records[0] is LeggingsRecord || records[0] is BootsRecord) &&
                records.Any(IsPactComponent);

            private static List<BasePickupItemRecord> GetNativeProjectRecords(CompositeItemRecord source)
            {
                List<BasePickupItemRecord> records = source.Records;
                if (!AddedOccultRecipes.Contains(source.Id)) return records;
                // Preserve list identity and native behavior for every ordinary item.
                // Unknown supplemental types still reach the native exception path.
                return IsPactEquipment(records)
                    ? records.Where(record => !IsPactComponent(record)).ToList()
                    : records;
            }

            private static void Postfix(MagnumProject project, bool __runOriginal)
            {
                // A skipped original has not built a new native custom record.
                if (!__runOriginal || project == null || !AddedOccultRecipes.Contains(project.DevelopId)) return;
                if (Data.Items.GetRecord(project.DevelopId) is not CompositeItemRecord source ||
                    !IsPactEquipment(source.Records)) return;

                string customId = project.DevelopId + "_custom";
                if (Data.Items.GetRecord(customId) is not CompositeItemRecord custom || custom.Records.Count == 0)
                    throw new InvalidOperationException("Native equipment project did not create its primary record: " + customId);
                // Respect a component already supplied by another compatible patch.
                if (custom.GetRecord<PactComponentRecord>() != null) return;

                foreach (BasePickupItemRecord record in source.Records)
                {
                    if (!IsPactComponent(record)) continue;
                    var donor = (PactComponentRecord)record;
                    var clone = (PactComponentRecord)CloneRecordMethod.Invoke(donor, null);
                    clone.Id = customId;
                    clone.Categories = donor.Categories == null ? null : [.. donor.Categories];
                    clone.Disassembly = donor.Disassembly == null ? null : donor.Disassembly
                        .Select(item => new ItemQuantity(item.ItemId, item.Count)).ToList();
                    // Native primary records were added first, so their descriptor and
                    // project modifications remain authoritative. The pact's four
                    // scalar/string properties and other inherited data are preserved.
                    Data.Items.AddRecord(customId, clone);
                }
                UnityEngine.Debug.Log("[Cvar_ImperiumOfMan] Preserved project pact components: " +
                    project.DevelopId + " -> " + customId + " (" + source.Records.Count(IsPactComponent) + ").");
            }
        }
    }
}

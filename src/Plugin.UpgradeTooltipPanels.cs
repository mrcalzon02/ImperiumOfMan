using HarmonyLib;
using MGSC;
using UnityEngine;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private readonly struct ShipUpgradeTooltipRow
        {
            public readonly string Label;
            public readonly string Value;

            public ShipUpgradeTooltipRow(string label, string value)
            {
                Label = label;
                Value = value;
            }
        }

        // Keep the native builder responsible for the header, availability, built-in
        // modifiers, purchase quantities and pooled content lifetime. Our effects use
        // the current native property rows and icon used by Producer Codes.
        [HarmonyPatch(typeof(TooltipFactory), nameof(TooltipFactory.BuildMagnumPerkTooltip))]
        private static class AddCustomShipUpgradeTooltipPanels
        {
            private static void Postfix(TooltipFactory __instance, MagnumPerkRecord record,
                bool isPerkPurchased, bool showDefaultValues, State ____state,
                RectTransform[] ____itemsGridBlocks)
            {
                if (record == null || !HasCustomShipUpgradeTooltip(record.Id))
                {
                    return;
                }

                bool added = false;
                var progression = showDefaultValues ? null : ____state?.Get<MagnumProgression>();
                foreach (var row in ShipUpgradeTooltipRows(record.Id, progression,
                    showDefaultValues ? null : ____state?.Get<StoryTriggers>()))
                {
                    var panel = __instance.AddPanelToTooltip();
                    if (row.Value == null)
                    {
                        panel.SetMultilineName(row.Label).SetNameColor(Colors.AltGreen);
                    }
                    else
                    {
                        panel.SetIcon("common_datadisk").SetName(row.Label).SetValue(row.Value);
                    }
                    added = true;
                }

                if (!added)
                {
                    return;
                }

                // Native construction has already appended the price grid. Place it
                // after our effect rows, retaining its normal available/required counts.
                if (!isPerkPurchased && ____itemsGridBlocks != null &&
                    ____itemsGridBlocks.Length > 0 && ____itemsGridBlocks[0] != null)
                {
                    ____itemsGridBlocks[0].SetAsLastSibling();
                }

                // BuildEmptyTooltip re-enables PropertiesTooltip before the original
                // builder adds its rows. OnEnable schedules the native LateUpdate layout,
                // which sees these rows and the reordered costs before sizing/positioning
                // the popup. Keep that native lifecycle and its fade behavior intact.
            }
        }
    }
}

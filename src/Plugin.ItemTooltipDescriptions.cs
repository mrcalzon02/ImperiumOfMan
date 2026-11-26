using HarmonyLib;
using MGSC;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private static void AddCustomItemTooltipDescription(TooltipFactory factory, string id)
        {
            if (factory != null && TranslationPatch.TryGetItemTooltipDescription(id, out var description))
            {
                // The native builder uses shortdesc only for its single-line subtitle.
                // Keep the complete item text in the game's wrapping property panel.
                factory.AddPanelToTooltip().SetMultilineName(description).SetNameColor(Colors.AltGreen);
            }
        }

        [HarmonyPatch(typeof(ItemTooltipBuilder), nameof(ItemTooltipBuilder.Build),
            new[] { typeof(BasePickupItem), typeof(Player), typeof(Mercenary) })]
        private static class AddOwnedItemTooltipDescription
        {
            private static void Postfix(BasePickupItem item, TooltipFactory ____factory)
            {
                if (item != null)
                {
                    AddCustomItemTooltipDescription(____factory, item.Id);
                }
            }
        }

        [HarmonyPatch(typeof(ItemTooltipBuilder), nameof(ItemTooltipBuilder.Build),
            new[] { typeof(BasePickupItemRecord) })]
        private static class AddCatalogItemTooltipDescription
        {
            private static void Postfix(BasePickupItemRecord itemRecord, TooltipFactory ____factory)
            {
                if (itemRecord != null)
                {
                    AddCustomItemTooltipDescription(____factory, itemRecord.Id);
                }
            }
        }

        [HarmonyPatch(typeof(ItemTooltipBuilder), nameof(ItemTooltipBuilder.BuildAmmoSlot),
            new[] { typeof(BasePickupItemRecord), typeof(string), typeof(BasePickupItemRecord), typeof(Mercenary) })]
        private static class AddAmmoSlotTooltipDescription
        {
            private static void Postfix(string captionId, TooltipFactory ____factory)
            {
                // The native slot builder uses captionId for both displayed captions;
                // record and overrideRecord supply the compared ammunition statistics.
                AddCustomItemTooltipDescription(____factory, captionId);
            }
        }
    }
}

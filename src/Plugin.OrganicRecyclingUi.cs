using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MGSC;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private const string OrganicRecyclingTooltipPrefix = "iom.recycling.tooltip.";
        private static readonly Dictionary<string, string> OrganicRecyclingTooltipText = new()
        {
            ["ship_recycling"] = "Ship recycling",
            ["rations_per_batch"] = "{0} rations / batch",
            ["per_batch"] = "{0} / batch",
            ["remainder"] = "Remainder",
            ["retained"] = "Retained",
            ["whole_batches"] = "Selected stacks count together. Fewer than {0} eligible rations are not consumed.",
            ["selected"] = "Rations in recycler",
            ["eligible"] = "Eligible rations",
            ["returns"] = "Biomass returned",
            ["retained_count"] = "Rations retained"
        };

        private static void RegisterOrganicRecyclingUiText()
        {
            foreach (var entry in OrganicRecyclingTooltipText)
                TranslationPatch.AddShipSupportText(OrganicRecyclingTooltipPrefix + entry.Key, entry.Value);
        }

        private static string OrganicRecyclingUiText(string key)
        {
            string tag = OrganicRecyclingTooltipPrefix + key;
            return Localization.HasKey(tag) ? Localization.Get(tag) : OrganicRecyclingTooltipText[key];
        }

        private static void AddOrganicRecyclingTooltip(TooltipFactory factory, string id, bool includeSelection)
        {
            int inputBatch = GetShredInputBatch(id);
            if (factory == null || !_industryReady || id != DirtyStarch || inputBatch <= 1 ||
                !ShredProfiles.TryGetValue(id, out var outputs)) return;

            // The native disassembly record is per item, so it cannot describe this
            // aggregate recipe. Keep its real batch rate in native tooltip rows.
            factory.AddPanelToTooltip().SetIcon("common_items")
                .SetName(OrganicRecyclingUiText("ship_recycling"))
                .SetValue(string.Format(OrganicRecyclingUiText("rations_per_batch"), inputBatch));
            foreach (var output in outputs)
                factory.AddPanelToTooltip().SetIcon("common_items")
                    .SetName(OccultUiItemName(output.ItemId, output.ItemId == Biomass ? "Raw Vat Biomass" : output.ItemId))
                    .SetValue(string.Format(OrganicRecyclingUiText("per_batch"), output.Count));
            factory.AddPanelToTooltip().SetName(OrganicRecyclingUiText("remainder"))
                .SetValue(OrganicRecyclingUiText("retained"));
            factory.AddPanelToTooltip().SetMultilineName(
                string.Format(OrganicRecyclingUiText("whole_batches"), inputBatch));

            // Owned-item tooltips may show the live recycling selection. Catalogue
            // tooltips stay independent of the player's currently selected stacks.
            if (!includeSelection) return;
            var game = SingletonMonoBehaviour<SpaceGameMode>.Instance;
            var cargo = game != null ? game.Get<MagnumCargo>() : null;
            int totalSelected = cargo?.RecyclingStorage?.Items.Where(item => item.Id == id)
                .Sum(item => (int)item.StackCount) ?? 0;
            if (totalSelected <= 0) return;

            int eligible = CountSelectedShredInput(cargo, id);
            int batches = eligible / inputBatch;
            factory.AddPanelToTooltip().SetName(OrganicRecyclingUiText("selected")).SetValue(totalSelected);
            if (eligible != totalSelected)
                factory.AddPanelToTooltip().SetName(OrganicRecyclingUiText("eligible")).SetValue(eligible);
            foreach (var output in outputs)
                factory.AddPanelToTooltip()
                    .SetName(output.ItemId == Biomass ? OrganicRecyclingUiText("returns") :
                        OccultUiItemName(output.ItemId, output.ItemId))
                    .SetValue(output.Count * batches);
            factory.AddPanelToTooltip().SetName(OrganicRecyclingUiText("retained_count"))
                .SetValue(totalSelected - batches * inputBatch);
        }

        [HarmonyPatch(typeof(ItemTooltipBuilder), nameof(ItemTooltipBuilder.Build),
            new[] { typeof(BasePickupItem), typeof(Player), typeof(Mercenary) })]
        private static class AddOwnedOrganicRecyclingTooltip
        {
            private static void Postfix(BasePickupItem item, TooltipFactory ____factory)
            {
                if (item != null) AddOrganicRecyclingTooltip(____factory, item.Id, includeSelection: true);
            }
        }

        [HarmonyPatch(typeof(ItemTooltipBuilder), nameof(ItemTooltipBuilder.Build),
            new[] { typeof(BasePickupItemRecord) })]
        private static class AddCatalogueOrganicRecyclingTooltip
        {
            private static void Postfix(BasePickupItemRecord itemRecord, TooltipFactory ____factory)
            {
                if (itemRecord != null) AddOrganicRecyclingTooltip(____factory, itemRecord.Id, includeSelection: false);
            }
        }
    }
}

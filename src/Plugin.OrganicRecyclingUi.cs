using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MGSC;
using UnityEngine;

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
            ["retained_count"] = "Rations retained",
            ["rate"] = "{0} rations → {1} {2}"
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

        private static bool HasOrganicBulkSelection(MagnumCargo cargo) =>
            _industryReady && GetShredInputBatch(DirtyStarch) > 1 &&
            ShredProfiles.ContainsKey(DirtyStarch) &&
            (cargo?.RecyclingStorage?.Items.Any(item => item.Id == DirtyStarch && item.StackCount > 0) ?? false);

        private static string OrganicRecyclingSelectionPreview(MagnumCargo cargo)
        {
            int inputBatch = GetShredInputBatch(DirtyStarch);
            int selected = cargo.RecyclingStorage.Items.Where(item => item.Id == DirtyStarch)
                .Sum(item => (int)item.StackCount);
            int eligible = CountSelectedShredInput(cargo, DirtyStarch);
            int batches = eligible / inputBatch;
            var lines = new List<string>
            {
                OrganicRecyclingUiText("ship_recycling") + ": " +
                    OccultUiItemName(DirtyStarch, "Dirty Corpse Starch")
            };
            foreach (var output in ShredProfiles[DirtyStarch])
                lines.Add(string.Format(OrganicRecyclingUiText("rate"), inputBatch, output.Count,
                    OccultUiItemName(output.ItemId, output.ItemId == Biomass ? "Raw Vat Biomass" : output.ItemId)));
            lines.Add(OrganicRecyclingUiText("selected") + ": " + selected);
            if (eligible != selected)
                lines.Add(OrganicRecyclingUiText("eligible") + ": " + eligible);
            foreach (var output in ShredProfiles[DirtyStarch])
                lines.Add((output.ItemId == Biomass ? OrganicRecyclingUiText("returns") :
                    OccultUiItemName(output.ItemId, output.ItemId)) + ": " + output.Count * batches);
            lines.Add(OrganicRecyclingUiText("retained_count") + ": " + (selected - batches * inputBatch));
            lines.Add(string.Format(OrganicRecyclingUiText("whole_batches"), inputBatch));
            return string.Join(Environment.NewLine, lines);
        }

        [HarmonyPatch(typeof(ScreenWithShipCargo), nameof(ScreenWithShipCargo.RefreshView))]
        private static class RefreshOrganicRecyclingControl
        {
            private static void Prefix(CommonButton ____recycleButton) =>
                ____recycleButton?.GetComponent<OrganicRecyclingButtonPreview>()?.Restore();

            private static void Postfix(MagnumCargo ____magnumCargo, ItemStorage ____activeShipCargo,
                CommonButton ____recycleButton)
            {
                if (____recycleButton == null || ____magnumCargo == null ||
                    ____activeShipCargo != ____magnumCargo.RecyclingStorage ||
                    ____magnumCargo.RecyclingInProgress || !HasOrganicBulkSelection(____magnumCargo)) return;

                var preview = ____recycleButton.GetComponent<OrganicRecyclingButtonPreview>() ??
                    ____recycleButton.gameObject.AddComponent<OrganicRecyclingButtonPreview>();
                preview.Configure(____recycleButton, ____magnumCargo, HasRecyclableSelection(____magnumCargo));
                // Keep incomplete batches visible but disabled so hovering the real
                // native control can explain the requirement and retained amount.
                ____recycleButton.gameObject.SetActive(true);
            }
        }

        [HarmonyPatch(typeof(ScreenWithShipCargo), "RecycleButtonOnClick")]
        private static class RecheckOrganicRecyclingControl
        {
            [HarmonyPriority(Priority.First)]
            private static bool Prefix(MagnumCargo ____magnumCargo, CommonButton ____recycleButton)
            {
                if (!HasOrganicBulkSelection(____magnumCargo) || ____magnumCargo.RecyclingInProgress ||
                    HasRecyclableSelection(____magnumCargo)) return true;
                ____recycleButton?.GetComponent<OrganicRecyclingButtonPreview>()?.Show();
                // Avoid the native start sound when the selection changed after its
                // last refresh and no complete batch or ordinary recyclable remains.
                return false;
            }
        }

        private sealed class OrganicRecyclingButtonPreview : MonoBehaviour
        {
            private CommonButton _button;
            private MagnumCargo _cargo;
            private bool _originalInteractable;
            private bool _configured;
            private bool _shown;

            public void Configure(CommonButton button, MagnumCargo cargo, bool canRecycle)
            {
                Restore();
                _button = button;
                _cargo = cargo;
                _originalInteractable = button.IsInteractable;
                _configured = true;
                button.SetInteractable(_originalInteractable && canRecycle);
            }

            public void Restore()
            {
                Hide();
                if (_configured && _button != null) _button.SetInteractable(_originalInteractable);
                _configured = false;
                _cargo = null;
            }

            public void Show()
            {
                if (!_configured || _cargo == null || _cargo.RecyclingInProgress ||
                    !HasOrganicBulkSelection(_cargo)) return;
                var factory = SingletonMonoBehaviour<TooltipFactory>.Instance;
                if (factory == null) return;
                factory.HideTooltip();
                factory.ShowSimpleTextTooltip(OrganicRecyclingSelectionPreview(_cargo));
                factory.SetSimpleTextTooltipColor(HasRecyclableSelection(_cargo) ? Colors.AltGreen : Colors.LightRed);
                _shown = true;
            }

            public void Hide()
            {
                if (_shown) SingletonMonoBehaviour<TooltipFactory>.Instance?.HideSimpleTextTooltip();
                _shown = false;
            }

            private void OnDisable() => Restore();
        }

        // CommonButton.Select/Diselect also call these methods, keeping the same
        // preview available through native mouse and keyboard navigation.
        [HarmonyPatch(typeof(CommonButton), nameof(CommonButton.OnPointerEnter))]
        private static class ShowOrganicRecyclingControlPreview
        {
            private static void Postfix(CommonButton __instance) =>
                __instance.GetComponent<OrganicRecyclingButtonPreview>()?.Show();
        }

        [HarmonyPatch(typeof(CommonButton), nameof(CommonButton.OnPointerExit))]
        private static class HideOrganicRecyclingControlPreview
        {
            private static void Postfix(CommonButton __instance) =>
                __instance.GetComponent<OrganicRecyclingButtonPreview>()?.Hide();
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

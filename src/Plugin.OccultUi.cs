using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MGSC;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private const string OccultTooltipPrefix = "iom.occult.tooltip.";
        private static readonly Dictionary<string, string> OccultTooltipText = new()
        {
            ["culture"] = "Culture",
            ["containment"] = "Containment",
            ["separation"] = "Separation",
            ["condensation"] = "Condensation",
            ["materials"] = "Materials",
            ["relics"] = "Relics",
            ["spores"] = "Spore cultures",
            ["body_parts"] = "Body parts",
            ["soulforge"] = "Soulforge",
            ["augmentation"] = "Augmentation",
            ["recipe_knowledge"] = "Recipes",
            ["acquired_in_game"] = "Acquired in game",
            ["requires_research"] = "Requires Occult Research",
            ["known_pattern"] = "Recipe known; research capability required.",
            ["agga"] = "Agga",
            ["gannix"] = "Gannix",
            ["shartamakhum"] = "Shartamakhum",
            ["duggur_ron"] = "Duggur / Ron",
            ["scrivnus"] = "Scrivnus",
            ["ur_cattaram"] = "Ur / Cattaram",
            ["flauros"] = "Flauros",
            ["mars_replacements"] = "Six Mars replacements",
            ["advanced_occult"] = "Advanced occult work",
            ["quasi_implants"] = "Quasi implants"
        };

        private static void RegisterOccultUiText()
        {
            foreach (var entry in OccultTooltipText)
                TranslationPatch.AddShipSupportText(OccultTooltipPrefix + entry.Key, entry.Value);
            RegisterOrganicRecyclingUiText();
        }

        private static string OccultUiText(string key)
        {
            string tag = OccultTooltipPrefix + key;
            return Localization.HasKey(tag) ? Localization.Get(tag) : OccultTooltipText[key];
        }

        private static string OccultUiItemName(string id, string fallback)
        {
            string tag = "item." + id + ".name";
            return Localization.HasKey(tag) ? Localization.Get(tag) : fallback;
        }

        private static bool MissingOccultCapability(ItemProduceReceipt receipt,
            MagnumProgression progression, out string perk)
        {
            perk = null;
            return receipt != null && OccultRecipeRequirements.TryGetValue(receipt.Id, out perk) &&
                !HasOccultCapability(progression, perk);
        }

        private static bool _occultPickerReasonShown;
        private static ItemReceiptPanel _occultPickerReasonOwner;

        private static void ShowOccultPickerReason(string perk, ItemReceiptPanel owner)
        {
            string titleTag = "mgperk." + perk + ".name";
            string title = Localization.HasKey(titleTag) ? Localization.Get(titleTag) : OccultResearchTitle(perk);
            var factory = SingletonMonoBehaviour<TooltipFactory>.Instance;
            factory.HideTooltip();
            factory.ShowSimpleTextTooltip(OccultUiText("requires_research") + ": " + title +
                "\n" + OccultUiText("known_pattern"));
            factory.SetSimpleTextTooltipColor(Colors.LightRed);
            _occultPickerReasonOwner = owner;
            _occultPickerReasonShown = true;
        }

        private static void HideOccultPickerReason()
        {
            if (!_occultPickerReasonShown) return;
            SingletonMonoBehaviour<TooltipFactory>.Instance?.HideSimpleTextTooltip();
            _occultPickerReasonShown = false;
            _occultPickerReasonOwner = null;
        }

        [HarmonyPatch(typeof(ItemReceiptPanel), nameof(ItemReceiptPanel.Initialize),
            [typeof(MagnumCargo), typeof(MagnumProgression), typeof(MagnumProjects),
             typeof(ItemProduceReceipt), typeof(Difficulty)])]
        private static class ShowOccultReceiptAvailability
        {
            private static void Prefix(ItemReceiptPanel __instance) =>
                __instance.GetComponent<OccultReceiptAvailability>()?.Restore();

            private static void Postfix(ItemReceiptPanel __instance, MagnumProgression __1,
                ItemProduceReceipt __3)
            {
                if (!MissingOccultCapability(__3, __1, out _)) return;
                var availability = __instance.GetComponent<OccultReceiptAvailability>() ??
                    __instance.gameObject.AddComponent<OccultReceiptAvailability>();
                availability.MarkUnavailable();
            }
        }

        private sealed class OccultReceiptAvailability : MonoBehaviour, IPointerExitHandler
        {
            private CanvasGroup _group;
            private float _originalAlpha;
            private bool _changed;

            public void Restore()
            {
                if (_changed && _group != null) _group.alpha = _originalAlpha;
                _changed = false;
            }

            public void MarkUnavailable()
            {
                _group ??= GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
                _originalAlpha = _group.alpha;
                _group.alpha *= 0.45f;
                _changed = true;
                // Keep native hover and selection so the requirement can be read.
                // The normal Start menu is replaced by its explanation below.
            }

            public void OnPointerExit(PointerEventData eventData)
            {
                if (_occultPickerReasonOwner == GetComponent<ItemReceiptPanel>()) HideOccultPickerReason();
            }

            private void OnDisable()
            {
                Restore();
                if (_occultPickerReasonOwner == GetComponent<ItemReceiptPanel>()) HideOccultPickerReason();
            }
        }

        [HarmonyPatch(typeof(MagnumSelectItemToProduceWindow), "ReceiptPanelOnStartProduction")]
        private static class ExplainOccultPickerRequirement
        {
            [HarmonyPriority(Priority.First)]
            private static bool Prefix(ItemReceiptPanel panel, MagnumProgression ____magnumSpaceship,
                ref ItemProduceReceipt ____receiptToProduce)
            {
                HideOccultPickerReason();
                if (!MissingOccultCapability(panel?.Receipt, ____magnumSpaceship, out string perk)) return true;
                ____receiptToProduce = null;
                panel.MakeSelected(false);
                ShowOccultPickerReason(perk, panel);
                return false;
            }
        }

        [HarmonyPatch(typeof(MagnumSelectItemToProduceWindow), "ContextMenuOnCmdSelected")]
        private static class RecheckOccultPickerRequirement
        {
            [HarmonyPriority(Priority.First)]
            private static bool Prefix(MagnumProgression ____magnumSpaceship,
                ref ItemProduceReceipt ____receiptToProduce, List<ItemReceiptPanel> ____receiptPanels)
            {
                if (!MissingOccultCapability(____receiptToProduce, ____magnumSpaceship, out string perk)) return true;
                var pending = ____receiptToProduce;
                var owner = ____receiptPanels.FirstOrDefault(panel => panel.Receipt == pending);
                ____receiptToProduce = null;
                UI.Back();
                ShowOccultPickerReason(perk, owner);
                // Do not emit the native success sound or ItemSelectedToProduce
                // event when a pending order can no longer be started.
                return false;
            }
        }

        [HarmonyPatch(typeof(MagnumSelectItemToProduceWindow), "OnDisable")]
        private static class ClearOccultPickerRequirement
        {
            private static void Prefix() => HideOccultPickerReason();
        }

        private static IEnumerable<(string Label, string Value)> OccultCapabilityRows(string id)
        {
            switch (id)
            {
                case OccultRankOne:
                    yield return (OccultUiText("culture"), OccultUiItemName(RawClone, "Raw Clone"));
                    yield return (OccultUiText("containment"), OccultUiItemName(TormentImprint, "Torment Imprint"));
                    break;
                case OccultRankTwo:
                    yield return (OccultUiText("separation"), OccultUiItemName(SufferingEssence, "Essence of Suffering"));
                    yield return (OccultUiText("condensation"), OccultUiItemName(NativeGavvakh, "Gavvakh"));
                    break;
                case OccultRankThree:
                    yield return (OccultUiText("materials"), OccultUiText("agga"));
                    yield return (OccultUiText("materials"), OccultUiText("gannix"));
                    yield return (OccultUiText("materials"), OccultUiText("shartamakhum"));
                    break;
                case OccultRankFour:
                    yield return (OccultUiText("materials"), OccultUiText("duggur_ron"));
                    yield return (OccultUiText("relics"), OccultUiText("scrivnus"));
                    break;
                case OccultRankFive:
                    yield return (OccultUiText("materials"), OccultUiText("ur_cattaram"));
                    yield return (OccultUiText("spores"), OccultUiText("flauros"));
                    yield return (OccultUiText("body_parts"), OccultUiText("mars_replacements"));
                    break;
                case OccultRankSix:
                    yield return (OccultUiText("soulforge"), OccultUiText("advanced_occult"));
                    yield return (OccultUiText("augmentation"), OccultUiText("quasi_implants"));
                    break;
            }
            yield return (OccultUiText("recipe_knowledge"), OccultUiText("acquired_in_game"));
        }

        // The native builder still owns the caption, status, purchase costs and
        // pooled tooltip layout. These rows describe capacity, never recipe grants.
        [HarmonyPatch(typeof(TooltipFactory), nameof(TooltipFactory.BuildMagnumPerkTooltip))]
        private static class AddOccultCapabilityTooltip
        {
            private static void Postfix(TooltipFactory __instance, MagnumPerkRecord record,
                bool isPerkPurchased, RectTransform[] ____itemsGridBlocks)
            {
                if (record?.ModuleId != OccultModule || !OccultRanks.Contains(record.Id)) return;

                foreach (var row in OccultCapabilityRows(record.Id))
                    __instance.AddPanelToTooltip().SetIcon("common_datadisk")
                        .SetName(row.Label).SetValue(row.Value);

                // Prices remain the game's actual cargo/required icons, below the
                // capability details. Native LateUpdate sizes the completed popup.
                if (!isPerkPurchased && ____itemsGridBlocks != null && ____itemsGridBlocks.Length > 0)
                    ____itemsGridBlocks[0]?.SetAsLastSibling();
            }
        }

        [HarmonyPatch(typeof(SpaceshipScreen), "InitModulePoints")]
        private static class BindOccultPipAnchors
        {
            private static void Postfix(Spaceship ____spaceship, List<MagnumModulePoint> ____modulePoints)
            {
                // Use the active native list, not inactive pooled points with stale
                // module IDs. This runs once whenever the native screen is rebuilt.
                foreach (var point in ____modulePoints)
                {
                    if (point.ModuleId != OccultModule) continue;
                    var placement = point.GetComponent<OccultPipPlacement>() ??
                        point.gameObject.AddComponent<OccultPipPlacement>();
                    placement.Configure(____spaceship, point._captionBlock, point._button);
                }
            }
        }

        private sealed class OccultPipPlacement : MonoBehaviour
        {
            private Spaceship _ship;
            private Transform _engineering;
            private Transform _research;
            private RectTransform _caption;
            private CommonButton _button;
            private bool _configured;

            public void Configure(Spaceship ship, RectTransform caption, CommonButton button)
            {
                _ship = ship;
                _caption = caption;
                _button = button;
                _engineering = null;
                _research = null;
                _configured = true;

                if (ship == null) return;
                ship.UiAnchors.TryGetValue("engineer", out _engineering);
                var candidates = ship.ModuleIds
                    .Where(id => !string.IsNullOrEmpty(id) && !id.StartsWith("iom_", StringComparison.Ordinal) &&
                        ship.UiAnchors.ContainsKey(id) && IsResearchAnchor(id)).Distinct().ToArray();
                if (candidates.Length == 1)
                    _research = ship.UiAnchors[candidates[0]];

                if (_engineering == null || _research == null)
                    Debug.LogWarning("[Cvar_ImperiumOfMan] Occult midpoint anchor unavailable; native modules: " +
                        string.Join(", ", ship.ModuleIds.Where(id => !string.IsNullOrEmpty(id) &&
                            !id.StartsWith("iom_", StringComparison.Ordinal))));
                else
                    Debug.Log("[Cvar_ImperiumOfMan] Occult Research midpoint: engineer / " + candidates[0]);
                Place();
            }

            public void Place()
            {
                if (!_configured || _ship == null || _ship.Camera == null ||
                    _engineering == null || _research == null)
                {
                    // Native SnapToWorldBone sets the Engineering projection before
                    // calling this method. Keep the new tab accessible if a future
                    // game version removes or renames the Research anchor.
                    if (_ship != null && _ship.Camera != null && _engineering != null &&
                        RectTransformUtility.ScreenPointToLocalPointInRectangle(UI.ScreenRoot,
                            RectTransformUtility.WorldToScreenPoint(_ship.Camera, _engineering.position),
                            null, out Vector2 fallbackPoint))
                        transform.localPosition = new Vector3(fallbackPoint.x + 90f, fallbackPoint.y + 45f, 0f);
                    else
                        transform.localPosition += new Vector3(90f, 45f, 0f);
                    return;
                }

                // Project the world anchors directly in this frame. Reading the
                // other UI points here would depend on their LateUpdate order.
                Vector2 engineering = RectTransformUtility.WorldToScreenPoint(_ship.Camera, _engineering.position);
                Vector2 research = RectTransformUtility.WorldToScreenPoint(_ship.Camera, _research.position);
                Vector2 midpoint = (engineering + research) * 0.5f;
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(UI.ScreenRoot,
                    midpoint, null, out Vector2 localPoint))
                    transform.localPosition = localPoint;

                if (_caption == null || _button == null) return;
                Vector3 viewport = _ship.Camera.ScreenToViewportPoint(midpoint);
                int horizontal = viewport.x > 0.5f ? 1 : -1;
                int vertical = viewport.y > 0.5f ? 1 : -1;
                Vector2 buttonSize = _button.background.rectTransform.sizeDelta;
                _caption.localPosition = new Vector3(
                    horizontal * (_caption.sizeDelta.x - buttonSize.x) * 0.5f,
                    vertical * (_caption.sizeDelta.y + buttonSize.y) * 0.5f, 0f);
            }

            private static bool IsResearchAnchor(string id)
            {
                // Match an entry that actually exists in the installed anchor map;
                // never insert a guessed native module ID. Localized labels cover
                // native keys that do not themselves name the Research module.
                if (id.IndexOf("research", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    id.IndexOf("science", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    id.IndexOf("scient", StringComparison.OrdinalIgnoreCase) >= 0) return true;
                string tag = "ui.magnum.module." + id + ".name";
                if (!Localization.HasKey(tag)) return false;
                string label = Localization.Get(tag).Trim();
                return label.Equals("Research", StringComparison.OrdinalIgnoreCase) ||
                    label.Equals("Исследования", StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}

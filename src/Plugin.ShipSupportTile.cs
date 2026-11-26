using System;
using System.Globalization;
using MGSC;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        // The research artwork identifies the call-in; its remaining uses are the only
        // text on the tile. Disabled tiles retain their raycast so they can explain why.
        private sealed class SupportCallInTile : CommonButton, IPointerEnterHandler, IPointerExitHandler
        {
            private Image _frame;
            private Image _background;
            private Image _icon;
            private Image _badge;
            private Text _count;
            private Action _onClick;
            private Action<string> _showTooltip;
            private string _caption;
            private int _remaining;
            private bool _available;
            private bool _hovered;

            public override void Awake()
            {
                EnsureVisuals();
                base.Awake();
            }

            public void Configure(Sprite icon, string caption, Action onClick, Action<string> showTooltip)
            {
                EnsureVisuals();
                ClearHover();
                _caption = caption;
                _onClick = onClick;
                _showTooltip = showTooltip;
                _icon.sprite = icon;
                _icon.preserveAspect = true;
                _icon.enabled = icon != null;
                RefreshVisuals();
            }

            public void SetCharges(int remaining, bool available)
            {
                EnsureVisuals();
                remaining = Math.Max(0, remaining);
                bool interactable = available && remaining > 0;
                if (_remaining == remaining && _available == interactable)
                    return;
                _remaining = remaining;
                _available = interactable;
                RefreshVisuals();
                if (_hovered)
                    ShowStatus();
            }

            public void ClearHover()
            {
                if (_frame != null)
                    base.OnPointerExit(null);
                if (!_hovered)
                    return;
                _hovered = false;
                _showTooltip?.Invoke(null);
                RefreshVisuals();
            }

            void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
            {
                base.OnPointerEnter(eventData);
                ShowHover();
            }

            void IPointerExitHandler.OnPointerExit(PointerEventData eventData) => ClearHover();

            public override void Select()
            {
                base.Select();
                ShowHover();
            }

            public override void Diselect()
            {
                base.Diselect();
                ClearHover();
            }

            private void ShowHover()
            {
                _hovered = true;
                RefreshVisuals();
                ShowStatus();
            }

            public override void OnDisable()
            {
                ClearHover();
                base.OnDisable();
            }

            private void ShowStatus()
            {
                string status = _remaining == 0 ? "0 remaining · Exhausted" :
                    _remaining.ToString(CultureInfo.InvariantCulture) + " remaining" +
                    (_available ? string.Empty : " · Unavailable now");
                _showTooltip?.Invoke(_caption + "\n" + status);
            }

            private void EnsureVisuals()
            {
                if (_frame != null)
                    return;
                _frame = gameObject.AddComponent<Image>();
                _frame.raycastTarget = true;
                background = _frame;
                // CommonButton supplies the game's navigation, click sound and input
                // filtering. Null sprites leave our solid-colour frame intact.
                normalBgSprite = hoverBgSprite = pressedBgSprite = disabledBgSprite = null;
                _background = TileImage("Inset", 1f);
                _icon = TileImage("Research icon", 4f);

                var badgeRect = CreateSupportRect(transform, "Remaining uses", Vector2.one,
                    new Vector2(20f, 19f), new Vector2(-2f, -2f));
                badgeRect.pivot = Vector2.one;
                _badge = badgeRect.gameObject.AddComponent<Image>();
                _badge.raycastTarget = false;
                var countRect = CreateSupportRect(badgeRect, "Count", Vector2.zero, Vector2.zero, Vector2.zero);
                countRect.anchorMax = Vector2.one;
                countRect.offsetMin = countRect.offsetMax = Vector2.zero;
                _count = countRect.gameObject.AddComponent<Text>();
                _count.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                _count.fontSize = 15;
                _count.fontStyle = FontStyle.Bold;
                _count.alignment = TextAnchor.MiddleCenter;
                _count.resizeTextForBestFit = false;
                _count.raycastTarget = false;

                OnClick += (_, _) =>
                {
                    if (!_available || _remaining <= 0)
                        return;
                    ClearHover();
                    _onClick?.Invoke();
                };
                RefreshVisuals();
            }

            private Image TileImage(string name, float inset)
            {
                var rect = CreateSupportRect(transform, name, Vector2.zero, Vector2.zero, Vector2.zero);
                rect.anchorMax = Vector2.one;
                rect.offsetMin = new Vector2(inset, inset);
                rect.offsetMax = new Vector2(-inset, -inset);
                var image = rect.gameObject.AddComponent<Image>();
                image.raycastTarget = false;
                return image;
            }

            private void RefreshVisuals()
            {
                if (_count == null)
                    return;
                bool exhausted = _remaining == 0;
                SetInteractable(_available && !exhausted, refreshVisual: false);
                _frame.color = _hovered ? new Color32(139, 163, 137, 255) :
                    exhausted ? new Color32(35, 48, 39, 255) : new Color32(75, 101, 78, 255);
                _background.color = exhausted ? new Color32(8, 15, 11, 255) :
                    _available ? new Color32(28, 43, 36, 255) : new Color32(16, 29, 23, 255);
                _icon.color = exhausted ? new Color(0.28f, 0.31f, 0.28f, 1f) :
                    _available ? Color.white : new Color(0.62f, 0.66f, 0.62f, 1f);
                _badge.color = new Color32(12, 22, 16, 250);
                _count.color = exhausted ? new Color32(139, 163, 137, 255) :
                    new Color32(197, 214, 189, 255);
                _count.text = _remaining.ToString(CultureInfo.InvariantCulture);
                _badge.rectTransform.sizeDelta = new Vector2(Math.Max(20f, 9f * _count.text.Length + 6f), 19f);
            }
        }
    }
}

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private sealed partial class CraftingMapWindow
        {
            // Session preference stays with the map when native production
            // windows recreate its owner. Graph zoom remains independent.
            private static int _interfacePercent = 100;
            private static bool _inspectorVisible = true;
            private int _viewportWidth, _viewportHeight;
            private CraftingViewportLayout _viewportLayout;
            private RectTransform _graphFrame, _graphTools, _details, _inputColumn, _outputColumn;
            private RectTransform _detailNoteRoot, _detailNoteContent;
            private TextMeshProUGUI _uiScaleLabel, _inspectorToggle;
            private bool _detailHasSources;

            private void ChangeInterfaceScale(int delta)
            {
                int next = Mathf.Clamp(_interfacePercent + delta,
                    CraftingViewportLayout.MinUiPercent, CraftingViewportLayout.MaxUiPercent);
                if (next == _interfacePercent) return;
                _interfacePercent = next;
                ApplyViewport(false);
            }

            private void ToggleInspector()
            {
                _inspectorVisible = !_inspectorVisible;
                ApplyViewport(false);
                if (_inspectorVisible) DrawDetails();
            }

            private void ApplyViewport(bool fit)
            {
                _viewportWidth = Screen.width;
                _viewportHeight = Screen.height;
                if (_viewportWidth <= 0 || _viewportHeight <= 0 || _graphFrame == null || _details == null)
                    return;
                var layout = _viewportLayout = CraftingViewportLayout.Calculate(
                    _viewportWidth, _viewportHeight, _interfacePercent, _inspectorVisible);
                GetComponent<CanvasScaler>().scaleFactor = layout.Scale;
                GetComponent<Canvas>().scaleFactor = layout.Scale;
                _uiScaleLabel.text = _interfacePercent + "%";
                _inspectorToggle.text = _inspectorVisible ? "HIDE INFO" : "SHOW INFO";
                _graphTools.anchoredPosition = layout.SharedTools
                    ? new Vector2(CraftingViewportLayout.FilterRight + 8, -36) : new Vector2(8, -68);
                _graphFrame.offsetMin = new Vector2(8, layout.GraphBottom);
                _graphFrame.offsetMax = new Vector2(-layout.GraphRight, -layout.GraphTop);

                _details.gameObject.SetActive(_inspectorVisible);
                if (_inspectorVisible)
                {
                    if (layout.Side)
                    {
                        _details.anchorMin = new Vector2(1, 0);
                        _details.anchorMax = Vector2.one;
                        _details.offsetMin = new Vector2(-layout.InspectorWidth - 8, 8);
                        _details.offsetMax = new Vector2(-8, -layout.GraphTop);
                    }
                    else
                    {
                        _details.anchorMin = Vector2.zero;
                        _details.anchorMax = new Vector2(1, 0);
                        _details.offsetMin = new Vector2(8, 8);
                        _details.offsetMax = new Vector2(-8, layout.InspectorHeight + 8);
                    }
                }

                // Recalculate slide distance after resolution/scale changes.
                float eased = _slide * _slide * (3f - 2f * _slide);
                _panel.anchoredPosition = _openPanelPosition + new Vector2(
                    -(1f - eased) * (layout.Width + 40f), 0f);
                Canvas.ForceUpdateCanvases();
                LayoutInspector();
                if (_nodes.Count == 0) return;
                DrawDetailRows();
                if (_searchPanel.gameObject.activeSelf) DrawSearchResults();
                if (fit) _view.Fit(); else _view.RefreshVisuals();
            }

            private void SetDetailNote(string text)
            {
                _detailNote.text = (text ?? "").Trim();
                LayoutInspector();
            }

            private void LayoutInspector()
            {
                if (_viewportLayout == null || !_inspectorVisible || _details == null) return;
                bool side = _viewportLayout.Side;
                float width = _viewportLayout.InspectorWidth, height = _viewportLayout.InspectorHeight;
                bool meta = !string.IsNullOrEmpty(_detailMeta.text);
                _detailMeta.gameObject.SetActive(meta);
                var source = (RectTransform)_acquisitionToggle.transform.parent;
                if (side)
                {
                    PlaceSpan(_detailTitle.rectTransform, 0, 1, 3, 26, 8, _detailHasSources ? 94 : 8);
                    PlaceSpan(source, 1, 1, 3, 26, -88, 4);
                    PlaceSpan(_detailMeta.rectTransform, 0, 1, 32, 20, 8, 8);
                    _detailMeta.alignment = TextAlignmentOptions.MidlineLeft;
                }
                else
                {
                    float right = meta ? 0.70f : 1f;
                    PlaceSpan(_detailTitle.rectTransform, 0, right, 3, 26, 8, _detailHasSources ? 94 : 8);
                    PlaceSpan(source, right, right, 3, 26, -88, 4);
                    PlaceSpan(_detailMeta.rectTransform, 0.70f, 1, 3, 26, 4, 8);
                    _detailMeta.alignment = TextAlignmentOptions.MidlineRight;
                }

                bool note = !string.IsNullOrWhiteSpace(_detailNote.text);
                _detailNoteRoot.gameObject.SetActive(note);
                float noteHeight = 0;
                if (note)
                {
                    float preferred = _detailNote.GetPreferredValues(_detailNote.text,
                        Math.Max(48f, width - 44f), 10000f).y + 8f;
                    float cap = side ? Math.Min(100f, height * 0.22f) : Math.Min(48f, height * 0.28f);
                    noteHeight = Math.Max(22f, Math.Min(cap, preferred));
                    _detailNoteRoot.offsetMin = new Vector2(8, 6);
                    _detailNoteRoot.offsetMax = new Vector2(-8, 6 + noteHeight - height);
                    _detailNoteContent.sizeDelta = new Vector2(0, preferred);
                    _detailNoteContent.anchoredPosition = Vector2.zero;
                    PlaceSpan(_detailNote.rectTransform, 0, 1, 3, preferred - 5f, 5, 5);
                }

                float top = side && meta ? 56 : 36;
                float space = Math.Max(48, height - top - 6f - (note ? noteHeight + 4 : 0));
                if (side)
                {
                    float first = (space - 6f) / 2f;
                    PlaceSpan(_inputColumn, 0, 1, top, first, 8, 8);
                    PlaceSpan(_outputColumn, 0, 1, top + first + 6, space - first - 6, 8, 8);
                }
                else
                {
                    PlaceSpan(_inputColumn, 0, 0.5f, top, space, 8, 4);
                    PlaceSpan(_outputColumn, 0.5f, 1, top, space, 4, 8);
                }
                Canvas.ForceUpdateCanvases();
            }

            private static void PlaceSpan(RectTransform rect, float min, float max,
                float top, float height, float left, float right)
            {
                rect.anchorMin = new Vector2(min, 1);
                rect.anchorMax = new Vector2(max, 1);
                rect.offsetMin = new Vector2(left, -top - height);
                rect.offsetMax = new Vector2(-right, -top);
            }

            private static string RecipeDisplayName(CraftingRecipe recipe)
            {
                string name = recipe.Name ?? "";
                if (name.StartsWith("Produce ", StringComparison.Ordinal)) return name.Substring(8);
                if (name.StartsWith("Craft ", StringComparison.Ordinal)) return name.Substring(6);
                return name;
            }
        }
    }
}

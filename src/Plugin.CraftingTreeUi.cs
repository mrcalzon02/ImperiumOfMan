using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MGSC;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private static CraftingMapWindow _craftingMapWindow;
        private static int _craftingMapClosedFrame = -1;

        // A production-window tab, not another permanent ship module.
        private static void EnsureCraftingMapTab(Component productionOwner)
        {
            if (productionOwner == null)
                return;
            if (_craftingMapWindow != null)
            {
                _craftingMapWindow.SetOwner(productionOwner);
                return;
            }
            var root = new GameObject("Imperium Crafting Map", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 29000;
            canvas.pixelPerfect = true;
            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.matchWidthOrHeight = 0.5f;
            _craftingMapWindow = root.AddComponent<CraftingMapWindow>();
            _craftingMapWindow.Initialize(productionOwner);
        }

        private static void RemoveCraftingMapTab(Component productionOwner = null)
        {
            if (_craftingMapWindow == null || productionOwner != null && !_craftingMapWindow.IsOwner(productionOwner))
                return;
            var window = _craftingMapWindow;
            _craftingMapWindow = null;
            window.CloseForOwner();
            UnityEngine.Object.Destroy(window.gameObject);
        }

        // Native SpaceGameMode ticks the game before UI.Process handles navigation and hotkeys.
        // Suppress only that UI processing while our expanded map owns focus.
        [HarmonyPatch(typeof(MGSC.UI), nameof(MGSC.UI.Process))]
        private static class CraftingMapNativeInput
        {
            private static bool Prefix(ref bool interruptProcessing)
            {
                if ((_craftingMapWindow == null || !_craftingMapWindow.BlocksNativeInput) &&
                    _craftingMapClosedFrame != Time.frameCount)
                    return true;
                interruptProcessing = true;
                return false;
            }
        }

        private sealed partial class CraftingMapWindow : MonoBehaviour
        {
            private const int MapNodeLimit = 240;
            private const int MapEdgeLimit = 2400;
            private const int BranchPage = 8;
            private const int SearchPage = 10;
            private const int DetailPage = 14;
            // Match the native Production window: dark glass, green rules and yellow focus marks.
            private static readonly Color PanelColor = new Color32(10, 14, 12, 248);
            private static readonly Color RecessColor = new Color32(4, 10, 8, 244);
            private static readonly Color ButtonColor = new Color32(8, 19, 14, 255);
            private static readonly Color SelectedColor = new Color32(36, 75, 64, 255);
            private static readonly Color BorderColor = new Color32(58, 121, 102, 255);
            private static readonly Color TextColor = new Color32(129, 181, 122, 255);
            private static readonly Color DimColor = new Color32(112, 146, 123, 255);
            private static readonly Color AccentColor = new Color32(255, 253, 184, 255);
            private Component _owner;
            private GameObject _previousSelection;
            private TMP_FontAsset _font;
            private RectTransform _panel;
            private Vector2 _openPanelPosition;
            private GameObject _collapsedTab;
            private Image _modalBlocker;
            private bool _expanded;
            private float _slide;
            private CraftingIndex _index;
            private List<CraftingItem> _allItems = new List<CraftingItem>();
            private readonly Dictionary<string, CraftingRecipe> _recipes = new Dictionary<string, CraftingRecipe>();
            private readonly Dictionary<string, MapNode> _nodes = new Dictionary<string, MapNode>();
            private readonly List<MapEdge> _edges = new List<MapEdge>();
            private readonly Dictionary<string, MapEdge> _edgeLookup = new Dictionary<string, MapEdge>();
            private readonly HashSet<string> _highlightNodes = new HashSet<string>();
            private string _selectedKey;
            private string _rootItem;
            private string _venue = "All";
            private readonly Dictionary<string, Button> _venueButtons = new Dictionary<string, Button>();
            private CraftingMapGraphic _view;
            private TextMeshProUGUI _mapStatus;
            private TextMeshProUGUI _catalogStatus;
            private TextMeshProUGUI _zoomLabel;
            private TMP_InputField _search;
            private RectTransform _searchPanel;
            private RectTransform _searchRows;
            private TextMeshProUGUI _searchCount;
            private int _searchPage;
            private List<CraftingItem> _matches = new List<CraftingItem>();
            private TextMeshProUGUI _detailTitle;
            private TextMeshProUGUI _detailMeta;
            private TextMeshProUGUI _detailNote;
            private TextMeshProUGUI _acquisitionToggle;
            private bool _showAcquisition;
            private int _selectedAcquisition;
            private RectTransform _inputRows;
            private RectTransform _outputRows;
            private TextMeshProUGUI _inputTitle;
            private TextMeshProUGUI _outputTitle;
            private int _inputPage;
            private int _outputPage;
            private List<DetailRow> _inputDetails = new List<DetailRow>();
            private List<DetailRow> _outputDetails = new List<DetailRow>();
            private TextMeshProUGUI _expandInputsLabel;
            private TextMeshProUGUI _expandUsesLabel;

            public bool BlocksNativeInput => _expanded || _slide > 0.001f;
            public bool IsOwner(Component owner) => _owner == owner;
            public void SetOwner(Component owner) => _owner = owner;

            public void Initialize(Component owner)
            {
                _owner = owner;
                _font = Localization.GetActualFont();
                // Normally supplied by Quasimorph. Own a fallback only if none exists.
                if (EventSystem.current == null)
                {
                    gameObject.AddComponent<EventSystem>();
                    gameObject.AddComponent<StandaloneInputModule>();
                }
                var blocker = Stretch(transform, "Map input blocker", 0, 0, 0, 0);
                _modalBlocker = blocker.gameObject.AddComponent<Image>();
                _modalBlocker.color = new Color(0f, 0f, 0f, 0.24f);
                _modalBlocker.gameObject.SetActive(false);
                _panel = Stretch(transform, "Crafting map panel", 14, 24, 52, 24);
                Paint(_panel, PanelColor);
                Frame(_panel, BorderColor, true);
                _openPanelPosition = _panel.anchoredPosition;
                BuildHeader();
                BuildCanvas();
                BuildDetails();
                BuildSearchResults();

                var handle = EdgeTab(_panel, "Collapse crafting map", false, Collapse);
                handle.anchorMin = handle.anchorMax = new Vector2(1f, 0.62f);
                handle.pivot = new Vector2(0f, 0.5f);
                handle.anchoredPosition = Vector2.zero;
                _collapsedTab = EdgeTab(transform, "Open crafting map", true, Expand).gameObject;
                _panel.gameObject.SetActive(false);
            }

            private RectTransform EdgeTab(Transform parent, string name, bool opening, Action action)
            {
                var rect = Rect(parent, name, new Vector2(0f, 0.62f), new Vector2(0f, 0.62f));
                rect.pivot = new Vector2(0f, 0.5f);
                rect.sizeDelta = new Vector2(38f, 184f);
                Button(rect, name + " button", action);
                var captionRect = Rect(rect, "Crafting map label", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
                captionRect.sizeDelta = new Vector2(148f, 24f);
                var caption = Text(captionRect, "CRAFTING MAP", 16);
                caption.alignment = TextAlignmentOptions.Midline;
                caption.rectTransform.localEulerAngles = new Vector3(0f, 0f, 90f);
                var arrow = Text(At(rect, "Direction", 5f, 7f, 28f, 30f), opening ? ">" : "<", 19);
                arrow.alignment = TextAlignmentOptions.Midline;
                return rect;
            }

            private void BuildHeader()
            {
                var heading = Span(_panel, "Production-style heading", 0, 1, 0, 38);
                Paint(heading, new Color32(15, 16, 16, 255));
                Frame(heading, TextColor, true);
                Text(At(heading, "Title", 12f, 3f, 240f, 30f), "CRAFTING MAP", 20);
                _catalogStatus = Text(Span(_panel, "Catalogue status", 0.23f, 0.66f, 12f, 22f),
                    "Open the map to explore recipes", 12);
                var refresh = AtRight(_panel, "Refresh", 92f, 5f, 88f, 28f);
                TextButton(refresh, "REFRESH", () => RefreshIndex(true));
                TextButton(AtRight(_panel, "Close", 8f, 5f, 76f, 28f), "CLOSE", Collapse);

                var searchRect = At(_panel, "Find item", 14f, 46f, 286f, 32f);
                Paint(searchRect, RecessColor);
                Frame(searchRect, BorderColor);
                _search = searchRect.gameObject.AddComponent<TMP_InputField>();
                var viewport = Stretch(searchRect, "Search viewport", 9, 3, 9, 3);
                viewport.gameObject.AddComponent<RectMask2D>();
                var inputText = Text(Stretch(viewport, "Search text", 0, 0, 0, 0), "", 14);
                inputText.alignment = TextAlignmentOptions.MidlineLeft;
                inputText.enableWordWrapping = false;
                inputText.overflowMode = TextOverflowModes.Overflow;
                var placeholder = Text(Stretch(viewport, "Search hint", 0, 0, 0, 0), "FIND ITEM  [CTRL+F]", 14);
                placeholder.color = DimColor;
                _search.textViewport = viewport;
                _search.textComponent = inputText;
                _search.placeholder = placeholder;
                _search.targetGraphic = searchRect.GetComponent<Image>();
                _search.lineType = TMP_InputField.LineType.SingleLine;
                _search.richText = false;
                _search.customCaretColor = true;
                _search.caretColor = AccentColor;
                _search.caretWidth = 2;
                _search.selectionColor = new Color32(58, 121, 102, 170);
                _search.onValueChanged.AddListener(_ => { _searchPage = 0; FindItems(); });
                var events = searchRect.gameObject.AddComponent<EventTrigger>();
                var select = new EventTrigger.Entry { eventID = EventTriggerType.Select };
                select.callback.AddListener(_ => { _searchPage = 0; FindItems(); });
                events.triggers.Add(select);

                string[] venues = { "All", "Magnum", "Workbench", "Station", "Recycling" };
                string[] captions = { "ALL", "MAGNUM", "WORKBENCH", "STATIONS", "RECOVERY" };
                for (int i = 0; i < venues.Length; i++)
                {
                    string venue = venues[i];
                    var rect = At(_panel, venue + " source", 310f + 85f * i, 46f, 80f, 32f);
                    TextButton(rect, captions[i], () => SetVenue(venue), 12);
                    _venueButtons.Add(venue, rect.GetComponent<Button>());
                }
                MarkVenue();
            }

            private void BuildCanvas()
            {
                var frame = Stretch(_panel, "Graph frame", 14, 204, 14, 88);
                Paint(frame, RecessColor);
                Frame(frame, BorderColor, true);
                frame.gameObject.AddComponent<RectMask2D>();
                var graphViewport = Stretch(frame, "Graph viewport", 2, 36, 2, 29);
                graphViewport.gameObject.AddComponent<RectMask2D>();
                var graphicRect = Stretch(graphViewport, "Connected crafting graph", 0, 0, 0, 0);
                _view = graphicRect.gameObject.AddComponent<CraftingMapGraphic>();
                _view.Initialize(this);
                var controls = Rect(frame, "Map controls", new Vector2(0f, 0f), new Vector2(1f, 0f));
                controls.pivot = new Vector2(0.5f, 0f);
                controls.sizeDelta = new Vector2(0f, 34f);
                Paint(controls, new Color32(10, 18, 14, 255));
                Frame(controls, BorderColor);
                TextButton(At(controls, "Zoom out", 6, 4, 28, 26), "-", () => _view.ZoomAtCenter(1f / 1.2f), 18);
                _zoomLabel = Text(At(controls, "Zoom amount", 38, 4, 48, 26), "100%", 12);
                _zoomLabel.alignment = TextAlignmentOptions.Midline;
                TextButton(At(controls, "Zoom in", 90, 4, 28, 26), "+", () => _view.ZoomAtCenter(1.2f), 18);
                TextButton(At(controls, "Fit map", 126, 4, 44, 26), "FIT", () => _view.Fit(), 12);
                TextButton(At(controls, "Focus node", 178, 4, 60, 26), "FOCUS", FocusSelection, 12);
                _expandInputsLabel = TextButton(At(controls, "Expand inputs", 248, 4, 142, 26),
                    "EXPAND INPUTS", () => ExpandSelection(true), 12);
                _expandUsesLabel = TextButton(At(controls, "Expand uses", 398, 4, 142, 26),
                    "EXPAND USES", () => ExpandSelection(false), 12);
                TextButton(At(controls, "Trace ingredients", 550, 4, 172, 26), "INGREDIENT TREE", TraceSelectedIngredients, 12);
                var status = Span(frame, "Graph information strip", 0, 1, 1, 27, 1, 1);
                Paint(status, new Color32(10, 18, 14, 255));
                _mapStatus = Text(Stretch(status, "Map status", 9, 2, 9, 2),
                    "Drag to pan • wheel to zoom • click to highlight", 12);
                _mapStatus.color = DimColor;
            }

            private void BuildDetails()
            {
                var details = Rect(_panel, "Selected node details", new Vector2(0f, 0f), new Vector2(1f, 0f));
                details.pivot = new Vector2(0.5f, 0f);
                details.offsetMin = new Vector2(14, 12);
                details.offsetMax = new Vector2(-14, 192);
                Paint(details, PanelColor);
                Frame(details, BorderColor, true);
                var heading = Span(details, "Selected node heading", 0, 1, 0, 35);
                Paint(heading, new Color32(15, 16, 16, 255));
                Frame(heading, TextColor);
                _detailTitle = Text(Span(details, "Selected name", 0, 0.68f, 4, 27, 12, 4), "SELECT AN ITEM OR RECIPE", 18);
                _detailTitle.color = AccentColor;
                _acquisitionToggle = TextButton(Span(details, "Occult acquisition sources", 0.55f, 0.68f,
                    4, 27, 4, 4), "SOURCES", () =>
                    {
                        _showAcquisition = !_showAcquisition;
                        _selectedAcquisition = _inputPage = _outputPage = 0;
                        DrawDetails();
                    }, 12);
                _acquisitionToggle.transform.parent.gameObject.SetActive(false);
                _detailMeta = Text(Span(details, "Time and venue", 0.68f, 1f, 7, 27, 4, 12), "", 13);
                _detailMeta.alignment = TextAlignmentOptions.MidlineRight;
                var inputs = Span(details, "Input column", 0f, 0.5f, 39, 101, 12, 8);
                var outputs = Span(details, "Output column", 0.5f, 1f, 39, 101, 8, 12);
                _inputTitle = Text(Span(inputs, "Input heading", 0, 1, 0, 22, 0, 68), "INPUTS", 12);
                _outputTitle = Text(Span(outputs, "Output heading", 0, 1, 0, 22, 0, 68), "OUTPUTS", 12);
                TextButton(AtRight(inputs, "Previous inputs", 34, 0, 28, 22), "<", () => { _inputPage = Math.Max(0, _inputPage - 1); DrawDetailRows(); }, 16);
                TextButton(AtRight(inputs, "Next inputs", 0, 0, 28, 22), ">", () => { _inputPage = Math.Min(Math.Max(0, (_inputDetails.Count - 1) / DetailPage), _inputPage + 1); DrawDetailRows(); }, 16);
                TextButton(AtRight(outputs, "Previous outputs", 34, 0, 28, 22), "<", () => { _outputPage = Math.Max(0, _outputPage - 1); DrawDetailRows(); }, 16);
                TextButton(AtRight(outputs, "Next outputs", 0, 0, 28, 22), ">", () => { _outputPage = Math.Min(Math.Max(0, (_outputDetails.Count - 1) / DetailPage), _outputPage + 1); DrawDetailRows(); }, 16);
                _inputRows = Scroll(Stretch(inputs, "Input list", 0, 0, 0, 25));
                _outputRows = Scroll(Stretch(outputs, "Output list", 0, 0, 0, 25));
                _detailNote = Text(Span(details, "Selected information", 0, 1, 142, 34, 12, 12),
                    "Expand a branch to follow its recipes. Counts belong to each individual production order.", 11);
                _detailNote.color = DimColor;
            }

            private void BuildSearchResults()
            {
                _searchPanel = At(_panel, "All item search results", 14, 82, 380, 350);
                Paint(_searchPanel, PanelColor);
                Frame(_searchPanel, TextColor, true);
                _searchCount = Text(At(_searchPanel, "Results count", 8, 5, 250, 24), "", 12);
                TextButton(AtRight(_searchPanel, "Previous results", 70, 5, 28, 24), "<", () => { _searchPage = Math.Max(0, _searchPage - 1); DrawSearchResults(); }, 16);
                TextButton(AtRight(_searchPanel, "Next results", 36, 5, 28, 24), ">", () => { _searchPage = Math.Min(Math.Max(0, (_matches.Count - 1) / SearchPage), _searchPage + 1); DrawSearchResults(); }, 16);
                TextButton(AtRight(_searchPanel, "Hide results", 4, 5, 26, 24), "×", () => _searchPanel.gameObject.SetActive(false), 16);
                _searchRows = Stretch(_searchPanel, "Results", 6, 6, 6, 34);
                _searchPanel.gameObject.SetActive(false);
            }

            private void Expand()
            {
                _previousSelection = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
                _expanded = true;
                if (_slide == 0f)
                    _panel.anchoredPosition = _openPanelPosition - new Vector2(_panel.rect.width + 40f, 0f);
                _panel.gameObject.SetActive(true);
                _modalBlocker.gameObject.SetActive(true);
                _collapsedTab.SetActive(false);
                RefreshIndex(_nodes.Count > 0);
            }

            private void Collapse()
            {
                _expanded = false;
                _search.DeactivateInputField();
                _searchPanel.gameObject.SetActive(false);
                _craftingMapClosedFrame = Time.frameCount;
                RestoreSelection();
            }

            public void CloseForOwner()
            {
                _craftingMapClosedFrame = Time.frameCount;
                RestoreSelection();
            }

            private void RestoreSelection()
            {
                if (EventSystem.current != null)
                    EventSystem.current.SetSelectedGameObject(_previousSelection != null && _previousSelection.activeInHierarchy ? _previousSelection : null);
            }

            private void Update()
            {
                // Only lifecycle/input/slide animation here; never enumerate game records or cargo.
                if (_owner == null || !_owner.gameObject.activeInHierarchy)
                {
                    RemoveCraftingMapTab();
                    return;
                }
                float target = _expanded ? 1f : 0f;
                if (!Mathf.Approximately(_slide, target))
                {
                    _slide = Mathf.MoveTowards(_slide, target, Time.unscaledDeltaTime * 7f);
                    float eased = _slide * _slide * (3f - 2f * _slide);
                    _panel.anchoredPosition = _openPanelPosition + new Vector2(-(1f - eased) * (_panel.rect.width + 40f), 0f);
                    if (_slide == 0f)
                    {
                        _panel.gameObject.SetActive(false);
                        _modalBlocker.gameObject.SetActive(false);
                        _collapsedTab.SetActive(true);
                        _craftingMapClosedFrame = Time.frameCount;
                    }
                }
                if (!_expanded)
                    return;
                if (Input.GetKeyDown(KeyCode.Escape))
                    Collapse();
                else if (Input.GetKeyDown(KeyCode.F) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
                    _search.ActivateInputField();
            }

            private void OnDestroy()
            {
                if (_craftingMapWindow == this)
                    _craftingMapWindow = null;
            }

            private void RefreshIndex(bool preserveMap)
            {
                try
                {
                    var oldNodes = preserveMap ? _nodes.Values.ToArray() : new MapNode[0];
                    _index = BuildCraftingIndex();
                    _allItems = _index.Items.Values.OrderBy(item => item.Name ?? item.Id, StringComparer.OrdinalIgnoreCase).ToList();
                    _recipes.Clear();
                    foreach (var recipe in _index.Recipes)
                        _recipes[recipe.Key] = recipe;
                    bool partial = _index.IncompleteSources.Length > 0;
                    _catalogStatus.text = partial ? "Some details unavailable. Refresh to try again." :
                        _index.Items.Count + " items · " + _index.Recipes.Count + " recipes";
                    _catalogStatus.color = partial ? new Color32(220, 195, 135, 255) : DimColor;
                    if (string.IsNullOrEmpty(_rootItem) || !_index.Items.ContainsKey(_rootItem))
                        _rootItem = _index.Items.ContainsKey("iom_metal_ore") ? "iom_metal_ore" : _allItems.FirstOrDefault()?.Id;
                    if (preserveMap && oldNodes.Length > 0)
                    {
                        var oldPositions = oldNodes.ToDictionary(node => node.Key, node => node);
                        string selection = _selectedKey;
                        ClearMap();
                        if (_rootItem != null)
                            AddItem(_rootItem, 0);
                        foreach (var old in oldNodes.Where(node => node.Recipe != null))
                            if (_recipes.TryGetValue(old.Recipe.Key, out var current) && InVenue(current))
                                AddRecipe(current, old.Column);
                        foreach (var node in _nodes.Values)
                            if (oldPositions.TryGetValue(node.Key, out var previous))
                            {
                                node.Position = previous.Position;
                                node.Column = previous.Column;
                            }
                        _selectedKey = _nodes.ContainsKey(selection ?? "") ? selection : ItemKey(_rootItem);
                        Changed(false);
                    }
                    else if (_rootItem != null)
                        FocusItem(_rootItem);
                    else
                        _mapStatus.text = "No items are available. Refresh to try again.";
                }
                catch (Exception error)
                {
                    Debug.LogError("[Cvar_ImperiumOfMan] Crafting map capture failed: " + error);
                    _catalogStatus.text = "Recipes unavailable — refresh to try again";
                    _detailNote.text = "Recipes could not be refreshed. Use Refresh to try again.";
                }
            }

            private void SetVenue(string venue)
            {
                _venue = venue;
                _searchPanel.gameObject.SetActive(false);
                MarkVenue();
                if (_rootItem != null)
                    FocusItem(_rootItem);
            }

            private void MarkVenue()
            {
                foreach (var button in _venueButtons)
                {
                    bool selected = button.Key == _venue;
                    SetButtonColors(button.Value, selected);
                    var label = button.Value.GetComponentInChildren<TextMeshProUGUI>();
                    if (label != null)
                        label.color = selected ? AccentColor : TextColor;
                    button.Value.GetComponentInChildren<CraftingMapFrame>()?.SetStyle(selected ? TextColor : BorderColor, selected);
                }
            }

            private bool InVenue(CraftingRecipe recipe) => _venue == "All" || recipe.Venue == _venue;
            private static string ItemKey(string id) => "item:" + id;
            private static string RecipeKey(string id) => "recipe:" + id;
            private string Name(string id) => _index != null && _index.Items.TryGetValue(id, out var item) ? item.Name ?? CraftingItemName(id) : CraftingItemName(id);

            private void FindItems()
            {
                if (_index == null)
                    return;
                string query = (_search.text ?? "").Trim();
                _matches = _allItems.Where(item => query.Length == 0 ||
                    (item.Name ?? "").IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    item.Id.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (item.Category ?? "").IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    GroupTitle(ItemGroup(item)).IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                _searchPanel.gameObject.SetActive(true);
                _searchPanel.SetAsLastSibling();
                DrawSearchResults();
            }

            private void DrawSearchResults()
            {
                Clear(_searchRows);
                _searchPage = Math.Min(_searchPage, Math.Max(0, (_matches.Count - 1) / SearchPage));
                int start = _searchPage * SearchPage;
                _searchCount.text = _matches.Count == 0 ? "No matching items" :
                    (start + 1) + "–" + Math.Min(start + SearchPage, _matches.Count) + " / " + _matches.Count + " items";
                for (int i = start; i < Math.Min(start + SearchPage, _matches.Count); i++)
                {
                    var item = _matches[i];
                    string id = item.Id;
                    ItemButton(Span(_searchRows, id, 0, 1, (i - start) * 30f, 28f), item.Icon,
                        Name(id), () =>
                        {
                            _search.DeactivateInputField();
                            _searchPanel.gameObject.SetActive(false);
                            FocusItem(id);
                        }, 12);
                }
            }

            private void ClearMap()
            {
                _nodes.Clear();
                _edges.Clear();
                _edgeLookup.Clear();
                _layout = null;
                _layoutDirty = true;
            }

            private void FocusItem(string id)
            {
                if (_index == null || !_index.Items.ContainsKey(id))
                    return;
                _rootItem = id;
                _showAcquisition = false;
                _selectedAcquisition = _inputPage = _outputPage = 0;
                ClearMap();
                AddItem(id, 0);
                _selectedKey = ItemKey(id);
                AddBranches(id, true, BranchPage);
                AddBranches(id, false, BranchPage);
                Changed(true);
            }

            private void FocusSelection()
            {
                if (!_nodes.TryGetValue(_selectedKey ?? "", out var selected))
                    return;
                string id = selected.ItemId ?? selected.Recipe?.Outputs.FirstOrDefault()?.ItemId;
                if (id != null)
                    FocusItem(id);
            }

            private MapNode AddItem(string id, int column)
            {
                string key = ItemKey(id);
                if (_nodes.TryGetValue(key, out var existing))
                    return existing;
                _index.Items.TryGetValue(id, out var item);
                var node = new MapNode { Key = key, ItemId = id, Item = item, Column = column,
                    Position = Vector2.zero, Size = new Vector2(220, 84) };
                _nodes.Add(key, node);
                _layoutDirty = true;
                return node;
            }

            private bool AddRecipe(CraftingRecipe recipe, int column)
            {
                string key = RecipeKey(recipe.Key);
                if (_nodes.ContainsKey(key))
                    return false;
                int newItems = recipe.Inputs.Concat(recipe.Outputs).Select(x => x.ItemId).Distinct()
                    .Count(id => !_nodes.ContainsKey(ItemKey(id)));
                if (_nodes.Count + newItems + 1 > MapNodeLimit || _edges.Count + recipe.Inputs.Count + recipe.Outputs.Count > MapEdgeLimit)
                    return false;
                var node = new MapNode { Key = key, Recipe = recipe, Column = column,
                    Position = Vector2.zero, Size = new Vector2(244, 84) };
                _nodes.Add(key, node);
                _layoutDirty = true;
                foreach (var input in recipe.Inputs)
                    AddEdge(AddItem(input.ItemId, column - 1), node, input.Count);
                foreach (var output in recipe.Outputs)
                    AddEdge(node, AddItem(output.ItemId, column + 1), output.Count);
                return true;
            }

            private void AddEdge(MapNode from, MapNode to, int count)
            {
                string key = from.Key + "\n" + to.Key;
                if (_edgeLookup.TryGetValue(key, out var existing))
                {
                    long sum = (long)existing.Count + count;
                    existing.Count = existing.Count > 0 && count > 0 && sum <= int.MaxValue ? (int)sum : 0;
                    return;
                }
                var edge = new MapEdge { From = from, To = to, Count = count };
                _edges.Add(edge);
                _edgeLookup.Add(key, edge);
                _layoutDirty = true;
            }

            private List<CraftingRecipe> Routes(string id, bool inputs)
            {
                var lookup = inputs ? _index.Producers : _index.Consumers;
                return lookup.TryGetValue(id, out var routes) ? routes.Where(InVenue)
                    .OrderBy(recipe => recipe.Venue).ThenBy(recipe => recipe.Name).ThenBy(recipe => recipe.Key).ToList()
                    : new List<CraftingRecipe>();
            }

            private int AddBranches(string id, bool inputs, int maximum)
            {
                var itemNode = AddItem(id, 0);
                int added = 0;
                foreach (var recipe in Routes(id, inputs))
                {
                    if (_nodes.ContainsKey(RecipeKey(recipe.Key)))
                        continue;
                    if (AddRecipe(recipe, itemNode.Column + (inputs ? -1 : 1)))
                        added++;
                    if (added >= maximum)
                        break;
                }
                return added;
            }

            private IEnumerable<string> SelectedItems(bool inputs)
            {
                if (!_nodes.TryGetValue(_selectedKey ?? "", out var node))
                    return new string[0];
                return node.ItemId != null ? new[] { node.ItemId } :
                    (inputs ? node.Recipe.Inputs : node.Recipe.Outputs).Select(amount => amount.ItemId).Distinct();
            }

            private int RemainingBranches(bool inputs) => SelectedItems(inputs).SelectMany(id => Routes(id, inputs))
                .Where(recipe => !_nodes.ContainsKey(RecipeKey(recipe.Key))).Select(recipe => recipe.Key).Distinct().Count();

            private void ExpandSelection(bool inputs)
            {
                int added = 0;
                foreach (string id in SelectedItems(inputs).ToArray())
                {
                    added += AddBranches(id, inputs, BranchPage - added);
                    if (added >= BranchPage)
                        break;
                }
                Changed(false);
                if (added == 0 && RemainingBranches(inputs) > 0)
                    _mapStatus.text = "This branch view is full. Focus a selected item to continue; the complete item catalogue remains searchable.";
            }

            private void SelectNode(string key)
            {
                if (!_nodes.ContainsKey(key))
                    return;
                if (_selectedKey != key)
                {
                    _showAcquisition = false;
                    _selectedAcquisition = 0;
                }
                _selectedKey = key;
                _inputPage = _outputPage = 0;
                _searchPanel.gameObject.SetActive(false);
                Changed(false);
                // Selection intentionally does not move, zoom, or rebuild the graph.
            }

            private void Changed(bool fit)
            {
                ArrangeGraph();
                Highlight();
                DrawDetails();
                _expandInputsLabel.text = "EXPAND INPUTS · " + RemainingBranches(true);
                _expandUsesLabel.text = "EXPAND USES · " + RemainingBranches(false);
                _mapStatus.text = _nodes.Count + " items & recipes · drag to pan · wheel to zoom · click to highlight · double-click to focus";
                _view.RefreshVisuals();
                if (fit)
                    _view.FocusSelection();
            }

            private void Highlight()
            {
                _highlightNodes.Clear();
                if (string.IsNullOrEmpty(_selectedKey))
                    return;
                _highlightNodes.Add(_selectedKey);
                foreach (var edge in _edges)
                    if (edge.From.Key == _selectedKey || edge.To.Key == _selectedKey)
                    {
                        _highlightNodes.Add(edge.From.Key);
                        _highlightNodes.Add(edge.To.Key);
                    }
                foreach (var edge in _edges)
                    if (edge.From.Recipe != null && _highlightNodes.Contains(edge.From.Key) ||
                        edge.To.Recipe != null && _highlightNodes.Contains(edge.To.Key))
                    {
                        _highlightNodes.Add(edge.From.Key);
                        _highlightNodes.Add(edge.To.Key);
                    }
            }

            private void DrawDetails()
            {
                _inputDetails.Clear();
                _outputDetails.Clear();
                if (!_nodes.TryGetValue(_selectedKey ?? "", out var node))
                    return;
                string sourceKey = node.ItemId ?? node.Recipe?.OccultOutput;
                List<CraftingAcquisition> sources = null;
                bool hasSources = sourceKey != null && _index.Acquisition.TryGetValue(sourceKey, out sources) &&
                    sources.Count > 0;
                if (!hasSources) _showAcquisition = false;
                var toggle = _acquisitionToggle.transform.parent.gameObject;
                toggle.SetActive(hasSources);
                _acquisitionToggle.text = _showAcquisition ? "DETAILS" : "SOURCES";
                SetButtonColors(toggle.GetComponent<Button>(), _showAcquisition);
                var titleAnchor = _detailTitle.rectTransform.anchorMax;
                titleAnchor.x = hasSources ? 0.55f : 0.68f;
                _detailTitle.rectTransform.anchorMax = titleAnchor;
                if (node.ItemId != null)
                {
                    var producers = Routes(node.ItemId, true);
                    var consumers = Routes(node.ItemId, false);
                    _detailTitle.text = Name(node.ItemId);
                    _detailMeta.text = GroupTitle(ItemGroup(node.Item)) + " · " +
                        producers.Count + " recipes · " + consumers.Count + " uses";
                    foreach (var recipe in producers)
                        _inputDetails.Add(RecipeDetail(recipe));
                    foreach (var recipe in consumers)
                        _outputDetails.Add(RecipeDetail(recipe));
                    _detailNote.text = node.Item != null && !node.Item.Registered
                        ? "This ingredient is unavailable in the current game."
                        : producers.Count == 0
                        ? "No producing recipe in this source view. Loot, trade, rewards, or another system may supply this item. Try All sources."
                        : "Select a recipe for its ingredients, products, requirements and time. Expand follows more recipes; Focus starts a map here.";
                    if (hasSources)
                        _detailNote.text = "Occult acquisition: " + sources.Count +
                            " sources. Open Sources for faction, research and stock details.";
                    else
                    {
                        string sampleHint = OccultResearchSampleHint(node.ItemId);
                        if (sampleHint != null && node.Item?.Registered == true)
                            _detailNote.text = sampleHint + " " + _detailNote.text;
                    }
                }
                else
                {
                    var recipe = node.Recipe;
                    _detailTitle.text = recipe.Name;
                    _detailMeta.text = RecipeVenueLabel(recipe) + " · " + RecipeTimeLabel(recipe);
                    foreach (var input in recipe.Inputs)
                        _inputDetails.Add(AmountDetail(input));
                    foreach (var output in recipe.Outputs)
                        _outputDetails.Add(AmountDetail(output));
                    _detailNote.text = (recipe.Ready == true ? "Materials ready. " : "") + recipe.Access;
                    if (_inputDetails.Count == 0)
                        _inputDetails.Add(new DetailRow { Caption = "No inventory ingredients", Action = null });
                }
                if (_showAcquisition && hasSources)
                {
                    _inputDetails.Clear();
                    _outputDetails.Clear();
                    _selectedAcquisition = Mathf.Clamp(_selectedAcquisition, 0, sources.Count - 1);
                    for (int j = 0; j < sources.Count; j++)
                    {
                        int selected = j;
                        var source = sources[j];
                        _index.Items.TryGetValue(source.ItemId, out var acquisitionItem);
                        _inputDetails.Add(new DetailRow
                        {
                            Caption = (j == _selectedAcquisition ? "> " : "") + source.Caption,
                            Icon = acquisitionItem?.Icon,
                            Action = () =>
                            {
                                _selectedAcquisition = selected;
                                _outputPage = 0;
                                DrawDetails();
                            }
                        });
                    }
                    foreach (string detail in sources[_selectedAcquisition].Details)
                        _outputDetails.Add(new DetailRow { Caption = detail });
                    _detailNote.text = "Campaign snapshot: stock is not a guaranteed sale; eligible rewards are not guaranteed rolls. Refresh to update.";
                }
                DrawDetailRows();
            }

            private DetailRow RecipeDetail(CraftingRecipe recipe)
            {
                return new DetailRow { Caption = RecipeVenueLabel(recipe) + " · " + recipe.Name, Icon = RecipeIcon(recipe),
                    Action = () =>
                    {
                        string key = RecipeKey(recipe.Key);
                        if (!_nodes.ContainsKey(key))
                        {
                            if (!AddRecipe(recipe, _nodes.TryGetValue(_selectedKey ?? "", out var chosen) ? chosen.Column - 1 : 0))
                            {
                                string root = recipe.Outputs.FirstOrDefault()?.ItemId;
                                if (root != null)
                                    FocusItem(root);
                                AddRecipe(recipe, -1);
                            }
                        }
                        SelectNode(key);
                    } };
            }

            private DetailRow AmountDetail(CraftingAmount amount)
            {
                _index.Items.TryGetValue(amount.ItemId, out var item);
                return new DetailRow { Caption = (amount.Count > 0 ? amount.Count.ToString() : "?") + " × " + Name(amount.ItemId),
                    Icon = item?.Icon, Action = () => SelectNode(ItemKey(amount.ItemId)) };
            }

            private void DrawDetailRows()
            {
                bool item = _nodes.TryGetValue(_selectedKey ?? "", out var node) && node.ItemId != null;
                DrawDetailColumn(_inputRows, _inputTitle, _inputDetails, ref _inputPage,
                    _showAcquisition ? "Acquisition" : item ? "Produced by" : "Ingredients");
                DrawDetailColumn(_outputRows, _outputTitle, _outputDetails, ref _outputPage,
                    _showAcquisition ? "Current access" : item ? "Used by" : "Produces");
            }

            private void DrawDetailColumn(RectTransform content, TextMeshProUGUI title, List<DetailRow> rows, ref int page, string heading)
            {
                Clear(content);
                page = Math.Min(page, Math.Max(0, (rows.Count - 1) / DetailPage));
                int start = page * DetailPage;
                title.text = heading.ToUpperInvariant() + " · " + (rows.Count == 0 ? "0" : (start + 1) + "–" + Math.Min(start + DetailPage, rows.Count) + "/" + rows.Count);
                int shown = 0;
                for (int i = start; i < Math.Min(start + DetailPage, rows.Count); i++, shown++)
                {
                    var row = rows[i];
                    ItemButton(Span(content, "Entry " + i, 0, 1, shown * 27f, 25f, 0, 10), row.Icon, row.Caption, row.Action, 12);
                }
                if (shown == 0)
                    Text(Span(content, "No routes", 0, 1, 0, 25), "None in this source view", 12).color = DimColor;
                content.sizeDelta = new Vector2(0, Math.Max(27, shown * 27));
                content.anchoredPosition = Vector2.zero;
            }

            private sealed class DetailRow
            {
                public string Caption;
                public Sprite Icon;
                public Action Action;
            }

            private sealed class MapNode
            {
                public string Key;
                public string ItemId;
                public CraftingItem Item;
                public CraftingRecipe Recipe;
                public int Column;
                public Vector2 Position;
                public Vector2 Size;
                public int Group;
            }

            private sealed class MapEdge
            {
                public MapNode From;
                public MapNode To;
                public int Count;
                public CraftingGraphLayout.Edge Layout;
            }

            private sealed class CraftingMapGraphic : MaskableGraphic, IPointerDownHandler, IPointerClickHandler,
                IBeginDragHandler, IDragHandler, IEndDragHandler, IScrollHandler
            {
                private CraftingMapWindow _window;
                private Vector2 _pan;
                private float _zoom = 0.8f;
                private Vector2 _lastDrag;
                private bool _dragged;
                private readonly List<NodeLabel> _labels = new List<NodeLabel>();
                private readonly List<CountLabel> _counts = new List<CountLabel>();
                private readonly List<TextMeshProUGUI> _bandLabels = new List<TextMeshProUGUI>();
                private readonly List<CraftingMapConnections> _contextLines = new List<CraftingMapConnections>();
                private readonly List<CraftingMapConnections> _selectedLines = new List<CraftingMapConnections>();
                private RectTransform _contextRoot, _selectedRoot;
                private static readonly Color LineColor = new Color32(45, 94, 76, 210);

                public void Initialize(CraftingMapWindow window)
                {
                    _window = window;
                    raycastTarget = true;
                    _contextRoot = Stretch(transform, "Recipe connections", 0, 0, 0, 0);
                    _selectedRoot = Stretch(transform, "Selected connections", 0, 0, 0, 0);
                }

                private Vector2 ScreenPoint(Vector2 graph) => rectTransform.rect.center + _pan + graph * _zoom;
                private UnityEngine.Rect NodeRect(MapNode node)
                {
                    Vector2 size = node.Size * _zoom;
                    return new UnityEngine.Rect(ScreenPoint(node.Position) - size * 0.5f, size);
                }

                public void Fit()
                {
                    if (_window._nodes.Count == 0)
                        return;
                    Canvas.ForceUpdateCanvases();
                    var bounds = Bounds();
                    var viewport = rectTransform.rect;
                    _zoom = Mathf.Clamp(Mathf.Min((viewport.width - 76f) / bounds.width, (viewport.height - 100f) / bounds.height), 0.15f, 1f);
                    _pan = -bounds.center * _zoom;
                    RefreshVisuals();
                }

                public void FocusSelection()
                {
                    if (!_window._nodes.TryGetValue(_window._selectedKey ?? "", out var node))
                        return;
                    _zoom = 0.9f;
                    _pan = -node.Position * _zoom;
                    RefreshVisuals();
                }

                private UnityEngine.Rect Bounds()
                {
                    if (_window._layout != null)
                    {
                        var area = _window._layout.Bounds;
                        return new UnityEngine.Rect(area.Left, area.Bottom, area.Width, area.Height);
                    }
                    float left = float.MaxValue, right = float.MinValue, top = float.MinValue, bottom = float.MaxValue;
                    foreach (var node in _window._nodes.Values)
                    {
                        left = Mathf.Min(left, node.Position.x - node.Size.x / 2f);
                        right = Mathf.Max(right, node.Position.x + node.Size.x / 2f);
                        bottom = Mathf.Min(bottom, node.Position.y - node.Size.y / 2f);
                        top = Mathf.Max(top, node.Position.y + node.Size.y / 2f);
                    }
                    return UnityEngine.Rect.MinMaxRect(left, bottom, right, top);
                }

                public void ZoomAtCenter(float multiplier) => Zoom(multiplier, rectTransform.rect.center);

                private void Zoom(float multiplier, Vector2 localPoint)
                {
                    Vector2 at = (localPoint - rectTransform.rect.center - _pan) / _zoom;
                    _zoom = Mathf.Clamp(_zoom * multiplier, 0.15f, 2.25f);
                    _pan = localPoint - rectTransform.rect.center - at * _zoom;
                    RefreshVisuals();
                }

                public void RefreshVisuals()
                {
                    SetVerticesDirty();
                    if (_window == null)
                        return;
                    RefreshConnections(_contextRoot, _contextLines, false);
                    RefreshConnections(_selectedRoot, _selectedLines, true);
                    _window._zoomLabel.text = Mathf.RoundToInt(_zoom * 100f) + "%";
                    var viewport = rectTransform.rect;
                    int labelIndex = 0, countIndex = 0, bandIndex = 0;
                    if (_window._layout != null && _zoom >= 0.4f)
                        foreach (var band in _window._layout.Bands)
                        {
                            var area = ScreenRect(band.Bounds);
                            if (!viewport.Overlaps(area)) continue;
                            while (_bandLabels.Count <= bandIndex)
                                _bandLabels.Add(_window.Text(Rect(transform, "Processing heading", Vector2.zero, Vector2.zero), "", 12));
                            var heading = _bandLabels[bandIndex++];
                            heading.gameObject.SetActive(true);
                            heading.text = GroupTitle(band.Group).ToUpperInvariant();
                            heading.fontSize = Mathf.Clamp(12f * _zoom, 8f, 15f);
                            heading.color = GroupColor(band.Group);
                            heading.enableWordWrapping = false;
                            heading.overflowMode = TextOverflowModes.Overflow;
                            Position(heading.rectTransform, new UnityEngine.Rect(area.x + 8f, area.yMax - 21f * _zoom - 12f, Math.Max(40f, area.width - 16f), 24f));
                        }
                    if (_zoom >= 0.18f)
                    {
                        foreach (var node in _window._nodes.Values)
                        {
                            var area = NodeRect(node);
                            if (!viewport.Overlaps(area))
                                continue;
                            var label = GetLabel(labelIndex++);
                            label.Root.gameObject.SetActive(true);
                            Position(label.Root, area);
                            bool item = node.ItemId != null;
                            bool showText = _zoom >= 0.42f, showMeta = _zoom >= 0.7f;
                            label.Icon.sprite = item ? node.Item?.Icon : _window.RecipeIcon(node.Recipe);
                            float iconSize = Mathf.Min(Mathf.Clamp(34f * _zoom, 12f, 44f), Math.Min(area.height - 4f, area.width - 4f));
                            label.Icon.gameObject.SetActive(label.Icon.sprite != null && iconSize >= 8f);
                            float iconWidth = label.Icon.gameObject.activeSelf && showText ? iconSize + 5f : 0f;
                            label.Icon.rectTransform.anchorMin = label.Icon.rectTransform.anchorMax = new Vector2(showText ? 0f : 0.5f, 0.5f);
                            label.Icon.rectTransform.pivot = new Vector2(showText ? 0f : 0.5f, 0.5f);
                            label.Icon.rectTransform.anchoredPosition = new Vector2(showText ? 6f : 0f, 0f);
                            label.Icon.rectTransform.sizeDelta = new Vector2(iconSize, iconSize);
                            label.Name.gameObject.SetActive(showText);
                            label.Meta.gameObject.SetActive(showMeta);
                            float metaFont = Mathf.Clamp(Mathf.RoundToInt(10f * _zoom), 9, 16);
                            float metaHeight = Math.Max(16f, metaFont * 1.6f);
                            label.Name.rectTransform.offsetMin = new Vector2(7f + iconWidth, showMeta ? metaHeight + 4f : 3f);
                            label.Name.rectTransform.offsetMax = new Vector2(-6f, -3f);
                            label.Name.text = item ? _window.Name(node.ItemId) : node.Recipe.Name;
                            label.Name.fontSize = Mathf.Clamp(Mathf.RoundToInt(14f * _zoom), 9, 24);
                            label.Name.color = _window._selectedKey == node.Key ? AccentColor : TextColor;
                            label.Meta.rectTransform.offsetMin = new Vector2(7f + iconWidth, 2f);
                            label.Meta.rectTransform.offsetMax = new Vector2(-6f, metaHeight + 2f - area.height);
                            label.Meta.fontSize = metaFont;
                            label.Meta.text = item ? node.Item != null && !node.Item.Registered ? "UNAVAILABLE ITEM" : "ITEM" :
                                RecipeVenueLabel(node.Recipe).ToUpperInvariant() +
                                (node.Recipe.Hours.HasValue ? " · " + RecipeTimeLabel(node.Recipe) : "");
                        }
                        if (_zoom >= 0.42f) foreach (var edge in _window._edges)
                        {
                            // Keep quantities tied to the current selection; unrelated
                            // connections can share the same label midpoint.
                            if (edge.From.Key != _window._selectedKey && edge.To.Key != _window._selectedKey)
                                continue;
                            if (edge.Layout == null) continue;
                            var position = edge.From.Key == _window._selectedKey ? edge.Layout.ToLabel : edge.Layout.FromLabel;
                            Vector2 point = ScreenPoint(new Vector2(position.X, position.Y));
                            if (!viewport.Contains(point))
                                continue;
                            var label = GetCount(countIndex++);
                            label.Root.gameObject.SetActive(true);
                            label.Text.text = edge.Count > 0 ? edge.Count.ToString() : "?";
                            label.Text.fontSize = Mathf.Clamp(Mathf.RoundToInt(11f * _zoom), 9, 14);
                            label.Text.color = AccentColor;
                            Position(label.Root, new UnityEngine.Rect(point.x - 17f, point.y - 8f, 34f, 18f));
                        }
                    }
                    for (int i = labelIndex; i < _labels.Count; i++)
                        _labels[i].Root.gameObject.SetActive(false);
                    for (int i = countIndex; i < _counts.Count; i++)
                        _counts[i].Root.gameObject.SetActive(false);
                    for (int i = bandIndex; i < _bandLabels.Count; i++)
                        _bandLabels[i].gameObject.SetActive(false);
                }

                private UnityEngine.Rect ScreenRect(CraftingGraphLayout.Box area)
                    => new UnityEngine.Rect(ScreenPoint(new Vector2(area.Left, area.Bottom)), new Vector2(area.Width, area.Height) * _zoom);

                private void RefreshConnections(RectTransform root, List<CraftingMapConnections> layers, bool selected)
                {
                    if (root == null) return;
                    int needed = Math.Max(1, (_window._edges.Count + 511) / 512);
                    while (layers.Count < needed)
                    {
                        var layer = Stretch(root, "Connection group", 0, 0, 0, 0).gameObject.AddComponent<CraftingMapConnections>();
                        layer.Initialize(this, selected, layers.Count * 512);
                        layers.Add(layer);
                    }
                    for (int i = 0; i < layers.Count; i++)
                    {
                        layers[i].gameObject.SetActive(i < needed);
                        if (i < needed) layers[i].SetVerticesDirty();
                    }
                }

                private bool Highlighted(MapEdge edge) =>
                    edge.From.Recipe != null && _window._highlightNodes.Contains(edge.From.Key) ||
                    edge.To.Recipe != null && _window._highlightNodes.Contains(edge.To.Key);

                protected override void OnRectTransformDimensionsChange()
                {
                    base.OnRectTransformDimensionsChange();
                    if (_window != null && _window._zoomLabel != null)
                        RefreshVisuals();
                }

                protected override void OnPopulateMesh(VertexHelper helper)
                {
                    helper.Clear();
                    if (_window == null)
                        return;
                    var viewport = rectTransform.rect;
                    // Keep the complete canvas draggable even after panning past every node.
                    Quad(helper, viewport, new Color(0f, 0f, 0f, 0f));
                    if (_window._layout != null) foreach (var band in _window._layout.Bands)
                    {
                        var area = ScreenRect(band.Bounds);
                        if (!viewport.Overlaps(area)) continue;
                        Color tint = GroupColor(band.Group);
                        tint.a = 0.055f;
                        Quad(helper, area, tint);
                        tint.a = 0.42f;
                        Segment(helper, new Vector2(area.xMin, area.yMin), new Vector2(area.xMin, area.yMax), 2f, tint, viewport);
                        Segment(helper, new Vector2(area.xMin, area.yMax), new Vector2(area.xMax, area.yMax), 1f, tint, viewport);
                    }
                    foreach (var node in _window._nodes.Values)
                    {
                        var area = NodeRect(node);
                        if (!viewport.Overlaps(area))
                            continue;
                        bool selected = _window._selectedKey == node.Key;
                        bool related = _window._highlightNodes.Contains(node.Key);
                        Color border = selected ? TextColor : related ? new Color32(98, 148, 109, 255) : BorderColor;
                        Quad(helper, area, border);
                        float inset = selected ? 1.5f : 1f;
                        Quad(helper, new UnityEngine.Rect(area.x + inset, area.y + inset, area.width - inset * 2, area.height - inset * 2),
                            selected ? SelectedColor : node.Recipe == null ? new Color32(8, 19, 13, 255) : new Color32(13, 29, 23, 255));
                        Quad(helper, new UnityEngine.Rect(area.x + inset, area.y + inset, 2f, area.height - inset * 2), GroupColor(node.Group));
                        if (selected)
                        {
                            float tick = Mathf.Min(4f, area.height / 4f);
                            Quad(helper, new UnityEngine.Rect(area.xMin, area.yMin, tick, tick), AccentColor);
                            Quad(helper, new UnityEngine.Rect(area.xMin, area.yMax - tick, tick, tick), AccentColor);
                            Quad(helper, new UnityEngine.Rect(area.xMax - tick, area.yMin, tick, tick), AccentColor);
                            Quad(helper, new UnityEngine.Rect(area.xMax - tick, area.yMax - tick, tick, tick), AccentColor);
                        }
                    }
                }

                // Separate graphics keep the bounded 2,400-edge view below Unity's
                // per-mesh vertex limit, including emphasis for dense selections.
                private sealed class CraftingMapConnections : MaskableGraphic
                {
                    private CraftingMapGraphic _map;
                    private bool _directOnly;
                    private int _firstEdge;
                    public void Initialize(CraftingMapGraphic map, bool directOnly, int firstEdge)
                    { _map = map; _directOnly = directOnly; _firstEdge = firstEdge; raycastTarget = false; }

                    protected override void OnPopulateMesh(VertexHelper helper)
                    {
                        helper.Clear();
                        if (_map?._window == null) return;
                        var viewport = rectTransform.rect;
                        int endEdge = Math.Min(_map._window._edges.Count, _firstEdge + 512);
                        for (int edgeIndex = _firstEdge; edgeIndex < endEdge; edgeIndex++)
                        {
                            var edge = _map._window._edges[edgeIndex];
                            if (edge.Layout == null || edge.Layout.Points.Count < 2) continue;
                            bool direct = edge.From.Key == _map._window._selectedKey || edge.To.Key == _map._window._selectedKey;
                            if (_directOnly && !direct) continue;
                            var points = edge.Layout.Points;
                            if (!_directOnly && direct)
                            {
                                // All halos stay below all selected lines, including adjacent chunks.
                                for (int i = 1; i < points.Count; i++)
                                    Segment(helper, Point(points[i - 1]), Point(points[i]), 5.5f, new Color32(5, 12, 8, 255), viewport);
                                continue;
                            }
                            bool related = _map.Highlighted(edge);
                            Color tint = direct ? AccentColor : related ? GroupColor(edge.From.Recipe != null ? edge.From.Group : edge.To.Group) : LineColor;
                            if (!direct && !related) tint.a = 0.22f;
                            float width = direct ? 2f : related ? 1.6f : 1f;
                            for (int i = 1; i < points.Count; i++)
                                Segment(helper, Point(points[i - 1]), Point(points[i]), width, tint, viewport);
                            var end = Point(points[points.Count - 1]);
                            if (!viewport.Contains(end)) continue;
                            var direction = (end - Point(points[points.Count - 2])).normalized;
                            var tail = end - direction * (direct ? 7f : 5f);
                            var side = new Vector2(-direction.y, direction.x) * (direct ? 3.5f : 2.5f);
                            int start = helper.currentVertCount;
                            helper.AddVert(end, tint, Vector2.zero);
                            helper.AddVert(tail + side, tint, Vector2.zero);
                            helper.AddVert(tail - side, tint, Vector2.zero);
                            helper.AddTriangle(start, start + 1, start + 2);
                        }
                    }

                    private Vector2 Point(CraftingGraphLayout.Point point) => _map.ScreenPoint(new Vector2(point.X, point.Y));
                }

                private static void Quad(VertexHelper helper, UnityEngine.Rect rect, Color color)
                {
                    int start = helper.currentVertCount;
                    helper.AddVert(new Vector3(rect.xMin, rect.yMin), color, Vector2.zero);
                    helper.AddVert(new Vector3(rect.xMin, rect.yMax), color, Vector2.zero);
                    helper.AddVert(new Vector3(rect.xMax, rect.yMax), color, Vector2.zero);
                    helper.AddVert(new Vector3(rect.xMax, rect.yMin), color, Vector2.zero);
                    helper.AddTriangle(start, start + 1, start + 2);
                    helper.AddTriangle(start, start + 2, start + 3);
                }

                private static void Segment(VertexHelper helper, Vector2 from, Vector2 to, float width, Color color, UnityEngine.Rect viewport)
                {
                    // Orthogonal segments can be clipped cheaply, keeping a large panned map out of the mesh.
                    if (Mathf.Abs(from.x - to.x) < 0.01f)
                    {
                        if (from.x < viewport.xMin || from.x > viewport.xMax)
                            return;
                        float bottom = Mathf.Max(Mathf.Min(from.y, to.y), viewport.yMin);
                        float top = Mathf.Min(Mathf.Max(from.y, to.y), viewport.yMax);
                        if (top > bottom)
                            Quad(helper, new UnityEngine.Rect(from.x - width / 2f, bottom, width, top - bottom), color);
                    }
                    else if (Mathf.Abs(from.y - to.y) < 0.01f)
                    {
                        if (from.y < viewport.yMin || from.y > viewport.yMax)
                            return;
                        float left = Mathf.Max(Mathf.Min(from.x, to.x), viewport.xMin);
                        float right = Mathf.Min(Mathf.Max(from.x, to.x), viewport.xMax);
                        if (right > left)
                            Quad(helper, new UnityEngine.Rect(left, from.y - width / 2f, right - left, width), color);
                    }
                    else if (viewport.Contains(from) || viewport.Contains(to))
                    {
                        Vector2 side = new Vector2(-(to - from).y, (to - from).x).normalized * width / 2f;
                        int start = helper.currentVertCount;
                        helper.AddVert(from - side, color, Vector2.zero);
                        helper.AddVert(from + side, color, Vector2.zero);
                        helper.AddVert(to + side, color, Vector2.zero);
                        helper.AddVert(to - side, color, Vector2.zero);
                        helper.AddTriangle(start, start + 1, start + 2);
                        helper.AddTriangle(start, start + 2, start + 3);
                    }
                }

                private NodeLabel GetLabel(int index)
                {
                    if (index < _labels.Count)
                        return _labels[index];
                    var root = Rect(transform, "Node label", Vector2.zero, Vector2.zero);
                    root.pivot = Vector2.zero;
                    var iconRect = Rect(root, "Item icon", Vector2.zero, new Vector2(0, 1));
                    var icon = iconRect.gameObject.AddComponent<Image>();
                    icon.preserveAspect = true;
                    icon.raycastTarget = false;
                    var name = _window.Text(Stretch(root, "Name", 6, 22, 6, 3), "", 14);
                    name.alignment = TextAlignmentOptions.MidlineLeft;
                    var meta = _window.Text(Stretch(root, "Kind", 6, 2, 6, 48), "", 10);
                    meta.color = DimColor;
                    var label = new NodeLabel { Root = root, Icon = icon, Name = name, Meta = meta };
                    _labels.Add(label);
                    return label;
                }

                private CountLabel GetCount(int index)
                {
                    if (index < _counts.Count)
                        return _counts[index];
                    var root = Rect(transform, "Recipe quantity", Vector2.zero, Vector2.zero);
                    var background = Paint(root, new Color32(5, 12, 8, 244));
                    background.raycastTarget = false;
                    var text = _window.Text(Stretch(root, "Quantity", 1, 0, 1, 0), "", 11);
                    text.alignment = TextAlignmentOptions.Midline;
                    text.enableWordWrapping = false;
                    var label = new CountLabel { Root = root, Text = text };
                    _counts.Add(label);
                    return label;
                }

                private void Position(RectTransform rect, UnityEngine.Rect local)
                {
                    rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
                    rect.pivot = Vector2.zero;
                    rect.anchoredPosition = new Vector2(local.x, local.y) - rectTransform.rect.center;
                    rect.sizeDelta = local.size;
                }

                public void OnPointerDown(PointerEventData data)
                {
                    _dragged = false;
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, data.position, data.pressEventCamera, out _lastDrag);
                    _window._searchPanel.gameObject.SetActive(false);
                    _window._search.DeactivateInputField();
                }

                public void OnBeginDrag(PointerEventData data) => _dragged = true;
                public void OnEndDrag(PointerEventData data) { }
                public void OnDrag(PointerEventData data)
                {
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, data.position, data.pressEventCamera, out var point);
                    _pan += point - _lastDrag;
                    _lastDrag = point;
                    RefreshVisuals();
                }

                public void OnScroll(PointerEventData data)
                {
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, data.position, data.enterEventCamera, out var point);
                    Zoom(Mathf.Pow(1.16f, data.scrollDelta.y), point);
                    data.Use();
                }

                public void OnPointerClick(PointerEventData data)
                {
                    if (_dragged || data.button != PointerEventData.InputButton.Left)
                        return;
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, data.position, data.pressEventCamera, out var point);
                    var node = _window._nodes.Values.LastOrDefault(candidate => NodeRect(candidate).Contains(point));
                    if (node == null)
                        return;
                    _window.SelectNode(node.Key);
                    if (data.clickCount > 1)
                        _window.FocusSelection();
                }

                private sealed class NodeLabel
                {
                    public RectTransform Root;
                    public Image Icon;
                    public TextMeshProUGUI Name;
                    public TextMeshProUGUI Meta;
                }

                private sealed class CountLabel
                {
                    public RectTransform Root;
                    public TextMeshProUGUI Text;
                }
            }

            private static RectTransform Rect(Transform parent, string name, Vector2 min, Vector2 max)
            {
                var rect = (RectTransform)new GameObject(name, typeof(RectTransform)).transform;
                rect.SetParent(parent, false);
                rect.anchorMin = min;
                rect.anchorMax = max;
                rect.offsetMin = rect.offsetMax = Vector2.zero;
                return rect;
            }

            private static RectTransform Stretch(Transform parent, string name, float left, float bottom, float right, float top)
            {
                var rect = Rect(parent, name, Vector2.zero, Vector2.one);
                rect.offsetMin = new Vector2(left, bottom);
                rect.offsetMax = new Vector2(-right, -top);
                return rect;
            }

            private static RectTransform At(Transform parent, string name, float x, float y, float width, float height)
            {
                var rect = Rect(parent, name, new Vector2(0, 1), new Vector2(0, 1));
                rect.pivot = new Vector2(0, 1);
                rect.anchoredPosition = new Vector2(x, -y);
                rect.sizeDelta = new Vector2(width, height);
                return rect;
            }

            private static RectTransform AtRight(Transform parent, string name, float right, float y, float width, float height)
            {
                var rect = Rect(parent, name, Vector2.one, Vector2.one);
                rect.pivot = Vector2.one;
                rect.anchoredPosition = new Vector2(-right, -y);
                rect.sizeDelta = new Vector2(width, height);
                return rect;
            }

            private static RectTransform Span(Transform parent, string name, float min, float max, float top, float height, float left = 0, float right = 0)
            {
                var rect = Rect(parent, name, new Vector2(min, 1), new Vector2(max, 1));
                rect.offsetMin = new Vector2(left, -top - height);
                rect.offsetMax = new Vector2(-right, -top);
                return rect;
            }

            private static Image Paint(RectTransform rect, Color color)
            {
                var image = rect.gameObject.AddComponent<Image>();
                image.color = color;
                return image;
            }

            private static CraftingMapFrame Frame(RectTransform rect, Color border, bool corners = false)
            {
                var graphic = Stretch(rect, "Native-style frame", 0, 0, 0, 0).gameObject.AddComponent<CraftingMapFrame>();
                graphic.raycastTarget = false;
                graphic.SetStyle(border, corners);
                return graphic;
            }

            // Simple geometry keeps borders crisp at the canvas scale without borrowing
            // a native button's listeners, navigation registration or mutable materials.
            private sealed class CraftingMapFrame : MaskableGraphic
            {
                private bool _corners;

                public void SetStyle(Color border, bool corners)
                {
                    color = border;
                    _corners = corners;
                    SetVerticesDirty();
                }

                protected override void OnPopulateMesh(VertexHelper helper)
                {
                    helper.Clear();
                    var area = rectTransform.rect;
                    if (area.width <= 0 || area.height <= 0)
                        return;
                    Fill(helper, new UnityEngine.Rect(area.xMin, area.yMin, area.width, 1f), color);
                    Fill(helper, new UnityEngine.Rect(area.xMin, area.yMax - 1f, area.width, 1f), color);
                    Fill(helper, new UnityEngine.Rect(area.xMin, area.yMin, 1f, area.height), color);
                    Fill(helper, new UnityEngine.Rect(area.xMax - 1f, area.yMin, 1f, area.height), color);
                    if (!_corners)
                        return;
                    const float tick = 3f;
                    Fill(helper, new UnityEngine.Rect(area.xMin, area.yMin, tick, tick), AccentColor);
                    Fill(helper, new UnityEngine.Rect(area.xMin, area.yMax - tick, tick, tick), AccentColor);
                    Fill(helper, new UnityEngine.Rect(area.xMax - tick, area.yMin, tick, tick), AccentColor);
                    Fill(helper, new UnityEngine.Rect(area.xMax - tick, area.yMax - tick, tick, tick), AccentColor);
                }

                private static void Fill(VertexHelper helper, UnityEngine.Rect area, Color tint)
                {
                    int start = helper.currentVertCount;
                    helper.AddVert(new Vector3(area.xMin, area.yMin), tint, Vector2.zero);
                    helper.AddVert(new Vector3(area.xMin, area.yMax), tint, Vector2.zero);
                    helper.AddVert(new Vector3(area.xMax, area.yMax), tint, Vector2.zero);
                    helper.AddVert(new Vector3(area.xMax, area.yMin), tint, Vector2.zero);
                    helper.AddTriangle(start, start + 1, start + 2);
                    helper.AddTriangle(start, start + 2, start + 3);
                }
            }

            private TextMeshProUGUI Text(RectTransform rect, string caption, int size)
            {
                var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
                // Resolve even initially empty labels: ActualizeFontAndSize skips empty text.
                if (_font != null)
                    text.font = _font;
                text.text = caption ?? string.Empty;
                // Resolve the active native language font without cloning or altering its assets.
                Localization.ActualizeFontAndSize(text, TextContext.IgnoreSize);
                text.fontSize = size;
                text.fontStyle = FontStyles.Normal;
                text.color = TextColor;
                text.alignment = TextAlignmentOptions.MidlineLeft;
                text.enableWordWrapping = true;
                text.overflowMode = TextOverflowModes.Truncate;
                text.richText = false;
                text.raycastTarget = false;
                return text;
            }

            private Button Button(RectTransform rect, string name, Action action)
            {
                var image = rect.GetComponent<Image>() ?? Paint(rect, Color.white);
                Frame(rect, BorderColor);
                var button = rect.gameObject.AddComponent<Button>();
                button.targetGraphic = image;
                SetButtonColors(button, false);
                button.navigation = new UnityEngine.UI.Navigation { mode = UnityEngine.UI.Navigation.Mode.None };
                if (action != null)
                    button.onClick.AddListener(() => action());
                else
                    button.interactable = false;
                return button;
            }

            private static void SetButtonColors(Button button, bool selected)
            {
                var colors = button.colors;
                colors.normalColor = selected ? SelectedColor : ButtonColor;
                colors.highlightedColor = new Color32(31, 64, 47, 255);
                colors.pressedColor = new Color32(55, 87, 58, 255);
                colors.selectedColor = colors.normalColor;
                colors.disabledColor = new Color32(8, 13, 10, 255);
                colors.colorMultiplier = 1f;
                colors.fadeDuration = 0.06f;
                button.colors = colors;
            }

            private TextMeshProUGUI TextButton(RectTransform rect, string caption, Action action, int size = 13)
            {
                Button(rect, caption, action);
                var text = Text(Stretch(rect, "Label", 4, 1, 4, 1), caption, size);
                text.alignment = TextAlignmentOptions.Midline;
                if (caption != null && caption.Length == 1)
                {
                    // Native font line metrics can exceed these compact symbol buttons.
                    text.enableWordWrapping = false;
                    text.overflowMode = TextOverflowModes.Overflow;
                }
                return text;
            }

            private void ItemButton(RectTransform rect, Sprite icon, string caption, Action action, int size)
            {
                Button(rect, caption, action);
                if (icon != null)
                {
                    var image = At(rect, "Icon", 3, 2, 23, 23).gameObject.AddComponent<Image>();
                    image.sprite = icon;
                    image.preserveAspect = true;
                    image.raycastTarget = false;
                }
                Text(Stretch(rect, "Label", icon == null ? 7 : 31, 1, 5, 1), caption, size);
            }

            private static RectTransform Scroll(RectTransform root)
            {
                Paint(root, RecessColor);
                Frame(root, new Color32(27, 60, 45, 255));
                var scroll = root.gameObject.AddComponent<ScrollRect>();
                // Reserve the gutter even when the bar hides, so labels never
                // reflow or sit beneath the 10-unit track and its margins.
                var viewport = Stretch(root, "Viewport", 0, 0, 16, 0);
                viewport.gameObject.AddComponent<RectMask2D>();
                var content = Rect(viewport, "Content", new Vector2(0, 1), Vector2.one);
                content.pivot = new Vector2(0.5f, 1);
                content.sizeDelta = Vector2.zero;

                var track = Rect(root, "Vertical scrollbar", new Vector2(1, 0), Vector2.one);
                track.offsetMin = new Vector2(-12, 2);
                track.offsetMax = new Vector2(-2, -2);
                Paint(track, BorderColor);
                var slidingArea = Stretch(track, "Sliding area", 1, 1, 1, 1);
                var handle = Stretch(slidingArea, "Handle", 0, 0, 0, 0);
                var handleImage = Paint(handle, TextColor);
                var scrollbar = track.gameObject.AddComponent<Scrollbar>();
                scrollbar.direction = Scrollbar.Direction.BottomToTop;
                scrollbar.handleRect = handle;
                scrollbar.targetGraphic = handleImage;
                scrollbar.navigation = new UnityEngine.UI.Navigation { mode = UnityEngine.UI.Navigation.Mode.None };
                scrollbar.size = 1f;
                scrollbar.value = 1f;

                scroll.viewport = viewport;
                scroll.content = content;
                scroll.horizontal = false;
                scroll.vertical = true;
                scroll.verticalScrollbar = scrollbar;
                scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
                scroll.movementType = ScrollRect.MovementType.Clamped;
                scroll.scrollSensitivity = 28f;
                scroll.inertia = false;
                return content;
            }

            private static void Clear(Transform root)
            {
                for (int i = root.childCount - 1; i >= 0; i--)
                {
                    var child = root.GetChild(i);
                    child.gameObject.SetActive(false);
                    child.SetParent(null, false);
                    UnityEngine.Object.Destroy(child.gameObject);
                }
            }
        }
    }
}

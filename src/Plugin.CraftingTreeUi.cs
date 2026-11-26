using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MGSC;
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

        // Native SpaceGameMode processes global shortcuts after individual window input.
        // Suppress that input read while our expanded map owns focus; keep its game tick intact.
        [HarmonyPatch(typeof(InputController), "GetInput", new[] { typeof(KeyLayout) })]
        private static class CraftingMapNativeInput
        {
            private static bool Prefix(ref GameKey.KeyType __result)
            {
                if ((_craftingMapWindow == null || !_craftingMapWindow.BlocksNativeInput) &&
                    _craftingMapClosedFrame != Time.frameCount)
                    return true;
                __result = GameKey.KeyType.None;
                return false;
            }
        }

        private sealed class CraftingMapWindow : MonoBehaviour
        {
            private const int MapNodeLimit = 240;
            private const int MapEdgeLimit = 2400;
            private const int BranchPage = 8;
            private const int SearchPage = 10;
            private const int DetailPage = 14;
            private static readonly Color PanelColor = new Color32(28, 43, 36, 252);
            private static readonly Color RecessColor = new Color32(16, 29, 23, 255);
            private static readonly Color ButtonColor = new Color32(49, 68, 55, 255);
            private static readonly Color TextColor = new Color32(197, 214, 189, 255);
            private static readonly Color DimColor = new Color32(139, 163, 137, 255);
            private static readonly Color AccentColor = new Color32(183, 210, 158, 255);
            private Component _owner;
            private GameObject _previousSelection;
            private Font _font;
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
            private readonly Dictionary<int, int> _columnRows = new Dictionary<int, int>();
            private readonly HashSet<string> _highlightNodes = new HashSet<string>();
            private string _selectedKey;
            private string _rootItem;
            private string _venue = "All";
            private readonly Dictionary<string, Image> _venueButtons = new Dictionary<string, Image>();
            private CraftingMapGraphic _view;
            private Text _mapStatus;
            private Text _catalogStatus;
            private Text _zoomLabel;
            private InputField _search;
            private RectTransform _searchPanel;
            private RectTransform _searchRows;
            private Text _searchCount;
            private int _searchPage;
            private List<CraftingItem> _matches = new List<CraftingItem>();
            private Text _detailTitle;
            private Text _detailMeta;
            private Text _detailNote;
            private RectTransform _inputRows;
            private RectTransform _outputRows;
            private Text _inputTitle;
            private Text _outputTitle;
            private int _inputPage;
            private int _outputPage;
            private List<DetailRow> _inputDetails = new List<DetailRow>();
            private List<DetailRow> _outputDetails = new List<DetailRow>();
            private Text _expandInputsLabel;
            private Text _expandUsesLabel;

            public bool BlocksNativeInput => _expanded || _slide > 0.001f;
            public bool IsOwner(Component owner) => _owner == owner;
            public void SetOwner(Component owner) => _owner = owner;

            public void Initialize(Component owner)
            {
                _owner = owner;
                _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
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
                _panel = Stretch(transform, "Crafting map panel", 0, 24, 52, 24);
                Paint(_panel, PanelColor);
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
                var caption = Text(captionRect, "Crafting map", 16);
                caption.alignment = TextAnchor.MiddleCenter;
                caption.rectTransform.localEulerAngles = new Vector3(0f, 0f, 90f);
                var arrow = Text(At(rect, "Direction", 5f, 7f, 28f, 30f), opening ? "▶" : "◀", 19);
                arrow.alignment = TextAnchor.MiddleCenter;
                return rect;
            }

            private void BuildHeader()
            {
                Text(At(_panel, "Title", 16f, 9f, 230f, 30f), "Crafting map", 22, true);
                _catalogStatus = Text(Span(_panel, "Catalogue status", 0.23f, 0.66f, 12f, 22f),
                    "Open the map to read the loaded recipes", 11);
                var refresh = AtRight(_panel, "Refresh", 92f, 10f, 88f, 28f);
                TextButton(refresh, "Refresh", () => RefreshIndex(true));
                TextButton(AtRight(_panel, "Close", 12f, 10f, 70f, 28f), "Collapse", Collapse);

                var searchRect = At(_panel, "Find item", 14f, 46f, 286f, 32f);
                Paint(searchRect, RecessColor);
                _search = searchRect.gameObject.AddComponent<InputField>();
                var inputText = Text(Stretch(searchRect, "Search text", 9, 3, 9, 3), "", 14);
                inputText.alignment = TextAnchor.MiddleLeft;
                var placeholder = Text(Stretch(searchRect, "Search hint", 9, 3, 9, 3), "Find any item…", 14);
                placeholder.color = DimColor;
                _search.textComponent = inputText;
                _search.placeholder = placeholder;
                _search.targetGraphic = searchRect.GetComponent<Image>();
                _search.selectionColor = new Color(0.45f, 0.58f, 0.42f, 0.65f);
                _search.onValueChanged.AddListener(_ => { _searchPage = 0; FindItems(); });
                var events = searchRect.gameObject.AddComponent<EventTrigger>();
                var select = new EventTrigger.Entry { eventID = EventTriggerType.Select };
                select.callback.AddListener(_ => { _searchPage = 0; FindItems(); });
                events.triggers.Add(select);

                string[] venues = { "All", "Magnum", "Workbench", "Station", "Recycling" };
                string[] captions = { "All", "Magnum", "Workbench", "Stations", "Recovery" };
                for (int i = 0; i < venues.Length; i++)
                {
                    string venue = venues[i];
                    var rect = At(_panel, venue + " source", 310f + 85f * i, 46f, 80f, 32f);
                    TextButton(rect, captions[i], () => SetVenue(venue), 12);
                    _venueButtons.Add(venue, rect.GetComponent<Image>());
                }
                MarkVenue();
            }

            private void BuildCanvas()
            {
                var frame = Stretch(_panel, "Graph frame", 14, 204, 14, 88);
                Paint(frame, RecessColor);
                frame.gameObject.AddComponent<RectMask2D>();
                var graphicRect = Stretch(frame, "Connected crafting graph", 0, 0, 0, 0);
                _view = graphicRect.gameObject.AddComponent<CraftingMapGraphic>();
                _view.Initialize(this);
                var controls = Rect(frame, "Map controls", new Vector2(0f, 0f), new Vector2(1f, 0f));
                controls.pivot = new Vector2(0.5f, 0f);
                controls.sizeDelta = new Vector2(0f, 34f);
                Paint(controls, new Color32(30, 46, 36, 246));
                TextButton(At(controls, "Zoom out", 6, 4, 28, 26), "−", () => _view.ZoomAtCenter(1f / 1.2f), 18);
                _zoomLabel = Text(At(controls, "Zoom amount", 38, 4, 48, 26), "100%", 12);
                _zoomLabel.alignment = TextAnchor.MiddleCenter;
                TextButton(At(controls, "Zoom in", 90, 4, 28, 26), "+", () => _view.ZoomAtCenter(1.2f), 18);
                TextButton(At(controls, "Fit map", 126, 4, 44, 26), "Fit", () => _view.Fit(), 12);
                TextButton(At(controls, "Focus node", 178, 4, 60, 26), "Focus", FocusSelection, 12);
                _expandInputsLabel = TextButton(At(controls, "Expand inputs", 248, 4, 142, 26),
                    "Expand inputs", () => ExpandSelection(true), 12);
                _expandUsesLabel = TextButton(At(controls, "Expand uses", 398, 4, 142, 26),
                    "Expand uses", () => ExpandSelection(false), 12);
                _mapStatus = Text(Span(frame, "Map status", 0f, 1f, 5, 22, 10, 10),
                    "Drag to pan • wheel to zoom • click to highlight", 11);
                _mapStatus.color = DimColor;
            }

            private void BuildDetails()
            {
                var details = Rect(_panel, "Selected node details", new Vector2(0f, 0f), new Vector2(1f, 0f));
                details.pivot = new Vector2(0.5f, 0f);
                details.offsetMin = new Vector2(14, 12);
                details.offsetMax = new Vector2(-14, 192);
                Paint(details, new Color32(36, 52, 41, 255));
                _detailTitle = Text(Span(details, "Selected name", 0, 0.68f, 7, 27, 12, 4), "Select an item or recipe", 18, true);
                _detailMeta = Text(Span(details, "Time and venue", 0.68f, 1f, 7, 27, 4, 12), "", 13);
                _detailMeta.alignment = TextAnchor.MiddleRight;
                var inputs = Span(details, "Input column", 0f, 0.5f, 39, 101, 12, 8);
                var outputs = Span(details, "Output column", 0.5f, 1f, 39, 101, 8, 12);
                _inputTitle = Text(Span(inputs, "Input heading", 0, 1, 0, 22, 0, 68), "Inputs", 12, true);
                _outputTitle = Text(Span(outputs, "Output heading", 0, 1, 0, 22, 0, 68), "Outputs", 12, true);
                TextButton(AtRight(inputs, "Previous inputs", 34, 0, 28, 22), "‹", () => { _inputPage = Math.Max(0, _inputPage - 1); DrawDetailRows(); }, 16);
                TextButton(AtRight(inputs, "Next inputs", 0, 0, 28, 22), "›", () => { _inputPage = Math.Min(Math.Max(0, (_inputDetails.Count - 1) / DetailPage), _inputPage + 1); DrawDetailRows(); }, 16);
                TextButton(AtRight(outputs, "Previous outputs", 34, 0, 28, 22), "‹", () => { _outputPage = Math.Max(0, _outputPage - 1); DrawDetailRows(); }, 16);
                TextButton(AtRight(outputs, "Next outputs", 0, 0, 28, 22), "›", () => { _outputPage = Math.Min(Math.Max(0, (_outputDetails.Count - 1) / DetailPage), _outputPage + 1); DrawDetailRows(); }, 16);
                _inputRows = Scroll(Stretch(inputs, "Input list", 0, 0, 0, 25));
                _outputRows = Scroll(Stretch(outputs, "Output list", 0, 0, 0, 25));
                _detailNote = Text(Span(details, "Selected information", 0, 1, 146, 28, 12, 12),
                    "Expand a branch to follow its recipes. Counts belong to each individual production order.", 11);
                _detailNote.color = DimColor;
            }

            private void BuildSearchResults()
            {
                _searchPanel = At(_panel, "All item search results", 14, 82, 380, 350);
                Paint(_searchPanel, PanelColor);
                _searchCount = Text(At(_searchPanel, "Results count", 8, 5, 250, 24), "", 12);
                TextButton(AtRight(_searchPanel, "Previous results", 70, 5, 28, 24), "‹", () => { _searchPage = Math.Max(0, _searchPage - 1); DrawSearchResults(); }, 16);
                TextButton(AtRight(_searchPanel, "Next results", 36, 5, 28, 24), "›", () => { _searchPage = Math.Min(Math.Max(0, (_matches.Count - 1) / SearchPage), _searchPage + 1); DrawSearchResults(); }, 16);
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
                    int partial = (_index.Summary ?? "").IndexOf("Partial capture:", StringComparison.Ordinal);
                    _catalogStatus.text = partial < 0 ? _index.Items.Count + " items · " + _index.Recipes.Count + " loaded recipes" : _index.Summary.Substring(partial);
                    _catalogStatus.color = partial < 0 ? DimColor : new Color32(220, 195, 135, 255);
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
                        _columnRows.Clear();
                        foreach (var node in _nodes.Values)
                        {
                            int offset = Mathf.RoundToInt(node.Position.y / 104f);
                            int row = offset == 0 ? 0 : Math.Abs(offset) * 2 - (offset < 0 ? 1 : 0);
                            _columnRows.TryGetValue(node.Column, out int used);
                            _columnRows[node.Column] = Math.Max(used, row + 1);
                        }
                        _selectedKey = _nodes.ContainsKey(selection ?? "") ? selection : ItemKey(_rootItem);
                        Changed(false);
                    }
                    else if (_rootItem != null)
                        FocusItem(_rootItem);
                    else
                        _mapStatus.text = "No registered inventory items were available in this capture.";
                }
                catch (Exception error)
                {
                    Debug.LogError("[Cvar_ImperiumOfMan] Crafting map capture failed: " + error);
                    _catalogStatus.text = "Recipe capture failed — see game log";
                    _detailNote.text = "Could not refresh the loaded crafting data. Use Refresh to try again.";
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
                    button.Value.color = button.Key == _venue ? new Color32(79, 101, 68, 255) : ButtonColor;
            }

            private bool InVenue(CraftingRecipe recipe) => _venue == "All" || recipe.Venue == _venue;
            private static string ItemKey(string id) => "item:" + id;
            private static string RecipeKey(string id) => "recipe:" + id;
            private string Name(string id) => _index != null && _index.Items.TryGetValue(id, out var item) ? item.Name ?? item.Id : id;

            private void FindItems()
            {
                if (_index == null)
                    return;
                string query = (_search.text ?? "").Trim();
                _matches = _allItems.Where(item => query.Length == 0 ||
                    (item.Name ?? "").IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    item.Id.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (item.Category ?? "").IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
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
                        item.Name ?? item.Id, () =>
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
                _columnRows.Clear();
            }

            private void FocusItem(string id)
            {
                if (_index == null || !_index.Items.ContainsKey(id))
                    return;
                _rootItem = id;
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
                    Position = NextPosition(column), Size = new Vector2(190, 68) };
                _nodes.Add(key, node);
                return node;
            }

            private Vector2 NextPosition(int column)
            {
                _columnRows.TryGetValue(column, out int row);
                _columnRows[column] = row + 1;
                // Stable positions: expanding a branch never shuffles existing nodes.
                int offset = row == 0 ? 0 : (row + 1) / 2 * (row % 2 == 1 ? -1 : 1);
                return new Vector2(column * 260f, offset * 104f);
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
                    Position = NextPosition(column), Size = new Vector2(154, 64) };
                _nodes.Add(key, node);
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
                _selectedKey = key;
                _inputPage = _outputPage = 0;
                _searchPanel.gameObject.SetActive(false);
                Changed(false);
                // Selection intentionally does not move, zoom, or rebuild the graph.
            }

            private void Changed(bool fit)
            {
                Highlight();
                DrawDetails();
                _expandInputsLabel.text = "Expand inputs · " + RemainingBranches(true);
                _expandUsesLabel.text = "Expand uses · " + RemainingBranches(false);
                _mapStatus.text = _nodes.Count + " map nodes · drag to pan · wheel to zoom · click to highlight · double-click to focus";
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
                if (node.ItemId != null)
                {
                    var producers = Routes(node.ItemId, true);
                    var consumers = Routes(node.ItemId, false);
                    _detailTitle.text = Name(node.ItemId);
                    _detailMeta.text = producers.Count + " producing routes · " + consumers.Count + " uses";
                    foreach (var recipe in producers)
                        _inputDetails.Add(RecipeDetail(recipe));
                    foreach (var recipe in consumers)
                        _outputDetails.Add(RecipeDetail(recipe));
                    _detailNote.text = producers.Count == 0
                        ? "No producing recipe in this source view. Loot, trade, rewards, or another system may supply this item. Try All sources."
                        : "Select a process node for exact per-order inputs, outputs, access, and base time. Expand follows more branches; Focus starts a map here.";
                }
                else
                {
                    var recipe = node.Recipe;
                    _detailTitle.text = recipe.Name;
                    _detailMeta.text = recipe.Venue + " · " + (recipe.Hours.HasValue ? recipe.Hours.Value.ToString("0.##") + " base h" : "time not captured");
                    foreach (var input in recipe.Inputs)
                        _inputDetails.Add(AmountDetail(input));
                    foreach (var output in recipe.Outputs)
                        _outputDetails.Add(AmountDetail(output));
                    _detailNote.text = (recipe.Ready == true ? "Materials ready. " : "") + recipe.Access;
                    if (_inputDetails.Count == 0)
                        _inputDetails.Add(new DetailRow { Caption = "No inventory ingredients", Action = null });
                }
                DrawDetailRows();
            }

            private DetailRow RecipeDetail(CraftingRecipe recipe)
            {
                return new DetailRow { Caption = recipe.Venue + " · " + recipe.Name,
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
                DrawDetailColumn(_inputRows, _inputTitle, _inputDetails, ref _inputPage, item ? "Produced by" : "All required inputs");
                DrawDetailColumn(_outputRows, _outputTitle, _outputDetails, ref _outputPage, item ? "Used by" : "Outputs of this order");
            }

            private void DrawDetailColumn(RectTransform content, Text title, List<DetailRow> rows, ref int page, string heading)
            {
                Clear(content);
                page = Math.Min(page, Math.Max(0, (rows.Count - 1) / DetailPage));
                int start = page * DetailPage;
                title.text = heading + " · " + (rows.Count == 0 ? "0" : (start + 1) + "–" + Math.Min(start + DetailPage, rows.Count) + "/" + rows.Count);
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
            }

            private sealed class MapEdge
            {
                public MapNode From;
                public MapNode To;
                public int Count;
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
                private readonly List<Text> _counts = new List<Text>();
                private static readonly Color LineColor = new Color32(95, 126, 95, 210);
                private static readonly Color LitLineColor = new Color32(199, 221, 156, 255);

                public void Initialize(CraftingMapWindow window)
                {
                    _window = window;
                    raycastTarget = true;
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
                    _window._zoomLabel.text = Mathf.RoundToInt(_zoom * 100f) + "%";
                    var viewport = rectTransform.rect;
                    int labelIndex = 0, countIndex = 0;
                    if (_zoom >= 0.4f)
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
                            label.Icon.sprite = item ? node.Item?.Icon : null;
                            label.Icon.gameObject.SetActive(label.Icon.sprite != null && _zoom >= 0.65f);
                            float iconWidth = label.Icon.gameObject.activeSelf ? 30f * _zoom : 0f;
                            label.Icon.rectTransform.offsetMin = new Vector2(5f, 6f);
                            label.Icon.rectTransform.offsetMax = new Vector2(5f + iconWidth, -6f);
                            label.Name.rectTransform.offsetMin = new Vector2(7f + iconWidth, area.height * 0.27f);
                            label.Name.rectTransform.offsetMax = new Vector2(-6f, -3f);
                            label.Name.text = item ? _window.Name(node.ItemId) : node.Recipe.Name;
                            label.Name.fontSize = Mathf.Clamp(Mathf.RoundToInt(13f * _zoom), 10, 22);
                            label.Name.color = _window._selectedKey == node.Key ? Color.white : TextColor;
                            label.Meta.rectTransform.offsetMin = new Vector2(7f + iconWidth, 2f);
                            label.Meta.rectTransform.offsetMax = new Vector2(-6f, -area.height * 0.68f);
                            label.Meta.fontSize = Mathf.Clamp(Mathf.RoundToInt(10f * _zoom), 9, 16);
                            label.Meta.text = item ? "ITEM" : node.Recipe.Venue + (node.Recipe.Hours.HasValue ? " · " + node.Recipe.Hours.Value.ToString("0.##") + " h" : "");
                        }
                        foreach (var edge in _window._edges)
                        {
                            Vector2 point = ScreenPoint((edge.From.Position + edge.To.Position) * 0.5f);
                            if (!viewport.Contains(point))
                                continue;
                            var label = GetCount(countIndex++);
                            label.gameObject.SetActive(true);
                            label.text = edge.Count > 0 ? edge.Count.ToString() : "?";
                            label.fontSize = Mathf.Clamp(Mathf.RoundToInt(11f * _zoom), 10, 16);
                            label.color = Highlighted(edge) ? AccentColor : DimColor;
                            Position(label.rectTransform, new UnityEngine.Rect(point.x - 20f, point.y + 2f, 40f, 20f));
                        }
                    }
                    for (int i = labelIndex; i < _labels.Count; i++)
                        _labels[i].Root.gameObject.SetActive(false);
                    for (int i = countIndex; i < _counts.Count; i++)
                        _counts[i].gameObject.SetActive(false);
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
                    foreach (var edge in _window._edges)
                    {
                        bool forward = edge.To.Position.x >= edge.From.Position.x;
                        Vector2 from = ScreenPoint(edge.From.Position + new Vector2(edge.From.Size.x * (forward ? 0.5f : -0.5f), 0));
                        Vector2 to = ScreenPoint(edge.To.Position - new Vector2(edge.To.Size.x * (forward ? 0.5f : -0.5f), 0));
                        float mid = (from.x + to.x) * 0.5f;
                        Color color = Highlighted(edge) ? LitLineColor : LineColor;
                        float width = Highlighted(edge) ? 2.2f : 1.2f;
                        Segment(helper, from, new Vector2(mid, from.y), width, color, viewport);
                        Segment(helper, new Vector2(mid, from.y), new Vector2(mid, to.y), width, color, viewport);
                        Segment(helper, new Vector2(mid, to.y), to, width, color, viewport);
                        Vector2 back = new Vector2(forward ? -6f : 6f, 0);
                        Segment(helper, to, to + back + new Vector2(0, 4), width, color, viewport);
                        Segment(helper, to, to + back - new Vector2(0, 4), width, color, viewport);
                    }
                    foreach (var node in _window._nodes.Values)
                    {
                        var area = NodeRect(node);
                        if (!viewport.Overlaps(area))
                            continue;
                        bool selected = _window._selectedKey == node.Key;
                        bool related = _window._highlightNodes.Contains(node.Key);
                        Color border = selected ? new Color32(219, 234, 173, 255) : related ? AccentColor : new Color32(86, 114, 83, 255);
                        Quad(helper, area, border);
                        float inset = selected ? 3f : 1.3f;
                        Quad(helper, new UnityEngine.Rect(area.x + inset, area.y + inset, area.width - inset * 2, area.height - inset * 2),
                            node.Recipe == null ? new Color32(41, 61, 45, 255) : new Color32(64, 76, 52, 255));
                    }
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
                    var name = _window.Text(Stretch(root, "Name", 6, 22, 6, 3), "", 13, true);
                    name.alignment = TextAnchor.MiddleLeft;
                    var meta = _window.Text(Stretch(root, "Kind", 6, 2, 6, 48), "", 10);
                    meta.color = DimColor;
                    var label = new NodeLabel { Root = root, Icon = icon, Name = name, Meta = meta };
                    _labels.Add(label);
                    return label;
                }

                private Text GetCount(int index)
                {
                    if (index < _counts.Count)
                        return _counts[index];
                    var label = _window.Text(Rect(transform, "Per-order quantity", Vector2.zero, Vector2.zero), "", 11, true);
                    label.alignment = TextAnchor.MiddleCenter;
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
                    public Text Name;
                    public Text Meta;
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

            private Text Text(RectTransform rect, string caption, int size, bool bold = false)
            {
                var text = rect.gameObject.AddComponent<Text>();
                text.font = _font;
                text.text = caption;
                text.fontSize = size;
                text.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
                text.color = TextColor;
                text.alignment = TextAnchor.MiddleLeft;
                text.horizontalOverflow = HorizontalWrapMode.Wrap;
                text.verticalOverflow = VerticalWrapMode.Truncate;
                text.supportRichText = false;
                text.raycastTarget = false;
                return text;
            }

            private Button Button(RectTransform rect, string name, Action action)
            {
                var image = rect.GetComponent<Image>() ?? Paint(rect, ButtonColor);
                var button = rect.gameObject.AddComponent<Button>();
                button.targetGraphic = image;
                var colors = button.colors;
                colors.normalColor = Color.white;
                colors.highlightedColor = new Color(1.22f, 1.22f, 1.16f, 1);
                colors.pressedColor = new Color(0.7f, 0.85f, 0.7f, 1);
                colors.selectedColor = new Color(1.12f, 1.17f, 1.08f, 1);
                button.colors = colors;
                button.navigation = new UnityEngine.UI.Navigation { mode = UnityEngine.UI.Navigation.Mode.None };
                if (action != null)
                    button.onClick.AddListener(() => action());
                else
                    button.interactable = false;
                return button;
            }

            private Text TextButton(RectTransform rect, string caption, Action action, int size = 13)
            {
                Button(rect, caption, action);
                var text = Text(Stretch(rect, "Label", 4, 1, 4, 1), caption, size);
                text.alignment = TextAnchor.MiddleCenter;
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
                Paint(root, new Color32(24, 39, 29, 255));
                var scroll = root.gameObject.AddComponent<ScrollRect>();
                var viewport = Stretch(root, "Viewport", 0, 0, 0, 0);
                viewport.gameObject.AddComponent<RectMask2D>();
                var content = Rect(viewport, "Content", new Vector2(0, 1), Vector2.one);
                content.pivot = new Vector2(0.5f, 1);
                content.sizeDelta = Vector2.zero;
                scroll.viewport = viewport;
                scroll.content = content;
                scroll.horizontal = false;
                scroll.vertical = true;
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

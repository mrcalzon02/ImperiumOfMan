using System;
using System.Collections.Generic;
using HarmonyLib;
using MGSC;
using UnityEngine;
using UnityEngine.UI;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private static ShipSupportHud _shipSupportHud;
        private static int _shipSupportConsumedFrame = -1;

        private static void CreateShipSupportDrawer(State state)
        {
            // The native HUD can survive a dungeon change. Retire the old state even
            // when the next dungeon has no call-ins or does not allow ship support.
            RemoveShipSupportDrawer();
            var hud = UI.Get<DungeonHudScreen>();
            if (hud == null)
                return;
            foreach (var previous in hud.GetComponentsInChildren<ShipSupportHud>(true))
            {
                previous.gameObject.SetActive(false);
                UnityEngine.Object.Destroy(previous.gameObject);
            }
            if (!IsSupportRaid(state))
                return;
            var progression = state.Get<MagnumProgression>();
            int[] mounts = AvailableMounts(progression);
            int squad = SupportSelection(state.Get<StoryTriggers>(), SelectedSquad,
                AvailableSquads(progression, state.Get<Factions>()), newest: true);
            bool supply = progression.IsPerkPurchased(SupplyDrop);
            if (mounts.Length == 0 && !supply && squad < 0)
                return;

            var root = CreateSupportRect(hud.transform, "ImperialShipSupport", new Vector2(0.5f, 1f),
                Vector2.zero, Vector2.zero);
            root.pivot = new Vector2(0.5f, 1f);
            _shipSupportHud = root.gameObject.AddComponent<ShipSupportHud>();
            _shipSupportHud.Initialize(state, mounts, supply, squad);
        }

        [Hook(ModHookType.DungeonFinished)]
        public static void RemoveShipSupportHud(IModContext context) => RemoveShipSupportDrawer();

        private static void RemoveShipSupportDrawer()
        {
            if (_shipSupportHud == null)
                return;
            var previous = _shipSupportHud;
            _shipSupportHud = null;
            previous.gameObject.SetActive(false);
            UnityEngine.Object.Destroy(previous.gameObject);
        }

        private static bool ProcessShipSupportDrawerInput(ref bool interruptProcessing)
        {
            if (_shipSupportConsumedFrame != Time.frameCount &&
                (_shipSupportHud == null || !_shipSupportHud.ConsumesNativeInput()))
                return true;
            interruptProcessing = true;
            return false;
        }

        // An ordinary raid calls the HUD directly, after simulation/turn updates
        // and before player commands. Native overlays instead use UI.Process.
        [HarmonyPatch(typeof(DungeonHudScreen), nameof(DungeonHudScreen.Process))]
        private static class ShipSupportRaidInput
        {
            private static bool Prefix(ref bool interruptProcessing) =>
                ProcessShipSupportDrawerInput(ref interruptProcessing);
        }

        [HarmonyPatch(typeof(MGSC.UI), nameof(MGSC.UI.Process))]
        private static class ShipSupportDrawerInput
        {
            private static bool Prefix(ref bool interruptProcessing) =>
                ProcessShipSupportDrawerInput(ref interruptProcessing);
        }

        private sealed class ShipSupportHud : MonoBehaviour
        {
            private const float TileSize = 52f;
            private const float TileGap = 6f;
            private const float GroupHeight = 84f;
            private const float Padding = 8f;
            private const float HandleHeight = 24f;
            private readonly List<(SupportAction Action, int Choice, SupportCallInTile Tile)> _tiles = new();
            private State _state;
            private Canvas _canvas;
            private CanvasGroup _visibility;
            private RectTransform _clip;
            private RectTransform _panel;
            private RectTransform _handle;
            private RectTransform _weapons;
            private RectTransform _pods;
            private RectTransform _tooltip;
            private Text _tooltipText;
            private ShipSupportButton _toggle;
            private float _weaponsWidth;
            private float _podsWidth;
            private float _panelHeight;
            private float _parentWidth = -1f;
            private bool _expanded;
            private bool _visible;
            private float _slide;
            private int _ownedMouseButtons;

            public void Initialize(State state, int[] mounts, bool supply, int squad)
            {
                _state = state;
                _canvas = GetComponentInParent<Canvas>();
                _visibility = gameObject.AddComponent<CanvasGroup>();
                _clip = TopRect(transform, "Drawer viewport", Vector2.zero);
                _clip.gameObject.AddComponent<RectMask2D>();
                _panel = TopRect(_clip, "Call-in drawer", Vector2.zero);
                Paint(_panel, new Color32(75, 101, 78, 255));
                var inset = CreateSupportRect(_panel, "Sage panel", Vector2.zero, Vector2.zero, Vector2.zero);
                inset.anchorMax = Vector2.one;
                inset.offsetMin = Vector2.one;
                inset.offsetMax = -Vector2.one;
                Paint(inset, new Color32(28, 43, 36, 252));

                _weaponsWidth = GroupWidth(mounts.Length);
                _podsWidth = GroupWidth((supply ? 1 : 0) + (squad >= 0 ? 1 : 0));
                _weapons = AddGroup("Weapons", _weaponsWidth);
                _pods = AddGroup("Pod Support", _podsWidth);
                for (int index = 0; index < mounts.Length; index++)
                {
                    int choice = mounts[index];
                    var mount = ShipMounts[choice];
                    AddTile(_weapons, index, mount.Id, mount.Art, mount.Name, SupportAction.Fire, choice);
                }
                if (mounts.Length == 0)
                    EmptyGroup(_weapons, "No mounts installed");
                int podIndex = 0;
                if (supply)
                    AddTile(_pods, podIndex++, SupplyDrop, "SupplyPod", "Supply Pod", SupportAction.Supply, 0);
                if (squad >= 0)
                    AddTile(_pods, podIndex++, TroopDrop, "MercenaryPod",
                        "Mercenary Pod · " + DropSquads[squad].Name, SupportAction.Troops, squad);
                if (podIndex == 0)
                    EmptyGroup(_pods, "No pods installed");

                _handle = CreateSupportRect(_panel, "Pull tab", new Vector2(1f, 0f),
                    new Vector2(118f, HandleHeight), new Vector2(-12f, 0f));
                _handle.pivot = new Vector2(1f, 1f);
                _toggle = _handle.gameObject.AddComponent<ShipSupportButton>();
                _toggle.Configure(null, "▼ CALL-INS", true, Toggle);

                _tooltip = TopRect(transform, "Call-in details", new Vector2(326f, 60f));
                var tooltipBackground = Paint(_tooltip, new Color32(16, 29, 23, 250));
                tooltipBackground.raycastTarget = false;
                _tooltipText = Label(_tooltip, "Details", "", 12,
                    new Vector2(8f, 4f), new Vector2(310f, 52f));
                _tooltipText.horizontalOverflow = HorizontalWrapMode.Wrap;
                _tooltip.gameObject.SetActive(false);
                Layout();
                PositionPanel();
                SetContentActive(false);
                Update();
            }

            private static float GroupWidth(int count) => count == 0 ? 132f :
                Math.Max(112f, 2f * Padding + count * TileSize + (count - 1) * TileGap);

            private RectTransform AddGroup(string caption, float width)
            {
                var group = At(_panel, caption, 0f, 0f, width, GroupHeight);
                Paint(group, new Color32(16, 29, 23, 255));
                Label(group, "Category", caption, 13, new Vector2(Padding, 2f), new Vector2(width - 2f * Padding, 22f));
                return group;
            }

            private static void EmptyGroup(RectTransform group, string caption) =>
                Label(group, "No call-ins", caption, 12, new Vector2(Padding, 28f),
                    new Vector2(group.sizeDelta.x - 2f * Padding, 40f));

            private void AddTile(RectTransform group, int index, string perkId, string art,
                string caption, SupportAction action, int choice)
            {
                var rect = At(group, caption, Padding + index * (TileSize + TileGap), 24f, TileSize, TileSize);
                var tile = rect.gameObject.AddComponent<SupportCallInTile>();
                // Use the research descriptor itself so any icon replacement follows it.
                var descriptor = Data.MagnumPerks.GetRecord(perkId)?.ContentDescriptor as MagnumPerkDescriptor;
                var icon = descriptor != null && descriptor._icon != null ? descriptor._icon : SupportIcon(art);
                string reserve = action == SupportAction.Fire ? "Shared weapon reserve" : "Shared pod reserve";
                if (action == SupportAction.Troops)
                {
                    int count = TroopPodCapacity(_state.Get<MagnumProgression>());
                    reserve = count + (count == 1 ? " mercenary" : " mercenaries") + " per call · " + reserve;
                }
                tile.Configure(icon, caption, () => Activate(action, choice),
                    text => ShowDetails(text == null ? null : text + "\n" + reserve));
                _tiles.Add((action, choice, tile));
            }

            private void Toggle()
            {
                _shipSupportConsumedFrame = Time.frameCount;
                if (!CanShow())
                    return;
                _expanded = !_expanded;
                _toggle.SetLabel(_expanded ? "▲ CALL-INS" : "▼ CALL-INS");
                if (_expanded)
                {
                    transform.SetAsLastSibling();
                    SetContentActive(true);
                }
                ClearHover();
            }

            private void Collapse()
            {
                _expanded = false;
                _toggle.SetLabel("▼ CALL-INS");
                ClearHover();
            }

            private void Activate(SupportAction action, int choice)
            {
                // Reserve this release before opening the native target view. It must
                // not also confirm a target underneath the tile in the same frame.
                _shipSupportConsumedFrame = Time.frameCount;
                if (!_expanded || !CanShow() || !CanRequestSupport(_state, action, choice))
                    return;
                Collapse();
                StartSupportTargeting(_state, action, choice);
                SetVisible(CanShow());
            }

            private bool CanShow() => isActiveAndEnabled && _state != null && IsSupportRaid(_state) &&
                !UI.IsAnyShowing(typeof(DungeonHudScreen));

            public bool ConsumesNativeInput()
            {
                // This fresh check also yields to native menus opened after Update.
                if (!CanShow())
                {
                    _ownedMouseButtons = 0;
                    SetVisible(false);
                    return false;
                }
                var input = SingletonMonoBehaviour<InputController>.Instance;
                if (input.IsKeyDown("Back", "UI") || input.IsKeyDown("Menu"))
                {
                    if (_expanded)
                    {
                        _shipSupportConsumedFrame = Time.frameCount;
                        Collapse();
                        return true;
                    }
                    // Resting the cursor over the closed handle must not prevent
                    // Escape from opening the game's normal menu.
                    if (_ownedMouseButtons == 0)
                        return false;
                }
                if (input.Mode != InputController.InputMode.KeyboardAndMouse)
                    return false;

                bool over = PointerOverDrawer();
                bool owned = _ownedMouseButtons != 0;
                for (int button = 0; button < 3; button++)
                {
                    int bit = 1 << button;
                    if (over && Input.GetMouseButtonDown(button))
                        _ownedMouseButtons |= bit;
                    if ((_ownedMouseButtons & bit) != 0 && !Input.GetMouseButton(button))
                    {
                        _ownedMouseButtons &= ~bit;
                        _shipSupportConsumedFrame = Time.frameCount;
                    }
                }
                return over || owned || _ownedMouseButtons != 0 || _shipSupportConsumedFrame == Time.frameCount;
            }

            private bool PointerOverDrawer()
            {
                Camera camera = _canvas != null && _canvas.renderMode != RenderMode.ScreenSpaceOverlay
                    ? _canvas.worldCamera : null;
                Vector2 pointer = Input.mousePosition;
                // Clip the hit area to the visible portion of the moving body. When
                // closed, only the thin rim and pull tab own screen input.
                return RectTransformUtility.RectangleContainsScreenPoint(_clip, pointer, camera) &&
                    (RectTransformUtility.RectangleContainsScreenPoint(_panel, pointer, camera) ||
                     RectTransformUtility.RectangleContainsScreenPoint(_handle, pointer, camera));
            }

            private void Update()
            {
                if (_state == null)
                    return;
                if (!IsSupportRaid(_state))
                {
                    SetVisible(false);
                    return;
                }
                Layout();
                SetVisible(CanShow());
                float previousSlide = _slide;
                _slide = Mathf.MoveTowards(_slide, _expanded ? 1f : 0f, Time.unscaledDeltaTime * 6f);
                if (_slide != previousSlide)
                {
                    PositionPanel();
                    if (_slide == 0f)
                        SetContentActive(false);
                }
                bool targetable = _visible && CanTargetSupport(_state);
                bool fireReady = targetable && !HasPendingBarrage(_state);
                int fire = SupportCharges(_state, SupportAction.Fire);
                int pods = SupportCharges(_state, SupportAction.Supply);
                foreach (var entry in _tiles)
                    entry.Tile.SetCharges(entry.Action == SupportAction.Fire ? fire : pods,
                        entry.Action == SupportAction.Fire ? fireReady : targetable);
            }

            private void SetVisible(bool visible)
            {
                if (_visibility == null)
                    return;
                _visibility.alpha = visible ? 1f : 0f;
                _visibility.blocksRaycasts = visible;
                _visibility.interactable = visible;
                if (_visible && !visible)
                {
                    _ownedMouseButtons = 0;
                    ClearHover();
                }
                _visible = visible;
                // Native Navigation checks interactability, not CanvasGroup alpha.
                _toggle?.SetInteractable(visible);
            }

            private void Layout()
            {
                var parent = transform.parent as RectTransform;
                float available = parent == null ? Screen.width : parent.rect.width;
                if (available <= 0f || Mathf.Approximately(available, _parentWidth))
                    return;
                _parentWidth = available;
                float horizontalWidth = _weaponsWidth + _podsWidth + 3f * Padding;
                bool stacked = horizontalWidth > available - 2f * Padding;
                float width = stacked ? Math.Max(_weaponsWidth, _podsWidth) + 2f * Padding : horizontalWidth;
                _panelHeight = stacked ? 2f * GroupHeight + 3f * Padding : GroupHeight + 2f * Padding;
                ((RectTransform)transform).sizeDelta = new Vector2(width, _panelHeight + HandleHeight);
                _clip.sizeDelta = new Vector2(width, _panelHeight + HandleHeight);
                _panel.sizeDelta = new Vector2(width, _panelHeight);
                _weapons.anchoredPosition = new Vector2(Padding, -Padding);
                _pods.anchoredPosition = stacked ? new Vector2(Padding, -GroupHeight - 2f * Padding) :
                    new Vector2(_weaponsWidth + 2f * Padding, -Padding);
                _tooltip.sizeDelta = new Vector2(Math.Max(280f, width), 60f);
                _tooltip.anchoredPosition = new Vector2(0f, -_panelHeight - HandleHeight - 4f);
                _tooltipText.rectTransform.sizeDelta = new Vector2(_tooltip.sizeDelta.x - 16f, 52f);
                PositionPanel();
            }

            private void PositionPanel()
            {
                float eased = _slide * _slide * (3f - 2f * _slide);
                _panel.anchoredPosition = new Vector2(0f, (_panelHeight - 2f) * (1f - eased));
            }

            private void SetContentActive(bool active)
            {
                _weapons.gameObject.SetActive(active);
                _pods.gameObject.SetActive(active);
            }

            private void ShowDetails(string text)
            {
                if (_tooltip == null)
                    return;
                bool show = !string.IsNullOrEmpty(text) && _expanded && _visible && CanShow();
                _tooltip.gameObject.SetActive(show);
                if (show)
                    _tooltipText.text = text;
            }

            private void ClearHover()
            {
                foreach (var entry in _tiles)
                    if (entry.Tile != null)
                        entry.Tile.ClearHover();
                _tooltip?.gameObject.SetActive(false);
            }

            private void OnDisable()
            {
                _ownedMouseButtons = 0;
                ClearHover();
            }

            private void OnDestroy()
            {
                if (_shipSupportHud == this)
                    _shipSupportHud = null;
            }

            private static RectTransform TopRect(Transform parent, string name, Vector2 size)
            {
                var rect = CreateSupportRect(parent, name, new Vector2(0.5f, 1f), size, Vector2.zero);
                rect.pivot = new Vector2(0.5f, 1f);
                return rect;
            }

            private static RectTransform At(Transform parent, string name, float x, float y, float width, float height)
            {
                var rect = CreateSupportRect(parent, name, new Vector2(0f, 1f),
                    new Vector2(width, height), new Vector2(x, -y));
                rect.pivot = new Vector2(0f, 1f);
                return rect;
            }

            private static Image Paint(RectTransform rect, Color color)
            {
                var image = rect.gameObject.AddComponent<Image>();
                image.color = color;
                return image;
            }

            private static Text Label(Transform parent, string name, string caption, int size, Vector2 position, Vector2 bounds)
            {
                var text = At(parent, name, position.x, position.y, bounds.x, bounds.y).gameObject.AddComponent<Text>();
                text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                text.fontSize = size;
                text.color = new Color32(197, 214, 189, 255);
                text.text = caption;
                text.alignment = TextAnchor.MiddleLeft;
                text.raycastTarget = false;
                return text;
            }
        }
    }
}

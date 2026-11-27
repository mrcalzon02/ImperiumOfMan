using System;
using HarmonyLib;
using MGSC;
using UnityEngine;
using UnityEngine.UI;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        [HarmonyPatch(typeof(SpaceshipScreen), "InitModulePoints")]
        private static class AddSupportModulePoints
        {
            private static void Prefix(Spaceship ____spaceship)
            {
                Add(WeaponsModule, "engineer");
                Add(DropModule, "engineer");
                // The native point builder enumerates every registered perk module,
                // even a module also reached from a nested research tab.
                Add(SupplyModule, "engineer");
                Add(CommsModule, "engineer");

                void Add(string id, string anchor)
                {
                    ____spaceship.UiAnchors[id] = ____spaceship.UiAnchors[anchor];
                    if (!____spaceship.ModuleIds.Contains(id))
                    {
                        ____spaceship.ModuleIds.Add(id);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Spaceship), nameof(Spaceship.SetActiveModuleCamera))]
        private static class SupportModuleCamera
        {
            private static void Prefix(ref string moduleId)
            {
                moduleId = moduleId switch
                {
                    WeaponsModule or DropModule or SupplyModule or CommsModule => "engineer",
                    _ => moduleId
                };
            }
        }

        [HarmonyPatch(typeof(MagnumModulePoint), "SnapToWorldBone")]
        private static class OffsetSupportModulePoint
        {
            private static void Postfix(MagnumModulePoint __instance)
            {
                if (__instance.ModuleId is not (WeaponsModule or DropModule or SupplyModule or CommsModule))
                {
                    return;
                }
                // Native SnapToWorldBone resets localPosition every frame. These offsets are
                // canvas units relative to Engineering, so they follow the ship and UI scale.
                Vector3 offset = __instance.ModuleId switch
                {
                    WeaponsModule => new Vector3(-80f, 0f, 0f),
                    CommsModule => new Vector3(32f, -46f, 0f),
                    DropModule => new Vector3(0f, 64f, 0f),
                    SupplyModule => new Vector3(80f, 64f, 0f),
                    _ => Vector3.zero
                };
                __instance.transform.localPosition += offset;
            }
        }

        [HarmonyPatch(typeof(MagnumModuleTechTreeWindow), "RefreshGrid")]
        private static class SupportLoadoutPanel
        {
            private static void Postfix(MagnumModuleTechTreeWindow __instance, string ____moduleId,
                MagnumProgression ____magnumSpaceship)
            {
                Transform existing = __instance.transform.Find("ImperialShipSupportLoadout");
                ConfigureSupplyPodPage(__instance, ____moduleId, ____magnumSpaceship);
                if (____moduleId is not (WeaponsModule or DropModule))
                {
                    existing?.gameObject.SetActive(false);
                    return;
                }
                var game = SingletonMonoBehaviour<SpaceGameMode>.Instance;
                var triggers = game.Get<StoryTriggers>();
                bool weapons = ____moduleId == WeaponsModule;
                int[] options = weapons ? AvailableMounts(____magnumSpaceship) :
                    AvailableSquads(____magnumSpaceship, game.Get<Factions>());
                string key = weapons ? SelectedMount : SelectedSquad;
                int selected = SupportSelection(triggers, key, options, newest: !weapons);
                var panel = existing == null
                    ? CreateSupportRect(__instance.transform, "ImperialShipSupportLoadout", new Vector2(0.5f, 0f),
                        new Vector2(246f, 32f), new Vector2(0f, 24f))
                    : (RectTransform)existing;
                // Reserve the right-hand half for the Supply Pod research tab.
                panel.sizeDelta = new Vector2(weapons ? 246f : 142f, 32f);
                panel.anchoredPosition = new Vector2(weapons ? 0f : -52f, 24f);
                var control = panel.GetComponent<ShipSupportButton>() ?? panel.gameObject.AddComponent<ShipSupportButton>();
                control.Configure(null, Label(), options.Length > 1, Cycle);
                panel.gameObject.SetActive(true);

                string Label() => selected < 0 ? (weapons ? "Install a weapon mount" : "Install the Mercenary Pod") :
                    (weapons ? "Mount · " + ShipMounts[selected].Name : "Squad · " + DropSquads[selected].Name) +
                    (options.Length > 1 ? "  ›" : string.Empty);

                void Cycle()
                {
                    selected = options[(Array.IndexOf(options, selected) + 1) % options.Length];
                    triggers.FloatMap[key] = selected;
                    control.SetLabel(Label());
                }
            }
        }

        [Hook(ModHookType.DungeonStarted)]
        public static void AddShipSupportHud(IModContext context)
        {
            CreateShipSupportDrawer(context.State);
        }

        private static RectTransform CreateSupportRect(Transform parent, string name, Vector2 anchor,
            Vector2 size, Vector2 offset)
        {
            var rect = (RectTransform)new GameObject(name, typeof(RectTransform)).transform;
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = anchor;
            rect.sizeDelta = size;
            rect.anchoredPosition = offset;
            return rect;
        }

        private sealed class ShipSupportButton : CommonButton
        {
            private Text _label;
            private Action _action;
            private string _caption;
            private Image _icon;

            private void ResetVisual()
            {
                background.color = IsInteractable ? new Color32(22, 54, 44, 246) : new Color32(19, 29, 26, 234);
                _label.color = IsInteractable ? new Color32(196, 226, 208, 255) : new Color32(136, 158, 147, 255);
            }

            public override void Awake()
            {
                EnsureVisuals();
                base.Awake();
            }

            private void EnsureVisuals()
            {
                if (_label != null)
                {
                    return;
                }
                background = gameObject.AddComponent<Image>();
                _icon = CreateSupportRect(transform, "Icon", new Vector2(0f, 0.5f), new Vector2(24f, 24f),
                    new Vector2(15f, 0f)).gameObject.AddComponent<Image>();
                _icon.raycastTarget = false;
                var textRect = CreateSupportRect(transform, "Label", Vector2.zero, Vector2.zero, Vector2.zero);
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = new Vector2(6f, 0f);
                textRect.offsetMax = new Vector2(-6f, 0f);
                _label = textRect.gameObject.AddComponent<Text>();
                _label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                _label.fontSize = 13;
                _label.resizeTextForBestFit = true;
                _label.resizeTextMinSize = 10;
                _label.resizeTextMaxSize = 13;
                _label.alignment = TextAnchor.MiddleCenter;
                _label.raycastTarget = false;
                OnClick += (_, _) => _action?.Invoke();
            }

            public void Configure(Sprite icon, string caption, bool enabled, Action action)
            {
                EnsureVisuals();
                _action = action;
                _icon.sprite = icon;
                _icon.gameObject.SetActive(icon != null);
                _label.rectTransform.offsetMin = new Vector2(icon == null ? 6f : 31f, 0f);
                SetLabel(caption);
                SetInteractable(enabled);
                ResetVisual();
            }

            public void SetLabel(string caption) => _label.text = _caption = caption;

            public void SetStatus(string caption, bool available)
            {
                if (_caption != caption)
                {
                    SetLabel(caption);
                }
                if (IsInteractable != available)
                {
                    SetInteractable(available);
                    ResetVisual();
                }
            }

            public void SetCharges(int remaining, bool available)
            {
                _label.text = $"{_caption}  [{remaining}]";
                SetInteractable(available && remaining > 0);
                ResetVisual();
            }
        }
    }
}

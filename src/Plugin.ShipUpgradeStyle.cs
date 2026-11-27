using System.Collections.Generic;
using HarmonyLib;
using MGSC;
using UnityEngine;
using UnityEngine.UI;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        // RefreshGrid copies panel positions to the native navigation cells before
        // calling Initialize. Restore pooled geometry before that copy, not just
        // during Initialize, so a noncentral pivot cannot accumulate drift.
        [HarmonyPatch(typeof(MagnumModuleTechTreeWindow), "RefreshGrid")]
        private static class RestoreCustomShipUpgradeGrid
        {
            private static void Prefix(List<MagnumTechTreePanel> ____techPanels)
            {
                foreach (var panel in ____techPanels)
                    panel.GetComponent<ShipUpgradeNodeGeometry>()?.Restore();
            }
        }

        [HarmonyPatch(typeof(MagnumTechTreePanel), nameof(MagnumTechTreePanel.Initialize))]
        private static class SizeCustomShipUpgradeNodes
        {
            private static void Prefix(MagnumTechTreePanel __instance)
            {
                // The native tree reuses these cells for every module. Restore the
                // original geometry before native code assigns the next node's sprite.
                __instance.GetComponent<ShipUpgradeNodeGeometry>()?.Restore();
            }

            private static void Postfix(MagnumTechTreePanel __instance, MagnumPerkRecord __0, bool __1, bool __2,
                Image ____icon, Image ____upgradeAvailableIcon, Image ____blackCover, CommonButton ____button)
            {
                var perkRecord = __0;
                if (perkRecord.ModuleId is not (WeaponsModule or DropModule or CommsModule or OccultModule or SupplyModule))
                {
                    return;
                }

                bool major = (perkRecord.ModuleId == OccultModule && !IsOccultProductionUpgrade(perkRecord.Id)) ||
                    IsSupplyBranchMajor(perkRecord.Id) ||
                    perkRecord.Id is SupplyDrop or TroopDrop or SecurityIntrusion or EntranceOverride or ExtractionOverride ||
                    perkRecord.ModuleId == WeaponsModule &&
                    perkRecord.Id is not (BarrageDuration or BarrageRounds or BarrageCalls);
                var geometry = __instance.GetComponent<ShipUpgradeNodeGeometry>() ??
                    __instance.gameObject.AddComponent<ShipUpgradeNodeGeometry>();
                geometry.Apply(__instance, ____icon, ____upgradeAvailableIcon, ____blackCover, ____button,
                    major, perkRecord.Id, __1 || __2);
            }
        }

        private sealed class ShipUpgradeNodeGeometry : MonoBehaviour
        {
            private readonly List<NodeRect> _original = new();
            public string PerkId { get; private set; }
            public bool PurchasedOrAvailable { get; private set; }

            private sealed class NodeRect(RectTransform rect)
            {
                public readonly RectTransform Rect = rect;
                public readonly Vector2 Size = rect.sizeDelta;
                public readonly Vector3 Position = rect.anchoredPosition3D;
                public readonly Vector3 Scale = rect.localScale;
            }

            public void Restore()
            {
                foreach (var original in _original)
                {
                    if (original.Rect == null)
                    {
                        continue;
                    }
                    original.Rect.sizeDelta = original.Size;
                    original.Rect.anchoredPosition3D = original.Position;
                    original.Rect.localScale = original.Scale;
                }
                _original.Clear();
                PerkId = null;
                PurchasedOrAvailable = false;
            }

            private static Vector3 Center(RectTransform rect) =>
                rect.TransformPoint(rect.rect.center);

            private static void CenterOn(RectTransform rect, Vector3 center)
            {
                rect.position += center - Center(rect);
            }

            public void Apply(MagnumTechTreePanel panel, Image icon, Image upgradeMarker,
                Image blackCover, CommonButton button, bool major, string perkId, bool purchasedOrAvailable)
            {
                var root = panel.transform as RectTransform;
                if (root == null || icon == null) return;
                PerkId = perkId;
                PurchasedOrAvailable = purchasedOrAvailable;
                var rects = panel.GetComponentsInChildren<RectTransform>(true);
                foreach (var rect in rects) _original.Add(new NodeRect(rect));

                // The slot's center is invariant under sprite size, anchors and pivot.
                // Fit every painted/clickable layer to that same center. Do not
                // multiply independent anchor offsets: nested transforms would
                // otherwise shrink twice or move to different centers.
                Vector3 center = Center(root);
                var tileLayers = new HashSet<RectTransform> { icon.rectTransform };
                if (blackCover != null) tileLayers.Add(blackCover.rectTransform);
                if (button != null)
                {
                    if (button.transform is RectTransform buttonRect) tileLayers.Add(buttonRect);
                    if (button.background != null) tileLayers.Add(button.background.rectTransform);
                }

                // Unity returns parents before children, so correcting a parent
                // cannot displace an already-corrected child. Leave the grid root's
                // native dimensions untouched.
                foreach (var rect in rects)
                {
                    if (rect == root || !tileLayers.Contains(rect)) continue;
                    rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 36f);
                    rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 36f);
                    CenterOn(rect, center);
                }

                // Scale the whole tile once, including its hover/raycast geometry.
                // Keep the root's actual center fixed even for a noncentral pivot.
                float scale = major ? 1f : 0.5f;
                root.localScale = Vector3.Scale(root.localScale, new Vector3(scale, scale, 1f));
                CenterOn(root, center);

                // Retain the native yellow upgrade marker's displayed size.
                if (upgradeMarker != null && upgradeMarker.rectTransform != root)
                {
                    var marker = upgradeMarker.rectTransform;
                    marker.localScale = Vector3.Scale(marker.localScale, new Vector3(1f / scale, 1f / scale, 1f));
                }
            }
        }


    }
}

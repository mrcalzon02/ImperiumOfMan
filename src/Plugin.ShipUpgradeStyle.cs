using System.Collections.Generic;
using HarmonyLib;
using MGSC;
using UnityEngine;
using UnityEngine.UI;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
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
                Image ____icon, Image ____upgradeAvailableIcon)
            {
                var perkRecord = __0;
                if (perkRecord.ModuleId is not (WeaponsModule or DropModule or CommsModule or OccultModule))
                {
                    return;
                }

                bool major = perkRecord.ModuleId == OccultModule ||
                    perkRecord.Id is SupplyDrop or TroopDrop or SecurityIntrusion or EntranceOverride or ExtractionOverride ||
                    perkRecord.ModuleId == WeaponsModule &&
                    perkRecord.Id is not (BarrageDuration or BarrageRounds or BarrageCalls);
                var geometry = __instance.GetComponent<ShipUpgradeNodeGeometry>() ??
                    __instance.gameObject.AddComponent<ShipUpgradeNodeGeometry>();
                geometry.Apply(__instance, ____icon, ____upgradeAvailableIcon, major, perkRecord.Id, __1 || __2);
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
                public readonly Vector2 Position = rect.anchoredPosition;
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
                    original.Rect.anchoredPosition = original.Position;
                }
                _original.Clear();
                PerkId = null;
                PurchasedOrAvailable = false;
            }

            public void Apply(MagnumTechTreePanel panel, Image icon, Image upgradeMarker, bool major,
                string perkId, bool purchasedOrAvailable)
            {
                PerkId = perkId;
                PurchasedOrAvailable = purchasedOrAvailable;
                float scale = major ? 1f : 0.5f;
                // Include the button and hover rectangles, not only the painted icon.
                // Stretch-anchored children follow their parent's new dimensions.
                foreach (var rect in panel.GetComponentsInChildren<RectTransform>(true))
                {
                    _original.Add(new NodeRect(rect));
                    if (rect.anchorMin != rect.anchorMax)
                    {
                        continue;
                    }
                    if (rect != panel.transform)
                    {
                        rect.anchoredPosition *= scale;
                    }
                    if (upgradeMarker == null || rect != upgradeMarker.rectTransform)
                    {
                        rect.sizeDelta *= scale;
                    }
                }
                // Keep the native yellow notification at its normal size while its
                // anchor follows the smaller node. Main nodes retain the full icon.
                icon.rectTransform.sizeDelta = major ? new Vector2(36f, 36f) : new Vector2(18f, 18f);
            }
        }


    }
}

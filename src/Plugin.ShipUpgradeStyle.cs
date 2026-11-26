using System.Collections.Generic;
using System.Linq;
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
                    perkRecord.Id is SupplyDrop or TroopDrop or SecurityIntrusion ||
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

        [HarmonyPatch(typeof(MagnumModuleTechTreeWindow), "RefreshGrid")]
        private static class LinkSharedWeaponUpgrades
        {
            private static void Postfix(MagnumModuleTechTreeWindow __instance, string ____moduleId,
                MagnumProgression ____magnumSpaceship)
            {
                if (____moduleId != WeaponsModule)
                {
                    return;
                }

                bool fitted = ShipMounts.Any(mount => ____magnumSpaceship.IsPerkPurchased(mount.Id));
                var nodes = __instance.GetComponentsInChildren<ShipUpgradeNodeGeometry>(true);
                string[] upgrades = [BarrageDuration, BarrageRounds, BarrageCalls];
                for (int column = 1; column <= 3; column++)
                {
                    AddLink(column, 0, column, 1,
                        !____magnumSpaceship.IsPerkPurchased(ShipMounts[column - 1].Id));
                    // Reuse the native node's result so the branch also reflects the
                    // individual upgrade's current availability, not just its prerequisite.
                    bool available = nodes.Any(node => node.PerkId == upgrades[column - 1] &&
                        node.PurchasedOrAvailable);
                    AddLink(column, 1, column, 2, !fitted || !available);
                    if (column < 3)
                    {
                        AddLink(column, 1, column + 1, 1, !fitted);
                    }
                }

                void AddLink(int fromColumn, int fromRow, int toColumn, int toRow, bool locked)
                {
                    // Drawing coordinates only: these records are never registered,
                    // purchased or added to Parents/Childs. Any fitted mount still
                    // unlocks all three shared upgrades through the existing gate.
                    var from = new MagnumPerkRecord { PositionInGraph = new CellPosition(fromColumn, fromRow) };
                    var to = new MagnumPerkRecord { PositionInGraph = new CellPosition(toColumn, toRow) };
                    __instance.AddArrow(from, to, locked);
                }
            }
        }
    }
}

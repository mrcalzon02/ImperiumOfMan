using HarmonyLib;
using MGSC;
using UnityEngine;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        // Current production views are plain MonoBehaviours enabled by MGSC.UI.
        // Observe each concrete OnEnable and let a companion track deactivation.
        [HarmonyPatch(typeof(MagnumProductionWindow), "OnEnable")]
        private static class ShowProductionCraftingMap
        {
            private static void Postfix(MagnumProductionWindow __instance) => BindCraftingMapOwner(__instance);
        }

        [HarmonyPatch(typeof(MagnumSelectItemToProduceWindow), "OnEnable")]
        private static class ShowRecipePickerCraftingMap
        {
            private static void Postfix(MagnumSelectItemToProduceWindow __instance) => BindCraftingMapOwner(__instance);
        }

        private static void BindCraftingMapOwner(Component owner)
        {
            if (owner == null)
                return;
            var lifecycle = owner.GetComponent<CraftingMapProductionOwner>();
            if (lifecycle == null)
                lifecycle = owner.gameObject.AddComponent<CraftingMapProductionOwner>();
            lifecycle.Bind(owner);
            EnsureCraftingMapTab(owner);
        }

        private sealed class CraftingMapProductionOwner : MonoBehaviour
        {
            private Component _owner;

            public void Bind(Component owner) => _owner = owner;

            private void OnDisable() => Release();
            private void OnDestroy() => Release();

            private void Release()
            {
                // Unity's destroyed-object null comparison must not turn delayed
                // cleanup into an unqualified close of another production view.
                if (_owner != null)
                    RemoveCraftingMapTab(_owner);
            }
        }
    }
}

using HarmonyLib;
using MGSC;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        // Keep the tab with Production and its recipe picker. Native transitions
        // hide the previous owner before showing the next one.
        [HarmonyPatch(typeof(MagnumProductionWindow), nameof(MagnumProductionWindow.Show), [])]
        private static class ShowProductionCraftingMap
        {
            private static void Postfix(MagnumProductionWindow __instance) => EnsureCraftingMapTab(__instance);
        }

        [HarmonyPatch(typeof(MagnumProductionWindow), nameof(MagnumProductionWindow.Hide), [])]
        private static class HideProductionCraftingMap
        {
            private static void Prefix(MagnumProductionWindow __instance) => RemoveCraftingMapTab(__instance);
        }

        [HarmonyPatch(typeof(MagnumSelectItemToProduceWindow), nameof(MagnumSelectItemToProduceWindow.Show), [typeof(int)])]
        private static class ShowRecipePickerCraftingMap
        {
            private static void Postfix(MagnumSelectItemToProduceWindow __instance) => EnsureCraftingMapTab(__instance);
        }

        [HarmonyPatch(typeof(MagnumSelectItemToProduceWindow), nameof(MagnumSelectItemToProduceWindow.Hide), [])]
        private static class HideRecipePickerCraftingMap
        {
            private static void Prefix(MagnumSelectItemToProduceWindow __instance) => RemoveCraftingMapTab(__instance);
        }
    }
}

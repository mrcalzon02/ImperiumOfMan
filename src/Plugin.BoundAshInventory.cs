using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using MGSC;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        [HarmonyPatch(typeof(PickupItem), nameof(PickupItem.OnAfterLoad))]
        private static class BoundAshInventoryLoadPatch
        {
            internal static readonly List<PickupItem> Loaded = new();
            private static readonly PropertyInfo InventoryWidth =
                AccessTools.Property(typeof(BasePickupItem), nameof(BasePickupItem.InventoryWidthSize));
            internal static readonly FieldInfo FactoryState = AccessTools.Field(typeof(ItemFactory), "_state");

            private static void Postfix(PickupItem __instance)
            {
                if (__instance.Id != BoundAsh)
                {
                    return;
                }

                // Width and stack maximum are saved on the instance, not refreshed from its record.
                InventoryWidth.SetValue(__instance, 1, null);
                Loaded.Add(__instance);
            }
        }

        [Hook(ModHookType.AfterSaveLoaded)]
        public static void RestoreBoundAshAfterSave(IModContext context) => RestoreBoundAshInventory();

        [Hook(ModHookType.AfterSpaceLoaded)]
        public static void RestoreBoundAshAfterSpace(IModContext context) => RestoreBoundAshInventory();

        [Hook(ModHookType.AfterDungeonLoaded)]
        public static void RestoreBoundAshAfterDungeon(IModContext context) => RestoreBoundAshInventory();

        private static void RestoreBoundAshInventory()
        {
            var loaded = BoundAshInventoryLoadPatch.Loaded;
            if (loaded.Count == 0)
            {
                return;
            }

            // Difficulty is available here; use the same stack limit as newly created items.
            var record = Data.Items.GetSimpleRecord<PactComponentRecord>(BoundAsh);
            var factory = SingletonMonoBehaviour<ItemFactory>.Instance;
            if (record == null || factory == null ||
                BoundAshInventoryLoadPatch.FactoryState?.GetValue(factory) is not State state ||
                state.Get<Difficulty>()?.Preset == null)
            {
                // Keep the queue for the later space/dungeon hook if the factory is not ready yet.
                return;
            }
            short maxStack = factory.GetMaxStackSize(record);
            foreach (PickupItem item in loaded)
            {
                if (item.Comp<StackableItemComponent>() == null)
                {
                    item.Add(new StackableItemComponent(maxStack) { Count = item.StackCount });
                }
                else
                {
                    item.SetMaxStack(maxStack);
                }
            }
            loaded.Clear();
        }
    }
}

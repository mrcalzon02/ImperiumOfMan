using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HarmonyLib;
using MGSC;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private const string OreCargo = "ore_cargo";
        private static readonly Dictionary<string, ItemQuantity[]> ShredProfiles = new();
        private static readonly Dictionary<string, int> ShredInputBatches = new();

        private static int GetShredInputBatch(string id) =>
            ShredInputBatches.TryGetValue(id, out int batch) ? batch : 1;

        private static bool IsEligibleShredInput(BasePickupItem item) =>
            item != null && item.StackCount > 0 && !item.Locked && !ItemInteractionSystem.IsQuestItem(item);

        private static int CountSelectedShredInput(MagnumCargo cargo, string id) =>
            cargo?.RecyclingStorage?.Items.Where(item => item.Id == id && IsEligibleShredInput(item))
                .Sum(item => (int)item.StackCount) ?? 0;

        private static bool HasRecyclableSelection(MagnumCargo cargo)
        {
            if (cargo?.RecyclingStorage == null) return false;
            foreach (BasePickupItem item in cargo.RecyclingStorage.Items)
            {
                int inputBatch = GetShredInputBatch(item.Id);
                if (ShredProfiles.ContainsKey(item.Id))
                {
                    if (IsEligibleShredInput(item) &&
                        (inputBatch == 1 || CountSelectedShredInput(cargo, item.Id) >= inputBatch))
                        return true;
                }
                else if (Data.Items.GetSimpleRecord<ItemRecord>(item.Id)?.CanDisassembly == true)
                {
                    // Keep the current native button criterion for native-only items.
                    return true;
                }
            }
            return false;
        }

        private static void RegisterShredding()
        {
            Registrations.Add(() =>
            {
                var cargo = Data.Items.GetSimpleRecord<TrashRecord>(OreCargo)
                    ?? throw new InvalidDataException("Missing native Ore Cargo record");
                cargo.Disassembly = [new(Ore, 12), new(Silicate, 6), new(Conductors, 6)];
            });

            // Recovered scrap remains useful without making it the mandatory source of raw materials.
            AddShredProfile([new(Ore, 1)], MetalScrap);
            AddShredProfile([new(Silicate, 1)], Cullet);
            AddShredProfile([new(PolymerStock, 1)], Polymer);
            AddShredProfile([new(Fiber, 1)], _lightBoots.Id, _lightLeggings.Id, _lightHelmet.Id,
                _cadiaBoots.Id, _cadiaLeggings.Id, _cadiaHelmet.Id, WinterCap, WinterTrousers, WinterBoots);
            AddShredProfile([new(Fiber, 1), new(MetalScrap, 1)], _lightChestplate.Id, _cadiaChestplate.Id, ArbitesHelmet,
                ArbitesGreaves, ArbitesBoots, "rwa_light_helmet_1", "rwa_light_boots_1", "rwa_light_pants_1");
            AddShredProfile([new(MetalScrap, 2), new(Fiber, 1)], ArbitesCuirass, SubductorCuirass, "rwa_light_armor_1");
            AddShredProfile([new(MetalScrap, 2), new(Polymer, 1)], _lasgun.Id, Judgement, _bolter.Id, _longLas.Id, WinterLasgun);
            AddShredProfile([new(Fiber, 2), new(Polymer, 1)], WinterCoat);
            AddShredProfile([new(Polymer, 1), new(Fiber, 1)], LegionTrousers, LegionBoots);
            AddShredProfile([new(MetalScrap, 1), new(Polymer, 1)], LegionMask, LegionCoat);
            AddShredProfile([new(MetalScrap, 2), new(Polymer, 1)], LegionCarbine);
            AddShredProfile([new(MetalScrap, 1), new(Conductors, 1)], ShockMaul, _plasmaPistol.Id, _melta.Id);
            AddShredProfile([new(MetalScrap, 1), new(Polymer, 1)], _servitorBackpack.Id);
            AddShredProfile([new(Cullet, 1)], GlassStock);
            AddShredProfile([new(Cullet, 1), new(Conductors, 1)], Cell);
            AddShredProfile([new(Polymer, 1)], Housing);
            AddShredProfile([new(MetalScrap, 2), new(Polymer, 1)], AutomataChassis);
            AddShredProfile([new(Conductors, 2), new(Polymer, 1)], LogicCore);
            // Six manufactured rations cannot recover their whole biomass input.
            // The ship recycler combines selected stacks and retains smaller remainders.
            AddShredProfile([new(Biomass, 1)], 8, DirtyStarch);
        }

        [HarmonyPatch(typeof(ItemInteractionSystem), nameof(ItemInteractionSystem.Disassemble))]
        private static class UnpackOreCargo
        {
            private static void Prefix(BasePickupItem item, ref bool guaranteed)
            {
                if (_industryReady && item?.Id == OreCargo && item.StackCount > 0 &&
                    !item.Locked && !ItemInteractionSystem.IsQuestItem(item))
                {
                    // A sealed cargo delivers its contents; native code consumes the selected
                    // crates and handles stack limits, inventory placement, and overflow.
                    guaranteed = true;
                }
            }
        }

        private static void AddShredProfile(ItemQuantity[] outputs, params string[] items) =>
            AddShredProfile(outputs, 1, items);

        private static void AddShredProfile(ItemQuantity[] outputs, int inputBatch, params string[] items)
        {
            if (inputBatch < 1) throw new ArgumentOutOfRangeException(nameof(inputBatch));
            foreach (string id in items)
            {
                ShredProfiles.Add(id, outputs);
                ShredInputBatches.Add(id, inputBatch);
                // A bulk recipe cannot be represented by native per-item Disassembly.
                // In particular, do not advertise one biomass for every spoiled ration.
                if (inputBatch > 1) continue;
                Registrations.Add(() =>
                {
                    // Also enables the native recycle button for new equipment with no field-disassembly record.
                    var record = Data.Items.GetSimpleRecord<ItemRecord>(id);
                    if (!record.CanDisassembly)
                    {
                        record.Disassembly = [outputs[0]];
                    }
                });
            }
        }

        [HarmonyPatch(typeof(MagnumCargoSystem), nameof(MagnumCargoSystem.StartRecycle))]
        private static class RequireCompleteBulkShredBatch
        {
            private static bool Prefix(MagnumCargo magnumCargo)
            {
                if (!_industryReady || magnumCargo == null || magnumCargo.RecyclingInProgress ||
                    !magnumCargo.RecyclingStorage.Items.Any(item => GetShredInputBatch(item.Id) > 1))
                    return true;
                // A partial bulk-only selection has no output. Mixed selections may
                // still recycle their ordinary items while retaining these rations.
                return HasRecyclableSelection(magnumCargo);
            }
        }

        [HarmonyPatch(typeof(MagnumCargoSystem), nameof(MagnumCargoSystem.FinishRecycle))]
        private static class ShredIndustryEquipment
        {
            private static void Prefix(MagnumCargo magnumCargo, out List<BasePickupItem> __state)
            {
                __state = null;
                if (!_industryReady || !magnumCargo.RecyclingInProgress)
                {
                    return;
                }
                var selected = magnumCargo.RecyclingStorage.Items.ToArray();
                foreach (var group in selected.Where(item => GetShredInputBatch(item.Id) > 1)
                    .GroupBy(item => item.Id))
                {
                    int inputBatch = GetShredInputBatch(group.Key);
                    int available = group.Where(IsEligibleShredInput).Sum(item => (int)item.StackCount);
                    int batches = available / inputBatch;
                    int remainingToConsume = batches * inputBatch;
                    __state ??= new List<BasePickupItem>();
                    foreach (ItemQuantity output in ShredProfiles[group.Key])
                    {
                        if (batches > 0)
                            ItemInteractionSystem.CreateItem(__state, output.ItemId, output.Count * batches);
                    }
                    // Protect every bulk-treated stack from native per-item recycling.
                    // Return original remainder objects, preserving decay and other state.
                    foreach (BasePickupItem item in group)
                    {
                        int consumed = IsEligibleShredInput(item)
                            ? Math.Min(remainingToConsume, item.StackCount) : 0;
                        remainingToConsume -= consumed;
                        magnumCargo.RecyclingStorage.Remove(item);
                        if (consumed == 0)
                        {
                            __state.Add(item);
                            continue;
                        }
                        item.StackCount -= (short)consumed;
                        if (item.StackCount > 0)
                        {
                            if (item.IsUsable)
                                item.Comp<UsableItemComponent>()?.ChangeStacksCapacity((short)consumed);
                            __state.Add(item);
                        }
                    }
                }
                foreach (BasePickupItem item in selected)
                {
                    if (GetShredInputBatch(item.Id) > 1 || !ShredProfiles.TryGetValue(item.Id, out var outputs))
                    {
                        continue;
                    }
                    __state ??= new List<BasePickupItem>();
                    if (!IsEligibleShredInput(item))
                    {
                        // Native disassembly does not check Locked. Keep protected
                        // custom-profile items out of that pass, including their ammo.
                        magnumCargo.RecyclingStorage.Remove(item);
                        __state.Add(item);
                        continue;
                    }
                    foreach (ItemQuantity output in outputs)
                    {
                        ItemInteractionSystem.CreateItem(__state, output.ItemId, output.Count * item.StackCount);
                    }
                    if (item.Is<WeaponRecord>())
                    {
                        var unloaded = new ItemStorage(ItemStorageSource.ShipCargo, 10, 10);
                        ItemInteractionSystem.UnloadWeapon(item, null, unloaded);
                        foreach (BasePickupItem ammo in unloaded.Items.ToArray())
                        {
                            unloaded.Remove(ammo);
                            __state.Add(ammo);
                        }
                    }
                    magnumCargo.RecyclingStorage.Remove(item);
                }
            }

            private static void Postfix(MagnumCargo magnumCargo, SpaceTime spaceTime, List<BasePickupItem> __state)
            {
                if (__state == null)
                {
                    return;
                }
                foreach (BasePickupItem output in __state)
                {
                    // Native AddCargo's split-item path skips merging. Preserve the
                    // original remainder/protected item's expiry, capacity and lock state.
                    MagnumCargoSystem.AddCargo(magnumCargo, spaceTime, output, magnumCargo.RecyclingStorage,
                        splittedItem: GetShredInputBatch(output.Id) > 1 || !IsEligibleShredInput(output));
                }
            }
        }
    }
}

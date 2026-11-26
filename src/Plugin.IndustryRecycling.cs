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

        private static void AddShredProfile(ItemQuantity[] outputs, params string[] items)
        {
            foreach (string id in items)
            {
                ShredProfiles.Add(id, outputs);
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
                foreach (BasePickupItem item in magnumCargo.RecyclingStorage.Items.ToArray())
                {
                    if (!ShredProfiles.TryGetValue(item.Id, out var outputs) ||
                        ItemInteractionSystem.IsQuestItem(item) || item.Locked)
                    {
                        continue;
                    }
                    __state ??= new List<BasePickupItem>();
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
                    MagnumCargoSystem.AddCargo(magnumCargo, spaceTime, output, magnumCargo.RecyclingStorage);
                }
            }
        }
    }
}

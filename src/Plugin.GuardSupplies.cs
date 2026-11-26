using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using MGSC;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private const string LasClip = "iom_las_clip";
        private const string CorpseStarch = "iom_corpse_starch";
        private const string DirtyStarch = "iom_dirty_corpse_starch";
        private const string GuardFrag = "iom_frag_grenade";
        private const string LunaReceipt = "iom_luna_provisions";
        private const string PhobosReceipt = "iom_phobos_munitions";
        private static readonly MethodInfo CloneRecordMethod = AccessTools.Method(typeof(object), "MemberwiseClone");
        private static readonly Assembly ModAssembly = typeof(Plugin).Assembly;
        private static readonly Dictionary<string, string> SpriteResources = ModAssembly.GetManifestResourceNames()
            .Where(name => name.StartsWith("ImperiumOfMan.Art.", StringComparison.Ordinal))
            .ToDictionary(name => name, StringComparer.OrdinalIgnoreCase);

        private static void RegisterGuardSupplies()
        {
            AmmoRecord clip = CloneItem<AmmoRecord>(LasClip, "battery_basic_ammo", 1, 5, "LasClip");
            RegisterItem("ammo", clip, "Las Clip", "Standard Guard power cells for Lasguns and other compatible weapons.");

            AddRation(CorpseStarch, "Corpse-starch", "Corpse Starch", 450, 12, false);
            AddRation(DirtyStarch, "Corpse-starch-Dirty", "Dirty Corpse Starch", 260, 4, true);

            GrenadeRecord frag = CloneItem<GrenadeRecord>(GuardFrag, "frag_grenade", 2, 75, "FRAG");
            RegisterItem("grenades", frag, "Guard Frag Grenade", "Standard fragmentation grenade issued to Cadian support troops.");
        }

        private static void AddRation(string id, string artwork, string name, int calories, float price, bool spoiled)
        {
            var ration = CloneItem<ConsumableRecord>(id, "ration_pack_3", 1, price, artwork);
            // Finished rations are production outputs, never raw-material recovery inputs.
            // Use a new list so the native donor's disassembly record stays unchanged.
            ration.Disassembly = [];
            ration.StarvationValue = calories;
            ration.HealthValue = spoiled ? -4 : 0;
            ration.EffectChance = spoiled ? new() { ["poisonEffect"] = 0.25f } : new();
            ration.EffectProgression = spoiled ? new() { ["poisonEffect"] = 18 } : new();
            RegisterItem("consumables", ration, name,
                spoiled ? "A contaminated field ration. It can cause poisoning." : "A compact Imperial field ration.");
        }

        private static void RegisterGuardProduction()
        {
            var clipRecipe = new ItemProduceReceipt
            {
                Id = LasClip,
                OutputItem = LasClip,
                RequiredItems = [new("glass", 1), new("acid", 1)],
                ProduceTimeInHours = 4,
                ModifyStartCost = 1,
                ModifyStep = 1.05f,
                ModifyItemsGrades = []
            };
            Registrations.Add(() => Data.ProduceReceipts.Add(clipRecipe));

            // Unfinished orders in older saves still resolve their outputs by receipt ID.
            AddStationReceipt(LunaReceipt, 36, [new("regular_food_container", 1)],
                [new(CorpseStarch, 6), new(DirtyStarch, 1), new(LasClip, 12)]);
            AddStationReceipt(PhobosReceipt, 48, [new("glass", 2), new("acid", 2)],
                [new(LasClip, 16), new(GuardFrag, 1)]);
        }

        private static void AddStationReceipt(string id, float hours, List<ItemQuantity> inputs, List<ItemQuantity> outputs,
            int powerGain = 0, float techGain = 0)
        {
            StageRecord(Data.BarterReceipts, id, new BarterReceipt
            {
                Id = id,
                InputItems = inputs,
                OutputItems = outputs,
                ProduceTimeInHours = hours,
                OutputPowerGain = powerGain,
                OutputTechLevelGain = techGain
            });
        }

        // Starting stock is placed only when the game creates a new campaign's stations.
        [HarmonyPatch(typeof(StationSystem), nameof(StationSystem.Create))]
        private static class GuardStartingStock
        {
            private static void Postfix(Stations __result, bool initContent, SpaceTime spaceTime, Factions factions)
            {
                if (!initContent || Data.BarterReceipts.GetRecord(LunaReceipt) == null)
                {
                    return;
                }
                Stock(__result.Get("iomMoom", logMissing: false), spaceTime, factions,
                    new(LasClip, 20), new(CorpseStarch, 6), new(DirtyStarch, 2));
                Stock(__result.Get("iomPhobos", logMissing: false), spaceTime, factions,
                    new ItemQuantity(LasClip, 24));
            }

            private static void Stock(Station station, SpaceTime spaceTime, Factions factions, params ItemQuantity[] supplies)
            {
                if (station?.OwnerFactionId != ItemCategory)
                {
                    return;
                }
                var items = new List<BasePickupItem>();
                foreach (ItemQuantity supply in supplies)
                {
                    ItemInteractionSystem.CreateItem(items, supply.ItemId, supply.Count);
                }
                foreach (BasePickupItem item in items)
                {
                    StationSystem.AddItemToStationStorage(spaceTime, factions, station, item);
                }
            }
        }

        private static T CloneItem<T>(string id, string donorId, int level, float price, string artwork = null,
            int maxIconSize = 0)
            where T : ItemRecord
        {
            if (Data.Items.GetRecord(donorId) is not CompositeItemRecord composite ||
                composite.GetRecord<T>() is not T donor || donor.ItemDesc == null)
            {
                throw new InvalidDataException("Imperial item " + id + " requires donor " + donorId);
            }
            var record = (T)CloneRecordMethod.Invoke(donor, null);
            record.Id = id;
            record.Categories = [ItemCategory];
            record.TechLevel = level;
            record.Price = price;
            ItemContentDescriptor descriptor = Object.Instantiate(donor.ItemDesc);
            descriptor.name = id;
            descriptor._overridenRenderId = string.Empty;
            if (artwork != null)
            {
                ApplyItemArtwork(descriptor, artwork, maxIconSize);
            }
            record.ContentDescriptor = descriptor;
            return record;
        }

        private static void ApplyItemArtwork(ItemContentDescriptor descriptor, string artwork, int maxIconSize = 0)
        {
            descriptor._icon = ReadItemSprite(artwork + "_icon", descriptor.Icon, maxSize: maxIconSize);
            descriptor._smallIcon = ReadItemSprite(artwork + "_floor", descriptor.SmallIcon);
            descriptor._shadow = ReadItemSprite(artwork + "_shadow", descriptor.ShadowOnFloor);
        }

        private static Sprite ReadItemSprite(string name, Sprite donor, bool keepReadable = false, int maxSize = 0)
        {
            string resource = "ImperiumOfMan.Art." + name + ".png";
            if (!SpriteResources.TryGetValue(resource, out string path))
            {
                throw new InvalidDataException("Missing Imperial artwork " + resource);
            }
            using Stream stream = ModAssembly.GetManifestResourceStream(path);
            using var buffer = new MemoryStream();
            stream.CopyTo(buffer);
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false) { name = name, filterMode = FilterMode.Point };
            if (!ImageConversion.LoadImage(texture, buffer.ToArray(), markNonReadable: !keepReadable))
            {
                Object.Destroy(texture);
                throw new InvalidDataException("Invalid Imperial artwork " + resource);
            }
            Vector2 pivot = donor == null
                ? new Vector2(0.5f, 0.5f)
                : new Vector2(donor.pivot.x / donor.rect.width, donor.pivot.y / donor.rect.height);
            const float standardPixelsPerUnit = 100f;
            float pixelsPerUnit = donor == null ? standardPixelsPerUnit : donor.pixelsPerUnit;
            if (maxSize > 0)
            {
                // Quasimorph uses the sprite's native size in its inventory and production UI.
                pixelsPerUnit = Mathf.Max(pixelsPerUnit, standardPixelsPerUnit * Mathf.Max(texture.width, texture.height) / maxSize);
            }
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), pivot, pixelsPerUnit);
            sprite.name = name;
            return sprite;
        }

        private static void RegisterItem(string table, ItemRecord record, string name, string description)
        {
            StageRecord(Data.Items, record.Id, record);
            StageDescriptor(table, record.Id, record.ContentDescriptor);
            Registrations.Add(() => TranslationPatch.AddItem(record.Id, name, description));
        }
    }
}

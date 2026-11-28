using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using MGSC;
using UnityEngine;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        // Settings are deliberately independent of MCM. The active snapshot is
        // immutable for the running game; the saved snapshot is for next boot.
        private sealed class ImperialOption(string page, string section, string key, string label,
            int defaultValue, int minimum, int maximum, string tooltip, string[] choices = null)
        {
            internal readonly string Page = page, Section = section, Key = key, Label = label, Tooltip = tooltip;
            internal readonly int Default = defaultValue, Minimum = minimum, Maximum = maximum;
            internal readonly string[] Choices = choices;
            internal int Clamp(int value) => Math.Min(Maximum, Math.Max(Minimum, value));
        }

        private static readonly List<ImperialOption> ImperialOptions = new();
        private static readonly Dictionary<string, ImperialOption> ImperialOptionIndex = new(StringComparer.Ordinal);
        private static Dictionary<string, int> _imperialActiveSettings = new(StringComparer.Ordinal);
        private static Dictionary<string, int> _imperialSavedSettings = new(StringComparer.Ordinal);
        private static HashSet<string> _imperialExistingItems;
        private static HashSet<string> _imperialExistingStationRecipes;
        private static HashSet<ContentDropRecord> _imperialExistingDrops;
        private static bool _imperialSettingsInitialized;

        private static string ImperialSettingsPath => Path.Combine(Application.persistentDataPath,
            "ImperiumReforged", "settings-v1.ini");

        private static int ImperialSetting(string key, int fallback) =>
            _imperialActiveSettings.TryGetValue(key, out int value) ? value : fallback;

        private static int ImperialSavedSetting(ImperialOption option) =>
            _imperialSavedSettings.TryGetValue(option.Key, out int value) ? option.Clamp(value) : option.Default;

        private static void AddImperialOption(string page, string section, string key, string label,
            int value, int min, int max, string tooltip, string[] choices = null)
        {
            if (ImperialOptionIndex.ContainsKey(key)) return;
            var option = new ImperialOption(page, section, key, label, value, min, max, tooltip, choices);
            ImperialOptionIndex.Add(key, option);
            ImperialOptions.Add(option);
        }

        private static void AddScale(string page, string section, string key, string label,
            string tip, int min = 25, int max = 300) =>
            AddImperialOption(page, section, key, label, 100, min, max,
                tip + " 100% = original. Restart required.");

        private static readonly string[] ImperialPaletteChoices =
            ["Adaptive (original)", "Foundry iron", "Fortress basalt", "Scholastic slate"];

        private static void DefineImperialGlobalSettings()
        {
            const string common = "All Imperium content";
            const string drops = "Drops";
            AddImperialOption(drops, "Loot chances", "drops.rack_chance", "Data Rack chance (%)", 12, 0, 100,
                "Chance of one Data Rack placement attempt on a new eligible procedural first floor.");
            AddImperialOption(drops, "Loot chances", "drops.chaos_cache_chance", "Chaos supply cache chance (%)", 100, 0, 100,
                "Chance of the existing Chaos procedural supply cache. Does not change quest rewards.");
            AddImperialOption(drops, "Data Rack payout", "drops.rack_min_chips", "Minimum chips", 5, 2, 20,
                "Minimum physical chip payout. At least two chips.");
            AddImperialOption(drops, "Data Rack payout", "drops.rack_max_chips", "Maximum chips", 8, 2, 20,
                "Maximum physical chip payout. Must be at least the minimum.");
            AddScale(drops, "Supplies", "drops.ration_quantity", "Imperial rations (%)",
                "Amount of existing loose and container ration supplies on a new floor.", 0, 300);
            AddScale(drops, "Reward weights", "drops.reward_equipment", "Equipment weight (%)",
                "Relative selection weight of mod-added equipment reward entries, not an absolute drop probability.", 10, 300);
            AddScale(drops, "Reward weights", "drops.reward_consumables", "Consumables weight (%)",
                "Relative selection weight of mod-added consumable reward entries.", 10, 300);
            AddScale(drops, "Reward weights", "drops.reward_chips", "Recipe chip weight (%)",
                "Relative selection weight of mod-added chip reward entries. Does not bypass faction tiers.", 10, 300);
            AddScale("Weapons", common, "weapons.damage", "Damage (%)", "Base damage range of Imperium weapons.");
            AddScale("Weapons", common, "weapons.range", "Range (%)", "Ranged weapon range; melee reach is unchanged.", 50, 200);
            AddScale("Weapons", common, "weapons.durability", "Durability (%)", "Maximum durability.", 100, 300);
            AddScale("Weapons", common, "weapons.magazine", "Magazine capacity (%)", "Increase ranged weapon capacity; existing ammo is not removed.", 100, 300);
            AddScale("Armor", common, "armor.resist", "Resistance (%)", "Positive damage resistance; weaknesses are unchanged.", 25, 200);
            AddScale("Armor", common, "armor.durability", "Durability (%)", "Maximum armor durability.", 100, 300);
            AddScale("Items", common, "items.weight", "Weight (%)", "Weight of mod-added equipment and items.", 25, 300);
            AddScale("Items", common, "items.price", "Base value (%)", "Item record price; does not rewrite quest or faction reward point costs.");
            AddScale("Items", common, "items.stack", "Stack capacity (%)", "Increase existing stack sizes. Unique and nonstackable items remain unchanged.", 100, 300);
            AddScale("Recipes", "Magnum fabrication", "recipes.cost", "Ingredient cost (%)", "Existing ingredient quantities. Positive counts round up.", 25, 300);
            AddScale("Recipes", "Magnum fabrication", "recipes.time", "Production time (%)", "Base production hours, before native difficulty and occult research.", 25, 300);
            AddScale("Recipes", "Magnum fabrication", "recipes.batch", "Output batch (%)", "Mod-managed fabrication batch size. Positive yields never become zero.", 25, 300);
            AddScale("Recipes", "Station workshops", "station.cost", "Ingredient cost (%)", "Mod-added item-only station workshop recipes.", 25, 300);
            AddScale("Recipes", "Station workshops", "station.time", "Production time (%)", "Mod-added item-only station workshop hours.", 25, 300);
            AddScale("Recipes", "Station workshops", "station.batch", "Output batch (%)", "Mod-added item-only station workshop item quantities.", 25, 300);
            AddImperialOption("Rooms", "Floor style", "rooms.palette", "Imperial room palette", 0, 0, 3,
                "Adaptive or one of three curated native-sized Imperial floor material families. Newly generated clean rooms only.", ImperialPaletteChoices);
            AddImperialOption("Rooms", "Floor style", "rooms.keep_native_floors", "Retain native floors (adaptive)", 1, 0, 1,
                "Keep dominant native authored floors in the adaptive mode.", ["Off", "On"]);
            AddImperialOption("Rooms", "Wall detailing", "rooms.facades", "Imperial facade accents", 1, 0, 1,
                "Enable both approved decorative wall libraries on newly generated Imperial rooms.", ["Off", "On"]);
            AddImperialOption("Rooms", "Wall detailing", "rooms.facade_spacing", "Accent spacing (tiles)", 4, 4, 12,
                "Minimum interval; two additional seeded spacing possibilities. Plain seams remain protected.");
            AddImperialOption("Rooms", "Wall detailing", "rooms.strong_every", "Strong accent interval", 3, 0, 8,
                "Every Nth eligible accent on long walls; zero disables molding interruptions including signature accents.");
            AddImperialOption("Rooms", "Room variety", "rooms.extra_rooms", "Additional room limit", 4, 0, 4,
                "Optional unique native rooms. Zero disables the extra-room pass.");
            AddImperialOption("Rooms", "Room variety", "rooms.extra_floor_budget", "Additional floor tiles", 240, 0, 240,
                "Total optional-room floor budget. Geometry audits still decide what fits.");
            AddImperialOption("Rooms", "Native furnishings", "rooms.furniture_limit", "Extra props per floor", 48, 0, 48,
                "Maximum additional native props, subject to safe footprints and connectivity.");
            AddImperialOption("Rooms", "Native furnishings", "rooms.furniture_per_room", "Extra props per room", 3, 0, 3,
                "Room-specific maximum. Smaller rooms retain their lower limit.");
        }

        private static void CaptureImperiumConfigBaseline()
        {
            if (_imperialSettingsInitialized) return;
            try
            {
                _imperialExistingItems = new HashSet<string>(Data.Items.Ids, StringComparer.Ordinal);
                _imperialExistingStationRecipes = new HashSet<string>(Data.BarterReceipts.Records.Select(record => record.Id),
                    StringComparer.Ordinal);
                _imperialExistingDrops = Data.FactionDrop._recordsByFactions.Values
                    .SelectMany(tiers => tiers.Values).SelectMany(records => records).ToHashSet();
            }
            catch (Exception error)
            {
                // Fail closed: never scale existing native or unrelated mod records.
                _imperialExistingItems = null;
                _imperialExistingStationRecipes = null;
                _imperialExistingDrops = null;
                Debug.LogWarning("[Cvar_ImperiumOfMan] Config baseline unavailable: " + error.Message);
            }
        }

        private static Dictionary<string, int> ReadImperialSettings()
        {
            var result = new Dictionary<string, int>(StringComparer.Ordinal);
            string path = ImperialSettingsPath;
            if (!File.Exists(path)) return result;
            if (new FileInfo(path).Length > 1024 * 1024) throw new InvalidDataException("Settings file exceeds 1 MB.");
            foreach (string raw in File.ReadAllLines(path))
            {
                string line = raw.Trim();
                if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal)) continue;
                int split = line.IndexOf('=');
                if (split <= 0) continue;
                string key = line.Substring(0, split).Trim();
                if (ImperialOptionIndex.TryGetValue(key, out var option) &&
                    int.TryParse(line.Substring(split + 1).Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int value))
                    result[key] = option.Clamp(value);
            }
            if (result.TryGetValue("drops.rack_min_chips", out int min))
            {
                int max = result.TryGetValue("drops.rack_max_chips", out int savedMax) ? savedMax : 8;
                if (max < min) result["drops.rack_max_chips"] = min;
            }
            return result;
        }

        private static void WriteImperialSettings(Dictionary<string, int> values)
        {
            string path = ImperialSettingsPath;
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            string temp = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                File.WriteAllLines(temp, new[] { "# Imperium Reforged settings v1. Restart to apply." }
                    .Concat(ImperialOptions.Where(option => values.TryGetValue(option.Key, out int value) &&
                        value != option.Default).Select(option => option.Key + "=" +
                        values[option.Key].ToString(CultureInfo.InvariantCulture))));
                if (File.Exists(path)) File.Replace(temp, path, null);
                else File.Move(temp, path);
            }
            finally
            {
                if (File.Exists(temp)) File.Delete(temp);
            }
        }

        private static void InitializeImperiumSettings()
        {
            if (_imperialSettingsInitialized) return;
            _imperialSettingsInitialized = true;
            try
            {
                DefineImperialGlobalSettings();
                var items = (_imperialExistingItems == null ? Enumerable.Empty<string>() :
                    Data.Items.Ids.Where(id => !_imperialExistingItems.Contains(id)))
                    .OrderBy(id => id, StringComparer.Ordinal)
                    .Select(id => Data.Items.GetSimpleRecord<ItemRecord>(id)).Where(item => item != null).ToArray();
                var ship = Data.ProduceReceipts.Where(receipt => IndustryBatches.ContainsKey(receipt.OutputItem))
                    .GroupBy(receipt => receipt.OutputItem, StringComparer.Ordinal)
                    .Where(group => group.Count() == 1).Select(group => group.First())
                    .OrderBy(receipt => receipt.OutputItem, StringComparer.Ordinal).ToArray();
                var stations = (_imperialExistingStationRecipes == null ? Enumerable.Empty<BarterReceipt>() :
                    Data.BarterReceipts.Records.Where(receipt => !_imperialExistingStationRecipes.Contains(receipt.Id)))
                    .Where(receipt => receipt.InputPopulation == 0 && receipt.OutputPowerGain == 0 &&
                        receipt.OutputTechLevelGain == 0 && receipt.OutputItems != null && receipt.OutputItems.Count > 0)
                    .OrderBy(receipt => receipt.Id, StringComparer.Ordinal).ToArray();

                var rewardItems = _imperialExistingDrops == null ? new HashSet<string>(StringComparer.Ordinal) :
                    Data.FactionDrop._recordsByFactions.Values.SelectMany(tiers => tiers.Values)
                        .SelectMany(records => records).Where(drop => !_imperialExistingDrops.Contains(drop) &&
                            drop.ContentIds?.Count == 1 && drop.Weight > 0)
                        .Select(drop => drop.ContentIds[0]).ToHashSet(StringComparer.Ordinal);
                foreach (var item in items)
                {
                    string id = item.Id, section = "item." + id + ".name";
                    if (item is WeaponRecord weapon)
                    {
                        AddScale("Weapons detail", section, "weapon." + id + ".damage", "Damage (%)", "Individual multiplier, combined with the weapon category.");
                        AddScale("Weapons detail", section, "weapon." + id + ".durability", "Durability (%)", "Individual multiplier.", 100, 300);
                        if (!weapon.IsMelee)
                        {
                            AddScale("Weapons detail", section, "weapon." + id + ".range", "Range (%)", "Individual multiplier.", 50, 200);
                            if (weapon.MagazineCapacity > 0)
                                AddScale("Weapons detail", section, "weapon." + id + ".magazine", "Magazine (%)", "Individual multiplier.", 100, 300);
                        }
                    }
                    else if (item is ResistRecord && item is IArmorRecord)
                    {
                        AddScale("Armor detail", section, "armor." + id + ".resist", "Resistance (%)", "Individual multiplier.", 25, 200);
                        AddScale("Armor detail", section, "armor." + id + ".durability", "Durability (%)", "Individual multiplier.", 100, 300);
                    }
                    AddScale("Items detail", section, "item." + id + ".weight", "Weight (%)", "Individual weight multiplier.");
                    AddScale("Items detail", section, "item." + id + ".price", "Value (%)", "Individual value multiplier.");
                    if (ImperialStackCapacity(item) > 1)
                        AddScale("Items detail", section, "item." + id + ".stack", "Stack capacity (%)", "Individual multiplier; only increases.", 100, 300);
                    if (rewardItems.Contains(id))
                        AddScale("Drops detail", section, "drop." + id + ".weight", "Reward weight (%)",
                            "Relative selection weight of this item's added reward entry. Not an absolute percentage chance.", 10, 300);
                }

                foreach (var receipt in ship)
                {
                    string id = receipt.OutputItem, section = "item." + id + ".name";
                    AddScale("Recipes detail", section, "recipe." + id + ".cost", "Ingredient cost (%)", "Individual ship recipe multiplier.");
                    AddScale("Recipes detail", section, "recipe." + id + ".time", "Hours (%)", "Individual ship recipe duration multiplier.");
                    AddScale("Recipes detail", section, "recipe." + id + ".batch", "Output (%)", "Individual ship recipe batch multiplier.");
                    DefineImperialIngredientOptions("Recipes detail", section, "recipe." + id, receipt.RequiredItems);
                }
                foreach (var receipt in stations)
                {
                    string section = "iom.mcm.station." + receipt.Id;
                    string name = string.Join(" + ", receipt.OutputItems.Select(output => CraftingItemName(output.ItemId)));
                    AddMcmInformationCaption(section, name, name);
                    AddScale("Workshop detail", section, "station." + receipt.Id + ".cost", "Ingredient cost (%)", "Individual station workshop multiplier.");
                    AddScale("Workshop detail", section, "station." + receipt.Id + ".time", "Hours (%)", "Individual station workshop duration multiplier.");
                    AddScale("Workshop detail", section, "station." + receipt.Id + ".batch", "Output (%)", "Individual station workshop batch multiplier.");
                    DefineImperialIngredientOptions("Workshop detail", section, "station." + receipt.Id, receipt.InputItems);
                }

                try { _imperialSavedSettings = ReadImperialSettings(); }
                catch (Exception error)
                {
                    Debug.LogWarning("[Cvar_ImperiumOfMan] Settings unreadable; original defaults retained: " + error.Message);
                    _imperialSavedSettings = new Dictionary<string, int>(StringComparer.Ordinal);
                }
                _imperialActiveSettings = new Dictionary<string, int>(_imperialSavedSettings, StringComparer.Ordinal);
                foreach (var item in items) ApplyImperialItemSettings(item);
                foreach (var receipt in ship)
                {
                    string id = receipt.OutputItem;
                    float cost = ImperialScale("recipes.cost", "recipe." + id + ".cost");
                    float time = ImperialScale("recipes.time", "recipe." + id + ".time");
                    float batch = ImperialScale("recipes.batch", "recipe." + id + ".batch");
                    receipt.RequiredItems = ScaleImperialQuantities(receipt.RequiredItems, cost, "recipe." + id);
                    if (time != 1f) receipt.ProduceTimeInHours = Math.Max(1f, receipt.ProduceTimeInHours * time);
                    if (batch != 1f) IndustryBatches[id] = ScalePositiveInteger(IndustryBatches[id], batch, 999);
                }
                foreach (var receipt in stations)
                {
                    string id = receipt.Id;
                    float cost = ImperialScale("station.cost", "station." + id + ".cost");
                    float time = ImperialScale("station.time", "station." + id + ".time");
                    float batch = ImperialScale("station.batch", "station." + id + ".batch");
                    receipt.InputItems = ScaleImperialQuantities(receipt.InputItems, cost, "station." + id);
                    if (time != 1f) receipt.ProduceTimeInHours = Math.Max(1f, receipt.ProduceTimeInHours * time);
                    if (batch != 1f) receipt.OutputItems = ScaleImperialQuantities(receipt.OutputItems, batch);
                }
                if (_imperialExistingDrops != null)
                {
                    var scaled = new HashSet<ContentDropRecord>();
                    foreach (var faction in Data.FactionDrop._recordsByFactions)
                    {
                        string key = faction.Key.EndsWith("_rewardEquipment", StringComparison.Ordinal) ? "drops.reward_equipment" :
                            faction.Key.EndsWith("_rewardConsumables", StringComparison.Ordinal) ? "drops.reward_consumables" :
                            faction.Key.EndsWith("_rewardChips", StringComparison.Ordinal) ? "drops.reward_chips" : null;
                        if (key == null) continue;
                        foreach (var drop in faction.Value.Values.SelectMany(records => records))
                        {
                            if (_imperialExistingDrops.Contains(drop) || !scaled.Add(drop) || drop.Weight <= 0) continue;
                            string itemKey = drop.ContentIds?.Count == 1 ? "drop." + drop.ContentIds[0] + ".weight" : null;
                            float scale = ImperialScale(key, itemKey);
                            if (scale != 1f) drop.Weight *= scale;
                        }
                    }
                }
                Debug.Log("[Cvar_ImperiumOfMan] Config initialized; options=" + ImperialOptions.Count +
                    "; items=" + items.Length + "; ship recipes=" + ship.Length + "; workshop recipes=" + stations.Length);
            }
            catch (Exception error)
            {
                Debug.LogWarning("[Cvar_ImperiumOfMan] Optional configuration initialization failed: " + error);
            }
            finally
            {
                _imperialExistingItems = null;
                _imperialExistingStationRecipes = null;
                _imperialExistingDrops = null;
            }
        }

        private static float ImperialScale(string global, string individual = null)
        {
            float a = ImperialSetting(global, 100) / 100f;
            float b = individual == null ? 1f : ImperialSetting(individual, 100) / 100f;
            return Math.Min(4f, Math.Max(.1f, a * b));
        }

        private static int ScalePositiveInteger(int original, float scale, int maximum)
        {
            if (original <= 0 || scale == 1f) return original;
            return Math.Max(1, (int)Math.Min(maximum, Math.Round(original * (double)scale, MidpointRounding.AwayFromZero)));
        }

        private static void DefineImperialIngredientOptions(string page, string section, string prefix,
            List<ItemQuantity> quantities)
        {
            if (quantities == null) return;
            foreach (var ingredient in quantities.Where(q => q.Count > 0 && !string.IsNullOrWhiteSpace(q.ItemId))
                .GroupBy(q => q.ItemId, StringComparer.Ordinal))
            {
                AddScale(page, section, prefix + ".input." + ingredient.Key,
                    CraftingItemName(ingredient.Key) + " (%)",
                    "Original ingredient quantity: " + ingredient.Sum(q => q.Count) +
                    ". Combines with recipe and category cost; positive counts round up.");
            }
        }

        private static List<ItemQuantity> ScaleImperialQuantities(List<ItemQuantity> original, float scale,
            string recipePrefix = null)
        {
            if (original == null) return original;
            float Factor(ItemQuantity quantity) => recipePrefix == null ? scale :
                Math.Min(4f, Math.Max(.1f, scale * ImperialSetting(
                    recipePrefix + ".input." + quantity.ItemId, 100) / 100f));
            if (original.All(quantity => Factor(quantity) == 1f)) return original;
            return original.Select(quantity =>
            {
                float factor = Factor(quantity);
                int count = quantity.Count <= 0 || factor == 1f ? quantity.Count :
                    Math.Max(1, (int)Math.Min(Math.Max(999, quantity.Count), Math.Ceiling(quantity.Count * (double)factor)));
                return new ItemQuantity(quantity.ItemId, count);
            }).ToList();
        }

        // Allowlisted numeric properties have changed type across native builds.
        // Preserve the actual member's numeric type and skip absent/unsupported members.
        private static int ImperialStackCapacity(ItemRecord item)
        {
            // MaxStack belongs to concrete stackable records, not ItemRecord.
            // Only offer a slider when the native record exposes that member.
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;
            var property = item.GetType().GetProperty("MaxStack", flags);
            var field = item.GetType().GetField("MaxStack", flags);
            object value = property != null && property.CanRead && property.CanWrite
                ? property.GetValue(item) : field != null && !field.IsInitOnly ? field.GetValue(item) : null;
            return value is int count ? count : value is short small ? small :
                value is byte tiny ? tiny : value is uint positive && positive <= int.MaxValue ? (int)positive : 1;
        }

        private static void ScaleImperialNumeric(object record, string member, float scale, double maximum)
        {
            if (record == null || scale == 1f) return;
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;
            var property = record.GetType().GetProperty(member, flags);
            var field = record.GetType().GetField(member, flags);
            if (property == null && field == null || property != null && !property.CanWrite) return;
            Type type = property != null ? property.PropertyType : field.FieldType;
            if (type != typeof(int) && type != typeof(short) && type != typeof(byte) &&
                type != typeof(float) && type != typeof(double) && type != typeof(uint)) return;
            object value = property != null ? property.GetValue(record) : field.GetValue(record);
            double number = Convert.ToDouble(value, CultureInfo.InvariantCulture);
            if (double.IsNaN(number) || double.IsInfinity(number) || number <= 0) return;
            double scaled = Math.Min(Math.Max(maximum, number), number * scale);
            if (type != typeof(float) && type != typeof(double))
                scaled = Math.Max(1, Math.Round(scaled, MidpointRounding.AwayFromZero));
            if (type == typeof(byte)) scaled = Math.Min(byte.MaxValue, scaled);
            if (type == typeof(short)) scaled = Math.Min(short.MaxValue, scaled);
            if (type == typeof(int)) scaled = Math.Min(int.MaxValue, scaled);
            if (type == typeof(uint)) scaled = Math.Min(uint.MaxValue, scaled);
            object converted = Convert.ChangeType(scaled, type, CultureInfo.InvariantCulture);
            if (property != null) property.SetValue(record, converted);
            else field.SetValue(record, converted);
        }

        private static void ApplyImperialItemSettings(ItemRecord item)
        {
            try
            {
                string id = item.Id;
                ScaleImperialNumeric(item, "Weight", ImperialScale("items.weight", "item." + id + ".weight"), 1000);
                ScaleImperialNumeric(item, "Price", ImperialScale("items.price", "item." + id + ".price"), 1000000);
                if (ImperialStackCapacity(item) > 1)
                    ScaleImperialNumeric(item, "MaxStack", ImperialScale("items.stack", "item." + id + ".stack"), 999);
                if (item is WeaponRecord weapon)
                {
                    float damage = ImperialScale("weapons.damage", "weapon." + id + ".damage");
                    if (damage != 1f)
                    {
                        var original = weapon.Damage;
                        // Preserve all fields, including fields introduced by
                        // newer game builds. Copy before mutating a cloned donor.
                        var copy = (DmgInfo)typeof(object).GetMethod("MemberwiseClone",
                            BindingFlags.Instance | BindingFlags.NonPublic).Invoke(original, null);
                        copy.minDmg = ScalePositiveInteger(Convert.ToInt32(original.minDmg), damage, 10000);
                        copy.maxDmg = ScalePositiveInteger(Convert.ToInt32(original.maxDmg), damage, 10000);
                        weapon.Damage = copy;
                    }
                    ScaleImperialNumeric(weapon, "MaxDurability", ImperialScale("weapons.durability", "weapon." + id + ".durability"), 5000);
                    if (!weapon.IsMelee)
                    {
                        ScaleImperialNumeric(weapon, "Range", ImperialScale("weapons.range", "weapon." + id + ".range"), 40);
                        ScaleImperialNumeric(weapon, "MagazineCapacity", ImperialScale("weapons.magazine", "weapon." + id + ".magazine"), 500);
                    }
                }
                else if (item is ResistRecord armor && item is IArmorRecord)
                {
                    float resistance = ImperialScale("armor.resist", "armor." + id + ".resist");
                    if (resistance != 1f && armor.ResistSheet != null)
                        armor.ResistSheet = armor.ResistSheet.Select(resist => new DmgResist
                        {
                            damage = resist.damage, resistPercent = resist.resistPercent <= 0 ? resist.resistPercent :
                                Math.Min(Math.Max(95f, resist.resistPercent), resist.resistPercent * resistance)
                        }).ToList();
                    ScaleImperialNumeric(armor, "MaxDurability", ImperialScale("armor.durability", "armor." + id + ".durability"), 5000);
                }
            }
            catch (Exception error)
            {
                Debug.LogWarning("[Cvar_ImperiumOfMan] Item setting skipped for " + item.Id + ": " + error.Message);
            }
        }

        // Three curated room palettes using existing native-sized, registered
        // deck rows. No new tileset ID or saved-map migration is required.
        private static CampaignDeck SelectImperialRoomPalette(NativePresetArea area, int palette)
        {
            int index = palette switch
            {
                1 => area switch
                {
                    NativePresetArea.Corridor or NativePresetArea.LiftLobby => 8,
                    NativePresetArea.Cargo => 7,
                    NativePresetArea.Workshop => 8,
                    NativePresetArea.Medical or NativePresetArea.Laboratory => 1,
                    NativePresetArea.Ceremonial => 3,
                    NativePresetArea.Relay => 6,
                    _ => 0
                },
                2 => area switch
                {
                    NativePresetArea.Corridor or NativePresetArea.LiftLobby => 2,
                    NativePresetArea.Cargo => 6,
                    NativePresetArea.Workshop => 3,
                    NativePresetArea.Medical or NativePresetArea.Laboratory => 4,
                    NativePresetArea.Relay => 2,
                    _ => 9
                },
                _ => area switch
                {
                    NativePresetArea.Corridor or NativePresetArea.LiftLobby => 1,
                    NativePresetArea.Cargo => 2,
                    NativePresetArea.Workshop => 0,
                    NativePresetArea.Ceremonial => 9,
                    NativePresetArea.Relay => 5,
                    _ => 4
                }
            };
            return ImperialCampaignDecks[index];
        }
    }
}

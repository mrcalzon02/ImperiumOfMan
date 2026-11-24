using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using MGSC;
using Newtonsoft.Json;
using UnityEngine;

namespace ImperiumOfMan
{
    internal static class ImperialTuning
    {
        private const string PriceScale = "imperium.price_scale";
        private const string DamageScale = "imperium.damage_scale";
        private const string MagazineScale = "imperium.magazine_scale";
        private const string MissionChance = "imperium.mission_chance_percent";
        private const string MissionPayout = "imperium.mission_payout_scale";
        private const string FactionId = LegacyConflictGuard.SharedFactionId;
        private static readonly List<Setting> Settings = new();
        private static readonly List<ItemRecord> Items = new();
        private static readonly Dictionary<string, int> DefaultMinimumAmmo = new();
        private static Dictionary<string, float> _values;
        private static bool _menuRegistered;
        private static string ConfigPath => Path.Combine(Application.persistentDataPath, "Cvar_ImperiumOfMan", "tuning.json");

        private sealed class Setting
        {
            internal string Key;
            internal string Header;
            internal string Label;
            internal string Tooltip;
            internal float Default;
            internal float Min;
            internal float Max;
            internal bool Whole;
        }

        internal static void Initialize()
        {
            if (_values != null)
            {
                return;
            }
            const string general = "Imperium balance";
            Add(PriceScale, general, "Item price multiplier", 1, 0.1f, 5, tooltip: "Scales base prices for all Imperial items.");
            Add(DamageScale, general, "Weapon damage multiplier", 1, 0.1f, 5, tooltip: "Scales both ends of the Imperial weapon damage range.");
            Add(MagazineScale, general, "Magazine size multiplier", 1, 0.1f, 5, tooltip: "Scales firearm magazine capacity. Values are rounded to whole rounds.");
            Add(MissionChance, general, "Mission frequency (%)", 10, 0, 100, true,
                "Chance an eligible Imperium mission roll passes. Normal station cooldowns and faction mission caps apply.");
            Add(MissionPayout, general, "Mission payout multiplier", 1, 0, 5, tooltip:
                "Scales the contract reward budget for new Imperium procedural missions, before rewards are selected.");

            foreach (string id in Data.Items.Ids.Where(id => id.StartsWith("iom_", StringComparison.Ordinal)))
            {
                var item = Data.Items.GetSimpleRecord<ItemRecord>(id);
                if (item?.Categories == null || !item.Categories.Contains(Plugin.ItemCategory))
                {
                    continue;
                }
                Items.Add(item);
                string name = Localization.Get("item." + id + ".name", warnIfMissingTag: false);
                string group = item is WeaponRecord ? name : item.ItemClass + " prices";
                Add(id + ".price", group, item is WeaponRecord ? "Base price" : name, item.Price, 0, 5000);
                if (item is WeaponRecord weapon)
                {
                    Add(id + ".damage_min", group, "Base minimum damage", weapon.Damage.minDmg, 1, 500, true);
                    Add(id + ".damage_max", group, "Base maximum damage", weapon.Damage.maxDmg, 1, 500, true);
                    if (HasMagazine(weapon))
                    {
                        Add(id + ".magazine", group, "Base magazine size", weapon.MagazineCapacity, 1, 250, true);
                        DefaultMinimumAmmo.Add(id, weapon.MinRandomAmmoCount);
                    }
                }
            }

            Dictionary<string, float> saved = null;
            try
            {
                if (File.Exists(ConfigPath))
                {
                    saved = JsonConvert.DeserializeObject<Dictionary<string, float>>(File.ReadAllText(ConfigPath));
                }
            }
            catch (Exception error) when (error is IOException || error is UnauthorizedAccessException || error is JsonException)
            {
                Debug.LogWarning("[Cvar_ImperiumOfMan] Using default balance settings. " + error.Message);
            }
            Apply(Normalize(saved));
        }

        private static void Add(string key, string header, string label, float value, float min, float max,
            bool whole = false, string tooltip = "")
        {
            Settings.Add(new Setting
            {
                Key = key, Header = header, Label = label, Tooltip = tooltip, Default = value,
                Min = min, Max = max, Whole = whole
            });
        }

        private static bool HasMagazine(WeaponRecord weapon) => !weapon.IsMelee && weapon.MagazineCapacity > 0;

        private static Dictionary<string, float> Normalize(Dictionary<string, float> values)
        {
            var normalized = new Dictionary<string, float>();
            foreach (Setting setting in Settings)
            {
                float value = setting.Default;
                if (values != null && values.TryGetValue(setting.Key, out float supplied) &&
                    !float.IsNaN(supplied) && !float.IsInfinity(supplied))
                {
                    value = Mathf.Clamp(supplied, setting.Min, setting.Max);
                }
                normalized[setting.Key] = setting.Whole ? Mathf.RoundToInt(value) : value;
            }
            foreach (WeaponRecord weapon in Items.OfType<WeaponRecord>())
            {
                string min = weapon.Id + ".damage_min";
                string max = weapon.Id + ".damage_max";
                normalized[max] = Math.Max(normalized[min], normalized[max]);
            }
            return normalized;
        }

        private static void Apply(Dictionary<string, float> values)
        {
            _values = values;
            foreach (ItemRecord item in Items)
            {
                item.Price = values[item.Id + ".price"] * values[PriceScale];
                if (item is WeaponRecord weapon)
                {
                    var damage = weapon.Damage;
                    damage.minDmg = Mathf.Max(1, Mathf.RoundToInt(values[item.Id + ".damage_min"] * values[DamageScale]));
                    damage.maxDmg = Mathf.Max(damage.minDmg, Mathf.RoundToInt(values[item.Id + ".damage_max"] * values[DamageScale]));
                    weapon.Damage = damage;
                    if (values.TryGetValue(item.Id + ".magazine", out float baseCapacity))
                    {
                        weapon.MagazineCapacity = Mathf.Clamp(Mathf.RoundToInt(baseCapacity * values[MagazineScale]), 1, 250);
                        weapon.MinRandomAmmoCount = Math.Min(weapon.MagazineCapacity, DefaultMinimumAmmo[weapon.Id]);
                    }
                }
            }
            Data.Factions.GetRecord(FactionId).SpawnMissionChance = values[MissionChance] / 100f;
        }

        internal static void RegisterMenu(IModContext context)
        {
            if (_values == null || _menuRegistered)
            {
                return;
            }
            var mcm = context.State.Get<UserMods>().Values.FirstOrDefault(mod => mod.UniqueModName == "Crynano_ModConfigMenu");
            Assembly assembly = mcm?.LoadedAssemblies.FirstOrDefault(candidate =>
                candidate.GetType("ModConfigMenu.ModConfigMenuAPI") != null);
            if (assembly == null)
            {
                return;
            }

            try
            {
                Type api = assembly.GetType("ModConfigMenu.ModConfigMenuAPI", throwOnError: true);
                Type value = assembly.GetType("ModConfigMenu.Contracts.IConfigValue", throwOnError: true);
                Type range = assembly.GetType("ModConfigMenu.Implementations.RangeConfig`1", throwOnError: true);
                Type callback = api.GetNestedType("ConfigStoredDelegate");
                Type listType = typeof(List<>).MakeGenericType(value);
                var entries = (IList)Activator.CreateInstance(listType);
                foreach (Setting setting in Settings.OrderBy(setting => setting.Header == "Imperium balance" ? 0 : 1)
                    .ThenBy(setting => setting.Header, StringComparer.Ordinal))
                {
                    Type type = setting.Whole ? typeof(int) : typeof(float);
                    entries.Add(Activator.CreateInstance(range.MakeGenericType(type), setting.Key,
                        Convert.ChangeType(_values[setting.Key], type, CultureInfo.InvariantCulture),
                        Convert.ChangeType(setting.Default, type, CultureInfo.InvariantCulture),
                        Convert.ChangeType(setting.Min, type, CultureInfo.InvariantCulture),
                        Convert.ChangeType(setting.Max, type, CultureInfo.InvariantCulture),
                        setting.Header, setting.Tooltip, setting.Label));
                }
                var onSave = Delegate.CreateDelegate(callback,
                    typeof(ImperialTuning).GetMethod(nameof(Save), BindingFlags.Static | BindingFlags.NonPublic));
                MethodInfo register = api.GetMethod("RegisterModConfig", new[] { typeof(string), listType, callback });
                if (register == null)
                {
                    throw new MissingMethodException(api.FullName, "RegisterModConfig");
                }
                register.Invoke(null, new object[] { "Imperium of Man", entries, onSave });
                _menuRegistered = true;
                Debug.Log("[Cvar_ImperiumOfMan] Registered " + entries.Count + " options with Mod Configuration Menu.");
            }
            catch (Exception error)
            {
                Debug.LogWarning("[Cvar_ImperiumOfMan] Mod Configuration Menu integration unavailable. " + error.Message);
            }
        }

        private static bool Save(Dictionary<string, object> options, out string feedback)
        {
            var values = new Dictionary<string, float>();
            foreach (Setting setting in Settings)
            {
                if (!options.TryGetValue(setting.Key, out object input) ||
                    !float.TryParse(Convert.ToString(input, CultureInfo.InvariantCulture), NumberStyles.Float,
                        CultureInfo.InvariantCulture, out float number) || float.IsNaN(number) || float.IsInfinity(number))
                {
                    feedback = "Invalid value for " + setting.Label + ".";
                    return false;
                }
                values[setting.Key] = number;
            }
            foreach (WeaponRecord weapon in Items.OfType<WeaponRecord>())
            {
                if (values[weapon.Id + ".damage_min"] > values[weapon.Id + ".damage_max"])
                {
                    feedback = Localization.Get("item." + weapon.Id + ".name", false) + " has minimum damage above maximum damage.";
                    return false;
                }
            }

            values = Normalize(values);
            try
            {
                string path = ConfigPath;
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                string pending = path + ".tmp";
                File.WriteAllText(pending, JsonConvert.SerializeObject(values, Formatting.Indented));
                if (File.Exists(path))
                {
                    File.Replace(pending, path, null);
                }
                else
                {
                    File.Move(pending, path);
                }
                Apply(values);
                feedback = "Imperium balance saved.";
                return true;
            }
            catch (Exception error) when (error is IOException || error is UnauthorizedAccessException)
            {
                feedback = "Could not save Imperium balance. " + error.Message;
                return false;
            }
        }

        [HarmonyPatch(typeof(MissionFactory), "GenerateReward")]
        private static class MissionPayoutPatch
        {
            private static void Prefix(Mission mission, Faction faction, ref float victoryPoints,
                FactionDropCollection.TradeItemsFor category)
            {
                if (_values != null && faction.Id == FactionId && category == FactionDropCollection.TradeItemsFor.Equipment)
                {
                    victoryPoints *= _values[MissionPayout];
                    mission.OriginalRewardPoints = Mathf.RoundToInt(victoryPoints);
                }
            }
        }
    }
}

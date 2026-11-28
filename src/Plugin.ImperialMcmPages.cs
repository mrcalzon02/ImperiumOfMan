using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private const int ImperialOptionsPerPage = 72;
        private static readonly HashSet<string> RegisteredImperiumOptionPages = new(StringComparer.Ordinal);
        private sealed class ImperialSettingsPage(ImperialOption[] options)
        {
            // The typed MCM API is fileless. The owner callback persists
            // only this page's validated values, atomically, for next restart.
            internal bool Save(Dictionary<string, object> values, out string feedback)
            {
                feedback = "";
                try
                {
                    var next = new Dictionary<string, int>(_imperialSavedSettings, StringComparer.Ordinal);
                    foreach (var option in options)
                    {
                        if (!values.TryGetValue(option.Key, out object raw)) continue;
                        int value;
                        if (option.Choices != null)
                        {
                            value = Array.IndexOf(option.Choices, raw as string);
                            if (value < 0) throw new ArgumentException("Invalid choice: " + option.Label);
                        }
                        else
                        {
                            if (raw is not int number) throw new ArgumentException("Invalid number: " + option.Label);
                            value = number;
                        }
                        if (value < option.Minimum || value > option.Maximum)
                            throw new ArgumentException(option.Label + " is outside its allowed range.");
                        if (value == option.Default) next.Remove(option.Key);
                        else next[option.Key] = value;
                    }
                    int min = next.TryGetValue("drops.rack_min_chips", out int lo) ? lo : 5;
                    int max = next.TryGetValue("drops.rack_max_chips", out int hi) ? hi : 8;
                    if (min > max) throw new ArgumentException("Data Rack minimum chips cannot exceed maximum.");
                    WriteImperialSettings(next);
                    _imperialSavedSettings = next;
                    Debug.Log("[Cvar_ImperiumOfMan] Config saved; restart Quasimorph to apply.");
                    return true;
                }
                catch (Exception error)
                {
                    feedback = "Imperium settings not saved: " + error.Message;
                    return false;
                }
            }
        }

        private static void RegisterImperiumSettingsPages(Assembly mcm, Type valueType, Type listType,
            Type callbackType, MethodInfo register)
        {
            if (ImperialOptions.Count == 0) return;
            try
            {
                Type range = mcm.GetType("ModConfigMenu.Implementations.RangeConfig`1", true).MakeGenericType(typeof(int));
                Type dropdown = mcm.GetType("ModConfigMenu.Implementations.DropdownConfig", true);
                Type textType = mcm.GetType("ModConfigMenu.Implementations.StringConfig", true);
                Type meta = mcm.GetType("ModConfigMenu.MetaData", true);
                foreach (var category in ImperialOptions.GroupBy(option => option.Page))
                {
                    var groups = category.GroupBy(option => option.Section).ToArray();
                    var pages = new List<List<ImperialOption>>();
                    var page = new List<ImperialOption>();
                    foreach (var group in groups)
                    {
                        if (page.Count + group.Count() > ImperialOptionsPerPage && page.Count > 0)
                        {
                            pages.Add(page);
                            page = new List<ImperialOption>();
                        }
                        page.AddRange(group);
                    }
                    if (page.Count > 0) pages.Add(page);
                    for (int i = 0; i < pages.Count; i++)
                    {
                        string pageName = "IOM - " + category.Key +
                            (pages.Count > 1 ? " " + (i + 1).ToString("00", CultureInfo.InvariantCulture) : "");
                        if (RegisteredImperiumOptionPages.Contains(pageName)) continue;
                        try
                        {
                            var list = (IList)Activator.CreateInstance(listType);
                            string notice = "iom.mcm.restart_notice";
                            AddMcmInformationCaption(notice, "Save changes, then restart Quasimorph.", "Сохраните изменения, затем перезапустите игру.");
                            list.Add(CreateMcmInformationRow(textType, meta, "restart_notice", notice));
                            foreach (var option in pages[i])
                            {
                                string header = option.Section.StartsWith("item.", StringComparison.Ordinal) ||
                                    option.Section.StartsWith("iom.mcm.", StringComparison.Ordinal)
                                    ? option.Section : RegisterImperiumMcmCaption("section." + option.Section, option.Section);
                                string label = RegisterImperiumMcmCaption("option." + option.Key, option.Label);
                                string tip = option.Tooltip;
                                int saved = ImperialSavedSetting(option);
                                object row = option.Choices == null
                                    ? Activator.CreateInstance(range, option.Key, saved, option.Default,
                                        option.Minimum, option.Maximum, header, tip, label)
                                    : Activator.CreateInstance(dropdown, option.Key, option.Choices[saved], header,
                                        option.Choices[option.Default], tip, label, option.Choices.Cast<object>().ToList());
                                if (!valueType.IsInstanceOfType(row)) throw new InvalidOperationException("Incompatible MCM row.");
                                list.Add(row);
                            }
                            var handler = new ImperialSettingsPage(pages[i].ToArray());
                            var method = typeof(ImperialSettingsPage).GetMethod(nameof(ImperialSettingsPage.Save),
                                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                            var callback = Delegate.CreateDelegate(callbackType, handler, method);
                            register.Invoke(null, new object[] { pageName, list, callback });
                            RegisteredImperiumOptionPages.Add(pageName);
                        }
                        catch (Exception error)
                        {
                            Debug.LogWarning("[Cvar_ImperiumOfMan] Optional MCM page " + pageName + " unavailable: " + error.Message);
                        }
                    }
                }
                Debug.Log("[Cvar_ImperiumOfMan] MCM configuration pages registered: " + RegisteredImperiumOptionPages.Count);
            }
            catch (Exception error)
            {
                // Preserve the original proven information entry.
                Debug.LogWarning("[Cvar_ImperiumOfMan] MCM settings controls unavailable: " + error.Message);
            }
        }

        private static string RegisterImperiumMcmCaption(string key, string value)
        {
            string id = "iom.mcm." + key;
            AddMcmInformationCaption(id, value, value);
            return id;
        }
    }
}

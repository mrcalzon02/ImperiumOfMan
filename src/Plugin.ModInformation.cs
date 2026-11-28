using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using MGSC;
using UnityEngine;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private const string McmDisplayName = "Imperium Reforged";
        private const string McmInformationHeader = "iom.mcm.information";
        private const string McmVersionCaption = "iom.mcm.installed_version";
        private const string McmBuildCaption = "iom.mcm.build_id";
        private static bool _mcmInformationRegistered;
        private static bool _versionLogged;

        // MainMenuStarted is also the hook used by MCM itself. All mod
        // assemblies should be loaded by this point; registration order does
        // not matter because MCM builds its mod list when the menu is opened.
        [Hook(ModHookType.MainMenuStarted)]
        public static void RegisterModInformation(IModContext context)
        {
            try
            {
                Assembly ownAssembly = typeof(Plugin).Assembly;
                string version = ownAssembly.GetName().Version?.ToString() ?? "Unknown";
                string build = ownAssembly.ManifestModule.ModuleVersionId.ToString("N");
                if (!_versionLogged)
                {
                    Debug.Log("[Cvar_ImperiumOfMan] Loaded version " + version +
                              "; build MVID " + build);
                    _versionLogged = true;
                }

                if (_mcmInformationRegistered)
                    return;

                // Do not reference, load, copy, or require MCM.dll. Only use
                // the public API of an MCM assembly the game has already loaded.
                Type api = null;
                foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    api = assembly.GetType("ModConfigMenu.ModConfigMenuAPI", false);
                    if (api != null)
                        break;
                }
                if (api == null)
                    return;

                Assembly mcm = api.Assembly;
                Type valueType = mcm.GetType("ModConfigMenu.Contracts.IConfigValue", true);
                Type stringType = mcm.GetType("ModConfigMenu.Implementations.StringConfig", true);
                Type metadataType = mcm.GetType("ModConfigMenu.MetaData", true);
                Type callbackType = api.GetNestedType("ConfigStoredDelegate", BindingFlags.Public);
                Type listType = typeof(List<>).MakeGenericType(valueType);
                if (callbackType == null)
                    throw new MissingMemberException("MCM ConfigStoredDelegate is unavailable.");

                MethodInfo register = api.GetMethod("RegisterModConfig",
                    BindingFlags.Public | BindingFlags.Static, null,
                    new[] { typeof(string), listType, callbackType }, null);
                if (register == null)
                    throw new MissingMethodException("MCM typed RegisterModConfig is unavailable.");

                // StringConfig renders as display-only text in MCM 1.6.2+
                // (not a textbox). Keys are stable and values are regenerated
                // from the loaded DLL, never from the manifest or saved config.
                AddMcmInformationCaption(McmInformationHeader, "Mod information", "Информация о моде");
                AddMcmInformationCaption(McmVersionCaption, "Installed version: " + version,
                    "Установленная версия: " + version);
                AddMcmInformationCaption(McmBuildCaption, "Build ID: " + build.Substring(0, 12),
                    "Идентификатор сборки: " + build.Substring(0, 12));

                var entries = (IList)Activator.CreateInstance(listType);
                entries.Add(CreateMcmInformationRow(stringType, metadataType,
                    "installed_version", McmVersionCaption));
                entries.Add(CreateMcmInformationRow(stringType, metadataType,
                    "build_id", McmBuildCaption));

                MethodInfo callback = typeof(Plugin).GetMethod(nameof(OnModInformationSaved),
                    BindingFlags.NonPublic | BindingFlags.Static);
                Delegate onSaved = Delegate.CreateDelegate(callbackType, callback);
                register.Invoke(null, new object[] { McmDisplayName, entries, onSaved });
                _mcmInformationRegistered = true;
                Debug.Log("[Cvar_ImperiumOfMan] MCM mod information registered.");
                RegisterImperiumSettingsPages(mcm, valueType, listType, callbackType, register);
            }
            catch (Exception ex)
            {
                // MCM is optional. An API mismatch or UI/localization issue
                // must never prevent Imperium or Quasimorph from starting.
                Debug.LogWarning("[Cvar_ImperiumOfMan] Optional MCM information unavailable: " + ex);
            }
        }

        private static object CreateMcmInformationRow(Type stringType, Type metadataType,
            string key, string caption)
        {
            object row = Activator.CreateInstance(stringType, key, caption, McmInformationHeader);
            // MCM's reset-all action should retain the same authoritative
            // localization key rather than emptying the information row.
            var properties = (IList)stringType.GetProperty("Properties").GetValue(row, null);
            properties.Add(Activator.CreateInstance(metadataType, "default", caption));
            return row;
        }

        private static void AddMcmInformationCaption(string key, string english, string russian)
        {
            // Keep the English fallback in the existing localization registry
            // and also populate the already-loaded dictionaries immediately.
            TranslationPatch.AddMachineCaption(key, english);
            var localization = Singleton<Localization>.Instance;
            if (localization == null || localization.db == null)
                return;
            foreach (var language in localization.db)
            {
                if (language.Value != null)
                    language.Value[key] = language.Key == Localization.Lang.Russian ? russian : english;
            }
        }

        private static bool OnModInformationSaved(Dictionary<string, object> values,
            out string feedbackMessage)
        {
            // There are no user-editable values yet. Never use incoming
            // version/build values to change runtime identity or game state.
            feedbackMessage = string.Empty;
            return true;
        }
    }
}

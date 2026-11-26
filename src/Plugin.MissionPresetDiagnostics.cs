using System;
using System.Collections.Generic;
using HarmonyLib;
using MGSC;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        // Observe the native failure before it throws. This patch does not repair
        // preset collections, replace generation, or handle the original exception.
        [HarmonyPatch(typeof(MissionFactory), "GenerateProceduralPlan")]
        private static class DiagnoseMissingMissionPresets
        {
            private const int MaximumReports = 64;
            private const int MaximumFolders = 64;
            private const int MaximumLabelLength = 120;
            private static readonly HashSet<(string Station, string Template, string Folder, bool Static)> Reported = new();

            private static void Prefix(Mission mission, ProcMissionTemplate template,
                BinaryPresetsMap.Presets presets, bool isStaticLocation, bool isFirstStage,
                bool __runOriginal)
            {
                if (!__runOriginal) return;
                bool elevatorsNull = presets.Elevators == null;
                bool commonNull = presets.Common == null;
                bool corridorsNull = presets.Corridors == null;
                bool defenseNull = presets.Defense == null;
                if (isStaticLocation ? !defenseNull : !elevatorsNull && !commonNull && !corridorsNull)
                    return;

                try
                {
                    var folders = template?.PresetFolders;
                    int folderIndex = !isFirstStage && folders != null && folders.Count > 1 ? 1 : 0;
                    string requestedFolder = folders != null && folderIndex < folders.Count ? folders[folderIndex] : null;
                    var key = (mission?.StationId, template?.Id, requestedFolder, isStaticLocation);
                    lock (Reported)
                    {
                        if (Reported.Count >= MaximumReports || !Reported.Add(key)) return;
                    }

                    string requiredNull = isStaticLocation
                        ? "Defense=" + defenseNull
                        : "Elevators=" + elevatorsNull + ",Common=" + commonNull + ",Corridors=" + corridorsNull;
                    UnityEngine.Debug.LogWarning("[Cvar_ImperiumOfMan] Mission preset diagnostic: station=" +
                        Label(mission?.StationId) + "; template=" + Label(template?.Id) +
                        "; firstStage=" + isFirstStage + "; static=" + isStaticLocation +
                        "; folderIndex=" + folderIndex + "; requestedFolder=" + Label(requestedFolder) +
                        "; resolvedFolder=" + Label(presets.Folder) + "; requiredNull={" + requiredNull +
                        "}; availableFolders=" + AvailableFolders());
                }
                catch (Exception)
                {
                    // A diagnostic failure must not replace native generation or
                    // its exception. The original method runs after this void prefix.
                }
            }

            private static string AvailableFolders()
            {
                var all = Data.PresetsMap?.AllPresets;
                if (all == null) return "<unavailable>";
                int shown = Math.Min(all.Count, MaximumFolders);
                var names = new List<string>(shown);
                for (int i = 0; i < shown; i++) names.Add(Label(all[i].Folder));
                return "[" + string.Join(",", names) + "] (total=" + all.Count +
                    ",omitted=" + (all.Count - shown) + ")";
            }

            private static string Label(string value)
            {
                if (value == null) return "<null>";
                if (value.Length > MaximumLabelLength) value = value.Substring(0, MaximumLabelLength) + "...";
                return "\"" + value.Replace("\\", "\\\\").Replace("\r", "\\r").Replace("\n", "\\n")
                    .Replace("\"", "\\\"") + "\"";
            }
        }
    }
}

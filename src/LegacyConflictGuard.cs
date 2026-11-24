using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using MGSC;
using UnityEngine;

namespace ImperiumOfMan
{
    /// <summary>
    /// Prevents the maintained fork and the discontinued Imperium of Man package
    /// from registering the same faction, station, item, descriptor, and Harmony IDs
    /// in one Quasimorph session.
    ///
    /// The guard deliberately disables this fork for the current session instead of
    /// trying to unload another assembly. Unity/Mono assemblies cannot be safely
    /// unloaded after load, and partial unregistration would risk save corruption.
    /// </summary>
    public static class LegacyConflictGuard
    {
        public const string CurrentUniqueModName = "Cvar_ImperiumOfMan";
        public const string LegacyUniqueModName = "andre_ImperiumOfMan";
        public const string SharedFactionId = "iom_faction";
        public const string LegacyWorkshopItemId = "3416931502";
        public const string QuasimorphWorkshopAppId = "2059170";


        private static readonly HashSet<string> ReportedReasons =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private static bool _warningShown;

        public static bool ConflictDetected { get; private set; }

        public static bool BeforeImperiumRegistration(IModContext context)
        {
            EvaluateConflict(context);

            if (!ConflictDetected)
            {
                return true;
            }

            string details = BuildWarningDetails();
            Debug.LogError("[Cvar_ImperiumOfMan] Startup blocked by legacy conflict guard. " + details);
            ShowConflictWarning(details);
            return false;
        }

        private static void EvaluateConflict(IModContext context)
        {
            foreach (string manifestPath in EnumerateCandidateManifests(context))
            {
                if (ManifestDeclaresLegacyPackage(manifestPath))
                {
                    AddConflictReason("Legacy manifest detected: " + manifestPath);
                }
            }

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                string assemblyLocation = GetAssemblyLocation(assembly);
                if (string.IsNullOrWhiteSpace(assemblyLocation))
                {
                    continue;
                }

                string assemblyDirectory = Path.GetDirectoryName(assemblyLocation);
                if (string.IsNullOrWhiteSpace(assemblyDirectory))
                {
                    continue;
                }

                string manifestPath = Path.Combine(assemblyDirectory, "modmanifest.json");
                if (ManifestDeclaresLegacyPackage(manifestPath))
                {
                    AddConflictReason("Loaded legacy mod assembly detected beside: " + manifestPath);
                }
            }

            if (IsImperiumFactionAlreadyRegistered())
            {
                AddConflictReason(
                    "The shared faction ID '" + SharedFactionId +
                    "' was already registered before this fork began registration.");
            }

            if (ReportedReasons.Count > 0)
            {
                ConflictDetected = true;
                ShowConflictWarning(BuildWarningDetails());
            }
        }

        private static void AddConflictReason(string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                return;
            }

            if (ReportedReasons.Add(reason))
            {
                Debug.LogError("[Cvar_ImperiumOfMan] " + reason);
            }
        }

        private static bool IsImperiumFactionAlreadyRegistered()
        {
            try
            {
                FactionRecord existingFaction = Data.Factions.GetRecord(SharedFactionId);
                return existingFaction != null;
            }
            catch
            {
                return false;
            }
        }

        private static string GetAssemblyLocation(Assembly assembly)
        {
            try
            {
                if (assembly == null || assembly.IsDynamic)
                {
                    return string.Empty;
                }

                return assembly.Location;
            }
            catch
            {
                return string.Empty;
            }
        }

        private static IEnumerable<string> EnumerateCandidateManifests(IModContext context)
        {
            HashSet<string> yielded = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            HashSet<string> roots = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            AddSearchRoot(roots, context == null ? null : context.ModContentPath);

            if (context != null && !string.IsNullOrWhiteSpace(context.ModContentPath))
            {
                DirectoryInfo currentDirectory = new DirectoryInfo(context.ModContentPath);
                AddSearchRoot(roots, currentDirectory.Parent == null ? null : currentDirectory.Parent.FullName);
            }

            string gameRoot = GetGameRoot();
            AddSearchRoot(roots, Path.Combine(gameRoot, "Mods"));

            string steamAppsRoot = GetSteamAppsRoot(gameRoot);
            if (!string.IsNullOrWhiteSpace(steamAppsRoot))
            {
                AddSearchRoot(
                    roots,
                    Path.Combine(
                        steamAppsRoot,
                        "workshop",
                        "content",
                        QuasimorphWorkshopAppId,
                        LegacyWorkshopItemId));
            }

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                string assemblyLocation = GetAssemblyLocation(assembly);
                if (!string.IsNullOrWhiteSpace(assemblyLocation))
                {
                    AddSearchRoot(roots, Path.GetDirectoryName(assemblyLocation));
                }
            }

            foreach (string root in roots)
            {
                foreach (string manifestPath in EnumerateManifestFiles(root, 4))
                {
                    string normalizedPath = NormalizePath(manifestPath);
                    if (yielded.Add(normalizedPath))
                    {
                        yield return normalizedPath;
                    }
                }
            }
        }

        private static void AddSearchRoot(HashSet<string> roots, string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            try
            {
                string normalized = NormalizePath(path);
                if (Directory.Exists(normalized))
                {
                    roots.Add(normalized);
                }
            }
            catch
            {
                // Ignore inaccessible optional search roots. Other detection methods
                // and the duplicate-faction check still remain active.
            }
        }

        private static IEnumerable<string> EnumerateManifestFiles(string directory, int remainingDepth)
        {
            if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory))
            {
                yield break;
            }

            string manifestPath = Path.Combine(directory, "modmanifest.json");
            if (File.Exists(manifestPath))
            {
                yield return manifestPath;
            }

            if (remainingDepth <= 0)
            {
                yield break;
            }

            string[] childDirectories;
            try
            {
                childDirectories = Directory.GetDirectories(directory);
            }
            catch
            {
                yield break;
            }

            foreach (string childDirectory in childDirectories)
            {
                foreach (string childManifest in EnumerateManifestFiles(childDirectory, remainingDepth - 1))
                {
                    yield return childManifest;
                }
            }
        }

        private static bool ManifestDeclaresLegacyPackage(string manifestPath)
        {
            if (string.IsNullOrWhiteSpace(manifestPath) || !File.Exists(manifestPath))
            {
                return false;
            }

            try
            {
                string manifestText = File.ReadAllText(manifestPath);
                return manifestText.IndexOf(
                    LegacyUniqueModName,
                    StringComparison.OrdinalIgnoreCase) >= 0;
            }
            catch
            {
                return false;
            }
        }

        private static string GetGameRoot()
        {
            try
            {
                DirectoryInfo dataDirectory = new DirectoryInfo(Application.dataPath);
                return dataDirectory.Parent == null
                    ? Environment.CurrentDirectory
                    : dataDirectory.Parent.FullName;
            }
            catch
            {
                return Environment.CurrentDirectory;
            }
        }

        private static string GetSteamAppsRoot(string gameRoot)
        {
            try
            {
                DirectoryInfo gameDirectory = new DirectoryInfo(gameRoot);
                DirectoryInfo commonDirectory = gameDirectory.Parent;
                DirectoryInfo steamAppsDirectory = commonDirectory == null ? null : commonDirectory.Parent;
                return steamAppsDirectory == null ? string.Empty : steamAppsDirectory.FullName;
            }
            catch
            {
                return string.Empty;
            }
        }

        private static string NormalizePath(string path)
        {
            return Path.GetFullPath(path)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }

        private static string BuildWarningDetails()
        {
            string reasonText = ReportedReasons.Count == 0
                ? "A conflicting Imperium registration was detected."
                : string.Join("\n", ReportedReasons);

            return
                "The discontinued Imperium of Man mod ('" + LegacyUniqueModName +
                "') is still installed, loaded, or has already registered the shared faction data.\n\n" +
                reasonText +
                "\n\nBoth versions use the same faction and gameplay record IDs. Loading them together can create duplicate factions, broken stations, overwritten items, startup exceptions, and unsafe save data.\n\n" +
                "For safety, '" + CurrentUniqueModName +
                "' has cancelled its own content registration for this session. Disable, unsubscribe from, or remove the discontinued version, then restart Quasimorph.";
        }

        private static void ShowConflictWarning(string message)
        {
            if (_warningShown)
            {
                return;
            }

            _warningShown = true;

            GameObject warningObject = new GameObject("Cvar_ImperiumOfMan_LegacyConflictWarning");
            UnityEngine.Object.DontDestroyOnLoad(warningObject);

            LegacyConflictWarningOverlay overlay =
                warningObject.AddComponent<LegacyConflictWarningOverlay>();
            overlay.SetMessage(message);
        }
    }

    /// <summary>
    /// IMGUI fallback used so the conflict notice does not depend on an unstable
    /// or undocumented Quasimorph menu API.
    /// </summary>
    public sealed class LegacyConflictWarningOverlay : MonoBehaviour
    {
        private string _message = string.Empty;
        private bool _visible = true;

        public void SetMessage(string message)
        {
            _message = message ?? string.Empty;
        }

        private void OnGUI()
        {
            if (!_visible)
            {
                return;
            }

            GUI.depth = -10000;

            Color previousColor = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.78f);
            GUI.DrawTexture(
                new Rect(0f, 0f, Screen.width, Screen.height),
                Texture2D.whiteTexture);
            GUI.color = previousColor;

            float width = Mathf.Min(820f, Mathf.Max(360f, Screen.width - 40f));
            float height = Mathf.Min(570f, Mathf.Max(360f, Screen.height - 40f));
            Rect panel = new Rect(
                (Screen.width - width) * 0.5f,
                (Screen.height - height) * 0.5f,
                width,
                height);

            GUI.Box(panel, string.Empty);

            GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 24,
                fontStyle = FontStyle.Bold,
                wordWrap = true
            };

            GUIStyle messageStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.UpperLeft,
                fontSize = 16,
                wordWrap = true,
                richText = false
            };

            GUI.Label(
                new Rect(panel.x + 24f, panel.y + 18f, panel.width - 48f, 52f),
                "IMPERIUM OF MAN MOD CONFLICT",
                titleStyle);

            GUI.Label(
                new Rect(panel.x + 28f, panel.y + 78f, panel.width - 56f, panel.height - 160f),
                _message,
                messageStyle);

            float buttonWidth = 190f;
            float buttonY = panel.yMax - 62f;

            if (GUI.Button(
                new Rect(panel.center.x - buttonWidth - 10f, buttonY, buttonWidth, 38f),
                "Acknowledge Warning"))
            {
                _visible = false;
            }

            if (GUI.Button(
                new Rect(panel.center.x + 10f, buttonY, buttonWidth, 38f),
                "Quit Quasimorph"))
            {
                Application.Quit();
            }
        }
    }
}

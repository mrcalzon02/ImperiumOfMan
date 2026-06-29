#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace ImperiumOfMan.Editor
{
    internal static class ImperiumAssetInclusionValidator
    {
        private const string AssetRoot = "Assets/ImperiumOfManMod";
        private const string ManifestPath = "Assets/Manifest.asset";
        private const string BundleName = "ImperiumOfMan.bundle";

        private static readonly HashSet<string> BundleSourceExtensions =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ".asset",
                ".png",
                ".mp3",
                ".wav",
                ".prefab",
                ".mat"
            };

        [MenuItem("Imperium of Man/Assets/Repair Import Settings and Validate")]
        private static void RepairAndValidate()
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            string[] guids = AssetDatabase.FindAssets(string.Empty, new[] { AssetRoot });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string extension = Path.GetExtension(path);
                if (extension.Equals(".png", StringComparison.OrdinalIgnoreCase)
                    || extension.Equals(".mp3", StringComparison.OrdinalIgnoreCase))
                {
                    AssetDatabase.ImportAsset(
                        path,
                        ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            ValidateSourceInclusion(true);
        }

        [MenuItem("Imperium of Man/Assets/Validate Source Inclusion")]
        private static void ValidateSourceInclusionMenu()
        {
            ValidateSourceInclusion(true);
        }

        [MenuItem("Imperium of Man/Assets/Validate Built Bundle...")]
        private static void ValidateBuiltBundle()
        {
            string bundlePath = EditorUtility.OpenFilePanel(
                "Select the built ImperiumOfMan bundle",
                string.Empty,
                "bundle");

            if (string.IsNullOrEmpty(bundlePath))
            {
                return;
            }

            AssetBundle bundle = AssetBundle.LoadFromFile(bundlePath);
            if (bundle == null)
            {
                throw new InvalidOperationException(
                    "Unity could not load the selected bundle: " + bundlePath);
            }

            try
            {
                HashSet<string> includedPaths = new HashSet<string>(
                    bundle.GetAllAssetNames(),
                    StringComparer.OrdinalIgnoreCase);

                List<string> expectedPaths = GetBundleSourcePaths()
                    .Select(path => path.ToLowerInvariant())
                    .ToList();

                List<string> missingPaths = expectedPaths
                    .Where(path => !includedPaths.Contains(path))
                    .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (missingPaths.Count > 0)
                {
                    throw new InvalidOperationException(
                        "The selected bundle is missing " + missingPaths.Count
                        + " Imperium source assets:\n"
                        + string.Join("\n", missingPaths));
                }

                Debug.Log(
                    "[ImperiumOfMan] Built bundle validation passed. "
                    + includedPaths.Count + " assets are present in " + BundleName + ".");

                EditorUtility.DisplayDialog(
                    "Imperium asset validation",
                    "Bundle validation passed. All " + expectedPaths.Count
                    + " current source assets are included.",
                    "OK");
            }
            finally
            {
                bundle.Unload(true);
            }
        }

        internal static void ValidateSourceInclusion(bool showSuccessDialog)
        {
            List<string> errors = new List<string>();

            if (!Directory.Exists(AssetRoot))
            {
                errors.Add("Missing asset root folder: " + AssetRoot);
            }

            ValidateManifest(errors);
            ValidateMetadataPairs(errors);
            ValidateUniqueGuids(errors);
            ValidateImportedObjects(errors);
            ValidateDescriptorNames(errors);

            if (errors.Count > 0)
            {
                foreach (string error in errors)
                {
                    Debug.LogError("[ImperiumOfMan] " + error);
                }

                throw new InvalidOperationException(
                    "Imperium asset validation failed with " + errors.Count
                    + " error(s). Review the Unity Console before building the bundle.");
            }

            int sourceCount = GetBundleSourcePaths().Count;
            Debug.Log(
                "[ImperiumOfMan] Source inclusion validation passed for "
                + sourceCount + " bundle source assets.");

            if (showSuccessDialog)
            {
                EditorUtility.DisplayDialog(
                    "Imperium asset validation",
                    "Source validation passed for " + sourceCount
                    + " bundle source assets.",
                    "OK");
            }
        }

        private static void ValidateManifest(List<string> errors)
        {
            if (!File.Exists(ManifestPath))
            {
                errors.Add("Missing ThunderKit manifest: " + ManifestPath);
                return;
            }

            string assetRootGuid = AssetDatabase.AssetPathToGUID(AssetRoot);
            if (string.IsNullOrEmpty(assetRootGuid))
            {
                errors.Add("Unity has no GUID for the asset root folder: " + AssetRoot);
                return;
            }

            string manifestText = File.ReadAllText(ManifestPath);
            if (manifestText.IndexOf(assetRootGuid, StringComparison.OrdinalIgnoreCase) < 0)
            {
                errors.Add(
                    "ThunderKit manifest does not reference the Imperium asset-root GUID "
                    + assetRootGuid + ".");
            }

            if (manifestText.IndexOf(
                    "assetBundleName: " + BundleName,
                    StringComparison.Ordinal) < 0)
            {
                errors.Add(
                    "ThunderKit manifest does not define the expected bundle name "
                    + BundleName + ".");
            }
        }

        private static void ValidateMetadataPairs(List<string> errors)
        {
            if (!Directory.Exists(AssetRoot))
            {
                return;
            }

            foreach (string sourcePath in Directory.GetFiles(
                         AssetRoot,
                         "*",
                         SearchOption.AllDirectories))
            {
                string normalizedPath = NormalizePath(sourcePath);
                if (normalizedPath.EndsWith(".meta", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!File.Exists(normalizedPath + ".meta"))
                {
                    errors.Add("Missing Unity metadata: " + normalizedPath + ".meta");
                }
            }

            foreach (string metaPath in Directory.GetFiles(
                         AssetRoot,
                         "*.meta",
                         SearchOption.AllDirectories))
            {
                string normalizedMetaPath = NormalizePath(metaPath);
                string pairedPath = normalizedMetaPath.Substring(
                    0,
                    normalizedMetaPath.Length - ".meta".Length);

                if (!File.Exists(pairedPath) && !Directory.Exists(pairedPath))
                {
                    errors.Add("Orphaned Unity metadata with no paired asset: " + normalizedMetaPath);
                }
            }
        }

        private static void ValidateUniqueGuids(List<string> errors)
        {
            Dictionary<string, string> firstPathByGuid =
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (string metaPath in Directory.GetFiles(
                         "Assets",
                         "*.meta",
                         SearchOption.AllDirectories))
            {
                string normalizedPath = NormalizePath(metaPath);
                string text = File.ReadAllText(normalizedPath);
                Match match = Regex.Match(
                    text,
                    @"^guid:\s*([0-9a-fA-F]{32})\s*$",
                    RegexOptions.Multiline);

                if (!match.Success)
                {
                    errors.Add("Metadata file has no valid GUID: " + normalizedPath);
                    continue;
                }

                string guid = match.Groups[1].Value;
                string firstPath;
                if (firstPathByGuid.TryGetValue(guid, out firstPath))
                {
                    errors.Add(
                        "Duplicate Unity GUID " + guid + " in " + firstPath
                        + " and " + normalizedPath + ".");
                }
                else
                {
                    firstPathByGuid.Add(guid, normalizedPath);
                }
            }
        }

        private static void ValidateImportedObjects(List<string> errors)
        {
            foreach (string path in GetBundleSourcePaths())
            {
                string extension = Path.GetExtension(path);
                UnityEngine.Object loadedObject;

                if (extension.Equals(".png", StringComparison.OrdinalIgnoreCase))
                {
                    loadedObject = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                }
                else if (extension.Equals(".mp3", StringComparison.OrdinalIgnoreCase)
                         || extension.Equals(".wav", StringComparison.OrdinalIgnoreCase))
                {
                    loadedObject = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                }
                else
                {
                    loadedObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                }

                if (loadedObject == null)
                {
                    errors.Add(
                        "Unity could not import the bundle source as its expected type: " + path);
                }
            }
        }

        private static void ValidateDescriptorNames(List<string> errors)
        {
            Dictionary<string, string> firstPathByName =
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (string assetPath in Directory.GetFiles(
                         AssetRoot,
                         "*.asset",
                         SearchOption.AllDirectories))
            {
                string normalizedPath = NormalizePath(assetPath);
                string text = File.ReadAllText(normalizedPath);
                Match match = Regex.Match(text, @"^\s*m_Name:\s*(.+?)\s*$", RegexOptions.Multiline);
                if (!match.Success)
                {
                    continue;
                }

                string objectName = match.Groups[1].Value.Trim();
                if (string.IsNullOrEmpty(objectName))
                {
                    continue;
                }

                string firstPath;
                if (firstPathByName.TryGetValue(objectName, out firstPath))
                {
                    errors.Add(
                        "Duplicate serialized asset name '" + objectName + "' in "
                        + firstPath + " and " + normalizedPath + ".");
                }
                else
                {
                    firstPathByName.Add(objectName, normalizedPath);
                }
            }
        }

        private static List<string> GetBundleSourcePaths()
        {
            if (!Directory.Exists(AssetRoot))
            {
                return new List<string>();
            }

            return Directory.GetFiles(AssetRoot, "*", SearchOption.AllDirectories)
                .Select(NormalizePath)
                .Where(path => !path.EndsWith(".meta", StringComparison.OrdinalIgnoreCase))
                .Where(path => BundleSourceExtensions.Contains(Path.GetExtension(path)))
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static string NormalizePath(string path)
        {
            return path.Replace('\\', '/');
        }
    }
}
#endif

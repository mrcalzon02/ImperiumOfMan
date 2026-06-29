#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace ImperiumOfMan.Editor
{
    internal sealed class ImperiumAssetImportPolicy : AssetPostprocessor
    {
        internal const string AssetRoot = "Assets/ImperiumOfManMod/";

        private static bool IsManagedAsset(string path)
        {
            return !string.IsNullOrEmpty(path)
                && path.StartsWith(AssetRoot, StringComparison.OrdinalIgnoreCase);
        }

        private void OnPreprocessTexture()
        {
            if (!IsManagedAsset(assetPath)
                || !assetPath.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            TextureImporter importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100f;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.filterMode = FilterMode.Point;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.textureCompression = TextureImporterCompression.Compressed;
        }

        private void OnPreprocessAudio()
        {
            if (!IsManagedAsset(assetPath)
                || !assetPath.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            AudioImporter importer = (AudioImporter)assetImporter;
            importer.forceToMono = false;
            importer.loadInBackground = false;
            importer.ambisonic = false;

            AudioImporterSampleSettings settings = importer.defaultSampleSettings;
            settings.loadType = AudioClipLoadType.DecompressOnLoad;
            settings.compressionFormat = AudioCompressionFormat.Vorbis;
            settings.quality = 1f;
            settings.sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate;
            importer.defaultSampleSettings = settings;
        }
    }
}
#endif

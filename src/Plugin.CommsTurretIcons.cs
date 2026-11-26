using System;
using System.Collections.Generic;
using MGSC;
using UnityEngine;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private const int CommsTurretIconSize = 36;
        private static readonly Dictionary<string, (Sprite Normal, Sprite Locked)> CommsTurretIcons = new();
        private static readonly HashSet<string> CommsTurretIconFailures = new();
        private static readonly Color32 CommsTurretIconBackground = new(3, 8, 6, 255);
        private static readonly Color32[] CommsTurretNormalPalette =
        [
            new(9, 24, 17, 255), new(16, 40, 29, 255), new(24, 57, 44, 255),
            new(37, 78, 65, 255), new(57, 127, 104, 255), new(86, 143, 114, 255)
        ];
        private static readonly Color32[] CommsTurretLockedPalette =
        [
            new(30, 8, 13, 255), new(58, 7, 20, 255), new(88, 7, 28, 255),
            new(141, 17, 49, 255), new(181, 21, 62, 255), new(194, 22, 66, 255)
        ];

        // Call after staged item/descriptor registration, including custom sentry kits.
        // Both existing SupportIcon fallbacks stay assigned unless the complete pair succeeds.
        private static void ApplyCommsTurretIcon(MagnumPerkRecord perk, string packageId, string fallbackArt)
        {
            if (perk?.ContentDescriptor is not MagnumPerkDescriptor descriptor ||
                string.IsNullOrEmpty(packageId) || CommsTurretIconFailures.Contains(packageId)) return;

            if (!CommsTurretIcons.TryGetValue(packageId, out var icons) ||
                icons.Normal == null || icons.Locked == null)
            {
                Sprite normal = null;
                Sprite locked = null;
                try
                {
                    var package = Data.Items.GetSimpleRecord<PlaceableDeviceRecord>(packageId);
                    var source = package?.ItemDesc?.Icon;
                    if (source == null) throw new InvalidOperationException("Deployable kit has no item icon.");
                    var pixels = SampleCommsTurretIcon(source);
                    normal = CreateCommsTurretIcon(packageId, pixels, locked: false);
                    locked = CreateCommsTurretIcon(packageId, pixels, locked: true);
                    icons = (normal, locked);
                    CommsTurretIcons[packageId] = icons;
                }
                catch (Exception error)
                {
                    DestroyCommsTurretIcon(normal);
                    DestroyCommsTurretIcon(locked);
                    CommsTurretIconFailures.Add(packageId);
                    Debug.LogWarning("[Cvar_ImperiumOfMan] Turret upgrade icon kept its " + fallbackArt +
                        " fallback for " + packageId + ": " + error.Message);
                    return;
                }
            }

            descriptor._icon = icons.Normal;
            descriptor._lockedIcon = icons.Locked;
        }

        private static Color32[] SampleCommsTurretIcon(Sprite source)
        {
            var texture = source.texture;
            var vertices = source.vertices;
            var uv = source.uv;
            var triangles = source.triangles;
            var rect = source.rect;
            if (texture == null || vertices.Length < 3 || uv.Length != vertices.Length ||
                triangles.Length < 3 || triangles.Length % 3 != 0 ||
                rect.width <= 0 || rect.height <= 0 || source.pixelsPerUnit <= 0)
                throw new InvalidOperationException("Deployable kit has invalid sprite geometry.");
            // Desktop game sprites use ordinary RGBA textures. Preserve the fallback if
            // another platform supplies a separate alpha texture instead of losing its mask.
            if (source.associatedAlphaSplitTexture != null)
                throw new InvalidOperationException("Deployable kit uses a separate alpha texture.");

            // UVs and mesh vertices retain atlas rotation, trimming and tight packing.
            // Sprite.textureRect throws for tight sprites, and rect is not an atlas crop.
            Vector2 minUv = uv[0], maxUv = uv[0];
            for (int i = 1; i < uv.Length; i++)
            {
                minUv = Vector2.Min(minUv, uv[i]);
                maxUv = Vector2.Max(maxUv, uv[i]);
            }
            int cropX = Mathf.Clamp(Mathf.FloorToInt(minUv.x * texture.width), 0, texture.width - 1);
            int cropY = Mathf.Clamp(Mathf.FloorToInt(minUv.y * texture.height), 0, texture.height - 1);
            int cropWidth = Mathf.Clamp(Mathf.CeilToInt(maxUv.x * texture.width), cropX + 1, texture.width) - cropX;
            int cropHeight = Mathf.Clamp(Mathf.CeilToInt(maxUv.y * texture.height), cropY + 1, texture.height) - cropY;
            var atlasPixels = ReadCommsTurretAtlasCrop(texture, cropX, cropY, cropWidth, cropHeight);

            // These 36px canvases render as 18px minor nodes. Reserve two source pixels
            // for a frame that stays one logical pixel wide after the native-size reduction.
            const float contentSize = CommsTurretIconSize - 4;
            float scale = Mathf.Min(contentSize / rect.width, contentSize / rect.height);
            var offset = new Vector2((CommsTurretIconSize - rect.width * scale) * 0.5f,
                (CommsTurretIconSize - rect.height * scale) * 0.5f);
            for (int i = 0; i < vertices.Length; i++)
                vertices[i] = (vertices[i] * source.pixelsPerUnit + source.pivot) * scale + offset;

            var pixels = new Color32[CommsTurretIconSize * CommsTurretIconSize];
            bool hasArtwork = false;
            for (int triangle = 0; triangle < triangles.Length; triangle += 3)
            {
                int ia = triangles[triangle], ib = triangles[triangle + 1], ic = triangles[triangle + 2];
                Vector2 a = vertices[ia], b = vertices[ib], c = vertices[ic];
                Vector2 ab = b - a, ac = c - a;
                float denominator = CommsIconCross(ab, ac);
                if (Mathf.Abs(denominator) < 0.0001f) continue;

                int minX = Mathf.Max(2, Mathf.FloorToInt(Mathf.Min(a.x, Mathf.Min(b.x, c.x))));
                int maxX = Mathf.Min(CommsTurretIconSize - 3, Mathf.CeilToInt(Mathf.Max(a.x, Mathf.Max(b.x, c.x))));
                int minY = Mathf.Max(2, Mathf.FloorToInt(Mathf.Min(a.y, Mathf.Min(b.y, c.y))));
                int maxY = Mathf.Min(CommsTurretIconSize - 3, Mathf.CeilToInt(Mathf.Max(a.y, Mathf.Max(b.y, c.y))));
                for (int y = minY; y <= maxY; y++)
                for (int x = minX; x <= maxX; x++)
                {
                    Vector2 point = new Vector2(x + 0.5f, y + 0.5f) - a;
                    float wb = CommsIconCross(point, ac) / denominator;
                    float wc = CommsIconCross(ab, point) / denominator;
                    float wa = 1f - wb - wc;
                    if (wa < -0.0001f || wb < -0.0001f || wc < -0.0001f) continue;
                    Vector2 sampleUv = uv[ia] * wa + uv[ib] * wb + uv[ic] * wc;
                    int sampleX = Mathf.Clamp(Mathf.FloorToInt(sampleUv.x * texture.width) - cropX, 0, cropWidth - 1);
                    int sampleY = Mathf.Clamp(Mathf.FloorToInt(sampleUv.y * texture.height) - cropY, 0, cropHeight - 1);
                    Color32 pixel = atlasPixels[sampleY * cropWidth + sampleX];
                    pixels[y * CommsTurretIconSize + x] = pixel;
                    if (pixel.a >= 16 && CommsIconLuminance(pixel) > 8) hasArtwork = true;
                }
            }
            if (!hasArtwork) throw new InvalidOperationException("Deployable kit icon readback contained no artwork.");
            return pixels;
        }

        private static float CommsIconCross(Vector2 a, Vector2 b) => a.x * b.y - a.y * b.x;

        private static Color32[] ReadCommsTurretAtlasCrop(Texture2D source, int x, int y, int width, int height)
        {
            var previous = RenderTexture.active;
            bool previousSrgbWrite = GL.sRGBWrite;
            RenderTexture target = null;
            Texture2D readable = null;
            try
            {
                target = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32,
                    RenderTextureReadWrite.sRGB);
                target.filterMode = FilterMode.Point;
                GL.sRGBWrite = QualitySettings.activeColorSpace == ColorSpace.Linear;
                // Copy at one texel per pixel; the native atlas need not be CPU-readable.
                Graphics.Blit(source, target,
                    new Vector2((float)width / source.width, (float)height / source.height),
                    new Vector2((float)x / source.width, (float)y / source.height));
                RenderTexture.active = target;
                readable = new Texture2D(width, height, TextureFormat.RGBA32, false);
                readable.ReadPixels(new Rect(0, 0, width, height), 0, 0, false);
                return readable.GetPixels32();
            }
            finally
            {
                RenderTexture.active = previous;
                GL.sRGBWrite = previousSrgbWrite;
                if (target != null) RenderTexture.ReleaseTemporary(target);
                if (readable != null) UnityEngine.Object.Destroy(readable);
            }
        }

        private static int CommsIconLuminance(Color32 pixel) =>
            (pixel.r * 299 + pixel.g * 587 + pixel.b * 114) / 1000;

        private static Sprite CreateCommsTurretIcon(string packageId, Color32[] source, bool locked)
        {
            var palette = locked ? CommsTurretLockedPalette : CommsTurretNormalPalette;
            int maxLight = 1;
            foreach (Color32 pixel in source)
                if (pixel.a >= 16) maxLight = Math.Max(maxLight, CommsIconLuminance(pixel));

            var pixels = new Color32[source.Length];
            for (int y = 0; y < CommsTurretIconSize; y++)
            for (int x = 0; x < CommsTurretIconSize; x++)
            {
                int index = y * CommsTurretIconSize + x;
                Color32 color = CommsTurretIconBackground;
                if (x < 2 || y < 2 || x >= CommsTurretIconSize - 2 || y >= CommsTurretIconSize - 2)
                {
                    color = palette[3];
                }
                else
                {
                    Color32 pixel = source[index];
                    int light = CommsIconLuminance(pixel);
                    if (pixel.a >= 16 && light > 8)
                    {
                        // A short palette and nearest sampling keep the reduced kit legible.
                        // Retain shading rather than flattening the turret into a silhouette.
                        int shade = Mathf.Min(palette.Length - 1,
                            Mathf.FloorToInt(Mathf.Sqrt((float)light / maxLight) * palette.Length));
                        color = Color32.Lerp(color, palette[shade], pixel.a / 255f);
                    }
                }
                pixels[index] = color;
            }

            string name = "Comms " + packageId + (locked ? " Locked" : string.Empty);
            var texture = new Texture2D(CommsTurretIconSize, CommsTurretIconSize, TextureFormat.RGBA32, false)
            {
                name = name,
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            try
            {
                texture.SetPixels32(pixels);
                texture.Apply(false, true);
                var sprite = Sprite.Create(texture, new Rect(0, 0, CommsTurretIconSize, CommsTurretIconSize),
                    new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
                if (sprite == null) throw new InvalidOperationException("Could not create turret upgrade sprite.");
                sprite.name = name;
                return sprite;
            }
            catch
            {
                UnityEngine.Object.Destroy(texture);
                throw;
            }
        }

        private static void DestroyCommsTurretIcon(Sprite sprite)
        {
            if (sprite == null) return;
            UnityEngine.Object.Destroy(sprite.texture);
            UnityEngine.Object.Destroy(sprite);
        }
    }
}

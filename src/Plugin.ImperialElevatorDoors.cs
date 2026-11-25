using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MGSC;
using UnityEngine;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private static readonly System.Reflection.FieldInfo LiftBrokenSprite = AccessTools.Field(typeof(InteractElevator), "_brokenSprite");
        private static readonly Dictionary<(Sprite Donor, int Facade, int Crown), Sprite> ImperialLiftSprites = new();
        private static readonly Dictionary<Texture2D, Color32[]> NativeLiftPixels = new();

        [HarmonyPatch(typeof(MapObstacleFactory), nameof(MapObstacleFactory.Spawn))]
        private static class FitImperialElevatorDoors
        {
            private static void Postfix(State ____state, MapObstacle __result)
            {
                var lift = __result?.Elevator;
                if (lift == null || lift.IsPortal || lift.spRenderer == null ||
                    lift.liftAnimator?.Renderer == null || lift.floorAnimator?.Renderer == null ||
                    lift.liftAnimator.Sprites == null || lift.floorAnimator.Sprites == null ||
                    GetImperialRaidPalette(____state) == null)
                {
                    return;
                }

                var map = ____state.Get<MapGrid>();
                var position = __result.Position;
                bool mounted = map != null &&
                    map.GetCell(position.X, position.Y + 1)?.Type == MapCellType.FacadeWall &&
                    map.GetCell(position.X, position.Y + 2)?.Type == MapCellType.Wall;

                // Both renderers keep their native transforms. The structural tiles provide the
                // in-wall edges and roof wherever the lift sits in a facade.
                int FrontBase(SpriteRenderer renderer) => __result.Offset.Y +
                    Mathf.RoundToInt((renderer.transform.position.y - __result.transform.position.y) * 100f);
                int frontBase = FrontBase(lift.spRenderer);
                var nativeBroken = (Sprite)LiftBrokenSprite.GetValue(lift);
                LiftBrokenSprite.SetValue(lift, SkinLiftSprite(nativeBroken, frontBase, mounted));
                __result.DefaultSprite = SkinLiftSprite(__result.DefaultSprite, frontBase, mounted);
                RecolorLiftAnimator(lift.liftAnimator, FrontBase(lift.liftAnimator.Renderer), mounted);
                RecolorLiftAnimator(lift.floorAnimator, FrontBase(lift.floorAnimator.Renderer), mounted);
            }
        }

        private static void RecolorLiftAnimator(FrameAnimator animator, int baseY, bool mounted)
        {
            animator.SetSprites(animator.Sprites.Select(sprite => SkinLiftSprite(sprite, baseY, mounted)).ToArray());
            animator.Renderer.sprite = SkinLiftSprite(animator.Renderer.sprite, baseY, mounted);
        }

        private static Sprite SkinLiftSprite(Sprite donor, int baseY, bool mounted)
        {
            if (donor == null) return null;

            int bottom = baseY - Mathf.RoundToInt(donor.pivot.y * 100f / donor.pixelsPerUnit);
            int face = mounted ? Mathf.RoundToInt((ImperialTilePixels - bottom) * donor.pixelsPerUnit / 100f) : int.MaxValue;
            int crown = mounted ? Mathf.RoundToInt((ImperialTilePixels * 2 - bottom) * donor.pixelsPerUnit / 100f) : int.MaxValue;
            var key = (donor, face, crown);
            if (ImperialLiftSprites.TryGetValue(key, out var cached)) return cached;

            var source = donor.texture;
            if (!NativeLiftPixels.TryGetValue(source, out var pixels))
            {
                pixels = ReadNativeTexturePixels(source);
                NativeLiftPixels.Add(source, pixels);
            }

            var rect = donor.rect;
            int startX = (int)rect.x, startY = (int)rect.y;
            int width = (int)rect.width, height = (int)rect.height;
            var colors = new Color32[width * height];
            for (int y = 0; y < height && y < crown; y++)
            for (int x = 0; x < width; x++)
            {
                var color = pixels[(startY + y) * source.width + startX + x];
                if (color.a == 0) continue;
                if (y >= face)
                {
                    int edge = Math.Min(x, width - 1 - x);
                    // Give the actual facade a narrow reveal beside the native lift jamb.
                    if (edge < 3) color.a = (byte)(color.a * edge / 3);
                }
                colors[y * width + x] = ImperialLiftMetal(color);
            }

            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                name = "ImperialLift_" + donor.name,
                filterMode = source.filterMode,
                wrapMode = TextureWrapMode.Clamp,
            };
            texture.SetPixels32(colors);
            texture.Apply(false, true);
            var pivot = new Vector2(donor.pivot.x / width, donor.pivot.y / height);
            var sprite = Sprite.Create(texture, new Rect(0, 0, width, height), pivot, donor.pixelsPerUnit,
                0, SpriteMeshType.FullRect, donor.border);
            sprite.name = texture.name;
            ImperialLiftSprites.Add(key, sprite);
            return sprite;
        }

        private static Color32 ImperialLiftMetal(Color32 color)
        {
            int light = (54 * color.r + 183 * color.g + 19 * color.b) >> 8;
            byte Shade(int percent) => (byte)Math.Min(255, light * percent / 100);

            // Retain the live green indicators and use dark bronze, oxblood and warm iron for the hardware.
            if (color.g > 96 && color.g - color.r > 28 && color.g - color.b > 21) return color;
            if (color.r > color.g + 12 && color.r > color.b + 16)
            {
                return color.g * 100 >= color.r * 63
                    ? new Color32(Shade(122), Shade(97), Shade(62), color.a)
                    : new Color32(Shade(149), Shade(70), Shade(58), color.a);
            }
            return new Color32(Shade(112), Shade(102), Shade(88), color.a);
        }
    }
}

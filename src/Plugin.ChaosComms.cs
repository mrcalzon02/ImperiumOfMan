using System;
using System.IO;
using HarmonyLib;
using MGSC;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        // The atlas stores only the eye region, with frames in reading order.
        private const int ChaosEyeLeft = 66;
        private const int ChaosEyeTop = 81;
        private const int ChaosEyeWidth = 99;
        private const int ChaosEyeHeight = 53;
        private const int ChaosEyeFramesPerSecond = 20;
        private const int ChaosEyeFrameCount = 120;

        private static PortraitDescriptor _chaosCommsPortrait;
        private static Color32[][] _chaosEyeFrames;
        private static float _chaosCommsStartedAt;
        private static int _chaosEyeFrame = -1;

        private static void RegisterChaosUndividedComms()
        {
            const string portraitId = "faction_iom_chaos_undivided";
            const string artwork = "ChaosUndividedComms";
            _chaosEyeFrames = LoadChaosEyeFrames();
            var portrait = ScriptableObject.CreateInstance<PortraitDescriptor>();
            portrait.name = artwork;
            portrait._normalSprite = ReadItemSprite(artwork, null, keepReadable: true);
            _chaosCommsPortrait = portrait;
            _chaosEyeFrame = -1;

            StageRecord(Data.Portraits, portraitId, new ConfigTableRecord
            {
                Id = portraitId,
                ContentDescriptor = portrait
            });
            StageDescriptor("portraits", portraitId, portrait);
        }

        private static Color32[][] LoadChaosEyeFrames()
        {
            var atlasSprite = ReadItemSprite("ChaosUndividedCommsEye", null, keepReadable: true);
            var atlas = atlasSprite.texture;
            try
            {
                int columns = atlas.width / ChaosEyeWidth;
                int rows = atlas.height / ChaosEyeHeight;
                if (atlas.width % ChaosEyeWidth != 0 || atlas.height % ChaosEyeHeight != 0 ||
                    columns * rows != ChaosEyeFrameCount)
                {
                    throw new InvalidDataException("Invalid Chaos Undivided comms eye atlas dimensions.");
                }

                Color32[] pixels = atlas.GetPixels32();
                var frames = new Color32[ChaosEyeFrameCount][];
                for (int frame = 0; frame < frames.Length; frame++)
                {
                    int x = frame % columns * ChaosEyeWidth;
                    int y = (rows - 1 - frame / columns) * ChaosEyeHeight;
                    var eye = new Color32[ChaosEyeWidth * ChaosEyeHeight];
                    for (int row = 0; row < ChaosEyeHeight; row++)
                    {
                        Array.Copy(pixels, (y + row) * atlas.width + x, eye, row * ChaosEyeWidth, ChaosEyeWidth);
                    }
                    frames[frame] = eye;
                }
                return frames;
            }
            finally
            {
                Object.Destroy(atlasSprite);
                Object.Destroy(atlas);
            }
        }

        private static void SetChaosEyeFrame(int frame)
        {
            if (_chaosEyeFrame == frame)
            {
                return;
            }

            Texture2D texture = _chaosCommsPortrait.NormalSprite.texture;
            int y = texture.height - ChaosEyeTop - ChaosEyeHeight;
            texture.SetPixels32(ChaosEyeLeft, y, ChaosEyeWidth, ChaosEyeHeight, _chaosEyeFrames[frame]);
            texture.Apply(updateMipmaps: false);
            _chaosEyeFrame = frame;
        }

        [HarmonyPatch(typeof(PortraitView), nameof(PortraitView.SetPortrait))]
        private static class StartChaosCommsEye
        {
            private static void Prefix(PortraitDescriptor portrait)
            {
                if (portrait != null && ReferenceEquals(portrait, _chaosCommsPortrait))
                {
                    _chaosCommsStartedAt = Time.time;
                    SetChaosEyeFrame(0);
                }
            }
        }

        [HarmonyPatch(typeof(PortraitView), "Update")]
        private static class AnimateChaosCommsEye
        {
            private static void Prefix(PortraitDescriptor ____portrait)
            {
                if (____portrait != null && ReferenceEquals(____portrait, _chaosCommsPortrait))
                {
                    float elapsed = Time.time - _chaosCommsStartedAt;
                    int frame = (int)(elapsed * ChaosEyeFramesPerSecond) % ChaosEyeFrameCount;
                    SetChaosEyeFrame(frame);
                }
            }
        }
    }
}

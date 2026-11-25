using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MGSC;
using UnityEngine;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private static void ApplyChaosActorModel(ChaosEnemy enemy, ActorDescriptor actor, ActorDescriptor template)
        {
            var templateMesh = template.LoadedMesh;
            var animations = templateMesh.GripAnimationMeshes[HandsGrip.BareHands];
            Mesh reference = animations[AnimationType.Idle][0].FrameMesh;
            var (points, uv, triangles) = ReadEmbeddedMesh(enemy.Id);
            bool daemonEngine = enemy.Body == "mars_cyborg";
            if (!daemonEngine)
            {
                // The donor supplies the skin, limbs and joints. Keep the authored outer surfaces.
                var attachments = new List<int>();
                for (int i = 0; i < triangles.Length; i += 3)
                {
                    if (uv[triangles[i]].y < 0.90f)
                    {
                        attachments.Add(triangles[i]);
                        attachments.Add(triangles[i + 1]);
                        attachments.Add(triangles[i + 2]);
                    }
                }
                triangles = attachments.ToArray();
            }
            var attachmentSkin = ReadItemSprite(enemy.Id + "_model", null, keepReadable: true).texture;
            var skin = MakeChaosActorSkin(template.Skin[0], attachmentSkin, out var donorRegion, out var attachmentRegion);
            for (int i = 0; i < uv.Length; i++) uv[i] = MapActorUv(uv[i], attachmentRegion);

            Bounds authored = new(points[0], Vector3.zero);
            foreach (var point in points)
            {
                authored.Encapsulate(point);
            }

            Bounds native = reference.bounds;
            float height = Mathf.Clamp(native.size.y, 0.29f, 0.42f) * enemy.Size;
            float scale = height / authored.size.y;
            Vector3 authoredFeet = new(authored.center.x, authored.min.y, authored.center.z);
            Vector3 nativeFeet = new(native.center.x, native.min.y, native.center.z);
            for (int i = 0; i < points.Length; i++)
            {
                points[i] = nativeFeet + (points[i] - authoredFeet) * scale;
            }

            var referencePoints = reference.vertices;
            var mappings = points.Select(point => NearestPoseVertex(referencePoints, point)).ToArray();
            var packed = new PackedActorMesh { BonesIds = templateMesh.BonesIds };
            var customAnimations = new Dictionary<AnimationType, MeshFrameData[]>();
            packed.GripAnimationMeshes.Add(HandsGrip.BareHands, customAnimations);
            foreach (var entry in animations)
            {
                var animation = entry.Key;
                var frames = entry.Value;
                var customFrames = new MeshFrameData[frames.Length];
                for (int i = 0; i < frames.Length; i++)
                {
                    var frame = frames[i];
                    float fall = (float)i / Math.Max(1, frames.Length - 1);
                    Mesh bodyFrame = animation == AnimationType.Death ? reference : frame.FrameMesh;
                    var bodyPose = ReshapeChaosDonor(enemy, bodyFrame.vertices, native, nativeFeet, height);
                    var bodyUv = bodyFrame.uv;
                    var bodyTriangles = bodyFrame.triangles;
                    var graftPose = PoseChaosActor(points, referencePoints, mappings, bodyFrame, native, height);
                    var pose = new Vector3[graftPose.Length + bodyPose.Length];
                    graftPose.CopyTo(pose, 0);
                    bodyPose.CopyTo(pose, graftPose.Length);
                    if (animation == AnimationType.Death) PoseChaosCorpse(pose, nativeFeet, height, fall);

                    var combinedUv = new Vector2[pose.Length];
                    uv.CopyTo(combinedUv, 0);
                    for (int j = 0; j < bodyUv.Length; j++) combinedUv[uv.Length + j] = MapActorUv(bodyUv[j], donorRegion);
                    var combinedTriangles = new int[triangles.Length + bodyTriangles.Length];
                    triangles.CopyTo(combinedTriangles, 0);
                    for (int j = 0; j < bodyTriangles.Length; j++) combinedTriangles[triangles.Length + j] = bodyTriangles[j] + graftPose.Length;
                    var mesh = new Mesh
                    {
                        name = $"{enemy.Id}_{animation}_{i}",
                        vertices = pose,
                        uv = combinedUv,
                        triangles = combinedTriangles
                    };
                    mesh.RecalculateNormals();
                    mesh.RecalculateBounds();
                    mesh.UploadMeshData(markNoLongerReadable: true);
                    customFrames[i] = new MeshFrameData { FrameMesh = mesh, BonePositions = frame.BonePositions };
                }
                customAnimations.Add(animation, customFrames);
            }

            actor._skin = [skin];
            actor._activityStatusY = Mathf.Max(actor._activityStatusY, height + 0.03f);
            actor.LoadedMesh = packed;
        }

        private static Texture2D MakeChaosActorSkin(Texture donor, Texture2D attachment, out Rect donorRegion, out Rect attachmentRegion)
        {
            var donorPixels = ReadNativeTexturePixels(donor);

            Color tint = attachment.GetPixel(attachment.width * 5 / 8, attachment.height * 11 / 12);
            for (int i = 0; i < donorPixels.Length; i++)
            {
                Color pixel = donorPixels[i];
                float light = 0.58f + pixel.grayscale * 0.95f;
                var glaze = Color.Lerp(pixel, tint * light, 0.57f);
                glaze.a = pixel.a;
                donorPixels[i] = glaze;
            }

            int width = donor.width + attachment.width + 2;
            int height = Math.Max(donor.height, attachment.height);
            var skin = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                name = attachment.name + "_combined",
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            skin.SetPixels32(0, 0, donor.width, donor.height, donorPixels);
            skin.SetPixels32(donor.width + 2, 0, attachment.width, attachment.height, attachment.GetPixels32());
            skin.Apply(false, true);
            donorRegion = new Rect(0.5f / width, 0.5f / height, (donor.width - 1f) / width, (donor.height - 1f) / height);
            attachmentRegion = new Rect((donor.width + 2.5f) / width, 0.5f / height,
                (attachment.width - 1f) / width, (attachment.height - 1f) / height);
            return skin;
        }

        private static Color32[] ReadNativeTexturePixels(Texture source)
        {
            if (source is Texture2D readable && readable.isReadable)
            {
                return readable.GetPixels32();
            }

            var canvas = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGB32);
            var previous = RenderTexture.active;
            var copy = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
            try
            {
                Graphics.Blit(source, canvas);
                RenderTexture.active = canvas;
                copy.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0, false);
                return copy.GetPixels32();
            }
            finally
            {
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(canvas);
                UnityEngine.Object.Destroy(copy);
            }
        }

        private static Vector2 MapActorUv(Vector2 point, Rect region) =>
            region.position + Vector2.Scale(region.size, new Vector2(Mathf.Clamp01(point.x), Mathf.Clamp01(point.y)));

        private static Vector3[] ReshapeChaosDonor(ChaosEnemy enemy, Vector3[] points, Bounds rest, Vector3 feet, float height)
        {
            var proportions = enemy.Id switch
            {
                ChaosFury => new Vector3(1.13f, 0.69f, 1.12f),
                ChaosMutant => new Vector3(0.97f, 0.86f, 1.02f),
                ChaosItemPrefix + "torment" => new Vector3(1.24f, 0.81f, 1.20f),
                ChaosItemPrefix + "spawn" => new Vector3(1.42f, 0.86f, 1.21f),
                ChaosItemPrefix + "possessed" => new Vector3(1.09f, 0.87f, 0.99f),
                ChaosItemPrefix + "warp_talon" => new Vector3(0.85f, 0.85f, 0.94f),
                ChaosItemPrefix + "venomcrawler" => new Vector3(0.56f, 0.42f, 1.12f),
                ChaosItemPrefix + "obliterator" => new Vector3(1.32f, 0.90f, 1.20f),
                ChaosItemPrefix + "soul_grinder" => new Vector3(0.65f, 0.45f, 1.24f),
                ChaosPrince => new Vector3(1.10f, 0.79f, 1.04f),
                _ => new Vector3(1.00f, 0.79f, 1.05f)
            };
            proportions *= height / rest.size.y;
            float mount = enemy.Body == "mars_cyborg" ? height * 0.26f : 0f;
            for (int i = 0; i < points.Length; i++)
            {
                var local = Vector3.Scale(points[i] - feet, proportions);
                float waist = Mathf.Clamp01(local.y / height);
                if (enemy.Id is ChaosItemPrefix + "torment" or ChaosItemPrefix + "spawn")
                {
                    local.x += height * 0.21f * waist * waist;
                    local.z += height * 0.16f * waist;
                }
                points[i] = feet + local + Vector3.up * mount;
            }
            return points;
        }

        private static (Vector3[] Points, Vector2[] Uv, int[] Triangles) ReadEmbeddedMesh(string id)
        {
            using var stream = ModAssembly.GetManifestResourceStream($"ImperiumOfMan.Art.{id}.meshbytes")
                ?? throw new InvalidDataException($"Missing embedded mesh for {id}");
            using var reader = new BinaryReader(stream);
            const uint formatVersion = 0x314D4349; // ICM1, stored little-endian.
            uint format = reader.ReadUInt32();
            int pointCount = reader.ReadInt32();
            int faceCount = reader.ReadInt32();
            if (format != formatVersion || pointCount is < 3 or > 65535 || faceCount < 1 ||
                stream.Length != 12L + 20L * pointCount + 12L * faceCount)
            {
                throw new InvalidDataException($"Invalid embedded mesh for {id}");
            }
            var points = new Vector3[pointCount];
            var uv = new Vector2[pointCount];
            var triangles = new int[faceCount * 3];
            for (int i = 0; i < pointCount; i++)
            {
                points[i] = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                uv[i] = new Vector2(reader.ReadSingle(), reader.ReadSingle());
            }
            for (int i = 0; i < triangles.Length; i++)
            {
                int index = reader.ReadInt32();
                if (index < 0 || index >= pointCount)
                {
                    throw new InvalidDataException($"Invalid embedded mesh index for {id}");
                }
                triangles[i] = index;
            }
            return (points, uv, triangles);
        }

        private static int NearestPoseVertex(Vector3[] reference, Vector3 point)
        {
            int nearest = 0;
            float best = float.MaxValue;
            for (int i = 0; i < reference.Length; i++)
            {
                float distance = (reference[i] - point).sqrMagnitude;
                if (distance < best)
                {
                    nearest = i;
                    best = distance;
                }
            }
            return nearest;
        }

        private static Vector3[] PoseChaosActor(Vector3[] points, Vector3[] reference, int[] mappings,
            Mesh nativeFrame, Bounds nativeRest, float height)
        {
            var pose = new Vector3[points.Length];
            var frame = nativeFrame.vertices;
            Vector3 offset = nativeFrame.bounds.center - nativeRest.center;
            bool matchingTopology = frame.Length == reference.Length;
            for (int i = 0; i < pose.Length; i++)
            {
                Vector3 localMotion = matchingTopology ? frame[mappings[i]] - reference[mappings[i]] : offset;
                pose[i] = points[i] + Vector3.ClampMagnitude(Vector3.Lerp(offset, localMotion, 0.38f), height * 0.20f);
            }
            return pose;
        }

        private static void PoseChaosCorpse(Vector3[] pose, Vector3 feet, float height, float time)
        {
            var center = feet + Vector3.up * height * 0.40f;
            var fall = Quaternion.AngleAxis(86f * Mathf.SmoothStep(0f, 1f, time), Vector3.forward);
            float low = float.MaxValue;
            for (int i = 0; i < pose.Length; i++)
            {
                pose[i] = center + fall * (pose[i] - center);
                low = Mathf.Min(low, pose[i].y);
            }
            float correction = feet.y - low;
            for (int i = 0; i < pose.Length; i++)
            {
                pose[i].y += correction;
            }
        }
    }
}

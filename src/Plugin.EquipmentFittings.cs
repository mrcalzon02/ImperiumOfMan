using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using MGSC;
using UnityEngine;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private enum FittingSlot { Held, Head, Torso, Legs, Feet }

        private sealed class EquipmentFitting(string id)
        {
            internal readonly string Id = id;
            internal readonly (Vector3[] Points, Vector2[] Uv, int[] Triangles) Geometry = ReadEmbeddedMesh(id);
            internal readonly Texture2D Skin = ReadItemSprite(id + "_model", null, keepReadable: true).texture;
            internal Color Swatch(int surface) => Skin.GetPixel(Skin.width * 5 / 8, Skin.height * (11 - surface * 2) / 12);
        }

        private static readonly Dictionary<string, (EquipmentFitting Main, FittingSlot Slot, EquipmentFitting Shoulder)> EquipmentFittings = new();
        private static readonly Dictionary<(string, Mesh, bool, float), Mesh> FittedEquipmentMeshes = new();
        private static readonly Dictionary<(string, Texture), Texture2D> FittedEquipmentSkins = new();
        [ThreadStatic] private static string _equippingFittedItem;

        private static void RegisterEquipmentFittings(ItemRecord item)
        {
            var slot = item switch
            {
                WeaponRecord => FittingSlot.Held,
                HelmetRecord => FittingSlot.Head,
                ArmorRecord => FittingSlot.Torso,
                LeggingsRecord => FittingSlot.Legs,
                BootsRecord => FittingSlot.Feet,
                _ => (FittingSlot?)null
            };
            if (slot == null)
            {
                return;
            }
            string id = item.Id;
            var shoulder = slot == FittingSlot.Torso ? new EquipmentFitting(id + "_shoulder_fittings") : null;
            EquipmentFittings.Add(id, (new EquipmentFitting(id + "_fittings"), slot.Value, shoulder));
        }

        private static void RegisterImperialEquipmentFittings()
        {
            const string prefix = "ImperiumOfMan.Art.";
            const string suffix = "_fittings.meshbytes";
            foreach (string resource in ModAssembly.GetManifestResourceNames().Where(name =>
                name.StartsWith(prefix + "iom_", StringComparison.Ordinal) &&
                !name.StartsWith(prefix + ChaosItemPrefix, StringComparison.Ordinal) &&
                name.EndsWith(suffix, StringComparison.Ordinal) &&
                !name.EndsWith("_shoulder" + suffix, StringComparison.Ordinal)))
            {
                string id = resource.Substring(prefix.Length, resource.Length - prefix.Length - suffix.Length);
                if (Data.Items.GetRecord(id) is not CompositeItemRecord composite || composite.GetRecord<ItemRecord>() is not { } item)
                {
                    throw new InvalidOperationException($"Equipment fitting has no registered item {id}");
                }
                RegisterEquipmentFittings(item);
            }
        }

        [HarmonyPatch]
        private static class IdentifyFittedEquipment
        {
            private static MethodBase[] TargetMethods() =>
                [AccessTools.Method(typeof(Creature3dView), "EquipItem"), AccessTools.Method(typeof(Creature3dView), "RefreshWeapon")];

            private static void Prefix(string id, out string __state)
            {
                __state = _equippingFittedItem;
                _equippingFittedItem = id != null && EquipmentFittings.ContainsKey(id) ? id : null;
            }

            private static void Finalizer(string __state) => _equippingFittedItem = __state;
        }

        [HarmonyPatch(typeof(Creature3dView), "SetupMaterial")]
        private static class AttachEquipmentFittings
        {
            private static void Postfix(GameObject instance, List<Material> ____materials)
            {
                if (_equippingFittedItem == null)
                {
                    return;
                }
                // Fit before the native call reparents the mounted parts to the actor's bones.
                ApplyEquipmentFittings(_equippingFittedItem, instance, ____materials);
            }
        }

        private static void ApplyEquipmentFittings(string id, GameObject instance, List<Material> materials)
        {
            var (baseFitting, slot, shoulders) = EquipmentFittings[id];
            bool weapon = slot == FittingSlot.Held;
            ItemBone muzzle = weapon ? Array.Find(instance.GetComponentsInChildren<ItemBone>(true), bone => bone.TargetBoneId == "Muzzle") : null;
            foreach (var filter in instance.GetComponentsInChildren<MeshFilter>(true))
            {
                var renderer = filter.GetComponent<MeshRenderer>();
                if (filter.sharedMesh == null || renderer == null || renderer.sharedMaterial == null)
                {
                    continue;
                }
                var sourceMaterial = renderer.sharedMaterial;
                if (materials.Contains(sourceMaterial) && sourceMaterial.mainTexture is { } donorSkin)
                {
                    sourceMaterial.mainTexture = FinishEquipmentDonor(baseFitting, donorSkin);
                }

                string mount = filter.GetComponentInParent<ItemBone>(true)?.TargetBoneId ?? filter.sharedMesh.name;
                var (fitting, mirrored, shoulderScale) = FittingForMount(id, mount, slot, baseFitting, shoulders);
                if (fitting == null)
                {
                    continue;
                }
                var key = (fitting.Id, filter.sharedMesh, mirrored, shoulderScale);
                if (!FittedEquipmentMeshes.TryGetValue(key, out var mesh))
                {
                    Vector3? muzzlePosition = muzzle == null ? null : filter.transform.InverseTransformPoint(muzzle.transform.position);
                    mesh = FitEquipmentMesh(id, fitting, filter.sharedMesh.bounds, weapon, mirrored, shoulderScale, muzzlePosition);
                    FittedEquipmentMeshes.Add(key, mesh);
                }
                AddFittingRenderer(filter, renderer, mesh, fitting.Skin, materials);
            }
        }

        private static Texture2D FinishEquipmentDonor(EquipmentFitting fitting, Texture donor)
        {
            var key = (fitting.Id, donor);
            if (FittedEquipmentSkins.TryGetValue(key, out var skin))
            {
                return skin;
            }

            // Retain the native UV layout and its pixel shading. Map recessed cloth,
            // painted plates and bright hardware to the fitted kit's corresponding materials.
            var pixels = ReadNativeTexturePixels(donor);
            Color plate = fitting.Swatch(0);
            Color textile = fitting.Swatch(3);
            Color hardware = fitting.Swatch(5);
            for (int i = 0; i < pixels.Length; i++)
            {
                Color original = pixels[i];
                float light = original.grayscale;
                Color tone = Color.Lerp(textile, plate, Mathf.SmoothStep(0f, 1f, (light - 0.12f) / 0.36f));
                tone = Color.Lerp(tone, hardware, Mathf.SmoothStep(0f, 0.52f, (light - 0.48f) / 0.34f));
                var painted = Color.Lerp(original, tone * (0.51f + light * 1.15f), 0.83f);
                painted.a = original.a;
                pixels[i] = painted;
            }
            skin = new Texture2D(donor.width, donor.height, TextureFormat.RGBA32, false)
            {
                name = fitting.Id + "_donor_" + donor.name,
                filterMode = donor.filterMode,
                wrapMode = donor.wrapMode
            };
            skin.SetPixels32(pixels);
            skin.Apply(false, true);
            FittedEquipmentSkins.Add(key, skin);
            return skin;
        }

        private static (EquipmentFitting Fitting, bool Mirror, float Scale) FittingForMount(string id, string mount,
            FittingSlot slot, EquipmentFitting fitting, EquipmentFitting shoulders)
        {
            if (slot == FittingSlot.Held || (slot == FittingSlot.Head && mount == "Head") ||
                (slot == FittingSlot.Torso && mount == "Body"))
            {
                return (fitting, false, 1f);
            }
            bool left = mount.StartsWith("L", StringComparison.Ordinal);
            if (slot == FittingSlot.Torso && mount is "LShoulder" or "RShoulder")
            {
                bool offsetPlate = id is ChaosItemPrefix + "possessed_carapace_torso" or "iom_ordoxenos_coat";
                float asymmetry = left && offsetPlate ? 0.74f : 1f;
                return (shoulders, left, asymmetry);
            }
            if ((slot == FittingSlot.Legs && mount is "LLeg" or "RLeg") ||
                (slot == FittingSlot.Feet && mount is "LFeet" or "RFeet"))
            {
                return (fitting, left, 1f);
            }
            return (null, false, 1f);
        }

        private static Mesh FitEquipmentMesh(string item, EquipmentFitting fitting, Bounds donor, bool weapon,
            bool mirror, float shoulderScale, Vector3? muzzle)
        {
            Vector3 forward = !weapon ? Vector3.back : item switch
            {
                ChaosItemPrefix + "fury_fang_knife" => new Vector3(-0.68f, 0.73f, 0f),
                ChaosItemPrefix + "spawnbone_maul" => new Vector3(-0.13f, 0.99f, 0f),
                ChaosItemPrefix + "bound_cleaver" => new Vector3(-0.79f, 0.61f, 0f),
                ChaosItemPrefix + "hellforged_blade" or ChaosItemPrefix + "shadowblade" => new Vector3(-0.66f, 0.75f, 0f),
                "iom_arbites_shock_maul" => new Vector3(-0.60f, 0.80f, 0f),
                "iom_krieg_entrenching_tool" => new Vector3(-0.56f, 0.83f, 0f),
                "iom_ordoxenos_power_blade" or "iom_grey_knights_nemesis_sword" or "iom_golden_nemesis_sword" => new Vector3(-0.71f, 0.71f, 0f),
                _ => Vector3.left
            };
            forward.Normalize();
            Vector3 right = weapon ? Vector3.forward : Vector3.left;
            Vector3 up = Vector3.Cross(forward, right);
            Vector3 size = new(ProjectedSpan(donor, right), ProjectedSpan(donor, up), ProjectedSpan(donor, forward));
            Vector3 center = donor.center;
            if (muzzle is Vector3 bore)
            {
                // Terminate the new barrel fittings behind the original flash/trajectory origin.
                float rear = Vector3.Dot(center, forward) - size.z * 0.5f;
                float nose = Vector3.Dot(bore, forward);
                size.z = Mathf.Max(size.z * 0.55f, nose - rear);
                center = right * Vector3.Dot(bore, right) + up * (Vector3.Dot(bore, up) - size.y * 0.17f) + forward * (nose - size.z * 0.5f);
            }
            else if (weapon && forward != Vector3.left)
            {
                // Hold the added blade or hammer mass above the donor's grip.
                size.y *= 0.68f;
            }
            var (points, uv, sourceTriangles) = fitting.Geometry;
            var vertices = new Vector3[points.Length];
            float horizontal = (mirror ? -1f : 1f) * shoulderScale;
            for (int i = 0; i < points.Length; i++)
            {
                Vector3 point = points[i];
                vertices[i] = center + right * (point.x * size.x * horizontal) + up * (point.y * size.y) + forward * (point.z * size.z);
            }
            var triangles = sourceTriangles;
            if (mirror)
            {
                triangles = (int[])sourceTriangles.Clone();
                for (int i = 0; i < triangles.Length; i += 3)
                {
                    (triangles[i + 1], triangles[i + 2]) = (triangles[i + 2], triangles[i + 1]);
                }
            }
            var result = new Mesh { name = fitting.Id, vertices = vertices, uv = uv, triangles = triangles };
            result.RecalculateNormals();
            result.RecalculateBounds();
            return result;
        }

        private static float ProjectedSpan(Bounds bounds, Vector3 axis) =>
            Mathf.Abs(axis.x) * bounds.size.x + Mathf.Abs(axis.y) * bounds.size.y + Mathf.Abs(axis.z) * bounds.size.z;

        private static void AddFittingRenderer(MeshFilter parent, MeshRenderer donor, Mesh mesh, Texture2D skin, List<Material> materials)
        {
            var part = new GameObject(mesh.name, typeof(MeshFilter), typeof(MeshRenderer), typeof(EquipmentFittingView))
            {
                layer = donor.gameObject.layer
            };
            part.transform.SetParent(parent.transform, worldPositionStays: false);
            part.GetComponent<MeshFilter>().sharedMesh = mesh;
            var overlay = part.GetComponent<MeshRenderer>();
            overlay.enabled = donor.enabled;
            overlay.sortingLayerID = donor.sortingLayerID;
            overlay.sortingOrder = donor.sortingOrder;
            overlay.shadowCastingMode = donor.shadowCastingMode;
            overlay.receiveShadows = donor.receiveShadows;
            var material = new Material(donor.sharedMaterial);
            material.SetTexture("_MainTex", skin);
            material.SetColor("_Albedo", Color.white);
            overlay.sharedMaterial = material;
            materials.Add(material);
            part.GetComponent<EquipmentFittingView>().OwnMaterial(materials, material);
        }
    }

    public sealed class EquipmentFittingView : MonoBehaviour
    {
        private List<Material> _viewMaterials;
        private Material _material;

        internal void OwnMaterial(List<Material> viewMaterials, Material material)
        {
            _viewMaterials = viewMaterials;
            _material = material;
        }

        private void OnDestroy()
        {
            _viewMaterials?.Remove(_material);
            if (_material != null) Destroy(_material);
        }
    }
}

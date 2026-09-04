using System;
using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using MGSC;
using UnityEngine;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        public static readonly Logger Logger = new Logger();

        public const string ItemCategory = "iom_faction";
        private const string HarmonyId = "Cvar_ImperiumOfMan";

        public static AssetBundle ModBundle;

        [Hook(ModHookType.BeforeBootstrap)]
        public static void BeforeConfig(IModContext context)
        {
            new Harmony(HarmonyId).PatchAll(typeof(Plugin).Assembly);
            Debug.Log("[Cvar_ImperiumOfMan] BeforeBootstrap complete.");
        }

        [Hook(ModHookType.AfterBootstrap)]
        public static void AfterBootstrap(IModContext context)
        {
            if (context == null || string.IsNullOrWhiteSpace(context.ModContentPath))
            {
                Debug.LogError("[Cvar_ImperiumOfMan] Missing mod content path; registration aborted.");
                return;
            }

            string bundlePath = Path.Combine(context.ModContentPath, "imperiumofman.bundle");
            ModBundle = AssetBundle.LoadFromFile(bundlePath);
            if (ModBundle == null)
            {
                Debug.LogError("[Cvar_ImperiumOfMan] Failed to load required asset bundle: " + bundlePath);
                return;
            }

            RegisterFaction();
            RegisterUnitDrops();
            RegisterStations();
            RegisterEquipment();
            RegisterFactionDrops();

            Debug.Log("[Cvar_ImperiumOfMan] AfterBootstrap registration complete.");
        }

        private static T LoadRequiredAsset<T>(string assetName) where T : UnityEngine.Object
        {
            T asset = ModBundle.LoadAsset<T>(assetName);
            if (asset == null)
            {
                throw new InvalidDataException("Required Imperium asset is missing from imperiumofman.bundle: " + assetName);
            }

            return asset;
        }

        private static List<DmgResist> CreateResists(
            float blunt = 0f,
            float pierce = 0f,
            float lacer = 0f,
            float fire = 0f,
            float cold = 0f,
            float poison = 0f,
            float shock = 0f,
            float beam = 0f)
        {
            return
            [
                new() { damage = "blunt", resistPercent = blunt },
                new() { damage = "pierce", resistPercent = pierce },
                new() { damage = "lacer", resistPercent = lacer },
                new() { damage = "fire", resistPercent = fire },
                new() { damage = "cold", resistPercent = cold },
                new() { damage = "poison", resistPercent = poison },
                new() { damage = "shock", resistPercent = shock },
                new() { damage = "beam", resistPercent = beam }
            ];
        }
    }
}

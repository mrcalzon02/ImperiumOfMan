using HarmonyLib;
using MGSC;
using UnityEngine;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private sealed class CommsTurretType(string key, string packageId, string name, string[] upgradePrice)
        {
            public readonly string Key = key;
            public readonly string PackageId = packageId;
            public readonly string Name = name;
            public readonly string[] UpgradePrice = upgradePrice;
        }

        private sealed class CommsLocation(string rootId, string label, string positionKey, int row, string art)
        {
            public readonly string RootId = rootId;
            public readonly string Label = label;
            public readonly string PositionKey = positionKey;
            public readonly int Row = row;
            public readonly string Art = art;

            public string PerkId(int tier) => tier == 0 ? RootId : RootId + "_" + CommsTurretTypes[tier].Key;
        }

        // The existing turret catalogue's ascending equipment tiers. These are
        // replacements, not additional spawns. Damage types and recharge still
        // retain their normal strengths/tradeoffs; this is not a normalized DPS stat.
        private static readonly CommsTurretType[] CommsTurretTypes =
        [
            new("ballistic", "stationary_turret", "Ballistic turret", []),
            new("flamer", "fire_turret", "Flamer turret",
                ["fire_turret", "electrical_parts_container"]),
            new("beamer", "energy_turret", "Beamer turret",
                ["energy_turret", "electrical_parts_container", "communication_relay"]),
            new("arc", ArcSentry, "Arc Sentry",
                [ArcSentry, "ai_module", "communication_relay"]),
            new("lance", LanceSentry, "Lance Sentry",
                [LanceSentry, "ai_module", "darknet_unit", "communication_relay"])
        ];

        private static readonly CommsLocation[] CommsLocations =
        [
            new(SecurityIntrusion, "Random", "random", 1, "CommsSecurityIntrusion"),
            new(EntranceOverride, "Entrance", "entrance", 2, "CommsEntranceOverride"),
            new(ExtractionOverride, "Extraction", "extraction", 3, "CommsExtractionOverride")
        ];

        private static bool TryGetCommsUpgrade(string id, out CommsLocation location, out int tier)
        {
            foreach (var candidate in CommsLocations)
            {
                for (int index = 0; index < CommsTurretTypes.Length; index++)
                {
                    if (id != candidate.PerkId(index)) continue;
                    location = candidate;
                    tier = index;
                    return true;
                }
            }
            location = null;
            tier = -1;
            return false;
        }

        private static int EffectiveCommsTurretTier(MagnumProgression progression, CommsLocation location)
        {
            // Resolve independently for every location. Old saves have only the
            // original root IDs, which continue to select the ballistic turret.
            if (progression != null)
                for (int tier = CommsTurretTypes.Length - 1; tier > 0; tier--)
                    if (progression.IsPerkPurchased(location.PerkId(tier))) return tier;
            return 0;
        }

        private static bool TryResolveCommsTurretRole(CommsTurretType type, out string mobClass)
        {
            // Match native deployment's package-to-mob mapping. In particular,
            // custom Arc/Lance package IDs are not their *_ally mob-class IDs.
            var item = Data.Items.GetRecord(type.PackageId) as CompositeItemRecord;
            var kit = item?.GetRecord<PlaceableDeviceRecord>();
            mobClass = kit?.TargetObjectId;
            return !string.IsNullOrWhiteSpace(mobClass) && Data.MobClasses.GetRecord(mobClass) != null;
        }

        private static void FinishCommsTurretUpgrades()
        {
            // Arc/Lance are staged by Industry after the Comms tree is declared.
            // Bind their actual kit icons and roles only once staging has finished.
            foreach (var location in CommsLocations)
            {
                for (int tier = 1; tier < CommsTurretTypes.Length; tier++)
                {
                    var type = CommsTurretTypes[tier];
                    var perk = Data.MagnumPerks.GetRecord(location.PerkId(tier));
                    if (!TryResolveCommsTurretRole(type, out _))
                    {
                        perk.Enabled = false;
                        Debug.LogWarning("[Cvar_ImperiumOfMan] Comms turret upgrade unavailable: " + type.PackageId);
                        continue;
                    }
                    ApplyCommsTurretIcon(perk, type.PackageId, location.Art);
                }
            }
        }

        private static bool IsCommsMission(State state)
        {
            var raid = state.Get<RaidMetadata>();
            if (raid?.RaidType != RaidType.ProcMission) return false;
            // Custom story campaigns can use procedural objective types; the
            // mission's story flag, not the objective type, is authoritative.
            var mission = state.Get<Missions>()?.Get(raid);
            return mission != null && !mission.IsStoryMission;
        }

        [HarmonyPatch(typeof(MagnumProgression), nameof(MagnumProgression.IsAvailableToUpgrade))]
        private static class RequirePreviousCommsTurretTier
        {
            private static void Postfix(MagnumProgression __instance, string id, ref bool __result)
            {
                if (!__result || !TryGetCommsUpgrade(id, out var location, out int tier) || tier == 0)
                    return;
                // Native availability also follows purchased children backwards.
                // Each optional replacement must have its own immediate predecessor.
                __result = __instance.IsPerkPurchased(location.RootId) &&
                    __instance.IsPerkPurchased(location.PerkId(tier - 1));
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using MGSC;
using UnityEngine;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        // Existing Core donor IDs drawn from our Imperial route catalog:
        // machinery, archives, inspection bays, cargo and dispatch rooms.
        // Validate installed headers and safe fixtures before considering them.
        private static readonly int[] ImperialVarietyRoomIds =
        [
            2148, 2153, 3518, 2498, 2499, 3541, 1524, 1527, 3526,
            1076, 1089, 3506, 1241, 1275, 3508, 2135, 2137, 3513,
            3444, 3445, 3558, 2518, 3457, 3542
        ];

        private static ImperialSpinePlan TryAddImperialVarietyRooms(ImperialSpinePlan original,
            PresetLoader loader, PresetHeader[] ordinaryDonors, CampaignRouteLayout route,
            int width, int height, int seed)
        {
            try
            {
                var used = new HashSet<int>(original.Rooms.Select(room => room.Room.header.binaryId));
                var candidates = new Dictionary<int, PresetHeader>();
                void Include(PresetHeader header)
                {
                    if (!CanBuildSpineRoom(header) || !IsImperialSpineDonor(header) ||
                        used.Contains(header.binaryId) || candidates.ContainsKey(header.binaryId)) return;
                    int floor = NativeRoomFloorCount(header);
                    if (floor >= 48 && floor <= 160) candidates.Add(header.binaryId, header);
                }

                // A bounded, seeded rotation prevents the same additional
                // rooms dominating every floor; no speculative full preset scan.
                int start = (int)((uint)seed % ImperialVarietyRoomIds.Length);
                for (int i = 0; i < 12; i++)
                {
                    int id = PresetIdRegistry.NormalizePresetId(
                        ImperialVarietyRoomIds[(start + i) % ImperialVarietyRoomIds.Length]);
                    if (used.Contains(id)) continue;
                    try { Include(loader.LoadSpecificPreset(id)); }
                    catch (Exception ex)
                    {
                        Debug.LogWarning("[Cvar_ImperiumOfMan] Optional native room " + id + " unavailable: " + ex.Message);
                    }
                }
                foreach (var donor in ordinaryDonors)
                {
                    if (candidates.Count >= 12) break;
                    Include(donor);
                }
                if (candidates.Count == 0) return original;

                // The existing planner mutates and recenters its input.
                // Clone the plan, including room and door positions, so a
                // rejected addition never damages the accepted base plan.
                var trial = new ImperialSpinePlan();
                var copies = new Dictionary<CommonRoom, CommonRoom>();
                foreach (var entry in original.Rooms)
                {
                    var room = entry.Room.Clone();
                    room.LeftLowerCorner = entry.Room.LeftLowerCorner;
                    room.RightUpperCorner = entry.Room.RightUpperCorner;
                    trial.Rooms.Add(new ImperialSpineRoom(room, entry.Group));
                    copies.Add(entry.Room, room);
                }
                foreach (var door in original.Doors)
                    trial.Doors.Add(new ImperialSpineDoor(copies[door.Room], door.Joint, door.Outward, door.Spine, door.End));
                Array.Copy(original.Spines, trial.Spines, original.Spines.Length);
                trial.Elbows.AddRange(original.Elbows);
                trial.CorridorCells.UnionWith(original.CorridorCells);

                AddCampaignEndgameCombatRooms(trial, candidates.Values.Take(12).ToArray(), route,
                    CampaignPlanFloorCount(original) + 120, width, height, maximumRooms: 2, preferUnused: true);
                if (trial.Rooms.Count <= original.Rooms.Count) return original;
                if (AuditImperialSpinePlan(trial, width, height, out string failure)) return trial;
                Debug.LogWarning("[Cvar_ImperiumOfMan] Optional variety-room plan rejected: " + failure);
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[Cvar_ImperiumOfMan] Optional Imperial room variation skipped: " + ex.Message);
            }
            return original;
        }
    }
}

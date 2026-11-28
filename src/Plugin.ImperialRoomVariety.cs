using System;
using System.Collections.Generic;
using System.Linq;
using MGSC;
using UnityEngine;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private const int ImperialVarietyMaxRooms = 4;
        private const int ImperialVarietyMaxLoads = 21;
        private const int ImperialVarietyMaxCandidates = 24;
        private const int ImperialVarietyFloorBudget = 240;
        private const int ImperialVarietySearchBudget = 6000;

        private static ImperialSpinePlan TryAddImperialVarietyRooms(ImperialSpinePlan original,
            PresetLoader loader, PresetHeader[] ordinaryDonors, CampaignRouteLayout route,
            int width, int height, int seed)
        {
            try
            {
                var used = new HashSet<int>(original.Rooms.Select(room => room.Room.header.binaryId));
                var catalog = BuildImperialRoomThemeCatalog();
                var random = new System.Random(unchecked(seed ^ 0x524F4F4D));
                var candidates = new Dictionary<int, PresetHeader>();
                var themes = new Dictionary<int, ImperialRoomTheme>();
                var fixtures = new Dictionary<string, ImperialRoomTheme>(StringComparer.Ordinal);
                void Include(PresetHeader header)
                {
                    if (candidates.Count >= ImperialVarietyMaxCandidates || !CanBuildSpineRoom(header) ||
                        used.Contains(header.binaryId) || candidates.ContainsKey(header.binaryId) ||
                        !IsImperialSpineDonor(header)) return;
                    // Extra rooms are not allowed to inject special mission items
                    // or special encounter tables beyond the native selected rooms.
                    if ((header.genMetadata.SpecialItemsTableIds?.Count ?? 0) != 0 ||
                        (header.genMetadata.SpecialMonstersTableIds?.Count ?? 0) != 0 ||
                        header.genMetadata.SpecialItemsSpawnCount > 0) return;
                    if (header.MapGrid.MaxWidth < 5 || header.MapGrid.MaxHeight < 5 ||
                        header.MapGrid.MaxWidth > 22 || header.MapGrid.MaxHeight > 22) return;
                    int floor = NativeRoomFloorCount(header);
                    if (floor < 24 || floor > 160) return;
                    if (!SupplyClosetDirections.Any(side => SpineJoints(header, side).Any())) return;
                    themes.Add(header.binaryId, ClassifyImperialDonorTheme(header, catalog, fixtures));
                    candidates.Add(header.binaryId, header);
                }

                // Sample each route family in turn rather than letting the
                // much larger industrial pool crowd out archives and chapels.
                var families = ImperialRoomFamilies
                    .Select(family => new Queue<int>(family.LocalIds
                        .Select(PresetIdRegistry.NormalizePresetId).Distinct()
                        .Where(id => !used.Contains(id)).OrderBy(_ => random.Next())))
                    .OrderBy(_ => random.Next()).ToArray();
                var inspected = new HashSet<int>(used);
                int loads = 0;
                while (loads < ImperialVarietyMaxLoads && families.Any(queue => queue.Count > 0))
                {
                    foreach (var queue in families)
                    {
                        if (loads >= ImperialVarietyMaxLoads || queue.Count == 0) continue;
                        int id = queue.Dequeue();
                        if (!inspected.Add(id)) continue;
                        loads++;
                        try { Include(loader.LoadSpecificPreset(id)); }
                        catch (Exception ex)
                        {
                            Debug.LogWarning("[Cvar_ImperiumOfMan] Optional native room " + id + " unavailable: " + ex.Message);
                        }
                    }
                }
                foreach (var donor in ordinaryDonors.OrderBy(_ => random.Next()))
                {
                    if (candidates.Count >= ImperialVarietyMaxCandidates) break;
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

                AddCampaignEndgameCombatRooms(trial, candidates.Values.ToArray(), route,
                    CampaignPlanFloorCount(original) + ImperialVarietyFloorBudget, width, height,
                    maximumRooms: ImperialVarietyMaxRooms, preferUnused: true,
                    minimumFloor: 24, minimumSide: 5, limitToTarget: true,
                    searchBudget: ImperialVarietySearchBudget,
                    varietyFamily: header => (int)themes[header.binaryId]);
                Debug.Log("[Cvar_ImperiumOfMan] Imperial room variety | candidates=" + candidates.Count +
                    " | native loads=" + loads + " | themes=" +
                    string.Join(",", themes.Values.Distinct()) + " | proposed=" +
                    (trial.Rooms.Count - original.Rooms.Count));
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

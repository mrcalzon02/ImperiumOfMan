using System;
using System.Collections.Generic;
using System.Linq;
using MGSC;
using UnityEngine;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private enum ImperialRoomTheme
        {
            General, Corridor, Lift, Cargo, Workshop, Medical, Laboratory,
            Ceremonial, Relay, Objective, Archive, Security, Quarters, Utility, Survey
        }

        // These are route-derived candidate families, not claims about every
        // individual native preset's contents. Live fixtures take precedence.
        // IDs are sourced exclusively from the existing Imperial route and
        // ore-bay catalogs, and resolved by the installed game's registry.
        private static readonly (ImperialRoomTheme Theme, int[] LocalIds)[] ImperialRoomFamilies =
        [
            (ImperialRoomTheme.Workshop, [2148, 2153, 2155, 3518, 2156, 2157, 2154,
                1076, 1089, 1077, 3506, 1066, 1072, 1078, 1067, 1059, 1063,
                2135, 2137, 2140, 3513, 2131, 2138, 2139, 2130, 2132, 2133,
                3504, 1132, 3505, 1120, 1118, 1121, 1075, 1094, 1119, 2211, 3532, 1093, 1099]),
            (ImperialRoomTheme.Archive, [2498, 2499, 3541, 253, 3558, 370, 1772, 263, 68]),
            (ImperialRoomTheme.Security, [3525, 1524, 3526, 1527, 1521, 1523, 1522, 1525, 3527]),
            (ImperialRoomTheme.Survey, [1241, 1273, 1275, 1279, 3508, 1261, 1278, 1274, 1246, 1263, 1250]),
            (ImperialRoomTheme.Relay, [248, 247, 2492, 290, 244, 285]),
            (ImperialRoomTheme.Cargo, [1024, 3444, 3445, 3450, 3441, 952, 995, 3448, 3449, 3545]),
            (ImperialRoomTheme.Ceremonial, [2518, 2516, 3457, 3542, 2174, 2513, 2512, 2175, 3455, 3460])
        ];

        private static Dictionary<int, ImperialRoomTheme> BuildImperialRoomThemeCatalog()
        {
            var result = new Dictionary<int, ImperialRoomTheme>();
            foreach (var family in ImperialRoomFamilies)
            foreach (int local in family.LocalIds)
            {
                int id = PresetIdRegistry.NormalizePresetId(local);
                if (!result.ContainsKey(id)) result.Add(id, family.Theme);
            }
            return result;
        }

        private static ImperialRoomTheme ImperialThemeFromArea(NativePresetArea area) => area switch
        {
            NativePresetArea.Corridor => ImperialRoomTheme.Corridor,
            NativePresetArea.LiftLobby => ImperialRoomTheme.Lift,
            NativePresetArea.Cargo => ImperialRoomTheme.Cargo,
            NativePresetArea.Workshop => ImperialRoomTheme.Workshop,
            NativePresetArea.Medical => ImperialRoomTheme.Medical,
            NativePresetArea.Laboratory => ImperialRoomTheme.Laboratory,
            NativePresetArea.Ceremonial => ImperialRoomTheme.Ceremonial,
            NativePresetArea.Relay => ImperialRoomTheme.Relay,
            NativePresetArea.Objective => ImperialRoomTheme.Objective,
            _ => ImperialRoomTheme.General
        };

        // Additional themes are inferred from actual installed fixture names.
        // Unknown names safely fall back to the established room classifier.
        private static ImperialRoomTheme ImperialThemeFromFixture(MapObstacle obstacle)
        {
            if (obstacle == null) return ImperialRoomTheme.General;
            var area = NativeFixtureArea(obstacle).Area;
            if (area != NativePresetArea.General && area != NativePresetArea.Cargo)
                return ImperialThemeFromArea(area);
            string name = (obstacle.name ?? "").ToLowerInvariant();
            if (name.Contains("bookshelf") || name.Contains("bookcase") ||
                name.Contains("archive") || name.Contains("library")) return ImperialRoomTheme.Archive;
            if (name.Contains("weaponrack") || name.Contains("weapon_rack") ||
                name.Contains("armory") || name.Contains("security") ||
                name.Contains("turret")) return ImperialRoomTheme.Security;
            if (name.Contains("bunk") || name.Contains("bed") ||
                name.Contains("sofa")) return ImperialRoomTheme.Quarters;
            if (name.Contains("generator") || name.Contains("transformer") ||
                name.Contains("pump") || name.Contains("ventilation")) return ImperialRoomTheme.Utility;
            return ImperialThemeFromArea(area);
        }

        private static ImperialRoomTheme ClassifyImperialRoomTheme(GeneratedRoomMetadata room,
            List<MapObstacle> obstacles, IReadOnlyDictionary<int, ImperialRoomTheme> catalog)
        {
            var area = ClassifyNativePresetArea(room, obstacles);
            if (area != NativePresetArea.General && area != NativePresetArea.Cargo)
                return ImperialThemeFromArea(area);
            if (room.GroupId >= 200 || IsImperialSupplyClosetPreset(room.PresetBinaryId))
                return ImperialRoomTheme.Cargo;
            var actual = obstacles.Where(obstacle => room.ContainsPos(obstacle.Position))
                .Select(ImperialThemeFromFixture)
                .Where(theme => theme is ImperialRoomTheme.Archive or ImperialRoomTheme.Security or
                    ImperialRoomTheme.Quarters or ImperialRoomTheme.Utility)
                .GroupBy(theme => theme).OrderByDescending(group => group.Count())
                .ThenBy(group => group.Key).FirstOrDefault();
            if (actual != null) return actual.Key;
            if (area == NativePresetArea.Cargo) return ImperialRoomTheme.Cargo;
            return catalog.TryGetValue(room.PresetBinaryId, out var fallback)
                ? fallback : ImperialRoomTheme.General;
        }

        private static NativePresetArea ImperialThemeFloorArea(ImperialRoomTheme theme) => theme switch
        {
            ImperialRoomTheme.Cargo => NativePresetArea.Cargo,
            ImperialRoomTheme.Workshop or ImperialRoomTheme.Utility => NativePresetArea.Workshop,
            ImperialRoomTheme.Medical => NativePresetArea.Medical,
            ImperialRoomTheme.Laboratory or ImperialRoomTheme.Archive or ImperialRoomTheme.Survey => NativePresetArea.Laboratory,
            ImperialRoomTheme.Ceremonial => NativePresetArea.Ceremonial,
            ImperialRoomTheme.Relay or ImperialRoomTheme.Security => NativePresetArea.Relay,
            _ => NativePresetArea.General
        };

        private static ImperialRoomTheme ClassifyImperialDonorTheme(PresetHeader header,
            IReadOnlyDictionary<int, ImperialRoomTheme> catalog, Dictionary<string, ImperialRoomTheme> fixtureCache)
        {
            var factory = SingletonMonoBehaviour<MapObstacleFactory>.Instance;
            // See live native prefab names, not guessed numeric room labels.
            var scores = new Dictionary<ImperialRoomTheme, int>();
            foreach (var obj in header.mapMetadata.MapObstacles)
            {
                string id = obj.ObstacleName;
                if (string.IsNullOrEmpty(id)) continue;
                if (!fixtureCache.TryGetValue(id, out var theme))
                {
                    var prefab = factory.GetObstaclePrefab(id);
                    theme = ImperialThemeFromFixture(prefab);
                    fixtureCache.Add(id, theme);
                }
                if (theme == ImperialRoomTheme.General) continue;
                scores.TryGetValue(theme, out int count);
                scores[theme] = count + (theme == ImperialRoomTheme.Cargo ? 1 : 4);
            }
            var actual = scores.Where(entry => entry.Key != ImperialRoomTheme.Cargo || entry.Value >= 2)
                .OrderByDescending(entry => entry.Key == ImperialRoomTheme.Cargo ? Math.Min(3, entry.Value) : entry.Value)
                .ThenBy(entry => entry.Key).FirstOrDefault();
            if (actual.Value > 0) return actual.Key;
            return catalog.TryGetValue(header.binaryId, out var fallback) ? fallback : ImperialRoomTheme.General;
        }
    }
}

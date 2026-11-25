using System;
using System.Collections.Generic;
using System.Linq;
using MGSC;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private enum CampaignRouteRoomRole
        {
            Junction,
            HorizontalTerminal,
            VerticalTerminal,
            SideRoom,
            ElbowTerminal,
            ElbowSideRoom
        }

        private sealed class CampaignRouteLayout
        {
            internal readonly string Key;
            internal readonly string Title;
            internal readonly int HorizontalMinimum;
            internal readonly int VerticalMinimum;
            internal readonly int ElbowLength;
            internal readonly int DoorPitch;
            internal readonly Direction HorizontalDirection;
            internal readonly Direction VerticalDirection;
            internal readonly int[] HorizontalSidePattern;
            internal readonly int[] VerticalSidePattern;
            private readonly Dictionary<CampaignRouteRoomRole, int[]> _roomPreferences;
            private readonly int[] _localRoomIds;

            internal int[] PreferredRoomIds => _localRoomIds.Select(PresetIdRegistry.NormalizePresetId).ToArray();

            internal CampaignRouteLayout(string key, string title,
                (int Horizontal, int Vertical, int Elbow, int Pitch) lengths,
                (Direction Horizontal, Direction Vertical) directions, string horizontalDoors, string verticalDoors,
                int[] junctions, int[] horizontalTerminals, int[] verticalTerminals, int[] sideRooms,
                int[] elbowTerminals, int[] elbowSideRooms)
            {
                Key = key;
                Title = title;
                (HorizontalMinimum, VerticalMinimum, ElbowLength, DoorPitch) = lengths;
                (HorizontalDirection, VerticalDirection) = directions;
                HorizontalSidePattern = DoorSides(horizontalDoors);
                VerticalSidePattern = DoorSides(verticalDoors);
                _roomPreferences = new Dictionary<CampaignRouteRoomRole, int[]>
                {
                    [CampaignRouteRoomRole.Junction] = junctions,
                    [CampaignRouteRoomRole.HorizontalTerminal] = horizontalTerminals,
                    [CampaignRouteRoomRole.VerticalTerminal] = verticalTerminals,
                    [CampaignRouteRoomRole.SideRoom] = sideRooms,
                    [CampaignRouteRoomRole.ElbowTerminal] = elbowTerminals,
                    [CampaignRouteRoomRole.ElbowSideRoom] = elbowSideRooms
                };
                _localRoomIds = _roomPreferences.Values.SelectMany(ids => ids).Distinct().ToArray();
            }

            internal int RoomPreference(PresetHeader header, CampaignRouteRoomRole role)
            {
                if (header?.presetNamespace?.Kind != PresetNamespaceKind.Core) return 0;
                int localId = header.presetNamespace.ToLocalId(header.binaryId);
                var ids = _roomPreferences[role];
                int index = Array.IndexOf(ids, localId);
                return index >= 0 ? 10 + ids.Length - index : _localRoomIds.Contains(localId) ? 1 : 0;
            }

            private static int[] DoorSides(string sides) => sides.Select(side => side == '+' ? 1 : -1).ToArray();
        }

        // The room numbers refer to installed Core common presets. The mission rules still supply
        // the shuttle, lifts, objective rooms, and per-floor encounter tables.
        private static readonly CampaignRouteLayout[] CampaignRouteLayoutList =
        [
            // Imperial 11–20
            new("caloris_levy", "Caloris ore lanes and hoist galleries", (78, 64, 18, 8), (Direction.Right, Direction.Up),
                "+-++-", "-+--+", [2149, 2152], [3519, 2149], [2368, 2151], [2148, 2153, 2155, 3518, 2156],
                [2157, 2154, 3518], [2155, 3518, 2153]),
            new("counterfeit_orbit", "Boethius signal exchange and archive", (64, 76, 19, 7), (Direction.Left, Direction.Up),
                "++--+", "+-+-+", [2497, 3540], [2495, 2494], [2500, 2496], [2498, 2499, 3541, 253, 3558],
                [2494, 2496, 370], [1772, 263, 68]),
            new("goethe_deadlock", "Goethe gun deck and blockade command", (74, 68, 17, 9), (Direction.Right, Direction.Down),
                "+-+-+", "--++-", [1528, 1529], [1531, 1518], [1519, 1520], [3525, 1524, 3526, 1527, 1521],
                [1523, 1522, 1525], [1527, 3527, 1521]),
            new("extinguish_crucible", "Cinder Crucible forge and controller line", (86, 62, 20, 10), (Direction.Left, Direction.Down),
                "++-+-", "-++--", [1101, 1132], [1122, 3503], [1075, 1123], [1076, 1089, 1077, 3506, 1066],
                [1072, 1078, 1127], [1067, 1059, 1063]),
            new("discovery_injunction", "Discovery survey vault and inspection bays", (66, 72, 16, 8), (Direction.Right, Direction.Up),
                "-++-+", "+--+-", [1272, 1247], [1276, 1243], [1248, 3507], [1241, 1273, 1275, 1279, 3508],
                [1261, 1278, 1274], [1246, 1263, 1250]),
            new("hollow_signal", "Hollow Signal relay and beacon room", (62, 84, 21, 7), (Direction.Left, Direction.Down),
                "--+-+", "++-++", [2495, 2494], [3541, 2496], [2500, 3540], [1527, 248, 247, 2492, 2499],
                [1072, 290, 2498], [1772, 244, 285]),
            new("cinder_basin", "Cinder Basin shafts and lower galleries", (70, 86, 20, 9), (Direction.Right, Direction.Down),
                "-+-++", "++--+", [3514, 2129], [3515, 2134], [2136, 2128], [2135, 2137, 2140, 3513, 2131],
                [2138, 2139, 2137], [2130, 2132, 2133]),
            new("last_bonded_shipment", "Red Tiangong cargo exam hall and registry", (84, 66, 18, 9), (Direction.Left, Direction.Up),
                "++-++", "-+-++", [3440, 3442], [3443, 951], [1006, 370], [1024, 3444, 3445, 3450, 3558],
                [3441, 952, 995], [3448, 3449, 3545]),
            new("prime_the_foundry", "Primaris loading levels and interlock bays", (88, 74, 22, 10), (Direction.Right, Direction.Up),
                "-+++-", "+-++-", [1123, 3503], [1124, 1122], [1129, 1127], [3504, 1132, 3505, 1076, 1120],
                [1118, 1121, 1075], [1094, 1119, 1059]),
            new("one_standard", "Hesperia dispatch court and workshops", (80, 80, 23, 9), (Direction.Left, Direction.Down),
                "+--++", "-++-+", [2585, 3452], [2517, 2519], [3559, 1126], [2518, 2516, 3457, 3542, 370],
                [2174, 2513, 2512], [2175, 3455, 3460]),

            // Chaos Undivided 11–20 and the alternate defense route
            new("white_frequency", "Bleached Mast nave and transmitter wings", (64, 90, 21, 8), (Direction.Right, Direction.Down),
                "-+--+", "+++--", [3379, 3369], [2495, 3373], [2496, 3378], [3368, 3370, 3372, 2492, 3541],
                [3377, 2494, 2497], [3374, 1772, 3376]),
            new("scales_of_passage", "Blood Toll altar and gun deck", (77, 63, 18, 8), (Direction.Left, Direction.Up),
                "--+++", "+--+-", [3105, 2314], [3095, 102], [3102, 1529], [3100, 3103, 3104, 3526, 107],
                [3098, 2315, 3094], [3096, 3097, 101]),
            new("no_salvage_rights", "Kedge breaker berth and bonded files", (88, 61, 16, 9), (Direction.Right, Direction.Down),
                "++--+", "--+-+", [3443, 947], [1101, 3442], [1006, 3558], [3449, 1029, 3450, 1024, 1066],
                [1075, 1077, 994], [3451, 995, 3455]),
            new("furnace_of_ur", "Cinder Lock reactor cradles and engine bay", (81, 72, 22, 10), (Direction.Left, Direction.Down),
                "-++--", "+-+++", [3137, 3142], [3138, 3145], [3503, 1078], [3139, 3135, 3141, 1122, 1077],
                [3136, 3143, 1127], [3144, 3134, 1067]),
            new("the_right_to_pass", "Bleached Mast replacement berths and muster", (72, 65, 17, 7), (Direction.Left, Direction.Up),
                "+--+-", "++---", [3377, 246], [952, 3372], [3375, 3432], [3368, 3371, 290, 717, 715],
                [3376, 3378, 247], [3374, 285, 3435]),
            new("prism_rent", "Blue Ruin lens court and observatory", (69, 90, 23, 8), (Direction.Right, Direction.Up),
                "+---+", "-+++-", [3081, 3510], [3079, 1223], [3078, 1229], [3074, 3075, 3080, 1220, 1227],
                [3072, 3073, 3076], [3077, 1263, 1278]),
            new("starless_oath", "Sable Mile pact corridors and guard rooms", (74, 76, 19, 8), (Direction.Left, Direction.Down),
                "--++-", "-++--", [3177, 3175], [3182, 3179], [3181, 2390], [3176, 3180, 3184, 3523, 1527],
                [3185, 3170, 2392], [3183, 3178, 1521]),
            new("hound_road", "Hound's Rest culverts and kennel passages", (63, 79, 20, 7), (Direction.Right, Direction.Up),
                "-+---", "++--+", [293, 3434], [2031, 245], [3433, 86], [3372, 3374, 242, 3538, 80],
                [3435, 243, 3539], [247, 285, 74]),
            new("unmake_the_seal", "Ebon Crown seal vault and reliquary approach", (85, 70, 22, 9), (Direction.Left, Direction.Up),
                "+++-+", "--++-", [3370, 3373], [3379, 2398], [3369, 3182], [3376, 3378, 1528, 3524, 3374],
                [3372, 3377, 2396], [2389, 2393, 1523]),
            new("crown_without_sun", "Ebon Crown citadel and baron hall", (92, 83, 24, 10), (Direction.Right, Direction.Down),
                "+-++-", "+--++", [3379, 1531], [3369, 1528], [3371, 2390], [3373, 3377, 3522, 1524, 2397],
                [3368, 3370, 1520], [3375, 1521, 3527]),
            new("crown_without_sun_alternate_route", "Ebon Crown sallyport and gatehouse controls", (66, 81, 18, 8), (Direction.Left, Direction.Down),
                "-++-+", "-+---", [1532, 3524], [1525, 1529], [2398, 2391], [3378, 2396, 1527, 3526, 2039],
                [1523, 3523, 3376], [1522, 2389, 2395])
        ];

        private static readonly Dictionary<string, CampaignRouteLayout> CampaignRouteLayouts =
            CampaignRouteLayoutList.ToDictionary(route => route.Key, StringComparer.Ordinal);

        private static bool TryGetCampaignRouteLayout(DungeonGenerationPlan plan, out CampaignRouteLayout route, out int floor)
        {
            route = null;
            return TryGetCampaignDeck(plan, out var deck, out floor) && CampaignRouteLayouts.TryGetValue(deck.Key, out route);
        }
    }
}

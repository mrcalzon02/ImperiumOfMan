using System;
using System.Collections.Generic;
using System.Linq;
using MGSC;
using UnityEngine;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private const int RoomDressingMaxDonors = 48;
        private const int RoomDressingMaxProps = 128;
        private const int RoomDressingMaxSamples = 256;
        private const int RoomDressingMaxRooms = 48;
        private const int RoomDressingMaxPlaced = 48;
        private const int RoomDressingMaxProbes = 4096;
        private const int RoomDressingMaxConnectivityChecks = 96;

        // Keep the actual native sprite, prefab, authored pixel offset and
        // original wall-facing context. No substitute sprite or guessed GUID.
        private sealed class NativeRoomDetail(string id, string name, ImperialRoomTheme theme,
            string role, CellPosition offset, CellPosition[] footprint, CellPosition wallSide)
        {
            internal readonly string Id = id;
            internal readonly string Name = name;
            internal readonly ImperialRoomTheme Theme = theme;
            internal readonly string Role = role;
            internal readonly CellPosition Offset = offset;
            internal readonly CellPosition[] Footprint = footprint;
            internal readonly CellPosition WallSide = wallSide;
        }

        // Exact native footprint calculation, also used for objects that
        // already exist on the stamped map. Unavailable samples fail closed.
        private sealed class NativeRoomDetailSampler : IDisposable
        {
            private readonly MapObstacleFactory _factory = SingletonMonoBehaviour<MapObstacleFactory>.Instance;
            private readonly GameObject _holder = new("Imperial room detail footprint samples");
            private readonly Dictionary<string, MapObstacle> _samples = new(StringComparer.Ordinal);
            private readonly Dictionary<(string Id, CellPosition Offset), CellPosition[]> _footprints = new();
            internal NativeRoomDetailSampler() => _holder.SetActive(false);
            internal MapObstacle Prefab(string id) => string.IsNullOrEmpty(id) ? null : _factory.GetObstaclePrefab(id);

            internal CellPosition[] Footprint(string id, CellPosition offset)
            {
                var key = (id, offset);
                if (_footprints.TryGetValue(key, out var cached)) return cached;
                if (_footprints.Count >= RoomDressingMaxSamples || string.IsNullOrEmpty(id)) return null;
                CellPosition[] result = null;
                try
                {
                    if (!_samples.TryGetValue(id, out var sample))
                    {
                        var prefab = Prefab(id);
                        if (prefab != null && prefab.Renderer != null)
                            sample = UnityEngine.Object.Instantiate(prefab, _holder.transform, false);
                        _samples.Add(id, sample);
                    }
                    if (sample != null)
                    {
                        sample.InitializeForGameEditor(CellPosition.Zero, offset);
                        result = sample.OccupiedCells.Append(CellPosition.Zero).Distinct().ToArray();
                    }
                }
                catch (Exception)
                {
                    // A changed native prefab is not a safe placement sample.
                }
                _footprints.Add(key, result);
                return result;
            }

            public void Dispose() => UnityEngine.Object.Destroy(_holder);
        }

        // Positive semantic allowlist: only ordinary furniture/decor from
        // installed Core room presets. Exclude active devices and all stores,
        // including normal loot containers, to avoid modifying loot economy.
        private static bool TryNativeRoomDetailProfile(MapObstacle prefab, out ImperialRoomTheme theme,
            out string role)
        {
            theme = ImperialRoomTheme.General;
            role = null;
            var sprite = prefab?.Renderer?.sprite;
            if (sprite == null || sprite.rect.width > ImperialTilePixels * 3 ||
                sprite.rect.height > ImperialTilePixels * 3 || prefab.Interactable ||
                prefab.gameObject.GetComponent<Store>() != null ||
                prefab.gameObject.GetComponent<Door>() != null ||
                prefab.gameObject.GetComponent<Window>() != null ||
                prefab.gameObject.GetComponent<InterfloorLadder>() != null ||
                prefab.gameObject.GetComponent<InteractElevator>() != null ||
                prefab.gameObject.GetComponent<Shuttle>() != null ||
                prefab.gameObject.GetComponent<ObjectiveObstacle>() != null ||
                prefab.gameObject.GetComponent<QuestInteractiveStorage>() != null ||
                prefab.gameObject.GetComponent<RemoveAfterInit>() != null) return false;

            string name = new string((prefab.name ?? "").ToLowerInvariant().Where(char.IsLetterOrDigit).ToArray());
            bool Has(params string[] words) => words.Any(word => name.Contains(word));
            if (Has("quest", "objective", "turret", "mine", "explosive", "bomb", "trap",
                "door", "window", "ladder", "elevator", "shuttle", "portal", "corpse",
                "quasi", "demon", "warp", "corrupt", "trigger", "spawn", "monster", "camera"))
                return false;

            if (Has("medical", "medbed", "hospitalbed", "surgerytable", "surgicaltable"))
                (theme, role) = (ImperialRoomTheme.Medical, "medical fixture");
            else if (Has("labtable", "microscope", "sciencebigcontainer", "cloningtank", "laboratory"))
                (theme, role) = (ImperialRoomTheme.Laboratory, "laboratory fixture");
            else if (Has("church", "templestatue", "altar", "idol", "reliquary", "statue", "shrine"))
                (theme, role) = (ImperialRoomTheme.Ceremonial, "ceremonial fixture");
            else if (Has("bookshelf", "bookcase", "archive", "library", "bookrack"))
                (theme, role) = (ImperialRoomTheme.Archive, "archive shelving");
            else if (Has("weaponrack", "armory", "securitypanel", "securitydesk"))
                (theme, role) = (ImperialRoomTheme.Security, "security fixture");
            else if (Has("server", "telegraph", "computer", "controlpanel", "monitor", "console"))
                (theme, role) = (ImperialRoomTheme.Relay, "technical fixture");
            else if (Has("indusfurnace", "induspress", "fabricator", "workbench", "toolrack", "conveyor"))
                (theme, role) = (ImperialRoomTheme.Workshop, "workshop fixture");
            else if (Has("generator", "transformer", "pump", "ventilation", "watertank", "pipe"))
                (theme, role) = (ImperialRoomTheme.Utility, "utility fixture");
            else if (Has("bigcontainer", "containerwide", "pallet", "emptybarrel", "indusbox", "woddenbox", "woodenbox"))
                (theme, role) = (ImperialRoomTheme.Cargo, "cargo fixture");
            else if (Has("bunk", "bed", "sofa"))
                (theme, role) = (ImperialRoomTheme.Quarters, "crew furniture");
            else if (Has("chair", "bench", "seat"))
                role = "seating";
            else if (Has("table", "desk"))
                role = "work surface";
            else if (Has("flowerscontainer", "flowerpot", "planter"))
                role = "plant";
            else if (Has("trashcan", "wastebin", "garbagebin"))
                role = "waste bin";
            else if (Has("shelf", "cabinet", "locker", "rack"))
                role = "furniture";
            return role != null;
        }

        private static bool NativeDetailMatches(ImperialRoomTheme room, NativeRoomDetail detail)
        {
            if (room == detail.Theme) return true;
            if (room is ImperialRoomTheme.Laboratory or ImperialRoomTheme.Survey &&
                detail.Theme == ImperialRoomTheme.Relay) return true;
            if (room == ImperialRoomTheme.Survey && detail.Theme == ImperialRoomTheme.Laboratory) return true;
            if (room == ImperialRoomTheme.Security && detail.Theme == ImperialRoomTheme.Relay) return true;
            if (room == ImperialRoomTheme.Workshop && detail.Theme == ImperialRoomTheme.Utility) return true;
            if (room == ImperialRoomTheme.Utility && detail.Theme == ImperialRoomTheme.Workshop) return true;
            if (detail.Theme != ImperialRoomTheme.General) return false;
            // Neutral furnishings are secondary, never a reason to turn
            // a cargo or forge room into a lounge.
            return detail.Role switch
            {
                "plant" => room is ImperialRoomTheme.General or ImperialRoomTheme.Quarters or
                    ImperialRoomTheme.Medical or ImperialRoomTheme.Archive,
                "seating" => room is ImperialRoomTheme.General or ImperialRoomTheme.Quarters or
                    ImperialRoomTheme.Archive or ImperialRoomTheme.Relay or ImperialRoomTheme.Security,
                "work surface" or "furniture" => room != ImperialRoomTheme.Ceremonial &&
                    room != ImperialRoomTheme.Cargo,
                "waste bin" => room != ImperialRoomTheme.Ceremonial,
                _ => false
            };
        }

        private static bool NativeDetailFloor(MapGrid grid, CellPosition pos) =>
            SpineCellWithinBounds(pos, grid.MaxWidth, grid.MaxHeight) &&
            grid.GetCell(pos) is MapCell cell && cell.Type == MapCellType.Floor && cell.LiquidLevel == 0;

        private static bool NativeDetailTouchesWall(MapGrid grid, IEnumerable<CellPosition> footprint, CellPosition side) =>
            footprint.Any(pos => SpineCellWithinBounds(pos + side, grid.MaxWidth, grid.MaxHeight) &&
                grid.GetCell(pos + side)?.IsWallSurfaceType == true);

        private static List<NativeRoomDetail> CollectNativeRoomDetails(ImperialSpinePlan layout,
            IEnumerable<PresetHeader> donors, NativeRoomDetailSampler sampler, System.Random random)
        {
            var result = new List<NativeRoomDetail>();
            var seen = new HashSet<(string Id, CellPosition Offset, CellPosition Side)>();
            var headers = layout.Rooms.Select(entry => entry.Room.header).Concat(donors)
                .Where(header => CanBuildSpineRoom(header) && header.presetNamespace?.Kind == PresetNamespaceKind.Core &&
                    !IsImperialSupplyClosetPreset(header.binaryId))
                .GroupBy(header => header.binaryId).Select(group => group.First())
                .OrderBy(_ => random.Next()).Take(RoomDressingMaxDonors);
            foreach (var header in headers)
            {
                foreach (var obj in header.mapMetadata.MapObstacles.OrderBy(_ => random.Next()))
                {
                    // Core donor references use registered native GUIDs. A
                    // custom mod object ID must never be copied by this pass.
                    if (!Guid.TryParse(obj.ObstacleName, out _) ||
                        !TryNativeRoomDetailProfile(sampler.Prefab(obj.ObstacleName), out var theme, out string role))
                        continue;
                    var footprint = sampler.Footprint(obj.ObstacleName, obj.Offset);
                    if (footprint == null || footprint.Length < 1 || footprint.Length > 4) continue;
                    var sourceCells = footprint.Select(cell => obj.Pos + cell).ToArray();
                    if (!sourceCells.All(pos => NativeDetailFloor(header.MapGrid, pos))) continue;
                    foreach (var side in ImperialSpineCardinals)
                    {
                        if (!NativeDetailTouchesWall(header.MapGrid, sourceCells, side) ||
                            !seen.Add((obj.ObstacleName, obj.Offset, side))) continue;
                        var prefab = sampler.Prefab(obj.ObstacleName);
                        result.Add(new NativeRoomDetail(obj.ObstacleName, prefab.name, theme, role,
                            obj.Offset, footprint, side));
                        if (result.Count >= RoomDressingMaxProps) return result;
                    }
                }
            }
            return result;
        }

        private static void DressImperialNativeRooms(MapGrid grid, MapMetadata metadata,
            ImperialSpinePlan layout, IEnumerable<PresetHeader> donors, int seed)
        {
            int placed = 0, probes = 0, connectivityChecks = 0, dressedRooms = 0;
            var placements = new Dictionary<string, int>(StringComparer.Ordinal);
            var roomThemes = new Dictionary<ImperialRoomTheme, int>();
            try
            {
                using var sampler = new NativeRoomDetailSampler();
                var random = new System.Random(unchecked(seed ^ 0x524F4F50));
                var details = CollectNativeRoomDetails(layout, donors, sampler, random);
                if (details.Count == 0)
                {
                    Debug.Log("[Cvar_ImperiumOfMan] Native room details: no safe authored donor fixtures.");
                    return;
                }

                var catalog = BuildImperialRoomThemeCatalog();
                var fixtureThemes = new Dictionary<string, ImperialRoomTheme>(StringComparer.Ordinal);
                var existing = metadata.MapObstacles.ToArray();
                var protectedCells = metadata.MapLights.Select(obj => obj.Pos)
                    .Concat(metadata.MapTriggers.Select(obj => obj.Pos))
                    .Concat(metadata.LevelEntries.Select(entry => entry.Position))
                    .Concat(metadata.LevelExits.Select(entry => entry.Position))
                    .Concat(metadata.Items.Select(entry => entry.Position))
                    .Concat(metadata.Monsters.Select(entry => entry.Position)).ToHashSet();
                foreach (var door in layout.Doors)
                {
                    protectedCells.UnionWith(door.Opening);
                    var entrance = SpineDoorInterior(door);
                    for (int dx = -2; dx <= 2; dx++)
                    for (int dy = -2; dy <= 2; dy++)
                        protectedCells.Add(entrance + new CellPosition(dx, dy));
                }

                foreach (var entry in layout.Rooms.Where(entry => entry.Group is -1 or 2 or 15)
                    .Where(entry => !IsImperialSupplyClosetPreset(entry.Room.header.binaryId))
                    .OrderBy(_ => random.Next()).Take(RoomDressingMaxRooms))
                {
                    if (placed >= RoomDressingMaxPlaced || probes >= RoomDressingMaxProbes ||
                        connectivityChecks >= RoomDressingMaxConnectivityChecks) break;
                    var room = entry.Room;
                    var header = room.header;
                    // Native required or scripted rooms with special tables
                    // are not changed by the optional decor pass.
                    if (header.genMetadata.SpawnUnique || header.mapMetadata.MapTriggers.Any() ||
                        header.mapMetadata.LevelEntries.Count != 0 || header.mapMetadata.LevelExits.Count != 0 ||
                        (header.genMetadata.SpecialItemsTableIds?.Count ?? 0) != 0 ||
                        (header.genMetadata.SpecialMonstersTableIds?.Count ?? 0) != 0) continue;
                    int width = room.RightUpperCorner.X - room.LeftLowerCorner.X + 1;
                    int height = room.RightUpperCorner.Y - room.LeftLowerCorner.Y + 1;
                    if (width < 5 || height < 5 || width * height > 900) continue;
                    var theme = ClassifyImperialDonorTheme(header, catalog, fixtureThemes);
                    var matching = details.Where(detail => NativeDetailMatches(theme, detail)).ToArray();
                    if (matching.Length == 0) continue;

                    var occupied = new HashSet<CellPosition>();
                    var blocked = new HashSet<CellPosition>();
                    var roomIds = new HashSet<string>(StringComparer.Ordinal);
                    bool unknown = false;
                    foreach (var obj in existing)
                    {
                        // Sample neighboring anchors too: large native
                        // furniture can overhang an adjacent room edge.
                        if (obj.Pos.X < room.LeftLowerCorner.X - 3 || obj.Pos.X > room.RightUpperCorner.X + 3 ||
                            obj.Pos.Y < room.LeftLowerCorner.Y - 3 || obj.Pos.Y > room.RightUpperCorner.Y + 3)
                            continue;
                        var prefab = sampler.Prefab(obj.ObstacleName);
                        var footprint = sampler.Footprint(obj.ObstacleName, obj.Offset);
                        if (prefab == null || footprint == null)
                        {
                            unknown = true;
                            break;
                        }
                        bool door = prefab.gameObject.GetComponent<Door>() != null;
                        foreach (var offset in footprint)
                        {
                            var pos = obj.Pos + offset;
                            if (!room.ContainsPos(pos)) continue;
                            occupied.Add(pos);
                            if (!door) blocked.Add(pos);
                        }
                        if (room.ContainsPos(obj.Pos)) roomIds.Add(obj.ObstacleName);
                    }
                    if (unknown) continue;

                    var floor = new HashSet<CellPosition>();
                    var walkable = new HashSet<CellPosition>();
                    for (int y = room.LeftLowerCorner.Y; y <= room.RightUpperCorner.Y; y++)
                    for (int x = room.LeftLowerCorner.X; x <= room.RightUpperCorner.X; x++)
                    {
                        var pos = new CellPosition(x, y);
                        var cell = grid.GetCell(pos);
                        if (NativeDetailFloor(grid, pos)) floor.Add(pos);
                        if (cell != null && cell.LiquidLevel == 0 &&
                            IsSpineRoomFloor(cell) && !blocked.Contains(pos) &&
                            (!cell.IsObjBlockPass || cell.IsDoor)) walkable.Add(pos);
                    }
                    var entrances = layout.Doors.Where(door => ReferenceEquals(door.Room, room))
                        .Select(SpineDoorInterior).Distinct().ToArray();
                    if (entrances.Length == 0 || entrances.Any(pos => !walkable.Contains(pos)) ||
                        floor.Count < 16 || walkable.Count < 12) continue;

                    // Do not try to repair an existing disconnected native
                    // room. Work only on an already connected free-floor graph.
                    connectivityChecks++;
                    if (WalkSpineCells(entrances[0], walkable.Contains, grid.GetFence).Count != walkable.Count)
                        continue;
                    int freeFloor = floor.Count(pos => walkable.Contains(pos) && !occupied.Contains(pos));
                    int budget = Math.Min(freeFloor / 8, Math.Max(0, floor.Count / 4 - (floor.Count - freeFloor)));
                    int roomLimit = freeFloor < 36 ? 1 : freeFloor < 70 ? 2 : 3;
                    if (budget < 1) continue;
                    var anchors = floor.Where(pos => ImperialSpineCardinals.Any(side =>
                            SpineCellWithinBounds(pos + side, grid.MaxWidth, grid.MaxHeight) &&
                            grid.GetCell(pos + side)?.IsWallSurfaceType == true))
                        .OrderByDescending(pos => entrances.Min(door => SpineManhattan(pos, door)))
                        .ThenBy(_ => random.Next()).ToArray();
                    var usedRoles = new HashSet<string>(StringComparer.Ordinal);
                    int roomPlaced = 0;
                    while (roomPlaced < roomLimit && budget > 0 && placed < RoomDressingMaxPlaced &&
                        probes < RoomDressingMaxProbes && connectivityChecks < RoomDressingMaxConnectivityChecks)
                    {
                        bool added = false;
                        foreach (var detail in matching.Where(detail => !roomIds.Contains(detail.Id))
                            .OrderBy(detail => detail.Theme != theme)
                            .ThenBy(detail => usedRoles.Contains(detail.Role))
                            .ThenBy(detail => placements.TryGetValue(detail.Id, out int count) ? count : 0)
                            .ThenBy(_ => random.Next()))
                        {
                            if (detail.Footprint.Length > budget) continue;
                            foreach (var anchor in anchors)
                            {
                                if (probes >= RoomDressingMaxProbes ||
                                    connectivityChecks >= RoomDressingMaxConnectivityChecks) break;
                                probes++;
                                var footprint = detail.Footprint.Select(pos => anchor + pos).ToHashSet();
                                if (footprint.Any(pos => !floor.Contains(pos) || !walkable.Contains(pos) ||
                                    occupied.Contains(pos) || protectedCells.Contains(pos) ||
                                    layout.CorridorCells.Contains(pos) ||
                                    grid.GetCell(pos).specialFlag != MapCellSpecialFlag.None ||
                                    grid.GetCell(pos).GetTilesetId(2) != 0 && grid.GetCell(pos).GetTileIndex(2) != byte.MaxValue ||
                                    ImperialSpineCardinals.Any(step => occupied.Contains(pos + step))) ||
                                    !NativeDetailTouchesWall(grid, footprint, detail.WallSide)) continue;

                                // Same wall-facing orientation as its native
                                // placement, with a free front/service side.
                                if (!footprint.Any(pos =>
                                {
                                    var front = pos - detail.WallSide;
                                    return !footprint.Contains(front) && floor.Contains(front) &&
                                        walkable.Contains(front) && !grid.GetFence(pos, front);
                                })) continue;

                                // Treat the full sprite footprint as solid,
                                // even if the native decorative object is
                                // actually passable. Never cut a door or aisle.
                                connectivityChecks++;
                                var remaining = WalkSpineCells(entrances[0],
                                    pos => walkable.Contains(pos) && !footprint.Contains(pos), grid.GetFence);
                                if (remaining.Count != walkable.Count - footprint.Count ||
                                    entrances.Any(pos => !remaining.Contains(pos))) continue;

                                metadata.AddObject(new MapObjectMetadata
                                {
                                    UniqueId = metadata.GetNextObjIdAndIncreaseCounter(),
                                    ObstacleName = detail.Id,
                                    Pos = anchor,
                                    Offset = detail.Offset
                                });
                                occupied.UnionWith(footprint);
                                walkable.ExceptWith(footprint);
                                budget -= footprint.Count;
                                roomIds.Add(detail.Id);
                                usedRoles.Add(detail.Role);
                                placements.TryGetValue(detail.Id, out int uses);
                                placements[detail.Id] = uses + 1;
                                placed++;
                                roomPlaced++;
                                added = true;
                                break;
                            }
                            if (added || probes >= RoomDressingMaxProbes ||
                                connectivityChecks >= RoomDressingMaxConnectivityChecks) break;
                        }
                        if (!added) break;
                    }
                    if (roomPlaced > 0)
                    {
                        dressedRooms++;
                        roomThemes.TryGetValue(theme, out int count);
                        roomThemes[theme] = count + 1;
                    }
                }
                Debug.Log("[Cvar_ImperiumOfMan] Native room details | catalog=" + details.Select(detail => detail.Id).Distinct().Count() +
                    " | rooms=" + dressedRooms + " | objects=" + placed +
                    " | probes=" + probes + " | connectivity checks=" + connectivityChecks +
                    " | themes=" + string.Join(",", roomThemes.Select(pair => pair.Key + ":" + pair.Value)) +
                    " | prefabs=" + string.Join(",", placements.Select(pair =>
                        details.First(detail => detail.Id == pair.Key).Name + ":" + pair.Value)));
            }
            catch (Exception error)
            {
                // This is a visual-only enhancement. Every previously
                // committed metadata placement already passed the checks.
                Debug.LogWarning("[Cvar_ImperiumOfMan] Optional native room detail stopped after " +
                    placed + " safe placements: " + error);
            }
        }
    }
}

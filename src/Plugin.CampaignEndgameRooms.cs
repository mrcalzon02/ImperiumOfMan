using System;
using System.Collections.Generic;
using System.Linq;
using MGSC;
using UnityEngine;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private static HashSet<CellPosition> FixedCampaignFixtureCells(PresetHeader source)
        {
            var generation = _imperialRoomGeneration;
            if (generation?.ProtectCampaignLandings != true) return null;
            if (generation.FixedFixtureCells.TryGetValue(source, out var cached)) return cached;

            var blocked = new HashSet<CellPosition>();
            foreach (var fixture in source.mapMetadata.MapObstacles.Concat(source.mapMetadata.MapLights))
            {
                if (string.IsNullOrEmpty(fixture.ObstacleName)) continue;
                var key = (fixture.ObstacleName, fixture.Offset);
                if (!generation.FixtureFootprints.TryGetValue(key, out var footprint))
                {
                    var sample = GetCampaignFixtureSample(generation, fixture.ObstacleName);
                    if (sample == null) footprint = Array.Empty<CellPosition>();
                    else
                    {
                        sample.InitializeForGameEditor(CellPosition.Zero, fixture.Offset);
                        footprint = sample.OccupiedCells.ToArray();
                    }
                    generation.FixtureFootprints.Add(key, footprint);
                }
                foreach (var cell in footprint) blocked.Add(fixture.Pos + cell);
            }
            generation.FixedFixtureCells.Add(source, blocked);
            return blocked;
        }

        private static MapObstacle GetCampaignFixtureSample(ImperialRoomGeneration generation, string id)
        {
            if (generation.FixtureSamples.TryGetValue(id, out var sample)) return sample;
            var factory = SingletonMonoBehaviour<MapObstacleFactory>.Instance;
            var prefab = factory.GetObstaclePrefab(id);
            if (prefab != null && prefab.Renderer != null && prefab.gameObject.GetComponent<Door>() == null &&
                prefab.gameObject.GetComponent<NoCellOccupy>() == null && prefab.gameObject.GetComponent<RemoveAfterInit>() == null)
            {
                var health = prefab.gameObject.GetComponent<ObstacleHealth>();
                bool fixedObstacle = health == null || health.IsInvulnerable ||
                    health.KeepRemains && !health.MakeRemainsPassable || health.InitialHitChanceOnShoot == 0 ||
                    health.Armor > Data.Global.PathfindingArmorThreshold;
                if (fixedObstacle)
                {
                    if (generation.FixtureSampleHolder == null)
                    {
                        generation.FixtureSampleHolder = new GameObject("Imperial native doorway fixture samples");
                        generation.FixtureSampleHolder.SetActive(false);
                    }
                    sample = UnityEngine.Object.Instantiate(prefab, generation.FixtureSampleHolder.transform, false);
                }
            }
            generation.FixtureSamples.Add(id, sample);
            return sample;
        }

        private static PresetHeader GetCampaignEndgameRoomVariant(DungeonGenerator generator, PresetHeader source,
            int canvasWidth, int canvasHeight, int seed, int tier)
        {
            var generation = _imperialRoomGeneration;
            if (generation == null || !IsImperialSpineDonor(source)) return source;

            if (generation.OriginalRooms.TryGetValue(source, out var original)) source = original;
            var standard = GetImperialSpineRoomVariant(generator, source, canvasWidth, canvasHeight, seed);
            if (tier <= 0) return standard;

            tier = Math.Min(tier, 2);
            var key = (source, canvasWidth, canvasHeight, (source.binaryId ^ seed) & 3, tier);
            if (generation.EndgameVariants.TryGetValue(key, out var cached)) return cached;

            try
            {
                var expanded = ExpandCampaignEndgameRoom(source, standard, canvasWidth, canvasHeight, tier);
                generation.EndgameVariants.Add(key, expanded);
                if (expanded != standard && expanded != source) generation.OriginalRooms.Add(expanded, source);
                return expanded;
            }
            finally
            {
                generator.FinalMapGrid.RefreshFenceCheckRef();
            }
        }

        private static PresetHeader ExpandCampaignEndgameRoom(PresetHeader source, PresetHeader standard,
            int canvasWidth, int canvasHeight, int tier)
        {
            var nativeFloor = LargestEndgameRoomFloor(source);
            if (nativeFloor.Count < 12) return standard;

            var x = FindImperialRoomSeam(source, 1, horizontal: true, endgameFloor: nativeFloor);
            var y = FindImperialRoomSeam(source, 1, horizontal: false, endgameFloor: nativeFloor);
            if (x.Extra == 0 && y.Extra == 0) return standard;

            int width = source.MapGrid.MaxWidth;
            int height = source.MapGrid.MaxHeight;
            int standardWidth = standard.MapGrid.MaxWidth - width;
            int standardHeight = standard.MapGrid.MaxHeight - height;
            int maximumWidth = x.Extra == 0 ? standardWidth :
                Math.Max(standardWidth, CampaignEndgameSeamLimit(width, standardWidth, canvasWidth, tier));
            int maximumHeight = y.Extra == 0 ? standardHeight :
                Math.Max(standardHeight, CampaignEndgameSeamLimit(height, standardHeight, canvasHeight, tier));
            if (x.Extra == 0 && standardWidth > 0) x = FindImperialRoomSeam(source, standardWidth, horizontal: true);
            if (y.Extra == 0 && standardHeight > 0) y = FindImperialRoomSeam(source, standardHeight, horizontal: false);

            int floorRows = x.Extra == 0 ? 0 : nativeFloor.Count(pos => pos.X == x.SourceLine);
            int floorColumns = y.Extra == 0 ? 0 : nativeFloor.Count(pos => pos.Y == y.SourceLine);
            bool floorCrossing = x.Extra != 0 && y.Extra != 0 && nativeFloor.Contains(new CellPosition(x.SourceLine, y.SourceLine));
            int baseline = source == standard ? nativeFloor.Count : LargestEndgameRoomFloor(standard).Count;
            int target = (int)Math.Ceiling(baseline * (tier == 2 ? 1.95 : 1.55));
            int bestWidth = 0, bestHeight = 0, bestScore = int.MaxValue;
            for (int extraWidth = standardWidth; extraWidth <= maximumWidth; extraWidth++)
            for (int extraHeight = standardHeight; extraHeight <= maximumHeight; extraHeight++)
            {
                int floor = nativeFloor.Count + extraWidth * floorRows + extraHeight * floorColumns;
                if (floorCrossing) floor += extraWidth * extraHeight;
                if (floor < baseline + Math.Max(4, baseline / 12)) continue;

                // Favor the requested area and avoid needlessly stretching a room in only one direction.
                int imbalance = Math.Abs((extraWidth - standardWidth) * standard.MapGrid.MaxHeight -
                    (extraHeight - standardHeight) * standard.MapGrid.MaxWidth);
                int score = Math.Abs(target - floor) * 8 + imbalance / 2;
                if (score >= bestScore) continue;
                bestWidth = extraWidth;
                bestHeight = extraHeight;
                bestScore = score;
            }

            return bestScore == int.MaxValue ? standard : BuildImperialRoomVariant(source,
                new ImperialRoomSeam(x.SourceLine, bestWidth), new ImperialRoomSeam(y.SourceLine, bestHeight));
        }

        private static int CampaignEndgameSeamLimit(int dimension, int standard, int canvasDimension, int tier) =>
            Math.Min(Math.Min(tier == 2 ? 16 : 10, dimension * 2 / 3 + standard + 2), canvasDimension / 5 - dimension);

        private static HashSet<CellPosition> LargestEndgameRoomFloor(PresetHeader source)
        {
            var grid = source.MapGrid;
            var visited = new HashSet<CellPosition>();
            var largest = new HashSet<CellPosition>();
            foreach (var direction in new[] { Direction.Up, Direction.Down, Direction.Left, Direction.Right })
            foreach (var joint in SpineJoints(source, direction))
            {
                var entry = FirstSpineInterior(joint, direction);
                if (!visited.Add(entry)) continue;
                var walkable = new HashSet<CellPosition>();
                var pending = new Queue<CellPosition>();
                pending.Enqueue(entry);
                while (pending.Count != 0)
                {
                    var current = pending.Dequeue();
                    if (grid.GetCell(current).Type == MapCellType.Floor) walkable.Add(current);
                    foreach (var step in ImperialSpineCardinals)
                    {
                        var next = current + step;
                        if (grid.GetCell(next)?.Type is not (MapCellType.Floor or MapCellType.ClosedDoor or MapCellType.OpenedDoor) ||
                            grid.GetFence(current, next) || !visited.Add(next)) continue;
                        pending.Enqueue(next);
                    }
                }
                if (walkable.Count > largest.Count) largest = walkable;
            }
            return largest;
        }
    }
}

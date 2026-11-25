using System;
using System.Collections.Generic;
using System.Linq;
using MGSC;
using UnityEngine;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private readonly struct CampaignExteriorView(CellPosition inside, CellPosition edge, CellPosition outside,
            CellPosition outward, bool crown)
        {
            internal readonly CellPosition Inside = inside;
            internal readonly CellPosition Edge = edge;
            internal readonly CellPosition Outside = outside;
            internal readonly CellPosition Outward = outward;
            internal readonly bool Crown = crown;
        }

        private static void DressCampaignEndgameViews(MapGrid grid, MapMetadata metadata, ImperialSpinePlan layout,
            DungeonGenerationPlan plan, bool warp, int seed)
        {
            if (!TryGetCampaignDeck(plan, out var deck, out _) ||
                plan.GetRule(PresetRuleType.FillBordersAsteroid) != null ||
                plan.GetRule(PresetRuleType.FillBordersSolid) != null ||
                plan.GetRule(PresetRuleType.FillBordersSpace) == null && plan.GetRule(PresetRuleType.FillBordersStation) == null)
                return;

            // The surface mines and Martian works keep their sealed rock faces.
            if (deck.Key is "caloris_levy" or "cinder_basin" or "prime_the_foundry" or "one_standard") return;

            var exterior = NativeExteriorSpace(grid);
            if (exterior.Count == 0) return;

            var protectedCells = metadata.MapObstacles.Select(prop => prop.Pos)
                .Concat(metadata.MapLights.Select(prop => prop.Pos))
                .Concat(metadata.MapTriggers.Select(prop => prop.Pos))
                .Concat(metadata.LevelEntries.Select(entry => entry.Position))
                .Concat(metadata.LevelExits.Select(exit => exit.Position)).ToHashSet();
            foreach (var door in layout.Doors)
            {
                protectedCells.UnionWith(door.Opening);
                protectedCells.Add(SpineDoorInterior(door));
            }

            var views = new List<CampaignExteriorView>();
            for (int y = 2; y < grid.MaxHeight - 2; y++)
            for (int x = 2; x < grid.MaxWidth - 2; x++)
            {
                var inside = new CellPosition(x, y);
                if (grid.GetCell(inside)?.Type != MapCellType.Floor || !IsUnclutteredViewCell(inside)) continue;
                foreach (var (dx, dy) in FloorNeighbors)
                {
                    var outward = new CellPosition(dx, dy);
                    var edge = inside + outward;
                    var boundary = grid.GetCell(edge);
                    if (boundary?.IsWallSurfaceType != true) continue;
                    bool crown = boundary.Type == MapCellType.FacadeWall;
                    if (crown && (dy != 1 || grid.GetCell(edge + outward)?.Type != MapCellType.Wall)) continue;
                    var outside = edge + outward * (crown ? 2 : 1);
                    if (!exterior.Contains(outside) || !exterior.Contains(outside + outward)) continue;

                    var across = new CellPosition(Math.Abs(dy), Math.Abs(dx));
                    if (!HasViewReturnWall(across) || !HasViewReturnWall(across * -1) ||
                        !IsUnclutteredViewCell(edge) || crown && !IsUnclutteredViewCell(edge + outward))
                        continue;
                    views.Add(new CampaignExteriorView(inside, edge, outside, outward, crown));

                    bool HasViewReturnWall(CellPosition lateral)
                    {
                        var neighbor = grid.GetCell(edge + lateral);
                        return neighbor?.Type == boundary.Type && grid.GetCell(inside + lateral)?.Type == MapCellType.Floor &&
                            (!crown || grid.GetCell(edge + outward + lateral)?.Type == MapCellType.Wall);
                    }
                }
            }

            var windows = FindNativeViewports();
            var random = new System.Random(seed ^ 0x225305);
            var unselected = views.OrderBy(_ => random.Next()).ToList();
            var usedDirections = new HashSet<CellPosition>();
            var placements = new List<CampaignExteriorView>();
            var edges = new List<CellPosition>();
            int glass = 0, breaches = 0;
            while (placements.Count < 5 && unselected.Count > 0)
            {
                bool hasHallView = placements.Any(view => layout.CorridorCells.Contains(view.Inside));
                bool wantsGlass = placements.Count % (warp ? 3 : 2) == 0;
                var choice = unselected.OrderBy(view => wantsGlass && !windows.Any(window => window.Horizontal == (view.Outward.Y != 0)))
                    .ThenBy(view => usedDirections.Contains(view.Outward))
                    .ThenBy(view => layout.CorridorCells.Contains(view.Inside) == hasHallView).First();
                bool placedGlass = wantsGlass && TryAddNativeWindow(choice, windows);
                if (placedGlass) glass++;
                else
                {
                    breaches++;
                    var across = new CellPosition(Math.Abs(choice.Outward.Y), Math.Abs(choice.Outward.X));
                    foreach (var companion in unselected)
                    {
                        if (!companion.Outward.Equals(choice.Outward) || companion.Crown != choice.Crown ||
                            (!companion.Edge.Equals(choice.Edge + across) && !companion.Edge.Equals(choice.Edge - across)))
                            continue;
                        OpenViewToExterior(companion);
                        break;
                    }
                }
                OpenViewToExterior(choice);
                placements.Add(choice);
                usedDirections.Add(choice.Outward);
                unselected.RemoveAll(view => SpineManhattan(view.Edge, choice.Edge) < 12);
            }

            if (placements.Count > 0)
                Debug.Log($"[Cvar_ImperiumOfMan] Campaign exterior views {deck.Key} native windows={glass}, " +
                    $"open hull views={breaches}, edges={string.Join(",", edges)}");

            bool IsUnclutteredViewCell(CellPosition pos)
            {
                for (int dy = -1; dy <= 1; dy++)
                for (int dx = -1; dx <= 1; dx++)
                    if (protectedCells.Contains(pos + new CellPosition(dx, dy))) return false;
                return true;
            }

            void OpenViewToExterior(CampaignExteriorView view)
            {
                var backdrop = grid.GetCell(view.Outside);
                ExtendDeepBackdrop(view.Edge, backdrop);
                if (view.Crown) ExtendDeepBackdrop(view.Edge + view.Outward, backdrop);
                edges.Add(view.Edge);
            }

            void ExtendDeepBackdrop(CellPosition into, MapCell backdrop)
            {
                var cell = grid.GetCell(into);
                cell.Clone(backdrop);
                cell.X = (byte)into.X;
                cell.Y = (byte)into.Y;
            }

            bool TryAddNativeWindow(CampaignExteriorView view, (string Id, bool Horizontal)[] catalog)
            {
                bool horizontal = view.Outward.Y != 0;
                string id = catalog.FirstOrDefault(window => window.Horizontal == horizontal).Id;
                if (id == null) return false;
                metadata.AddObject(new MapObjectMetadata
                {
                    ObstacleName = id,
                    UniqueId = metadata.GetNextObjIdAndIncreaseCounter(),
                    Pos = view.Edge,
                });
                return true;
            }
        }

        private static HashSet<CellPosition> NativeExteriorSpace(MapGrid grid)
        {
            var exterior = new HashSet<CellPosition>();
            var pending = new Queue<CellPosition>();
            int width = grid.MaxWidth;
            int height = grid.MaxHeight;
            for (int x = 0; x < width; x++)
            {
                Visit(new CellPosition(x, 0));
                Visit(new CellPosition(x, height - 1));
            }
            for (int y = 1; y < height - 1; y++)
            {
                Visit(new CellPosition(0, y));
                Visit(new CellPosition(width - 1, y));
            }
            while (pending.Count > 0)
            {
                var pos = pending.Dequeue();
                foreach (var (x, y) in FloorNeighbors) Visit(pos + new CellPosition(x, y));
            }
            return exterior;

            void Visit(CellPosition pos)
            {
                if (grid.GetCell(pos)?.Type == MapCellType.Deep && exterior.Add(pos)) pending.Enqueue(pos);
            }
        }

        private static (string Id, bool Horizontal)[] FindNativeViewports()
        {
            var descriptors = CustomResources.Load<DescriptorsCollection>("DescriptorsCollections/mapobjects_descriptors");
            var windows = new List<(string Id, bool Horizontal, float Area)>();
            foreach (var prop in descriptors.Descriptors.OfType<MapObstacle>())
            {
                var sprite = prop.Renderer?.sprite;
                if (!prop.name.EndsWith("_window_single", StringComparison.OrdinalIgnoreCase) ||
                    sprite == null || prop.gameObject.GetComponent<Window>() == null ||
                    !descriptors.TryGetId(prop, out string id) || sprite.rect.width > ImperialTilePixels + 4 ||
                    sprite.rect.height > ImperialTilePixels * 2)
                    continue;
                var rect = sprite.rect;
                bool horizontal = prop.name.StartsWith("hor_", StringComparison.OrdinalIgnoreCase);
                windows.Add((id, horizontal, rect.width * rect.height));
            }
            return windows.OrderBy(window => window.Area).ThenBy(window => window.Id, StringComparer.Ordinal)
                .Select(window => (window.Id, window.Horizontal)).ToArray();
        }
    }
}

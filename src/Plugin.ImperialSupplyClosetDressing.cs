using System;
using System.Collections.Generic;
using System.Linq;
using MGSC;
using UnityEngine;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private const string SupplyMopBucketId = "iom_supply_mop_bucket";
        private const string WarpSupplyMopBucketId = "iom_warp_supply_mop_bucket";

        private static readonly (string Use, string Id, Direction Facing)[] SupplyClosetStorage =
        [
            ("box", "81d3759e-5b2e-45a7-9de5-d591a983a339", Direction.Down),
            ("box", "ab9f85de-8926-4f6c-ba95-f74c296165e7", Direction.Down),
            ("box", "2fb21900-c31b-434f-9090-0d0cee06eb24", Direction.Down),
            ("wooden box", "f7f9dff0-f908-46b1-91e0-d06411b0d206", Direction.Down),
            ("wooden box", "930b4b81-d37d-45b7-b92d-dd83bd356e54", Direction.Down),
        ];

        private static void RegisterImperialSupplyClosetProps()
        {
            var descriptors = CustomResources.Load<DescriptorsCollection>("DescriptorsCollections/mapobjects_descriptors");
            var donor = descriptors.Descriptors.OfType<MapObstacle>().First(prop => prop.Renderer != null &&
                prop.gameObject.GetComponent<Store>() != null);
            var root = new GameObject("Imperial supply closet props");
            root.SetActive(false);
            UnityEngine.Object.DontDestroyOnLoad(root);

            foreach (bool warp in new[] { false, true })
            {
                string id = warp ? WarpSupplyMopBucketId : SupplyMopBucketId;
                if (descriptors.TryGetDescriptor(id, out _)) continue;
                var prefab = new GameObject(id);
                prefab.transform.SetParent(root.transform, false);
                prefab.layer = donor.gameObject.layer;

                var art = new GameObject("Sprite");
                art.transform.SetParent(prefab.transform, false);
                art.layer = donor.Renderer.gameObject.layer;
                var renderer = art.AddComponent<SpriteRenderer>();
                renderer.sharedMaterial = donor.Renderer.sharedMaterial;
                renderer.sortingLayerID = donor.Renderer.sortingLayerID;
                renderer.sortingOrder = donor.Renderer.sortingOrder;
                renderer.sprite = DrawImperialMopBucket(id, warp);

                var obstacle = prefab.AddComponent<MapObstacle>();
                obstacle.spRenderer = renderer;
                obstacle.spriteOutline = art.AddComponent<SpriteOutline>();
                obstacle.boxCollider = prefab.AddComponent<BoxCollider2D>();
                obstacle.boxCollider.enabled = false;
                obstacle.SetRenderMode(MapObstacle.RenderMode.Flat);
                prefab.AddComponent<NoCellOccupy>();
                prefab.AddComponent<ForceCellOccupy>();
                descriptors.AddDescriptor(id, obstacle);
            }
        }

        private static Sprite DrawImperialMopBucket(string id, bool warp)
        {
            int size = ImperialTilePixels;
            var pixels = new Color32[size * size];
            var shadow = new Color32(20, 22, 20, 154);
            var edge = new Color32(33, 35, 30, 255);
            var shell = warp ? new Color32(107, 62, 43, 255) : new Color32(77, 116, 103, 255);
            var lip = warp ? new Color32(153, 104, 73, 255) : new Color32(124, 153, 131, 255);
            var liquid = warp ? new Color32(72, 45, 33, 255) : new Color32(48, 79, 75, 255);
            var grip = new Color32(153, 126, 75, 255);
            var handle = warp ? new Color32(156, 107, 63, 255) : new Color32(190, 163, 99, 255);
            var linen = warp ? new Color32(151, 116, 88, 255) : new Color32(177, 169, 134, 255);
            var seam = new Color32(105, 102, 82, 255);

            for (int x = 14; x <= 25; x++) Pixel(x, 2, shadow);
            for (int y = 3; y <= 11; y++)
            {
                int left = y <= 5 ? 15 : 14;
                int right = y <= 5 ? 23 : 24;
                for (int x = left; x <= right; x++)
                    Pixel(x, y, x == left || x == right || y == 3 ? edge : x == left + 1 ? lip : shell);
            }
            for (int y = 9; y <= 14; y++)
            for (int x = 13; x <= 25; x++)
            {
                float radius = (x - 19) * (x - 19) / 36f + (y - 11) * (y - 11) / 9f;
                if (radius <= 1f) Pixel(x, y, radius >= .55f ? lip : liquid);
            }
            Pixel(15, 13, lip);
            Pixel(23, 13, lip);
            Pixel(20, 11, shell);
            Stroke(16, 14, 19, 17, edge);
            Stroke(19, 17, 23, 14, edge);

            Stroke(6, 25, 12, 8, edge);
            Stroke(7, 25, 13, 8, grip);
            Stroke(8, 24, 14, 8, handle);
            Stroke(13, 8, 8, 2, linen);
            Stroke(13, 8, 10, 1, seam);
            Stroke(13, 8, 12, 2, linen);
            Stroke(14, 8, 14, 1, seam);
            Stroke(14, 8, 16, 2, linen);
            Stroke(12, 7, 15, 6, grip);

            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = id,
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
            };
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            var sprite = Sprite.Create(texture, new Rect(0, 0, size, size), Vector2.zero, 100f, 0, SpriteMeshType.FullRect);
            sprite.name = id;
            return sprite;

            void Pixel(int x, int y, Color32 color) => pixels[y * size + x] = color;

            void Stroke(int x, int y, int endX, int endY, Color32 color)
            {
                int steps = Math.Max(Math.Abs(endX - x), Math.Abs(endY - y));
                for (int i = 0; i <= steps; i++)
                    Pixel(Mathf.RoundToInt(Mathf.Lerp(x, endX, (float)i / steps)),
                        Mathf.RoundToInt(Mathf.Lerp(y, endY, (float)i / steps)), color);
            }
        }

        private static void DressImperialSupplyClosets(MapGrid grid, MapMetadata metadata,
            ImperialSpinePlan layout, bool warp, int seed)
        {
            var doors = layout.Doors.Where(door => IsImperialSupplyClosetPreset(door.Room.header.binaryId)).ToArray();
            if (doors.Length == 0) return;
            var boxFootprints = LoadSpinePropFootprints(SupplyClosetStorage).OrderBy(prop => prop.Cells.Length).ToArray();
            var supplies = new[] { "paper_towel", "rags" }
                .Where(id => Data.Items.GetRecord(id) != null).ToArray();
            var extraSupplies = new[] { "tool_container", "water_container" }
                .Where(id => Data.Items.GetRecord(id) != null).ToArray();
            var random = new System.Random(seed ^ 0x3485ce);
            var boxUse = new Dictionary<string, int>();
            string mop = warp ? WarpSupplyMopBucketId : SupplyMopBucketId;
            var factory = SingletonMonoBehaviour<MapObstacleFactory>.Instance;
            bool canPlaceMop = factory.GetObstaclePrefab(mop) != null;
            int furnished = 0, boxCount = 0, mopCount = 0, supplyCount = 0;
            foreach (var door in doors)
            {
                var room = door.Room;
                var entrance = SpineDoorInterior(door);
                var floor = new HashSet<CellPosition>();
                for (int y = room.LeftLowerCorner.Y; y <= room.RightUpperCorner.Y; y++)
                for (int x = room.LeftLowerCorner.X; x <= room.RightUpperCorner.X; x++)
                {
                    var pos = new CellPosition(x, y);
                    if (grid.GetCell(pos)?.Type == MapCellType.Floor && !door.Opening.Contains(pos)) floor.Add(pos);
                }
                if (!floor.Contains(entrance)) continue;
                var spots = floor.Where(pos => !pos.Equals(entrance)).OrderByDescending(pos => SpineManhattan(pos, entrance))
                    .ThenBy(_ => random.Next()).ToArray();
                var placed = new HashSet<CellPosition>();
                if (TryPlaceBox()) boxCount++;
                if (canPlaceMop && TryGetFreeSpot(out var mopPosition))
                {
                    metadata.AddObject(new MapObjectMetadata
                    {
                        UniqueId = metadata.GetNextObjIdAndIncreaseCounter(), ObstacleName = mop, Pos = mopPosition,
                    });
                    placed.Add(mopPosition);
                    mopCount++;
                }
                if (supplies.Length > 0 && TryGetFreeSpot(out var supplyPosition))
                {
                    AddCleaningPickup(supplyPosition, supplies);
                    if (floor.Count >= 8 && extraSupplies.Length > 0 && TryGetFreeSpot(out var toolPosition))
                        AddCleaningPickup(toolPosition, extraSupplies);
                }
                if (placed.Count > 0) furnished++;

                void AddCleaningPickup(CellPosition position, string[] choices)
                {
                    metadata.AddItem(choices[random.Next(choices.Length)], position.X, position.Y);
                    placed.Add(position);
                    supplyCount++;
                }

                bool TryGetFreeSpot(out CellPosition position)
                {
                    foreach (var pos in spots)
                    {
                        if (placed.Contains(pos)) continue;
                        position = pos;
                        return true;
                    }
                    position = default;
                    return false;
                }

                bool TryPlaceBox()
                {
                    foreach (var prop in boxFootprints.OrderBy(prop => boxUse.TryGetValue(prop.Id, out int uses) ? uses : 0))
                    foreach (var pos in spots)
                    {
                        var footprint = prop.Cells.Select(offset => pos + offset).ToArray();
                        if (footprint.Contains(entrance) || !footprint.All(floor.Contains)) continue;
                        var clear = WalkSpineCells(entrance, cell => floor.Contains(cell) && !footprint.Contains(cell), grid.GetFence);
                        if (clear.Count != floor.Count - footprint.Length || clear.Count < 3) continue;
                        StampSpineProp(metadata, placed, prop, pos, footprint);
                        boxUse.TryGetValue(prop.Id, out int uses);
                        boxUse[prop.Id] = uses + 1;
                        return true;
                    }
                    return false;
                }
            }
            Debug.Log($"[Cvar_ImperiumOfMan] Imperial supply closets furnished={furnished}/{doors.Length}, " +
                $"boxes={boxCount}, mop buckets={mopCount}, cleaning pickups={supplyCount}");
        }
    }
}

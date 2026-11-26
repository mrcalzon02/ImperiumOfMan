using System;
using System.Collections.Generic;
using System.Linq;

namespace ImperiumOfMan
{
    // Pure geometry: the live browser supplies only its currently expanded graph.
    // No game records, Unity objects, or display strings are resolved here.
    internal static class CraftingGraphLayout
    {
        internal struct Point
        {
            public float X, Y;
            public Point(float x, float y) { X = x; Y = y; }
        }

        internal struct Box
        {
            public float Left, Bottom, Width, Height;
            public float Right => Left + Width;
            public float Top => Bottom + Height;
            public Box(float left, float bottom, float width, float height)
            { Left = left; Bottom = bottom; Width = width; Height = height; }
        }

        internal sealed class Node
        {
            public string Key, SortKey;
            public int Column, Group;
            public float Width, Height, X, Y;
            internal int ColumnIndex, Row, GroupRow;
            public Box Bounds => new Box(X - Width / 2, Y - Height / 2, Width, Height);
        }

        internal sealed class Edge
        {
            public string Key, From, To;
            public readonly List<Point> Points = new List<Point>();
            public Point FromLabel, ToLabel;
            internal Node Start, End;
            internal int StartSide, EndSide, Track, TrackLane;
            internal float StartY, EndY;
            internal Leg First, Last;
        }

        internal sealed class Band
        {
            public int Group;
            public Box Bounds;
        }

        internal sealed class Result
        {
            public readonly List<Band> Bands = new List<Band>();
            public Box Bounds;
        }

        private sealed class Row
        {
            public int Group;
            public readonly List<Node> Nodes = new List<Node>();
            public float Height, Bottom;
        }

        private sealed class Port
        {
            public Edge Edge;
            public bool Start;
        }

        internal sealed class Leg
        {
            public int Corridor, Lane;
            public float Low, High;
            public string Key;
        }

        private sealed class Lanes
        {
            private readonly List<List<Point>> _lanes = new List<List<Point>>();
            public int Count => _lanes.Count;
            public int Reserve(float first, float last, float margin)
            {
                float low = Math.Min(first, last), high = Math.Max(first, last);
                for (int lane = 0; lane < _lanes.Count; lane++)
                {
                    if (_lanes[lane].Any(span => low <= span.Y + margin && high >= span.X - margin))
                        continue;
                    _lanes[lane].Add(new Point(low, high));
                    return lane;
                }
                _lanes.Add(new List<Point> { new Point(low, high) });
                return _lanes.Count - 1;
            }
        }

        public static Result Arrange(IList<Node> nodes, IList<Edge> edges, string anchorKey, Point anchor)
        {
            var result = new Result();
            if (nodes.Count == 0) return result;
            var byKey = nodes.ToDictionary(node => node.Key, StringComparer.Ordinal);
            var columns = nodes.Select(node => node.Column).Distinct().OrderBy(value => value).ToArray();
            var columnIndex = columns.Select((column, index) => new { column, index })
                .ToDictionary(pair => pair.column, pair => pair.index);
            foreach (var node in nodes) node.ColumnIndex = columnIndex[node.Column];
            var orderedEdges = edges.OrderBy(edge => edge.Key, StringComparer.Ordinal).ToArray();
            var neighbours = nodes.ToDictionary(node => node.Key, node => new List<Node>(), StringComparer.Ordinal);
            var ports = new Dictionary<string, List<Port>>(StringComparer.Ordinal);
            foreach (var edge in orderedEdges)
            {
                edge.Points.Clear();
                edge.Start = byKey[edge.From];
                edge.End = byKey[edge.To];
                edge.StartSide = edge.End.ColumnIndex < edge.Start.ColumnIndex ? -1 : 1;
                edge.EndSide = edge.End.ColumnIndex == edge.Start.ColumnIndex ? 1 : -edge.StartSide;
                neighbours[edge.From].Add(edge.End);
                neighbours[edge.To].Add(edge.Start);
                AddPort(ports, edge.Start, edge.StartSide, new Port { Edge = edge, Start = true });
                AddPort(ports, edge.End, edge.EndSide, new Port { Edge = edge, Start = false });
            }
            foreach (var node in nodes)
            {
                int left = PortCount(ports, node, -1), right = PortCount(ports, node, 1);
                node.Height = Math.Max(node.Height, Math.Max(left, right) * 4f + 24f);
            }

            var rows = new List<Row>();
            foreach (int group in nodes.Select(node => node.Group).Distinct().OrderBy(value => value))
            {
                var buckets = columns.Select(column => nodes.Where(node => node.Group == group && node.Column == column)
                    .OrderBy(node => node.SortKey ?? node.Key, StringComparer.Ordinal)
                    .ThenBy(node => node.Key, StringComparer.Ordinal).ToList()).ToArray();
                foreach (var bucket in buckets)
                    for (int i = 0; i < bucket.Count; i++) bucket[i].GroupRow = i;
                // Bounded sweeps align related cards; stable keys break every tie.
                for (int sweep = 0; sweep < 2; sweep++)
                    foreach (int column in Enumerable.Range(0, buckets.Length).OrderBy(i => sweep == 0 ? i : -i))
                    {
                        var bucket = buckets[column];
                        var order = bucket.Select(node => new
                        {
                            Node = node,
                            Near = neighbours[node.Key].Where(other => other.Group == group).ToArray()
                        }).OrderBy(pair => pair.Near.Length == 0 ? pair.Node.GroupRow : pair.Near.Average(other => other.GroupRow))
                            .ThenBy(pair => pair.Node.SortKey ?? pair.Node.Key, StringComparer.Ordinal)
                            .ThenBy(pair => pair.Node.Key, StringComparer.Ordinal).Select(pair => pair.Node).ToList();
                        buckets[column] = order;
                        for (int i = 0; i < order.Count; i++) order[i].GroupRow = i;
                    }
                int count = buckets.Max(bucket => bucket.Count);
                for (int row = 0; row < count; row++)
                {
                    var entry = new Row { Group = group };
                    foreach (var bucket in buckets)
                        if (row < bucket.Count)
                        {
                            var node = bucket[row];
                            node.Row = rows.Count;
                            entry.Nodes.Add(node);
                            entry.Height = Math.Max(entry.Height, node.Height);
                        }
                    rows.Add(entry);
                }
            }

            var tracks = Enumerable.Range(0, rows.Count + 1).Select(_ => new Lanes()).ToArray();
            foreach (var edge in orderedEdges)
            {
                if (Math.Abs(edge.Start.ColumnIndex - edge.End.ColumnIndex) == 1)
                {
                    edge.Track = -1;
                    continue;
                }
                // Long and return connections cross columns only in a reserved row gap.
                edge.Track = (edge.Start.Row + edge.End.Row) / 2 + 1;
                edge.TrackLane = tracks[edge.Track].Reserve(edge.Start.ColumnIndex, edge.End.ColumnIndex, 0.25f);
            }
            var trackTop = new float[tracks.Length];
            float cursor = 0;
            Band band = null;
            for (int i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                if (band == null || band.Group != row.Group)
                {
                    band = new Band { Group = row.Group, Bounds = new Box(0, cursor, 0, 0) };
                    result.Bands.Add(band);
                    cursor -= 42f;
                }
                foreach (var node in row.Nodes) node.Y = cursor - row.Height / 2;
                cursor -= row.Height;
                row.Bottom = cursor;
                band.Bounds = new Box(0, cursor - 18f, 0, band.Bounds.Top - cursor + 18f);
                trackTop[i + 1] = cursor - 12f;
                cursor -= Math.Max(56f, tracks[i + 1].Count * 6f + 28f);
            }
            foreach (var pair in ports)
            {
                var ordered = pair.Value.OrderByDescending(port => port.Start ? port.Edge.End.Y : port.Edge.Start.Y)
                    .ThenBy(port => port.Edge.Key, StringComparer.Ordinal).ThenBy(port => port.Start ? 0 : 1).ToArray();
                for (int i = 0; i < ordered.Length; i++)
                {
                    var port = ordered[i];
                    var node = port.Start ? port.Edge.Start : port.Edge.End;
                    int side = port.Start ? port.Edge.StartSide : port.Edge.EndSide;
                    // Opposite sides use different offsets, avoiding coincident horizontal stubs.
                    float phase = side < 0 ? 0.381966f : 0.618034f;
                    float y = node.Y + node.Height / 2 - 12f - (i + phase) * (node.Height - 24f) / ordered.Length;
                    if (port.Start) port.Edge.StartY = y; else port.Edge.EndY = y;
                }
            }

            var legs = new List<Leg>();
            foreach (var edge in orderedEdges)
            {
                float trackY = edge.Track < 0 ? 0 : trackTop[edge.Track] - edge.TrackLane * 6f;
                edge.First = MakeLeg(edge.Start, edge.StartSide, edge.StartY,
                    edge.Track < 0 ? edge.EndY : trackY, edge.Key + "/start");
                legs.Add(edge.First);
                if (edge.Track >= 0)
                {
                    edge.Last = MakeLeg(edge.End, edge.EndSide, edge.EndY, trackY, edge.Key + "/end");
                    legs.Add(edge.Last);
                }
                else edge.Last = edge.First;
            }
            var corridors = Enumerable.Range(0, columns.Length + 1).Select(_ => new Lanes()).ToArray();
            foreach (var leg in legs.OrderBy(leg => leg.Low).ThenBy(leg => leg.High).ThenBy(leg => leg.Key, StringComparer.Ordinal))
                leg.Lane = corridors[leg.Corridor].Reserve(leg.Low, leg.High, 8f);
            var widths = columns.Select(column => nodes.Where(node => node.Column == column).Max(node => node.Width)).ToArray();
            var centres = new float[columns.Length];
            var gutterWidths = corridors.Select(lanes => Math.Max(112f, lanes.Count * 6f + 36f)).ToArray();
            for (int i = 1; i < columns.Length; i++)
                centres[i] = centres[i - 1] + widths[i - 1] / 2 + gutterWidths[i] + widths[i] / 2;
            foreach (var node in nodes) node.X = centres[node.ColumnIndex];
            var corridorCentre = new float[corridors.Length];
            corridorCentre[0] = centres[0] - widths[0] / 2 - gutterWidths[0] / 2;
            corridorCentre[columns.Length] = centres[columns.Length - 1] + widths[columns.Length - 1] / 2 + gutterWidths[columns.Length] / 2;
            for (int i = 1; i < columns.Length; i++)
                corridorCentre[i] = centres[i - 1] + widths[i - 1] / 2 + gutterWidths[i] / 2;
            foreach (var edge in orderedEdges)
            {
                var from = new Point(edge.Start.X + edge.StartSide * edge.Start.Width / 2, edge.StartY);
                var to = new Point(edge.End.X + edge.EndSide * edge.End.Width / 2, edge.EndY);
                float firstX = LaneX(edge.First, corridorCentre, corridors), lastX = LaneX(edge.Last, corridorCentre, corridors);
                Push(edge.Points, from);
                Push(edge.Points, new Point(firstX, from.Y));
                if (edge.Track >= 0)
                {
                    float y = trackTop[edge.Track] - edge.TrackLane * 6f;
                    Push(edge.Points, new Point(firstX, y));
                    Push(edge.Points, new Point(lastX, y));
                }
                Push(edge.Points, new Point(lastX, to.Y));
                Push(edge.Points, to);
                edge.FromLabel = new Point((from.X + firstX) / 2, from.Y + 10f);
                edge.ToLabel = new Point((to.X + lastX) / 2, to.Y + 10f);
            }
            foreach (var entry in result.Bands)
            {
                var members = nodes.Where(node => node.Group == entry.Group).ToArray();
                float left = members.Min(node => node.Bounds.Left) - 18f, right = members.Max(node => node.Bounds.Right) + 18f;
                entry.Bounds = new Box(left, entry.Bounds.Bottom, right - left, entry.Bounds.Height);
            }

            // Layout changes preserve the selected card's exact graph/screen anchor.
            if (anchorKey != null && byKey.TryGetValue(anchorKey, out var selected))
            {
                float dx = anchor.X - selected.X, dy = anchor.Y - selected.Y;
                foreach (var node in nodes) { node.X += dx; node.Y += dy; }
                foreach (var edge in orderedEdges)
                {
                    for (int i = 0; i < edge.Points.Count; i++) edge.Points[i] = Move(edge.Points[i], dx, dy);
                    edge.FromLabel = Move(edge.FromLabel, dx, dy);
                    edge.ToLabel = Move(edge.ToLabel, dx, dy);
                }
                foreach (var entry in result.Bands)
                    entry.Bounds = new Box(entry.Bounds.Left + dx, entry.Bounds.Bottom + dy, entry.Bounds.Width, entry.Bounds.Height);
            }
            float minX = result.Bands.Min(entry => entry.Bounds.Left), maxX = result.Bands.Max(entry => entry.Bounds.Right);
            float minY = result.Bands.Min(entry => entry.Bounds.Bottom), maxY = result.Bands.Max(entry => entry.Bounds.Top);
            foreach (var point in orderedEdges.SelectMany(edge => edge.Points))
            {
                minX = Math.Min(minX, point.X); maxX = Math.Max(maxX, point.X);
                minY = Math.Min(minY, point.Y); maxY = Math.Max(maxY, point.Y);
            }
            result.Bounds = new Box(minX, minY, maxX - minX, maxY - minY);
            return result;
        }

        private static Point Move(Point point, float dx, float dy) => new Point(point.X + dx, point.Y + dy);
        private static string PortKey(Node node, int side) => node.Key + (side < 0 ? "\nleft" : "\nright");
        private static int PortCount(Dictionary<string, List<Port>> ports, Node node, int side)
            => ports.TryGetValue(PortKey(node, side), out var list) ? list.Count : 0;
        private static void AddPort(Dictionary<string, List<Port>> ports, Node node, int side, Port port)
        {
            string key = PortKey(node, side);
            if (!ports.TryGetValue(key, out var list)) ports.Add(key, list = new List<Port>());
            list.Add(port);
        }
        private static Leg MakeLeg(Node node, int side, float first, float last, string key)
            => new Leg { Corridor = node.ColumnIndex + (side > 0 ? 1 : 0), Low = Math.Min(first, last), High = Math.Max(first, last), Key = key };
        private static float LaneX(Leg leg, float[] centres, Lanes[] lanes)
            => centres[leg.Corridor] + (leg.Lane - (lanes[leg.Corridor].Count - 1) / 2f) * 6f;
        private static void Push(List<Point> points, Point point)
        {
            if (points.Count > 0 && Math.Abs(points[points.Count - 1].X - point.X) < 0.001f && Math.Abs(points[points.Count - 1].Y - point.Y) < 0.001f)
                return;
            points.Add(point);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using ImperiumOfMan;
using Node = ImperiumOfMan.CraftingGraphLayout.Node;
using Edge = ImperiumOfMan.CraftingGraphLayout.Edge;
using Point = ImperiumOfMan.CraftingGraphLayout.Point;
using Box = ImperiumOfMan.CraftingGraphLayout.Box;

internal static class Program
{
    private const float Epsilon = 0.002f;
    private static int _assertions;

    private static void Main(string[] args)
    {
        var watch = Stopwatch.StartNew();
        var fan = Fanout(32);
        Check("dense categorized fan-out", fan.Nodes, fan.Edges, true);
        var shared = Shared();
        Check("shared ingredients and co-products", shared.Nodes, shared.Edges, true);
        var cycle = Cycle();
        Check("return recipes and same-column cycles", cycle.Nodes, cycle.Edges, true);
        var dense = Dense();
        Check("240 nodes and 2400 connections", dense.Nodes, dense.Edges, false);
        AnchorExpansion();
        ResponsiveViewport();
        if (args.Length == 1) WriteSvg(args[0], shared.Nodes, shared.Edges);
        Console.WriteLine("PASS: " + _assertions + " geometry assertions in " + watch.ElapsedMilliseconds + " ms");
    }

    private static void ResponsiveViewport()
    {
        var sizes = new[] { (1024,768), (1280,720), (1920,1080), (2048,576), (2560,1080), (3440,1440), (3840,1080) };
        foreach (var size in sizes)
            for (int percent = 70; percent <= 150; percent += 10)
            {
                var open = CraftingViewportLayout.Calculate(size.Item1, size.Item2, percent, true);
                var closed = CraftingViewportLayout.Calculate(size.Item1, size.Item2, percent, false);
                Require(open.GraphWidth > 0 && open.GraphHeight > 0, "empty responsive viewport");
                Require(closed.GraphWidth >= open.GraphWidth && closed.GraphHeight >= open.GraphHeight, "collapse lost graph space");
                Require(open.GraphTop >= 70 && open.GraphTop <= 102, "unexpected toolbar stack");
                Require(open.Side ? open.GraphBottom == 8 && open.GraphRight > 8 : open.GraphRight == 8, "inspector overlaps graph");
                if (open.SharedTools)
                    Require(open.Width >= CraftingViewportLayout.FilterRight + CraftingViewportLayout.ToolWidth + 16, "shared toolbar overflow");
                float zoom = CraftingViewportLayout.FitZoom(open.GraphWidth, open.GraphHeight, 1400, 1500);
                Require(1400 * zoom <= open.GraphWidth && 1500 * zoom <= open.GraphHeight, "fit clips representative graph");
            }
        var wide = CraftingViewportLayout.Calculate(2048,576,100,true);
        Require(wide.Side && wide.SharedTools, "reported ultrawide resolution lacks side docking");
        Require(wide.GraphHeight * wide.Scale >= 480, "reported ultrawide graph loses height");
        Console.WriteLine("PASS responsive crafting viewport and fit");
    }

    private sealed class Graph
    {
        public readonly List<Node> Nodes = new List<Node>();
        public readonly List<Edge> Edges = new List<Edge>();
        public Node Add(string key, int column, int group)
        {
            var node = new Node { Key = key, SortKey = key, Column = column, Group = group, Width = 220, Height = 84 };
            Nodes.Add(node); return node;
        }
        public void Link(Node from, Node to) => Edges.Add(new Edge { Key = from.Key + ":" + to.Key, From = from.Key, To = to.Key });
    }

    private static Graph Fanout(int count)
    {
        var graph = new Graph();
        var root = graph.Add("Shared ore", 0, 0);
        for (int i = 0; i < count; i++)
        {
            var recipe = graph.Add("Process " + i.ToString("00"), 1, 1 + i % 4);
            var output = graph.Add("Material " + i.ToString("00"), 2, 1 + i % 4);
            graph.Link(root, recipe); graph.Link(recipe, output);
        }
        return graph;
    }

    private static Graph Shared()
    {
        var graph = new Graph();
        var water = graph.Add("Water", 0, 0);
        var mineral = graph.Add("Mineral feedstock", 0, 0);
        var biomass = graph.Add("Biomass", 0, 0);
        var extract = graph.Add("Chemical extract", 2, 3);
        for (int i = 0; i < 9; i++)
        {
            int family = 1 + i % 3;
            var recipe = graph.Add("Processing " + i, 1, family);
            var output = graph.Add("Product " + i, 2, family);
            graph.Link(i % 2 == 0 ? water : biomass, recipe);
            graph.Link(mineral, recipe);
            graph.Link(recipe, output);
            if (i % 3 == 0) graph.Link(recipe, extract);
        }
        var recovery = graph.Add("Recover ingredients", 3, 4);
        graph.Link(extract, recovery); graph.Link(recovery, mineral);
        return graph;
    }

    private static Graph Cycle()
    {
        var graph = new Graph();
        var a = graph.Add("A", 0, 1); var b = graph.Add("B", 1, 1); var c = graph.Add("C", 2, 2);
        var d = graph.Add("D", 0, 2); var e = graph.Add("E", 0, 2);
        graph.Link(a, b); graph.Link(b, c); graph.Link(c, a); graph.Link(a, d); graph.Link(d, e); graph.Link(e, a);
        graph.Link(b, a); graph.Link(c, e);
        return graph;
    }

    private static Graph Dense()
    {
        var graph = new Graph();
        for (int i = 0; i < 240; i++) graph.Add("N" + i.ToString("000"), i % 6, i % 11);
        for (int i = 0; i < 240; i++)
            for (int j = 1; j <= 10; j++) graph.Link(graph.Nodes[i], graph.Nodes[(i + 7 * j) % 240]);
        return graph;
    }

    private static void Check(string name, List<Node> nodes, List<Edge> edges, bool checkCoincident)
    {
        var result = CraftingGraphLayout.Arrange(nodes, edges, nodes[0].Key, new Point(53, -27));
        var byKey = nodes.ToDictionary(node => node.Key);
        for (int i = 0; i < nodes.Count; i++)
            for (int j = i + 1; j < nodes.Count; j++)
                Require(!Overlap(nodes[i].Bounds, nodes[j].Bounds), name + ": cards overlap " + nodes[i].Key + "/" + nodes[j].Key);
        var segments = new List<(string Key, Point A, Point B)>();
        var endpointPorts = new Dictionary<string, List<Point>>();
        foreach (var edge in edges)
        {
            Require(edge.Points.Count >= 2 && edge.Points.Count <= 6, name + ": route segment budget");
            Require(OnBoundary(edge.Points[0], byKey[edge.From].Bounds), name + ": wrong source port");
            Require(OnBoundary(edge.Points.Last(), byKey[edge.To].Bounds), name + ": wrong destination port");
            RegisterPort(endpointPorts, edge.From, edge.Points[0], name);
            RegisterPort(endpointPorts, edge.To, edge.Points.Last(), name);
            for (int i = 1; i < edge.Points.Count; i++)
            {
                Point a = edge.Points[i - 1], b = edge.Points[i];
                Require(Near(a.X, b.X) || Near(a.Y, b.Y), name + ": diagonal dependency line");
                Require(!Near(a.X, b.X) || !Near(a.Y, b.Y), name + ": zero-length line");
                foreach (var node in nodes)
                    Require(!CrossesInterior(a, b, node.Bounds), name + ": line crosses " + node.Key + " on " + edge.Key);
                segments.Add((edge.Key, a, b));
            }
            int contextVertices = (edge.Points.Count - 1) * 4 + 3;
            int selectedVertices = (edge.Points.Count - 1) * 8 + 3;
            Require(contextVertices * 512 < 65000 && selectedVertices * 512 < 65000, name + ": mesh chunk budget");
        }
        if (checkCoincident)
            for (int i = 0; i < segments.Count; i++)
                for (int j = i + 1; j < segments.Count; j++)
                    if (segments[i].Key != segments[j].Key)
                        Require(!Coincident(segments[i].A, segments[i].B, segments[j].A, segments[j].B),
                            name + ": coincident dependency lines " + segments[i].Key + "/" + segments[j].Key);
        string first = Snapshot(nodes, edges);
        var reorderedNodes = nodes.AsEnumerable().Reverse().ToList();
        var reorderedEdges = edges.AsEnumerable().Reverse().ToList();
        CraftingGraphLayout.Arrange(reorderedNodes, reorderedEdges, nodes[0].Key, new Point(53, -27));
        Require(first == Snapshot(nodes, edges), name + ": ordering changed the layout");
        Require(result.Bounds.Width > 0 && result.Bounds.Height > 0, name + ": invalid fit bounds");
        Console.WriteLine("PASS " + name + ": " + nodes.Count + " cards, " + edges.Count + " connections");
    }

    private static void AnchorExpansion()
    {
        var graph = Fanout(4);
        var selected = graph.Nodes[3];
        CraftingGraphLayout.Arrange(graph.Nodes, graph.Edges, selected.Key, new Point(123.25f, -47.5f));
        var before = new Point(selected.X, selected.Y);
        for (int i = 0; i < 15; i++)
        {
            var node = graph.Add("New upstream " + i, -1, i % 5);
            graph.Link(node, selected);
        }
        CraftingGraphLayout.Arrange(graph.Nodes, graph.Edges, selected.Key, before);
        Require(Near(selected.X, before.X) && Near(selected.Y, before.Y), "expansion moved the selected anchor");
        const float zoom = 0.73f, panX = 82, panY = -19;
        Require(Near(selected.X * zoom + panX, before.X * zoom + panX) && Near(selected.Y * zoom + panY, before.Y * zoom + panY),
            "expansion moved the screen anchor with retained pan/zoom");
        Console.WriteLine("PASS selection anchor across expansion");
    }

    private static void RegisterPort(Dictionary<string, List<Point>> ports, string node, Point point, string test)
    {
        if (!ports.TryGetValue(node, out var list)) ports.Add(node, list = new List<Point>());
        Require(list.All(other => !Near(point.X, other.X) || !Near(point.Y, other.Y)), test + ": duplicate port on " + node);
        list.Add(point);
    }
    private static bool Near(float a, float b) => Math.Abs(a - b) < Epsilon;
    private static bool Overlap(Box a, Box b) => a.Left < b.Right - Epsilon && a.Right > b.Left + Epsilon && a.Bottom < b.Top - Epsilon && a.Top > b.Bottom + Epsilon;
    private static bool OnBoundary(Point p, Box b) => p.Y >= b.Bottom - Epsilon && p.Y <= b.Top + Epsilon && (Near(p.X, b.Left) || Near(p.X, b.Right));
    private static bool CrossesInterior(Point a, Point b, Box box) => Near(a.X, b.X)
        ? a.X > box.Left + Epsilon && a.X < box.Right - Epsilon && Math.Max(a.Y, b.Y) > box.Bottom + Epsilon && Math.Min(a.Y, b.Y) < box.Top - Epsilon
        : a.Y > box.Bottom + Epsilon && a.Y < box.Top - Epsilon && Math.Max(a.X, b.X) > box.Left + Epsilon && Math.Min(a.X, b.X) < box.Right - Epsilon;
    private static bool Coincident(Point a, Point b, Point c, Point d) => Near(a.X, b.X) && Near(c.X, d.X)
        ? Near(a.X, c.X) && Math.Min(Math.Max(a.Y, b.Y), Math.Max(c.Y, d.Y)) - Math.Max(Math.Min(a.Y, b.Y), Math.Min(c.Y, d.Y)) > Epsilon
        : Near(a.Y, b.Y) && Near(c.Y, d.Y) && Near(a.Y, c.Y) && Math.Min(Math.Max(a.X, b.X), Math.Max(c.X, d.X)) - Math.Max(Math.Min(a.X, b.X), Math.Min(c.X, d.X)) > Epsilon;
    private static string Snapshot(List<Node> nodes, List<Edge> edges) => string.Join("|", nodes.OrderBy(n => n.Key).Select(n => n.Key + ":" + n.X + ":" + n.Y)) +
        string.Join("|", edges.OrderBy(e => e.Key).Select(e => e.Key + ":" + string.Join(";", e.Points.Select(p => p.X + "," + p.Y))));
    private static void Require(bool condition, string message) { _assertions++; if (!condition) throw new InvalidOperationException(message); }

    private static void WriteSvg(string path, List<Node> nodes, List<Edge> edges)
    {
        var result = CraftingGraphLayout.Arrange(nodes, edges, nodes[0].Key, new Point(0, 0));
        var box = result.Bounds;
        string F(float value) => value.ToString("0.###", CultureInfo.InvariantCulture);
        var svg = new StringBuilder("<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"");
        svg.Append(F(box.Left - 20)).Append(' ').Append(F(-box.Top - 20)).Append(' ').Append(F(box.Width + 40)).Append(' ').Append(F(box.Height + 40)).Append("\">");
        svg.Append("<rect x=\"-100000\" y=\"-100000\" width=\"200000\" height=\"200000\" fill=\"#06100b\"/>");
        foreach (var band in result.Bands)
            svg.Append("<rect x=\"").Append(F(band.Bounds.Left)).Append("\" y=\"").Append(F(-band.Bounds.Top)).Append("\" width=\"").Append(F(band.Bounds.Width)).Append("\" height=\"").Append(F(band.Bounds.Height)).Append("\" fill=\"#15251a\" stroke=\"#45664b\"/>");
        foreach (var edge in edges)
            svg.Append("<polyline fill=\"none\" stroke=\"#91a979\" stroke-width=\"1.5\" points=\"").Append(string.Join(" ", edge.Points.Select(p => F(p.X) + "," + F(-p.Y)))).Append("\"/>");
        foreach (var node in nodes)
        {
            var b = node.Bounds;
            svg.Append("<rect x=\"").Append(F(b.Left)).Append("\" y=\"").Append(F(-b.Top)).Append("\" width=\"").Append(F(b.Width)).Append("\" height=\"").Append(F(b.Height)).Append("\" fill=\"#102318\" stroke=\"#81b57a\"/>");
            svg.Append("<text x=\"").Append(F(b.Left + 8)).Append("\" y=\"").Append(F(-node.Y + 5)).Append("\" font-family=\"sans-serif\" font-size=\"13\" fill=\"#c4dab9\">").Append(System.Security.SecurityElement.Escape(node.Key)).Append("</text>");
        }
        svg.Append("</svg>");
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)));
        File.WriteAllText(path, svg.ToString());
        Console.WriteLine("Geometry preview: " + path);
    }
}

using System;
using System.Linq;
using ImperiumOfMan;

static void Require(bool condition, string message)
{
    if (!condition) throw new Exception(message);
}

var random = new Random(1729);
string[][] pools = [["a1", "a2", "a3"], [], ["b1"]];
var sizes = new int[9];
int a = 0, b = 0;
for (int i = 0; i < 10000; i++)
{
    var roll = DataRackRoller.Roll(random, "basic", pools, 5, 8);
    Require(roll.Length >= 5 && roll.Length <= 8, "payout count");
    Require(roll.Contains("basic") && roll.Any(x => x != "basic"), "mixed payout");
    Require(roll.All(x => x == "basic" || pools.SelectMany(p => p).Contains(x)), "unknown chip");
    sizes[roll.Length]++;
    a += roll.Count(x => x.StartsWith("a"));
    b += roll.Count(x => x.StartsWith("b"));
}
Require(sizes.Skip(5).All(x => x > 2000), "all four sizes should be represented");
Require(Math.Abs(a - b) < (a + b) * 0.04, "faction weighting should be independent of pool size");
for (int i = 0; i < 1000; i++)
{
    var roll = DataRackRoller.Roll(random, "basic", [], 5, 8);
    Require(roll.Length >= 5 && roll.Length <= 8 && roll.All(x => x == "basic"),
        "no-faction compatibility");
}
Console.WriteLine("Data Rack: 10,000 mixed rolls + 1,000 basic-only rolls passed.");

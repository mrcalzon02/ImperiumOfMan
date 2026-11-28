using System;
using System.Linq;

namespace ImperiumOfMan
{
    // Game-independent payout selector. Random is injected for reproducible
    // tests; each array entry represents one physical, separately rolled chip.
    internal static class DataRackRoller
    {
        internal static string[] Roll(Random random, string basic, string[][] factions,
            int minimum, int maximum)
        {
            if (random == null) throw new ArgumentNullException(nameof(random));
            if (string.IsNullOrEmpty(basic)) throw new ArgumentException(nameof(basic));
            if (minimum < 2 || maximum < minimum || maximum == int.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(minimum));
            var pools = (factions ?? Array.Empty<string[]>())
                .Where(pool => pool != null && pool.Length > 0).ToArray();
            var chips = new string[random.Next(minimum, maximum + 1)];
            for (int i = 0; i < chips.Length; i++)
            {
                chips[i] = basic;
                // Guarantee both kinds when a faction pool exists. Balance by
                // faction first, rather than by number of records per faction.
                if (pools.Length > 0 && (i == 1 || (i > 1 && random.Next(100) < 40)))
                {
                    string[] pool = pools[random.Next(pools.Length)];
                    chips[i] = pool[random.Next(pool.Length)];
                }
            }
            return chips;
        }
    }
}

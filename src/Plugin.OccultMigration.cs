using System;
using System.Collections.Generic;
using HarmonyLib;
using MGSC;
using UnityEngine;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private static readonly string[] LegacyOccultFacilities =
        [
            "iom_occult_clone_vat", "iom_occult_containment_cradle",
            "iom_occult_essence_still", "iom_occult_gavvakh_retort"
        ];

        [HarmonyPatch(typeof(MagnumProgression), nameof(MagnumProgression.OnAfterLoad))]
        private static class MigrateLegacyOccultCapabilities
        {
            private static void Prefix(MagnumProgression __instance, List<string> ____purchasedPerks)
            {
                if (!_industryReady || __instance == null || ____purchasedPerks == null) return;

                // The old layout had four facilities before research I–V. Those
                // facility IDs distinguish it from saves using the six-node layout.
                var previous = new HashSet<string>(____purchasedPerks, StringComparer.Ordinal);
                if (!previous.Overlaps(LegacyOccultFacilities)) return;
                foreach (string rank in OccultRanks)
                    if (Data.MagnumPerks.GetRecord(rank) == null) return;

                int mappedRank = previous.Contains(LegacyOccultFacilities[2]) ||
                    previous.Contains(LegacyOccultFacilities[3]) ? 2 : 1;
                // Read the entire old state before adding reused rank IDs: old
                // research I/II/III become III/IV/V; old IV and V both become VI.
                for (int index = 0; index < 5; index++)
                    if (previous.Contains(OccultRanks[index]))
                        mappedRank = Math.Max(mappedRank, Math.Min(index + 3, OccultRanks.Length));

                for (int index = 0; index < mappedRank; index++)
                    __instance.AddPerk(OccultRanks[index], silent: true);

                // Retire only the obsolete markers. Removing them makes subsequent
                // loads a no-op; native/other perks and all recipe knowledge remain.
                ____purchasedPerks.RemoveAll(id => Array.IndexOf(LegacyOccultFacilities, id) >= 0);
                Debug.Log("[Cvar_ImperiumOfMan] Migrated legacy occult capabilities through " +
                    OccultResearchTitle(OccultRanks[mappedRank - 1]) + ". Recipe knowledge unchanged.");
            }
        }
    }
}

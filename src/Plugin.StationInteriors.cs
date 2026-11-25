using System.Collections.Generic;
using HarmonyLib;
using MGSC;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private const string HavocFabrication = "iom_havoc_fabrication";

        private static readonly Dictionary<string, string> StationVisitLayouts = new()
        {
            [TerraStation] = "PineGapTraining",
            [LunaStation] = "SterlingsFarm",
            [PhobosStation] = "Canaan",
            [HavocStation] = "SampoQuern",
            [FoundryStation] = "Gloria",
            [NeithVault] = "FrescoPlatforms",
            [TitanFortress] = "Carcosa",
            [BroadswordStation] = "CassinisShadow",
            ["iomHollowSignalDepot"] = "Canaan",
            ["iomOssuaryWorks"] = "QiHelium",
            ["iomVeiledGate"] = "FrescoPlatforms",
            ["iomCinderCrucible"] = "Goethe",
            ["iomChainforge"] = "AliJinnah",
            ["iomBlackMeridian"] = "CassinisShadow",
        };

        [HarmonyPatch(typeof(GameWorldStructure), nameof(GameWorldStructure.LoadWorldStructure))]
        private static class ImperialStationVisits
        {
            private static void Prefix(string folder, ref string id)
            {
                // The raid metadata retains the actual station ID, which scopes trade and stash contents.
                if (folder == "Stations" && StationVisitLayouts.TryGetValue(id, out string layout))
                {
                    id = layout;
                }
            }
        }
    }
}

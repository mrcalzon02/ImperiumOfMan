using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using MGSC;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private const int GoldenTier = 11;
        private static int? _standardTechLimit;

        private static void RegisterTechnologyTier()
        {
            int currentLimit = Data.Global.MaxTechLevel;
            if (currentLimit >= GoldenTier)
            {
                return;
            }

            var previous = Data.TechLevels[GoldenTier - 1];
            Data.TechLevels.Add(GoldenTier, new TechLevelRecord
            {
                TechLevel = GoldenTier,
                MaxMissionsCount = previous.MaxMissionsCount
            });
            previous.ExperienceToLevelUp = 5500;

            // Existing trade tables were filled before the mod raised the cap.
            foreach (var tiers in Data.FactionDrop._recordsByFactions.Values)
            {
                tiers.Add(GoldenTier, new List<ContentDropRecord>(tiers[currentLimit]));
            }
            _standardTechLimit = currentLimit;
            Data.Global.MaxTechLevel = GoldenTier;
        }

        private static int GetFactionTechCap(Faction faction) =>
            faction.Id == ChaosFaction ? 10 :
            faction.Id == ItemCategory ? Data.Global.MaxTechLevel : _standardTechLimit ?? Data.Global.MaxTechLevel;

        [HarmonyPatch]
        private static class FactionTechnologyCaps
        {
            private static IEnumerable<MethodBase> TargetMethods()
            {
                yield return AccessTools.Method(typeof(FactionSystem), nameof(FactionSystem.TechLevelTick));
                yield return AccessTools.Method(typeof(StationSystem), nameof(StationSystem.GetAverageTravelTechLevelGainPercent));
                yield return AccessTools.Method(typeof(StationSystem), "ConvertTechLevelGainToPercent");
                yield return AccessTools.Method(typeof(FactionTechnologyWindow), nameof(FactionTechnologyWindow.Configure));
                yield return AccessTools.Method(typeof(FactionTechnologyWindow), "InitRewardPanels");
                yield return AccessTools.Method(typeof(TooltipFactory), nameof(TooltipFactory.BuildFactionTechLevelTooltip));
                yield return AccessTools.Method(typeof(FactionDropCollection), nameof(FactionDropCollection.GetTechLevelLimit));
                yield return AccessTools.Method(typeof(FactionDropCollection), nameof(FactionDropCollection.GetTradeItems),
                    [typeof(Faction), typeof(int), typeof(FactionDropCollection.TradeItemsFor), typeof(bool)]);
            }

            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original)
            {
                var codes = instructions.ToList();
                var global = AccessTools.PropertyGetter(typeof(Data), nameof(Data.Global));
                var max = AccessTools.PropertyGetter(typeof(GlobalSettings), nameof(GlobalSettings.MaxTechLevel));
                var current = AccessTools.Field(typeof(Faction), nameof(Faction.CurrentTechLevel));
                var cap = AccessTools.Method(typeof(Plugin), nameof(GetFactionTechCap));
                int replacements = 0;
                for (int i = 0; i < codes.Count - 1; i++)
                {
                    if (!codes[i].Calls(global) || !codes[i + 1].Calls(max))
                    {
                        continue;
                    }

                    var load = codes[i];
                    if (original.Name == nameof(FactionSystem.TechLevelTick) ||
                        original.Name == nameof(StationSystem.GetAverageTravelTechLevelGainPercent))
                    {
                        // These loops are already reading the current faction's level.
                        if (i < 2 || !codes[i - 1].LoadsField(current))
                        {
                            throw new InvalidOperationException("Cannot locate the faction in " + original.Name);
                        }
                        load.opcode = codes[i - 2].opcode;
                        load.operand = codes[i - 2].operand;
                    }
                    else
                    {
                        load.opcode = original.IsStatic || original.Name == "InitRewardPanels" ? OpCodes.Ldarg_0 : OpCodes.Ldarg_1;
                        load.operand = null;
                        if (original.Name == "InitRewardPanels")
                        {
                            codes.Insert(++i, new CodeInstruction(OpCodes.Ldfld,
                                AccessTools.Field(typeof(FactionTechnologyWindow), "_faction")));
                        }
                    }
                    codes[i + 1].opcode = OpCodes.Call;
                    codes[i + 1].operand = cap;
                    replacements++;
                }
                int expected = original.Name == nameof(FactionSystem.TechLevelTick) ? 2 : 1;
                if (replacements != expected)
                {
                    throw new InvalidOperationException("Could not replace the technology cap in " + original.Name);
                }
                return codes;
            }
        }
    }
}

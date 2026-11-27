using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MGSC;
using UnityEngine;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        // Native UnlockDatadisk grants default ammunition with weapons, and
        // every member of an armor set with some armor chips. Our tracked
        // occult patterns promise one saved selection, not those bonus recipes.
        // Ordinary native/other-mod chips and non-occult legacy rolls retain
        // the original method and existing learned recipes are never revoked.
        [HarmonyPatch(typeof(ItemInteractionSystem), nameof(ItemInteractionSystem.UnlockDatadisk))]
        private static class LearnExactOccultPattern
        {
            [HarmonyPriority(Priority.First)]
            private static bool Prefix(Statistics statistics, MagnumCargo magnumCargo, BasePickupItem item)
            {
                if (!_industryReady || item == null || !IsTrackedOccultPattern(item.Id))
                    return true;

                var disk = item.Record<DatadiskRecord>();
                var selection = item.Comp<DatadiskComponent>();
                string output = selection?.UnlockId;
                if (disk == null || disk.UnlockType != DatadiskUnlockType.ProductionItem ||
                    string.IsNullOrEmpty(output) || !OccultRecipeRequirements.ContainsKey(output))
                    return true;

                // The selected output lives on the physical item, including an
                // old saved chip. Do not reroll, consult a random pool or require
                // research here. Capability is enforced when manufacturing.
                if (magnumCargo == null || item.Storage == null || statistics == null ||
                    Data.Items.GetRecord(output) == null ||
                    !Data.ProduceReceipts.Any(recipe => recipe.OutputItem == output))
                {
                    Debug.LogWarning("[Cvar_ImperiumOfMan] Occult pattern learning rejected | chip=" +
                        item.Id + " | selection=" + output + " | reason=missing_recipe_or_storage");
                    return false;
                }

                if (magnumCargo.UnlockedProductionItems.Contains(output))
                {
                    // Match native single-item duplicate behavior: no removal,
                    // no statistic increment and no incidental unlock.
                    Debug.Log("[Cvar_ImperiumOfMan] Occult pattern already learned | chip=" +
                        item.Id + " | selection=" + output);
                    return false;
                }

                string chipId = item.Id;
                var storage = item.Storage;
                magnumCargo.UnlockedProductionItems.Add(output);
                UI.Staff.NotificationPanel.AddUnlockDatadiskNotify(item);
                statistics.IncreaseStatistic(StatisticType.ChipUnlock);
                storage.Remove(item);

                Debug.Log("[Cvar_ImperiumOfMan] Occult pattern learned | chip=" + chipId +
                    " | selection=" + output + " | learned_added=1 | chip_removed=" +
                    !storage.Items.Any(existing => ReferenceEquals(existing, item)));
                return false;
            }
        }

        private sealed class OccultPurchaseTrace
        {
            public string Id;
            public bool PreviouslyPurchased;
            public Dictionary<string, int> Required;
            public Dictionary<string, int> Before;
            public HashSet<string> LearnedBefore;
        }

        // The native UI is the authority for eligibility, payment and purchase.
        // Observe its actual Yes confirmation rather than AddPerk (which is also
        // called by migration and other systems). No cargo or capability grants.
        [HarmonyPatch(typeof(MagnumModuleTechTreeWindow), "ConfirmUpgradePerkDialogOnOptionSelected")]
        private static class TraceOccultResearchPurchase
        {
            private static void Prefix(ConfirmMagnumUpgradeWindow.Option obj,
                MagnumTechTreePanel ____panelToUpgrade, MagnumProgression ____magnumSpaceship,
                MagnumCargo ____magnumCargo, Mercenaries ____mercenaries, out OccultPurchaseTrace __state)
            {
                __state = null;
                if (!_industryReady || obj != ConfirmMagnumUpgradeWindow.Option.Yes ||
                    ____panelToUpgrade?.Record == null || ____magnumSpaceship == null ||
                    ____magnumCargo == null || ____mercenaries == null) return;
                var record = ____panelToUpgrade.Record;
                if (!OccultRanks.Contains(record.Id) && !IsOccultProductionUpgrade(record.Id)) return;
                try
                {
                    var required = record.UpgradePrice.GroupBy(id => id, StringComparer.Ordinal)
                        .ToDictionary(group => group.Key, group => group.Count(), StringComparer.Ordinal);
                    __state = new OccultPurchaseTrace
                    {
                        Id = record.Id,
                        PreviouslyPurchased = ____magnumSpaceship.IsPerkPurchased(record.Id),
                        Required = required,
                        Before = required.Keys.ToDictionary(id => id,
                            id => ItemInteractionSystem.Count(____mercenaries, ____magnumCargo, id),
                            StringComparer.Ordinal),
                        LearnedBefore = new HashSet<string>(____magnumCargo.UnlockedProductionItems,
                            StringComparer.Ordinal)
                    };
                }
                catch (Exception ex) { OccultAuditUnavailable("research purchase before", ex); }
            }

            private static void Postfix(MagnumProgression ____magnumSpaceship, MagnumCargo ____magnumCargo,
                Mercenaries ____mercenaries, OccultPurchaseTrace __state, bool __runOriginal)
            {
                if (!__runOriginal || __state == null) return;
                try
                {
                    bool installed = ____magnumSpaceship.IsPerkPurchased(__state.Id);
                    bool purchased = !__state.PreviouslyPurchased && installed;
                    string payment = string.Join(",", __state.Required.Select(entry =>
                    {
                        int after = ItemInteractionSystem.Count(____mercenaries, ____magnumCargo, entry.Key);
                        int before = __state.Before[entry.Key];
                        return entry.Key + ":required=" + entry.Value + "/before=" + before +
                            "/after=" + after + "/delta=" + (after - before);
                    }));
                    string[] newKnowledge = ____magnumCargo.UnlockedProductionItems
                        .Where(id => !__state.LearnedBefore.Contains(id)).Distinct(StringComparer.Ordinal).ToArray();
                    Debug.Log("[Cvar_ImperiumOfMan] Occult research purchase | perk=" + __state.Id +
                        " | purchased=" + purchased + " | installed=" + installed +
                        " | payment=" + payment + " | recipes_added=" +
                        (newKnowledge.Length == 0 ? "none" : string.Join(",", newKnowledge)));
                }
                catch (Exception ex) { OccultAuditUnavailable("research purchase after", ex); }
            }
        }
    }
}

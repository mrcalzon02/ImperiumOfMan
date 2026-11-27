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
        // The native four-argument cancellation method refunds RequiredItems even
        // if List.Remove returns false. Reject stale/foreign occult orders before
        // native code runs; valid cancellation keeps the game's refund semantics.
        [HarmonyPatch(typeof(ItemProductionSystem), nameof(ItemProductionSystem.CancelProduction),
            [typeof(MagnumCargo), typeof(SpaceTime), typeof(int), typeof(ProduceOrder)])]
        private static class GuardOccultCancellation
        {
            [HarmonyPriority(Priority.First)]
            private static bool Prefix(MagnumCargo magnumCargo, int lineIndex, ProduceOrder orderToRemove,
                out Dictionary<string, long> __state)
            {
                __state = null;
                if (!IsOccultLine(lineIndex)) return true;
                if (magnumCargo == null || orderToRemove == null ||
                    !magnumCargo.ItemProduceOrders.TryGetValue(lineIndex, out var orders) ||
                    !orders.Any(order => ReferenceEquals(order, orderToRemove)))
                {
                    Debug.LogWarning("[Cvar_ImperiumOfMan] Occult cancellation rejected | line=" +
                        lineIndex + " | reason=order_not_in_line");
                    return false;
                }
                try { __state = OccultCargoSnapshot(magnumCargo, orderToRemove.RequiredItems); }
                catch (Exception ex) { OccultAuditUnavailable("cancel before", ex); }
                return true;
            }

            private static void Postfix(MagnumCargo magnumCargo, int lineIndex, ProduceOrder orderToRemove,
                Dictionary<string, long> __state, bool __runOriginal)
            {
                if (!__runOriginal || __state == null) return;
                try
                {
                    bool removed = !magnumCargo.ItemProduceOrders.TryGetValue(lineIndex, out var orders) ||
                        !orders.Any(order => ReferenceEquals(order, orderToRemove));
                    Debug.Log("[Cvar_ImperiumOfMan] Occult cancellation | line=" + lineIndex +
                        " | output=" + orderToRemove.OrderId + " | removed=" + removed +
                        " | remaining=" + (orders?.Count ?? 0) +
                        " | refund=" + OccultCargoDeltas(magnumCargo, __state,
                            OccultItemCounts(orderToRemove.RequiredItems), 1));
                }
                catch (Exception ex) { OccultAuditUnavailable("cancel after", ex); }
            }
        }

        private sealed class OccultStartTrace
        {
            public Dictionary<string, long> Before;
            public int QueueCount;
        }

        private static OccultStartTrace CaptureOccultStart(MagnumCargo cargo, ItemProduceReceipt receipt, int line)
        {
            try
            {
                return new OccultStartTrace
                {
                    Before = OccultCargoSnapshot(cargo, receipt.RequiredItems.SelectMany(input =>
                        new[] { input.ItemId, ItemProductionSystem.GetCustomId(input.ItemId) })),
                    QueueCount = cargo.ItemProduceOrders.TryGetValue(line, out var queue) ? queue.Count : 0
                };
            }
            catch (Exception ex)
            {
                OccultAuditUnavailable("start before", ex);
                return null;
            }
        }

        // Called after the existing batch/time postfix, so logs describe the
        // actual persistent order rather than the vanilla pre-adjustment count.
        private static void ReportOccultStart(MagnumCargo cargo, ItemProduceReceipt receipt, int line,
            int batches, ProduceOrder order, OccultStartTrace trace)
        {
            if (trace == null) return;
            try
            {
                Debug.Log("[Cvar_ImperiumOfMan] Occult production start | line=" + line +
                    " | recipe=" + receipt.OutputItem + " | output=" + order.OrderId +
                    " | batches=" + batches + " | output_count=" + order.Count +
                    " | hours=" + order.DurationInHours + " | queue_before=" + trace.QueueCount +
                    " | queue_after=" + cargo.ItemProduceOrders[line].Count +
                    " | formula=" + string.Join(",", receipt.RequiredItems.GroupBy(input => input.ItemId)
                        .Select(group => group.Key + ":" + group.Sum(input => (long)input.Count) * batches)) +
                    " | consumed=" + OccultCargoDeltas(cargo, trace.Before,
                        OccultItemCounts(order.RequiredItems), -1));
            }
            catch (Exception ex) { OccultAuditUnavailable("start after", ex); }
        }

        private sealed class OccultCompletionTrace
        {
            public List<(int Line, ProduceOrder Order)> Due;
            public Dictionary<string, long> Before;
        }

        [HarmonyPatch(typeof(ItemProductionSystem), nameof(ItemProductionSystem.Update))]
        private static class TraceOccultCompletion
        {
            private static void Prefix(MagnumCargo magnumCargo, SpaceTime spaceTime,
                out OccultCompletionTrace __state)
            {
                __state = null;
                if (!_industryReady || magnumCargo == null || spaceTime == null) return;
                try
                {
                    // The hot path checks only the three stable occult line IDs.
                    // Do not allocate, scan storage or walk ordinary queues until
                    // at least one occult order is actually due.
                    bool occultDue = false;
                    for (int i = 0; i < OccultLineLimit; i++)
                    {
                        if (magnumCargo.ItemProduceOrders.TryGetValue(OccultLineBase + i, out var queue) &&
                            queue.Count > 0 && queue[0].FinishTime <= spaceTime.Time)
                        {
                            occultDue = true;
                            break;
                        }
                    }
                    if (!occultDue) return;

                    // Include coincident ordinary orders for the same output so
                    // their delivery is not misattributed to occult production.
                    var due = magnumCargo.ItemProduceOrders
                        .Where(entry => entry.Value.Count > 0 && entry.Value[0].FinishTime <= spaceTime.Time)
                        .Select(entry => (Line: entry.Key, Order: entry.Value[0])).ToList();
                    var ids = new HashSet<string>(due.Where(entry => IsOccultLine(entry.Line))
                        .Select(entry => entry.Order.OrderId), StringComparer.Ordinal);
                    due.RemoveAll(entry => !ids.Contains(entry.Order.OrderId));
                    __state = new OccultCompletionTrace { Due = due, Before = OccultCargoSnapshot(magnumCargo, ids) };
                }
                catch (Exception ex) { OccultAuditUnavailable("completion before", ex); }
            }

            private static void Postfix(MagnumCargo magnumCargo, OccultCompletionTrace __state,
                bool __runOriginal)
            {
                if (!__runOriginal || __state == null) return;
                try
                {
                    var completed = __state.Due.Where(entry =>
                        !magnumCargo.ItemProduceOrders.TryGetValue(entry.Line, out var queue) ||
                        !queue.Any(order => ReferenceEquals(order, entry.Order))).ToArray();
                    foreach (var entry in __state.Due.Where(entry => IsOccultLine(entry.Line)))
                    {
                        Debug.Log("[Cvar_ImperiumOfMan] Occult production completion | line=" + entry.Line +
                            " | output=" + entry.Order.OrderId + " | count=" + entry.Order.Count +
                            " | removed=" + completed.Any(done => ReferenceEquals(done.Order, entry.Order)) +
                            " | remaining=" + (magnumCargo.ItemProduceOrders.TryGetValue(entry.Line, out var queue)
                                ? queue.Count : 0));
                    }
                    var expected = completed.GroupBy(entry => entry.Order.OrderId, StringComparer.Ordinal)
                        .ToDictionary(group => group.Key, group => group.Sum(entry => (long)entry.Order.Count),
                            StringComparer.Ordinal);
                    Debug.Log("[Cvar_ImperiumOfMan] Occult production delivery | cargo=" +
                        OccultCargoDeltas(magnumCargo, __state.Before, expected, 1));
                }
                catch (Exception ex) { OccultAuditUnavailable("completion after", ex); }
            }
        }

        private static Dictionary<string, long> OccultItemCounts(IEnumerable<string> ids) =>
            ids.GroupBy(id => id, StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.LongCount(), StringComparer.Ordinal);

        // Include all saved ship cargo, even a temporarily unavailable recycler.
        // The snapshot is observational and never creates or moves an item.
        private static Dictionary<string, long> OccultCargoSnapshot(MagnumCargo cargo, IEnumerable<string> ids)
        {
            var result = ids.Distinct(StringComparer.Ordinal)
                .ToDictionary(id => id, _ => 0L, StringComparer.Ordinal);
            if (result.Count == 0) return result;
            foreach (var storage in cargo.ShipCargo.Concat(new[] { cargo.FridgeStorage, cargo.RecyclingStorage }))
            {
                if (storage == null) continue;
                foreach (var item in storage.Items)
                    if (result.TryGetValue(item.Id, out long count))
                        result[item.Id] = count + item.StackCount;
            }
            return result;
        }

        private static string OccultCargoDeltas(MagnumCargo cargo, Dictionary<string, long> before,
            Dictionary<string, long> expected, int sign)
        {
            var after = OccultCargoSnapshot(cargo, before.Keys);
            return before.Count == 0 && expected.Count == 0 ? "none" :
                string.Join(",", before.Keys.Union(expected.Keys, StringComparer.Ordinal).Select(id =>
                {
                    expected.TryGetValue(id, out long quantity);
                    return id + ":expected=" + (sign * quantity) + "/observed=" +
                        (before.TryGetValue(id, out long prior) && after.TryGetValue(id, out long current)
                            ? (current - prior).ToString() : "unknown");
                }));
        }

        private static void AuditOccultLoadedOrders(MagnumCargo cargo)
        {
            try
            {
                int learned = cargo.UnlockedProductionItems.Distinct(StringComparer.Ordinal)
                    .Count(OccultRecipeRequirements.ContainsKey);
                Debug.Log("[Cvar_ImperiumOfMan] Occult save load | learned_patterns=" + learned +
                    " | active_orders=" + cargo.ItemProduceOrders.Where(entry => IsOccultLine(entry.Key))
                        .Sum(entry => entry.Value.Count));
                foreach (var entry in cargo.ItemProduceOrders.Where(entry => IsOccultLine(entry.Key)))
                    foreach (var order in entry.Value)
                        Debug.Log("[Cvar_ImperiumOfMan] Occult saved order | line=" + entry.Key +
                            " | output=" + order.OrderId + " | count=" + order.Count +
                            " | hours=" + order.DurationInHours + " | start=" + order.StartTime.ToString("O") +
                            " | required=" + string.Join(",", OccultItemCounts(order.RequiredItems)
                                .Select(item => item.Key + ":" + item.Value)));
            }
            catch (Exception ex) { OccultAuditUnavailable("save load", ex); }
        }

        private static void OccultAuditUnavailable(string stage, Exception error) =>
            Debug.LogWarning("[Cvar_ImperiumOfMan] Occult transaction audit unavailable | stage=" +
                stage + " | " + error.Message);
    }
}

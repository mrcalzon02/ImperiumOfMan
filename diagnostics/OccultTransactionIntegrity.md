# Occult transaction integrity and acceptance

Source baseline: `f4351cf1e359e83ab2c27930aee69e7162967232`. Native evidence: [46060522083](https://github.com/mrcalzon02/Dens-Mortis/actions/runs/46060522083), installed `MGSC.ItemProductionSystem`, `MGSC.MagnumCargo` and `MGSC.MagnumCargoSystem` exports. No new recipe, research cost, chip grant, cargo injection or native scheduler.

The native four-argument CancelProduction overload ignores the return value of List.Remove before issuing refunds. A stale order or one belonging to another line can therefore refund its RequiredItems without removal. The occult-only prefix requires the exact order reference in the specified queue before allowing native cancellation. The three-argument active cancellation delegates to this overload. Ordinary lines retain native behavior.

The start audit records the actual post-batch persistent order, actual consumed ingredient IDs, formula, queue occupancy and exact aggregate cargo deltas. Cancel reports exact ingredient refund deltas. The completion observer checks only three occult line heads on idle updates; it snapshots cargo only when an occult order is due, and accounts for simultaneous ordinary deliveries of the same output. The save-load observer reports retained learned-pattern count and exact persistent occult orders. Observers never grant items or modify saves. A delta mismatch is evidence to investigate, not automatic rollback or a claim of success.

## Targeted acceptance cases

| Case | Required evidence |
| --- | --- |
| Learn and install Culture independently | Purchase consumes native research cost; physical chip teaches only its actual pattern. |
| Start a one-batch biomass-only Human Flesh order | Formula 9 biomass; consumed biomass delta -9; saved order count 1; line 10000. |
| Insufficient cargo / full queue | No order or ingredient delta and no false confirmation. |
| Cancel real active and waiting orders | Order removed exactly once; refund matches the saved RequiredItems; surviving queue preserved. |
| Stale/foreign cancellation | No refund or queue mutation; rejection logged. |
| Finish a batch | Removed order and matching output delta; ordinary and other occult orders remain independent. |
| Save/reload with queued work | Same line, order output, count, duration, required items and learned patterns. |

The historical fixture lacked Culture purchase materials. Do not claim any of the above transactions passed until the exact source runs and the corresponding evidence exists. Never use the original occupied campaign slots for destructive testing. No release from source inspection alone.

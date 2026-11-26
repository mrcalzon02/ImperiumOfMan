# Ore Cargo station stock: source and native API audit

The existing Industry markets stocked loose Metal smelting feedstock (`iom_metal_ore`), but did not explicitly replenish the sealed native Ore Cargo item (`ore_cargo`). These are separate records: the ore-cargo disassembly path yields feedstock, silicates and conductors, so stock of the loose material does not satisfy a request to buy the sealed cargo.

## Verified native storage behavior

The [native inspection run](https://github.com/mrcalzon02/Dens-Mortis/actions/runs/45884698405) completed successfully. Its immutable delivery is `1a2aae8cb6f5eb9a19909ed922c433e984db5f7f`, under `runner-results/quasimorph-native/45884698405-1`. The request/controller was `63ffd056e429dbda8588c4b754dadd23cade8150`; it compiled source `e6c3026b3a11e4194b4597866099fcee850d836a` and inspected the installed native game assembly. It did not compile the subsequent market or intermediate-material changes and did not run the game.

The inspected `Assembly-CSharp.dll` SHA-256 is `d72cd91f98de4d6cce8ee420ac1a906d89e68ba0cb164694d266b56f46339ebe`. All four requested market types were read, and each downloaded file's size and Git blob hash matched the immutable delivery:

| Evidence | Bytes | Git blob SHA |
| --- | ---: | --- |
| [MGSC.BarterReceipt.txt](https://github.com/mrcalzon02/Dens-Mortis/blob/1a2aae8cb6f5eb9a19909ed922c433e984db5f7f/runner-results/quasimorph-native/45884698405-1/native-api/MGSC.BarterReceipt.txt) | 806 | `c1aa4dc36f4853020f6f9384c05c25d3a3ee89f0` |
| [MGSC.Station.txt](https://github.com/mrcalzon02/Dens-Mortis/blob/1a2aae8cb6f5eb9a19909ed922c433e984db5f7f/runner-results/quasimorph-native/45884698405-1/native-api/MGSC.Station.txt) | 3109 | `925c3c315e95d9601d00b3ac6472b1e8420e577f` |
| [MGSC.StationBarterRecord.txt](https://github.com/mrcalzon02/Dens-Mortis/blob/1a2aae8cb6f5eb9a19909ed922c433e984db5f7f/runner-results/quasimorph-native/45884698405-1/native-api/MGSC.StationBarterRecord.txt) | 1008 | `ef3a8c9a45863de67bb9b57e4ffdca483ea817fd` |
| [MGSC.StationSystem.txt](https://github.com/mrcalzon02/Dens-Mortis/blob/1a2aae8cb6f5eb9a19909ed922c433e984db5f7f/runner-results/quasimorph-native/45884698405-1/native-api/MGSC.StationSystem.txt) | 32138 | `452613543f15761c4d70d87c34f4a7fc5e773252` |

The native methods establish the following:

- `AddItemToStationStorage` merges or places the supplied item directly in `Station.InternalStorage`. It does not divert consumable ingredients to a separate store or apply a demand-reserve threshold. Storage expands when an item cannot initially fit.
- `GetProduceItemsInStock` returns every item ID present in `InternalStorage`, without excluding `ConsumableItems`.
- `RefreshConsumableItems` derives demand and prices from current station receipts and population requirements. Demand metadata is separate from the stored item count.
- `ProduceItems` later removes the inputs of actual station production from that same storage. Adding stock therefore does not create a permanent reserve against future production or shipping.
- Native `StationsAddItemsTick` uses its existing timed restock cadence. The mod's stock refresh also runs after save load and during fresh-station initialization.

## Targeted source correction

The root-owned market change uses the existing `TopUp` path and introduces the following paid-stock targets:

| Station | Required owner | Minimum faction tech | Ore Cargo stock target |
| --- | --- | ---: | ---: |
| Rachmaninoff | CoreWard | 1 | 3 |
| Cinder Basin Mine | CoreWard | 1 | 2 |
| Foundry Station | Imperium | 1 | 2 |

`TopUp` subtracts the existing `InternalStorage.CountItems(ore_cargo)` from the target, creates only the shortage and sends it through native `AddItemToStationStorage`. Ownership/tier checks and the normal barter transaction remain in force. These are refresh-time minimum stock targets, not additional free player cargo or unlimited stock. No Ore Cargo manufacturing recipe or finished-ration recovery is added by this correction.

## Acceptance boundary

This evidence supports real native station inventory and inclusion in the stock summary. The inspected four types do not include the barter screen's final sale-item selection. A live Ore Cargo listing and a paid transfer to the player's cargo still require the headed acceptance check. No visual purchase, completed trade or durable stock guarantee is claimed here.

[Native build receipt](https://github.com/mrcalzon02/Dens-Mortis/blob/1a2aae8cb6f5eb9a19909ed922c433e984db5f7f/runner-results/quasimorph-native/45884698405-1/build-receipt.json) · [Native inspection receipt](https://github.com/mrcalzon02/Dens-Mortis/blob/1a2aae8cb6f5eb9a19909ed922c433e984db5f7f/runner-results/quasimorph-native/45884698405-1/native-api-receipt.json)


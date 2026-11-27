using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MGSC;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private sealed class IndustryMarket(string faction, ItemQuantity[] stock, string[] salvage,
            int minimumTier = 1, ItemQuantity[] tieredStock = null)
        {
            internal readonly string Faction = faction;
            internal readonly ItemQuantity[] Stock = stock;
            internal readonly string[] Salvage = salvage;
            internal readonly int MinimumTier = minimumTier;
            internal readonly ItemQuantity[] TieredStock = tieredStock ?? [];
        }

        private static readonly Dictionary<string, IndustryMarket> IndustryMarkets = new()
        {
            [PhobosStation] = new(ItemCategory, [new(Cell, 3), new(Housing, 3), new(CellStock, 4), new(Silicate, 6),
                new("acid", 4), new("wire", 4)],
                [Cell, Housing, CellStock], tieredStock: [new(BoltClip, 24), new(BoltPistol, 1), new(HellfireClip, 8), new(Gland, 1)]),
            [HavocStation] = new(ItemCategory, [new(Ore, 8), new(Metal, 4), new(PolymerStock, 2), new(Flak, 3),
                new(Liner, 4), new(Fittings, 4)], [Flak, Liner, Fittings, LegionSeal, AutomataChassis],
                tieredStock: [new(KrakenClip, 8), new(KrakGrenade, 1), new(BoltRifle, 1)]),
            [FoundryStation] = new(ItemCategory, [new(OreCargo, 2), new(Ore, 8), new(Metal, 5), new(Sheet, 3), new(Fittings, 3)],
                [Metal, Sheet, Fittings, LogicCore], tieredStock: [new(AntiphasicClip, 8), new(Underplate, 1)]),
            [NeithVault] = new(ItemCategory, [new(Biomass, 6), new("water_bottle_1", 5)], [Biomass, Treatment]),
            [TerraStation] = new(ItemCategory, [new(Sheet, 4), new(Textile, 2), new(Plate, 3), new(Liner, 4),
                new(Fittings, 4)], [Plate, Liner, Fittings, Dressing, Tray],
                tieredStock: [new("iom_arbites_weapon_chip", 1), new(DropPlans, 1), new(SiegePlans, 1), new(ScionPlans, 1),
                    new(MetalStormClip, 8), new(SkitariiPlans, 1), new(SororitasPlans, 1), new(InfernoClip, 8),
                    new(MeltaGrenade, 1), new(XenosPlans, 1), new(PaladinPlans, 1), new(GoldenPlans, 1)]),
            [TitanFortress] = new(ItemCategory, [new(Sheet, 4), new("scientific_equipment", 1), new(HeavyPlate, 2),
                new("tech_repair_kit", 1)], [HeavyPlate, Plate], tieredStock: [new(TempestClip, 8), new(PowerFist, 1)]),
            [BroadswordStation] = new(ItemCategory, [new(Metal, 4), new("electrical_parts_container", 1),
                new("firearm_repair_kit", 2), new("armor_repair_kit", 2)], [Metal, Fittings]),
            [LunaStation] = new(ItemCategory, [new(Biomass, 4), new(RationBase, 3), new(Wrapper, 4)],
                [RationBase, Wrapper, DirtyStarch, WinterInsulation], tieredStock: [new(HelfrostClip, 8)]),
            ["Akatsuki"] = new("AnCom", [new(Case, 4), new(Module, 4)], [Case, Module, Shell, Flechette],
                tieredStock: [new(Shell, 2), new(Flechette, 2)]),
            ["Clotho"] = new("RealWare", [new(Metal, 5), new(Sheet, 4), new(Plate, 3), new(Liner, 3)], [Sheet, Plate, Liner]),
            ["Paragon"] = new("DaydreamChems", [new(Leaf, 4), new(Loose, 2), new(Carton, 2), new(Dressing, 3),
                new(Treatment, 3), new(Tray, 2), new("cigarettes_1", 4), new("medical_kit_1", 2)], [Loose, Carton, Dressing, Treatment, Tray]),
            [AvicennasClinic] = new(DaydreamChems, [new(Clinical, 2), new(Instruments, 2), new("medical_kit_2", 1),
                new("surgical_kit", 1)], [Clinical, Instruments], minimumTier: 2, tieredStock: [new(Graft, 1)]),
            // Native mining demand alone does not guarantee a crate remains for sale.
            // These are ordinary paid station goods, replenished by the existing
            // new-campaign, save-load and station-restock paths without changing prices.
            ["Rachmaninoff"] = new("CoreWard", [new(OreCargo, 3), new(Ore, 12), new(Silicate, 12)], [Ore, Silicate]),
            [CinderBasinMine] = new("CoreWard", [new(OreCargo, 2)], [Ore, Silicate]),
            ["Mammoth"] = new("PlanetBridge", [new(MetalScrap, 6), new(Fiber, 6), new(Polymer, 6),
                new(Cullet, 6), new(Conductors, 4)], [MetalScrap, Fiber, Polymer, Cullet, Conductors])
        };

        [Hook(ModHookType.AfterSaveLoaded)]
        public static void RefreshIndustryOnLoad(IModContext context)
        {
            if (!_industryReady)
            {
                return;
            }
            var state = context.State;
            if (state.Get<MagnumCargo>() is MagnumCargo cargo)
            {
                cargo.UnlockedProductionItems.RemoveAll(id => id == FurySigilSkull);
                if (state.Get<SpaceTime>() is SpaceTime spaceTime)
                {
                    // Old saves can still have an order from the retired recipe. Native
                    // cancellation refunds its ingredients and preserves the other jobs.
                    foreach (var line in cargo.ItemProduceOrders.ToArray())
                    {
                        foreach (var order in line.Value.Where(order => order.OrderId == FurySigilSkull).ToArray())
                        {
                            ItemProductionSystem.CancelProduction(cargo, spaceTime, line.Key, order);
                        }
                    }
                }
                LearnRecipes(cargo, SharedIndustryRecipes);
            }
            RefreshIndustryMarkets(state.Get<Stations>(), state.Get<Factions>(), state.Get<SpaceTime>());
        }

        private static void RefreshIndustryMarkets(Stations stations, Factions factions, SpaceTime time)
        {
            if (!_industryReady || stations == null || factions == null || time == null)
            {
                return;
            }
            foreach (var entry in CatalogStock)
            {
                var maker = factions.Get(entry.Key, logMissing: false);
                bool makerAvailable = maker?.Record.CanBeTraded == true && factions.IsEnabledFaction(maker);
                var faction = makerAvailable ? maker : factions.Get("UnchainedBelt", logMissing: false);
                if (faction?.Record.CanBeTraded != true || !factions.IsEnabledFaction(faction))
                {
                    continue;
                }
                var fallback = stations.Values.FirstOrDefault(station => station.OwnerFactionId == faction.Id &&
                    !string.IsNullOrEmpty(station.SpaceObjectId));
                foreach (var (chip, tier, supplier) in entry.Value)
                {
                    // Faction-specific occult knowledge does not silently become
                    // generic pirate stock when its maker is absent or disabled.
                    if (!makerAvailable && IsOccultPatternChip(chip)) continue;
                    if (tier > faction.CurrentTechLevel)
                    {
                        continue;
                    }
                    var station = supplier == null ? fallback : stations.Get(supplier, logMissing: false);
                    if (station?.OwnerFactionId != faction.Id || string.IsNullOrEmpty(station.SpaceObjectId))
                    {
                        station = fallback;
                    }
                    if (station != null)
                    {
                        if (IsOccultPatternChip(chip))
                            TopUpRegionalStock(station, chip, 1, "pattern");
                        else
                            TopUp(station, chip, 1);
                    }
                }
            }

            // Regional prototype feedstock is also available through ordinary paid
            // faction trade. Match the native reward tiers and provide at most one
            // first-stage batch of tissue per restock. No pirate fallback, free
            // cargo grant, recipe unlock, finished ammunition or new drop table.
            if (ActiveRegionalAmmoChains.Contains(IgvasAmmo))
            {
                TopUpRegionalSeed(TezctlanFaction, null, 2, "venus_guts", 1);
                TopUpRegionalSeed(TezctlanFaction, null, 2, "quasi_venus_feather", 1);
            }
            if (ActiveRegionalAmmoChains.Contains(NailsOfPainAmmo))
                TopUpRegionalSeed(ChaosFaction, "iomOssuaryWorks", 3, "moon_guts", 2);

            var chaos = factions.Get(ChaosFaction, logMissing: false);
            if (chaos?.Record.CanBeTraded == true && factions.IsEnabledFaction(chaos) && chaos.CurrentTechLevel >= 1)
            {
                // Keep the sigil in paid station stock, outside the shared contract-reward
                // and raid-loot tables. Captured stations no longer receive it.
                foreach (Station station in stations.Values.Where(station => station.OwnerFactionId == ChaosFaction &&
                    !string.IsNullOrEmpty(station.SpaceObjectId)))
                {
                    TopUp(station, FurySigilSkull, 1);
                }
            }
            foreach (var entry in IndustryMarkets)
            {
                Station station = stations.Get(entry.Key, logMissing: false);
                IndustryMarket market = entry.Value;
                if (station?.OwnerFactionId != market.Faction ||
                    factions.Get(market.Faction).CurrentTechLevel < market.MinimumTier)
                {
                    continue;
                }
                foreach (ItemQuantity item in market.Stock)
                {
                    TopUp(station, item.ItemId, item.Count);
                }
                int tier = factions.Get(market.Faction).CurrentTechLevel;
                foreach (ItemQuantity item in market.TieredStock)
                {
                    if (Data.Items.GetSimpleRecord<ItemRecord>(item.ItemId).TechLevel <= tier)
                    {
                        TopUp(station, item.ItemId, item.Count);
                    }
                }
                if (market.Faction == ItemCategory && SupportMarkets.TryGetValue(station.Id, out var supportStock))
                {
                    foreach (var (minimumTier, supply) in supportStock)
                    {
                        if (minimumTier <= tier)
                        {
                            TopUp(station, supply.ItemId, supply.Count);
                        }
                    }
                }
                if (station.Id == TerraStation && tier >= ArbitesTier)
                {
                    TopUp(station, HeavyPlate, 2);
                }
                if (station.Id == LunaStation && tier >= ValhallanTier)
                {
                    TopUp(station, WinterInsulation, 3);
                    TopUp(station, Tanna, 4);
                }
                if (station.Id == HavocStation && tier >= SteelLegionTier)
                {
                    TopUp(station, LegionSeal, 4);
                    TopUp(station, LegionFilter, 3);
                    TopUp(station, LegionSmoke, 2);
                }
            }

            void TopUpRegionalSeed(string factionId, string preferredStation, int tier, string id, int target)
            {
                var owner = factions.Get(factionId, logMissing: false);
                if (owner?.Record.CanBeTraded != true || !factions.IsEnabledFaction(owner) ||
                    owner.CurrentTechLevel < tier || Data.Items.GetRecord(id) == null)
                    return;

                bool IsOwnedSupplier(Station candidate) => candidate?.OwnerFactionId == owner.Id &&
                    !string.IsNullOrEmpty(candidate.SpaceObjectId);
                var station = preferredStation == null ? null : stations.Get(preferredStation, logMissing: false);
                if (!IsOwnedSupplier(station))
                    station = stations.Values.FirstOrDefault(IsOwnedSupplier);
                if (station != null)
                    TopUpRegionalStock(station, id, target, "seed");
            }

            void TopUpRegionalStock(Station station, string id, int target, string kind)
            {
                int before = station.InternalStorage.CountItems(id);
                TopUp(station, id, target);
                int after = station.InternalStorage.CountItems(id);
                if (after > before)
                    UnityEngine.Debug.Log("[Cvar_ImperiumOfMan] Regional occult " + kind +
                        " restock | item=" + id + " | faction=" + station.OwnerFactionId +
                        " | station=" + station.Id + " | before=" + before + " | after=" + after);
            }

            void TopUp(Station station, string id, int target)
            {
                int shortage = target - station.InternalStorage.CountItems(id);
                if (shortage <= 0)
                {
                    return;
                }
                var items = new List<BasePickupItem>();
                ItemInteractionSystem.CreateItem(items, id, shortage);
                foreach (BasePickupItem item in items)
                {
                    StationSystem.AddItemToStationStorage(time, factions, station, item);
                }
            }
        }

        [HarmonyPatch(typeof(StationSystem), nameof(StationSystem.Create))]
        private static class IndustryStartingStock
        {
            private static void Postfix(Stations __result, bool initContent, Factions factions, SpaceTime spaceTime)
            {
                if (initContent)
                {
                    RefreshIndustryMarkets(__result, factions, spaceTime);
                }
            }
        }

        [HarmonyPatch(typeof(StationSystem), nameof(StationSystem.StationsAddItemsTick))]
        private static class IndustryMarketRestock
        {
            private static void Prefix(SpaceTickTimers spaceTickTimers, out DateTime __state)
            {
                __state = spaceTickTimers.WasStationsItemsAdded ? spaceTickTimers.LastAddedItemsToStations : DateTime.MinValue;
            }

            private static void Postfix(Stations stations, Factions factions, SpaceTime spaceTime,
                SpaceTickTimers spaceTickTimers, DateTime __state)
            {
                if (spaceTickTimers.LastAddedItemsToStations != __state)
                {
                    RefreshIndustryMarkets(stations, factions, spaceTime);
                }
            }
        }

        // Use existing supply containers on fresh procedural floors. Loading the floor does not run Populate.
        [HarmonyPatch(typeof(DungeonBuilder), "Populate")]
        private static class IndustryFacilitySalvage
        {
            private static void Postfix(State ____state)
            {
                if (!_industryReady)
                {
                    return;
                }
                var raid = ____state.Get<RaidMetadata>();
                var location = ____state.Get<LocationMetadata>();
                if (raid.RaidType != RaidType.ProcMission || location.LocationId != "stage1" ||
                    string.IsNullOrEmpty(raid.StationId) || !IndustryMarkets.TryGetValue(raid.StationId, out var market) ||
                    ____state.Get<Stations>().Get(raid.StationId, logMissing: false)?.OwnerFactionId != market.Faction)
                {
                    return;
                }
                int tier = ____state.Get<Factions>().Get(market.Faction).CurrentTechLevel;
                if (tier < market.MinimumTier)
                {
                    return;
                }
                var stores = ____state.Get<MapObstacles>().Obstacles
                    .Where(obstacle => obstacle.QuestStorage == null && obstacle.Store != null)
                    .Select(obstacle => obstacle.Store)
                    .Where(store => store.UseForSpawnItems && store.HasAccess())
                    .ToList();
                var random = new Random(location.Seed);
                foreach (var itemRecord in market.Salvage.Select(id => Data.Items.GetSimpleRecord<ItemRecord>(id))
                    .Where(item => item.TechLevel <= tier).OrderBy(_ => random.Next()).Take(2))
                {
                    var compatible = stores.Where(store => store.AllowedItemClasses.Contains(itemRecord.ItemClass))
                        .OrderBy(_ => random.Next()).ToList();
                    if (compatible.Count > 0)
                    {
                        var item = SingletonMonoBehaviour<ItemFactory>.Instance.CreateForInventory(itemRecord.Id);
                        foreach (Store store in compatible)
                        {
                            if (store.storage.TryPutItem(item, CellPosition.Zero))
                            {
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}

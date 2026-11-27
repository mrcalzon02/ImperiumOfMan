using System;
using System.Collections.Generic;
using System.Linq;
using MGSC;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private sealed class CraftingAcquisition
        {
            public string ItemId;
            public string Caption;
            public string[] Details;
        }

        private sealed class OccultIssuerSnapshot
        {
            public bool Enabled;
            public bool Tradable;
            public int Tech;
            public int ReputationCap;
            public int Stations;
            public readonly HashSet<string> Chips = new(StringComparer.Ordinal);
            public readonly HashSet<string> Samples = new(StringComparer.Ordinal);
        }

        // Snapshot only on index capture; count the saved selection of a
        // physical chip, not all copies of the same randomized pool ID.
        private static void CaptureOccultAcquisition(CraftingIndex index, MagnumCargo cargo,
            Stations stations, Factions factions)
        {
            var specimens = new HashSet<string>(OccultResearchSamples.Select(sample => sample.Item),
                StringComparer.Ordinal);
            var issuers = OccultPatternIssuers.Values.Select(issuer => issuer.Faction)
                .Concat(OccultResearchSamples.Select(sample => sample.Faction)).Distinct(StringComparer.Ordinal);
            var factionInfo = new Dictionary<string, OccultIssuerSnapshot>(StringComparer.Ordinal);
            var markets = new Dictionary<(string Item, string Selection), Dictionary<string, long>>();
            var stationNames = new Dictionary<string, (string Owner, string Name)>(StringComparer.Ordinal);
            var ship = new Dictionary<(string Item, string Selection), long>();

            if (stations != null)
            {
                foreach (var station in stations.Values)
                {
                    if (station == null || string.IsNullOrEmpty(station.Id) ||
                        string.IsNullOrEmpty(station.SpaceObjectId) || station.InternalStorage == null)
                        continue;
                    stationNames[station.Id] = (station.OwnerFactionId, CraftingStationName(station.Id));
                    foreach (var item in station.InternalStorage.Items)
                    {
                        var key = StockKey(item);
                        if (key == null || item.StackCount <= 0) continue;
                        if (!markets.TryGetValue(key.Value, out var locations))
                            markets.Add(key.Value, locations = new Dictionary<string, long>(StringComparer.Ordinal));
                        locations.TryGetValue(station.Id, out long count);
                        locations[station.Id] = count + item.StackCount;
                    }
                }
            }

            if (cargo != null)
                foreach (var storage in MagnumCargoSystem.GetAvailableCargoStorages(cargo))
                    foreach (var item in storage.Items)
                    {
                        var key = StockKey(item);
                        if (key == null || item.StackCount <= 0) continue;
                        ship.TryGetValue(key.Value, out long count);
                        ship[key.Value] = count + item.StackCount;
                    }

            if (factions != null)
                foreach (string id in issuers)
                {
                    var owner = factions.Get(id, logMissing: false);
                    if (owner == null) continue;
                    var state = new OccultIssuerSnapshot
                    {
                        Enabled = factions.IsEnabledFaction(owner),
                        Tradable = owner.Record?.CanBeTraded == true,
                        Tech = owner.CurrentTechLevel,
                        ReputationCap = Data.FactionDrop.GetTechLevelLimit(owner),
                        Stations = stationNames.Values.Count(station => station.Owner == id)
                    };
                    factionInfo[id] = state;
                    if (!state.Enabled) continue;
                    foreach (var reward in Data.FactionDrop.GetTradeItems(owner, FactionDropCollection.TradeItemsFor.Chips))
                        state.Chips.UnionWith(reward.ContentIds);
                    foreach (var reward in Data.FactionDrop.GetTradeItems(owner, FactionDropCollection.TradeItemsFor.Consumables))
                        state.Samples.UnionWith(reward.ContentIds);
                }

            foreach (var output in OccultPatternSources)
                foreach (string chip in output.Value.Distinct(StringComparer.Ordinal))
                    if (OccultPatternIssuers.TryGetValue(chip, out var issuer))
                        Add(output.Key, chip, issuer.Faction, issuer.Tier, true, 0);
            foreach (var sample in OccultResearchSamples)
                Add(sample.Item, sample.Item, sample.Faction, sample.Tech, false, sample.Rank);

            (string Item, string Selection)? StockKey(BasePickupItem item)
            {
                if (item == null || string.IsNullOrEmpty(item.Id)) return null;
                if (IsTrackedOccultPattern(item.Id))
                {
                    string selection = item.Comp<DatadiskComponent>()?.UnlockId;
                    return string.IsNullOrEmpty(selection) ? null : (item.Id, selection);
                }
                return specimens.Contains(item.Id) ? (item.Id, (string)null) : null;
            }

            void Add(string target, string itemId, string faction, int tier, bool pattern, int rank)
            {
                if (!index.Items.TryGetValue(itemId, out var item) || !item.Registered ||
                    !index.Items.ContainsKey(target)) return;
                string issuer = OccultIssuerName(faction);
                var detail = new List<string> { "Issuer: " + issuer + " · Tech " + tier };
                if (pattern)
                {
                    var pool = Data.Items.GetSimpleRecord<DatadiskRecord>(itemId)?.UnlockIds;
                    detail.Add("Pattern: " + CraftingItemName(target));
                    detail.Add(pool == null || !pool.Contains(target)
                        ? "Current chip pool does not list this pattern"
                        : pool.Count == 1 ? "Chip contains this pattern"
                        : "Chip selects 1 of " + pool.Count + " recipes; inspect before buying");
                }
                else
                    detail.Add("Research specimen · " + OccultResearchTitle(OccultRanks[rank - 1]));

                if (factions == null)
                    detail.Add("Faction and reward access: not captured");
                else if (!factionInfo.TryGetValue(faction, out var access))
                    detail.Add("Issuer absent from this campaign");
                else if (!access.Enabled)
                    detail.Add("Issuer inactive in this campaign");
                else
                {
                    detail.Add("Current faction tech: " + access.Tech + " / required " + tier);
                    detail.Add("Reputation reward cap: Tech " + access.ReputationCap);
                    detail.Add(stations == null ? "Station ownership: not captured" :
                        (access.Tradable ? "Trade enabled" : "Trade disabled") +
                        " · " + access.Stations + " owned stations");
                    bool eligible = (pattern ? access.Chips : access.Samples).Contains(itemId);
                    detail.Add(eligible ? "Contract reward pool: eligible (random)" :
                        "Contract reward pool: not currently eligible");
                }

                var key = (itemId, pattern ? target : null);
                if (cargo == null)
                    detail.Add("Ship cargo: not captured");
                else
                {
                    ship.TryGetValue(key, out long count);
                    detail.Add("Available ship cargo: " + count + (pattern ? " matching chips" : " specimens"));
                }

                if (stations == null)
                    detail.Add("Station storage: not captured");
                else
                {
                    bool found = false;
                    if (markets.TryGetValue(key, out var stock))
                        foreach (var location in stock.OrderBy(pair => stationNames[pair.Key].Name,
                            StringComparer.OrdinalIgnoreCase))
                        {
                            var station = stationNames[location.Key];
                            if (!pattern && station.Owner != faction) continue;
                            found = true;
                            detail.Add("Stock: " + station.Name + " · " + location.Value +
                                (station.Owner != faction ? " (other owner)" : ""));
                        }
                    if (!found)
                        detail.Add(pattern ? "No matching chip selection in station storage" :
                            "No specimen stock at issuer stations");
                }

                if (!index.Acquisition.TryGetValue(target, out var sources))
                    index.Acquisition.Add(target, sources = new List<CraftingAcquisition>());
                sources.Add(new CraftingAcquisition
                {
                    ItemId = itemId,
                    Caption = pattern ? item.Name + " · " + issuer : "Specimen · " + issuer,
                    Details = detail.ToArray()
                });
            }
        }
    }
}

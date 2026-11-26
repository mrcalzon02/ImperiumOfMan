using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MGSC;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private static bool HasCustomShipUpgradeTooltip(string id) =>
            id is SecurityIntrusion or EntranceOverride or ExtractionOverride or
                BarrageDuration or BarrageRounds or BarrageCalls or
                SupplyDrop or TroopDrop or PodCapacity or PodCalls or
                CloneVat or ContainmentCradle or EssenceStill or GavvakhRetort ||
            ShipMounts.Any(mount => mount.Id == id) || TooltipTroopTier(id) > 0 || OccultRanks.Contains(id) ||
            TranslationPatch.HasUpgradeTooltipDescription(id);

        private static int TooltipTroopTier(string id)
        {
            const string prefix = "iom_drop_troops_";
            return id != null && id.StartsWith(prefix, StringComparison.Ordinal) &&
                int.TryParse(id.Substring(prefix.Length), out int tier) && tier is >= 2 and <= 11 &&
                id == TroopTier(tier) ? tier : 0;
        }

        private static ShipUpgradeTooltipRow UpgradeRow(string label, string value) =>
            new(TranslationPatch.UpgradeText(label), value);

        private static string UpgradeNumber(int number) => number.ToString(CultureInfo.InvariantCulture);

        private static string UpgradeCount(int number, string unit) =>
            UpgradeNumber(number) + " " + TranslationPatch.UpgradeText(number == 1 ? unit : unit + "s");

        private static string UpgradeName(string id, string fallback)
        {
            string tag = "mgperk." + id + ".name";
            return Localization.HasKey(tag) ? Localization.Get(tag) : fallback;
        }

        private static string UpgradeItemName(string id, string fallback)
        {
            string tag = "item." + id + ".name";
            return Localization.HasKey(tag) ? Localization.Get(tag) : fallback;
        }

        private static IEnumerable<ShipUpgradeTooltipRow> ShipUpgradeTooltipRows(string id,
            MagnumProgression progression)
        {
            // Native "default values" previews pass no progression. Normal hovers
            // show the fitted ship's totals. Incremental upgrades always state the
            // added amount, even after purchase, rather than promising it twice.
            int fireCalls = 1 + Bonus(BarrageCalls);
            int podCalls = 1 + Bonus(PodCalls);
            int firingTurns = 1 + Bonus(BarrageDuration);
            int troopCount = 2 + Bonus(PodCapacity);

            if (id is SecurityIntrusion or EntranceOverride or ExtractionOverride)
            {
                yield return UpgradeRow("turrets", "+1");
                yield return UpgradeRow("turret_type", TranslationPatch.UpgradeText("ballistic"));
                yield return UpgradeRow("position", TranslationPatch.UpgradeText(id == SecurityIntrusion
                    ? "random" : id == EntranceOverride ? "entrance" : "extraction"));
                yield return UpgradeRow("deployment", TranslationPatch.UpgradeText("arrival"));
                yield return UpgradeRow("frequency", TranslationPatch.UpgradeText("once_per_raid"));
                yield return UpgradeRow("placement", TranslationPatch.UpgradeText("clear_tile"));
                if (id != SecurityIntrusion)
                {
                    yield return UpgradeRow("requires", id == EntranceOverride
                        ? UpgradeName(SecurityIntrusion, "Security Intrusion")
                        : UpgradeName(EntranceOverride, "Entrance Override"));
                }
                yield break;
            }

            var mount = ShipMounts.FirstOrDefault(candidate => candidate.Id == id);
            if (mount != null)
            {
                // Use the same mount definition and salvo calculation as the raid.
                yield return UpgradeRow("damage", UpgradeNumber(mount.Damage));
                yield return UpgradeRow("damage_type", TranslationPatch.UpgradeText(
                    mount.DamageType == "beam" ? "beam" : "explosive"));
                yield return mount.Radius == 0
                    ? UpgradeRow("pattern", TranslationPatch.UpgradeText("line"))
                    : UpgradeRow("blast_radius", UpgradeCount(mount.Radius, "tile"));
                yield return UpgradeRow("impacts", UpgradeNumber(progression == null
                    ? mount.Rounds : SalvoSize(progression, mount)));
                yield return UpgradeRow("firing_turns", UpgradeNumber(firingTurns));
                yield return UpgradeRow("delay", UpgradeCount(Array.IndexOf(ShipMounts, mount) == 1 ? 2 : 1, "turn"));
                yield return UpgradeRow("calls", UpgradeNumber(fireCalls));
                yield return UpgradeRow("self_damage", TranslationPatch.UpgradeText("yes"));
                yield break;
            }

            if (id is CloneVat or ContainmentCradle or EssenceStill or GavvakhRetort)
            {
                foreach (var row in OccultUpgradeTooltipRows(id))
                {
                    yield return row;
                }
                yield break;
            }

            int researchRank = Array.IndexOf(OccultRanks, id);
            if (researchRank >= 0)
            {
                foreach (var row in OccultResearchTooltipRows(id, researchRank))
                {
                    yield return row;
                }
                yield break;
            }

            switch (id)
            {
                case BarrageDuration:
                    yield return UpgradeRow("firing_turns", "+1");
                    yield return UpgradeRow("total_turns", "2");
                    yield return UpgradeRow("requires", TranslationPatch.UpgradeText("fitted_mount"));
                    yield break;
                case BarrageRounds:
                    yield return UpgradeRow("impacts", "+1");
                    yield return UpgradeRow("gun_total", UpgradeNumber(ShipMounts[0].Rounds + 1));
                    yield return UpgradeRow("missile_total", UpgradeNumber(ShipMounts[1].Rounds + 1));
                    yield return UpgradeRow("laser_total", UpgradeNumber(ShipMounts[2].Rounds + 1));
                    yield return UpgradeRow("requires", TranslationPatch.UpgradeText("fitted_mount"));
                    yield break;
                case BarrageCalls:
                    yield return UpgradeRow("calls", "+1");
                    yield return UpgradeRow("total_calls", "2");
                    yield return UpgradeRow("requires", TranslationPatch.UpgradeText("fitted_mount"));
                    yield break;
                case SupplyDrop:
                    yield return new ShipUpgradeTooltipRow(UpgradeItemName(LasClip, "Las Clip"), "12");
                    yield return new ShipUpgradeTooltipRow(UpgradeItemName("rifle_basic_ammo", "Rifle ammo"), "8");
                    yield return new ShipUpgradeTooltipRow(UpgradeItemName("medical_glue", "Medical glue"), "2");
                    yield return UpgradeRow("calls", UpgradeNumber(podCalls));
                    yield return UpgradeRow("shared_reserve", TranslationPatch.UpgradeText("supply_and_troops"));
                    yield return UpgradeRow("landing_tiles", "1");
                    yield break;
                case TroopDrop:
                    yield return UpgradeRow("roster", DropSquads[0].Name);
                    yield return UpgradeRow("troops", UpgradeNumber(troopCount));
                    yield return UpgradeRow("equipment", TranslationPatch.UpgradeText("clone_equipment"));
                    yield return UpgradeRow("calls", UpgradeNumber(podCalls));
                    yield return UpgradeRow("shared_reserve", TranslationPatch.UpgradeText("supply_and_troops"));
                    yield return UpgradeRow("landing_tiles", UpgradeNumber(troopCount));
                    yield break;
                case PodCapacity:
                    yield return UpgradeRow("troops", "+1");
                    yield return UpgradeRow("total_troops", "3");
                    yield return UpgradeRow("applies_to", TranslationPatch.UpgradeText("all_rosters"));
                    yield return UpgradeRow("landing_tiles", "3");
                    yield break;
                case PodCalls:
                    yield return UpgradeRow("calls", "+1");
                    yield return UpgradeRow("total_calls", "2");
                    yield return UpgradeRow("shared_reserve", TranslationPatch.UpgradeText("supply_and_troops"));
                    yield break;
            }

            int tier = TooltipTroopTier(id);
            if (tier == 0)
            {
                // Newly registered upgrades retain their explanation in a native
                // wrapping body until they receive dedicated quantitative rows.
                if (TranslationPatch.TryGetUpgradeTooltipDescription(id, out string description))
                {
                    yield return new ShipUpgradeTooltipRow(description, null);
                }
                yield break;
            }
            foreach (var squad in DropSquads.Where(squad => squad.Tier == tier))
            {
                // A separate native line for every choice avoids the tier-three and
                // tier-ten rosters overflowing the upgrade's name or subtitle.
                yield return UpgradeRow("unlocks", squad.Name);
            }
            yield return UpgradeRow("imperial_tech", UpgradeNumber(tier));
            yield return UpgradeRow("troops", UpgradeNumber(troopCount));
            yield return UpgradeRow("requires", UpgradeName(TroopTier(tier - 1), tier == 2
                ? "Mercenary Pod" : TranslationPatch.UpgradeText("troop_roster") + " " + UpgradeNumber(tier - 1)));

            int Bonus(string perk) => progression?.IsPerkPurchased(perk) == true ? 1 : 0;
        }

        private static IEnumerable<ShipUpgradeTooltipRow> OccultUpgradeTooltipRows(string id)
        {
            string output = id switch
            {
                CloneVat => RawClone,
                ContainmentCradle => TormentImprint,
                EssenceStill => SufferingEssence,
                _ => NativeGavvakh
            };
            string outputName = id switch
            {
                CloneVat => "Raw Clone",
                ContainmentCradle => "Torment Imprint",
                EssenceStill => "Essence of Suffering",
                _ => "Gavvakh"
            };

            // Read the registered order, including the mod's effective batch size.
            // The production registration can deliberately skip a conflicting or
            // unavailable recipe; never advertise an unlock that did not register.
            var receipt = OccultRecipeRequirements.TryGetValue(output, out string requirement) && requirement == id
                ? Data.ProduceReceipts.FirstOrDefault(recipe => recipe.Id == output && recipe.OutputItem == output)
                : null;
            if (receipt == null)
            {
                yield return UpgradeRow("production", TranslationPatch.UpgradeText("unavailable"));
                yield break;
            }
            yield return UpgradeRow("produces", UpgradeItemName(output, outputName));
            yield return UpgradeRow("output_batch", UpgradeNumber(
                IndustryBatches.TryGetValue(output, out int batch) ? batch : 1));
            yield return UpgradeRow("time_batch", receipt.ProduceTimeInHours.ToString("0.##", CultureInfo.InvariantCulture));
            if (receipt.RequiredItems == null || receipt.RequiredItems.Count == 0)
            {
                yield return UpgradeRow("inputs", TranslationPatch.UpgradeText("none"));
            }
            else
            {
                foreach (var ingredient in receipt.RequiredItems)
                {
                    string ingredientName = ingredient.ItemId == RawClone ? "Raw Clone"
                        : ingredient.ItemId == TormentImprint ? "Torment Imprint" : "Essence of Suffering";
                    yield return UpgradeRow("consumes", UpgradeItemName(ingredient.ItemId, ingredientName));
                    yield return UpgradeRow("input_batch", UpgradeNumber(ingredient.Count));
                }
            }
            if (id != CloneVat)
            {
                yield return UpgradeRow("requires", id == ContainmentCradle ? UpgradeName(CloneVat, "Clone Vat")
                    : id == EssenceStill ? UpgradeName(ContainmentCradle, "Containment Cradle")
                    : UpgradeName(EssenceStill, "Essence Still"));
            }
        }

        private static IEnumerable<ShipUpgradeTooltipRow> OccultResearchTooltipRows(string id, int rankIndex)
        {
            // Count only real, registered outputs, including recipes taught by the
            // Chaos catalog. A skipped native recipe must not inflate the benefit.
            var recipes = new HashSet<string>(Data.ProduceReceipts.Select(recipe => recipe.OutputItem),
                StringComparer.Ordinal);
            var taught = new HashSet<string>(OccultRecipeRequirements.Where(entry => entry.Value == id)
                .Select(entry => entry.Key).Where(recipes.Contains), StringComparer.Ordinal);
            var chaos = OccultResearchUnlocks.Where(entry => entry.Value == id)
                .Select(entry => entry.Key).Where(recipes.Contains).ToArray();
            taught.UnionWith(chaos);
            yield return UpgradeRow("recipe_unlocks", "+" + UpgradeNumber(taught.Count));
            yield return UpgradeRow("chaos_designs", UpgradeNumber(chaos.Length));
            yield return UpgradeRow("design_tiers", UpgradeNumber(rankIndex * 2 + 1) + "–" +
                UpgradeNumber(rankIndex * 2 + 2));

            // Compact families identify the bundle; the existing Crafting Map holds
            // the complete item-by-item recipes, quantities and prerequisite links.
            string[] traditions = rankIndex switch
            {
                0 => ["Agga", "Gannix", "Shartamakhum"],
                1 => ["Lunar", "Ron", "Scrivnus"],
                2 => ["Ur", "Flauros", "Cattaram"],
                _ => []
            };
            foreach (string tradition in traditions)
            {
                yield return UpgradeRow("tradition", tradition);
            }
            if (rankIndex >= 3)
            {
                yield return UpgradeRow("research", TranslationPatch.UpgradeText(
                    rankIndex == 3 ? "grafts_weapons" : "advanced_gear"));
            }
            string parent = rankIndex == 0 ? GavvakhRetort : OccultRanks[rankIndex - 1];
            yield return UpgradeRow("requires", UpgradeName(parent, OccultResearchTitle(parent)));
            yield return UpgradeRow("recipe_details", TranslationPatch.UpgradeText("crafting_map"));
        }
    }
}

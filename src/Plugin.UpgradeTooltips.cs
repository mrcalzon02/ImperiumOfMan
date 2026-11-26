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
            // Occult capability rows have one dedicated supplier in OccultUi.
            !OccultRanks.Contains(id) &&
            (TryGetCommsUpgrade(id, out _, out _) ||
            id is BarrageDuration or BarrageRounds or BarrageCalls or
                SupplyDrop or TroopDrop or PodCapacity or PodCalls ||
            ShipMounts.Any(mount => mount.Id == id) || TooltipTroopTier(id) > 0 ||
            TranslationPatch.HasUpgradeTooltipDescription(id));

        internal static bool IsCommsTurretReplacement(string id) => !string.IsNullOrEmpty(id) &&
            TryGetCommsUpgrade(id, out _, out int tier) && tier > 0;

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

            if (TryGetCommsUpgrade(id, out var location, out int commsTier))
            {
                bool major = commsTier == 0;
                int shownTier = major && progression != null
                    ? EffectiveCommsTurretTier(progression, location) : commsTier;
                var turret = CommsTurretTypes[shownTier];
                yield return UpgradeRow("turrets", major ? "+1" : "+0");
                yield return UpgradeRow("turrets_at_location", "1");
                yield return UpgradeRow("turret_type", UpgradeItemName(turret.PackageId, turret.Name));
                yield return UpgradeRow("position", TranslationPatch.UpgradeText(location.PositionKey));
                if (!major)
                {
                    var previous = CommsTurretTypes[commsTier - 1];
                    yield return UpgradeRow("replaces", UpgradeItemName(previous.PackageId, previous.Name));
                    string previousTitle = commsTier == 1
                        ? location.RootId == SecurityIntrusion ? "Security Intrusion"
                            : location.RootId == EntranceOverride ? "Entrance Override" : "Extraction Override"
                        : location.Label + " " + previous.Name;
                    yield return UpgradeRow("requires", UpgradeName(location.PerkId(commsTier - 1), previousTitle));
                }
                yield return new ShipUpgradeTooltipRow(TranslationPatch.UpgradeText("non_story_missions_only"), null);
                yield return UpgradeRow("deployment", TranslationPatch.UpgradeText("arrival"));
                yield return UpgradeRow("frequency", TranslationPatch.UpgradeText("once_per_raid"));
                yield return UpgradeRow("placement", TranslationPatch.UpgradeText("clear_tile"));
                if (major && id != SecurityIntrusion)
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

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using MGSC;
using UnityEngine;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private const string SupplyModule = "iom_supply_support";
        private const string SelectedSupplyAmmo = "iom.support.supply_ammo";
        private const string SelectedSupplyAmount = "iom.support.supply_amount";

        private sealed class SupplyCargo(string id, string name, int count)
        {
            public readonly string Id = id;
            public readonly string Name = name;
            public readonly int Count = count;
        }

        private sealed class SupplyAmmo(int rank, string label, params SupplyCargo[] cargo)
        {
            public readonly int Rank = rank;
            public readonly string Label = label;
            public readonly SupplyCargo[] Cargo = cargo;
        }

        // Append profiles rather than reordering them: the saved choice is an index.
        // Existing saves and new campaigns retain the original mixed load by default.
        private static readonly SupplyAmmo[] SupplyAmmoProfiles =
        [
            new(0, "Mixed Las / Rifle", new SupplyCargo(LasClip, "Las Clips", 12), new SupplyCargo("rifle_basic_ammo", "Rifle ammo", 8)),
            new(1, "Las Clips", new SupplyCargo(LasClip, "Las Clips", 20)),
            new(1, "Battery ammo", new SupplyCargo("battery_basic_ammo", "Battery ammo", 20)),
            new(1, "Small ammo", new SupplyCargo("small_basic_ammo", "Small ammo", 24)),
            new(1, "Medium ammo", new SupplyCargo("medium_basic_ammo", "Medium ammo", 20)),
            new(1, "Rifle ammo", new SupplyCargo("rifle_basic_ammo", "Rifle ammo", 16)),
            new(1, "Shotgun ammo", new SupplyCargo("shotgun_bullet_ammo", "Shotgun ammo", 12)),
            new(1, "Heavy ammo", new SupplyCargo("heavy_basic_ammo", "Heavy ammo", 12)),
            new(2, "Armor-piercing rifle", new SupplyCargo("rifle_armorpierce_ammo", "Armor-piercing rifle", 12)),
            new(2, "Plasma rifle", new SupplyCargo("rifle_plasma_ammo", "Plasma rifle", 12)),
            new(2, "Flechette shells", new SupplyCargo("shotgun_flechette_ammo", "Flechette shells", 12)),
            new(2, "Incendiary heavy", new SupplyCargo("heavy_incendiary_ammo", "Incendiary heavy", 8)),
            new(3, "Rockets", new SupplyCargo("rocket_basic_ammo", "Rockets", 4)),
            new(3, "Flak ammo", new SupplyCargo("heavy_flak_ammo", "Flak ammo", 8)),
            new(3, "Explosive shells", new SupplyCargo("shotgun_explosion_ammo", "Explosive shells", 8)),
            new(4, "Gas ammo", new SupplyCargo("gas_ammo", "Gas ammo", 12)),
            new(4, "Toxic ammo", new SupplyCargo("toxic_ammo", "Toxic ammo", 8))
        ];

        private enum SupplyBranch { Food, Medical, AmmoType, AmmoAmount }

        private sealed class SupplyResearch(string id, string name, string description, SupplyBranch branch,
            int rank, int x, int y, string parent, string itemIcon, string fallbackArt, string[] price,
            params SupplyCargo[] cargo)
        {
            public readonly string Id = id;
            public readonly string Name = name;
            public readonly string Description = description;
            public readonly SupplyBranch Branch = branch;
            public readonly int Rank = rank;
            public readonly int X = x;
            public readonly int Y = y;
            public readonly string Parent = parent;
            public readonly string ItemIcon = itemIcon;
            public readonly string FallbackArt = fallbackArt;
            public readonly string[] Price = price;
            public readonly SupplyCargo[] Cargo = cargo;
        }

        // Native six-column grid. Every edge connects adjacent cells; food and
        // medicine are optional, and ammunition type/quantity split at logistics.
        private static readonly SupplyResearch[] SupplyResearchNodes =
        [
            new("iom_supply_food_1", "Field Rations", "Add basic food and water.", SupplyBranch.Food, 1, 0, 0, SupplyDrop,
                CorpseStarch, "SupplyPod", ["biomass_tank", "water_container", "engineering_parts_container"],
                new SupplyCargo(CorpseStarch, "Corpse Starch", 2), new SupplyCargo("water_bottle_1", "Water", 1)),
            new("iom_supply_food_2", "Extended Rations", "Add more food and water.", SupplyBranch.Food, 2, 1, 0, "iom_supply_food_1",
                "ration_pack_1", "SupplyPod", ["biomass_tank", "water_container", "water_container"],
                new SupplyCargo(CorpseStarch, "Corpse Starch", 2), new SupplyCargo("water_bottle_1", "Water", 1)),
            new("iom_supply_food_3", "Emergency Provisions", "Add preserved meals and water.", SupplyBranch.Food, 3, 2, 0, "iom_supply_food_2",
                "ration_pack_1", "SupplyPod", ["water_container", "construction_equipment", "engineering_parts_container"],
                new SupplyCargo("ration_pack_1", "Ration pack", 2), new SupplyCargo("water_bottle_1", "Water", 1)),

            new("iom_supply_medical_1", "Field Medicine", "Add dressings and a first-aid kit.", SupplyBranch.Medical, 1, 0, 2, SupplyDrop,
                "medical_kit_1", "MercenaryEquipment", ["medical_equipment", "engineering_parts_container"],
                new SupplyCargo("medical_glue", "Medical glue", 2), new SupplyCargo("medical_kit_1", "Medical kit", 1)),
            new("iom_supply_medical_2", "Medical Stores", "Add antibiotics and painkillers.", SupplyBranch.Medical, 2, 0, 3, "iom_supply_medical_1",
                "pills_antibiotics", "MercenaryEquipment", ["medical_equipment", "medical_equipment", "scientific_equipment"],
                new SupplyCargo("pills_antibiotics", "Antibiotics", 1), new SupplyCargo("pills_painkillers", "Painkillers", 1)),
            new("iom_supply_medical_3", "Trauma Supplies", "Add a trauma kit and antidote.", SupplyBranch.Medical, 3, 1, 3, "iom_supply_medical_2",
                "medical_kit_2", "MercenaryEquipment", ["medical_equipment", "scientific_equipment", "construction_equipment"],
                new SupplyCargo("medical_kit_2", "Advanced medical kit", 1), new SupplyCargo("syringe_antidote", "Antidote", 1)),

            new("iom_supply_ammo_1", "Ammunition Logistics", "Choose standard ammunition.", SupplyBranch.AmmoType, 1, 1, 1, SupplyDrop,
                "rifle_basic_ammo", "MercenaryWeapons", ["weapon_container", "military_parts_container"]),
            new("iom_supply_ammo_2", "Specialized Ammunition", "Unlock specialized ammunition profiles.", SupplyBranch.AmmoType, 2, 1, 2, "iom_supply_ammo_1",
                "rifle_armorpierce_ammo", "MercenaryWeapons", ["military_parts_container", "weapon_container", "scientific_equipment"]),
            new("iom_supply_ammo_3", "Heavy Ordnance", "Unlock rockets, flak and explosive shells.", SupplyBranch.AmmoType, 3, 2, 2, "iom_supply_ammo_2",
                "rocket_basic_ammo", "MissileMount", ["military_parts_container", "weapon_container", "construction_equipment"]),
            new("iom_supply_ammo_4", "Chemical Ammunition", "Unlock gas and toxic ammunition.", SupplyBranch.AmmoType, 4, 3, 2, "iom_supply_ammo_3",
                "gas_ammo", "MercenaryWeapons", ["scientific_equipment", "military_parts_container", "medical_equipment"]),

            new("iom_supply_ammo_amount_1", "Ammo Load I", "Choose up to twice the standard ammunition load.", SupplyBranch.AmmoAmount, 1, 2, 1, "iom_supply_ammo_1",
                "small_basic_ammo", "RoundsFired", ["engineering_parts_container", "military_parts_container"]),
            new("iom_supply_ammo_amount_2", "Ammo Load II", "Choose up to three times the standard load.", SupplyBranch.AmmoAmount, 2, 3, 1, "iom_supply_ammo_amount_1",
                "medium_basic_ammo", "RoundsFired", ["engineering_parts_container", "military_parts_container", "construction_equipment"]),
            new("iom_supply_ammo_amount_3", "Ammo Load III", "Choose up to four times the standard load.", SupplyBranch.AmmoAmount, 3, 4, 1, "iom_supply_ammo_amount_2",
                "heavy_basic_ammo", "RoundsFired", ["engineering_parts_container", "military_parts_container", "military_parts_container", "construction_equipment"])
        ];

        private static bool TryGetSupplyResearch(string id, out SupplyResearch node)
        {
            node = Array.Find(SupplyResearchNodes, candidate => candidate.Id == id);
            return node != null;
        }

        private static bool IsSupplyBranchMajor(string id) =>
            TryGetSupplyResearch(id, out var node) && node.Rank == 1 && node.Branch != SupplyBranch.AmmoAmount;

        private static void RegisterSupplyPodResearch()
        {
            AddModuleText(SupplyModule, "Supply Pod", "Customize ammunition, food and medicine.");
            TranslationPatch.AddShipSupportText("iom.supply.tab", "Supply Pod");
            TranslationPatch.AddShipSupportText("iom.supply.back", "Drops");
            TranslationPatch.AddShipSupportText("iom.supply.mixed", "Mixed Las / Rifle");
            TranslationPatch.AddShipSupportText("iom.supply.ammo_load", "Ammo");
            TranslationPatch.AddShipSupportText("iom.supply.food", "Food");
            TranslationPatch.AddShipSupportText("iom.supply.medical", "Medical");
            var root = RegisterSupportPerk(SupplyModule, SupplyDrop, "SupplyPod", "Supply Pod",
                "Deliver a configurable supply cache. Shares its reserve with troops.", 0, 1, null,
                ["engineering_parts_container", "communication_relay", "medical_equipment"]);
            var records = new Dictionary<string, MagnumPerkRecord> { [SupplyDrop] = root };
            foreach (var node in SupplyResearchNodes)
                records.Add(node.Id, RegisterSupportPerk(SupplyModule, node.Id, node.FallbackArt, node.Name,
                    node.Description, node.X, node.Y, records[node.Parent], node.Price.ToList()));
        }

        private static void FinishSupplyPodResearch()
        {
            foreach (var node in SupplyResearchNodes)
            {
                Sprite normal = null, locked = null;
                try
                {
                    var descriptor = Data.MagnumPerks.GetRecord(node.Id)?.ContentDescriptor as MagnumPerkDescriptor;
                    var icon = Data.Items.GetSimpleRecord<ItemRecord>(node.ItemIcon)?.ItemDesc?.Icon;
                    if (descriptor == null || icon == null) continue;
                    var pixels = SampleCommsTurretIcon(icon);
                    normal = CreateCommsTurretIcon(node.Id, pixels, false);
                    locked = CreateCommsTurretIcon(node.Id, pixels, true);
                    descriptor._icon = normal;
                    descriptor._lockedIcon = locked;
                }
                catch (Exception error)
                {
                    DestroyCommsTurretIcon(normal);
                    DestroyCommsTurretIcon(locked);
                    Debug.LogWarning("[Cvar_ImperiumOfMan] Supply research icon fallback " + node.Id + ": " + error.Message);
                }
                foreach (var cargo in node.Cargo)
                    if (Data.Items.GetRecord(cargo.Id) == null)
                        Debug.LogWarning("[Cvar_ImperiumOfMan] Missing supply cargo " + cargo.Id);
            }
        }

        private static int SupplyRank(MagnumProgression progression, SupplyBranch branch)
        {
            int rank = 0;
            foreach (var node in SupplyResearchNodes)
                if (node.Branch == branch && node.Rank == rank + 1 && progression?.IsPerkPurchased(node.Id) == true)
                    rank++;
            return rank;
        }

        private static int[] SupplyAmmoOptions(MagnumProgression progression)
        {
            int rank = SupplyRank(progression, SupplyBranch.AmmoType);
            return Enumerable.Range(0, SupplyAmmoProfiles.Length)
                .Where(i => SupplyAmmoProfiles[i].Rank <= rank &&
                    SupplyAmmoProfiles[i].Cargo.All(cargo => Data.Items.GetRecord(cargo.Id) != null)).ToArray();
        }

        private static int[] SupplyAmountOptions(MagnumProgression progression) =>
            Enumerable.Range(1, 1 + SupplyRank(progression, SupplyBranch.AmmoAmount)).ToArray();

        private static int SupplySelected(StoryTriggers triggers, string key, int[] available, int fallback) =>
            triggers != null && triggers.FloatMap.TryGetValue(key, out float value) &&
                available.Contains((int)value) ? (int)value : fallback;

        private static int SupplyAmmoChoice(MagnumProgression progression, StoryTriggers triggers) =>
            SupplySelected(triggers, SelectedSupplyAmmo, SupplyAmmoOptions(progression), 0);

        private static int SupplyAmmoAmount(MagnumProgression progression, StoryTriggers triggers) =>
            SupplySelected(triggers, SelectedSupplyAmount, SupplyAmountOptions(progression), 1);

        private static IEnumerable<SupplyCargo> SupplyPodManifest(MagnumProgression progression, StoryTriggers triggers)
        {
            // Merge by real item ID, before native stack creation. Only the ammo
            // profile scales; purchased food and medical additions stay cumulative.
            var stock = new Dictionary<string, SupplyCargo>(StringComparer.Ordinal);
            var profile = SupplyAmmoProfiles[SupplyAmmoChoice(progression, triggers)];
            int amount = SupplyAmmoAmount(progression, triggers);
            foreach (var cargo in profile.Cargo) Add(cargo, amount);
            Add(new SupplyCargo("medical_glue", "Medical glue", 2), 1);
            int food = SupplyRank(progression, SupplyBranch.Food);
            int medical = SupplyRank(progression, SupplyBranch.Medical);
            foreach (var node in SupplyResearchNodes)
                if ((node.Branch == SupplyBranch.Food && node.Rank <= food) ||
                    (node.Branch == SupplyBranch.Medical && node.Rank <= medical))
                    foreach (var cargo in node.Cargo) Add(cargo, 1);
            return stock.Values;

            void Add(SupplyCargo cargo, int multiplier)
            {
                if (Data.Items.GetRecord(cargo.Id) == null) return;
                int total = cargo.Count * multiplier + (stock.TryGetValue(cargo.Id, out var existing) ? existing.Count : 0);
                stock[cargo.Id] = new SupplyCargo(cargo.Id, cargo.Name, total);
            }
        }

        private static string SupplyText(string key, string fallback)
        {
            string tag = "iom.supply." + key;
            return Localization.HasKey(tag) ? Localization.Get(tag) : fallback;
        }

        private static string SupplyAmmoLabel(int index)
        {
            var profile = SupplyAmmoProfiles[index];
            return index == 0 ? SupplyText("mixed", profile.Label) : UpgradeItemName(profile.Cargo[0].Id, profile.Label);
        }

        private static string SupplyPodSummary(MagnumProgression progression, StoryTriggers triggers) =>
            SupplyAmmoLabel(SupplyAmmoChoice(progression, triggers)) + " ×" + SupplyAmmoAmount(progression, triggers) +
            " · " + SupplyText("food", "Food") + " " + SupplyRank(progression, SupplyBranch.Food) +
            " · " + SupplyText("medical", "Medical") + " " + SupplyRank(progression, SupplyBranch.Medical);

        private static IEnumerable<ShipUpgradeTooltipRow> SupplyResearchTooltipRows(SupplyResearch node, MagnumProgression progression)
        {
            if (node.Branch == SupplyBranch.AmmoType)
            {
                foreach (var profile in SupplyAmmoProfiles.Where(profile => profile.Rank == node.Rank))
                    if (profile.Cargo.All(cargo => Data.Items.GetRecord(cargo.Id) != null))
                        yield return UpgradeRow("unlocks", UpgradeItemName(profile.Cargo[0].Id, profile.Label));
                yield return UpgradeRow("ammo_selection", TranslationPatch.UpgradeText("select_on_supply_page"));
            }
            else if (node.Branch == SupplyBranch.AmmoAmount)
            {
                yield return UpgradeRow("ammo_multiplier", "×" + (node.Rank + 1));
                yield return UpgradeRow("ammo_selection", TranslationPatch.UpgradeText("select_on_supply_page"));
            }
            else
            {
                foreach (var cargo in node.Cargo)
                    yield return new ShipUpgradeTooltipRow(UpgradeItemName(cargo.Id, cargo.Name), "+" + UpgradeNumber(cargo.Count));
            }
            yield return UpgradeRow("requires", UpgradeName(node.Parent, node.Parent == SupplyDrop ? "Supply Pod" :
                TryGetSupplyResearch(node.Parent, out var parent) ? parent.Name : "Previous upgrade"));
        }

        // Dedicated native research page, reached from Drop Support rather than
        // another crowded ship anchor. Uses native purchase panels and tooltips.
        private static void ConfigureSupplyPodPage(MagnumModuleTechTreeWindow window, string module, MagnumProgression progression)
        {
            Transform existing = window.transform.Find("ImperialSupplyPodPage");
            if (module is not (DropModule or SupplyModule))
            {
                existing?.gameObject.SetActive(false);
                return;
            }
            var root = existing == null ? CreateSupportRect(window.transform, "ImperialSupplyPodPage",
                new Vector2(0.5f, 0f), new Vector2(246f, 32f), new Vector2(0f, 24f)) : (RectTransform)existing;
            root.gameObject.SetActive(true);
            var tab = Control("Page", module == DropModule ? 74f : -94f, module == DropModule ? 98f : 58f);
            var ammo = Control("Ammo", 0f, 122f);
            var amount = Control("Amount", 94f, 60f);
            bool supply = module == SupplyModule;
            ammo.gameObject.SetActive(supply);
            amount.gameObject.SetActive(supply);
            tab.Configure(null, supply ? SupplyText("back", "Drops") : SupplyText("tab", "Supply Pod"),
                true, () => window.Configure(supply ? DropModule : SupplyModule));
            if (!supply) return;

            var triggers = SingletonMonoBehaviour<SpaceGameMode>.Instance.Get<StoryTriggers>();
            var options = SupplyAmmoOptions(progression);
            int[] quantities = SupplyAmountOptions(progression);
            int selected = SupplyAmmoChoice(progression, triggers);
            int load = SupplyAmmoAmount(progression, triggers);
            bool installed = progression.IsPerkPurchased(SupplyDrop);
            ammo.Configure(null, SupplyAmmoLabel(selected) + (options.Length > 1 ? " ›" : ""), installed && options.Length > 1,
                () =>
                {
                    selected = options[(Array.IndexOf(options, selected) + 1) % options.Length];
                    triggers.FloatMap[SelectedSupplyAmmo] = selected;
                    ammo.SetLabel(SupplyAmmoLabel(selected) + " ›");
                });
            amount.Configure(null, SupplyText("ammo_load", "Ammo") + " ×" + load, installed && quantities.Length > 1,
                () =>
                {
                    load = quantities[(Array.IndexOf(quantities, load) + 1) % quantities.Length];
                    triggers.FloatMap[SelectedSupplyAmount] = load;
                    amount.SetLabel(SupplyText("ammo_load", "Ammo") + " ×" + load);
                });

            ShipSupportButton Control(string name, float x, float width)
            {
                var child = root.Find(name) as RectTransform ?? CreateSupportRect(root, name, new Vector2(0.5f, 0.5f),
                    new Vector2(width, 32f), new Vector2(x, 0f));
                child.sizeDelta = new Vector2(width, 32f);
                child.anchoredPosition = new Vector2(x, 0f);
                return child.GetComponent<ShipSupportButton>() ?? child.gameObject.AddComponent<ShipSupportButton>();
            }
        }
    }
}

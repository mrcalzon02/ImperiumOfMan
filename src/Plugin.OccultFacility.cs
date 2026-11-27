using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using MGSC;
using TMPro;
using UnityEngine;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private const int OccultLineBase = 10000;
        private const int OccultLineLimit = 3;
        private const string OccultYieldCultureI = "iom_occult_culture_yield_1";
        private const string OccultYieldCultureII = "iom_occult_culture_yield_2";
        private const string OccultSpeedI = "iom_occult_speed_1";
        private const string OccultSpeedII = "iom_occult_speed_2";
        private const string OccultYieldMaterialI = "iom_occult_material_yield_1";
        private const string OccultYieldMaterialII = "iom_occult_material_yield_2";
        private const string OccultQueueI = "iom_occult_queue_1";
        private const string OccultQueueII = "iom_occult_queue_2";
        private const string OccultLineII = "iom_occult_line_2";
        private const string OccultLineIII = "iom_occult_line_3";

        private sealed class OccultFacilityUpgrade(string id, string title, string description,
            string parent, string requiredRank, int x, int y, string art, string icon, string[] price)
        {
            public readonly string Id = id;
            public readonly string Title = title;
            public readonly string Description = description;
            public readonly string Parent = parent;
            public readonly string RequiredRank = requiredRank;
            public readonly int X = x;
            public readonly int Y = y;
            public readonly string Art = art;
            public readonly string Icon = icon;
            public readonly string[] Price = price;
        }

        // Six retained major research ranks occupy row 1. Optional facilities
        // branch above/below them on the native six-column grid.
        private static readonly OccultFacilityUpgrade[] OccultFacilityUpgrades =
        [
            new(OccultYieldCultureI, "Culture Expansion I", "+1 reagent per batch.", OccultRankOne, OccultRankOne,
                0, 0, "OccultResearch1", RawClone, ["medical_equipment", "biomass_tank", "quasisamples"]),
            new(OccultYieldCultureII, "Culture Expansion II", "+2 reagents per batch in total.", OccultYieldCultureI, OccultRankTwo,
                1, 0, "OccultResearch2", SufferingEssence, ["scientific_equipment", "medical_equipment", NativeGavvakh]),
            new(OccultSpeedI, "Ritual Automation I", "Reduce occult batch time by 15%.", OccultRankThree, OccultRankThree,
                2, 0, "OccultResearch3", "venus_weapon_parts", ["engineering_parts_container", "scientific_equipment", "mercury_glass"]),
            new(OccultSpeedII, "Ritual Automation II", "Reduce occult batch time by 30% in total.", OccultSpeedI, OccultRankFour,
                3, 0, "OccultResearch4", "moon_weapon_parts", ["engineering_parts_container", "scientific_equipment", "ron_blood"]),
            new(OccultYieldMaterialI, "Material Recovery I", "+1 occult material per batch.", OccultRankFive, OccultRankFive,
                4, 0, "OccultResearch5", "cattaram_geode", ["construction_equipment", "scientific_equipment", "cattaram_geode"]),
            new(OccultYieldMaterialII, "Material Recovery II", "+2 occult materials per batch in total.", OccultYieldMaterialI, OccultRankSix,
                5, 0, "OccultResearch6", ForgeEmber, ["prototype_schematics", "scientific_equipment", "cattaram_stone"]),
            new(OccultQueueI, "Ritual Scheduling I", "Two orders per occult line.", OccultRankOne, OccultRankOne,
                0, 2, "AdditionalCallIns", "low_chip", ["communication_relay", "engineering_parts_container"]),
            new(OccultQueueII, "Ritual Scheduling II", "Three orders per occult line.", OccultQueueI, OccultRankTwo,
                0, 3, "AdditionalCallIns", "scientific_equipment", ["communication_relay", "management_unit", NativeGavvakh]),
            new(OccultLineII, "Secondary Occult Line", "Add a second independent fabrication line.", OccultRankThree, OccultRankThree,
                2, 2, "OccultResearch3", "quasisamples", ["construction_equipment", "scientific_equipment", "demonic_eye"]),
            new(OccultLineIII, "Tertiary Occult Line", "Add a third independent fabrication line.", OccultLineII, OccultRankFive,
                3, 2, "OccultResearch5", "jupiter_mushrooms", ["construction_equipment", "prototype_schematics", "jupiter_mushrooms"])
        ];

        private static bool TryGetOccultFacilityUpgrade(string id, out OccultFacilityUpgrade node)
        {
            node = Array.Find(OccultFacilityUpgrades, candidate => candidate.Id == id);
            return node != null;
        }

        private static bool IsOccultProductionUpgrade(string id) => TryGetOccultFacilityUpgrade(id, out _);
        private static bool IsOccultLine(int line) => line >= OccultLineBase && line < OccultLineBase + OccultLineLimit;

        private static bool TryGetOccultRequirement(ItemProduceReceipt receipt, out string perk)
        {
            perk = null;
            return receipt != null && ((!string.IsNullOrEmpty(receipt.Id) && OccultRecipeRequirements.TryGetValue(receipt.Id, out perk)) ||
                (!string.IsNullOrEmpty(receipt.OutputItem) && OccultRecipeRequirements.TryGetValue(receipt.OutputItem, out perk)));
        }

        private static bool OccultPurchased(MagnumProgression progression, string id) =>
            progression?.IsPerkPurchased(id) == true;

        private static int OccultLineCount(MagnumProgression progression) =>
            !OccultPurchased(progression, OccultRankOne) ? 0 :
            1 + (OccultPurchased(progression, OccultLineII) ?
                (OccultPurchased(progression, OccultLineIII) ? 2 : 1) : 0);

        private static int OccultQueueCount(MagnumProgression progression) =>
            1 + (OccultPurchased(progression, OccultQueueI) ? 1 : 0) +
                (OccultPurchased(progression, OccultQueueII) && OccultPurchased(progression, OccultQueueI) ? 1 : 0);

        private static int OccultCultureBonus(MagnumProgression progression) =>
            !OccultPurchased(progression, OccultYieldCultureI) ? 0 : OccultPurchased(progression, OccultYieldCultureII) ? 2 : 1;

        private static int OccultMaterialBonus(MagnumProgression progression) =>
            !OccultPurchased(progression, OccultYieldMaterialI) ? 0 : OccultPurchased(progression, OccultYieldMaterialII) ? 2 : 1;

        private static int OccultSpeedPercent(MagnumProgression progression) =>
            !OccultPurchased(progression, OccultSpeedI) ? 0 : OccultPurchased(progression, OccultSpeedII) ? 30 : 15;

        private static readonly HashSet<string> OccultMaterialOutputs = new(StringComparer.Ordinal)
        {
            "demonic_eye", "quasi_venus_rags", "venus_armor_plates", "venus_weapon_parts", "venus_melee_parts",
            "quasi_venus_feather", "venus_guts", "mercury_glass", "mercury_rags", "mercury_scales", "mercury_guts",
            "moon_guts", "moon_armor_plates", "moon_weapon_parts", "ron_blood", "ron_tentacle",
            "mars_skin", "mars_gear", "cattaram_geode", "cattaram_stone", "jupiter_mushrooms", "jupiter_grass", "jupiter_guts",
            BoundAsh, ForgeEmber
        };

        private static int OccultBatchSize(ItemProduceReceipt receipt, MagnumProgression progression, int baseBatch)
        {
            if (!TryGetOccultRequirement(receipt, out _)) return baseBatch;
            string id = receipt.OutputItem;
            if (id is RawClone or TormentImprint or SufferingEssence or NativeGavvakh)
                return checked(baseBatch + OccultCultureBonus(progression));
            return OccultMaterialOutputs.Contains(id) ? checked(baseBatch + OccultMaterialBonus(progression)) : baseBatch;
        }

        // Independent timing: ordinary Production upgrades do not change occult
        // machinery. Preserve native difficulty scaling, whole hours and minimum.
        private static int OccultBatchHours(ItemProduceReceipt receipt, MagnumProgression progression, Difficulty difficulty) =>
            (int)Mathf.Max(receipt.ProduceTimeInHours * (1f - OccultSpeedPercent(progression) / 100f) *
                (1f / difficulty.Preset.MagnumCraftingTime), 1f);

        private static void RegisterOccultFacilityUpgrades(Dictionary<string, MagnumPerkRecord> ranks)
        {
            foreach (var node in OccultFacilityUpgrades)
            {
                var perk = RegisterSupportPerk(OccultModule, node.Id, node.Art, node.Title, node.Description,
                    node.X, node.Y, ranks[node.Parent], node.Price.ToList());
                ranks.Add(node.Id, perk);
            }
            TranslationPatch.AddShipSupportText("iom.occult.production.open", "Occult Production");
            TranslationPatch.AddShipSupportText("iom.occult.production.ordinary", "Ordinary Production");
            TranslationPatch.AddShipSupportText("iom.occult.production.line", "Occult Line");
            TranslationPatch.AddShipSupportText("iom.occult.production.wrong_line", "Use the corresponding production line.");
            TranslationPatch.AddShipSupportText("iom.occult.production.queue_full", "Occult line unavailable or queue full.");
            TranslationPatch.AddShipSupportText("iom.occult.production.batch", "Per batch");
        }

        private static string OccultProductionText(string key, string fallback)
        {
            string tag = "iom.occult.production." + key;
            return Localization.HasKey(tag) ? Localization.Get(tag) : fallback;
        }

        private static void FinishOccultFacilityIcons()
        {
            foreach (var node in OccultFacilityUpgrades)
            {
                Sprite normal = null, locked = null;
                try
                {
                    var descriptor = Data.MagnumPerks.GetRecord(node.Id)?.ContentDescriptor as MagnumPerkDescriptor;
                    var icon = Data.Items.GetSimpleRecord<ItemRecord>(node.Icon)?.ItemDesc?.Icon;
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
                    Debug.LogWarning("[Cvar_ImperiumOfMan] Occult facility icon fallback " + node.Id + ": " + error.Message);
                }
            }
        }

        private static bool OccultLineCanQueue(MagnumCargo cargo, MagnumProgression progression, int lineIndex) =>
            cargo != null && IsOccultLine(lineIndex) && lineIndex - OccultLineBase < OccultLineCount(progression) &&
            (!cargo.ItemProduceOrders.TryGetValue(lineIndex, out var orders) || orders.Count < OccultQueueCount(progression));

        private static bool OccultProductionRouteAllowed(ItemProduceReceipt receipt, MagnumCargo cargo,
            MagnumProgression progression, int lineIndex)
        {
            if (receipt == null) return false;
            bool occult = TryGetOccultRequirement(receipt, out string perk);
            if (!occult) return !IsOccultLine(lineIndex);
            return _industryReady && HasOccultCapability(progression, perk) &&
                cargo != null && cargo.UnlockedProductionItems.Contains(receipt.OutputItem) &&
                OccultLineCanQueue(cargo, progression, lineIndex);
        }

        private sealed class OccultProductionView : MonoBehaviour
        {
            public bool Occult;
        }

        // Reuse the game's actual queue panels, not a parallel scheduler. Fixed
        // IDs remain stable when ordinary line slots or research change.
        [HarmonyPatch(typeof(MagnumProductionWindow), "InitPanels")]
        private static class OccultProductionPanels
        {
            private static bool Prefix(MagnumProductionWindow __instance, MagnumCargo ____magnumCargo,
                MagnumProgression ____magnumSpaceship, SpaceTime ____spaceTime, RectTransform ____panelsRoot,
                Pool ____panelsPool, List<ItemProductionPanel> ____panels)
            {
                var mode = __instance.GetComponent<OccultProductionView>();
                if (mode == null || !mode.Occult) return true;
                if (OccultLineCount(____magnumSpaceship) == 0 &&
                    !____magnumCargo.ItemProduceOrders.Any(entry => IsOccultLine(entry.Key) && entry.Value.Count > 0))
                {
                    mode.Occult = false;
                    return true;
                }

                var start = (Action<int>)AccessTools.Method(typeof(MagnumProductionWindow), "OnStartProduction")
                    .CreateDelegate(typeof(Action<int>), __instance);
                var cancel = (Action<int>)AccessTools.Method(typeof(MagnumProductionWindow), "OnCancelProduction")
                    .CreateDelegate(typeof(Action<int>), __instance);
                var cancelQueued = (Action<int, ProduceOrder>)AccessTools.Method(typeof(MagnumProductionWindow), "OnCancelQueuedProduction")
                    .CreateDelegate(typeof(Action<int, ProduceOrder>), __instance);

                foreach (var panel in ____panels)
                {
                    panel.OnStartProduction -= start;
                    panel.OnCancelProduction -= cancel;
                    panel.OnCancelQueuedProduction -= cancelQueued;
                    var label = panel.transform.Find("ImperialOccultLineLabel");
                    if (label != null) label.gameObject.SetActive(false);
                    ____panelsPool.Put(panel.transform.parent.gameObject);
                }
                ____panels.Clear();

                int unlocked = OccultLineCount(____magnumSpaceship);
                for (int ordinal = 0; ordinal < OccultLineLimit; ordinal++)
                {
                    int line = OccultLineBase + ordinal;
                    // Never hide or destroy a previously queued order if capability
                    // is absent in an unusual imported save. It can still be canceled.
                    bool existing = ____magnumCargo.ItemProduceOrders.TryGetValue(line, out var orders);
                    if (ordinal >= unlocked && (!existing || orders.Count == 0)) continue;
                    if (!existing)
                    {
                        orders = new List<ProduceOrder>();
                        ____magnumCargo.ItemProduceOrders.Add(line, orders);
                    }

                    var panel = ____panelsPool.Take().GetComponentInChildren<ItemProductionPanel>();
                    panel.OnStartProduction += start;
                    panel.OnCancelProduction += cancel;
                    panel.OnCancelQueuedProduction += cancelQueued;
                    panel.Initialize(line, orders, OccultQueueCount(____magnumSpaceship), ____spaceTime.Time);
                    panel.transform.parent.SetParent(____panelsRoot, false);
                    panel.transform.parent.SetAsLastSibling();
                    ____panels.Add(panel);

                    // A persistent, explicit label identifies the dedicated line.
                    var label = panel.transform.Find("ImperialOccultLineLabel") as RectTransform ??
                        CreateSupportRect(panel.transform, "ImperialOccultLineLabel", new Vector2(0.5f, 1f),
                            new Vector2(122f, 14f), new Vector2(0f, -8f));
                    var caption = label.GetComponent<UnityEngine.UI.Text>() ??
                        label.gameObject.AddComponent<UnityEngine.UI.Text>();
                    caption.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                    caption.fontSize = 11;
                    caption.alignment = TextAnchor.MiddleCenter;
                    caption.color = new Color32(184, 220, 196, 255);
                    caption.raycastTarget = false;
                    caption.text = OccultProductionText("line", "Occult Line") + " " + (ordinal + 1);
                    label.gameObject.SetActive(true);
                }
                return false;
            }

            private static void Postfix(MagnumProductionWindow __instance, MagnumCargo ____magnumCargo,
                MagnumProgression ____magnumSpaceship)
            {
                var mode = __instance.GetComponent<OccultProductionView>() ??
                    __instance.gameObject.AddComponent<OccultProductionView>();
                bool available = OccultLineCount(____magnumSpaceship) > 0 ||
                    ____magnumCargo.ItemProduceOrders.Any(entry => IsOccultLine(entry.Key) && entry.Value.Count > 0);
                var tab = __instance.transform.Find("ImperialOccultProductionTab") as RectTransform;
                if (!available && !mode.Occult)
                {
                    if (tab != null) tab.gameObject.SetActive(false);
                    return;
                }
                tab ??= CreateSupportRect(__instance.transform, "ImperialOccultProductionTab", new Vector2(0.5f, 0f),
                    new Vector2(160f, 28f), new Vector2(0f, 24f));
                var control = tab.GetComponent<ShipSupportButton>() ?? tab.gameObject.AddComponent<ShipSupportButton>();
                control.Configure(null, mode.Occult ? OccultProductionText("ordinary", "Ordinary Production") :
                    OccultProductionText("open", "Occult Production"), true, () =>
                    {
                        mode.Occult = !mode.Occult;
                        RebuildOccultProduction(__instance);
                    });
                tab.gameObject.SetActive(true);
            }
        }

        // Normal pooled panels must not retain occult labels after returning to
        // ordinary production, including the native cancel/rebuild lifecycle.
        [HarmonyPatch(typeof(ItemProductionPanel), nameof(ItemProductionPanel.Initialize))]
        private static class ResetOccultLineLabel
        {
            private static void Prefix(ItemProductionPanel __instance)
            {
                var label = __instance.transform.Find("ImperialOccultLineLabel");
                if (label != null) label.gameObject.SetActive(false);
            }
        }

        private static void RebuildOccultProduction(MagnumProductionWindow window)
        {
            AccessTools.Method(typeof(MagnumProductionWindow), "InitPanels").Invoke(window, null);
            var panels = (List<ItemProductionPanel>)AccessTools.Field(typeof(MagnumProductionWindow), "_panels").GetValue(window);
            foreach (var panel in panels) UI.Navigation.RegisterElement(panel);
            if (panels.Count > 0) UI.Navigation.Select(panels[0]);
        }

        [HarmonyPatch(typeof(MagnumModuleTechTreeWindow), "RefreshGrid")]
        private static class OpenOccultProduction
        {
            private static void Postfix(MagnumModuleTechTreeWindow __instance, string ____moduleId, MagnumProgression ____magnumSpaceship)
            {
                var rect = __instance.transform.Find("ImperialOccultProductionOpen") as RectTransform;
                if (____moduleId != OccultModule)
                {
                    if (rect != null) rect.gameObject.SetActive(false);
                    return;
                }
                rect ??= CreateSupportRect(__instance.transform, "ImperialOccultProductionOpen", new Vector2(0.5f, 0f),
                    new Vector2(158f, 28f), new Vector2(0f, 24f));
                var button = rect.GetComponent<ShipSupportButton>() ?? rect.gameObject.AddComponent<ShipSupportButton>();
                button.Configure(null, OccultProductionText("open", "Occult Production"),
                    OccultLineCount(____magnumSpaceship) > 0, () =>
                    {
                        UI.Get<SpaceshipScreen>().ShowProductionWindow();
                        var window = UI.Get<MagnumProductionWindow>();
                        var mode = window.GetComponent<OccultProductionView>() ??
                            window.gameObject.AddComponent<OccultProductionView>();
                        mode.Occult = true;
                        RebuildOccultProduction(window);
                    });
                rect.gameObject.SetActive(true);
            }
        }

        // Keep native category, knowledge, project and augmentation filters.
        // Rebuild the virtualized panel list only when the line-type filter changes it.
        [HarmonyPatch(typeof(MagnumSelectItemToProduceWindow), "InitPanels")]
        private static class FilterOccultProductionReceipts
        {
            private static void Postfix(MagnumSelectItemToProduceWindow __instance, int ____lineIndex,
                List<ItemProduceReceipt> ____receipts)
            {
                bool occult = IsOccultLine(____lineIndex);
                bool WrongLine(ItemProduceReceipt receipt) => TryGetOccultRequirement(receipt, out _) != occult;
                if (!____receipts.Any(WrongLine)) return;
                AccessTools.Method(typeof(MagnumSelectItemToProduceWindow), "FreePanels").Invoke(__instance, null);
                ____receipts.RemoveAll(receipt => WrongLine(receipt));
                AccessTools.Method(typeof(MagnumSelectItemToProduceWindow), "RefreshContentHeight").Invoke(__instance, null);
                AccessTools.Method(typeof(MagnumSelectItemToProduceWindow), "RefreshVisiblePanels").Invoke(__instance, new object[] { true });
            }
        }

        [HarmonyPatch(typeof(ItemReceiptPanel), nameof(ItemReceiptPanel.Initialize),
            [typeof(MagnumCargo), typeof(MagnumProgression), typeof(MagnumProjects), typeof(ItemProduceReceipt), typeof(Difficulty)])]
        private static class ShowOccultBatchHours
        {
            private static void Postfix(MagnumProgression __1, ItemProduceReceipt __3, Difficulty __4,
                TextMeshProUGUI ____produceDate)
            {
                if (!TryGetOccultRequirement(__3, out _)) return;
                string hours = FormatHelper.ToLocalizedDaysAndHours(OccultBatchHours(__3, __1, __4));
                int batch = IndustryBatches.TryGetValue(__3.OutputItem, out int baseBatch)
                    ? OccultBatchSize(__3, __1, baseBatch) : 1;
                ____produceDate.text = "×" + batch + " · " + hours;
                Localization.ActualizeFontAndSize(____produceDate, TextContext.ButtonCaption);
            }
        }
    }
}

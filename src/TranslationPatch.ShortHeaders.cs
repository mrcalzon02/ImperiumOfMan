using System;
using System.Collections.Generic;
using MGSC;

namespace ImperiumOfMan
{
    public static partial class TranslationPatch
    {
        // Only items whose prose belongs to this mod receive a description panel.
        // Native items and text supplied by other mods keep their own presentation.
        private static readonly HashSet<string> ItemTooltipIds = new(StringComparer.Ordinal)
        {
            "ore_cargo", "iom_lasgun", "iom_melta", "iom_lassniper", "iom_bolter", "iom_plasma_pistol",
            "iom_sevitor_backpack", "iom_lightboots", "iom_lightleggings", "iom_lightChestplate",
            "iom_lightHelmet", "iom_cadiaboots", "iom_cadialeggings", "iom_cadiaChestplate", "iom_cadiaHelmet"
        };

        private static readonly Dictionary<string, string> ItemSubtitles = new(StringComparer.Ordinal)
        {
            ["ore_cargo"] = "Bulk mineral cargo",
            ["iom_lasgun"] = "Guard laser rifle",
            ["iom_melta"] = "Thermal weapon",
            ["iom_lassniper"] = "Scoped laser rifle",
            ["iom_bolter"] = "Heavy bolt weapon",
            ["iom_plasma_pistol"] = "Plasma sidearm",
            ["iom_sevitor_backpack"] = "Powered carry pack",
            ["iom_lightboots"] = "Patrol footwear",
            ["iom_lightleggings"] = "Light leg protection",
            ["iom_lightChestplate"] = "Light body armor",
            ["iom_lightHelmet"] = "Light head protection",
            ["iom_cadiaboots"] = "Infantry footwear",
            ["iom_cadialeggings"] = "Flak leg protection",
            ["iom_cadiaChestplate"] = "Flak body armor",
            ["iom_cadiaHelmet"] = "Flak head protection",
            ["iom_corpse_starch"] = "Field ration",
            ["iom_dirty_corpse_starch"] = "Contaminated ration",
            ["iom_valhallan_tanna"] = "Warming drink",
            ["iom_chaos_oathbread"] = "Cult ration",
            ["iom_chaos_infernal_draught"] = "Pain suppressant",
            ["iom_requisition"] = "Requisition token",
            ["iom_purity_seal"] = "Devotional token",
            ["iom_sororitas_reliquary"] = "Mission relic",
            ["iom_necromancer_chip"] = "Mercenary class chip",
            ["iom_combat_engineer_chip"] = "Mercenary class chip",
            ["iom_vigils_chip"] = "Mercenary class chip",
            ["iom_occult_raw_clone"] = "Clone stock",
            ["iom_occult_torment_imprint"] = "Occult imprint",
            ["iom_occult_suffering_essence"] = "Occult reagent"
        };

        private static readonly Dictionary<string, string> RussianItemSubtitles = new(StringComparer.Ordinal)
        {
            ["Bulk mineral cargo"] = "Минеральный груз",
            ["Guard laser rifle"] = "Лазерная винтовка",
            ["Thermal weapon"] = "Тепловое оружие",
            ["Scoped laser rifle"] = "Снайперская винтовка",
            ["Heavy bolt weapon"] = "Болтовое оружие",
            ["Plasma sidearm"] = "Плазменное оружие",
            ["Powered carry pack"] = "Грузовой усилитель",
            ["Patrol footwear"] = "Патрульная обувь",
            ["Light leg protection"] = "Лёгкая защита ног",
            ["Light body armor"] = "Лёгкая броня",
            ["Light head protection"] = "Лёгкая защита головы",
            ["Infantry footwear"] = "Пехотная обувь",
            ["Flak leg protection"] = "Пехотная защита ног",
            ["Flak body armor"] = "Пехотная броня",
            ["Flak head protection"] = "Пехотная защита головы",
            ["Field ration"] = "Полевой паёк",
            ["Contaminated ration"] = "Заражённый паёк",
            ["Warming drink"] = "Согревающий напиток",
            ["Cult ration"] = "Паёк культа",
            ["Pain suppressant"] = "Обезболивающее",
            ["Requisition token"] = "Талон снабжения",
            ["Devotional token"] = "Религиозный знак",
            ["Mission relic"] = "Реликвия задания",
            ["Mercenary class chip"] = "Чип класса",
            ["Clone stock"] = "Сырьё для клонов",
            ["Occult imprint"] = "Оккультный отпечаток",
            ["Occult reagent"] = "Оккультный реагент",
            ["Mission component"] = "Компонент задания",
            ["Mission objective"] = "Предмет задания",
            ["Machine wreck"] = "Остов машины",
            ["Melee weapon"] = "Холодное оружие",
            ["Ranged weapon"] = "Стрелковое оружие",
            ["Head protection"] = "Защита головы",
            ["Body armor"] = "Броня",
            ["Leg protection"] = "Защита ног",
            ["Protective footwear"] = "Защитная обувь",
            ["Carrying equipment"] = "Переноска снаряжения",
            ["Tactical vest"] = "Разгрузочный жилет",
            ["Ammunition"] = "Боеприпасы",
            ["Thrown ordnance"] = "Метательный боеприпас",
            ["Thrown weapon"] = "Метательное оружие",
            ["Fabrication schematic"] = "Чертёж производства",
            ["Implant"] = "Имплантат",
            ["Body replacement"] = "Протез",
            ["Deployable equipment"] = "Размещаемое устройство",
            ["Utility device"] = "Вспомогательное устройство",
            ["Repair supplies"] = "Ремонтные материалы",
            ["Wound treatment"] = "Лечение ран",
            ["Pact focus"] = "Фокус пакта",
            ["Ritual component"] = "Ритуальный компонент",
            ["Food ration"] = "Пищевой паёк",
            ["Consumable"] = "Расходный предмет",
            ["Trade goods"] = "Товар для обмена",
            ["Crafting material"] = "Материал производства",
            ["Equipment"] = "Снаряжение"
        };

        private static readonly Dictionary<string, string> UpgradeSubtitles = new(StringComparer.Ordinal)
        {
            ["iom_comms_security_intrusion"] = "Security override",
            ["iom_comms_entrance_override"] = "Entrance defense",
            ["iom_comms_extraction_override"] = "Extraction defense",
            ["iom_gun_mount"] = "Orbital gun",
            ["iom_missile_mount"] = "Orbital missiles",
            ["iom_laser_mount"] = "Orbital laser",
            ["iom_barrage_duration"] = "Barrage duration",
            ["iom_barrage_rounds"] = "Salvo capacity",
            ["iom_barrage_calls"] = "Fire support charges",
            ["iom_supply_pod"] = "Supply delivery",
            ["iom_mercenary_pod"] = "Troop deployment",
            ["iom_pod_capacity"] = "Troop capacity",
            ["iom_pod_calls"] = "Drop support charges",
            ["iom_occult_clone_vat"] = "Clone cultivation",
            ["iom_occult_containment_cradle"] = "Occult containment",
            ["iom_occult_essence_still"] = "Essence extraction",
            ["iom_occult_gavvakh_retort"] = "Reagent distillation",
            ["iom_occult_research_1"] = "Occult research",
            ["iom_occult_research_2"] = "Occult research",
            ["iom_occult_research_3"] = "Occult research",
            ["iom_occult_research_4"] = "Occult research",
            ["iom_occult_research_5"] = "Occult research"
        };

        private static readonly Dictionary<string, string> ModuleSubtitles = new(StringComparer.Ordinal)
        {
            ["ui.magnum.module.iom_ship_weapons.shortdesc"] = "Orbital fire support",
            ["ui.magnum.module.iom_drop_support.shortdesc"] = "Supply and troop drops",
            ["ui.magnum.module.iom_comms_node.shortdesc"] = "Non-story missions only",
            ["ui.magnum.module.iom_occult_production.shortdesc"] = "Occult fabrication"
        };

        private static bool TryTroopRosterTier(string id, out int tier)
        {
            const string prefix = "iom_drop_troops_";
            tier = 0;
            return id.StartsWith(prefix, StringComparison.Ordinal) &&
                int.TryParse(id.Substring(prefix.Length), out tier) && tier >= 2 && tier <= 11;
        }

        private static string UpgradeSubtitle(string id) => UpgradeSubtitles.TryGetValue(id, out string subtitle)
            ? subtitle : Plugin.IsCommsTurretReplacement(id) ? "Turret replacement"
                : TryTroopRosterTier(id, out _) ? "Troop roster" : "Ship upgrade";

        private static void ApplyItemHeaders(Dictionary<string, string> target, bool russian)
        {
            // Registration has staged the real records before ApplyToLoadedLanguages.
            // Repeating the classification here also handles a later language reload.
            foreach (string id in ItemTooltipIds)
            {
                target["item." + id + ".shortdesc"] = ItemSubtitle(id, russian);
            }
        }

        private static string ItemSubtitle(string id, bool russian)
        {
            string subtitle;
            if (ItemSubtitles.TryGetValue(id, out string explicitSubtitle))
                subtitle = explicitSubtitle;
            else if (id.EndsWith("_access_component", StringComparison.Ordinal))
                subtitle = "Mission component";
            else if (id.EndsWith("_evidence", StringComparison.Ordinal))
                subtitle = "Mission objective";
            else if (id.StartsWith("iom_wreck_", StringComparison.Ordinal))
                subtitle = "Machine wreck";
            else
            {
                subtitle = ItemRole(Data.Items?.GetRecord(id) as CompositeItemRecord);
            }
            return russian && RussianItemSubtitles.TryGetValue(subtitle, out string translated) ? translated : subtitle;
        }

        private static string ItemRole(CompositeItemRecord item)
        {
            // GetSimpleRecord dereferences a missing composite in the native API.
            // A language reload before registration must still be harmless.
            if (item == null)
                return "Equipment";

            // Ask for each semantic record explicitly: a composite may place a
            // shared ItemRecord or TrashRecord before its actual equipment record.
            // Mechanical values and special behavior remain in the wrapped body.
            if (item.GetRecord<WeaponRecord>() is WeaponRecord weapon)
                return weapon.IsMelee ? "Melee weapon" : "Ranged weapon";
            if (item.GetRecord<HelmetRecord>() != null) return "Head protection";
            if (item.GetRecord<ArmorRecord>() != null) return "Body armor";
            if (item.GetRecord<LeggingsRecord>() != null) return "Leg protection";
            if (item.GetRecord<BootsRecord>() != null) return "Protective footwear";
            if (item.GetRecord<BackpackRecord>() != null) return "Carrying equipment";
            if (item.GetRecord<VestRecord>() != null) return "Tactical vest";
            if (item.GetRecord<GrenadeRecord>() != null) return "Thrown ordnance";
            if (item.GetRecord<ThrowingWeaponRecord>() != null) return "Thrown weapon";
            if (item.GetRecord<AmmoRecord>() != null) return "Ammunition";
            if (item.GetRecord<DatadiskRecord>() != null) return "Fabrication schematic";
            if (item.GetRecord<ImplantRecord>() != null) return "Implant";
            if (item.GetRecord<AugmentationRecord>() != null) return "Body replacement";
            if (item.GetRecord<PlaceableDeviceRecord>() != null) return "Deployable equipment";
            if (item.GetRecord<DeviceRecord>() != null) return "Utility device";
            if (item.GetRecord<RepairRecord>() != null) return "Repair supplies";
            if (item.GetRecord<FixationMedicineRecord>() != null) return "Wound treatment";
            if (item.GetRecord<SkullRecord>() != null) return "Pact focus";
            if (item.GetRecord<PactComponentRecord>() != null) return "Ritual component";
            if (item.GetRecord<ConsumableRecord>() is ConsumableRecord consumable)
                return consumable.StarvationValue > 0 ? "Food ration" : "Consumable";
            if (item.GetRecord<TrashRecord>() is TrashRecord trash)
                return trash.ItemClass == ItemClass.QuestItem ? "Mission objective"
                    : trash.ItemClass == ItemClass.ValuableBarter ? "Trade goods" : "Crafting material";
            return "Equipment";
        }

        internal static bool TryGetItemTooltipDescription(string id, out string description)
        {
            description = null;
            if (string.IsNullOrEmpty(id) || !ItemTooltipIds.Contains(id))
                return false;
            string key = "item." + id + ".desc";
            if (!English.TryGetValue(key, out string fallback) || string.IsNullOrWhiteSpace(fallback))
                return false;
            description = Localization.HasKey(key) ? Localization.Get(key) : fallback;
            if (string.IsNullOrWhiteSpace(description) || description == key)
                description = fallback;
            string headerKey = "item." + id + ".shortdesc";
            return !Localization.HasKey(headerKey) || description != Localization.Get(headerKey);
        }
    }
}

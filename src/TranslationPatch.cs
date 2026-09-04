using System.Collections.Generic;
using HarmonyLib;
using MGSC;

namespace ImperiumOfMan
{
    [HarmonyPatch(typeof(Localization))]
    public static class TranslationPatch
    {
        private static readonly Dictionary<string, string> English = new Dictionary<string, string>
        {
            ["item.iom_lasgun.name"] = "Lasgun",
            ["item.iom_lasgun.shortdesc"] = "Laser Rifle",
            ["item.iom_melta.name"] = "Melta Rifle",
            ["item.iom_melta.shortdesc"] = "Melta Rifle",
            ["item.iom_lassniper.name"] = "Long-Las",
            ["item.iom_lassniper.shortdesc"] = "Laser Sniper Rifle",
            ["item.iom_bolter.name"] = "Bolter",
            ["item.iom_bolter.shortdesc"] = "Bolter",
            ["item.iom_plasma_pistol.name"] = "Plasma Pistol",
            ["item.iom_plasma_pistol.shortdesc"] = "Pistol",
            ["item.iom_sevitor_backpack.name"] = "Servitor",
            ["item.iom_sevitor_backpack.shortdesc"] = "Your Backpack",
            ["item.iom_lightboots.name"] = "Planetary Defence Force",
            ["item.iom_lightboots.shortdesc"] = "Boots",
            ["item.iom_lightleggings.name"] = "Planetary Defence Force",
            ["item.iom_lightleggings.shortdesc"] = "Leggings",
            ["item.iom_lightChestplate.name"] = "Planetary Defence Force",
            ["item.iom_lightChestplate.shortdesc"] = "Armor",
            ["item.iom_lightHelmet.name"] = "Planetary Defence Force",
            ["item.iom_lightHelmet.shortdesc"] = "Helmet",
            ["item.iom_cadiaboots.name"] = "Cadia",
            ["item.iom_cadiaboots.shortdesc"] = "Boots",
            ["item.iom_cadialeggings.name"] = "Cadia",
            ["item.iom_cadialeggings.shortdesc"] = "Leggings",
            ["item.iom_cadiaChestplate.name"] = "Cadia",
            ["item.iom_cadiaChestplate.shortdesc"] = "Armor",
            ["item.iom_cadiaHelmet.name"] = "Cadia",
            ["item.iom_cadiaHelmet.shortdesc"] = "Helmet",
            ["faction.iom_faction.name"] = "Imperium of Man",
            ["faction.iom_faction.shortdesc"] = "The martyr's grave is the keystone of the Imperium.",
            ["faction.iom_faction.desc"] = "The Imperium of Man, also called simply the Imperium, is a galaxy-spanning, interstellar Human empire, the ultimate authority for the vast majority of the Human species in the Milky Way galaxy in the 41st Millennium A.D. It is ruled by the living god who is known as the Emperor of Mankind.",
            ["alliance.iom_alliance.name"] = "Imperium of Man",
            ["station.iomTerra.name"] = "Terra",
            ["station.iomTerra.type"] = "Holy Terra",
            ["station.iomMoom.name"] = "Moon",
            ["station.iomMoom.type"] = "Station",
            ["station.iomPhobos.name"] = "Phobos",
            ["station.iomPhobos.type"] = "Station",
            ["station.iomHavoc.name"] = "Havoc",
            ["station.iomHavoc.type"] = "Station"
        };

        private static readonly Dictionary<string, string> Russian = new Dictionary<string, string>
        {
            ["item.iom_lasgun.name"] = "Лазган",
            ["item.iom_lasgun.shortdesc"] = "Лазерная винтовка",
            ["item.iom_melta.name"] = "Мелта",
            ["item.iom_melta.shortdesc"] = "Мелта",
            ["item.iom_lassniper.name"] = "Long-Las",
            ["item.iom_lassniper.shortdesc"] = "Лазерная снайперская винтовка",
            ["item.iom_bolter.name"] = "Болтер",
            ["item.iom_bolter.shortdesc"] = "Болтер",
            ["item.iom_plasma_pistol.name"] = "Плазменный пистолет",
            ["item.iom_plasma_pistol.shortdesc"] = "Пистолет",
            ["item.iom_sevitor_backpack.name"] = "Сервитор",
            ["item.iom_sevitor_backpack.shortdesc"] = "Послушный Рюкзак",
            ["item.iom_lightboots.name"] = "Силы планетарной обороны",
            ["item.iom_lightboots.shortdesc"] = "Обувь",
            ["item.iom_lightleggings.name"] = "Силы планетарной обороны",
            ["item.iom_lightleggings.shortdesc"] = "Штаны",
            ["item.iom_lightChestplate.name"] = "Силы планетарной обороны",
            ["item.iom_lightChestplate.shortdesc"] = "Броня",
            ["item.iom_lightHelmet.name"] = "Силы планетарной обороны",
            ["item.iom_lightHelmet.shortdesc"] = "Шлем",
            ["item.iom_cadiaboots.name"] = "Кадия",
            ["item.iom_cadiaboots.shortdesc"] = "Обувь",
            ["item.iom_cadialeggings.name"] = "Кадия",
            ["item.iom_cadialeggings.shortdesc"] = "Штаны",
            ["item.iom_cadiaChestplate.name"] = "Кадия",
            ["item.iom_cadiaChestplate.shortdesc"] = "Броня",
            ["item.iom_cadiaHelmet.name"] = "Кадия",
            ["item.iom_cadiaHelmet.shortdesc"] = "Шлем",
            ["faction.iom_faction.name"] = "Империум Человечества",
            ["faction.iom_faction.shortdesc"] = "Галактическая империя, объединившая подавляющее большинство людей в галактике.",
            ["faction.iom_faction.desc"] = "Империум — самое большое государство в галактике, насчитывающее более миллиона звёздных систем, находящихся в Галактике Млечного Пути и разделённых между собой многими световыми годами. Столицей Империума является родина человечества Священная Терра.",
            ["alliance.iom_alliance.name"] = "Империум Человечества",
            ["station.iomTerra.name"] = "Терра",
            ["station.iomTerra.type"] = "Священная Терра",
            ["station.iomMoom.name"] = "Луна",
            ["station.iomMoom.type"] = "Станция",
            ["station.iomPhobos.name"] = "Фобос",
            ["station.iomPhobos.type"] = "Станция",
            ["station.iomHavoc.name"] = "Хавок",
            ["station.iomHavoc.type"] = "Станция"
        };

        [HarmonyPatch("LoadDB"), HarmonyPostfix]
        private static void LoadDBPostfix(Dictionary<Localization.Lang, Dictionary<string, string>> ___db)
        {
            if (___db == null)
            {
                return;
            }

            foreach (KeyValuePair<Localization.Lang, Dictionary<string, string>> language in ___db)
            {
                Dictionary<string, string> target = language.Value;
                if (target == null)
                {
                    continue;
                }

                Apply(target, language.Key == Localization.Lang.Russian ? Russian : English);
                ApplyStationAliases(target);
            }
        }

        private static void Apply(Dictionary<string, string> target, Dictionary<string, string> source)
        {
            foreach (KeyValuePair<string, string> translation in source)
            {
                target[translation.Key] = translation.Value;
            }
        }

        private static void ApplyStationAliases(Dictionary<string, string> target)
        {
            target["station.iomTerra.shortname"] = "TER";
            target["station.iomMoom.shortname"] = "MOO";
            target["station.iomPhobos.shortname"] = "PHB";
            target["station.iomHavoc.shortname"] = "HAV";

            // Preserve keys used by older builds/saves while the active records use iomMoom/iomHavoc.
            target["station.iomMoon.name"] = target["station.iomMoom.name"];
            target["station.iomMoon.type"] = target["station.iomMoom.type"];
            target["station.iomMoon.shortname"] = target["station.iomMoom.shortname"];
            target["station.iomVizg.name"] = target["station.iomHavoc.name"];
            target["station.iomVizg.type"] = target["station.iomHavoc.type"];
            target["station.iomVizg.shortname"] = target["station.iomHavoc.shortname"];
        }
    }
}

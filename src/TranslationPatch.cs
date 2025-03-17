using System;
using System.Collections.Generic;
using HarmonyLib;
using MGSC;
using UnityEngine;

namespace ImperiumOfMan
{
    [HarmonyPatch(typeof(Localization))]
    public static class TranslationPatch
    {
        [HarmonyPatch("LoadDB"), HarmonyPostfix]
        static void LoadDBPostfix(Dictionary<Localization.Lang, Dictionary<string, string>> ___db)
        {
            Debug.Log("Postfix patch runned");
            foreach (Localization.Lang lang in Enum.GetValues(typeof(Localization.Lang)))
            {
                ___db[lang]["station.iomTerra.shortname"] = "TER";
                ___db[lang]["station.iomMoom.shortname"] = "MOO";
                ___db[lang]["station.iomPhobos.shortname"] = "PHB";
                ___db[lang]["station.iomVizg.shortname"] = "HAV";
                if (lang == Localization.Lang.Russian)
                {
                    ___db[lang]["item.iom_lasgun.name"] = "Лазган";
                    ___db[lang]["item.iom_lasgun.shortdesc"] = "Лазерная винтовка";
                    ___db[lang]["item.iom_melta.name"] = "Мелта";
                    ___db[lang]["item.iom_melta.shortdesc"] = "Мелта";
                    ___db[lang]["item.iom_lassniper.name"] = "Long-Las";
                    ___db[lang]["item.iom_lassniper.shortdesc"] = "Лазерная снайперская винтовка";
                    ___db[lang]["item.iom_bolter.name"] = "Болтер";
                    ___db[lang]["item.iom_bolter.shortdesc"] = "Болтер";
                    ___db[lang]["item.iom_plasma_pistol.name"] = "Плазменный пистолет";
                    ___db[lang]["item.iom_plasma_pistol.shortdesc"] = "Пистолет";
                    ___db[lang]["item.iom_sevitor_backpack.name"] = "Сервитор";
                    ___db[lang]["item.iom_sevitor_backpack.shortdesc"] = "Послушный Рюкзак";
                    ___db[lang]["faction.iom_faction.name"] = "Империум Человечества";
                    ___db[lang]["station.iomTerra.name"] = "Терра";
                    ___db[lang]["station.iomTerra.type"] = "Священная Терра";
                    ___db[lang]["station.iomMoom.name"] = "Луна";
                    ___db[lang]["station.iomMoom.type"] = "Станция";
                    ___db[lang]["station.iomPhobos.name"] = "Фобос";
                    ___db[lang]["station.iomPhobos.type"] = "Станция";
                    ___db[lang]["station.iomVizg.name"] = "Визг";
                    ___db[lang]["station.iomVizg.type"] = "Станция";
                    
                    ___db[lang]["item.iom_lightboots.name"] = "Силы планетарной обороны";
                    ___db[lang]["item.iom_lightboots.shortdesc"] = "Обувь";
                    ___db[lang]["item.iom_lightleggings.name"] = "Силы планетарной обороны";
                    ___db[lang]["item.iom_lightleggings.shortdesc"] = "Штаны";
                    ___db[lang]["item.iom_lightChestplate.name"] = "Силы планетарной обороны";
                    ___db[lang]["item.iom_lightChestplate.shortdesc"] = "Броня";
                    ___db[lang]["item.lightHelmet.name"] = "Силы планетарной обороны";
                    ___db[lang]["item.lightHelmet.shortdesc"] = "Шлем";
                    
                    ___db[lang]["item.iom_cadiaboots.name"] = "Кадия";
                    ___db[lang]["item.iom_cadiaboots.shortdesc"] = "Обувь";
                    ___db[lang]["item.iom_cadialeggings.name"] = "Кадия";
                    ___db[lang]["item.iom_cadialeggings.shortdesc"] = "Штаны";
                    ___db[lang]["item.iom_cadiaChestplate.name"] = "Кадия";
                    ___db[lang]["item.iom_cadiaChestplate.shortdesc"] = "Броня";
                    ___db[lang]["item.cadiaHelmet.name"] = "Кадия";
                    ___db[lang]["item.cadiaHelmet.shortdesc"] = "Шлем";
                    
                    ___db[lang]["faction.iom_faction.shortdesc"] = "Галактическая империя, объединившая подавляющее большинство людей в галактике.";
                    ___db[lang]["faction.iom_faction.desc"] = "Империум — самое большое государство в галактике, насчитывающее более миллиона звёздных систем, находящихся в Галактике Млечного Пути и раздёленных между собой многими световыми годами. Размер Империума как межзвёздного государства исчисляется не протяжённостью территорий, а именно звёздными системами, которые он контролирует. Столицей Империума является родина человечества Священная Терра (Земля).";
                    
                    ___db[lang]["alliance.iom_alliance.name"] = "Империум Человечества";
                    ___db[lang]["alliance.iom_alliance.name"] = "Империум Человечества";
                }
                else
                {
                    ___db[lang]["item.iom_lasgun.name"] = "Lasgun";
                    ___db[lang]["item.iom_lasgun.shortdesc"] = "Laser Rifle";
                    ___db[lang]["item.iom_melta.name"] = "Melta Rifle";
                    ___db[lang]["item.iom_melta.shortdesc"] = "Melta Rifle";
                    ___db[lang]["item.iom_lassniper.name"] = "Long-Las";
                    ___db[lang]["item.iom_lassniper.shortdesc"] = "Laser Sniper Rifle";
                    ___db[lang]["item.iom_bolter.name"] = "Bolter";
                    ___db[lang]["item.iom_bolter.shortdesc"] = "Bolter";
                    ___db[lang]["item.iom_plasma_pistol.name"] = "Plasma Pistol";
                    ___db[lang]["item.iom_plasma_pistol.shortdesc"] = "Pistol";
                    ___db[lang]["item.iom_sevitor_backpack.name"] = "Servitor";
                    ___db[lang]["item.iom_sevitor_backpack.shortdesc"] = "Your Backpack";
                    ___db[lang]["faction.iom_faction.name"] = "Imperium of Man";
                    ___db[lang]["station.iomTerra.name"] = "Terra";
                    ___db[lang]["station.iomTerra.type"] = "Holy Terra";
                    ___db[lang]["station.iomMoon.name"] = "Moon";
                    ___db[lang]["station.iomMoon.type"] = "Station";
                    ___db[lang]["station.iomPhobos.name"] = "Phobos";
                    ___db[lang]["station.iomPhobos.type"] = "Station";
                    ___db[lang]["station.iomVizg.name"] = "Havoc";
                    ___db[lang]["station.iomVizg.type"] = "Station";
                    
                    ___db[lang]["item.iom_lightboots.name"] = "Planetary Defence Force";
                    ___db[lang]["item.iom_lightboots.shortdesc"] = "Boots";
                    ___db[lang]["item.iom_lightleggings.name"] = "Planetary Defence Force";
                    ___db[lang]["item.iom_lightleggings.shortdesc"] = "Leggings";
                    ___db[lang]["item.iom_lightChestplate.name"] = "Planetary Defence Force";
                    ___db[lang]["item.iom_lightChestplate.shortdesc"] = "Armor";
                    ___db[lang]["item.lightHelmet.name"] = "Planetary Defence Force";
                    ___db[lang]["item.lightHelmet.shortdesc"] = "Helmet";
                    
                    ___db[lang]["item.iom_cadiaboots.name"] = "Cadia";
                    ___db[lang]["item.iom_cadiaboots.shortdesc"] = "Boots";
                    ___db[lang]["item.iom_cadialeggings.name"] = "Cadia";
                    ___db[lang]["item.iom_cadialeggings.shortdesc"] = "Leggings";
                    ___db[lang]["item.iom_cadiaChestplate.name"] = "Cadia";
                    ___db[lang]["item.iom_cadiaChestplate.shortdesc"] = "Armor";
                    ___db[lang]["item.cadiaHelmet.name"] = "Cadia";
                    ___db[lang]["item.cadiaHelmet.shortdesc"] = "Helmet";
                    
                    ___db[lang]["faction.iom_faction.shortdesc"] = "The martyr's grave is the keystone of the Imperium.";
                    ___db[lang]["faction.iom_faction.desc"] = "The Imperium of Man, also called simply the Imperium, is a galaxy-spanning, interstellar Human empire, the ultimate authority for the vast majority of the Human species in the Milky Way galaxy in the 41st Millennium A.D. It is ruled by the living god who is known as the Emperor of Mankind.";
                    
                    ___db[lang]["alliance.iom_alliance.name"] = "Imperium of Man";
                    ___db[lang]["alliance.iom_alliance.name"] = "Imperium of Man";
                }
            }
        }
    }
}
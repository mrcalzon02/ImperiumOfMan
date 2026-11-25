using System.Collections.Generic;
using HarmonyLib;
using MGSC;

namespace ImperiumOfMan
{
    [HarmonyPatch(typeof(Localization))]
    public static class TranslationPatch
    {
        internal const string CampaignStandingRequired = "iom.campaign.standing.required";
        internal const string CampaignLiftLocked = "iom.campaign.lift.locked";
        internal const string FloorNameTemplate = "mercurySurfaceLevels";
        internal const string ArrivalShuttleCaption = "mission." + FloorNameTemplate + ".arrival.name";
        internal const string CatalogRecipes = "iom.catalog.recipes";

        private static readonly Dictionary<string, string> English = new Dictionary<string, string>
        {
            [CampaignStandingRequired] = "This campaign requires at least +{0} reputation with {1}.",
            [CampaignLiftLocked] = "Find the {0} on this deck to unlock the lift.",
            [ArrivalShuttleCaption] = "Arrival Shuttle",
            [CatalogRecipes] = "Unlocks these Magnum recipes",
            ["item.iom_lasgun.name"] = "Lasgun",
            ["item.iom_lasgun.shortdesc"] = "A Guard service rifle with single shots and two-shot bursts. Takes Las Clips and compatible power cells.",
            ["item.iom_melta.name"] = "Melta Rifle",
            ["item.iom_melta.shortdesc"] = "A short-range melta weapon that projects intense plasma heat. Uses gas ammunition.",
            ["item.iom_lassniper.name"] = "Long-Las",
            ["item.iom_lassniper.shortdesc"] = "A scoped laser rifle for long-range shots. Takes Las Clips and compatible power cells.",
            ["item.iom_bolter.name"] = "Bolter",
            ["mission.iom_sororitas_reliquary_recovery.name"] = "Sororitas Reliquary Recovery",
            ["mission.iom_sororitas_reliquary_recovery.details"] = "The Battle Sisters are holding the inner precinct at %STATION_NAME%. The raiders are searching for a sealed Sororitas reliquary.",
            ["mission.iom_sororitas_reliquary_recovery.desc"] = "Recover the Sororitas Reliquary and extract with it. A Battle Sister and a flamer specialist are on the final floor.",
            ["mission.iom_sororitas_reliquary_recovery.briefing"] = "The Battle Sisters are holding the inner precinct at %STATION_NAME%. Reach them before the raiders break through, then bring the sealed reliquary back aboard.",
            ["mission.iom_sororitas_reliquary_recovery.objective0"] = "Recover the sealed Sororitas Reliquary and extract with it.",
            ["mission.iom_sororitas_reliquary_recovery.after"] = "The reliquary is secure aboard the Magnum. The Battle Sisters have asked us to return it to their keeping at the Golden Throne.",
            ["mission.iom_phobos_quarantine.name"] = "Phobos Quarantine",
            ["mission.iom_phobos_quarantine.details"] = "The Ordo Xenos has traced an unclassified specimen to a private research crew at %STATION_NAME%. The crew is preparing to ship it off-station.",
            ["mission.iom_phobos_quarantine.desc"] = "Destroy the marked containment equipment and extract. An Ordo Xenos Interrogator and Pistoleer will support you on the final floor.",
            ["mission.iom_phobos_quarantine.briefing"] = "Ordo Xenos has sealed the exits at %STATION_NAME%. The research crew still has its specimen in containment. Meet the Interrogator and destroy the marked equipment before the crew can move it.",
            ["mission.iom_phobos_quarantine.objective0"] = "Destroy the marked containment equipment and extract.",
            ["mission.iom_phobos_quarantine.after"] = "The containment equipment is down. The Interrogator has called off the shipment and ordered the research bay sealed for inspection.",
            ["item.iom_bolter.shortdesc"] = "An Imperial boltgun with single shots and four-round bursts. Takes Bolt Clips and other Heavy ammunition.",
            ["item.iom_plasma_pistol.name"] = "Plasma Pistol",
            ["item.iom_plasma_pistol.shortdesc"] = "A compact plasma sidearm for close engagements. Runs on power cells.",
            ["item.iom_sevitor_backpack.name"] = "Servitor Backpack",
            ["item.iom_sevitor_backpack.shortdesc"] = "A powered pack that lightens its contents and assists with weapon reloads.",
            ["item.iom_lightboots.name"] = "PDF Field Boots",
            ["item.iom_lightboots.shortdesc"] = "Reinforced patrol boots issued to Planetary Defence Force troops.",
            ["item.iom_lightleggings.name"] = "PDF Flak Leggings",
            ["item.iom_lightleggings.shortdesc"] = "Flexible leg protection with a padded lining for exposed patrols.",
            ["item.iom_lightChestplate.name"] = "PDF Flak Chestplate",
            ["item.iom_lightChestplate.shortdesc"] = "A light flak chestplate for local garrison troops.",
            ["item.iom_lightHelmet.name"] = "PDF Flak Helmet",
            ["item.iom_lightHelmet.shortdesc"] = "A standard-issue garrison helmet with a reinforced shell.",
            ["item.iom_cadiaboots.name"] = "Cadian Field Boots",
            ["item.iom_cadiaboots.shortdesc"] = "Cadian infantry boots with sturdy protection around the foot and ankle.",
            ["item.iom_cadialeggings.name"] = "Cadian Flak Leggings",
            ["item.iom_cadialeggings.shortdesc"] = "Layered flak leggings for Cadian line infantry.",
            ["item.iom_cadiaChestplate.name"] = "Cadian Flak Chestplate",
            ["item.iom_cadiaChestplate.shortdesc"] = "A Cadian field chestplate reinforced across the torso.",
            ["item.iom_cadiaHelmet.name"] = "Cadian Flak Helmet",
            ["item.iom_cadiaHelmet.shortdesc"] = "A Cadian infantry helmet with a reinforced brow and cheek guards.",
            ["faction.iom_faction.name"] = "Imperium of Man",
            ["faction.iom_faction.shortdesc"] = "The martyr's grave is the keystone of the Imperium.",
            ["faction.iom_faction.desc"] = "The Imperium of Man, also called simply the Imperium, is a galaxy-spanning, interstellar Human empire, the ultimate authority for the vast majority of the Human species in the Milky Way galaxy in the 41st Millennium A.D. It is ruled by the living god who is known as the Emperor of Mankind.",
            ["alliance.iom_alliance.name"] = "Imperium of Man",
            ["station.iomTerra.name"] = "Golden Throne",
            ["station.iomTerra.type"] = "Imperial Capital, Himalayas",
            ["station.iomTerra.desc"] = "The Golden Throne lies within the Imperial Palace in Earth's Himalayas. Palace workshops press armor inserts and fit liners for the garrison. The Adeptus Arbites secure the outer precincts.",
            ["station.iomTitan.name"] = "Titan Fortress-Monastery",
            ["station.iomTitan.type"] = "Grey Knights Fortress-Monastery",
            ["station.iomTitan.desc"] = "The Grey Knights' stronghold on the surface of Saturn's moon Titan guards the Chapter's sealed vaults. Its workshops reinforce armor and service specialist equipment. The fortress commissions vault containment at Imperial technology level 10.",
            ["station.iomBroadsword.name"] = "Broadsword Station",
            ["station.iomBroadsword.type"] = "Grey Knights Orbital Fleet Base",
            ["station.iomBroadsword.desc"] = "The Grey Knights' fleet base in orbit over Titan berths and resupplies the Chapter's ships. Naval workshops supply arms and armor repair kits. Boarding defense contracts open at Imperial technology level 10.",
            ["iom.contract.titan.name"] = "Titan Vault Containment",
            ["iom.contract.broadsword.name"] = "Broadsword Boarding Defense",
            ["iom.contract.golden.name"] = "Golden Paladin Guard",
            ["station.iomMoom.name"] = "Luna",
            ["station.iomMoom.type"] = "Orbital Provisioning Station",
            ["station.iomMoom.desc"] = "Luna's provisioning station on Fuller makes Corpse Starch and Las Clips. Its farm and cargo bays receive rations, water and biomass. The Valhallan detachment keeps winter supplies in the station stores.",
            ["station.iomPhobos.name"] = "Phobos",
            ["station.iomPhobos.type"] = "Imperial Munitions Depot",
            ["station.iomPhobos.desc"] = "A military depot and cargo port on Phobos. Its production crews use glass and acid to make Las Clips and Guard Frag Grenades. The port distributes field plans and cell components.",
            ["station.iomHavoc.name"] = "Havoc",
            ["station.iomHavoc.type"] = "Imperial Fabrication Works",
            ["station.iomHavoc.desc"] = "Havoc's orbital workshops shape refined metal and polymer into fittings and rolled plate. The Steel Legion draws seal lining and respirator supplies from the station stores.",
            ["station.iomFoundryPrimaris.name"] = "Foundry Primaris",
            ["station.iomFoundryPrimaris.type"] = "Mechanicus Foundry",
            ["station.iomFoundryPrimaris.desc"] = "The Mechanicus foundry on Mars receives raw ore through its freight port and produces refined metal and rolled plate for Imperial workshops.",
            ["station.iomFoundryPrimaris.briefing"] = "Foundry Primaris to incoming vessel. The Martian forge is open for Imperial requisitions.",
            ["spaceobject.iomNeith.name"] = "Neith",
            ["spaceobject.iomNeith.shortdesc"] = "Artificial moon of Venus",
            ["spaceobject.iomNeith.desc"] = "Neith is a fortified Mechanicus seed vault. Its guarded archives hold samples of every known plant species in the Imperium.",
            ["station.iomNeithVault.name"] = "Neith Seed Vault",
            ["station.iomNeithVault.type"] = "Mechanicus Seed Vault",
            ["station.iomNeithVault.desc"] = "This orbital lab preserves the Imperium's seed archive. Its propagation bays use water to grow vat biomass.",
            ["station.iomNeithVault.briefing"] = "Neith Seed Vault to incoming vessel. Follow the marked approach and keep clear of the archive.",
            ["station.iomMeridianWaterworks.name"] = "Meridian Waterworks",
            ["station.iomMeridianWaterworks.type"] = "Water Extraction Terminal",
            ["station.iomMeridianWaterworks.desc"] = "The terminal in Earth orbit receives water from surface intake stations, treats it and loads it into bulk containers for farms and settlements.",
            ["station.iomGreenlineProvisions.name"] = "Greenline Provisions",
            ["station.iomGreenlineProvisions.type"] = "Orbital Farm and Ration Works",
            ["station.iomGreenlineProvisions.desc"] = "Water deliveries feed the growing racks. The packing line turns harvested biomass into rations for stations around Earth.",
            ["station.iomJunctionReclamation.name"] = "Junction Reclamation",
            ["station.iomJunctionReclamation.type"] = "Recycling and Tool Works",
            ["station.iomJunctionReclamation.desc"] = "A freight yard in Earth orbit sorts scrap into metal cargo. Repair crews use recovered metal and engineering parts to rebuild tools for other workshops.",
            ["station.iomCinderBasinMine.name"] = "Cinder Basin Mine",
            ["station.iomCinderBasinMine.type"] = "Ore and Ice Mine",
            ["station.iomCinderBasinMine.desc"] = "Excavators cut metal ore and buried ice from the basin on Mars. The loading yard ships ore to fabrication works and water to farms.",
            ["station.iomHesperiaFabrication.name"] = "Hesperia Fabrication",
            ["station.iomHesperiaFabrication.type"] = "Industrial Parts Works",
            ["station.iomHesperiaFabrication.desc"] = "These Martian workshops process ore into engineering parts. The electrical line consumes ore and tools to supply electrical parts to nearby stations.",
            ["station.iomVallisGrowworks.name"] = "Vallis Growworks",
            ["station.iomVallisGrowworks.type"] = "Orbital Farm and Ration Works",
            ["station.iomVallisGrowworks.desc"] = "Bulk water from Mars supplies the growing racks in orbit. The station exports biomass tanks and packs rations for the Martian mining settlements.",
        };

        private static readonly Dictionary<string, string> Russian = new Dictionary<string, string>
        {
            [ArrivalShuttleCaption] = "Десантный шаттл",
            ["item.iom_lasgun.name"] = "Лазган",
            ["item.iom_lasgun.shortdesc"] = "Лазерная винтовка Гвардии. Ведёт огонь одиночными выстрелами и короткими очередями.",
            ["item.iom_melta.name"] = "Мелта",
            ["item.iom_melta.shortdesc"] = "Мельта для ближнего боя. Использует газовые боеприпасы.",
            ["item.iom_lassniper.name"] = "Long-Las",
            ["item.iom_lassniper.shortdesc"] = "Лазерная винтовка с оптическим прицелом для дальних выстрелов.",
            ["item.iom_bolter.name"] = "Болтер",
            ["item.iom_bolter.shortdesc"] = "Имперский болтер с одиночным огнём и очередями по четыре выстрела.",
            ["item.iom_plasma_pistol.name"] = "Плазменный пистолет",
            ["item.iom_plasma_pistol.shortdesc"] = "Компактный плазменный пистолет для ближнего боя. Питается от батарей.",
            ["item.iom_sevitor_backpack.name"] = "Рюкзак-сервитор",
            ["item.iom_sevitor_backpack.shortdesc"] = "Снижает вес груза и помогает быстрее перезаряжать оружие.",
            ["item.iom_lightboots.name"] = "Полевые ботинки СПО",
            ["item.iom_lightboots.shortdesc"] = "Усиленные ботинки для патрулей сил планетарной обороны.",
            ["item.iom_lightleggings.name"] = "Бронештаны СПО",
            ["item.iom_lightleggings.shortdesc"] = "Гибкая защита ног с мягкой подкладкой для полевых патрулей.",
            ["item.iom_lightChestplate.name"] = "Бронежилет СПО",
            ["item.iom_lightChestplate.shortdesc"] = "Лёгкий бронежилет для войск местного гарнизона.",
            ["item.iom_lightHelmet.name"] = "Шлем СПО",
            ["item.iom_lightHelmet.shortdesc"] = "Штатный гарнизонный шлем с усиленным корпусом.",
            ["item.iom_cadiaboots.name"] = "Кадианские ботинки",
            ["item.iom_cadiaboots.shortdesc"] = "Кадианские пехотные ботинки с усиленной защитой стопы и лодыжки.",
            ["item.iom_cadialeggings.name"] = "Кадианские бронештаны",
            ["item.iom_cadialeggings.shortdesc"] = "Многослойная защита ног для кадианской линейной пехоты.",
            ["item.iom_cadiaChestplate.name"] = "Кадианский бронежилет",
            ["item.iom_cadiaChestplate.shortdesc"] = "Кадианский полевой бронежилет с усиленной защитой корпуса.",
            ["item.iom_cadiaHelmet.name"] = "Кадианский шлем",
            ["item.iom_cadiaHelmet.shortdesc"] = "Кадианский пехотный шлем с усиленной защитой лба и щёк.",
            ["faction.iom_faction.name"] = "Империум Человечества",
            ["faction.iom_faction.shortdesc"] = "Галактическая империя, объединившая подавляющее большинство людей в галактике.",
            ["faction.iom_faction.desc"] = "Империум — самое большое государство в галактике, насчитывающее более миллиона звёздных систем, находящихся в Галактике Млечного Пути и разделённых между собой многими световыми годами. Столицей Империума является родина человечества Священная Терра.",
            ["alliance.iom_alliance.name"] = "Империум Человечества",
            ["station.iomTerra.name"] = "Золотой Трон",
            ["station.iomTerra.type"] = "Имперская столица, Гималаи",
            ["station.iomTerra.desc"] = "Золотой Трон находится в Императорском дворце в Гималаях на Терре. Дворцовые мастерские изготавливают броневставки и подкладки для гарнизона. Адептус Арбитрес охраняют внешние кварталы.",
            ["station.iomTitan.name"] = "Крепость-монастырь Титана",
            ["station.iomTitan.type"] = "Крепость-монастырь Серых Рыцарей",
            ["station.iomTitan.desc"] = "На поверхности Титана Серые Рыцари охраняют закрытые хранилища ордена. Мастерские укрепляют броню и обслуживают специальное снаряжение. Задания по защите хранилищ открываются на 10-м уровне технологий Империума.",
            ["station.iomBroadsword.name"] = "Станция «Бродсворд»",
            ["station.iomBroadsword.type"] = "Орбитальная база флота Серых Рыцарей",
            ["station.iomBroadsword.desc"] = "База флота Серых Рыцарей на орбите Титана принимает и снабжает корабли ордена. Мастерские выпускают комплекты для ремонта оружия и брони. Задания по отражению абордажа открываются на 10-м уровне технологий Империума.",
            ["iom.contract.titan.name"] = "Защита хранилища Титана",
            ["iom.contract.broadsword.name"] = "Отражение абордажа на «Бродсворде»",
            ["iom.contract.golden.name"] = "Гвардия Золотого паладина",
            ["station.iomMoom.name"] = "Луна",
            ["station.iomMoom.type"] = "Орбитальная станция снабжения",
            ["station.iomMoom.desc"] = "Станция снабжения на Фуллере выпускает трупный крахмал и лазерные батареи. Её фермы и склады принимают рационы, воду и биомассу. На станции хранятся зимние припасы валльхалльцев.",
            ["station.iomPhobos.name"] = "Фобос",
            ["station.iomPhobos.type"] = "Имперский склад боеприпасов",
            ["station.iomPhobos.desc"] = "Военный склад и грузовой порт на Фобосе. Мастерские используют стекло и кислоту для выпуска лазерных батарей и осколочных гранат Гвардии.",
            ["station.iomHavoc.name"] = "Хавок",
            ["station.iomHavoc.type"] = "Имперский завод",
            ["station.iomHavoc.desc"] = "Орбитальные цеха Хавока выпускают крепёж и бронелисты из очищенного металла и полимера. Склады снабжают Стальной легион герметизирующими материалами и фильтрами.",
            ["station.iomMeridianWaterworks.name"] = "Водный терминал «Меридиан»",
            ["station.iomMeridianWaterworks.type"] = "Терминал водоснабжения",
            ["station.iomMeridianWaterworks.desc"] = "Терминал на орбите Земли принимает воду с поверхности, очищает её и отправляет на фермы и в поселения.",
            ["station.iomGreenlineProvisions.name"] = "Ферма «Гринлайн»",
            ["station.iomGreenlineProvisions.type"] = "Орбитальная ферма и упаковка пайков",
            ["station.iomGreenlineProvisions.desc"] = "Из доставляемой воды выращивают биомассу. Упаковочная линия выпускает пайки для земных станций.",
            ["station.iomJunctionReclamation.name"] = "Утилизационный узел «Джанкшен»",
            ["station.iomJunctionReclamation.type"] = "Переработка и выпуск инструментов",
            ["station.iomJunctionReclamation.desc"] = "Орбитальный грузовой двор сортирует металлолом. В цехах из металла и инженерных деталей собирают инструменты.",
            ["station.iomCinderBasinMine.name"] = "Шахта «Пепельная котловина»",
            ["station.iomCinderBasinMine.type"] = "Рудная и ледовая шахта",
            ["station.iomCinderBasinMine.desc"] = "В марсианской котловине добывают руду и лёд. Грузовой двор снабжает рудой заводы, а водой фермы.",
            ["station.iomHesperiaFabrication.name"] = "Завод «Гесперия»",
            ["station.iomHesperiaFabrication.type"] = "Завод промышленных деталей",
            ["station.iomHesperiaFabrication.desc"] = "Из руды завод выпускает инженерные детали. Электротехническая линия расходует руду и инструменты.",
            ["station.iomVallisGrowworks.name"] = "Ферма «Валлис»",
            ["station.iomVallisGrowworks.type"] = "Орбитальная ферма и упаковка пайков",
            ["station.iomVallisGrowworks.desc"] = "Вода с Марса питает орбитальные установки. Станция поставляет биомассу и пайки марсианским шахтёрским поселениям."
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

                Apply(target, English);
                if (language.Key == Localization.Lang.Russian)
                {
                    Apply(target, Russian);
                }
                ApplyStationAliases(target);
                foreach (string key in LegacyEquipmentDescriptions)
                {
                    target[key + ".desc"] = target[key + ".shortdesc"];
                }
            }
        }

        internal static void AddItem(string id, string name, string description)
        {
            English["item." + id + ".name"] = name;
            English["item." + id + ".shortdesc"] = description ?? name;
            English["item." + id + ".desc"] = description ?? name;
        }

        private static readonly string[] LegacyEquipmentDescriptions =
        [
            "item.iom_lasgun", "item.iom_melta", "item.iom_lassniper", "item.iom_bolter", "item.iom_plasma_pistol",
            "item.iom_sevitor_backpack", "item.iom_lightboots", "item.iom_lightleggings", "item.iom_lightChestplate",
            "item.iom_lightHelmet", "item.iom_cadiaboots", "item.iom_cadialeggings", "item.iom_cadiaChestplate", "item.iom_cadiaHelmet"
        ];

        internal static void AddRole(string id, string name)
        {
            English["monster." + id + ".name"] = name;
            English["monster." + id + "_male.name"] = name;
            English["monster." + id + "_female.name"] = name;
        }

        internal static void AddMachineCaption(string tag, string name) => English[tag] = name;

        internal static void AddImplantPerk(string id, string name, string description)
        {
            English["perk." + id + ".name"] = name;
            English["perk." + id + ".desc"] = description;
        }

        internal static void AddMercenaryClass(string id, string name, string description)
        {
            English["class." + id + ".name"] = name;
            English["class." + id + ".desc"] = description;
        }

        internal static void AddAugmentationSlot(string id, string name) =>
            English["woundslot." + id + ".name"] = name;

        internal static void AddChaosFaction(string faction, string alliance)
        {
            English["faction." + faction + ".name"] = "Chaos Undivided";
            English["faction." + faction + ".shortdesc"] = "The host of the Dark Master.";
            English["faction." + faction + ".desc"] = "Cult cells and Soulforge workshops arm Be’lakor’s host with materiel stripped from the breach.";
            English["alliance." + alliance + ".name"] = "Chaos Undivided";
            English["pact." + faction + ".subName"] = "Chaos Undivided";
            English["iom.chaos.anchor_broken"] = "A shadow anchor shatters. Be’lakor’s shroud weakens.";
            English["iom.chaos.shadow_warning"] = "Be’lakor marks the floor. Move away from the marked tile before the shadow falls.";
        }

        internal static void AddChaosPact(string id, string name, string description)
        {
            English["perk." + id + ".name"] = name;
            English["perk." + id + ".desc"] = description;
        }

        internal static void AddChaosCurse(string id, string name)
        {
            English["curse." + id + ".name"] = name;
            English["curse." + id + ".desc"] = name;
        }

        internal static void AddMagnumUpgrade(string id, string name, string description)
        {
            English["mgperk." + id + ".name"] = name;
            English["mgperk." + id + ".subName"] = description;
        }

        internal static void AddShipSupportText(string key, string text) => English[key] = text;

        internal static void AddStation(string id, string name, string shortName, string type, string description)
        {
            string key = "station." + id;
            English[key + ".name"] = name;
            English[key + ".shortname"] = shortName;
            English[key + ".type"] = type;
            English[key + ".desc"] = description;
        }

        internal static void AddFringeBody(string id, string name, string description)
        {
            string key = "spaceobject." + id;
            English[key + ".name"] = name;
            English[key + ".shortdesc"] = "Outer-fringe asteroid";
            English[key + ".desc"] = description;
        }

        internal static void AddCampaignQuest(string id, string name, string briefing, string objective, string report, string strategy,
            string alternateReport = null)
        {
            string story = "story." + id;
            English[story + ".name"] = name;
            English[story + ".briefing"] = briefing;
            English[story + ".details"] = briefing;
            English[story + ".desc"] = objective;
            English[story + ".objective0"] = objective;
            English[story + ".objective1"] = "Objective complete. Return to extraction.";
            English[story + ".after"] = report;
            if (alternateReport != null)
            {
                English[story + ".after_alternate"] = alternateReport;
            }
            English["strategy." + id + ".name"] = name;
            English["strategy." + id + ".desc"] = strategy;
        }

        internal static void AddCampaignDeckNames(string id, string entry, string middle, string final)
        {
            string deck = "story." + id + ".deck.";
            English[deck + "entry.name"] = entry;
            English[deck + "middle.name"] = middle;
            English[deck + "final.name"] = final;
            // The native generator supports up to seven floors with custom difficulty.
            for (int floor = 2; floor <= 7; floor++)
            {
                English[$"{deck}stage{floor}.name"] = $"{middle} {floor}";
            }
        }

        internal static void ApplyToLoadedLanguages()
        {
            LoadDBPostfix(Singleton<Localization>.Instance.db);
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
            target["station.iomTitan.shortname"] = "TIT";
            target["station.iomBroadsword.shortname"] = "BRS";
            target["station.iomMoom.shortname"] = "MOO";
            target["station.iomPhobos.shortname"] = "PHB";
            target["station.iomHavoc.shortname"] = "HAV";
            target["station.iomFoundryPrimaris.shortname"] = "FPR";
            target["station.iomNeithVault.shortname"] = "NEI";
            target["station.iomMeridianWaterworks.shortname"] = "MDW";
            target["station.iomGreenlineProvisions.shortname"] = "GLP";
            target["station.iomJunctionReclamation.shortname"] = "JRC";
            target["station.iomCinderBasinMine.shortname"] = "CBM";
            target["station.iomHesperiaFabrication.shortname"] = "HPF";
            target["station.iomVallisGrowworks.shortname"] = "VWG";

            // Preserve keys used by older builds/saves while the active records use iomMoom/iomHavoc.
            target["station.iomMoon.name"] = target["station.iomMoom.name"];
            target["station.iomMoon.type"] = target["station.iomMoom.type"];
            target["station.iomMoon.desc"] = target["station.iomMoom.desc"];
            target["station.iomMoon.shortname"] = target["station.iomMoom.shortname"];
            target["station.iomVizg.name"] = target["station.iomHavoc.name"];
            target["station.iomVizg.type"] = target["station.iomHavoc.type"];
            target["station.iomVizg.desc"] = target["station.iomHavoc.desc"];
            target["station.iomVizg.shortname"] = target["station.iomHavoc.shortname"];
        }
    }
}

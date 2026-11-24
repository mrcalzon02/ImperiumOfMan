using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MGSC;
using UnityEngine;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private const string Judgement = "iom_judgement";
        private const string ShockMaul = "iom_arbites_shock_maul";
        private const string ArbitesHelmet = "iom_arbites_helmet";
        private const string ArbitesCuirass = "iom_arbites_cuirass";
        private const string ArbitesGreaves = "iom_arbites_greaves";
        private const string ArbitesBoots = "iom_arbites_boots";
        private const string SubductorCuirass = "iom_arbites_subductor_cuirass";
        private const string VigilantRole = "iom_arbites_vigilant";
        private const string SubductorRole = "iom_arbites_subductor";
        private const string TerraStation = "iomTerra";
        private const string TerraArbitesTable = "iomTerraArbitesSecurity";
        private const string TerraArbitesGarrison = "iomTerraArbitesGarrison";
        private const string TerraSecurityOffered = "iom.arbites.terra_security_offered";
        private const int ArbitesTier = 3;
        private const float ArbitesPatrolPoints = 100f;
        private static readonly Dictionary<(Texture Source, ArbitesFinish Finish), Texture2D> ArbitesTextures = new();

        private enum ArbitesFinish { Carapace, Subductor, Judgement, ShockMaul }

        private static void RegisterArbitesEquipment()
        {
            AddCarapace<HelmetRecord>("helmets", ArbitesHelmet, "police_medium_helmet_1", "ArbitesHelmet", "Arbites Helmet", 135);
            AddCarapace<ArmorRecord>("armors", ArbitesCuirass, "police_medium_armor_1", "ArbitesCuirass", "Vigilant Cuirass", 225);
            AddCarapace<LeggingsRecord>("leggings", ArbitesGreaves, "police_medium_pants_1", "ArbitesGreaves", "Arbites Greaves", 170);
            AddCarapace<BootsRecord>("boots", ArbitesBoots, "police_medium_boots_1", "ArbitesBoots", "Arbites Boots", 105);
            AddCarapace<ArmorRecord>("armors", SubductorCuirass, "police_heavy_armor_1", "SubductorCuirass", "Subductor Cuirass", 320);

            var shotgun = CloneItem<WeaponRecord>(Judgement, "common_shotgun_1", ArbitesTier, 270, "Judgement");
            SetDamage(shotgun, 28, 60);
            shotgun.MagazineCapacity = 5;
            ApplyWeaponFinish(shotgun, ArbitesFinish.Judgement);
            RegisterItem("rangeweapons", shotgun, "Judgement", "Compact Arbites combat shotgun chambered for standard shells.");

            var maul = CloneItem<WeaponRecord>(ShockMaul, "fra_police_club_1", ArbitesTier, 205, "ShockMaul");
            SetDamage(maul, 23, 31);
            ((WeaponDescriptor)maul.ContentDescriptor)._grip = HandsGrip.MeleeOneHanded;
            ApplyWeaponFinish(maul, ArbitesFinish.ShockMaul);
            RegisterItem("meleeweapons", maul, "Arbites Shock Maul", "Battery-powered one-handed shock weapon issued to Subductors.");
        }

        private static void AddCarapace<T>(string table, string id, string donor, string artwork, string name, float price)
            where T : ResistRecord
        {
            var armor = CloneItem<T>(id, donor, ArbitesTier, price, artwork);
            armor.ResistSheet = [.. armor.ResistSheet];
            armor.SetResist("lacer", (float)Math.Round(armor.GetResist("lacer") * 1.33f));
            switch (armor.ContentDescriptor)
            {
                case HelmetDescriptor helmet:
                    helmet._parts = RecolorArmorParts(helmet.Parts);
                    break;
                case ArmorDescriptor cuirass:
                    cuirass._parts = RecolorArmorParts(cuirass.Parts,
                        id == SubductorCuirass ? ArbitesFinish.Subductor : ArbitesFinish.Carapace);
                    break;
                case LeggingsDescriptor greaves:
                    greaves._parts = RecolorArmorParts(greaves.Parts);
                    break;
                case BootsDescriptor boots:
                    boots._parts = RecolorArmorParts(boots.Parts);
                    break;
            }
            RegisterItem(table, armor, name, "Overlapping blackened steel plates and an oxblood liner add laceration resistance.");
        }

        private static void ApplyWeaponFinish(WeaponRecord weapon, ArbitesFinish finish)
        {
            var descriptor = (WeaponDescriptor)weapon.ContentDescriptor;
            descriptor._texture = GetArbitesTexture(descriptor.Texture, finish);
        }

        private static List<ArmorPartInfo> RecolorArmorParts(List<ArmorPartInfo> parts, ArbitesFinish finish = ArbitesFinish.Carapace)
        {
            var recolored = new List<ArmorPartInfo>(parts.Count);
            foreach (var original in parts)
            {
                var part = original;
                part.Texture = GetArbitesTexture(part.Texture, finish);
                recolored.Add(part);
            }
            return recolored;
        }

        private static Texture GetArbitesTexture(Texture source, ArbitesFinish finish)
        {
            if (source == null)
            {
                return null;
            }
            if (!ArbitesTextures.TryGetValue((source, finish), out var texture))
            {
                texture = RecolorTexture(source, finish);
                ArbitesTextures.Add((source, finish), texture);
            }
            return texture;
        }

        private static Texture2D RecolorTexture(Texture original, ArbitesFinish finish)
        {
            var previous = RenderTexture.active;
            var target = RenderTexture.GetTemporary(original.width, original.height, 0, RenderTextureFormat.ARGB32);
            var result = new Texture2D(original.width, original.height, TextureFormat.RGBA32, false)
            {
                name = "Arbites " + finish + " " + original.name,
                filterMode = FilterMode.Point,
                wrapMode = original.wrapMode
            };
            try
            {
                Graphics.Blit(original, target);
                RenderTexture.active = target;
                result.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0, false);
            }
            finally
            {
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(target);
            }

            Color32[] pixels = result.GetPixels32();
            for (int index = 0; index < pixels.Length; index++)
            {
                Color32 pixel = pixels[index];
                float light = (0.21f * pixel.r + 0.72f * pixel.g + 0.07f * pixel.b) / 255f;
                bool warm = pixel.r > pixel.b + 12 && pixel.r > pixel.g + 8;
                Color shaded = finish switch
                {
                    ArbitesFinish.Judgement when light >= 0.25f =>
                        Color.Lerp(new Color(0.20f, 0.045f, 0.035f), new Color(0.85f, 0.40f, 0.25f), light),
                    ArbitesFinish.ShockMaul when pixel.b > pixel.r + 26 && pixel.g > pixel.r + 16 =>
                        Color.Lerp(new Color(0.07f, 0.22f, 0.25f), new Color(0.65f, 0.93f, 0.98f), light),
                    ArbitesFinish.Subductor when warm =>
                        Color.Lerp(new Color(0.09f, 0.015f, 0.026f), new Color(0.63f, 0.25f, 0.26f), light),
                    _ when warm =>
                        Color.Lerp(new Color(0.09f, 0.025f, 0.028f), new Color(0.60f, 0.36f, 0.25f), light),
                    _ => Color.Lerp(new Color(0.035f, 0.033f, 0.035f), new Color(0.55f, 0.53f, 0.49f), light)
                };
                shaded.a = pixel.a / 255f;
                pixels[index] = shaded;
            }
            result.SetPixels32(pixels);
            result.Apply(false, true);
            return result;
        }

        private static void SetDamage(WeaponRecord weapon, int min, int max)
        {
            var damage = weapon.Damage;
            damage.minDmg = min;
            damage.maxDmg = max;
            weapon.Damage = damage;
        }

        private static void RegisterArbitesPatrol()
        {
            RegisterImperialRole(VigilantRole, "security", "Arbites Vigilant",
                Judgement, ArbitesHelmet, ArbitesCuirass, ArbitesGreaves, ArbitesBoots);
            RegisterImperialRole(SubductorRole, "security", "Arbites Subductor",
                ShockMaul, ArbitesHelmet, SubductorCuirass, ArbitesGreaves, ArbitesBoots);
            var patrol = CreateUnitDrop(ArbitesTier, 1, 1, ArbitesPatrolPoints, SubductorRole, VigilantRole);
            Registrations.Add(() =>
            {
                Data.LocationUnitDrop.AddRecord(TerraArbitesTable, patrol);
                Data.LocationUnitDrop.AddRecord(TerraArbitesGarrison, patrol);
            });
        }

        [Hook(ModHookType.SpaceUpdateAfterGameLoop)]
        public static void OfferTerraSecurity(IModContext context)
        {
            var state = context.State;
            var triggers = state.Get<StoryTriggers>();
            if (triggers == null || triggers.IsPassed(TerraSecurityOffered) || Data.LocationUnitDrop.Get(TerraArbitesTable) == null)
            {
                return;
            }

            var factions = state.Get<Factions>();
            var imperium = factions.Values.Find(faction => faction.Id == ItemCategory);
            if (imperium == null || imperium.CurrentTechLevel < ArbitesTier || !factions.IsEnabledFaction(imperium))
            {
                return;
            }

            var terra = state.Get<Stations>().Get(TerraStation, logMissing: false);
            var missions = state.Get<Missions>();
            if (terra?.OwnerFactionId != ItemCategory || missions.Values.Any(mission => mission.StationId == TerraStation))
            {
                return;
            }

            var cache = state.Get<FactionCachedData>();
            if (!cache.stationNeighbours.TryGetValue(TerraStation, out var neighbors) ||
                !cache.factionFriends.TryGetValue(ItemCategory, out var friends))
            {
                return;
            }

            var raiders = neighbors.Select(station => factions.Get(station.OwnerFactionId))
                .FirstOrDefault(faction => faction != null && faction.Id != ItemCategory && !friends.Contains(faction.Id) &&
                    faction.FactionType == FactionType.Corp && factions.IsEnabledFaction(faction));
            if (raiders == null)
            {
                return;
            }

            // A security response is a defensive contract at Terra and leaves its existing capture rules in place.
            MissionSystem.CreateMission(state.Get<MissionFactory>(), missions, state.Get<News>(), state.Get<SpaceTime>(),
                state.Get<TravelMetadata>(), ProceduralMissionType.Security, terra, imperium, raiders);
            var contract = missions.Values.Last(mission => mission.StationId == TerraStation);
            foreach (var plan in contract.LocationPlans.Values)
            {
                plan.MonstersTableIds.Add(TerraArbitesTable);
                plan.AlliesTechLevel = ArbitesTier;
                plan.AlliesPointsMult = 0.25f;
            }
            var reverse = missions.Get(TerraStation, reversed: true);
            if (reverse != null && reverse.LocationPlans.TryGetValue("stage1", out var entrance))
            {
                entrance.MonstersTableIds.Add(TerraArbitesGarrison);
            }
            triggers.Pass(TerraSecurityOffered);
        }

        [HarmonyPatch(typeof(UnitGenerationSystem), nameof(UnitGenerationSystem.GenerateUnitsByPoints))]
        private static class ArbitesSecurityPatrol
        {
            private static void Prefix(ref List<string> locationIds, string factionId, ref float totalPoints, int techLevel,
                UnitGenerationConditions conditions, out bool __state)
            {
                __state = false;
                if (factionId == ItemCategory && locationIds.Contains(TerraArbitesTable))
                {
                    locationIds = [TerraArbitesTable];
                    totalPoints = ArbitesPatrolPoints;
                }
                else if (locationIds.Contains(TerraArbitesGarrison))
                {
                    locationIds = locationIds.Where(table => table != TerraArbitesGarrison).ToList();
                    if (factionId == ItemCategory && techLevel >= ArbitesTier && totalPoints > 0 &&
                        !conditions.IsEventGroup && !conditions.IsQuestGroup)
                    {
                        __state = true;
                        totalPoints = Math.Max(0, totalPoints - ArbitesPatrolPoints);
                    }
                }
            }

            private static void Postfix(List<string> locationIds, string factionId, Statistics statistics, Difficulty difficulty,
                int techLevel, List<Unit> __result, bool __state)
            {
                if (__state)
                {
                    var patrol = Data.LocationUnitDrop.Get(TerraArbitesGarrison)[0];
                    __result.Add(UnitGenerationSystem.GenerateUnit(statistics, difficulty, patrol, factionId, techLevel));
                }
                if (!__state && (factionId != ItemCategory || !locationIds.Contains(TerraArbitesTable)))
                {
                    return;
                }

                // The native difficulty curve can enlarge a one-member unit. Keep the assigned patrol at two officers.
                foreach (var unit in __result)
                {
                    if (unit.LeaderMobClassId == SubductorRole && unit.Members.Count > 1)
                    {
                        unit.Members.RemoveRange(1, unit.Members.Count - 1);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(ExordiumDungeonGenerator), nameof(ExordiumDungeonGenerator.SpawnMonsters))]
        private static class ArbitesPatrolPlacement
        {
            private static bool Prefix(DungeonGenerationPlan plan, ref List<Unit> units)
            {
                if (!plan.MonstersTableIds.Contains(TerraArbitesTable) || !units.Any(AlreadyPlaced))
                {
                    return true;
                }

                // The dungeon builder passes allied units through a second spawn pass. The first pass assigns group indexes.
                units = units.Where(unit => !AlreadyPlaced(unit)).ToList();
                return units.Count > 0;
            }

            private static bool AlreadyPlaced(Unit unit) =>
                unit.GroupIndex >= 0 && unit.Alliance == CreatureAlliance.PlayerAlliance &&
                unit.FactionId == ItemCategory && unit.LeaderMobClassId == SubductorRole;
        }
    }
}

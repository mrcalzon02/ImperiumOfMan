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
        private const float FormationPatrolPoints = 100f;
        private static readonly Dictionary<(Texture Source, EquipmentFinish Finish), Texture2D> EquipmentTextures = new();

        private enum EquipmentFinish
        {
            Carapace, Subductor, Judgement, ShockMaul, WinterCloth, WinterRifle, LegionCloth, LegionMask, LegionCarbine,
            ElysianCloth, ElysianCarbine, KriegCloth, KriegMask, KriegSteel, ScionCarapace, ScionWeapon,
            SkitariiCloth, SkitariiMetal, SororitasPlate, SororitasArms, XenosCarapace, XenosWeapon,
            PaladinPlate, PaladinWeapon, GoldenPlate, GoldenWeapon
        }

        private enum FormationDeployment { AllFloors, Entrance, FinalFloor }

        private static readonly (string AlliedTable, string GarrisonTable, string Leader, int Tier)[] FormationPatrols =
        [
            (TerraArbitesTable, TerraArbitesGarrison, SubductorRole, ArbitesTier),
            (LunaValhallanTable, LunaValhallanGarrison, IceMarksmanRole, ValhallanTier),
            (HavocLegionTable, HavocLegionGarrison, LegionSupportRole, SteelLegionTier),
            (PhobosElysianTable, PhobosElysianGarrison, DropSpotterRole, ElysianTier),
            (HavocKriegTable, HavocKriegGarrison, CombatEngineerRole, KriegTier),
            (TerraScionTable, TerraScionGarrison, ScionSpecialistRole, ScionTier),
            (FoundrySkitariiTable, FoundrySkitariiGarrison, VanguardRole, SkitariiTier),
            (TerraSororitasTable, TerraSororitasGarrison, FlamerSisterRole, SororitasTier),
            (PhobosXenosTable, PhobosXenosGarrison, InterrogatorRole, OrdoXenosTier),
            (TitanPaladinPatrol, TitanPaladinGarrison, PaladinRole, TitanContractTier),
            (BroadswordPaladinPatrol, BroadswordPaladinGarrison, PaladinGunnerRole, TitanContractTier),
            (TerraGoldenPatrol, TerraGoldenGarrison, GoldenPaladinRole, GoldenTier)
        ];

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
            ApplyWeaponFinish(shotgun, EquipmentFinish.Judgement);
            RegisterItem("rangeweapons", shotgun, "Judgement", "Compact Arbites combat shotgun chambered for standard shells.");

            var maul = CloneItem<WeaponRecord>(ShockMaul, "fra_police_club_1", ArbitesTier, 205, "ShockMaul");
            SetDamage(maul, 23, 31);
            ((WeaponDescriptor)maul.ContentDescriptor)._grip = HandsGrip.MeleeOneHanded;
            ApplyWeaponFinish(maul, EquipmentFinish.ShockMaul);
            RegisterItem("meleeweapons", maul, "Arbites Shock Maul", "Battery-powered one-handed shock weapon issued to Subductors.");
        }

        private static void AddCarapace<T>(string table, string id, string donor, string artwork, string name, float price)
            where T : ResistRecord
        {
            var armor = CloneItem<T>(id, donor, ArbitesTier, price, artwork);
            armor.ResistSheet = [.. armor.ResistSheet];
            armor.SetResist("lacer", (float)Math.Round(armor.GetResist("lacer") * 1.33f));
            ApplyArmorFinish(armor, id == SubductorCuirass ? EquipmentFinish.Subductor : EquipmentFinish.Carapace);
            RegisterItem(table, armor, name, "Overlapping blackened steel plates and an oxblood liner add laceration resistance.");
        }

        private static void ApplyArmorFinish(ResistRecord armor, EquipmentFinish finish)
        {
            switch (armor.ContentDescriptor)
            {
                case HelmetDescriptor helmet:
                    helmet._parts = RecolorArmorParts(helmet.Parts, finish);
                    break;
                case ArmorDescriptor cuirass:
                    cuirass._parts = RecolorArmorParts(cuirass.Parts, finish);
                    break;
                case LeggingsDescriptor greaves:
                    greaves._parts = RecolorArmorParts(greaves.Parts, finish);
                    break;
                case BootsDescriptor boots:
                    boots._parts = RecolorArmorParts(boots.Parts, finish);
                    break;
            }
        }

        private static void ApplyWeaponFinish(WeaponRecord weapon, EquipmentFinish finish)
        {
            var descriptor = (WeaponDescriptor)weapon.ContentDescriptor;
            descriptor._texture = GetEquipmentTexture(descriptor.Texture, finish);
        }

        private static void ApplyEquipmentSprites(ItemContentDescriptor descriptor, EquipmentFinish finish)
        {
            descriptor._icon = RecolorEquipmentSprite(descriptor.Icon, finish);
            descriptor._smallIcon = RecolorEquipmentSprite(descriptor.SmallIcon, finish);
        }

        private static Sprite RecolorEquipmentSprite(Sprite source, EquipmentFinish finish)
        {
            if (source == null || source.packed)
            {
                return source;
            }
            var pivot = new Vector2(source.pivot.x / source.rect.width, source.pivot.y / source.rect.height);
            var sprite = Sprite.Create((Texture2D)GetEquipmentTexture(source.texture, finish), source.rect,
                pivot, source.pixelsPerUnit);
            sprite.name = FormationFinishName(finish) + " " + source.name;
            return sprite;
        }

        private static List<ArmorPartInfo> RecolorArmorParts(List<ArmorPartInfo> parts, EquipmentFinish finish)
        {
            var recolored = new List<ArmorPartInfo>(parts.Count);
            foreach (var original in parts)
            {
                var part = original;
                part.Texture = GetEquipmentTexture(part.Texture, finish);
                recolored.Add(part);
            }
            return recolored;
        }

        private static Texture GetEquipmentTexture(Texture source, EquipmentFinish finish)
        {
            if (source == null)
            {
                return null;
            }
            if (!EquipmentTextures.TryGetValue((source, finish), out var texture))
            {
                texture = RecolorTexture(source, finish);
                EquipmentTextures.Add((source, finish), texture);
            }
            return texture;
        }

        private static Texture2D RecolorTexture(Texture original, EquipmentFinish finish)
        {
            var previous = RenderTexture.active;
            var target = RenderTexture.GetTemporary(original.width, original.height, 0, RenderTextureFormat.ARGB32);
            var result = new Texture2D(original.width, original.height, TextureFormat.RGBA32, false)
            {
                name = FormationFinishName(finish) + " " + finish + " " + original.name,
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
                    EquipmentFinish.GoldenPlate or EquipmentFinish.GoldenWeapon when pixel.b > pixel.r + 20 =>
                        Color.Lerp(new Color(0.04f, 0.12f, 0.27f), new Color(0.24f, 0.73f, 1f), light),
                    EquipmentFinish.GoldenPlate or EquipmentFinish.GoldenWeapon =>
                        Color.Lerp(new Color(0.15f, 0.095f, 0.035f), new Color(0.98f, 0.74f, 0.29f), light),
                    EquipmentFinish.PaladinPlate when warm =>
                        Color.Lerp(new Color(0.11f, 0.03f, 0.04f), new Color(0.65f, 0.24f, 0.19f), light),
                    EquipmentFinish.PaladinPlate =>
                        Color.Lerp(new Color(0.13f, 0.17f, 0.24f), new Color(0.86f, 0.88f, 0.92f), light),
                    EquipmentFinish.PaladinWeapon when pixel.b > pixel.r + 20 =>
                        Color.Lerp(new Color(0.06f, 0.20f, 0.36f), new Color(0.58f, 0.88f, 1.0f), light),
                    EquipmentFinish.PaladinWeapon =>
                        Color.Lerp(new Color(0.08f, 0.10f, 0.15f), new Color(0.71f, 0.76f, 0.83f), light),
                    EquipmentFinish.XenosCarapace when warm =>
                        Color.Lerp(new Color(0.16f, 0.095f, 0.06f), new Color(0.83f, 0.68f, 0.35f), light),
                    EquipmentFinish.XenosCarapace =>
                        Color.Lerp(new Color(0.04f, 0.085f, 0.09f), new Color(0.37f, 0.60f, 0.57f), light),
                    EquipmentFinish.XenosWeapon when pixel.b > pixel.r + 20 && pixel.g > pixel.r + 12 =>
                        Color.Lerp(new Color(0.10f, 0.29f, 0.20f), new Color(0.68f, 1.0f, 0.62f), light),
                    EquipmentFinish.XenosWeapon =>
                        Color.Lerp(new Color(0.055f, 0.045f, 0.04f), new Color(0.64f, 0.56f, 0.46f), light),
                    EquipmentFinish.SororitasPlate when warm =>
                        Color.Lerp(new Color(0.30f, 0.22f, 0.18f), new Color(0.96f, 0.88f, 0.72f), light),
                    EquipmentFinish.SororitasPlate =>
                        Color.Lerp(new Color(0.035f, 0.030f, 0.043f), new Color(0.41f, 0.36f, 0.43f), light),
                    EquipmentFinish.SororitasArms when light >= 0.32f =>
                        Color.Lerp(new Color(0.15f, 0.018f, 0.034f), new Color(0.86f, 0.26f, 0.22f), light),
                    EquipmentFinish.SororitasArms =>
                        Color.Lerp(new Color(0.037f, 0.033f, 0.046f), new Color(0.48f, 0.44f, 0.48f), light),
                    EquipmentFinish.SkitariiCloth when warm =>
                        Color.Lerp(new Color(0.17f, 0.08f, 0.04f), new Color(0.85f, 0.61f, 0.32f), light),
                    EquipmentFinish.SkitariiCloth =>
                        Color.Lerp(new Color(0.13f, 0.026f, 0.035f), new Color(0.79f, 0.25f, 0.23f), light),
                    EquipmentFinish.SkitariiMetal when pixel.b > pixel.r + 25 && pixel.g > pixel.r + 15 =>
                        Color.Lerp(new Color(0.06f, 0.23f, 0.25f), new Color(0.62f, 0.90f, 0.97f), light),
                    EquipmentFinish.SkitariiMetal =>
                        Color.Lerp(new Color(0.08f, 0.047f, 0.032f), new Color(0.69f, 0.49f, 0.29f), light),
                    EquipmentFinish.ScionCarapace when warm =>
                        Color.Lerp(new Color(0.18f, 0.12f, 0.045f), new Color(0.92f, 0.72f, 0.32f), light),
                    EquipmentFinish.ScionCarapace =>
                        Color.Lerp(new Color(0.025f, 0.045f, 0.12f), new Color(0.36f, 0.57f, 0.78f), light),
                    EquipmentFinish.ScionWeapon when light > 0.45f =>
                        Color.Lerp(new Color(0.12f, 0.27f, 0.35f), new Color(0.65f, 0.88f, 0.94f), light),
                    EquipmentFinish.ScionWeapon =>
                        Color.Lerp(new Color(0.04f, 0.05f, 0.10f), new Color(0.37f, 0.48f, 0.64f), light),
                    EquipmentFinish.KriegMask when warm =>
                        Color.Lerp(new Color(0.15f, 0.07f, 0.045f), new Color(0.75f, 0.46f, 0.25f), light),
                    EquipmentFinish.KriegCloth when warm =>
                        Color.Lerp(new Color(0.09f, 0.065f, 0.05f), new Color(0.56f, 0.41f, 0.27f), light),
                    EquipmentFinish.KriegMask or EquipmentFinish.KriegCloth =>
                        Color.Lerp(new Color(0.045f, 0.065f, 0.09f), new Color(0.38f, 0.48f, 0.57f), light),
                    EquipmentFinish.KriegSteel =>
                        Color.Lerp(new Color(0.085f, 0.075f, 0.055f), new Color(0.65f, 0.61f, 0.48f), light),
                    EquipmentFinish.ElysianCloth when warm =>
                        Color.Lerp(new Color(0.16f, 0.10f, 0.055f), new Color(0.83f, 0.67f, 0.40f), light),
                    EquipmentFinish.ElysianCloth =>
                        Color.Lerp(new Color(0.055f, 0.095f, 0.13f), new Color(0.55f, 0.69f, 0.76f), light),
                    EquipmentFinish.ElysianCarbine =>
                        Color.Lerp(new Color(0.065f, 0.10f, 0.12f), new Color(0.69f, 0.78f, 0.81f), light),
                    EquipmentFinish.LegionCloth when warm =>
                        Color.Lerp(new Color(0.14f, 0.11f, 0.08f), new Color(0.65f, 0.48f, 0.27f), light),
                    EquipmentFinish.LegionCloth =>
                        Color.Lerp(new Color(0.08f, 0.095f, 0.04f), new Color(0.64f, 0.64f, 0.36f), light),
                    EquipmentFinish.LegionMask when warm =>
                        Color.Lerp(new Color(0.18f, 0.10f, 0.035f), new Color(0.81f, 0.61f, 0.23f), light),
                    EquipmentFinish.LegionMask or EquipmentFinish.LegionCarbine =>
                        Color.Lerp(new Color(0.07f, 0.083f, 0.064f), new Color(0.54f, 0.56f, 0.43f), light),
                    EquipmentFinish.WinterCloth when warm =>
                        Color.Lerp(new Color(0.16f, 0.13f, 0.09f), new Color(0.82f, 0.77f, 0.62f), light),
                    EquipmentFinish.WinterCloth =>
                        Color.Lerp(new Color(0.09f, 0.13f, 0.15f), new Color(0.57f, 0.66f, 0.69f), light),
                    EquipmentFinish.WinterRifle =>
                        Color.Lerp(new Color(0.06f, 0.09f, 0.11f), new Color(0.78f, 0.80f, 0.74f), light),
                    EquipmentFinish.Judgement when light >= 0.25f =>
                        Color.Lerp(new Color(0.20f, 0.045f, 0.035f), new Color(0.85f, 0.40f, 0.25f), light),
                    EquipmentFinish.ShockMaul when pixel.b > pixel.r + 26 && pixel.g > pixel.r + 16 =>
                        Color.Lerp(new Color(0.07f, 0.22f, 0.25f), new Color(0.65f, 0.93f, 0.98f), light),
                    EquipmentFinish.Subductor when warm =>
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

        private static string FormationFinishName(EquipmentFinish finish) => finish switch
        {
            EquipmentFinish.WinterCloth or EquipmentFinish.WinterRifle => "Valhallan",
            EquipmentFinish.LegionCloth or EquipmentFinish.LegionMask or EquipmentFinish.LegionCarbine => "Steel Legion",
            EquipmentFinish.ElysianCloth or EquipmentFinish.ElysianCarbine => "Elysian",
            EquipmentFinish.KriegCloth or EquipmentFinish.KriegMask or EquipmentFinish.KriegSteel => "Krieg",
            EquipmentFinish.ScionCarapace or EquipmentFinish.ScionWeapon => "Scion",
            EquipmentFinish.SkitariiCloth or EquipmentFinish.SkitariiMetal => "Skitarii",
            EquipmentFinish.SororitasPlate or EquipmentFinish.SororitasArms => "Sororitas",
            EquipmentFinish.XenosCarapace or EquipmentFinish.XenosWeapon => "Ordo Xenos",
            EquipmentFinish.PaladinPlate or EquipmentFinish.PaladinWeapon => "Grey Knights",
            EquipmentFinish.GoldenPlate or EquipmentFinish.GoldenWeapon => "Golden Paladin",
            _ => "Arbites"
        };

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
            RegisterFormationPatrol(ArbitesTier, TerraArbitesTable, TerraArbitesGarrison, SubductorRole, VigilantRole);
        }

        private static void RegisterFormationPatrol(int tier, string alliedTable, string garrisonTable, string leader, string member)
        {
            var patrol = CreateUnitDrop(tier, 1, 1, FormationPatrolPoints, leader, member);
            Registrations.Add(() =>
            {
                Data.LocationUnitDrop.AddRecord(alliedTable, patrol);
                Data.LocationUnitDrop.AddRecord(garrisonTable, patrol);
            });
        }

        [Hook(ModHookType.SpaceUpdateAfterGameLoop)]
        public static void OfferTerraContracts(IModContext context)
        {
            OfferFormationContract(context, TerraStation, TerraGoldenPatrol, TerraGoldenGarrison, TerraGoldenOffered,
                GoldenTier, FormationDeployment.Entrance);
            OfferFormationContract(context, TerraStation, TerraSororitasTable, TerraSororitasGarrison, TerraReliquaryOffered,
                SororitasTier, FormationDeployment.FinalFloor, ProceduralMissionType.Espionage, ConfigureReliquaryRecovery,
                intervalDays: 28);
            OfferFormationContract(context, TerraStation, TerraScionTable, TerraScionGarrison, TerraExtractionOffered,
                ScionTier, FormationDeployment.FinalFloor, ProceduralMissionType.Escort);
            OfferFormationContract(context, TerraStation, TerraArbitesTable, TerraArbitesGarrison, TerraSecurityOffered, ArbitesTier);
        }

        private static void OfferFormationContract(IModContext context, string stationId, string patrolTable,
            string garrisonTable, string trigger, int tier, FormationDeployment deployment = FormationDeployment.AllFloors,
            ProceduralMissionType missionType = ProceduralMissionType.Security, Action<Mission> configure = null,
            Action<DungeonGenerationPlan> configureReverseFloor = null, int intervalDays = 0)
        {
            var state = context.State;
            var triggers = state.Get<StoryTriggers>();
            var time = state.Get<SpaceTime>();
            if (triggers == null || Data.LocationUnitDrop.Get(patrolTable) == null ||
                (intervalDays == 0 ? triggers.IsPassed(trigger) :
                    triggers.TimeEventMap.ContainsKey(trigger) && !triggers.HasTimeEventElapsed(trigger, intervalDays, time.Time)))
            {
                return;
            }

            var factions = state.Get<Factions>();
            var imperium = factions.Values.Find(faction => faction.Id == ItemCategory);
            if (imperium == null || imperium.CurrentTechLevel < tier || !factions.IsEnabledFaction(imperium))
            {
                return;
            }

            var station = state.Get<Stations>().Get(stationId, logMissing: false);
            var missions = state.Get<Missions>();
            if (station?.OwnerFactionId != ItemCategory || missions.Values.Any(mission => mission.StationId == stationId) ||
                missions.Values.Count(mission => mission.BeneficiaryFactionId == ItemCategory) >=
                    Data.TechLevels[imperium.CurrentTechLevel].MaxMissionsCount)
            {
                return;
            }

            var cache = state.Get<FactionCachedData>();
            if (!cache.stationNeighbours.TryGetValue(stationId, out var neighbors) ||
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

            MissionSystem.CreateMission(state.Get<MissionFactory>(), missions, state.Get<News>(), time,
                state.Get<TravelMetadata>(), missionType, station, imperium, raiders);
            var contract = missions.Values.LastOrDefault(mission => mission.StationId == stationId);
            if (contract == null)
            {
                return;
            }
            configure?.Invoke(contract);
            DeployFormation(contract, missions.Get(stationId, reversed: true), patrolTable, garrisonTable, tier, deployment, configureReverseFloor);
            if (intervalDays > 0)
            {
                triggers.TimeEventMap[trigger] = time.Time;
            }
            else
            {
                triggers.Pass(trigger);
            }
        }

        private static void DeployFormation(Mission contract, Mission reverse, string patrolTable, string garrisonTable, int tier,
            FormationDeployment deployment, Action<DungeonGenerationPlan> configureReverseFloor = null)
        {
            string alliedFloor = GetFormationFloor(contract, deployment);
            foreach (var entry in contract.LocationPlans)
            {
                if (deployment != FormationDeployment.AllFloors && entry.Key != alliedFloor)
                {
                    continue;
                }
                var plan = entry.Value;
                if (!plan.MonstersTableIds.Contains(patrolTable))
                {
                    plan.MonstersTableIds.Add(patrolTable);
                }
                plan.AlliesTechLevel = tier;
                plan.AlliesPointsMult = 0.25f;
            }
            if (reverse != null)
            {
                string garrisonFloor = GetFormationFloor(reverse, deployment);
                if (reverse.LocationPlans.TryGetValue(garrisonFloor, out var floor))
                {
                    if (configureReverseFloor == null)
                    {
                        if (!floor.MonstersTableIds.Contains(garrisonTable))
                        {
                            floor.MonstersTableIds.Add(garrisonTable);
                        }
                    }
                    else
                    {
                        configureReverseFloor(floor);
                    }
                }
            }
        }

        private static string GetFormationFloor(Mission mission, FormationDeployment deployment) =>
            deployment == FormationDeployment.FinalFloor
                ? mission.WorldStructure.Locations.Last(location => location.ID.StartsWith("stage", StringComparison.Ordinal)).ID
                : "stage1";

        [HarmonyPatch(typeof(UnitGenerationSystem), nameof(UnitGenerationSystem.GenerateUnitsByPoints))]
        private static class FormationSecurityPatrol
        {
            private static void Prefix(ref List<string> locationIds, string factionId, ref float totalPoints, int techLevel,
                UnitGenerationConditions conditions, out string __state)
            {
                __state = null;
                var patrol = FindFormationPatrol(locationIds);
                if (patrol.AlliedTable != null)
                {
                    if (factionId == ItemCategory && techLevel >= patrol.Tier && totalPoints > 0 &&
                        !conditions.IsEventGroup && !conditions.IsQuestGroup)
                    {
                        locationIds = [patrol.AlliedTable];
                        totalPoints = FormationPatrolPoints;
                        return;
                    }
                    locationIds = locationIds.Where(table => table != patrol.AlliedTable).ToList();
                }
                string garrison = FormationGarrisonTable(locationIds, out int tier);
                if (garrison != null)
                {
                    locationIds = locationIds.Where(table => table != garrison).ToList();
                    if (factionId == ItemCategory && techLevel >= tier && totalPoints > 0 &&
                        !conditions.IsEventGroup && !conditions.IsQuestGroup)
                    {
                        __state = garrison;
                        totalPoints = Math.Max(0, totalPoints - FormationPatrolPoints);
                    }
                }
            }

            private static void Postfix(List<string> locationIds, string factionId, Statistics statistics, Difficulty difficulty,
                int techLevel, List<Unit> __result, string __state)
            {
                if (__state != null)
                {
                    var patrol = Data.LocationUnitDrop.Get(__state)[0];
                    __result.Add(UnitGenerationSystem.GenerateUnit(statistics, difficulty, patrol, factionId, techLevel));
                }
                if (__state == null && (factionId != ItemCategory || FindFormationPatrol(locationIds).AlliedTable == null))
                {
                    return;
                }

                // The native difficulty curve can enlarge a one-member unit. Keep the assigned patrol at two members.
                foreach (var unit in __result)
                {
                    if (IsFormationPatrol(unit) && unit.Members.Count > 1)
                    {
                        unit.Members.RemoveRange(1, unit.Members.Count - 1);
                    }
                }
            }
        }

        private static (string AlliedTable, string GarrisonTable, string Leader, int Tier) FindFormationPatrol(List<string> ids)
            => Array.Find(FormationPatrols, patrol => ids.Contains(patrol.AlliedTable));

        private static string FormationGarrisonTable(List<string> ids, out int tier)
        {
            var patrol = Array.Find(FormationPatrols, entry => ids.Contains(entry.GarrisonTable));
            tier = patrol.Tier;
            return patrol.GarrisonTable;
        }

        private static bool IsFormationPatrol(Unit unit)
            => Array.Exists(FormationPatrols, patrol => unit.LeaderMobClassId == patrol.Leader);

        [HarmonyPatch(typeof(DungeonBuilder), "Populate")]
        private static class FormationPatrolEligibility
        {
            private static void Prefix(State ____state, DungeonGenerationPlan ____plan)
            {
                var raid = ____state.Get<RaidMetadata>();
                if (raid.RaidType != RaidType.ProcMission)
                {
                    return;
                }

                var station = ____state.Get<Stations>().Get(raid.StationId, logMissing: false);
                int tier = station?.OwnerFactionId == ItemCategory
                    ? ____state.Get<Factions>().Get(ItemCategory).CurrentTechLevel
                    : 0;
                foreach (var patrol in FormationPatrols)
                {
                    if (tier >= patrol.Tier)
                    {
                        continue;
                    }
                    // A contract can outlive Imperial ownership of its station.
                    if (____plan.MonstersTableIds.Remove(patrol.AlliedTable))
                    {
                        ____plan.AlliesTechLevel = -1;
                    }
                    ____plan.MonstersTableIds.Remove(patrol.GarrisonTable);
                }
            }
        }

        [HarmonyPatch(typeof(ExordiumDungeonGenerator), nameof(ExordiumDungeonGenerator.SpawnMonsters))]
        private static class FormationPatrolPlacement
        {
            private static bool Prefix(DungeonGenerationPlan plan, ref List<Unit> units)
            {
                if (FindFormationPatrol(plan.MonstersTableIds).AlliedTable == null || !units.Any(AlreadyPlaced))
                {
                    return true;
                }

                // The dungeon builder passes allied units through a second spawn pass. The first pass assigns group indexes.
                units = units.Where(unit => !AlreadyPlaced(unit)).ToList();
                return units.Count > 0;
            }

            private static bool AlreadyPlaced(Unit unit) =>
                unit.GroupIndex >= 0 && unit.Alliance == CreatureAlliance.PlayerAlliance &&
                unit.FactionId == ItemCategory && IsFormationPatrol(unit);
        }
    }
}

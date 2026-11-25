using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MGSC;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private const string Requisition = "iom_requisition";
        private const string PuritySeal = "iom_purity_seal";
        private const string BoltClip = "iom_bolt_clip";
        private const string KrakenClip = "iom_kraken_clip";
        private const string MetalStormClip = "iom_metal_storm_clip";
        private const string HelfrostClip = "iom_helfrost_clip";
        private const string AntiphasicClip = "iom_antiphasic_clip";
        private const string InfernoClip = "iom_inferno_clip";
        private const string HellfireClip = "iom_hellfire_clip";
        private const string TempestClip = "iom_tempest_clip";
        private const string KrakGrenade = "iom_krak_grenade";
        private const string MeltaGrenade = "iom_melta_grenade";
        private const string BoltPistol = "iom_bolt_pistol";
        private const string BoltRifle = "iom_bolt_rifle";
        private const string PowerFist = "iom_power_fist";
        private const string Graft = "iom_graft";
        private const string Underplate = "iom_underplate";
        private const string Gland = "iom_gland";
        private const string DaydreamChems = "DaydreamChems";
        private const string AvicennasClinic = "AvicennasClinic";

        private static void RegisterSpecialistSupplies()
        {
            RegisterBoltClip(BoltClip, "Bolt Clip", ElysianTier, 6, 40, 1.02f, "pierce",
                "Standard ammunition for the Imperial bolt weapons. Fits Heavy weapons.");
            RegisterBoltClip(KrakenClip, "Kraken Bolt Clip", KriegTier, 11, 24, 0.95f, "pierce",
                "Armor-piercing Heavy ammunition with a small range bonus.", trait: "armor_pen", range: 1);
            RegisterBoltClip(MetalStormClip, "Metal Storm Bolt Clip", ScionTier, 12, 20, 0.52f, "lacer",
                "Close-range Heavy ammunition that scatters two lacerating fragments per round.",
                range: -1, projectile: "shrapnel", fragments: 2);
            RegisterBoltClip(HelfrostClip, "Helfrost Bolt Clip", ScionTier, 13, 20, 0.86f, "cold",
                "Cold Heavy ammunition with a chance to build up freezing on hit.", "coldEffect", projectile: "frost");
            RegisterBoltClip(AntiphasicClip, "Antiphasic Bolt Clip", SkitariiTier, 14, 18, 0.87f, "shock",
                "Shock Heavy ammunition with a chance to build up electrical trauma on hit.", "shockEffect", projectile: "lighting");
            RegisterBoltClip(InfernoClip, "Inferno Bolt Clip", SororitasTier, 14, 18, 0.88f, "fire",
                "Incendiary Heavy ammunition that can start fires.", trait: "incendiary", projectile: "thermal");
            RegisterBoltClip(HellfireClip, "Hellfire Bolt Clip", OrdoXenosTier, 16, 16, 0.89f, "poison",
                "Poison Heavy ammunition with a chance to build up toxin on hit.", "poisonEffect", projectile: "toxic");
            RegisterBoltClip(TempestClip, "Tempest Bolt Clip", TitanContractTier, 18, 14, 0.93f, "beam",
                "Beam Heavy ammunition with a small range bonus.", range: 1, projectile: "laser");

            RegisterArmoryGrenade(KrakGrenade, "Krak Grenade", "highexplosion_grenade", KriegTier,
                145, 6, 120, "pierce", "A siege grenade with a narrow blast that deals Pierce damage.");
            RegisterArmoryGrenade(MeltaGrenade, "Melta Grenade", "urparp_grenade", SororitasTier,
                230, 4, 145, "plasma", "A short-throw relic charge with a narrow blast that deals Plasma damage.");

            RegisterBoltWeapon(BoltPistol, "Bolt Pistol", "trucker_pistol_2", ElysianTier,
                245, 1.15f, 26, 38, 7, 5, ["rifle_1"], 1.12f,
                "A compact single-shot sidearm that uses Bolt Clips and other Heavy ammunition.");
            RegisterBoltWeapon(BoltRifle, "Bolt Rifle", "trucker_marksman_2", KriegTier,
                525, 3.6f, 35, 52, 16, 8, ["rifle_1", "rifle_2"], 0.94f,
                "A long-range bolt weapon with single and paired shots. Uses Heavy ammunition.");

            var fist = CloneItem<WeaponRecord>(PowerFist, "civ_powerfist_1", TitanContractTier, 795, PowerFist);
            fist.Weight = 1.9f;
            fist.MaxDurability = 155;
            fist.MagazineCapacity = 10;
            fist.MinRandomAmmoCount = 6;
            fist.DefaultAmmoId = LasClip;
            fist.RepairItemIds = ["rod_parts", "weapon_parts", "capacitor_parts"];
            SetDamage(fist, 49, 66);
            RegisterItem("meleeweapons", fist, "Power Fist",
                "A charged gauntlet that delivers Blunt damage in close combat. Uses BatteryCells.");

            RegisterSupplyToken(Requisition, "Imperial Requisition", ArbitesTier, 220,
                "Earned on Imperial contracts. Redeem it by selling it at the Golden Throne for Imperial trade credit.");
            RegisterSupplyToken(PuritySeal, "Purity Seal", SororitasTier, 140,
                "Awarded for Sororitas reliquary recovery. Consumed when manufacturing selected relics.");

            RegisterSupplyImplant(Graft, "Tissue Graft", "cyber_heart", ScionTier, 610, DaydreamChems,
                new() { ["max_health"] = 12, ["wound_heal_chance"] = 0.10f },
                new() { ["resist_shock"] = -6 },
                "A chest implant that raises maximum health and improves wound recovery, with reduced shock resistance.");
            RegisterSupplyImplant(Underplate, "Subdermal Underplate", "reinforced_epidermis", SkitariiTier, 730, ItemCategory,
                new() { ["resist_blunt"] = 10, ["resist_pierce"] = 14, ["resist_lacer"] = 8 },
                new() { ["resist_shock"] = -10 },
                "A body implant that protects against Blunt, Pierce and Laceration damage at the cost of shock resistance.");
            RegisterSupplyImplant(Gland, "Detox Gland", "chemical_barrier", OrdoXenosTier, 910, ItemCategory,
                new() { ["resist_poison"] = 18, ["status_immune_poisonEffect"] = 1 },
                new() { ["resist_fire"] = -8 },
                "A stomach implant that resists Poison damage and prevents poison buildup, at the cost of fire resistance.");
        }

        private static void RegisterBoltClip(string id, string title, int tier, float price, short stack, float damage,
            string damageType, string description, string status = null, string trait = null, int range = 0,
            string projectile = "pistol", int fragments = 1)
        {
            var clip = CloneItem<AmmoRecord>(id, "rifle_basic_ammo", tier, price, id);
            clip.DmgType = damageType;
            clip.DamageMult = damage;
            clip.MaxStack = stack;
            clip.MinAmmoAmount = id == BoltClip ? 8 : 3;
            clip.MaxAmmoAmount = id == BoltClip ? 14 : 6;
            clip.RangeBonus = range;
            clip.ProjectileId = projectile;
            clip.BulletCastsPerShot = fragments;
            clip.Traits = trait == null ? [] : [trait];
            if (status != null)
            {
                clip.StatusEffectId = status;
                clip.ChanceToApply = 0.25f;
                clip.StatusDamageModifier = 0.35f;
            }
            RegisterItem("ammo", clip, title, description);
        }

        private static void RegisterArmoryGrenade(string id, string title, string donor, int tier, float price,
            int range, int damage, string damageType, string description)
        {
            var grenade = CloneItem<GrenadeRecord>(id, donor, tier, price, id);
            var source = Data.Explosions.GetRecord(grenade.Explosion);
            var parameters = source.Parameters;
            parameters.Radius = 1;
            parameters.Damage = damage;
            parameters.DamageType = damageType;
            parameters.GainDmgToCreature = true;
            parameters.PropagateFire = false;
            var descriptor = (ExplosionDescriptor)Object.Instantiate(source.ContentDescriptor);
            grenade.Explosion = id + "_explosion";
            grenade.Range = range;
            grenade.MaxStack = 2;
            descriptor.name = grenade.Explosion;
            StageRecord(Data.Explosions, grenade.Explosion, new ExplosionRecord
            {
                Id = grenade.Explosion, ContentDescriptor = descriptor, Parameters = parameters
            });
            StageDescriptor("explosions", grenade.Explosion, descriptor);
            RegisterItem("grenades", grenade, title, description);
        }

        private static void RegisterBoltWeapon(string id, string title, string donor, int tier, float price, float weight,
            int minimum, int maximum, int capacity, int range, List<string> fireModes, float soundPitch, string description)
        {
            var weapon = CloneItem<WeaponRecord>(id, donor, tier, price, id);
            weapon.Weight = weight;
            weapon.MagazineCapacity = capacity;
            weapon.MinRandomAmmoCount = capacity / 2;
            weapon.ReloadDuration = id == BoltPistol ? 2 : 3;
            weapon.Range = range;
            weapon.Firemodes = fireModes;
            weapon.Traits = id == BoltPistol ? ["offhand"] : ["optic_sight"];
            weapon.DefaultAmmoId = BoltClip;
            SetDamage(weapon, minimum, maximum);
            var sound = Object.Instantiate(LoadRequiredAsset<SoundBank>("boltersoundbank"));
            sound.name = id + "_attack";
            sound._pitch = soundPitch;
            ((WeaponDescriptor)weapon.ContentDescriptor)._attackSoundBanks = [sound];
            RegisterItem("rangeweapons", weapon, title, description);
        }

        private static void RegisterSupplyToken(string id, string title, int tier, float price, string description)
        {
            var token = CloneItem<TrashRecord>(id, "tin", tier, price, id);
            token.Categories = [];
            token.ItemClass = ItemClass.ValuableBarter;
            token.SubType = TrashSubtype.BartherResource;
            token.Disassembly = [];
            token.MaxStack = 12;
            RegisterItem("trash", token, title, description);
        }

        private static void RegisterSupplyImplant(string id, string title, string donor, int tier, float price,
            string faction, Dictionary<string, float> bonuses, Dictionary<string, float> penalties, string description)
        {
            var implant = CloneItem<ImplantRecord>(id, donor, tier, price, id);
            implant.Categories = [faction];
            implant.Disassembly = [];
            implant.ImplicitBonusEffects = bonuses;
            implant.ImplicitPenaltyEffects = penalties;
            RegisterItem("implants", implant, title, description);
        }

        private static void RegisterSpecialistSupplyProduction()
        {
            IndustryRecipe(Graft, 20, 1, new(Clinical, 1), new(Biomass, 2), new("medical_glue", 1));
            var graftChip = CloneItem<DatadiskRecord>(Graft + "_plans", "low_chip", ScionTier, 320, FieldPlans);
            graftChip.Categories = [DaydreamChems];
            RegisterCatalog(graftChip, "Tissue Graft Schematic", "A DaydreamChems pattern for fabricating the Tissue Graft on the Magnum.",
                DaydreamChems, [Graft], supplier: AvicennasClinic);

            RegisterOrdnanceLine(BoltClip, PhobosStation, 4, 3, 96, 24, 12, new("rifle_basic_ammo", 8), new(Metal, 1));
            IndustryRecipe(BoltPistol, 18, 1, new(Sheet, 1), new(Fittings, 1), new("weapon_parts", 2), new("spring", 1));
            ExtendSupplyCatalog(DropPlans, BoltClip, BoltPistol);

            RegisterOrdnanceLine(KrakenClip, HavocStation, 5, 5, 120, 8, 6,
                new(BoltClip, 6), new("rifle_armorpierce_ammo", 2), new(Metal, 1));
            RegisterOrdnanceLine(KrakGrenade, HavocStation, 5, 8, 144, 6, 1,
                new(Fittings, 1), new("explosives", 1), new("armor_plates", 1));
            IndustryRecipe(BoltRifle, 26, 1, new(Sheet, 2), new("weapon_parts", 3), new("lens", 1));
            ExtendSupplyCatalog(SiegePlans, KrakenClip, KrakGrenade, BoltRifle);

            RegisterOrdnanceLine(MetalStormClip, TerraStation, 6, 5, 120, 8, 6,
                new(BoltClip, 6), new("shotgun_flechette_ammo", 3));
            RegisterOrdnanceLine(HelfrostClip, LunaStation, 6, 6, 120, 8, 6,
                new(BoltClip, 6), new("pills_fibromine", 1), new(GlassStock, 1));
            ExtendSupplyCatalog(ScionPlans, MetalStormClip, HelfrostClip);

            RegisterOrdnanceLine(AntiphasicClip, FoundryStation, 7, 6, 120, 8, 6,
                new(BoltClip, 6), new("capacitor_parts", 1));
            IndustryRecipe(Underplate, 20, 1, new(Plate, 2), new(PolymerStock, 2), new(CircuitAssembly, 1));
            ExtendSupplyCatalog(SkitariiPlans, AntiphasicClip, Underplate);

            RegisterOrdnanceLine(InfernoClip, TerraStation, 8, 6, 120, 8, 6,
                new(BoltClip, 6), new("gas_ammo", 4), new(Housing, 1));
            RegisterOrdnanceLine(MeltaGrenade, TerraStation, 8, 12, 168, 4, 1,
                new(Fittings, 1), new(Cell, 1), new("explosives", 1), new(PuritySeal, 1));
            ExtendSupplyCatalog(SororitasPlans, InfernoClip, MeltaGrenade);

            RegisterOrdnanceLine(HellfireClip, PhobosStation, 9, 7, 120, 8, 6,
                new(BoltClip, 6), new("toxic_ammo", 4), new(GlassStock, 1));
            IndustryRecipe(Gland, 25, 1, new(Clinical, 1), new("syringe_antidote", 2), new("rubber", 2));
            ExtendSupplyCatalog(XenosPlans, HellfireClip, Gland);

            RegisterOrdnanceLine(TempestClip, TitanFortress, 10, 8, 120, 8, 6,
                new(BoltClip, 6), new(Cell, 1), new("lens", 1));
            IndustryRecipe(PowerFist, 34, 1, new(HeavyPlate, 2), new("hydraulic_pump", 1), new("weapon_parts", 2));
            ExtendSupplyCatalog(PaladinPlans, TempestClip, PowerFist);
        }

        private static void RegisterOrdnanceLine(string product, string station, int tier, int magnumHours,
            int stationHours, int stationBatch, int magnumBatch, params ItemQuantity[] inputs)
        {
            IndustryRecipe(product, magnumHours, magnumBatch, inputs);
            AddSupportWorkshop(station, tier, product + "_packing", stationHours, [.. inputs], [new(product, stationBatch)]);
        }

        private static void ExtendSupplyCatalog(string chip, params string[] products)
        {
            var original = IndustryCatalogs[chip];
            IndustryCatalogs[chip] = original.Concat(products).Distinct().ToArray();
            LicensedProducts.UnionWith(products);
        }

        private static void AddPuritySealRewards(Mission mission)
        {
            // Keep the published and paid reward visible through the game's normal mission interface.
            if (!mission.RewardItems.Any(item => item.Id == PuritySeal))
            {
                var factory = SingletonMonoBehaviour<ItemFactory>.Instance;
                MissionSystem.AddReward(factory, mission, PuritySeal);
                MissionSystem.AddReward(factory, mission, PuritySeal);
            }
        }

        [HarmonyPatch(typeof(MissionSystem), nameof(MissionSystem.MissionFinishedByPlayer))]
        private static class ExistingReliquaryRewards
        {
            private static void Prefix(Mission mission)
            {
                if (_industryReady && mission.BeneficiaryFactionId == ItemCategory &&
                    mission.StoryId == SororitasReliquaryStory)
                {
                    AddPuritySealRewards(mission);
                }
            }
        }

        [HarmonyPatch(typeof(MissionFactory), "InitReward")]
        private static class ImperialRequisitionReward
        {
            private static void Postfix(Mission mission, Faction beneficiary)
            {
                if (_industryReady && !mission.IsStoryMission && beneficiary.Id == ItemCategory &&
                    beneficiary.CurrentTechLevel >= ArbitesTier && mission.VictimFactionId != ItemCategory)
                {
                    MissionSystem.AddReward(SingletonMonoBehaviour<ItemFactory>.Instance, mission, Requisition);
                }
            }
        }

        [HarmonyPatch(typeof(TradeSystem), nameof(TradeSystem.IsValidItem))]
        private static class RequisitionTrade
        {
            private static void Postfix(Faction faction, Station station, string itemId, ref bool __result)
            {
                if (_industryReady && itemId == Requisition)
                {
                    __result = station.Id == TerraStation && faction.Id == ItemCategory &&
                        faction.Record.CanBeTraded && faction.CurrentTechLevel >= ArbitesTier;
                }
            }
        }

        [HarmonyPatch(typeof(TradeSystem), "MoveSoldItemsToStation")]
        private static class ConsumeRedeemedRequisitions
        {
            private static void Prefix(Station station, List<BasePickupItem> toRemove, out int __state)
            {
                __state = _industryReady && station.Id == TerraStation && toRemove.Any(item => item.Id == Requisition)
                    ? station.InternalStorage.CountItems(Requisition) : -1;
            }

            private static void Postfix(Station station, int __state)
            {
                if (__state >= 0)
                {
                    int delivered = station.InternalStorage.CountItems(Requisition) - __state;
                    if (delivered > 0)
                    {
                        station.InternalStorage.RemoveSpecificItem(Requisition, (short)delivered);
                    }
                }
            }
        }
    }
}

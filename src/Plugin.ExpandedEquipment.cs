using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using MGSC;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private const string LasClip = "iom_las_clip";
        private const string BoltClip = "iom_bolt_clip";
        private const string Requisition = "iom_requisition";
        private const string PuritySeal = "iom_purity_seal";
        private const string CorpseStarch = "iom_corpse_starch";
        private const string DirtyStarch = "iom_dirty_corpse_starch";
        private static readonly MethodInfo CloneRecordMethod = AccessTools.Method(typeof(object), "MemberwiseClone");
        private static readonly Assembly ModAssembly = typeof(Plugin).Assembly;
        private static readonly Dictionary<string, string> ResourceNames = ModAssembly.GetManifestResourceNames()
            .ToDictionary(name => name, StringComparer.OrdinalIgnoreCase);
        private static AudioClip _grenadeAudio;

        private static void RegisterExpandedEquipment()
        {
            AddAmmo(LasClip, "LasClip", "battery_basic_ammo", "Las Clip", 1, 10, 40, "beam");
            AddAmmo(BoltClip, "Boltclip", "rifle_basic_ammo", "Bolt Clip", 2, 7, 36, "pierce", damageMultiplier: 1.10f);
            AddAmmo("iom_kraken_clip", "PierceKrakenBoltclip", "rifle_armorpierce_ammo", "Kraken Bolt Clip",
                3, 11, 24, "pierce", range: 1);
            AddAmmo("iom_inferno_clip", "FireInfernoBoltclip", "rifle_basic_ammo", "Inferno Bolt Clip",
                4, 13, 22, "fire", damageMultiplier: 0.82f, trait: "incendiary");
            AddAmmo("iom_hellfire_clip", "PoisonHellfiretBoltclip", "rifle_basic_ammo", "Hellfire Bolt Clip",
                4, 13, 22, "poison", damageMultiplier: 0.80f, status: "poisonEffect");
            AddAmmo("iom_metal_storm_clip", "CutMetalStormFragBoltclip", "rifle_basic_ammo", "Metal Storm Bolt Clip",
                4, 13, 22, "cut", damageMultiplier: 0.58f, range: -1, casts: 2);
            AddAmmo("iom_tempest_clip", "BeamTempestBoltclip", "rifle_basic_ammo", "Tempest Bolt Clip",
                5, 15, 20, "beam", damageMultiplier: 0.90f, range: 1, projectile: "laser");
            AddAmmo("iom_helfrost_clip", "ColdHelfrostBoltclip", "rifle_basic_ammo", "Helfrost Bolt Clip",
                5, 15, 20, "cold", damageMultiplier: 0.85f, status: "coldEffect");
            AddAmmo("iom_antiphasic_clip", "ShockAntiphasicBoltclip", "rifle_basic_ammo", "Antiphasic Bolt Clip",
                5, 15, 20, "shock", damageMultiplier: 0.85f, status: "shockEffect");

            AddRation(CorpseStarch, "Corpse-starch", "Corpse Starch", 550, false);
            AddRation(DirtyStarch, "Corpse-starch-Dirty", "Dirty Corpse Starch", 300, true);
            AddResource(Requisition, "Imperial", "Imperial Requisition", 1, 45, 12);
            AddResource(PuritySeal, "Purity", "Purity Seal", 3, 100, 5);

            AddGrenade("iom_frag_grenade", "FRAG", "frag_grenade", "Frag Grenade", 2, 90, 7, 3, 85);
            AddGrenade("iom_krak_grenade", "KRACK", "highexplosion_grenade", "Krak Grenade", 3, 145, 6, 1, 135);
            AddGrenade("iom_melta_grenade", "MELTA-g", "thermal_grenade", "Melta Grenade", 5, 220, 4, 1, 105);

            AddNewWeapon("iom_power_fist", "PowerFist", "civ_powerfist_1", "Power Fist",
                4, 475, 1.7f, 38, 51, 0, LasClip, melee: true);
            AddNewWeapon("iom_power_blade", "Powerblade", "sun_laser_powersword_1", "Power Blade",
                5, 625, 1.5f, 42, 55, 0, LasClip, melee: true);
            AddNewWeapon("iom_bolt_pistol", "spec_assaultP", "trucker_pistol_2", "Bolt Pistol",
                2, 225, 1.15f, 27, 38, 7, BoltClip, boltSound: true);
            AddNewWeapon("iom_bolt_rifle", "spec_assaultR", "military_assault_1", "Bolt Rifle",
                5, 525, 4.5f, 36, 51, 16, BoltClip, boltSound: true);
            AddNewWeapon("iom_judgement", "Judgement", "common_shotgun_1", "Judgement",
                3, 350, 2.6f, 31, 62, 5, "shotgun_bullet_ammo");

            _lasgun.DefaultAmmoId = LasClip;
            _longLas.DefaultAmmoId = LasClip;
            _bolter.DefaultAmmoId = BoltClip;
        }

        private static void AddAmmo(string id, string artwork, string donorId, string name, int tier,
            float price, short stack, string damage, float damageMultiplier = 1f, int range = 0,
            int casts = 1, string trait = null, string status = null, string projectile = null)
        {
            var record = CloneItem<AmmoRecord>(id, donorId, artwork, tier, price);
            record.MaxStack = stack;
            record.DmgType = damage;
            record.DamageMult *= damageMultiplier;
            record.RangeBonus += range;
            record.BulletCastsPerShot = casts;
            record.ProjectileId = projectile ?? record.ProjectileId;
            record.Traits = record.Traits == null ? [] : [.. record.Traits];
            if (trait != null)
            {
                record.Traits.Add(trait);
            }
            if (status != null)
            {
                record.StatusEffectId = status;
                record.ChanceToApply = 0.25f;
                record.StatusDamageModifier = 0.35f;
            }
            RegisterExpansionItem("ammo", record, name);
        }

        private static void AddRation(string id, string artwork, string name, int calories, bool spoiled)
        {
            var record = CloneItem<ConsumableRecord>(id, "ration_pack_3", artwork, 1, spoiled ? 4 : 12);
            record.StarvationValue = calories;
            record.HealthValue = spoiled ? -4 : 0;
            record.EffectChance = spoiled ? new() { ["poisonEffect"] = 0.25f } : new();
            record.EffectProgression = spoiled ? new() { ["poisonEffect"] = 18 } : new();
            RegisterExpansionItem("consumables", record, name,
                spoiled ? "A contaminated field ration. It can cause poisoning." : "A compact Imperial field ration.");
        }

        private static void AddResource(string id, string artwork, string name, int tier, float price, short stack)
        {
            var record = CloneItem<TrashRecord>(id, "tin", artwork, tier, price);
            record.ItemClass = ItemClass.ValuableBarter;
            record.SubType = TrashSubtype.BartherResource;
            record.MaxStack = stack;
            RegisterExpansionItem("trash", record, name, id == Requisition
                ? "A document redeemed through Imperial station barter."
                : "An Imperial seal used in advanced equipment production.");
        }

        private static void AddGrenade(string id, string artwork, string donorId, string name, int tier,
            float price, int range, int radius, int damage)
        {
            var record = CloneItem<GrenadeRecord>(id, donorId, artwork, tier, price);
            var donor = Data.Explosions.GetRecord(record.Explosion);
            var parameters = donor.Parameters;
            parameters.Radius = radius;
            parameters.Damage = damage;
            parameters.GainDmgToCreature = true;
            var visual = Object.Instantiate((ExplosionDescriptor)donor.ContentDescriptor);
            var explosionId = id + "_explosion";
            var sound = visual.explosionSoundBank == null
                ? ScriptableObject.CreateInstance<SoundBank>()
                : Object.Instantiate(visual.explosionSoundBank);
            sound.name = id + "_sound";
            sound._clips = [_grenadeAudio ??= ReadGrenadeAudio()];
            visual.explosionSoundBank = sound;
            StageRecord(Data.Explosions, explosionId, new ExplosionRecord
            {
                Id = explosionId,
                ContentDescriptor = visual,
                Parameters = parameters
            });
            StageDescriptor("explosions", explosionId, visual);
            record.Explosion = explosionId;
            record.Range = range;
            record.MaxStack = 3;
            RegisterExpansionItem("grenades", record, name);
        }

        private static void AddNewWeapon(string id, string artwork, string donorId, string name, int tier,
            float price, float weight, int minimumDamage, int maximumDamage, int capacity, string defaultAmmo,
            bool melee = false, bool boltSound = false)
        {
            var record = CloneItem<WeaponRecord>(id, donorId, artwork, tier, price);
            record.Damage = new DmgInfo
            {
                damage = record.Damage.damage,
                minDmg = minimumDamage,
                maxDmg = maximumDamage,
                critChance = record.Damage.critChance,
                critDmg = record.Damage.critDmg
            };
            record.Weight = weight;
            if (!melee)
            {
                record.MagazineCapacity = capacity;
                record.MinRandomAmmoCount = Math.Min(record.MinRandomAmmoCount, capacity);
            }
            record.DefaultAmmoId = defaultAmmo;
            if (boltSound)
            {
                var descriptor = (WeaponDescriptor)record.ContentDescriptor;
                var bank = Object.Instantiate(LoadRequiredAsset<SoundBank>("boltersoundbank"));
                bank.name = id + "_attack";
                bank._pitch = capacity <= 7 ? 1.12f : 0.94f;
                descriptor._attackSoundBanks = [bank];
            }
            RegisterExpansionItem(melee ? "meleeweapons" : "rangeweapons", record, name);
        }

        private static T CloneItem<T>(string id, string donorId, string artwork, int tier, float price)
            where T : ItemRecord
        {
            if (Data.Items.GetRecord(donorId) is not CompositeItemRecord composite ||
                composite.GetRecord<T>() is not T donor || donor.ItemDesc == null)
            {
                throw new InvalidDataException("Imperium item " + id + " requires donor " + donorId);
            }
            var record = (T)CloneRecordMethod.Invoke(donor, null);
            record.Id = id;
            record.Categories = [ItemCategory];
            record.TechLevel = tier;
            record.Price = price;
            var descriptor = Object.Instantiate(donor.ItemDesc);
            descriptor.name = id;
            descriptor._overridenRenderId = string.Empty;
            if (artwork != null)
            {
                descriptor._icon = ReadSprite(artwork + "_icon", donor.ItemDesc.Icon);
                descriptor._smallIcon = ReadSprite(artwork + "_floor", donor.ItemDesc.SmallIcon);
                descriptor._shadow = ReadSprite(artwork + "_shadow", donor.ItemDesc.ShadowOnFloor);
            }
            record.ContentDescriptor = descriptor;
            return record;
        }

        private static Sprite ReadSprite(string name, Sprite donor)
        {
            string resource = "ImperiumOfMan.Art." + name + ".png";
            using Stream stream = OpenResource(resource);
            using var buffer = new MemoryStream();
            stream.CopyTo(buffer);
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false)
            {
                name = name,
                filterMode = FilterMode.Point
            };
            if (!ImageConversion.LoadImage(texture, buffer.ToArray(), markNonReadable: true))
            {
                Object.Destroy(texture);
                throw new InvalidDataException("Invalid Imperial sprite " + resource);
            }
            Vector2 pivot = donor == null
                ? new Vector2(0.5f, 0.5f)
                : new Vector2(donor.pivot.x / donor.rect.width, donor.pivot.y / donor.rect.height);
            float pixelsPerUnit = donor == null ? 100f : donor.pixelsPerUnit;
            var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), pivot, pixelsPerUnit);
            sprite.name = name;
            return sprite;
        }

        private static AudioClip ReadGrenadeAudio()
        {
            const int sampleRate = 22050;
            using Stream stream = OpenResource("ImperiumOfMan.Audio.ImperialGrenade.pcm");
            if (stream.Length == 0 || stream.Length % sizeof(short) != 0)
            {
                throw new InvalidDataException("Invalid Imperial grenade audio");
            }
            using var reader = new BinaryReader(stream);
            var samples = new float[stream.Length / sizeof(short)];
            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] = reader.ReadInt16() / 32768f;
            }
            var clip = AudioClip.Create("Imperial grenade", samples.Length, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static Stream OpenResource(string resource)
        {
            if (!ResourceNames.TryGetValue(resource, out string path))
            {
                throw new InvalidDataException("Missing Imperial resource " + resource);
            }
            return ModAssembly.GetManifestResourceStream(path);
        }

        private static void RegisterExpansionItem(string table, ItemRecord record, string name, string description = null)
        {
            StageRecord(Data.Items, record.Id, record);
            StageDescriptor(table, record.Id, record.ContentDescriptor);
            Registrations.Add(() => TranslationPatch.AddItem(record.Id, name, description));
        }
    }
}

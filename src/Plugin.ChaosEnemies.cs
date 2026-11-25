using System;
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
        private const string ChaosFury = ChaosItemPrefix + "fury";
        private const string ChaosMutant = ChaosItemPrefix + "accursed_mutant";
        private const string ChaosPrince = ChaosItemPrefix + "daemon_prince";
        private const string ChaosFinale = ChaosItemPrefix + "belakor";
        private const string FinaleClaimed = "iom.chaos.black_meridian_relic_recovered";

        private sealed class ChaosEnemy(string key, string name, int tier, string body, string role,
            int health, int ap, int melee, string damage, List<DmgResist> resist, int bounty, int skullChance,
            string skull, float size, string weapon = null, string firemode = null)
        {
            internal string Id => ChaosItemPrefix + key;
            internal string Actor => Id + "_actor";
            internal string Name { get; } = name;
            internal int Tier { get; } = tier;
            internal string Body { get; } = body;
            internal string Role { get; } = role;
            internal int Health { get; } = health;
            internal int AP { get; } = ap;
            internal int Melee { get; } = melee;
            internal string Damage { get; } = damage;
            internal List<DmgResist> Resist { get; } = resist;
            internal int Bounty { get; } = bounty;
            internal int SkullChance { get; } = skullChance;
            internal string Skull { get; } = skull;
            internal float Size { get; } = size;
            internal string Weapon { get; } = weapon;
            internal string Firemode { get; } = firemode;
        }

        private static readonly ChaosEnemy[] ChaosBestiary =
        [
            new("fury", "Fury", 1, "venus_cat", "venus_demon_cat", 54, 3, 18, "lacer",
                CreateResists(blunt: 3, lacer: 8, beam: 8), 2, 25, "fury_sigil", 0.84f),
            new("accursed_mutant", "Accursed Mutant", 2, "moon_slave", "moon_slave", 53, 2, 13, "pierce",
                CreateResists(blunt: 7, lacer: 11), 4, 28, "carrion_mark", 0.93f, "venus_quasi_pistol_1", "quasi_1"),
            new("torment", "Torment", 2, "possessed_bigarm", "possessed_bigarm", 112, 3, 26, "lacer",
                CreateResists(blunt: 20, lacer: 17, fire: 5, beam: 12), 6, 40, "carrion_mark", 1.03f),
            new("spawn", "Chaos Spawn", 3, "possessed_bigarm", "possessed_bigarm", 148, 2, 38, "blunt",
                CreateResists(blunt: 48, pierce: 9, lacer: 19, beam: 18), 9, 50, "rib_cage", 1.10f),
            new("possessed", "Possessed Legionary", 4, "venus_knight", "venus_demon_warrior", 178, 3, 34, "lacer",
                CreateResists(blunt: 22, pierce: 32, lacer: 29, fire: 12, beam: 14), 12, 60, "step_between", 1.06f),
            new("warp_talon", "Warp Talon", 5, "venus_knight", "venus_demon_warrior", 134, 4, 27, "pierce",
                CreateResists(blunt: 10, pierce: 18, lacer: 24, shock: 4, beam: 14), 15, 65, "sever_binding", 1.06f),
            new("venomcrawler", "Venomcrawler", 6, "mars_cyborg", "mars_battle_gun", 172, 2, 27, "lacer",
                CreateResists(blunt: 35, pierce: 26, lacer: 32, fire: 21, shock: 3, beam: 20), 19, 70, "black_carapace", 1.15f,
                "fra_shock_smg_1", "lightning_2"),
            new("obliterator", "Obliterator", 7, "moon_golem", "mars_battle_gun", 224, 2, 33, "blunt",
                CreateResists(blunt: 30, pierce: 48, lacer: 26, fire: 9, shock: 10, beam: 20), 25, 75, "excruciator_chain", 1.15f,
                "army_sniper_1", "rifle_1"),
            new("soul_grinder", "Soul Grinder", 8, "mars_cyborg", "mars_battle_gun", 246, 2, 38, "blunt",
                CreateResists(blunt: 47, pierce: 39, lacer: 42, fire: 48, shock: 8, beam: 22), 34, 85, "soulforge_muster", 1.27f,
                "army_hmg_1", "rifle_5"),
            new("daemon_prince", "Undivided Daemon Prince", 9, "olirna_baron", "olirna_baron", 250, 3, 46, "chaos",
                CreateResists(blunt: 38, pierce: 34, lacer: 42, fire: 33, shock: 10, beam: 28), 46, 100, "crown_of_ash", 1.30f,
                "venus_quasi_pistol_1", "quasi_1"),
            new("belakor", "Be’lakor, the Dark Master", 10, "olirna_baron", "olirna_baron", 360, 3, 32, "chaos",
                CreateResists(blunt: 45, pierce: 37, lacer: 48, fire: 30, shock: 24, beam: 30), 70, 100, "black_meridian", 1.42f)
        ];

        private static readonly Dictionary<string, ChaosEnemy> ChaosEnemies = ChaosBestiary.ToDictionary(entry => entry.Id);

        private static void RegisterChaosEnemies()
        {
            foreach (ChaosEnemy enemy in ChaosBestiary)
            {
                var bodyDonor = Data.BodyTypes.GetRecord(enemy.Body);
                var actorDonor = Data.Actors.GetRecord(bodyDonor.ActorId);
                var template = (ActorDescriptor)actorDonor.ContentDescriptor;
                var actor = Object.Instantiate(template);
                actor.name = enemy.Actor;
                ApplyChaosActorModel(enemy, actor, template);
                actor._putCorpse = true;
                actor._deathDissolveEffect = false;
                StageRecord(Data.Actors, enemy.Actor, new ActorRecord { Id = enemy.Actor, ContentDescriptor = actor, Gender = Gender.None });
                StageDescriptor("actors", enemy.Actor, actor);
                RegisterChaosCreaturePortrait(enemy);

                string bite = RegisterChaosNaturalWeapon(enemy);
                var body = (BodyTypeRecord)CloneRecordMethod.Invoke(bodyDonor, null);
                body.Id = enemy.Id + "_body";
                body.ActorId = enemy.Actor;
                body.Health = enemy.Health;
                if (enemy.Body == "mars_cyborg")
                {
                    // Native cyborg anatomy applies its max-health penalties again when the actor spawns.
                    body.Health -= (int)body.WoundSlots.Sum(id => Data.WoundSlots.GetRecord(id).ImplicitPenaltyEffects?
                        .TryGetValue("max_health", out float penalty) == true ? penalty : 0f);
                }
                body.BaseActionPoints = enemy.AP;
                body.ResistSheet = [.. enemy.Resist, new() { damage = "chaos", resistPercent = enemy.Tier * 5 }];
                body.MeleeDamage = new DmgInfo { damage = enemy.Damage };
                body.OverrideBareHandWeapon = bite;
                body.HasInventoryWeaponSlots = enemy.Weapon != null;
                body.HasInventoryEquipmentSlots = false;
                body.Eccolapse = false;
                body.Immobile = false;
                body.CanFly = enemy.Id == ChaosItemPrefix + "warp_talon";
                body.IgnoreThrowback |= enemy.Id is ChaosItemPrefix + "spawn" or ChaosItemPrefix + "soul_grinder";
                if (enemy.Id == ChaosFinale)
                {
                    body.MeleeDamage = new DmgInfo { damage = enemy.Damage, minDmg = enemy.Melee, maxDmg = enemy.Melee + 12 };
                    body.AttackWoundChanceMult = Mathf.Min(body.AttackWoundChanceMult, 0.65f);
                }
                body.ScaleQuasimorphStats = false;
                body.DeathExplosionId = string.Empty;
                body.QuasimorphosisReward = -3 * enemy.Tier;
                body.PainThresholdLimit = Math.Max(9, enemy.Health / 5);
                body.PainThresholdRegen = 2;
                StageRecord(Data.BodyTypes, body.Id, body);

                string[] weapons = enemy.Weapon == null ? [] : [RegisterChaosIntegralGun(enemy)];
                var role = RegisterEquippedRole(enemy.Id, enemy.Role, enemy.Name, ChaosFaction, weapons);
                role.BodyTypes = [body.Id];
                role.PortraitId = enemy.Id + "_portrait";
                var ai = (AiPresetRecord)CloneRecordMethod.Invoke(Data.AiPresets.GetRecord(role.AiPresetId), null);
                ai.Id = enemy.Id + "_ai";
                ai.SomniaBehave = SomniaBehave.Ignore;
                ai.PeriodicallySleeps = false;
                if (enemy.Id is ChaosPrince or ChaosFinale) ai.MaxAttacksPercent = 0.55f;
                StageRecord(Data.AiPresets, ai.Id, ai);
                role.AiPresetId = ai.Id;
                role.HealthMod = 0;
                role.ActionPointsMod = 0;
                role.DodgeMod = enemy.Id == ChaosItemPrefix + "warp_talon" ? 0.12f : 0;
                role.MeleeAccuracyMod = enemy.Tier >= 4 ? 0.08f : 0;
                role.RangeAccuracyMod = enemy.Weapon == null ? 0 : 0.10f;
                role.ResistMod = [];
                role.AdditAmmo = new(0, 0);
                role.AdditItemCount = new(0, 0);
                role.GrantedAugmentations = [];
                role.GrantedImplants = [];
                role.AugCount = new(0, 0);
                role.ImplantCount = new(0, 0);
                role.AllowMutate = false;
                role.AllowEccolapse = false;
                role.Los = enemy.Weapon == null ? 7 : 9;
            }
        }

        private static string RegisterChaosNaturalWeapon(ChaosEnemy enemy)
        {
            string id = enemy.Id + "_strike";
            var attack = ChaosItem<WeaponRecord>(id, "nature_cyborg_recreation_fist", enemy.Tier, 0);
            attack.Categories = [];
            attack.Unbreakable = true;
            attack.GetMeleeDamageFromCreature = enemy.Id == ChaosFinale;
            attack.Firemodes = [enemy.Id == ChaosItemPrefix + "spawn" ? "smash_2" : "smash_1"];
            string ammo = enemy.Damage switch
            {
                "lacer" => "implicted_sword_brutal",
                "pierce" => "implicted_claw",
                "chaos" => "implicted_xiomara_shrapnel",
                _ => "implicted_club"
            };
            attack.DefaultAmmoId = ammo;
            attack.OverrideAmmo = [];
            var damage = attack.Damage;
            damage.damage = enemy.Damage;
            damage.minDmg = enemy.Melee;
            damage.maxDmg = enemy.Melee + 9 + enemy.Tier;
            attack.Damage = damage;
            attack.Traits = enemy.Id == ChaosItemPrefix + "warp_talon" ? ["piercing"] : [];
            RegisterItem("meleeweapons", attack, enemy.Name + " Strike", "The creature's natural close-combat attack.");
            return id;
        }

        private static string RegisterChaosIntegralGun(ChaosEnemy enemy)
        {
            string id = enemy.Id + "_integral";
            var weapon = ChaosItem<WeaponRecord>(id, enemy.Weapon, enemy.Tier, 0);
            weapon.Categories = [];
            weapon.IsImplicit = true;
            weapon.Unbreakable = true;
            weapon.RequiredAmmo = string.Empty;
            weapon.MinRandomAmmoCount = weapon.MagazineCapacity;
            weapon.Firemodes = [enemy.Firemode];
            if (enemy.Id == ChaosItemPrefix + "obliterator")
            {
                weapon.Range = 7;
                weapon.DefaultAmmoId = "rifle_armorpierce_ammo";
                SetDamage(weapon, 48, 70);
            }
            if (enemy.Id == ChaosMutant)
            {
                weapon.Range = 4;
                SetDamage(weapon, 12, 22);
            }
            if (enemy.Id == ChaosItemPrefix + "venomcrawler")
            {
                weapon.Range = 6;
                SetDamage(weapon, 20, 39);
            }
            if (enemy.Id == ChaosItemPrefix + "soul_grinder")
            {
                weapon.BonusAccuracy = 0.10f;
                SetDamage(weapon, 22, 34);
            }
            if (enemy.Id == ChaosPrince)
            {
                weapon.Range = 6;
                weapon.OverrideAmmo = ["implicted_xiomara_range"];
                SetDamage(weapon, 23, 34);
            }
            ((WeaponDescriptor)weapon.ItemDesc)._prefab = null;
            RegisterItem("rangeweapons", weapon, enemy.Name + " Integral Weapon", "A ranged weapon built into the creature's body.");
            return id;
        }

        [HarmonyPatch(typeof(CreatureSystem), nameof(CreatureSystem.SpawnMonsterFromMobClass))]
        private static class ChaosEnemyPacts
        {
            private static void Postfix(PerkFactory perkFactory, string mobClassId, CreatureAlliance creatureAlliance, Monster __result)
            {
                if (__result == null || creatureAlliance == CreatureAlliance.PlayerAlliance || !ChaosEnemies.TryGetValue(mobClassId, out var enemy) ||
                    enemy.Tier < 4 || Data.Items.GetSimpleRecord<SkullRecord>(ChaosSkullId(enemy.Skull)) is not { CanBaronUse: true } skull)
                {
                    return;
                }
                var data = __result.CreatureData;
                data.HasUltimate = true;
                data.UltimateSkullItemId = skull.Id;
                data.MonsterUltimatePerk = perkFactory.CreatePerk(Data.Perks.GetRecord(skull.PerkId));
            }
        }

        [HarmonyPatch(typeof(Creature), "TryCreateUltimateItem")]
        private static class ChaosKillLoot
        {
            private static bool Prefix(Monster monster, Creatures ____creatures, RaidMetadata ____raidMetadata)
            {
                var data = monster.CreatureData;
                if (!ChaosEnemies.TryGetValue(data.MobClassId, out var enemy))
                {
                    return true;
                }
                if (data.IsSummonedCreature || ____creatures.Player == null || monster.IsAlly(____creatures.Player) ||
                    data.Health.ReasonOfDeath == HealthInfo.DeathReason.EccolapseBack)
                {
                    return false;
                }
                string bountyKey = "iom.chaos.bounty." + data.UniqueId;
                if (____raidMetadata.CustomCounters.ContainsKey(bountyKey))
                {
                    return false;
                }
                ____raidMetadata.CustomCounters[bountyKey] = 1;
                var tier = _chaosSupply[enemy.Tier - 1];
                var loot = new List<BasePickupItem>();
                ItemInteractionSystem.CreateItem(loot, WarpScrip, enemy.Bounty);
                ItemInteractionSystem.CreateItem(loot, tier.Salvage.Id, enemy.Tier >= 6 ? 2 : 1);
                ItemInteractionSystem.CreateItem(loot, enemy.Tier >= 6 ? ForgeEmber : BoundAsh, enemy.Tier >= 8 ? 2 : 1);
                if (UnityEngine.Random.Range(0, 100) < enemy.SkullChance)
                {
                    ItemInteractionSystem.CreateItem(loot, ChaosSkullId(enemy.Skull), 1);
                }
                if (enemy.Id == ChaosFinale && !____raidMetadata.CustomCounters.ContainsKey(FinaleClaimed))
                {
                    ____raidMetadata.CustomCounters[FinaleClaimed] = 1;
                    ItemInteractionSystem.CreateItem(loot, tier.Weapon.Id, 1);
                    ItemInteractionSystem.CreateItem(loot, tier.Chip, 1);
                }
                foreach (var item in loot)
                {
                    data.Inventory.BackpackStore.AddItemAndReshuffle(item);
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(Monster), nameof(Monster.CloneInventoryForCorpse))]
        private static class KeepIntegralWeaponsOnCreature
        {
            private static void Prefix(Monster __instance)
            {
                if (!ChaosEnemies.ContainsKey(__instance.CreatureData.MobClassId))
                {
                    return;
                }
                var inventory = __instance.CreatureData.Inventory;
                foreach (var storage in inventory.Slots.Concat(inventory.Storages))
                {
                    foreach (var weapon in storage.Items.Where(item =>
                        item.Id.StartsWith(ChaosItemPrefix, StringComparison.Ordinal) &&
                        item.Id.EndsWith("_integral", StringComparison.Ordinal)).ToArray())
                    {
                        storage.Remove(weapon, sendEvent: false);
                    }
                }
            }
        }
    }
}

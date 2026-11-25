using System;
using System.Collections.Generic;
using System.Linq;
using MGSC;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private const string ChaosFaction = "iom_chaos_undivided";
        private const string ChaosItemPrefix = "iom_chaos_";
        private const string HideBinding = ChaosItemPrefix + "hide_binding";
        private const string Oathbread = ChaosItemPrefix + "oathbread";
        private const string ShardCells = ChaosItemPrefix + "shard_cells";
        private const string BoneSplint = ChaosItemPrefix + "bone_splint";
        private const string RivetKit = ChaosItemPrefix + "rivet_kit";
        private const string TalonSmoke = ChaosItemPrefix + "talon_smoke";
        private const string CoilCells = ChaosItemPrefix + "coil_cells";
        private const string FleshmetalSpanner = ChaosItemPrefix + "fleshmetal_spanner";
        private const string CinderSalve = ChaosItemPrefix + "cinder_salve";
        private const string InfernalDraught = ChaosItemPrefix + "infernal_draught";
        private const string AnchorFragment = ChaosItemPrefix + "shadow_anchor_fragment";
        private const string UmbralSuture = ChaosItemPrefix + "umbral_suture";

        private sealed class ChaosSupplyTier(int level, WeaponRecord weapon, string[] armor, ItemRecord salvage,
            ItemRecord utility)
        {
            internal readonly int Level = level;
            internal readonly WeaponRecord Weapon = weapon;
            internal readonly string[] Armor = armor;
            internal readonly ItemRecord Salvage = salvage;
            internal readonly ItemRecord Utility = utility;
            internal string Chip => ChaosItemPrefix + "tier_" + Level + "_chip";
        }

        private static ChaosSupplyTier[] _chaosSupply;

        private static void RegisterChaosEquipment()
        {
            var bread = ChaosItem<ConsumableRecord>(Oathbread, "ration_pack_3", 1, 11);
            bread.StarvationValue = 280;
            bread.QmorphosValue = 20;
            AddChaosItem("consumables", bread, "Oathbread", "A compact cult ration that raises local Quasimorphosis.");

            var shards = ChaosItem<AmmoRecord>(ShardCells, "quasi_basic_ammo", 2, 11);
            AddChaosItem("ammo", shards, "Shard Cells", "Quasi cells issued in cult supply pouches. Compatible with the Shard Pistol.");

            var splint = ChaosItem<FixationMedicineRecord>(BoneSplint, "splint", 3, 27);
            splint.HealWoundChances = new() { ["blunt"] = 0.8f };
            AddChaosItem("fixation_medicine", splint, "Bone Splint", "A lightweight dressing for blunt wounds.");

            var rivets = ChaosItem<RepairRecord>(RivetKit, "armor_repair_kit", 4, 105);
            rivets.RestoreAmount = 30;
            AddChaosItem("repairs", rivets, "Binding Rivet Kit", "A portable repair kit for ordinary armor.");

            var smoke = ChaosItem<GrenadeRecord>(TalonSmoke, "smoke_grenade", 5, 78);
            AddChaosItem("grenades", smoke, "Talon Smoke Charge", "Deploys smoke for crossing exposed lanes.");

            var coils = ChaosItem<AmmoRecord>(CoilCells, "battery_basic_ammo", 6, 12);
            coils.DmgType = "shock";
            AddChaosItem("ammo", coils, "Excruciator Coil Cells", "Power cells that deal shock damage in weapons whose ammunition determines the damage type.");

            var spanner = ChaosItem<RepairRecord>(FleshmetalSpanner, "tech_repair_kit", 7, 175);
            AddChaosItem("repairs", spanner, "Fleshmetal Spanner", "A field kit for compatible energy weapons.");

            var salve = ChaosItem<FixationMedicineRecord>(CinderSalve, "goldenstar_balm", 8, 66);
            salve.HealWoundChances = new() { ["fire"] = 0.85f, ["plasma"] = 0.45f };
            salve.EffectProgression = new();
            AddChaosItem("fixation_medicine", salve, "Cinder Salve", "A dressing for fire and plasma wounds.");

            var draught = ChaosItem<ConsumableRecord>(InfernalDraught, "pills_painkillers", 9, 98);
            draught.PainValue = -35;
            draught.QmorphosValue = 65;
            draught.EffectProgression = new();
            AddChaosItem("consumables", draught, "Infernal Draught", "Suppresses pain and raises local Quasimorphosis.");

            var suture = ChaosItem<FixationMedicineRecord>(UmbralSuture, "surgical_kit", 10, 135);
            suture.HealWoundChances = new() { ["chaos"] = 0.9f };
            suture.QmorphosValue = 35;
            AddChaosItem("fixation_medicine", suture, "Umbral Suture", "Treats Chaos wounds and raises local Quasimorphosis.");

            _chaosSupply =
            [
                new(1, ChaosWeapon(1, "fury_fang_knife", "bone_knife", "Fury Fang Knife", "An unpowered lacerating knife.",
                        80, 0.45f, 50, 20, 27, "lacer", ["stab_1"],
                        attackAmmo: "implicted_sword_brutal"),
                    ChaosArmor(1, "hidewraps", "Hidewraps", ["Hood", "Vest", "Wraps", "Soft Boots"],
                        ["military_light_helmet_1", "miner_shirt_1", "miner_pants_1", "miner_boots_1"],
                        "Lightweight hide and cloth for crews travelling with little equipment.", 53, 0.95f, 55, CreateResists(blunt: 5, pierce: 2, lacer: 7, cold: 4)),
                    ChaosRepairSalvage(1, HideBinding, "mercury_rags", "Hide Binding",
                        "Flexible salvage for field repairs and Hidewrap gear.", 12),
                    bread),
                new(2, ChaosWeapon(2, "shard_pistol", "venus_quasi_pistol_1", "Shard Pistol", "A close-range pistol that takes quasi cells.",
                        155, 0.55f, 80, 23, 34, null, ["quasi_1", "quasi_2"], 4, 10, ShardCells),
                    ChaosArmor(2, "ritual_flak", "Ritual Flak", ["Veil", "Coat", "Trousers", "Boots"],
                        ["military_mask_1", "military_light_armor_1", "military_light_pants_1", "military_light_boots_1"],
                        "Cult flak layered against lacerations.", 115, 2.2f, 90,
                        CreateResists(blunt: 9, pierce: 4, lacer: 17, cold: 7)),
                    ChaosSalvage(2, "shard_glass", "mercury_glass", "Shard Glass", "Shaped salvage for ritual flak and quasi cells.", 23), shards),
                new(3, ChaosWeapon(3, "spawnbone_maul", "moon_mace_1", "Spawnbone Maul", "A heavy blunt weapon with a slow committed swing.",
                        220, 1.9f, 120, 46, 57, "blunt", ["smash_2"]),
                    ChaosArmor(3, "ribplate", "Ribplate", ["Mask", "Cuirass", "Leggings", "Boots"],
                        ["military_heavy_helmet_1", "military_heavy_armor_1", "military_heavy_pants_1", "military_heavy_boots_1"],
                        "Heavy rib plates absorb blunt hits.", 180, 4.1f, 130,
                        CreateResists(blunt: 24, pierce: 10, lacer: 13, fire: 9)),
                    ChaosSalvage(3, "spawn_rib", "demonic_eye", "Spawn Rib", "Dense rib stock for plate, mauls and splints.", 31), splint),
                new(4, ChaosWeapon(4, "bound_cleaver", "common_axe_1", "Bound Cleaver", "A portable cleaver bound to a stable grip.",
                        320, 1.4f, 145, 44, 60, "lacer", ["chop_strong_1", "chop_2"]),
                    ChaosArmor(4, "possessed_carapace", "Possessed Carapace", ["Helm", "Chestplate", "Greaves", "Boots"],
                        ["military_medium_helmet_1", "military_medium_armor_1", "military_medium_pants_1", "military_medium_boots_1"],
                        "Recovered plate with balanced protection and a binding against Chaos damage.", 315, 3.75f, 154, CreateResists(blunt: 15, pierce: 18, lacer: 16, fire: 10, cold: 8), chaos: 8),
                    ChaosSalvage(4, "binding_icon", "demonic_eye", "Binding Icon", "The bound core of portable cleavers and Possessed plate.",
                        51), rivets),
                new(5, ChaosWeapon(5, "warp_claw_gauntlet", "civ_powerfist_1", "Warp-Claw Gauntlet", "A fast powered strike designed to puncture plate.",
                        455, 1.10f, 150, 24, 32, "pierce", ["smash_power_1"], 1, 9,
                        "battery_basic_ammo", "implicted_claw"),
                    ChaosArmor(5, "talon_plate", "Talon Plate", ["Helm", "Flight Cuirass", "Greaves", "Landing Boots"],
                        ["military_medium_helmet_1", "military_medium_armor_1", "military_medium_pants_1", "military_medium_boots_1"],
                        "Light assault plate with less blunt protection than a heavy siege set.", 330, 3.0f, 155, CreateResists(blunt: 12, pierce: 20, lacer: 18, fire: 12, cold: 9), chaos: 10),
                    ChaosSalvage(5, "talon_actuator", "transformer", "Talon Actuator", "An actuator salvaged from a damaged Talon harness.",
                        67), smoke),
                new(6, ChaosWeapon(6, "excruciator_carbine", "fra_shock_smg_1", "Excruciator Carbine", "A compact shock weapon with two-round bursts.",
                        570, 2.95f, 180, 18, 43, null, ["lightning_2"], 5, 18, CoilCells),
                    ChaosArmor(6, "forge_carapace", "Forge Carapace", ["Visor", "Insulated Shell", "Trousers", "Boots"],
                        ["fra_heavy_mask_1", "fra_heavy_armor_1", "fra_heavy_pants_1", "fra_heavy_boots_1"],
                        "Insulated forge gear with strong shock protection.", 440, 4.4f, 185, CreateResists(blunt: 17, pierce: 18, lacer: 15, fire: 16, shock: 32), chaos: 9),
                    ChaosSalvage(6, "insulated_conduit", "wire", "Insulated Conduit", "A salvaged forge conduit for shock weapons and insulation.",
                        79), coils),
                new(7, ChaosWeapon(7, "fleshmetal_projector", "army_sniper_1", "Fleshmetal Projector", "A close-range heavy-ammo weapon built around a single armor-piercing shot.",
                        740, 4.5f, 200, 54, 72, null, ["rifle_1"], 4, 4, "rifle_armorpierce_ammo"),
                    ChaosArmor(7, "fleshmetal_plate", "Fleshmetal Plate", ["Helm", "Breastplate", "Greaves", "Sabatons"],
                        ["military_power_helmet_1", "military_power_armor_1", "military_power_pants_1", "military_power_boots_1"],
                        "Heavy fused plates offer high pierce protection at substantial weight.", 600, 5.85f, 215, CreateResists(blunt: 22, pierce: 37, lacer: 18, fire: 12, shock: 9), chaos: 14),
                    ChaosRepairSalvage(7, ChaosItemPrefix + "fleshmetal_blank", "armor_plates", "Fleshmetal Blank",
                        "A plate of fused stock for heavy arms and armor repairs.", 115),
                    spanner),
                new(8, ChaosWeapon(8, "reforged_harvester", "army_hmg_1", "Reforged Harvester", "A heavy support gun with five- and eight-round bursts.",
                        920, 7.4f, 270, 33, 52, null, ["rifle_5", "rifle_8"], 6, 40, "rifle_basic_ammo"),
                    ChaosArmor(8, "soulforge_siege_plate", "Soulforge Siege Plate", ["Visor", "Breastplate", "Legplates", "Tread Boots"],
                        ["military_power_helmet_1", "military_power_armor_1", "military_power_pants_1", "military_power_boots_1"],
                        "Heavy siege plate designed for blunt impacts and fire.", 750, 7.3f, 255, CreateResists(blunt: 36, pierce: 31, lacer: 19, fire: 34, shock: 10), chaos: 17),
                    ChaosSalvage(8, "harvester_feed_block", "weapon_parts", "Harvester Feed Block", "A feed assembly for the Harvester and Soulforge plate.",
                        142), salve),
                new(9, ChaosWeapon(9, "hellforged_blade", "tech_blade_1", "Hellforged Blade", "A heavy Chaos blade that consumes two power cells per powered swing.",
                        1050, 2.4f, 185, 47, 66, "chaos", ["slash_power_2"], 1, 12,
                        "battery_basic_ammo", "implicted_xiomara_shrapnel"),
                    ChaosArmor(9, "hellforged_panoply", "Hellforged Panoply", ["Crown-Helm", "Cuirass", "Greaves", "Sabatons"],
                        ["military_power_helmet_1", "military_power_armor_1", "military_power_pants_1", "military_power_boots_1"],
                        "Heavy infernal plate with strong Chaos protection and limited insulation.", 900, 6.9f, 270, CreateResists(blunt: 25, pierce: 29, lacer: 22, fire: 20, shock: 6), chaos: 38),
                    ChaosSalvage(9, "infernal_seal", "demonic_eye", "Infernal Seal", "A seal for binding the blade and fitting the panoply.",
                        177), draught),
                new(10, ChaosWeapon(10, "shadowblade", "tech_blade_1", "Shadowblade", "A unique relic with a charged single-target Chaos strike.",
                        1500, 1.85f, 220, 65, 90, "chaos", ["stab_power_2"], 1, 10,
                        "battery_basic_ammo", "implicted_xiomara_shrapnel"),
                    ChaosArmor(10, "shadowbound_plate", "Shadowbound Plate", ["Crown", "Mantle", "Greaves", "Treads"],
                        ["fra_heavy_mask_1", "military_power_armor_1", "military_power_pants_1", "military_power_boots_1"],
                        "Shadowbound relic armor with high Chaos resistance and moderate ballistic protection.", 1060, 5.6f, 285, CreateResists(blunt: 25, pierce: 25, lacer: 23, fire: 21, cold: 15, shock: 11), chaos: 51),
                    ChaosRepairSalvage(10, AnchorFragment, "circuit_board", "Shadow Anchor Fragment",
                        "An anchor fragment used to bind Shadowbound gear and repair Shadowblade.", 220),
                    suture)
            ];
        }

        private static T ChaosItem<T>(string id, string donor, int level, float price) where T : ItemRecord
        {
            var item = CloneItem<T>(id, donor, level, price);
            item.Categories = [ChaosFaction];
            return item;
        }

        private static void AddChaosItem(string table, ItemRecord item, string name, string description)
        {
            ApplyChaosItemArtwork(item);
            RegisterItem(table, item, name, description);
        }

        private static TrashRecord ChaosSalvage(int level, string id, string donor, string name, string description,
            float price) =>
            AddMaterial(ChaosItemPrefix + id, donor, name, description, price, level, ChaosFaction);

        private static RepairRecord ChaosRepairSalvage(int level, string id, string donor, string name,
            string description, float price)
        {
            var salvage = ChaosItem<RepairRecord>(id, donor, level, price);
            AddChaosItem("repairs", salvage, name, description);
            return salvage;
        }

        private static WeaponRecord ChaosWeapon(int level, string id, string donor, string name, string description,
            float price, float weight, int durability, int min, int max, string damageType, string[] modes,
            int range = 1, int capacity = 0, string ammo = null, string attackAmmo = null)
        {
            var weapon = ChaosItem<WeaponRecord>(ChaosItemPrefix + id, donor, level, price);
            weapon.Weight = weight;
            weapon.Range = range;
            weapon.MaxDurability = durability;
            weapon.Firemodes = [.. modes];
            weapon.MagazineCapacity = capacity;
            weapon.MinRandomAmmoCount = capacity / 2;
            if (ammo != null)
            {
                weapon.DefaultAmmoId = ammo;
            }
            if (attackAmmo != null)
            {
                weapon.OverrideAmmo = [attackAmmo];
            }
            SetDamage(weapon, min, max);
            if (damageType != null)
            {
                var damage = weapon.Damage;
                damage.damage = damageType;
                weapon.Damage = damage;
            }
            if (level == 5)
            {
                weapon.Traits = [.. weapon.Traits, "piercing"];
            }
            if (level == 10)
            {
                weapon.Categories = [ChaosFaction + "_relic"];
                weapon.RepairItemIds = [AnchorFragment, "circuit_board"];
                weapon.Disassembly = [];
            }
            AddChaosItem(weapon.IsMelee ? "meleeweapons" : "rangeweapons", weapon, name, description);
            return weapon;
        }

        private static string[] ChaosArmor(int level, string key, string name, string[] pieceNames, string[] donors,
            string description, float price, float weight, int durability, List<DmgResist> resists,
            float chaos = 0)
        {
            if (chaos > 0)
            {
                resists.Add(new DmgResist { damage = "chaos", resistPercent = chaos });
            }
            string id = ChaosItemPrefix + key;
            return
            [
                AddChaosArmorPiece<HelmetRecord>("helmets", id + "_head", 0, 0.57f, 0.50f),
                AddChaosArmorPiece<ArmorRecord>("armors", id + "_torso", 1, 1f, 1f),
                AddChaosArmorPiece<LeggingsRecord>("leggings", id + "_legs", 2, 0.75f, 0.74f),
                AddChaosArmorPiece<BootsRecord>("boots", id + "_boots", 3, 0.49f, 0.47f)
            ];

            string AddChaosArmorPiece<T>(string table, string pieceId, int slot, float scale, float weightScale)
                where T : ResistRecord, IArmorRecord
            {
                var item = ChaosItem<T>(pieceId, donors[slot], level, (float)Math.Round(price * scale));
                item.Weight = (float)Math.Round(weight * weightScale, 2);
                item.MaxDurability = durability;
                item.ResistSheet = resists.Select(resist => new DmgResist
                {
                    damage = resist.damage, resistPercent = (float)Math.Round(resist.resistPercent * scale)
                }).ToList();
                if (level == 1)
                {
                    item.RepairItemIds = [HideBinding, "rags"];
                }
                if (level == 7)
                {
                    item.RepairItemIds = [ChaosItemPrefix + "fleshmetal_blank", "armor_plates"];
                }
                AddChaosItem(table, item, name + " " + pieceNames[slot], description);
                return item.Id;
            }
        }
    }
}

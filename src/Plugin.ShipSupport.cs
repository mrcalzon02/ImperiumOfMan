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
        private const string WeaponsModule = "iom_ship_weapons";
        private const string DropModule = "iom_drop_support";
        private const string BarrageDuration = "iom_barrage_duration";
        private const string BarrageRounds = "iom_barrage_rounds";
        private const string BarrageCalls = "iom_barrage_calls";
        private const string SupplyDrop = "iom_supply_pod";
        private const string TroopDrop = "iom_mercenary_pod";
        private const string PodCapacity = "iom_pod_capacity";
        private const string PodCalls = "iom_pod_calls";
        private const string SelectedMount = "iom.support.selected_mount";
        private const string SelectedSquad = "iom.support.selected_squad";

        private sealed class ShipMount(string id, string art, string name, string description,
            string explosion, int radius, int rounds, int damage, string damageType)
        {
            public readonly string Id = id;
            public readonly string Art = art;
            public readonly string Name = name;
            public readonly string Description = description;
            public readonly string Explosion = explosion;
            public readonly int Radius = radius;
            public readonly int Rounds = rounds;
            public readonly int Damage = damage;
            public readonly string DamageType = damageType;
        }

        private static readonly ShipMount[] ShipMounts =
        [
            new("iom_gun_mount", "GunMount", "Gun Mount", "Fire a tight salvo of two shell impacts.",
                "frag_low_explosion", 1, 2, 43, "explosion"),
            new("iom_missile_mount", "MissileMount", "Missile Mount", "Fire a wide blast with an extra turn of warning.",
                "rocket_basic_explosion", 3, 1, 78, "explosion"),
            new("iom_laser_mount", "LaserMount", "Laser Mount", "Strike a narrow three-tile line with beam damage.",
                "flak_plasma_explosion", 0, 3, 67, "beam")
        ];

        private sealed class DropSquad(int tier, string name, string leader, string partner)
        {
            public readonly int Tier = tier;
            public readonly string Name = name;
            public readonly string Leader = leader;
            public readonly string Partner = partner;
        }

        private static readonly DropSquad[] DropSquads =
        [
            new(1, "Clone Guardsmen", "iom_drop_clone_guard", "iom_drop_clone_guard"),
            new(2, "Cadian Clones", CadianRole, CadianRole),
            new(3, "Arbites", VigilantRole, SubductorRole),
            new(3, "Valhallans", IceRiflemanRole, IceMarksmanRole),
            new(3, "Steel Legion", LegionRiflemanRole, LegionSupportRole),
            new(4, "Elysian Drop Troops", DropRiflemanRole, DropSpotterRole),
            new(5, "Death Korps of Krieg", SiegeRiflemanRole, CombatEngineerRole),
            new(6, "Tempestus Scions", ScionRiflemanRole, ScionSpecialistRole),
            new(7, "Skitarii", RangerRole, VanguardRole),
            new(8, "Battle Sisters", BattleSisterRole, FlamerSisterRole),
            new(9, "Ordo Xenos", InterrogatorRole, PistoleerRole),
            new(10, "Grey Knights Gunner", PaladinGunnerRole, ArmsmanRole),
            new(10, "Grey Knights", PaladinRole, ArmsmanRole),
            new(11, "Golden Paladin Guard", GoldenPaladinRole, ArmsmanRole)
        ];

        private static string TroopTier(int tier) => tier == 1 ? TroopDrop : "iom_drop_troops_" + tier;

        private static readonly Dictionary<string, Sprite> SupportSprites = new();

        private static void RegisterShipSupportUpgrades()
        {
            RegisterImperialRole(DropSquads[0].Leader, "soldier", "Clone Guardsman",
                _lasgun.Id, _lightHelmet.Id, _lightChestplate.Id, _lightLeggings.Id, _lightBoots.Id);

            AddModuleText(WeaponsModule, "Ship Weapons", "Fit mounts and upgrade Fire for Effect.");
            AddModuleText(DropModule, "Drop Support", "Prepare supply pods and Imperial troop detachments.");
            TranslationPatch.AddShipSupportText("iom.support.confirm", "Confirm call-in");
            for (int i = 0; i < ShipMounts.Length; i++)
            {
                var mount = ShipMounts[i];
                RegisterSupportPerk(WeaponsModule, mount.Id, mount.Art, mount.Name,
                    mount.Description + " Select a fitted mount in the ship loadout.", i + 1, 0, null,
                    ["weapon_container", "engineering_parts_container", "military_parts_container",
                        i == 0 ? "tool_container" : i == 1 ? "construction_equipment" : "prototype_schematics"]);
            }
            RegisterSupportPerk(WeaponsModule, BarrageDuration, "BarrageDuration", "Sustained Fire",
                "Add one turn of fire to each call. Requires any fitted mount.", 1, 2, null,
                ["engineering_parts_container", "military_parts_container", "communication_relay"]);
            RegisterSupportPerk(WeaponsModule, BarrageRounds, "RoundsFired", "Increased Salvo",
                "Add one shell, missile, or laser cell per firing turn. Requires any fitted mount.", 2, 2, null,
                ["weapon_container", "military_parts_container", "military_parts_container"]);
            RegisterSupportPerk(WeaponsModule, BarrageCalls, "AdditionalCallIns", "Fire Support Reserve",
                "Add one Fire for Effect call per raid. Requires any fitted mount.", 3, 2, null,
                ["communication_relay", "military_parts_container", "management_unit"]);

            var supply = RegisterSupportPerk(DropModule, SupplyDrop, "SupplyPod", "Supply Pod",
                "Call down a cache of Las Clips, standard ammunition and dressings once per raid.", 0, 0, null,
                ["engineering_parts_container", "communication_relay", "medical_equipment"]);
            var troop = RegisterSupportPerk(DropModule, TroopDrop, "MercenaryPod", "Mercenary Pod",
                "Deploy two clone Guardsmen with PDF flak and Lasguns. Shares the drop reserve with supplies.", 1, 0, supply,
                ["military_unit", "armor_container", "biomass_tank"]);
            RegisterSupportPerk(DropModule, PodCapacity, "MercenaryEquipment", "Expanded Troop Pod",
                "Add one trooper to every crewed drop.", 2, 0, troop,
                ["construction_equipment", "armor_container", "engineering_parts_container"]);
            RegisterSupportPerk(DropModule, PodCalls, "AdditionalCallIns", "Drop Support Reserve",
                "Add one supply or troop call per raid.", 0, 1, supply,
                ["communication_relay", "engineering_parts_container", "management_unit"]);

            string[] trainingArt = ["MercenaryWeapons", "MercenaryArmor", "MercenaryEquipment"];
            for (int tier = 2; tier <= 11; tier++)
            {
                string roster = string.Join(" / ", DropSquads.Where(squad => squad.Tier == tier).Select(squad => squad.Name));
                int column = tier <= 6 ? tier - 1 : 12 - tier;
                troop = RegisterSupportPerk(DropModule, TroopTier(tier), trainingArt[(tier - 2) % trainingArt.Length],
                    $"Tech {tier} · {roster}", $"Unlock this roster for troop drops. Requires Imperial Tech {tier}.",
                    column, tier <= 6 ? 1 : 2, troop, TroopUpgradePrice(tier));
            }
        }

        private static List<string> TroopUpgradePrice(int tier) => tier switch
        {
            <= 4 => ["military_unit", "military_parts_container"],
            <= 7 => ["military_unit", "armor_container", "weapon_container"],
            <= 9 => ["military_unit", "expert_disk", "armor_container", "weapon_container"],
            _ => ["military_unit", "prototype_schematics", "prototype_schematics", "armor_container", "weapon_container"]
        };

        private static MagnumPerkRecord RegisterSupportPerk(string module, string id, string art, string name,
            string description, int column, int row, MagnumPerkRecord parent, List<string> price)
        {
            var descriptor = ScriptableObject.CreateInstance<MagnumPerkDescriptor>();
            descriptor.name = id;
            descriptor._icon = SupportIcon(art);
            descriptor._lockedIcon = SupportIcon(art, locked: true);
            var perk = new MagnumPerkRecord
            {
                Id = id,
                Enabled = true,
                ModuleId = module,
                DepartmentId = string.Empty,
                Parents = parent == null ? [] : [parent.Id],
                Modifiers = [],
                PositionInGraph = new CellPosition(column, row),
                UpgradePrice = price,
                ContentDescriptor = descriptor
            };
            parent?.Childs.Add(id);
            StageRecord(Data.MagnumPerks, id, perk);
            TranslationPatch.AddMagnumUpgrade(id, name, description);
            return perk;
        }

        private static Sprite SupportIcon(string art, bool locked = false)
        {
            string key = locked ? art + "Locked" : art;
            if (SupportSprites.TryGetValue(key, out var sprite))
            {
                return sprite;
            }
            if (!locked)
            {
                return SupportSprites[key] = ReadItemSprite(art, null, keepReadable: true);
            }
            // Occult research ships matched native-green/crimson artwork. Use the
            // explicit locked image, keeping the existing fallback for other art.
            if (art.StartsWith("OccultResearch", StringComparison.Ordinal) &&
                SpriteResources.ContainsKey("ImperiumOfMan.Art." + key + ".png"))
            {
                return SupportSprites[key] = ReadItemSprite(key, null);
            }
            var source = SupportIcon(art).texture;
            var pixels = source.GetPixels32();
            for (int i = 0; i < pixels.Length; i++)
            {
                Color32 pixel = pixels[i];
                int brightness = Math.Max(pixel.r, Math.Max(pixel.g, pixel.b));
                if (pixel.a == 0 || brightness <= 8)
                {
                    continue;
                }
                // Match the native tree's crimson shadow, line and highlight palette.
                // Keep the original dark background and alpha; retain the artwork's
                // separate brightness levels instead of tinting it to a flat silhouette.
                Color32 shadow = new(88, 7, 28, 255);
                Color32 line = new(141, 17, 49, 255);
                Color32 highlight = new(194, 22, 66, 255);
                Color32 lockedPixel = brightness <= 57
                    ? Color32.Lerp(new Color32(3, 8, 6, 255), shadow, (brightness - 8) / 49f)
                    : brightness <= 78
                        ? Color32.Lerp(shadow, line, (brightness - 57) / 21f)
                        : Color32.Lerp(line, highlight, (brightness - 78) / 65f);
                lockedPixel.a = pixel.a;
                pixels[i] = lockedPixel;
            }
            var texture = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false)
                { name = key, filterMode = FilterMode.Point };
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            return SupportSprites[key] = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f), 100f);
        }

        private static void AddModuleText(string id, string name, string description)
        {
            string key = "ui.magnum.module." + id;
            TranslationPatch.AddShipSupportText(key + ".name", name);
            TranslationPatch.AddShipSupportText(key + ".shortdesc", description);
            TranslationPatch.AddShipSupportText(key + ".treecaption", name);
        }

        private static int ImperialSupportTech(Factions factions) => factions?.Get(ItemCategory)?.CurrentTechLevel ?? 1;

        [HarmonyPatch(typeof(MagnumProgression), nameof(MagnumProgression.IsAvailableToUpgrade))]
        private static class RequireSupportTech
        {
            private static void Postfix(MagnumProgression __instance, Factions ____factions, string id, ref bool __result)
            {
                if (!__result)
                {
                    return;
                }
                if (id is BarrageDuration or BarrageRounds or BarrageCalls)
                {
                    __result = ShipMounts.Any(mount => __instance.IsPerkPurchased(mount.Id));
                }
                else if (id.StartsWith("iom_drop_troops_", StringComparison.Ordinal) &&
                    int.TryParse(id.Substring("iom_drop_troops_".Length), out int tier))
                {
                    __result = ImperialSupportTech(____factions) >= tier && __instance.IsPerkPurchased(TroopTier(tier - 1));
                }
            }
        }

        private static int[] AvailableMounts(MagnumProgression progression) => Enumerable.Range(0, ShipMounts.Length)
            .Where(index => progression.IsPerkPurchased(ShipMounts[index].Id)).ToArray();

        private static int[] AvailableSquads(MagnumProgression progression, Factions factions)
        {
            int tech = ImperialSupportTech(factions);
            return Enumerable.Range(0, DropSquads.Length)
                .Where(index => DropSquads[index].Tier <= tech && progression.IsPerkPurchased(TroopTier(DropSquads[index].Tier)))
                .ToArray();
        }

        private static int SupportSelection(StoryTriggers triggers, string key, int[] available, bool newest = false) =>
            triggers.FloatMap.TryGetValue(key, out float value) && available.Contains((int)value)
                ? (int)value : available.Length == 0 ? -1 : available[newest ? available.Length - 1 : 0];
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using MGSC;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private static readonly Dictionary<string, List<(int Tier, ItemQuantity Supply)>> SupportMarkets = new();
        private static readonly Dictionary<string, List<(string Faction, int Tier, string Recipe)>> SupportWorkshops = new();
        private static readonly HashSet<string> MaterialGatedWorkshops = new();

        private static void RegisterSupportProduction()
        {
            RegisterSupportTier(1, "Field support patterns", PhobosStation,
                [new("automap", 1), new("defibrillator_device", 1), new("robot_resurrect_device", 1)],
                "automap", "quasi_sensor_device", "defibrillator_device", "robot_resurrect_device",
                "small_basic_ammo", "battery_crafted_ammo", "nail_bolts_ammo",
                "frag_mine", "stun_grenade", "handmade_frag_grenade", "throwing_knife");
            RegisterSupportTier(2, "Industrial automata and ballistic defense patterns", HavocStation,
                [new("stationary_turret", 1), new("packaged_industrial_robot", 1), new("robot_parts_container", 1)],
                "stationary_turret", "packaged_industrial_robot", "melee_repair_kit",
                "cyborg_head", "cyborg_wrist", "cyborg_hand", "cyborg_feet", "cyborg_leg", "cyborg_chest",
                "cyborg_guts", "cyborg_knee", "cyborg_drill", "battery_basic_ammo", "sawblade_ammo",
                "medium_police_ammo", "shotgun_bullet_ammo", "handmade_smoke_grenade", "handmade_thermal_grenade");
            RegisterSupportTier(3, "Recreational automata and flame defense patterns", LunaStation,
                [new("packaged_recreational_robot", 1), new("fire_turret", 1)],
                "packaged_recreational_robot", "fire_turret", "engineering_repair_kit",
                "cyborg_recreation_head", "cyborg_recreation_wrist", "cyborg_recreation_hand",
                "cyborg_recreation_feet", "cyborg_recreation_leg", "cyborg_recreation_chest",
                "cyborg_recreation_guts", "cyborg_recreation_knee", "cyborg_recreation_blade",
                "dil_battery_ammo", "small_burst_ammo", "shotgun_shrapnel_ammo", "heavy_basic_ammo",
                "frag_grenade", "highexplosion_grenade", "toxic_grenade");
            RegisterSupportTier(4, "Security automata and energy defense patterns", FoundryStation,
                [new("packaged_security_robot", 1), new("energy_turret", 1), new("placeable_barrel", 1)],
                "packaged_security_robot", "energy_turret", "placeable_barrel", "firearm_repair_kit", "tech_repair_kit",
                "cyborg_battle_head", "cyborg_battle_wrist", "cyborg_battle_hand", "cyborg_battle_feet",
                "cyborg_battle_leg", "cyborg_battle_chest", "cyborg_battle_guts", "cyborg_battle_knee",
                "cyborg_battle_machinegun", "small_expansive_ammo", "rifle_basic_ammo", "thermal_grenade", "smoke_grenade");
            RegisterSupportTier(5, "Drone and launcher support patterns", BroadswordStation,
                [new("packaged_drone", 1), new("rocket_basic_ammo", 2)],
                "packaged_drone", AiModule, "rocket_basic_ammo", "gas_ammo", "medium_cold_ammo", "shotgun_explosion_ammo");
            RegisterSupportTier(6, "Special munitions and field repair patterns", TerraStation,
                [new("armor_repair_kit", 1), new("nanorobot_grenade", 1)],
                "armor_repair_kit", "rifle_plasma_ammo", "heavy_flak_ammo", "sawblade_poison_ammo",
                "frost_gas_grenade", "toxic_gas_grenade", "nanorobot_grenade", "syringe_nanoinjection");
            RegisterSupportTier(7, "Forge incendiary patterns", FoundryStation,
                [new("hellfire_grenade", 1), new("rocket_fire_ammo", 2)],
                "rocket_fire_ammo", "heavy_incendiary_ammo", "shotgun_dragon_ammo", "hellfire_grenade");
            RegisterSupportTier(8, "Shield and penetrator patterns", TerraStation,
                [new("placeable_barrier", 1)],
                "placeable_barrier", "rifle_armorpierce_ammo");
            RegisterSupportTier(9, "Quarantine support patterns", PhobosStation,
                [new("frost_grenade", 1), new("rocket_poison_ammo", 2)],
                "frost_grenade", "rocket_poison_ammo", "toxic_ammo");
            RegisterSupportTier(10, "Vault ordnance patterns", TitanFortress,
                [new("rocket_nuke_ammo", 1)],
                "rocket_nuke_ammo");

            // AnCom keeps the manufacturing patterns for these shared finished goods.
            AddStandardSupplies(1, 2, "medium_basic_ammo");
            AddStandardSupplies(ElysianTier, 2, "shotgun_flechette_ammo");
            AddSupportStock(GoldenTier, TerraStation, new("placeable_barrier", 1), new("packaged_security_robot", 1),
                new("energy_turret", 1), new("packaged_drone", 1), new("tech_repair_kit", 1));

            RegisterAutomataWorkshops();

            AddSupportWorkshop(HavocStation, 2, "iom_havoc_automata", 96,
                [new(AutomataChassis, 2), new(LogicCore, 2), new("engineering_parts_container", 1)],
                [new("stationary_turret", 1), new("packaged_industrial_robot", 1)]);
            AddSupportWorkshop(LunaStation, 3, "iom_luna_automata", 108,
                [new(AutomataChassis, 2), new(LogicCore, 2), new("rubber", 2), new("gas_ammo", 8)],
                [new("fire_turret", 1), new("packaged_recreational_robot", 1)]);
            AddSupportWorkshop(FoundryStation, 4, "iom_foundry_automata", 120,
                [new(AutomataChassis, 2), new(LogicCore, 2), new(HeavyPlate, 2)],
                [new("energy_turret", 1), new("packaged_security_robot", 1)]);
            AddSupportWorkshop(BroadswordStation, 5, "iom_broadsword_drone_works", 120,
                [new(AutomataChassis, 1), new(LogicCore, 1), new(AiModule, 1)],
                [new("packaged_drone", 1)]);
            AddSupportWorkshop(TerraStation, 8, "iom_palace_shield_works", 132,
                [new(LogicCore, 1), new(AiModule, 1), new(HeavyPlate, 1)],
                [new("placeable_barrier", 1)]);
        }

        private static void AddSupportWorkshop(string station, int tier, string receipt, int hours,
            List<ItemQuantity> inputs, List<ItemQuantity> outputs, int powerGain = 0) =>
            AddFactionWorkshop(ItemCategory, station, tier, receipt, hours, inputs, outputs, powerGain);

        private static void AddFactionWorkshop(string faction, string station, int tier, string receipt, int hours,
            List<ItemQuantity> inputs, List<ItemQuantity> outputs, int powerGain = 0, float techGain = 0)
        {
            AddStationReceipt(receipt, hours, inputs, outputs, powerGain, techGain);
            if (!SupportWorkshops.TryGetValue(station, out var workshops))
            {
                workshops = [];
                SupportWorkshops.Add(station, workshops);
            }
            workshops.Add((faction, tier, receipt));
            MaterialGatedWorkshops.Add(receipt);
        }

        private static bool CanUseSupportWorkshop(Faction owner, string manufacturer, int tier) =>
            owner?.Id == manufacturer && owner.FactionType != FactionType.None && owner.CurrentTechLevel >= tier;

        private static string RetiredWorkshop(Station station) => station.Id switch
        {
            LunaStation => LunaReceipt,
            PhobosStation => PhobosReceipt,
            NeithVault => NeithReceipt,
            HavocStation => "device_robot",
            SampoQuern when station.OwnerFactionId == CoreWard => "device_robot",
            AllianceArcology when station.OwnerFactionId == PlanetBridge => "device_robot",
            WinterBay when station.OwnerFactionId == PlanetBridge => "factory_wield",
            _ => null
        };

        private static IEnumerable<string> IncludeSupportWorkshop(List<string> recipes, Factions factions, Station station)
        {
            if (_industryReady && SupportWorkshops.TryGetValue(station.Id, out var workshops))
            {
                var faction = factions.Get(station.OwnerFactionId);
                string retired = RetiredWorkshop(station);
                return recipes.Where(recipe => recipe != retired)
                    .Concat(workshops.Where(workshop => CanUseSupportWorkshop(faction, workshop.Faction, workshop.Tier))
                    .Select(workshop => workshop.Recipe));
            }
            return recipes;
        }

        [HarmonyPatch(typeof(StationSystem), nameof(StationSystem.RefreshConsumableItems))]
        private static class SupportWorkshopDemand
        {
            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var recipeGetters = new HashSet<System.Reflection.MethodInfo>
                {
                    AccessTools.PropertyGetter(typeof(StationBarterRecord), nameof(StationBarterRecord.CorpProduceItems)),
                    AccessTools.PropertyGetter(typeof(StationBarterRecord), nameof(StationBarterRecord.CivResProduceItems)),
                    AccessTools.PropertyGetter(typeof(StationBarterRecord), nameof(StationBarterRecord.QuasiProduceItems)),
                    AccessTools.PropertyGetter(typeof(StationBarterRecord), nameof(StationBarterRecord.PiratesProduceItems))
                };
                var includeWorkshop = AccessTools.Method(typeof(Plugin), nameof(IncludeSupportWorkshop));
                foreach (var instruction in instructions)
                {
                    yield return instruction;
                    if (instruction.operand is System.Reflection.MethodInfo getter && recipeGetters.Contains(getter))
                    {
                        yield return new CodeInstruction(OpCodes.Ldarg_2);
                        yield return new CodeInstruction(OpCodes.Ldarg_3);
                        yield return new CodeInstruction(OpCodes.Call, includeWorkshop);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(StationSystem), nameof(StationSystem.Update))]
        private static class RefreshSupportWorkshopTier
        {
            private static void Prefix(Stations stations, Factions factions)
            {
                if (!_industryReady)
                {
                    return;
                }
                foreach (var entry in SupportWorkshops)
                {
                    var station = stations.Get(entry.Key, logMissing: false);
                    if (station == null)
                    {
                        continue;
                    }
                    var faction = factions.Get(station.OwnerFactionId, logMissing: false);
                    string retired = RetiredWorkshop(station);
                    if ((retired != null && station.CurrentReceipts.Contains(retired)) ||
                        entry.Value.Any(workshop => station.CurrentReceipts.Contains(workshop.Recipe) !=
                            CanUseSupportWorkshop(faction, workshop.Faction, workshop.Tier)))
                    {
                        station.OwnerChangedDirty = true;
                    }
                }
            }
        }

        private static void RegisterSupportTier(int tier, string title, string supplier, ItemQuantity[] stock, params string[] products)
        {
            string id = "iom_support_plans_" + tier;
            var chip = CloneItem<DatadiskRecord>(id, "low_chip", tier, 100 + tier * 38, FieldPlans);
            RegisterCatalog(chip, title, "Magnum fabrication patterns for Imperial support stock at this tech level.",
                ItemCategory, products, restrictOtherSources: false, supplier: supplier);
            AddSupportStock(tier, supplier, stock);
            AddStandardSupplies(tier, 2, products);
            foreach (string product in products)
            {
                EnsureSupportRecipe(product);
            }
        }

        private static void AddSupportStock(int tier, string supplier, params ItemQuantity[] stock)
        {
            if (!SupportMarkets.TryGetValue(supplier, out var entries))
            {
                entries = [];
                SupportMarkets.Add(supplier, entries);
            }
            entries.AddRange(stock.Select(supply => (tier, supply)));
        }

        private static void EnsureSupportRecipe(string product)
        {
            if (IndustryBatches.ContainsKey(product) || Data.ProduceReceipts.Any(receipt => receipt.OutputItem == product))
            {
                return;
            }

            var record = Data.Items.GetSimpleRecord<ItemRecord>(product);
            ItemQuantity[] inputs = product switch
            {
                "defibrillator_device" => [new("capacitor_parts", 1), new("wire", 2), new("plastic", 1)],
                "robot_resurrect_device" => [new("circuit_board", 1), new("microelectronics_parts", 1), new("capacitor_parts", 1)],
                "quasi_sensor_device" => [new("transformer", 1), new("microelectronics_parts", 1), new("lens", 1)],
                "battery_crafted_ammo" => [new("glass", 1), new("acid", 1), new("wire", 1)],
                "packaged_industrial_robot" => RobotAssembly("cyborg"),
                "packaged_recreational_robot" => RobotAssembly("cyborg_recreation"),
                "packaged_security_robot" => RobotAssembly("cyborg_battle"),
                "packaged_drone" => [new(AutomataChassis, 1), new(LogicCore, 1), new(AiModule, 1),
                    new("cyborg_hand", 1), new("cyborg_knee", 1), new("lens", 1)],
                "hellfire_grenade" => [new(Fittings, 1), new("gas_ammo", 2), new("powder", 1)],
                "nanorobot_grenade" => [new("tin", 1), new("microelectronics_parts", 1), new("circuit_board", 1)],
                "syringe_nanoinjection" => [new("plastic", 1), new("microelectronics_parts", 1), new("medical_glue", 1)],
                _ when product.StartsWith("cyborg", StringComparison.Ordinal) =>
                    [new(Fittings, 1), new(Metal, 1), new("wire", 1)],
                _ when record.CanDisassembly => [.. record.Disassembly],
                _ => throw new InvalidOperationException("No Imperial fabrication inputs for " + product)
            };
            float time = record.ItemClass switch
            {
                ItemClass.Ammo => 3,
                ItemClass.Cyborg => 36,
                ItemClass.Grenade or ItemClass.Mine => 12,
                _ => 24
            };
            IndustryRecipe(product, time, record.ItemClass == ItemClass.Ammo ? 4 : 1, inputs);
        }

        private static ItemQuantity[] RobotAssembly(string chassis) =>
        [
            new(chassis + "_head", 1), new(AutomataChassis, 1), new(LogicCore, 1),
            new(chassis + "_knee", 1), new(chassis + "_hand", 1), new(chassis + "_leg", 1)
        ];
    }
}

using System;
using System.Collections.Generic;
using MGSC;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private static void RegisterChaosProduction()
        {
            foreach (ChaosSupplyTier tier in _chaosSupply)
            {
                int level = tier.Level;
                string salvage = tier.Salvage.Id;
                var outputs = new List<string>();
                // Shadowblade is reserved for the finale. Its chip teaches the armor and dressing.
                if (level < 10)
                {
                    string stock = level <= 3 ? Metal : level <= 6 ? Sheet : HeavyPlate;
                    IndustryRecipe(tier.Weapon.Id, 6 + 3 * level, 1,
                        new(salvage, 1), new(stock, 1 + level / 3),
                        new(level is 1 or 3 ? "rod_parts" : "weapon_parts", 1 + level / 4));
                    outputs.Add(tier.Weapon.Id);
                }

                string insert = level == 1 ? Textile : level == 2 ? Flak : level <= 6 ? Plate : HeavyPlate;
                for (int slot = 0; slot < tier.Armor.Length; slot++)
                {
                    string piece = tier.Armor[slot];
                    int plateCount = slot == 1 ? 2 : 1;
                    IndustryRecipe(piece, 4 + 2 * level + (slot == 1 ? 5 : 0), 1,
                        new(salvage, 1), new(insert, plateCount),
                        new(slot == 3 ? "rubber" : Liner, level >= 7 && slot == 1 ? 2 : 1));
                    outputs.Add(piece);
                }

                RegisterChaosUtilityRecipe(tier);
                outputs.Add(tier.Utility.Id);
                RegisterChaosMaterialRecipe(tier);
                outputs.Add(tier.Salvage.Id);
                if (level == 2)
                {
                    IndustryRecipe(BoundAsh, 5, 1, new("demonic_eye", 1), new(tier.Salvage.Id, 1));
                    outputs.Add(BoundAsh);
                }
                if (level == 6)
                {
                    IndustryRecipe(ForgeEmber, 12, 1, new(BoundAsh, 2), new(tier.Salvage.Id, 1), new("circuit_board", 1));
                    outputs.Add(ForgeEmber);
                }
                foreach (var path in ChaosPacts)
                {
                    foreach (var pact in path)
                    {
                        if (pact.Key == FurySigilKey || pact.Tier * 2 - 1 != level) continue;
                        string skull = ChaosSkullId(pact.Key);
                        // Tier-one skulls use loose essence so their patterns work before the ash schematic is available.
                        IndustryRecipe(skull, 6 + level * 2, 1, new("demonic_eye", pact.Tier),
                            new(level >= 7 ? ForgeEmber : level >= 3 ? BoundAsh : Textile, 1), new(tier.Salvage.Id, 1));
                        outputs.Add(skull);
                    }
                }
                int price = 100 + 55 * level;
                var chip = ChaosItem<DatadiskRecord>(tier.Chip, "low_chip", level, price);
                ApplyChaosItemArtwork(chip);
                string name = level switch
                {
                    1 => "Hollow Signal Pattern",
                    2 => "Ritual Flak Schematic",
                    3 => "Ossuary Ribplate Pattern",
                    4 => "Possessed Binding Schematic",
                    5 => "Veiled Gate Assault Pattern",
                    6 => "Excruciator Forge Schematic",
                    7 => "Fleshmetal Assembly Pattern",
                    8 => "Chainforge Siege Schematic",
                    9 => "Hellforged Panoply Pattern",
                    _ => "Black Meridian Anchor Schematic"
                };
                string description = level == 10
                    ? "Fabrication patterns for Shadowbound armor, the Umbral Suture, and Shadow Anchor Fragments."
                    : "Fabrication patterns for the Undivided's weapons, armor, field supplies, and forge materials.";
                string supplier = Array.Find(ChaosSites, site => site.FirstTier <= level && level <= site.LastTier).Id;
                RegisterCatalog(chip, name, description, ChaosFaction, outputs.ToArray(), supplier: supplier);
            }
        }

        private static void RegisterChaosMaterialRecipe(ChaosSupplyTier tier)
        {
            ItemQuantity[] inputs = tier.Level switch
            {
                1 => [new(Textile, 2), new("rags", 1)],
                2 => [new(GlassStock, 2), new("mercury_glass", 1)],
                3 => [new("demonic_eye", 1), new("rod_parts", 1)],
                4 => [new("demonic_eye", 1), new(Fittings, 2)],
                5 => [new("transformer", 1), new(ServoCartridge, 1)],
                6 => [new("wire", 2), new(PolymerStock, 1)],
                7 => [new(HeavyPlate, 1), new("armor_plates", 1)],
                8 => [new(Fittings, 2), new("weapon_parts", 2)],
                9 => [new("demonic_eye", 2), new(Fittings, 1)],
                10 => [new(ImplantController, 1), new("demonic_eye", 2), new("capacitor_parts", 2)],
                _ => throw new ArgumentOutOfRangeException(nameof(tier))
            };
            IndustryRecipe(tier.Salvage.Id, 3 + tier.Level, 2, inputs);
        }

        private static void RegisterChaosUtilityRecipe(ChaosSupplyTier tier)
        {
            string id = tier.Utility.Id;
            string salvage = tier.Salvage.Id;
            switch (tier.Level)
            {
                case 1: IndustryRecipe(id, 3, 3, new(RationBase, 2), new(Wrapper, 2)); break;
                case 2: IndustryRecipe(id, 3, 8, new(salvage, 1), new("quasi_basic_ammo", 6)); break;
                case 3: IndustryRecipe(id, 4, 2, new(salvage, 1), new(Textile, 2)); break;
                case 4: IndustryRecipe(id, 6, 1, new(Fittings, 2), new(Liner, 1), new("armor_plates", 1)); break;
                case 5: IndustryRecipe(id, 6, 2, new(PolymerStock, 1), new("smoke_grenade", 1)); break;
                case 6: IndustryRecipe(id, 5, 8, new(salvage, 1), new("battery_basic_ammo", 6)); break;
                case 7: IndustryRecipe(id, 8, 1, new(salvage, 1), new(Fittings, 2), new("tech_repair_kit", 1)); break;
                case 8: IndustryRecipe(id, 6, 2, new(Textile, 1), new(Treatment, 1), new("goldenstar_balm", 1)); break;
                case 9: IndustryRecipe(id, 9, 2, new("pills_painkillers", 1), new("water_bottle_1", 1), new(Treatment, 1)); break;
                case 10: IndustryRecipe(id, 9, 1, new("surgical_kit", 1), new("pills_cryopentol", 1), new(Clinical, 1)); break;
            }
        }

        private static void RegisterChaosRewards()
        {
            foreach (ChaosSupplyTier tier in _chaosSupply)
            {
                if (tier.Level < 10)
                {
                    AddChaosReward("rewardEquipment", tier.Level, 3, tier.Weapon);
                }
                foreach (string piece in tier.Armor)
                {
                    Registrations.Add(() =>
                    {
                        var armor = Data.Items.GetSimpleRecord<ItemRecord>(piece);
                        Data.FactionDrop.AddRecord(ChaosFaction + "_rewardEquipment", new ContentDropRecord
                        {
                            ContentIds = [piece], TechLevel = tier.Level, Weight = 2, Points = armor.Price
                        });
                    });
                }
                AddChaosReward("rewardConsumables", tier.Level, 4, tier.Utility);
                AddChaosReward("rewardConsumables", tier.Level, 3, tier.Salvage);
            }

            AddNativeSupply(1, "water_bottle_1");
            AddNativeSupply(1, "rags");
            AddNativeSupply(2, "quasi_basic_ammo");
            AddNativeSupply(5, "battery_basic_ammo");
            AddNativeSupply(7, "rifle_basic_ammo");
            AddNativeSupply(7, "rifle_armorpierce_ammo");

            void AddNativeSupply(int level, string id) =>
                AddChaosReward("rewardConsumables", level, 3, Data.Items.GetSimpleRecord<ItemRecord>(id));
        }

        private static void AddChaosReward(string table, int level, float weight, ItemRecord item) =>
            AddFactionReward(ChaosFaction, table, level, weight, item.Price, item.Id);
    }
}

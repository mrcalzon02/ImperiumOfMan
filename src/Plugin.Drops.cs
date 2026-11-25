using MGSC;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private static void RegisterFactionDrops()
        {
            AddRewards("rewardEquipment", 1, 4, 115, _lasgun.Id);
            AddRewards("rewardEquipment", 1, 2, 65, _lightBoots.Id, _lightLeggings.Id, _lightChestplate.Id, _lightHelmet.Id);
            AddRewards("rewardEquipment", 2, 3, 105, _cadiaBoots.Id, _cadiaLeggings.Id, _cadiaChestplate.Id, _cadiaHelmet.Id);
            AddRewards("rewardEquipment", 3, 4, 275, _servitorBackpack.Id, _longLas.Id, _plasmaPistol.Id);
            AddRewards("rewardEquipment", 3, 3, 225, Judgement, ShockMaul, ArbitesHelmet, ArbitesCuirass, ArbitesGreaves, ArbitesBoots);
            AddRewards("rewardEquipment", 3, 1, 325, SubductorCuirass);
            AddRewards("rewardEquipment", ValhallanTier, 3, 165, WinterCap, WinterCoat, WinterTrousers, WinterBoots, WinterLasgun);
            AddRewards("rewardEquipment", SteelLegionTier, 3, 170, LegionMask, LegionCoat, LegionTrousers, LegionBoots, LegionCarbine);
            AddRewards("rewardEquipment", ElysianTier, 3, 175, DropHelmet, DropVest, DropTrousers, DropBoots, DropCarbine, DropHarness);
            AddRewards("rewardEquipment", 4, 5, 320, _bolter.Id);
            AddRewards("rewardEquipment", KriegTier, 3, 235, SiegeMask, SiegeCoat, TrenchTrousers, TrenchBoots,
                SiegeLasgun, EngineerShotgun, EntrenchingTool);
            AddRewards("rewardEquipment", 5, 3, 400, _melta.Id);
            AddRewards("rewardEquipment", ScionTier, 3, 330, ScionHelmet, ScionCuirass, ScionGreaves, ScionBoots, HotShotLasgun);
            AddRewards("rewardEquipment", ScionTier, 1, 475, ScionTargeter);
            AddRewards("rewardEquipment", SkitariiTier, 3, 315, SkitariiHood, SkitariiCoat, SkitariiLeggings, SkitariiBoots);
            AddRewards("rewardEquipment", SkitariiTier, 2, 430, GalvanicRifle, ArcRifle);
            AddRewards("rewardEquipment", SororitasTier, 3, 545, SororitasHelmet, SororitasCuirass, SororitasGreaves, SororitasBoots);
            AddRewards("rewardEquipment", SororitasTier, 2, 615, SororitasBolter, SororitasFlamer);
            AddRewards("rewardEquipment", OrdoXenosTier, 3, 465, XenosHelmet, XenosCoat, XenosGreaves, XenosBoots);
            AddRewards("rewardEquipment", OrdoXenosTier, 2, 690, PowerBlade, NeedlePistol);
            AddRewards("rewardEquipment", TitanContractTier, 3, 735, PaladinHelm, PaladinCuirass, PaladinGreaves, PaladinSabatons);
            AddRewards("rewardEquipment", TitanContractTier, 2, 1125, NemesisSword, Psycannon);
            AddRewards("rewardEquipment", GoldenTier, 3, 915, GoldenHelm, GoldenCuirass, GoldenGreaves, GoldenSabatons);
            AddRewards("rewardEquipment", GoldenTier, 2, 1400, GoldenSword, GoldenCannon);

            AddRewards("rewardConsumables", 1, 5, 30, CorpseStarch, LasClip);
            AddRewards("rewardConsumables", 2, 3, 62, GuardFrag);
            AddRewards("rewardConsumables", ValhallanTier, 2, 30, Tanna);
            AddRewards("rewardConsumables", SteelLegionTier, 2, 46, LegionSmoke, LegionFilter);
            AddRewards("rewardConsumables", KriegTier, 1, 145, DemolitionCharge);
            AddRewards("rewardConsumables", ScionTier, 2, 55, HotShotPack);
            AddRewards("rewardConsumables", SkitariiTier, 2, 125, SkitariiAuspex, SkitariiRepairKit);
            AddRewards("rewardConsumables", SkitariiTier, 2, 55, CircuitAssembly);
            AddRewards("rewardConsumables", SororitasTier, 2, 125, DevotionalWard);
            AddRewards("rewardConsumables", TitanContractTier, 2, 230, AegisWard);
            AddRewards("rewardConsumables", GoldenTier, 2, 300, GoldenWard);

            AddStandardSupplies(1, 4, "water_bottle_1", "ration_pack_1", "bandage", "splint");
            AddStandardSupplies(2, 3, "hydrogel_bandage");
            AddStandardSupplies(ArbitesTier, 3, "medical_kit_1");
            AddStandardSupplies(KriegTier, 3, "medical_kit_2");

            AddRewards("rewardChips", 1, 4, 135, "low_chip");
            AddRewards("rewardChips", 2, 4, 250, "mercenary_chip");
        }

        private static void AddStandardSupplies(int level, float weight, params string[] ids)
        {
            foreach (string id in ids)
            {
                var item = Data.Items.GetSimpleRecord<ItemRecord>(id);
                AddFactionReward(ItemCategory, "rewardConsumables", level, weight, item.Price, id);
            }
        }

        private static void AddRewards(string table, int level, float weight, float points, params string[] ids)
        {
            foreach (string id in ids)
            {
                AddFactionReward(ItemCategory, table, level, weight, points, id);
            }
        }

        private static void AddFactionReward(string faction, string table, int level, float weight, float points, string id)
        {
            var drop = new ContentDropRecord
            {
                ContentIds = [id],
                TechLevel = level,
                Weight = weight,
                Points = points
            };
            Registrations.Add(() => Data.FactionDrop.AddRecord(faction + "_" + table, drop));
        }
    }
}

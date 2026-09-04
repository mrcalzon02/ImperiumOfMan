using System.Collections.Generic;
using MGSC;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private static void RegisterFactionDrops()
        {
            ItemRecord sawblade = Data.Items.GetSimpleRecord<ItemRecord>("trash_sawblade_1");
            if (sawblade != null && !sawblade.Categories.Contains(ItemCategory))
            {
                sawblade.Categories.Add(ItemCategory);
            }

            var equipmentDrops = CreateDropTable();
            equipmentDrops[1] =
            [
                _lightBoots.ToDropRecord(3f, 200),
                _lightLeggings.ToDropRecord(3f, 200),
                _lightChestplate.ToDropRecord(3f, 200),
                _lightHelmet.ToDropRecord(3f, 200)
            ];
            equipmentDrops[2] =
            [
                _lasgun.ToDropRecord(10f, 200),
                _cadiaBoots.ToDropRecord(3f, 200),
                _cadiaLeggings.ToDropRecord(3f, 200),
                _cadiaChestplate.ToDropRecord(3f, 200),
                _cadiaHelmet.ToDropRecord(3f, 200)
            ];
            equipmentDrops[3] =
            [
                _servitorBackpack.ToDropRecord(10f, 250),
                _longLas.ToDropRecord(5f, 400),
                _plasmaPistol.ToDropRecord(3f, 250)
            ];
            equipmentDrops[4] = [_bolter.ToDropRecord(10f, 300)];
            equipmentDrops[5] = [_melta.ToDropRecord(7f, 310)];
            Data.FactionDrop._recordsByFactions.Add("iom_faction_rewardEquipment", equipmentDrops);

            var consumableDrops = CreateDropTable();
            consumableDrops[1] = [CreateContentDrop(1, "low_chip", 1f, 100f)];
            consumableDrops[2] = [CreateContentDrop(2, "mercenary_chip", 1f, 100f)];
            consumableDrops[3] = [CreateContentDrop(3, "low_chip", 1f, 100f)];
            Data.FactionDrop._recordsByFactions.Add("iom_faction_rewardConsumables", consumableDrops);

            var ammunitionDrops = CreateDropTable();
            ammunitionDrops[1] = [CreateContentDrop(1, "rifle_basic_ammo", 4f, 200f)];
            ammunitionDrops[2] =
            [
                CreateContentDrop(2, "rifle_armorpierce_ammo", 5f, 150f),
                CreateContentDrop(2, "battery_basic_ammo", 3f, 100f)
            ];
            Data.FactionDrop._recordsByFactions.Add("iom_faction_rewardChips", ammunitionDrops);
        }

        private static Dictionary<int, List<ContentDropRecord>> CreateDropTable()
        {
            var table = new Dictionary<int, List<ContentDropRecord>>(11);
            for (int techLevel = 0; techLevel <= 10; techLevel++)
            {
                table.Add(techLevel, []);
            }

            return table;
        }

        private static ContentDropRecord CreateContentDrop(int techLevel, string contentId, float weight, float points)
        {
            return new ContentDropRecord
            {
                TechLevel = techLevel,
                ContentIds = [contentId],
                Weight = weight,
                Points = points
            };
        }
    }
}

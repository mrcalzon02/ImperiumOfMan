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
            AddRewards("rewardEquipment", 4, 5, 320, _bolter.Id);
            AddRewards("rewardEquipment", 5, 3, 400, _melta.Id);

            AddRewards("rewardConsumables", 1, 5, 30, CorpseStarch, LasClip);
            AddRewards("rewardConsumables", 2, 3, 62, GuardFrag);

            AddRewards("rewardChips", 1, 4, 135, FieldPlans, "low_chip");
            AddRewards("rewardChips", 2, 4, 250, "mercenary_chip");
        }

        private static void AddRewards(string table, int level, float weight, float points, params string[] ids)
        {
            foreach (string id in ids)
            {
                var drop = new ContentDropRecord
                {
                    ContentIds = [id],
                    TechLevel = level,
                    Weight = weight,
                    Points = points
                };
                Registrations.Add(() => Data.FactionDrop.AddRecord("iom_faction_" + table, drop));
            }
        }
    }
}

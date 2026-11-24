using MGSC;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private static void RegisterFactionDrops()
        {
            AddRewards("rewardEquipment", 1, 3, 200,
                _lasgun.Id, _lightBoots.Id, _lightLeggings.Id, _lightChestplate.Id, _lightHelmet.Id);
            AddRewards("rewardEquipment", 2, 5, 200,
                _cadiaBoots.Id, _cadiaLeggings.Id, _cadiaChestplate.Id, _cadiaHelmet.Id, "iom_bolt_pistol");
            AddRewards("rewardEquipment", 3, 4, 275,
                _servitorBackpack.Id, _longLas.Id, _plasmaPistol.Id, "iom_judgement");
            AddRewards("rewardEquipment", 4, 5, 320, _bolter.Id, "iom_power_fist");
            AddRewards("rewardEquipment", 5, 3, 400, _melta.Id, "iom_bolt_rifle", "iom_power_blade");

            AddRewards("rewardConsumables", 1, 5, 60, CorpseStarch, DirtyStarch, LasClip, Requisition);
            AddRewards("rewardConsumables", 2, 4, 100, BoltClip, "iom_frag_grenade");
            AddRewards("rewardConsumables", 3, 3, 140, "iom_kraken_clip", "iom_krak_grenade", PuritySeal);
            AddRewards("rewardConsumables", 4, 2, 160, "iom_inferno_clip", "iom_hellfire_clip", "iom_metal_storm_clip");
            AddRewards("rewardConsumables", 5, 2, 200,
                "iom_tempest_clip", "iom_helfrost_clip", "iom_antiphasic_clip", "iom_melta_grenade");

            AddRewards("rewardChips", 1, 4, 180, FieldPlans);
            AddRewards("rewardChips", 2, 4, 250, "mercenary_chip");
            AddRewards("rewardChips", 3, 3, 275, ArsenalPlans);
            AddRewards("rewardChips", 5, 3, 425, ForgePlans);
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

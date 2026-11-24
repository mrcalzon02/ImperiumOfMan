using System.Collections.Generic;
using MGSC;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private const string FieldPlans = "iom_field_plans";
        private const string ArsenalPlans = "iom_arsenal_plans";
        private const string ForgePlans = "iom_forge_plans";
        private const string TerraReceipt = "iom_terra_requisitions";
        private const string LunaReceipt = "iom_luna_provisions";
        private const string PhobosReceipt = "iom_phobos_munitions";
        private const string HavocReceipt = "iom_havoc_forge";

        private static void RegisterProduction()
        {
            AddProduction(CorpseStarch, 5, new("human_meat", 1), new("plastic", 1));
            AddProduction(LasClip, 6, new("glass", 1), new("acid", 1));
            AddProduction(BoltClip, 8, new("powder", 2), new("tin", 1));
            AddProduction("iom_frag_grenade", 14, new("tin", 1), new("explosives", 1));
            AddProduction(_lasgun.Id, 24, new("lens", 1), new("microelectronics_parts", 1), new("weapon_parts", 2));
            AddBlueprint(FieldPlans, "low_chip", "Imperial Field Plans", 1,
                CorpseStarch, LasClip, BoltClip, "iom_frag_grenade", _lasgun.Id);

            AddProduction("iom_kraken_clip", 12, new(BoltClip, 1), new("pierce_parts", 1));
            AddProduction("iom_inferno_clip", 12, new(BoltClip, 1), new("gas_ammo", 2));
            AddProduction("iom_hellfire_clip", 12, new(BoltClip, 1), new("acid", 1));
            AddProduction("iom_metal_storm_clip", 12, new(BoltClip, 1), new("pierce_parts", 1), new("powder", 1));
            AddProduction("iom_krak_grenade", 20, new("tin", 1), new("explosives", 2));
            AddProduction("iom_bolt_pistol", 30, new("weapon_parts", 2), new("spring", 1), new(Requisition, 1));
            AddProduction("iom_judgement", 32, new("weapon_parts", 2), new("armor_plates", 1), new("spring", 1));
            AddProduction(_bolter.Id, 38, new("weapon_parts", 3), new("armor_plates", 2), new(Requisition, 2));
            AddProduction(_plasmaPistol.Id, 34, new("capacitor_parts", 2), new("microelectronics_parts", 2), new(PuritySeal, 1));
            AddBlueprint(ArsenalPlans, "medium_chip", "Imperial Arsenal Plans", 3,
                "iom_kraken_clip", "iom_inferno_clip", "iom_hellfire_clip", "iom_metal_storm_clip",
                "iom_krak_grenade", "iom_bolt_pistol", "iom_judgement", _bolter.Id, _plasmaPistol.Id);

            AddProduction("iom_tempest_clip", 16, new(BoltClip, 1), new("lens", 1));
            AddProduction("iom_helfrost_clip", 16, new(BoltClip, 1), new("water_bottle_1", 1), new("capacitor_parts", 1));
            AddProduction("iom_antiphasic_clip", 16, new(BoltClip, 1), new("transformer", 1));
            AddProduction("iom_melta_grenade", 28, new("tin", 1), new("gas_ammo", 6), new("explosives", 2));
            AddProduction("iom_bolt_rifle", 44, new("weapon_parts", 4), new("armor_plates", 2), new(PuritySeal, 1));
            AddProduction("iom_power_fist", 44, new("rod_parts", 3), new("armor_plates", 2), new(PuritySeal, 1));
            AddProduction("iom_power_blade", 48, new("lens", 2), new("ceramite_plates", 1), new(PuritySeal, 1));
            AddProduction(_melta.Id, 48, new("capacitor_parts", 3), new("microelectronics_parts", 2), new(PuritySeal, 1));
            AddBlueprint(ForgePlans, "high_chip", "Imperial Forge Plans", 5,
                "iom_tempest_clip", "iom_helfrost_clip", "iom_antiphasic_clip", "iom_melta_grenade",
                "iom_bolt_rifle", "iom_power_fist", "iom_power_blade", _melta.Id);

            AddBarter(TerraReceipt, 72, [new(Requisition, 2)], [new(FieldPlans, 1), new(PuritySeal, 1)]);
            AddBarter(LunaReceipt, 36, [new(Requisition, 1), new("water_container", 1)],
                [new(CorpseStarch, 6), new(LasClip, 12)]);
            AddBarter(PhobosReceipt, 48, [new(Requisition, 1), new("powder", 4)],
                [new(BoltClip, 14), new("iom_frag_grenade", 2)]);
            AddBarter(HavocReceipt, 96, [new(Requisition, 3), new("microelectronics_parts", 2)],
                [new(ArsenalPlans, 1), new("iom_kraken_clip", 8)]);
        }

        private static void AddBlueprint(string id, string donorId, string name, int tier, params string[] unlocks)
        {
            var record = CloneItem<DatadiskRecord>(id, donorId, null, tier, 300 + 75 * tier);
            record.UnlockIds = [.. unlocks];
            RegisterExpansionItem("datadisks", record, name, "Unlocks an Imperial production recipe.");
        }

        private static void AddProduction(string id, float hours, params ItemQuantity[] materials)
        {
            var receipt = new ItemProduceReceipt
            {
                Id = id,
                OutputItem = id,
                RequiredItems = [.. materials],
                ProduceTimeInHours = hours,
                ModifyStartCost = 1,
                ModifyStep = 1.05f,
                ModifyItemsGrades = []
            };
            Registrations.Add(() => Data.ProduceReceipts.Add(receipt));
        }

        private static void AddBarter(string id, float hours, List<ItemQuantity> input, List<ItemQuantity> output)
        {
            StageRecord(Data.BarterReceipts, id, new BarterReceipt
            {
                Id = id,
                InputItems = input,
                OutputItems = output,
                ProduceTimeInHours = hours
            });
        }
    }
}

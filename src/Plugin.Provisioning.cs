using MGSC;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private static void RegisterProvisioningWorkshops()
        {
            AddSupportWorkshop(HavocStation, 1, "iom_havoc_fiber_recovery", 48,
                [new("rags", 3)], [new(Fiber, 6)]);
            AddSupportWorkshop(HavocStation, 1, "iom_havoc_textile_mills", 60,
                [new(Fiber, 3)], [new(Textile, 8)]);
            AddSupportWorkshop(FoundryStation, 1, "iom_foundry_polymer_recovery", 60,
                [new("plastic", 3)], [new(PolymerStock, 8)]);

            RegisterRationWorkshops();
            RegisterMedicalWorkshops();
            RegisterMunitionsWorkshops();
        }

        private static void RegisterRationWorkshops()
        {
            AddSupportWorkshop(HavocStation, 1, "iom_havoc_water_reclamation", 120,
                [], [new("water_container", 8)]);
            AddSupportWorkshop(NeithVault, 1, "iom_neith_biomass_vats", 72,
                [new("water_container", 2)], [new(Biomass, 12)]);
            AddSupportWorkshop(LunaStation, 1, "iom_luna_ration_mills", 60,
                [new(Biomass, 3)], [new(RationBase, 8)]);
            AddSupportWorkshop(LunaStation, 1, "iom_luna_wrapper_press", 60,
                [new(Textile, 1), new(PolymerStock, 1)], [new(Wrapper, 8)]);
            AddSupportWorkshop(LunaStation, 1, "iom_luna_ration_packing", 72,
                [new(RationBase, 2), new(Wrapper, 2)], [new(CorpseStarch, 8)]);
            AddStationIssue(PhobosStation, 1, "iom_phobos_ration_issue", CorpseStarch, 2);
        }

        private static void RegisterMedicalWorkshops()
        {
            AddSupportWorkshop(NeithVault, 1, "iom_neith_treatment_packs", 144,
                [new(Biomass, 2), new("pills_sorbent", 1)], [new(Treatment, 8)]);
            AddSupportWorkshop(TerraStation, 1, "iom_palace_sterile_dressings", 72,
                [new(Textile, 1), new("medical_glue", 1)], [new(Dressing, 6)]);
            AddSupportWorkshop(TerraStation, 1, "iom_palace_dressing_trays", 72,
                [new(Dressing, 1), new(PolymerStock, 1)], [new(Tray, 6)]);
            AddSupportWorkshop(TerraStation, 3, "iom_palace_field_medkits", 96,
                [new(Tray, 1), new(Treatment, 1), new("splint", 1)], [new("medical_kit_1", 6)]);
            AddStationIssue(BroadswordStation, 3, "iom_broadsword_field_aid", "medical_kit_1", 1);

            AddSupportWorkshop(NeithVault, 5, "iom_neith_clinical_refills", 168,
                [new(Treatment, 1), new("pills_osteodron", 1), new("pills_tranexam", 1)], [new(Clinical, 6)]);
            AddSupportWorkshop(BroadswordStation, 5, "iom_broadsword_advanced_medkits", 120,
                [new("medical_kit_1", 1), new(Clinical, 1)], [new("medical_kit_2", 6)]);
            AddStationIssue(TitanFortress, 5, "iom_titan_clinical_aid", "medical_kit_2", 1);

            AddSupportWorkshop(TerraStation, 7, "iom_palace_instrument_trays", 144,
                [new(Metal, 1), new(Dressing, 1), new("medical_glue", 1)], [new(Instruments, 6)]);
            AddSupportWorkshop(NeithVault, 7, "iom_neith_surgical_kits", 144,
                [new("medical_kit_1", 1), new(Instruments, 1), new("syringe_naloxone", 1)], [new("surgical_kit", 6)]);
            AddStationIssue(TitanFortress, 7, "iom_titan_surgical_aid", "surgical_kit", 1);

            AddSupportWorkshop(TerraStation, 6, "iom_palace_nanoinjections", 144,
                [new(Clinical, 1), new("microelectronics_parts", 1), new(PolymerStock, 1)], [new("syringe_nanoinjection", 6)]);
            AddStationIssue(BroadswordStation, 6, "iom_broadsword_nanoinjection_issue", "syringe_nanoinjection", 1);
        }

        private static void RegisterMunitionsWorkshops()
        {
            AddSupportWorkshop(PhobosStation, 1, "iom_phobos_silicate_sorting", 72,
                [new("ore_cargo", 1)], [new(Silicate, 9)]);
            AddSupportWorkshop(HavocStation, 1, "iom_havoc_clip_housings", 72,
                [new(PolymerStock, 1)], [new(Housing, 8)]);
            AddSupportWorkshop(PhobosStation, 1, "iom_phobos_las_clip_packing", 108,
                [new(Cell, 1), new(Housing, 1)], [new(LasClip, 16)]);
            AddStationIssue(TerraStation, 1, "iom_palace_las_clip_issue", LasClip, 4);

            AddSupportWorkshop(PhobosStation, 2, "iom_phobos_guard_frag_packing", 120,
                [new(Fittings, 1), new("explosives", 1)], [new(GuardFrag, 6)]);
            AddStationIssue(BroadswordStation, 2, "iom_broadsword_frag_issue", GuardFrag, 1);

            AddSupportWorkshop(HavocStation, 4, "iom_havoc_rifle_munitions", 72,
                [new(Metal, 1), new("powder", 1)], [new("rifle_basic_ammo", 24)]);
            AddSupportWorkshop(BroadswordStation, 5, "iom_broadsword_gas_munitions", 120,
                [new("drug_cargo", 1), new("tin", 1)], [new("gas_ammo", 24)]);
            AddSupportWorkshop(PhobosStation, 9, "iom_phobos_quarantine_munitions", 168,
                [new("drug_cargo", 1), new(GlassStock, 1)], [new("toxic_ammo", 20)]);
            AddStationIssue(BroadswordStation, 4, "iom_broadsword_bolt_issue", BoltClip, 4);
            AddStationIssue(BroadswordStation, 5, "iom_broadsword_kraken_issue", KrakenClip, 2);
            AddStationIssue(FoundryStation, 5, "iom_foundry_krak_issue", KrakGrenade, 1);
            AddStationIssue(BroadswordStation, 6, "iom_broadsword_metal_storm_issue", MetalStormClip, 2);
            AddSupportWorkshop(LunaStation, 6, "iom_luna_glassworks", 120,
                [new(Silicate, 3)], [new(GlassStock, 4)]);
            AddStationIssue(PhobosStation, 6, "iom_phobos_helfrost_issue", HelfrostClip, 2);
            AddStationIssue(BroadswordStation, 7, "iom_broadsword_antiphasic_issue", AntiphasicClip, 2);
            AddStationIssue(FoundryStation, 8, "iom_foundry_inferno_issue", InfernoClip, 2);
            AddStationIssue(TitanFortress, 8, "iom_titan_melta_issue", MeltaGrenade, 1);
            AddStationIssue(TitanFortress, 9, "iom_titan_hellfire_issue", HellfireClip, 2);
            AddStationIssue(TerraStation, 11, "iom_palace_tempest_issue", TempestClip, 2);
        }

        private static void AddStationIssue(string station, int tier, string receipt, string product, int count) =>
            AddSupportWorkshop(station, tier, receipt, 180, [new(product, count)], [], powerGain: 1);
    }
}

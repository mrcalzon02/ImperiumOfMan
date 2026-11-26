using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HarmonyLib;
using MGSC;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private const string Ore = "iom_metal_ore";
        private const string MetalScrap = "iom_sorted_metal_scrap";
        private const string Fiber = "iom_reclaimed_fiber";
        private const string Polymer = "iom_polymer_scrap";
        private const string Cullet = "iom_glass_cullet";
        private const string Conductors = "iom_conductive_scrap";
        private const string Metal = "iom_refined_metal";
        private const string Textile = "iom_processed_textile";
        private const string Sheet = "iom_rolled_plate";
        private const string Fittings = "iom_armor_fittings";
        private const string Flak = "iom_flak_laminate";
        private const string Liner = "iom_fitted_liner";
        private const string Plate = "iom_pressed_insert";
        private const string HeavyPlate = "iom_reinforced_insert";
        private const string PolymerStock = "iom_polymer_stock";
        private const string Silicate = "iom_raw_silicate";
        private const string GlassStock = "iom_industrial_glass";
        private const string CellStock = "iom_cell_stock";
        private const string Cell = "iom_sealed_cell";
        private const string Housing = "iom_las_clip_housing";
        private const string Case = "iom_medium_casing";
        private const string Module = "iom_medium_payload";
        private const string Shell = "iom_shell_casing";
        private const string Flechette = "iom_flechette_module";
        private const string Leaf = "iom_harvested_tobacco";
        private const string Tobacco = "iom_prepared_tobacco";
        private const string FilterStock = "iom_filter_stock";
        private const string Filter = "iom_cigarette_filter";
        private const string Carton = "iom_cigarette_box";
        private const string Loose = "iom_loose_cigarettes";
        private const string Dressing = "iom_sterile_dressing_roll";
        private const string Treatment = "iom_sealed_treatment_pack";
        private const string Tray = "iom_dressing_tray";
        private const string Clinical = "iom_clinical_refill";
        private const string Instruments = "iom_sterile_instrument_tray";
        private const string Biomass = "iom_raw_vat_biomass";
        private const string RationBase = "iom_vat_ration_base";
        private const string Wrapper = "iom_ration_wrapper";
        private const string FieldPlans = "iom_field_plans";

        private static readonly Dictionary<string, string[]> IndustryCatalogs = new();
        private static readonly Dictionary<string, List<(string Chip, int Tier, string Supplier)>> CatalogStock = new();
        private static readonly HashSet<string> LicensedProducts = new();
        private static readonly Dictionary<string, int> IndustryBatches = new();
        private static readonly HashSet<string> SharedIndustryRecipes = new();
        private static bool _industryReady;

        private static void RegisterIndustry()
        {
            AddMaterial(Ore, "rusty_plates", "Metal smelting feedstock", "Mixed raw ore for metal, conductors, silicates, reactive minerals, and carbon feedstock. The Magnum can collect it using production time alone.", 4);
            AddMaterial(MetalScrap, "rusty_plates", "Sorted metal scrap", "Reclaimed alloy and ferrous scrap. Recycle it aboard the Magnum to recover smelting feedstock.", 2);
            AddMaterial(Fiber, "rags", "Reclaimed fiber", "Separated pulp fiber or recovered textiles, ready for textile processing.", 2);
            AddMaterial(Polymer, "plastic", "Polymer scrap", "Shredded housings and armor polymer. Recycle it aboard the Magnum to recover moldable polymer stock.", 2);
            AddMaterial(Cullet, "glass", "Glass cullet", "Crushed glass. Recycle it aboard the Magnum to recover silicate feedstock.", 2);
            AddMaterial(Conductors, "wire", "Conductive scrap", "Conductive material separated from ore or recovered from equipment, ready for cable production.", 3);
            AddMaterial(Silicate, "glass", "Silicate feedstock", "Mineral feedstock separated from ore for basic or industrial glass production.", 3);
            AddMaterial(Metal, "rusty_plates", "Refined metal stock", "Smelted stock for sheet and hardware.", 9);
            AddMaterial(Textile, "rags", "Processed textile", "Refined fiber stock ready for cutting and forming.", 7);
            AddMaterial(Sheet, "rusty_plates", "Rolled armor plate", "Metal sheet ready for pressing into inserts.", 14);
            AddMaterial(Fittings, "spring", "Armor fittings", "Fasteners and hardware for equipment assembly.", 9);
            AddMaterial(Flak, "rags", "Flak laminate", "Layered protective fabric for cutting and fitting.", 16);
            AddMaterial(Liner, "rags", "Fitted armor liner", "A padded liner for field and protective armor.", 19);
            AddMaterial(Plate, "rusty_plates", "Pressed armor insert", "Shaped plate for standard protective armor.", 24);
            AddMaterial(HeavyPlate, "rusty_plates", "Reinforced carapace insert", "Heavy plate for reinforced carapace armor.", 45);
            AddMaterial(PolymerStock, "plastic", "Moldable polymer stock", "Polymer refined from organic oil and chemical extract, or recovered from scrap, for housings and medical trays.", 7);
            AddMaterial(GlassStock, "glass", "Industrial glass", "Refined glass for power cells.", 7);
            AddMaterial(CellStock, "acid", "Cell stock", "Prepared chemical stock for sealed power cells.", 8);
            AddMaterial(Cell, "battery_basic_ammo", "Sealed cell assembly", "Tested cell assembly for a Guard Las Clip.", 11);
            AddMaterial(Housing, "plastic", "Empty Las Clip housing", "Molded clip case ready for a sealed cell.", 6);
            AddMaterial(Case, "wire", "Sorted medium casing", "Prepared casing for AnCom medium ammunition.", 4);
            AddMaterial(Module, "powder", "Medium cartridge payload", "A sealed charge used to assemble AnCom medium cartridges.", 5);
            AddMaterial(Shell, "plastic", "Empty specialist shell", "Prepared shell casing for AnCom flechette ammunition.", 5, 2);
            AddMaterial(Flechette, "powder", "Sealed flechette module", "Packaged component for a specialist shell.", 9, 2);
            AddMaterial(Leaf, "rags", "Harvested tobacco", "Raw leaf stock from a DaydreamChems grow cycle.", 5);
            AddMaterial(Tobacco, "rags", "Prepared tobacco", "Processed leaf stock for the rolling line.", 10);
            AddMaterial(FilterStock, "plastic", "Filter stock", "Prepared feedstock for cigarette filters.", 7);
            AddMaterial(Filter, "plastic", "Cigarette filters", "A set of finished filters for the rolling line.", 8);
            AddMaterial(Carton, "paper_towel", "Empty cigarette box", "Folded packaging for loose cigarettes.", 5);
            AddMaterial(Loose, "cigarettes_1", "Loose cigarettes", "Rolled stock ready for the packing order.", 18);
            AddMaterial(Dressing, "rags", "Sterile dressing roll", "Prepared medical fiber for dressings and trays.", 11);
            AddMaterial(Treatment, "pills_antibiotics", "Sealed treatment pack", "Packaged medical stock for field medkits.", 20);
            AddMaterial(Tray, "plastic", "Prepacked dressing tray", "A prepared kit tray with a sterile dressing.", 26);
            AddMaterial(Clinical, "pills_antibiotics", "Sealed clinical refill", "An advanced refill for a military medkit.", 58, 2);
            AddMaterial(Instruments, "plastic", "Sterile instrument tray", "An arranged tray for surgical kits.", 47, 2);
            AddMaterial(Biomass, "rags", "Raw vat biomass", "Unprocessed organic feedstock for pulp, oil, chemical separation, ration vats, and grow cycles.", 3);
            AddMaterial(RationBase, "ration_pack_1", "Vat ration base", "Processed biomass ready for ration packing.", 5);
            AddMaterial(Wrapper, "paper_towel", "Ration wrapper", "An empty wrapper for one field ration.", 2);

            RegisterRawMaterialChemistry();
            RegisterManufacturingIntermediates();

            // Extraction and water recovery start the material chains without consuming finished goods.
            SharedRecipe(Ore, 3, 2);
            SharedRecipe("water_bottle_1", 3, 2);
            SharedRecipe(Metal, 6, 2, new ItemQuantity(Ore, 3));
            SharedRecipe(Textile, 4, 2, new ItemQuantity(Fiber, 3));
            SharedRecipe(Fiber, 2, 2, new ItemQuantity(RawPulp, 3));
            SharedRecipe(PolymerStock, 4, 2, new(OrganicOil, 2), new(ChemicalExtract, 1));
            SharedRecipe(Polymer, 2, 2, new ItemQuantity("plastic", 3));
            SharedRecipe(Silicate, 2, 2, new ItemQuantity(Ore, 3));
            SharedRecipe("glass", 2, 2, new ItemQuantity(Silicate, 2));
            SharedRecipe(GlassStock, 4, 2, new ItemQuantity(Silicate, 3));
            SharedRecipe("wire", 3, 1, new ItemQuantity(Conductors, 3));
            SharedRecipe(Sheet, 3, 2, new ItemQuantity(Metal, 2));
            SharedRecipe(Fittings, 3, 2, new(Metal, 1), new("spring", 1));
            SharedRecipe(Flak, 3, 2, new(Textile, 2), new(PolymerStock, 1));
            SharedRecipe(Liner, 3, 2, new ItemQuantity(Textile, 2));
            SharedRecipe(Plate, 3, 2, new ItemQuantity(Sheet, 2));
            SharedRecipe(HeavyPlate, 5, 1, new(Plate, 2), new(Sheet, 1));
            IndustryRecipe(MetalScrap, 3, 2, new ItemQuantity("rusty_plates", 3));
            IndustryRecipe(Cullet, 2, 2, new ItemQuantity("glass", 3));
            SharedRecipe(Conductors, 2, 2, new ItemQuantity(Ore, 3));
            AddCatalog("iom_workshop_fundamentals_chip", "Workshop Fundamentals Schematic", "Patterns for sorting metal and glass scrap. Ore separation is available as a basic Magnum recipe.",
                ItemCategory, 1, 95, MetalScrap, Cullet);

            IndustryRecipe(CellStock, 3, 3, new("acid", 2), new("wire", 3));
            IndustryRecipe(Cell, 4, 4, new(GlassStock, 2), new(CellStock, 2));
            IndustryRecipe(Housing, 2, 4, new ItemQuantity(PolymerStock, 3));
            IndustryRecipe(LasClip, 3, 6, new(Cell, 1), new(Housing, 1));
            AddCatalog(FieldPlans, "Imperial Field Plans", "Las Clips, cell stock, sealed cells and empty housings.",
                ItemCategory, 1, 85, LasClip, CellStock, Cell, Housing);

            IndustryRecipe(Case, 3, 4, new ItemQuantity(Metal, 1));
            IndustryRecipe(Module, 3, 4, new ItemQuantity("powder", 2));
            IndustryRecipe("medium_basic_ammo", 2, 8, new(Case, 1), new(Module, 1));
            AddCatalog("iom_ancom_cartridge_chip", "AnCom cartridge schematic", "Medium cartridges and their prepared components.",
                "AnCom", 1, 90, "medium_basic_ammo", Case, Module);
            IndustryRecipe(Shell, 3, 4, new(PolymerStock, 1), new(Metal, 1));
            IndustryRecipe(Flechette, 4, 3, new("nail_bolts_ammo", 4), new("powder", 2));
            IndustryRecipe("shotgun_flechette_ammo", 3, 6, new(Shell, 1), new(Flechette, 1));
            AddCatalog("iom_ancom_shell_chip", "AnCom specialist shell schematic", "Flechette shells and their prepared components.",
                "AnCom", 2, 185, "shotgun_flechette_ammo", Shell, Flechette);

            AddArmorCatalog("iom_realware_armor_chip", "RealWare protective armor pattern", "RealWare", 1, 165, Plate,
                "rwa_light_armor_1", "rwa_light_helmet_1", "rwa_light_pants_1", "rwa_light_boots_1");
            AddArmorCatalog("iom_pdf_armor_chip", "PDF flak patterns", ItemCategory, 1, 115, Flak,
                _lightChestplate.Id, _lightHelmet.Id, _lightLeggings.Id, _lightBoots.Id);
            AddArmorCatalog("iom_cadian_armor_chip", "Cadian flak patterns", ItemCategory, 2, 170, Flak,
                _cadiaChestplate.Id, _cadiaHelmet.Id, _cadiaLeggings.Id, _cadiaBoots.Id);
            AddArmorCatalog("iom_arbites_armor_chip", "Arbites carapace patterns", ItemCategory, 3, 245, Plate,
                ArbitesCuirass, ArbitesHelmet, ArbitesGreaves, ArbitesBoots, SubductorCuirass);
            IndustryRecipe(_lasgun.Id, 12, 1, new(Sheet, 2), new(Fittings, 1), new("weapon_parts", 2));
            AddCatalog("iom_guard_weapon_chip", "Guard armory schematic", "Guard service Lasgun manufacture.",
                ItemCategory, 1, 125, _lasgun.Id);
            IndustryRecipe(GuardFrag, 5, 2, new(Fittings, 1), new("powder", 2), new("tin", 1));
            AddCatalog("iom_guard_munitions_chip", "Guard Munitions Schematic", "Magnum fabrication patterns for Guard Frag Grenades.",
                ItemCategory, 2, 140, GuardFrag);
            IndustryRecipe(_servitorBackpack.Id, 16, 1, new(Liner, 2), new(Fittings, 2), new("circuit_board", 1), new("rod_parts", 2));
            IndustryRecipe(_longLas.Id, 16, 1, new(Sheet, 2), new(Fittings, 1), new("weapon_parts", 2), new("lens", 2));
            IndustryRecipe(_plasmaPistol.Id, 18, 1, new(Fittings, 2), new(Cell, 1), new("weapon_parts", 2), new("capacitor_parts", 1));
            AddCatalog("iom_guard_specialist_chip", "Guard Specialist Equipment Schematic", "Fabrication patterns for the Servitor Backpack, Long-Las, and Plasma Pistol.",
                ItemCategory, 3, 235, _servitorBackpack.Id, _longLas.Id, _plasmaPistol.Id);
            IndustryRecipe(_bolter.Id, 24, 1, new(Sheet, 3), new(Fittings, 2), new("weapon_parts", 3), new("spring", 2));
            IndustryRecipe(_melta.Id, 26, 1, new(Sheet, 2), new(Fittings, 2), new("weapon_parts", 2), new("capacitor_parts", 2));
            AddCatalog("iom_guard_heavy_armory_chip", "Guard Heavy Armory Schematic", "Fabrication patterns for the Bolter and Melta Rifle.",
                ItemCategory, 5, 385, _bolter.Id, _melta.Id);
            IndustryRecipe(Judgement, 14, 1, new(Sheet, 2), new(Fittings, 2), new("weapon_parts", 3));
            IndustryRecipe(ShockMaul, 10, 1, new(Metal, 2), new(Cell, 1), new("rod_parts", 1));
            AddCatalog("iom_arbites_weapon_chip", "Arbites armory schematic", "Judgement shotgun and shock maul manufacture.",
                ItemCategory, 3, 230, Judgement, ShockMaul);

            IndustryRecipe(Leaf, 14, 4, new(Biomass, 1), new("water_bottle_1", 1));
            IndustryRecipe(Tobacco, 4, 4, new ItemQuantity(Leaf, 4));
            IndustryRecipe(FilterStock, 4, 2, new(PolymerStock, 1), new("acid", 1));
            IndustryRecipe(Filter, 2, 2, new(FilterStock, 1), new(Textile, 1));
            IndustryRecipe(Carton, 2, 2, new ItemQuantity("paper_towel", 2));
            IndustryRecipe(Loose, 4, 2, new(Tobacco, 2), new(Filter, 1), new("paper_towel", 1));
            IndustryRecipe("cigarettes_1", 2, 2, new(Loose, 1), new(Carton, 1));
            AddCatalog("iom_cigarette_chip", "DaydreamChems cigarette line schematic", "Tobacco grow orders, filters, boxes, rolling and packing.",
                "DaydreamChems", 1, 110, "cigarettes_1", Leaf, Tobacco, FilterStock, Filter, Carton, Loose);

            IndustryRecipe(Dressing, 4, 2, new(Textile, 1), new("medical_glue", 1));
            IndustryRecipe(Treatment, 5, 2, new("pills_antibiotics", 1), new("pills_sorbent", 1));
            IndustryRecipe(Tray, 3, 2, new(Dressing, 1), new(PolymerStock, 1));
            IndustryRecipe("medical_kit_1", 6, 1, new(Tray, 1), new(Treatment, 1), new("splint", 1));
            AddCatalog("iom_field_medical_chip", "DaydreamChems field medical schematic", "Field medkits, dressing rolls, treatment packs and trays.",
                "DaydreamChems", 1, 125, "medical_kit_1", Dressing, Treatment, Tray);
            IndustryRecipe(Clinical, 6, 1, new(Treatment, 1), new("pills_osteodron", 1), new("pills_tranexam", 1));
            IndustryRecipe(Instruments, 7, 1, new(Metal, 1), new(Dressing, 1), new("medical_glue", 1));
            IndustryRecipe("medical_kit_2", 9, 1, new("medical_kit_1", 1), new(Clinical, 1));
            IndustryRecipe("surgical_kit", 12, 1, new("medical_kit_1", 1), new(Instruments, 1), new("syringe_naloxone", 1));
            AddCatalog("iom_clinical_chip", "DaydreamChems clinical schematic", "Advanced medkits, surgical kits, refills and instrument trays.",
                "DaydreamChems", 2, 235, "medical_kit_2", "surgical_kit", Clinical, Instruments);

            IndustryRecipe(RationBase, 3, 4, new ItemQuantity(Biomass, 3));
            IndustryRecipe(Wrapper, 2, 4, new ItemQuantity("paper_towel", 2));
            IndustryRecipe(CorpseStarch, 3, 4, new(RationBase, 2), new(Wrapper, 2));
            // Dirty Corpse Starch is a finished food output; it has no reverse recycling route.
            IndustryRecipe(DirtyStarch, 2, 3, new(Biomass, 2), new(Wrapper, 1));
            AddCatalog("iom_ration_chip", "Imperial ration line schematic", "Ration base, wrappers, and two grades of Corpse Starch. Raw biomass preparation is available as a basic Magnum recipe.",
                ItemCategory, 1, 95, CorpseStarch, RationBase, Wrapper, DirtyStarch);

            RegisterValhallanProduction();
            RegisterSteelLegionProduction();
            RegisterElysianProduction();
            RegisterKriegProduction();
            RegisterScionProduction();
            RegisterSkitariiProduction();
            RegisterSororitasProduction();
            RegisterOrdoXenosProduction();
            RegisterGreyKnightsProduction();
            RegisterGoldenPaladinProduction();
            RegisterSpecialistSupplyProduction();
            RegisterAutomataComponents();
            RegisterComponentFoundations();
            RegisterSupportProduction();
            RegisterProvisioningWorkshops();
            RegisterBaseGameWorkshops();
            RegisterDeployableWorkshops();
            RegisterMaintenanceWorkshops();
            RegisterArmoryWorkshops();
            RegisterCyberneticsWorkshops();
            // Occult recipes validate after their cybernetics ingredients are staged.
            RegisterOccultProduction();
            RegisterPharmaceuticalWorkshops();
            RegisterChaosProduction();
            RegisterShredding();
        }

        private static TrashRecord AddMaterial(string id, string donor, string title, string description, float price,
            int techLevel = 1, string category = ItemCategory, int maxIconSize = 0, string artwork = null)
        {
            var source = Data.Items.GetSimpleRecord<ItemRecord>(donor)
                ?? throw new InvalidDataException("No production material descriptor donor " + donor);
            var descriptor = UnityEngine.Object.Instantiate(source.ItemDesc);
            descriptor.name = id;
            descriptor._overridenRenderId = string.Empty;
            ApplyItemArtwork(descriptor, artwork ?? id, maxIconSize);
            var material = new TrashRecord
            {
                Id = id, ContentDescriptor = descriptor, Categories = [category], TechLevel = techLevel,
                Price = price, Weight = 0.05f, InventoryWidthSize = 1, ItemClass = ItemClass.Parts,
                MaxStack = 30, SubType = TrashSubtype.BartherResource
            };
            RegisterItem("trash", material, title, description);
            return material;
        }

        private static void SharedRecipe(string output, float hours, int batch, params ItemQuantity[] inputs)
        {
            SharedIndustryRecipes.Add(output);
            IndustryRecipe(output, hours, batch, inputs);
        }

        private static void IndustryRecipe(string output, float hours, int batch, params ItemQuantity[] inputs)
        {
            // Keep each ingredient type; whole-item costs round up when halved.
            // Clone quantities because station workshops can share the caller's input objects.
            var fabricationInputs = inputs.Select(input => new ItemQuantity(input.ItemId,
                Math.Max(1, input.Count / 2 + input.Count % 2))).ToArray();
            IndustryBatches.Add(output, checked(batch * 2));
            Registrations.Add(() =>
            {
                if (Data.Items.GetRecord(output) == null || fabricationInputs.Any(input => Data.Items.GetRecord(input.ItemId) == null))
                {
                    throw new InvalidDataException("Unregistered item in production recipe " + output);
                }
                var existing = Data.ProduceReceipts.FirstOrDefault(recipe => recipe.OutputItem == output);
                if (existing == null)
                {
                    Data.ProduceReceipts.Add(new ItemProduceReceipt
                    {
                        Id = output, OutputItem = output, RequiredItems = [.. fabricationInputs], ProduceTimeInHours = hours,
                        ModifyStartCost = 1, ModifyStep = 1.05f, ModifyItemsGrades = []
                    });
                }
                else
                {
                    existing.RequiredItems = [.. fabricationInputs];
                    existing.ProduceTimeInHours = hours;
                }
            });
        }

        private static void AddArmorCatalog(string chip, string title, string faction, int tier,
            int price, string insert, params string[] outputs)
        {
            for (int i = 0; i < outputs.Length; i++)
            {
                string piece = outputs[i];
                IndustryRecipe(piece, i == 0 || i == 4 ? 10 : 6, 1,
                    new(piece == SubductorCuirass ? HeavyPlate : insert, i == 0 || i == 2 ? 2 : 1),
                    new(Liner, 1), new(Fittings, 1));
            }
            AddCatalog(chip, title, "Patterns for the individual armor pieces in this set.",
                faction, tier, price, outputs);
        }

        private static void AddCatalog(string id, string title, string description, string faction,
            int tier, int price, params string[] outputs)
        {
            string artwork = SpriteResources.ContainsKey("ImperiumOfMan.Art." + id + "_icon.png") ? id : FieldPlans;
            var disk = CloneItem<DatadiskRecord>(id, "low_chip", tier, price, artwork);
            disk.Categories = [faction];
            RegisterCatalog(disk, title, description, faction, outputs);
        }

        private static void RegisterCatalog(DatadiskRecord disk, string title, string description, string faction,
            string[] outputs, bool restrictOtherSources = true, string supplier = null)
        {
            outputs = outputs.Distinct().ToArray();
            IndustryCatalogs.Add(disk.Id, outputs);
            if (restrictOtherSources)
            {
                LicensedProducts.UnionWith(outputs);
            }
            // Native item creation rolls one ID from this pool and saves it on that chip.
            disk.UnlockIds = [.. outputs];
            if (outputs.Length > 1)
            {
                description = "Contains one randomly selected fabrication recipe. " + description;
            }
            RegisterItem("datadisks", disk, title, description);
            AddFactionReward(faction, "rewardChips", disk.TechLevel, 4, disk.Price, disk.Id);
            if (!CatalogStock.TryGetValue(faction, out var stock))
            {
                stock = [];
                CatalogStock.Add(faction, stock);
            }
            stock.Add((disk.Id, disk.TechLevel, supplier ?? (faction == ItemCategory ? TerraStation : null)));
        }

        private static void FinishIndustryRegistration()
        {
            Data.Global.StartReceipts.RemoveAll(LicensedProducts.Contains);
            Data.WorkbenchReceipts.RemoveAll(recipe => LicensedProducts.Contains(recipe.OutputItem));
            // Specialist supplies extend several pools after their chips are registered.
            // Finalize only our records; native chips retain their original recipe pools.
            foreach (var catalog in IndustryCatalogs)
            {
                Data.Items.GetSimpleRecord<DatadiskRecord>(catalog.Key).UnlockIds =
                    catalog.Value.Distinct().ToList();
            }
            _industryReady = true;
        }

        private static void LearnRecipes(MagnumCargo cargo, IEnumerable<string> outputs)
        {
            foreach (string output in outputs)
            {
                if (!cargo.UnlockedProductionItems.Contains(output))
                {
                    cargo.UnlockedProductionItems.Add(output);
                }
            }
        }

        [HarmonyPatch(typeof(MagnumCargoSystem), nameof(MagnumCargoSystem.Create))]
        private static class IndustryStarterRecipes
        {
            private static void Postfix(MagnumCargo __result, bool initContent)
            {
                if (_industryReady && initContent)
                {
                    LearnRecipes(__result, SharedIndustryRecipes);
                }
            }
        }

        [HarmonyPatch(typeof(ItemProductionSystem), nameof(ItemProductionSystem.StartMagnumItemProduction))]
        private static class IndustryProduction
        {
            private static bool Prefix(MagnumCargo magnumCargo, ItemProduceReceipt receipt, int count, out int __state)
            {
                __state = 0;
                if (!_industryReady || !IndustryBatches.TryGetValue(receipt.OutputItem, out int batch))
                {
                    return true;
                }
                if (count < 1 || count > int.MaxValue / batch ||
                    !magnumCargo.UnlockedProductionItems.Contains(receipt.OutputItem) ||
                    ItemProductionSystem.GetAvailableToProduceCount(
                        MagnumCargoSystem.GetAvailableCargoStorages(magnumCargo), receipt.RequiredItems) < count)
                {
                    return false;
                }
                __state = batch;
                return true;
            }

            private static void Postfix(MagnumCargo magnumCargo, int lineIndex, int __state)
            {
                if (__state > 1)
                {
                    var orders = magnumCargo.ItemProduceOrders[lineIndex];
                    orders[orders.Count - 1].Count *= __state;
                }
            }
        }
    }
}

namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        private enum CampaignObjective { Clear, Recover, Sabotage, Hunt, Escort, Control }

        private sealed class CampaignQuest(string key, string name, string station, string rival, CampaignObjective objective,
            (string Entry, string Middle, string Final) decks, string briefing, string task, string report,
            string target = null, bool claim = false, string reward = null)
        {
            internal readonly string Key = key;
            internal readonly string Name = name;
            internal readonly string Station = station;
            internal readonly string Rival = rival;
            internal readonly CampaignObjective Objective = objective;
            internal readonly (string Entry, string Middle, string Final) Decks = decks;
            internal readonly string Briefing = briefing;
            internal readonly string Task = task;
            internal readonly string Report = report;
            internal readonly string Target = target;
            internal readonly bool Claim = claim;
            internal readonly string Reward = reward;
        }

        private const string Server = "serverStation";
        private const string Scanner = "spaceScaner";
        private const string CargoBlock = "cargoBlock";
        private const string Reliquary = "relicvarium";

        private static readonly CampaignQuest[] ImperialCampaign =
        [
            // Earth and Luna: control the supplies that keep a garrison alive.
            new("palace_writ", "The Palace Writ", TerraStation, "CResistance", CampaignObjective.Recover,
                ("Outer Precinct", "Arbites Barracks", "Palace Archive"),
                "The Palace sent our commission to the outer precinct for countersignature. A breakaway cell got to the archive first. Bring the sealed writ back so the Arbites can put the docks under Imperial authority.",
                "Recover the Palace Writ and extract with it.",
                "The Palace has countersigned the writ. The waterworks are first on the requisition list.", "Palace Writ"),
            new("water_tithe", "The Water Tithe", MeridianWaterworks, "FrancheComte", CampaignObjective.Clear,
                ("Intake Terminal", "Treatment Galleries", "Pump Galleries"),
                "Franche-Comté is holding the orbital water tanks until the Palace pays its tariff. Our ration works have three days of water. Take the pump galleries and leave the pipes intact.",
                "Clear the waterworks so Imperial crews can take over the pumps.",
                "The pumps are running under Palace authority. Follow the water tankers to Greenline.", claim: true),
            new("bread_and_seal", "Bread and Seal", GreenlineProvisions, "DaydreamChems", CampaignObjective.Sabotage,
                ("Receiving Dock", "Growing Racks", "Harvest Dispatch"),
                "Daydream is routing the farm's harvest through its own bonded warehouse. Break the dispatch server that holds the diversion orders. The Palace has loaders waiting.",
                "Destroy the marked dispatch server and extract.",
                "The first harvest is loading for Fuller. The ration crews are asking for the tools held at Junction.", Server, claim: true),
            new("yard_ledgers", "The Yard Ledgers", JunctionReclamation, "PlanetBridge", CampaignObjective.Recover,
                ("Scrap Receiving", "Tool Workshops", "Reclamation Office"),
                "The workshop foremen agreed to the tithe. PlanetBridge's security arm kept the books and diverted the tools. The ledger will tell our quartermasters which lockers and haulers are ours to requisition.",
                "Recover the Reclamation Ledger and extract with it.",
                "Junction's foremen have opened the tool lockers. The convoy is taking on cargo at Fuller.", "Reclamation Ledger", claim: true),
            new("fuller_picket", "The Fuller Picket", LunaStation, "UnchainedBelt", CampaignObjective.Clear,
                ("Fuller Dock", "Ration Stores", "Convoy Loading Bays"),
                "The first water and ration convoy is at Fuller. Belt raiders have boarded our provisioning station and are cutting toward the cargo bays. Clear the dock before the haulers lose their launch window.",
                "Clear the boarders from the Fuller provisioning station.",
                "The garrison has the docks. The haulers will take the first Imperial convoy through Venus customs.", reward: WinterPlans),

            // Venus: take the customs corridor and bring its industry under the writ.
            new("false_clearance", "False Clearance", HavocStation, "CoreWard", CampaignObjective.Recover,
                ("Havoc Receiving", "Fabrication Shops", "Inspection Office"),
                "A CoreWard inspection team used forged clearance to board the Havoc works. Recover their seal matrix before they send copies to every Venus traffic controller.",
                "Recover the Forged Seal Matrix and extract with it.",
                "The matrix traces to a press at Sampo Quern. The Fabricator's crews are waiting on the order to board.", "Forged Seal Matrix"),
            new("quern_yard", "The Quern Yard", "SampoQuern", "CoreWard", CampaignObjective.Clear,
                ("Quern Freight Yard", "Fabrication Gantries", "Seal Press Hall"),
                "The trail leads to Sampo Quern. Its private guards have sealed the gantries where the forged passes were pressed. Clear them out and turn the yard over to the Fabricator's crews.",
                "Clear the Quern Yard and claim it for the Imperium.",
                "Our fabrication crews have the press. The forged seals also opened a line into Neith.", claim: true, reward: LegionPlans),
            new("cold_archive", "The Cold Archive", NeithVault, "DaydreamChems", CampaignObjective.Sabotage,
                ("Vault Approach", "Propagation Bays", "Seed Archive"),
                "Daydream has boarded Neith with an uplink for its stolen access codes. Break the uplink before it transmits the seed vault's inventory. The archive must remain intact.",
                "Destroy the marked uplink and extract.",
                "Neith has sealed the archive. The last vessels to use its stolen codes launched from Veiled Gate.", Scanner),
            new("cult_airlock", "The Cult at the Airlock", "iomVeiledGate", ChaosFaction, CampaignObjective.Hunt,
                ("Veiled Gate Airlock", "Boarding Lanes", "Launch Hangar"),
                "The stolen seals have been turning up on cult shuttles. At Veiled Gate a Warp Talon is leading the last boarding cadre. Kill it before the cadre disperses along the Venusian coast.",
                "Eliminate the Warp Talon and extract.",
                "Our crews are clearing the runway. The customs brokers at EStation still hold the private registry.", ChaosItemPrefix + "warp_talon", claim: true),
            new("customs_blackout", "Customs Blackout", "EStation", "DaydreamChems", CampaignObjective.Sabotage,
                ("Customs Intake", "Inspection Decks", "Private Registry"),
                "The brokers at EStation can still issue competing manifests from a private registry. Cut that registry. From this shift onward the Imperial seal clears cargo in Venus orbit.",
                "Destroy the marked manifest server and claim EStation.",
                "The Venus controllers are accepting the writ. The first clearance request came from a stranded ore convoy at Caloris.", Server, claim: true),

            // Mercury: seize the mines, defend the route, and enforce one traffic code.
            new("caloris_levy", "The Caloris Levy", "CalorisPlanitia", "FrancheComte", CampaignObjective.Clear,
                ("Caloris Ore Lanes", "Mine Galleries", "Hoist Galleries"),
                "Caloris has enough ore to keep the foundries lit. Its concession guard has locked the loaders down. Clear the mine and hand the hoists to the levy office.",
                "Clear the concession guard and claim Caloris Planitia.",
                "The Caloris hoists are loading. Boethius has trapped the departing barges in a false queue.", claim: true),
            new("counterfeit_orbit", "Counterfeit Orbit", "Boethius", "SBN", CampaignObjective.Recover,
                ("Signal Exchange", "Relay Decks", "Routing Archive"),
                "SBN rerouted the ore barges into a customs queue that never clears. The routing cipher is still on Boethius. Bring it aboard so we can release the barges under Imperial call signs.",
                "Recover the Mercury Routing Cipher and extract with it.",
                "The ore barges have their new call signs. Goethe's gun decks are still refusing passage.", "Mercury Routing Cipher"),
            new("goethe_deadlock", "The Goethe Deadlock", "Goethe", "RealWare", CampaignObjective.Hunt,
                ("Goethe Gun Deck", "Battery Galleries", "Blockade Command"),
                "RealWare's blockade commander has refused passage to the released barges. A senior officer is directing the gun decks from Goethe. Remove the officer and the convoy can pass.",
                "Eliminate the RealWare blockade commander and extract.",
                "Goethe's crews stood down. The convoy logged stolen ore moving through the Cinder Crucible.", "rwa_elite"),
            new("extinguish_crucible", "Extinguish the Crucible", "iomCinderCrucible", ChaosFaction, CampaignObjective.Sabotage,
                ("Crucible Intake", "Forge Line", "Forge Control Room"),
                "The Crucible is melting stolen cargo into daemon-engine parts. Knock out the marked forge controller. The Mechanicus will secure the works before production can resume.",
                "Destroy the marked forge controller and claim the Crucible.",
                "The daemon line has gone cold. Discovery still holds the survey our pilots need to avoid the wrecks.", Server, claim: true, reward: XenosPlans),
            new("discovery_injunction", "Discovery Injunction", "Discovery", "AnCom", CampaignObjective.Recover,
                ("Discovery Freight Entrance", "Inspection Bays", "Survey Vault"),
                "AnCom's surveyors kept the only complete chart of the Mercury freight lanes. Their garrison will burn the survey before it honors the injunction. Lift the original chart from the vault.",
                "Recover the Mercury Survey Plate and extract with it.",
                "The survey is in the pilot books. The first load for Mars is asking Phobos for a depot signal.", "Mercury Survey Plate", claim: true),

            // Mars and its moons: link the mines, foundry, and munitions depot.
            new("hollow_signal", "Silence the Hollow Signal", "iomHollowSignalDepot", ChaosFaction, CampaignObjective.Sabotage,
                ("Depot Approach", "Relay Galleries", "Beacon Room"),
                "Hollow Signal is impersonating our munitions depot on Phobos. Shut down the cult's beacon and seize the station before the next arms convoy takes its bearings.",
                "Destroy the marked spoofing beacon and claim the depot.",
                "Phobos has a clear frequency. Foundry Primaris is waiting for an ore load from Cinder Basin.", Scanner, claim: true),
            new("cinder_basin", "Secure the Basin", CinderBasinMine, "CoreWard", CampaignObjective.Clear,
                ("Cinder Basin Shafts", "Excavation Galleries", "Lower Mine"),
                "CoreWard wants to trap the ore crews underground and starve the forge of raw stock. Clear its contract troops from Cinder Basin and reopen the lifts.",
                "Clear Cinder Basin Mine and put it under Imperial control.",
                "The lifts are open. Tianming filed a bonded cargo claim on the first furnace shipment.", claim: true),
            new("last_bonded_shipment", "The Last Bonded Shipment", "RedTiangong", "Tianming", CampaignObjective.Recover,
                ("Tiangong Cargo Hall", "Bonded Holds", "Cargo Registry"),
                "Tianming's freight controllers can still impound finished arms on a technicality. Take the bonded cargo registry from Red Tiangong and settle the last outstanding claim.",
                "Recover the Martian Bond Register and extract with it.",
                "The claim is struck from the register. Belt raiders followed the freed cargo into Foundry Primaris.", "Martian Bond Register"),
            new("prime_the_foundry", "Prime the Foundry", FoundryStation, "UnchainedBelt", CampaignObjective.Hunt,
                ("Foundry Loading Bay", "Interlock Galleries", "Foundry Floor"),
                "A Belt strike commander has followed the ore convoys into Foundry Primaris. Kill the commander on the loading levels so the forge crews can restart the line.",
                "Eliminate the Belt strike commander and extract.",
                "The loading crews are back on shift. Hesperia's dispatch office is the last signature the Palace needs.", "unc_elite", reward: SororitasPlans),
            new("one_standard", "Under One Standard", HesperiaFabrication, "ChurchRevelation", CampaignObjective.Clear,
                ("Hesperia Dispatch Yard", "Fabrication Shops", "Dispatch Court"),
                "The last independent dispatch office on this route is holding Hesperia's parts behind a private garrison. Take the floor and register the workshops under the Palace writ. The inner supply corridor will answer to one standard.",
                "Clear Hesperia Fabrication, claim it for the Imperium, and extract.",
                "Hesperia has entered the writ in its books. From the Fuller ration bays to the Martian forge, the inner supply corridor now ships under Imperial seals.", claim: true)
        ];

        private static readonly CampaignQuest[] UndividedCampaign =
        [
            // The inner fringe: win anchorage and learn who holds the outer lanes.
            new("beyond_saturn", "Beyond Saturn's Light", "iomFringeAshmouth", "CattaramFeral", CampaignObjective.Clear,
                ("Ashmouth Landing", "Anchorage Galleries", "Mooring Locks"),
                "Cattaram's servitors have chained the landing locks at Ashmouth. Break the watch. The Undivided need a place where a ship can dock without paying a rival prince.",
                "Clear Cattaram's watch and claim Ashmouth Anchorage.",
                "The locks answer to us. Ron's listeners have marked every wake that came through the crossing.", claim: true),
            new("borrowed_wake", "A Borrowed Wake", "iomFringeWakescar", "RonFeral", CampaignObjective.Recover,
                ("Wakescar Approach", "Listening Galleries", "Wake Chart Archive"),
                "Ron's shades are tracking ships by the residue in their wakes. Steal their route chart from Wakescar and we can cross the first belt without following their lights.",
                "Recover the Wake Chart and extract with it.",
                "The chart gives us a way across. One relay still sells tracks along that route.", "Wake Chart"),
            new("mute_the_buoy", "Mute the Buoy", "iomFringeBlackglass", "UnchainedBelt", CampaignObjective.Sabotage,
                ("Blackglass Mooring", "Relay Decks", "Tracking Station"),
                "The Belt sells the Blackglass track to anyone who will pay. Burn out their scanner before our crossing and leave them listening to empty space.",
                "Destroy the marked tracking scanner and extract.",
                "Blackglass has lost the crossing. We can approach the cutters at Copper Jaw in the dark.", Scanner),
            new("copper_hunger", "Copper Hunger", "iomFringeCopperJaw", "ScrivnusFeral", CampaignObjective.Hunt,
                ("Breaker Berth", "Hull-Cutting Decks", "Copper Jaw Yard"),
                "A knight of Scrivnus has laid claim to every hull cut at Copper Jaw. Put it down and the cutters can start opening ships for us.",
                "Eliminate the Scrivnus knight and claim Copper Jaw.",
                "The cutters have cleared a berth. They report Flauros reclaimers boarding at Ashmouth.", "scrivnus_knight", claim: true),
            new("hand_on_the_lock", "A Hand on the Lock", "iomFringeAshmouth", "FlaurosFeral", CampaignObjective.Clear,
                ("Ashmouth Airlock", "Mooring Galleries", "Reclaimer Berth"),
                "Flauros's reclaimers moved into Ashmouth while we were tracing the lanes. The berth is worth more than anything they brought. Take it back before they tear out the locks.",
                "Clear the Flauros boarding party and secure Ashmouth.",
                "Ashmouth's berth is open again. The reclaimers took their orders from Slag Vigil.", claim: true),

            // The middle fringe: break the rival hosts' supply chain.
            new("iron_muster", "The Iron Muster", "iomFringeSlagVigil", "FlaurosFeral", CampaignObjective.Sabotage,
                ("Slag Vigil Landing", "Casting Decks", "Muster Office"),
                "Slag Vigil issues coordinates to the reclaimers. Cut the muster server and follow the silence to its source.",
                "Destroy the marked muster server and extract.",
                "The reclaimers are waiting for orders. Ron's census will tell us which berths they were promised.", Server),
            new("census_of_shadows", "Census of Shadows", "iomFringeSaltCairn", "RonFeral", CampaignObjective.Recover,
                ("Salt Cairn Approach", "Archive Galleries", "Census Vault"),
                "At Salt Cairn the shades have recorded the names of every berth they can reach in one crossing. Take the census before they can mark our new docks.",
                "Recover the Salt Census and extract with it.",
                "Our docks are on the census. So are a cage deck and a bonded hold supplying the same buyers.", "Salt Census"),
            new("the_sump_tithe", "The Sump Tithe", "iomFringeSumpChoir", "DuggurFeral", CampaignObjective.Hunt,
                ("Sump Landing", "Cage Galleries", "Tithe Pens"),
                "A cage-keeper of Duggur is weighing captives against a rival tithe in Sump Choir. Kill the keeper. Its buyers can argue over the empty cages.",
                "Eliminate Duggur's cage-keeper and extract.",
                "The cages are open. Cattaram's levy is the last cargo due to the foundry guard.", "moon_pact_torture"),
            new("debt_in_kind", "Debt in Kind", "iomFringeStoneDebt", "CattaramFeral", CampaignObjective.Sabotage,
                ("Stone Debt Freight Dock", "Levy Holds", "Bonded Hold"),
                "Cattaram has stacked its levy in the bonded holds of Stone Debt. Ruin the marked freight seal so the levy cannot be sent to its elemental guard.",
                "Destroy the marked cargo block and extract.",
                "The levy has missed its lift. Slag Vigil's guard will have to hold with what it has.", CargoBlock),
            new("forges_dont_pray", "Forges Don't Pray", "iomFringeSlagVigil", "FlaurosFeral", CampaignObjective.Clear,
                ("Foundry Approach", "Casting Galleries", "Smelting Hall"),
                "The foundry is still hot after the muster blackout. Clear the reclaimers from the line. We need their metal if we mean to fight beyond the next crossing.",
                "Clear the foundry garrison and claim Slag Vigil.",
                "The foundry is ours. Olirna's choir has started transmitting the ships queuing for the next crossing.", claim: true),

            // The far fringe: take the toll stations that feed the rival coalition.
            new("white_frequency", "The White Frequency", "iomFringeBleachedMast", "OlirnaFeral", CampaignObjective.Sabotage,
                ("Bleached Mast Nave", "Transmitter Wings", "Choir Loft"),
                "Olirna's monks are broadcasting our approach from Bleached Mast. Their choir can keep singing after the mast goes dark.",
                "Destroy the marked choir transmitter and extract.",
                "The mast has dropped off the band. The tollkeeper is still watching the debris gap.", Scanner),
            new("scales_of_passage", "Scales of Passage", "iomFringeBloodToll", "GannixFeral", CampaignObjective.Hunt,
                ("Blood Toll Altar", "Toll Galleries", "Blood Toll Gun Deck"),
                "Gannix has posted a jaguar knight to collect passage in blood. Kill the tollkeeper in its own station. Our crossing is already paid for.",
                "Eliminate the jaguar knight and claim Blood Toll Gate.",
                "The gun deck has ceased fire. A CoreWard tug went through with condemned hulls before the gate closed.", "venus_jaguar_knight", claim: true),
            new("no_salvage_rights", "No Salvage Rights", "iomFringeKedge", "CoreWard", CampaignObjective.Recover,
                ("Kedge Breaker Berth", "Salvage Holds", "Bonded Records"),
                "CoreWard is selling condemned Imperial hulls to the hosts behind us. Take the bill of sale. It names the buyers and every cache they still expect delivered.",
                "Recover the Kedge Bill of Sale and extract with it.",
                "The buyers are named. Ur is fitting the first reactors at Cinder Lock.", "Kedge Bill of Sale"),
            new("furnace_of_ur", "The Furnace of Ur", "iomFringeCinderLock", "UrFeral", CampaignObjective.Hunt,
                ("Cinder Lock Landing", "Reactor Cradles", "Engine Bay"),
                "The Ur host is fitting stolen reactor cores into battle engines. One is already walking the deck. Break that engine before it can board a carrier.",
                "Eliminate the Ur battle engine and extract.",
                "The furnace has stopped taking delivery. Olirna is sending a replacement choir to the mast.", "mars_battle_gun"),
            new("the_right_to_pass", "The Right to Pass", "iomFringeBleachedMast", "OlirnaFeral", CampaignObjective.Clear,
                ("Replacement Berths", "Guard Galleries", "Muster Rooms"),
                "The mast has gone quiet. Olirna's monks are bringing in a new choir under armed guard. Clear the berths and leave our ships a lane through.",
                "Clear the replacement choir and claim Bleached Mast.",
                "The choir's berth is ours. The next crossing goes through unlit debris. Blue Ruin has the only working guide.", claim: true),

            // The outermost belt: dismantle the coalition and take its gate.
            new("prism_rent", "Prism Rent", "iomFringeBlueRuin", "ShartamakhumFeral", CampaignObjective.Recover,
                ("Lens Court", "Optics Galleries", "Observatory"),
                "The lens priests have built a prism that reads the crossings between the farthest rocks. Recover it before they sell the final route to Olirna.",
                "Recover the Passage Prism and extract with it.",
                "The prism shows the last four rocks. Scrivnus has brought a baron to the nearest redoubt.", "Passage Prism"),
            new("starless_oath", "The Starless Oath", "iomFringeSableMile", "ScrivnusFeral", CampaignObjective.Hunt,
                ("Sable Mile Redoubt", "Pact Corridors", "Baron's Guardroom"),
                "Scrivnus has sent a baron to enforce the rival hosts' pact. Put it down at Sable Mile and let the others learn how little the oath buys them.",
                "Eliminate the Scrivnus baron and extract.",
                "The pact has lost its enforcer. Olirna's hounds are loading at the kennels.", "scrivnus_baron"),
            new("hound_road", "The Hound Road", "iomFringeHoundsRest", "OlirnaFeral", CampaignObjective.Clear,
                ("Hound's Rest Culverts", "Kennel Passages", "Embarkation Pens"),
                "Olirna's hounds are being penned one rock short of the gate. If the pack reaches the docks, it can cut off our return. Clear the pens and hold the embarkation point.",
                "Clear the hound garrison and claim Hound's Rest.",
                "Our ships hold the return berth. The Crown is the last gate ahead.", claim: true),
            new("unmake_the_seal", "Unmake the Seal", "iomFringeEbonCrown", "OlirnaFeral", CampaignObjective.Sabotage,
                ("Ebon Crown Approach", "Reliquary Passages", "Seal Vault"),
                "Olirna's baron has sealed the outer gate with a reliquary bound into its works. Break the reliquary and force the baron to come down to the docks.",
                "Destroy the marked seal reliquary and extract.",
                "The collar is opening. Olirna's baron is disembarking onto the gate deck.", Reliquary),
            new("crown_without_sun", "A Crown Without Sun", "iomFringeEbonCrown", "OlirnaFeral", CampaignObjective.Hunt,
                ("Ebon Crown Gate Deck", "Gatehouse Galleries", "Baron's Hall"),
                "The gate is open. The baron has come to hold it in person. Kill it and take the Crown. Every ship that crosses the black frontier will know who holds the last harbor.",
                "Eliminate the Olirna baron, claim the Ebon Crown, and extract.",
                "The Crown answers to the Undivided. The next ship through the last crossing will hear our own signal from the gate.", "olirna_baron", claim: true)
        ];
    }
}

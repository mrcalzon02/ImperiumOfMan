using System.Collections.Generic;
using MGSC;

namespace ImperiumOfMan
{
    public static partial class TranslationPatch
    {
        // These belong to the native tooltip body. Header subtitles are kept in
        // ShortHeaders so a new effect never turns a one-line caption into prose.
        internal static readonly Dictionary<string, string> UpgradeTooltipTextEnglish = new()
        {
            ["iom.tooltip.turrets"] = "Allied turrets",
            ["iom.tooltip.turrets_at_location"] = "Turrets at location",
            ["iom.tooltip.turret_type"] = "Turret type",
            ["iom.tooltip.replaces"] = "Replaces",
            ["iom.tooltip.non_story_missions_only"] = "Non-story missions only",
            ["iom.tooltip.ballistic"] = "Ballistic",
            ["iom.tooltip.position"] = "Position",
            ["iom.tooltip.random"] = "Random site tile",
            ["iom.tooltip.entrance"] = "Near entrance",
            ["iom.tooltip.extraction"] = "Near extraction",
            ["iom.tooltip.deployment"] = "Deployment",
            ["iom.tooltip.arrival"] = "Mission arrival",
            ["iom.tooltip.frequency"] = "Frequency",
            ["iom.tooltip.once_per_raid"] = "Once per raid",
            ["iom.tooltip.placement"] = "Placement",
            ["iom.tooltip.clear_tile"] = "Clear tile needed",
            ["iom.tooltip.requires"] = "Requires",
            ["iom.tooltip.fitted_mount"] = "Any fitted mount",
            ["iom.tooltip.damage"] = "Damage per hit",
            ["iom.tooltip.damage_type"] = "Damage type",
            ["iom.tooltip.explosive"] = "Explosive",
            ["iom.tooltip.beam"] = "Beam",
            ["iom.tooltip.blast_radius"] = "Blast radius",
            ["iom.tooltip.pattern"] = "Strike pattern",
            ["iom.tooltip.line"] = "Straight line",
            ["iom.tooltip.impacts"] = "Impacts per turn",
            ["iom.tooltip.firing_turns"] = "Firing turns",
            ["iom.tooltip.delay"] = "Arrival delay",
            ["iom.tooltip.calls"] = "Calls per raid",
            ["iom.tooltip.self_damage"] = "Can hit you",
            ["iom.tooltip.yes"] = "Yes",
            ["iom.tooltip.turn"] = "turn",
            ["iom.tooltip.turns"] = "turns",
            ["iom.tooltip.tile"] = "tile",
            ["iom.tooltip.tiles"] = "tiles",
            ["iom.tooltip.total_turns"] = "Total firing turns",
            ["iom.tooltip.gun_total"] = "Gun impacts",
            ["iom.tooltip.missile_total"] = "Missile impacts",
            ["iom.tooltip.laser_total"] = "Laser impacts",
            ["iom.tooltip.total_calls"] = "Total calls per raid",
            ["iom.tooltip.shared_reserve"] = "Shared reserve",
            ["iom.tooltip.supply_and_troops"] = "Supply + troops",
            ["iom.tooltip.landing_tiles"] = "Landing tiles",
            ["iom.tooltip.troops"] = "Troops per drop",
            ["iom.tooltip.roster"] = "Roster",
            ["iom.tooltip.equipment"] = "Equipment",
            ["iom.tooltip.clone_equipment"] = "PDF flak / Lasguns",
            ["iom.tooltip.total_troops"] = "Total troops",
            ["iom.tooltip.applies_to"] = "Applies to",
            ["iom.tooltip.all_rosters"] = "All troop rosters",
            ["iom.tooltip.unlocks"] = "Unlocks",
            ["iom.tooltip.imperial_tech"] = "Imperial tech",
            ["iom.tooltip.troop_roster"] = "Troop Roster",
            ["iom.tooltip.production"] = "Production",
            ["iom.tooltip.unavailable"] = "Unavailable",
            ["iom.tooltip.produces"] = "Produces",
            ["iom.tooltip.output_batch"] = "Output per batch",
            ["iom.tooltip.time_batch"] = "Base batch hours",
            ["iom.tooltip.inputs"] = "Inputs",
            ["iom.tooltip.none"] = "None",
            ["iom.tooltip.consumes"] = "Consumes",
            ["iom.tooltip.input_batch"] = "Input per batch",
            ["iom.tooltip.recipe_unlocks"] = "Recipe unlocks",
            ["iom.tooltip.chaos_designs"] = "Chaos designs",
            ["iom.tooltip.design_tiers"] = "Design tiers",
            ["iom.tooltip.tradition"] = "Tradition",
            ["iom.tooltip.research"] = "Research",
            ["iom.tooltip.grafts_weapons"] = "Grafts and weapons",
            ["iom.tooltip.advanced_gear"] = "Advanced occult gear",
            ["iom.tooltip.recipe_details"] = "Recipe details",
            ["iom.tooltip.crafting_map"] = "Crafting Map"
        };

        internal static string UpgradeText(string key)
        {
            string tag = "iom.tooltip." + key;
            return Localization.HasKey(tag) ? Localization.Get(tag) : UpgradeTooltipTextEnglish[tag];
        }

        internal static bool HasUpgradeTooltipDescription(string id) => !string.IsNullOrEmpty(id) &&
            English.ContainsKey("mgperk." + id + ".desc");

        internal static bool TryGetUpgradeTooltipDescription(string id, out string description)
        {
            string tag = "mgperk." + id + ".desc";
            if (string.IsNullOrEmpty(id) || !English.TryGetValue(tag, out description))
            {
                description = null;
                return false;
            }
            if (Localization.HasKey(tag))
            {
                description = Localization.Get(tag);
            }
            return !string.IsNullOrWhiteSpace(description);
        }
    }
}

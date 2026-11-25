namespace ImperiumOfMan
{
    public static partial class Plugin
    {
        // Each atlas has one base and one adjoining material. The base tile owns the join.
        private static DeckFloorJoin? CampaignFloorJoin(DeckFloorZone from, DeckFloorZone to) => (from, to) switch
        {
            (DeckFloorZone.Field, DeckFloorZone.Lane) => DeckFloorJoin.Doorway,
            (DeckFloorZone.Field, DeckFloorZone.Secondary) => DeckFloorJoin.Material,
            (DeckFloorZone.Trim, DeckFloorZone.Feature) => DeckFloorJoin.Recess,
            (DeckFloorZone.Field, DeckFloorZone.Trim) => DeckFloorJoin.FieldTrim,
            (DeckFloorZone.Secondary, DeckFloorZone.Trim) => DeckFloorJoin.SecondaryTrim,
            (DeckFloorZone.Secondary, DeckFloorZone.Lane) => DeckFloorJoin.SecondaryLane,
            _ => null
        };

    }
}

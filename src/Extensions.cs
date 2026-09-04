using MGSC;

namespace ImperiumOfMan;

public static class Extensions
{
    public static ContentDropRecord ToDropRecord(this ItemRecord item, float weight, float points)
    {
        return new ContentDropRecord
        {
            TechLevel = item.TechLevel,
            ContentIds = [item.Id],
            Weight = weight,
            Points = points
        };
    }
}

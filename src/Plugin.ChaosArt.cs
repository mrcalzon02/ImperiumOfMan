using MGSC;
using UnityEngine;

namespace ImperiumOfMan
{
    public static partial class Plugin
    {

        private static void ApplyChaosItemArtwork(ItemRecord item)
        {
            ApplyItemArtwork(item.ItemDesc, item.Id);
            RegisterEquipmentFittings(item);
        }

        private static void RegisterChaosCreaturePortrait(ChaosEnemy enemy)
        {
            string id = enemy.Id + "_portrait";
            var portrait = ScriptableObject.CreateInstance<PortraitDescriptor>();
            portrait.name = id;
            portrait._normalSprite = ReadItemSprite(id, null);
            StageRecord(Data.Portraits, id, new ConfigTableRecord { Id = id, ContentDescriptor = portrait });
            StageDescriptor("portraits", id, portrait);
        }
    }
}

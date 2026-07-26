using System;
using System.Collections.Generic;

namespace FSO.Content.Interfaces
{
    public interface IObjectCatalog
    {
        List<ObjectCatalogItem> All();
        List<ObjectCatalogItem> GetItemsByCategory(sbyte category);
        ObjectCatalogItem? GetItemByGUID(uint guid);
        List<uint> GetUntradableGUIDs();
    }

    public struct ObjectCatalogItem
    {
        public uint GUID;
        public sbyte Category;
        public uint Price;
        public string Name;
        public string CatalogName;
        public string Tags;
        public byte DisableLevel; //1 = only shopping, 2 = rare (unsellable?)

        //DiscoSO: the window this item is sold in, as MMDD, from the catalogue's d="0915-1115".
        //0 means all year. A start after the end wraps over new year, which is how Christmas runs.
        public ushort SeasonStart;
        public ushort SeasonEnd;

        public byte RoomSort;
        public byte Subsort;
        public byte DowntownSort;
        public byte VacationSort;
        public byte CommunitySort;
        public byte StudiotownSort;
        public byte MagictownSort;

        public bool InSeason(DateTime utcNow)
        {
            if (SeasonStart == 0 || SeasonEnd == 0) return true;
            var today = (ushort)(utcNow.Month * 100 + utcNow.Day);
            return (SeasonStart <= SeasonEnd)
                ? (today >= SeasonStart && today <= SeasonEnd)
                : (today >= SeasonStart || today <= SeasonEnd);
        }

        //UTC so a player's timezone can never disagree with the server about what is on sale.
        public bool InSeason()
        {
            return InSeason(DateTime.UtcNow);
        }
    }
}

using System;

namespace FSO.Server.Database.DA.LotEvents
{
    public class DbLotEvent
    {
        public int event_id { get; set; }
        public int lot_id { get; set; }
        public uint? avatar_id { get; set; }
        public uint? target_avatar_id { get; set; }
        public DbLotEventType type { get; set; }
        public int value { get; set; }
        public string data { get; set; }
        public DateTime time { get; set; }

        //filled by the join in GetRecent
        public string actor_name { get; set; }
        public string target_name { get; set; }
    }

    public enum DbLotEventType : byte
    {
        admit_add = 0,
        admit_remove = 1,
        ban_add = 2,
        ban_remove = 3,
        admit_mode = 4,
        lot_expanded = 5,
        category = 6,
        renamed = 7,
        description = 8
    }
}

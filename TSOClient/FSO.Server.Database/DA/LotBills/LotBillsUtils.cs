using FSO.Common.Model;
using FSO.Server.Database.DA.Tuning;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FSO.Server.Database.DA.LotBills
{
    /// <summary>
    /// Database-backed storage for the shared metered billing rules. Sandbox runs the same rules
    /// against the standalone database instead.
    /// </summary>
    public class SqlLotBillsStore : ILotBillsStore
    {
        private readonly IDA DB;
        private readonly int LotID;
        private Dictionary<int, DbTuning> TuningRows;

        public SqlLotBillsStore(IDA db, int lot_id)
        {
            DB = db;
            LotID = lot_id;
        }

        public float Tuning(int index, float def)
        {
            if (TuningRows == null) TuningRows = DB.Tuning.AllCategory("discoso_bills", 0).ToDictionary(x => x.tuning_index);
            return TuningRows.ContainsKey(index) ? TuningRows[index].value : def;
        }

        public int? OldestOutstandingDay()
        {
            return DB.LotBills.OldestOutstandingDay(LotID);
        }

        public LotUsageHours GetUnbilledUsage()
        {
            return Hours(DB.LotUsage.GetUnbilled(LotID));
        }

        public LotUsageHours CollectUnbilledUsage()
        {
            return Hours(DB.LotUsage.CollectUnbilled(LotID));
        }

        public bool AddMeteredCharge(int day, int amount)
        {
            return DB.LotBills.AddToDay(LotID, day, amount);
        }

        private static LotUsageHours Hours(LotUsage.DbLotUsageTotal total)
        {
            return new LotUsageHours()
            {
                LightHours = total.light_hours,
                StallHours = total.stall_hours,
                RadioHours = total.radio_hours,
                TvHours = total.tv_hours
            };
        }
    }

    public static class LotBillsUtils
    {
        public const int TIER_NONE = 0;
        public const int TIER_NO_VISITORS = 1;
        public const int TIER_NO_BUILD = 2;

        /// <summary>
        /// Fold all unbilled metered usage (lights, stalls, stereos) into today's bill. Returns the
        /// amount charged, or 0 if nothing was billed (rates unset, accrual paused, or the charge
        /// would round below $1 - fractions stay unbilled until they add up).
        /// </summary>
        public static int DeliverUsage(IDA db, int lot_id)
        {
            return LotBillsPolicy.DeliverUsage(new SqlLotBillsStore(db, lot_id));
        }

        /// <summary>
        /// Overdue tier for a lot: 0 none, 1 = past grace (no visitors), 2 = further overdue (no build/buy).
        /// </summary>
        public static int GetOverdueTier(IDA db, int lot_id)
        {
            var oldest = db.LotBills.OldestOutstandingDay(lot_id);
            if (oldest == null) return TIER_NONE;

            var tuning = db.Tuning.AllCategory("discoso_bills", 0).ToDictionary(x => x.tuning_index);
            var grace = tuning.ContainsKey(3) ? Math.Max(0, (int)tuning[3].value) : 3;
            var tier2Extra = tuning.ContainsKey(4) ? Math.Max(1, (int)tuning[4].value) : 3;

            var today = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalDays;
            var overdue = today - oldest.Value;
            if (overdue > grace + tier2Extra) return TIER_NO_BUILD;
            if (overdue > grace) return TIER_NO_VISITORS;
            return TIER_NONE;
        }
    }
}

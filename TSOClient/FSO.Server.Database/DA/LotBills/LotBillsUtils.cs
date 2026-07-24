using System;
using System.Linq;

namespace FSO.Server.Database.DA.LotBills
{
    public static class LotBillsUtils
    {
        public const int TIER_NONE = 0;
        public const int TIER_NO_VISITORS = 1;
        public const int TIER_NO_BUILD = 2;

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

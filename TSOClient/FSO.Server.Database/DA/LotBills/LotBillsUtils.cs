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
        /// Fold all unbilled metered usage (lights, stalls) into today's bill. Returns the amount
        /// charged, or 0 if nothing was billed (rates unset, accrual paused, today's bill already
        /// paid, or the charge would round below $1 - fractions stay unbilled until they add up).
        /// </summary>
        public static int DeliverUsage(IDA db, int lot_id)
        {
            var tuning = db.Tuning.AllCategory("discoso_bills", 0).ToDictionary(x => x.tuning_index);
            Func<int, float, float> tune = (i, def) => tuning.ContainsKey(i) ? tuning[i].value : def;
            var lightsRate = tune(5, 0);
            var stallRate = tune(7, 0);
            var radioRate = tune(8, 0);
            if (lightsRate <= 0 && stallRate <= 0 && radioRate <= 0) return 0;

            var today = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalDays;
            var grace = Math.Max(0, (int)tune(3, 3));
            var pauseDays = Math.Max(1, (int)tune(6, 6));
            var oldest = db.LotBills.OldestOutstandingDay(lot_id);
            if (oldest != null && today - oldest.Value - grace >= pauseDays) return 0; //far overdue: accrual paused

            if (db.LotBills.HasPaidBillOnDay(lot_id, today)) return 0; //today's bill is settled; usage waits

            var projected = db.LotUsage.GetUnbilled(lot_id);
            if ((int)Math.Round(projected.light_hours * lightsRate + projected.stall_hours * stallRate + projected.radio_hours * radioRate) < 1) return 0;

            var actual = db.LotUsage.CollectUnbilled(lot_id);
            var charge = (int)Math.Round(actual.light_hours * lightsRate + actual.stall_hours * stallRate + actual.radio_hours * radioRate);
            if (charge < 1) return 0;
            return db.LotBills.AddToDay(lot_id, today, charge) ? charge : 0;
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

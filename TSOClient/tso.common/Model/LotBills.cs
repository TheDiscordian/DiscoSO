using System;

namespace FSO.Common.Model
{
    /// <summary>
    /// Metered usage accumulated by a lot, in game hours.
    /// </summary>
    public struct LotUsageHours
    {
        public double LightHours;
        public double StallHours;
        public double RadioHours;
        public double TvHours;
    }

    /// <summary>
    /// Storage the metered billing rules run against. The city server backs this with the
    /// database; sandbox backs it with the local standalone database.
    /// </summary>
    public interface ILotBillsStore
    {
        /// <summary>discoso_bills table 0 lookup, with the caller's default when unset.</summary>
        float Tuning(int index, float def);
        int? OldestOutstandingDay();
        /// <summary>Unbilled hours, left in place.</summary>
        LotUsageHours GetUnbilledUsage();
        /// <summary>Unbilled hours, marked as collected.</summary>
        LotUsageHours CollectUnbilledUsage();
        bool AddMeteredCharge(int day, int amount);
    }

    public static class LotBillsPolicy
    {
        public static int Today
        {
            get { return (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalDays; }
        }

        private static int Charge(LotUsageHours hours, float lights, float stalls, float radio, float tv)
        {
            return (int)Math.Round(hours.LightHours * lights + hours.StallHours * stalls
                + hours.RadioHours * radio + hours.TvHours * tv);
        }

        /// <summary>
        /// Fold all unbilled metered usage (lights, stalls, stereos, TVs) into today's bill. Returns the
        /// amount charged, or 0 if nothing was billed (rates unset, accrual paused, or the charge
        /// would round below $1 - fractions stay unbilled until they add up).
        /// </summary>
        public static int DeliverUsage(ILotBillsStore store)
        {
            var lightsRate = store.Tuning(5, 0);
            var stallRate = store.Tuning(7, 0);
            var radioRate = store.Tuning(8, 0);
            var tvRate = store.Tuning(9, 0);
            if (lightsRate <= 0 && stallRate <= 0 && radioRate <= 0 && tvRate <= 0) return 0;

            var today = Today;
            var grace = Math.Max(0, (int)store.Tuning(3, 3));
            var pauseDays = Math.Max(1, (int)store.Tuning(6, 6));
            var oldest = store.OldestOutstandingDay();
            if (oldest != null && today - oldest.Value - grace >= pauseDays) return 0; //far overdue: accrual paused

            var projected = store.GetUnbilledUsage();
            if (Charge(projected, lightsRate, stallRate, radioRate, tvRate) < 1) return 0;

            var actual = store.CollectUnbilledUsage();
            var charge = Charge(actual, lightsRate, stallRate, radioRate, tvRate);
            if (charge < 1) return 0;
            return store.AddMeteredCharge(today, charge) ? charge : 0;
        }
    }
}

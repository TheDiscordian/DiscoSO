using FSO.Server.Database.DA;
using FSO.Server.Database.DA.LotBills;
using FSO.Server.Database.DA.Tasks;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FSO.Server.Servers.Tasks.Domain
{
    /// <summary>
    /// DiscoSO daily property bills, metered by actual use. Each bill day a lot is charged
    /// per day it was actually visited: $rate x (size + 1), multiplied by 1 + mul per extra
    /// floor, plus a per-hour rate for time the lot was open (lights). Bills are ISSUED as
    /// outstanding (fso_lot_bills) and paid via the Budget Window; overdue lots escalate.
    /// Once bills are far enough overdue, accrual pauses until everything is paid off.
    /// Community lots are never billed.
    /// Tuning (discoso_bills table 0): 0 = $/active day per size step, 1 = per-extra-floor
    /// multiplier, 2 = bill period days, 3 = grace days, 4 = extra days to tier 2,
    /// 5 = lights $/hour open, 6 = overdue days past grace that pause accrual (and cut
    /// lights, once the VM side lands). Indices 0 and 5 both 0 = dormant.
    /// </summary>
    public class BillsTask : ITask
    {
        private static Logger LOG = LogManager.GetCurrentClassLogger();
        private IDAFactory DAFactory;
        private bool Running;

        public BillsTask(IDAFactory DAFactory)
        {
            this.DAFactory = DAFactory;
        }

        public void Run(TaskContext context)
        {
            Running = true;
            var epoch = new DateTime(1970, 1, 1);
            using (var db = DAFactory.Get())
            {
                var tuning = db.Tuning.AllCategory("discoso_bills", 0).ToDictionary(x => x.tuning_index);
                Func<int, float, float> tune = (i, def) => tuning.ContainsKey(i) ? tuning[i].value : def;
                var perDayPerSize = tune(0, 0);
                var floorMul = tune(1, 0.33f);
                var period = Math.Max(1, (int)tune(2, 1));
                var grace = Math.Max(0, (int)tune(3, 3));
                var pauseDays = Math.Max(1, (int)tune(6, 6));
                var lightsRate = tune(5, 0);
                if (perDayPerSize <= 0 && lightsRate <= 0)
                {
                    LOG.Info("Bills tuning not set (discoso_bills 0:0 per-day, 0:5 lights) - no bills issued.");
                    return;
                }

                var today = (int)(DateTime.UtcNow - epoch).TotalDays;
                var lots = db.Lots.GetBillableLots(context.ShardId ?? 1);
                int issued = 0, paused = 0; long total = 0;
                foreach (var lot in lots)
                {
                    if (!Running) break;

                    //far overdue: stop adding bills until everything is paid off
                    var oldest = db.LotBills.OldestOutstandingDay(lot.lot_id);
                    if (oldest != null && today - oldest.Value - grace >= pauseDays) { paused++; continue; }

                    var last = db.LotBills.LastBilledDay(lot.lot_id) ?? (today - period);
                    if (today - last < period) continue;

                    var windowStart = epoch.AddDays(last);
                    var windowEnd = epoch.AddDays(today);
                    var visits = db.LotVisits.GetVisitsBetween(lot.lot_id, windowStart, windowEnd);

                    //merge visit intervals (clamped to the window) for open-hours and active days
                    var merged = new List<Interval>();
                    foreach (var visit in visits.OrderBy(x => x.time_created))
                    {
                        var start = (visit.time_created < windowStart) ? windowStart : visit.time_created;
                        var end = (visit.time_closed.Value > windowEnd) ? windowEnd : visit.time_closed.Value;
                        if (end <= start) continue;
                        if (merged.Count > 0 && start <= merged[merged.Count - 1].End)
                        {
                            if (end > merged[merged.Count - 1].End) merged[merged.Count - 1].End = end;
                        }
                        else merged.Add(new Interval { Start = start, End = end });
                    }

                    var activeDays = new HashSet<int>();
                    double openHours = 0;
                    foreach (var iv in merged)
                    {
                        openHours += (iv.End - iv.Start).TotalHours;
                        for (var d = (int)(iv.Start - epoch).TotalDays; d <= (int)(iv.End.AddTicks(-1) - epoch).TotalDays; d++)
                            activeDays.Add(d);
                    }
                    if (activeDays.Count == 0 && openHours <= 0) continue; //unused lots owe nothing

                    var size = lot.size & 255;
                    var extraFloors = (lot.size >> 8) & 255;
                    var perDay = perDayPerSize * (size + 1) * (1f + floorMul * extraFloors);
                    var bill = (int)Math.Round(activeDays.Count * perDay + openHours * lightsRate);
                    if (bill <= 0) continue;

                    db.LotBills.Create(new DbLotBill { lot_id = lot.lot_id, amount = bill, billed_day = today });
                    issued++; total += bill;
                }
                LOG.Info("Bills: issued " + issued + " bill(s) totalling $" + total + (paused > 0 ? (", " + paused + " lot(s) paused for far-overdue bills.") : "."));
            }
        }

        private class Interval
        {
            public DateTime Start;
            public DateTime End;
        }

        public void Abort()
        {
            Running = false;
        }

        public DbTaskType GetTaskType()
        {
            return DbTaskType.bills;
        }
    }
}

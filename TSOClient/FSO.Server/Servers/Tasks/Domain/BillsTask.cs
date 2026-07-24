using FSO.Server.Database.DA;
using FSO.Server.Database.DA.LotBills;
using FSO.Server.Database.DA.Tasks;
using NLog;
using System;
using System.Linq;

namespace FSO.Server.Servers.Tasks.Domain
{
    /// <summary>
    /// DiscoSO daily property bills. On each lot's bill day a bill is ISSUED (not charged):
    /// it lands in fso_lot_bills as outstanding, is shown in the Budget Window, and is paid
    /// by a roommate (ledger code 108). Overdue bills escalate: past the grace period the
    /// lot stops admitting non-roommates; further overdue also blocks purchases and building.
    /// Tuning (discoso_bills table 0): 0 = flat base fee, 1 = per-mille of lot object value,
    /// 2 = bill period days (default 3), 3 = grace days (default 3), 4 = extra days to
    /// tier 2 (default 3). Base and per-mille both 0 = dormant, no bills issued.
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
            using (var db = DAFactory.Get())
            {
                var tuning = db.Tuning.AllCategory("discoso_bills", 0).ToDictionary(x => x.tuning_index);
                var baseFee = tuning.ContainsKey(0) ? (int)tuning[0].value : 0;
                var perMille = tuning.ContainsKey(1) ? tuning[1].value : 0f;
                var period = tuning.ContainsKey(2) ? Math.Max(1, (int)tuning[2].value) : 3;
                if (baseFee <= 0 && perMille <= 0)
                {
                    LOG.Info("Bills tuning not set (discoso_bills 0:0 flat, 0:1 per-mille) - no bills issued.");
                    return;
                }

                var today = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalDays;
                var lots = db.Lots.GetLotValueSummaries(context.ShardId ?? 1);
                int issued = 0; long total = 0;
                foreach (var lot in lots)
                {
                    if (!Running) break;
                    if (lot.owner_id == null) continue;
                    var last = db.LotBills.LastBilledDay(lot.lot_id);
                    if (last != null && today - last.Value < period) continue;

                    var bill = baseFee + (int)(lot.obj_value * perMille / 1000);
                    if (bill <= 0) continue;
                    db.LotBills.Create(new DbLotBill { lot_id = lot.lot_id, amount = bill, billed_day = today });
                    issued++; total += bill;
                }
                LOG.Info("Bills: issued " + issued + " bill(s) totalling $" + total + ".");
            }
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

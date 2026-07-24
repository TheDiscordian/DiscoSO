using FSO.Server.Database.DA;
using FSO.Server.Database.DA.Tasks;
using NLog;
using System;
using System.Linq;

namespace FSO.Server.Servers.Tasks.Domain
{
    /// <summary>
    /// DiscoSO daily property bills: each owned lot's owner is charged a flat base fee plus
    /// a per-mille of the lot's object value. Rates come from dynamic tuning type
    /// "discoso_bills" table 0 (index 0 = flat base, index 1 = per-mille); both default to
    /// 0, leaving the mechanic dormant until tuned. Charges go through the normal
    /// transaction path with ledger code 108, so they appear in the Budget Window.
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
                if (baseFee <= 0 && perMille <= 0)
                {
                    LOG.Info("Bills tuning not set (discoso_bills 0:0 flat, 0:1 per-mille) - no bills charged.");
                    return;
                }

                var lots = db.Lots.GetLotValueSummaries(context.ShardId ?? 1);
                int charged = 0; long total = 0;
                foreach (var lot in lots)
                {
                    if (!Running) break;
                    if (lot.owner_id == null) continue;
                    var bill = baseFee + (int)(lot.obj_value * perMille / 1000);
                    if (bill <= 0) continue;

                    //cap at what the owner can pay - budgets cannot go negative
                    var owner = db.Avatars.Get(lot.owner_id.Value);
                    if (owner == null) continue;
                    bill = Math.Min(bill, (int)Math.Max(0, owner.budget));
                    if (bill <= 0) continue;

                    var result = db.Avatars.Transaction(lot.owner_id.Value, uint.MaxValue, bill, 108);
                    if (result != null && result.success) { charged++; total += bill; }
                }
                LOG.Info("Bills: charged " + charged + " lot owner(s) a total of $" + total + ".");
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

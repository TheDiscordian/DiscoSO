using FSO.Server.Database.DA;
using FSO.Server.Database.DA.LotBills;
using FSO.Server.Database.DA.Tasks;
using NLog;

namespace FSO.Server.Servers.Tasks.Domain
{
    /// <summary>
    /// Folds banked metered usage (lit lamps, open stalls, playing stereos, on TVs) into the
    /// day's bill at game 7am, the same moment the paper carrier makes her round on a lot that
    /// happens to be loaded. Scheduled on the wall-clock times that game 7am maps to, because
    /// TSOTime derives game time from UTC with no per-lot state - so an offline lot is settled
    /// on the same clock as an online one, instead of the instant its owner walked out.
    ///
    /// Only lots carrying unbilled hours are visited. A lot that has been offline since its last
    /// settle has nothing left to collect, so this stays cheap however many lots exist.
    /// </summary>
    public class BillsMeteredTask : ITask
    {
        private static Logger LOG = LogManager.GetCurrentClassLogger();
        private IDAFactory DAFactory;
        private bool Running;

        public BillsMeteredTask(IDAFactory DAFactory)
        {
            this.DAFactory = DAFactory;
        }

        public void Run(TaskContext context)
        {
            Running = true;
            using (var db = DAFactory.Get())
            {
                var lots = db.LotUsage.GetLotsWithUnbilled(context.ShardId ?? 1);
                if (lots.Count == 0) return;

                int settled = 0; long total = 0;
                foreach (var lotId in lots)
                {
                    if (!Running) break;
                    try
                    {
                        var charge = LotBillsUtils.DeliverUsage(db, lotId);
                        if (charge > 0) { settled++; total += charge; }
                    }
                    catch (System.Exception e)
                    {
                        LOG.Warn(e, "metered settle failed for lot " + lotId);
                    }
                }
                LOG.Info("Metered bills: walked " + lots.Count + " lot(s) with unbilled usage, charged " + settled + " for $" + total + ".");
            }
        }

        public void Abort()
        {
            Running = false;
        }

        public DbTaskType GetTaskType()
        {
            return DbTaskType.bills_metered;
        }
    }
}

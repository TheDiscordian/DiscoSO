using Dapper;
using System.Linq;

namespace FSO.Server.Database.DA.LotUsage
{
    public class SqlLotUsage : AbstractSqlDA, ILotUsage
    {
        public SqlLotUsage(ISqlContext context) : base(context)
        {
        }

        public void AddUsage(int lot_id, int day, float lightHours, float stallHours)
        {
            Context.Connection.Execute(
                "INSERT INTO fso_lot_usage (lot_id, day, light_hours, stall_hours) VALUES (@lot_id, @day, @lightHours, @stallHours) "
                + "ON DUPLICATE KEY UPDATE light_hours = light_hours + @lightHours, stall_hours = stall_hours + @stallHours",
                new { lot_id, day, lightHours, stallHours });
        }

        public DbLotUsageTotal GetUsageBetween(int lot_id, int afterDay, int toDay)
        {
            return Context.Connection.Query<DbLotUsageTotal>(
                "SELECT COALESCE(SUM(light_hours), 0) AS light_hours, COALESCE(SUM(stall_hours), 0) AS stall_hours "
                + "FROM fso_lot_usage WHERE lot_id = @lot_id AND day > @afterDay AND day <= @toDay",
                new { lot_id, afterDay, toDay }).FirstOrDefault() ?? new DbLotUsageTotal();
        }

        public DbLotUsageTotal GetUnbilled(int lot_id)
        {
            return Context.Connection.Query<DbLotUsageTotal>(
                "SELECT COALESCE(SUM(GREATEST(light_hours - billed_light_hours, 0)), 0) AS light_hours, "
                + "COALESCE(SUM(GREATEST(stall_hours - billed_stall_hours, 0)), 0) AS stall_hours "
                + "FROM fso_lot_usage WHERE lot_id = @lot_id",
                new { lot_id }).FirstOrDefault() ?? new DbLotUsageTotal();
        }

        public DbLotUsageTotal CollectUnbilled(int lot_id)
        {
            var conn = Context.Connection;
            using (var txn = conn.BeginTransaction())
            {
                var rows = conn.Query<DbLotUsageDelta>(
                    "SELECT day, light_hours - billed_light_hours AS light_delta, stall_hours - billed_stall_hours AS stall_delta "
                    + "FROM fso_lot_usage WHERE lot_id = @lot_id "
                    + "AND (light_hours > billed_light_hours OR stall_hours > billed_stall_hours) FOR UPDATE",
                    new { lot_id }, txn).ToList();
                var total = new DbLotUsageTotal();
                foreach (var row in rows)
                {
                    var light = System.Math.Max(0, row.light_delta);
                    var stall = System.Math.Max(0, row.stall_delta);
                    conn.Execute(
                        "UPDATE fso_lot_usage SET billed_light_hours = billed_light_hours + @light, "
                        + "billed_stall_hours = billed_stall_hours + @stall WHERE lot_id = @lot_id AND day = @day",
                        new { light, stall, lot_id, row.day }, txn);
                    total.light_hours += light;
                    total.stall_hours += stall;
                }
                txn.Commit();
                return total;
            }
        }
    }
}

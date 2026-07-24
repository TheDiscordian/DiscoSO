using Dapper;
using System.Linq;

namespace FSO.Server.Database.DA.LotUsage
{
    public class SqlLotUsage : AbstractSqlDA, ILotUsage
    {
        public SqlLotUsage(ISqlContext context) : base(context)
        {
        }

        public void AddUsage(int lot_id, int day, float lightHours, float stallHours, float radioHours)
        {
            Context.Connection.Execute(
                "INSERT INTO fso_lot_usage (lot_id, day, light_hours, stall_hours, radio_hours) VALUES (@lot_id, @day, @lightHours, @stallHours, @radioHours) "
                + "ON DUPLICATE KEY UPDATE light_hours = light_hours + @lightHours, stall_hours = stall_hours + @stallHours, radio_hours = radio_hours + @radioHours",
                new { lot_id, day, lightHours, stallHours, radioHours });
        }

        public DbLotUsageTotal GetUsageBetween(int lot_id, int afterDay, int toDay)
        {
            return Context.Connection.Query<DbLotUsageTotal>(
                "SELECT COALESCE(SUM(light_hours), 0) AS light_hours, COALESCE(SUM(stall_hours), 0) AS stall_hours, COALESCE(SUM(radio_hours), 0) AS radio_hours "
                + "FROM fso_lot_usage WHERE lot_id = @lot_id AND day > @afterDay AND day <= @toDay",
                new { lot_id, afterDay, toDay }).FirstOrDefault() ?? new DbLotUsageTotal();
        }

        public DbLotUsageTotal GetUnbilled(int lot_id)
        {
            return Context.Connection.Query<DbLotUsageTotal>(
                "SELECT COALESCE(SUM(GREATEST(light_hours - billed_light_hours, 0)), 0) AS light_hours, "
                + "COALESCE(SUM(GREATEST(stall_hours - billed_stall_hours, 0)), 0) AS stall_hours, "
                + "COALESCE(SUM(GREATEST(radio_hours - billed_radio_hours, 0)), 0) AS radio_hours "
                + "FROM fso_lot_usage WHERE lot_id = @lot_id",
                new { lot_id }).FirstOrDefault() ?? new DbLotUsageTotal();
        }

        public DbLotUsageTotal CollectUnbilled(int lot_id)
        {
            var conn = Context.Connection;
            using (var txn = conn.BeginTransaction())
            {
                var rows = conn.Query<DbLotUsageDelta>(
                    "SELECT day, light_hours - billed_light_hours AS light_delta, stall_hours - billed_stall_hours AS stall_delta, "
                    + "radio_hours - billed_radio_hours AS radio_delta "
                    + "FROM fso_lot_usage WHERE lot_id = @lot_id "
                    + "AND (light_hours > billed_light_hours OR stall_hours > billed_stall_hours OR radio_hours > billed_radio_hours) FOR UPDATE",
                    new { lot_id }, txn).ToList();
                var total = new DbLotUsageTotal();
                foreach (var row in rows)
                {
                    var light = System.Math.Max(0, row.light_delta);
                    var stall = System.Math.Max(0, row.stall_delta);
                    var radio = System.Math.Max(0, row.radio_delta);
                    conn.Execute(
                        "UPDATE fso_lot_usage SET billed_light_hours = billed_light_hours + @light, "
                        + "billed_stall_hours = billed_stall_hours + @stall, "
                        + "billed_radio_hours = billed_radio_hours + @radio WHERE lot_id = @lot_id AND day = @day",
                        new { light, stall, radio, lot_id, row.day }, txn);
                    total.light_hours += light;
                    total.stall_hours += stall;
                    total.radio_hours += radio;
                }
                txn.Commit();
                return total;
            }
        }
    }
}

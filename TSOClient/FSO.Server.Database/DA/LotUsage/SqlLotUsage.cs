using Dapper;
using System.Collections.Generic;
using System.Linq;

namespace FSO.Server.Database.DA.LotUsage
{
    public class SqlLotUsage : AbstractSqlDA, ILotUsage
    {
        public SqlLotUsage(ISqlContext context) : base(context)
        {
        }

        public void AddUsage(int lot_id, int day, float lightHours, float stallHours, float radioHours, float tvHours)
        {
            Context.Connection.Execute(
                "INSERT INTO fso_lot_usage (lot_id, day, light_hours, stall_hours, radio_hours, tv_hours) VALUES (@lot_id, @day, @lightHours, @stallHours, @radioHours, @tvHours) "
                + "ON DUPLICATE KEY UPDATE light_hours = light_hours + @lightHours, stall_hours = stall_hours + @stallHours, radio_hours = radio_hours + @radioHours, tv_hours = tv_hours + @tvHours",
                new { lot_id, day, lightHours, stallHours, radioHours, tvHours });
        }

        public DbLotUsageTotal GetUsageBetween(int lot_id, int afterDay, int toDay)
        {
            return Context.Connection.Query<DbLotUsageTotal>(
                "SELECT COALESCE(SUM(light_hours), 0) AS light_hours, COALESCE(SUM(stall_hours), 0) AS stall_hours, COALESCE(SUM(radio_hours), 0) AS radio_hours, COALESCE(SUM(tv_hours), 0) AS tv_hours "
                + "FROM fso_lot_usage WHERE lot_id = @lot_id AND day > @afterDay AND day <= @toDay",
                new { lot_id, afterDay, toDay }).FirstOrDefault() ?? new DbLotUsageTotal();
        }

        public DbLotUsageTotal GetUnbilled(int lot_id)
        {
            return Context.Connection.Query<DbLotUsageTotal>(
                "SELECT COALESCE(SUM(GREATEST(light_hours - billed_light_hours, 0)), 0) AS light_hours, "
                + "COALESCE(SUM(GREATEST(stall_hours - billed_stall_hours, 0)), 0) AS stall_hours, "
                + "COALESCE(SUM(GREATEST(radio_hours - billed_radio_hours, 0)), 0) AS radio_hours, "
                + "COALESCE(SUM(GREATEST(tv_hours - billed_tv_hours, 0)), 0) AS tv_hours "
                + "FROM fso_lot_usage WHERE lot_id = @lot_id",
                new { lot_id }).FirstOrDefault() ?? new DbLotUsageTotal();
        }

        /// <summary>
        /// Lots with metered hours still waiting to be billed. The settle sweep only has to walk
        /// these - a lot that has been offline since its last settle has nothing left to collect,
        /// so the work stays proportional to what was actually played, not to the shard's size.
        /// </summary>
        public List<int> GetLotsWithUnbilled(int shard_id)
        {
            return Context.Connection.Query<int>(
                "SELECT u.lot_id FROM fso_lot_usage u JOIN fso_lots l ON l.lot_id = u.lot_id "
                + "WHERE l.shard_id = @shard_id AND l.category != 'community' "
                + "AND (u.light_hours > u.billed_light_hours OR u.stall_hours > u.billed_stall_hours "
                + "OR u.radio_hours > u.billed_radio_hours OR u.tv_hours > u.billed_tv_hours) "
                + "GROUP BY u.lot_id",
                new { shard_id }).ToList();
        }

        public DbLotUsageTotal CollectUnbilled(int lot_id)
        {
            var conn = Context.Connection;
            using (var txn = conn.BeginTransaction())
            {
                var rows = conn.Query<DbLotUsageDelta>(
                    "SELECT day, light_hours - billed_light_hours AS light_delta, stall_hours - billed_stall_hours AS stall_delta, "
                    + "radio_hours - billed_radio_hours AS radio_delta, tv_hours - billed_tv_hours AS tv_delta "
                    + "FROM fso_lot_usage WHERE lot_id = @lot_id "
                    + "AND (light_hours > billed_light_hours OR stall_hours > billed_stall_hours OR radio_hours > billed_radio_hours OR tv_hours > billed_tv_hours) FOR UPDATE",
                    new { lot_id }, txn).ToList();
                var total = new DbLotUsageTotal();
                foreach (var row in rows)
                {
                    var light = System.Math.Max(0, row.light_delta);
                    var stall = System.Math.Max(0, row.stall_delta);
                    var radio = System.Math.Max(0, row.radio_delta);
                    var tv = System.Math.Max(0, row.tv_delta);
                    conn.Execute(
                        "UPDATE fso_lot_usage SET billed_light_hours = billed_light_hours + @light, "
                        + "billed_stall_hours = billed_stall_hours + @stall, "
                        + "billed_radio_hours = billed_radio_hours + @radio, "
                        + "billed_tv_hours = billed_tv_hours + @tv WHERE lot_id = @lot_id AND day = @day",
                        new { light, stall, radio, tv, lot_id, row.day }, txn);
                    total.light_hours += light;
                    total.stall_hours += stall;
                    total.radio_hours += radio;
                    total.tv_hours += tv;
                }
                txn.Commit();
                return total;
            }
        }
    }
}

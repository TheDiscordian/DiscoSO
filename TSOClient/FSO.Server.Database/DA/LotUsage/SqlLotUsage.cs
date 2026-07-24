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
    }
}

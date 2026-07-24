using Dapper;
using System.Collections.Generic;
using System.Linq;

namespace FSO.Server.Database.DA.Transactions
{
    public class SqlTransactions : AbstractSqlDA, ITransactions
    {
        public SqlTransactions(ISqlContext context) : base(context)
        {
        }

        public void Purge(int day)
        {
            Context.Connection.Query("DELETE FROM fso_transactions WHERE day < @day", new { day = day });
        }

        public List<DbTransactionSummary> GetSummary(uint avatar_id, int minDay)
        {
            return Context.Connection.Query<DbTransactionSummary>(
                "SELECT transaction_type, day, "
                + "SUM(CASE WHEN to_id = @avatar_id THEN value ELSE 0 END) AS income, "
                + "SUM(CASE WHEN from_id = @avatar_id THEN value ELSE 0 END) AS expense "
                + "FROM fso_transactions "
                + "WHERE (from_id = @avatar_id OR to_id = @avatar_id) AND day >= @minDay "
                + "GROUP BY transaction_type, day",
                new { avatar_id = avatar_id, minDay = minDay }).ToList();
        }
    }

    public class DbTransactionSummary
    {
        public uint transaction_type { get; set; }
        public uint day { get; set; }
        public ulong income { get; set; }
        public ulong expense { get; set; }
    }
}

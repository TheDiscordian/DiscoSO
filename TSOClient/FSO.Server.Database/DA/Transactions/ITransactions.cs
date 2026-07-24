using System.Collections.Generic;

namespace FSO.Server.Database.DA.Transactions
{
    public interface ITransactions
    {
        void Purge(int day);
        List<DbTransactionSummary> GetSummary(uint avatar_id, int minDay);
    }
}

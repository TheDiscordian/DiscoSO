using System.Collections.Generic;

namespace FSO.Server.Database.DA.LotBills
{
    public interface ILotBills
    {
        void Create(DbLotBill bill);
        bool AddToDay(int lot_id, int day, int amount);
        bool HasPaidBillOnDay(int lot_id, int day);
        List<DbLotBill> GetOutstanding(int lot_id);
        List<DbLotBill> GetOutstandingForAvatarLots(uint avatar_id);
        int? LastBilledDay(int lot_id);
        int? OldestOutstandingDay(int lot_id);
        int MarkPaid(IEnumerable<int> bill_ids, uint avatar_id, int day);
    }
}

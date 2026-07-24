using Dapper;
using System.Collections.Generic;
using System.Linq;

namespace FSO.Server.Database.DA.LotBills
{
    public class SqlLotBills : AbstractSqlDA, ILotBills
    {
        public SqlLotBills(ISqlContext context) : base(context)
        {
        }

        public void Create(DbLotBill bill)
        {
            Context.Connection.Execute(
                "INSERT INTO fso_lot_bills (lot_id, amount, billed_day) VALUES (@lot_id, @amount, @billed_day) "
                + "ON DUPLICATE KEY UPDATE lot_id = lot_id",
                new { bill.lot_id, bill.amount, bill.billed_day });
        }

        public List<DbLotBill> GetOutstanding(int lot_id)
        {
            return Context.Connection.Query<DbLotBill>(
                "SELECT * FROM fso_lot_bills WHERE lot_id = @lot_id AND paid_day IS NULL ORDER BY billed_day",
                new { lot_id = lot_id }).ToList();
        }

        public List<DbLotBill> GetOutstandingForAvatarLots(uint avatar_id)
        {
            return Context.Connection.Query<DbLotBill>(
                "SELECT b.* FROM fso_lot_bills b JOIN fso_roommates r ON r.lot_id = b.lot_id "
                + "WHERE r.avatar_id = @avatar_id AND r.is_pending = 0 AND b.paid_day IS NULL ORDER BY b.billed_day",
                new { avatar_id = avatar_id }).ToList();
        }

        public int? LastBilledDay(int lot_id)
        {
            return Context.Connection.Query<int?>(
                "SELECT MAX(billed_day) FROM fso_lot_bills WHERE lot_id = @lot_id",
                new { lot_id = lot_id }).FirstOrDefault();
        }

        public int? OldestOutstandingDay(int lot_id)
        {
            return Context.Connection.Query<int?>(
                "SELECT MIN(billed_day) FROM fso_lot_bills WHERE lot_id = @lot_id AND paid_day IS NULL",
                new { lot_id = lot_id }).FirstOrDefault();
        }

        public int MarkPaid(IEnumerable<int> bill_ids, uint avatar_id, int day)
        {
            return Context.Connection.Execute(
                "UPDATE fso_lot_bills SET paid_day = @day, paid_by = @avatar_id WHERE bill_id IN @ids AND paid_day IS NULL",
                new { ids = bill_ids, avatar_id = avatar_id, day = day });
        }
    }
}

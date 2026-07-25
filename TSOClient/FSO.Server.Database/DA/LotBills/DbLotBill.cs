namespace FSO.Server.Database.DA.LotBills
{
    public class DbLotBill
    {
        public int bill_id { get; set; }
        public int lot_id { get; set; }
        public int amount { get; set; }
        public string kind { get; set; }
        public int billed_day { get; set; }
        public int? paid_day { get; set; }
        public uint? paid_by { get; set; }
    }
}

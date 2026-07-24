namespace FSO.Server.Database.DA.LotUsage
{
    public interface ILotUsage
    {
        void AddUsage(int lot_id, int day, float lightHours, float stallHours);
        DbLotUsageTotal GetUsageBetween(int lot_id, int afterDay, int toDay);
        DbLotUsageTotal GetUnbilled(int lot_id);
        DbLotUsageTotal CollectUnbilled(int lot_id);
    }

    public class DbLotUsageTotal
    {
        public double light_hours { get; set; }
        public double stall_hours { get; set; }
    }

    public class DbLotUsageDelta
    {
        public int day { get; set; }
        public double light_delta { get; set; }
        public double stall_delta { get; set; }
    }
}

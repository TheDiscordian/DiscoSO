namespace FSO.Server.Database.DA.LotUsage
{
    public interface ILotUsage
    {
        void AddUsage(int lot_id, int day, float lightHours, float stallHours);
        DbLotUsageTotal GetUsageBetween(int lot_id, int afterDay, int toDay);
    }

    public class DbLotUsageTotal
    {
        public double light_hours { get; set; }
        public double stall_hours { get; set; }
    }
}

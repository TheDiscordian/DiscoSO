namespace FSO.Server.Database.DA.Objects
{
    public class DbObjectNetWorth
    {
        public ulong money { get; set; } //sum of money stored in owned objects
        public ulong value { get; set; } //sum of owned objects' resale value
    }
}

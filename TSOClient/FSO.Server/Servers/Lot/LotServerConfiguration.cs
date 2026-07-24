using FSO.Server.Framework.Aries;

namespace FSO.Server.Servers.Lot
{
    public class LotServerConfiguration : AbstractAriesServerConfig
    {
        public int Max_Lots = 1;

        public string SimNFS;
        public int RingBufferSize = 10;
        public int Clock_Ticks_Per_Minute = 0; //0 = engine default (150: 1 game min per 5 real s); staging sets 30 for 5x faster testing
        public bool Timeout_No_Auth = true;
        public bool LogJobLots = false;

        //Which cities to provide lot hosting for
        public LotServerConfigurationCity[] Cities;

        //How often to reconnect lost connections to city servers and report capacity
        public int CityReportingInterval = 10000;
    }

    public class LotServerConfigurationCity
    {
        public int ID;
        public string Host;
    }
}

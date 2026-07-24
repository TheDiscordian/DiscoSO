using System.Collections.Generic;

namespace FSO.Server.Database.DA.LotEvents
{
    public interface ILotEvents
    {
        void Create(DbLotEvent evt);
        List<DbLotEvent> GetRecent(int lot_id, int limit);
    }
}

using Dapper;
using System.Collections.Generic;
using System.Linq;

namespace FSO.Server.Database.DA.LotEvents
{
    public class SqlLotEvents : AbstractSqlDA, ILotEvents
    {
        public SqlLotEvents(ISqlContext context) : base(context)
        {
        }

        public void Create(DbLotEvent evt)
        {
            Context.Connection.Execute(
                "INSERT INTO fso_lot_events (lot_id, avatar_id, target_avatar_id, type, value, data) "
                + "VALUES (@lot_id, @avatar_id, @target_avatar_id, @type, @value, @data)",
                new { evt.lot_id, evt.avatar_id, evt.target_avatar_id, type = (byte)evt.type, evt.value, evt.data });
        }

        public List<DbLotEvent> GetRecent(int lot_id, int limit)
        {
            return Context.Connection.Query<DbLotEvent>(
                "SELECT e.*, a.name AS actor_name, t.name AS target_name FROM fso_lot_events e "
                + "LEFT JOIN fso_avatars a ON a.avatar_id = e.avatar_id "
                + "LEFT JOIN fso_avatars t ON t.avatar_id = e.target_avatar_id "
                + "WHERE e.lot_id = @lot_id ORDER BY e.time DESC, e.event_id DESC LIMIT @limit",
                new { lot_id = lot_id, limit = limit }).ToList();
        }
    }
}

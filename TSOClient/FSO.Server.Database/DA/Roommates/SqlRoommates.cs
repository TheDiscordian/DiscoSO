using Dapper;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace FSO.Server.Database.DA.Roommates
{
    public class SqlRoommates : AbstractSqlDA, IRoommates
    {
        public SqlRoommates(ISqlContext context) : base(context)
        {
        }

        public bool Create(DbRoommate roomie)
        {
            try
            {
                return (uint)Context.Connection.Execute("INSERT INTO fso_roommates (avatar_id, lot_id, permissions_level, is_pending) " +
                                " VALUES (@avatar_id, @lot_id, @permissions_level, @is_pending);", roomie) > 0;
            } catch (SqlException)
            {
                return false;
            }
        }

        public bool CreateOrUpdate(DbRoommate roomie)
        {
            try
            {
                return (uint)Context.Connection.Execute("INSERT INTO fso_roommates (avatar_id, lot_id, permissions_level, is_pending) " +
                                "VALUES (@avatar_id, @lot_id, @permissions_level, @is_pending) " +
                                "ON DUPLICATE KEY UPDATE permissions_level = @permissions_level, is_pending = 0", roomie) > 0;
            }
            catch (SqlException)
            {
                return false;
            }
        }

        public DbRoommate Get(uint avatar_id, int lot_id)
        {
            return Context.Connection.Query<DbRoommate>("SELECT * FROM fso_roommates WHERE avatar_id = @avatar_id AND lot_id = @lot_id", 
                new { avatar_id = avatar_id, lot_id = lot_id }).FirstOrDefault();
        }
        public List<DbRoommate> GetAvatarsLots(uint avatar_id)
        {
            return Context.Connection.Query<DbRoommate>("SELECT * FROM fso_roommates WHERE avatar_id = @avatar_id",
                new { avatar_id = avatar_id }).ToList();
        }
        public List<DbRoommate> GetLotRoommates(int lot_id)
        {
            return Context.Connection.Query<DbRoommate>("SELECT * FROM fso_roommates WHERE lot_id = @lot_id",
                new { lot_id = lot_id }).ToList();
        }
        public uint RemoveRoommate(uint avatar_id, int lot_id)
        {
            return (uint)Context.Connection.Execute("DELETE FROM fso_roommates WHERE avatar_id = @avatar_id AND lot_id = @lot_id",
                new { avatar_id = avatar_id, lot_id = lot_id });
        }

        public bool DeclineRoommateRequest(uint avatar_id, int lot_id)
        {
            return Context.Connection.Execute("DELETE FROM fso_roommates WHERE avatar_id = @avatar_id AND lot_id = @lot_id AND is_pending = 1",
                new { avatar_id = avatar_id, lot_id = lot_id }) > 0;
        }
        public bool AcceptRoommateRequest(uint avatar_id, int lot_id)
        {
            return Context.Connection.Execute("UPDATE fso_roommates SET is_pending = 0 WHERE avatar_id = @avatar_id AND lot_id = @lot_id AND is_pending = 1", 
                new { avatar_id = avatar_id, lot_id = lot_id }) > 0;
        }
        public bool UpdatePermissionsLevel(uint avatar_id, int lot_id, byte level)
        {
            return Context.Connection.Execute("UPDATE fso_roommates SET permissions_level = @level WHERE avatar_id = @avatar_id AND lot_id = @lot_id",
                new { level = level, avatar_id = avatar_id, lot_id = lot_id }) > 0;
        }
    
        public List<DbRoommateInfo> GetLotRoommatesWithInfo(int lot_id)
        {
            //"recent activity": the roommate's latest visit to this lot, falling back to their move date
            return Context.Connection.Query<DbRoommateInfo>(
                "SELECT a.name, COALESCE(UNIX_TIMESTAMP(MAX(v.time_created)), a.move_date) AS last_active, r.permissions_level "
                + "FROM fso_roommates r "
                + "JOIN fso_avatars a ON a.avatar_id = r.avatar_id "
                + "LEFT JOIN fso_lot_visits v ON v.avatar_id = r.avatar_id AND v.lot_id = r.lot_id "
                + "WHERE r.lot_id = @lot_id AND r.is_pending = 0 "
                + "GROUP BY r.avatar_id, a.name, a.move_date, r.permissions_level "
                + "ORDER BY last_active DESC",
                new { lot_id = lot_id }).ToList();
        }
    }

    public class DbRoommateInfo
    {
        public string name { get; set; }
        public uint last_active { get; set; }
        public byte permissions_level { get; set; }
    }
}

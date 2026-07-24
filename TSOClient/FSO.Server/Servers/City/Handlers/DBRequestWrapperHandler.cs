using FSO.Common.DatabaseService.Model;
using FSO.Common.Serialization.Primitives;
using FSO.Server.Database.DA;
using FSO.Server.Domain;
using FSO.Server.Framework.Voltron;
using FSO.Server.Protocol.Voltron.Packets;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FSO.Server.Servers.City.Handlers
{
    public class DBRequestWrapperHandler
    {
        private IDAFactory DAFactory;
        private CityServerContext Context;
        private ServerTop100Domain Top100;

        public DBRequestWrapperHandler(CityServerContext context, IDAFactory da, ServerTop100Domain Top100)
        {
            this.DAFactory = da;
            this.Context = context;
            this.Top100 = Top100;
        }

        public void Handle(IVoltronSession session, DBRequestWrapperPDU packet)
        {
            if(packet.Body is cTSONetMessageStandard)
            {
                HandleNetMessage(session, (cTSONetMessageStandard)packet.Body, packet);
            }
        }

        private void HandleNetMessage(IVoltronSession session, cTSONetMessageStandard msg, DBRequestWrapperPDU packet)
        {
            if (!msg.DatabaseType.HasValue) { return; }
            var requestType = DBRequestTypeUtils.FromRequestID(msg.DatabaseType.Value);

            object response = null;

            switch (requestType)
            {
                case DBRequestType.LoadAvatarByID:
                    response = HandleLoadAvatarById(session, msg);
                    break;

                case DBRequestType.SearchExactMatch:
                    response = HandleSearchExact(session, msg);
                    break;

                case DBRequestType.Search:
                    response = HandleSearchWildcard(session, msg);
                    break;

                case DBRequestType.GetTopResultSetByID:
                    response = HandleGetTop100(session, msg);
                    break;

                case DBRequestType.GetDataServiceAvatarBudgetByID:
                    response = HandleGetBudget(session, msg);
                    break;

                case DBRequestType.GetLotList: //DiscoSO: house panel activity log
                    response = HandleGetLotLog(session, msg);
                    break;
            }

            if(response != null){
                session.Write(new DBRequestWrapperPDU {
                    SendingAvatarID = packet.SendingAvatarID,
                    Badge = packet.Badge,
                    IsAlertable = packet.IsAlertable,
                    Sender = packet.Sender,
                    Body = response
                });
            }
        }

        private object HandleGetTop100(IVoltronSession session, cTSONetMessageStandard msg)
        {
            var request = msg.ComplexParameter as GetTop100Request;
            if (request == null) { return null; }

            var results = Top100.Query(request.Category);

            return new cTSONetMessageStandard()
            {
                MessageID = 0x69AC83C4,
                DatabaseType = DBResponseType.GetTopResultSetByID.GetResponseID(),
                Parameter = msg.Parameter,

                ComplexParameter = new GetTop100Response()
                {
                    Items = results
                }
            };
        }

        private object HandleLoadAvatarById(IVoltronSession session, cTSONetMessageStandard msg)
        {
            var request = msg.ComplexParameter as LoadAvatarByIDRequest;
            if (request == null) { return null; }

            if(request.AvatarId != session.AvatarId){
                throw new Exception("Permission denied, you cannot load an avatar you do not own");
            }

            using (var da = DAFactory.Get())
            {
                var avatar = da.Avatars.Get(session.AvatarId);
                if (avatar == null) return null;

                var bonus = da.Bonus.GetByAvatarId(avatar.avatar_id);

                return new cTSONetMessageStandard()
                {
                    MessageID = 0x8ADF865D,
                    DatabaseType = DBResponseType.LoadAvatarByID.GetResponseID(),
                    Parameter = msg.Parameter,

                    ComplexParameter = new LoadAvatarByIDResponse()
                    {
                        AvatarId = session.AvatarId,
                        Cash = (uint)avatar.budget,
                        Bonus = bonus.Select(x =>
                        {
                            return new LoadAvatarBonus() {
                                PropertyBonus = x.bonus_property == null ? (uint)0 : (uint)x.bonus_property.Value,
                                SimBonus = x.bonus_sim == null ? (uint)0 : (uint)x.bonus_sim.Value,
                                VisitorBonus = x.bonus_visitor == null ? (uint)0 : (uint)x.bonus_visitor,
                                Date = x.period.ToShortDateString()
                            };
                        }).ToList()
                    }
                };
            };
        }


        private object HandleGetBudget(IVoltronSession session, cTSONetMessageStandard msg)
        {
            var request = msg.ComplexParameter as GetAvatarBudgetRequest;
            if (request == null) { return null; }

            if (request.AvatarId != session.AvatarId)
            {
                throw new Exception("Permission denied, you cannot view the budget of an avatar you do not own");
            }

            using (var da = DAFactory.Get())
            {
                var avatar = da.Avatars.Get(session.AvatarId);
                if (avatar == null) return null;

                var netWorth = da.Objects.GetNetWorth(session.AvatarId);
                var today = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalDays;
                var summary = da.Transactions.GetSummary(session.AvatarId, today - 30);

                var days = summary.GroupBy(x => x.day).Select(g => new BudgetDaySummary
                {
                    Day = g.Key,
                    Income = (uint)Math.Min(uint.MaxValue, g.Aggregate(0ul, (acc, x) => acc + x.income)),
                    Expense = (uint)Math.Min(uint.MaxValue, g.Aggregate(0ul, (acc, x) => acc + x.expense))
                }).OrderByDescending(x => x.Day).ToList();

                var billsDays = summary.Where(x => x.transaction_type == 108).Select(x => new BudgetDaySummary
                {
                    Day = x.day,
                    Income = (uint)Math.Min(uint.MaxValue, x.income),
                    Expense = (uint)Math.Min(uint.MaxValue, x.expense)
                }).OrderByDescending(x => x.Day).ToList();

                var categories = summary.GroupBy(x => x.transaction_type).Select(g => new BudgetCategorySummary
                {
                    TransactionType = (short)g.Key,
                    Income = (uint)Math.Min(uint.MaxValue, g.Aggregate(0ul, (acc, x) => acc + x.income)),
                    Expense = (uint)Math.Min(uint.MaxValue, g.Aggregate(0ul, (acc, x) => acc + x.expense))
                }).OrderByDescending(x => x.Income + x.Expense).ToList();

                return new cTSONetMessageStandard()
                {
                    MessageID = 0x4AA84819,
                    DatabaseType = DBResponseType.GetDataServiceAvatarBudgetByID.GetResponseID(),
                    Parameter = msg.Parameter,

                    ComplexParameter = new GetAvatarBudgetResponse()
                    {
                        AvatarId = session.AvatarId,
                        Cash = (uint)avatar.budget,
                        ObjectsMoney = (uint)Math.Min(uint.MaxValue, netWorth.money),
                        ObjectsValue = (uint)Math.Min(uint.MaxValue, netWorth.value),
                        Days = days,
                        Categories = categories,
                        BillsDays = billsDays,
                        BillsEnabled = da.Tuning.AllCategory("discoso_bills", 0).Any(x => x.value > 0)
                    }
                };
            }
        }

        private object HandleGetLotLog(IVoltronSession session, cTSONetMessageStandard msg)
        {
            var request = msg.ComplexParameter as GetLotLogRequest;
            if (request == null) { return null; }

            using (var da = DAFactory.Get())
            {
                var lot = da.Lots.GetByLocation(Context.ShardId, request.Location);
                if (lot == null) return null;

                //the log is for roommates (and moderators) only
                var roommates = da.Roommates.GetLotRoommates(lot.lot_id);
                if (!roommates.Any(r => r.avatar_id == session.AvatarId && r.is_pending == 0))
                {
                    var avatar = da.Avatars.Get(session.AvatarId);
                    if (avatar == null || avatar.moderation_level == 0) return null;
                }

                var epoch = new DateTime(1970, 1, 1);
                var visits = da.LotVisits.GetRecentVisits(lot.lot_id, 50).Select(x => new LotLogEntry
                {
                    Name = x.name,
                    Time = (uint)(x.time_created - epoch).TotalSeconds,
                    Type = (byte)x.type
                }).ToList();

                var roomies = da.Roommates.GetLotRoommatesWithInfo(lot.lot_id).Select(x => new LotLogEntry
                {
                    Name = x.name,
                    Time = x.last_active,
                    Type = x.permissions_level
                }).ToList();

                var events = da.LotEvents.GetRecent(lot.lot_id, 30).Select(x => new LotLogEvent
                {
                    Title = FormatLotEvent(x),
                    Description = "",
                    StartTime = (uint)(x.time - epoch).TotalSeconds,
                    EndTime = (uint)(x.time - epoch).TotalSeconds
                }).ToList();

                return new cTSONetMessageStandard()
                {
                    MessageID = 0x4AA84720,
                    DatabaseType = DBResponseType.GetLotList.GetResponseID(),
                    Parameter = msg.Parameter,

                    ComplexParameter = new GetLotLogResponse()
                    {
                        Visitors = visits,
                        Roommates = roomies,
                        Events = events
                    }
                };
            }
        }

        private static string FormatLotEvent(FSO.Server.Database.DA.LotEvents.DbLotEvent evt)
        {
            var actor = evt.actor_name ?? "A roommate";
            var target = evt.target_name ?? "someone";
            switch (evt.type)
            {
                case FSO.Server.Database.DA.LotEvents.DbLotEventType.admit_add: return actor + " admitted " + target;
                case FSO.Server.Database.DA.LotEvents.DbLotEventType.admit_remove: return actor + " un-admitted " + target;
                case FSO.Server.Database.DA.LotEvents.DbLotEventType.ban_add: return actor + " banned " + target;
                case FSO.Server.Database.DA.LotEvents.DbLotEventType.ban_remove: return actor + " unbanned " + target;
                case FSO.Server.Database.DA.LotEvents.DbLotEventType.admit_mode:
                    switch (evt.value)
                    {
                        case 0: return actor + " opened the lot to everyone";
                        case 1: return actor + " set admit list only";
                        case 2: return actor + " set ban list mode";
                        default: return actor + " closed the lot";
                    }
                case FSO.Server.Database.DA.LotEvents.DbLotEventType.lot_expanded: return actor + " expanded the lot ($" + evt.value + ")";
                case FSO.Server.Database.DA.LotEvents.DbLotEventType.category: return actor + " changed the category to " + ((FSO.Common.Enum.LotCategory)evt.value).ToString();
                case FSO.Server.Database.DA.LotEvents.DbLotEventType.renamed: return actor + " renamed the lot to " + (evt.data ?? "?");
                case FSO.Server.Database.DA.LotEvents.DbLotEventType.description: return actor + " updated the description";
                default: return actor + " did something";
            }
        }

        private object HandleSearchExact(IVoltronSession session, cTSONetMessageStandard msg)
        {
            var request = msg.ComplexParameter as SearchRequest;
            if (request == null) { return null; }

            using (var db = DAFactory.Get())
            {
                List<SearchResponseItem> results = null;

                if (request.Type == SearchType.SIMS)
                {
                    results = db.Avatars.SearchExact(Context.ShardId, request.Query, 100).Select(x => new SearchResponseItem
                    {
                        Name = x.name,
                        EntityId = x.avatar_id
                    }).ToList();
                }
                else if (request.Type == SearchType.NHOOD)
                {
                    results = db.Neighborhoods.SearchExact(Context.ShardId, request.Query, 100).Select(x => new SearchResponseItem
                    {
                        Name = x.name,
                        EntityId = (uint)x.neighborhood_id
                    }).ToList();
                }
                else
                {
                    results = db.Lots.SearchExact(Context.ShardId, request.Query, 100).Select(x => new SearchResponseItem
                    {
                        Name = x.name,
                        EntityId = x.location
                    }).ToList();
                }

                return new cTSONetMessageStandard()
                {
                    MessageID = 0xDBF301A9,
                    DatabaseType = DBResponseType.SearchExactMatch.GetResponseID(),
                    Parameter = msg.Parameter,

                    ComplexParameter = new SearchResponse()
                    {
                        Query = request.Query,
                        Type = request.Type,
                        Items = results
                    }
                };
            }
        }

        private object HandleSearchWildcard(IVoltronSession session, cTSONetMessageStandard msg)
        {
            var request = msg.ComplexParameter as SearchRequest;
            if (request == null) { return null; }

            using (var db = DAFactory.Get())
            {
                List<SearchResponseItem> results = null;

                if (request.Type == SearchType.SIMS)
                {
                    results = db.Avatars.SearchWildcard(Context.ShardId, request.Query, 100).Select(x => new SearchResponseItem
                    {
                        Name = x.name,
                        EntityId = x.avatar_id
                    }).ToList();
                }
                else if (request.Type == SearchType.NHOOD)
                {
                    results = db.Neighborhoods.SearchWildcard(Context.ShardId, request.Query, 100).Select(x => new SearchResponseItem
                    {
                        Name = x.name,
                        EntityId = (uint)x.neighborhood_id
                    }).ToList();
                }
                else
                {
                    results = db.Lots.SearchWildcard(Context.ShardId, request.Query, 100).Select(x => new SearchResponseItem
                    {
                        Name = x.name,
                        EntityId = x.location
                    }).ToList();
                }

                return new cTSONetMessageStandard()
                {
                    MessageID = 0xDBF301A9,
                    DatabaseType = DBResponseType.Search.GetResponseID(),
                    Parameter = msg.Parameter,

                    ComplexParameter = new SearchResponse()
                    {
                        Query = request.Query,
                        Type = request.Type,
                        Items = results
                    }
                };
            }
        }
    }
}

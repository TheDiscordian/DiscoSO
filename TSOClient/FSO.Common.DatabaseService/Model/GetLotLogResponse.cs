using FSO.Common.Serialization;
using System.Collections.Generic;
using Mina.Core.Buffer;
using FSO.Common.DatabaseService.Framework;

namespace FSO.Common.DatabaseService.Model
{
    [DatabaseResponse(DBResponseType.GetLotList)]
    public class GetLotLogResponse : IoBufferSerializable, IoBufferDeserializable
    {
        public List<LotLogEntry> Visitors = new List<LotLogEntry>();
        public List<LotLogEntry> Roommates = new List<LotLogEntry>();
        public List<LotLogEvent> Events = new List<LotLogEvent>();

        public void Serialize(IoBuffer output, ISerializationContext context)
        {
            PutEntries(output, Visitors);
            PutEntries(output, Roommates);
            output.PutUInt32((uint)Events.Count);
            foreach (var evt in Events)
            {
                output.PutPascalVLCString(evt.Title);
                output.PutPascalVLCString(evt.Description);
                output.PutUInt32(evt.StartTime);
                output.PutUInt32(evt.EndTime);
            }
        }

        public void Deserialize(IoBuffer input, ISerializationContext context)
        {
            Visitors = GetEntries(input);
            Roommates = GetEntries(input);
            var count = input.GetUInt32();
            Events = new List<LotLogEvent>((int)count);
            for (var i = 0; i < count; i++)
            {
                Events.Add(new LotLogEvent
                {
                    Title = input.GetPascalVLCString(),
                    Description = input.GetPascalVLCString(),
                    StartTime = input.GetUInt32(),
                    EndTime = input.GetUInt32()
                });
            }
        }

        private static void PutEntries(IoBuffer output, List<LotLogEntry> entries)
        {
            output.PutUInt32((uint)entries.Count);
            foreach (var entry in entries)
            {
                output.PutPascalVLCString(entry.Name);
                output.PutUInt32(entry.Time);
                output.Put(entry.Type);
            }
        }

        private static List<LotLogEntry> GetEntries(IoBuffer input)
        {
            var count = input.GetUInt32();
            var list = new List<LotLogEntry>((int)count);
            for (var i = 0; i < count; i++)
            {
                list.Add(new LotLogEntry
                {
                    Name = input.GetPascalVLCString(),
                    Time = input.GetUInt32(),
                    Type = input.Get()
                });
            }
            return list;
        }
    }

    public class LotLogEntry
    {
        public string Name;
        public uint Time; //unix seconds
        public byte Type; //visits: DbLotVisitorType; roommates: permissions_level
    }

    public class LotLogEvent
    {
        public string Title;
        public string Description;
        public uint StartTime; //unix seconds
        public uint EndTime;
    }
}

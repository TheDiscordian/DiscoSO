using FSO.Common.Serialization;
using Mina.Core.Buffer;
using FSO.Common.DatabaseService.Framework;

namespace FSO.Common.DatabaseService.Model
{
    //bound to the unused GetLotList protocol slot - DiscoSO uses it for the house panel activity log
    [DatabaseRequest(DBRequestType.GetLotList)]
    public class GetLotLogRequest : IoBufferSerializable, IoBufferDeserializable
    {
        public uint Location; //the lot's map coordinate, as known by the client

        public void Deserialize(IoBuffer input, ISerializationContext context)
        {
            Location = input.GetUInt32();
        }

        public void Serialize(IoBuffer output, ISerializationContext context)
        {
            output.PutUInt32(Location);
        }
    }
}

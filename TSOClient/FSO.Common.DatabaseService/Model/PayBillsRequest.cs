using FSO.Common.Serialization;
using Mina.Core.Buffer;
using FSO.Common.DatabaseService.Framework;

namespace FSO.Common.DatabaseService.Model
{
    //bound to the unused DebitCredit protocol slot - DiscoSO uses it to pay outstanding property bills
    [DatabaseRequest(DBRequestType.DebitCredit)]
    public class PayBillsRequest : IoBufferSerializable, IoBufferDeserializable
    {
        public uint AvatarId;

        public void Deserialize(IoBuffer input, ISerializationContext context)
        {
            AvatarId = input.GetUInt32();
        }

        public void Serialize(IoBuffer output, ISerializationContext context)
        {
            output.PutUInt32(AvatarId);
        }
    }
}

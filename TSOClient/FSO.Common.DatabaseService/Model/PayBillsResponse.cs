using FSO.Common.Serialization;
using Mina.Core.Buffer;
using FSO.Common.DatabaseService.Framework;

namespace FSO.Common.DatabaseService.Model
{
    [DatabaseResponse(DBResponseType.DebitCredit)]
    public class PayBillsResponse : IoBufferSerializable, IoBufferDeserializable
    {
        public bool Success;
        public uint AmountPaid;

        public void Deserialize(IoBuffer input, ISerializationContext context)
        {
            Success = input.GetBool();
            AmountPaid = input.GetUInt32();
        }

        public void Serialize(IoBuffer output, ISerializationContext context)
        {
            output.PutBool(Success);
            output.PutUInt32(AmountPaid);
        }
    }
}

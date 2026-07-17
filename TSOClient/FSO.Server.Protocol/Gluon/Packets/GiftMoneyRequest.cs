using FSO.Common.Serialization;
using Mina.Core.Buffer;

namespace FSO.Server.Protocol.Gluon.Packets
{
    public class GiftMoneyRequest : AbstractGluonCallPacket
    {
        public int LotId;
        public uint AvatarId;
        public int Amount;

        public override void Deserialize(IoBuffer input, ISerializationContext context)
        {
            base.Deserialize(input, context);
            LotId = input.GetInt32();
            AvatarId = input.GetUInt32();
            Amount = input.GetInt32();
        }

        public override void Serialize(IoBuffer output, ISerializationContext context)
        {
            base.Serialize(output, context);
            output.PutInt32(LotId);
            output.PutUInt32(AvatarId);
            output.PutInt32(Amount);
        }

        public override GluonPacketType GetPacketType()
        {
            return GluonPacketType.GiftMoneyRequest;
        }
    }
}

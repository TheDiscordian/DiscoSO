using System.IO;
using System.Linq;

namespace FSO.SimAntics.NetPlay.Model.Commands
{
    public class VMNetInteractionResultCmd : VMNetCommandBodyAbstract
    {
        public ushort ActionUID;
        public bool Accepted;
        public int Value; //optional payload for the accepting tree (DiscoSO: outstanding bill total into TempXL 0)
        public override bool Execute(VM vm, VMAvatar caller)
        {
            if (Value != 0 || Accepted)
                FSO.SimAntics.Utils.VMDiscoSOMailboxPatch.Log("result cmd: actor=" + ActorUID + " action=" + ActionUID
                    + " accepted=" + Accepted + " value=" + Value + " caller=" + (caller != null)
                    + " interaction=" + (caller != null && caller.Thread.Queue.Any(x => x.UID == ActionUID)));
            if (caller == null) return false;
            var interaction = caller.Thread.Queue.FirstOrDefault(x => x.UID == ActionUID);
            if (interaction != null)
            {
                interaction.InteractionResult = (sbyte)(Accepted ? 2 : 1);
                if (Accepted) caller.Thread.TempXL[0] = Value;
            }
            return true;
        }

        #region VMSerializable Members

        public override void SerializeInto(BinaryWriter writer)
        {
            base.SerializeInto(writer);
            writer.Write(ActionUID);
            writer.Write(Accepted);
            writer.Write(Value);
        }

        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            ActionUID = reader.ReadUInt16();
            Accepted = reader.ReadBoolean();
            Value = reader.ReadInt32();
        }

        #endregion
    }
}

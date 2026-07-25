using System;
using System.IO;
using System.Linq;

namespace FSO.SimAntics.NetPlay.Model.Commands
{
    /// <summary>
    /// Pushes the lot's outstanding bill count onto the mailbox, on every tile of the
    /// group. Sent by the server twice per carrier delivery: first as a pending signal
    /// (attribute 8 - tells the carrier's tree whether to visit the box at all), then
    /// after her insert animation as the visible count (attribute 1, "Number of Bills
    /// Inside" - the Pay Bills pie gate). CANNOT be sent by clients.
    /// </summary>
    public class VMNetMailboxBillsCmd : VMNetCommandBodyAbstract
    {
        public int Count;
        public bool Delivered; //false = pending signal (attr 8), true = visible count (attr 1)

        public override bool AcceptFromClient { get { return false; } }

        public override bool Execute(VM vm)
        {
            var mailbox = vm.Entities.FirstOrDefault(x => x.Object.OBJ.GUID == 0x39CCF441 || x.Object.OBJ.GUID == 0xEF121974 || x.Object.OBJ.GUID == 0x729C4842);
            if (mailbox == null) return false;
            var attr = Delivered ? 1 : 8;
            foreach (var tile in mailbox.MultitileGroup.Objects) tile.SetAttribute(attr, (short)Math.Min(short.MaxValue, Count));
            return true;
        }

        public override bool Verify(VM vm, VMAvatar caller)
        {
            return !FromNet;
        }

        #region VMSerializable Members
        public override void SerializeInto(BinaryWriter writer)
        {
            base.SerializeInto(writer);
            writer.Write(Count);
            writer.Write(Delivered);
        }

        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            Count = reader.ReadInt32();
            Delivered = reader.ReadBoolean();
        }
        #endregion
    }
}

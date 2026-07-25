using System;
using System.IO;
using System.Linq;

namespace FSO.SimAntics.NetPlay.Model.Commands
{
    /// <summary>
    /// Sets the mailbox's "Number of Bills Inside" attribute to the lot's outstanding
    /// bill count, on every tile of the group. Sent by the server after the mail
    /// carrier's delivery settles, so the carrier's insert check and the Pay Bills
    /// pie test both reflect the database. CANNOT be sent by clients.
    /// </summary>
    public class VMNetMailboxBillsCmd : VMNetCommandBodyAbstract
    {
        public int Count;

        public override bool AcceptFromClient { get { return false; } }

        public override bool Execute(VM vm)
        {
            var mailbox = vm.Entities.FirstOrDefault(x => x.Object.OBJ.GUID == 0x39CCF441 || x.Object.OBJ.GUID == 0xEF121974 || x.Object.OBJ.GUID == 0x729C4842);
            if (mailbox == null) return false;
            foreach (var tile in mailbox.MultitileGroup.Objects) tile.SetAttribute(1, (short)Math.Min(short.MaxValue, Count));
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
        }

        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            Count = reader.ReadInt32();
        }
        #endregion
    }
}

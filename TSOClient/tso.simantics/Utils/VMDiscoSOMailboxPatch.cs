using System.Linq;
using FSO.Files.Formats.IFF.Chunks;

namespace FSO.SimAntics.Utils
{
    /// <summary>
    /// Adds the DiscoSO "Pay Bills" pie interaction to the mailbox at runtime, after all
    /// content piffs have applied. This cannot ship as a piff: Content/Patch/Event already
    /// patches mailbox TTAB 129 (and adds BHAV 4111), so a second piff diffing the same
    /// chunks corrupts them. Runs on both client and server so the VMs stay in lockstep.
    /// The tree is a single generic TSO call, mode 200 (DiscoSOPayBills) - the lot server
    /// pays all outstanding bills for the lot; clients no-op.
    /// </summary>
    public static class VMDiscoSOMailboxPatch
    {
        public const uint MAILBOX_GUID = 0x39CCF441; //multi-tile master
        public const ushort PAY_BILLS_TREE = 4150; //clear of the mailbox's own trees and the Event piff's additions

        private static bool Applied;

        public static void Apply()
        {
            if (Applied) return;
            Applied = true;
            try
            {
                ApplyInternal();
            }
            catch (System.Exception e)
            {
                Log("FAILED: " + e);
            }
        }

        private static void ApplyInternal()
        {
            var mailbox = FSO.Content.Content.Get()?.WorldObjects?.Get(MAILBOX_GUID);
            var ttas = mailbox?.Resource?.Get<TTAs>(129);
            var ttab = mailbox?.Resource?.Get<TTAB>(129);
            if (ttas == null || ttab == null) { Log("mailbox chunks missing (object=" + (mailbox != null) + " ttas=" + (ttas != null) + " ttab=" + (ttab != null) + ")"); return; }
            if (mailbox.Resource.Get<BHAV>(PAY_BILLS_TREE) != null) { Log("tree " + PAY_BILLS_TREE + " already taken - stand down"); return; }
            if (ttab.Interactions.Any(x => x.ActionFunction == PAY_BILLS_TREE)) { Log("interaction already present"); return; }

            //interactions resolve through InteractionByIndex, keyed by TTAIndex - the index must be
            //unique across ALL entries, including hidden ones pointing past the string table (the
            //Event patch parks one at index 8). claim the first index above everything, padding the
            //string table to reach it.
            var stringIndex = (uint)ttas.Length;
            foreach (var existing in ttab.Interactions)
            {
                if (existing.TTAIndex >= stringIndex) stringIndex = existing.TTAIndex + 1;
            }
            while (ttas.Length < stringIndex) ttas.InsertString(ttas.Length, new STRItem { Value = "", Comment = "" });
            ttas.InsertString((int)stringIndex, new STRItem { Value = "Pay Bills", Comment = "" });
            ttab.InsertInteraction(new TTABInteraction
            {
                ActionFunction = PAY_BILLS_TREE,
                TestFunction = 0,
                MotiveEntries = new TTABMotiveEntry[0],
                Flags = 0,
                TTAIndex = stringIndex,
                AttenuationCode = 0,
                AttenuationValue = 0,
                AutonomyThreshold = 0,
                JoiningIndex = -1,
                Flags2 = TSOFlags.NonEmpty | TSOFlags.AllowObjectOwner | TSOFlags.AllowRoommates
            }, ttab.Interactions.Length);

            var template = mailbox.Resource.MainIff?.List<BHAV>()?.FirstOrDefault();
            var bhav = new BHAV
            {
                ChunkID = PAY_BILLS_TREE,
                ChunkLabel = "DiscoSO - Pay Bills",
                ChunkType = "BHAV",
                ChunkProcessed = true,
                AddedByPatch = true,
                Type = template?.Type ?? 0,
                Version = template?.Version ?? 0,
                Args = 0,
                Locals = 0,
                Instructions = new BHAVInstruction[]
                {
                    new BHAVInstruction
                    {
                        Opcode = 1, //generic tso call
                        TruePointer = 254,
                        FalsePointer = 253,
                        Operand = new byte[] { 200, 0, 0, 0, 0, 0, 0, 0 } //DiscoSOPayBills
                    }
                }
            };
            mailbox.Resource.MainIff.AddChunk(bhav);
            //the routine cache was built at content load; rebuild it so the new tree resolves
            //(GetRoutine misses -> GetAction null -> the pie silently drops the entry)
            mailbox.Resource.Recache();
            Log("installed Pay Bills at index " + stringIndex + " (" + ttab.Interactions.Length + " interactions, " + ttas.Length + " strings, routine=" + (mailbox.Resource.GetRoutine(PAY_BILLS_TREE) != null) + ")");
        }

        private static void Log(string msg)
        {
            try
            {
                var path = System.IO.Path.Combine(FSO.Common.FSOEnvironment.UserDir ?? ".", "mailbox-patch.log");
                System.IO.File.AppendAllText(path, System.DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " " + msg + "\n");
            }
            catch { }
        }
    }
}

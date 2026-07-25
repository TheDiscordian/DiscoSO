using System.Linq;
using FSO.Files.Formats.IFF.Chunks;

namespace FSO.SimAntics.Utils
{
    /// <summary>
    /// Adds the DiscoSO "Pay Bills" pie interaction to the mailbox at runtime, after all
    /// content piffs have applied. This cannot ship as a piff: Content/Patch/Event already
    /// patches mailbox TTAB 129 (and adds BHAV 4111), so a second piff diffing the same
    /// chunks corrupts them. Runs on both client and server so the VMs stay in lockstep.
    ///
    /// The interaction only shows while the mailbox holds bills (test tree: attribute 1,
    /// "Number of Bills Inside" - set at lot init and pushed by the server on each carrier
    /// delivery, zeroed on payment). The action tree routes the sim to the mailbox, plays the
    /// mailbox-open animation, asks the server for the outstanding total (interaction
    /// result -> TempXL 0), then confirms with a Yes/No dialog showing the amount before
    /// paying via generic TSO call 200.
    /// </summary>
    public static class VMDiscoSOMailboxPatch
    {
        public const uint MAILBOX_GUID = 0x39CCF441; //multi-tile master
        public const ushort PAY_BILLS_TREE = 4150; //clear of the mailbox's own trees and the Event piff's additions
        public const ushort PAY_BILLS_TEST_TREE = 4151;

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

            //dialog strings live in the private dialog set (STR 301); create or extend it
            var dialogs = mailbox.Resource.Get<STR>(301);
            if (dialogs == null)
            {
                dialogs = new STR
                {
                    ChunkID = 301,
                    ChunkLabel = "Dialog prim string set",
                    ChunkType = "STR#",
                    ChunkProcessed = true,
                    AddedByPatch = true
                };
                mailbox.Resource.MainIff.AddChunk(dialogs);
            }
            var dialogBase = dialogs.Length; //string ids in the operand are 1-based
            dialogs.InsertString(dialogBase, new STRItem { Value = "Pay Bills", Comment = "" });
            dialogs.InsertString(dialogBase + 1, new STRItem { Value = "Pay your outstanding bills for $MoneyXL:0?", Comment = "" });
            dialogs.InsertString(dialogBase + 2, new STRItem { Value = "Pay", Comment = "" });
            dialogs.InsertString(dialogBase + 3, new STRItem { Value = "Not Now", Comment = "" });

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
                TestFunction = PAY_BILLS_TEST_TREE,
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

            //action: route to the mailbox, open it for the getbills animation, close it, then
            //fetch the total (value-gated: the client's queue auto-accept races the server's
            //response, so the dialog waits for TempXL 0 to hold the amount), confirm, pay.
            //generic call 17 polls the interaction result; temp 0 = 3 means timeout.
            byte msg = (byte)(dialogBase + 2), yes = (byte)(dialogBase + 3), no = (byte)(dialogBase + 4), title = (byte)(dialogBase + 1);
            var action = new BHAV
            {
                ChunkID = PAY_BILLS_TREE,
                ChunkLabel = "DiscoSO - Pay Bills",
                ChunkType = "BHAV",
                ChunkProcessed = true,
                AddedByPatch = true,
                Type = template?.Type ?? 0,
                Version = template?.Version ?? 0,
                Args = 0,
                Locals = 2,
                Instructions = new BHAVInstruction[]
                {
                    Instr(27, 1, 255, new byte[] { 0x00, 0x00, 0x00, 0xFE, 0x00, 0x00, 0x06, 0x00 }),  //0: route in front of + facing the mailbox
                    Instr(2, 2, 253, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x05, 0x19, 0x0a }),   //1: local 0 := stack obj id (the pie callee)
                    Instr(2, 3, 253, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x05, 0x0a, 0x07 }),   //2: stack obj id := 0 (scan start)
                    Instr(31, 4, 21, new byte[] { 0x74, 0x19, 0x12, 0xEF, 0x84, 0x0A, 0x00, 0x00 }),   //3: set to next visible box tile (0xEF121974)
                    Instr(2, 5, 253, new byte[] { 0x01, 0x00, 0x00, 0x00, 0x00, 0x05, 0x19, 0x0a }),   //4: local 1 := stack obj id (the box)
                    Instr(2, 6, 253, new byte[] { 0x00, 0x00, 0x01, 0x00, 0x00, 0x05, 0x04, 0x07 }),   //5: graphic := 1 (open)
                    Instr(7, 7, 253, new byte[] { 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }),   //6: refresh stack object graphic
                    Instr(44, 8, 7, new byte[] { 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }),    //7: animate a2o-mailbox-getbills
                    Instr(2, 9, 253, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x05, 0x04, 0x07 }),   //8: graphic := 0 (closed - the box shuts with the animation)
                    Instr(7, 10, 253, new byte[] { 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }),  //9: refresh
                    Instr(2, 11, 253, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x05, 0x2a, 0x07 }),  //10: temp xl 0 := 0 (clear any stale amount)
                    Instr(1, 12, 253, new byte[] { 0xCA, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }),  //11: generic 202 - query outstanding total
                    Instr(1, 13, 253, new byte[] { 0x11, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }),  //12: generic 17 - poll interaction result
                    Instr(2, 21, 14, new byte[] { 0x00, 0x00, 0x03, 0x00, 0x00, 0x02, 0x08, 0x07 }),   //13: temp 0 == 3 (timeout? bail)
                    Instr(2, 16, 15, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x2a, 0x07 }),   //14: temp xl 0 > 0 (the amount arrived?)
                    Instr(280, 12, 253, new byte[] { 0x1e, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }),//15: global Idle(30 ticks), poll again
                    Instr(36, 17, 21, new byte[] { 0x00, 0x00, msg, yes, no, 0x01, title, 0x00 }),     //16: yes/no dialog with $MoneyXL:0
                    Instr(2, 18, 253, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x05, 0x0a, 0x19 }),  //17: stack obj id := local 0 (pie callee)
                    Instr(1, 19, 253, new byte[] { 0xC8, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }),  //18: generic 200 - server pays all bills
                    Instr(2, 20, 253, new byte[] { 0x01, 0x00, 0x00, 0x00, 0x00, 0x05, 0x01, 0x07 }),  //19: attr 1 ("Number of Bills Inside") := 0
                    Instr(44, 254, 20, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x03, 0x20, 0x02, 0x00 }), //20: animation reset, paid (true)
                    Instr(44, 255, 21, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x03, 0x20, 0x02, 0x00 })  //21: animation reset, declined/timeout/fallback (false)
                }
            };
            mailbox.Resource.MainIff.AddChunk(action);

            //test: only offer the interaction while the mailbox holds bills
            var test = new BHAV
            {
                ChunkID = PAY_BILLS_TEST_TREE,
                ChunkLabel = "DiscoSO - Pay Bills TEST",
                ChunkType = "BHAV",
                ChunkProcessed = true,
                AddedByPatch = true,
                Type = template?.Type ?? 0,
                Version = template?.Version ?? 0,
                Args = 0,
                Locals = 0,
                Instructions = new BHAVInstruction[]
                {
                    Instr(2, 254, 255, new byte[] { 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x07 }) //attr 1 > 0
                }
            };
            mailbox.Resource.MainIff.AddChunk(test);

            //the routine cache was built at content load; rebuild it so the new trees resolve
            //(GetRoutine misses -> GetAction null -> the pie silently drops the entry)
            mailbox.Resource.Recache();
            Log("installed Pay Bills at index " + stringIndex + " (" + ttab.Interactions.Length + " interactions, " + ttas.Length + " strings, routine=" + (mailbox.Resource.GetRoutine(PAY_BILLS_TREE) != null) + ", test=" + (mailbox.Resource.GetRoutine(PAY_BILLS_TEST_TREE) != null) + ")");
        }

        private static BHAVInstruction Instr(ushort opcode, byte t, byte f, byte[] operand)
        {
            return new BHAVInstruction { Opcode = opcode, TruePointer = t, FalsePointer = f, Operand = operand };
        }

        public static void Log(string msg)
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

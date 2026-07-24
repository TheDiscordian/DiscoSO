using FSO.Files.Formats.IFF.Chunks;

namespace FSO.SimAntics.Utils
{
    /// <summary>
    /// Extends FreeSO's lamp auto toggles (lightglobals_manage.piff trees 8300/8302) at
    /// runtime so toggling auto also clears the manual-override family (flag values 4/5).
    /// The stock trees only flip flag value 3; a lamp manually switched leaves value 4 set,
    /// and "Set state from Auto" (8214) refuses to act while it is - the lamp stays manual
    /// no matter how auto is toggled. Cannot ship as a piff: the trees come from
    /// lightglobals_manage.piff, and a second piff diffing the same chunks corrupts them.
    /// Runs on both client and server so the VMs stay in lockstep.
    /// </summary>
    public static class VMDiscoSOLightPatch
    {
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
                VMDiscoSOMailboxPatch.Log("light patch FAILED: " + e);
            }
        }

        private static void ApplyInternal()
        {
            var res = FSO.Content.Content.Get()?.WorldObjectGlobals?.Get("lightglobals")?.Resource;
            if (res == null) { VMDiscoSOMailboxPatch.Log("light patch: lightglobals missing"); return; }

            var changed = false;
            foreach (var id in new ushort[] { 8300, 8302 })
            {
                var bhav = res.Get<BHAV>(id);
                if (bhav == null || bhav.Instructions.Length != 1) continue; //no manage piff, or already patched
                var flagOp = bhav.Instructions[0];
                bhav.Instructions = new BHAVInstruction[]
                {
                    Instr(flagOp.Opcode, 1, 253, flagOp.Operand),                                      //0: original toggle of the auto-override flag (value 3)
                    Instr(2, 2, 253, new byte[] { 0x00, 0x00, 0x03, 0x20, 0x00, 0x0A, 0x01, 0x1A }),   //1: clear flag value 4 (day override - the wedge bit)
                    Instr(2, 254, 253, new byte[] { 0x00, 0x00, 0x04, 0x20, 0x00, 0x0A, 0x01, 0x1A })  //2: clear flag value 5 (phase changed)
                };
                changed = true;
            }

            if (changed)
            {
                res.Recache();
                VMDiscoSOMailboxPatch.Log("light patch: auto toggles clear the override family (8300="
                    + res.Get<BHAV>(8300)?.Instructions.Length + " ins, 8302=" + res.Get<BHAV>(8302)?.Instructions.Length + " ins)");
            }
        }

        private static BHAVInstruction Instr(ushort opcode, byte t, byte f, byte[] operand)
        {
            return new BHAVInstruction { Opcode = opcode, TruePointer = t, FalsePointer = f, Operand = operand };
        }
    }
}

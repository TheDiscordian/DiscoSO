using FSO.SimAntics.Entities;
using FSO.SimAntics.Model;
using System.Collections.Generic;
using System.Linq;

namespace FSO.SimAntics.Engine.TSOGlobalLink
{
    /// <summary>
    /// One in-game hour of billable objects. Each count is one hour owed for that stream.
    /// </summary>
    public struct VMLotUsageSample
    {
        public int LitLamps;
        public int OpenStalls;
        public int PlayingStereos;
        public int PlayingTvs;

        public bool Any
        {
            get { return LitLamps > 0 || OpenStalls > 0 || PlayingStereos > 0 || PlayingTvs > 0; }
        }
    }

    /// <summary>
    /// Counts what a lot is running, once per in-game hour. Whichever host owns the VM drives this
    /// and stores the result - the lot server into the database, sandbox into the standalone one.
    /// </summary>
    public class VMLotUsageSampler
    {
        private static readonly HashSet<string> StallIffs = new HashSet<string> { "foodcounter.iff", "foodcounterunleashed.iff" };
        private static readonly HashSet<string> StereoIffs = new HashSet<string> { "stereos.iff", "stereos2.iff", "jukebox.iff", "stereowallunleashed.iff", "stereospeakers.iff" };
        private static readonly HashSet<string> TvIffs = new HashSet<string> { "tvs.iff" };

        private int Ticker;

        public static bool IsStall(VMEntity obj)
        {
            var iff = (obj as VMGameObject)?.Object?.Resource?.MainIff?.Filename;
            return iff != null && StallIffs.Contains(iff);
        }

        public static bool IsTv(VMEntity obj)
        {
            var iff = (obj as VMGameObject)?.Object?.Resource?.MainIff?.Filename;
            return iff != null && TvIffs.Contains(iff);
        }

        public static bool IsStereo(VMEntity obj)
        {
            var iff = (obj as VMGameObject)?.Object?.Resource?.MainIff?.Filename;
            return iff != null && StereoIffs.Contains(iff);
        }

        /// <summary>
        /// Sample once per in-game hour (TicksPerMinute x 60 ticks - 5 real minutes on TSO lots).
        /// Returns null on every other tick.
        /// </summary>
        public VMLotUsageSample? Tick(VM vm)
        {
            if (++Ticker < vm.Context.Clock.TicksPerMinute * 60) return null;
            Ticker = 0;
            return Sample(vm);
        }

        public static VMLotUsageSample Sample(VM vm)
        {
            var groups = new HashSet<VMMultitileGroup>();
            foreach (var ent in vm.Entities)
            {
                if (ent is VMGameObject && ent.MultitileGroup != null) groups.Add(ent.MultitileGroup);
            }

            var result = new VMLotUsageSample();
            foreach (var group in groups)
            {
                //the engine's real-light test: windows and doors carry a daylight contribution
                //but aren't lamps, and an auto-off lamp contributes 0
                if (group.Objects.Any(o =>
                {
                    var lightFlags = (VMEntityFlags2)o.GetValue(VMStackObjectVariable.FlagField2);
                    return (lightFlags & VMEntityFlags2.GeneratesLight) > 0
                        && (lightFlags & (VMEntityFlags2.ArchitectualWindow | VMEntityFlags2.ArchitectualDoor)) == 0
                        && o.GetValue(VMStackObjectVariable.LightingContribution) > 0;
                })) result.LitLamps++;
                if (IsStall(group.BaseObject) && group.Objects.Any(o => o.GetAttribute(1) > 0)) result.OpenStalls++; //"Is open?" lives on the control segment, not the base tile
                if (IsStereo(group.BaseObject) && group.Objects.Any(o => o.GetAttribute(0) > 0)) result.PlayingStereos++; //attribute 0 = "Power (Off/On)" on every stereo family
                if (IsTv(group.BaseObject) && group.Objects.Any(o => o.GetAttribute(0) > 0)) result.PlayingTvs++; //attribute 0 = "Power (Off/On)" on tvs.iff
            }
            return result;
        }
    }
}

using FSO.Common.Model;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FSO.SimAntics.Engine.TSOGlobalLink
{
    public class VMStandaloneLotBill
    {
        public int Amount;
        public int BilledDay;
        public int PaidDay = -1; //-1 = outstanding

        public bool Paid { get { return PaidDay >= 0; } }
    }

    /// <summary>
    /// A sandbox lot's metered bills, plus the usage hours not yet folded into one. Stands in for
    /// fso_lot_bills and fso_lot_usage, which only the city server has.
    /// </summary>
    public class VMStandaloneLotBills
    {
        public double LightHours;
        public double StallHours;
        public double RadioHours;
        public double TvHours;
        public List<VMStandaloneLotBill> Bills = new List<VMStandaloneLotBill>();

        public void SerializeInto(BinaryWriter writer)
        {
            writer.Write(LightHours);
            writer.Write(StallHours);
            writer.Write(RadioHours);
            writer.Write(TvHours);
            writer.Write(Bills.Count);
            foreach (var bill in Bills)
            {
                writer.Write(bill.Amount);
                writer.Write(bill.BilledDay);
                writer.Write(bill.PaidDay);
            }
        }

        public void Deserialize(BinaryReader reader)
        {
            LightHours = reader.ReadDouble();
            StallHours = reader.ReadDouble();
            RadioHours = reader.ReadDouble();
            TvHours = reader.ReadDouble();
            var count = reader.ReadInt32();
            Bills = new List<VMStandaloneLotBill>();
            for (int i = 0; i < count; i++)
            {
                Bills.Add(new VMStandaloneLotBill()
                {
                    Amount = reader.ReadInt32(),
                    BilledDay = reader.ReadInt32(),
                    PaidDay = reader.ReadInt32()
                });
            }
        }

        public List<VMStandaloneLotBill> Outstanding()
        {
            return Bills.Where(x => !x.Paid).OrderBy(x => x.BilledDay).ToList();
        }

        public void AddUsage(int lamps, int stalls, int stereos, int tvs)
        {
            LightHours += lamps;
            StallHours += stalls;
            RadioHours += stereos;
            TvHours += tvs;
        }
    }

    /// <summary>
    /// Standalone-database storage for the shared metered billing rules. Sandbox has no fso_tuning,
    /// so the rates are the ones the live server ships with. The daily property fee has no meaning
    /// here - a sandbox lot has no size on file and nothing runs the nightly task - so only the
    /// metered streams are charged.
    /// </summary>
    public class VMStandaloneLotBillsStore : ILotBillsStore
    {
        public const float LIGHTS_RATE = 1;
        public const float STALL_RATE = 2;
        public const float RADIO_RATE = 1;
        public const float TV_RATE = 1;

        private readonly VMStandaloneLotBills Lot;

        public VMStandaloneLotBillsStore(VMStandaloneLotBills lot)
        {
            Lot = lot;
        }

        public float Tuning(int index, float def)
        {
            switch (index)
            {
                case 5: return LIGHTS_RATE;
                case 7: return STALL_RATE;
                case 8: return RADIO_RATE;
                case 9: return TV_RATE;
            }
            return def;
        }

        public int? OldestOutstandingDay()
        {
            var outstanding = Lot.Outstanding();
            if (outstanding.Count == 0) return null;
            return outstanding[0].BilledDay;
        }

        public LotUsageHours GetUnbilledUsage()
        {
            return new LotUsageHours()
            {
                LightHours = Lot.LightHours,
                StallHours = Lot.StallHours,
                RadioHours = Lot.RadioHours,
                TvHours = Lot.TvHours
            };
        }

        public LotUsageHours CollectUnbilledUsage()
        {
            var result = GetUnbilledUsage();
            Lot.LightHours = 0;
            Lot.StallHours = 0;
            Lot.RadioHours = 0;
            Lot.TvHours = 0;
            return result;
        }

        public bool AddMeteredCharge(int day, int amount)
        {
            //extend the day's unpaid bill if one exists, otherwise open a new one - same as the server
            var open = Lot.Bills.LastOrDefault(x => !x.Paid && x.BilledDay == day);
            if (open != null) open.Amount += amount;
            else Lot.Bills.Add(new VMStandaloneLotBill() { Amount = amount, BilledDay = day });
            return true;
        }
    }
}

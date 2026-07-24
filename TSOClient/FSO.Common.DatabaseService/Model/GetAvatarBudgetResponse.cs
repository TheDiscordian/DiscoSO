using FSO.Common.Serialization;
using System.Collections.Generic;
using Mina.Core.Buffer;
using FSO.Common.DatabaseService.Framework;

namespace FSO.Common.DatabaseService.Model
{
    [DatabaseResponse(DBResponseType.GetDataServiceAvatarBudgetByID)]
    public class GetAvatarBudgetResponse : IoBufferSerializable, IoBufferDeserializable
    {
        public uint AvatarId;
        public uint Cash;
        public uint ObjectsMoney; //money stored inside owned objects
        public uint ObjectsValue; //resale value of owned objects
        public List<BudgetDaySummary> Days = new List<BudgetDaySummary>();
        public List<BudgetCategorySummary> Categories = new List<BudgetCategorySummary>();

        public void Serialize(IoBuffer output, ISerializationContext context)
        {
            output.PutUInt32(AvatarId);
            output.PutUInt32(Cash);
            output.PutUInt32(ObjectsMoney);
            output.PutUInt32(ObjectsValue);

            output.PutUInt32((uint)Days.Count);
            foreach (var day in Days)
            {
                output.PutUInt32(day.Day);
                output.PutUInt32(day.Income);
                output.PutUInt32(day.Expense);
            }

            output.PutUInt32((uint)Categories.Count);
            foreach (var cat in Categories)
            {
                output.PutInt16(cat.TransactionType);
                output.PutUInt32(cat.Income);
                output.PutUInt32(cat.Expense);
            }
        }

        public void Deserialize(IoBuffer input, ISerializationContext context)
        {
            AvatarId = input.GetUInt32();
            Cash = input.GetUInt32();
            ObjectsMoney = input.GetUInt32();
            ObjectsValue = input.GetUInt32();

            var dayCount = input.GetUInt32();
            Days = new List<BudgetDaySummary>((int)dayCount);
            for (var i = 0; i < dayCount; i++)
            {
                Days.Add(new BudgetDaySummary
                {
                    Day = input.GetUInt32(),
                    Income = input.GetUInt32(),
                    Expense = input.GetUInt32()
                });
            }

            var catCount = input.GetUInt32();
            Categories = new List<BudgetCategorySummary>((int)catCount);
            for (var i = 0; i < catCount; i++)
            {
                Categories.Add(new BudgetCategorySummary
                {
                    TransactionType = input.GetInt16(),
                    Income = input.GetUInt32(),
                    Expense = input.GetUInt32()
                });
            }
        }
    }

    public class BudgetDaySummary
    {
        public uint Day; //days since unix epoch, matching fso_transactions.day
        public uint Income;
        public uint Expense;
    }

    public class BudgetCategorySummary
    {
        public short TransactionType; //VMTransferFundsExpenseType
        public uint Income;
        public uint Expense;
    }
}

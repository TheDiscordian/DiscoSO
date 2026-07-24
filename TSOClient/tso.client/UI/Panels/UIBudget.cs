using FSO.Client.Controllers;
using FSO.Client.Controllers.Panels;
using FSO.Client.UI.Controls;
using FSO.Client.UI.Framework;
using FSO.Client.UI.Framework.Parser;
using FSO.Common.DatabaseService.Model;
using FSO.Common.Rendering.Framework.Model;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FSO.Client.UI.Panels
{
    /// <summary>
    /// The TSO Budget Window (opened from the UCP budget button). Layout, art, and strings
    /// come from EA's budget.uis; the original game never had this window implemented in FreeSO.
    /// </summary>
    public class UIBudget : UIDialog
    {
        public UILabel TitleLabel { get; set; }

        public UIButton CashTabButton { get; set; }
        public UIButton NetWorthTabButton { get; set; }
        public UIButton DebtTabButton { get; set; }
        public UIButton IncomeTabButton { get; set; }
        public UIButton ExpensesTabButton { get; set; }

        public UILabel CashTabLabel { get; set; }
        public UILabel NetWorthTabLabel { get; set; }
        public UILabel DebtTabLabel { get; set; }
        public UILabel IncomeTabLabel { get; set; }
        public UILabel ExpensesTabLabel { get; set; }

        public UILabel CashTabValue { get; set; }
        public UILabel NetWorthTabValue { get; set; }
        public UILabel DebtTabValue { get; set; }
        public UILabel IncomeTabValue { get; set; }
        public UILabel ExpensesTabValue { get; set; }

        public UIListBox ListBox { get; set; }
        public UISlider ListBoxSlider { get; set; }
        public UIButton ListBoxScrollUpButton { get; set; }
        public UIButton ListBoxScrollDownButton { get; set; }

        private UIImage TabBackground;
        private UIImage PopOutBackground;
        private UIImage ListBoxBackground;
        private UIImage CashTabImage;
        private UIImage NetWorthTabImage;
        private UIImage DebtTabImage;
        private UIImage IncomeTabImage;
        private UIImage ExpensesTabImage;

        private UIScript Script;
        private GetAvatarBudgetResponse Data;
        private int SelectedTab = -1; //0 cash, 1 net worth, 2 debt, 3 income, 4 expenses

        //budget.uis WindowSetup sizeNormal/sizeExpanded
        private static readonly Point SizeNormal = new Point(240, 286);
        private static readonly Point SizeExpanded = new Point(492, 286);

        public UIBudget() : base(UIDialogStyle.Close, true)
        {
            Script = this.RenderScript("budget.uis");

            Caption = (string)Script["TitleText"];
            TitleLabel.Visible = false; //the dialog caption replaces the script's own title

            TabBackground = Script.Create<UIImage>("TabBackground");
            Add(TabBackground);
            //popout sits under the tab overlays - the selected tab's art bridges over its left edge
            PopOutBackground = Script.Create<UIImage>("PopOutBackground");
            Add(PopOutBackground);
            CashTabImage = Script.Create<UIImage>("CashTabImage");
            Add(CashTabImage);
            NetWorthTabImage = Script.Create<UIImage>("NetWorthTabImage");
            Add(NetWorthTabImage);
            DebtTabImage = Script.Create<UIImage>("DebtTabImage");
            Add(DebtTabImage);
            IncomeTabImage = Script.Create<UIImage>("IncomeTabImage");
            Add(IncomeTabImage);
            ExpensesTabImage = Script.Create<UIImage>("ExpensesTabImage");
            Add(ExpensesTabImage);
            ListBoxBackground = Script.Create<UIImage>("ListBoxBackground");
            Add(ListBoxBackground);

            //restore script controls above the backgrounds
            Add(CashTabLabel); Add(NetWorthTabLabel); Add(DebtTabLabel); Add(IncomeTabLabel); Add(ExpensesTabLabel);
            Add(CashTabValue); Add(NetWorthTabValue); Add(DebtTabValue); Add(IncomeTabValue); Add(ExpensesTabValue);
            Add(CashTabButton); Add(NetWorthTabButton); Add(DebtTabButton); Add(IncomeTabButton); Add(ExpensesTabButton);
            Add(ListBox); Add(ListBoxSlider); Add(ListBoxScrollUpButton); Add(ListBoxScrollDownButton);

            ListBox.AttachSlider(ListBoxSlider);
            ListBoxSlider.AttachButtons(ListBoxScrollUpButton, ListBoxScrollDownButton, 1);
            var listStyle = Script.Create<UIListBoxTextStyle>("ListBoxLeftColumnColors", ListBox.FontStyle);
            ListBox.TextStyle = listStyle;

            //the uis asks for label alignment 1 (left) and value alignment 5 (right), but the parser
            //only maps alignment 3 - both fall back to centered and overlap. set them directly,
            //and put each value on its label's row.
            var labels = new[] { CashTabLabel, NetWorthTabLabel, DebtTabLabel, IncomeTabLabel, ExpensesTabLabel };
            var values = new[] { CashTabValue, NetWorthTabValue, DebtTabValue, IncomeTabValue, ExpensesTabValue };
            for (int i = 0; i < labels.Length; i++)
            {
                labels[i].Alignment = TextAlignment.Left | TextAlignment.Middle;
                values[i].Alignment = TextAlignment.Right | TextAlignment.Middle;
                labels[i].Y -= 4; //sit the text on the tab finger's centre line
                values[i].Y = labels[i].Y;
            }

            CashTabButton.OnButtonClick += b => SelectTab(0);
            NetWorthTabButton.OnButtonClick += b => SelectTab(1);
            DebtTabButton.OnButtonClick += b => SelectTab(2);
            IncomeTabButton.OnButtonClick += b => SelectTab(3);
            ExpensesTabButton.OnButtonClick += b => SelectTab(4);

            ExpensesTabButton.Disabled = false; //EA's script ships it disabled
            DebtTabButton.Disabled = true; //enabled when the server reports a nonzero bills rate

            CloseButton.OnButtonClick += b => FindController<BudgetController>()?.Close();

            SetExpanded(false);
            SetTabSelection();
            SetTabValues();
        }

        public override void Update(UpdateState state)
        {
            base.Update(state);
            //UIDialog's render cache is only regenerated when Invalidated is set; resizing after
            //display (our expand/collapse) leaves stale fragments behind. re-render while open.
            if (Visible) Invalidated = true;
        }

        public void SetData(GetAvatarBudgetResponse data)
        {
            Data = data;
            DebtTabButton.Disabled = !data.BillsEnabled;
            SetTabValues();
            RefreshList();
        }

        private void SetTabValues()
        {
            CashTabValue.Caption = FormatMoney(Data?.Cash ?? 0);
            NetWorthTabValue.Caption = FormatMoney(NetWorthTotal());
            DebtTabValue.Caption = FormatMoney(TotalBills());
            IncomeTabValue.Caption = FormatMoney(TotalIncome());
            ExpensesTabValue.Caption = FormatMoney(TotalExpenses());
        }

        private void SelectTab(int tab)
        {
            if (SelectedTab == tab)
            {
                SelectedTab = -1;
                SetTabSelection();
                SetExpanded(false);
                return;
            }
            SelectedTab = tab;
            SetTabSelection();
            SetExpanded(true);
            RefreshList();
        }

        private void SetTabSelection()
        {
            CashTabButton.Selected = SelectedTab == 0;
            NetWorthTabButton.Selected = SelectedTab == 1;
            DebtTabButton.Selected = SelectedTab == 2;
            IncomeTabButton.Selected = SelectedTab == 3;
            ExpensesTabButton.Selected = SelectedTab == 4;

            //selected-tab state overlays - one visible at a time, like Bookmarks' SimsTab/IgnoreTab
            CashTabImage.Visible = SelectedTab == 0;
            NetWorthTabImage.Visible = SelectedTab == 1;
            DebtTabImage.Visible = SelectedTab == 2;
            IncomeTabImage.Visible = SelectedTab == 3;
            ExpensesTabImage.Visible = SelectedTab == 4;
        }

        private void SetExpanded(bool expanded)
        {
            DialogSize = expanded ? SizeExpanded : SizeNormal;
            PopOutBackground.Visible = expanded;
            ListBoxBackground.Visible = expanded;
            ListBox.Visible = expanded;
            ListBoxSlider.Visible = expanded;
            ListBoxScrollUpButton.Visible = expanded;
            ListBoxScrollDownButton.Visible = expanded;
        }

        private void RefreshList()
        {
            var items = new List<UIListBoxItem>();
            switch (SelectedTab)
            {
                case 0: //cash: recent days' net flow
                    if (Data != null)
                    {
                        foreach (var day in Data.Days)
                        {
                            var net = (long)day.Income - day.Expense;
                            items.Add(Row(DayLabel(day.Day), FormatSigned(net)));
                        }
                    }
                    break;
                case 2: //bills by day
                    if (Data != null)
                    {
                        foreach (var day in Data.BillsDays)
                            items.Add(Row(DayLabel(day.Day), FormatMoney(day.Expense)));
                    }
                    break;
                case 1: //net worth breakdown
                    items.Add(Row((string)Script["NetWorthCashValue"], FormatMoney(Data?.Cash ?? 0)));
                    items.Add(Row((string)Script["NetWorthMoneyInObjects"], FormatMoney(Data?.ObjectsMoney ?? 0)));
                    items.Add(Row((string)Script["NetWorthObjectValue"], FormatMoney(Data?.ObjectsValue ?? 0)));
                    items.Add(Row((string)Script["NetWorthSumTotal"], FormatMoney(NetWorthTotal())));
                    break;
                case 3: //income by category
                    foreach (var cat in ByCategory(x => x.Income))
                        items.Add(Row(cat.Key, FormatMoney(cat.Value)));
                    break;
                case 4: //expenses by category
                    foreach (var cat in ByCategory(x => x.Expense))
                        items.Add(Row(cat.Key, FormatMoney(cat.Value)));
                    break;
            }
            ListBox.Items = items;
        }

        private UIListBoxItem Row(string label, string value)
        {
            return new UIListBoxItem(label, new object[] { label, "", value });
        }

        private List<KeyValuePair<string, ulong>> ByCategory(Func<BudgetCategorySummary, uint> selector)
        {
            if (Data == null) return new List<KeyValuePair<string, ulong>>();
            return Data.Categories
                .Where(x => selector(x) > 0)
                .GroupBy(x => CategoryName(x.TransactionType))
                .Select(g => new KeyValuePair<string, ulong>(g.Key, g.Aggregate(0ul, (acc, x) => acc + selector(x))))
                .OrderByDescending(x => x.Value)
                .ToList();
        }

        private ulong NetWorthTotal()
        {
            if (Data == null) return 0;
            return (ulong)Data.Cash + Data.ObjectsMoney + Data.ObjectsValue;
        }

        private ulong TotalIncome()
        {
            if (Data == null) return 0;
            return Data.Categories.Aggregate(0ul, (acc, x) => acc + x.Income);
        }

        private ulong TotalBills()
        {
            if (Data == null) return 0;
            return Data.BillsDays.Aggregate(0ul, (acc, x) => acc + x.Expense);
        }

        public void ShowBills()
        {
            if (SelectedTab != 2) SelectTab(2);
        }

        private ulong TotalExpenses()
        {
            if (Data == null) return 0;
            return Data.Categories.Aggregate(0ul, (acc, x) => acc + x.Expense);
        }

        private static string FormatMoney(ulong value)
        {
            return "$" + value.ToString("##,#0");
        }

        private static string FormatSigned(long value)
        {
            return (value < 0 ? "-$" : "+$") + Math.Abs(value).ToString("##,#0");
        }

        private static string DayLabel(uint epochDay)
        {
            return new DateTime(1970, 1, 1).AddDays(epochDay).ToString("MMM d");
        }

        //VMTransferFundsExpenseType, grouped into player-facing categories
        private static string CategoryName(int type)
        {
            switch (type)
            {
                case 0: return "General";
                case 1: case 5: return "Miscellaneous";
                case 2: return "From Sims";
                case 3: return "From objects";
                case 4: case 60: return "Cheats";
                case 6: return "Refills and maintenance";
                case 7: return "Objects";
                case 8: return "Sim to Sim";
                case 9: case 10: return "Skill objects";
                case 11: case 12: return "Pizza";
                case 13: case 14: return "Paper chase";
                case 15: case 16: return "Maze";
                case 17: case 18: return "Roulette";
                case 19: case 20: return "Slots";
                case 21: case 22: return "Blackjack";
                case 23: case 24: return "Poker";
                case 25: return "NPC services";
                case 26: return "Gardener";
                case 27: return "Maid";
                case 28: return "Repairman";
                case 29: return "Butler";
                case 30: return "Job";
                case 31: return "Robot factory job";
                case 32: return "Restaurant job";
                case 33: return "Nightclub job";
                case 34: return "CSR";
                case 35: case 36: case 37: case 38: return "Cash transfer";
                case 39: return "Shared account";
                case 40: return "Door charges";
                case 41: return "Typewriter";
                case 42: case 43: return "Easel";
                case 44: return "Chalkboard";
                case 45: return "Canning";
                case 46: return "Chemistry";
                case 47: return "Workbench";
                case 48: case 49: return "Pinata";
                case 50: return "Telemarketing";
                case 51: case 52: return "Tip jar";
                case 53: case 54: return "Vibrating bed";
                case 55: case 56: case 57: case 58: return "Buffet";
                case 59: return "Slot machine";
                case 61: case 63: return "Food counter";
                case 64: case 65: case 66: return "Snack machine";
                case 67: case 68: case 69: return "Soda machine";
                case 70: case 71: return "Pinball";
                case 72: return "Fridge restocking";
                //DiscoSO ledger codes
                case 100: return "Purchases";
                case 101: return "Sell-backs";
                case 102: return "Building";
                case 103: return "Upgrades";
                case 104: return "Lot expansion";
                case 108: return "Bills";
                case 105: return "Visitor bonus";
                case 106: return "Property bonus";
                case 107: return "Sim bonus";
                default: return "Other";
            }
        }
    }
}

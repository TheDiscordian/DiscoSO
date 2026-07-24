using FSO.Client.UI.Panels;
using FSO.Common.DatabaseService;
using FSO.Common.DatabaseService.Model;
using FSO.Common.Utils;
using System;

namespace FSO.Client.Controllers.Panels
{
    public class BudgetController : IDisposable
    {
        private UIBudget View;
        private IDatabaseService DatabaseService;
        private Network.Network Network;

        public BudgetController(UIBudget view, IDatabaseService databaseService, Network.Network network)
        {
            View = view;
            DatabaseService = databaseService;
            Network = network;
        }

        public void Toggle()
        {
            if (View.Visible) Close();
            else Show();
        }

        public void Show()
        {
            View.Visible = true;
            Refresh();
        }

        public void Refresh()
        {
            DatabaseService.GetAvatarBudget(new GetAvatarBudgetRequest { AvatarId = Network.MyCharacter })
                .ContinueWith(x =>
                {
                    if (x.IsFaulted || x.Result == null) return;
                    GameThread.NextUpdate(y => View.SetData(x.Result));
                });
        }

        public void ShowBills()
        {
            View.ShowBills();
            Show();
        }

        public void Close()
        {
            View.Visible = false;
        }

        public void Dispose()
        {
        }
    }
}

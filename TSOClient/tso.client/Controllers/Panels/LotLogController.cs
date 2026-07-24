using FSO.Client.UI.Panels;
using FSO.Common.DatabaseService;
using FSO.Common.DatabaseService.Model;
using FSO.Common.Utils;
using System;

namespace FSO.Client.Controllers.Panels
{
    public class LotLogController : IDisposable
    {
        private UILogPanel View;
        private IDatabaseService DatabaseService;

        public LotLogController(UILogPanel view, IDatabaseService databaseService)
        {
            View = view;
            DatabaseService = databaseService;

            var location = GameFacade.Screens.CurrentUIScreen.FindController<CoreGameScreenController>()?.GetCurrentLotID() ?? 0;
            if (location == 0) return;
            DatabaseService.GetLotLog(new GetLotLogRequest { Location = location })
                .ContinueWith(x =>
                {
                    if (x.IsFaulted || x.Result == null) return;
                    GameThread.NextUpdate(y => View.SetData(x.Result));
                });
        }

        public void Dispose()
        {
        }
    }
}

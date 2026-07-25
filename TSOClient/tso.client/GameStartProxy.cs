using System;
using FSO.LotView;
using FSO.UI;

namespace FSO.Client
{
    /// <summary>
    /// To avoid dynamically linking monogame from Program.cs (where we have to choose the correct version for the OS),
    /// we use this mediator class.
    /// </summary>
    public class GameStartProxy : IGameStartProxy
    {
        public void Start(bool useDX)
        {
            GameFacade.DirectX = useDX;
			World.DirectX = useDX;
            TSOGame game = new TSOGame();
            game.Run();
            game.Dispose();
            //stuck foreground threads (audio) can keep the process alive after the loop ends,
            //leaving a zombie client whose open sockets hold the city session (ghost "online")
            Environment.Exit(0);
        }

		public void SetPath(string path)
		{
			GlobalSettings.Default.StartupPath = path;
            GlobalSettings.Default.Windowed = false;
		}


	}
}

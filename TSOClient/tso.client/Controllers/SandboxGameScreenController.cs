using FSO.Client.UI.Screens;
using System;

namespace FSO.Client.Controllers
{
    /// <summary>
    /// Exists so leaving sandbox mode tears the lot down. ChangeState disposes the outgoing
    /// controller, and without one here the world and its scenes were left in memory.
    /// </summary>
    public class SandboxGameScreenController : IDisposable
    {
        public SandboxGameScreen Screen;

        public SandboxGameScreenController(SandboxGameScreen view)
        {
            this.Screen = view;
        }

        public void Dispose()
        {
            Screen.CleanupLastWorld();
            GameFacade.Scenes.Clear();
        }
    }
}

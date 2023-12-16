using System;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace lxEF.Client
{
    public class ClientMain : BaseScript
    {
        public ClientMain()
        {
            EventHandlers.Add("playerSpawned", new Action(HandlePlayerSpawned));
            Debug.WriteLine("Hi from lxEF.Client!");
        }

        private void HandlePlayerSpawned()
        {
            TriggerServerEvent("OnPlayerConnected");
        }
    }
}
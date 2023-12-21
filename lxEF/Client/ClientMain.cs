using System;
using System.Dynamic;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace lxEF.Client
{
    public class ClientMain : BaseScript
    {
        public ClientMain()
        {
            Debug.WriteLine("Hi from lxEF.Client!");

            EventHandlers.Add("onClientMapStart", new Action(OnClientMapStart));
        }

        private void OnClientMapStart()
        {
            dynamic spawnData = new ExpandoObject();
            spawnData.x = 466.8401;
            spawnData.y = 197.7201;
            spawnData.z = 111.5291;
            spawnData.heading = 291.71;
            spawnData.model = "a_m_m_farmer_01";
            spawnData.skipFade = false;

            Exports["spawnmanager"].spawnPlayer(spawnData);
            Debug.WriteLine("Spawned successful");
        }
    }
}
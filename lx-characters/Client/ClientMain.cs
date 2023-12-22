using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace lx_characters.Client
{
    public class ClientMain : BaseScript
    {
        private bool _hasStarted = false;
        public ClientMain()
        {
            if (!_hasStarted)
                Tick += OnTick;
            RegisterNuiCallbackType("setupCharacters");
            EventHandlers["__cfx_nui:setupCharacters"] += new Action(OnSetupCharacters);
            EventHandlers["chars:setupCharactersResponse"] += new Action<string>(OnSetupCharactersResponse);
            EventHandlers["chars:start"] += new Action(OnStart);
        }


        private async Task OnTick()
        {
            while (true)
            {
                await Delay(0);

                if (API.NetworkIsSessionStarted())
                {
                    Debug.WriteLine("If Reached");
                    OnStart();
                    Tick -= OnTick;
                    break;
                }
            }
        }

        private void OnStart()
        {
            Debug.WriteLine("SetNuiFocus reached");
            var data = new
            {
                action = "ui",
                customNationality = false,
                toggle = true,
                nChar = 5,
                enableDeleteButton = true,
                translations = ""
            };

            TriggerServerEvent("chars:serializeJsonRequest", data, new Action<string>((result) =>
            {
                Debug.WriteLine(result);
                ShutdownLoadingScreenNui();
                ShutdownLoadingScreen();
                SetNuiFocus(true, true);
                SendNuiMessage(result);
            }));
        }

        private void OnSetupCharactersResponse(string jsonData)
        {
            Debug.WriteLine("DEBUG: CLIENT-SIDE SetupCharactersResponse TRIGGER");
            Debug.WriteLine(jsonData);
            SendNuiMessage(jsonData);
        }

        private void OnSetupCharacters()
        {
            Debug.WriteLine(Game.Player.Handle.ToString());
            Debug.WriteLine("DEBUG: CLIENT-SIDE OnSetupCharacters TRIGGER");
            TriggerServerEvent("chars:setupCharactersRequest");

        }
    }
}
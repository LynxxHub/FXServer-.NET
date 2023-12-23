using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;
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
            RegisterNuiCallbackType("createNewCharacter");
            RegisterNuiCallbackType("removeCharacter");
            EventHandlers["__cfx_nui:setupCharacters"] += new Action(OnSetupCharacters);
            EventHandlers["__cfx_nui:createNewCharacter"] += new Action<IDictionary<string, object>>(OnCreateCharacter);
            EventHandlers["__cfx_nui:removeCharacter"] += new Action<IDictionary<string, object>>(OnRemoveCharacter);
            EventHandlers["chars:setupCharactersResponse"] += new Action<string>(OnSetupCharactersResponse);
            EventHandlers["chars:start"] += new Action(OnStart);
        }

        private void OnRemoveCharacter(IDictionary<string, object> data)
        {
            try
            {
                Debug.WriteLine(data["citizenid"].ToString());
                TriggerServerEvent("EF:RemoveCharacter", data["citizenid"]);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.InnerException?.Message);
                Debug.WriteLine(ex.StackTrace);
            }
        }

        //TODO: Change functionality of EFCreateCharacter
        private void OnCreateCharacter(IDictionary<string, object> data)
        {
            TriggerServerEvent("EF:CreateCharacter", data);
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

            TriggerServerEvent("chars:serializeJson", data, new Action<string>((result) =>
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
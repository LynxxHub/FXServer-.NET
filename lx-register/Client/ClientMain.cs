using CitizenFX.Core;
using CitizenFX.Core.Native;
using System.Collections.Generic;
using System;
using static CitizenFX.Core.Native.API;
using Newtonsoft.Json;

namespace lx_register.Client
{
    public class ClientMain : BaseScript
    {

        private bool nui = false;

        public ClientMain()
        {
            RegisterEventHandlers();
            RegisterNUICommand();
        }

        private void RegisterNUICommand()
        {
            API.RegisterCommand("nui", new Action<int, List<object>, string>((source, args, raw) =>
            {
                if (nui)
                {
                    SetNuiFocus(false, false);
                    nui = false;

                }
                else
                {
                    SetNuiFocus(true, true);
                    nui = true;
                }


            }), false);
        }

        private void RegisterEventHandlers()
        {
            //EventHandlers["lx_register:ShowRegistrationUI"] += new Action(ShowRegistrationUI);
            RegisterNuiCallbackType("submitRegistrationForm");
            EventHandlers["__cfx_nui:submitRegistrationForm"] += new Action<IDictionary<string, object>, CallbackDelegate>((data, cb) =>
            {
                OnRegistrationFormSubmitted(data);
                cb();
            });

            EventHandlers["playerSpawned"] += new Action<dynamic>(ShowRegistrationUI);
        }

        private void ShowRegistrationUI(dynamic spawn)
        {

            Debug.WriteLine("Triggering NUI");
            // Enable NUI Focus - allows player to interact with the UI
            SetNuiFocus(true, true);
            nui = true;

            //// Prepare the data to be sent to the NUI
            var data = new
            {
                action = "showRegistrationForm",
                // You can send additional data to the UI if needed
            };

            //// Convert data to JSON format
            string jsonData = JsonConvert.SerializeObject(data);

            //// Send a message to the NUI to display the UI
            SendNuiMessage(jsonData);
        }

        private void OnRegistrationFormSubmitted(IDictionary<string, object> data)
        {
            Debug.WriteLine("OnRegistrationFormSubmitted");

            TriggerServerEvent("OnRegistrationDataReceived", data);

            SetNuiFocus(false, false);
            nui = false;
        }
    }
}
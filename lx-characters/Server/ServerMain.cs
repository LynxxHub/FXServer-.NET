using System;
using CitizenFX.Core;
using Newtonsoft.Json;

namespace lx_characters.Server
{
    public class ServerMain : BaseScript
    {
        //TODO: 1. Check gender validation 2.check music 3.character info icon 4. finish UI customization
        public ServerMain()
        {
            EventHandlers.Add("chars:setupCharactersRequest", new Action<Player>(OnSetupCharactersRequest));

            EventHandlers.Add("chars:serializeJson", new Action<dynamic, NetworkCallbackDelegate>((data, cb) =>
            {
                string json = JsonConvert.SerializeObject(data, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                Debug.WriteLine(json);
                cb(json);
            }));

        }


        private void OnSetupCharactersRequest([FromSource]Player player)
        {
            try
            {
                Debug.WriteLine(player.Name);
                Debug.WriteLine("DEBUG: SERVER-SIDE setupCharacters TRIGGERED");

                TriggerEvent("EF:GetUserCharacters", new Action<string>((cb) =>
                {
                    Debug.WriteLine("DEBUG: SERVER-SIDE GetUserCharacters TRIGGER");

                    Debug.WriteLine(cb);

                    TriggerClientEvent(player, "chars:setupCharactersResponse", cb);

                }), player.Handle, "setupCharacters");

            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.InnerException?.ToString());
                Debug.WriteLine(ex.StackTrace.ToString());
            }
        }

        [Command("hello_server")]
        public void HelloServer()
        {
            Debug.WriteLine("Sure, hello.");
        }
    }
}
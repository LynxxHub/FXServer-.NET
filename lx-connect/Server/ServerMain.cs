using CitizenFX.Core;
using Config.Reader;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using static CitizenFX.Core.Native.API;


namespace lx_connect.Server
{
    public class ServerMain : BaseScript
    {
        private readonly iniconfig _config;
        private readonly int _queueRefreshRate;

        public ServerMain()
        {
            _config = new iniconfig("lx-connect", "config.ini");
            _queueRefreshRate = _config.GetIntValue("General", "QueueRefreshRate", 0);

            EventHandlers["playerConnecting"] += new Action<Player, string, dynamic, dynamic>(OnPlayerConnecting);
        }

        [Command("hello_server")]
        public void HelloServer()
        {
            Debug.WriteLine("Sure, hello.");
        }

        private async void OnPlayerConnecting([FromSource] Player player, string playerName, dynamic setKickReason, dynamic deferrals)
        {

            Debug.WriteLine(GetPlayerIdentifierByType(player.Handle, "steam"));
            Dictionary<string, string> _userIdentifiers = new Dictionary<string, string>();
            deferrals.defer();
            //TODO: Fix ConfigReader!
            //string welcomeMessage = _config.GetStringValue("Language", "WelcomeMessage", "FAILED: NOT FOUND WelcomeMessage");
            deferrals.update($"Welcome to the LYNX server {playerName}, we're going to check your licenses. Please wait!");
            await Delay(3500);

            foreach (var identifier in player.Identifiers)
            {
                Debug.WriteLine(identifier);
                if (identifier.StartsWith("steam:"))
                {
                    string steamID = ConvertSteamIDHexToDec(identifier);
                    if (steamID != null)
                    _userIdentifiers.Add("Steam", identifier);
                }
                else if (identifier.StartsWith("license:"))
                    _userIdentifiers.Add("License", identifier);
                else if (identifier.StartsWith("ip:"))
                    _userIdentifiers.Add("IP", identifier);
                else if (identifier.StartsWith("discord:"))
                    _userIdentifiers.Add("Discord", identifier);
            }

            bool IsValid = await ValidateIdentifiersAsync(_userIdentifiers,deferrals);

            if(IsValid)
            {
                //TODO: Fix ConfigReader!
                //string joiningMessage = _config.GetStringValue("Language", "Joining", "FAILED: NOT FOUND Joining");

                deferrals.update("We are connecting you to the server...");
                await Delay(_queueRefreshRate + 3250);
                deferrals.done();
            }
            //TODO: Fix ConfigReader!
            //string errorMessage = _config.GetStringValue("Language", "Error", "FAILED: NOT FOUND Error");
            deferrals.done("[ERROR]: Please contact the owner(Lynx)!");
        }

        private async Task<bool> ValidateIdentifiersAsync(Dictionary<string,string> userIdentifiers, dynamic deferrals)
        {
            foreach (var id in userIdentifiers)
            {
                //TODO: Fix ConfigReader!
                //string licenseMessage = _config.GetStringValue("Language", "WelcomeMessage", "FAILED: NOT FOUND WelcomeMessage");
                //deferrals.update(string.Format(licenseMessage, id.Key, id.Value));

                deferrals.update($"Your {id.Key} - {id.Value} is being checked!");
                //TODO:Implament validations with DB
                await Delay(_queueRefreshRate + 2500);
            }

            return true;
        }

        public static string ConvertSteamIDHexToDec(string hexSteamID)
        {
            if (string.IsNullOrEmpty(hexSteamID) || !hexSteamID.StartsWith("steam:"))
            {
                return null;
            }

            string hexPart = hexSteamID.Replace("steam:", "");
            long decSteamID = long.Parse(hexPart, System.Globalization.NumberStyles.HexNumber);

            return "steam:" + decSteamID.ToString();
        }
    }
}
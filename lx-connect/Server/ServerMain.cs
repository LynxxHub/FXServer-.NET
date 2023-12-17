using CitizenFX.Core;
using lx_connect.Server.Manager;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace lx_connect.Server
{
    public class ServerMain : BaseScript
    {
        private readonly string _languageConst;
        private readonly int _queueRefreshRate;
        private readonly Dictionary<string, string> _config;
        private readonly Dictionary<string, string> _language;

        public ServerMain()
        {
            _config = ConfigManager.LoadConfig();
            _queueRefreshRate = int.Parse(_config["QueueRefreshRate"]);
            _languageConst = _config["Language"];
            _language = LanguageManager.LoadLanguage(_languageConst);

            EventHandlers["playerConnecting"] += new Action<Player, string, dynamic, dynamic>(OnPlayerConnecting);
        }

        private async void OnPlayerConnecting([FromSource] Player player, string playerName, dynamic setKickReason, dynamic deferrals)
        {


            deferrals.defer();

            deferrals.update(string.Format(_language["WelcomeMessage"],playerName));
            await Delay(3500);

            Dictionary<string, string> userIdentifiers = GetUserIdentifiers(player);

            bool IsValid = await ValidateIdentifiersAsync(player,userIdentifiers, deferrals);

            if(IsValid)
            {
                deferrals.update(_language["ConnectingMessage"]);
                await Delay(_queueRefreshRate + 3250);
                deferrals.done();
            }

            deferrals.done(_language["ErrorMessage"]);
        }

        private Dictionary<string, string> GetUserIdentifiers(Player player)
        {
            Dictionary<string, string> userIdentifiers = new Dictionary<string, string>();
            foreach (var identifier in player.Identifiers)
            {
                Debug.WriteLine(identifier);
                if (identifier.StartsWith("steam:"))
                {
                    string steamID = ConvertSteamIDHexToDec(identifier);
                    if (steamID != null)
                        userIdentifiers.Add("Steam", identifier);
                }
                else if (identifier.StartsWith("license:"))
                    userIdentifiers.Add("License", identifier);
                else if (identifier.StartsWith("ip:"))
                    userIdentifiers.Add("IP", identifier);
                else if (identifier.StartsWith("discord:"))
                    userIdentifiers.Add("Discord", identifier);
            }

            return userIdentifiers;
        }

        //TODO: call from lxEF
        private async Task<bool> ValidateIdentifiersAsync(Player player, Dictionary<string,string> userIdentifiers, dynamic deferrals)
        {
            var steamValid = false;
            var licenceValid = false;
            var ipValid = false;
            foreach (var id in userIdentifiers)
            {
                string emoji;
                switch (id.Key)
                {
                    case "Steam":
                        emoji = _language["SteamEmoji"];
                        steamValid = true;
                        break;
                    case "License":
                        emoji = _language["LicenseEmoji"];
                        licenceValid = true;
                        break;
                    case "IP":
                        emoji = _language["IPEmoji"];
                        ipValid = true;
                        break;
                    case "Discord":
                        emoji = _language["DiscordEmoji"];
                        break;
                    default:
                        emoji = _language["DefaultEmoji"];
                        break;
                }

                deferrals.update(string.Format(_language["CheckingLicenseMessage"], id.Key, emoji, id.Value));
                await Delay(_queueRefreshRate + 2500);
            }

            if (steamValid && licenceValid && ipValid)
            {
                deferrals.update(_language["AuthenticationMessage"]);

                TriggerEvent("EF:DoesUserExist", userIdentifiers["Steam"], userIdentifiers["License"], new Action<bool>(async exists =>
                {
                    Debug.WriteLine($"CONNECT: {exists}");

                    if (!exists)
                    {
                        TriggerEvent("EF:RegisterUser", player.Name, userIdentifiers["Steam"], userIdentifiers["License"], userIdentifiers["IP"]);
                        deferrals.update(_language["AuthenticationFailedMessage"]);
                        await Delay(5000);
                        deferrals.update(_language["LicenseRegistrationMessage"]);

                    }

                    if (exists)
                    {
                        deferrals.update(_language["AuthenticationSuccessMessage"]);
                    }
                }));

                await Delay(_queueRefreshRate + 5000);

                return true;
            }

            return false;
        }


        //TODO: call from lxEF
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
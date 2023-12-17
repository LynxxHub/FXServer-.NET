using CitizenFX.Core;
using lx_connect.Server.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace lx_connect.Server
{
    public class ServerMain : BaseScript
    {
        private readonly string _languageConst;
        private readonly int _queueRefreshRate;
        private readonly int _maxPlayers;
        private readonly Dictionary<string, string> _config;
        private readonly Dictionary<string, string> _language;
        private List<QueuePlayer> waitingList = new List<QueuePlayer>();

        public ServerMain()
        {
            _config = new Dictionary<string, string>();
            _config = ConfigManager.LoadConfig();
            _queueRefreshRate = int.Parse(_config["QueueRefreshRate"]);
            _languageConst = _config["Language"];
            _maxPlayers = int.Parse(_config["MaxPlayerCount"]);
            _language = LanguageManager.LoadLanguage(_languageConst);

            EventHandlers["playerConnecting"] += new Action<Player, string, dynamic, dynamic>(OnPlayerConnecting);
            EventHandlers["playerDropped"] += new Action<Player, string>(OnPlayerDropped);

            Task.Run(async () => await ProcessQueue());
        }

        private async Task ProcessQueue()
        {
            while (true)
            {
                if (waitingList.Any())
                {
                    if (Players.Count() < _maxPlayers)
                    {
                        waitingList[0].CanJoin = true;
                    } 
                }

                await Delay(10000);
            }
        }

        private void OnPlayerDropped(Player player, string reason)
        {
            var queuePlayer = waitingList.FirstOrDefault(p => p.Player.Handle == player.Handle);
            if (queuePlayer != null)
                waitingList.Remove(queuePlayer);
        }

        private async void OnPlayerConnecting([FromSource] Player player, string playerName, dynamic setKickReason, dynamic deferrals)
        {
            try
            {
                deferrals.defer();

                deferrals.update(string.Format(_language["WelcomeMessage"], playerName));
                await Delay(3500);

                Dictionary<string, string> userIdentifiers = GetUserIdentifiers(player);
                bool IsValid = await ValidateIdentifiersAsync(player, userIdentifiers, deferrals);

                if (IsValid)
                {
                    if (Players.Count() < _maxPlayers)
                    {
                        deferrals.update(_language["ConnectingMessage"]);
                        await Delay(_queueRefreshRate + 3250);
                        deferrals.done();
                    }
                    else
                    {
                        QueuePlayer queuePlayer = new QueuePlayer(player, DateTime.UtcNow, false);
                        waitingList.Add(queuePlayer);
                        await UpdateQueuePlayer(player.Handle, deferrals);
                    }
                }
                else
                {
                    deferrals.done(_language["ErrorMessage"]);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during player connection: {ex.Message}");
                deferrals.done($"An error occurred: {ex.Message}");
            }

            deferrals.done(_language["ErrorMessage"]);
        }

        private async Task UpdateQueuePlayer(string handle, dynamic deferrals)
        {
            while (true)
            {
                await Delay(1000);

                var queuePlayer = waitingList.FirstOrDefault(p => p.Player.Handle == handle);

                if (queuePlayer != null)
                {
                    if (queuePlayer.CanJoin)
                    {
                        deferrals.update(_language["ConnectingMessage"]);
                        waitingList.Remove(queuePlayer);
                        await Delay(_queueRefreshRate + 3250);
                        deferrals.done();
                        break;
                    }
                    else
                    {
                        // Calculate the player's position in the queue
                        int positionInQueue = waitingList.IndexOf(queuePlayer) + 1;

                        // Calculate the time in queue
                        TimeSpan timeInQueue = DateTime.UtcNow - queuePlayer.JoinedOn;
                        string timeInQueueFormatted = timeInQueue.ToString(@"mm\:ss");

                        // Update the deferral message with the queue position and time
                        string queueUpdateMessage = string.Format(_language["QueueUpdateMessage"], positionInQueue, timeInQueueFormatted);
                        deferrals.update(queueUpdateMessage);
                    }
                } 
                else
                {
                    break;
                }
                await Delay(0);


            }
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
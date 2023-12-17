using CitizenFX.Core;
using lx_connect.Server.Manager;
using lx_connect.Server.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


//TEST
//(GOD TEST && Drop test)
// 1. Viki join queue -> Lynx join as god - VERIFIED!
// 2. Lynx join queue -> Viki joins queue -> Lynx log in server -> Viki should be 1/1 -> Lynx left server -> Viki should change to 1/2

namespace lx_connect.Server
{
    public class ServerMain : BaseScript
    {
        private readonly Config _config;
        private readonly Dictionary<string, string> _language;
        private readonly List<QueuePlayer> _waitingList;
        private readonly Dictionary<string,DateTime> _droppedPlayers;
        private readonly PriorityManager _priorityManager;

        public ServerMain()
        {
            _config = ConfigManager.LoadConfig();
            _language = LanguageManager.LoadLanguage(_config.Language);
            _waitingList = new List<QueuePlayer>();
            _priorityManager = new PriorityManager(_config);
            _droppedPlayers = new Dictionary<string, DateTime>();

            RegisterEventHandlers();

            Task.Run(async () => await ProcessQueue());
            Task.Run(async () => await PeriodicCleanup());
        }

        private void RegisterEventHandlers()
        {
            EventHandlers["playerConnecting"] += new Action<Player, string, dynamic, dynamic>(OnPlayerConnecting);
            EventHandlers["playerDropped"] += new Action<Player, string>(OnPlayerDropped);
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

                    if (Players.Count() < _config.MaxPlayerCount)
                    {
                        deferrals.update(_language["ConnectingMessage"]);
                        await Delay(_config.QueueRefreshRate + 3250);
                        deferrals.done();
                    }
                    else
                    {
                        QueuePlayer queuePlayer = new QueuePlayer(player, userIdentifiers["Steam"]);

                        queuePlayer.IsGod = _priorityManager.IsPlayerGod(queuePlayer.SteamID);
                        queuePlayer.HasPriority = _priorityManager.HasPriority(queuePlayer.SteamID);


                        if (queuePlayer.IsGod)
                        {
                            int position = _priorityManager.FindGodPriorityPosition(_waitingList);
                            if (!IsPlayerWaiting(queuePlayer.SteamID))
                                _waitingList.Insert(position, queuePlayer);
                        }
                        else if (queuePlayer.HasPriority || _droppedPlayers.ContainsKey(queuePlayer.SteamID))
                        {
                            int position = _priorityManager.FindPlayerPriorityPosition(_waitingList);
                            if (!IsPlayerWaiting(queuePlayer.SteamID))
                                _waitingList.Insert(position, queuePlayer);

                            if (_droppedPlayers.ContainsKey(queuePlayer.SteamID))
                                _droppedPlayers.Remove(queuePlayer.SteamID);

                        } 
                        else
                        {
                            if (!IsPlayerWaiting(queuePlayer.SteamID))
                                _waitingList.Add(queuePlayer);
                        }

                        await UpdateQueuePlayer(userIdentifiers["Steam"], deferrals);
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
                Console.WriteLine($"INNER: {ex.InnerException}");
                Console.WriteLine($"STACK: {ex.StackTrace}");
                deferrals.done($"An error occurred: {ex.Message}");
            }
        }

        private bool IsPlayerWaiting(string steamID)
        {
            return _waitingList.Any(p => p.SteamID == steamID);
        }

        [Command("PlusOne")]
        public void PlusOne()
        {
            _config.MaxPlayerCount++;
        }

        [Command("MinusOne")]
        public void MinusOne()
        {
            _config.MaxPlayerCount--;
        }

        private void OnPlayerDropped([FromSource] Player player, string reason)
        {
            var identifiers = GetUserIdentifiers(player);

            _droppedPlayers.Add(identifiers["Steam"],DateTime.UtcNow);
            Debug.WriteLine($"Player {player.Name} dropped! REASON: {reason}");
        }

        private async Task ProcessQueue()
        {
            while (true)
            {
                if (_waitingList.Any())
                {
                    if (Players.Count() < _config.MaxPlayerCount)
                    {
                        _waitingList[0].CanJoin = true;
                    }
                }

                await Delay(10000);
            }
        }

        private async Task UpdateQueuePlayer(string steamId, dynamic deferrals)
        {
            while (true)
            {
                var queuePlayer = _waitingList.FirstOrDefault(p => p.SteamID == steamId);

                if (queuePlayer != null)
                {
                    if (queuePlayer.CanJoin)
                    {
                        deferrals.update(_language["ConnectingMessage"]);
                        _waitingList.Remove(queuePlayer);
                        await Delay(_config.QueueRefreshRate + 3250);
                        deferrals.done();
                        break;
                    }
                    else
                    {
                        int positionInQueue = _waitingList.IndexOf(queuePlayer) + 1;

                        TimeSpan timeInQueue = DateTime.UtcNow - queuePlayer.JoinedOn;
                        string timeInQueueFormatted = timeInQueue.ToString(@"mm\:ss");

                        string queueUpdateMessage = string.Format(_language["QueueUpdateMessage"], positionInQueue, _waitingList.Count, timeInQueueFormatted);
                        deferrals.update(queueUpdateMessage);
                    }
                }
                else
                {
                    break;
                }

                await Delay(1000);
            }
        }


        //TODO: call from lxEF
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
                        userIdentifiers.Add("Steam", steamID);
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
        private async Task<bool> ValidateIdentifiersAsync(Player player, Dictionary<string, string> userIdentifiers, dynamic deferrals)
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
                await Delay(_config.QueueRefreshRate + 2500);
            }

            Debug.WriteLine(steamValid.ToString());
            Debug.WriteLine(licenceValid.ToString());
            Debug.WriteLine(ipValid.ToString());

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

                await Delay(_config.QueueRefreshRate + 5000);

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

        private async Task PeriodicCleanup()
        {
            while (true)
            {
                await Delay(60 * 1000);

                var cutoffTime = DateTime.UtcNow.AddMinutes(-15);
                var keysToRemove = _droppedPlayers.Where(kvp => kvp.Value < cutoffTime)
                                                  .Select(kvp => kvp.Key)
                                                  .ToArray();

                foreach (var key in keysToRemove)
                {
                    _droppedPlayers.Remove(key);
                }
            }
        }
    }
}
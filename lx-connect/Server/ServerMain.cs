using CitizenFX.Core;
using CitizenFX.Core.Native;
using lx_connect.Server.Manager;
using lx_connect.Server.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace lx_connect.Server
{
    public class ServerMain : BaseScript
    {
        private readonly ConfigManager _configManager;
        private readonly Config _config;
        private readonly Dictionary<string, string> _language;
        private readonly List<QueuePlayer> _waitingList;
        private readonly Dictionary<string,DateTime> _droppedPlayers;
        private readonly PriorityManager _priorityManager;

        public ServerMain()
        {
            _configManager = new ConfigManager();
            _config = _configManager.LoadConfig();
            _language = LanguageManager.LoadLanguage(_config.Language);
            _waitingList = new List<QueuePlayer>();
            _priorityManager = new PriorityManager(_config);
            _droppedPlayers = new Dictionary<string, DateTime>();

            if (_config.StopHardCap)
            {
                API.ExecuteCommand("stop hardcap");
            }

            if (_config.Debug)
            {
                RegisterDebugCommands();
            }

            RegisterCommands();
            RegisterEventHandlers();

            Task.Run(async () => await ProcessQueue());
            Task.Run(async () => await PeriodicCleanup());
        }


        // Handles the player connection process, including queue and priority management.
        // TODO: Move this to QueueManager
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

        // Manages actions when a player disconnects from the server.
        // TODO: Move this to QueueManager
        private void OnPlayerDropped([FromSource] Player player, string reason)
        {
            var identifiers = GetUserIdentifiers(player);

            if (_config.DroppedPriority)
            {
                _droppedPlayers.Add(identifiers["Steam"],DateTime.UtcNow);
            }

            Debug.WriteLine($"Player {player.Name} dropped! REASON: {reason}");
        }


        // Checks if a player is already waiting in the queue.
        // TODO: Move this to QueueManager
        private bool IsPlayerWaiting(string steamID)
        {
            return _waitingList.Any(p => p.SteamID == steamID);
        }

        // Extracts and returns player identifiers like Steam ID and license.
        //TODO: call from lxEF (When ready)
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

        // Asynchronously validates player identifiers during the connection process.
        //TODO: call from lxEF (When ready)
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

            if (steamValid && licenceValid && ipValid)
            {
                deferrals.update(_language["AuthenticationMessage"]);

                TriggerEvent("EF:DoesUserExist", userIdentifiers["Steam"], userIdentifiers["License"], new Action<bool>(exists =>
                {
                    Debug.WriteLine($"CONNECT: {exists}");

                    if (!exists)
                    {
                        TriggerEvent("EF:RegisterUser", player.Name, userIdentifiers["Steam"], userIdentifiers["License"], userIdentifiers["IP"]);
                        deferrals.update(_language["AuthenticationFailedMessage"]);
                        Delay(5000);
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

        // Processes the player queue, allowing entry based on server capacity and priority.
        // TODO: Move this to QueueManager
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

        // Updates a player's queue status, providing information about their queue position.
        // TODO: Move this to QueueManager
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

                        if (_config.ShowPriorities)
                        {
                            int priorityPlayersCount = 0;

                            foreach (var player in _waitingList)
                            {
                                if (player.IsGod || player.HasPriority)
                                    priorityPlayersCount++;
                            }

                            queueUpdateMessage = string.Format(_language["QueueUpdateMessagePriority"], positionInQueue, _waitingList.Count, timeInQueueFormatted, priorityPlayersCount);
                        }

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

        // Converts a Steam ID from hexadecimal to decimal format.
        //TODO: call from lxEF (When ready)
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

        // Periodically cleans up the list of dropped players, managing server resources.
        private async Task PeriodicCleanup()
        {
            while (true)
            {
                await Delay((_config.DroppedPriorityTime) * 60 * 1000);

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

        // Registers server commands like adding/removing player priorities.
        private void RegisterCommands()
        {
            API.RegisterCommand("addpriority", new Action<int, List<object>, string>((source, args, raw) =>
            {
                //TODO: Add customized validation
                string username = args[0].ToString();
                string steamID = args[1].ToString();

                _priorityManager.AddPriority(username, steamID);
                _configManager.UpdateConfig(_config);

            }), true);

            API.RegisterCommand("removepriority", new Action<int, List<object>, string>((source, args, raw) =>
            {
                //TODO: Add customized validation
                string username = args[0].ToString();
                string steamID = args[1].ToString();

                _priorityManager.RemovePriority(steamID);
                _configManager.UpdateConfig(_config);

            }), true);
        }

        // Registers debug commands for testing purposes.(Only if Debug mode enabled)
        private void RegisterDebugCommands()
        {
            API.RegisterCommand("increase", new Action<int, List<object>, string>((source, args, raw) =>
            {
                _config.MaxPlayerCount++;
            }), false);

            API.RegisterCommand("increase", new Action<int, List<object>, string>((source, args, raw) =>
            {
                _config.MaxPlayerCount--;
            }), false);
        }

        // Sets up event handlers for player connections and disconnections.
        private void RegisterEventHandlers()
        {
            EventHandlers["playerConnecting"] += new Action<Player, string, dynamic, dynamic>(OnPlayerConnecting);
            EventHandlers["playerDropped"] += new Action<Player, string>(OnPlayerDropped);
        }
    }
}
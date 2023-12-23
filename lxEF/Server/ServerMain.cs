using CitizenFX.Core;
using System.Linq;
using CitizenFX.Core.Native;
using lxEF.Server.Data;
using lxEF.Server.Data.Models;
using lxEF.Server.Managers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using lxEF.Server.Data.DTO;

namespace lxEF.Server
{
    public class ServerMain : BaseScript
    {
        private CharacterManager _characterManager;
        public static List<DBUser> DBUsers { get; private set; }

        public ServerMain()
        {
            _characterManager = new CharacterManager();

            RegisterEventHandlers();
            LoadUsers();
        }

        public static Task LoadUsers()
        {
            Task.Run(async () =>
            {
                await Delay(0);
                DBUsers = await DBUserManager.LoadUsersAsync(); 
            });

            return Task.CompletedTask;
        }

        private void RegisterEventHandlers()
        {
            EventHandlers["EF:GetUserCharacters"] += new Action<CallbackDelegate, int, string>(OnGetUserCharacters);
            EventHandlers["EF:RegisterUser"] += new Action<string, string, string, string>(OnRegisterUser);
            EventHandlers["playerDropped"] += new Action<Player, string>(OnPlayerDropped);
            EventHandlers.Add("EF:CreateCharacter", new Action<Player, dynamic>(OnCreateCharacter));
            EventHandlers.Add("EF:RemoveCharacter", new Action<Player, string>(OnRemoveCharacter));
            EventHandlers.Add("EF:DoesUserExist", new Action<string, string, CallbackDelegate>(async (steamID, license, callback) =>
            {
                Debug.WriteLine("EF:DoesUserExist TRIGGER");
                var exists = await DoesUserExistAsync(steamID, license);
                await Delay(100);
                Debug.WriteLine($"Does Exists: {exists}");
                callback(exists);
            }));
        }

        private void OnGetUserCharacters(CallbackDelegate callback, int playerHandle, string action = "")
        {
            try
            {
                var player = Players[playerHandle];
                Debug.WriteLine(player.Name);
                var identifiers = GetUserIdentifiers(player);
                Debug.WriteLine(identifiers["Steam"]);
                Debug.WriteLine(identifiers["License"]);
                DBUser dbUser = DBUsers.FirstOrDefault(u => u.SteamID == identifiers["Steam"] && u.License == identifiers["License"]);

                var data = new
                {
                    action = action,
                    characters = dbUser.Characters,
                };

                string json = JsonConvert.SerializeObject(data);
                callback.Invoke(json);
            }
            catch (Exception ex)
            {
                LoggingManager.PrintExceptions(ex);
            }

        }

        private void OnRemoveCharacter([FromSource] Player player, string citizenId)
        {
            
            try
            {
                var identifiers = GetUserIdentifiers(player);
                Character character = _characterManager.GetCharacter(citizenId);

                bool steamAuth = identifiers["Steam"] == character.User.SteamID;
                bool licenseAuth = identifiers["License"] == character.User.License;

                if (steamAuth && licenseAuth)
                {
                    var success = Task.Run(async () => { return await _characterManager.RemoveCharacterAsync(citizenId, character.User.Username); });
                }
            }
            catch (Exception ex) { LoggingManager.PrintExceptions(ex); }

        }

        private void OnCreateCharacter([FromSource] Player player, dynamic data)
        {
            var jsonData = JsonConvert.SerializeObject(data);
            CharacterDTO characterDTO = JsonConvert.DeserializeObject<CharacterDTO>(jsonData);

            Task.Run(async () =>
            {
                if (player != null && characterDTO.FirstName != null && characterDTO.LastName != null && characterDTO.Nationality != null && characterDTO.DateOfBirth != null && characterDTO.Gender != null)
                {
                    var identifiers = GetUserIdentifiers(player);
                    //TODO: Implement caching
                    var dbUser = await DBUserManager.GetDBUserAsync(identifiers["Steam"], identifiers["License"]);
                    if (dbUser != null)
                    {
                        await _characterManager.CreateCharacterAsync(characterDTO, dbUser);
                    }
                    else
                    {
                        Debug.WriteLine("dbUser is null (CreateUserExport)");
                    }
                }
            });
        }

        //FixAsync
        private async void OnPlayerDropped([FromSource] Player player, string reason)
        {
            var identifiers = GetUserIdentifiers(player);

            await DBUserManager.LogoutAsync(identifiers["Steam"], identifiers["License"]);
        }

        //FixAsync
        public async void OnRegisterUser(string username, string steamId, string license, string ip)
        {
            try
            {
                var result = await DBUserManager.CreateUserAsync(username, steamId, license, ip);

                if (result == CreateUserResult.UserCreatedSuccessfully)
                    Debug.WriteLine($"EF: User - {username} created");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error: " + ex.Message);
                Debug.WriteLine("INNER: " + ex.InnerException);
                Debug.WriteLine("STACK: " + ex.StackTrace);
            }
        }

        public async Task<bool> DoesUserExistAsync(string steamID, string license)
        {
            try
            {
                using (var context = new lxDbContext())
                {
                    var dbUser = await DBUserManager.GetDBUserAsync(steamID, license);

                    if (dbUser == null)
                        return false;

                    dbUser.Authenticate();
                    await context.SaveChangesAsync();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

        }

        //TODO: move to user manager
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

        private string ConvertSteamIDHexToDec(string hexSteamID)
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
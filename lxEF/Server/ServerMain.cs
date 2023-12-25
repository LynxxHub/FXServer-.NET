using CitizenFX.Core;
using lxEF.Server.Data.DTO;
using lxEF.Server.Data.Models;
using lxEF.Server.Managers;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace lxEF.Server
{
    public class ServerMain : BaseScript
    {
        private readonly DBUserManager _dbUserManager;
        private readonly CharacterManager _characterManager;

        public ServerMain()
        {
            _dbUserManager = new DBUserManager();
            _characterManager = new CharacterManager(_dbUserManager);

            RegisterEventHandlers();
        }

        private void RegisterEventHandlers()
        {
            EventHandlers.Add("EF:GetUserCharacters", new Action<CallbackDelegate, int, string>(OnGetUserCharacters));
            EventHandlers.Add("EF:RegisterUser", new Action<string, string, string, string>(OnRegisterUser));
            EventHandlers.Add("playerDropped", new Action<Player, string>(OnPlayerDropped));
            EventHandlers.Add("EF:CreateCharacter", new Action<Player, dynamic>(OnCreateCharacter));
            EventHandlers.Add("EF:RemoveCharacter", new Action<Player, string>(OnRemoveCharacter));
            EventHandlers.Add("EF:DoesUserExist", new Action<string, string, CallbackDelegate>((steamID, license, callback) =>
            {
                bool exists = _dbUserManager.GetDBUser(steamID, license) != null;
                Debug.WriteLine($"EXISTING: {exists}");
                callback(exists);
            }));
        }

        private void OnGetUserCharacters(CallbackDelegate callback, int playerHandle, string action = "")
        {
            try
            {
                var player = Players[playerHandle];
                Debug.WriteLine(player.Name);
                var identifiers = _dbUserManager.GetUserIdentifiers(player);
                Debug.WriteLine(identifiers["Steam"]);
                Debug.WriteLine(identifiers["License"]);
                DBUser dbUser = _dbUserManager.GetDBUser(identifiers["Steam"], identifiers["License"]);

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
                var identifiers = _dbUserManager.GetUserIdentifiers(player);
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
                    var identifiers = _dbUserManager.GetUserIdentifiers(player);
                    var dbUser = await _dbUserManager.GetDBUserAsync(identifiers["Steam"], identifiers["License"]);
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

        private void OnPlayerDropped([FromSource] Player player, string reason)
        {
            var identifiers = _dbUserManager.GetUserIdentifiers(player);
            _dbUserManager.GetDBUser(identifiers["Steam"], identifiers["License"]).Logout();
        }

        public void OnRegisterUser(string username, string steamId, string license, string ip)
        {
            try
            {
                CreateUserResult result = Task.Run(async () =>
                {
                    var actionResult = await _dbUserManager.CreateUserAsync(username, steamId, license, ip);
                    return actionResult;
                }).Result;

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
    }
}
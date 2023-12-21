using CitizenFX.Core;
using CitizenFX.Core.Native;
using lxEF.Server.Data;
using lxEF.Server.Data.Models;
using lxEF.Server.Managers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace lxEF.Server
{
    //TODO: Change EH with exports!
    public class ServerMain : BaseScript
    {
        private CharacterManager _characterManager;
        public ServerMain()
        {
            _characterManager = new CharacterManager();
            RegisterEventHandlers();
            RegisterExports();
        }

        private void RegisterEventHandlers()
        {
            EventHandlers["EF:RegisterUser"] += new Action<string, string, string, string>(OnRegisterUser);
            EventHandlers["playerDropped"] += new Action<Player, string>(OnPlayerDropped);
            EventHandlers.Add("EF:DoesUserExist", new Action<string, string, CallbackDelegate>(async (steamID, license, callback) =>
            {
                Debug.WriteLine("EF:DoesUserExist TRIGGER");
                var exists = await DoesUserExistAsync(steamID, license);
                await Delay(100);
                Debug.WriteLine($"Does Exists: {exists}");
                callback(exists);
            }));
        }


        private void RegisterExports()
        {
            Exports.Add("CreateCharacterAsync", new Action<Player, string, string, int, DateTime, string, string>(OnCreateCharacter));
            EventHandlers.Add("GetUserCharacters", new Action<Player, CallbackDelegate>(async (player, callback) =>
            {
                Debug.WriteLine("DEBUG: SERVER-SIDE GetUserCharacters TRIGGERED");

                var identifiers = GetUserIdentifiers(player);
                DBUser dbUser = await DBUserManager.GetDBUserAsync(identifiers["Steam"], identifiers["License"]);
                Debug.WriteLine(dbUser.SteamID.ToString());
                callback(dbUser.Characters);
            }));
            Exports.Add("RemoveCharacter", new Action<Player,string>(OnRemoveCharacter));
        }

        private void OnRemoveCharacter([FromSource]Player player, string citizenId)
        {
            //TODO
        }

        private async void OnCreateCharacter([FromSource] Player player, string firstName, string lastName, int age, DateTime dob, string gender, string nationality)
        {
            if (player != null && firstName != null && lastName != null && age != 0 && dob != null && gender != null)
            {
                var identifiers = GetUserIdentifiers(player);
                //TODO: Implement caching
                var dbUser = await DBUserManager.GetDBUserAsync(identifiers["Steam"], identifiers["License"]);
                if (dbUser != null)
                {
                    await _characterManager.CreateCharacterAsync(firstName, lastName, age, dob, gender, nationality, dbUser);
                }
                else
                {
                    Debug.WriteLine("dbUser is null (CreateUserExport)");
                }
            }
        }

        private async void OnPlayerDropped([FromSource] Player player, string reason)
        {
            var identifiers = GetUserIdentifiers(player);

            await DBUserManager.LogoutAsync(identifiers["Steam"], identifiers["License"]);
        }


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
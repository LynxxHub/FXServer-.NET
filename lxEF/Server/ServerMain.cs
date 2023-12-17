using CitizenFX.Core;
using lxEF.Server.Data;
using lxEF.Server.Data.Models;
using lxEF.Server.Factories;
using lxEF.Server.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace lxEF.Server
{
    public class ServerMain : BaseScript
    {
        public ServerMain()
        {
            RegisterUserHandler();
            DoesPlayerExistHandler();
            OnPlayerDroppedHandler();
        }

        private void RegisterUserHandler()
        {
            EventHandlers["EF:RegisterUser"] += new Action<string,string,string,string>(OnRegisterUser);

        }

        private void OnPlayerDroppedHandler()
        {
            EventHandlers["playerDropped"] += new Action<Player, string>(OnPlayerDropped);
        }

        private void DoesPlayerExistHandler()
        {
            EventHandlers.Add("EF:DoesUserExist", new Action<string, string, CallbackDelegate>(async (steamID, license, callback) =>
            {
                Debug.WriteLine("EF:DoesUserExist TRIGGER");
                var exists = await DoesUserExistAsync(steamID, license);
                await Delay(100);
                Debug.WriteLine($"Does Exists: {exists}");
                callback(exists);
            }));
        }

        private async void OnPlayerDropped([FromSource]Player player, string reason)
        {
            var identifiers = GetUserIdentifiers(player);

            await DBUserManager.LogoutAsync(identifiers["Steam"], identifiers["License"]);
        }


        public async void OnRegisterUser(string username, string steamId, string license, string ip)
        {
            Debug.WriteLine("THIS IS THE 1st LINE FOR DEBUG");
            try
            {
                Debug.WriteLine("THIS IS THE 2nd LINE FOR DEBUG");
                await DBUserFactory.CreateUserAsync(username, steamId, license, ip);
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
            } catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

        }

        private Dictionary<string, string> GetUserIdentifiers(Player player)
        {
            Debug.WriteLine("THIS IS THE 3rd LINE FOR DEBUG");

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
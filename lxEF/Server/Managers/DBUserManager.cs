using CitizenFX.Core;
using lxEF.Server.Data;
using lxEF.Server.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace lxEF.Server.Managers
{
    public class DBUserManager
    {
        private readonly object _lockObject = new object();

        public List<DBUser> DBUsers { get; set; }

        public DBUserManager()
        {
            Task.Run(async () => await LoadUsersAsync());
            DatabaseSyncTask();
        }

        public async Task<bool> LoadUsersAsync()
        {
            try
            {
                using (var context = new lxDbContext())
                {
                    DBUsers = await context.DBUsers.Include(u => u.Characters).ToListAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                LoggingManager.PrintExceptions(ex);
                return false;
            }
        }
        public async Task<CreateUserResult> CreateUserAsync(string username, string steamID, string license, string ip)
        {
            try
            {
                var existingUser = await GetDBUserAsync(steamID, license);
                if (existingUser != null)
                {
                    return CreateUserResult.UserAlreadyExists;
                }

                var userID = Guid.NewGuid().ToString();
                var dbUser = new DBUser(userID, username, steamID, license, ip);

                var isBanned = IsUserBannedAsync(dbUser);

                if (isBanned)
                {
                    return CreateUserResult.UserIsBanned;
                }

                dbUser.Authenticate();

                using (var context = new lxDbContext())
                {
                    var result = context.DBUsers.Add(dbUser);
                    if (result != null)
                    {
                        await context.SaveChangesAsync();
                        SyncAllUsers();
                        await LoadUsersAsync();
                        return CreateUserResult.UserCreatedSuccessfully;
                    }
                }

                return CreateUserResult.ErrorOccurred;
            }
            catch (Exception ex)
            {
                LoggingManager.PrintExceptions(ex);
                return CreateUserResult.ErrorOccurred;
            }
        }

        public async Task<DBUser> GetDBUserAsync(string steamID, string license)
        {
            Debug.WriteLine("GetDBUserAsync TRIGGERED");
            try
            {
                using (var context = new lxDbContext())
                {
                    var dbUser = await context.DBUsers.Include(u => u.Characters).FirstOrDefaultAsync(dbp => dbp.SteamID == steamID && dbp.License == license);
                    if (dbUser != null)
                        return dbUser;

                    return null;
                }
            }
            catch (Exception ex)
            {
                LoggingManager.PrintExceptions(ex);
                return null;
            }

        }

        public DBUser GetDBUser(string steamID, string license)
        {
            try
            {
                var dbUser = DBUsers.FirstOrDefault(dbu => dbu.SteamID == steamID && dbu.License == license);

                if (dbUser != null)
                    return dbUser;

                return null;
            }
            catch (Exception ex)
            {
                LoggingManager.PrintExceptions(ex);
                return null;
            }

        }

        public bool Authenticate(string steamId, string license)
        {
            try
            {
                var dbUser = GetDBUser(steamId, license);
                if (dbUser != null)
                {
                    bool isAuthenticated = dbUser.Authenticate();

                    return isAuthenticated;
                }

                return false;
            }
            catch (Exception ex)
            {
                LoggingManager.PrintExceptions(ex);

                return false;
            }
        }

        public bool Logout(string steamID, string license)
        {
            try
            {
                var dbUser = GetDBUser(steamID, license);
                if (dbUser != null)
                {
                    dbUser.Logout();
                    return true;
                }
            }
            catch (Exception ex)
            {
                LoggingManager.PrintExceptions(ex);
                return false;
            }
            return false;
        }

        public Dictionary<string, string> GetUserIdentifiers(Player player)
        {
            Dictionary<string, string> userIdentifiers = new Dictionary<string, string>();
            foreach (var identifier in player.Identifiers)
            {
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

        public bool IsUserBannedAsync(DBUser user)
        {
            //TODO: Implement validation trough custom banlist

            return user.IsBanned;
        }


        //Caching mechanism
        //TODO: Change into a service
        private void SyncUser(DBUser cachedUser)
        {
            lock (_lockObject)
            {
                using (var context = new lxDbContext())
                {
                    var dbUser = context.DBUsers.FirstOrDefault(u => u.UserId == cachedUser.UserId);
                    context.Entry(dbUser).CurrentValues.SetValues(cachedUser);
                    context.SaveChanges();
                }
            }
        }

        public void SyncAllUsers()
        {
            lock (_lockObject)
            {
                using (var context = new lxDbContext())
                {
                    foreach (var cachedUser in DBUsers)
                    {
                        var dbUser = context.DBUsers.FirstOrDefault(u => u.UserId == cachedUser.UserId);
                        if (dbUser != null)
                        {
                            context.Entry(dbUser).CurrentValues.SetValues(cachedUser);
                        }
                    }

                    context.SaveChanges();
                }
            }
        }

        private void DatabaseSyncTask()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    //make configurable
                    await Task.Delay(5 * 60 * 1000);
                    SyncAllUsers();
                }
            });
        }
    }
}

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
    //TODO: IMPLEMENT CACHING!!
    public static class DBUserManager
    {
        public static async Task<List<DBUser>> LoadUsersAsync()
        {
            try
            {
                using (var context = new lxDbContext())
                {
                    return await context.DBUsers.Include(u => u.Characters).ToListAsync();
                }
            }
            catch (Exception ex)
            {
                LoggingManager.PrintExceptions(ex);
                return null;
            }
        }
        public static async Task<CreateUserResult> CreateUserAsync(string username, string steamID, string license, string ip)
        {
            try
            {
                var existingUser = await DBUserManager.GetDBUserAsync(steamID, license);
                if (existingUser != null)
                {
                    return CreateUserResult.UserAlreadyExists;
                }

                var userID = Guid.NewGuid().ToString();
                var dbUser = new DBUser(userID, username, steamID, license, ip);

                var isBanned = await DBUserManager.IsUserBannedAsync(dbUser);

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

        public static async Task<DBUser> GetDBUserAsync(string steamID, string license)
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

        public static DBUser GetDBUser(string steamID, string license)
        {
            Debug.WriteLine("GetDBUserAsync TRIGGERED");
            try
            {
                using (var context = new lxDbContext())
                {
                    Debug.WriteLine(steamID);
                    Debug.WriteLine(license);

                    var dbUser = context.DBUsers.FirstOrDefault(dbp => dbp.SteamID == steamID && dbp.License == license);
                    
                    Debug.WriteLine(dbUser.IP);
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


        public static async Task<bool> AuthenticateAsync(DBUser dbUser)
        {
            try
            {
                using (var context = new lxDbContext())
                {
                    if (dbUser != null)
                    {
                        bool isAuthenticated = dbUser.Authenticate();
                        if (isAuthenticated)
                            await context.SaveChangesAsync();

                        return isAuthenticated;
                    }

                    return false;
                }
            }
            catch (Exception ex)
            {
                LoggingManager.PrintExceptions(ex);

                return false;
            }
        }

        public static async Task<bool> LogoutAsync(string steamID, string license)
        {
            try
            {
                using (var context = new lxDbContext())
                {
                    var dbUser = await context.DBUsers.FirstOrDefaultAsync(dbp => dbp.SteamID == steamID && dbp.License == license);
                    if (dbUser != null)
                    {
                        dbUser.Logout();
                        await context.SaveChangesAsync();
                        return true;
                    }

                }
            }
            catch (Exception ex)
            {
                LoggingManager.PrintExceptions(ex);
                return false;
            }
            return false;

        }

        public static async Task<bool> IsUserBannedAsync(DBUser user)
        {
            //TODO: Implement validation trough custom banlist
            //DELETE THIS:
            await GetDBUserAsync(user.SteamID, user.License);
            return user.IsBanned;
        }
    }
}

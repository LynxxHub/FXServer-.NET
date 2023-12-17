using CitizenFX.Core;
using lxEF.Server.Data;
using lxEF.Server.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace lxEF.Server.Managers
{
    public static class DBUserManager
    {

        public static async Task<DBUser> GetDBUserAsync(string steamID, string license)
        {
            try
            {
                using (var context = new lxDbContext())
                {
                    var dbUser = await context.DBUsers.FirstOrDefaultAsync(dbp => dbp.SteamID == steamID && dbp.License == license);
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

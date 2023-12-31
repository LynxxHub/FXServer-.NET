﻿using lxEF.Server.Data;
using lxEF.Server.Data.Models;
using lxEF.Server.Managers;
using System;
using System.Threading.Tasks;

namespace lxEF.Server.Factories
{
    public static class DBUserFactory
    {
        public static async Task<CreateUserResult> CreateUserAsync(string username, string steamID, string license, string ip, DBUserManager dbUserManager)
        {
            try
            {
                var existingUser = await dbUserManager.GetDBUserAsync(steamID, license);
                if (existingUser != null)
                {
                    return CreateUserResult.UserAlreadyExists;
                }

                var userID = Guid.NewGuid().ToString();
                var dbUser = new DBUser(userID, username, steamID, license, ip);

                var isBanned = dbUserManager.IsUserBannedAsync(dbUser);

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
    }
}

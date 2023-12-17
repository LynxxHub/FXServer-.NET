using System;
using System.ComponentModel.DataAnnotations;

namespace lxEF.Server.Data.Models
{
    public class DBUser
    {
        [Key]
        public string UserId { get; private set; }
        public string Username { get; private set; }
        public string SteamID { get; private set; }
        public string License { get; private set; }
        public string IP { get; private set; }
        public bool IsAdmin { get; private set; }
        public bool IsBanned { get; private set; }
        public bool IsAuthenticated { get; private set; }

        // Constructor
        public DBUser()
        {

        }

        public DBUser(string userId, string username, string steamID, string license,
                        string ip)
        {
            UserId = userId;
            Username = username;
            SteamID = steamID;
            License = license;
            IP = ip;
            IsAdmin = false;
            IsBanned = false;
            IsAuthenticated = false;
        }


        public bool Authenticate()
        {
            if (!IsBanned)
            {
                IsAuthenticated = true;
                return true;
            }

            return false;
        }

        public void Logout()
        {
            if (IsAuthenticated)
                IsAuthenticated = false;
        }
    }
}
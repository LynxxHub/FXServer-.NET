using CitizenFX.Core;
using lxEF.Server.Data;
using lxEF.Server.Data.Models;
using System;
using System.Linq;

namespace lxEF.Server
{
    public class ServerMain : BaseScript
    {
        public ServerMain()
        {
            Debug.WriteLine("Hi from lxEF.Server!");
        }

        [Command("hello_server")]
        public void HelloServer()
        {
            try
            {
                using (var context = new lxDbContext())
                {
                    Debug.WriteLine("Instance created");
                    var user = new User();
                    user.Username = "username";
                    user.Email = "email";

                    context.Users.Add(user);
                    context.SaveChanges();
                    Debug.WriteLine("User created");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error: " + ex.Message);
                Debug.WriteLine("INNER: " + ex.InnerException);
                Debug.WriteLine("STACK: " + ex.StackTrace);
            }
        }

        [EventHandler("EF:RegisterUser")]
        public async void OnRegisterUser(string username, int age)
        {
            Debug.WriteLine($"EF: {username} - Age: {age}");
            try
            {
                using (var context = new lxDbContext())
                {
                    Debug.WriteLine("EF: Instance created");
                    var user = new User();
                    user.Username = username;
                    user.Email = $"{username}@LXServer.com";

                    context.Users.Add(user);
                    await context.SaveChangesAsync();
                    Debug.WriteLine($"EF: User - {username} created");
                }
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
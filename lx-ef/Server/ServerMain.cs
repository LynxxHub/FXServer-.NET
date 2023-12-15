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

        [EventHandler("OnPlayerConnected")]
        public void OnPlayerConnected()
        {
            Debug.WriteLine("OnPlayerConnected triggered");
            using (var context = new lxDbContext())
            {
                // Use your DbContext here to interact with the database
                // For example, to query users:
                var users = context.Users.ToList();

                // Or to add a new user:
                var newUser = new User { Username = "newuser", Email = "newuser@example.com" };
                context.Users.Add(newUser);
                context.SaveChanges();
                Debug.WriteLine("User added");
            }
        }
    }
}
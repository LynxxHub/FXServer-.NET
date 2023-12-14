using CitizenFX.Core;
using System;
using System.Collections.Generic;

namespace lx_register.Server
{
    public class ServerMain : BaseScript
    {
        public ServerMain()
        {
            Debug.WriteLine("Hi from lx_register.Server!");
        }

        [EventHandler("OnRegistrationDataReceived")]
        private void OnRegistrationDataReceived(IDictionary<string, object> data)
        {
            Debug.WriteLine("TRIGGER!!!!!");

            string name = data["name"].ToString();
            int age = Convert.ToInt32(data["age"]);

            Debug.WriteLine(name+ " " + age);
        }

    }
}
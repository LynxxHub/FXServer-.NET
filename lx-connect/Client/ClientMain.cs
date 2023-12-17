using System;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace lx_connect.Client
{
    public class ClientMain : BaseScript
    {
        public ClientMain()
        {
            Debug.WriteLine("Hi from lx_connect.Client!");
        }

        [Tick]
        public Task OnTick()
        {

            return Task.FromResult(0);
        }
    }
}
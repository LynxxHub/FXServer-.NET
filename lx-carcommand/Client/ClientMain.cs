using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace lx_register.Client
{
    public class ClientMain : BaseScript
    {
        public ClientMain()
        {
            API.RegisterCommand("car", new Action<int, List<object>, string>((source, args, raw) =>
            {
                // Check if a vehicle name is provided
                if (args.Count < 1)
                {
                    Debug.WriteLine("Usage: /car [vehicle_name]");
                    return;
                }

                // Get the vehicle model name from the command arguments
                string vehicleName = args[0].ToString();

                // Spawn the vehicle
                SpawnVehicle(vehicleName);

            }), false);


            Debug.WriteLine("Hi from lx_register.Client!");
        }

        private async void SpawnVehicle(string modelName)
        {
            Model model = new Model(modelName);
            if (!model.IsInCdImage || !model.IsVehicle)
            {
                Debug.WriteLine($"Vehicle model {modelName} not found.");
                return;
            }

            model.Request();
            while (!model.IsLoaded)
            {
                await BaseScript.Delay(100);
            }

            var player = Game.PlayerPed;
            var position = player.Position;
            var vehicle = await World.CreateVehicle(model, position, player.Heading);

            // Apply modifications
            vehicle.Mods.InstallModKit(); // Required to apply mods

            var mods = vehicle.Mods.GetAllMods();

            foreach (var mod in mods)
            {
                var modType = mod.ModType;
                var lastIndex = mod.ModCount - 1;

                API.SetVehicleMod(vehicle.Handle, (int)modType, lastIndex, false);
            }
            // Apply colors
            vehicle.Mods.PrimaryColor = VehicleColor.MatteBlack;
            vehicle.Mods.SecondaryColor = VehicleColor.MatteBlack;

            // Bulletproof tires
            vehicle.CanTiresBurst = false;
            

            player.SetIntoVehicle(vehicle, VehicleSeat.Driver);

            Debug.WriteLine($"Spawned vehicle {modelName}.");
        }

    }
}
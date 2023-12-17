using CitizenFX.Core;
using CitizenFX.Core.Native;
using lx_connect.Server.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace lx_connect.Server.Manager
{
    public static class ConfigManager
    {
        public static Config LoadConfig()
        {
            try
            {
                var currentDirectory = Directory.GetCurrentDirectory();
                string ConfigFilePath = Path.Combine(currentDirectory, "resources", "[lynx]", API.GetCurrentResourceName(), "Config", "config.json");

                string json = File.ReadAllText(ConfigFilePath);
                var configData = JsonConvert.DeserializeObject<Config>(json);

                return configData;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message}");
                return new Config();
            }
        }
    }
}

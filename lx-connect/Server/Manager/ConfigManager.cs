using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace lx_connect.Server.Manager
{
    public static class ConfigManager
    {
        public static Dictionary<string, string> LoadConfig()
        {
            try
            {
                var configData = new Dictionary<string, string>();
                var currentDirectory = Directory.GetCurrentDirectory();
                string ConfigFilePath = Path.Combine(currentDirectory, "resources", "[lynx]", API.GetCurrentResourceName(), "Config", "config.json");

                string json = File.ReadAllText(ConfigFilePath);
                configData = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

                return configData;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message}");
                return new Dictionary<string, string>();
            }
        }
    }
}

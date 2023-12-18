using CitizenFX.Core;
using CitizenFX.Core.Native;
using lx_connect.Server.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace lx_connect.Server.Manager
{
    public class ConfigManager
    {
        private readonly string _currentDirectory = string.Empty;
        private readonly string _configFilePath = string.Empty;

        public ConfigManager()
        {
            _currentDirectory = Directory.GetCurrentDirectory();
            _configFilePath = Path.Combine(_currentDirectory, "resources", "[lynx]", API.GetCurrentResourceName(), "Config", "config.json");
        }

        // Updates the configuration file with new configuration data.
        public void UpdateConfig(Config config)
        {
            if (config != null)
            {
                string configJson = JsonConvert.SerializeObject(config,Formatting.Indented);
                File.WriteAllText(_configFilePath, configJson);
            }

            Debug.WriteLine($"[{API.GetCurrentResourceName()}]ERROR: ConfigManager.UpdateConfig() -> config is null");
        }

        // Loads the configuration data from the configuration file.
        public Config LoadConfig()
        {
            try
            {
                string json = File.ReadAllText(_configFilePath);
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

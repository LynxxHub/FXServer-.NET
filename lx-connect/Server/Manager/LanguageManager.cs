using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace lx_connect.Server.Manager
{
    public class LanguageManager
    {
        // Loads and returns language data from the specified local JSON file.
        public static Dictionary<string, string> LoadLanguage(string languageFileName)
        {
            try
            {
                var langData = new Dictionary<string, string>();
                var currentDirectory = Directory.GetCurrentDirectory();
                string languageFilePath = Path.Combine(
                    currentDirectory, 
                    "resources", 
                    "[lynx]", 
                    API.GetCurrentResourceName(), 
                    "Config", 
                    "lang", 
                    languageFileName + ".json");

                string json = File.ReadAllText(languageFilePath);
                langData = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

                return langData;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message}");
                return new Dictionary<string, string>();
            }
        }
    }
}

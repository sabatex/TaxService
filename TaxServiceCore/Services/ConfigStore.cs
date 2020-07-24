using sabatex.Extensions;
using sabatex.Extensions.DateTimeExtensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using TaxService.Models;

namespace TaxService.Services
{
    public static class ConfigStore
    {
        //static string defaultConfigFilePath = Directory.GetCurrentDirectory() + @"\config.json";
        static string _configFilePath = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\ntics\\TaxService\\config.json";
        static string defaultConfigFilePath = _configFilePath;


        public static Config CurrentConfig = LoadConfigFromFile() ?? new Config();

        public static void SaveToFile(this Config config)
        {
            SaveToFile(config, defaultConfigFilePath);
        }

        public static void SaveToFile(this Config config, string filePath)
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllText(filePath, System.Text.Json.JsonSerializer.Serialize(config));
        }

        public static Config LoadConfigFromFile(string filePath)
        {
            if (File.Exists(filePath))
                return System.Text.Json.JsonSerializer.Deserialize<Config>(File.ReadAllText(filePath));
            else
                return null;
        }

        public static Config LoadConfigFromFile()
        {
            return LoadConfigFromFile(defaultConfigFilePath);
        }


    }
}

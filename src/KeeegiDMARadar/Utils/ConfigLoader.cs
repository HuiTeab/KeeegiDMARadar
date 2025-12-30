using System.Text.Json;
using KeeegiDMARadar.Models;

namespace KeeegiDMARadar.Utils
{
    /// <summary>
    /// Utility for loading and saving configuration
    /// </summary>
    public static class ConfigLoader
    {
        /// <summary>
        /// Load configuration from JSON file
        /// </summary>
        public static Configuration LoadConfiguration(string configPath)
        {
            try
            {
                if (!File.Exists(configPath))
                {
                    Console.WriteLine($"[ConfigLoader] Configuration file not found: {configPath}");
                    Console.WriteLine("[ConfigLoader] Using default configuration.");
                    return new Configuration();
                }

                string json = File.ReadAllText(configPath);
                var config = JsonSerializer.Deserialize<Configuration>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (config == null)
                {
                    Console.WriteLine("[ConfigLoader] Failed to parse configuration. Using defaults.");
                    return new Configuration();
                }

                Console.WriteLine($"[ConfigLoader] Configuration loaded from: {configPath}");
                return config;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ConfigLoader] Error loading configuration: {ex.Message}");
                Console.WriteLine("[ConfigLoader] Using default configuration.");
                return new Configuration();
            }
        }

        /// <summary>
        /// Save configuration to JSON file
        /// </summary>
        public static void SaveConfiguration(Configuration config, string configPath)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNameCaseInsensitive = true
                };

                string json = JsonSerializer.Serialize(config, options);
                
                // Ensure directory exists
                string? directory = Path.GetDirectoryName(configPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(configPath, json);
                Console.WriteLine($"[ConfigLoader] Configuration saved to: {configPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ConfigLoader] Error saving configuration: {ex.Message}");
            }
        }
    }
}

using KeeegiDMARadar.Misc.JSON;
using KeeegiDMARadar.UI;
using KeeegiDMARadar.UI.Misc;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace KeeegiDMARadar
{
    public sealed class ARCDMAConfig
    {
        public ARCDMAConfig() { }

        /// <summary>
        /// Filename of this Config File (not full path).
        /// </summary>
        [JsonIgnore]
        internal const string Filename = "Config-ARC.json";

        [JsonIgnore]
        private static readonly Lock _syncRoot = new();

        [JsonIgnore]
        private static readonly FileInfo _configFile = new(Path.Combine(Program.ConfigPath.FullName, Filename));

        [JsonIgnore]
        private static readonly FileInfo _tempFile = new(Path.Combine(Program.ConfigPath.FullName, Filename + ".tmp"));

        [JsonIgnore]
        private static readonly FileInfo _backupFile = new(Path.Combine(Program.ConfigPath.FullName, Filename + ".bak"));


        /// <summary>
        /// DMA Config
        /// </summary>
        [JsonPropertyName("dma")]
        public DMAConfig DMA { get; set; } = new();

        public static ARCDMAConfig Load()
        {
            ARCDMAConfig config;
            lock (_syncRoot)
            {
                Program.ConfigPath.Create();
                if (_configFile.Exists)
                {
                    config = TryLoad(_tempFile) ??
                        TryLoad(_configFile) ??
                        TryLoad(_backupFile);

                    if (config is null)
                    {
                        var dlg = MessageBox.Show(
                            RadarWindow.Handle,
                            "Config File Corruption Detected! If you backed up your config, you may attempt to restore it.\n" +
                            "Press OK to Reset Config and continue startup, or CANCEL to terminate program.",
                            Program.Name,
                            MessageBoxButton.OKCancel,
                            MessageBoxImage.Error);
                        if (dlg == MessageBoxResult.Cancel)
                            Environment.Exit(0); // Terminate program
                        config = new ARCDMAConfig();
                        SaveInternal(config);
                    }
                }
                else
                {
                    config = new();
                    SaveInternal(config);
                }

                return config;
            }
        }

        private static ARCDMAConfig TryLoad(FileInfo file)
        {
            try
            {
                if (!file.Exists)
                    return null;
                string json = File.ReadAllText(file.FullName);
                return JsonSerializer.Deserialize(json, AppJsonContext.Default.ARCDMAConfig);
            }
            catch
            {
                return null; // Ignore errors, return null to indicate failure
            }
        }

        /// <summary>
        /// Save the current configuration to disk.
        /// </summary>
        /// <exception cref="IOException"></exception>
        public void Save()
        {
            lock (_syncRoot)
            {
                try
                {
                    SaveInternal(this);
                }
                catch (Exception ex)
                {
                    throw new IOException($"ERROR Saving Config: {ex.Message}", ex);
                }
            }
        }

        /// <summary>
        /// Saves the current configuration to disk asynchronously.
        /// </summary>
        /// <returns></returns>
        public async Task SaveAsync() => await Task.Run(Save);

        private static void SaveInternal(ARCDMAConfig config)
        {
            var json = JsonSerializer.Serialize(config, AppJsonContext.Default.ARCDMAConfig);
            using (var fs = new FileStream(
                _tempFile.FullName,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 4096,
                options: FileOptions.WriteThrough))
            using (var sw = new StreamWriter(fs))
            {
                sw.Write(json);
                sw.Flush();
                fs.Flush(flushToDisk: true);
            }
            if (_configFile.Exists)
            {
                File.Replace(
                    sourceFileName: _tempFile.FullName,
                    destinationFileName: _configFile.FullName,
                    destinationBackupFileName: _backupFile.FullName,
                    ignoreMetadataErrors: true);
            }
            else
            {
                File.Copy(
                    sourceFileName: _tempFile.FullName,
                    destFileName: _backupFile.FullName);
                File.Move(
                    sourceFileName: _tempFile.FullName,
                    destFileName: _configFile.FullName);
            }
        }


        /// <summary>
        /// Persistent Cache Access.
        /// </summary>
        [JsonPropertyName("cache")]
        public PersistentCache Cache { get; set; } = new();
    }

    public sealed class DMAConfig
    {
        /// <summary>
        /// FPGA Read Algorithm
        /// </summary>
        [JsonPropertyName("fpgaAlgo")]
        public FpgaAlgo FpgaAlgo { get; set; } = FpgaAlgo.Auto;

        /// <summary>
        /// Use a Memory Map for FPGA DMA Connection.
        /// </summary>
        [JsonPropertyName("enableMemMap")]
        public bool MemMapEnabled { get; set; } = true;
    }

    /// <summary>
    /// Persistent Cache that stores data between sessions for the same Process ID.
    /// </summary>
    public sealed class PersistentCache
    {
        /// <summary>
        /// Process Id this cache is tied to.
        /// </summary>
        [JsonPropertyName("pid")]
        public uint PID { get; set; }

        /// <summary>
        /// Key: RaidId | Value: Dictionary: Key: PlayerId | Value: GroupId
        /// </summary>
        [JsonPropertyName("groups")]
        public ConcurrentDictionary<int, ConcurrentDictionary<int, int>> Groups { get; set; } = new();
    }
}

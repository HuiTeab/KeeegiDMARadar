namespace KeeegiDMARadar.Models
{
    /// <summary>
    /// Configuration model loaded from config.json
    /// </summary>
    public class Configuration
    {
        public string TargetProcess { get; set; } = "example.exe";
        public int UpdateInterval { get; set; } = 16;
        public RadarConfig Radar { get; set; } = new RadarConfig();
        public OffsetsConfig Offsets { get; set; } = new OffsetsConfig();
        public MemoryConfig Memory { get; set; } = new MemoryConfig();
    }

    public class RadarConfig
    {
        public int Size { get; set; } = 400;
        public int Range { get; set; } = 100;
        public int CenterX { get; set; } = 200;
        public int CenterY { get; set; } = 200;
    }

    public class OffsetsConfig
    {
        public string BaseAddress { get; set; } = "0x00000000";
        public string EntityList { get; set; } = "0x00000000";
        public string LocalPlayer { get; set; } = "0x00000000";
    }

    public class MemoryConfig
    {
        public int MaxEntities { get; set; } = 64;
        public int ScanInterval { get; set; } = 100;
    }
}

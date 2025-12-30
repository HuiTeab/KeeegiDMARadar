//using LoneEftDmaRadar.Tarkov.GameWorld.Hazards;
//using LoneEftDmaRadar.UI.ColorPicker;
//using LoneEftDmaRadar.UI.Loot;
//using LoneEftDmaRadar.UI.Maps;
//using LoneEftDmaRadar.Web.TarkovDev;
using KeeegiDMARadar.Misc.JSON;
using SkiaSharp;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
using VmmSharpEx.Extensions.Input;

namespace KeeegiDMARadar.Misc.JSON
{
    /// <summary>
    /// AOT-compatible JSON serializer context for the application's configuration types.
    /// </summary>
    [JsonSourceGenerationOptions(
        WriteIndented = false,
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        ReadCommentHandling = JsonCommentHandling.Skip,
        Converters = [
            typeof(Vector2JsonConverter),
            typeof(Vector3JsonConverter),
            typeof(SKRectJsonConverter)
        ])]
    // Main config
    [JsonSerializable(typeof(ARCDMAConfig))]
    //[JsonSerializable(typeof(UIConfig))]
    //[JsonSerializable(typeof(WebRadarConfig))]
    //[JsonSerializable(typeof(LootConfig))]
    //[JsonSerializable(typeof(ContainersConfig))]
    ////[JsonSerializable(typeof(LootFilterConfig))]
    //[JsonSerializable(typeof(AimviewWidgetConfig))]
    //[JsonSerializable(typeof(QuestHelperConfig))]
    //[JsonSerializable(typeof(PersistentCache))]
    //[JsonSerializable(typeof(MiscConfig))]
    [JsonSerializable(typeof(SKSize))]
    [JsonSerializable(typeof(SKRect))]
    [JsonSerializable(typeof(Vector2))]
    [JsonSerializable(typeof(Vector3))]
    [JsonSerializable(typeof(HashSet<string>))]
    [JsonSerializable(typeof(Dictionary<string, string>))]
    public partial class AppJsonContext : JsonSerializerContext
    {
    }
}

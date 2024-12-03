// ReSharper disable InconsistentNaming
namespace BG3SaveBrowser.Models;

public static class LsvMetadataKeys
{
    public const string GameId = "GameID";
    public const string GameSessionId = "GameSessionID";
    public const string LeaderName = "LeaderName";
    public const string SaveTime = "SaveTime";
    public const string TimeStamp = "TimeStamp";
}

public record LsvPackageData
{
    public string? SaveName { get; set;}
    public string? GameId { get; set;}
    public string? GameSessionId { get; set;}
    public string? LeaderName { get; set;}
    public string? SaveTime { get; set;}
    public string? TimeStamp { get; set;}
}
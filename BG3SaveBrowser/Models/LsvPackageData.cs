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
    public string SaveName
    {
        get => _saveName ?? throw new ArgumentNullException(nameof(SaveName));
        set => _saveName = value;
    }

    public string GameId
    {
        get => _gameId ?? throw new ArgumentNullException(nameof(GameId));
        set => _gameId = value;
    }

    public string GameSessionId
    {
        get => _gameSessionId ?? throw new ArgumentNullException(nameof(GameSessionId));
        set => _gameSessionId = value;
    }

    public string LeaderName
    {
        get => _leaderName ?? throw new ArgumentNullException(nameof(LeaderName));
        set => _leaderName = value;
    }

    public string SaveTime
    {
        get => _saveTime ?? throw new ArgumentNullException(nameof(SaveTime));
        set => _saveTime = value;
    }

    public string TimeStamp
    {
        get => _timeStamp ?? throw new ArgumentNullException(nameof(TimeStamp));
        set => _timeStamp = value;
    }

    private string? _saveName { get; set; }
    private string? _gameId { get; set; }
    private string? _gameSessionId { get; set; }
    private string? _leaderName { get; set; }
    private string? _saveTime { get; set; }
    private string? _timeStamp { get; set; }
}
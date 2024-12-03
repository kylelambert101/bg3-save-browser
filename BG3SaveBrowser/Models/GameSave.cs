namespace BG3SaveBrowser.Models;

public record GameSave
{
    public string Path { get; set; }
    public string ThumbnailPath { get; set; }
    
    public string Owner { get; private init; }
    public CampaignDuration? CampaignDuration { get; private init; } 
    public string GameId { get; private init; }
    public string GameSessionId { get; private init; }
    public DateTime SaveTime { get; private init; }
    public string SaveName { get; private init; }

    public GameSave(string path, string thumbnailPath, LsvPackageData lsvData)
    {
        var campaignDuration = new CampaignDuration(int.Parse(lsvData.TimeStamp));
        var saveTimeStamp = long.Parse(lsvData.SaveTime);
        var saveTime = DateTimeOffset.FromUnixTimeSeconds(saveTimeStamp).DateTime;
    
        CampaignDuration = campaignDuration;
        GameId = lsvData.GameId;
        GameSessionId = lsvData.GameSessionId;
        Owner = lsvData.LeaderName;
        Path = path;
        SaveName = lsvData.SaveName;
        SaveTime = saveTime;
        ThumbnailPath = thumbnailPath;
    }
}
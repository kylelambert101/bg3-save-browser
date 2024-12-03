namespace BG3SaveBrowser.Models;

public record GameSave(
    string Path,
    string ThumbnailPath,
    string Owner,
    CampaignDuration CampaignDuration,
    string GameId,
    string GameSessionId,
    DateTime SaveTime,
    string SaveName
);
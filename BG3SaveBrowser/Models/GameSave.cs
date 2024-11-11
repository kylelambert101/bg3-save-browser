namespace BG3SaveBrowser.Models;

public record GameSave(
    // Comes from meta.lsf
    string Path,
    string ThumbnailPath,
    string Owner,
    int TimeStamp,
    Guid GameId,
    Guid GameSessionId,
    long SaveTime,

    // Comes from SaveInfo.json
    string SaveName
);
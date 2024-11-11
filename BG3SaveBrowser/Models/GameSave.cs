namespace BG3SaveBrowser.Models;

public record GameSave
{
    // TODO make a respectable constructor and enforce non-null state on applicable properties
    
    // Comes from meta.lsf
    public string? Path { get; set; }
    public string? ThumbnailPath { get; set; }
    public string? Owner { get; set; }
    public int TimeStamp { get; set; }
    public Guid GameId { get; set; }
    public Guid GameSessionId { get; set; }
    public long SaveTime { get; set; }

    // Comes from SaveInfo.json
    public string? SaveName { get; set; }
}
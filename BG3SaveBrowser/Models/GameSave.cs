using System.Text.Json.Serialization;

namespace BG3SaveBrowser.Models;

public record GameSave
{
    // TODO make a respectable constructor and enforce non-null state on applicable properties
    
    public string? Path { get; set; }
    public string? ThumbnailPath { get; set; }
    
    // Comes from meta.lsf
    public string? Owner { get; set; }

    public CampaignDuration? CampaignDuration { get; set; } 
    public string? GameId { get; set; }
    public string? GameSessionId { get; set; }
    public long SaveTime { get; set; }

    // Comes from SaveInfo.json
    public string? SaveName { get; set; }
}

public record SaveInfo
{
    [JsonPropertyName("Save Name")]
    public string? SaveName { get; set; }}
namespace BG3SaveBrowser.Models;

public record CampaignDuration(int Seconds) : IComparable<CampaignDuration>
{
    // I haven't the foggiest idea why Larian adds 1h36m to their timestamp values here, but c'est la vie.
    private readonly int _adjustedSeconds = Seconds - 5760;
    
    public int Hours => _adjustedSeconds / 3600;
    public int Minutes => _adjustedSeconds % 3600 / 60;

    public int CompareTo(CampaignDuration? other) => Seconds.CompareTo(other?.Seconds);

    public override string ToString() => $"{Hours:D2}h {Minutes:D2}m";
}
using System.Diagnostics.CodeAnalysis;
using BG3SaveBrowser.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BG3SaveBrowser.Infrastructure.Mapping;

public class GameSaveMapper
{
    private readonly ILogger<GameSaveMapper> _logger;

    public GameSaveMapper()
    {
        _logger = App.ServiceProvider.GetRequiredService<ILogger<GameSaveMapper>>();
    }

    public GameSave? TryMapGameSave(string path, string? thumbnailPath, LsvPackageData? lsvData)
    {
        var missingFields = new List<string>();

        if (lsvData is null)
        {
            _logger.LogWarning("Failed to map GameSave at {Path} due to missing lsv data",path);
            return default;
        }

        if (thumbnailPath is null) missingFields.Add(nameof(thumbnailPath));
        if (lsvData.GameId is null) missingFields.Add(nameof(lsvData.GameId));
        if (lsvData.GameSessionId is null) missingFields.Add(nameof(lsvData.GameSessionId));
        if (lsvData.LeaderName is null) missingFields.Add(nameof(lsvData.LeaderName));
        if (lsvData.SaveName is null) missingFields.Add(nameof(lsvData.SaveName));
        if (lsvData.SaveTime is null) missingFields.Add(nameof(lsvData.SaveTime));
        if (lsvData.TimeStamp is null) missingFields.Add(nameof(lsvData.TimeStamp));
        
        if (missingFields.Count > 0)
        {
            _logger.LogWarning("Failed to map GameSave at {Path} with missing data: {@Fields}", path, missingFields);
            return default;
        }

        CampaignDuration campaignDuration;
        try
        {
            campaignDuration = new CampaignDuration(int.Parse(lsvData.TimeStamp!));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, 
                "Failed to map GameSave at {Path} due to unparseable campaign duration: {Timestamp}", 
                path, lsvData.TimeStamp);
            return default;
        }

        DateTime saveTime;
        try
        {
            var saveTimeStamp = long.Parse(lsvData.SaveTime!);
            saveTime = DateTimeOffset.FromUnixTimeSeconds(saveTimeStamp).DateTime;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, 
                "Failed to map GameSave at {Path} due to unparseable save time: {Timestamp}", 
                path, lsvData.TimeStamp);
            return default;
        }

        return new GameSave(
            path,
            thumbnailPath!,
            lsvData.LeaderName!,
            campaignDuration,
            lsvData.GameId!,
            lsvData.GameSessionId!,
            saveTime,
            lsvData.SaveName!
            );
    }
}
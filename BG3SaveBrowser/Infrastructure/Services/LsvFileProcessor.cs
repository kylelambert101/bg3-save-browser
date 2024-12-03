using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using BG3SaveBrowser.Models;
using LSLib.LS;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BG3SaveBrowser.Infrastructure.Services;

public class LsvFileProcessor
{
    private readonly ILogger<LsvFileProcessor> _logger;

    public LsvFileProcessor()
    {
        _logger = App.ServiceProvider.GetRequiredService<ILogger<LsvFileProcessor>>();
    }

    public async Task<LsvPackageData> ProcessLsvPackage(string filePath)
    {
        var reader = new PackageReader();
        var package = reader.Read(filePath);

        // Process SaveInfo.json
        var saveInfo =
            package.Files.FirstOrDefault(f => f.Name.Equals("SaveInfo.json", StringComparison.OrdinalIgnoreCase));
        if (saveInfo is null)
            throw new InvalidDataException("Package was missing SaveInfo.json");

        var lsvData = new LsvPackageData();
        await using var stream = saveInfo.CreateContentReader();
        try
        {
            // Deserialize JSON data from the stream
            var info = await JsonSerializer.DeserializeAsync<SaveInfo>(stream);
            lsvData.SaveName = info?.SaveName;
        }
        catch (JsonException ex)
        {
            // Handle any JSON deserialization errors
            _logger.LogError(ex, "Error deserializing JSON");
        }

        // Process meta
        var metaInfo = package.Files.FirstOrDefault(p => p.Name.Equals("meta.lsf", StringComparison.OrdinalIgnoreCase));
        if (metaInfo == null)
            throw new InvalidDataException("The specified package is not a valid savegame (meta.lsf not found)");

        await using var rsrcStream = metaInfo.CreateContentReader();
        using var rsrcReader = new LSFReader(rsrcStream);
        var metaResource = rsrcReader.Read();

        foreach (var kvp in metaResource.Regions["MetaData"].Children["MetaData"].First().Attributes)
        {
            var (k, v) = kvp;
            switch (k)
            {
                case LsvMetadataKeys.GameId:
                    lsvData.GameId = v.Value.ToString();
                    break;
                case LsvMetadataKeys.GameSessionId:
                    lsvData.GameSessionId = v.Value.ToString();
                    break;
                case LsvMetadataKeys.LeaderName:
                    lsvData.LeaderName = v.Value.ToString();
                    break;
                case LsvMetadataKeys.SaveTime:
                    lsvData.SaveTime = v.Value.ToString();
                    break;
                case LsvMetadataKeys.TimeStamp:
                    lsvData.TimeStamp = v.Value.ToString();
                    break;
            }
        }

        package.Dispose();
        return lsvData;
    }
    
    /// <summary>
    ///     Simple object to reflect JSON structure of SaveInfo.json
    /// </summary>
    private record SaveInfo
    {
        [JsonPropertyName("Save Name")]
        public string? SaveName { get; init; }
    }
}
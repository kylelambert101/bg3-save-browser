using System.IO;
using System.Text.Json;
using BG3SaveBrowser.Models;
using LSLib.LS;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BG3SaveBrowser.Infrastructure.Services;

public class FileProcessor
{
    private readonly ILogger<FileProcessor> _logger = App.ServiceProvider.GetRequiredService<ILogger<FileProcessor>>();

    public async Task<List<GameSave>> ProcessPath(string baseDirectory, IProgress<(int progress, string status)>? progress = null)
    {
        var result = new List<GameSave>();

        // Ensure the directory exists
        if (!Directory.Exists(baseDirectory))
            throw new DirectoryNotFoundException($"The directory {baseDirectory} does not exist.");

        // Get all subdirectories in the base directory
        var subdirectories = Directory.GetDirectories(baseDirectory);

        var totalSubdirectories = subdirectories.Length;
        var processedSubdirectories = 0;

        foreach (var subdirectory in subdirectories)
        {
            _logger.LogTrace("Processing directory {Dir}",subdirectory);
            try
            {
                // Add to the result list
                result.Add(await ProcessSaveDirectory(subdirectory));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process directory: {Directory}", subdirectory);
            }

            // Report progress
            var progressValue = (int)((processedSubdirectories++ / (double)totalSubdirectories) * 100);
            progress?.Report((progressValue, $"Processing folder: {Path.GetFileName(subdirectory)}"));
        }

        return result;
    }

    private async Task<GameSave> ProcessSaveDirectory(string directory)
    {
        var item = new GameSave { Path = directory };

        // Get all files in the directory
        var files = Directory.GetFiles(directory);

        foreach (var file in files)
        {
            if (file.EndsWith(".lsv"))
            {
                await ProcessLsvPackage(file, item);
            }
            else if (file.EndsWith(".webp"))
            {
                item.ThumbnailPath = file;
            }
        }
        
        // TODO Throw if item doesn't have all expected data (i.e. the directory was bad)

        return item;
    }


    private async Task ProcessLsvPackage(string filePath, GameSave gameSave)
    {
        var reader = new PackageReader();
        var package = reader.Read(filePath);
        
        // Process SaveInfo.json
        var saveInfo = package.Files.FirstOrDefault(f => f.Name.Equals("SaveInfo.json", StringComparison.OrdinalIgnoreCase));
        if (saveInfo is null)
            throw new InvalidDataException("Package was missing SaveInfo.json");
        await using var stream = saveInfo.CreateContentReader();
        try
        {
            // Deserialize JSON data from the stream
            var info = await JsonSerializer.DeserializeAsync<SaveInfo>(stream);
            gameSave.SaveName = info?.SaveName;
        }
        catch (JsonException ex)
        {
            // Handle any JSON deserialization errors
            _logger.LogError(ex,"Error deserializing JSON");
        }
        
        // Process meta
        var metaInfo = package.Files.FirstOrDefault(p => p.Name.Equals("meta.lsf", StringComparison.OrdinalIgnoreCase));
        if (metaInfo == null)
            throw new InvalidDataException("The specified package is not a valid savegame (meta.lsf not found)");

        await using var rsrcStream = metaInfo.CreateContentReader();
        using var rsrcReader = new LSFReader(rsrcStream);
        var metaResource = rsrcReader.Read();
        
        // TODO This should probably be in its own file. Alternatively, make this a direct metadata record and map that to GameSave later. 
        foreach (var kvp in metaResource.Regions["MetaData"].Children["MetaData"].First().Attributes)
        {
            var (k, v) = kvp;
            switch (k)
            {
                case "LeaderName":
                    gameSave.Owner = v.Value.ToString();
                    break;
                case "TimeStamp":
                    var timeStampVal = v.Value.ToString();
                    if (timeStampVal is null) break;
                    try
                    {
                        gameSave.CampaignDuration = new CampaignDuration(int.Parse(timeStampVal));
                    }
                    catch
                    {
                        _logger.LogError("Failed to parse timestamp as campaign duration: {Value}",timeStampVal);
                    }
                    break;
                case "GameID":
                    gameSave.GameId = v.Value.ToString();
                    break;
                case "GameSessionID":
                    gameSave.GameSessionId = v.Value.ToString();
                    break;
                case "SaveTime":
                    var saveTimeVal = v.Value.ToString();
                    if (saveTimeVal is null) break;
                    try
                    {
                        var saveTimeStamp = long.Parse(saveTimeVal);
                        gameSave.SaveTime = DateTimeOffset.FromUnixTimeSeconds(saveTimeStamp).DateTime;
                    }
                    catch
                    {
                        // don't worry about it. Maybe I'll handle this more gracefully later
                    }
                    break;
                default:
                    break;
            };
        }

        package.Dispose();
    }
}
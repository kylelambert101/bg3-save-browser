using System.IO;
using System.Text.Json;
using BG3SaveBrowser.Models;
using LSLib.LS;

namespace BG3SaveBrowser.Infrastructure.Services;

public class FileProcessor
{
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
            try
            {
                // Add to the result list
                result.Add(await ProcessSaveDirectory(subdirectory));
            }
            catch
            {
                //TODO Use a logger
                Console.WriteLine($"Failed to process directory: {subdirectory}");
            }

            // Report progress
            var progressValue = (int)((processedSubdirectories++ / (double)totalSubdirectories) * 100);
            progress?.Report((progressValue, $"Processing folder: {Path.GetFileName(subdirectory)}"));
        }

        return result;
    }

    private static async Task<GameSave> ProcessSaveDirectory(string directory)
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


    private static async Task ProcessLsvPackage(string filePath, GameSave gameSave)
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
            Console.WriteLine($"Error deserializing JSON: {ex.Message}");
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
                case "LeaderName":
                    gameSave.Owner = v.Value.ToString();
                    break;
                case "TimeStamp":
                    var timeStampVal = v.Value.ToString();
                    if (timeStampVal is null) break;
                    try
                    {
                        gameSave.TimeStampInt = int.Parse(timeStampVal);
                        // TODO Figure out how to parse actual dates
                        // var timeStamp = DateTime.Parse(timeStampVal);
                        // gameSave.TimeStamp = timeStamp;
                    }
                    catch
                    {
                        // don't worry about it. Maybe I'll handle this more gracefully later
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
                        gameSave.SaveTime = long.Parse(saveTimeVal);
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
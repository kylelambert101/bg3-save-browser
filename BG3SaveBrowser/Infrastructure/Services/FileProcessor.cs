using BG3SaveBrowser.Models;
using LSLib.LS.Story;

namespace BG3SaveBrowser.Infrastructure.Services;

using System.Collections.Generic;
using System.IO;
using LSLib.LS;

public class FileProcessor
{
    public List<FileItem> ProcessPath(string baseDirectory)
    {
        var result = new List<FileItem>();

        // Ensure the directory exists
        if (!Directory.Exists(baseDirectory))
        {
            throw new DirectoryNotFoundException($"The directory {baseDirectory} does not exist.");
        }

        // Get all subdirectories in the base directory
        var subdirectories = Directory.GetDirectories(baseDirectory);

        foreach (var subdirectory in subdirectories)
        {
            var item = new FileItem();
            
            // Get the name of the subdirectory (not the full path)
            item.Name = Path.GetFileName(subdirectory);
            item.Path = subdirectory;
            
            // Get all files in the subdirectory
            var files = Directory.GetFiles(subdirectory);

            // Add file names to the list
            var fileNames = new List<string>();
            foreach (var file in files)
            {
                fileNames.Add(Path.GetFileName(file));
                if (file.EndsWith(".lsv"))
                {
                    item.Owner = ProcessLSVPackage(file);
                }
            }
            item.FileCount = fileNames.Count;
            item.LastModifiedDate = File.GetLastWriteTime(subdirectory);

            // Add to the result dictionary
            result.Add(item);
        }

        return result;
    }

    private string? ProcessLSVPackage(string filePath)
    {
        string? owner = null;
        var reader = new PackageReader();
        var package = reader.Read(filePath);
        var metaInfo = package.Files.FirstOrDefault(p => p.Name.ToLowerInvariant() == "meta.lsf");
        if (metaInfo == null)
        {
            throw new InvalidDataException("The specified package is not a valid savegame (meta.lsf not found)");
        }

        using var rsrcStream = metaInfo.CreateContentReader();
        using var rsrcReader = new LSFReader(rsrcStream);
        var metaResource = rsrcReader.Read();
        foreach (var kvp in metaResource.Regions["MetaData"].Children["MetaData"].First().Attributes)
        {
            var (k,v) = kvp;
            if (k == "LeaderName")
            {
                owner = v.Value.ToString();
            }
        }
        package.Dispose();
        return owner;
    }
}

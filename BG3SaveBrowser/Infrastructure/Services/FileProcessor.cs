using BG3SaveBrowser.Models;

namespace BG3SaveBrowser.Infrastructure.Services;

using System.Collections.Generic;
using System.IO;

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
            }
            item.FileCount = fileNames.Count;
            item.LastModifiedDate = File.GetLastWriteTime(subdirectory);

            // Add to the result dictionary
            result.Add(item);
        }

        return result;
    }
}

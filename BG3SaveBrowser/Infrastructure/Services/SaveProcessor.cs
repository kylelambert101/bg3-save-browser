using System.IO;
using BG3SaveBrowser.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BG3SaveBrowser.Infrastructure.Services;

public class SaveProcessor
{
    private readonly LsvFileProcessor _lsvFileProcessor = App.ServiceProvider.GetRequiredService<LsvFileProcessor>();
    private readonly ILogger<SaveProcessor> _logger = App.ServiceProvider.GetRequiredService<ILogger<SaveProcessor>>();
    
    public async Task<GameSave> ProcessSaveDirectory(string directory)
    {
        // Get all files in the directory
        var files = Directory.GetFiles(directory);

        string? thumbnailPath = null;
        LsvPackageData? lsvData = null;
        foreach (var file in files)
        {
            if (file.EndsWith(".lsv", StringComparison.OrdinalIgnoreCase))
            {
                lsvData = await _lsvFileProcessor.ProcessLsvPackage(file);
            }
            else if (file.EndsWith(".webp", StringComparison.OrdinalIgnoreCase))
            {
                thumbnailPath = file;
            }
        }
        
        return new GameSave(
            directory,
            thumbnailPath ?? throw new ArgumentNullException(nameof(thumbnailPath)), 
            lsvData ?? throw new ArgumentNullException(nameof(lsvData))
        );
    }
}
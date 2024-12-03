using System.IO;
using System.Security.Policy;
using BG3SaveBrowser.Infrastructure.Mapping;
using BG3SaveBrowser.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BG3SaveBrowser.Infrastructure.Services;

public class SaveProcessor
{
    private readonly GameSaveMapper _gameSaveMapper;
    private readonly LsvFileProcessor _lsvFileProcessor;
    private readonly ILogger<SaveProcessor> _logger;

    public SaveProcessor()
    {
        _gameSaveMapper = App.ServiceProvider.GetRequiredService<GameSaveMapper>();
        _lsvFileProcessor = App.ServiceProvider.GetRequiredService<LsvFileProcessor>();
        _logger = App.ServiceProvider.GetRequiredService<ILogger<SaveProcessor>>();
    }
    
    public async Task<GameSave?> ProcessSaveDirectory(string directory)
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

        return _gameSaveMapper.TryMapGameSave(directory, thumbnailPath, lsvData);
    }
}
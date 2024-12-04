﻿using System.IO;
using BG3SaveBrowser.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BG3SaveBrowser.Infrastructure.Services;

public class SavesDirectoryProcessor
{
    private readonly SaveProcessor _saveProcessor;
    private readonly ILogger<SavesDirectoryProcessor> _logger;

    public SavesDirectoryProcessor()
    {
        _saveProcessor = App.ServiceProvider.GetRequiredService<SaveProcessor>();
        _logger = App.ServiceProvider.GetRequiredService<ILogger<SavesDirectoryProcessor>>();
    }
    
    public async Task<List<GameSave>> ProcessPath(string baseDirectory,
        IProgress<(int progress, string status)>? progress = null)
    {
        var allSaves = new List<GameSave>();

        // Ensure the directory exists
        if (!Directory.Exists(baseDirectory))
            throw new DirectoryNotFoundException($"The directory {baseDirectory} does not exist.");

        _logger.LogInformation("Processing target directory {Directory}", baseDirectory);
        // Get all subdirectories in the base directory
        var subdirectories = Directory.GetDirectories(baseDirectory);

        var totalSubdirectories = subdirectories.Length;
        var processedSubdirectories = 0;

        foreach (var subdirectory in subdirectories)
        {
            _logger.LogTrace("Processing save directory {Dir}", subdirectory);
            try
            {
                var gameSave = await _saveProcessor.ProcessSaveDirectory(subdirectory);
                if (gameSave != default) allSaves.Add(gameSave);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process save directory: {Directory}", subdirectory);
            }

            // Report progress
            var progressValue = (int)(processedSubdirectories++ / (double)totalSubdirectories * 100);
            progress?.Report((progressValue, $"Processing folder: {Path.GetFileName(subdirectory)}"));
        }
        _logger.LogInformation("Loaded {Count} valid saves from {DirCount} directories", allSaves.Count, subdirectories.Length);

        return allSaves;
    }
}
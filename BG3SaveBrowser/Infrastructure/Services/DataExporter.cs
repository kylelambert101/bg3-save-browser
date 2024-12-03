using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.IO;
using BG3SaveBrowser.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace BG3SaveBrowser.Infrastructure.Services;

public class DataExporter
{
    private readonly ILogger<DataExporter> _logger;

    public DataExporter()
    {
        _logger = App.ServiceProvider.GetRequiredService<ILogger<DataExporter>>();
    }

    public async Task ExportData(ObservableCollection<GameSave> saves)
    {
        // Prompt user for save location
        var saveDialog = new SaveFileDialog
        {
            Title = "Select export location",
            Filter = "SQLite Database (*.db)|*.db",
            FileName = "export" // Default export file name
        };
        
        if (saveDialog.ShowDialog() == true)
        {
            var selectedPath = Path.GetDirectoryName(saveDialog.FileName);
            var baseFileName = Path.GetFileNameWithoutExtension(saveDialog.FileName);

            var sqliteFilePath = Path.Combine(selectedPath, $"{baseFileName}.db");

            _logger.LogInformation("Exporting data...");
            await ExportToSqlite(sqliteFilePath, saves);
        }
    }
    
    private async Task ExportToSqlite(string filePath, ObservableCollection<GameSave> saves)
    {
        var connectionString = $"Data Source={filePath};Version=3;";

        await using var connection = new SQLiteConnection(connectionString);
        await connection.OpenAsync();

        // Create table if not exists
        var createTableCmd = 
            """
             CREATE TABLE IF NOT EXISTS GameSaves (
                 Id INTEGER PRIMARY KEY,
                 SaveName TEXT,
                 Owner TEXT,
                 TimeStamp INT,
                 GameId TEXT,
                 GameSessionId TEXT,
                 SaveTime INT
             )
            """;
        await using (var cmd = new SQLiteCommand(createTableCmd, connection))
        {
            _logger.LogDebug("Creating GameSaves table...");
            await cmd.ExecuteNonQueryAsync();
        }

        const string insertCmd = 
            """
             INSERT INTO GameSaves (SaveName, Owner, TimeStamp, GameId, GameSessionId, SaveTime)
             VALUES (@SaveName, @Owner, @TimeStamp, @GameId, @GameSessionId, @SaveTime)
            """;
            
        _logger.LogDebug("Inserting GameSave data...");
        foreach (var gameSave in saves)
        {
            await using var cmd = new SQLiteCommand(insertCmd, connection);
            cmd.Parameters.AddWithValue("@SaveName", gameSave.SaveName);
            cmd.Parameters.AddWithValue("@Owner", gameSave.Owner);
            cmd.Parameters.AddWithValue("@TimeStamp", gameSave.CampaignDuration?.Seconds);
            cmd.Parameters.AddWithValue("@GameId", gameSave.GameId);
            cmd.Parameters.AddWithValue("@GameSessionId", gameSave.GameSessionId);
            cmd.Parameters.AddWithValue("@SaveTime", gameSave.SaveTime);

            await cmd.ExecuteNonQueryAsync();
        }
        _logger.LogInformation("Finished exporting data for {Count} saves", saves.Count);

    }
}
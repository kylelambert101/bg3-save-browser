using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using BG3SaveBrowser.Models;
using Microsoft.Win32;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace BG3SaveBrowser.Infrastructure.Services;

public class DataExporter
{
    // Export data to SQLite database
    public async Task ExportToSqlite(string filePath, ObservableCollection<GameSave> files)
    {
        var connectionString = $"Data Source={filePath};Version=3;";

        using (var connection = new SQLiteConnection(connectionString))
        {
            await connection.OpenAsync();

            // Create table if not exists
            var createTableCmd = @"
                    CREATE TABLE IF NOT EXISTS GameSaves (
                        Id INTEGER PRIMARY KEY,
                        SaveName TEXT,
                        Owner TEXT,
                        TimeStamp INT,
                        GameId TEXT,
                        GameSessionId TEXT,
                        SaveTime INT
                    )";
            await ExecuteNonQueryAsync(connection, createTableCmd);

            // Insert data
            foreach (var file in files)
            {
                var insertCmd = $@"
                        INSERT INTO GameSaves (SaveName, Owner, TimeStamp, GameId, GameSessionId, SaveTime)
                        VALUES ('{file.SaveName}', '{file.Owner}', {file.TimeStampInt}, '{file.GameId}', '{file.GameSessionId}', {file.SaveTime})";
                await ExecuteNonQueryAsync(connection, insertCmd);
            }
        }
    }

    private async Task ExecuteNonQueryAsync(SQLiteConnection connection, string commandText)
    {
        using (var cmd = new SQLiteCommand(commandText, connection))
        {
            await cmd.ExecuteNonQueryAsync();
        }
    }

    // Export both CSV and SQLite files
    public async Task ExportData(ObservableCollection<GameSave> files)
    {
        // Prompt user for save location
        var saveDialog = new SaveFileDialog
        {
            Title = "Select export location",
            Filter = "SQLite Database (*.db)|*.db|CSV File (*.csv)|*.csv",
            FileName = "export" // Default export file name
        };

        if (saveDialog.ShowDialog() == true)
        {
            var selectedPath = Path.GetDirectoryName(saveDialog.FileName);
            var baseFileName = Path.GetFileNameWithoutExtension(saveDialog.FileName);

            // Export both CSV and SQLite in the same folder with a consistent name
            var sqliteFilePath = Path.Combine(selectedPath, $"{baseFileName}.db");

            await ExportToSqlite(sqliteFilePath, files);
        }
    }
}
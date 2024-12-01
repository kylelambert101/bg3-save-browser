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

            const string insertCmd = @"
                INSERT INTO GameSaves (SaveName, Owner, TimeStamp, GameId, GameSessionId, SaveTime)
                VALUES (@SaveName, @Owner, @TimeStamp, @GameId, @GameSessionId, @SaveTime)";
            
            // Insert data
            foreach (var file in files)
            {
                await using var cmd = new SQLiteCommand(insertCmd, connection);
                cmd.Parameters.AddWithValue("@SaveName", file.SaveName);
                cmd.Parameters.AddWithValue("@Owner", file.Owner);
                cmd.Parameters.AddWithValue("@TimeStamp", file.TimeStampInt);
                cmd.Parameters.AddWithValue("@GameId", file.GameId);
                cmd.Parameters.AddWithValue("@GameSessionId", file.GameSessionId);
                cmd.Parameters.AddWithValue("@SaveTime", file.SaveTime);

                await cmd.ExecuteNonQueryAsync();
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
            Filter = "SQLite Database (*.db)|*.db",
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
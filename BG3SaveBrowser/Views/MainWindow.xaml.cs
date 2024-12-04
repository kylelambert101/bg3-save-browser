using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Documents;
using BG3SaveBrowser.Infrastructure.Services;
using BG3SaveBrowser.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace BG3SaveBrowser.Views;
public partial class MainWindow : Window
{
    private readonly SavesDirectoryProcessor _savesDirectoryProcessor;
    private readonly DataExporter _dataExporter;
    private readonly ILogger<MainWindow> _logger;
    
    public MainWindow()
    {
        InitializeComponent();
        // Set the window to full screen
        this.WindowState = WindowState.Maximized;

        _savesDirectoryProcessor = App.ServiceProvider.GetRequiredService<SavesDirectoryProcessor>();
        _dataExporter = App.ServiceProvider.GetRequiredService<DataExporter>();
        _logger = App.ServiceProvider.GetRequiredService<ILogger<MainWindow>>();
        
        FilesListView.ItemsSource = Saves;
    }

    private ObservableCollection<GameSave> Saves { get; set; } = new();

    private async void BrowseButton_Click(object sender, RoutedEventArgs e)
    {
        using var folderDialog = new FolderBrowserDialog();
        if (folderDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
        
        var selectedPath = folderDialog.SelectedPath;
        DirectoryPathTextBox.Text = selectedPath;
        await LoadFiles(selectedPath);
    }
    
    private async void ExportButton_Click(object sender, RoutedEventArgs e) => 
        await _dataExporter.ExportData(Saves);

    private async Task LoadFiles(string directoryPath)
    {
        var progressWindow = new ProgressWindow();
        progressWindow.Show();

        var progressReporter = new Progress<(int progress, string status)>(update =>
        {
            progressWindow.Dispatcher.Invoke(() =>
            {
                progressWindow.UpdateProgress(update.progress, update.status);
            });
        });

        try
        {
            // Run the file processing on a background thread
            var saveData = await Task.Run(() => _savesDirectoryProcessor.ProcessPath(directoryPath, progressReporter));

            Saves.Clear();
            foreach (var gameSave in saveData)
            {
                // We add saves one at a time to play nicely with the observable which has already been bound to the UI
                Saves.Add(gameSave);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading files: {ex.Message}");
            _logger.LogError(ex, "Error loading files");
        }
        finally
        {
            progressWindow.Close();
        }
    }

    public void AppendLog(string message, LogLevel level)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var paragraph = new Paragraph();
            var textRange = new Run(message);
            
            // Set color based on log level
            // switch (level)
            // {
            //     case LogLevel.Information:
            //         textRange.Foreground = Brushes.Green;
            //         break;
            //     case LogLevel.Warning:
            //         textRange.Foreground = Brushes.Orange;
            //         break;
            //     case LogLevel.Error:
            //         textRange.Foreground = Brushes.Red;
            //         break;
            //     default:
            //         textRange.Foreground = Brushes.Black;
            //         break;
            // }

            paragraph.Inlines.Add(textRange);
            LogConsole.Document.Blocks.Add(paragraph);

            // Auto-scroll to the bottom
            LogConsole.ScrollToEnd();
        });
    }

}
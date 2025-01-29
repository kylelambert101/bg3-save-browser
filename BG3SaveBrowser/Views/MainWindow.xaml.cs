using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using BG3SaveBrowser.Infrastructure.Services;
using BG3SaveBrowser.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
        
        // Initialize CollectionView for sorting
        SavesView = CollectionViewSource.GetDefaultView(Saves);
        SavesView.SortDescriptions.Add(new SortDescription(nameof(GameSave.GameSessionId), ListSortDirection.Ascending));
        SavesView.SortDescriptions.Add(new SortDescription(nameof(GameSave.SaveTime), ListSortDirection.Ascending));
        
        FilesListView.ItemsSource = Saves;
    }

    private ObservableCollection<GameSave> Saves { get; set; } = new();
    private ICollectionView SavesView { get; set; }

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

            SavesView.Refresh(); // Ensure the sorting updates
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

}
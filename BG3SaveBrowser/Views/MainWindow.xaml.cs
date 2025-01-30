using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using BG3SaveBrowser.Infrastructure.Services;
using BG3SaveBrowser.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Binding = System.Windows.Data.Binding;
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
    
    private void FilesListView_ColumnHeader_Click(object sender, RoutedEventArgs e)
    {
        _logger.LogDebug("Column header clicked.");

        if (e.OriginalSource is GridViewColumnHeader header)
        {
            _logger.LogDebug("Header detected: {Header}", header.Content);

            if (header.Column is GridViewColumn column)
            {
                if (column.DisplayMemberBinding is Binding binding)
                {
                    var columnName = binding.Path.Path;
                    _logger.LogDebug("Sorting by column: {ColumnName}", columnName);
                    ToggleSort(columnName);
                }
                else if (column.Header is string headerText)
                {
                    // Fallback: Try to use the column header text if binding is missing
                    _logger.LogDebug("Column has no DisplayMemberBinding, falling back to header text: {HeaderText}", headerText);
                    ToggleSort(headerText);
                }
                else
                {
                    _logger.LogDebug("Could not determine column binding or header text.");
                }
            }
            else
            {
                _logger.LogDebug("No GridViewColumn found in header.");
            }
        }
        else
        {
            _logger.LogDebug("Click event did not originate from a GridViewColumnHeader.");
        }
    }


    private void ToggleSort(string columnName)
    {
        _logger.LogDebug("Toggling sort for: {ColumnName}", columnName);

        var collectionView = CollectionViewSource.GetDefaultView(Saves);
        if (collectionView == null)
        {
            _logger.LogDebug("CollectionView is null!");
            return;
        }

        var existingSort = collectionView.SortDescriptions.FirstOrDefault(s => s.PropertyName == columnName);

        ListSortDirection newDirection;
        if (existingSort.PropertyName != null)
        {
            newDirection = existingSort.Direction == ListSortDirection.Ascending
                ? ListSortDirection.Descending
                : ListSortDirection.Ascending;
            _logger.LogDebug("Existing sort found. Changing direction to {NewDirection}", newDirection);
        }
        else
        {
            newDirection = ListSortDirection.Ascending;
            _logger.LogDebug("No existing sort found. Defaulting to ascending.");
        }

        collectionView.SortDescriptions.Clear();
        collectionView.SortDescriptions.Add(new SortDescription(columnName, newDirection));
        collectionView.Refresh();

        _logger.LogDebug("Sorting applied: {ColumnName} {NewDirection}", columnName, newDirection);
    }



}
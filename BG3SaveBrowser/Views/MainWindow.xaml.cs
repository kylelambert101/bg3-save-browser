using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using BG3SaveBrowser.Infrastructure.Services;
using BG3SaveBrowser.Models;
using Microsoft.Extensions.DependencyInjection;
using MessageBox = System.Windows.MessageBox;
using Path = System.IO.Path;

namespace BG3SaveBrowser.Views;
public partial class MainWindow : Window
{
    private readonly FileProcessor _fileProcessor;
    public MainWindow()
    {
        InitializeComponent();

        _fileProcessor = App.ServiceProvider.GetRequiredService<FileProcessor>();
        FilesListView.ItemsSource = Files;
    }

    private ObservableCollection<GameSave> Files { get; set; } = new();

    private async void BrowseButton_Click(object sender, RoutedEventArgs e)
    {
        using var folderDialog = new FolderBrowserDialog();
        if (folderDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
        
        var selectedPath = folderDialog.SelectedPath;
        DirectoryPathTextBox.Text = selectedPath;
        await LoadFiles(selectedPath);
    }

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
            var files = await Task.Run(() => _fileProcessor.ProcessPath(directoryPath, progressReporter));

            // Add the processed files to the list
            Files.Clear();
            foreach (var file in files)
            {
                // We add files one at a time to play nicely with the observable which has already been bound to the UI
                Files.Add(file);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading files: {ex.Message}");
        }
        finally
        {
            progressWindow.Close();
        }
    }


}
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using BG3SaveBrowser.Infrastructure.Services;
using BG3SaveBrowser.Models;
using Microsoft.Extensions.DependencyInjection;
using Path = System.IO.Path;

namespace BG3SaveBrowser.Views;
public partial class MainWindow : Window
{
    private readonly FileProcessor _fileProcessor;
    public MainWindow()
    {
        InitializeComponent();

        // Get the instance of GameSaveService from the DI container
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
        Files.Clear();
        foreach (var x in await _fileProcessor.ProcessPath(directoryPath))
        {
            Files.Add(x);
        }
    }
}
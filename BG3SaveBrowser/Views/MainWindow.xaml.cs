using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using BG3SaveBrowser.Infrastructure.Services;
using BG3SaveBrowser.Models;
using Path = System.IO.Path;

namespace BG3SaveBrowser.Views;
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        FilesListView.ItemsSource = Files;
    }

    private ObservableCollection<FileItem> Files { get; set; } = new();

    private void BrowseButton_Click(object sender, RoutedEventArgs e)
    {
        using var folderDialog = new FolderBrowserDialog();
        if (folderDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
        
        var selectedPath = folderDialog.SelectedPath;
        DirectoryPathTextBox.Text = selectedPath;
        LoadFiles(selectedPath);
    }

    private void LoadFiles(string directoryPath)
    {
        var processor = new FileProcessor();
        Files.Clear();
        foreach (var x in processor.ProcessPath(directoryPath))
        {
            Files.Add(x);
        }
    }
}
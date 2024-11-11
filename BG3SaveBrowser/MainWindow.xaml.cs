using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using Path = System.IO.Path;

namespace BG3SaveBrowser;
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        FilesListView.ItemsSource = Files;
    }

    public ObservableCollection<FileItem> Files { get; set; } = new();

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
        Files.Clear();
        var files = Directory.GetFiles(directoryPath);
        foreach (var filePath in files)
            Files.Add(new FileItem
            {
                Name = Path.GetFileName(filePath),
                Path = filePath
            });
    }
}

public class FileItem
{
    public string Name { get; set; }
    public string Path { get; set; }
}
namespace BG3SaveBrowser.Models;

public class FileItem
{
    public string Name { get; set; }
    public string Path { get; set; }
    
    public int FileCount { get; set; }

    public DateTime LastModifiedDate { get; set; }
    
    public string? Owner { get; set; }
    
}
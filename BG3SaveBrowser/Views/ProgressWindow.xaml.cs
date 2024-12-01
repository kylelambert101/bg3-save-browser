using System.Windows;

namespace BG3SaveBrowser.Views;

public partial class ProgressWindow : Window
{
    public ProgressWindow()
    {
        InitializeComponent();
    }

    public void UpdateProgress(int progress, string status)
    {
        FileProgressBar.Value = progress;
        StatusText.Text = status;
    }
}
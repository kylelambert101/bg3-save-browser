using BG3SaveBrowser.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Application = System.Windows.Application;

namespace BG3SaveBrowser;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public static IServiceProvider ServiceProvider { get; private set; }

    public App()
    {
        InitializeComponent();
        ConfigureServices();
    }

    private void ConfigureServices()
    {
        var serviceCollection = new ServiceCollection();

        // Register your services
        serviceCollection.AddSingleton<FileProcessor>();
        
        // Build the service provider
        ServiceProvider = serviceCollection.BuildServiceProvider();
    }
}
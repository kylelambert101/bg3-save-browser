using BG3SaveBrowser.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
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

    private static void ConfigureServices()
    {
        var serviceCollection = new ServiceCollection();

        // Register your services
        serviceCollection
            .AddSingleton<DataExporter>()
            .AddSingleton<LsvFileProcessor>()
            .AddSingleton<SaveProcessor>()
            .AddSingleton<SavesDirectoryProcessor>()
        ;
        
        serviceCollection.AddLogging(builder =>
        {
            builder.ClearProviders(); // Optional: Clear default providers
            builder.AddSimpleConsole(options =>
            {
                options.IncludeScopes = true;
                options.SingleLine = true;
                options.TimestampFormat = "HH:mm:ss ";
                options.ColorBehavior = LoggerColorBehavior.Enabled;
            });
            builder.AddDebug();      // Add debug logging (visible in Output window)
            builder.SetMinimumLevel(LogLevel.Debug);
        });
        
        // Build the service provider
        ServiceProvider = serviceCollection.BuildServiceProvider();
    }
}
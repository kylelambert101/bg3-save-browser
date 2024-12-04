using BG3SaveBrowser.Views;
using Microsoft.Extensions.Logging;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace BG3SaveBrowser.Infrastructure.Logging;

public class RichTextBoxLogger : ILogger
{
    private readonly MainWindow _mainWindow;
    private readonly string _categoryName;

    public RichTextBoxLogger(MainWindow mainWindow, string categoryName)
    {
        _mainWindow = mainWindow;
        _categoryName = categoryName;
    }

    public IDisposable BeginScope<TState>(TState state) => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        var message = formatter(state, exception);
        _mainWindow.AppendLog(message, logLevel);
    }
}

public class RichTextBoxLoggerProvider : ILoggerProvider
{
    private readonly MainWindow _mainWindow;

    public RichTextBoxLoggerProvider(MainWindow mainWindow)
    {
        _mainWindow = mainWindow;
    }

    public ILogger CreateLogger(string categoryName) => new RichTextBoxLogger(_mainWindow, categoryName);

    public void Dispose() { }
}

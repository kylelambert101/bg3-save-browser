using System.Globalization;
using System.Windows.Data;

namespace BG3SaveBrowser.Infrastructure.Utilities;

public class UtcToLocalDateTimeConverter : IValueConverter
{
public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
{
    try
    {
        return DateTime.SpecifyKind(DateTime.Parse(value.ToString()), DateTimeKind.Utc).ToLocalTime();
    }
    catch
    {
        return null;
    }
}

public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
    throw new NotImplementedException();
}
}
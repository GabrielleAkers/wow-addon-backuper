using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace wow_addon_backuper;

class EnumConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Enum sourceEnum && parameter is string conversion && targetType.IsAssignableTo(typeof(string)))
        {
            switch (conversion)
            {
                case "separate_words":
                    return sourceEnum.SeparateWords();
                case "string":
                    return sourceEnum.StringValue();
                default:
                    break;
            }
        }
        return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
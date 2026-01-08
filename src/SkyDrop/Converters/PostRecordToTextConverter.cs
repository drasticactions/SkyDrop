using System;
using System.Globalization;
using Avalonia.Data.Converters;
using FishyFlip.Lexicon.App.Bsky.Feed;

namespace SkyDrop.Converters;

/// <summary>
/// Converts an ATObject (Post record) to its text content.
/// </summary>
public class PostRecordToTextConverter : IValueConverter
{
    public static readonly PostRecordToTextConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            Post post => post.Text ?? string.Empty,
            _ => string.Empty
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

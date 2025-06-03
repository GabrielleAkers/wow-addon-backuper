
using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace wow_addon_backuper;

[AttributeUsage(AttributeTargets.Field)]
public sealed class StringValueAttribute : Attribute
{
    public StringValueAttribute(string value)
    {
        Value = value;
    }

    public string Value { get; }
}

public static class EnumExtensions
{
    public static string StringValue<T>(this T value)
        where T : Enum
    {
        var fieldName = value.ToString();
        var field = typeof(T).GetField(fieldName, BindingFlags.Public | BindingFlags.Static);
        return field?.GetCustomAttribute<StringValueAttribute>()?.Value ?? fieldName;
    }

    public static string SeparateWords<T>(this T value) where T : Enum
    {
        var fieldName = value.ToString();
        string[] words = [.. Regex.Matches(fieldName, "([A-Z]+(?![a-z])|[A-Z][a-z]+|[0-9]+|[a-z]+)")
            .OfType<Match>()
            .Select(m => m.Value)];
        return string.Join(" ", words);
    }
}
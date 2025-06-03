using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json.Serialization;

namespace wow_addon_backuper;

record class AppSettings
{
    [JsonPropertyName("wow_install_dir")]
    public string? WowInstallDir { get; set; }

    public object this[string propertyName]
    {
        get
        {
            Type type = typeof(AppSettings);
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            PropertyInfo propertyInfo = type.GetProperty(propertyName);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8603 // Possible null reference return.
            return propertyInfo?.GetValue(this, null);
#pragma warning restore CS8603 // Possible null reference return.
        }
        set
        {
            Type type = typeof(AppSettings);
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            PropertyInfo propertyInfo = type.GetProperty(propertyName);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
            propertyInfo?.SetValue(this, value, null);
        }
    }
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(AppSettings))]
internal partial class AppSettingsJsonContext : JsonSerializerContext
{
}
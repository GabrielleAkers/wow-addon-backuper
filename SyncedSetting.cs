using System;
using System.ComponentModel;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace wow_addon_backuper;

public partial class SyncedSetting<T> : ObservableObject
{
    private readonly string _syncTo;
    private readonly string _name;

    public SyncedSetting(string name, string syncTo)
    {
        _name = name;
        _syncTo = syncTo;

        Task.Run(InitFromFile);

        PropertyChanged += HandleValueChanged;
    }

    private async Task InitFromFile()
    {
        var buffer = await Util.ReadFromFile(_syncTo);
        if (buffer != null)
        {
            AppSettings? app_settings = JsonSerializer.Deserialize(buffer, AppSettingsJsonContext.Default.AppSettings);
            if (app_settings == null)
                return;
            Value = (T)app_settings[_name];
        }
    }

    private async void HandleValueChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "Value" && sender != null)
        {
            var val = (sender as SyncedSetting<T>)!.Value;
            if (val == null) return;

            var buffer = await Util.ReadFromFile(_syncTo);
            AppSettings? app_settings = null;
            if (buffer != null)
                app_settings = JsonSerializer.Deserialize(buffer, AppSettingsJsonContext.Default.AppSettings);
            if (app_settings != null)
            {
                app_settings[_name] = val;
            }
            else
            {
                app_settings = new AppSettings();
                app_settings[_name] = val;
            }
            await Util.WriteToFile(_syncTo, JsonSerializer.SerializeToUtf8Bytes(app_settings, AppSettingsJsonContext.Default.AppSettings));
            OnPropertyChanged(_name);
        }
    }

    [ObservableProperty]
    private T? _value;
}
using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace wow_addon_backuper;

public partial class App : Application
{
    public static AppState AppState { get; } = new(new Dropbox.BearerToken());
    public static Dropbox.ApiClient Api { get; } = new(AppState.Token);

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        static async Task RefreshAndFetchAccount()
        {
            await Dropbox.OAuthHandler.Instance().RefreshToken(AppState.Token);
            if (!string.IsNullOrEmpty(AppState.Token.AccountId))
            {
                AppState.UserAccountInfo = await Api.GetAccount();
            }
        }
        Task.Run(RefreshAndFetchAccount);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = AppState,
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
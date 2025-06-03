using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace wow_addon_backuper;

public partial class App : Application
{
    public static MainViewModel AppState { get; } = new(new Dropbox.BearerToken());
    public static Dropbox.ApiClient DropboxApi { get; } = new(AppState.DropboxToken);

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        static async Task RefreshAndFetchAccount()
        {
            await Dropbox.OAuthHandler.Instance().RefreshToken(AppState.DropboxToken);
            if (!string.IsNullOrEmpty(AppState.DropboxToken.AccountId))
            {
                AppState.UserAccountInfo = await DropboxApi.GetAccount();
            }
        }
        Task.Run(RefreshAndFetchAccount);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = AppState
            };
            AppState.MainWindow = desktop.MainWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }
}
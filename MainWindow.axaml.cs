using Avalonia.Controls;
using Avalonia.Interactivity;

namespace wow_addon_backuper;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        // fix unhandled TaskCanceledException
        Closing += Util.OnWindowClosing;
    }

    private static async void DropBoxSignIn_OnClick(object? sender, RoutedEventArgs e)
    {
        await Dropbox.OAuthHandler.Instance().SignIn(App.AppState.Token);
        App.AppState.UserAccountInfo = await App.Api.GetAccount();
    }

    private static async void DropBoxCheck_OnClick(object? sender, RoutedEventArgs e)
    {
        await App.Api.CheckUser();
    }
}
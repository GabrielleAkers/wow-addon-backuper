using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace wow_addon_backuper;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        // fix unhandled TaskCanceledException
        Closing += Util.OnWindowClosing;
        SubscribeToWindowState();
    }

    private void CloseWindow(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void MaximizeWindow(object? sender, RoutedEventArgs e)
    {
        if (WindowState == WindowState.Normal)
            WindowState = WindowState.Maximized;
        else
            WindowState = WindowState.Normal;
    }

    private void MinimizeWindow(object? sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void SubscribeToWindowState()
    {
        ExtendClientAreaTitleBarHeightHint = 44;
        this.GetObservable(WindowStateProperty).Subscribe(s =>
        {
            Padding = OffScreenMargin;
            if (s != WindowState.Maximized)
            {
                MaximizeTooltip.Content = "Maximize window";
            }
            if (s == WindowState.Maximized)
            {
                MaximizeTooltip.Content = "Restore down";
            }
        });
    }

    private void StartPointerDrag(object? sender, PointerPressedEventArgs e)
    {
        var control = this.InputHitTest(e.GetPosition(this));

        // fix combobox with drag
        if (control is not null && control is not LightDismissOverlayLayer)
        {
            BeginMoveDrag(e);
        }
    }
}
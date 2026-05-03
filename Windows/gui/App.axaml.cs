using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Controls;
using JackBridge.GUI.ViewModels;
using JackBridge.GUI.Views;
using System;

namespace JackBridge.GUI;

public class App : Application
{
    public static bool StartMinimized { get; set; }

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow { DataContext = new MainWindowViewModel() };

            if (StartMinimized)
            {
                desktop.MainWindow.WindowState = WindowState.Minimized;
                desktop.MainWindow.ShowInTaskbar = false;
            }

            desktop.ShutdownRequested += (s, e) =>
            {
                (desktop.MainWindow?.DataContext as MainWindowViewModel)?.Cleanup();
            };

            desktop.ShutdownMode = Avalonia.Controls.ShutdownMode.OnMainWindowClose;
        }

        base.OnFrameworkInitializationCompleted();
    }

    public void TrayIcon_Show(object? sender, EventArgs e)
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainWindow = desktop.MainWindow;
            if (mainWindow != null)
            {
                mainWindow.ShowInTaskbar = true;
                mainWindow.Show();
                mainWindow.WindowState = WindowState.Normal;
                mainWindow.Activate();
            }
        }
    }

    public void TrayIcon_Exit(object? sender, EventArgs e)
    {
        (ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.Shutdown();
    }
}

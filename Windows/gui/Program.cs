using Avalonia;
using System;

namespace JackBridge.GUI;

class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        App.StartMinimized = args.Length > 0 && args[0] == "--minimized";
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}

using Avalonia;
using Avalonia.ReactiveUI;

namespace LiteDb.Studio.Avalonia;

class Program
{
    // This is the entry point
    [STAThread]
    public static void Main(string[] args)
    {
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration
    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder
            .Configure<App>()
            .UsePlatformDetect()
            .LogToTrace() // optional: specify areas if needed
            .UseReactiveUI();
    }
}
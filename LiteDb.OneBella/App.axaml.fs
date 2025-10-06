namespace OneBella

open Avalonia
open Avalonia.Controls.ApplicationLifetimes
open Avalonia.Markup.Xaml
open LiteDb.Studio.Avalonia.ViewModels
open LiteDb.Studio.Avalonia.Views

type App() =
    inherit Application()

    override this.Initialize() =
            AvaloniaXamlLoader.Load(this)

    override this.OnFrameworkInitializationCompleted() =
        match this.ApplicationLifetime with
        | :? IClassicDesktopStyleApplicationLifetime as desktop ->
             let vm = MainWindowViewModel()
             desktop.MainWindow <- MainWindow(DataContext = vm)
        | _ -> ()

        base.OnFrameworkInitializationCompleted()

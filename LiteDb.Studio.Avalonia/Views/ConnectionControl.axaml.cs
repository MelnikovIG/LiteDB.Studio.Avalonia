using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using LiteDb.Studio.Avalonia.ViewModels;

namespace LiteDb.Studio.Avalonia.Views;

public partial class ConnectionControl : UserControl
{
    public ConnectionControl()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public ConnectionViewModel? ViewModel => DataContext as ConnectionViewModel;

    private async void SelectDbFileClick(object? sender, RoutedEventArgs e)
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var storageProvider = desktop.MainWindow!.StorageProvider;
            var files = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
                {
                    Title = "Select the LiteDB file",
                    FileTypeFilter = [FilePickerFileTypes.All]
                }
            );
            
            var filePaths = files.Select(x => x.Path.LocalPath).ToArray();
            ViewModel?.Set(filePaths);
        }
    }
}
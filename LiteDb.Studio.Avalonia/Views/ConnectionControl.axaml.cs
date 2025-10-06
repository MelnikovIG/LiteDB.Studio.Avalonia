using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using LiteDb.Studio.Avalonia.ViewModels;

namespace LiteDb.Studio.Avalonia.Views;

public partial class ConnectionControl : UserControl
{
    public ConnectionControl()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public ConnectionViewModel ViewModel => DataContext as ConnectionViewModel;

    private async void SelectDbFileClick(object? sender, RoutedEventArgs e)
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var dialog = new OpenFileDialog
            {
                AllowMultiple = false,
                Title = "Select the LiteDB file"
            };

            if (this.Parent is Panel parentPanel &&
                parentPanel.Parent is Window window)
            {
                var files = await dialog.ShowAsync(window);
                ViewModel?.Set(files);
            }
        }
    }
}
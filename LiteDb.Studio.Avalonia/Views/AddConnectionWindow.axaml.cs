using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LiteDb.Studio.Avalonia.Core;
using LiteDb.Studio.Avalonia.ViewModels;

namespace LiteDb.Studio.Avalonia.Views;

public partial class AddConnectionWindow : Window
{
    public AddConnectionWindow(ConnectionViewModel conVm)
    {
        AvaloniaXamlLoader.Load(this);

        var control = this.FindControl<ConnectionControl>("ConnectionControl");
        control.DataContext = conVm;

        if (control.ViewModel != null)
        {
            control.ViewModel.Close = () => this.Close();
        }
    }

    public AddConnectionWindow()
        : this(new ConnectionViewModel([]))
    {
    }

    public Task<ConnParamType> SelectFileTask
    {
        get
        {
            var control = this.FindControl<ConnectionControl>("ConnectionControl");
            return control.ViewModel?.SelectFileTask!;
        }
    }
}
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using LiteDb.Studio.Avalonia.UseCases;
using LiteDb.Studio.Avalonia.ViewModels;

namespace LiteDb.Studio.Avalonia.Views;

public partial class MainWindow : Window
{
    private Flyout? fly;

    private MainWindowViewModel _vm => (this.DataContext as MainWindowViewModel)!;
    
    public MainWindow()
    {
        InitializeComponent();

        this.Opened += (_, __) => OpenConnectionWindowClick(this, null);

        this.Closed += (_, __) =>
        {
            var vm = _vm;
            Repo.Repo.Disconnect();
            foreach (var d in vm.DbItems)
            {
                if (d.IsConnected)
                    d.Disconnect();
            }
        };

        this.KeyDown += (sender, e) =>
        {
            if (e.Key == Key.F5)
            {
                var vm = _vm;
                if (vm.SelectedTab != null)
                {
                    ICommand cmd = vm.SelectedTab.RunCommand;
                    cmd.Execute(null);
                }
            }
        };
    }

    private void ScriptTabFlyoutOpened(object sender, EventArgs e)
    {
        fly = (Flyout)sender;
    }

    private void ScriptTabFlyoutClickYes(object sender, RoutedEventArgs e)
    {
        var main = _vm;
        if (main.Tabs.Count > 1)
        {
            var button = (Button)sender;
            var tab = (button.DataContext as ScriptViewModel)!;
            main.Tabs.Remove(tab);
        }

        fly?.Hide();
    }

    private void ScriptTabFlyoutClickNo(object sender, RoutedEventArgs e)
    {
        fly?.Hide();
    }

    private void OpenConnectionWindowClick(object sender, RoutedEventArgs? e)
    {
        async Task ShowAddWindowAsync(Window mainWindow, ConnectionViewModel conVm)
        {
            var w = new AddConnectionWindow(conVm);

            await w.ShowDialog(mainWindow);
            var con = await w.SelectFileTask;
            
            try
            {
                conVm.Error = "";
                _vm.Connect(con);
                var uc = StoredConnUseCase.Create(Repo.Repo.GetDb);
                StoredConnUseCase.Save(con, uc);
            }
            catch (Exception exc)
            {
                conVm.Error = exc.Message;
                await ShowAddWindowAsync(mainWindow, conVm);
            }
        }

        async Task RunAsync()
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var uc = StoredConnUseCase.Create(Repo.Repo.GetDb);
                var savedConnections = StoredConnUseCase.LoadAll(uc).ToArray();
                var vm = new ConnectionViewModel(savedConnections);
                await ShowAddWindowAsync(desktop.MainWindow!, vm);
            }
        }

        RunAsync().ConfigureAwait(false);
    }

    private void InitializeComponent()
    {
#if DEBUG
        this.AttachDevTools();
#endif
        AvaloniaXamlLoader.Load(this);
    }
}
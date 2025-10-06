using System.Reactive;
using ReactiveUI;

namespace LiteDb.Studio.Avalonia.ViewModels;

public class DbAction : ViewModelBase
{
    private string header = "";

    public DbAction(Action run)
    {
        Command = ReactiveCommand.Create(run);
    }

    public ReactiveCommand<Unit, Unit> Command { get; }

    public string Header
    {
        get => header;
        set => this.RaiseAndSetIfChanged(ref header, value);
    }
}
using System.Collections.ObjectModel;
using ReactiveUI;

namespace LiteDb.Studio.Avalonia.ViewModels;

public class DbItem : ViewModelBase
{
    private string title = "";
    private bool isExpanded = false;
    private bool isCollection = false;

    public DbItem()
    {
        Children = new ObservableCollection<DbItem>();
        ContextMenu = new ObservableCollection<DbAction>();
    }

    public ObservableCollection<DbItem> Children { get; }

    public ObservableCollection<DbAction> ContextMenu { get; }

    public string Title
    {
        get => title;
        set => this.RaiseAndSetIfChanged(ref title, value);
    }

    public bool IsExpanded
    {
        get => isExpanded;
        set => this.RaiseAndSetIfChanged(ref isExpanded, value);
    }

    public bool IsCollection
    {
        get => isCollection;
        set => this.RaiseAndSetIfChanged(ref isCollection, value);
    }

    public virtual bool IsConnected => true;

    public virtual void Disconnect()
    {
        // default implementation does nothing
    }
}
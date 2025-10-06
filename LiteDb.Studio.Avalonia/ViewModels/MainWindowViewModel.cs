using System.Collections.ObjectModel;
using System.Reactive;
using LiteDB;
using LiteDb.Studio.Avalonia.Core;
using LiteDb.Studio.Avalonia.Infra;
using ReactiveUI;

namespace LiteDb.Studio.Avalonia.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private ScriptViewModel _selectedTab;
    private readonly ObservableCollection<ScriptViewModel> _scriptTabs = new();
    public ObservableCollection<DbItem> DbItems { get; } = new();

    public MainWindowViewModel()
    {
        OpenNewTabCommand = ReactiveCommand.Create(OpenNewTabCommandRun);
    }

    private void OpenNewTabCommandRun()
    {
        var root = DbItems.FirstOrDefault();
        if (root is not DbFileItem f) return;
        OpenNewTab(() => f.LiteDb, f.ConnectionString.Filename, "<Todo>");
    }

    private void OpenNewTab(System.Func<LiteDatabase> getLiteDb, string dbFile, string tableName)
    {
        var scriptName = Utils.GetScriptName(_scriptTabs.Select(c => c.Header));
        var tab = new ScriptViewModel(getLiteDb, dbFile, scriptName)
        {
            Query = Utils.GetDefaultSql(tableName)
        };
        _scriptTabs.Add(tab);
        SelectedTab = tab;
    }

    private DbItem CreateTableItem(System.Func<LiteDatabase> getLiteDb, string dbFile, string tableName)
    {
        var temp = new DbItem { Title = tableName, IsCollection = true };
        var newTabAction = new DbAction(() => OpenNewTab(getLiteDb, dbFile, tableName))
        {
            Header = "open new tab"
        };
        temp.ContextMenu.Add(newTabAction);
        return temp;
    }

    public ReactiveCommand<Unit, Unit> OpenNewTabCommand { get; }

    public ObservableCollection<ScriptViewModel> Tabs => _scriptTabs;

    public ScriptViewModel SelectedTab
    {
        get => _selectedTab;
        set => this.RaiseAndSetIfChanged(ref _selectedTab, value);
    }

    public void Connect(ConnParamType con)
    {
        var dbFile = con.DbFile;
        var conString = ConnectionParameters.BuildConString(con);
        var liteDb = DbUtils.GetDb(conString);
        var name = Path.GetFileName(dbFile);
        var root = new DbFileItem(liteDb, conString)
        {
            Title = name,
            IsExpanded = true
        };

        var system = new DbItem { Title = "System" };
        root.Children.Add(system);

        var getLiteDb = () => root.LiteDb;

        var systemTables = DbUtils.GetSystemTables(liteDb);
        foreach (var doc in systemTables)
        {
            var item = new DbItem
            {
                Title = doc["name"].AsString,
                IsCollection = true
            };
            system.Children.Add(item);
        }

        var collections = DbUtils.GetCollectionNames(liteDb);
        foreach (var nameItem in collections)
        {
            var item = CreateTableItem(getLiteDb, dbFile, nameItem);
            root.Children.Add(item);
        }

        if (collections.Any())
            OpenNewTab(getLiteDb, dbFile, collections.First());
        else if (system.Children.Any())
            OpenNewTab(getLiteDb, dbFile, system.Children.First().Title);

        DbItems.Add(root);
        SelectedTab = _scriptTabs.FirstOrDefault();
    }
}
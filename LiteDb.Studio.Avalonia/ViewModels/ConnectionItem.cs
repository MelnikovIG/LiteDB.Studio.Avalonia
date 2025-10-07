using System.Collections.ObjectModel;
using System.Reactive;
using LiteDB;
using LiteDb.Studio.Avalonia.Core;
using LiteDb.Studio.Avalonia.Infra;
using LiteDb.Studio.Avalonia.UseCases;
using ReactiveUI;

namespace LiteDb.Studio.Avalonia.ViewModels;

public class ConnectionItem : ViewModelBase
{
    private readonly int _id;
    private bool _canDelete = true;
    private string _dbFile;
    private string _password = "";
    private bool _isDirect;
    private bool _isShared;
    private long _initSizeInMB;
    private bool _isReadOnly;
    private bool _isUpgradingFromV4;
    private string _selectedCulture;
    private string _selectedCompareOption;

    public ReactiveCommand<Unit, Unit> DeleteConnectionCommand { get; }

    public ConnectionItem(int id, ConnectionString cs, ObservableCollection<ConnectionItem> parent)
    {
        _id = id;
        _dbFile = cs.Filename;
        _isDirect = cs.Connection == ConnectionType.Direct;
        _isShared = !_isDirect;
        _initSizeInMB = cs.InitialSize;
        _isReadOnly = cs.ReadOnly;
        _isUpgradingFromV4 = cs.Upgrade;

        if (cs.Collation != null)
        {
            _selectedCulture = Array.Find(Utils.GetCultures(), x => x == cs.Collation.Culture.Name)!;
            _selectedCompareOption = Array.Find(Utils.GetCompareOptions(), x => x == cs.Collation.SortOptions.ToString())!;
        }
        else
        {
            _selectedCulture = "";
            _selectedCompareOption = "";
        }

        DeleteConnectionCommand = ReactiveCommand.Create(() =>
        {
            if (id > 0)
            {
                StoredConnUseCase.DeleteById(id, new StoredConnUseCase.T(Repo.Repo.GetDb));
                parent.Remove(this);
            }
        });
    }

    public string[] CompareOptions => Utils.GetCompareOptions();
    public string[] Cultures => Utils.GetCultures();

    public string SelectedCulture
    {
        get => _selectedCulture;
        set => this.RaiseAndSetIfChanged(ref _selectedCulture, value);
    }

    public string SelectedCompareOption
    {
        get => _selectedCompareOption;
        set => this.RaiseAndSetIfChanged(ref _selectedCompareOption, value);
    }

    public string DbFile
    {
        get => _dbFile;
        set => this.RaiseAndSetIfChanged(ref _dbFile, value);
    }

    public bool CanDelete
    {
        get => _canDelete;
        set => this.RaiseAndSetIfChanged(ref _canDelete, value);
    }

    public string Password
    {
        get => _password;
        set => this.RaiseAndSetIfChanged(ref _password, value);
    }

    public bool IsDirect
    {
        get => _isDirect;
        set => this.RaiseAndSetIfChanged(ref _isDirect, value);
    }

    public bool IsShared
    {
        get => _isShared;
        set => this.RaiseAndSetIfChanged(ref _isShared, value);
    }

    public bool IsReadOnly
    {
        get => _isReadOnly;
        set => this.RaiseAndSetIfChanged(ref _isReadOnly, value);
    }

    public long InitSizeInMB
    {
        get => _initSizeInMB;
        set => this.RaiseAndSetIfChanged(ref _initSizeInMB, value);
    }

    public bool IsUpgradingFromV4
    {
        get => _isUpgradingFromV4;
        set => this.RaiseAndSetIfChanged(ref _isUpgradingFromV4, value);
    }

    public ConnParamType GetParameters()
    {
        string collation = "";

        if (!string.IsNullOrWhiteSpace(_selectedCulture) && !string.IsNullOrWhiteSpace(_selectedCompareOption))
            collation = $"{_selectedCulture}/{_selectedCompareOption}";
        else if (!string.IsNullOrWhiteSpace(_selectedCulture))
            collation = _selectedCulture;

        return new ConnParamType
        {
            Id = _id,
            DbFile = DbFile,
            Password = Password,
            IsDirect = IsDirect,
            IsShared = IsShared,
            InitSizeInMB = InitSizeInMB,
            IsReadOnly = IsReadOnly,
            IsUpgradingFromV4 = IsUpgradingFromV4,
            Collation = collation
        };
    }
}
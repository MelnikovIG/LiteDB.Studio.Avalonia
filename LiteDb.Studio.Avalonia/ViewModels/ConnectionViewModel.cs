using System.Collections.ObjectModel;
using System.Reactive;
using LiteDB;
using LiteDb.Studio.Avalonia.Core;
using ReactiveUI;

namespace LiteDb.Studio.Avalonia.ViewModels
{
    public class ConnectionViewModel : ViewModelBase
    {
        private TaskCompletionSource<ConnParamType> _ts = new();
        private string _error = "";
        private Action _closeFunc = () => { };
        private readonly ObservableCollection<ConnectionItem> _connectionItems = new();
        private ConnectionItem _selectedConnectionItem = new(0, new ConnectionString(), new ObservableCollection<ConnectionItem>());

        public ReactiveCommand<Unit, Unit> ConnectCommand { get; }

        public ConnectionViewModel(ConnParamType[] savedConnections)
        {
            var conItems = _connectionItems;

            var items = savedConnections
                .Select(c =>
                {
                    var cstr = ConnectionParameters.BuildConString(c);
                    return new ConnectionItem(c.Id, cstr, conItems);
                })
                .ToList();

            var placeholder = new ConnectionItem(0, new ConnectionString { Filename = "<Select a db file>" }, conItems)
            {
                CanDelete = false
            };
            items.Insert(0, placeholder);

            foreach (var d in items)
            {
                if (File.Exists(d.DbFile))
                    conItems.Add(d);
            }

            if (conItems.Count > 0)
                _selectedConnectionItem = conItems[0];

            ConnectCommand = ReactiveCommand.Create(RunConnect);
        }

        private bool CanConnect()
        {
            var ok = !string.IsNullOrEmpty(_selectedConnectionItem.DbFile) &&
                     File.Exists(_selectedConnectionItem.DbFile);

            if (!ok)
                Error = "Please select a db file";

            return ok;
        }

        private void RunConnect()
        {
            if (CanConnect())
            {
                var param = _selectedConnectionItem.GetParameters();
                _ts.SetResult(param);
                _closeFunc();
            }
        }

        public Task<ConnParamType> SelectFileTask => _ts.Task;

        public Action Close
        {
            set => _closeFunc = value;
        }

        public ConnectionItem SelectedConnectionItem
        {
            get => _selectedConnectionItem;
            set => this.RaiseAndSetIfChanged(ref _selectedConnectionItem, value);
        }

        public ObservableCollection<ConnectionItem> ConnectionItems => _connectionItems;

        public bool CanConnectFlag =>
            !string.IsNullOrEmpty(SelectedConnectionItem.DbFile) &&
            File.Exists(SelectedConnectionItem.DbFile);

        public string Error
        {
            get => _error;
            set
            {
                this.RaiseAndSetIfChanged(ref _error, value);
                _ts = new TaskCompletionSource<ConnParamType>();
            }
        }

        public void Set(string[] result)
        {
            if (result == null || result.Length == 0)
                return;

            var found = _connectionItems.FirstOrDefault(d => d.DbFile == result[0]);

            if (found != null)
            {
                SelectedConnectionItem = found;
            }
            else
            {
                var connParams = SelectedConnectionItem.GetParameters();
                connParams.DbFile = result[0];
                var cs = ConnectionParameters.BuildConString(connParams);
                var c = new ConnectionItem(0, cs, _connectionItems);
                _connectionItems.Add(c);
                SelectedConnectionItem = c;
            }
        }
    }
}

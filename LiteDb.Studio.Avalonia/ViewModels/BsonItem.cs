using System.Reactive;
using LiteDB;
using LiteDb.Studio.Avalonia.Core;
using LiteDb.Studio.Avalonia.UseCases;
using ReactiveUI;

namespace LiteDb.Studio.Avalonia.ViewModels;

public class BsonItem : ReactiveObject
{
    private readonly string _name;
    private readonly BValType _bVal;
    private readonly int _index;
    private readonly BsonItem? _parent;
    private readonly string? _tableName;
    private readonly Func<LiteDatabase> _getLiteDb;

    private bool _isEditable;
    private bool _canShowEditor;
    private bool _isExpanded;
    private string _value;
    private string _storedValue;

    public BsonItem(string name, BValType bVal, int index, BsonItem? parent, string? tableName, Func<LiteDatabase> getLiteDb)
    {
        _name = name;
        _bVal = bVal;
        _index = index;
        _parent = parent;
        _tableName = tableName;
        _getLiteDb = getLiteDb;

        (Type, var displayValue) = bVal switch
        {
            BValType.Document d => (d.Value.Type, $"( {d.Value.Count} {(d.Value.Count > 1 ? "fields" : "field")} )"),
            BValType.Array d => (d.Value.Type, $"( {d.Value.Count} {(d.Value.Count > 1 ? "items" : "item")} )"),
            BValType.Nil d => (d.Value.Type, "<null>"),
            BValType.Bytes d => (d.Value.Type, $"{d.Value.SizeKB} KB"),
            BValType.Bool d => (d.Value.Type, d.Value.Value.ToString()),
            BValType.Decimal d => (d.Value.Type, d.Value.Value.ToString()),
            BValType.Double d => (d.Value.Type, d.Value.Value.ToString()),
            BValType.Guid d => (d.Value.Type, d.Value.Value.ToString()),
            BValType.Int d => (d.Value.Type, d.Value.Value.ToString()),
            BValType.Long d => (d.Value.Type, d.Value.Value.ToString()),
            BValType.String d => (d.Value.Type, d.Value.Value),
            BValType.DateTime d => (d.Value.Type, d.Value.Value.ToString()),
            BValType.ObjectId d => (d.Value.Type, d.Value.Value.ToString()),
            _ => ("unknown", "")
        };

        Children = bVal switch
        {
            BValType.Document d => d.Value.Value.Select(kv => new BsonItem(kv.Key, BVal.Create(kv.Value), -1, this, tableName, getLiteDb)).ToList(),
            BValType.Array d => d.Value.Value.Select((b, i) => new BsonItem($"[{i}]", BVal.Create(b), i, this, tableName, getLiteDb)).ToList(),
            _ => new List<BsonItem>()
        };

        _isEditable = parent != null &&
                      name != "_id" &&
                      parent.Type == "document" &&
                      BVal.FindObjectId(parent.BsonValue) != null &&
                      (bVal is BValType.String or BValType.Bool or BValType.Decimal or BValType.Double or BValType.Long or BValType.Int);

        _isExpanded = bVal is BValType.Array;
        _value = displayValue;
        _storedValue = _value;

        EditCommand = ReactiveCommand.Create(RunEditCommand);
    }

    private void RunEditCommand()
    {
        if (CanCommitChange)
        {
            var id = BVal.FindObjectId(_parent!.BsonValue);
            var sv = BVal.CreateBsonValue(_bVal, _storedValue);
            var nv = BVal.CreateBsonValue(_bVal, _value);
            var update = UpdateField.Create(_getLiteDb, _tableName!, _name, id, sv, nv);
            var updated = UpdateField.Run(update);
            if (updated) _storedValue = _value;
        }

        this.RaisePropertyChanged(nameof(CanCommitChange));
        CanShowEditor = !CanShowEditor;
    }

    public bool CanCommitChange => _isEditable && _storedValue != _value;
    public bool HasChildren => _bVal is BValType.Document or BValType.Array;
    public ReactiveCommand<Unit, Unit> EditCommand { get; }
    public BValType BsonValue => _bVal;

    public bool IsExpanded
    {
        get => _isExpanded;
        set => this.RaiseAndSetIfChanged(ref _isExpanded, value);
    }

    public IEnumerable<BsonItem> Children { get; }

    public string AsJson()
    {
        var list = new List<BsonValue> { BVal.GetRawValue(_bVal) };
        return DbUtils.ToJson(list);
    }

    public string Value
    {
        get => _value;
        set
        {
            this.RaiseAndSetIfChanged(ref _value, value);
            this.RaisePropertyChanged(nameof(CanCommitChange));
        }
    }

    public bool IsEditable
    {
        get => _isEditable;
        set => this.RaiseAndSetIfChanged(ref _isEditable, value);
    }

    public bool CanShowEditor
    {
        get => _canShowEditor;
        set => this.RaiseAndSetIfChanged(ref _canShowEditor, value);
    }

    public string Type { get; }

    public BsonItem Parent => _parent ?? default!;

    public string Name
    {
        get
        {
            if (_bVal is BValType.Document d && _index > -1)
            {
                var hasId = d.Value.Value.TryGetValue("_id", out var id);
                return hasId ? $"[{_index}] ( id = {id} )" : $"[{_index}]";
            }
            return _name;
        }
    }
}
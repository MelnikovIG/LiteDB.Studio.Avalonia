using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive;
using System.Text;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Threading;
using LiteDB;
using LiteDb.Studio.Avalonia.Core;
using LiteDb.Studio.Avalonia.Infra;
using LiteDb.Studio.Avalonia.UseCases;
using ReactiveUI;

namespace LiteDb.Studio.Avalonia.ViewModels;

public class ScriptViewModel : ViewModelBase, IDisposable
{
    private class Empty()
    {
        public static Empty Instance { get; } = new();
    };
    
    private int resultDisplayTabIndex = 0;
    private bool isBusy = false;
    private readonly StringBuilder logSb = new();
    private string query = "select * from DocRuleDao";

    private readonly ObservableCollection<BsonItem> result = new();
    private readonly PagingViewModel paging;
    private readonly HierarchicalTreeDataGridSource<BsonItem> source;

    private readonly Stopwatch querySw;
    private readonly System.Timers.Timer queryTimer;

    private readonly Func<LiteDatabase> db;
    private readonly string dbFile;
    private readonly string name;

    public ScriptViewModel(Func<LiteDatabase> db, string dbFile, string name)
    {
        this.db = db;
        this.dbFile = dbFile;
        this.name = name;

        paging = new PagingViewModel(result);

        source = new HierarchicalTreeDataGridSource<BsonItem>(paging.DisplaySource);
        source.Columns.Add(new HierarchicalExpanderColumn<BsonItem>(
            new TemplateColumn<BsonItem>("key", "BsonItemNameSelector"),
            x => x.Children,
            x => x.HasChildren,
            x => x.IsExpanded
        ));
        source.Columns.Add(new TemplateColumn<BsonItem>("value", "BsonItemValueSelector", width: new GridLength(4, GridUnitType.Star)));
        source.Columns.Add(new TextColumn<BsonItem, string>("type", b => b.Type, width: new GridLength(1, GridUnitType.Star)));

        querySw = Stopwatch.StartNew();
        queryTimer = new System.Timers.Timer(250.0) { AutoReset = true };
        queryTimer.Elapsed += (_, __) => paging.RunInfo = querySw.Elapsed.ToString("mm\\:ss\\.fff");

        RunCommand = ReactiveCommand.CreateFromTask(() => Task.Run(() => Execute(Query)));
        StopCommand = ReactiveCommand.Create(() => { isBusy = false; });
        BeginCommand = ReactiveCommand.CreateFromTask(() => Task.Run(() => Execute("BEGIN")));
        RollbackCommand = ReactiveCommand.CreateFromTask(() => Task.Run(() => Execute("ROLLBACK")));
        CommitCommand = ReactiveCommand.CreateFromTask(() => Task.Run(() => Execute("COMMIT")));
        CheckpointCommand = ReactiveCommand.CreateFromTask(() => Task.Run(Checkpoint));
        ShrinkCommand = ReactiveCommand.CreateFromTask(() => Task.Run(Shrink));
    }

    private void Info(string msg)
    {
        logSb.AppendLine(msg);
        Log.LogInfo(msg);
    }

    private void Err(Exception exc)
    {
        Log.LogExc(exc);
        logSb.AppendLine(exc.ToString());
    }

    private Empty BeforeRunSql()
    {
        querySw.Restart();
        queryTimer.Start();
        IsBusy = true;
        logSb.Clear();
        paging.RunInfo = "";
        result.Clear();
        return Empty.Instance;
    }

    private void AfterRunSql(BValType[] bsonValues)
    {
        queryTimer.Stop();
        querySw.Stop();
        IsBusy = false;

        Dispatcher.UIThread.Post(() =>
        {
            if (bsonValues != null)
            {
                string? tableName = RunSql.FindTableName(Query);
                foreach (var i in bsonValues)
                    result.Add(new BsonItem("result", i, -1, null, tableName, db)
                    {
                        IsExpanded = true
                    });

                paging.CalculatePages(querySw.Elapsed);

                ResultDisplayTabIndex = result.Count > 0 ? 0 : 1;
            }
        });
    }
    
    private void Execute(string sql)
    {
        using var cs = new CancellationTokenSource();

        Rop.Run(BeforeRunSql)
            .Log(_ => Info($"Executing {sql}"), Err)
            .Map(_ => RunSql.Run(RunSql.Create(querySw, sql, db, cs.Token)))
            .Log(_ => Info($"Done {sql}"), Err)
            .TryMapErr(_ => [])
            .Log(_ => Info("Showing query results"), Err)
            .Finish(AfterRunSql);
    }

    private void Checkpoint()
    {
        Rop.Run(BeforeRunSql)
            .Map(_ => db())
            .Inspect(_ => Info("Executing db checkpoint"), Err)
            .Map(d =>
            {
                DbUtils.Checkpoint(d).ConfigureAwait(false).GetAwaiter().GetResult();
                return Empty.Instance;
            })
            .Inspect(_ => Info("checkpoint done"), Err)
            .Finish(_ => AfterRunSql([]));
    }

    private void Shrink()
    {
        Rop.Run(() => BeforeRunSql())
            .Map(_ => db())
            .Inspect(_ => Info("Shrinking db"), Err)
            .Map(d =>
            {
                DbUtils.Shrink(d).ConfigureAwait(false).GetAwaiter().GetResult();
                return Empty.Instance;
            })
            .Inspect(_ => Info("shrink done"), Err)
            .Finish(_ => AfterRunSql([]));
    }

    public string QueryResultText => result.Count > 0 ? GetQueryJson() : logSb.ToString();

    private string GetQueryJson()
    {
        if (paging.DisplaySource.Count == 1)
            return paging.DisplaySource[0].AsJson();

        if (paging.DisplaySource.Count > 1)
        {
            var sb = new StringBuilder();
            var (start, end) = paging.GetCurrentPageBoundaries();
            for (int i = start; i <= end; i++)
                sb.AppendLine($"// ({i})").AppendLine(paging.DisplaySource[i].AsJson());
            return sb.ToString();
        }

        return "";
    }

    public string Header => $"{Path.GetFileName(dbFile)} - {name}";
    public PagingViewModel Paging => paging;
    public bool CanShowPaging => ResultDisplayTabIndex == 0;

    public int ResultDisplayTabIndex
    {
        get => resultDisplayTabIndex;
        set
        {
            this.RaiseAndSetIfChanged(ref resultDisplayTabIndex, value);
            this.RaisePropertyChanged(nameof(CanShowPaging));
            if (resultDisplayTabIndex == 1)
                this.RaisePropertyChanged(nameof(QueryResultText));
        }
    }

    public bool IsBusy
    {
        get => isBusy;
        set => this.RaiseAndSetIfChanged(ref isBusy, value);
    }

    public ReactiveCommand<Unit, Unit> BeginCommand { get; }
    public ReactiveCommand<Unit, Unit> ShrinkCommand { get; }
    public ReactiveCommand<Unit, Unit> RollbackCommand { get; }
    public ReactiveCommand<Unit, Unit> CommitCommand { get; }
    public ReactiveCommand<Unit, Unit> RunCommand { get; }
    public ReactiveCommand<Unit, Unit> StopCommand { get; }
    public ReactiveCommand<Unit, Unit> CheckpointCommand { get; }
    public HierarchicalTreeDataGridSource<BsonItem> Source => source;

    public string Query
    {
        get => query;
        set => this.RaiseAndSetIfChanged(ref query, value);
    }

    public void Dispose()
    {
        source.Dispose();
        queryTimer.Dispose();
    }
}
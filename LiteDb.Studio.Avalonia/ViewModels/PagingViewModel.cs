using System.Collections.ObjectModel;
using System.Reactive;
using LiteDb.Studio.Avalonia.Infra;
using ReactiveUI;

namespace LiteDb.Studio.Avalonia.ViewModels;

public class PagingViewModel : ViewModelBase
{
    private ObservableCollection<BsonItem> tempSource;
    private readonly Dictionary<int, (int start, int end)> pages = new();
    private readonly ObservableCollection<BsonItem> displaySource = new();
    private int pageSize = 50;
    private string runInfo = "";
    private int currentPage = 0;

    public PagingViewModel(ObservableCollection<BsonItem> source)
    {
        tempSource = source;

        StartPageCommand = ReactiveCommand.Create(StartPage);
        NextPageCommand = ReactiveCommand.Create(NextPage);
        BackPageCommand = ReactiveCommand.Create(BackPage);
        EndPageCommand = ReactiveCommand.Create(EndPage);
    }

    public ObservableCollection<BsonItem> DisplaySource => displaySource;

    public ReactiveCommand<Unit, Unit> StartPageCommand { get; }
    public ReactiveCommand<Unit, Unit> NextPageCommand { get; }
    public ReactiveCommand<Unit, Unit> BackPageCommand { get; }
    public ReactiveCommand<Unit, Unit> EndPageCommand { get; }

    public int PageSize
    {
        get => pageSize;
        set => this.RaiseAndSetIfChanged(ref pageSize, value);
    }

    public string RunInfo
    {
        get => runInfo;
        set => this.RaiseAndSetIfChanged(ref runInfo, value);
    }

    private void ShowPage(int pageNumber)
    {
        if (!pages.ContainsKey(pageNumber)) return;

        currentPage = pageNumber;
        displaySource.Clear();

        var (pageStart, pageEnd) = pages[pageNumber];
        for (int i = pageStart; i <= pageEnd; i++)
        {
            displaySource.Add(tempSource[i]);
        }

        if (displaySource.Count == 1)
        {
            displaySource[0].IsExpanded = true;
        }
    }

    private void StartPage() => ShowPage(currentPage = 0);

    private void NextPage()
    {
        if (pages.ContainsKey(currentPage + 1))
        {
            currentPage++;
            ShowPage(currentPage);
        }
    }

    private void BackPage()
    {
        if (pages.ContainsKey(currentPage - 1))
        {
            currentPage--;
            ShowPage(currentPage);
        }
    }

    private void EndPage()
    {
        if (pages.Count > 0)
        {
            currentPage = pages.Keys.Max();
            ShowPage(currentPage);
        }
    }

    private (bool ok, IEnumerable<BsonItem> flattened) TryFlatten(ObservableCollection<BsonItem> queryResult)
    {
        if (queryResult.Count == 0) return (false, Enumerable.Empty<BsonItem>());

        bool hasSingleDocResult = queryResult.Count == 1 && queryResult[0].Type == "document";
        bool documentHasSingleArrayChild = queryResult[0].Children.Count() == 1 && queryResult[0].Children.First().Type == "array";

        if (hasSingleDocResult && documentHasSingleArrayChild)
        {
            var docs = queryResult[0].Children.First().Children;
            return (docs.Count() > 0, docs);
        }

        return (false, Enumerable.Empty<BsonItem>());
    }

    public (int start, int end) GetCurrentPageBoundaries()
    {
        var page = pages[currentPage];
        return page;
    }

    public void CalculatePages(TimeSpan elapsed)
    {
        pages.Clear();
        displaySource.Clear();

        var (ok, flattened) = TryFlatten(tempSource);
        if (ok)
        {
            tempSource.Clear();
            foreach (var i in flattened)
                tempSource.Add(i);
        }

        if (tempSource.Count > 0)
        {
            var pageDict = Utils.GetPages(PageSize, tempSource.Count);
            foreach (var kvp in pageDict)
            {
                pages[kvp.Key] = kvp.Value;
            }

            ShowPage(0);
        }

        var d = tempSource.Count == 1 ? "document" : "documents";
        RunInfo = $"{tempSource.Count} {d} : {elapsed:mm\\:ss\\.fff}";
    }
}
using System.Xml;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.Folding;
using AvaloniaEdit.Highlighting;
using AvaloniaEdit.Highlighting.Xshd;

namespace LiteDb.Studio.Avalonia.Views;

public partial class JsonTextEditor : UserControl
{
    private DispatcherTimer foldingTimer;
    private CharFoldingStrategy folding;
    private FoldingManager? foldingManager;

    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<JsonTextEditor, string>(
            nameof(Text),
            defaultValue: "",
            defaultBindingMode: BindingMode.TwoWay,
            coerce: OnCoerceText);

    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public JsonTextEditor()
    {
        AvaloniaXamlLoader.Load(this);

        foldingTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
        foldingTimer.Tick += (s, e) => UpdateFoldings();

        var editor = this.FindControl<TextEditor>("Editor")!;
        editor.Document = new TextDocument { Text = "" };
        editor.TextChanged += (s, e) => Text = editor.Text;

        folding = new CharFoldingStrategy('{', '}');
        foldingTimer.IsEnabled = false;

        var assembly = typeof(JsonTextEditor).Assembly;
        using (var resource = assembly.GetManifestResourceStream("LiteDb.Studio.Avalonia.Resources.json.xshd"))
        {
            if (resource != null)
            {
                using (var reader = new XmlTextReader(resource))
                {
                    editor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                }
            }
        }
    }

    private void UpdateFoldings()
    {
        var editor = this.FindControl<TextEditor>("Editor")!;
        if (foldingManager == null)
        {
            foldingManager = FoldingManager.Install(editor.TextArea);
        }

        if (foldingManager != null && editor.Document.TextLength > 0)
        {
            folding.UpdateFoldings(foldingManager, editor.Document);
        }
    }

    private static string OnCoerceText(AvaloniaObject d, string arg)
    {
        var sender = (JsonTextEditor)d;
        var editor = sender.FindControl<TextEditor>("Editor")!;

        if (editor.Text != arg)
        {
            editor.Text = arg;
            sender.foldingTimer.IsEnabled = true;
        }

        return arg;
    }
}
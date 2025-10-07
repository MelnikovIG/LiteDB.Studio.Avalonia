using System.Xml;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.Highlighting;
using AvaloniaEdit.Highlighting.Xshd;

namespace LiteDb.Studio.Avalonia.Views;

public partial class SqlTextEditor : UserControl
{
    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<SqlTextEditor, string>(
            nameof(Text),
            defaultValue: "",
            defaultBindingMode: BindingMode.TwoWay,
            coerce: OnCoerceText);

    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public SqlTextEditor()
    {
        AvaloniaXamlLoader.Load(this);

        var editor = this.FindControl<TextEditor>("Editor")!;
        editor.Document = new TextDocument { Text = "" };

        editor.TextChanged += (s, e) =>
        {
            Text = editor.Text;
        };

        var assembly = typeof(SqlTextEditor).Assembly;
        using (var resource = assembly.GetManifestResourceStream("LiteDb.Studio.Avalonia.Resources.sql.xshd"))
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

    private static string OnCoerceText(AvaloniaObject d, string arg)
    {
        var sender = (SqlTextEditor)d;
        var editor = sender.FindControl<TextEditor>("Editor")!;
        if (editor.Text != arg)
        {
            editor.Text = arg;
        }
        return arg;
    }
}
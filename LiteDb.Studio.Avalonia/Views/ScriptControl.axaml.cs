using System.Reactive;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LiteDb.Studio.Avalonia.ViewModels;

namespace LiteDb.Studio.Avalonia.Views;

public partial class ScriptControl : UserControl
{
    public ScriptControl()
    {
        AvaloniaXamlLoader.Load(this);

        var tree = this.FindControl<TreeDataGrid>("TreeDataGridResults")!;

        tree.DoubleTapped += (sender, e) =>
        {
            if (e.Source is StyledElement element)
            {
                if (element.DataContext is BsonItem item)
                {
                    item.EditCommand?.Execute(Unit.Default);
                }
            }
        };
    }
}
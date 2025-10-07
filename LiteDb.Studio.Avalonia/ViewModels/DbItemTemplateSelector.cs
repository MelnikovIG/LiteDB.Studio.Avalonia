using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;

namespace LiteDb.Studio.Avalonia.ViewModels;

public class DbItemTemplateSelector : IDataTemplate
{
    [Content]
    public Dictionary<string, IDataTemplate> AvailableTemplates { get; set; } = new();

    public Control Build(object? param)
    {
        if (param is not DbItem item)
            return new TextBlock { Text = "Invalid DbItem" };

        var key = item.IsCollection ? "Table" : "Database";

        if (AvailableTemplates.TryGetValue(key, out var template))
            return template.Build(param)!;

        // fallback control
        return new TextBlock { Text = $"No template found for key '{key}'" };
    }

    public bool Match(object? data) => data is DbItem;
}
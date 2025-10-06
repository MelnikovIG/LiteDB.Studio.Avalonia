using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;

namespace LiteDb.Studio.Avalonia.ViewModels;

public class ValueTemplateSelector : IDataTemplate
{
    [Content]
    public Dictionary<string, IDataTemplate> AvailableTemplates { get; set; } = new();

    public Control Build(object param)
    {
        if (param is not BsonItem item)
            return new TextBlock { Text = "Invalid item" };

        string key = "Others";

        bool isJson = item.Type == "string" &&
                      (item.Value.StartsWith("{") || item.Value.StartsWith("["));

        if (isJson && AvailableTemplates.ContainsKey("JsonValue"))
            key = "JsonValue";
        else if (item.Type == "array" && AvailableTemplates.ContainsKey("ArrayValue"))
            key = "ArrayValue";
        else if (item.Type == "document" && AvailableTemplates.ContainsKey("DocValue"))
            key = "DocValue";
        else if (item.Type == "bool" && AvailableTemplates.ContainsKey("BoolValue"))
            key = "BoolValue";
        else if (item.Type == "string" && AvailableTemplates.ContainsKey("StringValue"))
            key = "StringValue";
        else if ((item.Type == "decimal" ||
                  item.Type == "double" ||
                  item.Type == "int" ||
                  item.Type == "long") &&
                 AvailableTemplates.ContainsKey("NumberValue"))
            key = "NumberValue";
        else if (AvailableTemplates.ContainsKey("OthersValue"))
            key = "OthersValue";
        else if (item.Type == "document" && AvailableTemplates.ContainsKey("Doc"))
            key = "Doc";

        if (AvailableTemplates.TryGetValue(key, out var template))
            return template.Build(param);

        // fallback control
        return new TextBlock { Text = $"No template for: {item.Type}" };
    }

    public bool Match(object data) => data is BsonItem;
}
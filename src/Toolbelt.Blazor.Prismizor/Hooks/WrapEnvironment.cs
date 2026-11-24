namespace Toolbelt.Blazor.Prismizor.Hooks;

/// <summary>
/// The environment that <c>Token.stringify</c> passes to the wrap hooks.
/// </summary>
internal sealed class WrapEnvironment(string type, string content, List<string> classes, string? language)
{
    internal string Type { get; } = type;

    internal string Content { get; set; } = content;

    internal List<string> Classes { get; } = classes;

    internal List<KeyValuePair<string, string>> Attributes { get; } = [];

    internal string? Language { get; } = language;

    internal void SetAttribute(string name, string value)
    {
        var index = this.Attributes.FindIndex(a => a.Key == name);
        if (index < 0) this.Attributes.Add(new(name, value));
        else this.Attributes[index] = new(name, value);
    }
}

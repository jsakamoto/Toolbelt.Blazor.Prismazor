namespace Toolbelt.Blazor.Prismazor.Hooks;

/// <summary>
/// The environment that <c>Prism.highlight</c> passes to the before-tokenize and after-tokenize hooks.
/// </summary>
internal sealed class HookEnvironment(string code, Grammar grammar, string? language)
{
    internal string Code { get; set; } = code;

    internal Grammar Grammar { get; set; } = grammar;

    internal string? Language { get; } = language;

    internal List<object> Tokens { get; set; } = [];

    /// <summary>
    /// Gets or sets the template expressions that were replaced with placeholders, by the index in the placeholder.
    /// </summary>
    internal SortedDictionary<int, string>? TokenStack { get; set; }
}

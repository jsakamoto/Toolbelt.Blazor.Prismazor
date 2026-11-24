using Toolbelt.Blazor.Prismizor.Hooks;

namespace Toolbelt.Blazor.Prismizor;

/// <summary>
/// Represents the grammar of a language that the highlighter uses to split code into tokens.
/// </summary>
public sealed class Grammar
{
    /// <summary>
    /// Gets the Prism language ID of this grammar, such as "csharp". It is null for a nested grammar.
    /// </summary>
    public string? Id { get; }

    internal IReadOnlyList<GrammarToken> Tokens { get; private set; } = [];

    internal GrammarHooks? Hooks { get; set; }

    internal Grammar(string? id = null)
    {
        this.Id = id;
    }

    // Tokens are set after construction because grammars can reference each other in cycles.
    internal void Define(GrammarToken[] tokens)
    {
        this.Tokens = tokens;
    }
}

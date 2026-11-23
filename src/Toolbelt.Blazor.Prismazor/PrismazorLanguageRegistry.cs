using System.Collections.Concurrent;

namespace Toolbelt.Blazor.Prismazor;

/// <summary>
/// Holds the languages that other languages look up by name while they are highlighted, such as the code blocks of Markdown.
/// It plays the role of <c>Prism.languages</c> in PrismJS. A language that is not registered is left plain, just as PrismJS
/// leaves the code block of a language that is not loaded.
/// </summary>
public static class PrismazorLanguageRegistry
{
    private static readonly ConcurrentDictionary<string, Grammar> Grammars = new();

    /// <summary>
    /// Makes the given language available by its ID and by the given aliases.
    /// </summary>
    /// <param name="grammar">The grammar of the language, such as <see cref="PrismazorLanguages.CSharp"/>.</param>
    /// <param name="aliases">The other names of the language, such as "cs".</param>
    public static void Register(Grammar grammar, params string[] aliases)
    {
        ArgumentNullException.ThrowIfNull(grammar);
        ArgumentNullException.ThrowIfNull(aliases);

        if (grammar.Id is { } id) Grammars[id] = grammar;
        foreach (var alias in aliases)
        {
            Grammars[alias] = grammar;
        }
    }

    /// <summary>
    /// Removes all registered languages.
    /// </summary>
    public static void Clear() => Grammars.Clear();

    internal static bool TryGet(string id, out Grammar grammar) => Grammars.TryGetValue(id, out grammar!);
}

namespace Toolbelt.Blazor.Prismizor.Generator;

/// <summary>
/// The object graph of Prism grammars that extract.mjs writes.
/// </summary>
internal record GrammarsFile(string PrismVersion, string[] HookSourceIds, LanguageEntry[] Languages, GrammarNode[] Grammars, PatternNode[] Patterns);

/// <summary>
/// A language to generate, with the index of its root grammar.
/// </summary>
internal record LanguageEntry(string Id, string ClassName, string Title, int Grammar, TemplatingEntry? Templating = null);

/// <summary>
/// What the hooks of a template language ask markup-templating to do. The language is replaced with placeholders that
/// <see cref="Source"/> matches, and the rest is tokenized with the grammar at the index <see cref="Grammar"/>.
/// </summary>
internal record TemplatingEntry(string Source, string Flags, bool HasFilter, int Grammar);

/// <summary>
/// A grammar, which is an ordered list of tokens.
/// </summary>
internal record GrammarNode(TokenNode[] Tokens);

/// <summary>
/// A token of a grammar, with the indexes of its patterns.
/// </summary>
internal record TokenNode(string Name, int[] Patterns);

/// <summary>
/// A pattern of a token, in the form of the JavaScript regular expression.
/// </summary>
internal record PatternNode(string Source, string Flags, bool Lookbehind, bool Greedy, string[]? Alias, int? Inside);

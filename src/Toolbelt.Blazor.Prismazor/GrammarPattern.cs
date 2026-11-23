using System.Text.RegularExpressions;

namespace Toolbelt.Blazor.Prismazor;

internal sealed class GrammarPattern(
    string pattern,
    RegexOptions options = RegexOptions.None,
    bool lookbehind = false,
    bool greedy = false,
    string[]? alias = null,
    Grammar? inside = null)
{
    private Regex? regex;

    // Created on first use so that patterns of unused tokens cost nothing at startup.
    internal Regex Regex => this.regex ??= new Regex(this.Source, this.Options);

    internal string Source { get; } = pattern;

    internal RegexOptions Options { get; } = options;

    internal bool Lookbehind { get; } = lookbehind;

    internal bool Greedy { get; } = greedy;

    internal string[]? Alias { get; } = alias;

    internal Grammar? Inside { get; } = inside;
}

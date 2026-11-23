namespace Toolbelt.Blazor.Prismazor;

internal sealed class GrammarToken(string name, GrammarPattern[] patterns)
{
    internal string Name { get; } = name;

    internal GrammarPattern[] Patterns { get; } = patterns;
}

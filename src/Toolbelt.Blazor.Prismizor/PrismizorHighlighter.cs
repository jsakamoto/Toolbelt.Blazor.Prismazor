using System.Text;
using Toolbelt.Blazor.Prismizor.Hooks;

namespace Toolbelt.Blazor.Prismizor;

/// <summary>
/// Converts code into HTML with syntax highlighting, in the same format as <c>Prism.highlight</c>.
/// </summary>
public static class PrismizorHighlighter
{
    /// <summary>
    /// Returns the HTML of the given code with syntax highlighting.
    /// </summary>
    /// <param name="code">The code to highlight.</param>
    /// <param name="grammar">The grammar of the language of the code, such as <see cref="PrismizorLanguages.CSharp"/>.</param>
    public static string Highlight(string code, Grammar grammar)
    {
        ArgumentNullException.ThrowIfNull(code);
        ArgumentNullException.ThrowIfNull(grammar);

        // The same steps as Prism.highlight. The hooks of a language only see the code of that language.
        var hooks = grammar.Hooks;
        var env = new HookEnvironment(code, grammar, grammar.Id);
        hooks?.BeforeTokenize(env);
        env.Tokens = Tokenizer.Tokenize(env.Code, env.Grammar);
        hooks?.AfterTokenize(env);

        var builder = new StringBuilder(code.Length * 2);
        Stringify(builder, env.Tokens, hooks, grammar.Id);
        return builder.ToString();
    }

    private static void Stringify(StringBuilder builder, object content, GrammarHooks? hooks, string? language)
    {
        switch (content)
        {
            case string text:
                AppendEncoded(builder, text);
                break;

            case List<object> list:
                foreach (var item in list)
                {
                    Stringify(builder, item, hooks, language);
                }
                break;

            case Token token:
                StringifyToken(builder, token, hooks, language);
                break;
        }
    }

    private static void StringifyToken(StringBuilder builder, Token token, GrammarHooks? hooks, string? language)
    {
        // The wrap hook of prism-markup.js, which shows the real character of an entity, works for every language.
        var isEntity = token.Type == "entity";
        if (!isEntity && hooks?.HasWrap != true)
        {
            builder.Append("<span class=\"token ").Append(token.Type);
            AppendAliases(builder, token.Alias);
            builder.Append("\">");
            Stringify(builder, token.Content, hooks, language);
            builder.Append("</span>");
            return;
        }

        var inner = new StringBuilder();
        Stringify(inner, token.Content, hooks, language);
        var classes = new List<string> { "token", token.Type };
        if (token.Alias is { } aliases) classes.AddRange(aliases);
        var env = new WrapEnvironment(token.Type, inner.ToString(), classes, language);

        if (isEntity)
        {
            var ampIndex = env.Content.IndexOf("&amp;", StringComparison.Ordinal);
            env.SetAttribute("title", ampIndex < 0 ? env.Content : string.Concat(env.Content.AsSpan(0, ampIndex), "&", env.Content.AsSpan(ampIndex + 5)));
        }
        hooks?.Wrap(env);

        builder.Append("<span class=\"").AppendJoin(' ', env.Classes).Append('"');
        foreach (var (name, value) in env.Attributes)
        {
            builder.Append(' ').Append(name).Append("=\"").Append(value.Replace("\"", "&quot;")).Append('"');
        }
        builder.Append('>').Append(env.Content).Append("</span>");
    }

    private static void AppendAliases(StringBuilder builder, string[]? aliases)
    {
        if (aliases is null) return;
        foreach (var alias in aliases)
        {
            builder.Append(' ').Append(alias);
        }
    }

    // Same as Prism.util.encode. It does not escape '>' or quotes.
    private static void AppendEncoded(StringBuilder builder, string text)
    {
        foreach (var c in text)
        {
            switch (c)
            {
                case '&': builder.Append("&amp;"); break;
                case '<': builder.Append("&lt;"); break;
                case ' ': builder.Append(' '); break;
                default: builder.Append(c); break;
            }
        }
    }
}

using System.Text.RegularExpressions;

namespace Toolbelt.Blazor.Prismizor.Hooks;

/// <summary>
/// The hooks of a language that is embedded in markup, which are the before-tokenize and after-tokenize hooks
/// that call <c>buildPlaceholders</c> and <c>tokenizePlaceholders</c> of prism-markup-templating.js.
/// </summary>
/// <param name="owner">The grammar of the template language.</param>
/// <param name="placeholderGrammar">The grammar for the text around the template expressions, which is markup.</param>
/// <param name="pattern">The pattern of the template expressions, which are replaced with placeholders.</param>
/// <param name="options">The options of <paramref name="pattern"/>.</param>
/// <param name="guard">The pattern that the code has to match for the hooks to work. PHP, for example, needs an opening tag.</param>
/// <param name="filter">Creates a function that decides for each match whether it is replaced. It is created for each call.</param>
internal sealed class TemplatingHooks(
    Grammar owner,
    Grammar placeholderGrammar,
    string pattern,
    RegexOptions options = RegexOptions.None,
    string? guard = null,
    Func<Func<string, bool>>? filter = null) : GrammarHooks
{
    private Regex? regex;

    private Regex? guardRegex;

    private Regex Regex => this.regex ??= new Regex(pattern, options);

    private string Language => owner.Id!;

    internal override void BeforeTokenize(HookEnvironment env)
    {
        if (guard is not null && !(this.guardRegex ??= new Regex(guard)).IsMatch(env.Code))
        {
            return;
        }

        var code = env.Code;
        var language = this.Language;
        var tokenStack = env.TokenStack = new SortedDictionary<int, string>();
        var replaceFilter = filter?.Invoke();
        var nextIndex = 0;

        env.Code = this.Regex.Replace(code, match =>
        {
            if (replaceFilter is not null && !replaceFilter(match.Value))
            {
                return match.Value;
            }

            // Skip the placeholders that the code already has.
            var index = nextIndex;
            var placeholder = GetPlaceholder(language, index);
            while (code.Contains(placeholder, StringComparison.Ordinal))
            {
                placeholder = GetPlaceholder(language, ++index);
            }

            tokenStack[index] = match.Value;
            nextIndex = index + 1;
            return placeholder;
        });

        env.Grammar = placeholderGrammar;
    }

    internal override void AfterTokenize(HookEnvironment env)
    {
        if (env.TokenStack is not { } tokenStack)
        {
            return;
        }

        env.Grammar = owner;

        var language = this.Language;
        var keys = tokenStack.Keys.ToList();
        var j = 0;

        List<object> WalkTokens(List<object> tokens)
        {
            for (var i = 0; i < tokens.Count; i++)
            {
                // All placeholders are replaced already.
                if (j >= keys.Count) break;

                var token = tokens[i];
                var text = token switch
                {
                    string s => s,
                    Token { Content: string { Length: > 0 } s } => s,
                    _ => null
                };

                if (text is not null)
                {
                    var key = keys[j];
                    var expression = tokenStack[key];
                    var placeholder = GetPlaceholder(language, key);

                    var index = text.IndexOf(placeholder, StringComparison.Ordinal);
                    if (index > -1)
                    {
                        j++;

                        var before = text[..index];
                        var middle = new Token(language, Tokenizer.Tokenize(expression, env.Grammar), [$"language-{language}"], expression.Length);
                        var after = text[(index + placeholder.Length)..];

                        var replacement = new List<object>();
                        if (before.Length > 0) replacement.AddRange(WalkTokens([before]));
                        replacement.Add(middle);
                        if (after.Length > 0) replacement.AddRange(WalkTokens([after]));

                        if (token is string)
                        {
                            tokens.RemoveAt(i);
                            tokens.InsertRange(i, replacement);
                        }
                        else
                        {
                            ((Token)token).Content = replacement;
                        }
                    }
                }
                else if (token is Token { Content: List<object> content })
                {
                    WalkTokens(content);
                }
            }

            return tokens;
        }

        WalkTokens(env.Tokens);
    }

    private static string GetPlaceholder(string language, int index) => $"___{language.ToUpperInvariant()}{index}___";
}

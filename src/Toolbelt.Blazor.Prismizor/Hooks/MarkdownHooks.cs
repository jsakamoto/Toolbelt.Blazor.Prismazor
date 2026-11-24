using System.Text.RegularExpressions;
using Toolbelt.Blazor.Prismizor.Languages;

namespace Toolbelt.Blazor.Prismizor.Hooks;

/// <summary>
/// The hooks of markdown, which are the after-tokenize and wrap hooks of prism-markdown.js.
/// They highlight the fenced code blocks with the languages in <see cref="PrismizorLanguageRegistry"/>.
/// </summary>
internal sealed partial class MarkdownHooks : GrammarHooks
{
    // A list of known entity names. It is always incomplete to save space, as in prism-markdown.js.
    private static readonly Dictionary<string, string> KnownEntityNames = new()
    {
        ["amp"] = "&",
        ["lt"] = "<",
        ["gt"] = ">",
        ["quot"] = "\"",
    };

    private static Regex? tagPattern;

    // JavaScript word boundaries and \w are ASCII only.
    [GeneratedRegex(@"\b#", RegexOptions.ECMAScript)]
    private static partial Regex SharpPattern();

    [GeneratedRegex(@"\b\+\+", RegexOptions.ECMAScript)]
    private static partial Regex PlusPlusPattern();

    [GeneratedRegex(@"[a-z][\w-]*", RegexOptions.ECMAScript | RegexOptions.IgnoreCase)]
    private static partial Regex LanguageNamePattern();

    [GeneratedRegex(@"&(\w{1,8}|#x?[\da-f]{1,8});", RegexOptions.ECMAScript | RegexOptions.IgnoreCase)]
    private static partial Regex EntityPattern();

    internal override bool HasWrap => true;

    internal override void AfterTokenize(HookEnvironment env)
    {
        WalkTokens(env.Tokens);
    }

    internal override void Wrap(WrapEnvironment env)
    {
        if (env.Type != "code-block")
        {
            return;
        }

        var codeLang = "";
        foreach (var cls in env.Classes)
        {
            var index = cls.IndexOf("language-", StringComparison.Ordinal);
            if (index >= 0 && index + "language-".Length < cls.Length)
            {
                codeLang = cls[(index + "language-".Length)..];
                break;
            }
        }

        // PrismJS loads a missing language with the autoloader plugin, but there is nothing like it here.
        if (PrismizorLanguageRegistry.TryGet(codeLang, out var grammar))
        {
            env.Content = PrismizorHighlighter.Highlight(TextContent(env.Content), grammar);
        }
    }

    private static void WalkTokens(List<object> tokens)
    {
        foreach (var token in tokens)
        {
            if (token is not Token { Type: "code" } codeToken)
            {
                if (token is Token { Content: List<object> children })
                {
                    WalkTokens(children);
                }
                continue;
            }

            // Add the correct "language-xxxx" class to this code block. The "code-language" token is optional, but
            // the grammar is defined so that there is only one case to handle, which looks like this.
            //   [punctuation "```", code-language "xxxx", "\n", code-block "...", "\n", punctuation "```"]
            if (TokenUtil.ChildAt(codeToken.Content, 1) is Token { Type: "code-language", Content: string codeLanguage }
                && TokenUtil.ChildAt(codeToken.Content, 3) is Token { Type: "code-block" } codeBlock)
            {
                // This might be a language that Prism does not support. C++, C# and F# need replacements.
                var lang = SharpPattern().Replace(codeLanguage, "sharp");
                lang = PlusPlusPattern().Replace(lang, "pp");
                // Only use the first word.
                lang = LanguageNamePattern().Match(lang) is { Success: true } match ? match.Value.ToLowerInvariant() : "";

                var alias = $"language-{lang}";
                codeBlock.Alias = codeBlock.Alias is { } aliases ? [.. aliases, alias] : [alias];
            }
        }
    }

    // Returns the text content of an HTML source code string.
    private static string TextContent(string html)
    {
        // Remove all tags.
        tagPattern ??= CreateTagPattern();
        var text = tagPattern.Replace(html, "");

        // Decode the known entities.
        return EntityPattern().Replace(text, match =>
        {
            var code = match.Groups[1].Value.ToLowerInvariant();
            if (code[0] != '#')
            {
                return KnownEntityNames.TryGetValue(code, out var known) ? known : match.Value;
            }

            var value = code[1] == 'x' ? Convert.ToInt32(code[2..], 16) : int.Parse(code[1..]);
            return value is >= 0 and <= 0x10FFFF
                ? (value is >= 0xD800 and <= 0xDFFF ? ((char)value).ToString() : char.ConvertFromUtf32(value))
                : match.Value;
        });
    }

    // The pattern of the tag token of markup, as a global and case-insensitive one.
    private static Regex CreateTagPattern()
    {
        var tag = MarkupLanguage.Root.Tokens.First(token => token.Name == "tag").Patterns[0];
        return new Regex(tag.Source, tag.Options | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    }
}

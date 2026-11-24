namespace Toolbelt.Blazor.Prismizor.Hooks;

/// <summary>
/// The hooks of xquery, which are the after-tokenize hook of prism-xquery.js.
/// </summary>
internal sealed class XqueryHooks : TagTextHooks
{
    protected override bool IsOpeningBrace(List<object> tokens, int index, Token token)
    {
        // "{{" is not an expression, so a brace next to another one does not count.
        return token.Type == "punctuation" && token.Content is "{"
            && !(index + 1 < tokens.Count && tokens[index + 1] is Token { Type: "punctuation", Content: "{" })
            && !(index > 0 && tokens[index - 1] is Token { Type: "plain-text", Content: "{" });
    }

    protected override bool IsText(Token token) => token.Type != "comment";

    protected override object CreatePlainText(string plainText)
    {
        // The white space between the tags is not text.
        return plainText.Length > 0 && plainText.All(IsWhiteSpace) ? plainText : base.CreatePlainText(plainText);
    }

    // The white space of JavaScript, which is what \s of a JavaScript regular expression matches.
    private static bool IsWhiteSpace(char c) => (int)c is 9 or 10 or 11 or 12 or 13 or 32 or 0xA0 or 0x1680 or (>= 0x2000 and <= 0x200A) or 0x2028 or 0x2029 or 0x202F or 0x205F or 0x3000 or 0xFEFF;
}

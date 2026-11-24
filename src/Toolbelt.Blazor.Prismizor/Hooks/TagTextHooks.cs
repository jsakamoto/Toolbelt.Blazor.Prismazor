namespace Toolbelt.Blazor.Prismizor.Hooks;

/// <summary>
/// The after-tokenize hook that jsx and xquery have in common. It turns the text between the tags of an element
/// into a plain-text token, which the grammar cannot tell from the code in the braces.
/// </summary>
internal abstract class TagTextHooks : GrammarHooks
{
    private sealed class OpenedTag(string tagName)
    {
        internal string TagName { get; } = tagName;

        internal int OpenedBraces { get; set; }
    }

    internal override void AfterTokenize(HookEnvironment env)
    {
        this.WalkTokens(env.Tokens);
    }

    /// <summary>
    /// Returns whether the token at the index is a brace that opens an expression inside an element.
    /// </summary>
    protected virtual bool IsOpeningBrace(List<object> tokens, int index, Token token)
        => token.Type == "punctuation" && token.Content is "{";

    /// <summary>
    /// Returns whether the token is a part of the text between the tags, if it is outside of the braces.
    /// </summary>
    protected virtual bool IsText(Token token) => true;

    /// <summary>
    /// Creates the token or the string that replaces the text between the tags.
    /// </summary>
    protected virtual object CreatePlainText(string plainText) => new Token("plain-text", plainText, null, plainText.Length);

    private void WalkTokens(List<object> tokens)
    {
        var openedTags = new List<OpenedTag>();
        for (var i = 0; i < tokens.Count; i++)
        {
            var token = tokens[i];
            var notTagNorBrace = false;

            if (token is Token t)
            {
                if (t.Type == "tag" && TokenUtil.ChildAt(t.Content, 0) is Token { Type: "tag" } tagStart)
                {
                    // We found a tag, now find its kind.
                    if (TokenUtil.ChildAt(tagStart.Content, 0) is Token { Content: "</" })
                    {
                        // Closing tag. Pop the matching opening tag.
                        if (openedTags.Count > 0 && openedTags[^1].TagName == TokenUtil.Stringify(TokenUtil.ChildAt(tagStart.Content, 1)))
                        {
                            openedTags.RemoveAt(openedTags.Count - 1);
                        }
                    }
                    else if (TokenUtil.LastChild(t.Content) is not Token { Content: "/>" })
                    {
                        // Opening tag. An autoclosed tag, which ends with "/>", is ignored.
                        openedTags.Add(new(TokenUtil.Stringify(TokenUtil.ChildAt(tagStart.Content, 1))));
                    }
                }
                else if (openedTags.Count > 0 && this.IsOpeningBrace(tokens, i, t))
                {
                    // Here we might have entered an expression inside a tag.
                    openedTags[^1].OpenedBraces++;
                }
                else if (openedTags.Count > 0 && openedTags[^1].OpenedBraces > 0 && t.Type == "punctuation" && t.Content is "}")
                {
                    // Here we might have left an expression inside a tag.
                    openedTags[^1].OpenedBraces--;
                }
                else if (this.IsText(t))
                {
                    notTagNorBrace = true;
                }
            }

            if ((notTagNorBrace || token is string) && openedTags.Count > 0 && openedTags[^1].OpenedBraces == 0)
            {
                // Here we are inside a tag, and not inside an expression. That is plain text, so drop any tokens matched.
                var plainText = TokenUtil.Stringify(token);

                // And merge the text with the adjacent text.
                if (i < tokens.Count - 1 && TokenUtil.IsStringOrPlainText(tokens[i + 1]))
                {
                    plainText += TokenUtil.Stringify(tokens[i + 1]);
                    tokens.RemoveAt(i + 1);
                }
                if (i > 0 && TokenUtil.IsStringOrPlainText(tokens[i - 1]))
                {
                    plainText = TokenUtil.Stringify(tokens[i - 1]) + plainText;
                    tokens.RemoveAt(i - 1);
                    i--;
                }

                tokens[i] = this.CreatePlainText(plainText);
            }

            if (token is Token { Content: List<object> content })
            {
                this.WalkTokens(content);
            }
        }
    }
}

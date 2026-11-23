namespace Toolbelt.Blazor.Prismazor.Hooks;

/// <summary>
/// The hooks of graphql, which are the after-tokenize hook of prism-graphql.js. It adds aliases to the tokens in mutations.
/// </summary>
internal sealed class GraphqlHooks : GrammarHooks
{
    internal override void AfterTokenize(HookEnvironment env)
    {
        // The token stream that we want to customize.
        var validTokens = env.Tokens
            .OfType<Token>()
            .Where(token => token.Type is not ("comment" or "scalar"))
            .ToList();

        var currentIndex = 0;

        // Returns the token relative to the current index, or null if there is none.
        Token? GetToken(int offset)
        {
            var index = currentIndex + offset;
            return index >= 0 && index < validTokens.Count ? validTokens[index] : null;
        }

        // Returns whether the tokens relative to the current index have the given types.
        bool IsTokenType(string[] types)
        {
            for (var i = 0; i < types.Length; i++)
            {
                if (GetToken(i)?.Type != types[i])
                {
                    return false;
                }
            }
            return true;
        }

        // Returns the index of the closing bracket to the opening bracket before the current index, or -1 if there is none.
        int FindClosingBracket(string open, string close)
        {
            var stackHeight = 1;
            for (var i = currentIndex; i < validTokens.Count; i++)
            {
                var token = validTokens[i];
                if (token.Type == "punctuation" && token.Content is string content)
                {
                    if (content == open)
                    {
                        stackHeight++;
                    }
                    else if (content == close)
                    {
                        stackHeight--;
                        if (stackHeight == 0)
                        {
                            return i;
                        }
                    }
                }
            }
            return -1;
        }

        static void AddAlias(Token token, string alias)
        {
            token.Alias = token.Alias is { } aliases ? [.. aliases, alias] : [alias];
        }

        while (currentIndex < validTokens.Count)
        {
            var startToken = validTokens[currentIndex++];

            // Add special aliases for mutation tokens.
            if (startToken.Type != "keyword" || startToken.Content is not "mutation")
            {
                continue;
            }

            // The names of all input variables, if any.
            var inputVariables = new List<object>();

            if (IsTokenType(["definition-mutation", "punctuation"]) && GetToken(1)!.Content is "(")
            {
                // Definition. Skip "definition-mutation" and "punctuation".
                currentIndex += 2;

                var definitionEnd = FindClosingBracket("(", ")");
                if (definitionEnd == -1)
                {
                    continue;
                }

                // Find all input variables.
                for (; currentIndex < definitionEnd; currentIndex++)
                {
                    var token = GetToken(0)!;
                    if (token.Type == "variable")
                    {
                        AddAlias(token, "variable-input");
                        inputVariables.Add(token.Content);
                    }
                }

                currentIndex = definitionEnd + 1;
            }

            if (IsTokenType(["punctuation", "property-query"]) && GetToken(0)!.Content is "{")
            {
                // Skip the opening bracket.
                currentIndex++;

                AddAlias(GetToken(0)!, "property-mutation");

                if (inputVariables.Count > 0)
                {
                    var mutationEnd = FindClosingBracket("{", "}");
                    if (mutationEnd == -1)
                    {
                        continue;
                    }

                    // Give the references to input variables a special alias.
                    for (var i = currentIndex; i < mutationEnd; i++)
                    {
                        var variableToken = validTokens[i];
                        if (variableToken.Type == "variable" && inputVariables.Any(name => Equals(name, variableToken.Content)))
                        {
                            AddAlias(variableToken, "variable-input");
                        }
                    }
                }
            }
        }
    }
}

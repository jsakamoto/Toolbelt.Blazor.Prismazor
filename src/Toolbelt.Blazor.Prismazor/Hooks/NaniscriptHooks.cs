namespace Toolbelt.Blazor.Prismazor.Hooks;

/// <summary>
/// The hooks of naniscript, which are the after-tokenize hook of prism-naniscript.js. It marks the lines with unbalanced brackets.
/// </summary>
internal sealed class NaniscriptHooks : GrammarHooks
{
    internal override void AfterTokenize(HookEnvironment env)
    {
        foreach (var token in env.Tokens.OfType<Token>())
        {
            if (token.Type != "generic-text")
            {
                continue;
            }

            var content = TokenUtil.Stringify(token);
            if (!IsBracketsBalanced(content))
            {
                token.Type = "bad-line";
                token.Content = content;
            }
        }
    }

    private static bool IsBracketsBalanced(string input)
    {
        const string brackets = "[]{}";
        var stack = new Stack<int>();
        foreach (var bracket in input)
        {
            var bracketsIndex = brackets.IndexOf(bracket);
            if (bracketsIndex == -1)
            {
                continue;
            }

            if (bracketsIndex % 2 == 0)
            {
                stack.Push(bracketsIndex + 1);
            }
            else if (!stack.TryPop(out var expected) || expected != bracketsIndex)
            {
                return false;
            }
        }
        return stack.Count == 0;
    }
}

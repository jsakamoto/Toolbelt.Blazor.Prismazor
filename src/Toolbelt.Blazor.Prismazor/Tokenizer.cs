using System.Text.RegularExpressions;

namespace Toolbelt.Blazor.Prismazor;

/// <summary>
/// A port of <c>Prism.tokenize</c> and <c>matchGrammar</c> in prism-core.js. Keep it close to the original.
/// </summary>
internal static class Tokenizer
{
    private sealed class Node(object? value)
    {
        internal object? Value { get; } = value;

        internal Node? Prev { get; set; }

        internal Node? Next { get; set; }

        internal int Length => this.Value switch
        {
            string s => s.Length,
            Token t => t.Length,
            _ => 0
        };
    }

    private sealed class TokenList
    {
        internal Node Head { get; } = new(null);

        internal Node Tail { get; } = new(null);

        internal int Count { get; private set; }

        internal TokenList()
        {
            this.Head.Next = this.Tail;
            this.Tail.Prev = this.Head;
        }

        internal Node AddAfter(Node node, object value)
        {
            var next = node.Next!;
            var newNode = new Node(value) { Prev = node, Next = next };
            node.Next = newNode;
            next.Prev = newNode;
            this.Count++;
            return newNode;
        }

        internal void RemoveRange(Node node, int count)
        {
            var next = node.Next!;
            var i = 0;
            for (; i < count && next != this.Tail; i++)
            {
                next = next.Next!;
            }
            node.Next = next;
            next.Prev = node;
            this.Count -= i;
        }

        internal List<object> ToList()
        {
            var list = new List<object>(this.Count);
            for (var node = this.Head.Next!; node != this.Tail; node = node.Next!)
            {
                list.Add(node.Value!);
            }
            return list;
        }
    }

    private sealed class Rematch(string causeName, int causeIndex, int reach)
    {
        internal string CauseName { get; } = causeName;

        internal int CauseIndex { get; } = causeIndex;

        internal int Reach { get; set; } = reach;
    }

    internal static List<object> Tokenize(string text, Grammar grammar)
    {
        var tokenList = new TokenList();
        tokenList.AddAfter(tokenList.Head, text);
        MatchGrammar(text, tokenList, grammar, tokenList.Head, 0, null);
        return tokenList.ToList();
    }

    private static bool MatchPattern(Regex regex, int pos, string text, bool lookbehind, out int index, out string value)
    {
        var match = regex.Match(text, pos);
        if (!match.Success)
        {
            index = 0;
            value = "";
            return false;
        }
        index = match.Index;
        value = match.Value;
        // Prism emulates lookbehind by dropping the first capturing group from the match.
        if (lookbehind && match.Groups[1].Length > 0)
        {
            var lookbehindLength = match.Groups[1].Length;
            index += lookbehindLength;
            value = value[lookbehindLength..];
        }
        return true;
    }

    private static void MatchGrammar(string text, TokenList tokenList, Grammar grammar, Node startNode, int startPos, Rematch? rematch)
    {
        foreach (var token in grammar.Tokens)
        {
            var patterns = token.Patterns;
            for (var j = 0; j < patterns.Length; j++)
            {
                if (rematch is not null && rematch.CauseName == token.Name && rematch.CauseIndex == j)
                {
                    return;
                }

                var pattern = patterns[j];
                var regex = pattern.Regex;

                var currentNode = startNode.Next!;
                var pos = startPos;
                for (; currentNode != tokenList.Tail; pos += currentNode.Length, currentNode = currentNode.Next!)
                {
                    if (rematch is not null && pos >= rematch.Reach)
                    {
                        break;
                    }

                    if (tokenList.Count > text.Length)
                    {
                        // Something went terribly wrong. Prism gives up here too.
                        return;
                    }

                    if (currentNode.Value is not string str)
                    {
                        continue;
                    }

                    var removeCount = 1;
                    int from;
                    string matchStr;

                    if (pattern.Greedy)
                    {
                        if (!MatchPattern(regex, pos, text, pattern.Lookbehind, out from, out matchStr) || from >= text.Length)
                        {
                            break;
                        }

                        var to = from + matchStr.Length;
                        var p = pos;

                        p += currentNode.Length;
                        while (from >= p)
                        {
                            currentNode = currentNode.Next!;
                            p += currentNode.Length;
                        }
                        p -= currentNode.Length;
                        pos = p;

                        // The current node is a token, so the match would overlap an existing token.
                        if (currentNode.Value is Token)
                        {
                            continue;
                        }

                        for (var k = currentNode; k != tokenList.Tail && (p < to || k.Value is string); k = k.Next!)
                        {
                            removeCount++;
                            p += k.Length;
                        }
                        removeCount--;

                        str = text[pos..p];
                        from -= pos;
                    }
                    else
                    {
                        if (!MatchPattern(regex, 0, str, pattern.Lookbehind, out from, out matchStr))
                        {
                            continue;
                        }
                    }

                    var before = str[..from];
                    var after = str[(from + matchStr.Length)..];

                    var reach = pos + str.Length;
                    if (rematch is not null && reach > rematch.Reach)
                    {
                        rematch.Reach = reach;
                    }

                    var removeFrom = currentNode.Prev!;

                    if (before.Length > 0)
                    {
                        removeFrom = tokenList.AddAfter(removeFrom, before);
                        pos += before.Length;
                    }

                    tokenList.RemoveRange(removeFrom, removeCount);

                    var content = pattern.Inside is { } inside ? (object)Tokenize(matchStr, inside) : matchStr;
                    var wrapped = new Token(token.Name, content, pattern.Alias, matchStr.Length);
                    currentNode = tokenList.AddAfter(removeFrom, wrapped);

                    if (after.Length > 0)
                    {
                        tokenList.AddAfter(currentNode, after);
                    }

                    if (removeCount > 1)
                    {
                        // At least one token was removed, so the removed text has to be matched again.
                        var nestedRematch = new Rematch(token.Name, j, reach);
                        MatchGrammar(text, tokenList, grammar, currentNode.Prev!, pos, nestedRematch);

                        if (rematch is not null && nestedRematch.Reach > rematch.Reach)
                        {
                            rematch.Reach = nestedRematch.Reach;
                        }
                    }
                }
            }
        }
    }
}

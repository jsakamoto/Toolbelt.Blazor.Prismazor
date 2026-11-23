using System.Text.RegularExpressions;

namespace Toolbelt.Blazor.Prismazor.Hooks;

/// <summary>
/// The functions that some template languages give to <c>buildPlaceholders</c> to leave some of the matches as they are.
/// Each method creates a new function, which keeps its state during one call of the highlighter.
/// </summary>
internal static partial class TemplatingFilters
{
    [GeneratedRegex(@"^\{%-?\s*([a-zA-Z0-9_]+)")]
    private static partial Regex LiquidTagPattern();

    /// <summary>
    /// Creates the filter of Liquid, which does not replace the tags between "raw" and "endraw".
    /// </summary>
    internal static Func<string, bool> Liquid()
    {
        var insideRaw = false;
        return match =>
        {
            var tagMatch = LiquidTagPattern().Match(match);
            if (tagMatch.Success)
            {
                var tag = tagMatch.Groups[1].Value;
                if (tag == "raw" && !insideRaw)
                {
                    insideRaw = true;
                    return true;
                }
                else if (tag == "endraw")
                {
                    insideRaw = false;
                    return true;
                }
            }

            return !insideRaw;
        };
    }

    /// <summary>
    /// Creates the filter of Smarty, which does not replace the tags in a "literal" block.
    /// </summary>
    internal static Func<string, bool> Smarty() => Literal("{literal}", "{/literal}");

    /// <summary>
    /// Creates the filter of Soy, which does not replace the tags in a "literal" block.
    /// </summary>
    internal static Func<string, bool> Soy() => Literal("{literal}", "{/literal}");

    private static Func<string, bool> Literal(string start, string end)
    {
        var literalMode = false;
        return match =>
        {
            if (match == end)
            {
                literalMode = false;
            }

            if (!literalMode)
            {
                if (match == start)
                {
                    literalMode = true;
                }

                return true;
            }

            return false;
        };
    }
}

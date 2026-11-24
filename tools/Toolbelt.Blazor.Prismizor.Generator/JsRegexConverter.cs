using System.Text;
using System.Text.RegularExpressions;

namespace Toolbelt.Blazor.Prismizor.Generator;

/// <summary>
/// Converts a JavaScript regular expression into a .NET one that behaves the same way.
/// </summary>
internal static class JsRegexConverter
{
    // \p{Zl} and \p{Zp} contain only LS and PS.
    private const string LineTerminators = @"\n\r\p{Zl}\p{Zp}";

    /// <summary>
    /// Converts the source and flags of a JavaScript regular expression into a .NET pattern and options.
    /// </summary>
    /// <param name="source">The source of the JavaScript regular expression.</param>
    /// <param name="flags">The flags of the JavaScript regular expression.</param>
    public static (string Pattern, RegexOptions Options) Convert(string source, string flags)
    {
        var options = RegexOptions.None;
        var multiline = false;
        var dotAll = false;
        foreach (var flag in flags)
        {
            switch (flag)
            {
                case 'g': break;
                case 'i': options |= RegexOptions.IgnoreCase | RegexOptions.CultureInvariant; break;
                case 'm': multiline = true; break;
                case 's': dotAll = true; break;
                default: throw new NotSupportedException($"The regex flag '{flag}' is not supported. /{source}/{flags}");
            }
        }

        var result = new StringBuilder(source.Length + 16);
        var inClass = false;
        var classStart = 0;
        var classNegated = false;
        var classComplement = default(char?);
        var lastWasShorthand = false;
        for (var i = 0; i < source.Length; i++)
        {
            var c = source[i];
            var isShorthand = false;

            if (c == '\\')
            {
                if (i + 1 >= source.Length) throw new NotSupportedException($"A trailing backslash is not supported. /{source}/");
                var e = source[++i];
                switch (e)
                {
                    // JavaScript \d and \w match ASCII only, but .NET ones match Unicode too.
                    case 'd':
                        result.Append(inClass ? "0-9" : "[0-9]");
                        isShorthand = true;
                        break;
                    case 'D' when !inClass:
                        result.Append("[^0-9]");
                        break;
                    case 'w':
                        result.Append(inClass ? "a-zA-Z0-9_" : "[a-zA-Z0-9_]");
                        isShorthand = true;
                        break;
                    case 'W' when !inClass:
                        result.Append("[^a-zA-Z0-9_]");
                        break;
                    // A class cannot hold the complement of a set in .NET, so the class is rewritten where it ends.
                    case 'D' or 'W' when classComplement is null:
                        classComplement = e;
                        break;
                    case 'D' or 'W':
                        throw new NotSupportedException($"More than one of \\D and \\W in a character class is not supported. /{source}/");
                    case 's' or 'S':
                        result.Append('\\').Append(e);
                        isShorthand = true;
                        break;
                    case 'k':
                        throw new NotSupportedException($"Named backreferences are not supported. /{source}/");
                    case 'b' or 'B' or 'n' or 'r' or 't' or 'f' or 'v' or 'c' or 'x' or 'u' or (>= '0' and <= '9'):
                        result.Append('\\').Append(e);
                        break;
                    // Without the "u" flag, JavaScript treats an unknown escaped letter as the letter itself.
                    case (>= 'a' and <= 'z') or (>= 'A' and <= 'Z'):
                        result.Append(e);
                        break;
                    default:
                        result.Append('\\').Append(e);
                        break;
                }
            }
            else if (inClass)
            {
                switch (c)
                {
                    case ']':
                        inClass = false;
                        result.Append(c);
                        if (classComplement is { } complement)
                        {
                            RewriteClassWithComplement(result, classStart, classNegated, complement);
                            classComplement = null;
                        }
                        break;
                    // .NET reads "-[" as a class subtraction, so a literal '[' must be escaped.
                    case '[':
                        result.Append(@"\[");
                        break;
                    // A '-' next to a shorthand class is a literal in JavaScript but a range in .NET.
                    case '-' when lastWasShorthand || (i + 2 < source.Length && source[i + 1] == '\\' && "dwsS".Contains(source[i + 2])):
                        result.Append(@"\-");
                        break;
                    default:
                        result.Append(c);
                        break;
                }
            }
            else
            {
                switch (c)
                {
                    case '[':
                        if (source.AsSpan(i).StartsWith("[^]"))
                        {
                            result.Append(@"[\s\S]");
                            i += 2;
                        }
                        else if (source.AsSpan(i).StartsWith("[]"))
                        {
                            result.Append("(?!)");
                            i += 1;
                        }
                        else
                        {
                            inClass = true;
                            classStart = result.Length;
                            classNegated = false;
                            result.Append(c);
                            if (i + 1 < source.Length && source[i + 1] == '^')
                            {
                                result.Append('^');
                                classNegated = true;
                                i++;
                            }
                        }
                        break;
                    case '.':
                        result.Append(dotAll ? @"[\s\S]" : $"[^{LineTerminators}]");
                        break;
                    // JavaScript treats CR, LS and PS as line terminators too, but .NET does not.
                    case '^':
                        result.Append(multiline ? $"(?<![^{LineTerminators}])" : "^");
                        break;
                    // .NET "$" also matches before a final "\n", but JavaScript "$" does not.
                    case '$':
                        result.Append(multiline ? $"(?![^{LineTerminators}])" : @"(?![\s\S])");
                        break;
                    case '(':
                        if (source.AsSpan(i).StartsWith("(?<") && i + 3 < source.Length && source[i + 3] is not ('=' or '!'))
                        {
                            throw new NotSupportedException($"Named groups are not supported. /{source}/");
                        }
                        result.Append(c);
                        break;
                    default:
                        result.Append(c);
                        break;
                }
            }

            lastWasShorthand = isShorthand;
        }

        var pattern = result.ToString();
        // Fail at generation time, not at run time, if the conversion produced an invalid pattern.
        _ = new Regex(pattern, options);
        return (pattern, options);
    }

    // The class that starts at "start" was written without its \D or \W. The members are what is between the brackets.
    //   [x\W]  becomes (?:[x]|[^a-zA-Z0-9_])      [^x\W]  becomes (?:(?![x])[a-zA-Z0-9_])
    private static void RewriteClassWithComplement(StringBuilder result, int start, bool negated, char complement)
    {
        var membersStart = start + (negated ? 2 : 1);
        var members = result.ToString(membersStart, result.Length - 1 - membersStart);
        result.Length = start;

        var set = complement == 'W' ? "a-zA-Z0-9_" : "0-9";
        if (negated)
        {
            result.Append(members.Length == 0 ? $"[{set}]" : $"(?:(?![{members}])[{set}])");
        }
        else
        {
            result.Append(members.Length == 0 ? $"[^{set}]" : $"(?:[{members}]|[^{set}])");
        }
    }
}

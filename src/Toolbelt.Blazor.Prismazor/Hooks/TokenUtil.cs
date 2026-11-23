using System.Text;

namespace Toolbelt.Blazor.Prismazor.Hooks;

/// <summary>
/// The helpers for the hooks, which read tokens as the JavaScript code of the hooks does.
/// </summary>
internal static class TokenUtil
{
    /// <summary>
    /// Returns the child of the given content at the index, or null if the content is not a list or has no such child.
    /// </summary>
    internal static object? ChildAt(object content, int index) => content is List<object> list && index >= 0 && index < list.Count ? list[index] : null;

    /// <summary>
    /// Returns the last child of the given content, or null if the content is not a list or is empty.
    /// </summary>
    internal static object? LastChild(object content) => content is List<object> { Count: > 0 } list ? list[^1] : null;

    /// <summary>
    /// Returns the text of the given token, which is the concatenation of the strings in it.
    /// </summary>
    internal static string Stringify(object? token)
    {
        switch (token)
        {
            case string text:
                return text;
            case Token { Content: string text }:
                return text;
            case Token { Content: List<object> children }:
                var builder = new StringBuilder();
                foreach (var child in children)
                {
                    builder.Append(Stringify(child));
                }
                return builder.ToString();
            default:
                return "";
        }
    }

    /// <summary>
    /// Returns whether the given token is a string or a token of the plain-text type.
    /// </summary>
    internal static bool IsStringOrPlainText(object token) => token is string or Token { Type: "plain-text" };
}

namespace Toolbelt.Blazor.Prismizor;

internal sealed class Token(string type, object content, string[]? alias, int length)
{
    internal string Type { get; set; } = type;

    /// <summary>
    /// Gets the content of this token, which is a <see cref="string"/> or a <see cref="List{T}"/> of strings and tokens.
    /// </summary>
    internal object Content { get; set; } = content;

    internal string[]? Alias { get; set; } = alias;

    internal int Length { get; } = length;
}

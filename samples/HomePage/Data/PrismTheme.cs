namespace HomePage.Data;

/// <summary>
/// One of the theme CSS files that the Prismizor package includes.
/// </summary>
public sealed record PrismTheme(string Id, string DisplayName)
{
    public const string Default = "prism-tomorrow";

    public static IReadOnlyList<PrismTheme> All { get; } =
    [
        new("prism", "Default"),
        new("prism-coy", "Coy"),
        new("prism-dark", "Dark"),
        new("prism-funky", "Funky"),
        new("prism-okaidia", "Okaidia"),
        new("prism-solarizedlight", "Solarized Light"),
        new("prism-tomorrow", "Tomorrow Night"),
        new("prism-twilight", "Twilight"),
    ];

    public string Href => $"_content/Toolbelt.Blazor.Prismizor/themes/{this.Id}.min.css";
}

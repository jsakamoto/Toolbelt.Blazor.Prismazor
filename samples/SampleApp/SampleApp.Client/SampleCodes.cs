using Toolbelt.Blazor.Prismazor;

namespace SampleApp.Client;

/// <summary>
/// Provides sample code for each supported language.
/// </summary>
public static class SampleCodes
{
    /// <summary>
    /// Gets the samples as pairs of a title, a grammar, and code.
    /// </summary>
    public static IReadOnlyList<(string Title, Grammar Grammar, string Code)> All { get; } =
    [
        ("C#", PrismazorLanguages.CSharp,
            """
            public record Person(string Name, int Age)
            {
                // Returns a greeting.
                public string Greet() => $"Hello, {this.Name}! You are {this.Age} years old.";
            }
            """),
        ("HTML", PrismazorLanguages.Markup,
            """
            <!DOCTYPE html>
            <p class="note">Tom &amp; Jerry</p>
            <script>console.log(`sum: ${1 + 2}`);</script>
            <style>p.note { color: #c00; }</style>
            """),
        ("CSS", PrismazorLanguages.Css,
            """
            @media (prefers-color-scheme: dark) {
              :root { --bg: #1e1e1e; }
            }
            .card:hover::after { content: "\2192"; margin: 0 auto !important; }
            """),
        ("JavaScript", PrismazorLanguages.JavaScript,
            """
            const re = /^[a-z]+\d{2,}$/i;
            export async function load(url) {
              const res = await fetch(url);
              return res.ok ? res.json() : null; // null on error
            }
            """),
    ];
}

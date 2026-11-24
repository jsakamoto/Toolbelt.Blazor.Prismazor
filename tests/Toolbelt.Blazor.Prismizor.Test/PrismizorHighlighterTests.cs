using System.Reflection;
using Toolbelt;

namespace Toolbelt.Blazor.Prismizor.Test;

public class PrismizorHighlighterTests
{
    // Every language of the library, found by reflection so that the tests cover the languages that are added later.
    internal static readonly Dictionary<string, Grammar> GrammarsById = typeof(PrismizorLanguages)
        .GetProperties(BindingFlags.Public | BindingFlags.Static)
        .Where(property => property.PropertyType == typeof(Grammar))
        .Select(property => (Grammar)property.GetValue(null)!)
        .ToDictionary(grammar => grammar.Id!);

    private static IEnumerable<TestCaseData> Fixtures()
    {
        var solutionDir = FileIO.FindContainerDirToAncestor("*.slnx");
        var fixturesDir = Path.Combine(solutionDir, "tests", "Toolbelt.Blazor.Prismizor.Test", "Fixtures");
        return Directory.GetFiles(fixturesDir, "*.txt", SearchOption.AllDirectories)
            .Order()
            .Select(path => new TestCaseData(path).SetArgDisplayNames($"{Path.GetFileName(Path.GetDirectoryName(path))}/{Path.GetFileName(path)}"));
    }

    [TearDown]
    public void TearDown()
    {
        PrismizorLanguageRegistry.Clear();
    }

    [TestCaseSource(nameof(Fixtures))]
    public void Highlight_MatchesPrismJs(string inputPath)
    {
        // ReadAllText keeps the line endings, which matters for the CRLF fixture.
        var code = File.ReadAllText(inputPath);
        var expected = File.ReadAllText(Path.ChangeExtension(inputPath, ".expected.html"));
        var languageId = Path.GetFileName(Path.GetDirectoryName(inputPath))!;
        var grammar = GrammarsById[languageId];

        // PrismJS highlights the code blocks of Markdown with the languages that are loaded, and the fixtures load all.
        if (languageId == "markdown")
        {
            foreach (var each in GrammarsById.Values) PrismizorLanguageRegistry.Register(each);
        }

        var html = PrismizorHighlighter.Highlight(code, grammar);

        html.Is(expected);
    }

    [Test]
    public void Highlight_EmptyCode()
    {
        PrismizorHighlighter.Highlight("", PrismizorLanguages.CSharp).Is("");
    }

    [Test]
    public void Highlight_Plain_OnlyEncodesTheCode()
    {
        // The same as "plain" of prism-core.js, which has no tokens.
        PrismizorHighlighter.Highlight("<b>int x = 1; // & \"str\"</b>", PrismizorLanguages.Plain)
            .Is("&lt;b>int x = 1; // &amp; \"str\"&lt;/b>");
        PrismizorLanguages.Plain.Id.Is("plain");
    }

    [Test]
    public void Highlight_EncodesPlainText()
    {
        PrismizorHighlighter.Highlight("a < b & c > d", PrismizorLanguages.Css).Is("a &lt; b &amp; c > d");
    }

    [Test]
    public void Highlight_Markdown_LeavesCodeBlockPlain_WhenTheLanguageIsNotRegistered()
    {
        var html = PrismizorHighlighter.Highlight("```csharp\nvar x = 1;\n```\n", PrismizorLanguages.Markdown);

        html.Is(
            "<span class=\"token code\"><span class=\"token punctuation\">```</span><span class=\"token code-language\">csharp</span>\n" +
            "<span class=\"token code-block language-csharp\">var x = 1;</span>\n" +
            "<span class=\"token punctuation\">```</span></span>\n");
    }

    [Test]
    public void Highlight_Markdown_HighlightsCodeBlock_WhenTheLanguageIsRegistered()
    {
        PrismizorLanguageRegistry.Register(PrismizorLanguages.CSharp);

        var html = PrismizorHighlighter.Highlight("```csharp\nvar x = 1;\n```\n", PrismizorLanguages.Markdown);

        html.Contains("<span class=\"token keyword\">var</span>").IsTrue();
    }

    [Test]
    public void Highlight_Markdown_FindsRegisteredLanguageByAlias()
    {
        PrismizorLanguageRegistry.Register(PrismizorLanguages.CSharp, "cs");

        var html = PrismizorHighlighter.Highlight("```cs\nvar x = 1;\n```\n", PrismizorLanguages.Markdown);

        html.Contains("<span class=\"token keyword\">var</span>").IsTrue();
    }

    [Test]
    public void Highlight_Liquid_DoesNotKeepTheRawStateOfThePreviousCall()
    {
        const string code = "{{ a }} {% endraw %}{{ b }}";
        var first = PrismizorHighlighter.Highlight(code, PrismizorLanguages.Liquid);

        // An unclosed raw tag leaves the state inside raw mode. The next call has to start outside of it.
        PrismizorHighlighter.Highlight("{% raw %}{{ a }}", PrismizorLanguages.Liquid);
        var second = PrismizorHighlighter.Highlight(code, PrismizorLanguages.Liquid);

        second.Is(first);
    }
}

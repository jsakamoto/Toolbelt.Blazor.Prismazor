using System.Reflection;
using Toolbelt;

namespace Toolbelt.Blazor.Prismazor.Test;

public class PrismazorHighlighterTests
{
    // Every language of the library, found by reflection so that the tests cover the languages that are added later.
    internal static readonly Dictionary<string, Grammar> GrammarsById = typeof(PrismazorLanguages)
        .GetProperties(BindingFlags.Public | BindingFlags.Static)
        .Where(property => property.PropertyType == typeof(Grammar))
        .Select(property => (Grammar)property.GetValue(null)!)
        .ToDictionary(grammar => grammar.Id!);

    private static IEnumerable<TestCaseData> Fixtures()
    {
        var solutionDir = FileIO.FindContainerDirToAncestor("*.slnx");
        var fixturesDir = Path.Combine(solutionDir, "tests", "Toolbelt.Blazor.Prismazor.Test", "Fixtures");
        return Directory.GetFiles(fixturesDir, "*.txt", SearchOption.AllDirectories)
            .Order()
            .Select(path => new TestCaseData(path).SetArgDisplayNames($"{Path.GetFileName(Path.GetDirectoryName(path))}/{Path.GetFileName(path)}"));
    }

    [TearDown]
    public void TearDown()
    {
        PrismazorLanguageRegistry.Clear();
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
            foreach (var each in GrammarsById.Values) PrismazorLanguageRegistry.Register(each);
        }

        var html = PrismazorHighlighter.Highlight(code, grammar);

        html.Is(expected);
    }

    [Test]
    public void Highlight_EmptyCode()
    {
        PrismazorHighlighter.Highlight("", PrismazorLanguages.CSharp).Is("");
    }

    [Test]
    public void Highlight_EncodesPlainText()
    {
        PrismazorHighlighter.Highlight("a < b & c > d", PrismazorLanguages.Css).Is("a &lt; b &amp; c > d");
    }

    [Test]
    public void Highlight_Markdown_LeavesCodeBlockPlain_WhenTheLanguageIsNotRegistered()
    {
        var html = PrismazorHighlighter.Highlight("```csharp\nvar x = 1;\n```\n", PrismazorLanguages.Markdown);

        html.Is(
            "<span class=\"token code\"><span class=\"token punctuation\">```</span><span class=\"token code-language\">csharp</span>\n" +
            "<span class=\"token code-block language-csharp\">var x = 1;</span>\n" +
            "<span class=\"token punctuation\">```</span></span>\n");
    }

    [Test]
    public void Highlight_Markdown_HighlightsCodeBlock_WhenTheLanguageIsRegistered()
    {
        PrismazorLanguageRegistry.Register(PrismazorLanguages.CSharp);

        var html = PrismazorHighlighter.Highlight("```csharp\nvar x = 1;\n```\n", PrismazorLanguages.Markdown);

        html.Contains("<span class=\"token keyword\">var</span>").IsTrue();
    }

    [Test]
    public void Highlight_Markdown_FindsRegisteredLanguageByAlias()
    {
        PrismazorLanguageRegistry.Register(PrismazorLanguages.CSharp, "cs");

        var html = PrismazorHighlighter.Highlight("```cs\nvar x = 1;\n```\n", PrismazorLanguages.Markdown);

        html.Contains("<span class=\"token keyword\">var</span>").IsTrue();
    }

    [Test]
    public void Highlight_Liquid_DoesNotKeepTheRawStateOfThePreviousCall()
    {
        const string code = "{{ a }} {% endraw %}{{ b }}";
        var first = PrismazorHighlighter.Highlight(code, PrismazorLanguages.Liquid);

        // An unclosed raw tag leaves the state inside raw mode. The next call has to start outside of it.
        PrismazorHighlighter.Highlight("{% raw %}{{ a }}", PrismazorLanguages.Liquid);
        var second = PrismazorHighlighter.Highlight(code, PrismazorLanguages.Liquid);

        second.Is(first);
    }
}

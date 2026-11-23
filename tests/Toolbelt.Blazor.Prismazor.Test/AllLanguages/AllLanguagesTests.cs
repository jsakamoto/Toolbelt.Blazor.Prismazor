using System.Text.Json;
using Toolbelt;

namespace Toolbelt.Blazor.Prismazor.Test;

/// <summary>
/// Highlights one sample with every language and compares the result with PrismJS, which finds the languages
/// that the conversion of the grammars or the hooks get wrong. The sample is not meant to be meaningful code.
/// </summary>
public class AllLanguagesTests
{
    private static string AllLanguagesDir => Path.Combine(FileIO.FindContainerDirToAncestor("*.slnx"), "tests", "Toolbelt.Blazor.Prismazor.Test", "AllLanguages");

    private static IEnumerable<TestCaseData> Languages()
    {
        var expected = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(Path.Combine(AllLanguagesDir, "expected.json")))!;
        return expected.Select(pair => new TestCaseData(pair.Key, pair.Value).SetArgDisplayNames(pair.Key));
    }

    [TearDown]
    public void TearDown()
    {
        PrismazorLanguageRegistry.Clear();
    }

    [TestCaseSource(nameof(Languages))]
    public void Highlight_MatchesPrismJs(string languageId, string expected)
    {
        var sample = File.ReadAllText(Path.Combine(AllLanguagesDir, "sample.txt"));
        foreach (var grammar in PrismazorHighlighterTests.GrammarsById.Values) PrismazorLanguageRegistry.Register(grammar);

        var html = PrismazorHighlighter.Highlight(sample, PrismazorHighlighterTests.GrammarsById[languageId]);

        html.Is(expected);
    }

    [Test]
    public void EveryLanguageHasAnExpectedOutput()
    {
        var expected = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(Path.Combine(AllLanguagesDir, "expected.json")))!;

        PrismazorHighlighterTests.GrammarsById.Keys.Order().Is(expected.Keys.Order());
    }
}

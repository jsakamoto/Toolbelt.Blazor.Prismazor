using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace Toolbelt.Blazor.Prismizor.Test;

public class CodeBlockTests
{
    private static async Task<string> RenderAsync(Dictionary<string, object?> parameters)
    {
        await using var services = new ServiceCollection().BuildServiceProvider();
        await using var renderer = new HtmlRenderer(services, NullLoggerFactory.Instance);
        return await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var output = await renderer.RenderComponentAsync<CodeBlock>(ParameterView.FromDictionary(parameters));
            return output.ToHtmlString();
        });
    }

    [Test]
    public async Task Render_OutputsHighlightedHtml()
    {
        // HtmlRenderer is what static SSR uses, so this shows that the first response is already highlighted.
        var html = await RenderAsync(new()
        {
            [nameof(CodeBlock.Code)] = "var x = 1;",
            [nameof(CodeBlock.Language)] = PrismizorLanguages.CSharp,
        });

        html.Is(
            "<pre tabindex=\"0\" class=\"language-csharp\"><code class=\"language-csharp\">" +
            "<span class=\"token class-name\"><span class=\"token keyword\">var</span></span> x " +
            "<span class=\"token operator\">=</span> <span class=\"token number\">1</span>" +
            "<span class=\"token punctuation\">;</span></code></pre>");
    }

    [Test]
    public async Task Render_MergesClassAndAdditionalAttributes()
    {
        var html = await RenderAsync(new()
        {
            [nameof(CodeBlock.Code)] = "a{}",
            [nameof(CodeBlock.Language)] = PrismizorLanguages.Css,
            ["class"] = "line-numbers",
            ["id"] = "sample",
            ["tabindex"] = "-1",
        });

        html.Is(
            "<pre id=\"sample\" tabindex=\"-1\" class=\"language-css line-numbers\"><code class=\"language-css\">" +
            "<span class=\"token selector\">a</span><span class=\"token punctuation\">{</span>" +
            "<span class=\"token punctuation\">}</span></code></pre>");
    }
}

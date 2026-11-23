# Prismazor - A Port of Prism.js to Blazor [![NuGet Package](https://img.shields.io/nuget/v/Toolbelt.Blazor.Prismazor.svg)](https://www.nuget.org/packages/Toolbelt.Blazor.Prismazor/) [![unit tests](https://github.com/jsakamoto/Toolbelt.Blazor.Prismazor/actions/workflows/unit-tests.yml/badge.svg)](https://github.com/jsakamoto/Toolbelt.Blazor.Prismazor/actions/workflows/unit-tests.yml) [![Discord](https://img.shields.io/discord/798312431893348414?style=flat&logo=discord&logoColor=white&label=Blazor%20Community&labelColor=5865f2&color=gray)](https://discord.com/channels/798312431893348414/1202165955900473375)

## Summary

A syntax highlighting component for Blazor. It is a C# port of [Prism.js](https://prismjs.com/), so it has no JavaScript at all.

```razor
<CodeBlock Code="@code" Language="@PrismazorLanguages.CSharp" />
```

- It works with Blazor Server, Blazor WebAssembly, and static SSR. The HTML is highlighted when the component is rendered, so even the first response from the server is already highlighted.
- It makes the same HTML as Prism.js 1.30.0. You can use the theme CSS files of Prism.js as they are.
- It supports all 288 languages of Prism.js, including the languages that embed other languages, such as PHP, Markdown, and JSX.
- It is small for Blazor WebAssembly. The languages that your app doesn't use are removed when you publish. For example, an app that uses only C# adds about 9 KB (Brotli compressed).

> **Note**  
> This library uses `new Regex(...)` with patterns that are given at run time. If your app already uses it, the library doesn't make your app bigger because of the regular expression engine.

## Supported Blazor versions

This library supports ASP.NET Core Blazor version 10.0 and 11.0.

## How to install and use?

1. Add this package to your project like this.

```shell
dotnet add package Toolbelt.Blazor.Prismazor
```

2. Open the `Toolbelt.Blazor.Prismazor` namespace in the `_Imports.razor` file.

```razor
@* This is "_Imports.razor" *@
...
@using Toolbelt.Blazor.Prismazor
```

3. Add a link to a theme CSS file. The theme files of Prism.js are included in this package.

```html
<!-- This is "App.razor" or "index.html" -->
<link rel="stylesheet" href="_content/Toolbelt.Blazor.Prismazor/themes/prism.min.css" />
```

These themes are available. `prism`, `prism-coy`, `prism-dark`, `prism-funky`, `prism-okaidia`, `prism-solarizedlight`, `prism-tomorrow`, and `prism-twilight`. Each file name ends with `.min.css`.

To switch the theme by the color scheme of the browser, use the `media` attribute.

```html
<link rel="stylesheet" href="_content/Toolbelt.Blazor.Prismazor/themes/prism.min.css" media="(prefers-color-scheme: light)" />
<link rel="stylesheet" href="_content/Toolbelt.Blazor.Prismazor/themes/prism-tomorrow.min.css" media="(prefers-color-scheme: dark)" />
```

4. Use the `CodeBlock` component, and give it the code and the language.

```razor
@code {
    private const string code = """
        var x = 1;
        Console.WriteLine(x);
        """;
}

<CodeBlock Code="@code" Language="@PrismazorLanguages.CSharp" />
```

The component makes a `<pre>` element that has a `<code>` element in it. Both have the `language-csharp` CSS class. If you add other attributes to the component, such as `class` or `id`, they go to the `<pre>` element.

## Languages

`PrismazorLanguages` has a property for each language, such as `CSharp`, `JavaScript`, `Markup` (HTML and XML), `Css`, and `Python`. The name is the Prism.js language ID in Pascal case. The ID is also in the `Id` property of the grammar. Prism.js aliases, such as `html`, `js`, and `md`, are not properties.

> **Warning**  
> Please don't make a dictionary that finds a language by its name in your app. It keeps every language that is in the dictionary, so the trimming can't remove them.

## Use the highlighter without the component

`PrismazorHighlighter.Highlight()` method returns the highlighted HTML as a string. It does the same thing as `Prism.highlight()` of Prism.js.

```csharp
var html = PrismazorHighlighter.Highlight("var x = 1;", PrismazorLanguages.CSharp);
```

## Code blocks in Markdown

Prism.js highlights the code blocks in Markdown with the languages that are loaded. This library does the same with the languages that you register. A code block of a language that is not registered stays plain text.

```csharp
// Program.cs
PrismazorLanguageRegistry.Register(PrismazorLanguages.CSharp, "cs");
PrismazorLanguageRegistry.Register(PrismazorLanguages.JavaScript, "js");
```

## Differences from Prism.js

- Prism.js changes the grammar of a language depending on the languages that are loaded. This library follows the state of `loadLanguages()` of Prism.js, which loads all languages. The plugins that change another language, such as `js-extras` and `jsdoc`, are not applied to the language they change.
- The hooks of a language work only when you highlight that language.
- The word boundary `\b` and the space `\s` in the patterns follow .NET, so they may not work the same way as Prism.js for non-ASCII characters.

For the details, see [the design document](https://github.com/jsakamoto/Toolbelt.Blazor.Prismazor/blob/main/docs/design.md).

## Release Notes

[Release notes](https://raw.githubusercontent.com/jsakamoto/Toolbelt.Blazor.Prismazor/main/RELEASE-NOTES.txt)

## License & Third Party Notices

- [MIT License](https://github.com/jsakamoto/Toolbelt.Blazor.Prismazor/blob/main/LICENSE)
- [Third party notices](https://github.com/jsakamoto/Toolbelt.Blazor.Prismazor/blob/main/src/Toolbelt.Blazor.Prismazor/THIRD-PARTY-NOTICES-PRISM.txt)

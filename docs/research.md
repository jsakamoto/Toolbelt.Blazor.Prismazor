# Survey of existing libraries (2026-10-18)

## Conclusion

We found no free library that is well maintained and meets all three conditions. The conditions are native Blazor, small size in WASM, and highlighted HTML output in static SSR.

## Candidates that use no JavaScript (C# only)

| Library | SSR | Languages | Drawbacks |
|---|---|---|---|
| [ColorCode.HTML](https://www.nuget.org/packages/ColorCode.HTML) (CommunityToolkit) | Yes. It returns an HTML string synchronously, so you can output it as `MarkupString`. | Many, such as C#, HTML, CSS, JS, TS, SQL, XML, and PowerShell | It is not a Blazor component. The last update was 2023-07 and it has stopped since then. It can output CSS classes with `HtmlClassFormatter`. MIT. |
| [Blism](https://github.com/ricardoboss/Blism) | Yes. It tokenizes synchronously in `OnParametersSet`. | Only C#, Dart, YAML, Bash, and PHP | It supports only net10.0. It outputs an inline `style` for each token. The lexical analysis is simple (for example, it does not support escapes in C# strings). 2 stars. MIT. |
| [S97SP.Prism.Blazor](https://www.nuget.org/packages/S97SP.Prism.Blazor) | Partly. It uses `Task.Run` in `OnParametersSetAsync`. Static SSR waits for it, but with `[StreamRendering]` the text "Processing code..." appears first. | C#, CSS, JS, TS, JSX/TSX, Razor, SQL, and XML | It has Prism in its name, but it is not a faithful port. 1 star. The repository has no LICENSE file. |

## Excluded libraries

- JavaScript wrappers (Brism, HighlightBlazor, and PSC.Blazor.Components.CodeSnippet) highlight in the browser, so the SSR output is not highlighted.
- TextMateSharp depends on Onigwrap (native Oniguruma), so it probably cannot run in WASM (not verified).
- ColorfulCode is a native wrapper of syntect, which is written in Rust. It cannot run in WASM.
- BlazorPrettyCode stopped in 2020 (.NET 5).

## About the size increase from Regex

If the app already uses Regex, the size does not grow by the Regex engine in most cases. The trimming works by each member, so the size can grow in these cases.

- The app uses only `[GeneratedRegex]`. Then the runtime parser and the interpreter are newly included.
- The library uses a feature that the app does not use, such as `RegexOptions.NonBacktracking`.

To check this, run `dotnet publish -c Release` before and after you add the library, and compare the size of `System.Text.RegularExpressions.wasm`.

## Details of Blism

- About themes, `ITheme` has only `DefaultDarkTheme`. The colors are of the `System.Drawing.Color` type, so CSS variables cannot be used, and it is hard to switch light and dark by CSS only. The workarounds are to choose the theme on the server, to write your own `ITokenStyleRenderer`, or to render twice and hide one of them by CSS. None of them fits our design.
- The size (v2.0.0, net10.0, before trimming) is as follows.

| Package | DLL | gzip -9 |
|---|---|---|
| Blism.Core | 12,288 B | 5,525 B |
| Blism.Blazor | 10,752 B | 4,778 B |
| Blism.Theme.DefaultDark | 6,144 B | 2,542 B |
| Blism.Language.CSharp.Core / .Blazor | 12,288 / 7,680 B | 5,339 / 3,229 B |
| .Core / .Blazor of the other languages (Php, Bash, Yaml, Dart) | 9,216 to 10,240 / 7,680 B | 3,668 to 4,423 / about 3,220 B |

- `System.Drawing.Primitives` may be included too.

## Comparison with PrismJS (prismjs@1.30.0, minified, gzip -9)

| Setup | Prism | Blism |
|---|---|---|
| Minimum setup with only C# | About 6.6 KB (core 3,165 + clike 464 + csharp 2,459 + theme CSS 690) | About 20.9 KB |
| Each added language | 0.8 to 3 KB | 6.9 to 8.6 KB |

- The difference is about ten and several KB. It is small compared with the whole Blazor WASM app, whose runtime alone is several MB.
- Prism has detailed grammars (about 300 languages), and a theme is only a CSS file to swap. But it uses JavaScript, so the SSR output is not highlighted.
- The only advantage of Blism over Prism is that it can output highlighted HTML in SSR.

# Development guide

## Folder structure

| Path | Content |
|---|---|
| `src/Toolbelt.Blazor.Prismazor/` | The library itself (a Razor class library) |
| `src/Toolbelt.Blazor.Prismazor/Languages/*.g.cs` | The language definitions that the generator makes. Do not edit them by hand. |
| `src/Toolbelt.Blazor.Prismazor/Hooks/` | The hooks of the languages. They are ports of the hooks that the components of PrismJS add. |
| `src/Toolbelt.Blazor.Prismazor/wwwroot/themes/` | The theme CSS files of PrismJS (copied by `npm run themes`) |
| `tools/prism-extract/` | Node scripts. They extract the grammars from PrismJS, make the expected values for the tests, and copy the themes. |
| `tools/prism-extract/prism-env.mjs` | The common module that makes an isolated PrismJS environment |
| `tools/prism-extract/languages.json` | The list of the languages to generate (the Prism language ID and the C# class name). It is made by `npm run languages`. |
| `tools/prism-extract/out/grammars.json` | The extracted grammars (committed) |
| `tools/Toolbelt.Blazor.Prismazor.Generator/` | The console app that makes C# from grammars.json. It also converts JS regular expressions to .NET. |
| `tests/Toolbelt.Blazor.Prismazor.Test/` | NUnit tests (MTP) |
| `tests/Toolbelt.Blazor.Prismazor.Test/Fixtures/<language ID>/` | The input `*.txt` and the PrismJS output `*.expected.html` |
| `tests/Toolbelt.Blazor.Prismazor.Test/AllLanguages/` | The output of PrismJS (`expected.json`) when the same input (`sample.txt`) is given to all languages |
| `samples/SampleApp/` | A Blazor Web App (Server and Client) to check the behavior |
| `samples/HomePage/` | The home page of the library, a Blazor WebAssembly standalone app for GitHub Pages (net11.0). The `gh-pages.yml` workflow publishes it. |
| `.assets/nupkg-icon.png` | The package icon. The home page shows it too. |

## Add a language, or update PrismJS

To update PrismJS, first change the version in `tools/prism-extract/package.json`, and run `npm install`.

1. Run `npm run languages` in `tools/prism-extract` to make `languages.json` again. If there is a language that the generator cannot convert, you can remove it from the list by writing it with the reason in `unsupported` of `make-language-list.mjs`.
2. Run `npm run extract` in `tools/prism-extract`.
3. Run `dotnet run --project tools/Toolbelt.Blazor.Prismazor.Generator` in the root of the repository.
   - The generator stops with `NotSupportedException` if there is a regular expression that cannot be converted (for example, the `u` or `y` flag, or a named group).
   - It also stops if the component of a language adds a before-tokenize or after-tokenize hook and the port of it does not exist. Write the port in `Hooks/`, and add the language ID to the `PortedHooks` table of the generator.
4. Put a sample `*.txt` in `tests/Toolbelt.Blazor.Prismazor.Test/Fixtures/<language ID>/`, and run `npm run fixtures` in `tools/prism-extract` to make the expected values. To make the expected values for all languages again, run `npm run all-languages-fixture`.
5. Run the tests.

## Tests

```
cd tests/Toolbelt.Blazor.Prismazor.Test
dotnet test
```

- The `global.json` in the root puts the tests in MTP mode. The test project runs on both net10.0 and net11.0.
- The fixtures are compared with the line endings as they are, so `.gitattributes` sets them to `-text`.
- The expected values are made by PrismJS in the same environment that the grammars are extracted from (`prism-env.mjs`). See [design.md](design.md) for the idea of the state of the grammars.
- The `AllLanguages` test gives the same input to all languages and compares the result with PrismJS. Its purpose is to find mistakes in the grammar conversion and the hook ports in a wide range, so the input has no meaning.

## Sample app

```
cd samples/SampleApp/SampleApp
dotnet run
```

- The sample app targets net11.0.
- `/` is static SSR, `/server` is Interactive Server, and `/wasm` is Interactive WebAssembly.
- The theme switches between `prism.min.css` and `prism-tomorrow.min.css` by `prefers-color-scheme`.

## Home page

```
cd samples/HomePage
dotnet run
```

- It is a Blazor WebAssembly standalone app, and the GitHub Pages workflow publishes it with `-p:GHPages=true`.
- BlazorWasmPreRendering.Build prerenders it into static HTML at publish time, with no loading screen. The controls (the language list, the theme radio buttons, the Copy buttons, and the color mode switch) are disabled until the first `OnAfterRender`, so they look disabled in the prerendered HTML. See `InteractiveComponentBase`.
- The services are registered in the `ConfigureServices` local function of `Program.cs`, because the prerendering calls it by that name.
- The color mode (auto, light, or dark) is kept in the local storage. The page uses only the built-in functions of the browser for it, so there is no JavaScript file. The only script is a short one in `index.html`, which applies the saved mode before the first paint.
- The theme CSS file of the code blocks is loaded by the layout, and the playground changes it. So all the code blocks on the page follow the selected theme.
- The fonts are the same as the ones of the ASP.NET Core home page (Open Sans and Space Grotesk), and they are hosted in `wwwroot/fonts` under the SIL Open Font License.

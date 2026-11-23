# Toolbelt.Blazor.Prismazor

This project re-implements Prism.js as a native Blazor syntax highlighting component named `Toolbelt.Blazor.Prismazor`. It is written in C# only and uses no JavaScript.

## Requirements

- It must work with Blazor Server, Blazor WebAssembly, and Blazor static SSR.
- The highlighted HTML must be made when the server renders the component. It must not depend on any JavaScript that runs later in the browser.
- The assembly size must be as small as possible for Blazor WebAssembly.
- It assumes that the app already uses `System.Text.RegularExpressions` (`new Regex` with a pattern given at run time).

## Documents

- [docs/research.md](docs/research.md) has the survey of the existing libraries, and the reason why we decided to port Prism.
- [docs/design.md](docs/design.md) has the design policy, the key points of the implementation, and the known differences from PrismJS.
- [docs/development.md](docs/development.md) has the folder structure, how to regenerate the grammars, and how to run the tests and the sample app.

## Decisions

- The target frameworks are net10.0 and net11.0 (multi-targeting, for the library and the tests).
- The grammars are extracted from PrismJS into JSON with Node, and a C# generator makes the .cs files from the JSON. The generated files are committed.
- The project name and the root namespace are `Toolbelt.Blazor.Prismazor`.
- Follow the behavior of Prism.js. When in doubt, match what Prism.js 1.30.0 really does.
- The state of the grammars is the same as `loadLanguages()` of Prism.js (all languages loaded). The extensions that rewrite the grammar of another language (such as js-extras and jsdoc) are not applied to the language they rewrite. See [docs/design.md](docs/design.md) for the details.
- The hooks of the languages are ported into `Hooks/`. The generator stops if a language needs a hook that is not ported. We never generate such a language without the hook.

## Status (2026-11-23)

- All 288 languages of PrismJS 1.30.0 are generated. The hooks are ported too (13 markup-templating languages, jsx / tsx, xquery, graphql, naniscript, and markdown).
- All 326 tests pass on both net10.0 and net11.0. The 24 fixture folders, and the comparison that gives the same input to all 288 languages, match the output of PrismJS byte by byte.
- The trimming is verified. When you publish in Release (WebAssembly), the languages that are not used are removed from `Toolbelt.Blazor.Prismazor.wasm`. An app that uses only C# gets 79,637 B, and 8,987 B after Brotli compression.
- The dll of the library in Release is 1.59 MB, and the nupkg is 330 KB. On Blazor Server (published in Release, measured once on a local machine), the memory right after startup is 68 MB and the first page takes 69 ms, which is almost the same as the 4-language version. Even if all languages are used once, it takes 305 ms and the memory grows by about 50 MB.
- The repository is on GitHub (private).

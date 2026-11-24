using System.Text.Json;
using Toolbelt.Blazor.Prismizor.Generator;

// Usage: dotnet run --project tools/Toolbelt.Blazor.Prismizor.Generator
// Reads tools/prism-extract/out/grammars.json and writes C# files into src/Toolbelt.Blazor.Prismizor/Languages.

var repoRoot = FindRepositoryRoot(AppContext.BaseDirectory) ?? FindRepositoryRoot(Environment.CurrentDirectory)
    ?? throw new InvalidOperationException("Could not find the folder that contains the *.slnx file.");
var inputPath = Path.Combine(repoRoot, "tools", "prism-extract", "out", "grammars.json");
var outputDir = Path.Combine(repoRoot, "src", "Toolbelt.Blazor.Prismizor", "Languages");

var grammarsFile = JsonSerializer.Deserialize<GrammarsFile>(File.ReadAllText(inputPath), new JsonSerializerOptions(JsonSerializerDefaults.Web))
    ?? throw new InvalidOperationException($"Could not read {inputPath}.");

// A language whose own component registers a before-tokenize or after-tokenize hook is wrong without it, so stop here.
var emitter = new CodeEmitter(grammarsFile);
var languageIds = grammarsFile.Languages.Select(l => l.Id).ToHashSet();
var withoutHooks = grammarsFile.HookSourceIds
    .Where(languageIds.Contains)
    .Where(id => !emitter.HasHooks(grammarsFile.Languages.Single(l => l.Id == id)))
    .ToArray();
if (withoutHooks.Length > 0)
{
    throw new InvalidOperationException($"The hooks of these languages are not ported: {string.Join(", ", withoutHooks)}.");
}

Directory.CreateDirectory(outputDir);
foreach (var oldFile in Directory.GetFiles(outputDir, "*.g.cs"))
{
    File.Delete(oldFile);
}

foreach (var language in grammarsFile.Languages)
{
    File.WriteAllText(Path.Combine(outputDir, $"{language.ClassName}Language.g.cs"), emitter.EmitLanguage(language));
}
File.WriteAllText(Path.Combine(outputDir, "PrismizorLanguages.g.cs"), emitter.EmitPrismizorLanguages());

Console.WriteLine($"Generated {grammarsFile.Languages.Length} languages into {outputDir}");

static string? FindRepositoryRoot(string startDir)
{
    for (var dir = new DirectoryInfo(startDir); dir is not null; dir = dir.Parent)
    {
        if (dir.GetFiles("*.slnx").Length > 0) return dir.FullName;
    }
    return null;
}

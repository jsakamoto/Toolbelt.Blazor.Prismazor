using HomePage;
using HomePage.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Toolbelt.Blazor.Prismizor;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

ConfigureServices(builder.Services);

await builder.Build().RunAsync();

// BlazorWasmPreRendering.Build calls this function by its name when it prerenders the app at publish time.
static void ConfigureServices(IServiceCollection services)
{
    services.AddScoped<ColorModeService>();
    services.AddScoped<ThemeState>();

    // The code blocks in Markdown are highlighted only with the languages that are registered here.
    PrismizorLanguageRegistry.Register(PrismizorLanguages.CSharp, "cs");
    PrismizorLanguageRegistry.Register(PrismizorLanguages.JavaScript, "js");
    PrismizorLanguageRegistry.Register(PrismizorLanguages.Css);
    PrismizorLanguageRegistry.Register(PrismizorLanguages.Json);
    PrismizorLanguageRegistry.Register(PrismizorLanguages.Bash, "sh");
}

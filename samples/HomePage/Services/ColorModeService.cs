using Microsoft.JSInterop;

namespace HomePage.Services;

/// <summary>
/// The color mode of the page. "Auto" follows the setting of the system.
/// </summary>
public enum ColorMode
{
    Auto,
    Light,
    Dark,
}

/// <summary>
/// Keeps the color mode of the page in the local storage, and applies it to the root element of the page.
/// It uses only the built-in functions of the browser, so there is no JavaScript file for it.
/// </summary>
public sealed class ColorModeService(IJSRuntime js)
{
    // The script in index.html reads the same key to avoid a flash of the wrong color.
    private const string StorageKey = "prismizor-color-mode";

    public ColorMode Mode { get; private set; } = ColorMode.Auto;

    public event Action? Changed;

    public async Task InitializeAsync()
    {
        try
        {
            var saved = await js.InvokeAsync<string?>("localStorage.getItem", StorageKey);
            this.Mode = Enum.TryParse<ColorMode>(saved, ignoreCase: true, out var mode) ? mode : ColorMode.Auto;
        }
        catch (JSException)
        {
            this.Mode = ColorMode.Auto;
        }

        await this.ApplyAsync();
        this.Changed?.Invoke();
    }

    public async Task SetModeAsync(ColorMode mode)
    {
        this.Mode = mode;
        try
        {
            await js.InvokeVoidAsync("localStorage.setItem", StorageKey, mode.ToString().ToLowerInvariant());
        }
        catch (JSException)
        {
            // The storage may be blocked. The mode still works until the page is closed.
        }

        await this.ApplyAsync();
        this.Changed?.Invoke();
    }

    private async Task ApplyAsync()
    {
        try
        {
            if (this.Mode == ColorMode.Auto)
            {
                await js.InvokeVoidAsync("document.documentElement.removeAttribute", "data-theme");
            }
            else
            {
                await js.InvokeVoidAsync("document.documentElement.setAttribute", "data-theme", this.Mode.ToString().ToLowerInvariant());
            }
        }
        catch (JSException)
        {
            // The page keeps the previous color mode.
        }
    }
}

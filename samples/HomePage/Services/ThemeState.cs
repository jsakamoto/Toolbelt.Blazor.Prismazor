using HomePage.Data;

namespace HomePage.Services;

/// <summary>
/// The theme of the code blocks on the page. The layout loads its CSS file, and the playground changes it.
/// </summary>
public sealed class ThemeState
{
    public PrismTheme Theme { get; private set; } = PrismTheme.All.Single(theme => theme.Id == PrismTheme.Default);

    public event Action? Changed;

    public void Select(PrismTheme theme)
    {
        if (theme == this.Theme) return;
        this.Theme = theme;
        this.Changed?.Invoke();
    }
}

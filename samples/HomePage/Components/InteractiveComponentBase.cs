using Microsoft.AspNetCore.Components;

namespace HomePage.Components;

/// <summary>
/// A component that has controls the user can operate. The controls are rendered as disabled until the component
/// becomes interactive, which is when <see cref="OnAfterRender(bool)"/> is called for the first time.
/// The prerendered HTML never reaches that point, so it shows the controls as disabled.
/// </summary>
public abstract class InteractiveComponentBase : ComponentBase
{
    /// <summary>
    /// Gets whether the component is running in the browser and can respond to the user.
    /// </summary>
    protected bool IsInteractive { get; private set; }

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            this.IsInteractive = true;
            this.StateHasChanged();
        }
    }
}

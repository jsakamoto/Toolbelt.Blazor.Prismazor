namespace Toolbelt.Blazor.Prismazor.Hooks;

/// <summary>
/// The hooks of a language, which correspond to the hooks that the component of the language adds to PrismJS.
/// </summary>
internal abstract class GrammarHooks
{
    /// <summary>
    /// Called before the code is tokenized. It may change the code and the grammar.
    /// </summary>
    internal virtual void BeforeTokenize(HookEnvironment env)
    {
    }

    /// <summary>
    /// Called after the code is tokenized. It may change the tokens.
    /// </summary>
    internal virtual void AfterTokenize(HookEnvironment env)
    {
    }

    /// <summary>
    /// Called for each token before it is turned into HTML. It does nothing unless <see cref="HasWrap"/> is true.
    /// </summary>
    internal virtual void Wrap(WrapEnvironment env)
    {
    }

    /// <summary>
    /// Gets whether <see cref="Wrap"/> does something, which lets the highlighter skip building the environment if not.
    /// </summary>
    internal virtual bool HasWrap => false;
}

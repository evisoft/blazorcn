namespace BlazorCN;

/// <summary>
/// Coordinates image load state between <c>AvatarImageCn</c> and <c>AvatarFallbackCn</c>
/// (cascaded from <c>AvatarCn</c> with IsFixed=true; the event keeps re-renders scoped
/// to the fallback instead of the whole avatar subtree). Mirrors Radix Avatar semantics:
/// the fallback unmounts once the image loads, and the image unmounts if it fails.
/// </summary>
internal sealed class AvatarLoadState
{
    public bool ImageLoaded { get; private set; }
    public bool ImageFailed { get; private set; }

    public event Action? Changed;

    public void SetLoaded()
    {
        ImageLoaded = true;
        ImageFailed = false;
        Changed?.Invoke();
    }

    public void SetFailed()
    {
        ImageFailed = true;
        ImageLoaded = false;
        Changed?.Invoke();
    }
}

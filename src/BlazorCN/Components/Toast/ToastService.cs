namespace BlazorCN;

/// <summary>
/// Service for showing toast notifications. Register as scoped via AddBlazorCN().
/// </summary>
public class ToastService
{
    /// <summary>
    /// Raised when a new toast should be shown.
    /// </summary>
    public event Action<ToastMessage>? OnShow;

    /// <summary>
    /// Shows a toast with the given message and variant.
    /// </summary>
    public void Show(string message, ToastVariant variant = ToastVariant.Default, string? title = null, int durationMs = 5000)
    {
        var toast = new ToastMessage(
            Id: Guid.NewGuid().ToString("N"),
            Message: message,
            Variant: variant,
            Title: title,
            DurationMs: durationMs
        );
        OnShow?.Invoke(toast);
    }

    /// <summary>
    /// Shows a success toast.
    /// </summary>
    public void Success(string message, string? title = null) => Show(message, ToastVariant.Success, title);

    /// <summary>
    /// Shows an error toast.
    /// </summary>
    public void Error(string message, string? title = null) => Show(message, ToastVariant.Error, title);

    /// <summary>
    /// Shows a warning toast.
    /// </summary>
    public void Warning(string message, string? title = null) => Show(message, ToastVariant.Warning, title);

    /// <summary>
    /// Shows an info toast.
    /// </summary>
    public void Info(string message, string? title = null) => Show(message, ToastVariant.Info, title);

    /// <summary>
    /// Shows a loading toast with a spinner icon.
    /// </summary>
    public void Loading(string message, string? title = null) => Show(message, ToastVariant.Loading, title);
}

namespace BlazorCN;

/// <summary>
/// Represents a toast notification message.
/// </summary>
public record ToastMessage(
    string Id,
    string Message,
    ToastVariant Variant,
    string? Title = null,
    int DurationMs = 5000
);

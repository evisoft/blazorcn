namespace BlazorCN;

/// <summary>
/// Cascaded by <see cref="FormFieldCn"/> so form children can be associated with the
/// actual control: <see cref="FormLabelCn"/> defaults its <c>for</c> to <see cref="FormItemId"/>,
/// <see cref="FormDescriptionCn"/>/<see cref="FormMessageCn"/> emit <see cref="DescriptionId"/>/
/// <see cref="MessageId"/>, and <see cref="FormControlCn"/> exposes the context to its child
/// content so consumers can bind <c>id</c>, <c>aria-describedby</c>, and <c>aria-invalid</c>
/// onto the control itself (Blazor has no Slot to do it automatically).
/// </summary>
public sealed class FormFieldContext
{
    /// <summary>Id for the form control; the default <c>for</c> of <see cref="FormLabelCn"/>.</summary>
    public string FormItemId { get; init; } = "";

    /// <summary>Id emitted on <see cref="FormDescriptionCn"/>.</summary>
    public string DescriptionId { get; init; } = "";

    /// <summary>Id emitted on <see cref="FormMessageCn"/>.</summary>
    public string MessageId { get; init; } = "";

    /// <summary>Whether the field is currently in an error state.</summary>
    public bool HasError { get; init; }

    /// <summary>
    /// Ready-made <c>aria-describedby</c> value: the description id, plus the message id
    /// while the field is in error (mirrors the reference FormControl).
    /// </summary>
    public string AriaDescribedBy => HasError ? $"{DescriptionId} {MessageId}" : DescriptionId;
}

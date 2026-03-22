using Microsoft.AspNetCore.Components;

namespace BlazorCN;

/// <summary>
/// Base class for all BlazorCN components. Provides Class, Style, and AdditionalAttributes parameters.
/// </summary>
public abstract class ComponentBaseCn : ComponentBase
{
    /// <summary>
    /// Additional CSS class names to apply to the component's root element.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Inline styles to apply to the component's root element.
    /// </summary>
    [Parameter]
    public string? Style { get; set; }

    /// <summary>
    /// Captures any additional attributes not explicitly declared as parameters.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object?>? AdditionalAttributes { get; set; }
}

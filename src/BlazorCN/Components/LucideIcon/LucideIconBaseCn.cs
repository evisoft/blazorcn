using Microsoft.AspNetCore.Components;

namespace BlazorCN;

/// <summary>
/// Base class for all Lucide icon components. Provides shared parameters for size, fill, stroke, and stroke width.
/// </summary>
public abstract class LucideIconBaseCn : ComponentBaseCn
{
    /// <summary>
    /// The width and height of the icon in pixels. Defaults to 24.
    /// </summary>
    [Parameter] public int Size { get; set; } = 24;

    /// <summary>
    /// The SVG fill color. Defaults to "none".
    /// </summary>
    [Parameter] public string Fill { get; set; } = "none";

    /// <summary>
    /// The SVG stroke color. Defaults to "currentColor" to inherit from the parent's text color.
    /// </summary>
    [Parameter] public string Stroke { get; set; } = "currentColor";

    /// <summary>
    /// The SVG stroke width. Defaults to 2.
    /// </summary>
    [Parameter] public int StrokeWidth { get; set; } = 2;
}

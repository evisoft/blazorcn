namespace BlazorCN;

/// <summary>
/// Side on which the floating element is positioned relative to the reference.
/// </summary>
public enum FloatingSide { Top, Right, Bottom, Left }

/// <summary>
/// Alignment of the floating element along the reference's edge.
/// </summary>
public enum FloatingAlign { Start, Center, End }

/// <summary>
/// Options for positioning a floating element relative to a reference element.
/// </summary>
public sealed class FloatingOptions
{
    /// <summary>Which side of the reference element to place the floating element on.</summary>
    public FloatingSide Side { get; set; } = FloatingSide.Bottom;

    /// <summary>Offset (in pixels) away from the reference element along the side axis.</summary>
    public int SideOffset { get; set; } = 4;

    /// <summary>Alignment along the reference element's edge.</summary>
    public FloatingAlign Align { get; set; } = FloatingAlign.Center;

    /// <summary>Offset (in pixels) along the alignment axis.</summary>
    public int AlignOffset { get; set; } = 0;
}

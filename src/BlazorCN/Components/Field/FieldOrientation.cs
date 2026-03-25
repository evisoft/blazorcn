namespace BlazorCN;

/// <summary>
/// Orientation for Field layout (vertical, horizontal, or responsive).
/// </summary>
public enum FieldOrientation
{
    /// <summary>Vertical layout (default). Children stack top-to-bottom.</summary>
    Vertical,
    /// <summary>Horizontal layout. Children arranged side-by-side.</summary>
    Horizontal,
    /// <summary>Responsive layout. Vertical on small screens, horizontal on medium+.</summary>
    Responsive
}

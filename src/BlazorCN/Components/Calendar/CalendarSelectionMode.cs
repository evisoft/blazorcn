namespace BlazorCN;

/// <summary>
/// Selection mode for the CalendarCn component.
/// </summary>
public enum CalendarSelectionMode
{
    /// <summary>Select a single date.</summary>
    Single,

    /// <summary>Select multiple individual dates.</summary>
    Multiple,

    /// <summary>Select a contiguous date range (start and end).</summary>
    Range
}

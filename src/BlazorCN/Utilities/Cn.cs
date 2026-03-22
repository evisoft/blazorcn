using System.Text.RegularExpressions;

namespace BlazorCN;

/// <summary>
/// Utility for merging Tailwind CSS class names. Conflicting utilities are resolved by last-wins.
/// Port of tailwind-merge + clsx.
/// </summary>
public static partial class Cn
{
    /// <summary>
    /// Merges multiple CSS class strings. Conflicting Tailwind utilities are resolved by last-wins.
    /// </summary>
    public static string Merge(params string?[] classes)
    {
        var allClasses = new List<string>();
        foreach (var cls in classes)
        {
            if (string.IsNullOrWhiteSpace(cls)) continue;
            allClasses.AddRange(cls.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        }

        if (allClasses.Count == 0) return string.Empty;

        // Process in reverse: first occurrence (from end) wins for each group
        var seen = new Dictionary<string, int>();
        var result = new List<(string className, int originalIndex)>();

        for (var i = allClasses.Count - 1; i >= 0; i--)
        {
            var className = allClasses[i];
            var group = GetUtilityGroup(className);

            var key = group ?? className;
            if (seen.ContainsKey(key)) continue;
            seen[key] = i;

            result.Add((className, i));
        }

        result.Reverse();
        return string.Join(' ', result.Select(r => r.className));
    }

    private static string? GetUtilityGroup(string className)
    {
        var prefixEnd = className.LastIndexOf(':');
        var prefix = prefixEnd >= 0 ? className[..(prefixEnd + 1)] : "";
        var baseClass = prefixEnd >= 0 ? className[(prefixEnd + 1)..] : className;

        var utilityKey = GetBaseUtilityKey(baseClass);
        if (utilityKey == null) return null;

        return prefix + utilityKey;
    }

    private static string? GetBaseUtilityKey(string baseClass)
    {
        var arbitraryMatch = ArbitraryRegex().Match(baseClass);
        if (arbitraryMatch.Success)
            return arbitraryMatch.Groups[1].Value;

        var spacingMatch = SpacingRegex().Match(baseClass);
        if (spacingMatch.Success) return spacingMatch.Groups[1].Value;

        var sizingMatch = SizingRegex().Match(baseClass);
        if (sizingMatch.Success) return sizingMatch.Groups[1].Value;

        if (TextSizeRegex().IsMatch(baseClass))
            return "text-size";
        if (FontWeightRegex().IsMatch(baseClass))
            return "font-weight";
        if (LeadingRegex().IsMatch(baseClass))
            return "leading";
        if (TrackingRegex().IsMatch(baseClass))
            return "tracking";

        if (BgColorRegex().IsMatch(baseClass))
            return "bg-color";
        if (TextColorRegex().IsMatch(baseClass))
            return "text-color";
        if (BorderColorRegex().IsMatch(baseClass))
            return "border-color";

        if (RoundedRegex().IsMatch(baseClass))
            return "rounded";
        if (BorderWidthRegex().IsMatch(baseClass))
            return "border-width";

        if (DisplayRegex().IsMatch(baseClass))
            return "display";
        if (PositionRegex().IsMatch(baseClass))
            return "position";

        if (JustifyRegex().IsMatch(baseClass))
            return "justify";
        if (ItemsRegex().IsMatch(baseClass))
            return "items";
        if (FlexRegex().IsMatch(baseClass))
            return "flex";
        if (GridColsRegex().IsMatch(baseClass))
            return "grid-cols";

        if (OpacityRegex().IsMatch(baseClass))
            return "opacity";
        if (ShadowRegex().IsMatch(baseClass))
            return "shadow";
        if (ZIndexRegex().IsMatch(baseClass))
            return "z-index";
        if (OverflowRegex().IsMatch(baseClass))
            return "overflow";
        if (CursorRegex().IsMatch(baseClass))
            return "cursor";

        return null;
    }

    [GeneratedRegex(@"^(.+?)-\[.+\]$")]
    private static partial Regex ArbitraryRegex();

    [GeneratedRegex(@"^(-?(?:p|px|py|pt|pr|pb|pl|ps|pe|m|mx|my|mt|mr|mb|ml|ms|me|gap|gap-x|gap-y|space-x|space-y|inset|inset-x|inset-y|top|right|bottom|left|start|end))-.+$")]
    private static partial Regex SpacingRegex();

    [GeneratedRegex(@"^(w|h|min-w|min-h|max-w|max-h|size)-.+$")]
    private static partial Regex SizingRegex();

    [GeneratedRegex(@"^text-(xs|sm|base|lg|xl|2xl|3xl|4xl|5xl|6xl|7xl|8xl|9xl)$")]
    private static partial Regex TextSizeRegex();

    [GeneratedRegex(@"^font-(thin|extralight|light|normal|medium|semibold|bold|extrabold|black)$")]
    private static partial Regex FontWeightRegex();

    [GeneratedRegex(@"^leading-.+$")]
    private static partial Regex LeadingRegex();

    [GeneratedRegex(@"^tracking-.+$")]
    private static partial Regex TrackingRegex();

    [GeneratedRegex(@"^bg-(transparent|current|inherit|white|black|slate|gray|zinc|neutral|stone|red|orange|amber|yellow|lime|green|emerald|teal|cyan|sky|blue|indigo|violet|purple|fuchsia|pink|rose|primary|secondary|destructive|muted|accent|popover|card|background|foreground|input|ring|border|chart|sidebar|surface|code|selection)")]
    private static partial Regex BgColorRegex();

    [GeneratedRegex(@"^text-(transparent|current|inherit|white|black|slate|gray|zinc|neutral|stone|red|orange|amber|yellow|lime|green|emerald|teal|cyan|sky|blue|indigo|violet|purple|fuchsia|pink|rose|primary|secondary|destructive|muted|accent|popover|card|background|foreground|input|ring|border|chart|sidebar|surface|code|selection)")]
    private static partial Regex TextColorRegex();

    [GeneratedRegex(@"^border-(transparent|current|inherit|white|black|slate|gray|zinc|neutral|stone|red|orange|amber|yellow|lime|green|emerald|teal|cyan|sky|blue|indigo|violet|purple|fuchsia|pink|rose|primary|secondary|destructive|muted|accent|popover|card|background|foreground|input|ring|border)")]
    private static partial Regex BorderColorRegex();

    [GeneratedRegex(@"^rounded(-|$)")]
    private static partial Regex RoundedRegex();

    [GeneratedRegex(@"^border(-[0-9]|$)")]
    private static partial Regex BorderWidthRegex();

    [GeneratedRegex(@"^(block|inline-block|inline|flex|inline-flex|table|inline-table|table-caption|table-cell|table-column|table-column-group|table-footer-group|table-header-group|table-row-group|table-row|flow-root|grid|inline-grid|contents|list-item|hidden)$")]
    private static partial Regex DisplayRegex();

    [GeneratedRegex(@"^(static|fixed|absolute|relative|sticky)$")]
    private static partial Regex PositionRegex();

    [GeneratedRegex(@"^justify-.+$")]
    private static partial Regex JustifyRegex();

    [GeneratedRegex(@"^items-.+$")]
    private static partial Regex ItemsRegex();

    [GeneratedRegex(@"^flex-.+$")]
    private static partial Regex FlexRegex();

    [GeneratedRegex(@"^grid-cols-.+$")]
    private static partial Regex GridColsRegex();

    [GeneratedRegex(@"^opacity-.+$")]
    private static partial Regex OpacityRegex();

    [GeneratedRegex(@"^shadow(-|$)")]
    private static partial Regex ShadowRegex();

    [GeneratedRegex(@"^z-.+$")]
    private static partial Regex ZIndexRegex();

    [GeneratedRegex(@"^overflow-.+$")]
    private static partial Regex OverflowRegex();

    [GeneratedRegex(@"^cursor-.+$")]
    private static partial Regex CursorRegex();
}

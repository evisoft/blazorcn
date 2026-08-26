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

        // Process in reverse: first occurrence (from end) wins for each group. A kept class also
        // blocks the groups it shorthands over (ConflictMap), scoped to the same variants and
        // important flag — mirroring tailwind-merge's conflictingClassGroups.
        var seen = new HashSet<string>();
        var result = new List<string>();

        for (var i = allClasses.Count - 1; i >= 0; i--)
        {
            var className = allClasses[i];
            var (scope, group) = Classify(className);

            if (group is null)
            {
                // Unknown utility: dedupe by exact text only.
                if (seen.Add("raw|" + className))
                    result.Add(className);
                continue;
            }

            if (!seen.Add(scope + group)) continue;
            if (ConflictMap.TryGetValue(group, out var evicted))
                foreach (var e in evicted)
                    seen.Add(scope + e);

            result.Add(className);
        }

        result.Reverse();
        return string.Join(' ', result);
    }

    /// <summary>
    /// Splits variants (bracket-aware, sorted so hover:focus == focus:hover), extracts the
    /// important flag into the scope, and resolves the base utility to its conflict group.
    /// Returns (scope, group); group is null for classes we cannot classify.
    /// </summary>
    private static (string Scope, string? Group) Classify(string className)
    {
        var baseStart = 0;
        List<string>? variants = null;
        var depth = 0;
        for (var i = 0; i < className.Length; i++)
        {
            var c = className[i];
            if (c is '[' or '(') depth++;
            else if (c is ']' or ')') depth--;
            else if (c == ':' && depth == 0)
            {
                (variants ??= new List<string>()).Add(className[baseStart..i]);
                baseStart = i + 1;
            }
        }

        var baseClass = className[baseStart..];
        var important = false;
        // v3 prefix form (!p-2, also legal before variants: !hover:p-2) and v4 suffix form (p-2!).
        if (baseClass.StartsWith('!') || className.StartsWith('!'))
        {
            important = true;
            baseClass = baseClass.TrimStart('!');
            if (variants is { Count: > 0 } && variants[0].StartsWith('!'))
                variants[0] = variants[0][1..];
        }
        if (baseClass.EndsWith('!'))
        {
            important = true;
            baseClass = baseClass[..^1];
        }

        // Negative and positive forms of one utility share a group (upstream: -mt-4 evicts mt-2).
        if (baseClass.StartsWith('-'))
            baseClass = baseClass[1..];

        string scope;
        if (variants is { Count: > 0 })
        {
            variants.Sort(StringComparer.Ordinal);
            scope = string.Join(':', variants) + (important ? ":!|" : ":|");
        }
        else
        {
            scope = important ? "!|" : "|";
        }

        return (scope, ResolveGroup(baseClass));
    }

    private static string? ResolveGroup(string baseClass)
    {
        // Bare arbitrary property: [margin:2px] — group by the property name.
        var arbProp = ArbitraryPropertyRegex().Match(baseClass);
        if (arbProp.Success)
            return "arb:" + arbProp.Groups[1].Value;

        // Arbitrary value: resolve through the prefix so p-[3px] conflicts p-2, and disambiguate
        // the prefixes whose arbitrary value can mean different properties (bg/text/border).
        var arb = ArbitraryValueRegex().Match(baseClass);
        if (arb.Success)
            return ResolveArbitrary(arb.Groups[1].Value, arb.Groups[2].Value);

        // Font size, including the line-height postfix (text-lg/7 is still the font-size group).
        if (TextSizeRegex().IsMatch(baseClass))
            return "text-size";
        if (FontWeightRegex().IsMatch(baseClass))
            return "font-weight";

        // Color groups (checked before width/prefix tables so border-t-red-500 is a color).
        var color = ColorRegex().Match(baseClass);
        if (color.Success)
            return color.Groups[1].Value + "-color";

        if (BorderStyleRegex().IsMatch(baseClass))
            return "border-style";
        var borderWidth = BorderWidthRegex().Match(baseClass);
        if (borderWidth.Success)
            return borderWidth.Groups[1].Success ? "border-w-" + borderWidth.Groups[1].Value : "border-w";

        var ring = RingRegex().Match(baseClass);
        if (ring.Success) return ring.Groups[1].Value switch
        {
            "ring-inset" => "ring-inset",
            "ring-offset" => "ring-offset-w",
            _ => "ring-w",
        };
        var outline = OutlineRegex().Match(baseClass);
        if (outline.Success) return outline.Groups[1].Value switch
        {
            "outline-offset" => "outline-offset",
            _ when outline.Groups[2].Success => "outline-w",
            _ => "outline-style",
        };

        if (DisplayRegex().IsMatch(baseClass))
            return "display";
        if (PositionRegex().IsMatch(baseClass))
            return "position";
        if (FlexDirectionRegex().IsMatch(baseClass))
            return "flex-direction";
        if (FlexWrapRegex().IsMatch(baseClass))
            return "flex-wrap";
        if (FlexRegex().IsMatch(baseClass))
            return "flex";
        if (TransitionRegex().IsMatch(baseClass))
            return "transition";
        if (ShadowRegex().IsMatch(baseClass))
            return "shadow";
        if (OverflowRegex().IsMatch(baseClass))
            return "overflow";

        // Longest dash-delimited prefix lookup: "gap-x-2" resolves to gap-x, not gap;
        // "rounded-tl-sm" to rounded-tl; "min-w-4" to min-w.
        var candidate = baseClass;
        while (true)
        {
            if (PrefixGroups.TryGetValue(candidate, out var group))
                return group;
            var dash = candidate.LastIndexOf('-');
            if (dash <= 0) return null;
            candidate = candidate[..dash];
        }
    }

    private static string? ResolveArbitrary(string prefix, string value)
    {
        var colorLike = value.StartsWith('#')
            || value.StartsWith("rgb", StringComparison.Ordinal)
            || value.StartsWith("hsl", StringComparison.Ordinal)
            || value.StartsWith("oklch", StringComparison.Ordinal)
            || value.StartsWith("color:", StringComparison.Ordinal)
            || value.StartsWith("var(--color", StringComparison.Ordinal);

        switch (prefix)
        {
            case "bg":
                if (colorLike) return "bg-color";
                if (value.StartsWith("url(", StringComparison.Ordinal)
                    || value.Contains("gradient(", StringComparison.Ordinal)
                    || value.StartsWith("image:", StringComparison.Ordinal))
                    return "bg-image";
                return null;
            case "text":
                if (colorLike) return "text-color";
                if (value.Length > 0 && (char.IsAsciiDigit(value[0]) || value.StartsWith("length:", StringComparison.Ordinal)))
                    return "text-size";
                return null;
            case "border":
                if (colorLike) return "border-color";
                if (value.Length > 0 && char.IsAsciiDigit(value[0]))
                    return "border-w";
                return null;
            case "ring":
                return colorLike ? "ring-color" : "ring-w";
            case "outline":
                return colorLike ? "outline-color" : "outline-w";
            case "shadow":
                return colorLike ? "shadow-color" : "shadow";
            case "font":
                return value.Length > 0 && char.IsAsciiDigit(value[0]) ? "font-weight" : null;
            default:
                // Unambiguous prefixes (p-[3px], w-[10%], gap-x-[2px], rounded-tl-[4px], z-[99]…)
                // join their named group; unknown prefixes stay dedupe-only.
                return PrefixGroups.TryGetValue(prefix, out var group) ? group : null;
        }
    }

    /// <summary>
    /// tailwind-merge's conflictingClassGroups: keeping the key group also evicts EARLIER classes
    /// in the listed groups (a later shorthand replaces its longhands; never the reverse).
    /// </summary>
    private static readonly Dictionary<string, string[]> ConflictMap = new()
    {
        ["p"] = ["px", "py", "pt", "pr", "pb", "pl", "ps", "pe"],
        ["px"] = ["pr", "pl"],
        ["py"] = ["pt", "pb"],
        ["m"] = ["mx", "my", "mt", "mr", "mb", "ml", "ms", "me"],
        ["mx"] = ["mr", "ml"],
        ["my"] = ["mt", "mb"],
        ["size"] = ["w", "h"],
        ["inset"] = ["inset-x", "inset-y", "top", "right", "bottom", "left", "start", "end"],
        ["inset-x"] = ["right", "left"],
        ["inset-y"] = ["top", "bottom"],
        ["gap"] = ["gap-x", "gap-y"],
        ["rounded"] = ["rounded-s", "rounded-e", "rounded-t", "rounded-r", "rounded-b", "rounded-l",
            "rounded-ss", "rounded-se", "rounded-ee", "rounded-es", "rounded-tl", "rounded-tr", "rounded-br", "rounded-bl"],
        ["rounded-s"] = ["rounded-ss", "rounded-es"],
        ["rounded-e"] = ["rounded-se", "rounded-ee"],
        ["rounded-t"] = ["rounded-tl", "rounded-tr"],
        ["rounded-r"] = ["rounded-tr", "rounded-br"],
        ["rounded-b"] = ["rounded-br", "rounded-bl"],
        ["rounded-l"] = ["rounded-tl", "rounded-bl"],
        ["border-w"] = ["border-w-t", "border-w-r", "border-w-b", "border-w-l", "border-w-s", "border-w-e", "border-w-x", "border-w-y"],
        ["border-w-x"] = ["border-w-r", "border-w-l"],
        ["border-w-y"] = ["border-w-t", "border-w-b"],
        ["text-size"] = ["leading"],
        ["overflow"] = ["overflow-x", "overflow-y"],
    };

    /// <summary>Utility prefix -> conflict group for prefixes whose value never changes the property.</summary>
    private static readonly Dictionary<string, string> PrefixGroups = new()
    {
        ["p"] = "p", ["px"] = "px", ["py"] = "py", ["pt"] = "pt", ["pr"] = "pr", ["pb"] = "pb", ["pl"] = "pl", ["ps"] = "ps", ["pe"] = "pe",
        ["m"] = "m", ["mx"] = "mx", ["my"] = "my", ["mt"] = "mt", ["mr"] = "mr", ["mb"] = "mb", ["ml"] = "ml", ["ms"] = "ms", ["me"] = "me",
        ["gap"] = "gap", ["gap-x"] = "gap-x", ["gap-y"] = "gap-y",
        ["space-x"] = "space-x", ["space-y"] = "space-y",
        ["inset"] = "inset", ["inset-x"] = "inset-x", ["inset-y"] = "inset-y",
        ["top"] = "top", ["right"] = "right", ["bottom"] = "bottom", ["left"] = "left", ["start"] = "start", ["end"] = "end",
        ["w"] = "w", ["h"] = "h", ["size"] = "size",
        ["min-w"] = "min-w", ["min-h"] = "min-h", ["max-w"] = "max-w", ["max-h"] = "max-h",
        ["leading"] = "leading", ["tracking"] = "tracking",
        ["rounded"] = "rounded", ["rounded-s"] = "rounded-s", ["rounded-e"] = "rounded-e",
        ["rounded-t"] = "rounded-t", ["rounded-r"] = "rounded-r", ["rounded-b"] = "rounded-b", ["rounded-l"] = "rounded-l",
        ["rounded-ss"] = "rounded-ss", ["rounded-se"] = "rounded-se", ["rounded-ee"] = "rounded-ee", ["rounded-es"] = "rounded-es",
        ["rounded-tl"] = "rounded-tl", ["rounded-tr"] = "rounded-tr", ["rounded-br"] = "rounded-br", ["rounded-bl"] = "rounded-bl",
        ["justify"] = "justify", ["items"] = "items", ["grid-cols"] = "grid-cols",
        ["opacity"] = "opacity", ["z"] = "z-index", ["cursor"] = "cursor",
        ["overflow-x"] = "overflow-x", ["overflow-y"] = "overflow-y",
        ["duration"] = "duration", ["delay"] = "delay", ["ease"] = "ease", ["animate"] = "animate",
        ["outline-offset"] = "outline-offset",
    };

    // [margin:2px] — arbitrary property.
    [GeneratedRegex(@"^\[([a-z-]+):.+\]$")]
    private static partial Regex ArbitraryPropertyRegex();

    // px-[10px] — prefix + arbitrary value.
    [GeneratedRegex(@"^(.+?)-\[(.+)\]$")]
    private static partial Regex ArbitraryValueRegex();

    // text-lg and the v4 line-height postfix text-lg/7.
    [GeneratedRegex(@"^text-(xs|sm|base|lg|xl|2xl|3xl|4xl|5xl|6xl|7xl|8xl|9xl)(/.+)?$")]
    private static partial Regex TextSizeRegex();

    [GeneratedRegex(@"^font-(thin|extralight|light|normal|medium|semibold|bold|extrabold|black)$")]
    private static partial Regex FontWeightRegex();

    // One regex for every color-bearing prefix; group 1 becomes "<prefix>-color".
    [GeneratedRegex(@"^(bg|text|border-[trblse]|border-x|border-y|border|ring-offset|ring|outline|decoration|divide|accent|caret|fill|stroke)-(transparent|current|inherit|white|black|slate|gray|zinc|neutral|stone|red|orange|amber|yellow|lime|green|emerald|teal|cyan|sky|blue|indigo|violet|purple|fuchsia|pink|rose|primary|secondary|destructive|muted|accent|popover|card|background|foreground|input|ring|border|chart|sidebar|surface|code|selection)([-/].*)?$")]
    private static partial Regex ColorRegex();

    [GeneratedRegex(@"^border-(solid|dashed|dotted|double|hidden|none)$")]
    private static partial Regex BorderStyleRegex();

    // border, border-2, border-t, border-t-2, border-x-4 … (colors are matched first).
    [GeneratedRegex(@"^border(?:-([trblsexy]))?(?:-[0-9]+(?:\.[0-9]+)?)?$")]
    private static partial Regex BorderWidthRegex();

    [GeneratedRegex(@"^(ring-inset|ring-offset|ring)(-[0-9]+)?$")]
    private static partial Regex RingRegex();

    [GeneratedRegex(@"^(outline-offset|outline)(-[0-9]+)?(-(none|dashed|dotted|double|solid|hidden))?$")]
    private static partial Regex OutlineRegex();

    [GeneratedRegex(@"^(block|inline-block|inline|flex|inline-flex|table|inline-table|table-caption|table-cell|table-column|table-column-group|table-footer-group|table-header-group|table-row-group|table-row|flow-root|grid|inline-grid|contents|list-item|hidden)$")]
    private static partial Regex DisplayRegex();

    [GeneratedRegex(@"^(static|fixed|absolute|relative|sticky)$")]
    private static partial Regex PositionRegex();

    [GeneratedRegex(@"^flex-(row|row-reverse|col|col-reverse)$")]
    private static partial Regex FlexDirectionRegex();

    [GeneratedRegex(@"^flex-(wrap|wrap-reverse|nowrap)$")]
    private static partial Regex FlexWrapRegex();

    [GeneratedRegex(@"^flex-(1|auto|initial|none|\d+)$")]
    private static partial Regex FlexRegex();

    [GeneratedRegex(@"^transition(-(all|colors|opacity|shadow|transform|none))?$")]
    private static partial Regex TransitionRegex();

    // shadow sizes only — shadow-red-500 is caught by ColorRegex? No: shadow is not a color
    // prefix above (shadow colors are rare in this codebase); shadow-* groups as one.
    [GeneratedRegex(@"^shadow(-|$)")]
    private static partial Regex ShadowRegex();

    [GeneratedRegex(@"^overflow-(auto|hidden|clip|visible|scroll)$")]
    private static partial Regex OverflowRegex();
}

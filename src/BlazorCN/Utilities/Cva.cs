namespace BlazorCN;

/// <summary>
/// Class Variance Authority — maps variant/size enum values to Tailwind class strings.
/// Port of the CVA JavaScript library.
/// </summary>
/// <typeparam name="TVariant">Variant enum type</typeparam>
/// <typeparam name="TSize">Size enum type</typeparam>
public sealed class Cva<TVariant, TSize>
    where TVariant : struct, Enum
    where TSize : struct, Enum
{
    private readonly string _base;
    private readonly Dictionary<TVariant, string> _variants;
    private readonly Dictionary<TSize, string> _sizes;

    public Cva(
        string baseClasses,
        Dictionary<TVariant, string> variants,
        Dictionary<TSize, string> sizes)
    {
        _base = baseClasses;
        _variants = variants;
        _sizes = sizes;
    }

    /// <summary>
    /// Resolves the final class string by combining base + variant + size + additional classes.
    /// Uses Cn.Merge() so conflicting utilities are resolved by last-wins.
    /// </summary>
    public string Apply(TVariant variant, TSize size, string? additionalClasses = null)
    {
        _variants.TryGetValue(variant, out var variantClasses);
        _sizes.TryGetValue(size, out var sizeClasses);

        return Cn.Merge(_base, variantClasses, sizeClasses, additionalClasses);
    }
}

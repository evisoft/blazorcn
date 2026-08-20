using System;
using System.Linq;
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

    /// <summary>
    /// Whether the consumer supplied the given attribute, ignoring case.
    /// </summary>
    /// <remarks>
    /// HTML attribute names are case-insensitive, and Blazor's unmatched-attribute capture keeps
    /// whatever casing the consumer typed — so <c>Id="x"</c> and <c>id="x"</c> mean the same thing
    /// to the browser but are different dictionary keys here. A case-sensitive
    /// <c>ContainsKey("id")</c> therefore misses <c>Id="x"</c>, which components rely on to decide
    /// whether the consumer already wired up labelling. Getting that wrong is silent and harmful:
    /// the component adds its own <c>aria-label</c>, which overrides the consumer's
    /// <c>&lt;label for&gt;</c> rather than complementing it.
    /// </remarks>
    protected bool HasAttribute(string name) =>
        AdditionalAttributes is not null
        && AdditionalAttributes.Keys.Any(k => string.Equals(k, name, StringComparison.OrdinalIgnoreCase));
}

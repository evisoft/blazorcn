using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorCN;

/// <summary>JS interop payload for createFloating. Plain class with parameterless ctor +
/// init-only properties so System.Text.Json can serialize it under AOT/trimming —
/// positional records and anonymous types both lose constructor parameter names.</summary>
internal sealed class FloatingJsOptions
{
    [JsonPropertyName("side")] public string Side { get; init; } = "bottom";
    [JsonPropertyName("sideOffset")] public int SideOffset { get; init; }
    [JsonPropertyName("align")] public string Align { get; init; } = "center";
    [JsonPropertyName("alignOffset")] public int AlignOffset { get; init; }
}

/// <summary>JS interop payload for setupKeyboardNavigation. See <see cref="FloatingJsOptions"/> for rationale.</summary>
internal sealed class KeyboardNavJsOptions
{
    [JsonPropertyName("selector")] public string Selector { get; init; } = "[data-menu-item]";
    [JsonPropertyName("orientation")] public string Orientation { get; init; } = "vertical";
}

/// <summary>
/// Typed wrapper for BlazorCN JavaScript interop calls.
/// </summary>
public sealed class JsInteropCn : IAsyncDisposable, IDisposable
{
    private readonly IJSRuntime _js;
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private IJSObjectReference? _module;

    /// <summary>
    /// Creates a new instance of <see cref="JsInteropCn"/>. Typically resolved from DI.
    /// </summary>
    public JsInteropCn(IJSRuntime js)
    {
        _js = js;
    }

    private async ValueTask<IJSObjectReference> GetModuleAsync()
    {
        if (_module is not null) return _module;

        await _initLock.WaitAsync();
        try
        {
            // Double-check after acquiring lock
            return _module ??= await _js.InvokeAsync<IJSObjectReference>(
                "import", "./_content/BlazorCN/blazorcn.js");
        }
        finally
        {
            _initLock.Release();
        }
    }

    /// <summary>
    /// Traps keyboard focus within the given element.
    /// </summary>
    public async ValueTask TrapFocusAsync(ElementReference element, string id)
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("trapFocus", element, id);
    }

    /// <summary>
    /// Registers a handler that fires when a click occurs outside the given element.
    /// </summary>
    public async ValueTask OnOutsideClickAsync<T>(
        ElementReference element, string id,
        DotNetObjectReference<T> dotnetRef, string methodName) where T : class
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("onOutsideClick", element, id, dotnetRef, methodName);
    }

    /// <summary>
    /// Locks page scrolling (e.g., when a modal is open).
    /// </summary>
    public async ValueTask LockScrollAsync(string id)
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("lockScroll", id);
    }

    /// <summary>
    /// Cleans up all JS resources (focus trap, outside-click listener, scroll lock) for the given ID.
    /// </summary>
    public async ValueTask CleanupAsync(string id)
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("cleanup", id);
    }

    /// <summary>
    /// Creates a floating element positioned relative to a reference element.
    /// Returns the actual side used (may differ from requested if flipped).
    /// </summary>
    public async ValueTask<string> CreateFloatingAsync(
        ElementReference reference, ElementReference floating, string id,
        FloatingOptions options)
    {
        var module = await GetModuleAsync();
        var jsOptions = new FloatingJsOptions
        {
            Side = options.Side.ToString().ToLowerInvariant(),
            SideOffset = options.SideOffset,
            Align = options.Align.ToString().ToLowerInvariant(),
            AlignOffset = options.AlignOffset,
        };
        return await module.InvokeAsync<string>(
            "createFloating", reference, floating, id, jsOptions);
    }

    /// <summary>
    /// Re-calculates the position of a floating element.
    /// </summary>
    public async ValueTask UpdateFloatingAsync(string id)
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("updateFloating", id);
    }

    /// <summary>
    /// Destroys a floating element and cleans up event listeners.
    /// </summary>
    public async ValueTask DestroyFloatingAsync(string id)
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("destroyFloating", id);
    }

    /// <summary>
    /// Sets up keyboard navigation for a menu/list container.
    /// Arrow keys navigate between items, Escape invokes .NET callback.
    /// </summary>
    public async ValueTask SetupKeyboardNavigationAsync<T>(
        ElementReference container, string id,
        DotNetObjectReference<T> dotnetRef, string escapeMethodName,
        string itemSelector = "[data-menu-item]",
        string orientation = "vertical") where T : class
    {
        var module = await GetModuleAsync();
        var jsOptions = new KeyboardNavJsOptions
        {
            Selector = itemSelector,
            Orientation = orientation,
        };
        await module.InvokeVoidAsync(
            "setupKeyboardNavigation", container, id, dotnetRef, escapeMethodName, jsOptions);
    }

    /// <summary>
    /// Cleans up keyboard navigation for a given ID.
    /// </summary>
    public async ValueTask CleanupKeyboardNavigationAsync(string id)
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("cleanupKeyboardNavigation", id);
    }

    /// <summary>
    /// Wires a custom scrollbar to a scroll-area root: sizes and positions the thumb
    /// to reflect scroll progress, hides the bar when content fits, and enables drag.
    /// </summary>
    public async ValueTask InitScrollAreaAsync(ElementReference root, string id)
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("initScrollArea", root, id);
    }

    /// <summary>
    /// Tears down a scroll-area's listeners and observers for the given ID.
    /// </summary>
    public async ValueTask DestroyScrollAreaAsync(string id)
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("destroyScrollArea", id);
    }

    /// <summary>
    /// Wires pointer-drag resizing to a resizable-panel-group element.
    /// </summary>
    public async ValueTask InitResizableAsync(ElementReference group, string id)
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("initResizable", group, id);
    }

    /// <summary>
    /// Tears down resizable drag listeners for the given ID.
    /// </summary>
    public async ValueTask DestroyResizableAsync(string id)
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("destroyResizable", id);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _initLock.Dispose();
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_module is not null)
        {
            await _module.DisposeAsync();
        }
        _initLock.Dispose();
    }
}

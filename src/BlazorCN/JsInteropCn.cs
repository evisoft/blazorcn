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
    [JsonPropertyName("flipSideOnRtl")] public bool FlipSideOnRtl { get; init; }
}

/// <summary>JS interop payload for setupKeyboardNavigation. See <see cref="FloatingJsOptions"/> for rationale.</summary>
internal sealed class KeyboardNavJsOptions
{
    [JsonPropertyName("selector")] public string Selector { get; init; } = "[data-menu-item]";
    [JsonPropertyName("orientation")] public string Orientation { get; init; } = "vertical";
    [JsonPropertyName("autoFocus")] public bool AutoFocus { get; init; } = true;
    /// <summary>CSS selector for the element to focus on open (e.g. the selected
    /// option, or a combobox's search input). Falls back to the first enabled item.</summary>
    [JsonPropertyName("initialSelector")] public string? InitialSelector { get; init; }
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
    /// Pass <paramref name="excluded"/> (typically the trigger) to treat it as part of
    /// the dismissable layer: clicking it will NOT fire the callback, letting the
    /// trigger's own click toggle perform the close instead of close-then-reopen.
    /// </summary>
    public async ValueTask OnOutsideClickAsync<T>(
        ElementReference element, string id,
        DotNetObjectReference<T> dotnetRef, string methodName,
        ElementReference? excluded = null) where T : class
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("onOutsideClick", element, id, dotnetRef, methodName, excluded);
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
    /// Force-syncs a DOM input's value property. Blazor only patches <c>value</c> when the
    /// rendered attribute changes, so rejecting input (leaving the bound value unchanged)
    /// produces no diff and the rejected characters would stay visible in the DOM.
    /// </summary>
    public async ValueTask SetInputValueAsync(ElementReference element, string value)
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("setInputValue", element, value);
    }

    /// <summary>
    /// Suppresses the browser default (page scroll) for the given keys on matching
    /// descendants of <paramref name="container"/>, without handling the keys.
    /// Release with <see cref="CleanupAsync"/>.
    /// </summary>
    public async ValueTask PreventKeyDefaultsAsync(
        ElementReference container, string id, string[] keys, string selector)
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("preventKeyDefaults", container, id, keys, selector);
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
            FlipSideOnRtl = options.FlipSideOnRtl,
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
    /// Positions a context-menu popup at pointer coordinates, flipping and clamping
    /// against the viewport edges, and sets --available-height/--transform-origin on it.
    /// </summary>
    public async ValueTask PositionContextMenuAsync(ElementReference element, double x, double y)
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("positionContextMenu", element, x, y);
    }

    /// <summary>
    /// Sets up keyboard navigation for a menu/list container.
    /// Arrow keys navigate between items, Escape invokes .NET callback.
    /// </summary>
    public async ValueTask SetupKeyboardNavigationAsync<T>(
        ElementReference container, string id,
        DotNetObjectReference<T> dotnetRef, string escapeMethodName,
        string itemSelector = "[data-menu-item]",
        string orientation = "vertical",
        string? initialSelector = null) where T : class
    {
        var module = await GetModuleAsync();
        var jsOptions = new KeyboardNavJsOptions
        {
            Selector = itemSelector,
            Orientation = orientation,
            InitialSelector = initialSelector,
        };
        await module.InvokeVoidAsync(
            "setupKeyboardNavigation", container, id, dotnetRef, escapeMethodName, jsOptions);
    }

    /// <summary>
    /// Sets up arrow-key navigation for a persistent widget (e.g. a tabs list) that
    /// has no Escape callback and must not steal focus on mount.
    /// </summary>
    public async ValueTask SetupKeyboardNavigationAsync(
        ElementReference container, string id,
        string itemSelector = "[data-menu-item]",
        string orientation = "vertical")
    {
        var module = await GetModuleAsync();
        var jsOptions = new KeyboardNavJsOptions
        {
            Selector = itemSelector,
            Orientation = orientation,
            AutoFocus = false,
        };
        await module.InvokeVoidAsync(
            "setupKeyboardNavigation", container, id, null, null, jsOptions);
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
    /// Returns an element's rendered text content (used to derive a command item's
    /// filter text when no explicit value is supplied).
    /// </summary>
    public async ValueTask<string> GetTextContentAsync(ElementReference element)
    {
        var module = await GetModuleAsync();
        return await module.InvokeAsync<string>("getTextContent", element);
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

    /// <summary>
    /// Watches a CSS media query. Invokes <paramref name="methodName"/> on
    /// <paramref name="dotnetRef"/> with the current match state immediately and on
    /// every change. Returns a watcher ID for <see cref="UnwatchMediaAsync"/>.
    /// </summary>
    public async ValueTask<string> WatchMediaAsync<T>(
        string query, DotNetObjectReference<T> dotnetRef, string methodName) where T : class
    {
        var module = await GetModuleAsync();
        return await module.InvokeAsync<string>("watchMedia", query, dotnetRef, methodName);
    }

    /// <summary>
    /// Stops a media-query watcher created by <see cref="WatchMediaAsync"/>.
    /// </summary>
    public async ValueTask UnwatchMediaAsync(string id)
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("unwatchMedia", id);
    }

    /// <summary>
    /// Registers the global Ctrl/Cmd+B sidebar-toggle shortcut. Torn down via
    /// <see cref="CleanupAsync"/> with the same ID.
    /// </summary>
    public async ValueTask InitSidebarShortcutAsync<T>(
        string id, DotNetObjectReference<T> dotnetRef, string methodName) where T : class
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("initSidebarShortcut", id, dotnetRef, methodName);
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
            try
            {
                await _module.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
                // Blazor Server: circuit already gone when scoped services dispose.
            }
            catch (ObjectDisposedException)
            {
                // Runtime already torn down (e.g. prerendering shutdown).
            }
        }
        _initLock.Dispose();
    }
}

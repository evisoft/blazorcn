using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorCN;

/// <summary>
/// Typed wrapper for BlazorCN JavaScript interop calls.
/// </summary>
public sealed class JsInteropCn : IAsyncDisposable, IDisposable
{
    private readonly IJSRuntime _js;
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
        return _module ??= await _js.InvokeAsync<IJSObjectReference>(
            "import", "./_content/BlazorCN/blazorcn.js");
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
        var jsOptions = new
        {
            side = options.Side.ToString().ToLowerInvariant(),
            sideOffset = options.SideOffset,
            align = options.Align.ToString().ToLowerInvariant(),
            alignOffset = options.AlignOffset
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
        var jsOptions = new
        {
            selector = itemSelector,
            orientation
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

    /// <inheritdoc />
    public void Dispose()
    {
        // Synchronous dispose for DI container compatibility.
        // The module will be cleaned up by the JS runtime.
        _module = null;
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_module is not null)
        {
            await _module.DisposeAsync();
        }
    }
}

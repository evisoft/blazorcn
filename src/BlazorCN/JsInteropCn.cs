using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorCN;

/// <summary>
/// Typed wrapper for BlazorCN JavaScript interop calls.
/// </summary>
public sealed class JsInteropCn : IAsyncDisposable
{
    private readonly IJSRuntime _js;
    private IJSObjectReference? _module;

    public JsInteropCn(IJSRuntime js)
    {
        _js = js;
    }

    private async ValueTask<IJSObjectReference> GetModuleAsync()
    {
        return _module ??= await _js.InvokeAsync<IJSObjectReference>(
            "import", "./_content/BlazorCN/blazorcn.js");
    }

    public async ValueTask TrapFocusAsync(ElementReference element, string id)
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("trapFocus", element, id);
    }

    public async ValueTask OnOutsideClickAsync<T>(
        ElementReference element, string id,
        DotNetObjectReference<T> dotnetRef, string methodName) where T : class
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("onOutsideClick", element, id, dotnetRef, methodName);
    }

    public async ValueTask LockScrollAsync(string id)
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("lockScroll", id);
    }

    public async ValueTask CleanupAsync(string id)
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("cleanup", id);
    }

    public async ValueTask DisposeAsync()
    {
        if (_module is not null)
        {
            await _module.DisposeAsync();
        }
    }
}

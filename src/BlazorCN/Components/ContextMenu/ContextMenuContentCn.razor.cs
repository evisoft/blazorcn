using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorCN;

public partial class ContextMenuContentCn : IAsyncDisposable
{
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [CascadingParameter] public ContextMenuCn? ContextMenu { get; set; }
    [Inject] private JsInteropCn JsInterop { get; set; } = default!;

    private ElementReference _contentRef;
    private readonly string _outsideClickId = $"context-outside-{Guid.NewGuid():N}";
    private readonly string _keyboardNavId = $"context-kbd-{Guid.NewGuid():N}";
    private DotNetObjectReference<ContextMenuContentCn>? _dotnetRef;
    private bool _jsInitialized;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (ContextMenu?.IsOpen == true && !_jsInitialized)
        {
            _jsInitialized = true;
            try
            {
                _dotnetRef = DotNetObjectReference.Create(this);
                await JsInterop.PositionContextMenuAsync(_contentRef, ContextMenu.ClientX, ContextMenu.ClientY);
                await JsInterop.OnOutsideClickAsync(_contentRef, _outsideClickId, _dotnetRef, "OnOutsideClick");
                await JsInterop.SetupKeyboardNavigationAsync(_contentRef, _keyboardNavId, _dotnetRef, "OnEscapeKey");
            }
            catch
            {
                _jsInitialized = false;
                _dotnetRef?.Dispose();
                _dotnetRef = null;
            }
        }
        else if (ContextMenu?.IsOpen != true && _jsInitialized)
        {
            await CleanupJs();
        }
    }

    [JSInvokable]
    public async Task OnOutsideClick()
    {
        if (ContextMenu is not null) await ContextMenu.SetOpen(false);
    }

    [JSInvokable]
    public async Task OnEscapeKey()
    {
        if (ContextMenu is not null) await ContextMenu.SetOpen(false);
    }

    private async Task CleanupJs()
    {
        if (_jsInitialized)
        {
            try
            {
                await JsInterop.CleanupAsync(_outsideClickId);
                await JsInterop.CleanupKeyboardNavigationAsync(_keyboardNavId);
            }
            catch { /* Component may be disposed after circuit */ }
            _jsInitialized = false;
            _dotnetRef?.Dispose();
            _dotnetRef = null;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await CleanupJs();
    }
}

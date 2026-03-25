using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorCN;

public partial class DropdownMenuSubContentCn : IAsyncDisposable
{
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public FloatingSide Side { get; set; } = FloatingSide.Right;
    [Parameter] public int SideOffset { get; set; }
    [Parameter] public FloatingAlign Align { get; set; } = FloatingAlign.Start;
    [Parameter] public int AlignOffset { get; set; } = -3;
    [CascadingParameter] public DropdownMenuSubCn? Sub { get; set; }
    [Inject] private JsInteropCn JsInterop { get; set; } = default!;

    private ElementReference _contentRef;
    private readonly string _id = $"dropdown-sub-{Guid.NewGuid():N}";
    private readonly string _keyboardNavId = $"dropdown-sub-kbd-{Guid.NewGuid():N}";
    private DotNetObjectReference<DropdownMenuSubContentCn>? _dotnetRef;
    private bool _jsInitialized;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (Sub?.IsOpen == true && !_jsInitialized)
        {
            _jsInitialized = true;
            try
            {
                _dotnetRef = DotNetObjectReference.Create(this);
                await JsInterop.CreateFloatingAsync(Sub.TriggerElement, _contentRef, _id,
                    new FloatingOptions
                    {
                        Side = Side,
                        SideOffset = SideOffset,
                        Align = Align,
                        AlignOffset = AlignOffset
                    });
                await JsInterop.SetupKeyboardNavigationAsync(_contentRef, _keyboardNavId, _dotnetRef, "OnEscapeKey");
            }
            catch
            {
                _jsInitialized = false;
                _dotnetRef?.Dispose();
                _dotnetRef = null;
            }
        }
        else if (Sub?.IsOpen != true && _jsInitialized)
        {
            await CleanupJs();
        }
    }

    [JSInvokable]
    public void OnEscapeKey()
    {
        Sub?.SetOpen(false);
    }

    private async Task CleanupJs()
    {
        if (_jsInitialized)
        {
            try
            {
                await JsInterop.DestroyFloatingAsync(_id);
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

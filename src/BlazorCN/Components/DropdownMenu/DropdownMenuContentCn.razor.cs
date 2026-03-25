using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorCN;

public partial class DropdownMenuContentCn : IAsyncDisposable
{
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public FloatingSide Side { get; set; } = FloatingSide.Bottom;
    [Parameter] public int SideOffset { get; set; } = 4;
    [Parameter] public FloatingAlign Align { get; set; } = FloatingAlign.Start;
    [Parameter] public int AlignOffset { get; set; }
    [CascadingParameter] public DropdownMenuCn? DropdownMenu { get; set; }
    [Inject] private JsInteropCn JsInterop { get; set; } = default!;

    private ElementReference _contentRef;
    private readonly string _id = $"dropdown-{Guid.NewGuid():N}";
    private readonly string _outsideClickId = $"dropdown-outside-{Guid.NewGuid():N}";
    private readonly string _keyboardNavId = $"dropdown-kbd-{Guid.NewGuid():N}";
    private DotNetObjectReference<DropdownMenuContentCn>? _dotnetRef;
    private bool _jsInitialized;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (DropdownMenu?.IsOpen == true && !_jsInitialized)
        {
            _jsInitialized = true;
            try
            {
                _dotnetRef = DotNetObjectReference.Create(this);
                await JsInterop.CreateFloatingAsync(DropdownMenu.TriggerElement, _contentRef, _id,
                    new FloatingOptions
                    {
                        Side = Side,
                        SideOffset = SideOffset,
                        Align = Align,
                        AlignOffset = AlignOffset
                    });
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
        else if (DropdownMenu?.IsOpen != true && _jsInitialized)
        {
            await CleanupJs();
        }
    }

    [JSInvokable]
    public async Task OnOutsideClick()
    {
        if (DropdownMenu is not null) await DropdownMenu.SetOpen(false);
    }

    [JSInvokable]
    public async Task OnEscapeKey()
    {
        if (DropdownMenu is not null) await DropdownMenu.SetOpen(false);
    }

    private async Task CleanupJs()
    {
        if (_jsInitialized)
        {
            try
            {
                await JsInterop.DestroyFloatingAsync(_id);
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

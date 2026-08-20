using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorCN;

public partial class MenubarContentCn : IAsyncDisposable
{
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public FloatingSide Side { get; set; } = FloatingSide.Bottom;
    [Parameter] public int SideOffset { get; set; } = 8;
    [Parameter] public FloatingAlign Align { get; set; } = FloatingAlign.Start;
    [Parameter] public int AlignOffset { get; set; } = -4;
    [CascadingParameter] public MenubarMenuCn? Menu { get; set; }
    [Inject] private JsInteropCn JsInterop { get; set; } = default!;

    private ElementReference _contentRef;
    private readonly string _id = $"menubar-{Guid.NewGuid():N}";
    private readonly string _outsideClickId = $"menubar-outside-{Guid.NewGuid():N}";
    private readonly string _keyboardNavId = $"menubar-kbd-{Guid.NewGuid():N}";
    private DotNetObjectReference<MenubarContentCn>? _dotnetRef;
    private bool _jsInitialized;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (Menu?.IsOpen == true && !_jsInitialized)
        {
            _jsInitialized = true;
            try
            {
                _dotnetRef = DotNetObjectReference.Create(this);
                await JsInterop.CreateFloatingAsync(Menu.TriggerElement, _contentRef, _id,
                    new FloatingOptions
                    {
                        Side = Side,
                        SideOffset = SideOffset,
                        Align = Align,
                        AlignOffset = AlignOffset
                    });
                await JsInterop.OnOutsideClickAsync(_contentRef, _outsideClickId, _dotnetRef, "OnOutsideClick", Menu?.TriggerElement);
                await JsInterop.SetupKeyboardNavigationAsync(_contentRef, _keyboardNavId, _dotnetRef, "OnEscapeKey");
            }
            catch
            {
                _jsInitialized = false;
                _dotnetRef?.Dispose();
                _dotnetRef = null;
            }
        }
        else if (Menu?.IsOpen != true && _jsInitialized)
        {
            await CleanupJs();
        }
    }

    [JSInvokable]
    public void OnOutsideClick()
    {
        Menu?.SetOpen(false);
    }

    [JSInvokable]
    public void OnEscapeKey()
    {
        Menu?.SetOpen(false);
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

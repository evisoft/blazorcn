using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorCN;

public partial class ComboboxContentCn : IAsyncDisposable
{
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public FloatingSide Side { get; set; } = FloatingSide.Bottom;
    [Parameter] public int SideOffset { get; set; } = 4;
    [Parameter] public FloatingAlign Align { get; set; } = FloatingAlign.Center;
    [Parameter] public int AlignOffset { get; set; }
    [CascadingParameter] public ComboboxCn? Combobox { get; set; }
    [Inject] private JsInteropCn JsInterop { get; set; } = default!;

    private ElementReference _contentRef;
    private readonly string _id = $"combobox-{Guid.NewGuid():N}";
    private readonly string _outsideClickId = $"combobox-outside-{Guid.NewGuid():N}";
    private readonly string _keyboardNavId = $"combobox-kbd-{Guid.NewGuid():N}";
    private DotNetObjectReference<ComboboxContentCn>? _dotnetRef;
    private bool _jsInitialized;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (Combobox?.IsOpen == true && !_jsInitialized)
        {
            _dotnetRef = DotNetObjectReference.Create(this);
            await JsInterop.CreateFloatingAsync(Combobox.TriggerElement, _contentRef, _id,
                new FloatingOptions
                {
                    Side = Side,
                    SideOffset = SideOffset,
                    Align = Align,
                    AlignOffset = AlignOffset
                });
            await JsInterop.OnOutsideClickAsync(_contentRef, _outsideClickId, _dotnetRef, "OnOutsideClick");
            await JsInterop.SetupKeyboardNavigationAsync(_contentRef, _keyboardNavId, _dotnetRef, "OnEscapeKey");
            _jsInitialized = true;
        }
        else if (Combobox?.IsOpen != true && _jsInitialized)
        {
            await CleanupJs();
        }
    }

    [JSInvokable]
    public async Task OnOutsideClick()
    {
        if (Combobox is not null) await Combobox.SetOpen(false);
    }

    [JSInvokable]
    public async Task OnEscapeKey()
    {
        if (Combobox is not null) await Combobox.SetOpen(false);
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

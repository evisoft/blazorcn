using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace BlazorCN;

public partial class ComboboxContentCn : IAsyncDisposable
{
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public FloatingSide Side { get; set; } = FloatingSide.Bottom;
    [Parameter] public int SideOffset { get; set; } = 4;
    [Parameter] public FloatingAlign Align { get; set; } = FloatingAlign.Start;
    [Parameter] public int AlignOffset { get; set; }

    /// <summary>Accessible name for the listbox. Set to localize the default ("Suggestions");
    /// set to null to label the listbox via the trigger instead (aria-labelledby).</summary>
    [Parameter] public string? AriaLabel { get; set; } = "Suggestions";

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
            _jsInitialized = true;
            try
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
                // Not `Combobox?.` — the guard above already proved it non-null, and a
                // null-conditional access here resets the compiler's null-state, which made
                // the FocusInputAsync call below warn CS8602.
                await JsInterop.OnOutsideClickAsync(_contentRef, _outsideClickId, _dotnetRef, "OnOutsideClick", Combobox.TriggerElement);
                // No-autofocus overload: DOM focus must land in the search input, not the
                // first option (Base UI behavior — arrows still work via the container
                // listener). Escape is handled by HandleKeyDown on the content div.
                await JsInterop.SetupKeyboardNavigationAsync(_contentRef, _keyboardNavId);
                await Combobox.FocusInputAsync();
            }
            catch
            {
                _jsInitialized = false;
                _dotnetRef?.Dispose();
                _dotnetRef = null;
            }
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
        if (Combobox is not null)
        {
            await Combobox.SetOpen(false);
            // Keyboard nav runs without autofocus, so JS won't restore focus — return
            // it to the trigger explicitly (Radix behavior).
            await Combobox.FocusTriggerAsync();
        }
    }

    private async Task HandleKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Escape") await OnEscapeKey();
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

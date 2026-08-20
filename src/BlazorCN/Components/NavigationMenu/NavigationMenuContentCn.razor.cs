using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorCN;

public partial class NavigationMenuContentCn : IAsyncDisposable
{
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [CascadingParameter] public NavigationMenuItemCn? NavItem { get; set; }
    [Inject] private JsInteropCn JsInterop { get; set; } = default!;

    private ElementReference _contentRef;
    private readonly string _id = $"navmenu-{Guid.NewGuid():N}";
    private readonly string _outsideClickId = $"navmenu-outside-{Guid.NewGuid():N}";
    private DotNetObjectReference<NavigationMenuContentCn>? _dotnetRef;
    private bool _jsInitialized;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (NavItem?.IsOpen == true && !_jsInitialized)
        {
            _jsInitialized = true;
            try
            {
                _dotnetRef = DotNetObjectReference.Create(this);
                await JsInterop.CreateFloatingAsync(NavItem.TriggerElement, _contentRef, _id,
                    new FloatingOptions
                    {
                        Side = FloatingSide.Bottom,
                        SideOffset = 4,
                        Align = FloatingAlign.Start,
                        AlignOffset = 0
                    });
                await JsInterop.OnOutsideClickAsync(_contentRef, _outsideClickId, _dotnetRef, "OnOutsideClick", NavItem?.TriggerElement);
            }
            catch
            {
                _jsInitialized = false;
                _dotnetRef?.Dispose();
                _dotnetRef = null;
            }
        }
        else if (NavItem?.IsOpen != true && _jsInitialized)
        {
            await CleanupJs();
        }
    }

    [JSInvokable]
    public Task OnOutsideClick()
    {
        NavItem?.CloseNow();
        return Task.CompletedTask;
    }

    private void HandleMouseEnter()
    {
        NavItem?.CancelClose();
    }

    private async Task HandleMouseLeave()
    {
        if (NavItem is not null) await NavItem.RequestClose();
    }

    private void HandleKeyDown(Microsoft.AspNetCore.Components.Web.KeyboardEventArgs e)
    {
        // WCAG 1.4.13 / APG: Escape dismisses the open panel.
        if (e.Key == "Escape") NavItem?.CloseNow();
    }

    private async Task CleanupJs()
    {
        if (_jsInitialized)
        {
            try
            {
                await JsInterop.DestroyFloatingAsync(_id);
                await JsInterop.CleanupAsync(_outsideClickId);
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

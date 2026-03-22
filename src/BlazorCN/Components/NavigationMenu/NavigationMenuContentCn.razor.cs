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
    private bool _jsInitialized;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (NavItem?.IsOpen == true && !_jsInitialized)
        {
            await JsInterop.CreateFloatingAsync(NavItem.TriggerElement, _contentRef, _id,
                new FloatingOptions
                {
                    Side = FloatingSide.Bottom,
                    SideOffset = 4,
                    Align = FloatingAlign.Start,
                    AlignOffset = 0
                });
            _jsInitialized = true;
        }
        else if (NavItem?.IsOpen != true && _jsInitialized)
        {
            await CleanupJs();
        }
    }

    private void HandleMouseEnter()
    {
        NavItem?.CancelClose();
    }

    private async Task HandleMouseLeave()
    {
        if (NavItem is not null) await NavItem.RequestClose();
    }

    private async Task CleanupJs()
    {
        if (_jsInitialized)
        {
            try
            {
                await JsInterop.DestroyFloatingAsync(_id);
            }
            catch { /* Component may be disposed after circuit */ }
            _jsInitialized = false;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await CleanupJs();
    }
}

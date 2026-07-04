using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BlazorCN.Tests.Components;

public class ResizableCnTests : BunitContext
{
    public ResizableCnTests()
    {
        // ResizablePanelGroupCn injects JsInteropCn and calls initResizable in OnAfterRenderAsync.
        // Loose mode lets those interop calls no-op, and registering the service satisfies [Inject].
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddScoped<JsInteropCn>();
    }

    // --- ResizablePanelGroupCn ---

    [Fact]
    public void PanelGroup_Renders_With_DataSlot()
    {
        var cut = Render<ResizablePanelGroupCn>(p => p.AddChildContent("Content"));
        cut.Find("[data-slot='resizable-panel-group']").Should().NotBeNull();
    }

    [Fact]
    public void PanelGroup_Has_Default_Classes()
    {
        var cut = Render<ResizablePanelGroupCn>(p => p.AddChildContent("Content"));
        var el = cut.Find("[data-slot='resizable-panel-group']");
        el.ClassList.Should().Contain("flex");
        el.ClassList.Should().Contain("h-full");
        el.ClassList.Should().Contain("w-full");
    }

    [Fact]
    public void PanelGroup_Horizontal_No_FlexCol()
    {
        var cut = Render<ResizablePanelGroupCn>(p => p
            .Add(c => c.Direction, ResizableDirection.Horizontal)
            .AddChildContent("Content"));
        cut.Find("[data-slot='resizable-panel-group']").ClassList.Should().NotContain("flex-col");
    }

    [Fact]
    public void PanelGroup_Vertical_Has_FlexCol()
    {
        var cut = Render<ResizablePanelGroupCn>(p => p
            .Add(c => c.Direction, ResizableDirection.Vertical)
            .AddChildContent("Content"));
        cut.Find("[data-slot='resizable-panel-group']").ClassList.Should().Contain("flex-col");
    }

    [Fact]
    public void PanelGroup_Has_DataDirection()
    {
        var cut = Render<ResizablePanelGroupCn>(p => p
            .Add(c => c.Direction, ResizableDirection.Horizontal)
            .AddChildContent("Content"));
        cut.Find("[data-slot='resizable-panel-group']").GetAttribute("data-direction").Should().Be("horizontal");
    }

    [Fact]
    public void PanelGroup_Vertical_DataDirection()
    {
        var cut = Render<ResizablePanelGroupCn>(p => p
            .Add(c => c.Direction, ResizableDirection.Vertical)
            .AddChildContent("Content"));
        cut.Find("[data-slot='resizable-panel-group']").GetAttribute("data-direction").Should().Be("vertical");
    }

    [Fact]
    public void PanelGroup_Class_Passthrough()
    {
        var cut = Render<ResizablePanelGroupCn>(p => p
            .Add(c => c.Class, "custom-group")
            .AddChildContent("Content"));
        cut.Find("[data-slot='resizable-panel-group']").ClassList.Should().Contain("custom-group");
    }

    [Fact]
    public void PanelGroup_AdditionalAttributes_Passthrough()
    {
        var cut = Render<ResizablePanelGroupCn>(p => p
            .Add(c => c.AdditionalAttributes, new Dictionary<string, object?> { { "id", "my-group" } })
            .AddChildContent("Content"));
        cut.Find("[data-slot='resizable-panel-group']").GetAttribute("id").Should().Be("my-group");
    }

    // --- ResizablePanelCn ---

    [Fact]
    public void Panel_Renders_With_DataSlot()
    {
        var cut = Render<ResizablePanelGroupCn>(p => p
            .AddChildContent<ResizablePanelCn>(panel => panel
                .AddChildContent("Panel content")));
        cut.Find("[data-slot='resizable-panel']").Should().NotBeNull();
    }

    [Fact]
    public void Panel_Has_Default_Flex_Style()
    {
        var cut = Render<ResizablePanelGroupCn>(p => p
            .AddChildContent<ResizablePanelCn>(panel => panel
                .AddChildContent("Panel content")));
        var style = cut.Find("[data-slot='resizable-panel']").GetAttribute("style");
        style.Should().Contain("flex: 1 1 0%");
    }

    [Fact]
    public void Panel_With_DefaultSize_Has_Flex_Style()
    {
        var cut = Render<ResizablePanelGroupCn>(p => p
            .AddChildContent<ResizablePanelCn>(panel => panel
                .Add(c => c.DefaultSize, 50.0)
                .AddChildContent("Panel content")));
        var style = cut.Find("[data-slot='resizable-panel']").GetAttribute("style");
        style.Should().Contain("flex: 50 50 0%");
    }

    [Fact]
    public void Panel_Class_Passthrough()
    {
        var cut = Render<ResizablePanelGroupCn>(p => p
            .AddChildContent<ResizablePanelCn>(panel => panel
                .Add(c => c.Class, "custom-panel")
                .AddChildContent("Panel content")));
        cut.Find("[data-slot='resizable-panel']").ClassList.Should().Contain("custom-panel");
    }

    [Fact]
    public void Panel_AdditionalAttributes_Passthrough()
    {
        var cut = Render<ResizablePanelGroupCn>(p => p
            .AddChildContent<ResizablePanelCn>(panel => panel
                .Add(c => c.AdditionalAttributes, new Dictionary<string, object?> { { "id", "panel-1" } })
                .AddChildContent("Panel content")));
        cut.Find("[data-slot='resizable-panel']").GetAttribute("id").Should().Be("panel-1");
    }

    // --- ResizableHandleCn ---

    [Fact]
    public void Handle_Renders_With_DataSlot()
    {
        var cut = Render<ResizablePanelGroupCn>(p => p
            .AddChildContent<ResizableHandleCn>());
        cut.Find("[data-slot='resizable-handle']").Should().NotBeNull();
    }

    [Fact]
    public void Handle_Has_Default_Classes()
    {
        var cut = Render<ResizablePanelGroupCn>(p => p
            .AddChildContent<ResizableHandleCn>());
        var handle = cut.Find("[data-slot='resizable-handle']");
        handle.ClassList.Should().Contain("relative");
        handle.ClassList.Should().Contain("flex");
        handle.ClassList.Should().Contain("items-center");
        handle.ClassList.Should().Contain("justify-center");
        handle.ClassList.Should().Contain("bg-border");
    }

    [Fact]
    public void Handle_Horizontal_Has_WPx()
    {
        var cut = Render<ResizablePanelGroupCn>(p => p
            .Add(c => c.Direction, ResizableDirection.Horizontal)
            .AddChildContent<ResizableHandleCn>());
        cut.Find("[data-slot='resizable-handle']").ClassList.Should().Contain("w-px");
    }

    [Fact]
    public void Handle_Vertical_Has_HPx()
    {
        var cut = Render<ResizablePanelGroupCn>(p => p
            .Add(c => c.Direction, ResizableDirection.Vertical)
            .AddChildContent<ResizableHandleCn>());
        cut.Find("[data-slot='resizable-handle']").ClassList.Should().Contain("h-px");
    }

    [Fact]
    public void Handle_Has_Separator_Role()
    {
        var cut = Render<ResizablePanelGroupCn>(p => p
            .AddChildContent<ResizableHandleCn>());
        cut.Find("[data-slot='resizable-handle']").GetAttribute("role").Should().Be("separator");
    }

    [Fact]
    public void Handle_Has_Default_AriaLabel()
    {
        var cut = Render<ResizablePanelGroupCn>(p => p
            .AddChildContent<ResizableHandleCn>());
        cut.Find("[data-slot='resizable-handle']").GetAttribute("aria-label").Should().Be("Resize");
    }

    [Fact]
    public void Handle_AriaLabel_Override_Via_AdditionalAttributes()
    {
        var cut = Render<ResizablePanelGroupCn>(p => p
            .AddChildContent<ResizableHandleCn>(h => h
                .Add(c => c.AdditionalAttributes, new Dictionary<string, object?> { { "aria-label", "Drag to resize" } })));
        cut.Find("[data-slot='resizable-handle']").GetAttribute("aria-label").Should().Be("Drag to resize");
    }

    [Fact]
    public void Handle_Without_Grip_Has_No_Inner_Div()
    {
        var cut = Render<ResizablePanelGroupCn>(p => p
            .AddChildContent<ResizableHandleCn>(h => h
                .Add(c => c.WithHandle, false)));
        cut.FindAll("[data-slot='resizable-handle'] > div").Should().BeEmpty();
    }

    [Fact]
    public void Handle_With_Grip_Has_Inner_Div()
    {
        var cut = Render<ResizablePanelGroupCn>(p => p
            .AddChildContent<ResizableHandleCn>(h => h
                .Add(c => c.WithHandle, true)));
        var inner = cut.Find("[data-slot='resizable-handle'] > div");
        inner.Should().NotBeNull();
        inner.ClassList.Should().Contain("z-10");
        inner.ClassList.Should().Contain("bg-border");
    }

    [Fact]
    public void Handle_With_Grip_Contains_Svg()
    {
        var cut = Render<ResizablePanelGroupCn>(p => p
            .AddChildContent<ResizableHandleCn>(h => h
                .Add(c => c.WithHandle, true)));
        // Grip is now a plain bar (matches shadcn radix-example), not a dotted SVG.
        cut.FindAll("[data-slot='resizable-handle'] svg").Should().BeEmpty();
    }

    [Fact]
    public void Handle_Class_Passthrough()
    {
        var cut = Render<ResizablePanelGroupCn>(p => p
            .AddChildContent<ResizableHandleCn>(h => h
                .Add(c => c.Class, "custom-handle")));
        cut.Find("[data-slot='resizable-handle']").ClassList.Should().Contain("custom-handle");
    }

    [Fact]
    public void Handle_AdditionalAttributes_Passthrough()
    {
        var cut = Render<ResizablePanelGroupCn>(p => p
            .AddChildContent<ResizableHandleCn>(h => h
                .Add(c => c.AdditionalAttributes, new Dictionary<string, object?> { { "id", "handle-1" } })));
        cut.Find("[data-slot='resizable-handle']").GetAttribute("id").Should().Be("handle-1");
    }

    // --- Integration ---

    [Fact]
    public void Full_Layout_With_Two_Panels_And_Handle()
    {
        var cut = Render<ResizablePanelGroupCn>(p => p
            .Add(c => c.Direction, ResizableDirection.Horizontal)
            .AddChildContent("<ResizablePanelCn DefaultSize=\"50\">Left</ResizablePanelCn><ResizableHandleCn WithHandle=\"true\" /><ResizablePanelCn DefaultSize=\"50\">Right</ResizablePanelCn>"));
        cut.Find("[data-slot='resizable-panel-group']").Should().NotBeNull();
    }
}

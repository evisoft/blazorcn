using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BlazorCN.Tests.Components;

public class DrawerCnTests : BunitContext
{
    private void SetupJsInterop()
    {
        var module = JSInterop.SetupModule("./_content/BlazorCN/blazorcn.js");
        module.SetupVoid("trapFocus", _ => true).SetVoidResult();
        module.SetupVoid("lockScroll", _ => true).SetVoidResult();
        module.SetupVoid("cleanup", _ => true).SetVoidResult();
        Services.AddScoped<JsInteropCn>();
    }

    // --- DrawerCn ---

    [Fact]
    public void Drawer_Renders_With_DataSlot()
    {
        var cut = Render<DrawerCn>(p => p.AddChildContent("Content"));
        cut.Find("[data-slot='drawer']").Should().NotBeNull();
    }

    [Fact]
    public void Drawer_Starts_Closed_By_Default()
    {
        var cut = Render<DrawerCn>(p => p.AddChildContent("Content"));
        cut.Find("[data-slot='drawer']").GetAttribute("data-state").Should().Be("closed");
    }

    [Fact]
    public void Drawer_Starts_Open_When_Open_Is_True()
    {
        var cut = Render<DrawerCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent("Content"));
        cut.Find("[data-slot='drawer']").GetAttribute("data-state").Should().Be("open");
    }

    [Fact]
    public void Drawer_Class_Passthrough()
    {
        var cut = Render<DrawerCn>(p => p
            .Add(c => c.Class, "custom-class")
            .AddChildContent("Content"));
        cut.Find("[data-slot='drawer']").ClassList.Should().Contain("custom-class");
    }

    [Fact]
    public void Drawer_AdditionalAttributes_Passthrough()
    {
        var cut = Render<DrawerCn>(p => p
            .Add(c => c.AdditionalAttributes, new Dictionary<string, object?> { { "id", "my-drawer" } })
            .AddChildContent("Content"));
        cut.Find("[data-slot='drawer']").GetAttribute("id").Should().Be("my-drawer");
    }

    // --- DrawerTriggerCn ---

    [Fact]
    public void DrawerTrigger_Renders_With_DataSlot()
    {
        var cut = Render<DrawerCn>(p => p
            .AddChildContent<DrawerTriggerCn>(t => t
                .AddChildContent("Open")));
        cut.Find("[data-slot='drawer-trigger']").Should().NotBeNull();
    }

    [Fact]
    public void DrawerTrigger_Opens_Drawer()
    {
        var isOpen = false;
        var cut = Render<DrawerCn>(p => p
            .Add(c => c.OpenChanged, EventCallback.Factory.Create<bool>(this, v => isOpen = v))
            .AddChildContent<DrawerTriggerCn>(t => t
                .AddChildContent("Open")));
        cut.Find("[data-slot='drawer-trigger']").Click();
        isOpen.Should().BeTrue();
    }

    [Fact]
    public void DrawerTrigger_Has_Button_Type()
    {
        var cut = Render<DrawerCn>(p => p
            .AddChildContent<DrawerTriggerCn>(t => t
                .AddChildContent("Open")));
        cut.Find("[data-slot='drawer-trigger']").GetAttribute("type").Should().Be("button");
    }

    // --- DrawerOverlayCn ---

    [Fact]
    public void DrawerOverlay_Not_Rendered_When_Closed()
    {
        var cut = Render<DrawerCn>(p => p
            .AddChildContent<DrawerOverlayCn>());
        cut.FindAll("[data-slot='drawer-overlay']").Should().BeEmpty();
    }

    [Fact]
    public void DrawerOverlay_Rendered_When_Open()
    {
        var cut = Render<DrawerCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<DrawerOverlayCn>());
        cut.Find("[data-slot='drawer-overlay']").Should().NotBeNull();
    }

    [Fact]
    public void DrawerOverlay_Has_Default_Classes()
    {
        var cut = Render<DrawerCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<DrawerOverlayCn>());
        var el = cut.Find("[data-slot='drawer-overlay']");
        el.ClassList.Should().Contain("fixed");
        el.ClassList.Should().Contain("inset-0");
        el.ClassList.Should().Contain("z-50");
    }

    [Fact]
    public void DrawerOverlay_Click_Closes_Drawer()
    {
        var isOpen = true;
        var cut = Render<DrawerCn>(p => p
            .Add(c => c.Open, true)
            .Add(c => c.OpenChanged, EventCallback.Factory.Create<bool>(this, v => isOpen = v))
            .AddChildContent<DrawerOverlayCn>());
        cut.Find("[data-slot='drawer-overlay']").Click();
        isOpen.Should().BeFalse();
    }

    [Fact]
    public void DrawerOverlay_Class_Passthrough()
    {
        var cut = Render<DrawerCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<DrawerOverlayCn>(o => o
                .Add(c => c.Class, "custom-overlay")));
        cut.Find("[data-slot='drawer-overlay']").ClassList.Should().Contain("custom-overlay");
    }

    // --- DrawerContentCn ---

    [Fact]
    public void DrawerContent_Not_Rendered_When_Closed()
    {
        SetupJsInterop();
        var cut = Render<DrawerCn>(p => p
            .AddChildContent<DrawerContentCn>(c => c
                .AddChildContent("Body")));
        cut.FindAll("[data-slot='drawer-content']").Should().BeEmpty();
    }

    [Fact]
    public void DrawerContent_Rendered_When_Open()
    {
        SetupJsInterop();
        var cut = Render<DrawerCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<DrawerContentCn>(c => c
                .AddChildContent("Body")));
        cut.Find("[data-slot='drawer-content']").Should().NotBeNull();
    }

    [Fact]
    public void DrawerContent_Default_Direction_Is_Bottom()
    {
        SetupJsInterop();
        var cut = Render<DrawerCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<DrawerContentCn>(c => c
                .AddChildContent("Body")));
        var el = cut.Find("[data-slot='drawer-content']");
        el.GetAttribute("data-direction").Should().Be("bottom");
        el.ClassList.Should().Contain("border-t");
        el.ClassList.Should().Contain("rounded-t-lg");
    }

    [Theory]
    [InlineData(DrawerDirection.Bottom, "bottom", "border-t")]
    [InlineData(DrawerDirection.Top, "top", "border-b")]
    [InlineData(DrawerDirection.Left, "left", "border-r")]
    [InlineData(DrawerDirection.Right, "right", "border-l")]
    public void DrawerContent_Renders_Correct_Classes_For_Direction(DrawerDirection direction, string expectedDataDir, string expectedBorderClass)
    {
        SetupJsInterop();
        var cut = Render<DrawerCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<DrawerContentCn>(c => c
                .Add(x => x.Direction, direction)
                .AddChildContent("Body")));
        var el = cut.Find("[data-slot='drawer-content']");
        el.GetAttribute("data-direction").Should().Be(expectedDataDir);
        el.ClassList.Should().Contain(expectedBorderClass);
    }

    [Fact]
    public void DrawerContent_Has_Base_Classes()
    {
        SetupJsInterop();
        var cut = Render<DrawerCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<DrawerContentCn>(c => c
                .AddChildContent("Body")));
        var el = cut.Find("[data-slot='drawer-content']");
        el.ClassList.Should().Contain("fixed");
        el.ClassList.Should().Contain("z-50");
        el.ClassList.Should().Contain("flex");
        el.ClassList.Should().Contain("flex-col");
        el.ClassList.Should().Contain("bg-background");
    }

    [Fact]
    public void DrawerContent_Bottom_Has_Drag_Handle()
    {
        SetupJsInterop();
        var cut = Render<DrawerCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<DrawerContentCn>(c => c
                .Add(x => x.Direction, DrawerDirection.Bottom)
                .AddChildContent("Body")));
        cut.Find("[data-slot='drawer-handle']").Should().NotBeNull();
    }

    [Fact]
    public void DrawerContent_Top_Has_No_Drag_Handle()
    {
        SetupJsInterop();
        var cut = Render<DrawerCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<DrawerContentCn>(c => c
                .Add(x => x.Direction, DrawerDirection.Top)
                .AddChildContent("Body")));
        cut.FindAll("[data-slot='drawer-handle']").Should().BeEmpty();
    }

    [Fact]
    public void DrawerContent_Class_Passthrough()
    {
        SetupJsInterop();
        var cut = Render<DrawerCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<DrawerContentCn>(c => c
                .Add(x => x.Class, "custom-content")
                .AddChildContent("Body")));
        cut.Find("[data-slot='drawer-content']").ClassList.Should().Contain("custom-content");
    }

    [Fact]
    public void DrawerContent_AdditionalAttributes_Passthrough()
    {
        SetupJsInterop();
        var cut = Render<DrawerCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<DrawerContentCn>(c => c
                .Add(x => x.AdditionalAttributes, new Dictionary<string, object?> { { "role", "dialog" } })
                .AddChildContent("Body")));
        cut.Find("[data-slot='drawer-content']").GetAttribute("role").Should().Be("dialog");
    }

    // --- DrawerHeaderCn ---

    [Fact]
    public void DrawerHeader_Renders_With_DataSlot()
    {
        var cut = Render<DrawerHeaderCn>(p => p.AddChildContent("Header"));
        cut.Find("[data-slot='drawer-header']").Should().NotBeNull();
    }

    [Fact]
    public void DrawerHeader_Has_Default_Classes()
    {
        var cut = Render<DrawerHeaderCn>(p => p.AddChildContent("Header"));
        var el = cut.Find("[data-slot='drawer-header']");
        el.ClassList.Should().Contain("grid");
        el.ClassList.Should().Contain("p-4");
    }

    [Fact]
    public void DrawerHeader_Class_Passthrough()
    {
        var cut = Render<DrawerHeaderCn>(p => p
            .Add(c => c.Class, "custom-header")
            .AddChildContent("Header"));
        cut.Find("[data-slot='drawer-header']").ClassList.Should().Contain("custom-header");
    }

    // --- DrawerFooterCn ---

    [Fact]
    public void DrawerFooter_Renders_With_DataSlot()
    {
        var cut = Render<DrawerFooterCn>(p => p.AddChildContent("Footer"));
        cut.Find("[data-slot='drawer-footer']").Should().NotBeNull();
    }

    [Fact]
    public void DrawerFooter_Has_Default_Classes()
    {
        var cut = Render<DrawerFooterCn>(p => p.AddChildContent("Footer"));
        var el = cut.Find("[data-slot='drawer-footer']");
        el.ClassList.Should().Contain("mt-auto");
        el.ClassList.Should().Contain("flex");
        el.ClassList.Should().Contain("flex-col");
        el.ClassList.Should().Contain("gap-2");
        el.ClassList.Should().Contain("p-4");
    }

    [Fact]
    public void DrawerFooter_Class_Passthrough()
    {
        var cut = Render<DrawerFooterCn>(p => p
            .Add(c => c.Class, "custom-footer")
            .AddChildContent("Footer"));
        cut.Find("[data-slot='drawer-footer']").ClassList.Should().Contain("custom-footer");
    }

    // --- DrawerTitleCn ---

    [Fact]
    public void DrawerTitle_Renders_With_DataSlot()
    {
        var cut = Render<DrawerTitleCn>(p => p.AddChildContent("Title"));
        cut.Find("[data-slot='drawer-title']").Should().NotBeNull();
    }

    [Fact]
    public void DrawerTitle_Has_Default_Classes()
    {
        var cut = Render<DrawerTitleCn>(p => p.AddChildContent("Title"));
        var el = cut.Find("[data-slot='drawer-title']");
        el.ClassList.Should().Contain("text-lg");
        el.ClassList.Should().Contain("font-semibold");
        el.ClassList.Should().Contain("leading-none");
        el.ClassList.Should().Contain("tracking-tight");
    }

    [Fact]
    public void DrawerTitle_Class_Passthrough()
    {
        var cut = Render<DrawerTitleCn>(p => p
            .Add(c => c.Class, "custom-title")
            .AddChildContent("Title"));
        cut.Find("[data-slot='drawer-title']").ClassList.Should().Contain("custom-title");
    }

    // --- DrawerDescriptionCn ---

    [Fact]
    public void DrawerDescription_Renders_With_DataSlot()
    {
        var cut = Render<DrawerDescriptionCn>(p => p.AddChildContent("Description"));
        cut.Find("[data-slot='drawer-description']").Should().NotBeNull();
    }

    [Fact]
    public void DrawerDescription_Has_Default_Classes()
    {
        var cut = Render<DrawerDescriptionCn>(p => p.AddChildContent("Description"));
        var el = cut.Find("[data-slot='drawer-description']");
        el.ClassList.Should().Contain("text-sm");
        el.ClassList.Should().Contain("text-muted-foreground");
    }

    [Fact]
    public void DrawerDescription_Class_Passthrough()
    {
        var cut = Render<DrawerDescriptionCn>(p => p
            .Add(c => c.Class, "custom-desc")
            .AddChildContent("Description"));
        cut.Find("[data-slot='drawer-description']").ClassList.Should().Contain("custom-desc");
    }

    // --- DrawerCloseCn ---

    [Fact]
    public void DrawerClose_Renders_With_DataSlot()
    {
        var cut = Render<DrawerCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<DrawerCloseCn>(c => c
                .AddChildContent("Close")));
        cut.Find("[data-slot='drawer-close']").Should().NotBeNull();
    }

    [Fact]
    public void DrawerClose_Closes_Drawer()
    {
        var isOpen = true;
        var cut = Render<DrawerCn>(p => p
            .Add(c => c.Open, true)
            .Add(c => c.OpenChanged, EventCallback.Factory.Create<bool>(this, v => isOpen = v))
            .AddChildContent<DrawerCloseCn>(c => c
                .AddChildContent("Close")));
        cut.Find("[data-slot='drawer-close']").Click();
        isOpen.Should().BeFalse();
    }

    [Fact]
    public void DrawerClose_Has_Button_Type()
    {
        var cut = Render<DrawerCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<DrawerCloseCn>(c => c
                .AddChildContent("Close")));
        cut.Find("[data-slot='drawer-close']").GetAttribute("type").Should().Be("button");
    }

    // --- Integration ---

    [Fact]
    public void Drawer_Full_Integration()
    {
        SetupJsInterop();
        var isOpen = false;
        var cut = Render<DrawerCn>(p => p
            .Add(c => c.OpenChanged, EventCallback.Factory.Create<bool>(this, v => isOpen = v))
            .AddChildContent<DrawerTriggerCn>(t => t
                .AddChildContent("Open Drawer")));

        cut.Find("[data-slot='drawer']").GetAttribute("data-state").Should().Be("closed");
        cut.Find("[data-slot='drawer-trigger']").Click();
        isOpen.Should().BeTrue();
        cut.Find("[data-slot='drawer']").GetAttribute("data-state").Should().Be("open");
    }
}

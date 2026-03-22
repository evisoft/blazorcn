using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BlazorCN.Tests.Components;

public class SheetCnTests : BunitContext
{
    private void SetupJsInterop()
    {
        var module = JSInterop.SetupModule("./_content/BlazorCN/blazorcn.js");
        module.SetupVoid("trapFocus", _ => true).SetVoidResult();
        module.SetupVoid("lockScroll", _ => true).SetVoidResult();
        module.SetupVoid("cleanup", _ => true).SetVoidResult();
        Services.AddScoped<JsInteropCn>();
    }

    // --- SheetCn ---

    [Fact]
    public void Sheet_Renders_With_DataSlot()
    {
        var cut = Render<SheetCn>(p => p.AddChildContent("Content"));
        cut.Find("[data-slot='sheet']").Should().NotBeNull();
    }

    [Fact]
    public void Sheet_Starts_Closed_By_Default()
    {
        var cut = Render<SheetCn>(p => p.AddChildContent("Content"));
        cut.Find("[data-slot='sheet']").GetAttribute("data-state").Should().Be("closed");
    }

    [Fact]
    public void Sheet_Starts_Open_When_Open_Is_True()
    {
        var cut = Render<SheetCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent("Content"));
        cut.Find("[data-slot='sheet']").GetAttribute("data-state").Should().Be("open");
    }

    [Fact]
    public void Sheet_Class_Passthrough()
    {
        var cut = Render<SheetCn>(p => p
            .Add(c => c.Class, "custom-class")
            .AddChildContent("Content"));
        cut.Find("[data-slot='sheet']").ClassList.Should().Contain("custom-class");
    }

    [Fact]
    public void Sheet_AdditionalAttributes_Passthrough()
    {
        var cut = Render<SheetCn>(p => p
            .Add(c => c.AdditionalAttributes, new Dictionary<string, object?> { { "id", "my-sheet" } })
            .AddChildContent("Content"));
        cut.Find("[data-slot='sheet']").GetAttribute("id").Should().Be("my-sheet");
    }

    // --- SheetTriggerCn ---

    [Fact]
    public void SheetTrigger_Renders_With_DataSlot()
    {
        var cut = Render<SheetCn>(p => p
            .AddChildContent<SheetTriggerCn>(t => t
                .AddChildContent("Open")));
        cut.Find("[data-slot='sheet-trigger']").Should().NotBeNull();
    }

    [Fact]
    public void SheetTrigger_Opens_Sheet()
    {
        var isOpen = false;
        var cut = Render<SheetCn>(p => p
            .Add(c => c.OpenChanged, EventCallback.Factory.Create<bool>(this, v => isOpen = v))
            .AddChildContent<SheetTriggerCn>(t => t
                .AddChildContent("Open")));
        cut.Find("[data-slot='sheet-trigger']").Click();
        isOpen.Should().BeTrue();
    }

    [Fact]
    public void SheetTrigger_Has_Button_Type()
    {
        var cut = Render<SheetCn>(p => p
            .AddChildContent<SheetTriggerCn>(t => t
                .AddChildContent("Open")));
        cut.Find("[data-slot='sheet-trigger']").GetAttribute("type").Should().Be("button");
    }

    // --- SheetOverlayCn ---

    [Fact]
    public void SheetOverlay_Not_Rendered_When_Closed()
    {
        var cut = Render<SheetCn>(p => p
            .AddChildContent<SheetOverlayCn>());
        cut.FindAll("[data-slot='sheet-overlay']").Should().BeEmpty();
    }

    [Fact]
    public void SheetOverlay_Rendered_When_Open()
    {
        var cut = Render<SheetCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<SheetOverlayCn>());
        cut.Find("[data-slot='sheet-overlay']").Should().NotBeNull();
    }

    [Fact]
    public void SheetOverlay_Has_Default_Classes()
    {
        var cut = Render<SheetCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<SheetOverlayCn>());
        var el = cut.Find("[data-slot='sheet-overlay']");
        el.ClassList.Should().Contain("fixed");
        el.ClassList.Should().Contain("inset-0");
        el.ClassList.Should().Contain("z-50");
    }

    [Fact]
    public void SheetOverlay_Click_Closes_Sheet()
    {
        var isOpen = true;
        var cut = Render<SheetCn>(p => p
            .Add(c => c.Open, true)
            .Add(c => c.OpenChanged, EventCallback.Factory.Create<bool>(this, v => isOpen = v))
            .AddChildContent<SheetOverlayCn>());
        cut.Find("[data-slot='sheet-overlay']").Click();
        isOpen.Should().BeFalse();
    }

    [Fact]
    public void SheetOverlay_Class_Passthrough()
    {
        var cut = Render<SheetCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<SheetOverlayCn>(o => o
                .Add(c => c.Class, "custom-overlay")));
        cut.Find("[data-slot='sheet-overlay']").ClassList.Should().Contain("custom-overlay");
    }

    // --- SheetContentCn ---

    [Fact]
    public void SheetContent_Not_Rendered_When_Closed()
    {
        SetupJsInterop();
        var cut = Render<SheetCn>(p => p
            .AddChildContent<SheetContentCn>(c => c
                .AddChildContent("Body")));
        cut.FindAll("[data-slot='sheet-content']").Should().BeEmpty();
    }

    [Fact]
    public void SheetContent_Rendered_When_Open()
    {
        SetupJsInterop();
        var cut = Render<SheetCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<SheetContentCn>(c => c
                .AddChildContent("Body")));
        cut.Find("[data-slot='sheet-content']").Should().NotBeNull();
    }

    [Fact]
    public void SheetContent_Default_Side_Is_Right()
    {
        SetupJsInterop();
        var cut = Render<SheetCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<SheetContentCn>(c => c
                .AddChildContent("Body")));
        var el = cut.Find("[data-slot='sheet-content']");
        el.GetAttribute("data-side").Should().Be("right");
        el.ClassList.Should().Contain("border-l");
    }

    [Theory]
    [InlineData(SheetSide.Top, "top", "border-b")]
    [InlineData(SheetSide.Right, "right", "border-l")]
    [InlineData(SheetSide.Bottom, "bottom", "border-t")]
    [InlineData(SheetSide.Left, "left", "border-r")]
    public void SheetContent_Renders_Correct_Classes_For_Side(SheetSide side, string expectedDataSide, string expectedBorderClass)
    {
        SetupJsInterop();
        var cut = Render<SheetCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<SheetContentCn>(c => c
                .Add(x => x.Side, side)
                .AddChildContent("Body")));
        var el = cut.Find("[data-slot='sheet-content']");
        el.GetAttribute("data-side").Should().Be(expectedDataSide);
        el.ClassList.Should().Contain(expectedBorderClass);
    }

    [Fact]
    public void SheetContent_Has_Base_Classes()
    {
        SetupJsInterop();
        var cut = Render<SheetCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<SheetContentCn>(c => c
                .AddChildContent("Body")));
        var el = cut.Find("[data-slot='sheet-content']");
        el.ClassList.Should().Contain("fixed");
        el.ClassList.Should().Contain("z-50");
        el.ClassList.Should().Contain("flex");
        el.ClassList.Should().Contain("flex-col");
        el.ClassList.Should().Contain("bg-background");
        el.ClassList.Should().Contain("shadow-lg");
    }

    [Fact]
    public void SheetContent_Has_Close_Button()
    {
        SetupJsInterop();
        var cut = Render<SheetCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<SheetContentCn>(c => c
                .AddChildContent("Body")));
        cut.FindAll("[data-slot='sheet-close']").Should().NotBeEmpty();
    }

    [Fact]
    public void SheetContent_Close_Button_Closes_Sheet()
    {
        SetupJsInterop();
        var isOpen = true;
        var cut = Render<SheetCn>(p => p
            .Add(c => c.Open, true)
            .Add(c => c.OpenChanged, EventCallback.Factory.Create<bool>(this, v => isOpen = v))
            .AddChildContent<SheetContentCn>(c => c
                .AddChildContent("Body")));
        cut.Find("[data-slot='sheet-content'] [data-slot='sheet-close']").Click();
        isOpen.Should().BeFalse();
    }

    [Fact]
    public void SheetContent_Class_Passthrough()
    {
        SetupJsInterop();
        var cut = Render<SheetCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<SheetContentCn>(c => c
                .Add(x => x.Class, "custom-content")
                .AddChildContent("Body")));
        cut.Find("[data-slot='sheet-content']").ClassList.Should().Contain("custom-content");
    }

    [Fact]
    public void SheetContent_AdditionalAttributes_Passthrough()
    {
        SetupJsInterop();
        var cut = Render<SheetCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<SheetContentCn>(c => c
                .Add(x => x.AdditionalAttributes, new Dictionary<string, object?> { { "role", "dialog" } })
                .AddChildContent("Body")));
        cut.Find("[data-slot='sheet-content']").GetAttribute("role").Should().Be("dialog");
    }

    // --- SheetHeaderCn ---

    [Fact]
    public void SheetHeader_Renders_With_DataSlot()
    {
        var cut = Render<SheetHeaderCn>(p => p.AddChildContent("Header"));
        cut.Find("[data-slot='sheet-header']").Should().NotBeNull();
    }

    [Fact]
    public void SheetHeader_Has_Default_Classes()
    {
        var cut = Render<SheetHeaderCn>(p => p.AddChildContent("Header"));
        var el = cut.Find("[data-slot='sheet-header']");
        el.ClassList.Should().Contain("flex");
        el.ClassList.Should().Contain("flex-col");
        el.ClassList.Should().Contain("gap-2");
    }

    [Fact]
    public void SheetHeader_Class_Passthrough()
    {
        var cut = Render<SheetHeaderCn>(p => p
            .Add(c => c.Class, "custom-header")
            .AddChildContent("Header"));
        cut.Find("[data-slot='sheet-header']").ClassList.Should().Contain("custom-header");
    }

    // --- SheetFooterCn ---

    [Fact]
    public void SheetFooter_Renders_With_DataSlot()
    {
        var cut = Render<SheetFooterCn>(p => p.AddChildContent("Footer"));
        cut.Find("[data-slot='sheet-footer']").Should().NotBeNull();
    }

    [Fact]
    public void SheetFooter_Has_Default_Classes()
    {
        var cut = Render<SheetFooterCn>(p => p.AddChildContent("Footer"));
        var el = cut.Find("[data-slot='sheet-footer']");
        el.ClassList.Should().Contain("mt-auto");
        el.ClassList.Should().Contain("flex");
        el.ClassList.Should().Contain("flex-col");
        el.ClassList.Should().Contain("gap-2");
    }

    [Fact]
    public void SheetFooter_Class_Passthrough()
    {
        var cut = Render<SheetFooterCn>(p => p
            .Add(c => c.Class, "custom-footer")
            .AddChildContent("Footer"));
        cut.Find("[data-slot='sheet-footer']").ClassList.Should().Contain("custom-footer");
    }

    // --- SheetTitleCn ---

    [Fact]
    public void SheetTitle_Renders_With_DataSlot()
    {
        var cut = Render<SheetTitleCn>(p => p.AddChildContent("Title"));
        cut.Find("[data-slot='sheet-title']").Should().NotBeNull();
    }

    [Fact]
    public void SheetTitle_Has_Default_Classes()
    {
        var cut = Render<SheetTitleCn>(p => p.AddChildContent("Title"));
        var el = cut.Find("[data-slot='sheet-title']");
        el.ClassList.Should().Contain("text-lg");
        el.ClassList.Should().Contain("font-semibold");
    }

    [Fact]
    public void SheetTitle_Class_Passthrough()
    {
        var cut = Render<SheetTitleCn>(p => p
            .Add(c => c.Class, "custom-title")
            .AddChildContent("Title"));
        cut.Find("[data-slot='sheet-title']").ClassList.Should().Contain("custom-title");
    }

    // --- SheetDescriptionCn ---

    [Fact]
    public void SheetDescription_Renders_With_DataSlot()
    {
        var cut = Render<SheetDescriptionCn>(p => p.AddChildContent("Description"));
        cut.Find("[data-slot='sheet-description']").Should().NotBeNull();
    }

    [Fact]
    public void SheetDescription_Has_Default_Classes()
    {
        var cut = Render<SheetDescriptionCn>(p => p.AddChildContent("Description"));
        var el = cut.Find("[data-slot='sheet-description']");
        el.ClassList.Should().Contain("text-sm");
        el.ClassList.Should().Contain("text-muted-foreground");
    }

    [Fact]
    public void SheetDescription_Class_Passthrough()
    {
        var cut = Render<SheetDescriptionCn>(p => p
            .Add(c => c.Class, "custom-desc")
            .AddChildContent("Description"));
        cut.Find("[data-slot='sheet-description']").ClassList.Should().Contain("custom-desc");
    }

    // --- SheetCloseCn ---

    [Fact]
    public void SheetClose_Renders_With_DataSlot()
    {
        var cut = Render<SheetCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<SheetCloseCn>(c => c
                .AddChildContent("Close")));
        cut.Find("[data-slot='sheet-close']").Should().NotBeNull();
    }

    [Fact]
    public void SheetClose_Closes_Sheet()
    {
        var isOpen = true;
        var cut = Render<SheetCn>(p => p
            .Add(c => c.Open, true)
            .Add(c => c.OpenChanged, EventCallback.Factory.Create<bool>(this, v => isOpen = v))
            .AddChildContent<SheetCloseCn>(c => c
                .AddChildContent("Close")));
        cut.Find("[data-slot='sheet-close']").Click();
        isOpen.Should().BeFalse();
    }

    [Fact]
    public void SheetClose_Has_Button_Type()
    {
        var cut = Render<SheetCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<SheetCloseCn>(c => c
                .AddChildContent("Close")));
        cut.Find("[data-slot='sheet-close']").GetAttribute("type").Should().Be("button");
    }

    // --- Integration ---

    [Fact]
    public void Sheet_Full_Integration()
    {
        SetupJsInterop();
        var isOpen = false;
        var cut = Render<SheetCn>(p => p
            .Add(c => c.OpenChanged, EventCallback.Factory.Create<bool>(this, v => isOpen = v))
            .AddChildContent<SheetTriggerCn>(t => t
                .AddChildContent("Open Sheet")));

        cut.Find("[data-slot='sheet']").GetAttribute("data-state").Should().Be("closed");
        cut.Find("[data-slot='sheet-trigger']").Click();
        isOpen.Should().BeTrue();
        cut.Find("[data-slot='sheet']").GetAttribute("data-state").Should().Be("open");
    }
}

using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BlazorCN.Tests.Components;

public class PopoverCnTests : BunitContext
{
    private void SetupJsInterop()
    {
        var module = JSInterop.SetupModule("./_content/BlazorCN/blazorcn.js");
        module.Setup<string>("createFloating", _ => true).SetResult("bottom");
        module.SetupVoid("onOutsideClick", _ => true).SetVoidResult();
        module.SetupVoid("destroyFloating", _ => true).SetVoidResult();
        module.SetupVoid("cleanup", _ => true).SetVoidResult();
        Services.AddScoped<JsInteropCn>();
    }

    // --- PopoverCn ---

    [Fact]
    public void Popover_Renders_With_DataSlot()
    {
        var cut = Render<PopoverCn>(p => p.AddChildContent("Content"));
        cut.Find("[data-slot='popover']").Should().NotBeNull();
    }

    [Fact]
    public void Popover_Starts_Closed_By_Default()
    {
        var cut = Render<PopoverCn>(p => p.AddChildContent("Content"));
        cut.Find("[data-slot='popover']").GetAttribute("data-state").Should().Be("closed");
    }

    [Fact]
    public void Popover_Starts_Open_When_Open_Is_True()
    {
        var cut = Render<PopoverCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent("Content"));
        cut.Find("[data-slot='popover']").GetAttribute("data-state").Should().Be("open");
    }

    [Fact]
    public void Popover_Has_Default_Classes()
    {
        var cut = Render<PopoverCn>(p => p.AddChildContent("Content"));
        var el = cut.Find("[data-slot='popover']");
        el.ClassList.Should().Contain("relative");
        el.ClassList.Should().Contain("inline-block");
    }

    [Fact]
    public void Popover_Class_Passthrough()
    {
        var cut = Render<PopoverCn>(p => p
            .Add(c => c.Class, "custom-class")
            .AddChildContent("Content"));
        cut.Find("[data-slot='popover']").ClassList.Should().Contain("custom-class");
    }

    [Fact]
    public void Popover_AdditionalAttributes_Passthrough()
    {
        var cut = Render<PopoverCn>(p => p
            .Add(c => c.AdditionalAttributes, new Dictionary<string, object?> { { "id", "my-popover" } })
            .AddChildContent("Content"));
        cut.Find("[data-slot='popover']").GetAttribute("id").Should().Be("my-popover");
    }

    // --- PopoverTriggerCn ---

    [Fact]
    public void PopoverTrigger_Renders_With_DataSlot()
    {
        var cut = Render<PopoverCn>(p => p
            .AddChildContent<PopoverTriggerCn>(t => t
                .AddChildContent("Open")));
        cut.Find("[data-slot='popover-trigger']").Should().NotBeNull();
    }

    [Fact]
    public void PopoverTrigger_Click_Toggles_Open()
    {
        var isOpen = false;
        var cut = Render<PopoverCn>(p => p
            .Add(c => c.OpenChanged, EventCallback.Factory.Create<bool>(this, v => isOpen = v))
            .AddChildContent<PopoverTriggerCn>(t => t
                .AddChildContent("Open")));
        cut.Find("[data-slot='popover-trigger']").Click();
        isOpen.Should().BeTrue();
        cut.Find("[data-slot='popover']").GetAttribute("data-state").Should().Be("open");
    }

    [Fact]
    public void PopoverTrigger_Click_Toggles_Closed()
    {
        var isOpen = true;
        var cut = Render<PopoverCn>(p => p
            .Add(c => c.Open, true)
            .Add(c => c.OpenChanged, EventCallback.Factory.Create<bool>(this, v => isOpen = v))
            .AddChildContent<PopoverTriggerCn>(t => t
                .AddChildContent("Close")));
        cut.Find("[data-slot='popover-trigger']").Click();
        isOpen.Should().BeFalse();
    }

    [Fact]
    public void PopoverTrigger_Class_Passthrough()
    {
        var cut = Render<PopoverCn>(p => p
            .AddChildContent<PopoverTriggerCn>(t => t
                .Add(c => c.Class, "trigger-class")
                .AddChildContent("Open")));
        cut.Find("[data-slot='popover-trigger']").ClassList.Should().Contain("trigger-class");
    }

    [Fact]
    public void PopoverTrigger_AdditionalAttributes_Passthrough()
    {
        var cut = Render<PopoverCn>(p => p
            .AddChildContent<PopoverTriggerCn>(t => t
                .Add(c => c.AdditionalAttributes, new Dictionary<string, object?> { { "aria-label", "toggle" } })
                .AddChildContent("Open")));
        cut.Find("[data-slot='popover-trigger']").GetAttribute("aria-label").Should().Be("toggle");
    }

    // --- PopoverContentCn ---

    [Fact]
    public void PopoverContent_Not_Rendered_When_Closed()
    {
        SetupJsInterop();
        var cut = Render<PopoverCn>(p => p
            .AddChildContent<PopoverContentCn>(c => c
                .AddChildContent("Popover body")));
        cut.FindAll("[data-slot='popover-content']").Should().BeEmpty();
    }

    [Fact]
    public void PopoverContent_Rendered_When_Open()
    {
        SetupJsInterop();
        var cut = Render<PopoverCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<PopoverContentCn>(c => c
                .AddChildContent("Popover body")));
        cut.Find("[data-slot='popover-content']").Should().NotBeNull();
        cut.Find("[data-slot='popover-content']").TextContent.Should().Contain("Popover body");
    }

    [Fact]
    public void PopoverContent_Has_Default_Classes()
    {
        SetupJsInterop();
        var cut = Render<PopoverCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<PopoverContentCn>(c => c
                .AddChildContent("Body")));
        var content = cut.Find("[data-slot='popover-content']");
        content.ClassList.Should().Contain("cn-popover-content");
        content.ClassList.Should().Contain("z-50");
        content.ClassList.Should().Contain("w-72");
        content.ClassList.Should().Contain("outline-hidden");
    }

    [Fact]
    public void PopoverContent_Class_Passthrough()
    {
        SetupJsInterop();
        var cut = Render<PopoverCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<PopoverContentCn>(c => c
                .Add(x => x.Class, "custom-content")
                .AddChildContent("Body")));
        cut.Find("[data-slot='popover-content']").ClassList.Should().Contain("custom-content");
    }

    [Fact]
    public void PopoverContent_AdditionalAttributes_Passthrough()
    {
        SetupJsInterop();
        var cut = Render<PopoverCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<PopoverContentCn>(c => c
                .Add(x => x.AdditionalAttributes, new Dictionary<string, object?> { { "role", "dialog" } })
                .AddChildContent("Body")));
        cut.Find("[data-slot='popover-content']").GetAttribute("role").Should().Be("dialog");
    }

    [Fact]
    public void PopoverContent_Default_Side_Is_Bottom()
    {
        SetupJsInterop();
        var cut = Render<PopoverCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<PopoverContentCn>(c => c
                .AddChildContent("Body")));
        cut.Find("[data-slot='popover-content']").GetAttribute("data-side").Should().Be("bottom");
    }

    [Fact]
    public void PopoverContent_Custom_Side()
    {
        SetupJsInterop();
        var cut = Render<PopoverCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<PopoverContentCn>(c => c
                .Add(x => x.Side, FloatingSide.Top)
                .AddChildContent("Body")));
        cut.Find("[data-slot='popover-content']").GetAttribute("data-side").Should().Be("top");
    }

    [Fact]
    public void PopoverContent_Default_Align_Is_Center()
    {
        SetupJsInterop();
        var cut = Render<PopoverCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<PopoverContentCn>(c => c
                .AddChildContent("Body")));
        cut.Find("[data-slot='popover-content']").GetAttribute("data-align").Should().Be("center");
    }

    [Fact]
    public void PopoverContent_Custom_Align()
    {
        SetupJsInterop();
        var cut = Render<PopoverCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<PopoverContentCn>(c => c
                .Add(x => x.Align, FloatingAlign.Start)
                .AddChildContent("Body")));
        cut.Find("[data-slot='popover-content']").GetAttribute("data-align").Should().Be("start");
    }

    // --- PopoverHeaderCn ---

    [Fact]
    public void PopoverHeader_Renders_With_DataSlot()
    {
        var cut = Render<PopoverHeaderCn>(p => p.AddChildContent("Header"));
        cut.Find("[data-slot='popover-header']").Should().NotBeNull();
    }

    [Fact]
    public void PopoverHeader_Has_Default_Classes()
    {
        // Layout moved from inline utilities to the nova cn-popover-header rule
        // (flex flex-col gap-0.5 text-sm via @apply).
        var cut = Render<PopoverHeaderCn>(p => p.AddChildContent("Header"));
        var el = cut.Find("[data-slot='popover-header']");
        el.ClassList.Should().Contain("cn-popover-header");
    }

    [Fact]
    public void PopoverHeader_Class_Passthrough()
    {
        var cut = Render<PopoverHeaderCn>(p => p
            .Add(c => c.Class, "custom-header")
            .AddChildContent("Header"));
        cut.Find("[data-slot='popover-header']").ClassList.Should().Contain("custom-header");
    }

    [Fact]
    public void PopoverHeader_AdditionalAttributes_Passthrough()
    {
        var cut = Render<PopoverHeaderCn>(p => p
            .Add(c => c.AdditionalAttributes, new Dictionary<string, object?> { { "id", "hdr" } })
            .AddChildContent("Header"));
        cut.Find("[data-slot='popover-header']").GetAttribute("id").Should().Be("hdr");
    }

    // --- PopoverTitleCn ---

    [Fact]
    public void PopoverTitle_Renders_With_DataSlot()
    {
        var cut = Render<PopoverTitleCn>(p => p.AddChildContent("Title"));
        cut.Find("[data-slot='popover-title']").Should().NotBeNull();
    }

    [Fact]
    public void PopoverTitle_Has_Default_Classes()
    {
        var cut = Render<PopoverTitleCn>(p => p.AddChildContent("Title"));
        var el = cut.Find("[data-slot='popover-title']");
        el.ClassList.Should().Contain("cn-popover-title");
        el.ClassList.Should().Contain("text-sm");
    }

    [Fact]
    public void PopoverTitle_Class_Passthrough()
    {
        var cut = Render<PopoverTitleCn>(p => p
            .Add(c => c.Class, "custom-title")
            .AddChildContent("Title"));
        cut.Find("[data-slot='popover-title']").ClassList.Should().Contain("custom-title");
    }

    [Fact]
    public void PopoverTitle_AdditionalAttributes_Passthrough()
    {
        var cut = Render<PopoverTitleCn>(p => p
            .Add(c => c.AdditionalAttributes, new Dictionary<string, object?> { { "id", "ttl" } })
            .AddChildContent("Title"));
        cut.Find("[data-slot='popover-title']").GetAttribute("id").Should().Be("ttl");
    }

    // --- PopoverDescriptionCn ---

    [Fact]
    public void PopoverDescription_Renders_With_DataSlot()
    {
        var cut = Render<PopoverDescriptionCn>(p => p.AddChildContent("Description"));
        cut.Find("[data-slot='popover-description']").Should().NotBeNull();
    }

    [Fact]
    public void PopoverDescription_Has_Default_Classes()
    {
        var cut = Render<PopoverDescriptionCn>(p => p.AddChildContent("Description"));
        var el = cut.Find("[data-slot='popover-description']");
        el.ClassList.Should().Contain("cn-popover-description");
        el.ClassList.Should().Contain("text-sm");
    }

    [Fact]
    public void PopoverDescription_Class_Passthrough()
    {
        var cut = Render<PopoverDescriptionCn>(p => p
            .Add(c => c.Class, "custom-desc")
            .AddChildContent("Description"));
        cut.Find("[data-slot='popover-description']").ClassList.Should().Contain("custom-desc");
    }

    [Fact]
    public void PopoverDescription_AdditionalAttributes_Passthrough()
    {
        var cut = Render<PopoverDescriptionCn>(p => p
            .Add(c => c.AdditionalAttributes, new Dictionary<string, object?> { { "id", "desc" } })
            .AddChildContent("Description"));
        cut.Find("[data-slot='popover-description']").GetAttribute("id").Should().Be("desc");
    }

    // --- ARIA ---

    [Fact]
    public void PopoverTrigger_AriaExpanded_Reflects_State()
    {
        var cut = Render<PopoverCn>(p => p
            .AddChildContent<PopoverTriggerCn>(t => t
                .AddChildContent("Open")));
        var trigger = cut.Find("[data-slot='popover-trigger']");
        trigger.GetAttribute("aria-expanded").Should().Be("false");
        trigger.Click();
        trigger.GetAttribute("aria-expanded").Should().Be("true");
    }

    // --- Integration ---

    [Fact]
    public void Popover_Full_Integration_Toggle()
    {
        SetupJsInterop();
        var isOpen = false;
        var cut = Render<PopoverCn>(p => p
            .Add(c => c.OpenChanged, EventCallback.Factory.Create<bool>(this, v => isOpen = v))
            .AddChildContent<PopoverTriggerCn>(t => t
                .AddChildContent("Toggle")));

        // Initially closed
        cut.Find("[data-slot='popover']").GetAttribute("data-state").Should().Be("closed");

        // Click trigger to open
        cut.Find("[data-slot='popover-trigger']").Click();
        isOpen.Should().BeTrue();
        cut.Find("[data-slot='popover']").GetAttribute("data-state").Should().Be("open");

        // Click trigger again to close
        cut.Find("[data-slot='popover-trigger']").Click();
        isOpen.Should().BeFalse();
        cut.Find("[data-slot='popover']").GetAttribute("data-state").Should().Be("closed");
    }
}

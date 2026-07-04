using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BlazorCN.Tests.Components;

public class TooltipCnTests : BunitContext
{
    private void SetupJsInterop()
    {
        var module = JSInterop.SetupModule("./_content/BlazorCN/blazorcn.js");
        module.Setup<string>("createFloating", _ => true).SetResult("top");
        module.SetupVoid("destroyFloating", _ => true).SetVoidResult();
        Services.AddScoped<JsInteropCn>();
    }

    // --- TooltipCn ---

    [Fact]
    public void Tooltip_Renders_With_DataSlot()
    {
        var cut = Render<TooltipCn>(p => p.AddChildContent("Content"));
        cut.Find("[data-slot='tooltip']").Should().NotBeNull();
    }

    [Fact]
    public void Tooltip_Starts_Closed_By_Default()
    {
        var cut = Render<TooltipCn>(p => p.AddChildContent("Content"));
        cut.Find("[data-slot='tooltip']").GetAttribute("data-state").Should().Be("closed");
    }

    [Fact]
    public void Tooltip_Has_Default_Classes()
    {
        var cut = Render<TooltipCn>(p => p.AddChildContent("Content"));
        var el = cut.Find("[data-slot='tooltip']");
        el.ClassList.Should().Contain("relative");
        el.ClassList.Should().Contain("inline-block");
    }

    [Fact]
    public void Tooltip_Class_Passthrough()
    {
        var cut = Render<TooltipCn>(p => p
            .Add(c => c.Class, "custom-tooltip")
            .AddChildContent("Content"));
        cut.Find("[data-slot='tooltip']").ClassList.Should().Contain("custom-tooltip");
    }

    [Fact]
    public void Tooltip_AdditionalAttributes_Passthrough()
    {
        var cut = Render<TooltipCn>(p => p
            .Add(c => c.AdditionalAttributes, new Dictionary<string, object?> { { "id", "my-tooltip" } })
            .AddChildContent("Content"));
        cut.Find("[data-slot='tooltip']").GetAttribute("id").Should().Be("my-tooltip");
    }

    // --- TooltipTriggerCn ---

    [Fact]
    public void TooltipTrigger_Renders_With_DataSlot()
    {
        var cut = Render<TooltipCn>(p => p
            .Add(c => c.OpenDelay, 0)
            .AddChildContent<TooltipTriggerCn>(t => t
                .AddChildContent("Hover me")));
        cut.Find("[data-slot='tooltip-trigger']").Should().NotBeNull();
    }

    [Fact]
    public void TooltipTrigger_Hover_Opens_Tooltip()
    {
        var cut = Render<TooltipCn>(p => p
            .Add(c => c.OpenDelay, 0)
            .AddChildContent<TooltipTriggerCn>(t => t
                .AddChildContent("Hover me")));

        cut.Find("[data-slot='tooltip-trigger']").TriggerEvent("onmouseenter", new MouseEventArgs());
        cut.WaitForAssertion(() => cut.Find("[data-slot='tooltip']").GetAttribute("data-state").Should().Be("open"));
        cut.Find("[data-slot='tooltip']").GetAttribute("data-state").Should().Be("open");
    }

    [Fact]
    public void TooltipTrigger_MouseLeave_Closes_Tooltip()
    {
        var cut = Render<TooltipCn>(p => p
            .Add(c => c.OpenDelay, 0)
            .AddChildContent<TooltipTriggerCn>(t => t
                .AddChildContent("Hover me")));

        cut.Find("[data-slot='tooltip-trigger']").TriggerEvent("onmouseenter", new MouseEventArgs());
        cut.WaitForAssertion(() => cut.Find("[data-slot='tooltip']").GetAttribute("data-state").Should().Be("open"));
        cut.Find("[data-slot='tooltip']").GetAttribute("data-state").Should().Be("open");

        cut.Find("[data-slot='tooltip-trigger']").TriggerEvent("onmouseleave", new MouseEventArgs());
        cut.Find("[data-slot='tooltip']").GetAttribute("data-state").Should().Be("closed");
    }

    [Fact]
    public void TooltipTrigger_Focus_Opens_Tooltip()
    {
        var cut = Render<TooltipCn>(p => p
            .Add(c => c.OpenDelay, 0)
            .AddChildContent<TooltipTriggerCn>(t => t
                .AddChildContent("Focus me")));

        cut.Find("[data-slot='tooltip-trigger']").TriggerEvent("onfocus", new FocusEventArgs());
        cut.WaitForAssertion(() => cut.Find("[data-slot='tooltip']").GetAttribute("data-state").Should().Be("open"));
        cut.Find("[data-slot='tooltip']").GetAttribute("data-state").Should().Be("open");
    }

    [Fact]
    public void TooltipTrigger_Blur_Closes_Tooltip()
    {
        var cut = Render<TooltipCn>(p => p
            .Add(c => c.OpenDelay, 0)
            .AddChildContent<TooltipTriggerCn>(t => t
                .AddChildContent("Focus me")));

        cut.Find("[data-slot='tooltip-trigger']").TriggerEvent("onfocus", new FocusEventArgs());
        cut.WaitForAssertion(() => cut.Find("[data-slot='tooltip']").GetAttribute("data-state").Should().Be("open"));
        cut.Find("[data-slot='tooltip']").GetAttribute("data-state").Should().Be("open");

        cut.Find("[data-slot='tooltip-trigger']").TriggerEvent("onblur", new FocusEventArgs());
        cut.Find("[data-slot='tooltip']").GetAttribute("data-state").Should().Be("closed");
    }

    [Fact]
    public void TooltipTrigger_Class_Passthrough()
    {
        var cut = Render<TooltipCn>(p => p
            .Add(c => c.OpenDelay, 0)
            .AddChildContent<TooltipTriggerCn>(t => t
                .Add(c => c.Class, "trigger-class")
                .AddChildContent("Hover")));
        cut.Find("[data-slot='tooltip-trigger']").ClassList.Should().Contain("trigger-class");
    }

    [Fact]
    public void TooltipTrigger_AdditionalAttributes_Passthrough()
    {
        var cut = Render<TooltipCn>(p => p
            .Add(c => c.OpenDelay, 0)
            .AddChildContent<TooltipTriggerCn>(t => t
                .Add(c => c.AdditionalAttributes, new Dictionary<string, object?> { { "aria-label", "info" } })
                .AddChildContent("Hover")));
        cut.Find("[data-slot='tooltip-trigger']").GetAttribute("aria-label").Should().Be("info");
    }

    // --- TooltipContentCn ---

    [Fact]
    public void TooltipContent_Not_Rendered_When_Closed()
    {
        SetupJsInterop();
        var cut = Render<TooltipCn>(p => p
            .AddChildContent<TooltipContentCn>(c => c
                .AddChildContent("Tooltip text")));
        cut.FindAll("[data-slot='tooltip-content']").Should().BeEmpty();
    }

    [Fact]
    public void TooltipContent_Rendered_When_Open()
    {
        SetupJsInterop();
        var cut = Render<TooltipCn>(p => p
            .Add(c => c.OpenDelay, 0)
            .AddChildContent<TooltipTriggerCn>(t => t
                .AddChildContent("Hover")));

        // Need to re-render with content after opening
        // Actually, let's build a full tooltip that includes both trigger and content
        var cut2 = Render<TooltipCn>(p => p.Add(c => c.OpenDelay, 0).AddChildContent(builder =>
        {
            builder.OpenComponent<TooltipTriggerCn>(0);
            builder.AddAttribute(1, "ChildContent", (RenderFragment)(b => b.AddContent(0, "Hover me")));
            builder.CloseComponent();
            builder.OpenComponent<TooltipContentCn>(2);
            builder.AddAttribute(3, "ChildContent", (RenderFragment)(b => b.AddContent(0, "Tooltip text")));
            builder.CloseComponent();
        }));

        // Open via hover
        cut2.Find("[data-slot='tooltip-trigger']").TriggerEvent("onmouseenter", new MouseEventArgs());
        cut2.WaitForAssertion(() => cut2.Find("[data-slot='tooltip-content']").Should().NotBeNull());
        cut2.Find("[data-slot='tooltip-content']").TextContent.Should().Contain("Tooltip text");
    }

    [Fact]
    public void TooltipContent_Has_Default_Classes()
    {
        SetupJsInterop();
        var cut = Render<TooltipCn>(p => p.Add(c => c.OpenDelay, 0).AddChildContent(builder =>
        {
            builder.OpenComponent<TooltipTriggerCn>(0);
            builder.AddAttribute(1, "ChildContent", (RenderFragment)(b => b.AddContent(0, "Hover")));
            builder.CloseComponent();
            builder.OpenComponent<TooltipContentCn>(2);
            builder.AddAttribute(3, "ChildContent", (RenderFragment)(b => b.AddContent(0, "Tip")));
            builder.CloseComponent();
        }));

        cut.Find("[data-slot='tooltip-trigger']").TriggerEvent("onmouseenter", new MouseEventArgs());
        cut.WaitForAssertion(() => cut.Find("[data-slot='tooltip']").GetAttribute("data-state").Should().Be("open"));
        var content = cut.Find("[data-slot='tooltip-content']");
        content.ClassList.Should().Contain("cn-tooltip-content");
        content.ClassList.Should().Contain("z-50");
        content.ClassList.Should().Contain("w-fit");
        content.ClassList.Should().Contain("bg-foreground");
        content.ClassList.Should().Contain("text-balance");
        content.ClassList.Should().Contain("text-background");
    }

    [Fact]
    public void TooltipContent_Default_Side_Is_Top()
    {
        SetupJsInterop();
        var cut = Render<TooltipCn>(p => p.Add(c => c.OpenDelay, 0).AddChildContent(builder =>
        {
            builder.OpenComponent<TooltipTriggerCn>(0);
            builder.AddAttribute(1, "ChildContent", (RenderFragment)(b => b.AddContent(0, "Hover")));
            builder.CloseComponent();
            builder.OpenComponent<TooltipContentCn>(2);
            builder.AddAttribute(3, "ChildContent", (RenderFragment)(b => b.AddContent(0, "Tip")));
            builder.CloseComponent();
        }));

        cut.Find("[data-slot='tooltip-trigger']").TriggerEvent("onmouseenter", new MouseEventArgs());
        cut.WaitForAssertion(() => cut.Find("[data-slot='tooltip']").GetAttribute("data-state").Should().Be("open"));
        cut.Find("[data-slot='tooltip-content']").GetAttribute("data-side").Should().Be("top");
    }

    [Fact]
    public void TooltipContent_Custom_Side()
    {
        SetupJsInterop();
        var cut = Render<TooltipCn>(p => p.Add(c => c.OpenDelay, 0).AddChildContent(builder =>
        {
            builder.OpenComponent<TooltipTriggerCn>(0);
            builder.AddAttribute(1, "ChildContent", (RenderFragment)(b => b.AddContent(0, "Hover")));
            builder.CloseComponent();
            builder.OpenComponent<TooltipContentCn>(2);
            builder.AddAttribute(3, "ChildContent", (RenderFragment)(b => b.AddContent(0, "Tip")));
            builder.AddAttribute(4, "Side", FloatingSide.Bottom);
            builder.CloseComponent();
        }));

        cut.Find("[data-slot='tooltip-trigger']").TriggerEvent("onmouseenter", new MouseEventArgs());
        cut.WaitForAssertion(() => cut.Find("[data-slot='tooltip']").GetAttribute("data-state").Should().Be("open"));
        cut.Find("[data-slot='tooltip-content']").GetAttribute("data-side").Should().Be("bottom");
    }

    [Fact]
    public void TooltipContent_Default_Align_Is_Center()
    {
        SetupJsInterop();
        var cut = Render<TooltipCn>(p => p.Add(c => c.OpenDelay, 0).AddChildContent(builder =>
        {
            builder.OpenComponent<TooltipTriggerCn>(0);
            builder.AddAttribute(1, "ChildContent", (RenderFragment)(b => b.AddContent(0, "Hover")));
            builder.CloseComponent();
            builder.OpenComponent<TooltipContentCn>(2);
            builder.AddAttribute(3, "ChildContent", (RenderFragment)(b => b.AddContent(0, "Tip")));
            builder.CloseComponent();
        }));

        cut.Find("[data-slot='tooltip-trigger']").TriggerEvent("onmouseenter", new MouseEventArgs());
        cut.WaitForAssertion(() => cut.Find("[data-slot='tooltip']").GetAttribute("data-state").Should().Be("open"));
        cut.Find("[data-slot='tooltip-content']").GetAttribute("data-align").Should().Be("center");
    }

    [Fact]
    public void TooltipContent_Class_Passthrough()
    {
        SetupJsInterop();
        var cut = Render<TooltipCn>(p => p.Add(c => c.OpenDelay, 0).AddChildContent(builder =>
        {
            builder.OpenComponent<TooltipTriggerCn>(0);
            builder.AddAttribute(1, "ChildContent", (RenderFragment)(b => b.AddContent(0, "Hover")));
            builder.CloseComponent();
            builder.OpenComponent<TooltipContentCn>(2);
            builder.AddAttribute(3, "ChildContent", (RenderFragment)(b => b.AddContent(0, "Tip")));
            builder.AddAttribute(4, "Class", "custom-tip");
            builder.CloseComponent();
        }));

        cut.Find("[data-slot='tooltip-trigger']").TriggerEvent("onmouseenter", new MouseEventArgs());
        cut.WaitForAssertion(() => cut.Find("[data-slot='tooltip']").GetAttribute("data-state").Should().Be("open"));
        cut.Find("[data-slot='tooltip-content']").ClassList.Should().Contain("custom-tip");
    }

    [Fact]
    public void TooltipContent_AdditionalAttributes_Passthrough()
    {
        SetupJsInterop();
        var cut = Render<TooltipCn>(p => p.Add(c => c.OpenDelay, 0).AddChildContent(builder =>
        {
            builder.OpenComponent<TooltipTriggerCn>(0);
            builder.AddAttribute(1, "ChildContent", (RenderFragment)(b => b.AddContent(0, "Hover")));
            builder.CloseComponent();
            builder.OpenComponent<TooltipContentCn>(2);
            builder.AddAttribute(3, "ChildContent", (RenderFragment)(b => b.AddContent(0, "Tip")));
            builder.AddAttribute(4, "AdditionalAttributes", new Dictionary<string, object?> { { "role", "tooltip" } });
            builder.CloseComponent();
        }));

        cut.Find("[data-slot='tooltip-trigger']").TriggerEvent("onmouseenter", new MouseEventArgs());
        cut.WaitForAssertion(() => cut.Find("[data-slot='tooltip']").GetAttribute("data-state").Should().Be("open"));
        cut.Find("[data-slot='tooltip-content']").GetAttribute("role").Should().Be("tooltip");
    }
}

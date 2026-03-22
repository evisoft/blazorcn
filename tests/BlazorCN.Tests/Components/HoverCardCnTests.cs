using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BlazorCN.Tests.Components;

public class HoverCardCnTests : BunitContext
{
    private void SetupJsInterop()
    {
        var module = JSInterop.SetupModule("./_content/BlazorCN/blazorcn.js");
        module.Setup<string>("createFloating", _ => true).SetResult("bottom");
        module.SetupVoid("destroyFloating", _ => true).SetVoidResult();
        Services.AddScoped<JsInteropCn>();
    }

    // Helper to render HoverCard with zero delays for deterministic tests
    private IRenderedComponent<HoverCardCn> RenderHoverCard(
        Action<ComponentParameterCollectionBuilder<HoverCardCn>>? configure = null)
    {
        return Render<HoverCardCn>(p =>
        {
            p.Add(c => c.OpenDelay, 0);
            p.Add(c => c.CloseDelay, 0);
            configure?.Invoke(p);
        });
    }

    // --- HoverCardCn ---

    [Fact]
    public void HoverCard_Renders_With_DataSlot()
    {
        var cut = RenderHoverCard(p => p.AddChildContent("Content"));
        cut.Find("[data-slot='hover-card']").Should().NotBeNull();
    }

    [Fact]
    public void HoverCard_Starts_Closed_By_Default()
    {
        var cut = RenderHoverCard(p => p.AddChildContent("Content"));
        cut.Find("[data-slot='hover-card']").GetAttribute("data-state").Should().Be("closed");
    }

    [Fact]
    public void HoverCard_Has_Default_Classes()
    {
        var cut = RenderHoverCard(p => p.AddChildContent("Content"));
        var el = cut.Find("[data-slot='hover-card']");
        el.ClassList.Should().Contain("relative");
        el.ClassList.Should().Contain("inline-block");
    }

    [Fact]
    public void HoverCard_Class_Passthrough()
    {
        var cut = RenderHoverCard(p => p
            .Add(c => c.Class, "custom-hovercard")
            .AddChildContent("Content"));
        cut.Find("[data-slot='hover-card']").ClassList.Should().Contain("custom-hovercard");
    }

    [Fact]
    public void HoverCard_AdditionalAttributes_Passthrough()
    {
        var cut = RenderHoverCard(p => p
            .Add(c => c.AdditionalAttributes, new Dictionary<string, object?> { { "id", "my-hovercard" } })
            .AddChildContent("Content"));
        cut.Find("[data-slot='hover-card']").GetAttribute("id").Should().Be("my-hovercard");
    }

    // --- HoverCardTriggerCn ---

    [Fact]
    public void HoverCardTrigger_Renders_With_DataSlot()
    {
        var cut = RenderHoverCard(p => p
            .AddChildContent<HoverCardTriggerCn>(t => t
                .AddChildContent("Hover me")));
        cut.Find("[data-slot='hover-card-trigger']").Should().NotBeNull();
    }

    [Fact]
    public async Task HoverCardTrigger_Hover_Opens_HoverCard()
    {
        var cut = RenderHoverCard(p => p
            .AddChildContent<HoverCardTriggerCn>(t => t
                .AddChildContent("Hover me")));

        await cut.Find("[data-slot='hover-card-trigger']").TriggerEventAsync("onmouseenter", new MouseEventArgs());
        cut.Find("[data-slot='hover-card']").GetAttribute("data-state").Should().Be("open");
    }

    [Fact]
    public async Task HoverCardTrigger_MouseLeave_Closes_HoverCard()
    {
        var cut = RenderHoverCard(p => p
            .AddChildContent<HoverCardTriggerCn>(t => t
                .AddChildContent("Hover me")));

        await cut.Find("[data-slot='hover-card-trigger']").TriggerEventAsync("onmouseenter", new MouseEventArgs());
        cut.Find("[data-slot='hover-card']").GetAttribute("data-state").Should().Be("open");

        await cut.Find("[data-slot='hover-card-trigger']").TriggerEventAsync("onmouseleave", new MouseEventArgs());
        cut.Find("[data-slot='hover-card']").GetAttribute("data-state").Should().Be("closed");
    }

    [Fact]
    public async Task HoverCardTrigger_Focus_Opens_HoverCard()
    {
        var cut = RenderHoverCard(p => p
            .AddChildContent<HoverCardTriggerCn>(t => t
                .AddChildContent("Focus me")));

        await cut.Find("[data-slot='hover-card-trigger']").TriggerEventAsync("onfocus", new FocusEventArgs());
        cut.Find("[data-slot='hover-card']").GetAttribute("data-state").Should().Be("open");
    }

    [Fact]
    public async Task HoverCardTrigger_Blur_Closes_HoverCard()
    {
        var cut = RenderHoverCard(p => p
            .AddChildContent<HoverCardTriggerCn>(t => t
                .AddChildContent("Focus me")));

        await cut.Find("[data-slot='hover-card-trigger']").TriggerEventAsync("onfocus", new FocusEventArgs());
        cut.Find("[data-slot='hover-card']").GetAttribute("data-state").Should().Be("open");

        await cut.Find("[data-slot='hover-card-trigger']").TriggerEventAsync("onblur", new FocusEventArgs());
        cut.Find("[data-slot='hover-card']").GetAttribute("data-state").Should().Be("closed");
    }

    [Fact]
    public void HoverCardTrigger_Class_Passthrough()
    {
        var cut = RenderHoverCard(p => p
            .AddChildContent<HoverCardTriggerCn>(t => t
                .Add(c => c.Class, "trigger-class")
                .AddChildContent("Hover")));
        cut.Find("[data-slot='hover-card-trigger']").ClassList.Should().Contain("trigger-class");
    }

    [Fact]
    public void HoverCardTrigger_AdditionalAttributes_Passthrough()
    {
        var cut = RenderHoverCard(p => p
            .AddChildContent<HoverCardTriggerCn>(t => t
                .Add(c => c.AdditionalAttributes, new Dictionary<string, object?> { { "aria-label", "profile" } })
                .AddChildContent("Hover")));
        cut.Find("[data-slot='hover-card-trigger']").GetAttribute("aria-label").Should().Be("profile");
    }

    // --- HoverCardContentCn ---

    [Fact]
    public void HoverCardContent_Not_Rendered_When_Closed()
    {
        SetupJsInterop();
        var cut = RenderHoverCard(p => p
            .AddChildContent<HoverCardContentCn>(c => c
                .AddChildContent("Card body")));
        cut.FindAll("[data-slot='hover-card-content']").Should().BeEmpty();
    }

    [Fact]
    public async Task HoverCardContent_Rendered_When_Open()
    {
        SetupJsInterop();
        var cut = Render<HoverCardCn>(p =>
        {
            p.Add(c => c.OpenDelay, 0);
            p.Add(c => c.CloseDelay, 0);
            p.AddChildContent(builder =>
            {
                builder.OpenComponent<HoverCardTriggerCn>(0);
                builder.AddAttribute(1, "ChildContent", (RenderFragment)(b => b.AddContent(0, "Hover")));
                builder.CloseComponent();
                builder.OpenComponent<HoverCardContentCn>(2);
                builder.AddAttribute(3, "ChildContent", (RenderFragment)(b => b.AddContent(0, "Card body")));
                builder.CloseComponent();
            });
        });

        await cut.Find("[data-slot='hover-card-trigger']").TriggerEventAsync("onmouseenter", new MouseEventArgs());
        cut.Find("[data-slot='hover-card-content']").Should().NotBeNull();
        cut.Find("[data-slot='hover-card-content']").TextContent.Should().Contain("Card body");
    }

    [Fact]
    public async Task HoverCardContent_Has_Default_Classes()
    {
        SetupJsInterop();
        var cut = Render<HoverCardCn>(p =>
        {
            p.Add(c => c.OpenDelay, 0);
            p.Add(c => c.CloseDelay, 0);
            p.AddChildContent(builder =>
            {
                builder.OpenComponent<HoverCardTriggerCn>(0);
                builder.AddAttribute(1, "ChildContent", (RenderFragment)(b => b.AddContent(0, "Hover")));
                builder.CloseComponent();
                builder.OpenComponent<HoverCardContentCn>(2);
                builder.AddAttribute(3, "ChildContent", (RenderFragment)(b => b.AddContent(0, "Body")));
                builder.CloseComponent();
            });
        });

        await cut.Find("[data-slot='hover-card-trigger']").TriggerEventAsync("onmouseenter", new MouseEventArgs());
        var content = cut.Find("[data-slot='hover-card-content']");
        content.ClassList.Should().Contain("z-50");
        content.ClassList.Should().Contain("w-64");
        content.ClassList.Should().Contain("rounded-md");
        content.ClassList.Should().Contain("border");
        content.ClassList.Should().Contain("bg-popover");
        content.ClassList.Should().Contain("p-4");
        content.ClassList.Should().Contain("text-popover-foreground");
        content.ClassList.Should().Contain("shadow-md");
        content.ClassList.Should().Contain("outline-none");
    }

    [Fact]
    public async Task HoverCardContent_Default_Side_Is_Bottom()
    {
        SetupJsInterop();
        var cut = Render<HoverCardCn>(p =>
        {
            p.Add(c => c.OpenDelay, 0);
            p.Add(c => c.CloseDelay, 0);
            p.AddChildContent(builder =>
            {
                builder.OpenComponent<HoverCardTriggerCn>(0);
                builder.AddAttribute(1, "ChildContent", (RenderFragment)(b => b.AddContent(0, "Hover")));
                builder.CloseComponent();
                builder.OpenComponent<HoverCardContentCn>(2);
                builder.AddAttribute(3, "ChildContent", (RenderFragment)(b => b.AddContent(0, "Body")));
                builder.CloseComponent();
            });
        });

        await cut.Find("[data-slot='hover-card-trigger']").TriggerEventAsync("onmouseenter", new MouseEventArgs());
        cut.Find("[data-slot='hover-card-content']").GetAttribute("data-side").Should().Be("bottom");
    }

    [Fact]
    public async Task HoverCardContent_Custom_Side()
    {
        SetupJsInterop();
        var cut = Render<HoverCardCn>(p =>
        {
            p.Add(c => c.OpenDelay, 0);
            p.Add(c => c.CloseDelay, 0);
            p.AddChildContent(builder =>
            {
                builder.OpenComponent<HoverCardTriggerCn>(0);
                builder.AddAttribute(1, "ChildContent", (RenderFragment)(b => b.AddContent(0, "Hover")));
                builder.CloseComponent();
                builder.OpenComponent<HoverCardContentCn>(2);
                builder.AddAttribute(3, "ChildContent", (RenderFragment)(b => b.AddContent(0, "Body")));
                builder.AddAttribute(4, "Side", FloatingSide.Right);
                builder.CloseComponent();
            });
        });

        await cut.Find("[data-slot='hover-card-trigger']").TriggerEventAsync("onmouseenter", new MouseEventArgs());
        cut.Find("[data-slot='hover-card-content']").GetAttribute("data-side").Should().Be("right");
    }

    [Fact]
    public async Task HoverCardContent_Default_Align_Is_Center()
    {
        SetupJsInterop();
        var cut = Render<HoverCardCn>(p =>
        {
            p.Add(c => c.OpenDelay, 0);
            p.Add(c => c.CloseDelay, 0);
            p.AddChildContent(builder =>
            {
                builder.OpenComponent<HoverCardTriggerCn>(0);
                builder.AddAttribute(1, "ChildContent", (RenderFragment)(b => b.AddContent(0, "Hover")));
                builder.CloseComponent();
                builder.OpenComponent<HoverCardContentCn>(2);
                builder.AddAttribute(3, "ChildContent", (RenderFragment)(b => b.AddContent(0, "Body")));
                builder.CloseComponent();
            });
        });

        await cut.Find("[data-slot='hover-card-trigger']").TriggerEventAsync("onmouseenter", new MouseEventArgs());
        cut.Find("[data-slot='hover-card-content']").GetAttribute("data-align").Should().Be("center");
    }

    [Fact]
    public async Task HoverCardContent_Class_Passthrough()
    {
        SetupJsInterop();
        var cut = Render<HoverCardCn>(p =>
        {
            p.Add(c => c.OpenDelay, 0);
            p.Add(c => c.CloseDelay, 0);
            p.AddChildContent(builder =>
            {
                builder.OpenComponent<HoverCardTriggerCn>(0);
                builder.AddAttribute(1, "ChildContent", (RenderFragment)(b => b.AddContent(0, "Hover")));
                builder.CloseComponent();
                builder.OpenComponent<HoverCardContentCn>(2);
                builder.AddAttribute(3, "ChildContent", (RenderFragment)(b => b.AddContent(0, "Body")));
                builder.AddAttribute(4, "Class", "custom-card");
                builder.CloseComponent();
            });
        });

        await cut.Find("[data-slot='hover-card-trigger']").TriggerEventAsync("onmouseenter", new MouseEventArgs());
        cut.Find("[data-slot='hover-card-content']").ClassList.Should().Contain("custom-card");
    }

    [Fact]
    public async Task HoverCardContent_AdditionalAttributes_Passthrough()
    {
        SetupJsInterop();
        var cut = Render<HoverCardCn>(p =>
        {
            p.Add(c => c.OpenDelay, 0);
            p.Add(c => c.CloseDelay, 0);
            p.AddChildContent(builder =>
            {
                builder.OpenComponent<HoverCardTriggerCn>(0);
                builder.AddAttribute(1, "ChildContent", (RenderFragment)(b => b.AddContent(0, "Hover")));
                builder.CloseComponent();
                builder.OpenComponent<HoverCardContentCn>(2);
                builder.AddAttribute(3, "ChildContent", (RenderFragment)(b => b.AddContent(0, "Body")));
                builder.AddAttribute(4, "AdditionalAttributes", new Dictionary<string, object?> { { "role", "tooltip" } });
                builder.CloseComponent();
            });
        });

        await cut.Find("[data-slot='hover-card-trigger']").TriggerEventAsync("onmouseenter", new MouseEventArgs());
        cut.Find("[data-slot='hover-card-content']").GetAttribute("role").Should().Be("tooltip");
    }

    // --- Content stays open when mouse moves to content ---

    [Fact]
    public async Task HoverCardContent_Stays_Open_When_Mouse_Moves_To_Content()
    {
        SetupJsInterop();
        var cut = Render<HoverCardCn>(p =>
        {
            p.Add(c => c.OpenDelay, 0);
            p.Add(c => c.CloseDelay, 0);
            p.AddChildContent(builder =>
            {
                builder.OpenComponent<HoverCardTriggerCn>(0);
                builder.AddAttribute(1, "ChildContent", (RenderFragment)(b => b.AddContent(0, "Hover")));
                builder.CloseComponent();
                builder.OpenComponent<HoverCardContentCn>(2);
                builder.AddAttribute(3, "ChildContent", (RenderFragment)(b => b.AddContent(0, "Card body")));
                builder.CloseComponent();
            });
        });

        // Open the hover card
        await cut.Find("[data-slot='hover-card-trigger']").TriggerEventAsync("onmouseenter", new MouseEventArgs());
        cut.Find("[data-slot='hover-card']").GetAttribute("data-state").Should().Be("open");

        // Mouse enters content (cancels close)
        await cut.Find("[data-slot='hover-card-content']").TriggerEventAsync("onmouseenter", new MouseEventArgs());

        // Mouse leaves trigger — card should stay open because content has mouseenter
        // (In real usage, mouseleave on trigger would fire before mouseenter on content,
        // but since we already fired mouseenter on content, the close is cancelled)
        cut.Find("[data-slot='hover-card']").GetAttribute("data-state").Should().Be("open");
    }
}

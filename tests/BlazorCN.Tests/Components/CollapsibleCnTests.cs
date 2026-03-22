using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace BlazorCN.Tests.Components;

public class CollapsibleCnTests : BunitContext
{
    [Fact]
    public void Collapsible_Renders_With_DataSlot()
    {
        var cut = Render<CollapsibleCn>(p => p.AddChildContent("Content"));
        cut.Find("[data-slot='collapsible']").Should().NotBeNull();
    }

    [Fact]
    public void Collapsible_Starts_Closed_By_Default()
    {
        var cut = Render<CollapsibleCn>(p => p.AddChildContent("Content"));
        var el = cut.Find("[data-slot='collapsible']");
        el.GetAttribute("data-state").Should().Be("closed");
    }

    [Fact]
    public void Collapsible_Starts_Open_When_Open_Is_True()
    {
        var cut = Render<CollapsibleCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent("Content"));
        var el = cut.Find("[data-slot='collapsible']");
        el.GetAttribute("data-state").Should().Be("open");
    }

    [Fact]
    public void Clicking_Trigger_Opens_Collapsible()
    {
        var isOpen = false;
        var cut = Render<CollapsibleCn>(p => p
            .Add(c => c.OpenChanged, EventCallback.Factory.Create<bool>(this, v => isOpen = v))
            .AddChildContent<CollapsibleTriggerCn>(t => t
                .AddChildContent("Toggle")));
        cut.Find("[data-slot='collapsible-trigger']").Click();
        isOpen.Should().BeTrue();
    }

    [Fact]
    public void Content_Is_Hidden_When_Closed()
    {
        var cut = Render<CollapsibleCn>(p => p
            .AddChildContent<CollapsibleContentCn>(c => c
                .AddChildContent("Hidden content")));
        cut.FindAll("[data-slot='collapsible-content']").Should().BeEmpty();
    }

    [Fact]
    public void Content_Is_Visible_When_Open()
    {
        var cut = Render<CollapsibleCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<CollapsibleContentCn>(c => c
                .AddChildContent("Visible content")));
        cut.Find("[data-slot='collapsible-content']").TextContent.Should().Contain("Visible content");
    }

    [Fact]
    public void Trigger_Has_Button_Type()
    {
        var cut = Render<CollapsibleCn>(p => p
            .AddChildContent<CollapsibleTriggerCn>(t => t
                .AddChildContent("Toggle")));
        var trigger = cut.Find("[data-slot='collapsible-trigger']");
        trigger.GetAttribute("type").Should().Be("button");
    }
}

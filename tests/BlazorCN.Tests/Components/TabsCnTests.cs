using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BlazorCN.Tests.Components;

public class TabsCnTests : BunitContext
{
    public TabsCnTests()
    {
        // TabsListCn injects JsInteropCn (arrow-key navigation).
        // Loose mode lets those interop calls no-op, and registering the service satisfies [Inject].
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddScoped<JsInteropCn>();
    }

    [Fact]
    public void Tabs_Renders_With_DataSlot()
    {
        var cut = Render<TabsCn>(p => p
            .Add(c => c.DefaultValue, "tab1")
            .AddChildContent("<span>Content</span>"));
        cut.Find("[data-slot='tabs']").Should().NotBeNull();
    }

    [Fact]
    public void Default_Tab_Is_Active()
    {
        var cut = Render<TabsCn>(p => p
            .Add(c => c.DefaultValue, "tab1")
            .AddChildContent<TabsListCn>(list => list
                .AddChildContent<TabsTriggerCn>(t => t
                    .Add(c => c.Value, "tab1")
                    .AddChildContent("Tab 1"))));
        var trigger = cut.Find("[data-slot='tabs-trigger']");
        trigger.GetAttribute("data-state").Should().Be("active");
        trigger.GetAttribute("aria-selected").Should().Be("true");
    }

    [Fact]
    public void Non_Active_Tab_Has_Inactive_State()
    {
        var cut = Render<TabsCn>(p => p
            .Add(c => c.DefaultValue, "tab1")
            .AddChildContent<TabsListCn>(list => list
                .AddChildContent<TabsTriggerCn>(t => t
                    .Add(c => c.Value, "tab2")
                    .AddChildContent("Tab 2"))));
        var trigger = cut.Find("[data-slot='tabs-trigger']");
        trigger.GetAttribute("data-state").Should().Be("inactive");
        trigger.GetAttribute("aria-selected").Should().Be("false");
    }

    [Fact]
    public void Clicking_Trigger_Switches_Tab()
    {
        string? selectedValue = null;
        var cut = Render<TabsCn>(p => p
            .Add(c => c.DefaultValue, "tab1")
            .Add(c => c.ValueChanged, EventCallback.Factory.Create<string?>(this, v => selectedValue = v))
            .AddChildContent<TabsListCn>(list => list
                .AddChildContent<TabsTriggerCn>(t => t
                    .Add(c => c.Value, "tab2")
                    .AddChildContent("Tab 2"))));
        cut.Find("[data-slot='tabs-trigger']").Click();
        selectedValue.Should().Be("tab2");
    }

    [Fact]
    public void Active_Content_Is_Visible()
    {
        var cut = Render<TabsCn>(p => p
            .Add(c => c.DefaultValue, "tab1")
            .AddChildContent<TabsContentCn>(content => content
                .Add(c => c.Value, "tab1")
                .AddChildContent("Content 1")));
        cut.Find("[data-slot='tabs-content']").TextContent.Should().Contain("Content 1");
    }

    [Fact]
    public void Inactive_Content_Is_Hidden()
    {
        var cut = Render<TabsCn>(p => p
            .Add(c => c.DefaultValue, "tab1")
            .AddChildContent<TabsContentCn>(content => content
                .Add(c => c.Value, "tab2")
                .AddChildContent("Content 2")));
        cut.FindAll("[data-slot='tabs-content']").Should().BeEmpty();
    }

    [Fact]
    public void TabsList_Has_Tablist_Role()
    {
        var cut = Render<TabsCn>(p => p
            .Add(c => c.DefaultValue, "tab1")
            .AddChildContent<TabsListCn>(list => list
                .AddChildContent("Items")));
        cut.Find("[role='tablist']").Should().NotBeNull();
    }

    [Fact]
    public void TabsList_Has_Default_AriaLabel()
    {
        var cut = Render<TabsCn>(p => p
            .Add(c => c.DefaultValue, "tab1")
            .AddChildContent<TabsListCn>(list => list
                .AddChildContent("Items")));
        cut.Find("[role='tablist']").GetAttribute("aria-label").Should().Be("Tabs");
    }

    [Fact]
    public void TabsList_AriaLabel_Override_Via_AdditionalAttributes()
    {
        var cut = Render<TabsCn>(p => p
            .Add(c => c.DefaultValue, "tab1")
            .AddChildContent<TabsListCn>(list => list
                .Add(x => x.AdditionalAttributes, new Dictionary<string, object?> { { "aria-label", "Settings sections" } })
                .AddChildContent("Items")));
        cut.Find("[role='tablist']").GetAttribute("aria-label").Should().Be("Settings sections");
    }

    [Fact]
    public void Disabled_Trigger_Has_Disabled_Attribute()
    {
        var cut = Render<TabsCn>(p => p
            .Add(c => c.DefaultValue, "tab1")
            .AddChildContent<TabsListCn>(list => list
                .AddChildContent<TabsTriggerCn>(t => t
                    .Add(c => c.Value, "tab2")
                    .Add(c => c.Disabled, true)
                    .AddChildContent("Tab 2"))));
        var trigger = cut.Find("[data-slot='tabs-trigger']");
        trigger.HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public void Roving_Tabindex_Only_Active_Trigger_Is_Focusable()
    {
        var cut = Render<TabsCn>(p => p
            .Add(c => c.DefaultValue, "tab1")
            .AddChildContent<TabsListCn>(list => list
                .AddChildContent(builder =>
                {
                    builder.OpenComponent<TabsTriggerCn>(0);
                    builder.AddAttribute(1, "Value", "tab1");
                    builder.AddAttribute(2, "ChildContent", (RenderFragment)(b => b.AddContent(0, "Tab 1")));
                    builder.CloseComponent();
                    builder.OpenComponent<TabsTriggerCn>(3);
                    builder.AddAttribute(4, "Value", "tab2");
                    builder.AddAttribute(5, "ChildContent", (RenderFragment)(b => b.AddContent(0, "Tab 2")));
                    builder.CloseComponent();
                })));
        var triggers = cut.FindAll("[data-slot='tabs-trigger']");
        triggers[0].GetAttribute("tabindex").Should().Be("0");
        triggers[1].GetAttribute("tabindex").Should().Be("-1");
    }

    [Fact]
    public void Focusing_Trigger_Activates_It()
    {
        var cut = Render<TabsCn>(p => p
            .Add(c => c.DefaultValue, "tab1")
            .AddChildContent<TabsListCn>(list => list
                .AddChildContent<TabsTriggerCn>(t => t
                    .Add(c => c.Value, "tab2")
                    .AddChildContent("Tab 2"))));
        var trigger = cut.Find("[data-slot='tabs-trigger']");
        trigger.Focus();
        cut.Find("[data-slot='tabs-trigger']").GetAttribute("data-state").Should().Be("active");
    }
}

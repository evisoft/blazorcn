using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BlazorCN.Tests.Components;

public class AccordionCnTests : BunitContext
{
    public AccordionCnTests()
    {
        // AccordionCn injects JsInteropCn (arrow-key scroll suppression).
        // Loose mode lets those interop calls no-op, and registering the service satisfies [Inject].
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddScoped<JsInteropCn>();
    }

    [Fact]
    public void Accordion_Renders_With_DataSlot()
    {
        var cut = Render<AccordionCn>(p => p.AddChildContent("Items"));
        cut.Find("[data-slot='accordion']").Should().NotBeNull();
    }

    [Fact]
    public void AccordionItem_Starts_Closed_By_Default()
    {
        var cut = Render<AccordionCn>(p => p
            .AddChildContent<AccordionItemCn>(item => item
                .AddChildContent<AccordionTriggerCn>(t => t
                    .AddChildContent("Section 1"))));
        var item = cut.Find("[data-slot='accordion-item']");
        item.GetAttribute("data-state").Should().Be("closed");
    }

    [Fact]
    public void AccordionItem_DefaultOpen_Starts_Open()
    {
        var cut = Render<AccordionCn>(p => p
            .AddChildContent<AccordionItemCn>(item => item
                .Add(c => c.DefaultOpen, true)
                .AddChildContent<AccordionTriggerCn>(t => t
                    .AddChildContent("Section 1"))));
        var item = cut.Find("[data-slot='accordion-item']");
        item.GetAttribute("data-state").Should().Be("open");
    }

    [Fact]
    public void Clicking_Trigger_Opens_Item()
    {
        var cut = Render<AccordionCn>(p => p
            .AddChildContent<AccordionItemCn>(item => item
                .AddChildContent<AccordionTriggerCn>(t => t
                    .AddChildContent("Section 1"))));
        cut.Find("[data-slot='accordion-trigger']").Click();
        var item = cut.Find("[data-slot='accordion-item']");
        item.GetAttribute("data-state").Should().Be("open");
    }

    [Fact]
    public void Clicking_Trigger_Twice_Closes_Item()
    {
        var cut = Render<AccordionCn>(p => p
            .AddChildContent<AccordionItemCn>(item => item
                .AddChildContent<AccordionTriggerCn>(t => t
                    .AddChildContent("Section 1"))));
        var trigger = cut.Find("[data-slot='accordion-trigger']");
        trigger.Click();
        trigger.Click();
        var item = cut.Find("[data-slot='accordion-item']");
        item.GetAttribute("data-state").Should().Be("closed");
    }

    [Fact]
    public void Content_Is_Hidden_When_Closed()
    {
        var cut = Render<AccordionCn>(p => p
            .AddChildContent<AccordionItemCn>(item => item
                .AddChildContent<AccordionContentCn>(c => c
                    .AddChildContent("Hidden content"))));
        cut.FindAll("[data-slot='accordion-content']").Should().BeEmpty();
    }

    [Fact]
    public void Content_Is_Visible_When_Open()
    {
        var cut = Render<AccordionCn>(p => p
            .AddChildContent<AccordionItemCn>(item => item
                .Add(c => c.DefaultOpen, true)
                .AddChildContent<AccordionContentCn>(c => c
                    .AddChildContent("Visible content"))));
        cut.Find("[data-slot='accordion-content']").TextContent.Should().Contain("Visible content");
    }

    [Fact]
    public void Trigger_AriaExpanded_Reflects_State()
    {
        var cut = Render<AccordionCn>(p => p
            .AddChildContent<AccordionItemCn>(item => item
                .AddChildContent<AccordionTriggerCn>(t => t
                    .AddChildContent("Section 1"))));
        var trigger = cut.Find("[data-slot='accordion-trigger']");
        trigger.GetAttribute("aria-expanded").Should().Be("false");
        trigger.Click();
        trigger.GetAttribute("aria-expanded").Should().Be("true");
    }

    [Fact]
    public void AccordionContent_AriaLabelledby_Matches_Trigger_Id()
    {
        var cut = Render<AccordionCn>(p => p
            .AddChildContent<AccordionItemCn>(item => item
                .Add(c => c.DefaultOpen, true)
                .AddChildContent(builder =>
                {
                    builder.OpenComponent<AccordionTriggerCn>(0);
                    builder.AddAttribute(1, "ChildContent", (Microsoft.AspNetCore.Components.RenderFragment)(b => b.AddContent(0, "Section 1")));
                    builder.CloseComponent();
                    builder.OpenComponent<AccordionContentCn>(2);
                    builder.AddAttribute(3, "ChildContent", (Microsoft.AspNetCore.Components.RenderFragment)(b => b.AddContent(0, "Content 1")));
                    builder.CloseComponent();
                })));
        var trigger = cut.Find("[data-slot='accordion-trigger']");
        var content = cut.Find("[data-slot='accordion-content']");
        var triggerId = trigger.GetAttribute("id");
        triggerId.Should().NotBeNullOrEmpty();
        content.GetAttribute("aria-labelledby").Should().Be(triggerId);
    }

    [Fact]
    public void SingleMode_Opening_Item_Closes_Others()
    {
        var cut = Render<AccordionCn>(p => p.AddChildContent(builder =>
        {
            builder.OpenComponent<AccordionItemCn>(0);
            builder.AddAttribute(1, "DefaultOpen", true);
            builder.AddAttribute(2, "ChildContent", (Microsoft.AspNetCore.Components.RenderFragment)(b =>
            {
                b.OpenComponent<AccordionTriggerCn>(0);
                b.AddAttribute(1, "ChildContent", (Microsoft.AspNetCore.Components.RenderFragment)(t => t.AddContent(0, "One")));
                b.CloseComponent();
            }));
            builder.CloseComponent();
            builder.OpenComponent<AccordionItemCn>(3);
            builder.AddAttribute(4, "ChildContent", (Microsoft.AspNetCore.Components.RenderFragment)(b =>
            {
                b.OpenComponent<AccordionTriggerCn>(0);
                b.AddAttribute(1, "ChildContent", (Microsoft.AspNetCore.Components.RenderFragment)(t => t.AddContent(0, "Two")));
                b.CloseComponent();
            }));
            builder.CloseComponent();
        }));

        cut.FindAll("[data-slot='accordion-trigger']")[1].Click();
        var items = cut.FindAll("[data-slot='accordion-item']");
        items[0].GetAttribute("data-state").Should().Be("closed");
        items[1].GetAttribute("data-state").Should().Be("open");
    }

    [Fact]
    public void MultipleMode_Keeps_Other_Items_Open()
    {
        var cut = Render<AccordionCn>(p => p
            .Add(c => c.Multiple, true)
            .AddChildContent(builder =>
        {
            builder.OpenComponent<AccordionItemCn>(0);
            builder.AddAttribute(1, "DefaultOpen", true);
            builder.AddAttribute(2, "ChildContent", (Microsoft.AspNetCore.Components.RenderFragment)(b =>
            {
                b.OpenComponent<AccordionTriggerCn>(0);
                b.AddAttribute(1, "ChildContent", (Microsoft.AspNetCore.Components.RenderFragment)(t => t.AddContent(0, "One")));
                b.CloseComponent();
            }));
            builder.CloseComponent();
            builder.OpenComponent<AccordionItemCn>(3);
            builder.AddAttribute(4, "ChildContent", (Microsoft.AspNetCore.Components.RenderFragment)(b =>
            {
                b.OpenComponent<AccordionTriggerCn>(0);
                b.AddAttribute(1, "ChildContent", (Microsoft.AspNetCore.Components.RenderFragment)(t => t.AddContent(0, "Two")));
                b.CloseComponent();
            }));
            builder.CloseComponent();
        }));

        cut.FindAll("[data-slot='accordion-trigger']")[1].Click();
        var items = cut.FindAll("[data-slot='accordion-item']");
        items[0].GetAttribute("data-state").Should().Be("open");
        items[1].GetAttribute("data-state").Should().Be("open");
    }

    [Fact]
    public void Wires_PreventKeyDefaults_Once_For_Trigger_Keys()
    {
        // Blazor's @onkeydown cannot conditionally preventDefault, so without this
        // JS guard every arrow/Home/End press on a trigger also scrolls the page.
        // A single root-level registration covers all triggers (regression).
        var module = JSInterop.SetupModule("./_content/BlazorCN/blazorcn.js");
        var handler = module.SetupVoid("preventKeyDefaults", _ => true);
        handler.SetVoidResult();

        Render<AccordionCn>(p => p.AddChildContent(builder =>
        {
            builder.OpenComponent<AccordionItemCn>(0);
            builder.AddAttribute(1, "ChildContent", (Microsoft.AspNetCore.Components.RenderFragment)(b =>
            {
                b.OpenComponent<AccordionTriggerCn>(0);
                b.AddAttribute(1, "ChildContent", (Microsoft.AspNetCore.Components.RenderFragment)(t => t.AddContent(0, "One")));
                b.CloseComponent();
            }));
            builder.CloseComponent();
            builder.OpenComponent<AccordionItemCn>(2);
            builder.AddAttribute(3, "ChildContent", (Microsoft.AspNetCore.Components.RenderFragment)(b =>
            {
                b.OpenComponent<AccordionTriggerCn>(0);
                b.AddAttribute(1, "ChildContent", (Microsoft.AspNetCore.Components.RenderFragment)(t => t.AddContent(0, "Two")));
                b.CloseComponent();
            }));
            builder.CloseComponent();
        }));

        var invocation = handler.Invocations.Should().ContainSingle().Subject;
        invocation.Arguments[2].Should().BeEquivalentTo(
            new[] { "ArrowDown", "ArrowUp", "Home", "End" });
        invocation.Arguments[3].Should().Be("[data-slot=\"accordion-trigger\"]");
    }
}

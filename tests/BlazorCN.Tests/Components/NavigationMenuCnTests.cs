using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BlazorCN.Tests.Components;

public class NavigationMenuCnTests : BunitContext
{
    private void SetupJsInterop()
    {
        var module = JSInterop.SetupModule("./_content/BlazorCN/blazorcn.js");
        module.Setup<string>("createFloating", _ => true).SetResult("bottom");
        module.SetupVoid("destroyFloating", _ => true).SetVoidResult();
        Services.AddScoped<JsInteropCn>();
    }

    // --- NavigationMenuCn ---

    [Fact]
    public void NavigationMenu_Renders_With_DataSlot()
    {
        var cut = Render<NavigationMenuCn>(p => p.AddChildContent("Content"));
        cut.Find("[data-slot='navigation-menu']").Should().NotBeNull();
    }

    [Fact]
    public void NavigationMenu_Is_Nav_Element()
    {
        var cut = Render<NavigationMenuCn>(p => p.AddChildContent("Content"));
        cut.Find("[data-slot='navigation-menu']").TagName.Should().Be("NAV");
    }

    [Fact]
    public void NavigationMenu_Has_Default_Classes()
    {
        var cut = Render<NavigationMenuCn>(p => p.AddChildContent("Content"));
        var el = cut.Find("[data-slot='navigation-menu']");
        el.ClassList.Should().Contain("group/navigation-menu");
        el.ClassList.Should().Contain("relative");
        el.ClassList.Should().Contain("flex");
        el.ClassList.Should().Contain("items-center");
        el.ClassList.Should().Contain("justify-center");
    }

    [Fact]
    public void NavigationMenu_Class_Passthrough()
    {
        var cut = Render<NavigationMenuCn>(p => p
            .Add(c => c.Class, "custom-nav")
            .AddChildContent("Content"));
        cut.Find("[data-slot='navigation-menu']").ClassList.Should().Contain("custom-nav");
    }

    [Fact]
    public void NavigationMenu_AdditionalAttributes_Passthrough()
    {
        var cut = Render<NavigationMenuCn>(p => p
            .Add(c => c.AdditionalAttributes, new Dictionary<string, object?> { { "id", "nav" } })
            .AddChildContent("Content"));
        cut.Find("[data-slot='navigation-menu']").GetAttribute("id").Should().Be("nav");
    }

    // --- NavigationMenuListCn ---

    [Fact]
    public void NavigationMenuList_Renders_With_DataSlot()
    {
        var cut = Render<NavigationMenuListCn>(p => p.AddChildContent("Items"));
        cut.Find("[data-slot='navigation-menu-list']").Should().NotBeNull();
    }

    [Fact]
    public void NavigationMenuList_Is_Ul_Element()
    {
        var cut = Render<NavigationMenuListCn>(p => p.AddChildContent("Items"));
        cut.Find("[data-slot='navigation-menu-list']").TagName.Should().Be("UL");
    }

    [Fact]
    public void NavigationMenuList_Has_Default_Classes()
    {
        var cut = Render<NavigationMenuListCn>(p => p.AddChildContent("Items"));
        var el = cut.Find("[data-slot='navigation-menu-list']");
        el.ClassList.Should().Contain("cn-navigation-menu-list");
        el.ClassList.Should().Contain("group");
        el.ClassList.Should().Contain("flex");
        el.ClassList.Should().Contain("list-none");
        el.ClassList.Should().Contain("items-center");
        el.ClassList.Should().Contain("justify-center");
    }

    [Fact]
    public void NavigationMenuList_Class_Passthrough()
    {
        var cut = Render<NavigationMenuListCn>(p => p
            .Add(c => c.Class, "custom-list")
            .AddChildContent("Items"));
        cut.Find("[data-slot='navigation-menu-list']").ClassList.Should().Contain("custom-list");
    }

    // --- NavigationMenuItemCn ---

    [Fact]
    public void NavigationMenuItem_Renders_With_DataSlot()
    {
        var cut = Render<NavigationMenuItemCn>(p => p.AddChildContent("Item"));
        cut.Find("[data-slot='navigation-menu-item']").Should().NotBeNull();
    }

    [Fact]
    public void NavigationMenuItem_Is_Li_Element()
    {
        var cut = Render<NavigationMenuItemCn>(p => p.AddChildContent("Item"));
        cut.Find("[data-slot='navigation-menu-item']").TagName.Should().Be("LI");
    }

    [Fact]
    public void NavigationMenuItem_Has_Default_Classes()
    {
        var cut = Render<NavigationMenuItemCn>(p => p.AddChildContent("Item"));
        cut.Find("[data-slot='navigation-menu-item']").ClassList.Should().Contain("relative");
    }

    [Fact]
    public void NavigationMenuItem_Starts_Closed()
    {
        var cut = Render<NavigationMenuItemCn>(p => p.AddChildContent("Item"));
        cut.Find("[data-slot='navigation-menu-item']").GetAttribute("data-state").Should().Be("closed");
    }

    [Fact]
    public void NavigationMenuItem_Hover_Opens()
    {
        var cut = Render<NavigationMenuItemCn>(p => p
            .Add(c => c.OpenDelay, 0)
            .AddChildContent("Item"));
        cut.Find("[data-slot='navigation-menu-item']").TriggerEvent("onmouseenter", new MouseEventArgs());
        cut.Find("[data-slot='navigation-menu-item']").GetAttribute("data-state").Should().Be("open");
    }

    [Fact]
    public void NavigationMenuItem_MouseLeave_Closes()
    {
        var cut = Render<NavigationMenuItemCn>(p => p
            .Add(c => c.OpenDelay, 0)
            .Add(c => c.CloseDelay, 0)
            .AddChildContent("Item"));
        cut.Find("[data-slot='navigation-menu-item']").TriggerEvent("onmouseenter", new MouseEventArgs());
        cut.Find("[data-slot='navigation-menu-item']").GetAttribute("data-state").Should().Be("open");

        cut.Find("[data-slot='navigation-menu-item']").TriggerEvent("onmouseleave", new MouseEventArgs());
        cut.Find("[data-slot='navigation-menu-item']").GetAttribute("data-state").Should().Be("closed");
    }

    [Fact]
    public void NavigationMenuItem_Class_Passthrough()
    {
        var cut = Render<NavigationMenuItemCn>(p => p
            .Add(c => c.Class, "custom-item")
            .AddChildContent("Item"));
        cut.Find("[data-slot='navigation-menu-item']").ClassList.Should().Contain("custom-item");
    }

    // --- NavigationMenuTriggerCn ---

    [Fact]
    public void NavigationMenuTrigger_Renders_With_DataSlot()
    {
        var cut = Render<NavigationMenuItemCn>(p => p
            .Add(c => c.OpenDelay, 0)
            .AddChildContent<NavigationMenuTriggerCn>(t => t
                .AddChildContent("Getting Started")));
        cut.Find("[data-slot='navigation-menu-trigger']").Should().NotBeNull();
    }

    [Fact]
    public void NavigationMenuTrigger_Is_Button()
    {
        var cut = Render<NavigationMenuItemCn>(p => p
            .Add(c => c.OpenDelay, 0)
            .AddChildContent<NavigationMenuTriggerCn>(t => t
                .AddChildContent("Getting Started")));
        cut.Find("[data-slot='navigation-menu-trigger']").TagName.Should().Be("BUTTON");
    }

    [Fact]
    public void NavigationMenuTrigger_Has_Default_Classes()
    {
        var cut = Render<NavigationMenuItemCn>(p => p
            .Add(c => c.OpenDelay, 0)
            .AddChildContent<NavigationMenuTriggerCn>(t => t
                .AddChildContent("Getting Started")));
        var el = cut.Find("[data-slot='navigation-menu-trigger']");
        el.ClassList.Should().Contain("cn-navigation-menu-trigger");
        el.ClassList.Should().Contain("group");
        el.ClassList.Should().Contain("inline-flex");
        el.ClassList.Should().Contain("h-9");
        el.ClassList.Should().Contain("items-center");
        el.ClassList.Should().Contain("justify-center");
        el.ClassList.Should().Contain("outline-none");
    }

    [Fact]
    public void NavigationMenuTrigger_Has_Chevron_Down()
    {
        var cut = Render<NavigationMenuItemCn>(p => p
            .Add(c => c.OpenDelay, 0)
            .AddChildContent<NavigationMenuTriggerCn>(t => t
                .AddChildContent("Getting Started")));
        cut.FindAll("svg").Should().NotBeEmpty();
    }

    [Fact]
    public void NavigationMenuTrigger_DataState_Reflects_Parent()
    {
        var cut = Render<NavigationMenuItemCn>(p => p
            .Add(c => c.OpenDelay, 0)
            .AddChildContent<NavigationMenuTriggerCn>(t => t
                .AddChildContent("Getting Started")));
        cut.Find("[data-slot='navigation-menu-trigger']").GetAttribute("data-state").Should().Be("closed");

        // Hover parent to open
        cut.Find("[data-slot='navigation-menu-item']").TriggerEvent("onmouseenter", new MouseEventArgs());
        cut.Find("[data-slot='navigation-menu-trigger']").GetAttribute("data-state").Should().Be("open");
    }

    [Fact]
    public void NavigationMenuTrigger_Class_Passthrough()
    {
        var cut = Render<NavigationMenuItemCn>(p => p
            .Add(c => c.OpenDelay, 0)
            .AddChildContent<NavigationMenuTriggerCn>(t => t
                .Add(c => c.Class, "custom-trigger")
                .AddChildContent("Getting Started")));
        cut.Find("[data-slot='navigation-menu-trigger']").ClassList.Should().Contain("custom-trigger");
    }

    // --- NavigationMenuContentCn ---

    [Fact]
    public void NavigationMenuContent_Not_Rendered_When_Closed()
    {
        SetupJsInterop();
        var cut = Render<NavigationMenuItemCn>(p => p
            .Add(c => c.OpenDelay, 0)
            .AddChildContent<NavigationMenuContentCn>(c => c
                .AddChildContent("Content panel")));
        cut.FindAll("[data-slot='navigation-menu-content']").Should().BeEmpty();
    }

    [Fact]
    public void NavigationMenuContent_Rendered_When_Open()
    {
        SetupJsInterop();
        var cut = Render<NavigationMenuItemCn>(p => p
            .Add(c => c.OpenDelay, 0)
            .AddChildContent(builder =>
            {
                builder.OpenComponent<NavigationMenuTriggerCn>(0);
                builder.AddAttribute(1, "ChildContent", (RenderFragment)(b => b.AddContent(0, "Getting Started")));
                builder.CloseComponent();
                builder.OpenComponent<NavigationMenuContentCn>(2);
                builder.AddAttribute(3, "ChildContent", (RenderFragment)(b => b.AddContent(0, "Content panel")));
                builder.CloseComponent();
            }));

        // Hover to open
        cut.Find("[data-slot='navigation-menu-item']").TriggerEvent("onmouseenter", new MouseEventArgs());
        cut.Find("[data-slot='navigation-menu-content']").Should().NotBeNull();
        cut.Find("[data-slot='navigation-menu-content']").TextContent.Should().Contain("Content panel");
    }

    [Fact]
    public void NavigationMenuContent_Has_Default_Classes()
    {
        SetupJsInterop();
        var cut = Render<NavigationMenuItemCn>(p => p
            .Add(c => c.OpenDelay, 0)
            .AddChildContent(builder =>
            {
                builder.OpenComponent<NavigationMenuTriggerCn>(0);
                builder.AddAttribute(1, "ChildContent", (RenderFragment)(b => b.AddContent(0, "Trigger")));
                builder.CloseComponent();
                builder.OpenComponent<NavigationMenuContentCn>(2);
                builder.AddAttribute(3, "ChildContent", (RenderFragment)(b => b.AddContent(0, "Panel")));
                builder.CloseComponent();
            }));

        cut.Find("[data-slot='navigation-menu-item']").TriggerEvent("onmouseenter", new MouseEventArgs());
        var content = cut.Find("[data-slot='navigation-menu-content']");
        content.ClassList.Should().Contain("cn-navigation-menu-content");
    }

    [Fact]
    public void NavigationMenuContent_Class_Passthrough()
    {
        SetupJsInterop();
        var cut = Render<NavigationMenuItemCn>(p => p
            .Add(c => c.OpenDelay, 0)
            .AddChildContent(builder =>
            {
                builder.OpenComponent<NavigationMenuTriggerCn>(0);
                builder.AddAttribute(1, "ChildContent", (RenderFragment)(b => b.AddContent(0, "Trigger")));
                builder.CloseComponent();
                builder.OpenComponent<NavigationMenuContentCn>(2);
                builder.AddAttribute(3, "ChildContent", (RenderFragment)(b => b.AddContent(0, "Panel")));
                builder.AddAttribute(4, "Class", "custom-content");
                builder.CloseComponent();
            }));

        cut.Find("[data-slot='navigation-menu-item']").TriggerEvent("onmouseenter", new MouseEventArgs());
        cut.Find("[data-slot='navigation-menu-content']").ClassList.Should().Contain("custom-content");
    }

    // --- NavigationMenuLinkCn ---

    [Fact]
    public void NavigationMenuLink_Renders_With_DataSlot()
    {
        var cut = Render<NavigationMenuLinkCn>(p => p
            .Add(c => c.Href, "/about")
            .AddChildContent("About"));
        cut.Find("[data-slot='navigation-menu-link']").Should().NotBeNull();
    }

    [Fact]
    public void NavigationMenuLink_Is_Anchor_Element()
    {
        var cut = Render<NavigationMenuLinkCn>(p => p
            .Add(c => c.Href, "/about")
            .AddChildContent("About"));
        cut.Find("[data-slot='navigation-menu-link']").TagName.Should().Be("A");
    }

    [Fact]
    public void NavigationMenuLink_Has_Href()
    {
        var cut = Render<NavigationMenuLinkCn>(p => p
            .Add(c => c.Href, "/about")
            .AddChildContent("About"));
        cut.Find("[data-slot='navigation-menu-link']").GetAttribute("href").Should().Be("/about");
    }

    [Fact]
    public void NavigationMenuLink_Has_Default_Classes()
    {
        var cut = Render<NavigationMenuLinkCn>(p => p
            .Add(c => c.Href, "/about")
            .AddChildContent("About"));
        var el = cut.Find("[data-slot='navigation-menu-link']");
        // Layout (flex/gap/padding/focus styles) now lives in the cn-navigation-menu-link CSS class.
        el.ClassList.Should().Contain("cn-navigation-menu-link");
    }

    [Fact]
    public void NavigationMenuLink_Active_Has_DataActive_Attribute()
    {
        var cut = Render<NavigationMenuLinkCn>(p => p
            .Add(c => c.Href, "/about")
            .Add(c => c.Active, true)
            .AddChildContent("About"));
        cut.Find("[data-slot='navigation-menu-link']").GetAttribute("data-active").Should().Be("true");
    }

    [Fact]
    public void NavigationMenuLink_NotActive_No_DataActive_Attribute()
    {
        var cut = Render<NavigationMenuLinkCn>(p => p
            .Add(c => c.Href, "/about")
            .AddChildContent("About"));
        cut.Find("[data-slot='navigation-menu-link']").GetAttribute("data-active").Should().BeNull();
    }

    [Fact]
    public void NavigationMenuLink_Class_Passthrough()
    {
        var cut = Render<NavigationMenuLinkCn>(p => p
            .Add(c => c.Href, "/about")
            .Add(c => c.Class, "custom-link")
            .AddChildContent("About"));
        cut.Find("[data-slot='navigation-menu-link']").ClassList.Should().Contain("custom-link");
    }

    // --- NavigationMenuIndicatorCn ---

    [Fact]
    public void NavigationMenuIndicator_Renders_With_DataSlot()
    {
        var cut = Render<NavigationMenuIndicatorCn>(p => p.AddChildContent("Arrow"));
        cut.Find("[data-slot='navigation-menu-indicator']").Should().NotBeNull();
    }

    [Fact]
    public void NavigationMenuIndicator_Has_Default_Classes()
    {
        var cut = Render<NavigationMenuIndicatorCn>(p => p.AddChildContent("Arrow"));
        var el = cut.Find("[data-slot='navigation-menu-indicator']");
        el.ClassList.Should().Contain("top-full");
        el.ClassList.Should().Contain("z-1");
        el.ClassList.Should().Contain("flex");
        el.ClassList.Should().Contain("h-1.5");
        el.ClassList.Should().Contain("items-end");
        el.ClassList.Should().Contain("justify-center");
        el.ClassList.Should().Contain("overflow-hidden");
    }

    [Fact]
    public void NavigationMenuIndicator_Class_Passthrough()
    {
        var cut = Render<NavigationMenuIndicatorCn>(p => p
            .Add(c => c.Class, "custom-indicator")
            .AddChildContent("Arrow"));
        cut.Find("[data-slot='navigation-menu-indicator']").ClassList.Should().Contain("custom-indicator");
    }

    // --- NavigationMenuViewportCn ---

    [Fact]
    public void NavigationMenuViewport_Renders_With_DataSlot()
    {
        var cut = Render<NavigationMenuViewportCn>(p => p.AddChildContent("Viewport"));
        cut.Find("[data-slot='navigation-menu-viewport']").Should().NotBeNull();
    }

    [Fact]
    public void NavigationMenuViewport_Has_Default_Classes()
    {
        var cut = Render<NavigationMenuViewportCn>(p => p.AddChildContent("Viewport"));
        var el = cut.Find("[data-slot='navigation-menu-viewport']");
        el.ClassList.Should().Contain("cn-navigation-menu-viewport");
        el.ClassList.Should().Contain("relative");
        el.ClassList.Should().Contain("mt-1.5");
        el.ClassList.Should().Contain("w-full");
        el.ClassList.Should().Contain("overflow-hidden");
    }

    [Fact]
    public void NavigationMenuViewport_Class_Passthrough()
    {
        var cut = Render<NavigationMenuViewportCn>(p => p
            .Add(c => c.Class, "custom-viewport")
            .AddChildContent("Viewport"));
        cut.Find("[data-slot='navigation-menu-viewport']").ClassList.Should().Contain("custom-viewport");
    }

    // --- ARIA ---

    [Fact]
    public void NavigationMenuTrigger_AriaExpanded_Reflects_State()
    {
        var cut = Render<NavigationMenuItemCn>(p => p
            .Add(c => c.OpenDelay, 0)
            .AddChildContent<NavigationMenuTriggerCn>(t => t
                .AddChildContent("Getting Started")));
        var trigger = cut.Find("[data-slot='navigation-menu-trigger']");
        trigger.GetAttribute("aria-expanded").Should().Be("false");
        trigger.Click();
        trigger.GetAttribute("aria-expanded").Should().Be("true");
    }

    // --- Integration ---

    [Fact]
    public void NavigationMenu_Full_Integration()
    {
        SetupJsInterop();
        var cut = Render<NavigationMenuCn>(p => p.AddChildContent(builder =>
        {
            builder.OpenComponent<NavigationMenuListCn>(0);
            builder.AddAttribute(1, "ChildContent", (RenderFragment)(listBuilder =>
            {
                listBuilder.OpenComponent<NavigationMenuItemCn>(0);
                listBuilder.AddAttribute(1, "OpenDelay", 0);
                listBuilder.AddAttribute(2, "CloseDelay", 0);
                listBuilder.AddAttribute(3, "ChildContent", (RenderFragment)(itemBuilder =>
                {
                    itemBuilder.OpenComponent<NavigationMenuTriggerCn>(0);
                    itemBuilder.AddAttribute(1, "ChildContent", (RenderFragment)(b => b.AddContent(0, "Getting Started")));
                    itemBuilder.CloseComponent();
                    itemBuilder.OpenComponent<NavigationMenuContentCn>(2);
                    itemBuilder.AddAttribute(3, "ChildContent", (RenderFragment)(b =>
                    {
                        b.OpenComponent<NavigationMenuLinkCn>(0);
                        b.AddAttribute(1, "Href", "/docs");
                        b.AddAttribute(2, "ChildContent", (RenderFragment)(lb => lb.AddContent(0, "Documentation")));
                        b.CloseComponent();
                    }));
                    itemBuilder.CloseComponent();
                }));
                listBuilder.CloseComponent();
            }));
            builder.CloseComponent();
        }));

        // Initially closed
        cut.Find("[data-slot='navigation-menu']").Should().NotBeNull();
        cut.Find("[data-slot='navigation-menu-list']").Should().NotBeNull();
        cut.Find("[data-slot='navigation-menu-item']").GetAttribute("data-state").Should().Be("closed");
        cut.FindAll("[data-slot='navigation-menu-content']").Should().BeEmpty();

        // Hover to open
        cut.Find("[data-slot='navigation-menu-item']").TriggerEvent("onmouseenter", new MouseEventArgs());
        cut.Find("[data-slot='navigation-menu-item']").GetAttribute("data-state").Should().Be("open");
        cut.Find("[data-slot='navigation-menu-content']").Should().NotBeNull();
        cut.Find("[data-slot='navigation-menu-link']").TextContent.Should().Contain("Documentation");

        // Mouse leave to close
        cut.Find("[data-slot='navigation-menu-item']").TriggerEvent("onmouseleave", new MouseEventArgs());
        cut.Find("[data-slot='navigation-menu-item']").GetAttribute("data-state").Should().Be("closed");
    }
}

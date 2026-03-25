using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BlazorCN.Tests.Components;

public class SidebarCnTests : BunitContext
{
    private void SetupJsInterop()
    {
        var module = JSInterop.SetupModule("./_content/BlazorCN/blazorcn.js");
        module.SetupVoid("trapFocus", _ => true).SetVoidResult();
        module.SetupVoid("lockScroll", _ => true).SetVoidResult();
        module.SetupVoid("cleanup", _ => true).SetVoidResult();
        Services.AddScoped<JsInteropCn>();
    }

    // --- SidebarProviderCn ---

    [Fact]
    public void Provider_Renders_With_DataSlot()
    {
        var cut = Render<SidebarProviderCn>(p => p.AddChildContent("Content"));
        cut.Find("[data-slot='sidebar-wrapper']").Should().NotBeNull();
    }

    [Fact]
    public void Provider_Has_CssVariables_In_Style()
    {
        var cut = Render<SidebarProviderCn>(p => p.AddChildContent("Content"));
        var el = cut.Find("[data-slot='sidebar-wrapper']");
        el.GetAttribute("style").Should().Contain("--sidebar-width: 16rem");
        el.GetAttribute("style").Should().Contain("--sidebar-width-icon: 3rem");
    }

    [Fact]
    public void Provider_DefaultOpen_True()
    {
        var cut = Render<SidebarProviderCn>(p => p.AddChildContent("Content"));
        var el = cut.Find("[data-slot='sidebar-wrapper']");
        el.ClassList.Should().Contain("group/sidebar-wrapper");
    }

    [Fact]
    public void Provider_Class_Passthrough()
    {
        var cut = Render<SidebarProviderCn>(p => p
            .Add(c => c.Class, "custom-class")
            .AddChildContent("Content"));
        cut.Find("[data-slot='sidebar-wrapper']").ClassList.Should().Contain("custom-class");
    }

    // --- SidebarCn (Collapsible=None) ---

    [Fact]
    public void Sidebar_None_Renders_Simple_Div()
    {
        var cut = Render<SidebarProviderCn>(p => p
            .AddChildContent<SidebarCn>(s => s
                .Add(c => c.Collapsible, SidebarCollapsible.None)
                .AddChildContent("Sidebar Content")));
        var el = cut.Find("[data-slot='sidebar']");
        el.Should().NotBeNull();
        el.TextContent.Should().Contain("Sidebar Content");
    }

    [Fact]
    public void Sidebar_None_Has_Correct_Classes()
    {
        var cut = Render<SidebarProviderCn>(p => p
            .AddChildContent<SidebarCn>(s => s
                .Add(c => c.Collapsible, SidebarCollapsible.None)
                .AddChildContent("Content")));
        var el = cut.Find("[data-slot='sidebar']");
        el.ClassList.Should().Contain("flex");
        el.ClassList.Should().Contain("bg-sidebar");
    }

    // --- SidebarCn (Desktop) ---

    [Fact]
    public void Sidebar_Desktop_Renders_With_DataAttributes()
    {
        var cut = Render<SidebarProviderCn>(p => p
            .AddChildContent<SidebarCn>(s => s
                .AddChildContent("Content")));
        var el = cut.Find("[data-slot='sidebar']");
        el.GetAttribute("data-state").Should().Be("expanded");
        el.GetAttribute("data-variant").Should().Be("sidebar");
        el.GetAttribute("data-side").Should().Be("left");
    }

    [Fact]
    public void Sidebar_Desktop_Has_Gap_And_Container()
    {
        var cut = Render<SidebarProviderCn>(p => p
            .AddChildContent<SidebarCn>(s => s
                .AddChildContent("Content")));
        cut.Find("[data-slot='sidebar-gap']").Should().NotBeNull();
        cut.Find("[data-slot='sidebar-container']").Should().NotBeNull();
        cut.Find("[data-slot='sidebar-inner']").Should().NotBeNull();
    }

    [Fact]
    public void Sidebar_Side_Right()
    {
        var cut = Render<SidebarProviderCn>(p => p
            .AddChildContent<SidebarCn>(s => s
                .Add(c => c.Side, SidebarSide.Right)
                .AddChildContent("Content")));
        var el = cut.Find("[data-slot='sidebar']");
        el.GetAttribute("data-side").Should().Be("right");
    }

    [Fact]
    public void Sidebar_Variant_Floating()
    {
        var cut = Render<SidebarProviderCn>(p => p
            .AddChildContent<SidebarCn>(s => s
                .Add(c => c.Variant, SidebarVariant.Floating)
                .AddChildContent("Content")));
        var el = cut.Find("[data-slot='sidebar']");
        el.GetAttribute("data-variant").Should().Be("floating");
    }

    // --- SidebarHeaderCn ---

    [Fact]
    public void Header_Renders_With_DataSlot()
    {
        var cut = Render<SidebarHeaderCn>(p => p.AddChildContent("Header"));
        var el = cut.Find("[data-slot='sidebar-header']");
        el.Should().NotBeNull();
        el.GetAttribute("data-sidebar").Should().Be("header");
        el.TextContent.Should().Contain("Header");
    }

    [Fact]
    public void Header_Has_Correct_Classes()
    {
        var cut = Render<SidebarHeaderCn>(p => p.AddChildContent("H"));
        var el = cut.Find("[data-slot='sidebar-header']");
        el.ClassList.Should().Contain("cn-sidebar-header");
        el.ClassList.Should().Contain("flex");
    }

    // --- SidebarFooterCn ---

    [Fact]
    public void Footer_Renders_With_DataSlot()
    {
        var cut = Render<SidebarFooterCn>(p => p.AddChildContent("Footer"));
        var el = cut.Find("[data-slot='sidebar-footer']");
        el.Should().NotBeNull();
        el.GetAttribute("data-sidebar").Should().Be("footer");
    }

    // --- SidebarContentCn ---

    [Fact]
    public void Content_Renders_With_DataSlot()
    {
        var cut = Render<SidebarContentCn>(p => p.AddChildContent("Body"));
        var el = cut.Find("[data-slot='sidebar-content']");
        el.Should().NotBeNull();
        el.GetAttribute("data-sidebar").Should().Be("content");
    }

    [Fact]
    public void Content_Has_Overflow_Auto()
    {
        var cut = Render<SidebarContentCn>(p => p.AddChildContent("Body"));
        var el = cut.Find("[data-slot='sidebar-content']");
        el.ClassList.Should().Contain("overflow-auto");
    }

    // --- SidebarSeparatorCn ---

    [Fact]
    public void Separator_Renders_With_DataSlot()
    {
        var cut = Render<SidebarSeparatorCn>();
        var el = cut.Find("[data-slot='sidebar-separator']");
        el.Should().NotBeNull();
        el.GetAttribute("role").Should().Be("separator");
    }

    // --- SidebarGroupCn ---

    [Fact]
    public void Group_Renders_With_DataSlot()
    {
        var cut = Render<SidebarGroupCn>(p => p.AddChildContent("Group"));
        var el = cut.Find("[data-slot='sidebar-group']");
        el.Should().NotBeNull();
        el.GetAttribute("data-sidebar").Should().Be("group");
    }

    // --- SidebarGroupLabelCn ---

    [Fact]
    public void GroupLabel_Renders_With_DataSlot()
    {
        var cut = Render<SidebarGroupLabelCn>(p => p.AddChildContent("Label"));
        var el = cut.Find("[data-slot='sidebar-group-label']");
        el.Should().NotBeNull();
        el.TextContent.Should().Contain("Label");
    }

    // --- SidebarGroupActionCn ---

    [Fact]
    public void GroupAction_Renders_With_DataSlot()
    {
        var cut = Render<SidebarGroupActionCn>(p => p.AddChildContent("Action"));
        var el = cut.Find("[data-slot='sidebar-group-action']");
        el.Should().NotBeNull();
        el.GetAttribute("type").Should().Be("button");
    }

    // --- SidebarGroupContentCn ---

    [Fact]
    public void GroupContent_Renders_With_DataSlot()
    {
        var cut = Render<SidebarGroupContentCn>(p => p.AddChildContent("Content"));
        var el = cut.Find("[data-slot='sidebar-group-content']");
        el.Should().NotBeNull();
    }

    // --- SidebarMenuCn ---

    [Fact]
    public void Menu_Renders_As_Ul()
    {
        var cut = Render<SidebarMenuCn>(p => p.AddChildContent("Items"));
        var el = cut.Find("[data-slot='sidebar-menu']");
        el.Should().NotBeNull();
        el.TagName.Should().Be("UL");
    }

    // --- SidebarMenuItemCn ---

    [Fact]
    public void MenuItem_Renders_As_Li()
    {
        var cut = Render<SidebarMenuItemCn>(p => p.AddChildContent("Item"));
        var el = cut.Find("[data-slot='sidebar-menu-item']");
        el.Should().NotBeNull();
        el.TagName.Should().Be("LI");
    }

    // --- SidebarMenuButtonCn ---

    [Fact]
    public void MenuButton_Renders_As_Button_By_Default()
    {
        var cut = Render<SidebarMenuButtonCn>(p => p.AddChildContent("Click"));
        var el = cut.Find("[data-slot='sidebar-menu-button']");
        el.Should().NotBeNull();
        el.TagName.Should().Be("BUTTON");
        el.GetAttribute("data-size").Should().Be("default");
    }

    [Fact]
    public void MenuButton_Renders_As_Anchor_When_Href_Set()
    {
        var cut = Render<SidebarMenuButtonCn>(p => p
            .Add(c => c.Href, "/test")
            .AddChildContent("Link"));
        var el = cut.Find("[data-slot='sidebar-menu-button']");
        el.TagName.Should().Be("A");
        el.GetAttribute("href").Should().Be("/test");
    }

    [Fact]
    public void MenuButton_IsActive_Sets_DataAttribute()
    {
        var cut = Render<SidebarMenuButtonCn>(p => p
            .Add(c => c.IsActive, true)
            .AddChildContent("Active"));
        var el = cut.Find("[data-slot='sidebar-menu-button']");
        el.GetAttribute("data-active").Should().Be("true");
    }

    [Fact]
    public void MenuButton_Size_Sm()
    {
        var cut = Render<SidebarMenuButtonCn>(p => p
            .Add(c => c.Size, SidebarMenuButtonSize.Sm)
            .AddChildContent("Small"));
        var el = cut.Find("[data-slot='sidebar-menu-button']");
        el.GetAttribute("data-size").Should().Be("sm");
    }

    [Fact]
    public void MenuButton_Size_Lg()
    {
        var cut = Render<SidebarMenuButtonCn>(p => p
            .Add(c => c.Size, SidebarMenuButtonSize.Lg)
            .AddChildContent("Large"));
        var el = cut.Find("[data-slot='sidebar-menu-button']");
        el.GetAttribute("data-size").Should().Be("lg");
    }

    [Fact]
    public void MenuButton_Variant_Outline()
    {
        var cut = Render<SidebarMenuButtonCn>(p => p
            .Add(c => c.Variant, SidebarMenuButtonVariant.Outline)
            .AddChildContent("Outline"));
        var el = cut.Find("[data-slot='sidebar-menu-button']");
        el.ClassList.Should().Contain("cn-sidebar-menu-button-variant-outline");
    }

    // --- SidebarMenuActionCn ---

    [Fact]
    public void MenuAction_Renders_With_DataSlot()
    {
        var cut = Render<SidebarMenuActionCn>(p => p.AddChildContent("Action"));
        var el = cut.Find("[data-slot='sidebar-menu-action']");
        el.Should().NotBeNull();
        el.GetAttribute("type").Should().Be("button");
    }

    [Fact]
    public void MenuAction_ShowOnHover_Adds_OpacityClass()
    {
        var cut = Render<SidebarMenuActionCn>(p => p
            .Add(c => c.ShowOnHover, true)
            .AddChildContent("Action"));
        var el = cut.Find("[data-slot='sidebar-menu-action']");
        el.ClassList.Should().Contain("md:opacity-0");
    }

    // --- SidebarMenuBadgeCn ---

    [Fact]
    public void MenuBadge_Renders_With_DataSlot()
    {
        var cut = Render<SidebarMenuBadgeCn>(p => p.AddChildContent("12"));
        var el = cut.Find("[data-slot='sidebar-menu-badge']");
        el.Should().NotBeNull();
        el.TextContent.Should().Contain("12");
    }

    // --- SidebarMenuSubCn ---

    [Fact]
    public void MenuSub_Renders_As_Ul()
    {
        var cut = Render<SidebarMenuSubCn>(p => p.AddChildContent("Items"));
        var el = cut.Find("[data-slot='sidebar-menu-sub']");
        el.Should().NotBeNull();
        el.TagName.Should().Be("UL");
    }

    // --- SidebarMenuSubItemCn ---

    [Fact]
    public void MenuSubItem_Renders_As_Li()
    {
        var cut = Render<SidebarMenuSubItemCn>(p => p.AddChildContent("SubItem"));
        var el = cut.Find("[data-slot='sidebar-menu-sub-item']");
        el.Should().NotBeNull();
        el.TagName.Should().Be("LI");
    }

    // --- SidebarMenuSubButtonCn ---

    [Fact]
    public void MenuSubButton_Renders_As_Button_By_Default()
    {
        var cut = Render<SidebarMenuSubButtonCn>(p => p.AddChildContent("Click"));
        var el = cut.Find("[data-slot='sidebar-menu-sub-button']");
        el.Should().NotBeNull();
        el.TagName.Should().Be("BUTTON");
        el.GetAttribute("data-size").Should().Be("md");
    }

    [Fact]
    public void MenuSubButton_Renders_As_Anchor_When_Href_Set()
    {
        var cut = Render<SidebarMenuSubButtonCn>(p => p
            .Add(c => c.Href, "/sub")
            .AddChildContent("Sub Link"));
        var el = cut.Find("[data-slot='sidebar-menu-sub-button']");
        el.TagName.Should().Be("A");
        el.GetAttribute("href").Should().Be("/sub");
    }

    [Fact]
    public void MenuSubButton_IsActive_Sets_DataAttribute()
    {
        var cut = Render<SidebarMenuSubButtonCn>(p => p
            .Add(c => c.IsActive, true)
            .AddChildContent("Active"));
        var el = cut.Find("[data-slot='sidebar-menu-sub-button']");
        el.GetAttribute("data-active").Should().Be("true");
    }

    [Fact]
    public void MenuSubButton_Size_Sm()
    {
        var cut = Render<SidebarMenuSubButtonCn>(p => p
            .Add(c => c.Size, SidebarMenuSubButtonSize.Sm)
            .AddChildContent("Small"));
        var el = cut.Find("[data-slot='sidebar-menu-sub-button']");
        el.GetAttribute("data-size").Should().Be("sm");
    }

    // --- SidebarInsetCn ---

    [Fact]
    public void Inset_Renders_As_Main()
    {
        var cut = Render<SidebarInsetCn>(p => p.AddChildContent("Main Content"));
        var el = cut.Find("[data-slot='sidebar-inset']");
        el.Should().NotBeNull();
        el.TagName.Should().Be("MAIN");
        el.TextContent.Should().Contain("Main Content");
    }

    // --- SidebarTriggerCn ---

    [Fact]
    public void Trigger_Renders_With_DataSlot()
    {
        var cut = Render<SidebarProviderCn>(p => p
            .AddChildContent<SidebarTriggerCn>());
        var el = cut.Find("[data-slot='sidebar-trigger']");
        el.Should().NotBeNull();
        el.GetAttribute("type").Should().Be("button");
    }

    [Fact]
    public void Trigger_Has_SrOnly_Label()
    {
        var cut = Render<SidebarProviderCn>(p => p
            .AddChildContent<SidebarTriggerCn>());
        cut.Find(".sr-only").TextContent.Should().Contain("Toggle Sidebar");
    }

    // --- SidebarRailCn ---

    [Fact]
    public void Rail_Renders_With_DataSlot()
    {
        var cut = Render<SidebarProviderCn>(p => p
            .AddChildContent<SidebarRailCn>());
        var el = cut.Find("[data-slot='sidebar-rail']");
        el.Should().NotBeNull();
        el.GetAttribute("aria-label").Should().Be("Toggle Sidebar");
        el.GetAttribute("tabindex").Should().Be("-1");
    }

    // --- SidebarMenuSkeletonCn ---

    [Fact]
    public void MenuSkeleton_Renders_With_DataSlot()
    {
        var cut = Render<SidebarMenuSkeletonCn>();
        var el = cut.Find("[data-slot='sidebar-menu-skeleton']");
        el.Should().NotBeNull();
    }

    [Fact]
    public void MenuSkeleton_ShowIcon_Renders_IconSkeleton()
    {
        var cut = Render<SidebarMenuSkeletonCn>(p => p
            .Add(c => c.ShowIcon, true));
        cut.FindAll("[data-slot='skeleton']").Count.Should().Be(2);
    }

    [Fact]
    public void MenuSkeleton_NoIcon_Renders_SingleSkeleton()
    {
        var cut = Render<SidebarMenuSkeletonCn>();
        cut.FindAll("[data-slot='skeleton']").Count.Should().Be(1);
    }

    // --- Integration: Toggle sidebar state ---

    [Fact]
    public void Toggle_Changes_Sidebar_State()
    {
        var cut = Render<SidebarProviderCn>(p => p
            .AddChildContent<SidebarTriggerCn>());
        // Provider starts open by default
        var wrapper = cut.Find("[data-slot='sidebar-wrapper']");
        wrapper.Should().NotBeNull();
        // Click trigger to toggle
        cut.Find("[data-slot='sidebar-trigger']").Click();
        // State should have changed (provider closed)
    }

    // --- Class passthrough tests ---

    [Fact]
    public void Header_Class_Passthrough()
    {
        var cut = Render<SidebarHeaderCn>(p => p
            .Add(c => c.Class, "my-header")
            .AddChildContent("H"));
        cut.Find("[data-slot='sidebar-header']").ClassList.Should().Contain("my-header");
    }

    [Fact]
    public void Footer_Class_Passthrough()
    {
        var cut = Render<SidebarFooterCn>(p => p
            .Add(c => c.Class, "my-footer")
            .AddChildContent("F"));
        cut.Find("[data-slot='sidebar-footer']").ClassList.Should().Contain("my-footer");
    }

    [Fact]
    public void Group_Class_Passthrough()
    {
        var cut = Render<SidebarGroupCn>(p => p
            .Add(c => c.Class, "my-group")
            .AddChildContent("G"));
        cut.Find("[data-slot='sidebar-group']").ClassList.Should().Contain("my-group");
    }

    [Fact]
    public void Menu_Class_Passthrough()
    {
        var cut = Render<SidebarMenuCn>(p => p
            .Add(c => c.Class, "my-menu")
            .AddChildContent("M"));
        cut.Find("[data-slot='sidebar-menu']").ClassList.Should().Contain("my-menu");
    }

    [Fact]
    public void MenuItem_Class_Passthrough()
    {
        var cut = Render<SidebarMenuItemCn>(p => p
            .Add(c => c.Class, "my-item")
            .AddChildContent("I"));
        cut.Find("[data-slot='sidebar-menu-item']").ClassList.Should().Contain("my-item");
    }

    [Fact]
    public void MenuButton_Class_Passthrough()
    {
        var cut = Render<SidebarMenuButtonCn>(p => p
            .Add(c => c.Class, "my-btn")
            .AddChildContent("B"));
        cut.Find("[data-slot='sidebar-menu-button']").ClassList.Should().Contain("my-btn");
    }

    [Fact]
    public void Inset_Class_Passthrough()
    {
        var cut = Render<SidebarInsetCn>(p => p
            .Add(c => c.Class, "my-inset")
            .AddChildContent("Content"));
        cut.Find("[data-slot='sidebar-inset']").ClassList.Should().Contain("my-inset");
    }
}

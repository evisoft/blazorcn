using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BlazorCN.Tests.Components;

public class ContextMenuCnTests : BunitContext
{
    private void SetupJsInterop()
    {
        var module = JSInterop.SetupModule("./_content/BlazorCN/blazorcn.js");
        module.Setup<string>("createFloating", _ => true).SetResult("right");
        module.SetupVoid("onOutsideClick", _ => true).SetVoidResult();
        module.SetupVoid("setupKeyboardNavigation", _ => true).SetVoidResult();
        module.SetupVoid("destroyFloating", _ => true).SetVoidResult();
        module.SetupVoid("cleanup", _ => true).SetVoidResult();
        module.SetupVoid("cleanupKeyboardNavigation", _ => true).SetVoidResult();
        Services.AddScoped<JsInteropCn>();
    }

    // --- ContextMenuCn ---

    [Fact]
    public void ContextMenu_Renders_With_DataSlot()
    {
        var cut = Render<ContextMenuCn>(p => p.AddChildContent("Content"));
        cut.Find("[data-slot='context-menu']").Should().NotBeNull();
    }

    [Fact]
    public void ContextMenu_Starts_Closed_By_Default()
    {
        var cut = Render<ContextMenuCn>(p => p.AddChildContent("Content"));
        cut.Find("[data-slot='context-menu']").GetAttribute("data-state").Should().Be("closed");
    }

    [Fact]
    public void ContextMenu_Has_Default_Classes()
    {
        var cut = Render<ContextMenuCn>(p => p.AddChildContent("Content"));
        var el = cut.Find("[data-slot='context-menu']");
        el.ClassList.Should().Contain("relative");
        el.ClassList.Should().Contain("inline-block");
    }

    [Fact]
    public void ContextMenu_Class_Passthrough()
    {
        var cut = Render<ContextMenuCn>(p => p
            .Add(c => c.Class, "custom-ctx")
            .AddChildContent("Content"));
        cut.Find("[data-slot='context-menu']").ClassList.Should().Contain("custom-ctx");
    }

    [Fact]
    public void ContextMenu_AdditionalAttributes_Passthrough()
    {
        var cut = Render<ContextMenuCn>(p => p
            .Add(c => c.AdditionalAttributes, new Dictionary<string, object?> { { "id", "ctx-menu" } })
            .AddChildContent("Content"));
        cut.Find("[data-slot='context-menu']").GetAttribute("id").Should().Be("ctx-menu");
    }

    // --- ContextMenuTriggerCn ---

    [Fact]
    public void ContextMenuTrigger_Renders_With_DataSlot()
    {
        var cut = Render<ContextMenuCn>(p => p
            .AddChildContent<ContextMenuTriggerCn>(t => t
                .AddChildContent("Right-click me")));
        cut.Find("[data-slot='context-menu-trigger']").Should().NotBeNull();
    }

    [Fact]
    public void ContextMenuTrigger_Has_SelectNone_Class()
    {
        var cut = Render<ContextMenuCn>(p => p
            .AddChildContent<ContextMenuTriggerCn>(t => t
                .AddChildContent("Right-click me")));
        cut.Find("[data-slot='context-menu-trigger']").ClassList.Should().Contain("select-none");
    }

    [Fact]
    public void ContextMenuTrigger_RightClick_Opens_Menu()
    {
        var isOpen = false;
        var cut = Render<ContextMenuCn>(p => p
            .Add(c => c.OpenChanged, EventCallback.Factory.Create<bool>(this, v => isOpen = v))
            .AddChildContent<ContextMenuTriggerCn>(t => t
                .AddChildContent("Right-click me")));
        cut.Find("[data-slot='context-menu-trigger']").TriggerEvent("oncontextmenu", new MouseEventArgs { ClientX = 100, ClientY = 200 });
        isOpen.Should().BeTrue();
        cut.Find("[data-slot='context-menu']").GetAttribute("data-state").Should().Be("open");
    }

    [Fact]
    public void ContextMenuTrigger_Class_Passthrough()
    {
        var cut = Render<ContextMenuCn>(p => p
            .AddChildContent<ContextMenuTriggerCn>(t => t
                .Add(c => c.Class, "trigger-class")
                .AddChildContent("Right-click")));
        cut.Find("[data-slot='context-menu-trigger']").ClassList.Should().Contain("trigger-class");
    }

    // --- ContextMenuContentCn ---

    [Fact]
    public void ContextMenuContent_Not_Rendered_When_Closed()
    {
        SetupJsInterop();
        var cut = Render<ContextMenuCn>(p => p
            .AddChildContent<ContextMenuContentCn>(c => c
                .AddChildContent("Menu body")));
        cut.FindAll("[data-slot='context-menu-content']").Should().BeEmpty();
    }

    [Fact]
    public void ContextMenuContent_Rendered_When_Open()
    {
        SetupJsInterop();
        var cut = Render<ContextMenuCn>(p => p.AddChildContent(builder =>
        {
            builder.OpenComponent<ContextMenuTriggerCn>(0);
            builder.AddAttribute(1, "ChildContent", (RenderFragment)(b => b.AddContent(0, "Right-click")));
            builder.CloseComponent();
            builder.OpenComponent<ContextMenuContentCn>(2);
            builder.AddAttribute(3, "ChildContent", (RenderFragment)(b => b.AddContent(0, "Menu body")));
            builder.CloseComponent();
        }));

        cut.Find("[data-slot='context-menu-trigger']").TriggerEvent("oncontextmenu", new MouseEventArgs { ClientX = 150, ClientY = 250 });
        cut.Find("[data-slot='context-menu-content']").Should().NotBeNull();
        cut.Find("[data-slot='context-menu-content']").TextContent.Should().Contain("Menu body");
    }

    [Fact]
    public void ContextMenuContent_Has_Default_Classes()
    {
        SetupJsInterop();
        var cut = Render<ContextMenuCn>(p => p.AddChildContent(builder =>
        {
            builder.OpenComponent<ContextMenuTriggerCn>(0);
            builder.AddAttribute(1, "ChildContent", (RenderFragment)(b => b.AddContent(0, "Right-click")));
            builder.CloseComponent();
            builder.OpenComponent<ContextMenuContentCn>(2);
            builder.AddAttribute(3, "ChildContent", (RenderFragment)(b => b.AddContent(0, "Body")));
            builder.CloseComponent();
        }));

        cut.Find("[data-slot='context-menu-trigger']").TriggerEvent("oncontextmenu", new MouseEventArgs { ClientX = 100, ClientY = 200 });
        var content = cut.Find("[data-slot='context-menu-content']");
        content.ClassList.Should().Contain("cn-context-menu-content");
        content.ClassList.Should().Contain("z-50");
        content.ClassList.Should().Contain("overflow-y-auto");
    }

    [Fact]
    public void ContextMenuContent_Has_Fixed_Position_At_MouseCoords()
    {
        SetupJsInterop();
        var cut = Render<ContextMenuCn>(p => p.AddChildContent(builder =>
        {
            builder.OpenComponent<ContextMenuTriggerCn>(0);
            builder.AddAttribute(1, "ChildContent", (RenderFragment)(b => b.AddContent(0, "Right-click")));
            builder.CloseComponent();
            builder.OpenComponent<ContextMenuContentCn>(2);
            builder.AddAttribute(3, "ChildContent", (RenderFragment)(b => b.AddContent(0, "Body")));
            builder.CloseComponent();
        }));

        cut.Find("[data-slot='context-menu-trigger']").TriggerEvent("oncontextmenu", new MouseEventArgs { ClientX = 150, ClientY = 250 });
        var content = cut.Find("[data-slot='context-menu-content']");
        var style = content.GetAttribute("style") ?? "";
        style.Should().Contain("position:fixed");
        style.Should().Contain("left:150px");
        style.Should().Contain("top:250px");
    }

    [Fact]
    public void ContextMenuContent_Class_Passthrough()
    {
        SetupJsInterop();
        var cut = Render<ContextMenuCn>(p => p.AddChildContent(builder =>
        {
            builder.OpenComponent<ContextMenuTriggerCn>(0);
            builder.AddAttribute(1, "ChildContent", (RenderFragment)(b => b.AddContent(0, "Right-click")));
            builder.CloseComponent();
            builder.OpenComponent<ContextMenuContentCn>(2);
            builder.AddAttribute(3, "ChildContent", (RenderFragment)(b => b.AddContent(0, "Body")));
            builder.AddAttribute(4, "Class", "custom-content");
            builder.CloseComponent();
        }));

        cut.Find("[data-slot='context-menu-trigger']").TriggerEvent("oncontextmenu", new MouseEventArgs { ClientX = 100, ClientY = 200 });
        cut.Find("[data-slot='context-menu-content']").ClassList.Should().Contain("custom-content");
    }

    // --- ContextMenuItemCn ---

    [Fact]
    public void ContextMenuItem_Renders_With_DataSlot()
    {
        var cut = Render<ContextMenuItemCn>(p => p.AddChildContent("Item"));
        cut.Find("[data-slot='context-menu-item']").Should().NotBeNull();
    }

    [Fact]
    public void ContextMenuItem_Has_DataMenuItem_Attribute()
    {
        var cut = Render<ContextMenuItemCn>(p => p.AddChildContent("Item"));
        cut.Find("[data-menu-item]").Should().NotBeNull();
    }

    [Fact]
    public void ContextMenuItem_Has_Default_Classes()
    {
        var cut = Render<ContextMenuItemCn>(p => p.AddChildContent("Item"));
        var item = cut.Find("[data-slot='context-menu-item']");
        item.ClassList.Should().Contain("cn-context-menu-item");
        item.ClassList.Should().Contain("relative");
        item.ClassList.Should().Contain("flex");
        item.ClassList.Should().Contain("cursor-default");
        item.ClassList.Should().Contain("select-none");
        item.ClassList.Should().Contain("items-center");
        item.ClassList.Should().Contain("outline-hidden");
    }

    [Fact]
    public void ContextMenuItem_Disabled_Has_DataDisabled()
    {
        var cut = Render<ContextMenuItemCn>(p => p
            .Add(c => c.Disabled, true)
            .AddChildContent("Item"));
        cut.Find("[data-slot='context-menu-item']").GetAttribute("data-disabled").Should().Be("true");
    }

    [Fact]
    public void ContextMenuItem_Destructive_Variant()
    {
        var cut = Render<ContextMenuItemCn>(p => p
            .Add(c => c.Variant, "destructive")
            .AddChildContent("Delete"));
        cut.Find("[data-slot='context-menu-item']").GetAttribute("data-variant").Should().Be("destructive");
    }

    [Fact]
    public void ContextMenuItem_Inset_Has_PaddingLeft()
    {
        var cut = Render<ContextMenuItemCn>(p => p
            .Add(c => c.Inset, true)
            .AddChildContent("Item"));
        cut.Find("[data-slot='context-menu-item']").GetAttribute("data-inset").Should().NotBeNull();
    }

    [Fact]
    public void ContextMenuItem_OnClick_Fires()
    {
        var clicked = false;
        var cut = Render<ContextMenuItemCn>(p => p
            .Add(c => c.OnClick, () => clicked = true)
            .AddChildContent("Item"));
        cut.Find("[data-slot='context-menu-item']").Click();
        clicked.Should().BeTrue();
    }

    [Fact]
    public void ContextMenuItem_Disabled_OnClick_DoesNotFire()
    {
        var clicked = false;
        var cut = Render<ContextMenuItemCn>(p => p
            .Add(c => c.Disabled, true)
            .Add(c => c.OnClick, () => clicked = true)
            .AddChildContent("Item"));
        cut.Find("[data-slot='context-menu-item']").Click();
        clicked.Should().BeFalse();
    }

    [Fact]
    public void ContextMenuItem_Class_Passthrough()
    {
        var cut = Render<ContextMenuItemCn>(p => p
            .Add(c => c.Class, "custom-item")
            .AddChildContent("Item"));
        cut.Find("[data-slot='context-menu-item']").ClassList.Should().Contain("custom-item");
    }

    // --- ContextMenuGroupCn ---

    [Fact]
    public void ContextMenuGroup_Renders_With_DataSlot()
    {
        var cut = Render<ContextMenuGroupCn>(p => p.AddChildContent("Group"));
        cut.Find("[data-slot='context-menu-group']").Should().NotBeNull();
    }

    [Fact]
    public void ContextMenuGroup_Has_Role_Group()
    {
        var cut = Render<ContextMenuGroupCn>(p => p.AddChildContent("Group"));
        cut.Find("[data-slot='context-menu-group']").GetAttribute("role").Should().Be("group");
    }

    // --- ContextMenuLabelCn ---

    [Fact]
    public void ContextMenuLabel_Renders_With_DataSlot()
    {
        var cut = Render<ContextMenuLabelCn>(p => p.AddChildContent("Label"));
        cut.Find("[data-slot='context-menu-label']").Should().NotBeNull();
    }

    [Fact]
    public void ContextMenuLabel_Has_Default_Classes()
    {
        var cut = Render<ContextMenuLabelCn>(p => p.AddChildContent("Label"));
        var el = cut.Find("[data-slot='context-menu-label']");
        el.ClassList.Should().Contain("cn-context-menu-label");
    }

    [Fact]
    public void ContextMenuLabel_Inset_Has_PaddingLeft()
    {
        var cut = Render<ContextMenuLabelCn>(p => p
            .Add(c => c.Inset, true)
            .AddChildContent("Label"));
        cut.Find("[data-slot='context-menu-label']").GetAttribute("data-inset").Should().NotBeNull();
    }

    // --- ContextMenuSeparatorCn ---

    [Fact]
    public void ContextMenuSeparator_Renders_With_DataSlot()
    {
        var cut = Render<ContextMenuSeparatorCn>();
        cut.Find("[data-slot='context-menu-separator']").Should().NotBeNull();
    }

    [Fact]
    public void ContextMenuSeparator_Has_Role_Separator()
    {
        var cut = Render<ContextMenuSeparatorCn>();
        cut.Find("[data-slot='context-menu-separator']").GetAttribute("role").Should().Be("separator");
    }

    [Fact]
    public void ContextMenuSeparator_Has_Default_Classes()
    {
        var cut = Render<ContextMenuSeparatorCn>();
        var el = cut.Find("[data-slot='context-menu-separator']");
        el.ClassList.Should().Contain("cn-context-menu-separator");
    }

    // --- ContextMenuShortcutCn ---

    [Fact]
    public void ContextMenuShortcut_Renders_With_DataSlot()
    {
        var cut = Render<ContextMenuShortcutCn>(p => p.AddChildContent("Ctrl+Z"));
        cut.Find("[data-slot='context-menu-shortcut']").Should().NotBeNull();
    }

    [Fact]
    public void ContextMenuShortcut_Has_Default_Classes()
    {
        var cut = Render<ContextMenuShortcutCn>(p => p.AddChildContent("Ctrl+Z"));
        var el = cut.Find("[data-slot='context-menu-shortcut']");
        el.ClassList.Should().Contain("cn-context-menu-shortcut");
    }

    [Fact]
    public void ContextMenuShortcut_Renders_Content()
    {
        var cut = Render<ContextMenuShortcutCn>(p => p.AddChildContent("Ctrl+Z"));
        cut.Find("[data-slot='context-menu-shortcut']").TextContent.Trim().Should().Be("Ctrl+Z");
    }

    // --- ContextMenuCheckboxItemCn ---

    [Fact]
    public void ContextMenuCheckboxItem_Renders_With_DataSlot()
    {
        var cut = Render<ContextMenuCheckboxItemCn>(p => p.AddChildContent("Check me"));
        cut.Find("[data-slot='context-menu-checkbox-item']").Should().NotBeNull();
    }

    [Fact]
    public void ContextMenuCheckboxItem_Unchecked_By_Default()
    {
        var cut = Render<ContextMenuCheckboxItemCn>(p => p.AddChildContent("Check me"));
        cut.Find("[data-slot='context-menu-checkbox-item']").GetAttribute("data-state").Should().Be("unchecked");
    }

    [Fact]
    public void ContextMenuCheckboxItem_Checked_Shows_Icon()
    {
        var cut = Render<ContextMenuCheckboxItemCn>(p => p
            .Add(c => c.Checked, true)
            .AddChildContent("Check me"));
        cut.Find("[data-slot='context-menu-checkbox-item']").GetAttribute("data-state").Should().Be("checked");
        cut.FindAll("svg").Should().NotBeEmpty();
    }

    [Fact]
    public void ContextMenuCheckboxItem_Click_Toggles()
    {
        var isChecked = false;
        var cut = Render<ContextMenuCheckboxItemCn>(p => p
            .Add(c => c.CheckedChanged, EventCallback.Factory.Create<bool>(this, v => isChecked = v))
            .AddChildContent("Check me"));
        cut.Find("[data-slot='context-menu-checkbox-item']").Click();
        isChecked.Should().BeTrue();
    }

    [Fact]
    public void ContextMenuCheckboxItem_Has_Role()
    {
        var cut = Render<ContextMenuCheckboxItemCn>(p => p.AddChildContent("Check me"));
        cut.Find("[data-slot='context-menu-checkbox-item']").GetAttribute("role").Should().Be("menuitemcheckbox");
    }

    [Fact]
    public void ContextMenuCheckboxItem_Has_Nova_Item_Class()
    {
        // Indicator padding (pr-8/pl-1.5) now comes from the cn-* nova CSS class, not a pl-8 utility.
        var cut = Render<ContextMenuCheckboxItemCn>(p => p.AddChildContent("Check me"));
        cut.Find("[data-slot='context-menu-checkbox-item']").ClassList.Should().Contain("cn-context-menu-checkbox-item");
    }

    // --- ContextMenuRadioGroupCn ---

    [Fact]
    public void ContextMenuRadioGroup_Renders_With_DataSlot()
    {
        var cut = Render<ContextMenuRadioGroupCn>(p => p.AddChildContent("Radio"));
        cut.Find("[data-slot='context-menu-radio-group']").Should().NotBeNull();
    }

    [Fact]
    public void ContextMenuRadioGroup_Has_Role_Radiogroup()
    {
        var cut = Render<ContextMenuRadioGroupCn>(p => p.AddChildContent("Radio"));
        cut.Find("[data-slot='context-menu-radio-group']").GetAttribute("role").Should().Be("radiogroup");
    }

    // --- ContextMenuRadioItemCn ---

    [Fact]
    public void ContextMenuRadioItem_Renders_With_DataSlot()
    {
        var cut = Render<ContextMenuRadioGroupCn>(p => p
            .Add(c => c.Value, "a")
            .AddChildContent<ContextMenuRadioItemCn>(i => i
                .Add(x => x.Value, "a")
                .AddChildContent("Option A")));
        cut.Find("[data-slot='context-menu-radio-item']").Should().NotBeNull();
    }

    [Fact]
    public void ContextMenuRadioItem_Selected_Shows_Dot()
    {
        var cut = Render<ContextMenuRadioGroupCn>(p => p
            .Add(c => c.Value, "a")
            .AddChildContent<ContextMenuRadioItemCn>(i => i
                .Add(x => x.Value, "a")
                .AddChildContent("Option A")));
        cut.Find("[data-slot='context-menu-radio-item']").GetAttribute("data-state").Should().Be("checked");
        cut.FindAll("svg").Should().NotBeEmpty();
    }

    [Fact]
    public void ContextMenuRadioItem_NotSelected_No_Dot()
    {
        var cut = Render<ContextMenuRadioGroupCn>(p => p
            .Add(c => c.Value, "b")
            .AddChildContent<ContextMenuRadioItemCn>(i => i
                .Add(x => x.Value, "a")
                .AddChildContent("Option A")));
        cut.Find("[data-slot='context-menu-radio-item']").GetAttribute("data-state").Should().Be("unchecked");
        cut.FindAll("svg").Should().BeEmpty();
    }

    [Fact]
    public void ContextMenuRadioItem_Click_Selects()
    {
        var selected = "";
        var cut = Render<ContextMenuRadioGroupCn>(p => p
            .Add(c => c.Value, "b")
            .Add(c => c.ValueChanged, EventCallback.Factory.Create<string>(this, v => selected = v))
            .AddChildContent<ContextMenuRadioItemCn>(i => i
                .Add(x => x.Value, "a")
                .AddChildContent("Option A")));
        cut.Find("[data-slot='context-menu-radio-item']").Click();
        selected.Should().Be("a");
    }

    [Fact]
    public void ContextMenuRadioItem_Has_Role()
    {
        var cut = Render<ContextMenuRadioGroupCn>(p => p
            .Add(c => c.Value, "a")
            .AddChildContent<ContextMenuRadioItemCn>(i => i
                .Add(x => x.Value, "a")
                .AddChildContent("Option A")));
        cut.Find("[data-slot='context-menu-radio-item']").GetAttribute("role").Should().Be("menuitemradio");
    }

    // --- ContextMenuSubCn ---

    [Fact]
    public void ContextMenuSub_Renders_With_DataSlot()
    {
        var cut = Render<ContextMenuSubCn>(p => p.AddChildContent("Sub"));
        cut.Find("[data-slot='context-menu-sub']").Should().NotBeNull();
    }

    [Fact]
    public void ContextMenuSub_Starts_Closed()
    {
        var cut = Render<ContextMenuSubCn>(p => p.AddChildContent("Sub"));
        cut.Find("[data-slot='context-menu-sub']").GetAttribute("data-state").Should().Be("closed");
    }

    // --- ContextMenuSubTriggerCn ---

    [Fact]
    public void ContextMenuSubTrigger_Renders_With_DataSlot()
    {
        var cut = Render<ContextMenuSubCn>(p => p
            .AddChildContent<ContextMenuSubTriggerCn>(t => t
                .AddChildContent("More")));
        cut.Find("[data-slot='context-menu-sub-trigger']").Should().NotBeNull();
    }

    [Fact]
    public void ContextMenuSubTrigger_Has_Chevron()
    {
        var cut = Render<ContextMenuSubCn>(p => p
            .AddChildContent<ContextMenuSubTriggerCn>(t => t
                .AddChildContent("More")));
        cut.FindAll("svg").Should().NotBeEmpty();
    }

    [Fact]
    public void ContextMenuSubTrigger_Click_Opens_Sub()
    {
        var cut = Render<ContextMenuSubCn>(p => p
            .AddChildContent<ContextMenuSubTriggerCn>(t => t
                .AddChildContent("More")));
        cut.Find("[data-slot='context-menu-sub-trigger']").Click();
        cut.Find("[data-slot='context-menu-sub']").GetAttribute("data-state").Should().Be("open");
    }

    [Fact]
    public void ContextMenuSubTrigger_Has_Default_Classes()
    {
        var cut = Render<ContextMenuSubCn>(p => p
            .AddChildContent<ContextMenuSubTriggerCn>(t => t
                .AddChildContent("More")));
        var el = cut.Find("[data-slot='context-menu-sub-trigger']");
        el.ClassList.Should().Contain("cn-context-menu-sub-trigger");
        el.ClassList.Should().Contain("flex");
        el.ClassList.Should().Contain("cursor-default");
        el.ClassList.Should().Contain("select-none");
        el.ClassList.Should().Contain("items-center");
    }

    // --- ContextMenuSubContentCn ---

    [Fact]
    public void ContextMenuSubContent_Not_Rendered_When_Closed()
    {
        SetupJsInterop();
        var cut = Render<ContextMenuSubCn>(p => p
            .AddChildContent<ContextMenuSubContentCn>(c => c
                .AddChildContent("Sub content")));
        cut.FindAll("[data-slot='context-menu-sub-content']").Should().BeEmpty();
    }

    [Fact]
    public void ContextMenuSubContent_Rendered_When_Open()
    {
        SetupJsInterop();
        var cut = Render<ContextMenuSubCn>(p => p.AddChildContent(builder =>
        {
            builder.OpenComponent<ContextMenuSubTriggerCn>(0);
            builder.AddAttribute(1, "ChildContent", (RenderFragment)(b => b.AddContent(0, "More")));
            builder.CloseComponent();
            builder.OpenComponent<ContextMenuSubContentCn>(2);
            builder.AddAttribute(3, "ChildContent", (RenderFragment)(b => b.AddContent(0, "Sub items")));
            builder.CloseComponent();
        }));

        cut.Find("[data-slot='context-menu-sub-trigger']").Click();
        cut.Find("[data-slot='context-menu-sub-content']").Should().NotBeNull();
    }

    [Fact]
    public void ContextMenuSubContent_Default_Side_Is_Right()
    {
        SetupJsInterop();
        var cut = Render<ContextMenuSubCn>(p => p.AddChildContent(builder =>
        {
            builder.OpenComponent<ContextMenuSubTriggerCn>(0);
            builder.AddAttribute(1, "ChildContent", (RenderFragment)(b => b.AddContent(0, "More")));
            builder.CloseComponent();
            builder.OpenComponent<ContextMenuSubContentCn>(2);
            builder.AddAttribute(3, "ChildContent", (RenderFragment)(b => b.AddContent(0, "Sub")));
            builder.CloseComponent();
        }));

        cut.Find("[data-slot='context-menu-sub-trigger']").Click();
        cut.Find("[data-slot='context-menu-sub-content']").GetAttribute("data-side").Should().Be("right");
    }

    [Fact]
    public void ContextMenuSubContent_Has_Default_Classes()
    {
        SetupJsInterop();
        var cut = Render<ContextMenuSubCn>(p => p.AddChildContent(builder =>
        {
            builder.OpenComponent<ContextMenuSubTriggerCn>(0);
            builder.AddAttribute(1, "ChildContent", (RenderFragment)(b => b.AddContent(0, "More")));
            builder.CloseComponent();
            builder.OpenComponent<ContextMenuSubContentCn>(2);
            builder.AddAttribute(3, "ChildContent", (RenderFragment)(b => b.AddContent(0, "Sub")));
            builder.CloseComponent();
        }));

        cut.Find("[data-slot='context-menu-sub-trigger']").Click();
        var content = cut.Find("[data-slot='context-menu-sub-content']");
        content.ClassList.Should().Contain("cn-context-menu-sub-content");
        content.ClassList.Should().Contain("z-50");
        content.ClassList.Should().Contain("overflow-y-auto");
    }

    // --- ARIA ---

    [Fact]
    public void ContextMenuSubTrigger_AriaExpanded_Reflects_State()
    {
        var cut = Render<ContextMenuSubCn>(p => p
            .AddChildContent<ContextMenuSubTriggerCn>(t => t
                .AddChildContent("More")));
        var trigger = cut.Find("[data-slot='context-menu-sub-trigger']");
        trigger.GetAttribute("aria-expanded").Should().Be("false");
        trigger.Click();
        trigger.GetAttribute("aria-expanded").Should().Be("true");
    }

    [Fact]
    public void ContextMenuSubTrigger_Has_AriaHasPopup_Menu()
    {
        var cut = Render<ContextMenuSubCn>(p => p
            .AddChildContent<ContextMenuSubTriggerCn>(t => t
                .AddChildContent("More")));
        cut.Find("[data-slot='context-menu-sub-trigger']").GetAttribute("aria-haspopup").Should().Be("menu");
    }

    // --- Integration ---

    [Fact]
    public void ContextMenu_Full_Integration_RightClick()
    {
        SetupJsInterop();
        var isOpen = false;
        var cut = Render<ContextMenuCn>(p => p
            .Add(c => c.OpenChanged, EventCallback.Factory.Create<bool>(this, v => isOpen = v))
            .AddChildContent(builder =>
            {
                builder.OpenComponent<ContextMenuTriggerCn>(0);
                builder.AddAttribute(1, "ChildContent", (RenderFragment)(b => b.AddContent(0, "Right-click area")));
                builder.CloseComponent();
                builder.OpenComponent<ContextMenuContentCn>(2);
                builder.AddAttribute(3, "ChildContent", (RenderFragment)(b => b.AddContent(0, "Menu items")));
                builder.CloseComponent();
            }));

        // Initially closed
        cut.Find("[data-slot='context-menu']").GetAttribute("data-state").Should().Be("closed");
        cut.FindAll("[data-slot='context-menu-content']").Should().BeEmpty();

        // Right-click to open
        cut.Find("[data-slot='context-menu-trigger']").TriggerEvent("oncontextmenu", new MouseEventArgs { ClientX = 200, ClientY = 300 });
        isOpen.Should().BeTrue();
        cut.Find("[data-slot='context-menu']").GetAttribute("data-state").Should().Be("open");
        cut.Find("[data-slot='context-menu-content']").Should().NotBeNull();
    }
}

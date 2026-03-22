using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BlazorCN.Tests.Components;

public class DropdownMenuCnTests : BunitContext
{
    private void SetupJsInterop()
    {
        var module = JSInterop.SetupModule("./_content/BlazorCN/blazorcn.js");
        module.Setup<string>("createFloating", _ => true).SetResult("bottom");
        module.SetupVoid("onOutsideClick", _ => true).SetVoidResult();
        module.SetupVoid("setupKeyboardNavigation", _ => true).SetVoidResult();
        module.SetupVoid("destroyFloating", _ => true).SetVoidResult();
        module.SetupVoid("cleanup", _ => true).SetVoidResult();
        module.SetupVoid("cleanupKeyboardNavigation", _ => true).SetVoidResult();
        Services.AddScoped<JsInteropCn>();
    }

    // --- DropdownMenuCn ---

    [Fact]
    public void DropdownMenu_Renders_With_DataSlot()
    {
        var cut = Render<DropdownMenuCn>(p => p.AddChildContent("Content"));
        cut.Find("[data-slot='dropdown-menu']").Should().NotBeNull();
    }

    [Fact]
    public void DropdownMenu_Starts_Closed_By_Default()
    {
        var cut = Render<DropdownMenuCn>(p => p.AddChildContent("Content"));
        cut.Find("[data-slot='dropdown-menu']").GetAttribute("data-state").Should().Be("closed");
    }

    [Fact]
    public void DropdownMenu_Starts_Open_When_Open_Is_True()
    {
        var cut = Render<DropdownMenuCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent("Content"));
        cut.Find("[data-slot='dropdown-menu']").GetAttribute("data-state").Should().Be("open");
    }

    [Fact]
    public void DropdownMenu_Has_Default_Classes()
    {
        var cut = Render<DropdownMenuCn>(p => p.AddChildContent("Content"));
        var el = cut.Find("[data-slot='dropdown-menu']");
        el.ClassList.Should().Contain("relative");
        el.ClassList.Should().Contain("inline-block");
    }

    [Fact]
    public void DropdownMenu_Class_Passthrough()
    {
        var cut = Render<DropdownMenuCn>(p => p
            .Add(c => c.Class, "custom-class")
            .AddChildContent("Content"));
        cut.Find("[data-slot='dropdown-menu']").ClassList.Should().Contain("custom-class");
    }

    [Fact]
    public void DropdownMenu_AdditionalAttributes_Passthrough()
    {
        var cut = Render<DropdownMenuCn>(p => p
            .Add(c => c.AdditionalAttributes, new Dictionary<string, object?> { { "id", "my-menu" } })
            .AddChildContent("Content"));
        cut.Find("[data-slot='dropdown-menu']").GetAttribute("id").Should().Be("my-menu");
    }

    // --- DropdownMenuTriggerCn ---

    [Fact]
    public void DropdownMenuTrigger_Renders_With_DataSlot()
    {
        var cut = Render<DropdownMenuCn>(p => p
            .AddChildContent<DropdownMenuTriggerCn>(t => t
                .AddChildContent("Open")));
        cut.Find("[data-slot='dropdown-menu-trigger']").Should().NotBeNull();
    }

    [Fact]
    public void DropdownMenuTrigger_Is_Button()
    {
        var cut = Render<DropdownMenuCn>(p => p
            .AddChildContent<DropdownMenuTriggerCn>(t => t
                .AddChildContent("Open")));
        cut.Find("[data-slot='dropdown-menu-trigger']").TagName.Should().Be("BUTTON");
    }

    [Fact]
    public void DropdownMenuTrigger_Click_Toggles_Open()
    {
        var isOpen = false;
        var cut = Render<DropdownMenuCn>(p => p
            .Add(c => c.OpenChanged, EventCallback.Factory.Create<bool>(this, v => isOpen = v))
            .AddChildContent<DropdownMenuTriggerCn>(t => t
                .AddChildContent("Open")));
        cut.Find("[data-slot='dropdown-menu-trigger']").Click();
        isOpen.Should().BeTrue();
        cut.Find("[data-slot='dropdown-menu']").GetAttribute("data-state").Should().Be("open");
    }

    [Fact]
    public void DropdownMenuTrigger_Click_Toggles_Closed()
    {
        var isOpen = true;
        var cut = Render<DropdownMenuCn>(p => p
            .Add(c => c.Open, true)
            .Add(c => c.OpenChanged, EventCallback.Factory.Create<bool>(this, v => isOpen = v))
            .AddChildContent<DropdownMenuTriggerCn>(t => t
                .AddChildContent("Close")));
        cut.Find("[data-slot='dropdown-menu-trigger']").Click();
        isOpen.Should().BeFalse();
    }

    [Fact]
    public void DropdownMenuTrigger_Class_Passthrough()
    {
        var cut = Render<DropdownMenuCn>(p => p
            .AddChildContent<DropdownMenuTriggerCn>(t => t
                .Add(c => c.Class, "trigger-class")
                .AddChildContent("Open")));
        cut.Find("[data-slot='dropdown-menu-trigger']").ClassList.Should().Contain("trigger-class");
    }

    // --- DropdownMenuContentCn ---

    [Fact]
    public void DropdownMenuContent_Not_Rendered_When_Closed()
    {
        SetupJsInterop();
        var cut = Render<DropdownMenuCn>(p => p
            .AddChildContent<DropdownMenuContentCn>(c => c
                .AddChildContent("Menu body")));
        cut.FindAll("[data-slot='dropdown-menu-content']").Should().BeEmpty();
    }

    [Fact]
    public void DropdownMenuContent_Rendered_When_Open()
    {
        SetupJsInterop();
        var cut = Render<DropdownMenuCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<DropdownMenuContentCn>(c => c
                .AddChildContent("Menu body")));
        cut.Find("[data-slot='dropdown-menu-content']").Should().NotBeNull();
        cut.Find("[data-slot='dropdown-menu-content']").TextContent.Should().Contain("Menu body");
    }

    [Fact]
    public void DropdownMenuContent_Has_Default_Classes()
    {
        SetupJsInterop();
        var cut = Render<DropdownMenuCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<DropdownMenuContentCn>(c => c
                .AddChildContent("Body")));
        var content = cut.Find("[data-slot='dropdown-menu-content']");
        content.ClassList.Should().Contain("z-50");
        content.ClassList.Should().Contain("overflow-hidden");
        content.ClassList.Should().Contain("rounded-md");
        content.ClassList.Should().Contain("border");
        content.ClassList.Should().Contain("bg-popover");
        content.ClassList.Should().Contain("p-1");
        content.ClassList.Should().Contain("text-popover-foreground");
        content.ClassList.Should().Contain("shadow-md");
    }

    [Fact]
    public void DropdownMenuContent_Default_Side_Is_Bottom()
    {
        SetupJsInterop();
        var cut = Render<DropdownMenuCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<DropdownMenuContentCn>(c => c
                .AddChildContent("Body")));
        cut.Find("[data-slot='dropdown-menu-content']").GetAttribute("data-side").Should().Be("bottom");
    }

    [Fact]
    public void DropdownMenuContent_Default_Align_Is_Start()
    {
        SetupJsInterop();
        var cut = Render<DropdownMenuCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<DropdownMenuContentCn>(c => c
                .AddChildContent("Body")));
        cut.Find("[data-slot='dropdown-menu-content']").GetAttribute("data-align").Should().Be("start");
    }

    [Fact]
    public void DropdownMenuContent_Class_Passthrough()
    {
        SetupJsInterop();
        var cut = Render<DropdownMenuCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<DropdownMenuContentCn>(c => c
                .Add(x => x.Class, "custom-content")
                .AddChildContent("Body")));
        cut.Find("[data-slot='dropdown-menu-content']").ClassList.Should().Contain("custom-content");
    }

    // --- DropdownMenuItemCn ---

    [Fact]
    public void DropdownMenuItem_Renders_With_DataSlot()
    {
        var cut = Render<DropdownMenuCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<DropdownMenuItemCn>(i => i
                .AddChildContent("Item 1")));
        cut.Find("[data-slot='dropdown-menu-item']").Should().NotBeNull();
    }

    [Fact]
    public void DropdownMenuItem_Has_DataMenuItem_Attribute()
    {
        var cut = Render<DropdownMenuCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<DropdownMenuItemCn>(i => i
                .AddChildContent("Item 1")));
        cut.Find("[data-menu-item]").Should().NotBeNull();
    }

    [Fact]
    public void DropdownMenuItem_Has_Default_Classes()
    {
        var cut = Render<DropdownMenuCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<DropdownMenuItemCn>(i => i
                .AddChildContent("Item")));
        var item = cut.Find("[data-slot='dropdown-menu-item']");
        item.ClassList.Should().Contain("relative");
        item.ClassList.Should().Contain("flex");
        item.ClassList.Should().Contain("cursor-default");
        item.ClassList.Should().Contain("select-none");
        item.ClassList.Should().Contain("items-center");
        item.ClassList.Should().Contain("gap-2");
        item.ClassList.Should().Contain("rounded-sm");
        item.ClassList.Should().Contain("px-2");
        item.ClassList.Should().Contain("py-1.5");
        item.ClassList.Should().Contain("text-sm");
        item.ClassList.Should().Contain("outline-none");
        item.ClassList.Should().Contain("transition-colors");
    }

    [Fact]
    public void DropdownMenuItem_Disabled_Has_DataDisabled()
    {
        var cut = Render<DropdownMenuCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<DropdownMenuItemCn>(i => i
                .Add(x => x.Disabled, true)
                .AddChildContent("Item")));
        cut.Find("[data-slot='dropdown-menu-item']").GetAttribute("data-disabled").Should().Be("true");
    }

    [Fact]
    public void DropdownMenuItem_NotDisabled_No_DataDisabled()
    {
        var cut = Render<DropdownMenuCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<DropdownMenuItemCn>(i => i
                .AddChildContent("Item")));
        cut.Find("[data-slot='dropdown-menu-item']").GetAttribute("data-disabled").Should().BeNull();
    }

    [Fact]
    public void DropdownMenuItem_Destructive_Variant_Classes()
    {
        var cut = Render<DropdownMenuCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<DropdownMenuItemCn>(i => i
                .Add(x => x.Variant, "destructive")
                .AddChildContent("Delete")));
        var item = cut.Find("[data-slot='dropdown-menu-item']");
        item.ClassList.Should().Contain("text-destructive");
    }

    [Fact]
    public void DropdownMenuItem_Inset_Has_PaddingLeft()
    {
        var cut = Render<DropdownMenuCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<DropdownMenuItemCn>(i => i
                .Add(x => x.Inset, true)
                .AddChildContent("Item")));
        cut.Find("[data-slot='dropdown-menu-item']").ClassList.Should().Contain("pl-8");
    }

    [Fact]
    public void DropdownMenuItem_OnClick_Fires()
    {
        var clicked = false;
        var cut = Render<DropdownMenuCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<DropdownMenuItemCn>(i => i
                .Add(x => x.OnClick, EventCallback.Factory.Create(this, () => clicked = true))
                .AddChildContent("Item")));
        cut.Find("[data-slot='dropdown-menu-item']").Click();
        clicked.Should().BeTrue();
    }

    [Fact]
    public void DropdownMenuItem_Disabled_OnClick_DoesNotFire()
    {
        var clicked = false;
        var cut = Render<DropdownMenuCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<DropdownMenuItemCn>(i => i
                .Add(x => x.Disabled, true)
                .Add(x => x.OnClick, EventCallback.Factory.Create(this, () => clicked = true))
                .AddChildContent("Item")));
        cut.Find("[data-slot='dropdown-menu-item']").Click();
        clicked.Should().BeFalse();
    }

    [Fact]
    public void DropdownMenuItem_Class_Passthrough()
    {
        var cut = Render<DropdownMenuCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<DropdownMenuItemCn>(i => i
                .Add(x => x.Class, "custom-item")
                .AddChildContent("Item")));
        cut.Find("[data-slot='dropdown-menu-item']").ClassList.Should().Contain("custom-item");
    }

    // --- DropdownMenuGroupCn ---

    [Fact]
    public void DropdownMenuGroup_Renders_With_DataSlot()
    {
        var cut = Render<DropdownMenuGroupCn>(p => p.AddChildContent("Group"));
        cut.Find("[data-slot='dropdown-menu-group']").Should().NotBeNull();
    }

    [Fact]
    public void DropdownMenuGroup_Has_Role_Group()
    {
        var cut = Render<DropdownMenuGroupCn>(p => p.AddChildContent("Group"));
        cut.Find("[data-slot='dropdown-menu-group']").GetAttribute("role").Should().Be("group");
    }

    [Fact]
    public void DropdownMenuGroup_Class_Passthrough()
    {
        var cut = Render<DropdownMenuGroupCn>(p => p
            .Add(c => c.Class, "custom-group")
            .AddChildContent("Group"));
        cut.Find("[data-slot='dropdown-menu-group']").ClassList.Should().Contain("custom-group");
    }

    // --- DropdownMenuLabelCn ---

    [Fact]
    public void DropdownMenuLabel_Renders_With_DataSlot()
    {
        var cut = Render<DropdownMenuLabelCn>(p => p.AddChildContent("Label"));
        cut.Find("[data-slot='dropdown-menu-label']").Should().NotBeNull();
    }

    [Fact]
    public void DropdownMenuLabel_Has_Default_Classes()
    {
        var cut = Render<DropdownMenuLabelCn>(p => p.AddChildContent("Label"));
        var el = cut.Find("[data-slot='dropdown-menu-label']");
        el.ClassList.Should().Contain("px-2");
        el.ClassList.Should().Contain("py-1.5");
        el.ClassList.Should().Contain("text-sm");
        el.ClassList.Should().Contain("font-semibold");
    }

    [Fact]
    public void DropdownMenuLabel_Inset_Has_PaddingLeft()
    {
        var cut = Render<DropdownMenuLabelCn>(p => p
            .Add(c => c.Inset, true)
            .AddChildContent("Label"));
        cut.Find("[data-slot='dropdown-menu-label']").ClassList.Should().Contain("pl-8");
    }

    [Fact]
    public void DropdownMenuLabel_Class_Passthrough()
    {
        var cut = Render<DropdownMenuLabelCn>(p => p
            .Add(c => c.Class, "custom-label")
            .AddChildContent("Label"));
        cut.Find("[data-slot='dropdown-menu-label']").ClassList.Should().Contain("custom-label");
    }

    // --- DropdownMenuSeparatorCn ---

    [Fact]
    public void DropdownMenuSeparator_Renders_With_DataSlot()
    {
        var cut = Render<DropdownMenuSeparatorCn>();
        cut.Find("[data-slot='dropdown-menu-separator']").Should().NotBeNull();
    }

    [Fact]
    public void DropdownMenuSeparator_Has_Role_Separator()
    {
        var cut = Render<DropdownMenuSeparatorCn>();
        cut.Find("[data-slot='dropdown-menu-separator']").GetAttribute("role").Should().Be("separator");
    }

    [Fact]
    public void DropdownMenuSeparator_Has_Default_Classes()
    {
        var cut = Render<DropdownMenuSeparatorCn>();
        var el = cut.Find("[data-slot='dropdown-menu-separator']");
        el.ClassList.Should().Contain("-mx-1");
        el.ClassList.Should().Contain("my-1");
        el.ClassList.Should().Contain("h-px");
        el.ClassList.Should().Contain("bg-muted");
    }

    [Fact]
    public void DropdownMenuSeparator_Class_Passthrough()
    {
        var cut = Render<DropdownMenuSeparatorCn>(p => p
            .Add(c => c.Class, "custom-sep"));
        cut.Find("[data-slot='dropdown-menu-separator']").ClassList.Should().Contain("custom-sep");
    }

    // --- DropdownMenuShortcutCn ---

    [Fact]
    public void DropdownMenuShortcut_Renders_With_DataSlot()
    {
        var cut = Render<DropdownMenuShortcutCn>(p => p.AddChildContent("Ctrl+K"));
        cut.Find("[data-slot='dropdown-menu-shortcut']").Should().NotBeNull();
    }

    [Fact]
    public void DropdownMenuShortcut_Has_Default_Classes()
    {
        var cut = Render<DropdownMenuShortcutCn>(p => p.AddChildContent("Ctrl+K"));
        var el = cut.Find("[data-slot='dropdown-menu-shortcut']");
        el.ClassList.Should().Contain("ml-auto");
        el.ClassList.Should().Contain("text-xs");
        el.ClassList.Should().Contain("tracking-widest");
        el.ClassList.Should().Contain("opacity-60");
    }

    [Fact]
    public void DropdownMenuShortcut_Renders_Content()
    {
        var cut = Render<DropdownMenuShortcutCn>(p => p.AddChildContent("Ctrl+K"));
        cut.Find("[data-slot='dropdown-menu-shortcut']").TextContent.Trim().Should().Be("Ctrl+K");
    }

    [Fact]
    public void DropdownMenuShortcut_Class_Passthrough()
    {
        var cut = Render<DropdownMenuShortcutCn>(p => p
            .Add(c => c.Class, "custom-shortcut")
            .AddChildContent("Ctrl+K"));
        cut.Find("[data-slot='dropdown-menu-shortcut']").ClassList.Should().Contain("custom-shortcut");
    }

    // --- DropdownMenuCheckboxItemCn ---

    [Fact]
    public void DropdownMenuCheckboxItem_Renders_With_DataSlot()
    {
        var cut = Render<DropdownMenuCheckboxItemCn>(p => p.AddChildContent("Check me"));
        cut.Find("[data-slot='dropdown-menu-checkbox-item']").Should().NotBeNull();
    }

    [Fact]
    public void DropdownMenuCheckboxItem_Has_DataMenuItem()
    {
        var cut = Render<DropdownMenuCheckboxItemCn>(p => p.AddChildContent("Check me"));
        cut.Find("[data-menu-item]").Should().NotBeNull();
    }

    [Fact]
    public void DropdownMenuCheckboxItem_Unchecked_By_Default()
    {
        var cut = Render<DropdownMenuCheckboxItemCn>(p => p.AddChildContent("Check me"));
        cut.Find("[data-slot='dropdown-menu-checkbox-item']").GetAttribute("data-state").Should().Be("unchecked");
        cut.Find("[data-slot='dropdown-menu-checkbox-item']").GetAttribute("aria-checked").Should().Be("false");
    }

    [Fact]
    public void DropdownMenuCheckboxItem_Checked_Shows_Check_Icon()
    {
        var cut = Render<DropdownMenuCheckboxItemCn>(p => p
            .Add(c => c.Checked, true)
            .AddChildContent("Check me"));
        cut.Find("[data-slot='dropdown-menu-checkbox-item']").GetAttribute("data-state").Should().Be("checked");
        cut.Find("[data-slot='dropdown-menu-checkbox-item']").GetAttribute("aria-checked").Should().Be("true");
        cut.FindAll("svg").Should().NotBeEmpty();
    }

    [Fact]
    public void DropdownMenuCheckboxItem_Click_Toggles_Checked()
    {
        var isChecked = false;
        var cut = Render<DropdownMenuCheckboxItemCn>(p => p
            .Add(c => c.CheckedChanged, EventCallback.Factory.Create<bool>(this, v => isChecked = v))
            .AddChildContent("Check me"));
        cut.Find("[data-slot='dropdown-menu-checkbox-item']").Click();
        isChecked.Should().BeTrue();
    }

    [Fact]
    public void DropdownMenuCheckboxItem_Disabled_Has_DataDisabled()
    {
        var cut = Render<DropdownMenuCheckboxItemCn>(p => p
            .Add(c => c.Disabled, true)
            .AddChildContent("Check me"));
        cut.Find("[data-slot='dropdown-menu-checkbox-item']").GetAttribute("data-disabled").Should().Be("true");
    }

    [Fact]
    public void DropdownMenuCheckboxItem_Has_Pl8()
    {
        var cut = Render<DropdownMenuCheckboxItemCn>(p => p.AddChildContent("Check me"));
        cut.Find("[data-slot='dropdown-menu-checkbox-item']").ClassList.Should().Contain("pl-8");
    }

    [Fact]
    public void DropdownMenuCheckboxItem_Has_Role_MenuitemCheckbox()
    {
        var cut = Render<DropdownMenuCheckboxItemCn>(p => p.AddChildContent("Check me"));
        cut.Find("[data-slot='dropdown-menu-checkbox-item']").GetAttribute("role").Should().Be("menuitemcheckbox");
    }

    // --- DropdownMenuRadioGroupCn ---

    [Fact]
    public void DropdownMenuRadioGroup_Renders_With_DataSlot()
    {
        var cut = Render<DropdownMenuRadioGroupCn>(p => p.AddChildContent("Radio group"));
        cut.Find("[data-slot='dropdown-menu-radio-group']").Should().NotBeNull();
    }

    [Fact]
    public void DropdownMenuRadioGroup_Has_Role_Radiogroup()
    {
        var cut = Render<DropdownMenuRadioGroupCn>(p => p.AddChildContent("Radio group"));
        cut.Find("[data-slot='dropdown-menu-radio-group']").GetAttribute("role").Should().Be("radiogroup");
    }

    // --- DropdownMenuRadioItemCn ---

    [Fact]
    public void DropdownMenuRadioItem_Renders_With_DataSlot()
    {
        var cut = Render<DropdownMenuRadioGroupCn>(p => p
            .Add(c => c.Value, "a")
            .AddChildContent<DropdownMenuRadioItemCn>(i => i
                .Add(x => x.Value, "a")
                .AddChildContent("Option A")));
        cut.Find("[data-slot='dropdown-menu-radio-item']").Should().NotBeNull();
    }

    [Fact]
    public void DropdownMenuRadioItem_Has_DataMenuItem()
    {
        var cut = Render<DropdownMenuRadioGroupCn>(p => p
            .Add(c => c.Value, "a")
            .AddChildContent<DropdownMenuRadioItemCn>(i => i
                .Add(x => x.Value, "a")
                .AddChildContent("Option A")));
        cut.Find("[data-menu-item]").Should().NotBeNull();
    }

    [Fact]
    public void DropdownMenuRadioItem_Selected_Shows_Dot()
    {
        var cut = Render<DropdownMenuRadioGroupCn>(p => p
            .Add(c => c.Value, "a")
            .AddChildContent<DropdownMenuRadioItemCn>(i => i
                .Add(x => x.Value, "a")
                .AddChildContent("Option A")));
        cut.Find("[data-slot='dropdown-menu-radio-item']").GetAttribute("data-state").Should().Be("checked");
        cut.Find("[data-slot='dropdown-menu-radio-item']").GetAttribute("aria-checked").Should().Be("true");
        cut.FindAll("svg").Should().NotBeEmpty();
    }

    [Fact]
    public void DropdownMenuRadioItem_NotSelected_No_Dot()
    {
        var cut = Render<DropdownMenuRadioGroupCn>(p => p
            .Add(c => c.Value, "b")
            .AddChildContent<DropdownMenuRadioItemCn>(i => i
                .Add(x => x.Value, "a")
                .AddChildContent("Option A")));
        cut.Find("[data-slot='dropdown-menu-radio-item']").GetAttribute("data-state").Should().Be("unchecked");
        cut.FindAll("svg").Should().BeEmpty();
    }

    [Fact]
    public void DropdownMenuRadioItem_Click_Selects_Value()
    {
        var selected = "";
        var cut = Render<DropdownMenuRadioGroupCn>(p => p
            .Add(c => c.Value, "b")
            .Add(c => c.ValueChanged, EventCallback.Factory.Create<string>(this, v => selected = v))
            .AddChildContent<DropdownMenuRadioItemCn>(i => i
                .Add(x => x.Value, "a")
                .AddChildContent("Option A")));
        cut.Find("[data-slot='dropdown-menu-radio-item']").Click();
        selected.Should().Be("a");
    }

    [Fact]
    public void DropdownMenuRadioItem_Has_Pl8()
    {
        var cut = Render<DropdownMenuRadioGroupCn>(p => p
            .Add(c => c.Value, "a")
            .AddChildContent<DropdownMenuRadioItemCn>(i => i
                .Add(x => x.Value, "a")
                .AddChildContent("Option A")));
        cut.Find("[data-slot='dropdown-menu-radio-item']").ClassList.Should().Contain("pl-8");
    }

    [Fact]
    public void DropdownMenuRadioItem_Has_Role_MenuitemRadio()
    {
        var cut = Render<DropdownMenuRadioGroupCn>(p => p
            .Add(c => c.Value, "a")
            .AddChildContent<DropdownMenuRadioItemCn>(i => i
                .Add(x => x.Value, "a")
                .AddChildContent("Option A")));
        cut.Find("[data-slot='dropdown-menu-radio-item']").GetAttribute("role").Should().Be("menuitemradio");
    }

    // --- DropdownMenuSubCn ---

    [Fact]
    public void DropdownMenuSub_Renders_With_DataSlot()
    {
        var cut = Render<DropdownMenuSubCn>(p => p.AddChildContent("Sub"));
        cut.Find("[data-slot='dropdown-menu-sub']").Should().NotBeNull();
    }

    [Fact]
    public void DropdownMenuSub_Starts_Closed()
    {
        var cut = Render<DropdownMenuSubCn>(p => p.AddChildContent("Sub"));
        cut.Find("[data-slot='dropdown-menu-sub']").GetAttribute("data-state").Should().Be("closed");
    }

    // --- DropdownMenuSubTriggerCn ---

    [Fact]
    public void DropdownMenuSubTrigger_Renders_With_DataSlot()
    {
        var cut = Render<DropdownMenuSubCn>(p => p
            .AddChildContent<DropdownMenuSubTriggerCn>(t => t
                .AddChildContent("More")));
        cut.Find("[data-slot='dropdown-menu-sub-trigger']").Should().NotBeNull();
    }

    [Fact]
    public void DropdownMenuSubTrigger_Has_DataMenuItem()
    {
        var cut = Render<DropdownMenuSubCn>(p => p
            .AddChildContent<DropdownMenuSubTriggerCn>(t => t
                .AddChildContent("More")));
        cut.Find("[data-menu-item]").Should().NotBeNull();
    }

    [Fact]
    public void DropdownMenuSubTrigger_Has_Chevron_Icon()
    {
        var cut = Render<DropdownMenuSubCn>(p => p
            .AddChildContent<DropdownMenuSubTriggerCn>(t => t
                .AddChildContent("More")));
        cut.FindAll("svg").Should().NotBeEmpty();
    }

    [Fact]
    public void DropdownMenuSubTrigger_Click_Toggles_SubOpen()
    {
        var cut = Render<DropdownMenuSubCn>(p => p
            .AddChildContent<DropdownMenuSubTriggerCn>(t => t
                .AddChildContent("More")));
        cut.Find("[data-slot='dropdown-menu-sub-trigger']").Click();
        cut.Find("[data-slot='dropdown-menu-sub']").GetAttribute("data-state").Should().Be("open");
    }

    [Fact]
    public void DropdownMenuSubTrigger_Has_Default_Classes()
    {
        var cut = Render<DropdownMenuSubCn>(p => p
            .AddChildContent<DropdownMenuSubTriggerCn>(t => t
                .AddChildContent("More")));
        var el = cut.Find("[data-slot='dropdown-menu-sub-trigger']");
        el.ClassList.Should().Contain("flex");
        el.ClassList.Should().Contain("cursor-default");
        el.ClassList.Should().Contain("select-none");
        el.ClassList.Should().Contain("items-center");
        el.ClassList.Should().Contain("gap-2");
        el.ClassList.Should().Contain("rounded-sm");
        el.ClassList.Should().Contain("px-2");
        el.ClassList.Should().Contain("py-1.5");
        el.ClassList.Should().Contain("text-sm");
    }

    [Fact]
    public void DropdownMenuSubTrigger_Inset_Has_PaddingLeft()
    {
        var cut = Render<DropdownMenuSubCn>(p => p
            .AddChildContent<DropdownMenuSubTriggerCn>(t => t
                .Add(c => c.Inset, true)
                .AddChildContent("More")));
        cut.Find("[data-slot='dropdown-menu-sub-trigger']").ClassList.Should().Contain("pl-8");
    }

    // --- DropdownMenuSubContentCn ---

    [Fact]
    public void DropdownMenuSubContent_Not_Rendered_When_Closed()
    {
        SetupJsInterop();
        var cut = Render<DropdownMenuSubCn>(p => p
            .AddChildContent<DropdownMenuSubContentCn>(c => c
                .AddChildContent("Sub content")));
        cut.FindAll("[data-slot='dropdown-menu-sub-content']").Should().BeEmpty();
    }

    [Fact]
    public void DropdownMenuSubContent_Rendered_When_SubOpen()
    {
        SetupJsInterop();
        var cut = Render<DropdownMenuSubCn>(p => p.AddChildContent(builder =>
        {
            builder.OpenComponent<DropdownMenuSubTriggerCn>(0);
            builder.AddAttribute(1, "ChildContent", (RenderFragment)(b => b.AddContent(0, "More")));
            builder.CloseComponent();
            builder.OpenComponent<DropdownMenuSubContentCn>(2);
            builder.AddAttribute(3, "ChildContent", (RenderFragment)(b => b.AddContent(0, "Sub items")));
            builder.CloseComponent();
        }));

        // Click trigger to open sub
        cut.Find("[data-slot='dropdown-menu-sub-trigger']").Click();
        cut.Find("[data-slot='dropdown-menu-sub-content']").Should().NotBeNull();
        cut.Find("[data-slot='dropdown-menu-sub-content']").TextContent.Should().Contain("Sub items");
    }

    [Fact]
    public void DropdownMenuSubContent_Has_Default_Classes()
    {
        SetupJsInterop();
        var cut = Render<DropdownMenuSubCn>(p => p.AddChildContent(builder =>
        {
            builder.OpenComponent<DropdownMenuSubTriggerCn>(0);
            builder.AddAttribute(1, "ChildContent", (RenderFragment)(b => b.AddContent(0, "More")));
            builder.CloseComponent();
            builder.OpenComponent<DropdownMenuSubContentCn>(2);
            builder.AddAttribute(3, "ChildContent", (RenderFragment)(b => b.AddContent(0, "Sub items")));
            builder.CloseComponent();
        }));

        cut.Find("[data-slot='dropdown-menu-sub-trigger']").Click();
        var content = cut.Find("[data-slot='dropdown-menu-sub-content']");
        content.ClassList.Should().Contain("z-50");
        content.ClassList.Should().Contain("overflow-hidden");
        content.ClassList.Should().Contain("rounded-md");
        content.ClassList.Should().Contain("border");
        content.ClassList.Should().Contain("bg-popover");
        content.ClassList.Should().Contain("p-1");
        content.ClassList.Should().Contain("text-popover-foreground");
        content.ClassList.Should().Contain("shadow-md");
    }

    [Fact]
    public void DropdownMenuSubContent_Default_Side_Is_Right()
    {
        SetupJsInterop();
        var cut = Render<DropdownMenuSubCn>(p => p.AddChildContent(builder =>
        {
            builder.OpenComponent<DropdownMenuSubTriggerCn>(0);
            builder.AddAttribute(1, "ChildContent", (RenderFragment)(b => b.AddContent(0, "More")));
            builder.CloseComponent();
            builder.OpenComponent<DropdownMenuSubContentCn>(2);
            builder.AddAttribute(3, "ChildContent", (RenderFragment)(b => b.AddContent(0, "Sub")));
            builder.CloseComponent();
        }));

        cut.Find("[data-slot='dropdown-menu-sub-trigger']").Click();
        cut.Find("[data-slot='dropdown-menu-sub-content']").GetAttribute("data-side").Should().Be("right");
    }

    [Fact]
    public void DropdownMenuSubContent_Default_Align_Is_Start()
    {
        SetupJsInterop();
        var cut = Render<DropdownMenuSubCn>(p => p.AddChildContent(builder =>
        {
            builder.OpenComponent<DropdownMenuSubTriggerCn>(0);
            builder.AddAttribute(1, "ChildContent", (RenderFragment)(b => b.AddContent(0, "More")));
            builder.CloseComponent();
            builder.OpenComponent<DropdownMenuSubContentCn>(2);
            builder.AddAttribute(3, "ChildContent", (RenderFragment)(b => b.AddContent(0, "Sub")));
            builder.CloseComponent();
        }));

        cut.Find("[data-slot='dropdown-menu-sub-trigger']").Click();
        cut.Find("[data-slot='dropdown-menu-sub-content']").GetAttribute("data-align").Should().Be("start");
    }

    // --- Integration ---

    [Fact]
    public void DropdownMenu_Full_Integration_Toggle()
    {
        SetupJsInterop();
        var isOpen = false;
        var cut = Render<DropdownMenuCn>(p => p
            .Add(c => c.OpenChanged, EventCallback.Factory.Create<bool>(this, v => isOpen = v))
            .AddChildContent<DropdownMenuTriggerCn>(t => t
                .AddChildContent("Toggle")));

        // Initially closed
        cut.Find("[data-slot='dropdown-menu']").GetAttribute("data-state").Should().Be("closed");

        // Click trigger to open
        cut.Find("[data-slot='dropdown-menu-trigger']").Click();
        isOpen.Should().BeTrue();
        cut.Find("[data-slot='dropdown-menu']").GetAttribute("data-state").Should().Be("open");

        // Click trigger again to close
        cut.Find("[data-slot='dropdown-menu-trigger']").Click();
        isOpen.Should().BeFalse();
        cut.Find("[data-slot='dropdown-menu']").GetAttribute("data-state").Should().Be("closed");
    }
}

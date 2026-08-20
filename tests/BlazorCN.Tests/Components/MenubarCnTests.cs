using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BlazorCN.Tests.Components;

public class MenubarCnTests : BunitContext
{
    public MenubarCnTests()
    {
        // MenubarCn wires arrow-key nav via JsInteropCn on render.
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddScoped<JsInteropCn>();
    }

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

    // --- MenubarCn ---

    [Fact]
    public void Menubar_Renders_With_DataSlot()
    {
        var cut = Render<MenubarCn>(p => p.AddChildContent("Content"));
        cut.Find("[data-slot='menubar']").Should().NotBeNull();
    }

    [Fact]
    public void Menubar_Has_Default_Classes()
    {
        var cut = Render<MenubarCn>(p => p.AddChildContent("Content"));
        var el = cut.Find("[data-slot='menubar']");
        el.ClassList.Should().Contain("cn-menubar");
        el.ClassList.Should().Contain("flex");
        el.ClassList.Should().Contain("items-center");
        el.ClassList.Should().Contain("bg-background");
    }

    [Fact]
    public void Menubar_Has_Role_Menubar()
    {
        var cut = Render<MenubarCn>(p => p.AddChildContent("Content"));
        cut.Find("[data-slot='menubar']").GetAttribute("role").Should().Be("menubar");
    }

    [Fact]
    public void Menubar_Has_Default_AriaLabel()
    {
        var cut = Render<MenubarCn>(p => p.AddChildContent("Content"));
        cut.Find("[data-slot='menubar']").GetAttribute("aria-label").Should().Be("Menu");
    }

    [Fact]
    public void Menubar_AriaLabel_Override_Via_AdditionalAttributes()
    {
        var cut = Render<MenubarCn>(p => p
            .Add(c => c.AdditionalAttributes, new Dictionary<string, object?> { { "aria-label", "Main navigation" } })
            .AddChildContent("Content"));
        cut.Find("[data-slot='menubar']").GetAttribute("aria-label").Should().Be("Main navigation");
    }

    [Fact]
    public void Menubar_Class_Passthrough()
    {
        var cut = Render<MenubarCn>(p => p
            .Add(c => c.Class, "custom-bar")
            .AddChildContent("Content"));
        cut.Find("[data-slot='menubar']").ClassList.Should().Contain("custom-bar");
    }

    [Fact]
    public void Menubar_AdditionalAttributes_Passthrough()
    {
        var cut = Render<MenubarCn>(p => p
            .Add(c => c.AdditionalAttributes, new Dictionary<string, object?> { { "id", "my-bar" } })
            .AddChildContent("Content"));
        cut.Find("[data-slot='menubar']").GetAttribute("id").Should().Be("my-bar");
    }

    // --- MenubarMenuCn ---

    [Fact]
    public void MenubarMenu_Renders_With_DataSlot()
    {
        var cut = Render<MenubarCn>(p => p
            .AddChildContent<MenubarMenuCn>(m => m
                .AddChildContent("Menu")));
        cut.Find("[data-slot='menubar-menu']").Should().NotBeNull();
    }

    [Fact]
    public void MenubarMenu_Starts_Closed()
    {
        var cut = Render<MenubarCn>(p => p
            .AddChildContent<MenubarMenuCn>(m => m
                .AddChildContent("Menu")));
        cut.Find("[data-slot='menubar-menu']").GetAttribute("data-state").Should().Be("closed");
    }

    // --- MenubarTriggerCn ---

    [Fact]
    public void MenubarTrigger_Renders_With_DataSlot()
    {
        var cut = Render<MenubarCn>(p => p
            .AddChildContent<MenubarMenuCn>(m => m
                .AddChildContent<MenubarTriggerCn>(t => t
                    .AddChildContent("File"))));
        cut.Find("[data-slot='menubar-trigger']").Should().NotBeNull();
    }

    [Fact]
    public void MenubarTrigger_Is_Button()
    {
        var cut = Render<MenubarCn>(p => p
            .AddChildContent<MenubarMenuCn>(m => m
                .AddChildContent<MenubarTriggerCn>(t => t
                    .AddChildContent("File"))));
        cut.Find("[data-slot='menubar-trigger']").TagName.Should().Be("BUTTON");
    }

    [Fact]
    public void MenubarTrigger_Has_Default_Classes()
    {
        var cut = Render<MenubarCn>(p => p
            .AddChildContent<MenubarMenuCn>(m => m
                .AddChildContent<MenubarTriggerCn>(t => t
                    .AddChildContent("File"))));
        var el = cut.Find("[data-slot='menubar-trigger']");
        el.ClassList.Should().Contain("cn-menubar-trigger");
        el.ClassList.Should().Contain("flex");
        el.ClassList.Should().Contain("items-center");
        el.ClassList.Should().Contain("select-none");
    }

    [Fact]
    public void MenubarTrigger_Click_Opens_Menu()
    {
        var cut = Render<MenubarCn>(p => p
            .AddChildContent<MenubarMenuCn>(m => m
                .AddChildContent<MenubarTriggerCn>(t => t
                    .AddChildContent("File"))));
        cut.Find("[data-slot='menubar-trigger']").Click();
        cut.Find("[data-slot='menubar-menu']").GetAttribute("data-state").Should().Be("open");
        cut.Find("[data-slot='menubar-trigger']").GetAttribute("data-state").Should().Be("open");
    }

    [Fact]
    public void MenubarTrigger_Click_Toggles_Closed()
    {
        var cut = Render<MenubarCn>(p => p
            .AddChildContent<MenubarMenuCn>(m => m
                .AddChildContent<MenubarTriggerCn>(t => t
                    .AddChildContent("File"))));
        // Open
        cut.Find("[data-slot='menubar-trigger']").Click();
        cut.Find("[data-slot='menubar-menu']").GetAttribute("data-state").Should().Be("open");
        // Close
        cut.Find("[data-slot='menubar-trigger']").Click();
        cut.Find("[data-slot='menubar-menu']").GetAttribute("data-state").Should().Be("closed");
    }

    [Fact]
    public void MenubarTrigger_Class_Passthrough()
    {
        var cut = Render<MenubarCn>(p => p
            .AddChildContent<MenubarMenuCn>(m => m
                .AddChildContent<MenubarTriggerCn>(t => t
                    .Add(c => c.Class, "custom-trigger")
                    .AddChildContent("File"))));
        cut.Find("[data-slot='menubar-trigger']").ClassList.Should().Contain("custom-trigger");
    }

    // --- MenubarContentCn ---

    [Fact]
    public void MenubarContent_Not_Rendered_When_Closed()
    {
        SetupJsInterop();
        var cut = Render<MenubarCn>(p => p
            .AddChildContent<MenubarMenuCn>(m => m
                .AddChildContent<MenubarContentCn>(c => c
                    .AddChildContent("Content"))));
        cut.FindAll("[data-slot='menubar-content']").Should().BeEmpty();
    }

    [Fact]
    public void MenubarContent_Rendered_When_Open()
    {
        SetupJsInterop();
        var cut = Render<MenubarCn>(p => p
            .AddChildContent<MenubarMenuCn>(m => m.AddChildContent(builder =>
            {
                builder.OpenComponent<MenubarTriggerCn>(0);
                builder.AddAttribute(1, "ChildContent", (RenderFragment)(b => b.AddContent(0, "File")));
                builder.CloseComponent();
                builder.OpenComponent<MenubarContentCn>(2);
                builder.AddAttribute(3, "ChildContent", (RenderFragment)(b => b.AddContent(0, "Menu items")));
                builder.CloseComponent();
            })));

        cut.Find("[data-slot='menubar-trigger']").Click();
        cut.Find("[data-slot='menubar-content']").Should().NotBeNull();
        cut.Find("[data-slot='menubar-content']").TextContent.Should().Contain("Menu items");
    }

    [Fact]
    public void MenubarContent_Has_Default_Classes()
    {
        SetupJsInterop();
        var cut = Render<MenubarCn>(p => p
            .AddChildContent<MenubarMenuCn>(m => m.AddChildContent(builder =>
            {
                builder.OpenComponent<MenubarTriggerCn>(0);
                builder.AddAttribute(1, "ChildContent", (RenderFragment)(b => b.AddContent(0, "File")));
                builder.CloseComponent();
                builder.OpenComponent<MenubarContentCn>(2);
                builder.AddAttribute(3, "ChildContent", (RenderFragment)(b => b.AddContent(0, "Body")));
                builder.CloseComponent();
            })));

        cut.Find("[data-slot='menubar-trigger']").Click();
        var content = cut.Find("[data-slot='menubar-content']");
        content.ClassList.Should().Contain("cn-menubar-content");
        content.ClassList.Should().Contain("z-50");
        content.ClassList.Should().Contain("overflow-y-auto");
    }

    [Fact]
    public void MenubarContent_Default_Align_Is_Start()
    {
        SetupJsInterop();
        var cut = Render<MenubarCn>(p => p
            .AddChildContent<MenubarMenuCn>(m => m.AddChildContent(builder =>
            {
                builder.OpenComponent<MenubarTriggerCn>(0);
                builder.AddAttribute(1, "ChildContent", (RenderFragment)(b => b.AddContent(0, "File")));
                builder.CloseComponent();
                builder.OpenComponent<MenubarContentCn>(2);
                builder.AddAttribute(3, "ChildContent", (RenderFragment)(b => b.AddContent(0, "Body")));
                builder.CloseComponent();
            })));

        cut.Find("[data-slot='menubar-trigger']").Click();
        cut.Find("[data-slot='menubar-content']").GetAttribute("data-align").Should().Be("start");
    }

    [Fact]
    public void MenubarContent_Class_Passthrough()
    {
        SetupJsInterop();
        var cut = Render<MenubarCn>(p => p
            .AddChildContent<MenubarMenuCn>(m => m.AddChildContent(builder =>
            {
                builder.OpenComponent<MenubarTriggerCn>(0);
                builder.AddAttribute(1, "ChildContent", (RenderFragment)(b => b.AddContent(0, "File")));
                builder.CloseComponent();
                builder.OpenComponent<MenubarContentCn>(2);
                builder.AddAttribute(3, "ChildContent", (RenderFragment)(b => b.AddContent(0, "Body")));
                builder.AddAttribute(4, "Class", "custom-content");
                builder.CloseComponent();
            })));

        cut.Find("[data-slot='menubar-trigger']").Click();
        cut.Find("[data-slot='menubar-content']").ClassList.Should().Contain("custom-content");
    }

    // --- MenubarItemCn ---

    [Fact]
    public void MenubarItem_Renders_With_DataSlot()
    {
        var cut = Render<MenubarItemCn>(p => p.AddChildContent("Item"));
        cut.Find("[data-slot='menubar-item']").Should().NotBeNull();
    }

    [Fact]
    public void MenubarItem_Has_DataMenuItem()
    {
        var cut = Render<MenubarItemCn>(p => p.AddChildContent("Item"));
        cut.Find("[data-menu-item]").Should().NotBeNull();
    }

    [Fact]
    public void MenubarItem_Has_Default_Classes()
    {
        var cut = Render<MenubarItemCn>(p => p.AddChildContent("Item"));
        var el = cut.Find("[data-slot='menubar-item']");
        el.ClassList.Should().Contain("cn-menubar-item");
        el.ClassList.Should().Contain("relative");
        el.ClassList.Should().Contain("flex");
        el.ClassList.Should().Contain("cursor-default");
        el.ClassList.Should().Contain("select-none");
        el.ClassList.Should().Contain("items-center");
        el.ClassList.Should().Contain("outline-hidden");
    }

    [Fact]
    public void MenubarItem_Disabled_Has_DataDisabled()
    {
        var cut = Render<MenubarItemCn>(p => p
            .Add(c => c.Disabled, true)
            .AddChildContent("Item"));
        cut.Find("[data-slot='menubar-item']").GetAttribute("data-disabled").Should().Be("true");
    }

    [Fact]
    public void MenubarItem_Destructive_Variant()
    {
        var cut = Render<MenubarItemCn>(p => p
            .Add(c => c.Variant, "destructive")
            .AddChildContent("Delete"));
        cut.Find("[data-slot='menubar-item']").GetAttribute("data-variant").Should().Be("destructive");
    }

    [Fact]
    public void MenubarItem_Inset_Has_PaddingLeft()
    {
        var cut = Render<MenubarItemCn>(p => p
            .Add(c => c.Inset, true)
            .AddChildContent("Item"));
        cut.Find("[data-slot='menubar-item']").GetAttribute("data-inset").Should().NotBeNull();
    }

    [Fact]
    public void MenubarItem_OnClick_Fires()
    {
        var clicked = false;
        var cut = Render<MenubarItemCn>(p => p
            .Add(c => c.OnClick, () => clicked = true)
            .AddChildContent("Item"));
        cut.Find("[data-slot='menubar-item']").Click();
        clicked.Should().BeTrue();
    }

    [Fact]
    public void MenubarItem_Class_Passthrough()
    {
        var cut = Render<MenubarItemCn>(p => p
            .Add(c => c.Class, "custom-item")
            .AddChildContent("Item"));
        cut.Find("[data-slot='menubar-item']").ClassList.Should().Contain("custom-item");
    }

    // --- MenubarGroupCn ---

    [Fact]
    public void MenubarGroup_Renders_With_DataSlot()
    {
        var cut = Render<MenubarGroupCn>(p => p.AddChildContent("Group"));
        cut.Find("[data-slot='menubar-group']").Should().NotBeNull();
    }

    [Fact]
    public void MenubarGroup_Has_Role_Group()
    {
        var cut = Render<MenubarGroupCn>(p => p.AddChildContent("Group"));
        cut.Find("[data-slot='menubar-group']").GetAttribute("role").Should().Be("group");
    }

    // --- MenubarLabelCn ---

    [Fact]
    public void MenubarLabel_Renders_With_DataSlot()
    {
        var cut = Render<MenubarLabelCn>(p => p.AddChildContent("Label"));
        cut.Find("[data-slot='menubar-label']").Should().NotBeNull();
    }

    [Fact]
    public void MenubarLabel_Has_Default_Classes()
    {
        var cut = Render<MenubarLabelCn>(p => p.AddChildContent("Label"));
        var el = cut.Find("[data-slot='menubar-label']");
        el.ClassList.Should().Contain("cn-menubar-label");
    }

    [Fact]
    public void MenubarLabel_Inset_Has_PaddingLeft()
    {
        var cut = Render<MenubarLabelCn>(p => p
            .Add(c => c.Inset, true)
            .AddChildContent("Label"));
        cut.Find("[data-slot='menubar-label']").GetAttribute("data-inset").Should().NotBeNull();
    }

    // --- MenubarSeparatorCn ---

    [Fact]
    public void MenubarSeparator_Renders_With_DataSlot()
    {
        var cut = Render<MenubarSeparatorCn>();
        cut.Find("[data-slot='menubar-separator']").Should().NotBeNull();
    }

    [Fact]
    public void MenubarSeparator_Has_Role_Separator()
    {
        var cut = Render<MenubarSeparatorCn>();
        cut.Find("[data-slot='menubar-separator']").GetAttribute("role").Should().Be("separator");
    }

    [Fact]
    public void MenubarSeparator_Has_Default_Classes()
    {
        var cut = Render<MenubarSeparatorCn>();
        var el = cut.Find("[data-slot='menubar-separator']");
        el.ClassList.Should().Contain("cn-menubar-separator");
        el.ClassList.Should().Contain("-mx-1");
        el.ClassList.Should().Contain("my-1");
        el.ClassList.Should().Contain("h-px");
    }

    // --- MenubarShortcutCn ---

    [Fact]
    public void MenubarShortcut_Renders_With_DataSlot()
    {
        var cut = Render<MenubarShortcutCn>(p => p.AddChildContent("Ctrl+N"));
        cut.Find("[data-slot='menubar-shortcut']").Should().NotBeNull();
    }

    [Fact]
    public void MenubarShortcut_Has_Default_Classes()
    {
        var cut = Render<MenubarShortcutCn>(p => p.AddChildContent("Ctrl+N"));
        var el = cut.Find("[data-slot='menubar-shortcut']");
        el.ClassList.Should().Contain("cn-menubar-shortcut");
        el.ClassList.Should().Contain("ml-auto");
    }

    [Fact]
    public void MenubarShortcut_Renders_Content()
    {
        var cut = Render<MenubarShortcutCn>(p => p.AddChildContent("Ctrl+N"));
        cut.Find("[data-slot='menubar-shortcut']").TextContent.Trim().Should().Be("Ctrl+N");
    }

    // --- MenubarCheckboxItemCn ---

    [Fact]
    public void MenubarCheckboxItem_Renders_With_DataSlot()
    {
        var cut = Render<MenubarCheckboxItemCn>(p => p.AddChildContent("Check"));
        cut.Find("[data-slot='menubar-checkbox-item']").Should().NotBeNull();
    }

    [Fact]
    public void MenubarCheckboxItem_Unchecked_By_Default()
    {
        var cut = Render<MenubarCheckboxItemCn>(p => p.AddChildContent("Check"));
        cut.Find("[data-slot='menubar-checkbox-item']").GetAttribute("data-state").Should().Be("unchecked");
    }

    [Fact]
    public void MenubarCheckboxItem_Checked_Shows_Icon()
    {
        var cut = Render<MenubarCheckboxItemCn>(p => p
            .Add(c => c.Checked, true)
            .AddChildContent("Check"));
        cut.Find("[data-slot='menubar-checkbox-item']").GetAttribute("data-state").Should().Be("checked");
        cut.FindAll("svg").Should().NotBeEmpty();
    }

    [Fact]
    public void MenubarCheckboxItem_Click_Toggles()
    {
        var isChecked = false;
        var cut = Render<MenubarCheckboxItemCn>(p => p
            .Add(c => c.CheckedChanged, EventCallback.Factory.Create<bool>(this, v => isChecked = v))
            .AddChildContent("Check"));
        cut.Find("[data-slot='menubar-checkbox-item']").Click();
        isChecked.Should().BeTrue();
    }

    [Fact]
    public void MenubarCheckboxItem_Has_Role()
    {
        var cut = Render<MenubarCheckboxItemCn>(p => p.AddChildContent("Check"));
        cut.Find("[data-slot='menubar-checkbox-item']").GetAttribute("role").Should().Be("menuitemcheckbox");
    }

    [Fact]
    public void MenubarCheckboxItem_Has_Nova_Item_Class()
    {
        // Indicator padding (pr-8/pl-1.5) now comes from the cn-* nova CSS class, not a pl-8 utility.
        var cut = Render<MenubarCheckboxItemCn>(p => p.AddChildContent("Check"));
        cut.Find("[data-slot='menubar-checkbox-item']").ClassList.Should().Contain("cn-menubar-checkbox-item");
    }

    // --- MenubarRadioGroupCn ---

    [Fact]
    public void MenubarRadioGroup_Renders_With_DataSlot()
    {
        var cut = Render<MenubarRadioGroupCn>(p => p.AddChildContent("Radio"));
        cut.Find("[data-slot='menubar-radio-group']").Should().NotBeNull();
    }

    [Fact]
    public void MenubarRadioGroup_Has_Role_Radiogroup()
    {
        var cut = Render<MenubarRadioGroupCn>(p => p.AddChildContent("Radio"));
        cut.Find("[data-slot='menubar-radio-group']").GetAttribute("role").Should().Be("group"); // menuitemradio children require group, not radiogroup
    }

    // --- MenubarRadioItemCn ---

    [Fact]
    public void MenubarRadioItem_Renders_With_DataSlot()
    {
        var cut = Render<MenubarRadioGroupCn>(p => p
            .Add(c => c.Value, "a")
            .AddChildContent<MenubarRadioItemCn>(i => i
                .Add(x => x.Value, "a")
                .AddChildContent("Option A")));
        cut.Find("[data-slot='menubar-radio-item']").Should().NotBeNull();
    }

    [Fact]
    public void MenubarRadioItem_Selected_Shows_Dot()
    {
        var cut = Render<MenubarRadioGroupCn>(p => p
            .Add(c => c.Value, "a")
            .AddChildContent<MenubarRadioItemCn>(i => i
                .Add(x => x.Value, "a")
                .AddChildContent("Option A")));
        cut.Find("[data-slot='menubar-radio-item']").GetAttribute("data-state").Should().Be("checked");
        cut.FindAll("svg").Should().NotBeEmpty();
    }

    [Fact]
    public void MenubarRadioItem_NotSelected_No_Dot()
    {
        var cut = Render<MenubarRadioGroupCn>(p => p
            .Add(c => c.Value, "b")
            .AddChildContent<MenubarRadioItemCn>(i => i
                .Add(x => x.Value, "a")
                .AddChildContent("Option A")));
        cut.Find("[data-slot='menubar-radio-item']").GetAttribute("data-state").Should().Be("unchecked");
        cut.FindAll("svg").Should().BeEmpty();
    }

    [Fact]
    public void MenubarRadioItem_Click_Selects()
    {
        var selected = "";
        var cut = Render<MenubarRadioGroupCn>(p => p
            .Add(c => c.Value, "b")
            .Add(c => c.ValueChanged, EventCallback.Factory.Create<string>(this, v => selected = v))
            .AddChildContent<MenubarRadioItemCn>(i => i
                .Add(x => x.Value, "a")
                .AddChildContent("Option A")));
        cut.Find("[data-slot='menubar-radio-item']").Click();
        selected.Should().Be("a");
    }

    [Fact]
    public void MenubarRadioItem_Has_Role()
    {
        var cut = Render<MenubarRadioGroupCn>(p => p
            .Add(c => c.Value, "a")
            .AddChildContent<MenubarRadioItemCn>(i => i
                .Add(x => x.Value, "a")
                .AddChildContent("Option A")));
        cut.Find("[data-slot='menubar-radio-item']").GetAttribute("role").Should().Be("menuitemradio");
    }

    // --- MenubarSubCn ---

    [Fact]
    public void MenubarSub_Renders_With_DataSlot()
    {
        var cut = Render<MenubarSubCn>(p => p.AddChildContent("Sub"));
        cut.Find("[data-slot='menubar-sub']").Should().NotBeNull();
    }

    [Fact]
    public void MenubarSub_Starts_Closed()
    {
        var cut = Render<MenubarSubCn>(p => p.AddChildContent("Sub"));
        cut.Find("[data-slot='menubar-sub']").GetAttribute("data-state").Should().Be("closed");
    }

    // --- MenubarSubTriggerCn ---

    [Fact]
    public void MenubarSubTrigger_Renders_With_DataSlot()
    {
        var cut = Render<MenubarSubCn>(p => p
            .AddChildContent<MenubarSubTriggerCn>(t => t
                .AddChildContent("More")));
        cut.Find("[data-slot='menubar-sub-trigger']").Should().NotBeNull();
    }

    [Fact]
    public void MenubarSubTrigger_Has_Chevron()
    {
        var cut = Render<MenubarSubCn>(p => p
            .AddChildContent<MenubarSubTriggerCn>(t => t
                .AddChildContent("More")));
        cut.FindAll("svg").Should().NotBeEmpty();
    }

    [Fact]
    public void MenubarSubTrigger_Click_Opens_Sub()
    {
        var cut = Render<MenubarSubCn>(p => p
            .AddChildContent<MenubarSubTriggerCn>(t => t
                .AddChildContent("More")));
        cut.Find("[data-slot='menubar-sub-trigger']").Click();
        cut.Find("[data-slot='menubar-sub']").GetAttribute("data-state").Should().Be("open");
    }

    [Fact]
    public void MenubarSubTrigger_Has_Default_Classes()
    {
        var cut = Render<MenubarSubCn>(p => p
            .AddChildContent<MenubarSubTriggerCn>(t => t
                .AddChildContent("More")));
        var el = cut.Find("[data-slot='menubar-sub-trigger']");
        el.ClassList.Should().Contain("cn-menubar-sub-trigger");
        el.ClassList.Should().Contain("flex");
        el.ClassList.Should().Contain("cursor-default");
        el.ClassList.Should().Contain("select-none");
        el.ClassList.Should().Contain("items-center");
    }

    // --- MenubarSubContentCn ---

    [Fact]
    public void MenubarSubContent_Not_Rendered_When_Closed()
    {
        SetupJsInterop();
        var cut = Render<MenubarSubCn>(p => p
            .AddChildContent<MenubarSubContentCn>(c => c
                .AddChildContent("Sub content")));
        cut.FindAll("[data-slot='menubar-sub-content']").Should().BeEmpty();
    }

    [Fact]
    public void MenubarSubContent_Rendered_When_Open()
    {
        SetupJsInterop();
        var cut = Render<MenubarSubCn>(p => p.AddChildContent(builder =>
        {
            builder.OpenComponent<MenubarSubTriggerCn>(0);
            builder.AddAttribute(1, "ChildContent", (RenderFragment)(b => b.AddContent(0, "More")));
            builder.CloseComponent();
            builder.OpenComponent<MenubarSubContentCn>(2);
            builder.AddAttribute(3, "ChildContent", (RenderFragment)(b => b.AddContent(0, "Sub items")));
            builder.CloseComponent();
        }));

        cut.Find("[data-slot='menubar-sub-trigger']").Click();
        cut.Find("[data-slot='menubar-sub-content']").Should().NotBeNull();
    }

    [Fact]
    public void MenubarSubContent_Default_Side_Is_Right()
    {
        SetupJsInterop();
        var cut = Render<MenubarSubCn>(p => p.AddChildContent(builder =>
        {
            builder.OpenComponent<MenubarSubTriggerCn>(0);
            builder.AddAttribute(1, "ChildContent", (RenderFragment)(b => b.AddContent(0, "More")));
            builder.CloseComponent();
            builder.OpenComponent<MenubarSubContentCn>(2);
            builder.AddAttribute(3, "ChildContent", (RenderFragment)(b => b.AddContent(0, "Sub")));
            builder.CloseComponent();
        }));

        cut.Find("[data-slot='menubar-sub-trigger']").Click();
        cut.Find("[data-slot='menubar-sub-content']").GetAttribute("data-side").Should().Be("right");
    }

    // --- ARIA ---

    [Fact]
    public void MenubarTrigger_AriaExpanded_Reflects_State()
    {
        var cut = Render<MenubarCn>(p => p
            .AddChildContent<MenubarMenuCn>(m => m
                .AddChildContent<MenubarTriggerCn>(t => t
                    .AddChildContent("File"))));
        var trigger = cut.Find("[data-slot='menubar-trigger']");
        trigger.GetAttribute("aria-expanded").Should().Be("false");
        trigger.Click();
        trigger.GetAttribute("aria-expanded").Should().Be("true");
    }

    [Fact]
    public void MenubarTrigger_Has_AriaHasPopup_Menu()
    {
        var cut = Render<MenubarCn>(p => p
            .AddChildContent<MenubarMenuCn>(m => m
                .AddChildContent<MenubarTriggerCn>(t => t
                    .AddChildContent("File"))));
        cut.Find("[data-slot='menubar-trigger']").GetAttribute("aria-haspopup").Should().Be("menu");
    }

    [Fact]
    public void MenubarSubTrigger_AriaExpanded_Reflects_State()
    {
        var cut = Render<MenubarSubCn>(p => p
            .AddChildContent<MenubarSubTriggerCn>(t => t
                .AddChildContent("More")));
        var trigger = cut.Find("[data-slot='menubar-sub-trigger']");
        trigger.GetAttribute("aria-expanded").Should().Be("false");
        trigger.Click();
        trigger.GetAttribute("aria-expanded").Should().Be("true");
    }

    [Fact]
    public void MenubarSubTrigger_Has_AriaHasPopup_Menu()
    {
        var cut = Render<MenubarSubCn>(p => p
            .AddChildContent<MenubarSubTriggerCn>(t => t
                .AddChildContent("More")));
        cut.Find("[data-slot='menubar-sub-trigger']").GetAttribute("aria-haspopup").Should().Be("menu");
    }

    // --- Integration ---

    [Fact]
    public void Menubar_Full_Integration()
    {
        SetupJsInterop();
        var cut = Render<MenubarCn>(p => p
            .AddChildContent<MenubarMenuCn>(m => m.AddChildContent(builder =>
            {
                builder.OpenComponent<MenubarTriggerCn>(0);
                builder.AddAttribute(1, "ChildContent", (RenderFragment)(b => b.AddContent(0, "File")));
                builder.CloseComponent();
                builder.OpenComponent<MenubarContentCn>(2);
                builder.AddAttribute(3, "ChildContent", (RenderFragment)(b =>
                {
                    b.OpenComponent<MenubarItemCn>(0);
                    b.AddAttribute(1, "ChildContent", (RenderFragment)(b2 => b2.AddContent(0, "New File")));
                    b.CloseComponent();
                }));
                builder.CloseComponent();
            })));

        // Initially closed
        cut.Find("[data-slot='menubar-menu']").GetAttribute("data-state").Should().Be("closed");
        cut.FindAll("[data-slot='menubar-content']").Should().BeEmpty();

        // Click trigger to open
        cut.Find("[data-slot='menubar-trigger']").Click();
        cut.Find("[data-slot='menubar-menu']").GetAttribute("data-state").Should().Be("open");
        cut.Find("[data-slot='menubar-content']").Should().NotBeNull();
        cut.Find("[data-slot='menubar-item']").TextContent.Should().Contain("New File");
    }
}

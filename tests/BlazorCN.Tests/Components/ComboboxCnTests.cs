using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BlazorCN.Tests.Components;

public class ComboboxCnTests : BunitContext
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

    // --- ComboboxCn ---

    [Fact]
    public void Combobox_Renders_With_DataSlot()
    {
        var cut = Render<ComboboxCn>(p => p.AddChildContent("Content"));
        cut.Find("[data-slot='combobox']").Should().NotBeNull();
    }

    [Fact]
    public void Combobox_Starts_Closed_By_Default()
    {
        var cut = Render<ComboboxCn>(p => p.AddChildContent("Content"));
        cut.Find("[data-slot='combobox']").GetAttribute("data-state").Should().Be("closed");
    }

    [Fact]
    public void Combobox_Starts_Open_When_Open_Is_True()
    {
        var cut = Render<ComboboxCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent("Content"));
        cut.Find("[data-slot='combobox']").GetAttribute("data-state").Should().Be("open");
    }

    [Fact]
    public void Combobox_Has_Default_Classes()
    {
        var cut = Render<ComboboxCn>(p => p.AddChildContent("Content"));
        var el = cut.Find("[data-slot='combobox']");
        el.ClassList.Should().Contain("relative");
        el.ClassList.Should().Contain("inline-block");
    }

    [Fact]
    public void Combobox_Class_Passthrough()
    {
        var cut = Render<ComboboxCn>(p => p
            .Add(c => c.Class, "custom-class")
            .AddChildContent("Content"));
        cut.Find("[data-slot='combobox']").ClassList.Should().Contain("custom-class");
    }

    [Fact]
    public void Combobox_AdditionalAttributes_Passthrough()
    {
        var cut = Render<ComboboxCn>(p => p
            .Add(c => c.AdditionalAttributes, new Dictionary<string, object?> { { "id", "my-combobox" } })
            .AddChildContent("Content"));
        cut.Find("[data-slot='combobox']").GetAttribute("id").Should().Be("my-combobox");
    }

    // --- ComboboxTriggerCn ---

    [Fact]
    public void ComboboxTrigger_Renders_With_DataSlot()
    {
        var cut = Render<ComboboxCn>(p => p
            .AddChildContent<ComboboxTriggerCn>(t => t
                .AddChildContent("Open")));
        cut.Find("[data-slot='combobox-trigger']").Should().NotBeNull();
    }

    [Fact]
    public void ComboboxTrigger_Is_Button()
    {
        var cut = Render<ComboboxCn>(p => p
            .AddChildContent<ComboboxTriggerCn>(t => t
                .AddChildContent("Open")));
        cut.Find("[data-slot='combobox-trigger']").TagName.Should().Be("BUTTON");
    }

    [Fact]
    public void ComboboxTrigger_Has_Default_Classes()
    {
        var cut = Render<ComboboxCn>(p => p
            .AddChildContent<ComboboxTriggerCn>(t => t
                .AddChildContent("Open")));
        var el = cut.Find("[data-slot='combobox-trigger']");
        el.ClassList.Should().Contain("cn-combobox-trigger");
        el.ClassList.Should().Contain("flex");
        el.ClassList.Should().Contain("h-9");
        el.ClassList.Should().Contain("w-full");
        el.ClassList.Should().Contain("items-center");
        el.ClassList.Should().Contain("justify-between");
        el.ClassList.Should().Contain("rounded-md");
        el.ClassList.Should().Contain("border");
        el.ClassList.Should().Contain("border-input");
        el.ClassList.Should().Contain("bg-transparent");
        el.ClassList.Should().Contain("px-3");
        el.ClassList.Should().Contain("py-2");
        el.ClassList.Should().Contain("text-sm");
        el.ClassList.Should().Contain("shadow-sm");
    }

    [Fact]
    public void ComboboxTrigger_Has_Sort_Icon()
    {
        var cut = Render<ComboboxCn>(p => p
            .AddChildContent<ComboboxTriggerCn>(t => t
                .AddChildContent("Open")));
        cut.FindAll("[data-slot='combobox-trigger'] svg").Should().NotBeEmpty();
    }

    [Fact]
    public void ComboboxTrigger_Click_Toggles_Open()
    {
        var isOpen = false;
        var cut = Render<ComboboxCn>(p => p
            .Add(c => c.OpenChanged, EventCallback.Factory.Create<bool>(this, v => isOpen = v))
            .AddChildContent<ComboboxTriggerCn>(t => t
                .AddChildContent("Open")));
        cut.Find("[data-slot='combobox-trigger']").Click();
        isOpen.Should().BeTrue();
        cut.Find("[data-slot='combobox']").GetAttribute("data-state").Should().Be("open");
    }

    [Fact]
    public void ComboboxTrigger_Click_Toggles_Closed()
    {
        var isOpen = true;
        var cut = Render<ComboboxCn>(p => p
            .Add(c => c.Open, true)
            .Add(c => c.OpenChanged, EventCallback.Factory.Create<bool>(this, v => isOpen = v))
            .AddChildContent<ComboboxTriggerCn>(t => t
                .AddChildContent("Close")));
        cut.Find("[data-slot='combobox-trigger']").Click();
        isOpen.Should().BeFalse();
    }

    [Fact]
    public void ComboboxTrigger_Class_Passthrough()
    {
        var cut = Render<ComboboxCn>(p => p
            .AddChildContent<ComboboxTriggerCn>(t => t
                .Add(c => c.Class, "trigger-class")
                .AddChildContent("Open")));
        cut.Find("[data-slot='combobox-trigger']").ClassList.Should().Contain("trigger-class");
    }

    // --- ComboboxContentCn ---

    [Fact]
    public void ComboboxContent_Not_Rendered_When_Closed()
    {
        SetupJsInterop();
        var cut = Render<ComboboxCn>(p => p
            .AddChildContent<ComboboxContentCn>(c => c
                .AddChildContent("Body")));
        cut.FindAll("[data-slot='combobox-content']").Should().BeEmpty();
    }

    [Fact]
    public void ComboboxContent_Rendered_When_Open()
    {
        SetupJsInterop();
        var cut = Render<ComboboxCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<ComboboxContentCn>(c => c
                .AddChildContent("Body")));
        cut.Find("[data-slot='combobox-content']").Should().NotBeNull();
        cut.Find("[data-slot='combobox-content']").TextContent.Should().Contain("Body");
    }

    [Fact]
    public void ComboboxContent_Has_Default_Classes()
    {
        SetupJsInterop();
        var cut = Render<ComboboxCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<ComboboxContentCn>(c => c
                .AddChildContent("Body")));
        var content = cut.Find("[data-slot='combobox-content']");
        content.ClassList.Should().Contain("cn-combobox-content");
        content.ClassList.Should().Contain("z-50");
        content.ClassList.Should().Contain("p-1");
    }

    [Fact]
    public void ComboboxContent_Default_Side_Is_Bottom()
    {
        SetupJsInterop();
        var cut = Render<ComboboxCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<ComboboxContentCn>(c => c
                .AddChildContent("Body")));
        cut.Find("[data-slot='combobox-content']").GetAttribute("data-side").Should().Be("bottom");
    }

    [Fact]
    public void ComboboxContent_Default_Align_Is_Center()
    {
        SetupJsInterop();
        var cut = Render<ComboboxCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<ComboboxContentCn>(c => c
                .AddChildContent("Body")));
        cut.Find("[data-slot='combobox-content']").GetAttribute("data-align").Should().Be("center");
    }

    [Fact]
    public void ComboboxContent_Class_Passthrough()
    {
        SetupJsInterop();
        var cut = Render<ComboboxCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<ComboboxContentCn>(c => c
                .Add(x => x.Class, "custom-content")
                .AddChildContent("Body")));
        cut.Find("[data-slot='combobox-content']").ClassList.Should().Contain("custom-content");
    }

    // --- ComboboxInputCn ---

    [Fact]
    public void ComboboxInput_Renders_With_DataSlot()
    {
        var cut = Render<ComboboxInputCn>(p => p
            .Add(c => c.Placeholder, "Search..."));
        cut.Find("[data-slot='combobox-input']").Should().NotBeNull();
    }

    [Fact]
    public void ComboboxInput_Is_Input_Element()
    {
        var cut = Render<ComboboxInputCn>(p => p
            .Add(c => c.Placeholder, "Search..."));
        cut.Find("[data-slot='combobox-input']").TagName.Should().Be("INPUT");
    }

    [Fact]
    public void ComboboxInput_Has_Default_Classes()
    {
        var cut = Render<ComboboxInputCn>(p => p
            .Add(c => c.Placeholder, "Search..."));
        var el = cut.Find("[data-slot='combobox-input']");
        el.ClassList.Should().Contain("flex");
        el.ClassList.Should().Contain("h-9");
        el.ClassList.Should().Contain("w-full");
        el.ClassList.Should().Contain("rounded-md");
        el.ClassList.Should().Contain("bg-transparent");
        el.ClassList.Should().Contain("text-sm");
        el.ClassList.Should().Contain("outline-hidden");
    }

    [Fact]
    public void ComboboxInput_Has_Placeholder()
    {
        var cut = Render<ComboboxInputCn>(p => p
            .Add(c => c.Placeholder, "Type to search..."));
        cut.Find("[data-slot='combobox-input']").GetAttribute("placeholder").Should().Be("Type to search...");
    }

    [Fact]
    public void ComboboxInput_Has_Search_Icon()
    {
        var cut = Render<ComboboxInputCn>(p => p
            .Add(c => c.Placeholder, "Search..."));
        cut.FindAll("svg").Should().NotBeEmpty();
    }

    [Fact]
    public void ComboboxInput_Fires_SearchValueChanged_On_Input()
    {
        string searchValue = "";
        var cut = Render<ComboboxInputCn>(p => p
            .Add(c => c.Placeholder, "Search...")
            .Add(c => c.SearchValueChanged, EventCallback.Factory.Create<string>(this, v => searchValue = v)));
        cut.Find("[data-slot='combobox-input']").Input("hello");
        searchValue.Should().Be("hello");
    }

    [Fact]
    public void ComboboxInput_Class_Passthrough()
    {
        var cut = Render<ComboboxInputCn>(p => p
            .Add(c => c.Placeholder, "Search...")
            .Add(c => c.Class, "custom-input"));
        cut.Find("[data-slot='combobox-input']").ClassList.Should().Contain("custom-input");
    }

    // --- ComboboxEmptyCn ---

    [Fact]
    public void ComboboxEmpty_Renders_With_DataSlot()
    {
        var cut = Render<ComboboxEmptyCn>(p => p.AddChildContent("No results"));
        cut.Find("[data-slot='combobox-empty']").Should().NotBeNull();
    }

    [Fact]
    public void ComboboxEmpty_Has_Default_Classes()
    {
        var cut = Render<ComboboxEmptyCn>(p => p.AddChildContent("No results"));
        var el = cut.Find("[data-slot='combobox-empty']");
        el.ClassList.Should().Contain("py-6");
        el.ClassList.Should().Contain("text-center");
        el.ClassList.Should().Contain("text-sm");
    }

    [Fact]
    public void ComboboxEmpty_Renders_Content()
    {
        var cut = Render<ComboboxEmptyCn>(p => p.AddChildContent("No results found."));
        cut.Find("[data-slot='combobox-empty']").TextContent.Trim().Should().Be("No results found.");
    }

    [Fact]
    public void ComboboxEmpty_Class_Passthrough()
    {
        var cut = Render<ComboboxEmptyCn>(p => p
            .Add(c => c.Class, "custom-empty")
            .AddChildContent("No results"));
        cut.Find("[data-slot='combobox-empty']").ClassList.Should().Contain("custom-empty");
    }

    // --- ComboboxGroupCn ---

    [Fact]
    public void ComboboxGroup_Renders_With_DataSlot()
    {
        var cut = Render<ComboboxGroupCn>(p => p.AddChildContent("Group"));
        cut.Find("[data-slot='combobox-group']").Should().NotBeNull();
    }

    [Fact]
    public void ComboboxGroup_Has_Role_Group()
    {
        var cut = Render<ComboboxGroupCn>(p => p.AddChildContent("Group"));
        cut.Find("[data-slot='combobox-group']").GetAttribute("role").Should().Be("group");
    }

    [Fact]
    public void ComboboxGroup_Class_Passthrough()
    {
        var cut = Render<ComboboxGroupCn>(p => p
            .Add(c => c.Class, "custom-group")
            .AddChildContent("Group"));
        cut.Find("[data-slot='combobox-group']").ClassList.Should().Contain("custom-group");
    }

    // --- ComboboxItemCn ---

    [Fact]
    public void ComboboxItem_Renders_With_DataSlot()
    {
        var cut = Render<ComboboxCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<ComboboxItemCn>(i => i
                .Add(x => x.Value, "val")
                .AddChildContent("Item 1")));
        cut.Find("[data-slot='combobox-item']").Should().NotBeNull();
    }

    [Fact]
    public void ComboboxItem_Has_DataMenuItem_Attribute()
    {
        var cut = Render<ComboboxCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<ComboboxItemCn>(i => i
                .Add(x => x.Value, "val")
                .AddChildContent("Item 1")));
        cut.Find("[data-menu-item]").Should().NotBeNull();
    }

    [Fact]
    public void ComboboxItem_Has_Default_Classes()
    {
        var cut = Render<ComboboxCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<ComboboxItemCn>(i => i
                .Add(x => x.Value, "val")
                .AddChildContent("Item")));
        var item = cut.Find("[data-slot='combobox-item']");
        item.ClassList.Should().Contain("cn-combobox-item");
        item.ClassList.Should().Contain("relative");
        item.ClassList.Should().Contain("flex");
        item.ClassList.Should().Contain("w-full");
        item.ClassList.Should().Contain("cursor-default");
        item.ClassList.Should().Contain("select-none");
        item.ClassList.Should().Contain("items-center");
        item.ClassList.Should().Contain("pl-8");
        item.ClassList.Should().Contain("outline-hidden");
    }

    [Fact]
    public void ComboboxItem_Click_Selects_Value()
    {
        string? selectedValue = null;
        var cut = Render<ComboboxCn>(p => p
            .Add(c => c.Open, true)
            .Add(c => c.ValueChanged, EventCallback.Factory.Create<string?>(this, v => selectedValue = v))
            .AddChildContent<ComboboxItemCn>(i => i
                .Add(x => x.Value, "apple")
                .AddChildContent("Apple")));
        cut.Find("[data-slot='combobox-item']").Click();
        selectedValue.Should().Be("apple");
    }

    [Fact]
    public void ComboboxItem_Click_Closes_Dropdown()
    {
        var isOpen = true;
        var cut = Render<ComboboxCn>(p => p
            .Add(c => c.Open, true)
            .Add(c => c.OpenChanged, EventCallback.Factory.Create<bool>(this, v => isOpen = v))
            .AddChildContent<ComboboxItemCn>(i => i
                .Add(x => x.Value, "apple")
                .AddChildContent("Apple")));
        cut.Find("[data-slot='combobox-item']").Click();
        isOpen.Should().BeFalse();
        cut.Find("[data-slot='combobox']").GetAttribute("data-state").Should().Be("closed");
    }

    [Fact]
    public void ComboboxItem_Selected_Shows_Check_Icon()
    {
        var cut = Render<ComboboxCn>(p => p
            .Add(c => c.Value, "apple")
            .AddChildContent<ComboboxItemCn>(i => i
                .Add(x => x.Value, "apple")
                .AddChildContent("Apple")));
        cut.FindAll("[data-slot='combobox-item'] svg").Should().NotBeEmpty();
    }

    [Fact]
    public void ComboboxItem_Not_Selected_No_Check_Icon()
    {
        var cut = Render<ComboboxCn>(p => p
            .Add(c => c.Value, "banana")
            .AddChildContent<ComboboxItemCn>(i => i
                .Add(x => x.Value, "apple")
                .AddChildContent("Apple")));
        cut.FindAll("[data-slot='combobox-item'] svg").Should().BeEmpty();
    }

    [Fact]
    public void ComboboxItem_Disabled_Has_DataDisabled()
    {
        var cut = Render<ComboboxCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<ComboboxItemCn>(i => i
                .Add(x => x.Value, "val")
                .Add(x => x.Disabled, true)
                .AddChildContent("Item")));
        cut.Find("[data-slot='combobox-item']").GetAttribute("data-disabled").Should().Be("true");
    }

    [Fact]
    public void ComboboxItem_NotDisabled_No_DataDisabled()
    {
        var cut = Render<ComboboxCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<ComboboxItemCn>(i => i
                .Add(x => x.Value, "val")
                .AddChildContent("Item")));
        cut.Find("[data-slot='combobox-item']").GetAttribute("data-disabled").Should().BeNull();
    }

    [Fact]
    public void ComboboxItem_Disabled_Click_Does_Not_Select()
    {
        string? selectedValue = null;
        var cut = Render<ComboboxCn>(p => p
            .Add(c => c.Open, true)
            .Add(c => c.ValueChanged, EventCallback.Factory.Create<string?>(this, v => selectedValue = v))
            .AddChildContent<ComboboxItemCn>(i => i
                .Add(x => x.Value, "apple")
                .Add(x => x.Disabled, true)
                .AddChildContent("Apple")));
        cut.Find("[data-slot='combobox-item']").Click();
        selectedValue.Should().BeNull();
    }

    [Fact]
    public void ComboboxItem_Class_Passthrough()
    {
        var cut = Render<ComboboxCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<ComboboxItemCn>(i => i
                .Add(x => x.Value, "val")
                .Add(x => x.Class, "custom-item")
                .AddChildContent("Item")));
        cut.Find("[data-slot='combobox-item']").ClassList.Should().Contain("custom-item");
    }

    // --- ComboboxSeparatorCn ---

    [Fact]
    public void ComboboxSeparator_Renders_With_DataSlot()
    {
        var cut = Render<ComboboxSeparatorCn>();
        cut.Find("[data-slot='combobox-separator']").Should().NotBeNull();
    }

    [Fact]
    public void ComboboxSeparator_Has_Role_Separator()
    {
        var cut = Render<ComboboxSeparatorCn>();
        cut.Find("[data-slot='combobox-separator']").GetAttribute("role").Should().Be("separator");
    }

    [Fact]
    public void ComboboxSeparator_Has_Default_Classes()
    {
        var cut = Render<ComboboxSeparatorCn>();
        var el = cut.Find("[data-slot='combobox-separator']");
        el.ClassList.Should().Contain("cn-combobox-separator");
    }

    [Fact]
    public void ComboboxSeparator_Class_Passthrough()
    {
        var cut = Render<ComboboxSeparatorCn>(p => p
            .Add(c => c.Class, "custom-sep"));
        cut.Find("[data-slot='combobox-separator']").ClassList.Should().Contain("custom-sep");
    }

    // --- ARIA ---

    [Fact]
    public void ComboboxTrigger_AriaExpanded_Reflects_State()
    {
        var cut = Render<ComboboxCn>(p => p
            .AddChildContent<ComboboxTriggerCn>(t => t
                .AddChildContent("Open")));
        var trigger = cut.Find("[data-slot='combobox-trigger']");
        trigger.GetAttribute("aria-expanded").Should().Be("false");
        trigger.Click();
        trigger.GetAttribute("aria-expanded").Should().Be("true");
    }

    [Fact]
    public void ComboboxTrigger_Has_AriaHasPopup_Listbox()
    {
        var cut = Render<ComboboxCn>(p => p
            .AddChildContent<ComboboxTriggerCn>(t => t
                .AddChildContent("Open")));
        cut.Find("[data-slot='combobox-trigger']").GetAttribute("aria-haspopup").Should().Be("listbox");
    }

    [Fact]
    public void ComboboxContent_Has_Role_Listbox()
    {
        SetupJsInterop();
        var cut = Render<ComboboxCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<ComboboxContentCn>(c => c
                .AddChildContent("Body")));
        cut.Find("[data-slot='combobox-content']").GetAttribute("role").Should().Be("listbox");
    }

    [Fact]
    public void ComboboxContent_Has_Default_AriaLabel()
    {
        SetupJsInterop();
        var cut = Render<ComboboxCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<ComboboxContentCn>(c => c
                .AddChildContent("Body")));
        cut.Find("[data-slot='combobox-content']").GetAttribute("aria-label").Should().Be("Suggestions");
    }

    [Fact]
    public void ComboboxContent_AriaLabel_Override_Via_AdditionalAttributes()
    {
        SetupJsInterop();
        var cut = Render<ComboboxCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<ComboboxContentCn>(c => c
                .Add(x => x.AdditionalAttributes, new Dictionary<string, object?> { { "aria-label", "Search results" } })
                .AddChildContent("Body")));
        cut.Find("[data-slot='combobox-content']").GetAttribute("aria-label").Should().Be("Search results");
    }

    [Fact]
    public void ComboboxItem_Has_Role_Option()
    {
        var cut = Render<ComboboxCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<ComboboxItemCn>(i => i
                .Add(x => x.Value, "val")
                .AddChildContent("Item")));
        cut.Find("[data-slot='combobox-item']").GetAttribute("role").Should().Be("option");
    }

    [Fact]
    public void ComboboxItem_AriaSelected_Reflects_Selection()
    {
        var cut = Render<ComboboxCn>(p => p
            .Add(c => c.Value, "apple")
            .AddChildContent<ComboboxItemCn>(i => i
                .Add(x => x.Value, "apple")
                .AddChildContent("Apple")));
        cut.Find("[data-slot='combobox-item']").GetAttribute("aria-selected").Should().Be("true");

        var cut2 = Render<ComboboxCn>(p => p
            .Add(c => c.Value, "banana")
            .AddChildContent<ComboboxItemCn>(i => i
                .Add(x => x.Value, "apple")
                .AddChildContent("Apple")));
        cut2.Find("[data-slot='combobox-item']").GetAttribute("aria-selected").Should().Be("false");
    }

    // --- Multiple Selection ---

    [Fact]
    public void Combobox_Multiple_Click_Toggles_Item_In_List()
    {
        List<string> selectedValues = [];
        var cut = Render<ComboboxCn>(p => p
            .Add(c => c.Multiple, true)
            .Add(c => c.Open, true)
            .Add(c => c.SelectedValuesChanged, EventCallback.Factory.Create<List<string>>(this, v => selectedValues = v))
            .AddChildContent(builder =>
            {
                builder.OpenComponent<ComboboxItemCn>(0);
                builder.AddAttribute(1, "Value", "apple");
                builder.AddAttribute(2, "ChildContent", (RenderFragment)(b => b.AddContent(0, "Apple")));
                builder.CloseComponent();
                builder.OpenComponent<ComboboxItemCn>(3);
                builder.AddAttribute(4, "Value", "banana");
                builder.AddAttribute(5, "ChildContent", (RenderFragment)(b => b.AddContent(0, "Banana")));
                builder.CloseComponent();
            }));

        // Select apple
        cut.FindAll("[data-slot='combobox-item']")[0].Click();
        selectedValues.Should().Contain("apple");

        // Select banana
        cut.FindAll("[data-slot='combobox-item']")[1].Click();
        selectedValues.Should().Contain("apple");
        selectedValues.Should().Contain("banana");

        // Deselect apple
        cut.FindAll("[data-slot='combobox-item']")[0].Click();
        selectedValues.Should().NotContain("apple");
        selectedValues.Should().Contain("banana");
    }

    [Fact]
    public void Combobox_Multiple_Does_Not_Close_On_Select()
    {
        var cut = Render<ComboboxCn>(p => p
            .Add(c => c.Multiple, true)
            .Add(c => c.Open, true)
            .AddChildContent<ComboboxItemCn>(i => i
                .Add(x => x.Value, "apple")
                .AddChildContent("Apple")));

        cut.Find("[data-slot='combobox-item']").Click();
        cut.Find("[data-slot='combobox']").GetAttribute("data-state").Should().Be("open");
    }

    [Fact]
    public void Combobox_Multiple_Shows_Check_For_Selected_Items()
    {
        var cut = Render<ComboboxCn>(p => p
            .Add(c => c.Multiple, true)
            .Add(c => c.SelectedValues, new List<string> { "apple" })
            .AddChildContent(builder =>
            {
                builder.OpenComponent<ComboboxItemCn>(0);
                builder.AddAttribute(1, "Value", "apple");
                builder.AddAttribute(2, "ChildContent", (RenderFragment)(b => b.AddContent(0, "Apple")));
                builder.CloseComponent();
                builder.OpenComponent<ComboboxItemCn>(3);
                builder.AddAttribute(4, "Value", "banana");
                builder.AddAttribute(5, "ChildContent", (RenderFragment)(b => b.AddContent(0, "Banana")));
                builder.CloseComponent();
            }));

        // Apple should show check icon
        cut.FindAll("[data-slot='combobox-item']")[0].QuerySelectorAll("svg").Should().NotBeEmpty();
        // Banana should not
        cut.FindAll("[data-slot='combobox-item']")[1].QuerySelectorAll("svg").Should().BeEmpty();
    }

    // --- Clear ---

    [Fact]
    public void ComboboxTrigger_ShowClear_False_No_Clear_Button()
    {
        var cut = Render<ComboboxCn>(p => p
            .Add(c => c.Value, "apple")
            .AddChildContent<ComboboxTriggerCn>(t => t
                .AddChildContent("Apple")));
        cut.FindAll("[data-slot='combobox-clear']").Should().BeEmpty();
    }

    [Fact]
    public void ComboboxTrigger_ShowClear_True_No_Value_No_Clear_Button()
    {
        var cut = Render<ComboboxCn>(p => p
            .AddChildContent<ComboboxTriggerCn>(t => t
                .Add(x => x.ShowClear, true)
                .AddChildContent("Select...")));
        cut.FindAll("[data-slot='combobox-clear']").Should().BeEmpty();
    }

    [Fact]
    public void ComboboxTrigger_ShowClear_True_With_Value_Shows_Clear_Button()
    {
        var cut = Render<ComboboxCn>(p => p
            .Add(c => c.Value, "apple")
            .AddChildContent<ComboboxTriggerCn>(t => t
                .Add(x => x.ShowClear, true)
                .AddChildContent("Apple")));
        cut.Find("[data-slot='combobox-clear']").Should().NotBeNull();
    }

    [Fact]
    public void ComboboxTrigger_Clear_Button_Clears_Value()
    {
        string? selectedValue = "apple";
        var cut = Render<ComboboxCn>(p => p
            .Add(c => c.Value, "apple")
            .Add(c => c.ValueChanged, EventCallback.Factory.Create<string?>(this, v => selectedValue = v))
            .AddChildContent<ComboboxTriggerCn>(t => t
                .Add(x => x.ShowClear, true)
                .AddChildContent("Apple")));
        cut.Find("[data-slot='combobox-clear']").Click();
        selectedValue.Should().BeNull();
    }

    [Fact]
    public void ComboboxTrigger_Clear_Multiple_Clears_All()
    {
        List<string> selectedValues = ["apple", "banana"];
        var cut = Render<ComboboxCn>(p => p
            .Add(c => c.Multiple, true)
            .Add(c => c.SelectedValues, new List<string> { "apple", "banana" })
            .Add(c => c.SelectedValuesChanged, EventCallback.Factory.Create<List<string>>(this, v => selectedValues = v))
            .AddChildContent<ComboboxTriggerCn>(t => t
                .Add(x => x.ShowClear, true)
                .AddChildContent("2 selected")));
        cut.Find("[data-slot='combobox-clear']").Click();
        selectedValues.Should().BeEmpty();
    }

    // --- Integration ---

    [Fact]
    public void Combobox_Full_Integration_Toggle()
    {
        SetupJsInterop();
        var isOpen = false;
        var cut = Render<ComboboxCn>(p => p
            .Add(c => c.OpenChanged, EventCallback.Factory.Create<bool>(this, v => isOpen = v))
            .AddChildContent<ComboboxTriggerCn>(t => t
                .AddChildContent("Toggle")));

        // Initially closed
        cut.Find("[data-slot='combobox']").GetAttribute("data-state").Should().Be("closed");

        // Click trigger to open
        cut.Find("[data-slot='combobox-trigger']").Click();
        isOpen.Should().BeTrue();
        cut.Find("[data-slot='combobox']").GetAttribute("data-state").Should().Be("open");

        // Click trigger again to close
        cut.Find("[data-slot='combobox-trigger']").Click();
        isOpen.Should().BeFalse();
        cut.Find("[data-slot='combobox']").GetAttribute("data-state").Should().Be("closed");
    }

    [Fact]
    public void Combobox_Full_Integration_Select_Item()
    {
        SetupJsInterop();
        string? selectedValue = null;
        var cut = Render<ComboboxCn>(p => p
            .Add(c => c.Open, true)
            .Add(c => c.ValueChanged, EventCallback.Factory.Create<string?>(this, v => selectedValue = v))
            .AddChildContent(builder =>
            {
                builder.OpenComponent<ComboboxTriggerCn>(0);
                builder.AddAttribute(1, "ChildContent", (RenderFragment)(b => b.AddContent(0, "Select...")));
                builder.CloseComponent();
                builder.OpenComponent<ComboboxContentCn>(2);
                builder.AddAttribute(3, "ChildContent", (RenderFragment)(b =>
                {
                    b.OpenComponent<ComboboxItemCn>(0);
                    b.AddAttribute(1, "Value", "react");
                    b.AddAttribute(2, "ChildContent", (RenderFragment)(ib => ib.AddContent(0, "React")));
                    b.CloseComponent();
                    b.OpenComponent<ComboboxItemCn>(3);
                    b.AddAttribute(4, "Value", "vue");
                    b.AddAttribute(5, "ChildContent", (RenderFragment)(ib => ib.AddContent(0, "Vue")));
                    b.CloseComponent();
                }));
                builder.CloseComponent();
            }));

        // Click vue
        cut.FindAll("[data-slot='combobox-item']")[1].Click();
        selectedValue.Should().Be("vue");
        // Should be closed now
        cut.Find("[data-slot='combobox']").GetAttribute("data-state").Should().Be("closed");
    }
}

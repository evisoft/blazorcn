using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BlazorCN.Tests.Components;

public class SelectCnTests : BunitContext
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

    // --- SelectCn ---

    [Fact]
    public void Select_Renders_With_DataSlot()
    {
        var cut = Render<SelectCn>(p => p.AddChildContent("Content"));
        cut.Find("[data-slot='select']").Should().NotBeNull();
    }

    [Fact]
    public void Select_Starts_Closed_By_Default()
    {
        var cut = Render<SelectCn>(p => p.AddChildContent("Content"));
        cut.Find("[data-slot='select']").GetAttribute("data-state").Should().Be("closed");
    }

    [Fact]
    public void Select_Starts_Open_When_Open_Is_True()
    {
        var cut = Render<SelectCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent("Content"));
        cut.Find("[data-slot='select']").GetAttribute("data-state").Should().Be("open");
    }

    [Fact]
    public void Select_Has_Default_Classes()
    {
        // Root is `block w-full` (not `inline-block`) so the Select fills its container
        // and doesn't shrink to the selected text — the parent layout controls width.
        var cut = Render<SelectCn>(p => p.AddChildContent("Content"));
        var el = cut.Find("[data-slot='select']");
        el.ClassList.Should().Contain("relative");
        el.ClassList.Should().Contain("block");
        el.ClassList.Should().Contain("w-full");
    }

    [Fact]
    public void Select_Class_Passthrough()
    {
        var cut = Render<SelectCn>(p => p
            .Add(c => c.Class, "custom-class")
            .AddChildContent("Content"));
        cut.Find("[data-slot='select']").ClassList.Should().Contain("custom-class");
    }

    [Fact]
    public void Select_AdditionalAttributes_Passthrough()
    {
        var cut = Render<SelectCn>(p => p
            .Add(c => c.AdditionalAttributes, new Dictionary<string, object?> { { "id", "my-select" } })
            .AddChildContent("Content"));
        cut.Find("[data-slot='select']").GetAttribute("id").Should().Be("my-select");
    }

    // --- SelectTriggerCn ---

    [Fact]
    public void SelectTrigger_Renders_With_DataSlot()
    {
        var cut = Render<SelectCn>(p => p
            .AddChildContent<SelectTriggerCn>(t => t
                .AddChildContent("Open")));
        cut.Find("[data-slot='select-trigger']").Should().NotBeNull();
    }

    [Fact]
    public void SelectTrigger_Is_Button()
    {
        var cut = Render<SelectCn>(p => p
            .AddChildContent<SelectTriggerCn>(t => t
                .AddChildContent("Open")));
        cut.Find("[data-slot='select-trigger']").TagName.Should().Be("BUTTON");
    }

    [Fact]
    public void SelectTrigger_Has_Default_Classes()
    {
        var cut = Render<SelectCn>(p => p
            .AddChildContent<SelectTriggerCn>(t => t
                .AddChildContent("Open")));
        var el = cut.Find("[data-slot='select-trigger']");
        el.ClassList.Should().Contain("cn-select-trigger");
        el.ClassList.Should().Contain("flex");
        // w-full (was w-fit) so the trigger fills its container and doesn't resize
        // as the selected text length changes.
        el.ClassList.Should().Contain("w-full");
        el.ClassList.Should().Contain("items-center");
        el.ClassList.Should().Contain("justify-between");
        el.ClassList.Should().Contain("outline-hidden");
    }

    [Fact]
    public void SelectTrigger_Has_Chevron_Icon()
    {
        var cut = Render<SelectCn>(p => p
            .AddChildContent<SelectTriggerCn>(t => t
                .AddChildContent("Open")));
        cut.FindAll("[data-slot='select-trigger'] svg").Should().NotBeEmpty();
    }

    [Fact]
    public void SelectTrigger_Click_Toggles_Open()
    {
        var isOpen = false;
        var cut = Render<SelectCn>(p => p
            .Add(c => c.OpenChanged, EventCallback.Factory.Create<bool>(this, v => isOpen = v))
            .AddChildContent<SelectTriggerCn>(t => t
                .AddChildContent("Open")));
        cut.Find("[data-slot='select-trigger']").Click();
        isOpen.Should().BeTrue();
        cut.Find("[data-slot='select']").GetAttribute("data-state").Should().Be("open");
    }

    [Fact]
    public void SelectTrigger_Click_Toggles_Closed()
    {
        var isOpen = true;
        var cut = Render<SelectCn>(p => p
            .Add(c => c.Open, true)
            .Add(c => c.OpenChanged, EventCallback.Factory.Create<bool>(this, v => isOpen = v))
            .AddChildContent<SelectTriggerCn>(t => t
                .AddChildContent("Close")));
        cut.Find("[data-slot='select-trigger']").Click();
        isOpen.Should().BeFalse();
    }

    [Fact]
    public void SelectTrigger_Disabled_Does_Not_Open()
    {
        var isOpen = false;
        var cut = Render<SelectCn>(p => p
            .Add(c => c.Disabled, true)
            .Add(c => c.OpenChanged, EventCallback.Factory.Create<bool>(this, v => isOpen = v))
            .AddChildContent<SelectTriggerCn>(t => t
                .AddChildContent("Open")));
        // Button is disabled so click won't fire
        cut.Find("[data-slot='select-trigger']").HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public void SelectTrigger_Class_Passthrough()
    {
        var cut = Render<SelectCn>(p => p
            .AddChildContent<SelectTriggerCn>(t => t
                .Add(c => c.Class, "trigger-class")
                .AddChildContent("Open")));
        cut.Find("[data-slot='select-trigger']").ClassList.Should().Contain("trigger-class");
    }

    // --- SelectValueCn ---

    [Fact]
    public void SelectValue_Renders_With_DataSlot()
    {
        var cut = Render<SelectCn>(p => p
            .AddChildContent<SelectValueCn>(v => v
                .Add(c => c.Placeholder, "Pick one")));
        cut.Find("[data-slot='select-value']").Should().NotBeNull();
    }

    [Fact]
    public void SelectValue_Shows_Placeholder_When_No_Value()
    {
        var cut = Render<SelectCn>(p => p
            .AddChildContent<SelectValueCn>(v => v
                .Add(c => c.Placeholder, "Pick one")));
        cut.Find("[data-slot='select-value']").TextContent.Trim().Should().Be("Pick one");
    }

    [Fact]
    public void SelectValue_Has_Truncate_Class()
    {
        var cut = Render<SelectCn>(p => p
            .AddChildContent<SelectValueCn>(v => v
                .Add(c => c.Placeholder, "Pick one")));
        var el = cut.Find("[data-slot='select-value']");
        el.ClassList.Should().Contain("cn-select-value");
        el.ClassList.Should().Contain("truncate");
    }

    [Fact]
    public void SelectValue_Shows_Selected_Text_After_Selection()
    {
        SetupJsInterop();
        var cut = Render<SelectCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent(builder =>
            {
                builder.OpenComponent<SelectValueCn>(0);
                builder.AddAttribute(1, "Placeholder", "Pick one");
                builder.CloseComponent();
                builder.OpenComponent<SelectContentCn>(2);
                builder.AddAttribute(3, "ChildContent", (RenderFragment)(b =>
                {
                    b.OpenComponent<SelectItemCn>(0);
                    b.AddAttribute(1, "Value", "apple");
                    b.AddAttribute(2, "ChildContent", (RenderFragment)(ib => ib.AddContent(0, "Apple")));
                    b.CloseComponent();
                }));
                builder.CloseComponent();
            }));

        cut.Find("[data-slot='select-item']").Click();
        // Trigger displays the item's ChildContent text ("Apple"), not the raw Value ("apple").
        // This is the whole point of having separate Value + ChildContent — the user sees
        // "Friday" while the form-bound value is "5".
        cut.Find("[data-slot='select-value']").TextContent.Trim().Should().Be("Apple");
    }

    // --- SelectContentCn ---

    [Fact]
    public void SelectContent_Hidden_When_Closed()
    {
        // SelectContentCn now renders even when closed (so child SelectItemCn instances
        // can register their display text), but applies the `hidden` HTML attribute to
        // remove it from layout and the accessibility tree.
        SetupJsInterop();
        var cut = Render<SelectCn>(p => p
            .AddChildContent<SelectContentCn>(c => c
                .AddChildContent("Body")));
        var content = cut.Find("[data-slot='select-content']");
        content.HasAttribute("hidden").Should().BeTrue();
        content.GetAttribute("data-state").Should().Be("closed");
    }

    [Fact]
    public void SelectContent_Rendered_When_Open()
    {
        SetupJsInterop();
        var cut = Render<SelectCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<SelectContentCn>(c => c
                .AddChildContent("Body")));
        cut.Find("[data-slot='select-content']").Should().NotBeNull();
        cut.Find("[data-slot='select-content']").TextContent.Should().Contain("Body");
    }

    [Fact]
    public void SelectContent_Has_Default_Classes()
    {
        SetupJsInterop();
        var cut = Render<SelectCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<SelectContentCn>(c => c
                .AddChildContent("Body")));
        var content = cut.Find("[data-slot='select-content']");
        content.ClassList.Should().Contain("cn-select-content");
        content.ClassList.Should().Contain("z-50");
        content.ClassList.Should().Contain("overflow-hidden");
        content.ClassList.Should().Contain("p-1");
    }

    [Fact]
    public void SelectContent_Default_Side_Is_Bottom()
    {
        SetupJsInterop();
        var cut = Render<SelectCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<SelectContentCn>(c => c
                .AddChildContent("Body")));
        cut.Find("[data-slot='select-content']").GetAttribute("data-side").Should().Be("bottom");
    }

    [Fact]
    public void SelectContent_Default_Align_Is_Center()
    {
        SetupJsInterop();
        var cut = Render<SelectCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<SelectContentCn>(c => c
                .AddChildContent("Body")));
        cut.Find("[data-slot='select-content']").GetAttribute("data-align").Should().Be("center");
    }

    [Fact]
    public void SelectContent_Class_Passthrough()
    {
        SetupJsInterop();
        var cut = Render<SelectCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<SelectContentCn>(c => c
                .Add(x => x.Class, "custom-content")
                .AddChildContent("Body")));
        cut.Find("[data-slot='select-content']").ClassList.Should().Contain("custom-content");
    }

    // --- SelectItemCn ---

    [Fact]
    public void SelectItem_Renders_With_DataSlot()
    {
        var cut = Render<SelectCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<SelectItemCn>(i => i
                .Add(x => x.Value, "val")
                .AddChildContent("Item 1")));
        cut.Find("[data-slot='select-item']").Should().NotBeNull();
    }

    [Fact]
    public void SelectItem_Has_DataMenuItem_Attribute()
    {
        var cut = Render<SelectCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<SelectItemCn>(i => i
                .Add(x => x.Value, "val")
                .AddChildContent("Item 1")));
        cut.Find("[data-menu-item]").Should().NotBeNull();
    }

    [Fact]
    public void SelectItem_Has_Default_Classes()
    {
        var cut = Render<SelectCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<SelectItemCn>(i => i
                .Add(x => x.Value, "val")
                .AddChildContent("Item")));
        var item = cut.Find("[data-slot='select-item']");
        item.ClassList.Should().Contain("cn-select-item");
        item.ClassList.Should().Contain("relative");
        item.ClassList.Should().Contain("flex");
        item.ClassList.Should().Contain("w-full");
        item.ClassList.Should().Contain("cursor-default");
        item.ClassList.Should().Contain("select-none");
        item.ClassList.Should().Contain("items-center");
        item.ClassList.Should().Contain("outline-hidden");
    }

    [Fact]
    public void SelectItem_Click_Selects_Value()
    {
        string? selectedValue = null;
        var cut = Render<SelectCn>(p => p
            .Add(c => c.Open, true)
            .Add(c => c.ValueChanged, EventCallback.Factory.Create<string?>(this, v => selectedValue = v))
            .AddChildContent<SelectItemCn>(i => i
                .Add(x => x.Value, "apple")
                .AddChildContent("Apple")));
        cut.Find("[data-slot='select-item']").Click();
        selectedValue.Should().Be("apple");
    }

    [Fact]
    public void SelectItem_Click_Closes_Dropdown()
    {
        var isOpen = true;
        var cut = Render<SelectCn>(p => p
            .Add(c => c.Open, true)
            .Add(c => c.OpenChanged, EventCallback.Factory.Create<bool>(this, v => isOpen = v))
            .AddChildContent<SelectItemCn>(i => i
                .Add(x => x.Value, "apple")
                .AddChildContent("Apple")));
        cut.Find("[data-slot='select-item']").Click();
        isOpen.Should().BeFalse();
        cut.Find("[data-slot='select']").GetAttribute("data-state").Should().Be("closed");
    }

    [Fact]
    public void SelectItem_Selected_Shows_Check_Icon()
    {
        var cut = Render<SelectCn>(p => p
            .Add(c => c.Value, "apple")
            .AddChildContent<SelectItemCn>(i => i
                .Add(x => x.Value, "apple")
                .AddChildContent("Apple")));
        cut.FindAll("[data-slot='select-item'] svg").Should().NotBeEmpty();
    }

    [Fact]
    public void SelectItem_Not_Selected_No_Check_Icon()
    {
        var cut = Render<SelectCn>(p => p
            .Add(c => c.Value, "banana")
            .AddChildContent<SelectItemCn>(i => i
                .Add(x => x.Value, "apple")
                .AddChildContent("Apple")));
        cut.FindAll("[data-slot='select-item'] svg").Should().BeEmpty();
    }

    [Fact]
    public void SelectItem_Disabled_Has_DataDisabled()
    {
        var cut = Render<SelectCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<SelectItemCn>(i => i
                .Add(x => x.Value, "val")
                .Add(x => x.Disabled, true)
                .AddChildContent("Item")));
        cut.Find("[data-slot='select-item']").GetAttribute("data-disabled").Should().Be("true");
    }

    [Fact]
    public void SelectItem_NotDisabled_No_DataDisabled()
    {
        var cut = Render<SelectCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<SelectItemCn>(i => i
                .Add(x => x.Value, "val")
                .AddChildContent("Item")));
        cut.Find("[data-slot='select-item']").GetAttribute("data-disabled").Should().BeNull();
    }

    [Fact]
    public void SelectItem_Disabled_Click_Does_Not_Select()
    {
        string? selectedValue = null;
        var cut = Render<SelectCn>(p => p
            .Add(c => c.Open, true)
            .Add(c => c.ValueChanged, EventCallback.Factory.Create<string?>(this, v => selectedValue = v))
            .AddChildContent<SelectItemCn>(i => i
                .Add(x => x.Value, "apple")
                .Add(x => x.Disabled, true)
                .AddChildContent("Apple")));
        cut.Find("[data-slot='select-item']").Click();
        selectedValue.Should().BeNull();
    }

    [Fact]
    public void SelectItem_Class_Passthrough()
    {
        var cut = Render<SelectCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<SelectItemCn>(i => i
                .Add(x => x.Value, "val")
                .Add(x => x.Class, "custom-item")
                .AddChildContent("Item")));
        cut.Find("[data-slot='select-item']").ClassList.Should().Contain("custom-item");
    }

    // --- SelectGroupCn ---

    [Fact]
    public void SelectGroup_Renders_With_DataSlot()
    {
        var cut = Render<SelectGroupCn>(p => p.AddChildContent("Group"));
        cut.Find("[data-slot='select-group']").Should().NotBeNull();
    }

    [Fact]
    public void SelectGroup_Has_Role_Group()
    {
        var cut = Render<SelectGroupCn>(p => p.AddChildContent("Group"));
        cut.Find("[data-slot='select-group']").GetAttribute("role").Should().Be("group");
    }

    [Fact]
    public void SelectGroup_Class_Passthrough()
    {
        var cut = Render<SelectGroupCn>(p => p
            .Add(c => c.Class, "custom-group")
            .AddChildContent("Group"));
        cut.Find("[data-slot='select-group']").ClassList.Should().Contain("custom-group");
    }

    // --- SelectLabelCn ---

    [Fact]
    public void SelectLabel_Renders_With_DataSlot()
    {
        var cut = Render<SelectLabelCn>(p => p.AddChildContent("Label"));
        cut.Find("[data-slot='select-label']").Should().NotBeNull();
    }

    [Fact]
    public void SelectLabel_Has_Default_Classes()
    {
        var cut = Render<SelectLabelCn>(p => p.AddChildContent("Label"));
        var el = cut.Find("[data-slot='select-label']");
        el.ClassList.Should().Contain("cn-select-label");
    }

    [Fact]
    public void SelectLabel_Class_Passthrough()
    {
        var cut = Render<SelectLabelCn>(p => p
            .Add(c => c.Class, "custom-label")
            .AddChildContent("Label"));
        cut.Find("[data-slot='select-label']").ClassList.Should().Contain("custom-label");
    }

    // --- SelectSeparatorCn ---

    [Fact]
    public void SelectSeparator_Renders_With_DataSlot()
    {
        var cut = Render<SelectSeparatorCn>();
        cut.Find("[data-slot='select-separator']").Should().NotBeNull();
    }

    [Fact]
    public void SelectSeparator_Has_Role_Separator()
    {
        var cut = Render<SelectSeparatorCn>();
        cut.Find("[data-slot='select-separator']").GetAttribute("role").Should().Be("separator");
    }

    [Fact]
    public void SelectSeparator_Has_Default_Classes()
    {
        var cut = Render<SelectSeparatorCn>();
        var el = cut.Find("[data-slot='select-separator']");
        el.ClassList.Should().Contain("cn-select-separator");
        el.ClassList.Should().Contain("pointer-events-none");
    }

    [Fact]
    public void SelectSeparator_Class_Passthrough()
    {
        var cut = Render<SelectSeparatorCn>(p => p
            .Add(c => c.Class, "custom-sep"));
        cut.Find("[data-slot='select-separator']").ClassList.Should().Contain("custom-sep");
    }

    // --- ARIA ---

    [Fact]
    public void SelectTrigger_AriaExpanded_Reflects_State()
    {
        var cut = Render<SelectCn>(p => p
            .AddChildContent<SelectTriggerCn>(t => t
                .AddChildContent("Open")));
        var trigger = cut.Find("[data-slot='select-trigger']");
        trigger.GetAttribute("aria-expanded").Should().Be("false");
        trigger.Click();
        trigger.GetAttribute("aria-expanded").Should().Be("true");
    }

    [Fact]
    public void SelectTrigger_Has_AriaHasPopup_Listbox()
    {
        var cut = Render<SelectCn>(p => p
            .AddChildContent<SelectTriggerCn>(t => t
                .AddChildContent("Open")));
        cut.Find("[data-slot='select-trigger']").GetAttribute("aria-haspopup").Should().Be("listbox");
    }

    [Fact]
    public void SelectContent_Has_Role_Listbox()
    {
        SetupJsInterop();
        var cut = Render<SelectCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<SelectContentCn>(c => c
                .AddChildContent("Body")));
        cut.Find("[data-slot='select-content']").GetAttribute("role").Should().Be("listbox");
    }

    [Fact]
    public void SelectContent_Has_Default_AriaLabel()
    {
        SetupJsInterop();
        var cut = Render<SelectCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<SelectContentCn>(c => c
                .AddChildContent("Body")));
        cut.Find("[data-slot='select-content']").GetAttribute("aria-label").Should().Be("Options");
    }

    [Fact]
    public void SelectContent_AriaLabel_Override_Via_AdditionalAttributes()
    {
        SetupJsInterop();
        var cut = Render<SelectCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<SelectContentCn>(c => c
                .Add(x => x.AdditionalAttributes, new Dictionary<string, object?> { { "aria-label", "Choose a fruit" } })
                .AddChildContent("Body")));
        cut.Find("[data-slot='select-content']").GetAttribute("aria-label").Should().Be("Choose a fruit");
    }

    [Fact]
    public void SelectItem_Has_Role_Option()
    {
        var cut = Render<SelectCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<SelectItemCn>(i => i
                .Add(x => x.Value, "val")
                .AddChildContent("Item")));
        cut.Find("[data-slot='select-item']").GetAttribute("role").Should().Be("option");
    }

    [Fact]
    public void SelectItem_AriaSelected_Reflects_Selection()
    {
        var cut = Render<SelectCn>(p => p
            .Add(c => c.Value, "apple")
            .AddChildContent<SelectItemCn>(i => i
                .Add(x => x.Value, "apple")
                .AddChildContent("Apple")));
        cut.Find("[data-slot='select-item']").GetAttribute("aria-selected").Should().Be("true");

        var cut2 = Render<SelectCn>(p => p
            .Add(c => c.Value, "banana")
            .AddChildContent<SelectItemCn>(i => i
                .Add(x => x.Value, "apple")
                .AddChildContent("Apple")));
        cut2.Find("[data-slot='select-item']").GetAttribute("aria-selected").Should().Be("false");
    }

    // --- Integration ---

    [Fact]
    public void Select_Full_Integration_Toggle()
    {
        SetupJsInterop();
        var isOpen = false;
        var cut = Render<SelectCn>(p => p
            .Add(c => c.OpenChanged, EventCallback.Factory.Create<bool>(this, v => isOpen = v))
            .AddChildContent<SelectTriggerCn>(t => t
                .AddChildContent("Toggle")));

        // Initially closed
        cut.Find("[data-slot='select']").GetAttribute("data-state").Should().Be("closed");

        // Click trigger to open
        cut.Find("[data-slot='select-trigger']").Click();
        isOpen.Should().BeTrue();
        cut.Find("[data-slot='select']").GetAttribute("data-state").Should().Be("open");

        // Click trigger again to close
        cut.Find("[data-slot='select-trigger']").Click();
        isOpen.Should().BeFalse();
        cut.Find("[data-slot='select']").GetAttribute("data-state").Should().Be("closed");
    }

    [Fact]
    public void Select_Full_Integration_Select_Item()
    {
        SetupJsInterop();
        string? selectedValue = null;
        var cut = Render<SelectCn>(p => p
            .Add(c => c.Open, true)
            .Add(c => c.ValueChanged, EventCallback.Factory.Create<string?>(this, v => selectedValue = v))
            .AddChildContent(builder =>
            {
                builder.OpenComponent<SelectTriggerCn>(0);
                builder.AddAttribute(1, "ChildContent", (RenderFragment)(b =>
                {
                    b.OpenComponent<SelectValueCn>(0);
                    b.AddAttribute(1, "Placeholder", "Pick a fruit");
                    b.CloseComponent();
                }));
                builder.CloseComponent();
                builder.OpenComponent<SelectContentCn>(2);
                builder.AddAttribute(3, "ChildContent", (RenderFragment)(b =>
                {
                    b.OpenComponent<SelectItemCn>(0);
                    b.AddAttribute(1, "Value", "apple");
                    b.AddAttribute(2, "ChildContent", (RenderFragment)(ib => ib.AddContent(0, "Apple")));
                    b.CloseComponent();
                    b.OpenComponent<SelectItemCn>(3);
                    b.AddAttribute(4, "Value", "banana");
                    b.AddAttribute(5, "ChildContent", (RenderFragment)(ib => ib.AddContent(0, "Banana")));
                    b.CloseComponent();
                }));
                builder.CloseComponent();
            }));

        // Click apple
        cut.FindAll("[data-slot='select-item']")[0].Click();
        selectedValue.Should().Be("apple");
        // Should be closed now
        cut.Find("[data-slot='select']").GetAttribute("data-state").Should().Be("closed");
    }
}

using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace BlazorCN.Tests.Components;

public class ToggleGroupCnTests : BunitContext
{
    // --- ToggleGroupCn ---

    [Fact]
    public void ToggleGroup_Renders_With_DataSlot()
    {
        var cut = Render<ToggleGroupCn>(p => p.AddChildContent("Content"));
        cut.Find("[data-slot='toggle-group']").Should().NotBeNull();
    }

    [Fact]
    public void ToggleGroup_Has_Group_Role()
    {
        var cut = Render<ToggleGroupCn>(p => p.AddChildContent("Content"));
        cut.Find("[data-slot='toggle-group']").GetAttribute("role").Should().Be("group");
    }

    [Fact]
    public void ToggleGroup_Has_Default_Classes()
    {
        // Gap is now driven by the --gap CSS variable (Spacing parameter), not a fixed gap-0.
        var cut = Render<ToggleGroupCn>(p => p.AddChildContent("Content"));
        var el = cut.Find("[data-slot='toggle-group']");
        el.ClassList.Should().Contain("flex");
        el.ClassList.Should().Contain("items-center");
        el.ClassList.Should().Contain("gap-[--spacing(var(--gap))]");
    }

    [Fact]
    public void ToggleGroup_Class_Passthrough()
    {
        var cut = Render<ToggleGroupCn>(p => p
            .Add(c => c.Class, "custom-group")
            .AddChildContent("Content"));
        cut.Find("[data-slot='toggle-group']").ClassList.Should().Contain("custom-group");
    }

    [Fact]
    public void ToggleGroup_AdditionalAttributes_Passthrough()
    {
        var cut = Render<ToggleGroupCn>(p => p
            .Add(c => c.AdditionalAttributes, new Dictionary<string, object?> { { "id", "my-group" } })
            .AddChildContent("Content"));
        cut.Find("[data-slot='toggle-group']").GetAttribute("id").Should().Be("my-group");
    }

    // --- ToggleGroupItemCn ---

    [Fact]
    public void ToggleGroupItem_Renders_With_DataSlot()
    {
        var cut = Render<ToggleGroupCn>(p => p
            .AddChildContent<ToggleGroupItemCn>(i => i
                .Add(c => c.Value, "bold")
                .AddChildContent("Bold")));
        cut.Find("[data-slot='toggle-group-item']").Should().NotBeNull();
    }

    [Fact]
    public void ToggleGroupItem_Starts_Off()
    {
        var cut = Render<ToggleGroupCn>(p => p
            .AddChildContent<ToggleGroupItemCn>(i => i
                .Add(c => c.Value, "bold")
                .AddChildContent("Bold")));
        var btn = cut.Find("[data-slot='toggle-group-item']");
        btn.GetAttribute("data-state").Should().Be("off");
        btn.GetAttribute("aria-pressed").Should().Be("false");
    }

    [Fact]
    public void ToggleGroupItem_On_When_Value_Matches()
    {
        var cut = Render<ToggleGroupCn>(p => p
            .Add(c => c.Value, "bold")
            .AddChildContent<ToggleGroupItemCn>(i => i
                .Add(c => c.Value, "bold")
                .AddChildContent("Bold")));
        var btn = cut.Find("[data-slot='toggle-group-item']");
        btn.GetAttribute("data-state").Should().Be("on");
        btn.GetAttribute("aria-pressed").Should().Be("true");
    }

    [Fact]
    public void ToggleGroupItem_Click_Selects_Item()
    {
        string? selected = null;
        var cut = Render<ToggleGroupCn>(p => p
            .Add(c => c.ValueChanged, EventCallback.Factory.Create<string?>(this, v => selected = v))
            .AddChildContent<ToggleGroupItemCn>(i => i
                .Add(c => c.Value, "bold")
                .AddChildContent("Bold")));
        cut.Find("[data-slot='toggle-group-item']").Click();
        selected.Should().Be("bold");
    }

    [Fact]
    public void ToggleGroupItem_Click_Deselects_When_Already_Selected()
    {
        string? selected = "bold";
        var cut = Render<ToggleGroupCn>(p => p
            .Add(c => c.Value, "bold")
            .Add(c => c.ValueChanged, EventCallback.Factory.Create<string?>(this, v => selected = v))
            .AddChildContent<ToggleGroupItemCn>(i => i
                .Add(c => c.Value, "bold")
                .AddChildContent("Bold")));
        cut.Find("[data-slot='toggle-group-item']").Click();
        selected.Should().BeNull();
    }

    // --- Sibling re-render on selection change ---

    [Fact]
    public void ToggleGroup_Siblings_Update_When_Selection_Changes()
    {
        // Regression test: without StateHasChanged() in SelectItem,
        // the previously-selected sibling wouldn't re-render to show "off" state
        var cut = Render<ToggleGroupCn>(p => p
            .Add(c => c.Value, "a")
            .AddChildContent(builder =>
            {
                builder.OpenComponent<ToggleGroupItemCn>(0);
                builder.AddAttribute(1, "Value", "a");
                builder.AddAttribute(2, "ChildContent", (RenderFragment)(b => b.AddContent(0, "A")));
                builder.CloseComponent();
                builder.OpenComponent<ToggleGroupItemCn>(3);
                builder.AddAttribute(4, "Value", "b");
                builder.AddAttribute(5, "ChildContent", (RenderFragment)(b => b.AddContent(0, "B")));
                builder.CloseComponent();
            }));

        var buttons = cut.FindAll("[data-slot='toggle-group-item']");
        buttons[0].GetAttribute("data-state").Should().Be("on");
        buttons[1].GetAttribute("data-state").Should().Be("off");

        // Click B — A should become "off", B should become "on"
        buttons[1].Click();

        buttons = cut.FindAll("[data-slot='toggle-group-item']");
        buttons[0].GetAttribute("data-state").Should().Be("off");
        buttons[1].GetAttribute("data-state").Should().Be("on");
    }
}

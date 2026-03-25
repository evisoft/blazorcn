using Bunit;
using FluentAssertions;
using Xunit;

namespace BlazorCN.Tests.Components;

public class ButtonGroupCnTests : BunitContext
{
    [Fact]
    public void ButtonGroup_Renders_With_DataSlot()
    {
        var cut = Render<ButtonGroupCn>(p => p.AddChildContent("Content"));
        cut.Find("[data-slot='button-group']").Should().NotBeNull();
    }

    [Fact]
    public void ButtonGroup_Has_Role_Group()
    {
        var cut = Render<ButtonGroupCn>(p => p.AddChildContent("Content"));
        cut.Find("[data-slot='button-group']").GetAttribute("role").Should().Be("group");
    }

    [Fact]
    public void ButtonGroup_Default_Orientation_Is_Horizontal()
    {
        var cut = Render<ButtonGroupCn>(p => p.AddChildContent("Content"));
        cut.Find("[data-slot='button-group']").GetAttribute("data-orientation").Should().Be("horizontal");
    }

    [Fact]
    public void ButtonGroup_Vertical_Orientation_Applies_FlexCol()
    {
        var cut = Render<ButtonGroupCn>(p => p
            .Add(c => c.Orientation, Orientation.Vertical)
            .AddChildContent("Content"));
        var el = cut.Find("[data-slot='button-group']");
        el.GetAttribute("data-orientation").Should().Be("vertical");
        el.ClassList.Should().Contain("flex-col");
    }

    [Fact]
    public void ButtonGroup_Has_Default_Classes()
    {
        var cut = Render<ButtonGroupCn>(p => p.AddChildContent("Content"));
        var el = cut.Find("[data-slot='button-group']");
        el.ClassList.Should().Contain("flex");
        el.ClassList.Should().Contain("w-fit");
        el.ClassList.Should().Contain("items-stretch");
    }

    [Fact]
    public void ButtonGroup_ChildContent_Renders()
    {
        var cut = Render<ButtonGroupCn>(p => p.AddChildContent("<span>Inner</span>"));
        cut.Find("[data-slot='button-group']").InnerHtml.Should().Contain("Inner");
    }

    [Fact]
    public void ButtonGroup_Class_Passthrough()
    {
        var cut = Render<ButtonGroupCn>(p => p
            .Add(c => c.Class, "custom-group")
            .AddChildContent("Content"));
        cut.Find("[data-slot='button-group']").ClassList.Should().Contain("custom-group");
    }

    [Fact]
    public void ButtonGroup_AdditionalAttributes_Passthrough()
    {
        var cut = Render<ButtonGroupCn>(p => p
            .Add(c => c.AdditionalAttributes, new Dictionary<string, object?> { { "id", "my-group" } })
            .AddChildContent("Content"));
        cut.Find("[data-slot='button-group']").GetAttribute("id").Should().Be("my-group");
    }

    // --- ButtonGroupTextCn ---

    [Fact]
    public void ButtonGroupText_Renders_Div()
    {
        var cut = Render<ButtonGroupTextCn>(p => p.AddChildContent("Label"));
        var el = cut.Find("div");
        el.Should().NotBeNull();
        el.TextContent.Should().Contain("Label");
    }

    [Fact]
    public void ButtonGroupText_Has_Default_Classes()
    {
        var cut = Render<ButtonGroupTextCn>(p => p.AddChildContent("Label"));
        var el = cut.Find("div");
        el.ClassList.Should().Contain("cn-button-group-text");
        el.ClassList.Should().Contain("flex");
        el.ClassList.Should().Contain("items-center");
    }

    [Fact]
    public void ButtonGroupText_Class_Passthrough()
    {
        var cut = Render<ButtonGroupTextCn>(p => p
            .Add(c => c.Class, "custom-text")
            .AddChildContent("Label"));
        cut.Find("div").ClassList.Should().Contain("custom-text");
    }

    // --- ButtonGroupSeparatorCn ---

    [Fact]
    public void ButtonGroupSeparator_Renders_With_DataSlot()
    {
        var cut = Render<ButtonGroupSeparatorCn>();
        cut.Find("[data-slot='button-group-separator']").Should().NotBeNull();
    }

    [Fact]
    public void ButtonGroupSeparator_Has_Role_Separator()
    {
        var cut = Render<ButtonGroupSeparatorCn>();
        cut.Find("[data-slot='button-group-separator']").GetAttribute("role").Should().Be("separator");
    }

    [Fact]
    public void ButtonGroupSeparator_Default_Orientation_Is_Vertical()
    {
        var cut = Render<ButtonGroupSeparatorCn>();
        cut.Find("[data-slot='button-group-separator']").GetAttribute("data-orientation").Should().Be("vertical");
    }

    [Fact]
    public void ButtonGroupSeparator_Horizontal_Orientation()
    {
        var cut = Render<ButtonGroupSeparatorCn>(p => p
            .Add(c => c.Orientation, Orientation.Horizontal));
        cut.Find("[data-slot='button-group-separator']").GetAttribute("data-orientation").Should().Be("horizontal");
    }

    [Fact]
    public void ButtonGroupSeparator_Has_Default_Classes()
    {
        var cut = Render<ButtonGroupSeparatorCn>();
        var el = cut.Find("[data-slot='button-group-separator']");
        el.ClassList.Should().Contain("cn-button-group-separator");
        el.ClassList.Should().Contain("shrink-0");
    }

    [Fact]
    public void ButtonGroupSeparator_Class_Passthrough()
    {
        var cut = Render<ButtonGroupSeparatorCn>(p => p
            .Add(c => c.Class, "custom-sep"));
        cut.Find("[data-slot='button-group-separator']").ClassList.Should().Contain("custom-sep");
    }
}

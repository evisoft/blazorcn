using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace BlazorCN.Tests.Components;

public class InputGroupCnTests : BunitContext
{
    // --- InputGroupCn ---

    [Fact]
    public void InputGroup_Renders_With_DataSlot()
    {
        var cut = Render<InputGroupCn>(p => p.AddChildContent("Content"));
        cut.Find("[data-slot='input-group']").Should().NotBeNull();
    }

    [Fact]
    public void InputGroup_Has_Role_Group()
    {
        var cut = Render<InputGroupCn>(p => p.AddChildContent("Content"));
        cut.Find("[data-slot='input-group']").GetAttribute("role").Should().Be("group");
    }

    [Fact]
    public void InputGroup_Renders_ChildContent()
    {
        var cut = Render<InputGroupCn>(p => p.AddChildContent("<span>Hello</span>"));
        cut.Find("[data-slot='input-group']").TextContent.Should().Contain("Hello");
    }

    [Fact]
    public void InputGroup_Class_Passthrough()
    {
        var cut = Render<InputGroupCn>(p => p
            .Add(c => c.Class, "custom-class")
            .AddChildContent("Content"));
        cut.Find("[data-slot='input-group']").ClassList.Should().Contain("custom-class");
    }

    [Fact]
    public void InputGroup_Has_Default_Classes()
    {
        var cut = Render<InputGroupCn>(p => p.AddChildContent("Content"));
        var el = cut.Find("[data-slot='input-group']");
        el.ClassList.Should().Contain("cn-input-group");
        el.ClassList.Should().Contain("relative");
        el.ClassList.Should().Contain("flex");
        el.ClassList.Should().Contain("items-center");
    }

    // --- InputGroupAddonCn ---

    [Fact]
    public void InputGroupAddon_Renders_With_DataSlot()
    {
        var cut = Render<InputGroupAddonCn>(p => p.AddChildContent("Addon"));
        cut.Find("[data-slot='input-group-addon']").Should().NotBeNull();
    }

    [Fact]
    public void InputGroupAddon_InlineStart_Alignment()
    {
        var cut = Render<InputGroupAddonCn>(p => p
            .Add(c => c.Align, InputGroupAddonAlign.InlineStart)
            .AddChildContent("Addon"));
        var el = cut.Find("[data-slot='input-group-addon']");
        el.GetAttribute("data-align").Should().Be("inline-start");
        el.ClassList.Should().Contain("cn-input-group-addon-align-inline-start");
        el.ClassList.Should().Contain("order-first");
    }

    [Fact]
    public void InputGroupAddon_InlineEnd_Alignment()
    {
        var cut = Render<InputGroupAddonCn>(p => p
            .Add(c => c.Align, InputGroupAddonAlign.InlineEnd)
            .AddChildContent("Addon"));
        var el = cut.Find("[data-slot='input-group-addon']");
        el.GetAttribute("data-align").Should().Be("inline-end");
        el.ClassList.Should().Contain("cn-input-group-addon-align-inline-end");
        el.ClassList.Should().Contain("order-last");
    }

    [Fact]
    public void InputGroupAddon_BlockStart_Alignment()
    {
        var cut = Render<InputGroupAddonCn>(p => p
            .Add(c => c.Align, InputGroupAddonAlign.BlockStart)
            .AddChildContent("Addon"));
        var el = cut.Find("[data-slot='input-group-addon']");
        el.GetAttribute("data-align").Should().Be("block-start");
        el.ClassList.Should().Contain("order-first");
        el.ClassList.Should().Contain("w-full");
    }

    [Fact]
    public void InputGroupAddon_BlockEnd_Alignment()
    {
        var cut = Render<InputGroupAddonCn>(p => p
            .Add(c => c.Align, InputGroupAddonAlign.BlockEnd)
            .AddChildContent("Addon"));
        var el = cut.Find("[data-slot='input-group-addon']");
        el.GetAttribute("data-align").Should().Be("block-end");
        el.ClassList.Should().Contain("order-last");
        el.ClassList.Should().Contain("w-full");
    }

    // --- InputGroupButtonCn ---

    [Fact]
    public void InputGroupButton_Renders_Button_With_DataSlot()
    {
        var cut = Render<InputGroupButtonCn>(p => p.AddChildContent("Click"));
        cut.Find("[data-slot='button']").Should().NotBeNull();
        cut.Find("[data-slot='button']").TagName.Should().Be("BUTTON");
    }

    [Fact]
    public void InputGroupButton_Size_Xs()
    {
        var cut = Render<InputGroupButtonCn>(p => p
            .Add(c => c.Size, InputGroupButtonSize.Xs)
            .AddChildContent("Click"));
        cut.Find("[data-slot='button']").GetAttribute("data-size").Should().Be("xs");
    }

    [Fact]
    public void InputGroupButton_Size_Sm()
    {
        var cut = Render<InputGroupButtonCn>(p => p
            .Add(c => c.Size, InputGroupButtonSize.Sm)
            .AddChildContent("Click"));
        cut.Find("[data-slot='button']").GetAttribute("data-size").Should().Be("sm");
    }

    [Fact]
    public void InputGroupButton_Size_IconXs()
    {
        var cut = Render<InputGroupButtonCn>(p => p
            .Add(c => c.Size, InputGroupButtonSize.IconXs)
            .AddChildContent("Click"));
        cut.Find("[data-slot='button']").GetAttribute("data-size").Should().Be("icon-xs");
    }

    // --- InputGroupTextCn ---

    [Fact]
    public void InputGroupText_Renders_Span()
    {
        var cut = Render<InputGroupTextCn>(p => p.AddChildContent("Text"));
        var el = cut.Find("span");
        el.Should().NotBeNull();
        el.TextContent.Should().Contain("Text");
    }

    [Fact]
    public void InputGroupText_Has_Default_Classes()
    {
        var cut = Render<InputGroupTextCn>(p => p.AddChildContent("Text"));
        var el = cut.Find("span");
        el.ClassList.Should().Contain("cn-input-group-text");
        el.ClassList.Should().Contain("flex");
        el.ClassList.Should().Contain("items-center");
    }

    // --- InputGroupInputCn ---

    [Fact]
    public void InputGroupInput_Renders_Input_With_DataSlot()
    {
        var cut = Render<InputGroupInputCn>();
        var input = cut.Find("[data-slot='input-group-control']");
        input.Should().NotBeNull();
        input.TagName.Should().Be("INPUT");
    }

    [Fact]
    public void InputGroupInput_Placeholder_Works()
    {
        var cut = Render<InputGroupInputCn>(p => p
            .Add(c => c.Placeholder, "Enter text..."));
        cut.Find("[data-slot='input-group-control']")
            .GetAttribute("placeholder").Should().Be("Enter text...");
    }

    [Fact]
    public void InputGroupInput_Disabled_State()
    {
        var cut = Render<InputGroupInputCn>(p => p
            .Add(c => c.Disabled, true));
        cut.Find("[data-slot='input-group-control']")
            .HasAttribute("disabled").Should().BeTrue();
    }

    // --- InputGroupTextareaCn ---

    [Fact]
    public void InputGroupTextarea_Renders_With_DataSlot()
    {
        var cut = Render<InputGroupTextareaCn>();
        var textarea = cut.Find("[data-slot='input-group-control']");
        textarea.Should().NotBeNull();
        textarea.TagName.Should().Be("TEXTAREA");
    }

    // --- Integration ---

    [Fact]
    public void Integration_InputGroup_With_Addon_And_Input_Renders_Full_Structure()
    {
        var cut = Render<InputGroupCn>(p => p
            .AddChildContent(builder =>
            {
                builder.OpenComponent<InputGroupAddonCn>(0);
                builder.AddAttribute(1, "Align", InputGroupAddonAlign.InlineStart);
                builder.AddAttribute(2, "ChildContent", (RenderFragment)(b => b.AddContent(0, "$")));
                builder.CloseComponent();
                builder.OpenComponent<InputGroupInputCn>(3);
                builder.AddAttribute(4, "Placeholder", "Amount");
                builder.CloseComponent();
            }));

        // Group wrapper
        cut.Find("[data-slot='input-group']").Should().NotBeNull();
        cut.Find("[data-slot='input-group']").GetAttribute("role").Should().Be("group");

        // Addon
        cut.Find("[data-slot='input-group-addon']").Should().NotBeNull();
        cut.Find("[data-slot='input-group-addon']").TextContent.Should().Contain("$");

        // Input
        var input = cut.Find("[data-slot='input-group-control']");
        input.TagName.Should().Be("INPUT");
        input.GetAttribute("placeholder").Should().Be("Amount");
    }
}

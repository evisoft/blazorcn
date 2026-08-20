using Bunit;
using FluentAssertions;
using Xunit;

namespace BlazorCN.Tests.Components;

public class FormCnTests : BunitContext
{
    [Fact]
    public void FormFieldCn_Renders_With_Gap_Class()
    {
        var cut = Render<FormFieldCn>(p => p.AddChildContent("Field content"));
        var div = cut.Find("[data-slot='form-item']");
        div.ClassList.Should().Contain("gap-2");
    }

    [Fact]
    public void FormLabelCn_Renders_Label_With_For_Attribute()
    {
        var cut = Render<FormLabelCn>(p => p
            .Add(c => c.For, "my-input")
            .AddChildContent("Label text"));
        var label = cut.Find("label");
        label.GetAttribute("for").Should().Be("my-input");
        label.GetAttribute("data-slot").Should().Be("form-label");
    }

    [Fact]
    public void FormDescriptionCn_Renders_With_Muted_Text()
    {
        var cut = Render<FormDescriptionCn>(p => p.AddChildContent("Description text"));
        var p = cut.Find("p");
        p.ClassList.Should().Contain("text-muted-foreground");
        p.GetAttribute("data-slot").Should().Be("form-description");
    }

    [Fact]
    public void FormMessageCn_Renders_With_Destructive_Text_And_Role_Alert()
    {
        var cut = Render<FormMessageCn>(p => p.AddChildContent("Error message"));
        var p = cut.Find("p");
        p.ClassList.Should().Contain("text-destructive");
        p.GetAttribute("role").Should().Be("alert");
        p.GetAttribute("data-slot").Should().Be("form-message");
    }

    [Fact]
    public void FormControlCn_Has_Correct_DataSlot()
    {
        // ChildContent is RenderFragment<FormFieldContext> (the cascaded field context
        // is handed to the child so consumers can wire id/aria onto the control).
        var cut = Render<FormControlCn>(p => p
            .Add(c => c.ChildContent, (FormFieldContext _) => "Control content"));
        var div = cut.Find("[data-slot='form-control']");
        div.Should().NotBeNull();
    }

    [Fact]
    public void FormFieldCn_Has_Correct_DataSlot()
    {
        var cut = Render<FormFieldCn>(p => p.AddChildContent("Content"));
        var div = cut.Find("[data-slot='form-item']");
        div.Should().NotBeNull();
    }

    [Fact]
    public void FormLabelCn_Has_Font_Medium_Class()
    {
        var cut = Render<FormLabelCn>(p => p.AddChildContent("Label"));
        var label = cut.Find("label");
        label.ClassList.Should().Contain("font-medium");
    }

    [Fact]
    public void FormMessageCn_Has_Text_Sm_Class()
    {
        var cut = Render<FormMessageCn>(p => p.AddChildContent("Error"));
        var p = cut.Find("p");
        p.ClassList.Should().Contain("text-sm");
    }
}

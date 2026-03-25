using Bunit;
using FluentAssertions;
using Xunit;

namespace BlazorCN.Tests.Components;

public class FieldCnTests : BunitContext
{
    [Fact]
    public void FieldCn_Renders_With_Correct_DataSlot()
    {
        var cut = Render<FieldCn>(p => p.AddChildContent("Field content"));
        var div = cut.Find("div");
        div.GetAttribute("data-slot").Should().Be("field");
        div.GetAttribute("role").Should().Be("group");
    }

    [Fact]
    public void FieldCn_Vertical_Orientation_Is_Default()
    {
        var cut = Render<FieldCn>(p => p.AddChildContent("Field"));
        var div = cut.Find("div");
        div.GetAttribute("data-orientation").Should().Be("vertical");
        div.ClassList.Should().Contain("flex-col");
    }

    [Fact]
    public void FieldCn_Horizontal_Orientation()
    {
        var cut = Render<FieldCn>(p => p
            .Add(c => c.Orientation, FieldOrientation.Horizontal)
            .AddChildContent("Field"));
        var div = cut.Find("div");
        div.GetAttribute("data-orientation").Should().Be("horizontal");
        div.ClassList.Should().Contain("flex-row");
    }

    [Fact]
    public void FieldCn_Responsive_Orientation()
    {
        var cut = Render<FieldCn>(p => p
            .Add(c => c.Orientation, FieldOrientation.Responsive)
            .AddChildContent("Field"));
        var div = cut.Find("div");
        div.GetAttribute("data-orientation").Should().Be("responsive");
    }

    [Fact]
    public void FieldLabelCn_Renders_With_Correct_DataSlot()
    {
        var cut = Render<FieldLabelCn>(p => p.AddChildContent("Label text"));
        var el = cut.Find("[data-slot='field-label']");
        el.Should().NotBeNull();
    }

    [Fact]
    public void FieldTitleCn_Renders_With_Correct_DataSlot()
    {
        var cut = Render<FieldTitleCn>(p => p.AddChildContent("Title"));
        var div = cut.Find("div");
        div.GetAttribute("data-slot").Should().Be("field-label");
        div.ClassList.Should().Contain("cn-field-title");
    }

    [Fact]
    public void FieldDescriptionCn_Renders_With_Correct_DataSlot()
    {
        var cut = Render<FieldDescriptionCn>(p => p.AddChildContent("Description"));
        var el = cut.Find("p");
        el.GetAttribute("data-slot").Should().Be("field-description");
        el.ClassList.Should().Contain("cn-field-description");
    }

    [Fact]
    public void FieldErrorCn_Renders_When_Has_ChildContent()
    {
        var cut = Render<FieldErrorCn>(p => p.AddChildContent("Error message"));
        var div = cut.Find("div");
        div.GetAttribute("data-slot").Should().Be("field-error");
        div.GetAttribute("role").Should().Be("alert");
        div.ClassList.Should().Contain("cn-field-error");
    }

    [Fact]
    public void FieldErrorCn_Does_Not_Render_When_No_Content()
    {
        var cut = Render<FieldErrorCn>();
        cut.Markup.Trim().Should().BeEmpty();
    }

    [Fact]
    public void FieldErrorCn_Renders_Single_Error_From_List()
    {
        var cut = Render<FieldErrorCn>(p => p
            .Add(c => c.Errors, new List<string> { "Required field" }));
        var div = cut.Find("div");
        div.GetAttribute("data-slot").Should().Be("field-error");
        div.TextContent.Should().Contain("Required field");
    }

    [Fact]
    public void FieldErrorCn_Renders_Multiple_Errors_As_List()
    {
        var cut = Render<FieldErrorCn>(p => p
            .Add(c => c.Errors, new List<string> { "Error one", "Error two" }));
        var items = cut.FindAll("li");
        items.Should().HaveCount(2);
    }

    [Fact]
    public void FieldContentCn_Renders_With_Correct_DataSlot()
    {
        var cut = Render<FieldContentCn>(p => p.AddChildContent("Content"));
        var div = cut.Find("div");
        div.GetAttribute("data-slot").Should().Be("field-content");
        div.ClassList.Should().Contain("flex-col");
    }

    [Fact]
    public void FieldGroupCn_Renders_With_Correct_DataSlot()
    {
        var cut = Render<FieldGroupCn>(p => p.AddChildContent("Group"));
        var div = cut.Find("div");
        div.GetAttribute("data-slot").Should().Be("field-group");
    }

    [Fact]
    public void FieldSetCn_Renders_Fieldset_Element()
    {
        var cut = Render<FieldSetCn>(p => p.AddChildContent("Fieldset content"));
        var el = cut.Find("fieldset");
        el.GetAttribute("data-slot").Should().Be("field-set");
        el.ClassList.Should().Contain("flex-col");
    }

    [Fact]
    public void FieldLegendCn_Renders_Legend_Element()
    {
        var cut = Render<FieldLegendCn>(p => p.AddChildContent("Legend text"));
        var el = cut.Find("legend");
        el.GetAttribute("data-slot").Should().Be("field-legend");
        el.GetAttribute("data-variant").Should().Be("legend");
        el.ClassList.Should().Contain("cn-field-legend");
    }

    [Fact]
    public void FieldLegendCn_Label_Variant()
    {
        var cut = Render<FieldLegendCn>(p => p
            .Add(c => c.Variant, FieldLegendVariant.Label)
            .AddChildContent("Label legend"));
        var el = cut.Find("legend");
        el.GetAttribute("data-variant").Should().Be("label");
    }

    [Fact]
    public void FieldSeparatorCn_Renders_With_Correct_DataSlot()
    {
        var cut = Render<FieldSeparatorCn>();
        var div = cut.Find("[data-slot='field-separator']");
        div.Should().NotBeNull();
    }

    [Fact]
    public void FieldCn_Custom_Class_Passed_Through()
    {
        var cut = Render<FieldCn>(p => p
            .Add(c => c.Class, "my-field-class")
            .AddChildContent("Content"));
        var div = cut.Find("div");
        div.ClassList.Should().Contain("my-field-class");
    }
}

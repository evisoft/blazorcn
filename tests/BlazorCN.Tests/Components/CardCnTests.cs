using Bunit;
using FluentAssertions;
using Xunit;

namespace BlazorCN.Tests.Components;

public class CardCnTests : BunitContext
{
    [Fact]
    public void CardCn_Renders_With_Correct_DataSlot_And_Classes()
    {
        var cut = Render<CardCn>(p => p.AddChildContent("Card content"));
        var div = cut.Find("div");
        div.GetAttribute("data-slot").Should().Be("card");
        div.ClassList.Should().Contain("cn-card");
        div.ClassList.Should().Contain("flex");
        div.ClassList.Should().Contain("flex-col");
    }

    [Fact]
    public void CardHeaderCn_Renders_With_Correct_DataSlot()
    {
        var cut = Render<CardHeaderCn>(p => p.AddChildContent("Header"));
        var div = cut.Find("div");
        div.GetAttribute("data-slot").Should().Be("card-header");
        div.ClassList.Should().Contain("cn-card-header");
        div.ClassList.Should().Contain("grid");
    }

    [Fact]
    public void CardTitleCn_Renders_With_Correct_DataSlot()
    {
        var cut = Render<CardTitleCn>(p => p.AddChildContent("Title"));
        var div = cut.Find("div");
        div.GetAttribute("data-slot").Should().Be("card-title");
        div.ClassList.Should().Contain("cn-card-title");
    }

    [Fact]
    public void CardDescriptionCn_Renders_With_Correct_DataSlot()
    {
        var cut = Render<CardDescriptionCn>(p => p.AddChildContent("Description"));
        var div = cut.Find("div");
        div.GetAttribute("data-slot").Should().Be("card-description");
        div.ClassList.Should().Contain("cn-card-description");
    }

    [Fact]
    public void CardActionCn_Renders_With_Correct_DataSlot()
    {
        var cut = Render<CardActionCn>(p => p.AddChildContent("Action"));
        var div = cut.Find("div");
        div.GetAttribute("data-slot").Should().Be("card-action");
        div.ClassList.Should().Contain("col-start-2");
    }

    [Fact]
    public void CardContentCn_Renders_With_Correct_DataSlot()
    {
        var cut = Render<CardContentCn>(p => p.AddChildContent("Content"));
        var div = cut.Find("div");
        div.GetAttribute("data-slot").Should().Be("card-content");
        div.ClassList.Should().Contain("cn-card-content");
    }

    [Fact]
    public void CardFooterCn_Renders_With_Correct_DataSlot()
    {
        var cut = Render<CardFooterCn>(p => p.AddChildContent("Footer"));
        var div = cut.Find("div");
        div.GetAttribute("data-slot").Should().Be("card-footer");
        div.ClassList.Should().Contain("cn-card-footer");
        div.ClassList.Should().Contain("items-center");
    }

    [Fact]
    public void CardCn_Custom_Class_Passed_Through()
    {
        var cut = Render<CardCn>(p => p
            .Add(c => c.Class, "my-card-class")
            .AddChildContent("Content"));
        var div = cut.Find("div");
        div.ClassList.Should().Contain("my-card-class");
    }

    [Fact]
    public void CardCn_Additional_Attributes_Passed_Through()
    {
        var cut = Render<CardCn>(p => p
            .AddUnmatched("data-testid", "card-1")
            .AddChildContent("Content"));
        var div = cut.Find("div");
        div.GetAttribute("data-testid").Should().Be("card-1");
    }
}

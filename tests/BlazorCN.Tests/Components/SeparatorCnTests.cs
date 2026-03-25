using Bunit;
using FluentAssertions;
using Xunit;

namespace BlazorCN.Tests.Components;

public class SeparatorCnTests : BunitContext
{
    [Fact]
    public void Default_Separator_Is_Horizontal()
    {
        var cut = Render<SeparatorCn>();
        var div = cut.Find("div");
        div.GetAttribute("data-slot").Should().Be("separator");
        div.GetAttribute("role").Should().Be("separator");
        div.GetAttribute("data-orientation").Should().Be("horizontal");
        div.ClassList.Should().Contain("cn-separator");
        div.ClassList.Should().Contain("cn-separator-horizontal");
    }

    [Fact]
    public void Vertical_Separator_Has_Correct_Classes()
    {
        var cut = Render<SeparatorCn>(p => p.Add(c => c.Orientation, Orientation.Vertical));
        var div = cut.Find("div");
        div.GetAttribute("data-orientation").Should().Be("vertical");
        div.ClassList.Should().Contain("cn-separator-vertical");
    }

    [Fact]
    public void Custom_Class_Is_Passed_Through()
    {
        var cut = Render<SeparatorCn>(p => p.Add(c => c.Class, "my-sep"));
        var div = cut.Find("div");
        div.ClassList.Should().Contain("my-sep");
    }

    [Fact]
    public void Additional_Attributes_Passed_Through()
    {
        var cut = Render<SeparatorCn>(p => p.AddUnmatched("data-testid", "sep-1"));
        var div = cut.Find("div");
        div.GetAttribute("data-testid").Should().Be("sep-1");
    }
}

using Bunit;
using FluentAssertions;
using Xunit;

namespace BlazorCN.Tests.Components;

public class SkeletonCnTests : BunitContext
{
    [Fact]
    public void Default_Skeleton_Renders_With_DataSlot_And_BaseClasses()
    {
        var cut = Render<SkeletonCn>();
        var div = cut.Find("div");
        div.GetAttribute("data-slot").Should().Be("skeleton");
        div.ClassList.Should().Contain("animate-pulse");
        div.ClassList.Should().Contain("rounded-md");
        div.ClassList.Should().Contain("bg-accent");
    }

    [Fact]
    public void Custom_Class_Is_Passed_Through()
    {
        var cut = Render<SkeletonCn>(p => p.Add(c => c.Class, "h-12 w-48"));
        var div = cut.Find("div");
        div.ClassList.Should().Contain("h-12");
        div.ClassList.Should().Contain("w-48");
    }

    [Fact]
    public void Additional_Attributes_Passed_Through()
    {
        var cut = Render<SkeletonCn>(p => p.AddUnmatched("data-testid", "skel-1"));
        var div = cut.Find("div");
        div.GetAttribute("data-testid").Should().Be("skel-1");
    }
}

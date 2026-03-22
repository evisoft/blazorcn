using FluentAssertions;
using Xunit;

namespace BlazorCN.Tests.Utilities;

public class CnTests
{
    [Fact]
    public void Merge_Combines_Multiple_Classes()
    {
        var result = Cn.Merge("foo", "bar");
        result.Should().Be("foo bar");
    }

    [Fact]
    public void Merge_Skips_Null_And_Empty()
    {
        var result = Cn.Merge("foo", null, "", "bar");
        result.Should().Be("foo bar");
    }

    [Fact]
    public void Merge_Last_Tailwind_Utility_Wins()
    {
        var result = Cn.Merge("p-2", "p-4");
        result.Should().Be("p-4");
    }

    [Fact]
    public void Merge_Different_Utilities_Kept()
    {
        var result = Cn.Merge("p-2 m-4", "text-sm");
        result.Should().Be("p-2 m-4 text-sm");
    }

    [Fact]
    public void Merge_Handles_Conflicting_Colors()
    {
        var result = Cn.Merge("bg-red-500", "bg-blue-500");
        result.Should().Be("bg-blue-500");
    }

    [Fact]
    public void Merge_Handles_Conflicting_Text_Sizes()
    {
        var result = Cn.Merge("text-sm", "text-lg");
        result.Should().Be("text-lg");
    }

    [Fact]
    public void Merge_Handles_Conflicting_Width()
    {
        var result = Cn.Merge("w-full", "w-1/2");
        result.Should().Be("w-1/2");
    }

    [Fact]
    public void Merge_Handles_Conflicting_Height()
    {
        var result = Cn.Merge("h-9 px-4 py-2", "h-10 px-6");
        result.Should().Be("py-2 h-10 px-6");
    }

    [Fact]
    public void Merge_Preserves_Arbitrary_Values()
    {
        var result = Cn.Merge("text-[14px]", "text-[16px]");
        result.Should().Be("text-[16px]");
    }

    [Fact]
    public void Merge_Handles_Responsive_Prefixes()
    {
        var result = Cn.Merge("md:text-sm", "md:text-lg");
        result.Should().Be("md:text-lg");
    }

    [Fact]
    public void Merge_Returns_Empty_For_No_Input()
    {
        var result = Cn.Merge();
        result.Should().BeEmpty();
    }

    [Fact]
    public void Merge_Handles_Conditional_Class_With_Null()
    {
        string? conditionalClass = null;
        var result = Cn.Merge("base-class", conditionalClass);
        result.Should().Be("base-class");
    }

    [Fact]
    public void Merge_Flex_Replaced_By_Hidden()
    {
        var result = Cn.Merge("flex", "hidden");
        result.Should().Be("hidden");
    }

    [Fact]
    public void Merge_Deduplicates_Display_Utilities()
    {
        var result = Cn.Merge("flex", "flex");
        result.Should().Be("flex");
    }

    [Fact]
    public void Merge_Border_Width_Conflict()
    {
        var result = Cn.Merge("border", "border-2");
        result.Should().Be("border-2");
    }

    [Fact]
    public void Merge_Rounded_Conflict()
    {
        var result = Cn.Merge("rounded", "rounded-lg");
        result.Should().Be("rounded-lg");
    }

    [Fact]
    public void Merge_Deduplicates_Unknown_Classes()
    {
        var result = Cn.Merge("custom-class", "custom-class");
        result.Should().Be("custom-class");
    }
}

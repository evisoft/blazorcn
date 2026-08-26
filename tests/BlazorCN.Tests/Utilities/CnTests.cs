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

    // Upstream tailwind-merge keeps important and non-important forms as SEPARATE groups:
    // !p-2 beats a later p-4 in the browser, so deleting the important class flips rendering.
    // (Verified against tailwind-merge 3.6.0: twMerge('!p-2 p-4') === '!p-2 p-4'.)
    [Fact]
    public void Merge_Important_Does_Not_Conflict_With_Regular()
    {
        Cn.Merge("!bg-red-500", "bg-blue-500").Should().Be("!bg-red-500 bg-blue-500");
        Cn.Merge("bg-red-500", "!bg-blue-500").Should().Be("bg-red-500 !bg-blue-500");
        Cn.Merge("!p-2", "p-4").Should().Be("!p-2 p-4");
        Cn.Merge("p-2!", "p-4").Should().Be("p-2! p-4");   // v4 suffix form
    }

    [Fact]
    public void Merge_Important_Conflicts_With_Important()
    {
        Cn.Merge("!p-3", "!p-4").Should().Be("!p-4");
    }

    // --- conflictingClassGroups: a later shorthand evicts earlier longhands, never the reverse ---

    [Theory]
    [InlineData("px-4 p-2", "p-2")]
    [InlineData("p-2 px-4", "p-2 px-4")]
    [InlineData("mx-2 m-4", "m-4")]
    [InlineData("top-1 inset-0", "inset-0")]
    [InlineData("w-5 size-4", "size-4")]
    [InlineData("size-4 w-5", "size-4 w-5")]
    [InlineData("border-t-2 border-2", "border-2")]
    [InlineData("border-2 border-t-4", "border-2 border-t-4")]
    [InlineData("gap-x-2 gap-4", "gap-4")]
    [InlineData("overflow-x-auto overflow-hidden", "overflow-hidden")]
    public void Merge_Shorthand_Evicts_Earlier_Longhand(string input, string expected)
    {
        Cn.Merge(input).Should().Be(expected);
    }

    // gap-x / gap-y / inset-x / inset-y are orthogonal axes — they must never evict each other.
    [Theory]
    [InlineData("gap-x-2 gap-y-4", "gap-x-2 gap-y-4")]
    [InlineData("inset-x-2 inset-y-4", "inset-x-2 inset-y-4")]
    [InlineData("gap-4 gap-x-2", "gap-4 gap-x-2")]
    public void Merge_Axis_Utilities_Do_Not_Conflict(string input, string expected)
    {
        Cn.Merge(input).Should().Be(expected);
    }

    // Per-corner radius refines an earlier base radius; only a later BASE radius resets corners.
    [Fact]
    public void Merge_Corner_Radius_Layers_On_Base_Radius()
    {
        Cn.Merge("rounded-lg", "rounded-tl-sm").Should().Be("rounded-lg rounded-tl-sm");
        Cn.Merge("rounded-tl-sm", "rounded-lg").Should().Be("rounded-lg");
        Cn.Merge("rounded-md", "rounded-t-none").Should().Be("rounded-md rounded-t-none");
    }

    // Negative and positive forms of one utility are the same group (upstream: -mt-4 evicts mt-2).
    [Theory]
    [InlineData("mt-2 -mt-4", "-mt-4")]
    [InlineData("-mt-4 mt-2", "mt-2")]
    [InlineData("-inset-1 inset-0", "inset-0")]
    public void Merge_Negative_Conflicts_With_Positive(string input, string expected)
    {
        Cn.Merge(input).Should().Be(expected);
    }

    // font-size sets line-height in Tailwind, so a later text-* clears an earlier leading-*.
    [Fact]
    public void Merge_FontSize_Evicts_Leading()
    {
        Cn.Merge("leading-6", "text-lg").Should().Be("text-lg");
        Cn.Merge("text-lg", "leading-7").Should().Be("text-lg leading-7");
        Cn.Merge("text-lg", "text-lg/7").Should().Be("text-lg/7");
        Cn.Merge("text-lg/7", "text-lg").Should().Be("text-lg");
        Cn.Merge("leading-6", "text-lg/7").Should().Be("text-lg/7");
    }

    // Arbitrary values join their semantic group instead of only deduping by raw text.
    [Fact]
    public void Merge_Arbitrary_Values_Join_Their_Semantic_Group()
    {
        Cn.Merge("bg-red-500", "bg-[#ff0000]").Should().Be("bg-[#ff0000]");
        Cn.Merge("text-lg", "text-[12px]").Should().Be("text-[12px]");
        Cn.Merge("text-[#fff]", "text-[12px]").Should().Be("text-[#fff] text-[12px]");
        Cn.Merge("p-2", "p-[3px]").Should().Be("p-[3px]");
        Cn.Merge("[margin:2px]", "[margin:3px]").Should().Be("[margin:3px]");
    }

    [Fact]
    public void Merge_Ring_And_Transition_Groups()
    {
        Cn.Merge("ring-2", "ring-4").Should().Be("ring-4");
        Cn.Merge("border-t", "border-t-2").Should().Be("border-t-2");
        Cn.Merge("transition", "transition-colors").Should().Be("transition-colors");
        Cn.Merge("border-dashed", "border-2").Should().Be("border-dashed border-2");
    }

    // tailwind-merge sorts variant modifiers, so hover:focus: and focus:hover: conflict.
    [Fact]
    public void Merge_Variant_Order_Is_Insensitive()
    {
        Cn.Merge("hover:focus:p-2", "focus:hover:p-4").Should().Be("focus:hover:p-4");
        Cn.Merge("hover:p-2", "focus:p-4").Should().Be("hover:p-2 focus:p-4");
    }

    [Fact]
    public void Merge_Color_With_Opacity_Modifier_Shares_The_Color_Group()
    {
        Cn.Merge("bg-red-500/50", "bg-blue-500").Should().Be("bg-blue-500");
    }
}

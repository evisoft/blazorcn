using System.Linq;
using Bunit;
using FluentAssertions;
using Xunit;

namespace BlazorCN.Tests.Components;

public class SliderCnTests : BunitContext
{
    [Fact]
    public void Renders_With_Default_Value()
    {
        var cut = Render<SliderCn>();
        var slider = cut.Find("[data-slot='slider']");
        slider.Should().NotBeNull();
    }

    [Fact]
    public void Has_Track_And_Thumb_Elements()
    {
        var cut = Render<SliderCn>();
        var track = cut.Find("[data-slot='slider-track']");
        track.Should().NotBeNull();
        var thumb = cut.Find("[data-slot='slider-thumb']");
        thumb.Should().NotBeNull();
    }

    [Fact]
    public void Has_DataSlot_Slider()
    {
        var cut = Render<SliderCn>();
        var slider = cut.Find("[data-slot='slider']");
        slider.GetAttribute("data-slot").Should().Be("slider");
    }

    [Fact]
    public void Has_Range_Element()
    {
        var cut = Render<SliderCn>(p => p.Add(c => c.Value, 50));
        var range = cut.Find("[data-slot='slider-range']");
        range.Should().NotBeNull();
    }

    [Fact]
    public void Custom_Class_Is_Passed_Through()
    {
        var cut = Render<SliderCn>(p => p.Add(c => c.Class, "my-slider"));
        var slider = cut.Find("[data-slot='slider']");
        slider.ClassList.Should().Contain("my-slider");
    }

    // --- Orientation ---

    [Fact]
    public void Default_Orientation_Is_Horizontal()
    {
        var cut = Render<SliderCn>();
        var slider = cut.Find("[data-slot='slider']");
        slider.GetAttribute("data-orientation").Should().Be("horizontal");
    }

    [Fact]
    public void Vertical_Orientation_Sets_DataAttribute()
    {
        var cut = Render<SliderCn>(p => p.Add(c => c.Orientation, SliderOrientation.Vertical));
        var slider = cut.Find("[data-slot='slider']");
        slider.GetAttribute("data-orientation").Should().Be("vertical");
    }

    [Fact]
    public void Vertical_Orientation_Adds_Flex_Col_Class()
    {
        var cut = Render<SliderCn>(p => p.Add(c => c.Orientation, SliderOrientation.Vertical));
        var slider = cut.Find("[data-slot='slider']");
        slider.ClassList.Should().Contain("flex-col");
    }

    [Fact]
    public void Track_Has_Orientation_DataAttribute()
    {
        var cut = Render<SliderCn>(p => p.Add(c => c.Orientation, SliderOrientation.Vertical));
        var track = cut.Find("[data-slot='slider-track']");
        track.GetAttribute("data-orientation").Should().Be("vertical");
    }

    // --- Range / Multiple Values ---

    [Fact]
    public void Range_Values_Renders_Two_Thumbs()
    {
        var cut = Render<SliderCn>(p => p.Add(c => c.Values, new[] { 25.0, 75.0 }));
        var thumbs = cut.FindAll("[data-slot='slider-thumb']");
        thumbs.Should().HaveCount(2);
    }

    [Fact]
    public void Multiple_Values_Renders_Three_Thumbs()
    {
        var cut = Render<SliderCn>(p => p.Add(c => c.Values, new[] { 10.0, 50.0, 90.0 }));
        var thumbs = cut.FindAll("[data-slot='slider-thumb']");
        thumbs.Should().HaveCount(3);
    }

    [Fact]
    public void Single_Value_Renders_One_Thumb()
    {
        var cut = Render<SliderCn>(p => p.Add(c => c.Value, 50));
        var thumbs = cut.FindAll("[data-slot='slider-thumb']");
        thumbs.Should().HaveCount(1);
    }

    [Fact]
    public void Range_Values_Renders_Two_Inputs()
    {
        var cut = Render<SliderCn>(p => p.Add(c => c.Values, new[] { 25.0, 75.0 }));
        var inputs = cut.FindAll("input[type='range']");
        inputs.Should().HaveCount(2);
    }

    [Fact]
    public void Disabled_Adds_Opacity_Class()
    {
        var cut = Render<SliderCn>(p => p.Add(c => c.Disabled, true));
        var slider = cut.Find("[data-slot='slider']");
        slider.ClassList.Should().Contain("opacity-50");
    }

    // --- Focus ring (thumb mirrors the invisible native input's focus) ---

    [Fact]
    public void Focusing_Input_Marks_Matching_Thumb_As_Focused()
    {
        var cut = Render<SliderCn>(p => p.Add(c => c.Values, new[] { 25.0, 75.0 }));
        cut.FindAll("input[type='range']")[1].Focus();
        var thumbs = cut.FindAll("[data-slot='slider-thumb']");
        thumbs[0].HasAttribute("data-focused").Should().BeFalse();
        thumbs[1].HasAttribute("data-focused").Should().BeTrue();
    }

    [Fact]
    public void Blurring_Input_Clears_Thumb_Focus()
    {
        var cut = Render<SliderCn>(p => p.Add(c => c.Value, 50));
        cut.Find("input[type='range']").Focus();
        cut.Find("[data-slot='slider-thumb']").HasAttribute("data-focused").Should().BeTrue();
        cut.Find("input[type='range']").Blur();
        cut.Find("[data-slot='slider-thumb']").HasAttribute("data-focused").Should().BeFalse();
    }

    // Blazor preserves the case the consumer typed, and PascalCase (`Id=`) is what a Blazor
    // developer naturally writes. Matching case-sensitively used to leave the id on the root
    // div AND put it on the input: the id appeared twice, and because getElementById finds the
    // div first — and a div is not labelable — `label[for]` resolved to nothing, leaving the
    // slider with no accessible name.
    [Theory]
    [InlineData("id")]
    [InlineData("Id")]
    public void Consumer_Id_Lands_Only_On_The_Focusable_Input(string attributeName)
    {
        var cut = Render<SliderCn>(p => p
            .Add(c => c.Value, 50)
            .AddUnmatched(attributeName, "volume"));

        cut.FindAll("[id='volume']").Count.Should().Be(1);
        cut.Find("[id='volume']").TagName.Should().Be("INPUT");
        cut.Find("[data-slot='slider']").HasAttribute("id").Should().BeFalse();
    }

    // Radix keeps the values array sorted on every update (getNextSortedValues) so thumbs
    // push each other instead of crossing. An unsorted array inverts the per-input clip-path
    // hit zones: each thumb lands inside the OTHER input's zone, so grabbing a thumb drags
    // the wrong value — browser-reproduced by arrow-keying thumb 0 past thumb 1.
    [Fact]
    public void Multi_Thumb_Pushed_Past_Its_Neighbor_Keeps_Values_Sorted()
    {
        double[]? reported = null;
        var cut = Render<SliderCn>(p => p
            .Add(c => c.Values, new[] { 25.0, 75.0 })
            .Add(c => c.ValuesChanged, v => reported = v));

        cut.FindAll("input[type='range']")[0].Input("90");

        reported.Should().Equal(75.0, 90.0);
        var inputs = cut.FindAll("input[type='range']");
        inputs[0].GetAttribute("value").Should().Be("75");
        inputs[1].GetAttribute("value").Should().Be("90");
    }

    // Horizontal multi-thumb hit zones are emitted as LOGICAL custom props
    // (--clip-is/--clip-ie) that blazorcn.css maps to physical inset() per
    // direction — a physical clip under dir="rtl" hit-tested every thumb to the
    // OTHER input, so dragging a thumb moved the wrong value (browser-reproduced).
    [Fact]
    public void Horizontal_Multi_Thumb_Inputs_Use_Logical_Clip_Props()
    {
        var cut = Render<SliderCn>(p => p.Add(c => c.Values, new[] { 25.0, 75.0 }));
        var styles = cut.FindAll("input[type='range']").Select(i => i.GetAttribute("style")).ToList();
        styles.Should().OnlyContain(s => s.Contains("--clip-is:") && s.Contains("--clip-ie:"));
        styles.Should().OnlyContain(s => !s.Contains("clip-path"));
    }

    [Fact]
    public void Vertical_Multi_Thumb_Inputs_Keep_Inline_ClipPath()
    {
        var cut = Render<SliderCn>(p => p
            .Add(c => c.Orientation, SliderOrientation.Vertical)
            .Add(c => c.Values, new[] { 25.0, 75.0 }));
        var styles = cut.FindAll("input[type='range']").Select(i => i.GetAttribute("style")).ToList();
        styles.Should().OnlyContain(s => s.Contains("clip-path: inset("));
    }

    // Radix renders value={[]} as a slider with no thumbs; RangeStyle used to index
    // sorted[0] unguarded and threw IndexOutOfRangeException on the empty array.
    [Fact]
    public void Empty_Values_Array_Renders_Without_Crashing()
    {
        var cut = Render<SliderCn>(p => p.Add(c => c.Values, Array.Empty<double>()));

        cut.FindAll("[data-slot='slider-thumb']").Should().BeEmpty();
        cut.FindAll("input[type='range']").Should().BeEmpty();
        cut.Find("[data-slot='slider-range']").GetAttribute("style").Should().Contain("width: 0%");
    }

    [Theory]
    [InlineData("aria-label")]
    [InlineData("Aria-Label")]
    public void Consumer_AriaLabel_Lands_On_The_Input_Not_The_Root(string attributeName)
    {
        var cut = Render<SliderCn>(p => p
            .Add(c => c.Value, 50)
            .AddUnmatched(attributeName, "Volume"));

        cut.Find("input[type='range']").GetAttribute("aria-label").Should().Be("Volume");
        cut.Find("[data-slot='slider']").HasAttribute("aria-label").Should().BeFalse();
    }
}

using Bunit;
using FluentAssertions;
using Xunit;

namespace BlazorCN.Tests.Components;

public class ScrollAreaCnTests : BunitContext
{
    // --- ScrollAreaCn ---

    [Fact]
    public void ScrollArea_Renders_With_DataSlot()
    {
        var cut = Render<ScrollAreaCn>(p => p.AddChildContent("Content"));
        cut.Find("[data-slot='scroll-area']").Should().NotBeNull();
    }

    [Fact]
    public void ScrollArea_Has_Default_Classes()
    {
        var cut = Render<ScrollAreaCn>(p => p.AddChildContent("Content"));
        var el = cut.Find("[data-slot='scroll-area']");
        el.ClassList.Should().Contain("relative");
        el.ClassList.Should().Contain("overflow-hidden");
    }

    [Fact]
    public void ScrollArea_Viewport_Has_Default_Classes()
    {
        var cut = Render<ScrollAreaCn>(p => p.AddChildContent("Content"));
        var viewport = cut.Find("[data-slot='scroll-area'] > div:first-child");
        viewport.ClassList.Should().Contain("h-full");
        viewport.ClassList.Should().Contain("w-full");
    }

    [Fact]
    public void ScrollArea_Vertical_Has_OverflowY_Auto()
    {
        var cut = Render<ScrollAreaCn>(p => p
            .Add(c => c.Orientation, ScrollOrientation.Vertical)
            .AddChildContent("Content"));
        var viewport = cut.Find("[data-slot='scroll-area'] > div:first-child");
        viewport.ClassList.Should().Contain("overflow-y-auto");
        viewport.ClassList.Should().Contain("overflow-x-hidden");
    }

    [Fact]
    public void ScrollArea_Horizontal_Has_OverflowX_Auto()
    {
        var cut = Render<ScrollAreaCn>(p => p
            .Add(c => c.Orientation, ScrollOrientation.Horizontal)
            .AddChildContent("Content"));
        var viewport = cut.Find("[data-slot='scroll-area'] > div:first-child");
        viewport.ClassList.Should().Contain("overflow-x-auto");
        viewport.ClassList.Should().Contain("overflow-y-hidden");
    }

    [Fact]
    public void ScrollArea_Both_Has_Overflow_Auto()
    {
        var cut = Render<ScrollAreaCn>(p => p
            .Add(c => c.Orientation, ScrollOrientation.Both)
            .AddChildContent("Content"));
        var viewport = cut.Find("[data-slot='scroll-area'] > div:first-child");
        viewport.ClassList.Should().Contain("overflow-auto");
    }

    [Fact]
    public void ScrollArea_Vertical_Shows_Vertical_Scrollbar()
    {
        var cut = Render<ScrollAreaCn>(p => p
            .Add(c => c.Orientation, ScrollOrientation.Vertical)
            .AddChildContent("Content"));
        cut.FindAll("[data-slot='scroll-area-scrollbar'][data-orientation='vertical']").Should().HaveCount(1);
        cut.FindAll("[data-slot='scroll-area-scrollbar'][data-orientation='horizontal']").Should().BeEmpty();
    }

    [Fact]
    public void ScrollArea_Horizontal_Shows_Horizontal_Scrollbar()
    {
        var cut = Render<ScrollAreaCn>(p => p
            .Add(c => c.Orientation, ScrollOrientation.Horizontal)
            .AddChildContent("Content"));
        cut.FindAll("[data-slot='scroll-area-scrollbar'][data-orientation='horizontal']").Should().HaveCount(1);
        cut.FindAll("[data-slot='scroll-area-scrollbar'][data-orientation='vertical']").Should().BeEmpty();
    }

    [Fact]
    public void ScrollArea_Both_Shows_Both_Scrollbars()
    {
        var cut = Render<ScrollAreaCn>(p => p
            .Add(c => c.Orientation, ScrollOrientation.Both)
            .AddChildContent("Content"));
        cut.FindAll("[data-slot='scroll-area-scrollbar'][data-orientation='vertical']").Should().HaveCount(1);
        cut.FindAll("[data-slot='scroll-area-scrollbar'][data-orientation='horizontal']").Should().HaveCount(1);
    }

    [Fact]
    public void ScrollArea_Class_Passthrough()
    {
        var cut = Render<ScrollAreaCn>(p => p
            .Add(c => c.Class, "custom-scroll")
            .AddChildContent("Content"));
        cut.Find("[data-slot='scroll-area']").ClassList.Should().Contain("custom-scroll");
    }

    [Fact]
    public void ScrollArea_AdditionalAttributes_Passthrough()
    {
        var cut = Render<ScrollAreaCn>(p => p
            .Add(c => c.AdditionalAttributes, new Dictionary<string, object?> { { "id", "my-scroll" } })
            .AddChildContent("Content"));
        cut.Find("[data-slot='scroll-area']").GetAttribute("id").Should().Be("my-scroll");
    }

    // --- ScrollBarCn ---

    [Fact]
    public void ScrollBar_Renders_With_DataSlot()
    {
        var cut = Render<ScrollBarCn>();
        cut.Find("[data-slot='scroll-area-scrollbar']").Should().NotBeNull();
    }

    [Fact]
    public void ScrollBar_Has_Default_Classes()
    {
        var cut = Render<ScrollBarCn>();
        var bar = cut.Find("[data-slot='scroll-area-scrollbar']");
        bar.ClassList.Should().Contain("flex");
        bar.ClassList.Should().Contain("touch-none");
        bar.ClassList.Should().Contain("select-none");
        bar.ClassList.Should().Contain("transition-colors");
    }

    [Fact]
    public void ScrollBar_Vertical_Has_Correct_Classes()
    {
        var cut = Render<ScrollBarCn>(p => p
            .Add(c => c.Orientation, ScrollOrientation.Vertical));
        var bar = cut.Find("[data-slot='scroll-area-scrollbar']");
        bar.ClassList.Should().Contain("cn-scroll-area-scrollbar");
    }

    [Fact]
    public void ScrollBar_Horizontal_Has_Correct_Classes()
    {
        var cut = Render<ScrollBarCn>(p => p
            .Add(c => c.Orientation, ScrollOrientation.Horizontal));
        var bar = cut.Find("[data-slot='scroll-area-scrollbar']");
        bar.ClassList.Should().Contain("cn-scroll-area-scrollbar");
    }

    [Fact]
    public void ScrollBar_Has_Thumb()
    {
        var cut = Render<ScrollBarCn>();
        var thumb = cut.Find("[data-slot='scroll-area-scrollbar'] > div");
        thumb.ClassList.Should().Contain("cn-scroll-area-thumb");
    }

    [Fact]
    public void ScrollBar_Has_DataOrientation_Attribute()
    {
        var cut = Render<ScrollBarCn>(p => p
            .Add(c => c.Orientation, ScrollOrientation.Vertical));
        cut.Find("[data-slot='scroll-area-scrollbar']").GetAttribute("data-orientation").Should().Be("vertical");
    }

    [Fact]
    public void ScrollBar_Horizontal_Has_DataOrientation()
    {
        var cut = Render<ScrollBarCn>(p => p
            .Add(c => c.Orientation, ScrollOrientation.Horizontal));
        cut.Find("[data-slot='scroll-area-scrollbar']").GetAttribute("data-orientation").Should().Be("horizontal");
    }

    [Fact]
    public void ScrollBar_Class_Passthrough()
    {
        var cut = Render<ScrollBarCn>(p => p
            .Add(c => c.Class, "custom-bar"));
        cut.Find("[data-slot='scroll-area-scrollbar']").ClassList.Should().Contain("custom-bar");
    }
}

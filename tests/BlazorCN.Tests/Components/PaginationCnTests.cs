using Bunit;
using FluentAssertions;
using Xunit;

namespace BlazorCN.Tests.Components;

public class PaginationCnTests : BunitContext
{
    // --- PaginationLinkCn ---

    [Fact]
    public void PaginationLink_Renders_With_DataSlot()
    {
        var cut = Render<PaginationLinkCn>(p => p
            .Add(c => c.Href, "/page/1")
            .AddChildContent("1"));
        cut.Find("[data-slot='pagination-link']").Should().NotBeNull();
    }

    [Fact]
    public void PaginationLink_Has_Focus_Ring_Classes()
    {
        var cut = Render<PaginationLinkCn>(p => p
            .Add(c => c.Href, "/page/1")
            .AddChildContent("1"));
        var el = cut.Find("[data-slot='pagination-link']");
        el.ClassList.Should().Contain("focus-visible:border-ring");
        el.ClassList.Should().Contain("focus-visible:ring-[3px]");
        el.ClassList.Should().Contain("focus-visible:ring-ring/50");
    }

    [Fact]
    public void PaginationLink_Active_Has_Border()
    {
        var cut = Render<PaginationLinkCn>(p => p
            .Add(c => c.Href, "/page/1")
            .Add(c => c.IsActive, true)
            .AddChildContent("1"));
        var el = cut.Find("[data-slot='pagination-link']");
        el.ClassList.Should().Contain("border");
        el.ClassList.Should().Contain("bg-background");
        el.GetAttribute("aria-current").Should().Be("page");
    }

    [Fact]
    public void PaginationLink_Inactive_No_AriaCurrent()
    {
        var cut = Render<PaginationLinkCn>(p => p
            .Add(c => c.Href, "/page/1")
            .AddChildContent("1"));
        cut.Find("[data-slot='pagination-link']").GetAttribute("aria-current").Should().BeNull();
    }

    [Fact]
    public void PaginationLink_Class_Passthrough()
    {
        var cut = Render<PaginationLinkCn>(p => p
            .Add(c => c.Href, "/page/1")
            .Add(c => c.Class, "custom-link")
            .AddChildContent("1"));
        cut.Find("[data-slot='pagination-link']").ClassList.Should().Contain("custom-link");
    }
}

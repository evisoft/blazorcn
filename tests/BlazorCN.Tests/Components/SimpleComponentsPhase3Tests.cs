using Bunit;
using Microsoft.Extensions.DependencyInjection;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace BlazorCN.Tests.Components;

public class SimpleComponentsPhase3Tests : BunitContext
{
    public SimpleComponentsPhase3Tests()
    {
        // ToggleGroupCn wires arrow-key nav via JsInteropCn on render.
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddScoped<JsInteropCn>();
    }

    // ProgressCn tests

    [Fact]
    public void Progress_Renders_With_DataSlot()
    {
        var cut = Render<ProgressCn>(p => p.Add(c => c.Value, 50));
        cut.Find("[data-slot='progress']").Should().NotBeNull();
    }

    [Fact]
    public void Progress_Has_Progressbar_Role()
    {
        var cut = Render<ProgressCn>(p => p.Add(c => c.Value, 50));
        cut.Find("[role='progressbar']").Should().NotBeNull();
    }

    [Fact]
    public void Progress_Value_Sets_AriaValuenow()
    {
        var cut = Render<ProgressCn>(p => p.Add(c => c.Value, 75));
        var el = cut.Find("[data-slot='progress']");
        el.GetAttribute("aria-valuenow").Should().Be("75");
    }

    [Fact]
    public void Progress_Inner_Div_Width_Reflects_Value()
    {
        // Root now hosts a track > indicator pair (nova composition).
        var cut = Render<ProgressCn>(p => p.Add(c => c.Value, 50));
        var inner = cut.Find("[data-slot='progress-track'] > div");
        inner.GetAttribute("style").Should().Contain("width: 50");
    }

    // AvatarCn tests

    [Fact]
    public void Avatar_Renders_With_DataSlot()
    {
        var cut = Render<AvatarCn>(p => p.AddChildContent("AB"));
        cut.Find("[data-slot='avatar']").Should().NotBeNull();
    }

    [Fact]
    public void AvatarImage_Renders_With_Src()
    {
        var cut = Render<AvatarCn>(p => p
            .AddChildContent<AvatarImageCn>(img => img
                .Add(c => c.Src, "https://example.com/photo.jpg")
                .Add(c => c.Alt, "User")));
        var img = cut.Find("[data-slot='avatar-image']");
        img.GetAttribute("src").Should().Be("https://example.com/photo.jpg");
        img.GetAttribute("alt").Should().Be("User");
    }

    [Fact]
    public void AvatarFallback_Renders_With_DataSlot()
    {
        var cut = Render<AvatarCn>(p => p
            .AddChildContent<AvatarFallbackCn>(fb => fb
                .AddChildContent("AB")));
        cut.Find("[data-slot='avatar-fallback']").TextContent.Should().Contain("AB");
    }

    // TableCn tests

    [Fact]
    public void Table_Renders_With_DataSlot()
    {
        var cut = Render<TableCn>(p => p.AddChildContent("<tr><td>cell</td></tr>"));
        cut.Find("[data-slot='table']").Should().NotBeNull();
    }

    [Fact]
    public void TableHeader_Renders_With_DataSlot()
    {
        var cut = Render<TableCn>(p => p
            .AddChildContent<TableHeaderCn>(h => h
                .AddChildContent("<tr><th>Header</th></tr>")));
        cut.Find("[data-slot='table-header']").Should().NotBeNull();
    }

    [Fact]
    public void TableBody_Renders_With_DataSlot()
    {
        var cut = Render<TableCn>(p => p
            .AddChildContent<TableBodyCn>(b => b
                .AddChildContent("<tr><td>Cell</td></tr>")));
        cut.Find("[data-slot='table-body']").Should().NotBeNull();
    }

    [Fact]
    public void TableRow_Renders_With_DataSlot()
    {
        var cut = Render<TableCn>(p => p
            .AddChildContent<TableBodyCn>(b => b
                .AddChildContent<TableRowCn>(r => r
                    .AddChildContent("<td>Cell</td>"))));
        cut.Find("[data-slot='table-row']").Should().NotBeNull();
    }

    [Fact]
    public void TableCell_Renders_With_DataSlot()
    {
        var cut = Render<TableCn>(p => p
            .AddChildContent<TableBodyCn>(b => b
                .AddChildContent<TableRowCn>(r => r
                    .AddChildContent<TableCellCn>(c => c
                        .AddChildContent("Data")))));
        cut.Find("[data-slot='table-cell']").TextContent.Should().Contain("Data");
    }

    [Fact]
    public void TableCaption_Renders_With_DataSlot()
    {
        var cut = Render<TableCn>(p => p
            .AddChildContent<TableCaptionCn>(c => c
                .AddChildContent("A caption")));
        cut.Find("[data-slot='table-caption']").TextContent.Should().Contain("A caption");
    }

    // BreadcrumbCn tests

    [Fact]
    public void Breadcrumb_Renders_With_DataSlot()
    {
        var cut = Render<BreadcrumbCn>(p => p.AddChildContent("Items"));
        cut.Find("[data-slot='breadcrumb']").Should().NotBeNull();
    }

    [Fact]
    public void Breadcrumb_Has_Navigation_Role()
    {
        var cut = Render<BreadcrumbCn>(p => p.AddChildContent("Items"));
        var nav = cut.Find("nav");
        nav.GetAttribute("aria-label").Should().Be("breadcrumb");
    }

    [Fact]
    public void BreadcrumbList_Renders_With_DataSlot()
    {
        var cut = Render<BreadcrumbCn>(p => p
            .AddChildContent<BreadcrumbListCn>(l => l
                .AddChildContent("Items")));
        cut.Find("[data-slot='breadcrumb-list']").Should().NotBeNull();
    }

    [Fact]
    public void BreadcrumbLink_Renders_Href()
    {
        var cut = Render<BreadcrumbCn>(p => p
            .AddChildContent<BreadcrumbListCn>(l => l
                .AddChildContent<BreadcrumbItemCn>(i => i
                    .AddChildContent<BreadcrumbLinkCn>(link => link
                        .Add(c => c.Href, "/home")
                        .AddChildContent("Home")))));
        var a = cut.Find("[data-slot='breadcrumb-link']");
        a.GetAttribute("href").Should().Be("/home");
    }

    [Fact]
    public void BreadcrumbPage_Has_AriaCurrent()
    {
        var cut = Render<BreadcrumbCn>(p => p
            .AddChildContent<BreadcrumbListCn>(l => l
                .AddChildContent<BreadcrumbItemCn>(i => i
                    .AddChildContent<BreadcrumbPageCn>(pg => pg
                        .AddChildContent("Current")))));
        var page = cut.Find("[data-slot='breadcrumb-page']");
        page.GetAttribute("aria-current").Should().Be("page");
    }

    [Fact]
    public void BreadcrumbSeparator_Shows_Default_Chevron()
    {
        // Reference default separator is a ChevronRight icon (rtl-flipped), not a "/" glyph.
        var cut = Render<BreadcrumbSeparatorCn>();
        cut.Find("[data-slot='breadcrumb-separator'] svg").Should().NotBeNull();
    }

    [Fact]
    public void BreadcrumbEllipsis_Shows_Dots_Icon()
    {
        // Reference renders the horizontal-dots svg (MoreHorizontalIcon), not literal text
        var cut = Render<BreadcrumbEllipsisCn>();
        cut.Find("[data-slot='breadcrumb-ellipsis'] svg").Should().NotBeNull();
        cut.Find("[data-slot='breadcrumb-ellipsis'] .sr-only").TextContent.Should().Contain("More");
    }

    // KbdCn tests

    [Fact]
    public void Kbd_Renders_With_DataSlot()
    {
        var cut = Render<KbdCn>(p => p.AddChildContent("Ctrl+C"));
        cut.Find("[data-slot='kbd']").Should().NotBeNull();
        cut.Find("kbd").TextContent.Should().Contain("Ctrl+C");
    }

    // SpinnerCn tests

    [Fact]
    public void Spinner_Renders_With_DataSlot()
    {
        var cut = Render<SpinnerCn>();
        cut.Find("[data-slot='spinner']").Should().NotBeNull();
    }

    [Fact]
    public void Spinner_Is_Svg_Element()
    {
        var cut = Render<SpinnerCn>();
        cut.Find("svg[data-slot='spinner']").Should().NotBeNull();
    }

    // EmptyCn tests

    [Fact]
    public void Empty_Renders_With_DataSlot()
    {
        var cut = Render<EmptyCn>(p => p.AddChildContent("No data"));
        cut.Find("[data-slot='empty']").Should().NotBeNull();
        cut.Find("[data-slot='empty']").TextContent.Should().Contain("No data");
    }

    // PaginationCn tests

    [Fact]
    public void Pagination_Renders_With_DataSlot()
    {
        var cut = Render<PaginationCn>(p => p.AddChildContent("Items"));
        cut.Find("[data-slot='pagination']").Should().NotBeNull();
    }

    [Fact]
    public void Pagination_Has_Navigation_Role()
    {
        var cut = Render<PaginationCn>(p => p.AddChildContent("Items"));
        var nav = cut.Find("nav");
        nav.GetAttribute("role").Should().Be("navigation");
        nav.GetAttribute("aria-label").Should().Be("pagination");
    }

    [Fact]
    public void PaginationContent_Renders_With_DataSlot()
    {
        var cut = Render<PaginationCn>(p => p
            .AddChildContent<PaginationContentCn>(c => c
                .AddChildContent("Items")));
        cut.Find("[data-slot='pagination-content']").Should().NotBeNull();
    }

    [Fact]
    public void PaginationLink_Renders_Href()
    {
        var cut = Render<PaginationLinkCn>(p => p
            .Add(c => c.Href, "/page/2")
            .AddChildContent("2"));
        var a = cut.Find("[data-slot='pagination-link']");
        a.GetAttribute("href").Should().Be("/page/2");
    }

    [Fact]
    public void PaginationLink_Active_Has_AriaCurrent()
    {
        var cut = Render<PaginationLinkCn>(p => p
            .Add(c => c.IsActive, true)
            .Add(c => c.Href, "/page/1")
            .AddChildContent("1"));
        var a = cut.Find("[data-slot='pagination-link']");
        a.GetAttribute("aria-current").Should().Be("page");
    }

    [Fact]
    public void PaginationPrevious_Has_AriaLabel()
    {
        var cut = Render<PaginationPreviousCn>(p => p.Add(c => c.Href, "/page/1"));
        var a = cut.Find("[data-slot='pagination-previous']");
        a.GetAttribute("aria-label").Should().Be("Go to previous page");
    }

    [Fact]
    public void PaginationNext_Has_AriaLabel()
    {
        var cut = Render<PaginationNextCn>(p => p.Add(c => c.Href, "/page/3"));
        var a = cut.Find("[data-slot='pagination-next']");
        a.GetAttribute("aria-label").Should().Be("Go to next page");
    }

    [Fact]
    public void PaginationEllipsis_Shows_Dots()
    {
        var cut = Render<PaginationEllipsisCn>();
        cut.Find("[data-slot='pagination-ellipsis']").TextContent.Should().Contain("...");
    }

    // ToggleGroupCn tests

    [Fact]
    public void ToggleGroup_Renders_With_DataSlot()
    {
        var cut = Render<ToggleGroupCn>(p => p.AddChildContent("Items"));
        cut.Find("[data-slot='toggle-group']").Should().NotBeNull();
    }

    [Fact]
    public void ToggleGroup_Has_Group_Role()
    {
        var cut = Render<ToggleGroupCn>(p => p.AddChildContent("Items"));
        cut.Find("[role='group']").Should().NotBeNull();
    }

    [Fact]
    public void ToggleGroupItem_Renders_With_DataSlot()
    {
        var cut = Render<ToggleGroupCn>(p => p
            .AddChildContent<ToggleGroupItemCn>(item => item
                .Add(c => c.Value, "bold")
                .AddChildContent("B")));
        cut.Find("[data-slot='toggle-group-item']").Should().NotBeNull();
    }

    // AspectRatioCn tests

    [Fact]
    public void AspectRatio_Renders_With_DataSlot()
    {
        var cut = Render<AspectRatioCn>(p => p
            .Add(c => c.Ratio, 16.0 / 9.0)
            .AddChildContent("<img src='test.jpg' />"));
        cut.Find("[data-slot='aspect-ratio']").Should().NotBeNull();
    }

    [Fact]
    public void AspectRatio_Style_Contains_Ratio()
    {
        var cut = Render<AspectRatioCn>(p => p
            .Add(c => c.Ratio, 2)
            .AddChildContent("<img src='test.jpg' />"));
        var el = cut.Find("[data-slot='aspect-ratio']");
        el.GetAttribute("style").Should().Contain("aspect-ratio: 2");
    }
}

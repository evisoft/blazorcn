using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BlazorCN.Tests.Components;

public class DialogCnTests : BunitContext
{
    private void SetupJsInterop()
    {
        var module = JSInterop.SetupModule("./_content/BlazorCN/blazorcn.js");
        module.SetupVoid("trapFocus", _ => true).SetVoidResult();
        module.SetupVoid("lockScroll", _ => true).SetVoidResult();
        module.SetupVoid("cleanup", _ => true).SetVoidResult();
        Services.AddScoped<JsInteropCn>();
    }

    // --- DialogCn ---

    [Fact]
    public void Dialog_Renders_With_DataSlot()
    {
        var cut = Render<DialogCn>(p => p.AddChildContent("Content"));
        cut.Find("[data-slot='dialog']").Should().NotBeNull();
    }

    [Fact]
    public void Dialog_Starts_Closed_By_Default()
    {
        var cut = Render<DialogCn>(p => p.AddChildContent("Content"));
        cut.Find("[data-slot='dialog']").GetAttribute("data-state").Should().Be("closed");
    }

    [Fact]
    public void Dialog_Starts_Open_When_Open_Is_True()
    {
        var cut = Render<DialogCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent("Content"));
        cut.Find("[data-slot='dialog']").GetAttribute("data-state").Should().Be("open");
    }

    [Fact]
    public void Dialog_Class_Passthrough()
    {
        var cut = Render<DialogCn>(p => p
            .Add(c => c.Class, "custom-class")
            .AddChildContent("Content"));
        cut.Find("[data-slot='dialog']").ClassList.Should().Contain("custom-class");
    }

    [Fact]
    public void Dialog_AdditionalAttributes_Passthrough()
    {
        var cut = Render<DialogCn>(p => p
            .Add(c => c.AdditionalAttributes, new Dictionary<string, object?> { { "id", "my-dialog" } })
            .AddChildContent("Content"));
        cut.Find("[data-slot='dialog']").GetAttribute("id").Should().Be("my-dialog");
    }

    // --- DialogTriggerCn ---

    [Fact]
    public void DialogTrigger_Renders_With_DataSlot()
    {
        var cut = Render<DialogCn>(p => p
            .AddChildContent<DialogTriggerCn>(t => t
                .AddChildContent("Open")));
        cut.Find("[data-slot='dialog-trigger']").Should().NotBeNull();
    }

    [Fact]
    public void DialogTrigger_Has_Button_Type()
    {
        var cut = Render<DialogCn>(p => p
            .AddChildContent<DialogTriggerCn>(t => t
                .AddChildContent("Open")));
        cut.Find("[data-slot='dialog-trigger']").GetAttribute("type").Should().Be("button");
    }

    [Fact]
    public void DialogTrigger_Opens_Dialog()
    {
        var isOpen = false;
        var cut = Render<DialogCn>(p => p
            .Add(c => c.OpenChanged, EventCallback.Factory.Create<bool>(this, v => isOpen = v))
            .AddChildContent<DialogTriggerCn>(t => t
                .AddChildContent("Open")));
        cut.Find("[data-slot='dialog-trigger']").Click();
        isOpen.Should().BeTrue();
    }

    [Fact]
    public void DialogTrigger_Class_Passthrough()
    {
        var cut = Render<DialogCn>(p => p
            .AddChildContent<DialogTriggerCn>(t => t
                .Add(c => c.Class, "trigger-class")
                .AddChildContent("Open")));
        cut.Find("[data-slot='dialog-trigger']").ClassList.Should().Contain("trigger-class");
    }

    // --- DialogOverlayCn ---

    [Fact]
    public void DialogOverlay_Not_Rendered_When_Closed()
    {
        var cut = Render<DialogCn>(p => p
            .AddChildContent<DialogOverlayCn>());
        cut.FindAll("[data-slot='dialog-overlay']").Should().BeEmpty();
    }

    [Fact]
    public void DialogOverlay_Rendered_When_Open()
    {
        var cut = Render<DialogCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<DialogOverlayCn>());
        cut.Find("[data-slot='dialog-overlay']").Should().NotBeNull();
    }

    [Fact]
    public void DialogOverlay_Has_Default_Classes()
    {
        var cut = Render<DialogCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<DialogOverlayCn>());
        var overlay = cut.Find("[data-slot='dialog-overlay']");
        overlay.ClassList.Should().Contain("fixed");
        overlay.ClassList.Should().Contain("inset-0");
        overlay.ClassList.Should().Contain("z-50");
    }

    [Fact]
    public void DialogOverlay_Click_Closes_Dialog()
    {
        var isOpen = true;
        var cut = Render<DialogCn>(p => p
            .Add(c => c.Open, true)
            .Add(c => c.OpenChanged, EventCallback.Factory.Create<bool>(this, v => isOpen = v))
            .AddChildContent<DialogOverlayCn>());
        cut.Find("[data-slot='dialog-overlay']").Click();
        isOpen.Should().BeFalse();
    }

    [Fact]
    public void DialogOverlay_Class_Passthrough()
    {
        var cut = Render<DialogCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<DialogOverlayCn>(o => o
                .Add(c => c.Class, "custom-overlay")));
        cut.Find("[data-slot='dialog-overlay']").ClassList.Should().Contain("custom-overlay");
    }

    // --- DialogContentCn ---

    [Fact]
    public void DialogContent_Not_Rendered_When_Closed()
    {
        SetupJsInterop();
        var cut = Render<DialogCn>(p => p
            .AddChildContent<DialogContentCn>(c => c
                .AddChildContent("Dialog body")));
        cut.FindAll("[data-slot='dialog-content']").Should().BeEmpty();
    }

    [Fact]
    public void DialogContent_Rendered_When_Open()
    {
        SetupJsInterop();
        var cut = Render<DialogCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<DialogContentCn>(c => c
                .AddChildContent("Dialog body")));
        cut.Find("[data-slot='dialog-content']").Should().NotBeNull();
        cut.Find("[data-slot='dialog-content']").TextContent.Should().Contain("Dialog body");
    }

    [Fact]
    public void DialogContent_Has_Default_Classes()
    {
        SetupJsInterop();
        var cut = Render<DialogCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<DialogContentCn>(c => c
                .AddChildContent("Dialog body")));
        var content = cut.Find("[data-slot='dialog-content']");
        content.ClassList.Should().Contain("cn-dialog-content");
        content.ClassList.Should().Contain("fixed");
        content.ClassList.Should().Contain("z-50");
    }

    [Fact]
    public void DialogContent_Has_Close_Button()
    {
        SetupJsInterop();
        var cut = Render<DialogCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<DialogContentCn>(c => c
                .AddChildContent("Dialog body")));
        // The built-in close button has data-slot="dialog-close"
        cut.FindAll("[data-slot='dialog-close']").Should().NotBeEmpty();
    }

    [Fact]
    public void DialogContent_Close_Button_Closes_Dialog()
    {
        SetupJsInterop();
        var isOpen = true;
        var cut = Render<DialogCn>(p => p
            .Add(c => c.Open, true)
            .Add(c => c.OpenChanged, EventCallback.Factory.Create<bool>(this, v => isOpen = v))
            .AddChildContent<DialogContentCn>(c => c
                .AddChildContent("Dialog body")));
        cut.Find("[data-slot='dialog-content'] [data-slot='dialog-close']").Click();
        isOpen.Should().BeFalse();
    }

    [Fact]
    public void DialogContent_Class_Passthrough()
    {
        SetupJsInterop();
        var cut = Render<DialogCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<DialogContentCn>(c => c
                .Add(x => x.Class, "custom-content")
                .AddChildContent("Dialog body")));
        cut.Find("[data-slot='dialog-content']").ClassList.Should().Contain("custom-content");
    }

    // --- DialogHeaderCn ---

    [Fact]
    public void DialogHeader_Renders_With_DataSlot()
    {
        var cut = Render<DialogHeaderCn>(p => p.AddChildContent("Header"));
        cut.Find("[data-slot='dialog-header']").Should().NotBeNull();
    }

    [Fact]
    public void DialogHeader_Has_Default_Classes()
    {
        var cut = Render<DialogHeaderCn>(p => p.AddChildContent("Header"));
        var el = cut.Find("[data-slot='dialog-header']");
        el.ClassList.Should().Contain("cn-dialog-header");
        el.ClassList.Should().Contain("flex");
        el.ClassList.Should().Contain("flex-col");
    }

    [Fact]
    public void DialogHeader_Class_Passthrough()
    {
        var cut = Render<DialogHeaderCn>(p => p
            .Add(c => c.Class, "custom-header")
            .AddChildContent("Header"));
        cut.Find("[data-slot='dialog-header']").ClassList.Should().Contain("custom-header");
    }

    // --- DialogFooterCn ---

    [Fact]
    public void DialogFooter_Renders_With_DataSlot()
    {
        var cut = Render<DialogFooterCn>(p => p.AddChildContent("Footer"));
        cut.Find("[data-slot='dialog-footer']").Should().NotBeNull();
    }

    [Fact]
    public void DialogFooter_Has_Default_Classes()
    {
        // Layout (flex/flex-col-reverse/gap-2) now lives in the cn-dialog-footer CSS class.
        var cut = Render<DialogFooterCn>(p => p.AddChildContent("Footer"));
        var el = cut.Find("[data-slot='dialog-footer']");
        el.ClassList.Should().Contain("cn-dialog-footer");
    }

    [Fact]
    public void DialogFooter_Class_Passthrough()
    {
        var cut = Render<DialogFooterCn>(p => p
            .Add(c => c.Class, "custom-footer")
            .AddChildContent("Footer"));
        cut.Find("[data-slot='dialog-footer']").ClassList.Should().Contain("custom-footer");
    }

    // --- DialogTitleCn ---

    [Fact]
    public void DialogTitle_Renders_With_DataSlot()
    {
        var cut = Render<DialogTitleCn>(p => p.AddChildContent("Title"));
        cut.Find("[data-slot='dialog-title']").Should().NotBeNull();
    }

    [Fact]
    public void DialogTitle_Has_Default_Classes()
    {
        var cut = Render<DialogTitleCn>(p => p.AddChildContent("Title"));
        var el = cut.Find("[data-slot='dialog-title']");
        el.ClassList.Should().Contain("cn-dialog-title");
    }

    [Fact]
    public void DialogTitle_Class_Passthrough()
    {
        var cut = Render<DialogTitleCn>(p => p
            .Add(c => c.Class, "custom-title")
            .AddChildContent("Title"));
        cut.Find("[data-slot='dialog-title']").ClassList.Should().Contain("custom-title");
    }

    // --- DialogDescriptionCn ---

    [Fact]
    public void DialogDescription_Renders_With_DataSlot()
    {
        var cut = Render<DialogDescriptionCn>(p => p.AddChildContent("Description"));
        cut.Find("[data-slot='dialog-description']").Should().NotBeNull();
    }

    [Fact]
    public void DialogDescription_Has_Default_Classes()
    {
        var cut = Render<DialogDescriptionCn>(p => p.AddChildContent("Description"));
        var el = cut.Find("[data-slot='dialog-description']");
        el.ClassList.Should().Contain("cn-dialog-description");
    }

    [Fact]
    public void DialogDescription_Class_Passthrough()
    {
        var cut = Render<DialogDescriptionCn>(p => p
            .Add(c => c.Class, "custom-desc")
            .AddChildContent("Description"));
        cut.Find("[data-slot='dialog-description']").ClassList.Should().Contain("custom-desc");
    }

    // --- DialogCloseCn ---

    [Fact]
    public void DialogClose_Renders_With_DataSlot()
    {
        var cut = Render<DialogCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<DialogCloseCn>(c => c
                .AddChildContent("Close")));
        cut.Find("[data-slot='dialog-close']").Should().NotBeNull();
    }

    [Fact]
    public void DialogClose_Closes_Dialog()
    {
        var isOpen = true;
        var cut = Render<DialogCn>(p => p
            .Add(c => c.Open, true)
            .Add(c => c.OpenChanged, EventCallback.Factory.Create<bool>(this, v => isOpen = v))
            .AddChildContent<DialogCloseCn>(c => c
                .AddChildContent("Close")));
        cut.Find("[data-slot='dialog-close']").Click();
        isOpen.Should().BeFalse();
    }

    [Fact]
    public void DialogClose_Has_Button_Type()
    {
        var cut = Render<DialogCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<DialogCloseCn>(c => c
                .AddChildContent("Close")));
        cut.Find("[data-slot='dialog-close']").GetAttribute("type").Should().Be("button");
    }

    // --- Focus Trap JS Interop ---

    [Fact]
    public void DialogContent_TrapFocus_Called_When_Open()
    {
        var module = JSInterop.SetupModule("./_content/BlazorCN/blazorcn.js");
        var trapFocusHandler = module.SetupVoid("trapFocus", _ => true);
        trapFocusHandler.SetVoidResult();
        module.SetupVoid("lockScroll", _ => true).SetVoidResult();
        module.SetupVoid("cleanup", _ => true).SetVoidResult();
        Services.AddScoped<JsInteropCn>();

        Render<DialogCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<DialogContentCn>(c => c
                .AddChildContent("Dialog body")));

        trapFocusHandler.Invocations.Should().NotBeEmpty();
    }

    [Fact]
    public void DialogContent_Cleanup_Called_When_Closed()
    {
        var module = JSInterop.SetupModule("./_content/BlazorCN/blazorcn.js");
        module.SetupVoid("trapFocus", _ => true).SetVoidResult();
        module.SetupVoid("lockScroll", _ => true).SetVoidResult();
        var cleanupHandler = module.SetupVoid("cleanup", _ => true);
        cleanupHandler.SetVoidResult();
        Services.AddScoped<JsInteropCn>();

        var isOpen = true;
        var cut = Render<DialogCn>(p => p
            .Add(c => c.Open, true)
            .Add(c => c.OpenChanged, EventCallback.Factory.Create<bool>(this, v => isOpen = v))
            .AddChildContent<DialogContentCn>(c => c
                .AddChildContent("Dialog body")));

        var countBefore = cleanupHandler.Invocations.Count;

        // Close the dialog
        cut.Find("[data-slot='dialog-content'] [data-slot='dialog-close']").Click();
        isOpen.Should().BeFalse();

        cleanupHandler.Invocations.Count.Should().BeGreaterThan(countBefore);
    }

    // --- Integration ---

    [Fact]
    public void Dialog_Full_Integration()
    {
        SetupJsInterop();
        var isOpen = false;
        var cut = Render<DialogCn>(p => p
            .Add(c => c.OpenChanged, EventCallback.Factory.Create<bool>(this, v => isOpen = v))
            .AddChildContent<DialogTriggerCn>(t => t
                .AddChildContent("Open Dialog")));

        // Initially closed
        cut.Find("[data-slot='dialog']").GetAttribute("data-state").Should().Be("closed");

        // Click trigger to open
        cut.Find("[data-slot='dialog-trigger']").Click();
        isOpen.Should().BeTrue();
        cut.Find("[data-slot='dialog']").GetAttribute("data-state").Should().Be("open");
    }

    [Fact]
    public void DialogContent_AdditionalAttributes_Passthrough()
    {
        SetupJsInterop();
        var cut = Render<DialogCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<DialogContentCn>(c => c
                .Add(x => x.AdditionalAttributes, new Dictionary<string, object?> { { "role", "dialog" } })
                .AddChildContent("Body")));
        cut.Find("[data-slot='dialog-content']").GetAttribute("role").Should().Be("dialog");
    }

    [Fact]
    public void DialogContent_AriaLabelledby_Matches_Title_Id()
    {
        SetupJsInterop();
        var cut = Render<DialogCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<DialogContentCn>(c => c
                .AddChildContent<DialogTitleCn>(t => t
                    .AddChildContent("My Title"))));
        var content = cut.Find("[data-slot='dialog-content']");
        var title = cut.Find("[data-slot='dialog-title']");
        var labelledBy = content.GetAttribute("aria-labelledby");
        labelledBy.Should().NotBeNullOrEmpty();
        title.GetAttribute("id").Should().Be(labelledBy);
    }

    [Fact]
    public void DialogContent_Without_Title_Has_AriaLabel_Fallback()
    {
        SetupJsInterop();
        var cut = Render<DialogCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<DialogContentCn>(c => c
                .AddChildContent("Body without title")));
        var content = cut.Find("[data-slot='dialog-content']");
        content.GetAttribute("aria-labelledby").Should().BeNull();
        content.GetAttribute("aria-label").Should().Be("Dialog");
    }
}

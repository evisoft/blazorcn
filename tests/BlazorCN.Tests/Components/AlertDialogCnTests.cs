using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BlazorCN.Tests.Components;

public class AlertDialogCnTests : BunitContext
{
    private void SetupJsInterop()
    {
        var module = JSInterop.SetupModule("./_content/BlazorCN/blazorcn.js");
        module.SetupVoid("trapFocus", _ => true).SetVoidResult();
        module.SetupVoid("lockScroll", _ => true).SetVoidResult();
        module.SetupVoid("cleanup", _ => true).SetVoidResult();
        Services.AddScoped<JsInteropCn>();
    }

    // --- AlertDialogCn ---

    [Fact]
    public void AlertDialog_Renders_With_DataSlot()
    {
        var cut = Render<AlertDialogCn>(p => p.AddChildContent("Content"));
        cut.Find("[data-slot='alert-dialog']").Should().NotBeNull();
    }

    [Fact]
    public void AlertDialog_Starts_Closed_By_Default()
    {
        var cut = Render<AlertDialogCn>(p => p.AddChildContent("Content"));
        cut.Find("[data-slot='alert-dialog']").GetAttribute("data-state").Should().Be("closed");
    }

    [Fact]
    public void AlertDialog_Starts_Open_When_Open_Is_True()
    {
        var cut = Render<AlertDialogCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent("Content"));
        cut.Find("[data-slot='alert-dialog']").GetAttribute("data-state").Should().Be("open");
    }

    [Fact]
    public void AlertDialog_Class_Passthrough()
    {
        var cut = Render<AlertDialogCn>(p => p
            .Add(c => c.Class, "custom-class")
            .AddChildContent("Content"));
        cut.Find("[data-slot='alert-dialog']").ClassList.Should().Contain("custom-class");
    }

    [Fact]
    public void AlertDialog_AdditionalAttributes_Passthrough()
    {
        var cut = Render<AlertDialogCn>(p => p
            .Add(c => c.AdditionalAttributes, new Dictionary<string, object?> { { "id", "my-alert" } })
            .AddChildContent("Content"));
        cut.Find("[data-slot='alert-dialog']").GetAttribute("id").Should().Be("my-alert");
    }

    // --- AlertDialogTriggerCn ---

    [Fact]
    public void AlertDialogTrigger_Renders_With_DataSlot()
    {
        var cut = Render<AlertDialogCn>(p => p
            .AddChildContent<AlertDialogTriggerCn>(t => t
                .AddChildContent("Open")));
        cut.Find("[data-slot='alert-dialog-trigger']").Should().NotBeNull();
    }

    [Fact]
    public void AlertDialogTrigger_Opens_AlertDialog()
    {
        var isOpen = false;
        var cut = Render<AlertDialogCn>(p => p
            .Add(c => c.OpenChanged, EventCallback.Factory.Create<bool>(this, v => isOpen = v))
            .AddChildContent<AlertDialogTriggerCn>(t => t
                .AddChildContent("Open")));
        cut.Find("[data-slot='alert-dialog-trigger']").Click();
        isOpen.Should().BeTrue();
    }

    [Fact]
    public void AlertDialogTrigger_Has_Button_Type()
    {
        var cut = Render<AlertDialogCn>(p => p
            .AddChildContent<AlertDialogTriggerCn>(t => t
                .AddChildContent("Open")));
        cut.Find("[data-slot='alert-dialog-trigger']").GetAttribute("type").Should().Be("button");
    }

    // --- AlertDialogOverlayCn ---

    [Fact]
    public void AlertDialogOverlay_Not_Rendered_When_Closed()
    {
        var cut = Render<AlertDialogCn>(p => p
            .AddChildContent<AlertDialogOverlayCn>());
        cut.FindAll("[data-slot='alert-dialog-overlay']").Should().BeEmpty();
    }

    [Fact]
    public void AlertDialogOverlay_Rendered_When_Open()
    {
        var cut = Render<AlertDialogCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<AlertDialogOverlayCn>());
        cut.Find("[data-slot='alert-dialog-overlay']").Should().NotBeNull();
    }

    [Fact]
    public void AlertDialogOverlay_Has_Default_Classes()
    {
        var cut = Render<AlertDialogCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<AlertDialogOverlayCn>());
        var el = cut.Find("[data-slot='alert-dialog-overlay']");
        el.ClassList.Should().Contain("fixed");
        el.ClassList.Should().Contain("inset-0");
        el.ClassList.Should().Contain("z-50");
    }

    [Fact]
    public void AlertDialogOverlay_Does_NOT_Have_Click_Handler()
    {
        var cut = Render<AlertDialogCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<AlertDialogOverlayCn>());
        var overlay = cut.Find("[data-slot='alert-dialog-overlay']");
        // AlertDialog overlay should NOT have a click handler - user must take explicit action
        // Verify there is no onclick attribute (bUnit throws if we try to click an element without a handler)
        var action = () => overlay.Click();
        action.Should().Throw<Bunit.MissingEventHandlerException>();
    }

    [Fact]
    public void AlertDialogOverlay_Class_Passthrough()
    {
        var cut = Render<AlertDialogCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<AlertDialogOverlayCn>(o => o
                .Add(c => c.Class, "custom-overlay")));
        cut.Find("[data-slot='alert-dialog-overlay']").ClassList.Should().Contain("custom-overlay");
    }

    // --- AlertDialogContentCn ---

    [Fact]
    public void AlertDialogContent_Not_Rendered_When_Closed()
    {
        SetupJsInterop();
        var cut = Render<AlertDialogCn>(p => p
            .AddChildContent<AlertDialogContentCn>(c => c
                .AddChildContent("Body")));
        cut.FindAll("[data-slot='alert-dialog-content']").Should().BeEmpty();
    }

    [Fact]
    public void AlertDialogContent_Rendered_When_Open()
    {
        SetupJsInterop();
        var cut = Render<AlertDialogCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<AlertDialogContentCn>(c => c
                .AddChildContent("Body")));
        cut.Find("[data-slot='alert-dialog-content']").Should().NotBeNull();
    }

    [Fact]
    public void AlertDialogContent_Has_Default_Classes()
    {
        SetupJsInterop();
        var cut = Render<AlertDialogCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<AlertDialogContentCn>(c => c
                .AddChildContent("Body")));
        var el = cut.Find("[data-slot='alert-dialog-content']");
        el.ClassList.Should().Contain("fixed");
        el.ClassList.Should().Contain("z-50");
        el.ClassList.Should().Contain("max-w-lg");
        el.ClassList.Should().Contain("border");
        el.ClassList.Should().Contain("bg-background");
        el.ClassList.Should().Contain("shadow-lg");
    }

    [Fact]
    public void AlertDialogContent_Has_No_Close_Button()
    {
        SetupJsInterop();
        var cut = Render<AlertDialogCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<AlertDialogContentCn>(c => c
                .AddChildContent("Body")));
        // AlertDialog content should NOT have a built-in close button (unlike Dialog)
        cut.FindAll("[data-slot='alert-dialog-content'] button").Should().BeEmpty();
    }

    [Fact]
    public void AlertDialogContent_Class_Passthrough()
    {
        SetupJsInterop();
        var cut = Render<AlertDialogCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<AlertDialogContentCn>(c => c
                .Add(x => x.Class, "custom-content")
                .AddChildContent("Body")));
        cut.Find("[data-slot='alert-dialog-content']").ClassList.Should().Contain("custom-content");
    }

    [Fact]
    public void AlertDialogContent_AdditionalAttributes_Passthrough()
    {
        SetupJsInterop();
        var cut = Render<AlertDialogCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<AlertDialogContentCn>(c => c
                .Add(x => x.AdditionalAttributes, new Dictionary<string, object?> { { "role", "alertdialog" } })
                .AddChildContent("Body")));
        cut.Find("[data-slot='alert-dialog-content']").GetAttribute("role").Should().Be("alertdialog");
    }

    // --- AlertDialogHeaderCn ---

    [Fact]
    public void AlertDialogHeader_Renders_With_DataSlot()
    {
        var cut = Render<AlertDialogHeaderCn>(p => p.AddChildContent("Header"));
        cut.Find("[data-slot='alert-dialog-header']").Should().NotBeNull();
    }

    [Fact]
    public void AlertDialogHeader_Has_Default_Classes()
    {
        var cut = Render<AlertDialogHeaderCn>(p => p.AddChildContent("Header"));
        var el = cut.Find("[data-slot='alert-dialog-header']");
        el.ClassList.Should().Contain("flex");
        el.ClassList.Should().Contain("flex-col");
        el.ClassList.Should().Contain("gap-2");
    }

    [Fact]
    public void AlertDialogHeader_Class_Passthrough()
    {
        var cut = Render<AlertDialogHeaderCn>(p => p
            .Add(c => c.Class, "custom-header")
            .AddChildContent("Header"));
        cut.Find("[data-slot='alert-dialog-header']").ClassList.Should().Contain("custom-header");
    }

    // --- AlertDialogFooterCn ---

    [Fact]
    public void AlertDialogFooter_Renders_With_DataSlot()
    {
        var cut = Render<AlertDialogFooterCn>(p => p.AddChildContent("Footer"));
        cut.Find("[data-slot='alert-dialog-footer']").Should().NotBeNull();
    }

    [Fact]
    public void AlertDialogFooter_Has_Default_Classes()
    {
        var cut = Render<AlertDialogFooterCn>(p => p.AddChildContent("Footer"));
        var el = cut.Find("[data-slot='alert-dialog-footer']");
        el.ClassList.Should().Contain("flex");
        el.ClassList.Should().Contain("flex-col-reverse");
        el.ClassList.Should().Contain("gap-2");
    }

    [Fact]
    public void AlertDialogFooter_Class_Passthrough()
    {
        var cut = Render<AlertDialogFooterCn>(p => p
            .Add(c => c.Class, "custom-footer")
            .AddChildContent("Footer"));
        cut.Find("[data-slot='alert-dialog-footer']").ClassList.Should().Contain("custom-footer");
    }

    // --- AlertDialogTitleCn ---

    [Fact]
    public void AlertDialogTitle_Renders_With_DataSlot()
    {
        var cut = Render<AlertDialogTitleCn>(p => p.AddChildContent("Title"));
        cut.Find("[data-slot='alert-dialog-title']").Should().NotBeNull();
    }

    [Fact]
    public void AlertDialogTitle_Has_Default_Classes()
    {
        var cut = Render<AlertDialogTitleCn>(p => p.AddChildContent("Title"));
        var el = cut.Find("[data-slot='alert-dialog-title']");
        el.ClassList.Should().Contain("text-lg");
        el.ClassList.Should().Contain("font-semibold");
    }

    [Fact]
    public void AlertDialogTitle_Class_Passthrough()
    {
        var cut = Render<AlertDialogTitleCn>(p => p
            .Add(c => c.Class, "custom-title")
            .AddChildContent("Title"));
        cut.Find("[data-slot='alert-dialog-title']").ClassList.Should().Contain("custom-title");
    }

    // --- AlertDialogDescriptionCn ---

    [Fact]
    public void AlertDialogDescription_Renders_With_DataSlot()
    {
        var cut = Render<AlertDialogDescriptionCn>(p => p.AddChildContent("Description"));
        cut.Find("[data-slot='alert-dialog-description']").Should().NotBeNull();
    }

    [Fact]
    public void AlertDialogDescription_Has_Default_Classes()
    {
        var cut = Render<AlertDialogDescriptionCn>(p => p.AddChildContent("Description"));
        var el = cut.Find("[data-slot='alert-dialog-description']");
        el.ClassList.Should().Contain("text-sm");
        el.ClassList.Should().Contain("text-muted-foreground");
    }

    [Fact]
    public void AlertDialogDescription_Class_Passthrough()
    {
        var cut = Render<AlertDialogDescriptionCn>(p => p
            .Add(c => c.Class, "custom-desc")
            .AddChildContent("Description"));
        cut.Find("[data-slot='alert-dialog-description']").ClassList.Should().Contain("custom-desc");
    }

    // --- AlertDialogActionCn ---

    [Fact]
    public void AlertDialogAction_Renders_With_DataSlot()
    {
        var cut = Render<AlertDialogCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<AlertDialogActionCn>(a => a
                .AddChildContent("Confirm")));
        cut.Find("[data-slot='alert-dialog-action']").Should().NotBeNull();
    }

    [Fact]
    public void AlertDialogAction_Closes_Dialog_And_Fires_OnClick()
    {
        var isOpen = true;
        var actionClicked = false;
        var cut = Render<AlertDialogCn>(p => p
            .Add(c => c.Open, true)
            .Add(c => c.OpenChanged, EventCallback.Factory.Create<bool>(this, v => isOpen = v))
            .AddChildContent<AlertDialogActionCn>(a => a
                .Add(x => x.OnClick, EventCallback.Factory.Create(this, () => actionClicked = true))
                .AddChildContent("Confirm")));
        cut.Find("[data-slot='alert-dialog-action']").Click();
        actionClicked.Should().BeTrue();
        isOpen.Should().BeFalse();
    }

    [Fact]
    public void AlertDialogAction_Has_Button_Type()
    {
        var cut = Render<AlertDialogCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<AlertDialogActionCn>(a => a
                .AddChildContent("Confirm")));
        cut.Find("[data-slot='alert-dialog-action']").GetAttribute("type").Should().Be("button");
    }

    // --- AlertDialogCancelCn ---

    [Fact]
    public void AlertDialogCancel_Renders_With_DataSlot()
    {
        var cut = Render<AlertDialogCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<AlertDialogCancelCn>(c => c
                .AddChildContent("Cancel")));
        cut.Find("[data-slot='alert-dialog-cancel']").Should().NotBeNull();
    }

    [Fact]
    public void AlertDialogCancel_Closes_Dialog()
    {
        var isOpen = true;
        var cut = Render<AlertDialogCn>(p => p
            .Add(c => c.Open, true)
            .Add(c => c.OpenChanged, EventCallback.Factory.Create<bool>(this, v => isOpen = v))
            .AddChildContent<AlertDialogCancelCn>(c => c
                .AddChildContent("Cancel")));
        cut.Find("[data-slot='alert-dialog-cancel']").Click();
        isOpen.Should().BeFalse();
    }

    [Fact]
    public void AlertDialogCancel_Has_Button_Type()
    {
        var cut = Render<AlertDialogCn>(p => p
            .Add(c => c.Open, true)
            .AddChildContent<AlertDialogCancelCn>(c => c
                .AddChildContent("Cancel")));
        cut.Find("[data-slot='alert-dialog-cancel']").GetAttribute("type").Should().Be("button");
    }

    // --- Integration ---

    [Fact]
    public void AlertDialog_Full_Integration()
    {
        SetupJsInterop();
        var isOpen = false;
        var cut = Render<AlertDialogCn>(p => p
            .Add(c => c.OpenChanged, EventCallback.Factory.Create<bool>(this, v => isOpen = v))
            .AddChildContent<AlertDialogTriggerCn>(t => t
                .AddChildContent("Delete")));

        cut.Find("[data-slot='alert-dialog']").GetAttribute("data-state").Should().Be("closed");
        cut.Find("[data-slot='alert-dialog-trigger']").Click();
        isOpen.Should().BeTrue();
        cut.Find("[data-slot='alert-dialog']").GetAttribute("data-state").Should().Be("open");
    }
}

using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BlazorCN.Tests.Components;

public class ToastCnTests : BunitContext
{
    private ToastService RegisterToastService()
    {
        var service = new ToastService();
        Services.AddSingleton(service);
        return service;
    }

    // --- ToastService ---

    [Fact]
    public void ToastService_Show_Raises_OnShow_Event()
    {
        var service = new ToastService();
        ToastMessage? received = null;
        service.OnShow += msg => received = msg;

        service.Show("Hello");

        received.Should().NotBeNull();
        received!.Message.Should().Be("Hello");
        received.Variant.Should().Be(ToastVariant.Default);
    }

    [Fact]
    public void ToastService_Success_Sets_Variant()
    {
        var service = new ToastService();
        ToastMessage? received = null;
        service.OnShow += msg => received = msg;

        service.Success("Done!");

        received.Should().NotBeNull();
        received!.Variant.Should().Be(ToastVariant.Success);
    }

    [Fact]
    public void ToastService_Error_Sets_Variant()
    {
        var service = new ToastService();
        ToastMessage? received = null;
        service.OnShow += msg => received = msg;

        service.Error("Failed!");

        received.Should().NotBeNull();
        received!.Variant.Should().Be(ToastVariant.Error);
    }

    [Fact]
    public void ToastService_Warning_Sets_Variant()
    {
        var service = new ToastService();
        ToastMessage? received = null;
        service.OnShow += msg => received = msg;

        service.Warning("Careful!");

        received.Should().NotBeNull();
        received!.Variant.Should().Be(ToastVariant.Warning);
    }

    [Fact]
    public void ToastService_Info_Sets_Variant()
    {
        var service = new ToastService();
        ToastMessage? received = null;
        service.OnShow += msg => received = msg;

        service.Info("FYI");

        received.Should().NotBeNull();
        received!.Variant.Should().Be(ToastVariant.Info);
    }

    [Fact]
    public void ToastService_Show_Generates_Unique_Ids()
    {
        var service = new ToastService();
        var ids = new List<string>();
        service.OnShow += msg => ids.Add(msg.Id);

        service.Show("One");
        service.Show("Two");

        ids.Should().HaveCount(2);
        ids[0].Should().NotBe(ids[1]);
    }

    [Fact]
    public void ToastService_Show_With_Title()
    {
        var service = new ToastService();
        ToastMessage? received = null;
        service.OnShow += msg => received = msg;

        service.Show("Body", title: "My Title");

        received!.Title.Should().Be("My Title");
    }

    [Fact]
    public void ToastService_Show_With_Custom_Duration()
    {
        var service = new ToastService();
        ToastMessage? received = null;
        service.OnShow += msg => received = msg;

        service.Show("Hello", durationMs: 10000);

        received!.DurationMs.Should().Be(10000);
    }

    [Fact]
    public void ToastService_Default_Duration_Is_5000()
    {
        var service = new ToastService();
        ToastMessage? received = null;
        service.OnShow += msg => received = msg;

        service.Show("Hello");

        received!.DurationMs.Should().Be(5000);
    }

    // --- ToasterCn ---

    [Fact]
    public void Toaster_Renders_With_DataSlot()
    {
        RegisterToastService();
        var cut = Render<ToasterCn>();
        cut.Find("[data-slot='toaster']").Should().NotBeNull();
    }

    [Fact]
    public void Toaster_Has_Default_Classes()
    {
        RegisterToastService();
        var cut = Render<ToasterCn>();
        var el = cut.Find("[data-slot='toaster']");
        el.ClassList.Should().Contain("fixed");
        el.ClassList.Should().Contain("top-0");
        el.ClassList.Should().Contain("right-0");
        el.ClassList.Should().Contain("flex");
    }

    [Fact]
    public void Toaster_Is_Empty_Initially()
    {
        RegisterToastService();
        var cut = Render<ToasterCn>();
        cut.FindAll("[data-slot='toast']").Should().BeEmpty();
    }

    [Fact]
    public void Toaster_Shows_Toast_When_Service_Fires()
    {
        var service = RegisterToastService();
        var cut = Render<ToasterCn>();

        service.Show("Test message");

        cut.FindAll("[data-slot='toast']").Should().HaveCount(1);
    }

    [Fact]
    public void Toaster_Shows_Multiple_Toasts()
    {
        var service = RegisterToastService();
        var cut = Render<ToasterCn>();

        service.Show("Message 1");
        service.Show("Message 2");

        cut.FindAll("[data-slot='toast']").Should().HaveCount(2);
    }

    [Fact]
    public void Toaster_Class_Passthrough()
    {
        RegisterToastService();
        var cut = Render<ToasterCn>(p => p
            .Add(c => c.Class, "custom-toaster"));
        cut.Find("[data-slot='toaster']").ClassList.Should().Contain("custom-toaster");
    }

    [Fact]
    public void Toaster_AdditionalAttributes_Passthrough()
    {
        RegisterToastService();
        var cut = Render<ToasterCn>(p => p
            .Add(c => c.AdditionalAttributes, new Dictionary<string, object?> { { "id", "my-toaster" } }));
        cut.Find("[data-slot='toaster']").GetAttribute("id").Should().Be("my-toaster");
    }

    // --- ToastCn ---

    [Fact]
    public void Toast_Renders_Message_Text()
    {
        var service = RegisterToastService();
        var cut = Render<ToasterCn>();

        service.Show("Hello world");

        cut.Find("[data-slot='toast-description']").TextContent.Should().Contain("Hello world");
    }

    [Fact]
    public void Toast_Has_Default_Classes()
    {
        var service = RegisterToastService();
        var cut = Render<ToasterCn>();

        service.Show("Hello");

        var toast = cut.Find("[data-slot='toast']");
        toast.ClassList.Should().Contain("cn-toast");
        toast.ClassList.Should().Contain("group");
        toast.ClassList.Should().Contain("pointer-events-auto");
        toast.ClassList.Should().Contain("relative");
        toast.ClassList.Should().Contain("flex");
        toast.ClassList.Should().Contain("w-full");
        toast.ClassList.Should().Contain("border");
        toast.ClassList.Should().Contain("p-4");
        toast.ClassList.Should().Contain("shadow-lg");
    }

    [Fact]
    public void Toast_Default_Variant_Has_Correct_Classes()
    {
        var service = RegisterToastService();
        var cut = Render<ToasterCn>();

        service.Show("Default toast");

        var toast = cut.Find("[data-slot='toast']");
        toast.ClassList.Should().Contain("cn-toast");
        toast.GetAttribute("data-variant").Should().Be("default");
    }

    [Fact]
    public void Toast_Success_Variant_Has_Correct_Classes()
    {
        var service = RegisterToastService();
        var cut = Render<ToasterCn>();

        service.Success("Success toast");

        var toast = cut.Find("[data-slot='toast']");
        toast.GetAttribute("data-variant").Should().Be("success");
        toast.ClassList.Should().Contain("cn-toast");
        cut.FindAll("[data-slot='toast'] svg").Count.Should().Be(1); // variant icon only — no built-in close X (reference Toaster has no closeButton)
    }

    [Fact]
    public void Toast_Error_Variant_Has_Correct_Classes()
    {
        var service = RegisterToastService();
        var cut = Render<ToasterCn>();

        service.Error("Error toast");

        var toast = cut.Find("[data-slot='toast']");
        toast.GetAttribute("data-variant").Should().Be("error");
        toast.ClassList.Should().Contain("cn-toast");
        cut.FindAll("[data-slot='toast'] svg").Count.Should().Be(1); // variant icon only — no built-in close X (reference Toaster has no closeButton)
    }

    [Fact]
    public void Toast_Warning_Variant_Has_Correct_Classes()
    {
        var service = RegisterToastService();
        var cut = Render<ToasterCn>();

        service.Warning("Warning toast");

        var toast = cut.Find("[data-slot='toast']");
        toast.GetAttribute("data-variant").Should().Be("warning");
        toast.ClassList.Should().Contain("cn-toast");
        cut.FindAll("[data-slot='toast'] svg").Count.Should().Be(1); // variant icon only — no built-in close X (reference Toaster has no closeButton)
    }

    [Fact]
    public void Toast_Info_Variant_Has_Correct_Classes()
    {
        var service = RegisterToastService();
        var cut = Render<ToasterCn>();

        service.Info("Info toast");

        var toast = cut.Find("[data-slot='toast']");
        toast.GetAttribute("data-variant").Should().Be("info");
        toast.ClassList.Should().Contain("cn-toast");
        cut.FindAll("[data-slot='toast'] svg").Count.Should().Be(1); // variant icon only — no built-in close X (reference Toaster has no closeButton)
    }

    [Fact]
    public void Toast_With_Title_Shows_Title()
    {
        var service = RegisterToastService();
        var cut = Render<ToasterCn>();

        service.Show("Body", title: "My Title");

        cut.Find("[data-slot='toast-title']").TextContent.Should().Contain("My Title");
    }

    [Fact]
    public void Toast_Without_Title_Has_No_Title_Element()
    {
        var service = RegisterToastService();
        var cut = Render<ToasterCn>();

        service.Show("Body only");

        cut.FindAll("[data-slot='toast-title']").Should().BeEmpty();
    }

    [Fact]
    public void Toast_Has_No_Close_Button_By_Default()
    {
        // Matches the reference Toaster, which never enables Sonner's closeButton
        var service = RegisterToastService();
        var cut = Render<ToasterCn>();

        service.Show("Hello");

        cut.FindAll("[data-slot='toast-close']").Should().BeEmpty();
    }

    [Fact]
    public void Toast_CloseButton_OptIn_Renders_And_Dismisses()
    {
        var dismissedId = "";
        var cut = Render<ToastCn>(p => p
            .Add(c => c.Message, new ToastMessage("t1", "Hello", ToastVariant.Default))
            .Add(c => c.CloseButton, true)
            .Add(c => c.OnDismiss, EventCallback.Factory.Create<string>(this, id => dismissedId = id)));

        cut.Find("[data-slot='toast-close']").Click();
        dismissedId.Should().Be("t1");
    }

    [Fact]
    public void Toast_Has_Alert_Role()
    {
        // role=alert (assertive) is reserved for Error/Warning; routine toasts are role=status.
        var service = RegisterToastService();
        var cut = Render<ToasterCn>();

        service.Show("Routine");
        cut.Find("[data-slot='toast']").GetAttribute("role").Should().Be("status");

        service.Error("Something failed");
        cut.FindAll("[data-slot='toast']").Should().Contain(t => t.GetAttribute("role") == "alert");
    }

    [Fact]
    public void Toast_Has_DataVariant_Attribute()
    {
        var service = RegisterToastService();
        var cut = Render<ToasterCn>();

        service.Show("Default");

        cut.Find("[data-slot='toast']").GetAttribute("data-variant").Should().Be("default");
    }

    // --- Pause / eviction (paused toast pushed out of the visible window) ---

    [Fact]
    public void Toaster_Evicted_Paused_Toast_Still_AutoDismisses()
    {
        // Regression: hovering pauses the countdown, and only the newest 3 toasts
        // render. When a 4th toast arrives, the hover-paused oldest toast leaves the
        // DOM, so the browser never fires the balancing mouseleave. Eviction must
        // reset the pause state and restart the timer, or the toast — which has no
        // close button — becomes permanent.
        var service = RegisterToastService();
        var cut = Render<ToasterCn>();

        service.Show("One", durationMs: 500);
        service.Show("Two", durationMs: 500);
        service.Show("Three", durationMs: 500);

        // Hover-pause the oldest visible toast.
        cut.FindAll("[data-slot='toast']")
            .Single(t => t.TextContent.Contains("One"))
            .TriggerEvent("onmouseenter", new MouseEventArgs());

        // A 4th toast evicts the paused one from the visible window.
        service.Show("Four", durationMs: 500);
        // Show marshals through InvokeAsync, so wait for the render to apply.
        cut.WaitForAssertion(() =>
        {
            var rendered = cut.FindAll("[data-slot='toast']");
            rendered.Should().HaveCount(3);
            rendered.Should().NotContain(t => t.TextContent.Contains("One"));
        });

        // Every toast — including the evicted, formerly paused one — must expire.
        cut.WaitForAssertion(
            () => cut.FindAll("[data-slot='toast']").Should().BeEmpty(),
            TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Toaster_Pause_Then_Resume_Dismisses_Without_Resurrection()
    {
        // Normal hover flow: pause freezes the countdown past the original duration,
        // resume restarts it, and the dismissed toast stays gone — the eviction reset
        // must never revive a genuinely removed toast.
        var service = RegisterToastService();
        var cut = Render<ToasterCn>();

        service.Show("Sticky", durationMs: 250);

        cut.Find("[data-slot='toast']").TriggerEvent("onmouseenter", new MouseEventArgs());

        // Paused: still on screen well past its 250ms duration.
        await Task.Delay(400);
        cut.FindAll("[data-slot='toast']").Should().HaveCount(1);

        cut.Find("[data-slot='toast']").TriggerEvent("onmouseleave", new MouseEventArgs());

        cut.WaitForAssertion(
            () => cut.FindAll("[data-slot='toast']").Should().BeEmpty(),
            TimeSpan.FromSeconds(5));

        // No resurrection after removal.
        await Task.Delay(300);
        cut.FindAll("[data-slot='toast']").Should().BeEmpty();
    }
}

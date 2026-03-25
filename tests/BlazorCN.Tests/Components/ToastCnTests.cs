using Bunit;
using FluentAssertions;
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
        toast.ClassList.Should().Contain("bg-background");
        toast.ClassList.Should().Contain("text-foreground");
    }

    [Fact]
    public void Toast_Success_Variant_Has_Correct_Classes()
    {
        var service = RegisterToastService();
        var cut = Render<ToasterCn>();

        service.Success("Success toast");

        var toast = cut.Find("[data-slot='toast']");
        toast.GetAttribute("data-variant").Should().Be("success");
        toast.ClassList.Should().Contain("bg-green-50");
        toast.ClassList.Should().Contain("text-green-900");
    }

    [Fact]
    public void Toast_Error_Variant_Has_Correct_Classes()
    {
        var service = RegisterToastService();
        var cut = Render<ToasterCn>();

        service.Error("Error toast");

        var toast = cut.Find("[data-slot='toast']");
        toast.GetAttribute("data-variant").Should().Be("error");
        toast.ClassList.Should().Contain("bg-destructive");
        toast.ClassList.Should().Contain("text-destructive-foreground");
    }

    [Fact]
    public void Toast_Warning_Variant_Has_Correct_Classes()
    {
        var service = RegisterToastService();
        var cut = Render<ToasterCn>();

        service.Warning("Warning toast");

        var toast = cut.Find("[data-slot='toast']");
        toast.GetAttribute("data-variant").Should().Be("warning");
        toast.ClassList.Should().Contain("bg-yellow-50");
        toast.ClassList.Should().Contain("text-yellow-900");
    }

    [Fact]
    public void Toast_Info_Variant_Has_Correct_Classes()
    {
        var service = RegisterToastService();
        var cut = Render<ToasterCn>();

        service.Info("Info toast");

        var toast = cut.Find("[data-slot='toast']");
        toast.GetAttribute("data-variant").Should().Be("info");
        toast.ClassList.Should().Contain("bg-blue-50");
        toast.ClassList.Should().Contain("text-blue-900");
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
    public void Toast_Has_Close_Button()
    {
        var service = RegisterToastService();
        var cut = Render<ToasterCn>();

        service.Show("Hello");

        cut.Find("[data-slot='toast-close']").Should().NotBeNull();
    }

    [Fact]
    public void Toast_Close_Button_Removes_Toast()
    {
        var service = RegisterToastService();
        var cut = Render<ToasterCn>();

        service.Show("Hello");
        cut.FindAll("[data-slot='toast']").Should().HaveCount(1);

        cut.Find("[data-slot='toast-close']").Click();
        cut.FindAll("[data-slot='toast']").Should().BeEmpty();
    }

    [Fact]
    public void Toast_Has_Alert_Role()
    {
        var service = RegisterToastService();
        var cut = Render<ToasterCn>();

        service.Show("Alert!");

        cut.Find("[data-slot='toast']").GetAttribute("role").Should().Be("alert");
    }

    [Fact]
    public void Toast_Has_DataVariant_Attribute()
    {
        var service = RegisterToastService();
        var cut = Render<ToasterCn>();

        service.Show("Default");

        cut.Find("[data-slot='toast']").GetAttribute("data-variant").Should().Be("default");
    }
}

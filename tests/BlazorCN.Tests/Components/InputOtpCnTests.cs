using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace BlazorCN.Tests.Components;

public class InputOtpCnTests : BunitContext
{
    // --- InputOtpCn ---

    [Fact]
    public void InputOtp_Renders_With_DataSlot()
    {
        var cut = Render<InputOtpCn>(p => p.AddChildContent("Content"));
        cut.Find("[data-slot='input-otp']").Should().NotBeNull();
    }

    [Fact]
    public void InputOtp_Has_Default_Classes()
    {
        var cut = Render<InputOtpCn>(p => p.AddChildContent("Content"));
        var el = cut.Find("[data-slot='input-otp']");
        el.ClassList.Should().Contain("cn-input-otp");
        el.ClassList.Should().Contain("flex");
        el.ClassList.Should().Contain("items-center");
    }

    [Fact]
    public void InputOtp_Contains_Hidden_Input()
    {
        var cut = Render<InputOtpCn>(p => p.AddChildContent("Content"));
        var input = cut.Find("[data-slot='input-otp'] input");
        input.Should().NotBeNull();
        input.GetAttribute("type").Should().Be("text");
        input.ClassList.Should().Contain("sr-only");
    }

    [Fact]
    public void InputOtp_Hidden_Input_Has_MaxLength()
    {
        var cut = Render<InputOtpCn>(p => p
            .Add(c => c.MaxLength, 4)
            .AddChildContent("Content"));
        var input = cut.Find("[data-slot='input-otp'] input");
        input.GetAttribute("maxlength").Should().Be("4");
    }

    [Fact]
    public void InputOtp_Default_MaxLength_Is_6()
    {
        var cut = Render<InputOtpCn>(p => p.AddChildContent("Content"));
        var input = cut.Find("[data-slot='input-otp'] input");
        input.GetAttribute("maxlength").Should().Be("6");
    }

    [Fact]
    public void InputOtp_Class_Passthrough()
    {
        var cut = Render<InputOtpCn>(p => p
            .Add(c => c.Class, "custom-otp")
            .AddChildContent("Content"));
        cut.Find("[data-slot='input-otp']").ClassList.Should().Contain("custom-otp");
    }

    [Fact]
    public void InputOtp_AdditionalAttributes_Passthrough()
    {
        var cut = Render<InputOtpCn>(p => p
            .Add(c => c.AdditionalAttributes, new Dictionary<string, object?> { { "id", "my-otp" } })
            .AddChildContent("Content"));
        cut.Find("[data-slot='input-otp']").GetAttribute("id").Should().Be("my-otp");
    }

    [Fact]
    public void InputOtp_Disabled_Disables_Hidden_Input()
    {
        var cut = Render<InputOtpCn>(p => p
            .Add(c => c.Disabled, true)
            .AddChildContent("Content"));
        cut.Find("[data-slot='input-otp'] input").HasAttribute("disabled").Should().BeTrue();
    }

    // --- InputOtpGroupCn ---

    [Fact]
    public void InputOtpGroup_Renders_With_DataSlot()
    {
        var cut = Render<InputOtpCn>(p => p
            .AddChildContent<InputOtpGroupCn>(g => g
                .AddChildContent("Slots")));
        cut.Find("[data-slot='input-otp-group']").Should().NotBeNull();
    }

    [Fact]
    public void InputOtpGroup_Has_Default_Classes()
    {
        var cut = Render<InputOtpCn>(p => p
            .AddChildContent<InputOtpGroupCn>(g => g
                .AddChildContent("Slots")));
        var el = cut.Find("[data-slot='input-otp-group']");
        el.ClassList.Should().Contain("flex");
        el.ClassList.Should().Contain("items-center");
    }

    [Fact]
    public void InputOtpGroup_Class_Passthrough()
    {
        var cut = Render<InputOtpCn>(p => p
            .AddChildContent<InputOtpGroupCn>(g => g
                .Add(c => c.Class, "custom-group")
                .AddChildContent("Slots")));
        cut.Find("[data-slot='input-otp-group']").ClassList.Should().Contain("custom-group");
    }

    // --- InputOtpSlotCn ---

    [Fact]
    public void InputOtpSlot_Renders_With_DataSlot()
    {
        var cut = Render<InputOtpCn>(p => p
            .AddChildContent<InputOtpSlotCn>(s => s
                .Add(c => c.Index, 0)));
        cut.Find("[data-slot='input-otp-slot']").Should().NotBeNull();
    }

    [Fact]
    public void InputOtpSlot_Has_Default_Classes()
    {
        var cut = Render<InputOtpCn>(p => p
            .AddChildContent<InputOtpSlotCn>(s => s
                .Add(c => c.Index, 0)));
        var el = cut.Find("[data-slot='input-otp-slot']");
        el.ClassList.Should().Contain("cn-input-otp-slot");
        el.ClassList.Should().Contain("relative");
        el.ClassList.Should().Contain("flex");
        el.ClassList.Should().Contain("items-center");
        el.ClassList.Should().Contain("justify-center");
    }

    [Fact]
    public void InputOtpSlot_Has_DataActive_False_By_Default()
    {
        var cut = Render<InputOtpCn>(p => p
            .AddChildContent<InputOtpSlotCn>(s => s
                .Add(c => c.Index, 0)));
        cut.Find("[data-slot='input-otp-slot']").GetAttribute("data-active").Should().Be("false");
    }

    [Fact]
    public void InputOtpSlot_Displays_Character_From_Value()
    {
        var cut = Render<InputOtpCn>(p => p
            .Add(c => c.Value, "123")
            .AddChildContent<InputOtpSlotCn>(s => s
                .Add(c => c.Index, 0)));
        cut.Find("[data-slot='input-otp-slot']").TextContent.Should().Contain("1");
    }

    [Fact]
    public void InputOtpSlot_Displays_Correct_Character_By_Index()
    {
        var cut = Render<InputOtpCn>(p => p
            .Add(c => c.Value, "456")
            .AddChildContent<InputOtpSlotCn>(s => s
                .Add(c => c.Index, 2)));
        cut.Find("[data-slot='input-otp-slot']").TextContent.Should().Contain("6");
    }

    [Fact]
    public void InputOtpSlot_Empty_When_No_Value_At_Index()
    {
        var cut = Render<InputOtpCn>(p => p
            .Add(c => c.Value, "12")
            .AddChildContent<InputOtpSlotCn>(s => s
                .Add(c => c.Index, 3)));
        cut.Find("[data-slot='input-otp-slot']").TextContent.Trim().Should().BeEmpty();
    }

    [Fact]
    public void InputOtpSlot_Class_Passthrough()
    {
        var cut = Render<InputOtpCn>(p => p
            .AddChildContent<InputOtpSlotCn>(s => s
                .Add(c => c.Index, 0)
                .Add(c => c.Class, "custom-slot")));
        cut.Find("[data-slot='input-otp-slot']").ClassList.Should().Contain("custom-slot");
    }

    // --- InputOtpSeparatorCn ---

    [Fact]
    public void InputOtpSeparator_Renders_With_DataSlot()
    {
        var cut = Render<InputOtpCn>(p => p
            .AddChildContent<InputOtpSeparatorCn>());
        cut.Find("[data-slot='input-otp-separator']").Should().NotBeNull();
    }

    [Fact]
    public void InputOtpSeparator_Has_Separator_Role()
    {
        var cut = Render<InputOtpCn>(p => p
            .AddChildContent<InputOtpSeparatorCn>());
        cut.Find("[data-slot='input-otp-separator']").GetAttribute("role").Should().Be("separator");
    }

    [Fact]
    public void InputOtpSeparator_Has_Default_Classes()
    {
        var cut = Render<InputOtpCn>(p => p
            .AddChildContent<InputOtpSeparatorCn>());
        var el = cut.Find("[data-slot='input-otp-separator']");
        el.ClassList.Should().Contain("flex");
        el.ClassList.Should().Contain("items-center");
    }

    [Fact]
    public void InputOtpSeparator_Contains_Svg()
    {
        var cut = Render<InputOtpCn>(p => p
            .AddChildContent<InputOtpSeparatorCn>());
        cut.Find("[data-slot='input-otp-separator'] svg").Should().NotBeNull();
    }

    [Fact]
    public void InputOtpSeparator_Class_Passthrough()
    {
        var cut = Render<InputOtpCn>(p => p
            .AddChildContent<InputOtpSeparatorCn>(s => s
                .Add(c => c.Class, "custom-sep")));
        cut.Find("[data-slot='input-otp-separator']").ClassList.Should().Contain("custom-sep");
    }

    // --- Focus/Blur behavior ---

    [Fact]
    public void InputOtpSlot_Becomes_Active_On_Focus()
    {
        var cut = Render<InputOtpCn>(p => p
            .AddChildContent<InputOtpSlotCn>(s => s
                .Add(c => c.Index, 0)));
        // Initially not active
        cut.Find("[data-slot='input-otp-slot']").GetAttribute("data-active").Should().Be("false");

        // Focus the hidden input
        cut.Find("[data-slot='input-otp'] input").Focus();

        // Slot should now be active
        cut.Find("[data-slot='input-otp-slot']").GetAttribute("data-active").Should().Be("true");
    }

    // --- InputOtp Input behavior ---

    [Fact]
    public void InputOtp_Has_Inputmode_Numeric()
    {
        var cut = Render<InputOtpCn>(p => p.AddChildContent("Content"));
        cut.Find("[data-slot='input-otp'] input").GetAttribute("inputmode").Should().Be("numeric");
    }

    [Fact]
    public void InputOtp_Has_Autocomplete_OneTimeCode()
    {
        var cut = Render<InputOtpCn>(p => p.AddChildContent("Content"));
        cut.Find("[data-slot='input-otp'] input").GetAttribute("autocomplete").Should().Be("one-time-code");
    }

    // --- Pattern ---

    [Fact]
    public void InputOtp_Default_Pattern_Is_DigitsOnly()
    {
        var cut = Render<InputOtpCn>(p => p.AddChildContent("Content"));
        var input = cut.Find("[data-slot='input-otp'] input");
        input.GetAttribute("inputmode").Should().Be("numeric");
    }

    [Fact]
    public void InputOtp_Alphanumeric_Pattern_Sets_Text_InputMode()
    {
        var cut = Render<InputOtpCn>(p => p
            .Add(c => c.Pattern, InputOtpCn.RegexpOnlyDigitsAndChars)
            .AddChildContent("Content"));
        var input = cut.Find("[data-slot='input-otp'] input");
        input.GetAttribute("inputmode").Should().Be("text");
    }

    [Fact]
    public void InputOtp_Custom_InputMode_Overrides_Default()
    {
        var cut = Render<InputOtpCn>(p => p
            .Add(c => c.InputMode, "tel")
            .AddChildContent("Content"));
        var input = cut.Find("[data-slot='input-otp'] input");
        input.GetAttribute("inputmode").Should().Be("tel");
    }

    [Fact]
    public void InputOtp_Pattern_Constants_Are_Defined()
    {
        InputOtpCn.RegexpOnlyDigits.Should().Be("[0-9]");
        InputOtpCn.RegexpOnlyDigitsAndChars.Should().Be("[0-9a-zA-Z]");
    }
}

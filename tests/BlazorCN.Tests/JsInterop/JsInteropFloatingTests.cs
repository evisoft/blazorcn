using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Xunit;

namespace BlazorCN.Tests.JsInterop;

public class JsInteropFloatingTests : BunitContext
{
    private BunitJSModuleInterop SetupModule()
    {
        var module = JSInterop.SetupModule("./_content/BlazorCN/blazorcn.js");
        return module;
    }

    [Fact]
    public async Task CreateFloatingAsync_Calls_JS_With_Correct_Parameters()
    {
        // Arrange
        var module = SetupModule();
        var planned = module.Setup<string>("createFloating", _ => true);
        planned.SetResult("bottom");

        var jsInterop = new JsInteropCn(JSInterop.JSRuntime);
        var options = new FloatingOptions
        {
            Side = FloatingSide.Bottom,
            SideOffset = 8,
            Align = FloatingAlign.Center,
            AlignOffset = 0
        };

        // Act
        var result = await jsInterop.CreateFloatingAsync(
            default, default, "popover-1", options);

        // Assert
        result.Should().Be("bottom");
        var invocation = planned.Invocations.Should().ContainSingle().Subject;
        invocation.Arguments[2].Should().Be("popover-1");

        // The 4th argument should be the serialized options object
        var optionsArg = invocation.Arguments[3];
        optionsArg.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateFloatingAsync_Serializes_Side_As_Lowercase()
    {
        // Arrange
        var module = SetupModule();
        var planned = module.Setup<string>("createFloating", _ => true);
        planned.SetResult("top");

        var jsInterop = new JsInteropCn(JSInterop.JSRuntime);
        var options = new FloatingOptions { Side = FloatingSide.Top };

        // Act
        var result = await jsInterop.CreateFloatingAsync(
            default, default, "test-1", options);

        // Assert
        result.Should().Be("top");
    }

    [Fact]
    public async Task CreateFloatingAsync_Serializes_Align_As_Lowercase()
    {
        // Arrange
        var module = SetupModule();
        var planned = module.Setup<string>("createFloating", _ => true);
        planned.SetResult("bottom");

        var jsInterop = new JsInteropCn(JSInterop.JSRuntime);
        var options = new FloatingOptions { Align = FloatingAlign.Start };

        // Act
        await jsInterop.CreateFloatingAsync(default, default, "test-2", options);

        // Assert
        planned.Invocations.Should().ContainSingle();
    }

    [Fact]
    public async Task UpdateFloatingAsync_Calls_JS()
    {
        // Arrange
        var module = SetupModule();
        var handler = module.SetupVoid("updateFloating", _ => true);
        handler.SetVoidResult();

        var jsInterop = new JsInteropCn(JSInterop.JSRuntime);

        // Act
        await jsInterop.UpdateFloatingAsync("popover-1");

        // Assert
        var invocation = handler.Invocations.Should().ContainSingle().Subject;
        invocation.Arguments[0].Should().Be("popover-1");
    }

    [Fact]
    public async Task DestroyFloatingAsync_Calls_JS()
    {
        // Arrange
        var module = SetupModule();
        var handler = module.SetupVoid("destroyFloating", _ => true);
        handler.SetVoidResult();

        var jsInterop = new JsInteropCn(JSInterop.JSRuntime);

        // Act
        await jsInterop.DestroyFloatingAsync("popover-1");

        // Assert
        var invocation = handler.Invocations.Should().ContainSingle().Subject;
        invocation.Arguments[0].Should().Be("popover-1");
    }

    [Fact]
    public void FloatingOptions_Has_Correct_Defaults()
    {
        var options = new FloatingOptions();

        options.Side.Should().Be(FloatingSide.Bottom);
        options.SideOffset.Should().Be(4);
        options.Align.Should().Be(FloatingAlign.Center);
        options.AlignOffset.Should().Be(0);
    }

    [Theory]
    [InlineData(FloatingSide.Top)]
    [InlineData(FloatingSide.Right)]
    [InlineData(FloatingSide.Bottom)]
    [InlineData(FloatingSide.Left)]
    public async Task CreateFloatingAsync_Supports_All_Sides(FloatingSide side)
    {
        // Arrange
        var module = SetupModule();
        var planned = module.Setup<string>("createFloating", _ => true);
        planned.SetResult(side.ToString().ToLowerInvariant());

        var jsInterop = new JsInteropCn(JSInterop.JSRuntime);
        var options = new FloatingOptions { Side = side };

        // Act
        var result = await jsInterop.CreateFloatingAsync(
            default, default, $"test-{side}", options);

        // Assert
        result.Should().Be(side.ToString().ToLowerInvariant());
    }

    [Theory]
    [InlineData(FloatingAlign.Start)]
    [InlineData(FloatingAlign.Center)]
    [InlineData(FloatingAlign.End)]
    public async Task CreateFloatingAsync_Supports_All_Alignments(FloatingAlign align)
    {
        // Arrange
        var module = SetupModule();
        var planned = module.Setup<string>("createFloating", _ => true);
        planned.SetResult("bottom");

        var jsInterop = new JsInteropCn(JSInterop.JSRuntime);
        var options = new FloatingOptions { Align = align };

        // Act
        await jsInterop.CreateFloatingAsync(
            default, default, $"test-{align}", options);

        // Assert
        planned.Invocations.Should().ContainSingle();
    }
}

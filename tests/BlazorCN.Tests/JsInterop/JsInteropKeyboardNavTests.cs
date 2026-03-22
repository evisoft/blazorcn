using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Xunit;

namespace BlazorCN.Tests.JsInterop;

public class JsInteropKeyboardNavTests : BunitContext
{
    private BunitJSModuleInterop SetupModule()
    {
        return JSInterop.SetupModule("./_content/BlazorCN/blazorcn.js");
    }

    [Fact]
    public async Task SetupKeyboardNavigationAsync_Calls_JS_With_Correct_Parameters()
    {
        // Arrange
        var module = SetupModule();
        var handler = module.SetupVoid("setupKeyboardNavigation", _ => true);
        handler.SetVoidResult();

        var jsInterop = new JsInteropCn(JSInterop.JSRuntime);
        var callbackTarget = new FakeCallbackTarget();
        var dotnetRef = DotNetObjectReference.Create(callbackTarget);

        // Act
        await jsInterop.SetupKeyboardNavigationAsync(
            default, "menu-1", dotnetRef, "OnEscape");

        // Assert
        var invocation = handler.Invocations.Should().ContainSingle().Subject;
        invocation.Arguments[1].Should().Be("menu-1");
        invocation.Arguments[3].Should().Be("OnEscape");
    }

    [Fact]
    public async Task SetupKeyboardNavigationAsync_Passes_Default_Selector()
    {
        // Arrange
        var module = SetupModule();
        var handler = module.SetupVoid("setupKeyboardNavigation", _ => true);
        handler.SetVoidResult();

        var jsInterop = new JsInteropCn(JSInterop.JSRuntime);
        var callbackTarget = new FakeCallbackTarget();
        var dotnetRef = DotNetObjectReference.Create(callbackTarget);

        // Act
        await jsInterop.SetupKeyboardNavigationAsync(
            default, "menu-2", dotnetRef, "OnEscape");

        // Assert
        var invocation = handler.Invocations.Should().ContainSingle().Subject;
        // The 5th argument is the options object containing selector and orientation
        invocation.Arguments[4].Should().NotBeNull();
    }

    [Fact]
    public async Task SetupKeyboardNavigationAsync_Custom_Selector_And_Orientation()
    {
        // Arrange
        var module = SetupModule();
        var handler = module.SetupVoid("setupKeyboardNavigation", _ => true);
        handler.SetVoidResult();

        var jsInterop = new JsInteropCn(JSInterop.JSRuntime);
        var callbackTarget = new FakeCallbackTarget();
        var dotnetRef = DotNetObjectReference.Create(callbackTarget);

        // Act
        await jsInterop.SetupKeyboardNavigationAsync(
            default, "menu-3", dotnetRef, "OnEscape",
            itemSelector: "[data-tab-item]",
            orientation: "horizontal");

        // Assert
        handler.Invocations.Should().ContainSingle();
    }

    [Fact]
    public async Task CleanupKeyboardNavigationAsync_Calls_JS()
    {
        // Arrange
        var module = SetupModule();
        var handler = module.SetupVoid("cleanupKeyboardNavigation", _ => true);
        handler.SetVoidResult();

        var jsInterop = new JsInteropCn(JSInterop.JSRuntime);

        // Act
        await jsInterop.CleanupKeyboardNavigationAsync("menu-1");

        // Assert
        var invocation = handler.Invocations.Should().ContainSingle().Subject;
        invocation.Arguments[0].Should().Be("menu-1");
    }

    [Theory]
    [InlineData("vertical")]
    [InlineData("horizontal")]
    [InlineData("both")]
    public async Task SetupKeyboardNavigationAsync_Supports_All_Orientations(string orientation)
    {
        // Arrange
        var module = SetupModule();
        var handler = module.SetupVoid("setupKeyboardNavigation", _ => true);
        handler.SetVoidResult();

        var jsInterop = new JsInteropCn(JSInterop.JSRuntime);
        var callbackTarget = new FakeCallbackTarget();
        var dotnetRef = DotNetObjectReference.Create(callbackTarget);

        // Act
        await jsInterop.SetupKeyboardNavigationAsync(
            default, $"menu-{orientation}", dotnetRef, "OnEscape",
            orientation: orientation);

        // Assert
        handler.Invocations.Should().ContainSingle();
    }

    [Fact]
    public async Task SetupKeyboardNavigationAsync_DotNetRef_Is_Passed()
    {
        // Arrange
        var module = SetupModule();
        var handler = module.SetupVoid("setupKeyboardNavigation", _ => true);
        handler.SetVoidResult();

        var jsInterop = new JsInteropCn(JSInterop.JSRuntime);
        var callbackTarget = new FakeCallbackTarget();
        var dotnetRef = DotNetObjectReference.Create(callbackTarget);

        // Act
        await jsInterop.SetupKeyboardNavigationAsync(
            default, "menu-ref", dotnetRef, "OnEscape");

        // Assert
        var invocation = handler.Invocations.Should().ContainSingle().Subject;
        // Argument 2 is the dotnetRef
        invocation.Arguments[2].Should().NotBeNull();
    }

    /// <summary>Helper class for DotNetObjectReference creation.</summary>
    private class FakeCallbackTarget
    {
        [JSInvokable]
        public void OnEscape() { }
    }
}

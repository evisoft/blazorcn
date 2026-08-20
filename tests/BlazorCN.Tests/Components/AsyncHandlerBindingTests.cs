using System.Threading.Tasks;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Xunit;

namespace BlazorCN.Tests.Components;

/// <summary>
/// Pins which <c>EventCallback</c> overload an <c>@onclick</c> lambda binds to, because the two
/// shapes look identical in a .razor file but behave completely differently:
///
///   @onclick="@(() =&gt; DoAsync())"      expression body, inferred return type Task
///   @onclick="@(() =&gt; { DoAsync(); })" statement body, inferred return type void
///
/// The first can bind to <c>Func&lt;Task&gt;</c>, which Blazor awaits and then re-renders. The
/// second can only bind to <c>Action</c>, so the Task is discarded: everything the method does
/// after its first await mutates state that nothing re-renders (a spinner that never stops).
///
/// These tests decide, empirically, which of the ~97 call sites in the demo are actually broken.
/// </summary>
public class AsyncHandlerBindingTests : BunitContext
{
    private sealed class ExpressionBodied : AsyncHandlerHost
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "button");
            // Expression-bodied: the lambda's value IS the Task.
            builder.AddAttribute(1, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => BumpAsync()));
            builder.CloseElement();
            builder.OpenElement(2, "p");
            builder.AddAttribute(3, "id", "v");
            builder.AddContent(4, Value);
            builder.CloseElement();
        }
    }

    private sealed class StatementBodied : AsyncHandlerHost
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "button");
            // Statement-bodied: the call is an expression-statement, the Task is dropped.
            #pragma warning disable CS4014 // discarding the Task is exactly what this test reproduces
            builder.AddAttribute(1, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => { BumpAsync(); }));
#pragma warning restore CS4014
            builder.CloseElement();
            builder.OpenElement(2, "p");
            builder.AddAttribute(3, "id", "v");
            builder.AddContent(4, Value);
            builder.CloseElement();
        }
    }

    private abstract class AsyncHandlerHost : ComponentBase
    {
        internal readonly TaskCompletionSource Gate = new(TaskCreationOptions.RunContinuationsAsynchronously);
        protected int Value;

        protected async Task BumpAsync()
        {
            await Gate.Task;
            Value++;                 // deliberately NO StateHasChanged — that is the point
        }
    }

    [Fact]
    public async Task Expression_Bodied_Lambda_Is_Awaited_So_Post_Await_State_Renders()
    {
        var cut = Render<ExpressionBodied>();
        var click = cut.Find("button").ClickAsync(new());
        cut.Instance.Gate.SetResult();
        await click;

        cut.WaitForAssertion(() => Assert.Equal("1", cut.Find("#v").TextContent));
    }

    [Fact]
    public async Task Statement_Bodied_Lambda_Discards_The_Task_So_The_Ui_Never_Updates()
    {
        var cut = Render<StatementBodied>();
        var click = cut.Find("button").ClickAsync(new());
        await click;                             // returns immediately — the Task was discarded
        cut.Instance.Gate.SetResult();
        await Task.Delay(150);                   // the continuation runs, but nothing re-renders

        Assert.Equal("0", cut.Find("#v").TextContent);
    }
}

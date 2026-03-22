using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BlazorCN;

/// <summary>
/// Extension methods for registering BlazorCN services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds BlazorCN services to the dependency injection container.
    /// </summary>
    public static IServiceCollection AddBlazorCN(this IServiceCollection services)
    {
        services.TryAddScoped<JsInteropCn>();
        services.TryAddScoped<ToastService>();
        return services;
    }
}

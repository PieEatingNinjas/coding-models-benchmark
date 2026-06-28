using Microsoft.Extensions.DependencyInjection;
using TodoApi.Application.Interfaces;
using TodoApi.Application.Services;

namespace TodoApi.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ITodoService, TodoService>();
        return services;
    }
}
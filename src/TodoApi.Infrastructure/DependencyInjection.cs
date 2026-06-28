using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TodoApi.Application.Interfaces;
using TodoApi.Infrastructure.Data;
using TodoApi.Infrastructure.Repositories;

namespace TodoApi.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddDbContext<TodoDb>(opt => opt.UseInMemoryDatabase("TodoList"));
        services.AddScoped<ITodoRepository, TodoRepository>();
        return services;
    }
}
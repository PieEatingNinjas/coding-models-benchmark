using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TodoApi.Application.Todos;
using TodoApi.Infrastructure.Data;

namespace TodoApi.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddTodoInfrastructure(this IServiceCollection services, string databaseName)
    {
        services.AddDbContext<TodoDb>(opt => opt.UseInMemoryDatabase(databaseName));
        services.AddScoped<ITodoRepository, TodoRepository>();
        return services;
    }
}

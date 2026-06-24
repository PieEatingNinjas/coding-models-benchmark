using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TodoApi.Data;

namespace TodoApi.Tests.Integration;

/// <summary>
/// Boots the real app but swaps the DbContext for a uniquely-named in-memory store,
/// so each test class gets an isolated database.
/// </summary>
public class TodoApiFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<TodoDb>>();
            services.AddDbContext<TodoDb>(opt => opt.UseInMemoryDatabase(_dbName));
        });
    }
}

using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TodoApi.Models;

namespace TodoApi.Data;

public class TodoDb(DbContextOptions<TodoDb> options) : DbContext(options)
{
    public DbSet<TodoItem> Todos => Set<TodoItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TodoItem>()
            .Property(todo => todo.Tags)
            .HasConversion(
                tags => JsonSerializer.Serialize(tags),
                json => JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>());
    }
}

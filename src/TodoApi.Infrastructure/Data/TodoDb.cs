using Microsoft.EntityFrameworkCore;
using TodoApi.Domain.Entities;

namespace TodoApi.Infrastructure.Data;

public class TodoDb(DbContextOptions<TodoDb> options) : DbContext(options)
{
    public DbSet<TodoItem> Todos => Set<TodoItem>();
}

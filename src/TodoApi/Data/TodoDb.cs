using Microsoft.EntityFrameworkCore;
using TodoApi.Models;

namespace TodoApi.Data;

public class TodoDb(DbContextOptions<TodoDb> options) : DbContext(options)
{
    public DbSet<TodoItem> Todos => Set<TodoItem>();
}

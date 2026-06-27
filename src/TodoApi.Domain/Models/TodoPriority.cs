namespace TodoApi.Models;

public enum TodoPriority
{
    Low,
    Medium,
    High,
}

public static class TodoPriorityExtensions
{
    public static bool TryParsePriority(string? value, out TodoPriority priority)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            priority = TodoPriority.Medium;
            return false;
        }

        if (Enum.TryParse<TodoPriority>(value.Trim(), ignoreCase: true, out priority))
        {
            return true;
        }

        priority = TodoPriority.Medium;
        return false;
    }
}

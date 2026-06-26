using System.Text.Json.Serialization;

namespace TodoApi.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
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
        if (Enum.TryParse(value, ignoreCase: true, out priority) && Enum.IsDefined(priority))
        {
            return true;
        }

        priority = TodoPriority.Medium;
        return false;
    }
}

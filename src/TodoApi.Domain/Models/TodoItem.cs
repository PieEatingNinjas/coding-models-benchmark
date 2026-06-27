namespace TodoApi.Models;

public class TodoItem
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public bool IsComplete { get; set; }
    public TodoPriority Priority { get; set; } = TodoPriority.Medium;
    public DateTimeOffset? DueDate { get; set; }
    public List<string> Tags { get; set; } = [];
    public string? Secret { get; set; }

    public void SetTags(IEnumerable<string>? tags) => Tags = NormalizeTags(tags);

    public bool HasTag(string? tag)
    {
        var normalizedTag = NormalizeTag(tag);
        return !string.IsNullOrEmpty(normalizedTag) &&
               Tags.Any(existingTag => existingTag.Equals(normalizedTag, StringComparison.OrdinalIgnoreCase));
    }

    public static List<string> NormalizeTags(IEnumerable<string>? tags)
    {
        if (tags is null)
        {
            return [];
        }

        return tags
            .Where(tag => !string.IsNullOrWhiteSpace(tag))
            .Select(NormalizeTag)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public static string NormalizeTag(string? tag) => tag?.Trim().ToLowerInvariant() ?? string.Empty;
}

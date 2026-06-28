using System.Text.Json.Serialization;

namespace TodoApi.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Priority
{
    Low,
    Medium,
    High
}

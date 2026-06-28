using System.Text.Json.Serialization;

namespace TodoApi.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Priority
{
    Low,
    Medium,
    High
}

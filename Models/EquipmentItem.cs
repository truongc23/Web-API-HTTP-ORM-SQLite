using System.Text.Json.Serialization;

namespace CampusEquipmentApi.Models;

public sealed class EquipmentItem
{
    [JsonPropertyName("id")]
    public int EquipmentItemId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
}

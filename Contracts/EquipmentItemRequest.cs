namespace CampusEquipmentApi.Contracts;

public sealed class EquipmentItemRequest
{
    public string? Name { get; set; }
    public string? Category { get; set; }
    public bool IsAvailable { get; set; }
}

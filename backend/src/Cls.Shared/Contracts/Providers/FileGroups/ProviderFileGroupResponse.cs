namespace Cls.Shared.Contracts.Providers.FileGroups;
public class ProviderFileGroupResponse
{
    public int Id { get; set; }
    public int ProviderId { get; set; }
    public string Label { get; set; } = string.Empty;
    public int CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<ProviderFileGroupItemResponse> GroupItems { get; set; } = new();
}

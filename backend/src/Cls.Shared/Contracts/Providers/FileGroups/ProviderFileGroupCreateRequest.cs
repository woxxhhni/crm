namespace Cls.Shared.Contracts.Providers.FileGroups;
public class ProviderFileGroupCreateRequest
{
    public int ProviderId { get; set; }
    public string Label { get; set; } = string.Empty;
    public int CreatedByUserId { get; set; }
    public List<int>? FileIds { get; set; }
}

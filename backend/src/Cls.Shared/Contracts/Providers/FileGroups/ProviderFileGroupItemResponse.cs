namespace Cls.Shared.Contracts.Providers.FileGroups;

public class ProviderFileGroupItemResponse
{
    public int Id { get; set; }
    public int ProviderFileGroupId { get; set; }
    public int FileId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}
